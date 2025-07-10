using System;

namespace Enterprise.ServiceManager.Host
{
	public interface IApplicationEmergencyExit
	{
		void ExitApplicationUnsafe(string message, Exception ex);
		void ExitApplicationUnsafe(string message);
	}
}