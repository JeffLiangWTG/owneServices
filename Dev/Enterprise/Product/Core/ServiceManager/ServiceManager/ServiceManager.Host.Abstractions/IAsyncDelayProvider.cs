using System;
using System.Threading;
using System.Threading.Tasks;

namespace ServiceManager.Host.Abstractions
{
	public interface IAsyncDelayProvider
	{
		Task DelayAsync(TimeSpan delaySpan, CancellationToken cancellationToken);
	}
}
