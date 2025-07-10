using System;
using System.Threading;
using System.Threading.Tasks;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	class DelayProvider : IDelayProvider, IAsyncDelayProvider
	{
		public void Delay(TimeSpan delaySpan)
		{
			Delay(delaySpan, CancellationToken.None);
		}

		public void Delay(TimeSpan delaySpan, CancellationToken cancellationToken)
		{
			WaitHandle.WaitAny(new[] { cancellationToken.WaitHandle }, delaySpan);
			cancellationToken.ThrowIfCancellationRequested();
		}

		public Task DelayAsync(TimeSpan delaySpan, CancellationToken cancellationToken)
		{
			return Task.Delay(delaySpan, cancellationToken);
		}
	}
}
