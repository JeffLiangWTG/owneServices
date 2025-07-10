using System;
using System.Text;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Freight.CFS;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Freight.CFS.Testing
{
	[TestedType(typeof(Report_ImportContainerDeliveryAndReturn))]
	internal class Report_ImportContainerDeliveryAndReturnTest : DbCreateScriptTest
	{
		public void TestJE_VesselNameShouldBeShow_WhenThereIsNoDataInRefVessel()
		{
			var company = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var branch = TestDataCreator.CreateBranch(company, "CHI", "USCHI");
			var importer = TestDataCreator.CreateOrganisation("TESTORG", "TestOrg1");
			var rcPK = Guid.NewGuid();
			var jcPK = Guid.NewGuid();
			var jePK = Guid.NewGuid();
			var coPK = Guid.NewGuid();

			var sb = new StringBuilder();
			sb.AppendLine($"INSERT INTO dbo.RefContainer(RC_PK, RC_Code, RC_StorageClass, RC_HandlingRateClass, RC_FreightRateClass) VALUES('{rcPK}', '40TT', '40S', '40H', '40R')");
			sb.AppendLine($"INSERT INTO dbo.JobContainer (JC_PK, JC_RC, JC_ContainerMode) VALUES ('{jcPK}', '{rcPK}', 'FCL')");
			sb.AppendLine($"INSERT INTO dbo.JobDeclaration(JE_PK, JE_DataModel, JE_MessageType, JE_GB, JE_GC, JE_RL_NKFinalDestination, JE_RL_NKOrigin, JE_OH_Importer, JE_VesselName, JE_ClusterKey) VALUES('{jePK}', 'US', 'IMP', '{branch}', '{company}', 'USCHI', 'CNCHI', '{importer}', 'Vessel1', 1)");
			sb.AppendLine($"INSERT INTO dbo.CusContainer(CO_PK, CO_JE, CO_JC, CO_ClusterKey, CO_DataModel, CO_SystemCreateTimeUtc, CO_SystemCreateUser, CO_SystemLastEditTimeUtc, CO_SystemLastEditUser) values('{coPK}', '{jePK}', '{jcPK}', 1, 'US', GetUtcDate(), '~BP', GetUtcDate(), '~BP');");
			Db.Connection.Command(sb.ToString()).ExecuteNonQuery();

			var sql = $"SELECT Vessel FROM Report_ImportContainerDeliveryAndReturn('US', '', '', '{company}', '','')";
			using (var command = Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals("Vessel1", reader[0].ToString());
					}
					AssertEquals(1,count);
				}
			}
		}

		public void TestJW_TerminalAvailabilityDateShouldBeShown_WhenJC_FCLAvailableIsEmpty()
		{
			var gcPK = TestDataCreator.CreateCompany("TC1", "US", "USD");
			var rcPK = Guid.NewGuid();
			var jcPK = Guid.NewGuid();
			var jwPK = Guid.NewGuid();
			var jkPK = Guid.NewGuid();
			var jsPK = Guid.NewGuid();

			var sb = new StringBuilder();
			sb.AppendLine($"INSERT INTO dbo.JobConsol([JK_PK], [JK_UniqueConsignRef], [JK_SystemCreateTimeUtc], [JK_SystemCreateUser], [JK_SystemLastEditTimeUtc], [JK_SystemLastEditUser], [JK_TransportMode], [JK_ConsolMode], [JK_RL_NKLoadPort], [JK_RL_NKDischargePort]) VALUES ('{jkPK}', 'CFS_JK301', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'SEA', 'FCL', 'FRPAR', 'USCHI')");
			sb.AppendLine($"INSERT INTO dbo.JobShipment([JS_PK], [JS_UniqueConsignRef], [JS_SystemCreateTimeUtc], [JS_SystemCreateUser], [JS_SystemLastEditTimeUtc], [JS_SystemLastEditUser], [JS_PackingMode], [JS_TransportMode], [JS_RL_NKOrigin], [JS_RL_NKDestination]) VALUES ('{jsPK}', 'JS123', GetUtcDate(), '~BP', GetUtcDate(), '~BP', 'FCL', 'SEA', 'FRPAR', 'USCHI')");
			sb.AppendLine($"INSERT INTO dbo.RefContainer(RC_PK, RC_Code, RC_StorageClass, RC_HandlingRateClass, RC_FreightRateClass) VALUES('{rcPK}', '40TT', '40S', '40H', '40R')");
			sb.AppendLine($"INSERT INTO dbo.JobContainer (JC_PK, JC_RC, JC_ContainerMode, JC_JK) VALUES ('{jcPK}', '{rcPK}', 'FCL', '{jkPK}')");
			sb.AppendLine($"INSERT INTO dbo.JobConsolTransport([JW_PK], [JW_ParentGUID], [JW_Vessel], [JW_VoyageFlight], [JW_SystemCreateTimeUtc], [JW_SystemCreateUser], [JW_SystemLastEditTimeUtc], [JW_SystemLastEditUser], [JW_TerminalAvailabilityDate], [JW_ParentType], [JW_TransportMode], [JW_LegOrder]) VALUES ('{jwPK}', '{jkPK}', '', '', GetUtcDate(), '~BP', GetUtcDate(), '~BP', '06/06/2025 12:00:00', 'CON', 'SEA', 1);");

			Db.Connection.Command(sb.ToString()).ExecuteNonQuery();
			TestDataCreator.CreateJobConShipLink(jsPK, jkPK);
			var jlPK = TestDataCreator.CreateJobPackLines(jsPK);
			TestDataCreator.CreateJobContainerPackPivot(jcPK, jlPK);
			var sql = $"SELECT JC_FCLAvailable FROM Report_ImportContainerDeliveryAndReturn('US', 'SEA', '', '{gcPK}', '','Y')";
			using (var command = Db.Connection.Command(sql))
			{
				using (var reader = command.ExecuteReader())
				{
					var count = 0;
					while (reader.Read())
					{
						count++;
						AssertEquals(new DateTime(2025, 6, 6, 12, 0, 0), (DateTime)reader["JC_FCLAvailable"]);
					}
					AssertEquals(1, count);
				}
			}
		}
	}
}

