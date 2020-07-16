using System;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System.Text;
using System.Threading;

class Worker
{
    public static void Main()
    {
        // TODO: Pull credentials from file
        var factory = new ConnectionFactory() { 
            HostName = "192.168.1.2", 
            UserName = "test", 
            Password = "test" };

        using(var connection = factory.CreateConnection())
        using(var channel = connection.CreateModel())
        {
            channel.BasicQos(prefetchSize: 0, prefetchCount: 1, global: false);

            Console.WriteLine(" [*] Waiting for messages.");

            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (sender, ea) =>
            {
                var body = ea.Body.ToArray();
                var message = Encoding.UTF8.GetString(body);
                Console.WriteLine("Received Message: {0}", message);

                var probe = message.Substring(0, 7);
                var value = message.Substring(8);

                //TODO: save the message
                string filePath = string.Empty;
                if(probe == "Probe_1"){
                    filePath = @"/home/tim/Documents/Programming/RabbitMQ/Worker/probe1.txt";
                } else if (probe == "Probe_2") {
                    filePath = @"/home/tim/Documents/Programming/RabbitMQ/Worker/probe2.txt";
                } else {
                    filePath = @"/home/tim/Documents/Programming/RabbitMQ/Worker/otherProbe1.txt";
                }

                var time = DateTime.Now.ToString();

                var entry = time + ", " + value;

                using (System.IO.StreamWriter file =
                    new System.IO.StreamWriter(filePath, true))
                    {
                        file.WriteLine(entry);
                    }

                // Note: it is possible to access the channel via
                //       ((EventingBasicConsumer)sender).Model here
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };
            channel.BasicConsume(queue: "esp8266_amqp",
                                 autoAck: false,
                                 consumer: consumer);

            Console.WriteLine(" Press [enter] to exit.");
            Console.ReadLine();
        }
    }
}
