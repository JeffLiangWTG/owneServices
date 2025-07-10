using System;
using System.Collections.Generic;
using Enterprise.DbUpgrader.Resource.Version;
using Enterprise.Upgrades;

namespace Enterprise.DbUpgrader.Startup
{
	class UnattendedUpgradeManager : UpgradeManager
	{
		public UnattendedUpgradeManager(IVersionChangeInfo versionInfo, UpgradeInfo softwareUpgrade)
			: base(versionInfo, softwareUpgrade)
		{
		}

		protected override bool GetConfirmationToDisconnectUsers(KeyValuePair<string, string>[] detailLines)
		{
			return true;
		}

		protected override bool GetConfirmationIfUserAttended(string title, string message, string[] detailLines)
		{
			return true;
		}

		int currentRetry;
		const int maxRetries = 30;

		protected override bool ShouldRetryToReleaseLockout
		{
			get
			{
				currentRetry++;
				return currentRetry <= maxRetries;
			}
		}

		protected override TimeSpan RetryWaitingPeriod
		{
			get { return TimeSpan.FromMinutes(1); }
		}
	}
}
