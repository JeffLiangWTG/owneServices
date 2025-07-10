using WTG.TestHelpers;

namespace CargoWise.Bi.GlobalMonitoring.Testing
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Xml;
	using CargoWise.BuildTools;
	using CargoWise.Data;
	using CargoWise.IO;
	using NUnit.Framework;

	[DatCapabilityRequirement("SOURCE_CODE")]
	abstract class GlobalMonitoringTest : TestCase
	{
		protected override void SetUp()
		{
			base.SetUp();
			TestAdminConnection = Db.NewAdminConnection(Db.SqlMasterDb);
			AdoTestUtils.DropDbIfExists(TestAdminConnection, TestDbName);
			RestoreTestDatabase(TestAdminConnection, TestDbName);
			((ICurrentDbControl)TestAdminConnection).UseDatabase(TestDbName);
		}

		protected override void TearDown()
		{
			((ICurrentDbControl)TestAdminConnection).UseDatabase(Db.SqlMasterDb);
			AdoTestUtils.DropDbIfExists(TestAdminConnection, TestDbName);
			Dispose(TestAdminConnection);
			base.TearDown();
		}

		void Dispose(IDisposable obj)
		{
			obj?.Dispose();
		}

		protected AdminConnection TestAdminConnection;

		#region Implementation

		#region Compare Data Tables

		protected void CompareDataTables(string description, DataTable expectedDataTable, DataTable actualDataTable)
		{
			var expectedDataColumns = GetColumnNamesFromDataTable(expectedDataTable);
			var actualDataColumns = GetColumnNamesFromDataTable(actualDataTable);

			CompareColumns(description, expectedDataColumns, actualDataColumns);
			CompareRows(description, expectedDataTable, actualDataTable, actualDataColumns);
		}

		void CompareColumns(string description, List<string> expectedDataColumns, List<string> actualDataColumns)
		{
			AssertEquals($"[{description}] Column count", expectedDataColumns.Count, actualDataColumns.Count);
			AssertContainsExactElementsInAnyOrder($"[{description}] Column names", expectedDataColumns, actualDataColumns);
		}

		void CompareRows(string description, DataTable expectedDataTable, DataTable actualDataTable, List<string> dataColumns)
		{
			var expectedDataRows = GetDataRows(expectedDataTable, dataColumns);
			var actualDataRows = GetDataRows(actualDataTable, dataColumns);
			AssertContainsExactElementsInAnyOrder($"[{description}] Data rows", expectedDataRows, actualDataRows);
		}

		List<string> GetColumnNamesFromDataTable(DataTable dataTable)
		{
			var result = new List<string>();
			foreach (DataColumn column in dataTable.Columns)
			{
				result.Add(column.ColumnName);
			}
			return result;
		}

		List<string> GetDataRows(DataTable dataTable, List<string> dataColumns)
		{
			var result = new List<string>();
			foreach (DataRow dataRow in dataTable.Rows)
			{
				var stringBuilder = new StringBuilder();
				foreach (var columnName in dataColumns)
				{
					if (!string.IsNullOrEmpty(stringBuilder.ToString()))
					{
						stringBuilder.Append(",");
					}
					stringBuilder.Append(dataRow[columnName]);
				}
				result.Add(stringBuilder.ToString());
			}
			return result;
		}

		#endregion

		#region Report File

		protected string[] GetDataSets()
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(ReportFilePath);

			var ns = new XmlNamespaceManager(doc.NameTable);
			ns.AddNamespace("rd", @"http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition");

			var dataSetNodes = doc.SelectNodes($"//rd:DataSet/@Name", ns);
			if (dataSetNodes.Count > 0)
			{
				var result = new List<string>();
				foreach (XmlNode node in dataSetNodes)
				{
					result.Add(node.InnerText);
				}
				return result.ToArray();
			}
			else
			{
				return null;
			}
		}

		protected string GetDataSetQuery(string dataSetName)
		{
			XmlDocument doc = new XmlDocument();
			doc.Load(ReportFilePath);

			var ns = new XmlNamespaceManager(doc.NameTable);
			ns.AddNamespace("rd", @"http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition");

			var cmdText = doc.SelectSingleNode($"//rd:DataSet[@Name='{dataSetName}']/rd:Query/rd:CommandText", ns)?.InnerText;

			var getUtcDateRegex = new Regex(@"\bGETUTCDATE\b\(\)", RegexOptions.IgnoreCase);
			var getLocalDateRegex = new Regex(@"\bGETDATE\b\(\)", RegexOptions.IgnoreCase);

			cmdText = getUtcDateRegex.Replace(cmdText, "@CurrentDateUtc");
			cmdText = getLocalDateRegex.Replace(cmdText, "@CurrentDateLocal");

			return cmdText;
		}

		protected Dictionary<int, string> GetValidParameterValues(string parameterName)
		{
			var result = new Dictionary<int, string>();

			XmlDocument doc = new XmlDocument();
			doc.Load(ReportFilePath);

			var ns = new XmlNamespaceManager(doc.NameTable);
			ns.AddNamespace("rd", @"http://schemas.microsoft.com/sqlserver/reporting/2016/01/reportdefinition");

			var parameterValues = doc.SelectNodes($"//rd:ReportParameter[@Name='{parameterName}']/rd:ValidValues/rd:ParameterValues/rd:ParameterValue", ns);
			foreach (XmlNode parameterValue in parameterValues)
			{
				result[Convert.ToInt32(parameterValue["Value"]?.InnerText)] = parameterValue["Label"]?.InnerText;
			}
			return result;
		}

		protected string ReportFilePath
		{
			get
			{
				return Path.Combine(BuildConstants.LocalEnterprisePath, ReportFileDirectory, ReportName + ReportFileExtension);
			}
		}

		protected abstract string ReportName { get; }

		const string ReportFileExtension = ".rdl";

		const string ReportFileDirectory = @"BusinessIntelligence/BiIntegration/Monitoring/Reports/GlobalMonitoring";

		#endregion

		#region Database

		void RestoreTestDatabase(DbConnection connection, string dbName)
		{
			using (var tempDirectory = new TempDirectory())
			{
				var backupFilePath = AssetsHelper.FetchTestAsset("BusinessIntelligence/content/DatabaseBackups/BiMonitoringTest.bak");
				var destinationFile = Path.Combine(tempDirectory.DirectoryName, "BiMonitoringTest.bak");
				File.Copy(backupFilePath, destinationFile, true);

				string sqlText = $@"
DECLARE @DataPath NVARCHAR(MAX) = CAST(SERVERPROPERTY ('InstanceDefaultDataPath') AS NVARCHAR(MAX)) + N'{dbName}.mdf'
DECLARE @LogPath NVARCHAR(MAX) = CAST(SERVERPROPERTY ('InstanceDefaultLogPath') AS NVARCHAR(MAX)) + N'{dbName}_log.ldf'

RESTORE DATABASE [{dbName}] FROM DISK = N'{destinationFile}' WITH FILE = 1, MOVE N'BiMonitoringTest' TO @DataPath,  MOVE N'BiMonitoringTest_log' TO @LogPath, NOUNLOAD, STATS = 5";
				connection.ExecuteNonQuery(sqlText);
			}
		}

		protected const string TestDbName = "BiMonitoringTestDb";

		#endregion

		#region Dates

		protected DateTime TestDateUtc
		{
			get
			{
				if (testDateUtc == null)
				{
					testDateUtc = GetDateFromReportSourceCode("CurrentDateUtc");
				}
				return testDateUtc.Value;
			}
		}
		DateTime? testDateUtc;

		protected DateTime TestDateLocal
		{
			get
			{
				if (testDateLocal == null)
				{
					testDateLocal = GetDateFromReportSourceCode("CurrentDateLocal");
				}
				return testDateLocal.Value;
			}
		}
		DateTime? testDateLocal;

		DateTime GetDateFromReportSourceCode(string columnName)
		{
			var sqlText = $"SELECT {columnName} FROM dbo.ReportSourceCode WHERE ReportName = '{ReportName}'";
			using (var cmd = TestAdminConnection.Command(sqlText))
			using (var reader = cmd.ExecuteReader())
			{
				if (reader.Read())
				{
					var dateVal = reader[0];
					if (dateVal != DBNull.Value)
					{
						return Convert.ToDateTime(dateVal);
					}
					else if (string.Equals(columnName, "CurrentDateUtc", StringComparison.OrdinalIgnoreCase))
					{
						return DateTime.UtcNow;
					}
					else
					{
						return DateTime.Now;
					}
				}
				else
				{
					throw new Exception($"No report source code found for report [{ReportName}].");
				}
			}
		}

		#endregion

		#endregion
	}
}
