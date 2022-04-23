using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Pulse.Core.Connections;

public class PortBruteForceNatTraversal : IConnectionEstablishmentStrategy
{
    private const int SourcePort = 54911;

    public async Task<IConnection> EstablishConnectionAsync(IPAddress destination, 
        CancellationToken cancellationToken = default)
    {
        var shouldSend = true;
        var destinationPort = 0;
        using var client = new UdpClient(SourcePort);
        _ = client.ReceiveAsync(cancellationToken)
            .AsTask().ContinueWith(task =>
            {
                Console.WriteLine("hey got message");
                var udpReceiveResult = task.Result;
                destinationPort = udpReceiveResult.RemoteEndPoint.Port;
                Console.WriteLine("Message received: " + Encoding.ASCII.GetString(udpReceiveResult.Buffer));
                shouldSend = false;
            }, cancellationToken);

        for (var i = 0; shouldSend; i++)
        {
            try
            {
                await destination.SendMessageAsync(client, i % ushort.MaxValue);
            }
            catch (Exception e)
            {
            }
        }

        while (true)
        {
            await destination.SendMessageAsync(client, destinationPort, message: "Yayy");
        }
        // TODO - should listen to the UdpPort and wait for an answer from the other person
        return null;testtt
    }
}