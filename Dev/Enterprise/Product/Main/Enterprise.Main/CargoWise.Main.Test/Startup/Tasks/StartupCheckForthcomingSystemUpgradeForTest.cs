using System;

namespace Enterprise.Startup.Testing
{
	sealed class StartupCheckForthcomingSystemUpgradeForTest : StartupCheckForthcomingSystemUpgrade
	{
		public DateTime GetSystemUpgradeDateTime_Exposed()
		{
			return base.GetSystemUpgradeDateTime();
		}
	}
}
