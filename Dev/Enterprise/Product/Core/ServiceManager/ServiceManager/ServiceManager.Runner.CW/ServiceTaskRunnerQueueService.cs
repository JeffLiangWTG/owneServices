using System.Threading.Channels;
using Enterprise.ServiceManager.Shared;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ServiceManager.Runner.Abstractions;
using ServiceManagerProto;
using Channel = System.Threading.Channels.Channel;
using Status = ServiceManagerProto.Status;

namespace Enterprise.ServiceManager.Runner
{
	public class ServiceTaskRunnerQueueService : IServiceTaskRunnerQueueService
	{
		public ServiceTaskRunnerQueueService(
			IHostCommunicationStrategy hostCommunicationStrategy,
			IServiceTaskRunnerCanceler serviceTaskRunnerCanceler,
			IRunnerLogger runnerLogger,
			IGrpcLogger grpcLogger,
			IRunnerRegistrySettings runnerRegistry)
		{
			this.hostCommunicationStrategy = hostCommunicationStrategy ?? throw new ArgumentNullException(nameof(hostCommunicationStrategy));
			this.serviceTaskRunnerCanceler = serviceTaskRunnerCanceler ?? throw new ArgumentNullException(nameof(serviceTaskRunnerCanceler));
			this.runnerLogger = runnerLogger ?? throw new ArgumentNullException(nameof(runnerLogger));
			this.grpcLogger = grpcLogger ?? throw new ArgumentNullException(nameof(grpcLogger));
			this.runnerRegistry = runnerRegistry ?? throw new ArgumentNullException(nameof(runnerRegistry));
		}

		public IServiceTaskRunnerQueue Start(string grpcWaitHandleNameBase)
		{
			try
			{
				GrpcEnvironment.SetThreadPoolSize(1);
				GrpcEnvironment.SetHandlerInlining(true);
				GrpcEnvironment.SetLogger(grpcLogger);
			}
			catch (InvalidOperationException)
			{
				runnerLogger.Log(LogLevel.Debug, "grpc environment already initialized");
			}

			var lockObject = new object();
			var messageWaitHandle = new ManualResetEvent(false);
			var serverState = new StreamStatus();
			var grpcEventHandleNames = new GrpcEventHandleNames(grpcWaitHandleNameBase);
			var responseChannel = Channel.CreateUnbounded<ServiceTaskRunResponse>();
			var requestChannel = Channel.CreateUnbounded<ServiceTaskRunRequest>();
			var server = new Server(new[]
			{
				new ChannelOption("grpc.keepalive_permit_without_calls", 1),
				new ChannelOption("grpc.keepalive_time_ms", 5000),
				new ChannelOption("grpc.keepalive_timeout_ms", 30000),
				new ChannelOption("grpc.optimization_target", "latency"),
			})
			{
				Services = { ServiceRunner.BindService(new ServiceRunnerGrpcImpl(requestChannel, responseChannel, serviceTaskRunnerCanceler, messageWaitHandle, lockObject, runnerLogger, serverState)) },
				Ports = { new ServerPort("localhost", 0, ServerCredentials.Insecure) },
			};

			if (!EventWaitHandle.TryOpenExisting(grpcEventHandleNames.RunnerEventWaitHandleName, out var hostReadyEvent))
			{
				throw new RunnerGrpcInitializationException("Failed to connect to Runner wait handle");
			}
			hostReadyEvent.WaitOne(TimeSpan.FromSeconds(10), false);
			if (!EventWaitHandle.TryOpenExisting(grpcEventHandleNames.HostEventWaitHandleName, out var runnerReadyEvent))
			{
				throw new RunnerGrpcInitializationException("Failed to connect to Host wait handle");
			}

			server.Start();
			hostCommunicationStrategy.GrpcPortOpened(server.Ports.Single().BoundPort);
			using (runnerReadyEvent)
			using (hostReadyEvent)
			{
				runnerReadyEvent.Set();
				hostReadyEvent.Set();
			}

			return new ServiceTaskRunnerQueue(server, serverState, requestChannel, responseChannel, messageWaitHandle, lockObject, runnerLogger, runnerRegistry);
		}

