using System;

namespace Enterprise.ServiceManager.Host
{
	interface IControllerUpgrade
	{
		bool IsUpgrading { get; }
		TimeSpan TimeSinceLastUpgradeCheck { get; }
		void UpgradeSoftwareIfNeeded();
	}
}
