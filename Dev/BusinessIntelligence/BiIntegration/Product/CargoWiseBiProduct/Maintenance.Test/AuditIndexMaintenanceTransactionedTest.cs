using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using NUnit.Framework;

namespace CargoWise.Bi.Maintenance.Testing
{
	class AuditIndexMaintenanceTransactionedTest : TransactionedTestCase
	{
		public void TestGetReorganizedTableIndexList()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].[TableState]
SET IsIndexReorganized = 0", BiConstants.BiAdminSchemaName);
				TestConnection.ExecuteNonQuery(sqlText);

				var auditIndexMaintenance = AuditIndexMaintenance.New(TestConnection, new LoggerForTest());
				AssertEquals("There should be no reorganized indexes.", false, auditIndexMaintenance.GetReorganizedTableIndexList().Any());

				sqlText = string.Format(CultureInfo.InvariantCulture,
@"UPDATE [{0}].[TableState]
SET IsIndexReorganized = 1", BiConstants.BiAdminSchemaName);
				TestConnection.ExecuteNonQuery(sqlText);

				AssertEquals("All tables should have indexes reorganized.", true, auditIndexMaintenance.GetReorganizedTableIndexList().Any());
			}
		}

		public void TestEnsureIndexNameConvention()
		{
			var sqlText = string.Format(@"
select tc.SourceSchemaName, tc.SourceTableName, i.name
from [{0}].[TableConfiguration] tc
inner join sys.indexes i
	on i.object_id = object_id(tc.SourceSchemaName + '.' + tc.SourceTableName)
where i.type = 5",
				BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			using (var cmd = TestConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				CombineAssertions(() =>
				{
					Assert(true);

					while (reader.Read())
					{
						var schema = reader.GetString(0);
						var table = reader.GetString(1);
						var indexName = reader.GetString(2);

						AssertEquals(string.Format("Clustered columnstore index name for [{0}].[{1}]", schema, table), string.Format("cci_{0}_{1}", schema, table), indexName);
					}
				});
			}
		}

		public void TestCdcHistorySummaryIndexReorganize()
		{
			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			{
				var sqlText = @"
IF EXISTS
	(SELECT 1
	FROM sys.TABLES t 
		INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
		INNER JOIN sys.indexes i ON t.object_id = i.object_id
	WHERE i.type = 5
		and t.name = 'CdcHistorySummary'
		and s.name = 'biadmin')
	SELECT 1
ELSE
	SELECT 0";

				Assert("CDC History Summary table should have CCI.", Convert.ToBoolean(TestConnection.ExecuteScalar(sqlText)));
			}
		}

		[UseSnapshotProtection(new[] { DatabaseType.Audit }, skipTransaction: true)]
		public void TestGetRebuiltTableIndexList()
		{
			var sqlText = string.Format(CultureInfo.InvariantCulture,
				@"[{0}].[usp_IndexMaintenance]",
				BiConstants.BiAdminSchemaName);

			using (((ICurrentDbControl)TestConnection).UseDatabase(Db.AuditDatabaseName))
			using (var cmd = TestConnection.Command(sqlText))
			{
				cmd.CommandType = CommandType.StoredProcedure;

				cmd.AddParameter("@timer_in_seconds", SqlDbType.Int, DBNull.Value);
				cmd.AddParameter("@print_messages", SqlDbType.Int, 0);

				cmd.AddOutputParameter("@ErrorCode", SqlDbType.Int, 0, 0, 0, null);
				cmd.AddOutputParameter("@IsIndexRebuilt", SqlDbType.Int, 0, 0, 0, null);
				cmd.AddOutputParameter("@CdcHistorySummaryErrorCode", SqlDbType.Int, 0, 0, 0, null);

				cmd.ExecuteNonQuery();

				var errorCode = (IndexErrorCode)cmd.GetParameterValue("@ErrorCode");
				var isIndexRebuilt = (IndexRebuildResultCode)cmd.GetParameterValue("@IsIndexRebuilt");
				var cdcHistorySummaryErrorCode = (CdcHistorySummaryErrorCode)cmd.GetParameterValue("@CdcHistorySummaryErrorCode");

				AssertEquals("There should be no rebuilt indexes.", IndexRebuildResultCode.NoIndexCorruption, isIndexRebuilt);
			}
		}
	}
}
