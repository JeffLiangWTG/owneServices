using System;
using System.Threading;
using CargoWise.Data;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW;

class RefreshRegistryTask : IServiceManagerTask
{
	public RefreshRegistryTask(IHostRegistry hostRegistry)
	{
		this.hostRegistry = hostRegistry;
	}

	public void Initialise(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return;
		}

		Db.DisableThreadSchemaVersionCheckPermanently();
	}

	public void Run(CancellationToken cancellationToken)
	{
		if (cancellationToken.IsCancellationRequested)
		{
			return;
		}

		try
		{
			using (Db.DisposableActionForDbConnection())
			{
				hostRegistry.Refresh();
			}
		}
		catch
		{
			// All exceptions are caught and ignored, even critical ones, since the settings in memory can still be used.
		}
	}

	public string Name => nameof(RefreshRegistryTask);
	public TimeSpan RunDelay => TimeSpan.FromMinutes(30);
	public TimeSpan ErrorDelay => RunDelay;

	readonly IHostRegistry hostRegistry;
}
