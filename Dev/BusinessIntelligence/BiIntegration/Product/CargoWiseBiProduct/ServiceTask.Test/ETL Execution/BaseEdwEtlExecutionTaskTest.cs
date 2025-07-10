using System.Data;
using System.Linq;
using CargoWise.Bi.Common;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using Enterprise.ChangeDataCapture.Common;
using Enterprise.ChangeDataCapture.Common.Testing;
using NUnit.Framework;

namespace CargoWise.Bi.Product.ServiceTask.Testing
{
	abstract class BaseEdwEtlExecutionTaskTest : TestCase
	{
		#region ETL execution

		protected void TruncateTables(DbConnection connection)
		{
			connection.ExecuteNonQuery(string.Format("TRUNCATE TABLE [{0}].[biadmin].[MasterState]", Db.EdwDatabaseName));
			connection.ExecuteNonQuery(string.Format(@"
UPDATE [{0}].[{1}].[StagingTableState]
SET
	CurrentState = 'New',
	CurrentMaxLsn = 0x0,
	InitialLoadRequired = 1,
	InitialLoadRecordCount = NULL,
	InitialLoadDurationMs = NULL,
	IncrementalLoadRecordCount = NULL,
	IncrementalLoadDurationMs = NULL,
	SqlErrorMessage = NULL,
	StateModifiedTimestamp = GETDATE()

UPDATE [{0}].[{1}].[TransformTableState]
SET
	CurrentState = 'New',
	InitialLoadRequired = 1,
	InitialTransformRecordCount = NULL,
	InitialTransformDurationMs = NULL,
	MergeTransformInsertRecordCount = NULL,
	MergeTransformInsertDurationMs = NULL,
	MergeTransformDeleteRecordCount = NULL,
	MergeTransformDeleteDurationMs = NULL,
	IndexReorganizeDurationMs = NULL,
	SqlErrorMessage = NULL,
	StateModifiedTimestamp = GETDATE()", Db.EdwDatabaseName, BiConstants.BiAdminSchemaName));
		}

		protected void EnableCdc(AdminConnection connection)
		{
			if (!CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.Enable(connection, Db.DatabaseName);
			}

			foreach (var (schema, table) in BiAutomationConfigLoader.Instance.ConfigData.CdcTableConfig.Where(t => t.TableInEdw && !t.IsEdiClient).Select(t => (t.SourceSchema, t.SourceTable)))
			{
				var cdcTable = new CdcTableForTesting(schema, table);
				if (!cdcTable.IsCdcEnabled(connection))
				{
					cdcTable.EnableCdc(connection);
				}
			}
		}

		protected void DisableCdc(AdminConnection connection)
		{
			if (CdcDatabase.IsEnabled(connection, Db.DatabaseName))
			{
				CdcDatabase.Disable(connection, Db.DatabaseName);
			}
		}

		protected void CreateChangeOnATable(DbConnection connection)
		{
			connection.ExecuteNonQuery("UPDATE dbo.RefCurrency SET RX_IsActive = CASE RX_IsActive WHEN 1 THEN 0 ELSE 1 END");
		}

		#endregion
	}
}
