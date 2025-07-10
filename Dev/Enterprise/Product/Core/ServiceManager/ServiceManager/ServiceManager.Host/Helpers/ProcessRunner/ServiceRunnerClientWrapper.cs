using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using ServiceManagerProto;

namespace Enterprise.ServiceManager.Host
{
	public class ServiceRunnerClientWrapper : IServiceRunnerClientWrapper
	{
		public ServiceRunnerClientWrapper(int port)
		{
			channel = new Channel($"localhost:{port}", ChannelCredentials.Insecure, new []
			{
				new ChannelOption("grpc.keepalive_permit_without_calls", 1),
				new ChannelOption("grpc.keepalive_time_ms", 5000),
				new ChannelOption("grpc.keepalive_timeout_ms", 30000),
				new ChannelOption("grpc.optimization_target", "latency"),
			});
			serviceRunner = new ServiceRunner.ServiceRunnerClient(channel);
			stream = serviceRunner.RunServiceTaskAsync();
		}

		public void SendRequest(ServiceTaskRunRequest request)
		{
			try
			{
				stream.RequestStream.WriteAsync(request).Wait();
			}
			catch (InvalidOperationException ex)
				when (string.Equals(
						string.Join(string.Empty, ex.Message.Where(c => !char.IsWhiteSpace(c))), // Grpc uses custom whitespace, whitespace stripped for comparison
						"Requeststreamhasalreadybeencompleted.",
						StringComparison.OrdinalIgnoreCase))
			{
				throw new HostGrpcIsClosedException(ex);
			}
		}

		public void Close()
		{
			stream.RequestStream.CompleteAsync().Wait();
		}

		public async Task<bool> NextResponseAsync(CancellationToken cancellationToken)
		{
			return await stream.ResponseStream.MoveNext(cancellationToken);
		}

		public ServiceTaskRunResponse CurrentResponse => stream.ResponseStream.Current;

		public void Dispose()
		{
			stream?.Dispose();
			channel?.ShutdownAsync().Wait();
		}

		readonly Channel channel;
		readonly ServiceRunner.ServiceRunnerClient serviceRunner;
		readonly AsyncDuplexStreamingCall<ServiceTaskRunRequest, ServiceTaskRunResponse> stream;
	}
}
