using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ServiceManager.Shared;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;
using ServiceManagerProto;

namespace Enterprise.ServiceManager.Host
{
	class RunnerCommandQueueProvider : IRunnerCommandQueueProvider
	{
		public RunnerCommandQueueProvider(IHostLogger hostLogger, int port, IErrorReporterProxy errorReporterProxy)
		{
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			serviceRunner = new ServiceRunnerClientWrapper(port);
		}

		internal RunnerCommandQueueProvider(IHostLogger hostLogger, IServiceRunnerClientWrapper serviceRunner, IErrorReporterProxy errorReporterProxy)
		{
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.errorReporterProxy = errorReporterProxy ?? throw new ArgumentNullException(nameof(errorReporterProxy));
			this.serviceRunner = serviceRunner;
		}

		public void Run(string assemblyName, string code, Guid id, string configString)
		{
			SendRequest(new ServiceTaskRunRequest
			{
				Command = RequestCommandType.DirectRun,
				AssemblyName = assemblyName,
				Code = code,
				GuidId = id.ToString(),
				ConfigString = configString,
			});
		}

		public void Run(string assemblyName, string code, Guid id, DateTime expectedNextRunTime, DateTime nextRunTime, string configString)
		{
			if (expectedNextRunTime.Kind != DateTimeKind.Utc)
			{
				hostLogger.Log(LogLevel.Warning, $"Expected next runtime in request for [{code}] had incorrect DateTimeKind: [{System.Enum.GetName(typeof(DateTimeKind), expectedNextRunTime.Kind)}]. Scheduled time may not be as expected");
				errorReporterProxy.ReportDeveloperExceptionOnce("Incorrect dateTimeKind on expected next run time", new FormatException($"Expected next runtime in request for [{code}] had incorrect DateTimeKind: [{System.Enum.GetName(typeof(DateTimeKind), expectedNextRunTime.Kind)}]"));
			}
			if (nextRunTime.Kind != DateTimeKind.Utc)
			{
				hostLogger.Log(LogLevel.Warning, $"Next runtime in request for [{code}] had incorrect DateTimeKind: [{System.Enum.GetName(typeof(DateTimeKind), nextRunTime.Kind)}]. Scheduled time may not be as expected");
				errorReporterProxy.ReportDeveloperExceptionOnce("Incorrect dateTimeKind on next run time", new FormatException($"Expected next runtime in request for [{code}] had incorrect DateTimeKind: [{System.Enum.GetName(typeof(DateTimeKind), nextRunTime.Kind)}]"));
			}

			SendRequest(new ServiceTaskRunRequest
			{
				Command = RequestCommandType.ScheduledRun,
				AssemblyName = assemblyName,
				Code = code,
				GuidId = id.ToString(),
				NextRunTime = Timestamp.FromDateTime(nextRunTime.ToUniversalTime()),
				ExpectedNextRunTime = Timestamp.FromDateTime(expectedNextRunTime.ToUniversalTime()),
				ConfigString = configString,
			});
		}

		public void Stop()
		{
			SendRequest(new ServiceTaskRunRequest
			{
				Command = RequestCommandType.Stop,
			});
		}

		public void Close()
		{
			lock (writeLock)
			{
				serviceRunner.Close();
			}

			isClosing = true;
			responseReaderTask?.Wait();
		}

		public void StartResponseTask(Action<ServiceTaskRunResponse> responseAction, CancellationToken cancellationToken)
		{
			responseReaderTask = Task.Run(async () => await ResponseReaderTaskAsync(responseAction, cancellationToken));
		}

		async Task ResponseReaderTaskAsync(Action<ServiceTaskRunResponse> responseAction, CancellationToken cancellationToken)
		{
			try
			{
				while (await serviceRunner.NextResponseAsync(cancellationToken))
				{
					if (isClosing)
					{
						continue;
					}

					responseAction(serviceRunner.CurrentResponse);
					if (cancellationToken.IsCancellationRequested)
					{
						return;
					}
				}
			}
			catch (RpcException ex)
			{
				hostLogger.LogDebug(ex, "gRPC has been closed");
			}
		}

		public void Dispose()
		{
			try
			{
				serviceRunner.Dispose();
			}
			catch (RpcException ex)
			{
				hostLogger.LogDebug(ex, "gRPC has been closed");
			}
		}

		void SendRequest(ServiceTaskRunRequest request)
		{
			lock (writeLock)
			{
				serviceRunner.SendRequest(request);
			}
		}

		readonly IHostLogger hostLogger;
		readonly IServiceRunnerClientWrapper serviceRunner;
		readonly IErrorReporterProxy errorReporterProxy;
		bool isClosing;
		Task responseReaderTask;
		readonly object writeLock = new object();
	}
}
