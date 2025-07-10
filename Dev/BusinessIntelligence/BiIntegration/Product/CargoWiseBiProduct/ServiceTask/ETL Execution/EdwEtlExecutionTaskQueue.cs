using CargoWise.Bi.Common;
using CargoWise.Data;

namespace CargoWise.Bi.Product.ServiceTask
{
	#region SuppressResourceStringsCheckRegion

	public class EdwEtlExecutionTaskQueue : EtlExecutionTaskQueue
	{
		protected override string BiServer => BiServers.LoadDataWarehouseServerUsingCacheIfPossible(Db.Connection);
		protected override string BiDbName => Db.EdwDatabaseName;
		protected override string LsnHwmParameter => BiConstants.MaxLsnToBeProcessed;
	}

	#endregion
}
