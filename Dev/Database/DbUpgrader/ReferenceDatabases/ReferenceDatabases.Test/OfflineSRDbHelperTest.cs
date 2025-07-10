using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.Database.Shared;
using CargoWise.DbUpgrader.Foundation;
using Moq;
using NUnit.Framework;

namespace Enterprise.DbUpgrader.ReferenceDatabases.Test
{
	sealed class OfflineSRDbHelperTest : TestCase
	{
		public void TestResetOfflineSRDbAndPopulateSchema()
		{
			using (var connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();
				AssertNoExceptionThrown(() => OfflineSRDbHelper.ResetSRDbAndPopulateSchema(connection, logger.Object));
				AssertEquals(true, connection.DatabaseExists(RefDbTableNameResolver.DefaultSingleRefDbName));

				((ICurrentDbControl)connection).UseDatabase(RefDbTableNameResolver.DefaultSingleRefDbName);
				var testingTableScripts = typeof(CargoWise.Database.Shared.SRDbOfflineSupportTables.OfflineTableScript).Assembly.GetTypes().Where(x => typeof(CargoWise.Database.Shared.SRDbOfflineSupportTables.OfflineTableScript).IsAssignableFrom(x) && !x.IsInterface && !x.IsAbstract);
				var tableInstances = testingTableScripts.Select(x => (CargoWise.Database.Shared.SRDbOfflineSupportTables.OfflineTableScript)Activator.CreateInstance(x));

				var sql_tableTemplate = "SELECT COUNT(1) FROM sys.tables WHERE name = '{0}'";
				var sql_viewTemplate = "SELECT COUNT(1) FROM sys.views WHERE name = '{0}'";
				var sql_triggerTemplate = "SELECT COUNT(1) FROM sys.triggers WHERE name = '{0}'";
				foreach (var item in tableInstances)
				{
					if (item.IsView)
					{
						AssertEquals(item.TableName + " Physical table should exist when start tests.", 1, connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, sql_viewTemplate, item.TableName)));
					}
					else
					{
						AssertEquals(item.TableName + " Physical view tables should exist when start tests.", 1, connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, sql_tableTemplate, item.TableName)));
					}
					if (!string.IsNullOrEmpty(item.PostCreateScriptForTrigger))
					{
						AssertEquals(item.TriggerName.Replace(DBHelper.SRDbOfflineTableSuffix, string.Empty) + " Trigger should exist when start tests.", 1, connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, sql_triggerTemplate, item.TriggerName.Replace(DBHelper.SRDbOfflineTableSuffix, string.Empty))));
					}
					AssertEquals(item.TableName + " TableView should exist when start tests.", 1, connection.ExecuteScalar(string.Format(CultureInfo.InvariantCulture, sql_viewTemplate, RefDatabaseVersionMapHelper.GetActualViewNameFromCombinedViewMappings(item.TableName))));
				}
			}
		}

		public void TestEnsureGuestHasAllPermissionsAsCwRestrictedWriterRole()
		{
			var sql = @"
	select permission_name,class_desc FROM sys.database_principals princ
	INNER JOIN sys.database_permissions perm
	ON princ.principal_id = perm.grantee_principal_id
	WHERE name = 'guest'
	";
			using (var connection = Db.NewAdminConnection())
			{
				connection.BeginTransaction();
				OfflineSRDbHelper.EnsureGuestHasAllPermissionsAsCwRestrictedWriterRole(connection);
				var permissions = new List<(string class_desc, string permission_name)>();
				connection.ExecuteReader(sql,
						dbRecord =>
						{
							permissions.Add(((string)dbRecord["class_desc"], (string)dbRecord["permission_name"]));
						});
				AssertContainsExactElementsInAnyOrder("Permissions are not correct.", expected, permissions.ToArray());
				connection.RollbackTransaction();
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			logger = new Mock<IUpgradeTaskWorkflowLogger>();
		}

		Mock<IUpgradeTaskWorkflowLogger> logger;
		static readonly (string class_desc, string permission_name)[] expected = {
			("DATABASE", "ALTER"), ("DATABASE", "CREATE SCHEMA"), ("DATABASE", "VIEW DATABASE STATE"), ("DATABASE", "VIEW DEFINITION"), ("DATABASE", "SHOWPLAN"), ("DATABASE", "REFERENCES"),
			("SCHEMA", "SELECT"), ("SCHEMA", "INSERT"), ("SCHEMA", "UPDATE"), ("SCHEMA", "DELETE"), ("SCHEMA", "EXECUTE"), ("SCHEMA", "ALTER"), ("SCHEMA", "CREATE SEQUENCE") };
	}
}
