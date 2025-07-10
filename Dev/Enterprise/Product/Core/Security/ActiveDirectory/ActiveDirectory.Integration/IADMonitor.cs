using System;

namespace Enterprise.Security.ActiveDirectory
{
	public interface IADMonitor
	{
		void CheckSynchronisedStatusInRange(Guid lowerPk, Guid upperPk);
	}
}
