using System.Collections.Generic;
using CargoWise.EntityFramework;

namespace Enterprise.ZArchitecture.Business
{
	public interface IAuditStmALogDecider
	{
		bool TryGetAutoLogConfigurations(IBusiness bizo, out (bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL) auditLogConfig);
		IDictionary<string, (bool IsEnabledForADD, bool IsEnabledForEDT, bool IsEnabledForDEL)> AutoLoggedTablesDefaultConfigurationValues { get; }
		bool AuditLogConfigNonPersistedForEventCode(IBusiness parent, string eventCode);
		bool IsAutoAdminBusinessObjectLoggerEnabled(BusinessObject bO, EnterpriseBusinessObject.AutologState logState);
	}
}
