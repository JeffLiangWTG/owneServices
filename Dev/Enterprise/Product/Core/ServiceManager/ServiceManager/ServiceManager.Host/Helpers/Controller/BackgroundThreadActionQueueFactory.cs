using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.ServiceManager.HostsController;
using ServiceManager.Host.Abstractions;

namespace Enterprise.ServiceManager.Host
{
	public sealed class BackgroundThreadActionQueueFactory(ICancellationTokenProvider cancellationProvider) : IBackgroundThreadActionQueueFactory
	{
		readonly IBackgroundThreadActionQueue backgroundThreadActionQueue = new BackgroundThreadActionQueue(cancellationProvider.Token);

		public IBackgroundThreadActionQueue BackgroundThreadActionQueue => backgroundThreadActionQueue;
	}
}
