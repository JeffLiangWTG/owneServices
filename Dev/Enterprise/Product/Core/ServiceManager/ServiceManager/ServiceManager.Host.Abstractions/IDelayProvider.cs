using System;
using System.Threading;

namespace ServiceManager.Host.Abstractions
{
	public interface IDelayProvider
	{
		void Delay(TimeSpan delaySpan);
		void Delay(TimeSpan delaySpan, CancellationToken cancellationToken);
	}
}
