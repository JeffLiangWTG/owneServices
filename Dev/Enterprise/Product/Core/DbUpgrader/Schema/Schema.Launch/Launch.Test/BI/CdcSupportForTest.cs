using System;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Shared;

namespace Enterprise.DbUpgrader.Schema.OnlineUpgrade.Testing.BI
{
	public class CdcSupportForTest : CdcSupport
	{
		public CdcSupportForTest(IUpgradeManager manager, string dbName, AdminConnection upgConnection) : base(manager, dbName, upgConnection)
		{
		}

		protected override void EnableCdcForSelectedTablesUnsafe()
		{
			var objectId = new Random().Next(1000000000, 2000000000).ToString();
			var enableTraceFlagException = SqlExceptionBuilder.CreateSqlError(3764, 1, 1, Db.Connection.ServerName, $"Cannot alter the procedure 'cdc.sp_batchinsert_{objectId}' because it is being used for Change Data Capture.", "", 1);
			throw SqlExceptionBuilder.CreateSqlException(enableTraceFlagException);
		}
	}
}
