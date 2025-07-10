using System;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ServiceManager.Shared;
using ServiceManager.Common.Abstractions;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public class GrpcClientSynchronizer : IGrpcClientSynchronizer
	{
		public GrpcClientSynchronizer(GrpcEventHandleNames eventHandleNames)
		{
			this.eventHandleNames = eventHandleNames ?? throw new ArgumentNullException(nameof(eventHandleNames));
			eventWaitHandleHost = new EventWaitHandle(false, EventResetMode.AutoReset, this.eventHandleNames.HostEventWaitHandleName);
			eventWaitHandleRunner = new EventWaitHandle(false, EventResetMode.AutoReset, this.eventHandleNames.RunnerEventWaitHandleName);
		}

		public bool WaitForServerReadySignal(
			IRunnableServiceTask task,
			IGrpcPortResolver portResolver,
			IProcess process,
			TimeSpan timeout)
		{
			SignalStdOutIsReadyToReceive();
			if (!WaitForGrpcPort(timeout, portResolver, process))
			{
				return false;
			}

			return true;
		}

		bool WaitForGrpcPort(TimeSpan timeoutTimeSpan, IGrpcPortResolver portResolver, IProcess process)
		{
			if (!portResolver.PortOpened)
			{
				var stopwatch = Stopwatch.StartNew();
				do
				{
					if (process == null || process.HasExited)
					{
						return false;
					}
				} while (stopwatch.ElapsedMilliseconds < timeoutTimeSpan.TotalMilliseconds && !eventWaitHandleHost.WaitOne(TimeSpan.FromSeconds(5), false));
			}

			using (var timeoutSource = new CancellationTokenSource(TimeSpan.FromSeconds(5)))
			{
				while (!timeoutSource.Token.IsCancellationRequested && !portResolver.PortOpened)
				{
					// Allow some time for StdOut to be processed
					Task.Delay(TimeSpan.FromMilliseconds(10)).Wait();
				}
			}

			return portResolver.PortOpened;
		}

		void SignalStdOutIsReadyToReceive()
		{
			eventWaitHandleRunner.Set();
		}

		public void Dispose()
		{
			eventWaitHandleHost.Dispose();
			eventWaitHandleRunner.Dispose();
		}

		readonly GrpcEventHandleNames eventHandleNames;
		readonly EventWaitHandle eventWaitHandleHost;
		readonly EventWaitHandle eventWaitHandleRunner;
	}
}
