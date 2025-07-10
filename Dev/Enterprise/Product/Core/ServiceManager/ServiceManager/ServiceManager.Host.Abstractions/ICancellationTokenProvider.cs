using System.Threading;

namespace ServiceManager.Host.Abstractions
{
	public interface ICancellationTokenProvider
	{
		public CancellationToken Token { get; }
	}
}
