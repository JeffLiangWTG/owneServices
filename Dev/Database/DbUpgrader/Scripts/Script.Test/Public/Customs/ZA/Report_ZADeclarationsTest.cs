using System;
using System.Collections.Generic;
using System.Data;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.ZA;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.Customs.ZA
{
	[TestedType(typeof(Report_ZADeclarations))]
	class Report_ZADeclarationsTest : DbCreateScriptTest
	{
		public void TestReport_ZADeclarations()
		{
			var companyPK = TestDataCreator.CreateCompany("TC1", "ZA", "ZAR");

			var branchPK = Guid.NewGuid();
			var sql = @"INSERT INTO dbo.GlbBranch (GB_PK, GB_Code, GB_RL_NKHomePort, GB_GC) VALUES (@branchPK, 'TB1', 'ZAJNB', @companyPK)";
			using (var command = Db.Connection.Command(sql))
			{
				command.AddParameter("@branchPK", SqlDbType.UniqueIdentifier, branchPK);
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.ExecuteNonQuery();
			}

			var declarationPK1 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00001", "IMP", 1, dataModel: "ZA");
			var declarationPK2 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00002", "IMP", 2, dataModel: "ZA");
			var declarationPK3 = TestDataCreator.CreateJobDeclaration(branchPK, companyPK, "B00003", "IMP", 3, dataModel: "ZA");

			AddCusEntryHeaderAndCusInstruction(declarationPK1, "10", new DateTime(2016, 12, 13), "MRN1", "1", "CLO", 1);
			AddCusEntryHeaderAndCusInstruction(declarationPK1, "20", new DateTime(2016, 12, 14), "MRN2", "3", "AWO", 1);
			AddCusEntryHeaderAndCusInstruction(declarationPK2, "30", new DateTime(2016, 12, 15), "MRN3", "6", "ERO", 2);
			AddCusEntryHeaderAndCusInstruction(declarationPK2, "40", new DateTime(2016, 12, 16), "MRN4", "30", "SNT", 2);

			var expectedMRNs = new Dictionary<Guid, string>();
			expectedMRNs.Add(declarationPK1, "MRN1, MRN2");
			expectedMRNs.Add(declarationPK2, "MRN3, MRN4");
			expectedMRNs.Add(declarationPK3, "");

			var reportSql = @"select JE_PK, MRNs from Report_ZADeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);

				using (var reader = command.ExecuteReader())
				{
					var declarationCount = 0;
					while (reader.Read())
					{
						var decPK = reader.GetGuid(0);
						AssertEquals("MRNs", expectedMRNs[decPK], reader.GetString(1));
						declarationCount++;
					}

					AssertEquals("Total declaration count", 3, declarationCount);
				}
			}

			reportSql = @"select JE_PK from Report_ZADeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, @assessmentDateFrom, @assessmentDateTo, null, null, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@assessmentDateFrom", SqlDbType.DateTime, new DateTime(2016, 12, 15));
				command.AddParameter("@assessmentDateTo", SqlDbType.DateTime, new DateTime(2016, 12, 15));

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals(declarationPK2, reader.GetGuid(0));
				}
			}

			reportSql = @"select JE_PK from Report_ZADeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, @cpc, null, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@cpc", SqlDbType.VarChar, "20");

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals(declarationPK1, reader.GetGuid(0));
				}
			}

			reportSql = @"select JE_PK from Report_ZADeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, @entryStatus, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, "3");

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals(declarationPK1, reader.GetGuid(0));
				}
			}

			reportSql = @"select JE_PK from Report_ZADeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, null, @messageStatus)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@messageStatus", SqlDbType.VarChar, "SNT");

				using (var reader = command.ExecuteReader())
				{
					Assert("only one record", reader.Read());
					AssertEquals(declarationPK2, reader.GetGuid(0));
				}
			}

			var additionalInfoHelper = new AdditionalInfoHelper(new[]
			{
				new AdditionalInfoConfig("AgentCode", "AGTCode", "C123"),
				new AdditionalInfoConfig("CargoCarrierCode", "CargoCarrier", "C234"),
				new AdditionalInfoConfig("MasterBillIssuedAt", "RL_NKMasterBillIssuedAt", "ABCD"),
				new AdditionalInfoConfig("MasterBillIssuedDate", new DateTime(2023, 1, 2, 3, 4, 5, 670)),
				new AdditionalInfoConfig("RemovalTransportCode", "R12"),
				new AdditionalInfoConfig("VATClaimBackIndicator", "Y")
			});
			UpdateDeclarationAdditionalInfo(declarationPK1, additionalInfoHelper.AdditionalInfoText);

			reportSql = $"select JE_PK, {additionalInfoHelper.SelectList} from Report_ZADeclarations(@companyPK, null, null, null, null, null, null, null, null, null, null, null, null, null, @entryStatus, null)";

			using (var command = Db.Connection.Command(reportSql))
			{
				command.AddParameter("@companyPK", SqlDbType.UniqueIdentifier, companyPK);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, "3");

				using (var reader = command.ExecuteReader())
				{
					CombineAssertions(() =>
					{
						Assert("only one record", reader.Read());
						AssertEquals(declarationPK1, reader.GetGuid(0));
						foreach (var config in additionalInfoHelper.Configurations)
						{
							AssertEquals($"Additional info \"{config.ColumnName}\" from ModelView", config.Value, reader[config.ColumnName]);
						}
					});
				}
			}
		}

		void AddCusEntryHeaderAndCusInstruction(Guid declarationPK, string cpc, DateTime? assessmentDate, string mrn, string entryStatus, string messageStaus, int clusterKey)
		{
			var cusInstructionPK = Guid.NewGuid();
			var cusEntryHeaderPK = Guid.NewGuid();

			var cusEntryHeaderSQL = @"
			insert into dbo.CusEntryInstruction (CEI_PK, CEI_DataModel, CEI_Style, CEI_JE, CEI_DateForDuty, CEI_ClusterKey, CEI_SystemCreateTimeUtc, CEI_SystemCreateUser, CEI_SystemLastEditTimeUtc, CEI_SystemLastEditUser)
values (@cusIntructionPK, 'ZA', @procedureCode, @declarationPK, @assessmentDate, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')
			insert into dbo.CusEntryHeader (CH_PK, CH_DataModel, CH_JE, CH_MessageType, CH_EntryStatus, CH_Status, CH_CEI_Instruction, CH_ClusterKey, CH_SystemCreateTimeUtc, CH_SystemCreateUser, CH_SystemLastEditTimeUtc, CH_SystemLastEditUser)
values (@cusEntryHeaderPK, 'ZA', @declarationPK, 'IMP', @entryStatus, @messageStaus, @cusIntructionPK, @clusterKey, getutcdate(), '~BP', getutcdate(), '~BP')";

			using (var command = Db.Connection.Command(cusEntryHeaderSQL))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@cusEntryHeaderPK", SqlDbType.UniqueIdentifier, cusEntryHeaderPK);
				command.AddParameter("@cusIntructionPK", SqlDbType.UniqueIdentifier, cusInstructionPK);
				command.AddParameter("@procedureCode", SqlDbType.VarChar, cpc);
				command.AddParameter("@assessmentDate", SqlDbType.SmallDateTime, assessmentDate);
				command.AddParameter("@entryStatus", SqlDbType.VarChar, entryStatus);
				command.AddParameter("@messageStaus", SqlDbType.VarChar, messageStaus);
				command.AddParameter("@clusterKey", SqlDbType.Int, clusterKey);

				command.ExecuteNonQuery();
			}

			TestDataCreator.CreateCusEntryNum(cusEntryHeaderPK, "CusEntryHeader", mrn, "MRN", "CUS", "ZA");
		}

		void UpdateDeclarationAdditionalInfo(Guid declarationPK, string additionalInfo)
		{
			var updateSQL = @"
UPDATE dbo.JobDeclaration
SET
	JE_AddInfo = @additionalInfo,
	JE_SystemLastEditTimeUtc = GETUTCDATE(),
	JE_SystemLastEditUser = '~BP'
WHERE
	JE_PK = @declarationPK";

			using (var command = Db.Connection.Command(updateSQL))
			{
				command.AddParameter("@declarationPK", SqlDbType.UniqueIdentifier, declarationPK);
				command.AddParameter("@additionalInfo", SqlDbType.VarChar, additionalInfo);
				command.ExecuteNonQuery();
			}
		}
	}
}

