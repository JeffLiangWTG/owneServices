using CargoWise.Types;
using Enterprise.ServiceManager.Shared;

namespace Enterprise.ServiceManager.Business
{
	public class StatusStringProvider
	{
		public string GetStatusString(int status)
		{
			ZString result;
			if (status >= 0)
			{
				if ((status & ServiceManagerHelper.IsBlockedMask) != 0)
				{
					if (status == ServiceManagerHelper.IsBlockedMask ||
						status == (ServiceManagerHelper.IsBlockedMask | ServiceManagerHelper.IsInitializedMask))
					{
						result = ZString.Empty; // not active
					}
					else
					{
						result = ServiceManagerHelper.StatusBlocked;
					}
				}
				else if ((status & ServiceManagerHelper.IsRunningMask) != 0)
				{
					result = ServiceManagerHelper.StatusRunning;
				}
				else if ((status & ServiceManagerHelper.IsLastRunFailedMask) != 0)
				{
					result = ServiceManagerHelper.StatusLastRunFailed;
				}
				else if ((status & ServiceManagerHelper.IsActiveMask) == 0)
				{
					result = ZString.Empty;
				}
				else
				{
					result = ServiceManagerHelper.StatusIdle;
				}
			}
			else
			{
				result = ServiceManagerHelper.StatusUnknown;
			}

			return result;
		}
	}
}

