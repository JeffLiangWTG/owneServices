using System;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class ServiceStopper : IServiceStopRequestConsumer
	{
		public ServiceStopper(IDelayProvider delayProvider, ICancellationTokenProvider cancellationTokenProvider)
		{
			this.delayProvider = delayProvider ?? throw new ArgumentNullException(nameof(delayProvider));
			this.cancellationTokenProvider = cancellationTokenProvider ?? throw new ArgumentNullException(nameof(cancellationTokenProvider));
		}

		public bool WaitForServiceStopRequest(TimeSpan timeSpan)
		{
			try
			{
				delayProvider.Delay(timeSpan, cancellationTokenProvider.Token);
				return cancellationTokenProvider.Token.IsCancellationRequested;
			}
			catch (OperationCanceledException)
			{
				return cancellationTokenProvider.Token.IsCancellationRequested;
			}
		}

		readonly IDelayProvider delayProvider;
		readonly ICancellationTokenProvider cancellationTokenProvider;
	}
}
