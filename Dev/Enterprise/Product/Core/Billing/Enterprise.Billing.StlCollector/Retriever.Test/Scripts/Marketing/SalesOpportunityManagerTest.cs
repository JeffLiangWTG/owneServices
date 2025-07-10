using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Billing.Collectors.Marketing;
using Enterprise.Integration.Billing;
using NUnit.Framework;

namespace Enterprise.Billing.StlCollector.Retriever.Scripts.Marketing
{
	[TestedType(typeof(SalesOpportunityManager))]
	sealed class SalesOpportunityManagerTest : RefStlScriptWithDefaultsTest
	{
		protected override void PrepareTestData()
		{
			const string sqlText = @"
				DECLARE @GcPk UNIQUEIDENTIFIER = (SELECT GC_PK FROM dbo.GlbCompany WHERE GC_Code = 'DEM');
				DECLARE @OhPk UNIQUEIDENTIFIER = (SELECT TOP (1) OH_PK FROM dbo.OrgHeader);
				DECLARE @OcPk UNIQUEIDENTIFIER = (SELECT TOP (1) OC_PK FROM dbo.OrgContact);
				DECLARE @InquiryPk UNIQUEIDENTIFIER = newid();
				DECLARE @CampaignPk UNIQUEIDENTIFIER = newid();
				INSERT dbo.OrgColdCallRegister (O1_PK, O1_LeadUniqueReference, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) VALUES
					(@InquiryPk, 'TSTINQ001', GetUtcDate(), 'E', GetUtcDate(), 'E');
				INSERT dbo.GlbCompanyCampaign (G0_PK, G0_GC, G0_CampaignName, G0_CampaignID, G0_SystemCreateTimeUtc, G0_SystemCreateUser, G0_SystemLastEditTimeUtc, G0_SystemLastEditUser)
					VALUES (@CampaignPk, @GcPk, 'Test Campaign', 'TST00001000', GetUtcDate(), 'E', GetUtcDate(), 'E');
				INSERT dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_GS_NKPrimarySalesPerson, P8_O1_Enquiry, P8_G0, P8_DiscountAmount, P8_RentalMultiplier, P8_EstimatedValue, P8_OC, P8_Source, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser) VALUES
					(newid(), 'OP01', @OhPk, @GcPk, 'US1', @InquiryPk,	null,			0,	25,	0,		null,	'WEB',	'2014-08-31 01:11:00', 'US1', GetUtcDate(), 'E'),
					(newid(), 'OP02', @OhPk, @GcPk, 'US2', @InquiryPk,	null,			0,	25,	0,		null,	'WEB',	'2014-09-30 00:00:00', 'US2', GetUtcDate(), 'E'),
					(newid(), 'OP03', @OhPk, @GcPk, 'US3', null,		@CampaignPk,	0,	50,	0,		null,	'',		'2013-09-12 03:13:00', 'US3', GetUtcDate(), 'E'),
					(newid(), 'OP04', @OhPk, @GcPk, 'US1', null,		@CampaignPk,	20,	50,	0,		null,	'',		'2014-09-23 14:24:00', 'US4', GetUtcDate(), 'E'),
					(newid(), 'OP05', @OhPk, @GcPk, 'US2', null,		null,			10,	0,	100,	@OcPk,	'OTH',	'2014-09-01 00:00:00', 'US5', GetUtcDate(), 'E');";
			TestConnection.ExecuteNonQuery(sqlText);
		}

		protected override void AssertResultSet(IEnumerable<IStlTransaction> transactions)
		{
			AssertEquals("Number of Transactions", 3, transactions.Count());

			var transaction1 = FindRowByRef1(transactions, "OP05");
			AssertEquals("[T1] CompanyCode", "DEM", transaction1.GetCompanyCode());
			AssertEquals("[T1] BranchCode", null, transaction1.GetBranchCode());
			AssertEquals("[T1] TransactionDateUtc", new DateTime(2014, 9, 1), transaction1.ServiceOccuredUTC);
			AssertEquals("[T1] UserCode", "US5", transaction1.ClientStaffCode);
			AssertEquals("[T1] ItemCount", 1, transaction1.BillableCount);
			AssertEquals("[T1] TransactionReference02", "CreationSource=Opportunity|LeadSource=OTH", transaction1.Reference2);
			AssertEquals("[T1] TransactionReference03", "HasCurrent=Y|HasPotential=N|HasTotalEst=Y", transaction1.Reference3);
			AssertEquals("[T1] TransactionReference04", "IsAssignedToCreator=N|HasContact=Y", transaction1.Reference4);

			var transaction2 = FindRowByRef1(transactions, "OP04");
			AssertEquals("[T2] CompanyCode", "DEM", transaction2.GetCompanyCode());
			AssertEquals("[T2] BranchCode", null, transaction2.GetBranchCode());
			AssertEquals("[T2] TransactionDateUtc", new DateTime(2014, 9, 23, 14, 24, 0), transaction2.ServiceOccuredUTC);
			AssertEquals("[T2] UserCode", "US4", transaction2.ClientStaffCode);
			AssertEquals("[T2] ItemCount", 1, transaction2.BillableCount);
			AssertEquals("[T2] TransactionReference02", "CreationSource=Campaign|LeadSource=", transaction2.Reference2);
			AssertEquals("[T2] TransactionReference03", "HasCurrent=Y|HasPotential=Y|HasTotalEst=N", transaction2.Reference3);
			AssertEquals("[T2] TransactionReference04", "IsAssignedToCreator=N|HasContact=N", transaction2.Reference4);

			var transaction3 = FindRowByRef1(transactions, "OP02");
			AssertEquals("[T3] CompanyCode", "DEM", transaction3.GetCompanyCode());
			AssertEquals("[T3] BranchCode", null, transaction3.GetBranchCode());
			AssertEquals("[T3] TransactionDateUtc", new DateTime(2014, 9, 30), transaction3.ServiceOccuredUTC);
			AssertEquals("[T3] UserCode", "US2", transaction3.ClientStaffCode);
			AssertEquals("[T3] ItemCount", 1, transaction3.BillableCount);
			AssertEquals("[T3] TransactionReference02", "CreationSource=Inquiry|LeadSource=WEB", transaction3.Reference2);
			AssertEquals("[T3] TransactionReference03", "HasCurrent=N|HasPotential=Y|HasTotalEst=N", transaction3.Reference3);
			AssertEquals("[T3] TransactionReference04", "IsAssignedToCreator=Y|HasContact=N", transaction3.Reference4);
		}

		protected override IDateTimeRange TestDateTimeRange
		{
			get { return AusydMonthRange.New(2014, 9); }
		}

		public void TestIsSystemLevelFeature()
		{
			AssertEquals("IsSystemLevel", false, ScriptToTest.IsSystemLevel);
		}
	}
}
