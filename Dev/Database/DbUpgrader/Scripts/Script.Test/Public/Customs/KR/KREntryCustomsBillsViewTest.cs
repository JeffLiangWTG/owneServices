using System;
using System.Collections.Generic;
using CargoWise.DbUpgrader.Scripts.Definitions.Customs.KR;
using Enterprise.Build.Database.Script.TestFramework;
using NUnit.Framework;
using static Enterprise.Build.Database.Script.Public.Customs.KR.Testing.KRTestDataCreator;

namespace Enterprise.Build.Database.Script.Public.Customs.KR.Testing
{
	[TestedType(typeof(KREntryCustomsBillsView))]
	class KREntryCustomsBillsViewTest : DbCreateScriptTest
	{
		public void TestCusStatementHeaderColumns()
		{
			var importerPK = TestDataCreator.CreateOrganisation("RDKOR", "READY KOREA");
			var statementHeaderItems = GetItemList(new List<string> { "B2_PrintDate", "B2_ProcessDate", "B2_DueDate", "B2_PaymentAuthorizationDate", "B2_StatementType", "B2_OH_Importer", "B2_PaymentStatus" },
				new List<object> { new DateTime(2024, 01, 01), new DateTime(2024, 01, 02), new DateTime(2024, 01, 03), new DateTime(2024, 01, 04), "I", importerPK, "PYC" });

			var statementHeaderPK = CreateCusStatementHeader(companyPK, statementHeaderItems);
			CreateCusStatementLine(statementHeaderPK);

			using (var command = TestConnection.Command("SELECT KEB_PrintDate, KEB_ProcessDate, KEB_DueDate, KEB_PaymentAuthorizationDate, KEB_StatementType, KEB_OH_Importer, KEB_PaymentStatus, KEB_GC FROM [dbo].[KREntryCustomsBillsView]"))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(new DateTime(2024, 01, 01), reader["KEB_PrintDate"]);
					AssertEquals(new DateTime(2024, 01, 02), reader["KEB_ProcessDate"]);
					AssertEquals(new DateTime(2024, 01, 03), reader["KEB_DueDate"]);
					AssertEquals(new DateTime(2024, 01, 04), reader["KEB_PaymentAuthorizationDate"]);
					AssertEquals("I", reader["KEB_StatementType"]);
					AssertEquals(importerPK, reader["KEB_OH_Importer"]);
					AssertEquals("PYC", reader["KEB_PaymentStatus"]);
					AssertEquals(companyPK, reader["KEB_GC"]);
				}
			}
		}

		public void TestCusStatementLineColumns()
		{
			var statementHeaderPK = CreateCusStatementHeader(companyPK);
			var statementLineItems = GetItemList(new List<string> { "B3_CustomsFeesTotal" }, new List<object> { 100m });
			CreateCusStatementLine(statementHeaderPK, statementLineItems);

			using (var command = TestConnection.Command("SELECT KEB_CustomsFeesTotal FROM [dbo].[KREntryCustomsBillsView]"))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(100m, reader["KEB_CustomsFeesTotal"]);
				}
			}
		}

		public void TestCusStatementChargeColumns()
		{
			var statementHeaderPK = CreateCusStatementHeader(companyPK);
			var statementLinePK = CreateCusStatementLine(statementHeaderPK);
			CreateStatementLineChargeData("DTY", 100);
			CreateStatementLineChargeData("VAT", 200);
			CreateStatementLineChargeData("LQT", 300);
			CreateStatementLineChargeData("AGT", 400);
			CreateStatementLineChargeData("SCT", 500);
			CreateStatementLineChargeData("TRT", 600);
			CreateStatementLineChargeData("EDT", 700);
			CreateStatementLineChargeData("PLT", 800);
			CreateStatementLineChargeData("PMT", 900);
			CreateStatementLineChargeData("VFV", 1000);

			using (var command = TestConnection.Command("SELECT KEB_Duty, KEB_ValueAddedTax, KEB_LiquorTax, KEB_AgricultureTax, KEB_SpecialConsumptionTax, KEB_TransportationTax, KEB_EducationTax, KEB_PenaltyAndInterest, KEB_LatePenalty, KEB_ValueForVAT FROM [dbo].[KREntryCustomsBillsView]"))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(100m, reader["KEB_Duty"]);
					AssertEquals(200m, reader["KEB_ValueAddedTax"]);
					AssertEquals(300m, reader["KEB_LiquorTax"]);
					AssertEquals(400m, reader["KEB_AgricultureTax"]);
					AssertEquals(500m, reader["KEB_SpecialConsumptionTax"]);
					AssertEquals(600m, reader["KEB_TransportationTax"]);
					AssertEquals(700m, reader["KEB_EducationTax"]);
					AssertEquals(800m, reader["KEB_PenaltyAndInterest"]);
					AssertEquals(900m, reader["KEB_LatePenalty"]);
					AssertEquals(1000m, reader["KEB_ValueForVAT"]);
				}
			}

			void CreateStatementLineChargeData(string chargeType, decimal chargeAmount)
			{
				var statementLineChargeItems1 = GetItemList(new List<string> { "B4_ChargeType", "B4_ChargeAmount" },
					new List<object> { chargeType, chargeAmount });
				CreateCusStatementLineCharge(statementLinePK, statementLineChargeItems1);
			}
		}

		public void TestKREntryHeaderDetailsViewColumns()
		{
			var branchPK = TestDataCreator.CreateBranch(companyPK, "KB1", "KRINC", "KR");
			var declarationPK = CreateJobDeclaration(10, "IMP", branchPK, companyPK);
			var entryItems = GetItemList(new List<string> { "CH_EntryReleaseDate" }, new List<object> { new DateTime(2024, 01, 01) });
			var entryPK = CreateCusEntryHeader(declarationPK, 10, entryItems);
			CreateCusEntryNum(entryPK, "CusEntryHeader", "6N00224000051U", "IMP", "CUS", new DateTime(2024, 01, 02), new DateTime(2024, 01, 03));

			var statementHeaderPK = CreateCusStatementHeader(companyPK);
			var statementLineItems = GetItemList(new List<string> { "B3_EntryNum" }, new List<object> { "6N00224000051U" });
			CreateCusStatementLine(statementHeaderPK, statementLineItems);

			using (var command = TestConnection.Command("SELECT KEB_BranchPK, KEB_EntryReleaseDate, KEB_ImportEntryNum, KEB_ImportIssueDate FROM [dbo].[KREntryCustomsBillsView]"))
			{
				using (var reader = command.ExecuteReader())
				{
					reader.Read();
					AssertEquals(branchPK, reader["KEB_BranchPK"]);
					AssertEquals(new DateTime(2024, 01, 01), reader["KEB_EntryReleaseDate"]);
					AssertEquals("6N00224000051U", reader["KEB_ImportEntryNum"]);
					AssertEquals(new DateTime(2024, 01, 02), reader["KEB_ImportIssueDate"]);
				}
			}
		}

		public void TestUserEnteredFilter()
		{
			var branchPK = TestDataCreator.CreateBranch(companyPK, "KB1", "KRINC");
			var importerPK1 = TestDataCreator.CreateOrganisation("RDKOR1", "READY KOREA1");
			SetKREntryCustomsBillsViewData(10, "6N00224000051U", new DateTime(2024, 01, 01, 01, 01, 00), importerPK1, new DateTime(2024, 01, 02, 01, 02, 00));

			var importerPK2 = TestDataCreator.CreateOrganisation("RDKOR2", "READY KOREA2");
			SetKREntryCustomsBillsViewData(20, "6N00224000052U", new DateTime(2024, 01, 03, 01, 03, 00), importerPK2, new DateTime(2024, 01, 04, 01, 04, 00));

			AssertFilter(2, string.Empty);

			AssertFilter(1, string.Format("WHERE KEB_OH_Importer = '{0}'", importerPK1));
			AssertFilter(1, string.Format("WHERE KEB_OH_Importer = '{0}'", importerPK2));

			AssertFilter(1, "WHERE KEB_ImportEntryNum = '6N00224000051U'");
			AssertFilter(1, "WHERE KEB_ImportEntryNum = '6N00224000052U'");

			AssertFilter(1, "WHERE KEB_ImportIssueDate = '2024-01-01 01:01:00'");
			AssertFilter(1, "WHERE KEB_ImportIssueDate = '2024-01-03 01:03:00'");

			AssertFilter(1, "WHERE KEB_PaymentAuthorizationDate = '2024-01-02 01:02:00'");
			AssertFilter(1, "WHERE KEB_PaymentAuthorizationDate = '2024-01-04 01:04:00'");

			void AssertFilter(int rowCount, string filter)
			{
				using (var command = TestConnection.Command(string.Format("SELECT Count(*) AS 'RowCount' FROM [dbo].[KREntryCustomsBillsView] {0}", filter)))
				{
					using (var reader = command.ExecuteReader())
					{
						reader.Read();
						AssertEquals(rowCount, reader["RowCount"]);
					}
				}
			}

			void SetKREntryCustomsBillsViewData(int clusterKey, string importEntryNum, DateTime importIssueDate, Guid importerPK, DateTime paymentAuthorisationDate)
			{
				var declarationPK = CreateJobDeclaration(clusterKey, "IMP", branchPK, companyPK);
				var entryPK = CreateCusEntryHeader(declarationPK, clusterKey);
				CreateCusEntryNum(entryPK, "CusEntryHeader", importEntryNum, "IMP", "CUS", importIssueDate, new DateTime(2024, 01, 01));

				var statementHeaderItems = GetItemList(new List<string> { "B2_OH_Importer", "B2_PaymentAuthorizationDate" }, new List<object> { importerPK, paymentAuthorisationDate });
				var statementHeaderPK = CreateCusStatementHeader(companyPK, statementHeaderItems);
				var statementLineItems = GetItemList(new List<String> { "B3_EntryNum" }, new List<object> { importEntryNum });
				CreateCusStatementLine(statementHeaderPK, statementLineItems);
			}
		}

		public void TestKEB_CustomsDisbursementBillNumber()
		{
			var statementLinePK1 = CreateKREntryCustomsBillsViewData("D", "1111111111", "2222222222");
			var statementLinePK2 = CreateKREntryCustomsBillsViewData("I", "1111111111", "2222222222");
			var statementLinePK3 = CreateKREntryCustomsBillsViewData("U", "1111111111", "2222222222");

			using (var command = TestConnection.Command("SELECT KEB_PK, KEB_StatementType, KEB_CustomsDisbursementBillNumber FROM [dbo].[KREntryCustomsBillsView]"))
			{
				using (var reader = command.ExecuteReader())
				{
					var result = new Dictionary<Guid, object[]>();
					while (reader.Read())
					{
						result.Add((Guid)reader["KEB_PK"], new object[] { reader["KEB_StatementType"], reader["KEB_CustomsDisbursementBillNumber"] });
					}

					AssertEquals(3, result.Count);

					AssertEquals("D", result[statementLinePK1][0]);
					AssertEquals("1111111111", result[statementLinePK1][1]);

					AssertEquals("I", result[statementLinePK2][0]);
					AssertEquals("2222222222", result[statementLinePK2][1]);

					AssertEquals("U", result[statementLinePK3][0]);
					AssertEquals("1111111111", result[statementLinePK3][1]);
				}
			}

			Guid CreateKREntryCustomsBillsViewData(string statementType, string statementNumber, string associatedEntry)
			{
				var statementHeaderItems = GetItemList(new List<string> { "B2_StatementType", "B2_StatementNumber" }, new List<object> { statementType, statementNumber });
				var statementHeaderPK = CreateCusStatementHeader(companyPK, statementHeaderItems);
				var statementLineItems = GetItemList(new List<string> { "B3_AssociatedEntry" }, new List<object> { associatedEntry });
				return CreateCusStatementLine(statementHeaderPK, statementLineItems);
			}
		}

		protected override void SetUp()
		{
			TestDataCreator.CreateRefDatabaseRefDataGrouping("KR", "South Korea");
			companyPK = TestDataCreator.CreateCompany("KC1", "KR", "KRW");
		}
		Guid companyPK;
	}
}
