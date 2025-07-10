using System;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.Consol;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.Consol.Testing
{
	[TestedType(typeof(Report_ConsolProfileReport))]
	class Report_ConsolProfileReportTest : DbCreateScriptTest
	{
		public void TestTotalCO2e()
		{
			var consolPk = Guid.NewGuid();

			var sqlCmd = $@"
INSERT INTO dbo.JobConsol (JK_PK, JK_IsValid, JK_TransportMode, JK_AgentType, JK_IsForwarding, JK_ConsolMode, JK_UniqueConsignRef)
VALUES ('{consolPk}', 1, 'SEA', 'AGT', 1, 'FCL', 'C00001234')

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_IsValid, JW_TransportMode, JW_ParentType, JW_ParentGUID, JW_ETD, JW_ETA) VALUES (NEWID(), 1, 'SEA', 'CON', '{consolPk}', '2012-05-23 15:57:00', NULL)

INSERT INTO dbo.JobCO2e
	(JCO_PK, JCO_ParentID, JCO_ParentTableCode, JCO_Status, JCO_CO2ePerTonneInKg, JCO_TotalCO2e, JCO_SystemCreateTimeUtc, JCO_SystemCreateUser, JCO_SystemLastEditTimeUtc, JCO_SystemLastEditUser)
VALUES
	(NEWID(), '{consolPk}', 'JK', 'CUR', 20, 0.02, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
";
			TestConnection.ExecuteNonQuery(sqlCmd);

			var query = "SELECT TotalCO2e FROM Report_ConsolProfileReport(null, null, null, null, null, null, '1900-01-01', '2079-06-06', '1900-01-01', '2079-06-06', null, null, null, null, null)";
			using (var command = TestConnection.Command(query))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("Should have a row", reader.Read());
					AssertEquals("TotalCO2e", 0.02m, (decimal)reader["TotalCO2e"]);
				}
			}

			sqlCmd = $"DELETE FROM dbo.JobCO2e WHERE JCO_ParentID = '{consolPk}'";
			TestConnection.ExecuteNonQuery(sqlCmd);

			using (var command = TestConnection.Command(query))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("Should have a row", reader.Read());
					AssertEquals("TotalCO2e when CO2e is not calculated", DBNull.Value, reader["TotalCO2e"]);
				}
			}
		}

		public void TestTotalCO2e_CanParseMaxValue()
		{
			var consolPk = Guid.NewGuid();

			var sqlCmd = $@"
INSERT INTO dbo.JobConsol (JK_PK, JK_IsValid, JK_TransportMode, JK_AgentType, JK_IsForwarding, JK_ConsolMode, JK_UniqueConsignRef)
VALUES ('{consolPk}', 1, 'SEA', 'AGT', 1, 'FCL', 'C00001234')

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_IsValid, JW_TransportMode, JW_ParentType, JW_ParentGUID, JW_ETD, JW_ETA) VALUES (NEWID(), 1, 'SEA', 'CON', '{consolPk}', '2012-05-23 15:57:00', NULL)

INSERT INTO dbo.JobCO2e
	(JCO_PK, JCO_ParentID, JCO_ParentTableCode, JCO_Status, JCO_CO2ePerTonneInKg, JCO_TotalCO2e, JCO_SystemCreateTimeUtc, JCO_SystemCreateUser, JCO_SystemLastEditTimeUtc, JCO_SystemLastEditUser)
VALUES
	(NEWID(), '{consolPk}', 'JK', 'CUR', 20, 99999999999999.9999999, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
";
			TestConnection.ExecuteNonQuery(sqlCmd);

			var query = "SELECT TotalCO2e FROM Report_ConsolProfileReport(null, null, null, null, null, null, '1900-01-01', '2079-06-06', '1900-01-01', '2079-06-06', null, null, null, null, null)";

			AssertNoExceptionThrown("TotalCO2e shouldn't be rounded", () =>
			{
				using (var command = TestConnection.Command(query))
				{
					using (var reader = command.ExecuteReader())
					{
						Assert("Should have a row", reader.Read());
						AssertEquals("TotalCO2e", 99999999999999.9999999m, (decimal)reader["TotalCO2e"]);
					}
				}
			});
		}

		public void TestStatusCO2e()
		{
			var consolPk = Guid.NewGuid();

			var sqlCmd = $@"
INSERT INTO dbo.JobConsol (JK_PK, JK_IsValid, JK_TransportMode, JK_AgentType, JK_IsForwarding, JK_ConsolMode, JK_UniqueConsignRef)
VALUES ('{consolPk}', 1, 'SEA', 'AGT', 1, 'FCL', 'C00001234')

INSERT INTO dbo.JobConsolTransport (JW_PK, JW_IsValid, JW_TransportMode, JW_ParentType, JW_ParentGUID, JW_ETD, JW_ETA) VALUES (NEWID(), 1, 'SEA', 'CON', '{consolPk}', '2012-05-23 15:57:00', NULL)

INSERT INTO dbo.JobCO2e
	(JCO_PK, JCO_ParentID, JCO_ParentTableCode, JCO_Status, JCO_CO2ePerTonneInKg, JCO_TotalCO2e, JCO_SystemCreateTimeUtc, JCO_SystemCreateUser, JCO_SystemLastEditTimeUtc, JCO_SystemLastEditUser)
VALUES
	(NEWID(), '{consolPk}', 'JK', 'CUR', 20, 10, GETUTCDATE(), '~BP', GETUTCDATE(), '~BP')
";
			TestConnection.ExecuteNonQuery(sqlCmd);

			var query = "SELECT StatusCO2e FROM Report_ConsolProfileReport(null, null, null, null, null, null, '1900-01-01', '2079-06-06', '1900-01-01', '2079-06-06', null, null, null, null, null)";

			using (var command = TestConnection.Command(query))
			{
				using (var reader = command.ExecuteReader())
				{
					Assert("Should have a row", reader.Read());
					AssertEquals("StatusCO2e should match the JobCO2e.JCO_Status", "CUR", reader["StatusCO2e"]);
				}
			}
		}
	}
}

