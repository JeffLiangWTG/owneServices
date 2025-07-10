using System;

namespace CargoWise.Data
{
	public interface IDbEnvironment
	{
		int ConnectionTimeout { get; }
		IConnectionPooling ConnectionPooling { get; }
		bool IsServingWebBasedApp { get; }
		IDbConnectionGuiPlugin ConnectionGuiPlugin { get; }
		int DeadlockPriority { get; set; }
		IDisposable DisableTimerDuringDbUpgrade();
		bool IsTimerDisabledDuringDbUpgrade { get; }
		IDbRecoveryModelManager DbRecoveryModelManager { get; }
		IDedicatedSqlServerInstanceDefinition DedicatedSqlServerInstanceDefinition { get; }
	}
}
