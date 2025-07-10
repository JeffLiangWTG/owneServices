using System;
using System.Threading;
using System.Threading.Tasks;
using Enterprise.Client.Common;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;

namespace ServiceManager.Host.CW
{
	class OldVersionsRemoverTask : IServiceManagerTask
	{
		public OldVersionsRemoverTask(IHostLogger hostLogger)
		{
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
		}

		public string Name { get; } = typeof(OldVersionsRemoverTask).FullName;

		public TimeSpan RunDelay { get; } = TimeSpan.FromDays(1);

		public TimeSpan ErrorDelay { get; } = TimeSpan.FromHours(4);

		public void Initialise(CancellationToken cancellationToken)
		{
		}

		protected virtual void RunOldVersionsRemover(CancellationToken cancellationToken)
		{
			OldVersionsRemover.Run(InstallationEnvironment.Instance.BaseInstallPath,
				ReleaseInfo.Instance.VersionNumber.ToVersion());
		}

		public void Run(CancellationToken cancellationToken)
		{
			hostLogger.Log(LogLevel.Debug, "Deleting old versions.");

			Task.Run(() => RunOldVersionsRemover(cancellationToken), cancellationToken);

			hostLogger.Log(LogLevel.Debug, "Started OldVersionsRemover in a new thread.");
		}

		readonly IHostLogger hostLogger;
	}
}
