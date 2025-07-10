namespace CargoWise.Bi.Development.SchemaSync.Testing
{
	using System.Linq;
	using System.Xml;
	using CargoWise.Data;
	using NUnit.Framework;

	class MemoryGrantTestCase : TestCase
	{
		public void TestMemoryGrantWarningNoTopKeyword()
		{
			try
			{
				CreateDatabaseWithColumnstoreIndex();

				using (((ICurrentDbControl)TestConnection).UseDatabase(TestDbName))
				using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
				{
					var sql = @"
INSERT INTO dbo.TestTable WITH (TABLOCK) (RowID, TestValue) 
SELECT ones.n + 10*tens.n + 100*hundreds.n,  CHECKSUM(NEWID())
FROM 
	(VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) ones(n),
	(VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) tens(n),
	(VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) hundreds(n)";

					using (var reader = TestConnection.Command(sql).ExecuteReader()) { while (reader.Read()) { } }

					var plans = TestConnection.ExecutedCommandsAndQueryPlans;
					var warningKind = GetMemoryGrantWarningKind(plans?.FirstOrDefault()?.Item2?.FirstOrDefault());

					AssertEquals($"Server Version: {TestConnection.ServerFullVersionText}\r\nMemoryGrantWarningKind", "Excessive Grant", warningKind);
				}
			}
			finally
			{
				DropTestDatabase();
			}
		}

		public void TestMemoryGrantWarningWithTopKeyword()
		{
			try
			{
				CreateDatabaseWithColumnstoreIndex();

				using (((ICurrentDbControl)TestConnection).UseDatabase(TestDbName))
				using (TestConnection.TrackExecutedCommands(includeQueryPlansForExecuteReaderCommands: true))
				{
					var sql = @"
					DECLARE @N BIGINT = 9223372036854775807

					INSERT INTO dbo.TestTable WITH (TABLOCK) (RowID, TestValue)
					SELECT 
						TOP (@N)
						ones.n + 10*tens.n + 100*hundreds.n,  CHECKSUM(NEWID())
					FROM 
						(VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) ones(n),
						(VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) tens(n),
						(VALUES(0),(1),(2),(3),(4),(5),(6),(7),(8),(9)) hundreds(n)";

					using (var reader = TestConnection.Command(sql).ExecuteReader()) { while (reader.Read()) { } }

					var plans = TestConnection.ExecutedCommandsAndQueryPlans;
					var warningKind = GetMemoryGrantWarningKind(plans?.FirstOrDefault()?.Item2?.FirstOrDefault());

					AssertNull($"Server Version: {TestConnection.ServerFullVersionText}\r\nMemoryGrantWarningKind", warningKind);
				}
			}
			finally
			{
				DropTestDatabase();
			}
		}

		#region Implementation

		DbConnection TestConnection
		{
			get
			{
				return testConnection ?? (testConnection = Db.NewAdminConnection(Db.ServerName, Db.SqlMasterDb));
			}
		}
		DbConnection testConnection;

		void CreateDatabaseWithColumnstoreIndex()
		{
			var sqlText = string.Format("IF NOT EXISTS (SELECT null FROM sys.databases WHERE name = '{0}') CREATE DATABASE [{0}]", TestDbName);
			TestConnection.ExecuteNonQuery(sqlText);

			using (((ICurrentDbControl)TestConnection).UseDatabase(TestDbName))
			{
				sqlText = $"CREATE TABLE dbo.TestTable (RowID int, TestValue int);\r\nCREATE CLUSTERED COLUMNSTORE INDEX cci_TestTable ON dbo.TestTable";
				TestConnection.ExecuteNonQuery(sqlText);
			}
		}

		void DropTestDatabase()
		{
			var sqlText = string.Format("IF EXISTS (SELECT null FROM sys.databases WHERE name = '{0}') DROP DATABASE [{0}]", TestDbName);
			TestConnection.ExecuteNonQuery(sqlText);
		}

		const string TestDbName = "TestDb_MemoryGrant";

		string GetMemoryGrantWarningKind(string queryPlan)
		{
			if (!string.IsNullOrEmpty(queryPlan))
			{
				const string xpath = "//vs:MemoryGrantWarning";

				XmlDocument document = new XmlDocument();
				document.LoadXml(queryPlan);

				XmlNamespaceManager manager = new XmlNamespaceManager(document.NameTable);
				manager.AddNamespace("vs", "http://schemas.microsoft.com/sqlserver/2004/07/showplan");

				var memoryGrantWarning = document.DocumentElement.SelectSingleNode(xpath, manager);
				if (memoryGrantWarning != null)
				{
					return memoryGrantWarning.Attributes["GrantWarningKind"].Value;
				}
				else
				{
					return null;
				}
			}
			else
			{
				return null;
			}
		}

		#endregion
	}
}
