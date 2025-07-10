using System;
using System.Threading;
using Enterprise.Integration.Licensing;
using ServiceManager.Integration.ServiceHostUtilities;
using static System.FormattableString;
using ServiceType = ServiceManager.Integration.ServiceHostUtilities.ServiceHostProcess.ServiceType;

namespace Enterprise.ServiceManager.Host
{
	class HostApplicationLockAcquirer : IHostApplicationLockAcquirer
	{
		public HostApplicationLockAcquirer(IApplicationEmergencyExit applicationEmergencyExit, IProductRegistration productRegistration)
		{
			this.applicationEmergencyExit = applicationEmergencyExit ?? throw new ArgumentNullException(nameof(applicationEmergencyExit));
			this.productRegistrationKey = (productRegistration ?? throw new ArgumentNullException(nameof(productRegistration))).Key;
		}

		public void AcquireHostApplicationLock()
		{
			var applicationLockKey = ServiceHostProcess.GetServiceName(ServiceType.ProcessController, productRegistrationKey.ServerCode, productRegistrationKey.EnterpriseCode);
			semaphore = new Semaphore(1, 1, applicationLockKey, out var createdNew);

			if (createdNew)
			{
				return;
			}

			var logMessage = Invariant($"Failed to acquire a lock on '{applicationLockKey}' to start the host because another instance of Process Controller Host is running on {System.Environment.MachineName}.");
			applicationEmergencyExit.ExitApplicationUnsafe(logMessage);
		}

		public void Dispose()
		{
			semaphore?.Dispose();
		}

		Semaphore semaphore;
		readonly IApplicationEmergencyExit applicationEmergencyExit;
		readonly IProductRegistrationKey productRegistrationKey;
	}
}
