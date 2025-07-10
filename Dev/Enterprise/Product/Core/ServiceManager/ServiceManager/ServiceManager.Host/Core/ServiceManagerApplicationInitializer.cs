using System;
using System.Diagnostics.CodeAnalysis;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Microsoft.Extensions.Logging;
using ServiceManager.Host.Abstractions;
using static System.FormattableString;

namespace Enterprise.ServiceManager.Host
{
	class ServiceManagerApplicationInitializer : IServiceManagerApplicationInitializer
	{
		public ServiceManagerApplicationInitializer(IControllerUpgrade upgrader,
			ITcpIpRegistryAdjuster tcpIpRegistryAdjuster,
			IHostLogger hostLogger,
			IServiceTaskLocksCleaner serviceTaskLocksCleaner,
			IHostApplicationLockAcquirer hostApplicationLockAcquirer)
		{
			this.upgrader = upgrader ?? throw new ArgumentNullException(nameof(upgrader));
			this.tcpIpRegistryAdjuster = tcpIpRegistryAdjuster ?? throw new ArgumentNullException(nameof(tcpIpRegistryAdjuster));
			this.hostLogger = hostLogger ?? throw new ArgumentNullException(nameof(hostLogger));
			this.serviceTaskLocksCleaner = serviceTaskLocksCleaner ?? throw new ArgumentNullException(nameof(serviceTaskLocksCleaner));
			this.hostApplicationLockAcquirer = hostApplicationLockAcquirer ?? throw new ArgumentNullException(nameof(hostApplicationLockAcquirer));
		}

		[SuppressMessage("CargoWiseOne", "CW1115:DoNotUseSetUserContext", Justification = "Baseline")]
		public void Initialize()
		{
			hostLogger.Log(LogLevel.Information, Invariant($"Version: {ReleaseInfo.Instance.VersionNumber}"));

			hostApplicationLockAcquirer.AcquireHostApplicationLock();

			upgrader.UpgradeSoftwareIfNeeded();
			if (upgrader.IsUpgrading)
			{
				return;
			}

			Env.SetUserContext(new UserContext(User.ServiceUserName, Guid.Empty, Guid.Empty));

			serviceTaskLocksCleaner.ReleaseLocksFromHost();
			tcpIpRegistryAdjuster.TryAdjustIfRequired(hostLogger);
		}

		readonly IHostApplicationLockAcquirer hostApplicationLockAcquirer;
		readonly IServiceTaskLocksCleaner serviceTaskLocksCleaner;
		readonly IHostLogger hostLogger;
		readonly IControllerUpgrade upgrader;
		readonly ITcpIpRegistryAdjuster tcpIpRegistryAdjuster;
	}
}
