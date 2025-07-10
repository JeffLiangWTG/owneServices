using System;
using System.Threading;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	sealed class CancellationTokenSourceWrapper : ICancellationTokenProvider, ICancellationRequester, IDisposable
	{
		public CancellationTokenSourceWrapper()
		{
			cancellationTokenSource = new CancellationTokenSource();
		}

		public void Cancel()
		{
			cancellationTokenSource.Cancel();
		}

		public CancellationToken Token => cancellationTokenSource.Token;

		readonly CancellationTokenSource cancellationTokenSource;

		public void Dispose()
		{
			cancellationTokenSource.Dispose();
		}
	}
}
