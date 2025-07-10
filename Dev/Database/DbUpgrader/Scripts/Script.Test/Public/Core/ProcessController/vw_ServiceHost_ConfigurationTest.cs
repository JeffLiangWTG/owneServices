using System.Collections.Generic;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Core.ProcessController;
using Enterprise.DbUpgrader.Shared;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Testing.Public.Core.ProcessController
{
	[TestedType(typeof(vw_ServiceHost_Configuration))]
	class vw_ServiceHost_ConfigurationTest : DbCreateScriptTest
	{
		public void TestHasRequiredFields()
		{
			CombineAssertions(() =>
			{
				//Act
				Test("Key");
				Test("Value");
			});

			void Test(string fieldName)
			{
				//Assert
				Assert(DbObjectCreator.ViewColumnExists(Db.Connection, "vw_ServiceHost_Configuration", fieldName));
			}
		}

		public void TestNoRequestedNumberOfProcessControllersRecordReturnsNoRecord()
		{
			// Arrange
			TestConnection.ExecuteNonQuery(@"
DELETE FROM dbo.StmData
WHERE
	SD_Name = 'RequestedNumberOfProcessControllers'
");
			const string script = $@"
SELECT * from dbo.vw_ServiceHost_Configuration
WHERE [key] = 'RequestedNumberOfProcessControllers'
";
			var valuesFromDb = new List<string>();

			// Act
			TestConnection.ExecuteReader(script,
				reader =>
				{
					valuesFromDb.Add(reader["Value"].ToString());
				});

			// Assert
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), valuesFromDb);
		}

		public void TestNullRequestedNumberOfProcessControllersRecordReturnsRecord()
		{
			// Arrange
			TestConnection.ExecuteNonQuery(@"
DELETE FROM dbo.StmData
WHERE
	SD_Name = 'RequestedNumberOfProcessControllers'

INSERT INTO dbo.StmData
(SD_PK                                 , SD_Name                              , SD_Type, SD_BinaryValue, SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
VALUES
('4895FA20-CC2F-42D5-86A7-D3412EABC001', 'RequestedNumberOfProcessControllers', 'INT'  , NULL          , GETUTCDATE()          , 'E'                , GETUTCDATE()            , 'E')
");
			const string script = $@"
SELECT * from dbo.vw_ServiceHost_Configuration
WHERE [key] = 'RequestedNumberOfProcessControllers'
";
			var valuesFromDb = new List<string>();

			// Act
			TestConnection.ExecuteReader(script,
				reader =>
				{
					valuesFromDb.Add(reader["Value"].ToString());
				});

			// Assert
			AssertContainsExactElementsInAnyOrder(Enumerable.Empty<string>(), valuesFromDb);
		}

		public void TestRequestedNumberOfProcessControllersRecordReturnsRecordValue()
		{
			Test("3100", 1);
			Test("3300", 3);
			Test("31003300", 13);

			void Test(string binaryValue, int value)
			{
				// Arrange
				TestConnection.ExecuteNonQuery($@"
DELETE FROM dbo.StmData
WHERE
	SD_Name = 'RequestedNumberOfProcessControllers'

INSERT INTO dbo.StmData
(SD_PK                                 , SD_Name                              , SD_BinaryValue , SD_SystemCreateTimeUtc, SD_SystemCreateUser, SD_SystemLastEditTimeUtc, SD_SystemLastEditUser)
VALUES
('4895FA20-CC2F-42D5-86A7-D3412EABC001', 'RequestedNumberOfProcessControllers', 0x{binaryValue}, GETUTCDATE()          , 'E'                , GETUTCDATE()            , 'E')
");
				const string script = $@"
SELECT * from dbo.vw_ServiceHost_Configuration
WHERE [key] = 'RequestedNumberOfProcessControllers'
";
				var valuesFromDb = new List<int>();

				// Act
				TestConnection.ExecuteReader(script,
					reader =>
					{
						valuesFromDb.Add(int.Parse(reader["Value"].ToString()));
					});

				// Assert
				AssertContainsExactElementsInAnyOrder(new[] { value }, valuesFromDb);
			}
		}
	}
}