		readonly IHostCommunicationStrategy hostCommunicationStrategy;
		readonly IServiceTaskRunnerCanceler serviceTaskRunnerCanceler;
		readonly IRunnerLogger runnerLogger;
		readonly IGrpcLogger grpcLogger;
		readonly IRunnerRegistrySettings runnerRegistry;

		class ServiceTaskRunnerQueue : IServiceTaskRunnerQueue
		{
			public ServiceTaskRunnerQueue(
				Server server,
				StreamStatus streamStatus,
				Channel<ServiceTaskRunRequest> requestChannel,
				Channel<ServiceTaskRunResponse> responseChannel,
				ManualResetEvent messageWaitHandle,
				object lockObject,
				IRunnerLogger runnerLogger,
				IRunnerRegistrySettings runnerRegistry)
			{
				this.server = server;
				this.streamStatus = streamStatus;
				this.requestChannel = requestChannel;
				this.responseChannel = responseChannel;
				this.messageWaitHandle = messageWaitHandle;
				this.lockObject = lockObject;
				this.runnerLogger = runnerLogger;
				this.runnerRegistry = runnerRegistry;
			}

			public ICommandInfo? GetNextCommand()
			{
				ServiceTaskRunRequest? request;
				var messageWaitTime = TimeSpan.FromTicks(runnerRegistry.ServiceTaskUnloadTimeout.Ticks * 2);
				messageWaitHandle.WaitOne(messageWaitTime);
				lock (lockObject)
				{
					if (messageWaitHandle.WaitOne(0))
					{
						messageWaitHandle.Reset();
					}

					if (!requestChannel.Reader.TryRead(out request))
					{
						return null;
					}
				}

				switch (request.Command)
				{
					case RequestCommandType.DirectRun:
						return new DirectRunCommandInfo(
							request.AssemblyName,
							request.Code,
							Guid.Parse(request.GuidId),
							request.ConfigString);
					case RequestCommandType.ScheduledRun:
						return new ScheduledRunCommandInfo(
							request.AssemblyName,
							request.Code,
							Guid.Parse(request.GuidId),
							request.ExpectedNextRunTime.ToDateTime(),
							request.NextRunTime.ToDateTime(),
							request.ConfigString);
					case RequestCommandType.Stop:
						return new StopCommandInfo();
				}

				return null;
			}

			public void EnqueueResponse(ServiceTaskRunResponse response)
			{
				responseChannel.Writer.TryWrite(response);
			}

			public void CloseStream(bool runnerWasCancelled, CancellationToken cancellationToken)
			{
				runnerLogger.Log(LogLevel.Debug, $"{nameof(ServiceTaskRunnerQueueService)}: Requesting grpc message stream closure");
				var response = runnerWasCancelled
					? new ServiceTaskRunResponse { Status = Status.RunnerExiting, FailureReason = FailureReasonType.RunnerWasCancelled }
					: new ServiceTaskRunResponse { Status = Status.RunnerExiting, };
				EnqueueResponse(response);
				runnerLogger.Log(LogLevel.Debug, $"{nameof(ServiceTaskRunnerQueueService)}: Waiting for grpc message stream closure");
				WaitForExit();

				void WaitForExit()
				{
					while (streamStatus.IsOpen)
					{
						if (cancellationToken.IsCancellationRequested)
						{
							runnerLogger.Log(LogLevel.Debug, $"{nameof(ServiceTaskRunnerQueueService)}: Grpc stream did not close prior to cancellation");
							break;
						}
						Task.Delay(TimeSpan.FromMilliseconds(100)).Wait();
					}
				}
			}

			public void Dispose()
			{
				try
				{
					if (server != null && !server.ShutdownAsync().Wait(serverShutdownTime))
					{
						runnerLogger.Log(LogLevel.Warning, "grpc server could not shutdown gracefully, killing server");
						server.KillAsync().Wait();
					}
				}
				catch (Exception ex)
				{
					runnerLogger.Log(LogLevel.Debug, ex, "Error during grpc shutdown procedure, process is exiting, ignoring");
				}
			}

