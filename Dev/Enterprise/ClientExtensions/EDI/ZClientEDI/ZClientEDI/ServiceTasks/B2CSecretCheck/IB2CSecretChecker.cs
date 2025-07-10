using System;

namespace Enterprise.Client.EDI.ServiceTasks.B2CSecretCheck
{
	interface IB2CSecretChecker
	{
		DateTime GetExpiryDate();

		bool IsCheckerReady(out string notReadyMessage);
	}
}
