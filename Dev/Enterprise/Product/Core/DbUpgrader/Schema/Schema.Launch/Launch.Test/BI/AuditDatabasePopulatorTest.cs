using System;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Bi.ConfigLoader;
using CargoWise.Data;
using CargoWise.Data.Testing;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.Schema
{
	sealed class AuditDatabasePopulatorTest : TestCase
	{
		[UseSnapshotProtection]
		public void TestAuditTablesReadPermission()
		{
			using (var writerConnection = Db.NewExtraUnrestrictedWriterConnection(Db.ServerName, Db.DatabaseName, null))
			{
				CombineAssertions("The audit loader does not have permission to run the below queries. Remove the tables from Audit in the BAM tool. Note this test does not run queries on EDI tables.", () =>
				{
					foreach (var table in ConfigData.CdcTableConfig.Where(t => t.TableInAudit && !t.IsEdiClient))
					{
						var query = $"SELECT TOP(0) * FROM [{table.SourceSchema}].[{table.SourceTable}]";
						AssertNoExceptionThrown(query, () => writerConnection.ExecuteNonQuery(query));
					}
				});
			}
		}

		public void TestIsTableConfigurationPopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				var populator = new AuditDatabasePopulator(connection);
				var auditTables = ConfigData.CdcTableConfig.Where(t => t.TableInAudit);

				AssertEquals("Table Configuration count", auditTables.Count(), Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM biadmin.TableConfiguration")));

				var sqlText = @"
SELECT AuditFilter, TableColumnList,TableColumnConvertList,ISNULL(PkName, '') AS PkName
FROM biadmin.TableConfiguration
WHERE SourceSchemaName = @SourceSchemaName AND
	SourceTableName = @SourceTableName";

				CombineAssertions(() =>
				{
					foreach (var table in auditTables)
					{
						using (var cmd = connection.Command(sqlText))
						{
							cmd.AddParameter("@SourceSchemaName", SqlDbType.VarChar, table.SourceSchema);
							cmd.AddParameter("@SourceTableName", SqlDbType.VarChar, table.SourceTable);

							using (var reader = cmd.ExecuteReader())
							{
								if (reader.Read())
								{
									var cdcColumnConfig = table.GetOrderedCdcColumnConfigRows().Where(c => c.ColumnInAudit);

									var expectedAuditFilter = table.AuditFilter ?? string.Empty;
									var expectedColumnList = string.Format(CultureInfo.InvariantCulture, "__$start_lsn,__$seqval,__$operation,__$update_mask,__$command_id,{0}", string.Join(",", cdcColumnConfig.Select(c => c.SourceColumn)));
									var expectedColumnConvertList = string.Format(CultureInfo.InvariantCulture, "__$start_lsn,__$seqval,__$operation,__$update_mask,__$command_id,{0}", string.Join(",", cdcColumnConfig.Select(c => (c.DataType == "xml") ? ($"convert(nvarchar(max),{c.SourceColumn}) AS {c.SourceColumn}") : ($"{c.SourceColumn}"))));
									var pkName = cdcColumnConfig.FirstOrDefault(c => c.IsPrimaryKey)?.SourceColumn;
									var expectedPkName = string.IsNullOrEmpty(pkName) ? string.Empty : pkName;

									var actualAuditFilter = reader["AuditFilter"].ToString();
									var actualTableColumnList = reader["TableColumnList"].ToString();
									var actualTableColumnConvertList = reader["TableColumnConvertList"].ToString();
									var actualPkName = reader["PkName"].ToString();

									AssertEquals("AuditFilter", expectedAuditFilter, actualAuditFilter);
									AssertEquals("TableColumnList", expectedColumnList, actualTableColumnList);
									AssertEquals("TableColumnConvertList", expectedColumnConvertList, actualTableColumnConvertList);
									AssertEquals("PkName", expectedPkName, actualPkName);
								}
								else
								{
									Fail(string.Format("Table {0} not in TableConfiguration", table.SourceTable));
								}
							}
						}
					}
				});
			}
		}

		public void TestIsTableStatePopulated()
		{
			using (var connection = Db.NewAdminConnection(Db.AuditDatabaseName))
			{
				var auditTables = ConfigData.CdcTableConfig.Where(t => t.TableInAudit);
				AssertEquals("Table State count", auditTables.Count(), Convert.ToInt32(connection.ExecuteScalar("SELECT COUNT(*) FROM biadmin.TableState")));

				var sqlText = @"IF EXISTS(SELECT NULL FROM biadmin.TableState WHERE SourceTableName = @SourceTableName AND SourceSchemaName = @SourceSchemaName) SELECT 1 ELSE SELECT 0";

				foreach (var table in auditTables)
				{
					using (var cmd = connection.Command(sqlText))
					{
						cmd.AddParameter("@SourceSchemaName", SqlDbType.VarChar, table.SourceSchema);
						cmd.AddParameter("@SourceTableName", SqlDbType.VarChar, table.SourceTable);

						Assert(string.Format("Table {0} not in TableState", table.SourceSchema + "." + table.SourceTable), Convert.ToBoolean(cmd.ExecuteScalar()));
					}
				}
			}
		}

		BiConfigurationData ConfigData
		{
			get
			{
				return configData ?? (configData = BiAutomationConfigLoader.Instance.ConfigData);
			}
		}
		BiConfigurationData configData;
	}
}