			readonly Server server;
			readonly StreamStatus streamStatus;
			readonly Channel<ServiceTaskRunRequest> requestChannel;
			readonly Channel<ServiceTaskRunResponse> responseChannel;
			readonly ManualResetEvent messageWaitHandle;
			readonly object lockObject;
			readonly IRunnerLogger runnerLogger;
			readonly TimeSpan serverShutdownTime = TimeSpan.FromSeconds(30);
			readonly IRunnerRegistrySettings runnerRegistry;
	}

		class ServiceRunnerGrpcImpl : ServiceRunner.ServiceRunnerBase
		{
			public ServiceRunnerGrpcImpl(
				Channel<ServiceTaskRunRequest> requests,
				Channel<ServiceTaskRunResponse> responses,
				IServiceTaskRunnerCanceler serviceTaskRunnerCanceler,
				ManualResetEvent messageWaitHandle,
				object lockObject,
				IRunnerLogger runnerLogger,
				StreamStatus streamStatus)
			{
				this.requests = requests;
				this.responses = responses;
				this.serviceTaskRunnerCanceler = serviceTaskRunnerCanceler;
				this.messageWaitHandle = messageWaitHandle;
				this.lockObject = lockObject;
				this.runnerLogger = runnerLogger;
				this.StreamStatus = streamStatus;
			}

			public override async Task RunServiceTaskAsync(IAsyncStreamReader<ServiceTaskRunRequest> requestStream, IServerStreamWriter<ServiceTaskRunResponse> responseStream, ServerCallContext context)
			{
				var tasks = new[]
				{
					ProcessCommandsAsync(requestStream, responseStream, context),
					ProcessResponsesAsync(responseStream, context),
				};

				await Task.WhenAll(tasks);
				StreamStatus.StreamCompleted();
			}

			async Task ProcessCommandsAsync(IAsyncStreamReader<ServiceTaskRunRequest> requestStream, IServerStreamWriter<ServiceTaskRunResponse> responseStream, ServerCallContext context)
			{
				try
				{
					while (await requestStream.MoveNext(context.CancellationToken))
					{
						var request = requestStream.Current;
						await requests.Writer.WriteAsync(request, context.CancellationToken);
						lock (lockObject)
						{
							messageWaitHandle.Set();
						}

						if (request.Command == RequestCommandType.Stop)
						{
							serviceTaskRunnerCanceler.Cancel();
						}
						else
						{
							await responseStream.WriteAsync(new ServiceTaskRunResponse { Status = Status.Queued });
						}
					}
				}
				catch (RpcException e) when (e.Status.StatusCode == StatusCode.Cancelled)
				{
					runnerLogger.Log(LogLevel.Debug, $"{e.GetType()}: Grpc stream was closed by the host, finish task");
				}
				finally
				{
					responses.Writer.Complete();
				}
			}

			async Task ProcessResponsesAsync(IServerStreamWriter<ServiceTaskRunResponse> responseStream, ServerCallContext context)
			{
				try
				{
					while (await responses.Reader.WaitToReadAsync(context.CancellationToken))
					{
						if (responses.Reader.TryRead(out var response))
						{
							await responseStream.WriteAsync(response);
						}
					}
				}
				catch (OperationCanceledException e)
				{
					runnerLogger.Log(LogLevel.Debug, $"{e.GetType()}: Grpc stream was closed by the host, finish task");
				}
				finally
				{
					requests.Writer.Complete();
				}
			}

			StreamStatus StreamStatus { get; }

			readonly Channel<ServiceTaskRunRequest> requests;
			readonly Channel<ServiceTaskRunResponse> responses;
			readonly IServiceTaskRunnerCanceler serviceTaskRunnerCanceler;
			readonly ManualResetEvent messageWaitHandle;
			readonly object lockObject;
			readonly IRunnerLogger runnerLogger;
		}

		class StreamStatus
		{
			public bool IsOpen => isOpen;

			public void StreamCompleted()
			{
				isOpen = false;
			}

			bool isOpen = true;
		}
	}
}
