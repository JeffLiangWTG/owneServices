using CargoWise.Bi.Common;
using CargoWise.Data;

namespace CargoWise.Bi.Product.ServiceTask
{
	#region SuppressResourceStringsCheckRegion

	public class AuditEtlExecutionTaskQueue : EtlExecutionTaskQueue
	{
		protected override string BiServer => BiServers.LoadAuditServerUsingCacheIfPossible(Db.Connection);
		protected override string BiDbName => Db.AuditDatabaseName;
		protected override string LsnHwmParameter => BiConstants.LastMaxLsnProcessed;
	}

	#endregion
}
