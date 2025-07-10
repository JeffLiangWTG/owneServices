using System;
using System.Threading;

namespace ServiceManager.Host.Abstractions
{
	public interface IServiceManagerTask
	{
		string Name { get; }
		TimeSpan RunDelay { get; }
		TimeSpan ErrorDelay { get; }
		void Initialise(CancellationToken cancellationToken);
		void Run(CancellationToken cancellationToken);
	}
}
