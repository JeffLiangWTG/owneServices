using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesCampaignRegistration))]
	sealed class SalesCampaignRegistrationTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			string sqlText = @"
				DECLARE @G0Pk01 UNIQUEIDENTIFIER = newid();
				DECLARE @G0Pk02 UNIQUEIDENTIFIER = newid();
				DECLARE @G0Pk03 UNIQUEIDENTIFIER = newid();
				DECLARE @G0Pk04 UNIQUEIDENTIFIER = newid();
				DECLARE @GcPk01 UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @GcPk02 UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'EDI');

				INSERT dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_BroadcastVoteSurveyExam, G0_Stage, G0_Category, G0_Type, G0_EmailSenderOption, G0_G0_Master, G0_AttachmentList, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser) VALUES
				(@G0Pk01, @GcPk01, 'CPG01',  'TST00001000', 'BRD', 'TRI', 'PRINT', 'PREAP', 'COR', NULL,    '""File1.txt""',                '2014-01-01', 'US1', GetUtcDate(), 'E'),
				(@G0Pk02, @GcPk02, 'CPG02',  'TST00001001', 'SVY', 'OPN', 'REFER', 'OTHER', 'ORG', NULL,    '',                             '2014-09-01', 'US2', GetUtcDate(), 'E'),
				(@G0Pk03, @GcPk01, 'Drip01', 'TST00001002', 'DRM', 'ASN', 'VIDEO', 'EXIST', 'ORG', NULL,    '""File1.txt"", nullFile2.txt""', '2014-09-21', 'US3', GetUtcDate(), 'E'),
				(@G0Pk04, @GcPk01, 'Exam01', 'TST00001003', 'LCT', 'COM', 'TELEV', 'SLT30', 'SPS', NULL,    '',                             '2014-09-25', 'US3', GetUtcDate(), 'E'),
				(NEWID(), @GcPk01, 'CPG03',  'TST00001004', 'BRD', 'OPN', 'RADIO', 'OTHER', 'COR', @G0Pk03, '',                             '2014-09-25', 'US4', GetUtcDate(), 'E'),
				(NEWID(), @GcPk01, 'CPG04',  'TST00001005', 'SVY', 'OPN', 'REFER', 'SLT30', 'COR', @G0Pk03, '',                             '2017-09-01', 'US4', GetUtcDate(), 'E');

				INSERT dbo.GenCustomAddOnValue (XV_PK, XV_Name, XV_Type, XV_Data, XV_ParentTableCode, XV_ParentID, XV_SystemCreateTimeUtc, XV_SystemCreateUser, XV_SystemLastEditTimeUtc, XV_SystemLastEditUser) VALUES
				(NEWID(), 'Custom Field', 'STR', 'Custom Value', 'G0', @G0Pk02, GetUtcDate(), 'E', GetUtcDate(), 'E');
				INSERT dbo.GlbCompanyCampaignBudgetItem (G9_PK, G9_G0, G9_SystemCreateTimeUtc, G9_SystemCreateUser, G9_SystemLastEditTimeUtc, G9_SystemLastEditUser) VALUES
				(NEWID(), @G0Pk03, GetUtcDate(), 'E', GetUtcDate(), 'E');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 2, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "Drip01");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 21), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US3", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] AdditionalRefs", @"{""CampaignType"":""DRM"",""CampaignStage"":""ASN"",""MediaCategory"":""VIDEO"",""TouchCount"":""2"",""MediaType"":""EXIST"",""MailSender"":""ORG"",""HasCustomFields"":""N"",""eDocs"":""Y"",""HasBudget"":""Y""}",
				transaction1.AdditionalRefs);

			var transaction2 = FindRowByRef1(transactions, "CPG02");
			AssertEquals("[T2] CompanyCode", "EDI", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 9, 1), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US2", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] AdditionalRefs", @"{""CampaignType"":""SVY"",""CampaignStage"":""OPN"",""MediaCategory"":""REFER"",""TouchCount"":""N\/A"",""MediaType"":""OTHER"",""MailSender"":""ORG"",""HasCustomFields"":""Y"",""eDocs"":""N"",""HasBudget"":""N""}",
				transaction2.AdditionalRefs);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get
			{
				return AusydMonthRange.New(2014, 9);
			}
		}
	}
}
