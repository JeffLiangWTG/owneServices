using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using CargoWise.Bi.Development.Common;
using CargoWise.Bi.Development.SchemaSync.DataSets;
using CargoWise.Bi.Development.SsasBuilder;
using CargoWise.BuildTools.Testing;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.DbUpgrader.Resource;
using NUnit.Framework;

namespace CargoWise.Bi.Development.SchemaSync.Testing
{
	[DatCapabilityRequirement("SOURCE_CODE")]
	class DatabaseScriptTestCase : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();

			MockSourceControl.Setup();

			BiFiles.ResetPaths();

			BiDatabase.ServerName = Db.ServerName;
			BiDatabase.AuditDatabaseName = AuditDbName;
			BiDatabase.EdwDatabaseName = EdwDbName;
		}

		protected override void TearDown()
		{
			MockSourceControl.TearDown();

			AdoTestUtils.DropDbIfExists(TestAdminConnection, AuditDbName);
			AdoTestUtils.DropDbIfExists(TestAdminConnection, EdwDbName);
			DisposeTestAdminConnection();

			base.TearDown();
		}

		[UseSnapshotProtection]
		public void TestParsedSchemaAndSchemaFromDatabaseAreEquivalent()
		{
			AssertParsedSchemaAndDatabaseSchemaAreEqual(useBinaries: true);
		}

		// Requires shared source code
		[DeveloperOnlyTest]
		[UseSnapshotProtection]
		public void TestParsedSchemaFromFilesAndSchemaFromDatabaseAreEquivalent()
		{
			var root = Path.GetPathRoot(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile));
			var shared = Path.Combine(root, "git", "GitHub", "WiseTechGlobal", "CargoWise.Shared");
			BiFiles.CWSharedPath = shared;

			AssertParsedSchemaAndDatabaseSchemaAreEqual(useBinaries: false);
		}

		void AssertParsedSchemaAndDatabaseSchemaAreEqual(bool useBinaries)
		{
			var databaseDataSet = new SchemaDataSet();
			using (var connection = Db.NewAdminConnection(BiDatabase.ServerName, Db.SqlMasterDb))
			{
				BiDatabase.CreateTemplateDatabase(connection);
				BiDatabase.LoadSchemaFromDatabase(connection, databaseDataSet);
			}

			var parsedDataSet = new SchemaDataSet();
			BiDatabase.LoadSchemaFromSQL(parsedDataSet, useBinaries: useBinaries);

			var databaseDefinition = databaseDataSet.Definition;
			var parsedDefinition = parsedDataSet.Definition;

			var databaseView = databaseDefinition.DefaultView;
			var parsedView = parsedDefinition.DefaultView;

			databaseView.Sort = "SourceSchema, SourceTable, SourceColumn";
			parsedView.Sort = "SourceSchema, SourceTable, SourceColumn";

			var databaseTable = databaseView.ToTable();
			var parsedTable = parsedView.ToTable();

			var columns = databaseDefinition.Columns;

			for (var i = 0; i < Math.Min(databaseTable.Rows.Count, parsedTable.Rows.Count); i++)
			{
				var databaseRow = databaseTable.Rows[i];
				var parsedRow = parsedTable.Rows[i];

				for (var j = 0; j < columns.Count; j++)
				{
					var column = databaseDefinition.Columns[j].ColumnName;
					var databaseValue = databaseRow[j];
					var parsedValue = parsedRow[j];

					if (!databaseValue.Equals(parsedValue))
					{
						CombineAssertions(() =>
						{
							AssertEquals(
								$"{column} column differs at row {i}",
								databaseValue,
								parsedValue);

							AssertEquals(
								BuildDataRowString(databaseRow, columns.Count),
								BuildDataRowString(parsedRow, columns.Count));
						});
					}
				}
			}

			// In case one they have the first N rows equal but one has extra rows at the end
			AssertEquals(databaseDataSet.Definition.Count, parsedDataSet.Definition.Count);
		}

		string BuildDataRowString(DataRow row, int columnCount)
		{
			var sb = new StringBuilder();
			for (var i = 0; i < columnCount - 1; i++)
			{
				var value = row[i];
				if (value is DBNull || value == null)
				{
					sb.Append("NULL | ");
				}
				else
				{
					sb.Append($"{value} | ");
				}
			}
			var lastValue = row[columnCount - 1];
			if (lastValue is DBNull || lastValue == null)
			{
				sb.Append("NULL");
			}
			else
			{
				sb.Append(lastValue);
			}

			return sb.ToString();
		}

		public void TestCreateAuditDatabasesWithCorrectSchema()
		{
			var config = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData;
			var mainDbSchema = LoadSchemaDataSet(Db.DatabaseName);
			var auditSchema = LoadSchemaDataSet(Db.AuditDatabaseName);

			var auditTables = (
				from otable in mainDbSchema.Definition
				join ctable in config.CdcTableConfig on otable.SourceTable equals ctable.SourceTable
				where
					(ctable.TableInAudit || ctable.TableInEdw) &&
					!(from atable in auditSchema.Definition
					  where atable.SourceTable == otable.SourceTable && atable.SourceSchema == ctable.SourceSchema
					  select atable.SourceTable).Any()
				select otable.SourceTable).ToList();

			Assert($"There are tables missing in Audit database.\r\n{string.Join("\r\n", auditTables)}", !auditTables.Any());

			var auditColumns = (
				from ocolumn in mainDbSchema.Definition
				join ccolumn in config.CdcColumnConfig on ocolumn.SourceColumn equals ccolumn.SourceColumn
				where ccolumn.CdcEnabled &&
				!(from acolumn in auditSchema.Definition
				  where acolumn.SourceColumn == ocolumn.SourceColumn
				  select acolumn.SourceColumn).Any()
				select ccolumn).ToList();

			Assert($"There are columns missing in Audit database.\r\n{string.Join("\r\n", auditColumns.Select(c => $"{c.SourceTable}.{c.SourceColumn}"))}", !auditColumns.Any());
		}

		public void TestCreateEdwDatabasesWithCorrectSchema()
		{
			var config = BiAutomationConfigLoaderForDevelopment.Instance.ConfigData;
			var mainDbSchema = LoadSchemaDataSet(Db.DatabaseName);
			var edwSchema = LoadSchemaDataSet(Db.EdwDatabaseName);

			var edwTables = (
				from otable in mainDbSchema.Definition
				join ctable in config.CdcTableConfig.Where(t => t.TableInEdw)
					on otable.SourceTable equals ctable.SourceTable
				where !(from etable in edwSchema.Definition
						where etable.SourceTable == otable.SourceTable && etable.SourceSchema == "Staging"
						select etable.SourceTable).Any()
				select otable.SourceTable).ToList();

			Assert($"There are tables missing in EDW database.\r\n{string.Join("\r\n", edwTables)}", !edwTables.Any());

			var edwColumns = (
				from ocolumn in mainDbSchema.Definition
				join ccolumn in config.CdcColumnConfig on ocolumn.SourceColumn equals ccolumn.SourceColumn
				join table in config.CdcTableConfig on ccolumn.SourceTable equals table.SourceTable
				where ccolumn.ColumnInEdw && table.TableInEdw &&
				!(from ecolumn in edwSchema.Definition
				  where ecolumn.SourceColumn == ocolumn.SourceColumn
				  select ecolumn.SourceColumn).Any()
				select ocolumn).ToList();

			Assert($"There are columns missing in EDW database.\r\n{string.Join("\r\n", edwColumns.Select(c => $"{c.SourceTable}.{c.SourceColumn}"))}", !edwColumns.Any());
		}

		#region Implementation

		AdminConnection TestAdminConnection
		{
			get { return testAdminConnection ?? (testAdminConnection = Db.NewAdminConnection()); }
		}
		AdminConnection testAdminConnection;

		void DisposeTestAdminConnection()
		{
			if (testAdminConnection != null)
			{
				testAdminConnection.Dispose();
				testAdminConnection = null;
			}
		}

		public static IDisposable InitializePaths(string path)
		{
			BiFiles.BasePath = BaseSourcePath;

			var biConfigFileTypes = Enum.GetValues(typeof(BiConfigFileType));
			foreach (BiConfigFileType biConfigFileType in biConfigFileTypes)
			{
				var biConfigFilePath = Path.Combine(path, biConfigFileType.ToString());
				if (!Directory.Exists(biConfigFilePath))
				{
					Directory.CreateDirectory(biConfigFilePath);
				}
			}

			var generatedConfigFilePath = Path.Combine(path, "AutoGenerated");
			if (!Directory.Exists(generatedConfigFilePath))
			{
				Directory.CreateDirectory(generatedConfigFilePath);
			}

			foreach (var biConfigFile in Directory.EnumerateFiles(BiAutomationConfigLoaderForDevelopment.Instance.BiConfigDirectory, "*.xml", SearchOption.AllDirectories))
			{
				var testBiConfigFilePath = biConfigFile.Replace(BiAutomationConfigLoaderForDevelopment.Instance.BiConfigDirectory, path);
				File.Copy(biConfigFile, testBiConfigFilePath);
				File.SetAttributes(testBiConfigFilePath, FileAttributes.Normal);
			}

			BiFiles.GeneratedAuditTableSchemaPath = path + @"\Schema_Audit.sql";
			BiFiles.GeneratedEDWTableSchemaPath = path + @"\Schema_EDW.sql";

			var manager = new ScriptManager();

			using (var stream = File.OpenWrite(path + @"\Schema_Main.sql"))
			using (var writer = new StreamWriter(stream))
			{
				writer.Write(manager.MaindDbXmlSchemaScript);
				writer.Write(manager.MaindDbSchemaScript);
			}

			BiFiles.MainSchemaFilePath = path + @"\Schema_Main.sql";
			File.SetAttributes(BiFiles.MainSchemaFilePath, FileAttributes.Normal);

			var mainDbProgrammabilityDirectory = Path.Combine(path, "MainDb");
			Directory.CreateDirectory(mainDbProgrammabilityDirectory);
			var definitionAssembly = Assembly.Load("CargoWise.DbUpgrader.Scripts.Definitions");

			const string Prefix = "CargoWise.DbUpgrader.Scripts.Definitions.BusinessIntelligence.MainDb.";

			foreach (var resource in definitionAssembly.GetManifestResourceNames())
			{
				if (resource.StartsWith(Prefix))
				{
					CopyEmbeddedResourceToDisk(definitionAssembly, resource, Path.Combine(mainDbProgrammabilityDirectory, resource.Substring(Prefix.Length)));
				}
			}

			BiFiles.MainDbProgrammabilityDirectory = mainDbProgrammabilityDirectory;

			return BiAutomationConfigLoaderForDevelopment.Instance.ResetConfiguration(path);
		}

		static void CopyEmbeddedResourceToDisk(Assembly assembly, string resourceName, string path)
		{
			using (var resourceStream = assembly.GetManifestResourceStream(resourceName))
			{
				if (resourceStream == null)
				{
					throw new ArgumentException("Resource not found: " + resourceName);
				}

				using (var fileStream = File.OpenWrite(path))
				{
					resourceStream.CopyTo(fileStream);
				}
			}
		}

		public static IDisposable InitializeTabularModelPaths(string path)
		{
			var tabularModelDirectory = SsasProjectBuilder.SsasCubeSourcePath;
			foreach (var tabularModelFile in Directory.GetFiles(tabularModelDirectory, "*.bim", SearchOption.AllDirectories))
			{
				var testTabularModelFile = Path.Combine(path, Path.GetFileName(tabularModelFile));
				File.Copy(tabularModelFile, testTabularModelFile);
				File.SetAttributes(testTabularModelFile, FileAttributes.Normal);
			}
			SsasProjectBuilder.SsasCubeSourcePath = path;

			return new DisposableAction(() =>
			{
				SsasProjectBuilder.SsasCubeSourcePath = tabularModelDirectory;
			});
		}

		SchemaDataSet LoadSchemaDataSet(string dbName)
		{
			var result = new SchemaDataSet();
			result.EnforceConstraints = false;

			var query = new FileInfo(BiFiles.BiDbSchemaQueryFilePath).OpenText().ReadToEnd();

			using (((ICurrentDbControl)TestAdminConnection).UseDatabase(dbName))
			using (var cmd = TestAdminConnection.Command(query))
			{
				var adapter = cmd.NewDataAdapter();
				adapter.Fill(result.Definition);
			}

			return result;
		}

		static readonly string AuditDbName = Db.DatabaseName + "_Test_Audit";
		static readonly string EdwDbName = Db.DatabaseName + "_Test_EDW";

		#endregion
	}
}
