using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.CommissionManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.CommissionManagement.Testing
{
	[TestedType(typeof(Report_OpportunityWithoutAgreementAddReport))]
	class Report_OpportunityWithoutAgreementAddReportTest : DbCreateScriptTest
	{
		public void TestReportFilter_StaffCodes()
		{
			SetupReportData();
			var filter = new ReportFilter() { StaffCodes = new[] { "SS1" } };
			var expectedOpportunities = new [] { ("WITHOUT_CA", "SS1"), ("WITH_CA_AND_NORATE", "SS1") };
			AssertReportFilter("StaffCodes: SS1", filter, expectedOpportunities);

			filter.StaffCodes = new[] { "SS2" };
			expectedOpportunities = new [] { ("WITH_DETAILS", "SS2"), };
			AssertReportFilter("StaffCodes: SS2", filter, expectedOpportunities);
		}

		public void TestReportFilter_OppOrgPks()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgPks = new[] { org1Pk } };
			var expectedOpportunities = new [] { ("WITH_CA_AND_NORATE", "SS1") };
			AssertReportFilter("OppOrgs: Org1", filter, expectedOpportunities);

			filter.OppOrgPks = new[] { org2Pk, org3Pk };
			expectedOpportunities = new [] { ("WITHOUT_CA", "SS1"), ("WITH_DETAILS", "SS2") };
			AssertReportFilter("OppOrgs: Org2, Org3", filter, expectedOpportunities);

			filter.OppOrgPks = new[] { org2Pk, org3Pk, org4Pk };
			expectedOpportunities = new [] { ("WITHOUT_CA", "SS1"), ("WITH_DETAILS", "SS2") };
			AssertReportFilter("OppOrgs: Org2, Org3, Org4", filter, expectedOpportunities);
		}
		public void TestReportFilter_OppStatusList()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppStatusList = "CRT", OppOrgPks = new[] { org1Pk, org2Pk, org3Pk, org4Pk } };
			var expectedRecipients = new [] { ("WITHOUT_CA", "SS1") };
			AssertReportFilter("OppStatuses: CRT", filter, expectedRecipients);

			filter.OppStatusList = "WON";
			expectedRecipients = new [] { ("WITH_DETAILS", "SS2") };
			AssertReportFilter("OppStatuses: WON", filter, expectedRecipients);

			filter.OppStatusList = "CRT, WON, ABA";
			expectedRecipients = new [] { ("WITHOUT_CA", "SS1"), ("WITH_DETAILS", "SS2"), ("WITH_CA_AND_NORATE", "SS1") };
			AssertReportFilter("OppStatuses: CRT, WON, ABA", filter, expectedRecipients);
		}
		public void TestReportFilter_OppCreateDates()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgPks = new[] { org1Pk, org2Pk, org3Pk, org4Pk } };
			filter.OppCreateFrom = new DateTime(2020, 01, 01);
			var expectedRecipients = new [] { ("WITH_DETAILS", "SS2"), ("WITH_CA_AND_NORATE", "SS1") };
			AssertReportFilter("From: 2020-01-01", filter, expectedRecipients);

			filter.OppCreateFrom = null;
			filter.OppCreateTo = new DateTime(2022, 02, 05);
			expectedRecipients = new [] { ("WITHOUT_CA", "SS1"), ("WITH_DETAILS", "SS2"), ("WITH_CA_AND_NORATE", "SS1") };
			AssertReportFilter("To: 2022-02-05", filter, expectedRecipients);

			filter.OppCreateFrom = new DateTime(2020, 01, 11);
			filter.OppCreateTo = new DateTime(2022, 02, 05);
			expectedRecipients = new [] { ("WITH_CA_AND_NORATE", "SS1") };
			AssertReportFilter("From: 2020-01-11 | To: 2022-02-05", filter, expectedRecipients);
		}

		public void TestReportWithFixedCommissionAmount()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgPks = new[] { org5Pk } };
			var expectedOpportunities = new[] { ("WITH_CA_ZERO_FIXED", "SS3") };
			AssertReportFilter("OppOrgs: Org5", filter, expectedOpportunities);
		}

		public void TestReportOutput()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgPks = new[] { org1Pk, org2Pk, org3Pk, org4Pk } };
			var result = GetReportResult(filter).AsEnumerable();
			AssertEquals("Precondition: Should have returned three results for selected opportunity organisation", 3, result.Count());

			var orderedResults = result.OrderByDescending(r => r["P8_OpportunityID"]).ToList();
			CombineAssertions("OpportunityIds are incorrect", () =>
			{
				AssertEquals("WITHOUT_CA", orderedResults[0]["P8_OpportunityId"]);
				AssertEquals("WITH_DETAILS", orderedResults[1]["P8_OpportunityId"]);
				AssertEquals("WITH_CA_AND_NORATE", orderedResults[2]["P8_OpportunityId"]);
			});

			CombineAssertions("OpportunityPK are incorrect", () =>
			{
				AssertEquals(opp1Pk, orderedResults[0]["OpportunityPK"]);
				AssertEquals(opp2Pk, orderedResults[1]["OpportunityPK"]);
				AssertEquals(opp3Pk, orderedResults[2]["OpportunityPK"]);
			});

			CombineAssertions("OpportunityOrgCodes are incorrect", () =>
			{
				AssertEquals("TESTORGBBB", orderedResults[0]["OpportunityOrgCode"]);
				AssertEquals("TESTORGCCC", orderedResults[1]["OpportunityOrgCode"]);
				AssertEquals("TESTORGAAA", orderedResults[2]["OpportunityOrgCode"]);
			});

			CombineAssertions("OpportunityOrgPKs are incorrect", () =>
			{
				AssertEquals(org2Pk, orderedResults[0]["OpportunityOrgPK"]);
				AssertEquals(org3Pk, orderedResults[1]["OpportunityOrgPK"]);
				AssertEquals(org1Pk, orderedResults[2]["OpportunityOrgPK"]);
			});

			CombineAssertions("OpportunityOrgNames are incorrect", () =>
			{
				AssertEquals("Test Organisation BBB", orderedResults[0]["OpportunityOrgName"]);
				AssertEquals("Test Organisation CCC", orderedResults[1]["OpportunityOrgName"]);
				AssertEquals("Test Organisation AAA", orderedResults[2]["OpportunityOrgName"]);
			});

			CombineAssertions("OpportunityCreateDates are incorrect", () =>
			{
				AssertEquals(new DateTime(2019, 10, 15, 11, 10, 0), orderedResults[0]["OpportunityCreateDate"]);
				AssertEquals(new DateTime(2020, 01, 10, 13, 40, 0), orderedResults[1]["OpportunityCreateDate"]);
				AssertEquals(new DateTime(2020, 02, 07, 09, 00, 0), orderedResults[2]["OpportunityCreateDate"]);
			});

			CombineAssertions("OpportunitySalesPersonNames are incorrect", () =>
			{
				AssertEquals("SS1", orderedResults[0]["OpportunitySalesPersonName"]);
				AssertEquals("SS2", orderedResults[1]["OpportunitySalesPersonName"]);
				AssertEquals("SS1", orderedResults[2]["OpportunitySalesPersonName"]);
			});

			CombineAssertions("OpportunitySalesPersonTeamss are incorrect", () =>
			{
				AssertEquals("ST1, ST2", orderedResults[0]["OpportunitySalesPersonTeams"]);
				AssertEquals("ST2", orderedResults[1]["OpportunitySalesPersonTeams"]);
				AssertEquals("ST1, ST2", orderedResults[2]["OpportunitySalesPersonTeams"]);
			});

			CombineAssertions("P8_OpportunityDescriptions are incorrect", () =>
			{
				AssertEquals("Test Opportunity 1", orderedResults[0]["P8_OpportunityDescription"]);
				AssertEquals("Test Opportunity 2", orderedResults[1]["P8_OpportunityDescription"]);
				AssertEquals("Test Opportunity 3", orderedResults[2]["P8_OpportunityDescription"]);
			});

			CombineAssertions("P8_Stages are incorrect", () =>
			{
				AssertEquals("A1", orderedResults[0]["P8_Stage"]);
				AssertEquals("A3", orderedResults[1]["P8_Stage"]);
				AssertEquals("A2", orderedResults[2]["P8_Stage"]);
			});

			CombineAssertions("OpportunityLastEditDates are incorrect", () =>
			{
				AssertEquals(new DateTime(2019, 10, 15, 13, 20, 0), orderedResults[0]["OpportunityLastEditDate"]);
				AssertEquals(new DateTime(2020, 01, 15, 14, 50, 0), orderedResults[1]["OpportunityLastEditDate"]);
				AssertEquals(new DateTime(2020, 02, 07, 09, 00, 0), orderedResults[2]["OpportunityLastEditDate"]);
			});

			CombineAssertions("P8_EstimatedCloseDates are incorrect", () =>
			{
				AssertEquals(new DateTime(2020, 07, 15, 00, 00, 0), orderedResults[0]["P8_EstimatedCloseDate"]);
				AssertEquals(DBNull.Value, orderedResults[1]["P8_EstimatedCloseDate"]);
				AssertEquals(DBNull.Value, orderedResults[2]["P8_EstimatedCloseDate"]);
			});

			CombineAssertions("P8_ClosedDates are incorrect", () =>
			{
				AssertEquals(new DateTime(2020, 08, 26, 00, 00, 0), orderedResults[0]["P8_ClosedDate"]);
				AssertEquals(new DateTime(2020, 02, 02, 00, 00, 0), orderedResults[1]["P8_ClosedDate"]);
				AssertEquals(DBNull.Value, orderedResults[2]["P8_ClosedDate"]);
			});

			CombineAssertions("P8_Outcomes are incorrect", () =>
			{
				AssertEquals("OUT", orderedResults[0]["P8_Outcome"]);
				AssertEquals("", orderedResults[1]["P8_Outcome"]);
				AssertEquals("", orderedResults[2]["P8_Outcome"]);
			});

			CombineAssertions("OpportunitySalesTypes are incorrect", () =>
			{
				AssertEquals("XXX", orderedResults[0]["OpportunitySalesType"]);
				AssertEquals("YYY", orderedResults[1]["OpportunitySalesType"]);
				AssertEquals("ZZZ", orderedResults[2]["OpportunitySalesType"]);
			});

			CombineAssertions("OpportunityProductTypes are incorrect", () =>
			{
				AssertEquals("AAA", orderedResults[0]["OpportunityProductType"]);
				AssertEquals("", orderedResults[1]["OpportunityProductType"]);
				AssertEquals("", orderedResults[2]["OpportunityProductType"]);
			});

			CombineAssertions("OpportunityStatus are incorrect", () =>
			{
				AssertEquals("CRT", orderedResults[0]["OpportunityStatus"]);
				AssertEquals("WON", orderedResults[1]["OpportunityStatus"]);
				AssertEquals("ABA", orderedResults[2]["OpportunityStatus"]);
			});
		}

		void AssertReportFilter(string message, ReportFilter filter, (string, string)[] expectedRecipients)
		{
			var rows = GetReportResult(filter).Rows.Cast<DataRow>();
			var actualRecipients = rows.Select(r => ((string)r["P8_OpportunityId"], (string)r["OpportunitySalesPersonName"]));
			AssertContainsExactElementsInAnyOrder(message, expectedRecipients, actualRecipients);
		}

		DataTable GetReportResult(ReportFilter filter)
		{
			var command = TestConnection.Command("SELECT * FROM Report_OpportunityWithoutAgreementAddReport(@StaffCodes, @StaffCodesIsEmpty, @OpportunityOrgPKs, @OpportunityOrgPKsIsEmpty, @OpportunityStatusList, @OpportunityCreateFromDate, @OpportunityCreateToDate, @CurrentDate)");
			command.AddTableValuedParameter("@StaffCodes", "dbo.TVP_varchar_250", filter.StaffCodes);
			command.AddTableValuedParameter("@OpportunityOrgPKs", "dbo.TVP_uniqueidentifier", filter.OppOrgPks);
			command.AddParameter("@StaffCodesIsEmpty", SqlDbType.Bit, filter.StaffCodes.Any() ? 0 : 1);
			command.AddParameter("@OpportunityOrgPKsIsEmpty", SqlDbType.Bit, filter.OppOrgPks.Any() ? 0 : 1);

			command.AddParameter("@OpportunityStatusList", SqlDbType.VarChar, filter.OppStatusList);
			command.AddParameter("@OpportunityCreateFromDate", SqlDbType.SmallDateTime, (object)filter.OppCreateFrom ?? DBNull.Value);
			command.AddParameter("@OpportunityCreateToDate", SqlDbType.SmallDateTime, (object)filter.OppCreateTo ?? DBNull.Value);
			command.AddParameter("@CurrentDate", SqlDbType.Date, filter.CurrentDate);

			return DataUtils.GetDataTableFromCommand(command);
		}

		void SetupReportData()
		{
			var commandText = $@"
DECLARE @CompanyPk UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesStaff1Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesStaff2Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesTeam1Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesTeam2Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @RatePk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @RatePk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @RatePk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @RatePk4 UNIQUEIDENTIFIER = NEWID();
DECLARE @RatePk5 UNIQUEIDENTIFIER = NEWID();
DECLARE @RatePk6 UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement1Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement2Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement3Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement4Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement5Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk4 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk5 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk6 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'TES', 'AU company', 'AU', 'AUD')
INSERT dbo.GlbStaff(GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@SalesStaff1Pk, 'SS1', 'Sales Staff 1', 'SS1 Login', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT dbo.GlbStaff(GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@SalesStaff2Pk, 'SS2', 'Sales Staff 2', 'SS2 Login', GetUtcDate(), '~BP', GetUtcDate(), '~BP')

INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsSales) VALUES (@SalesTeam1Pk, 'ST1', 'Team 1', 1)
INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsSales) VALUES (@SalesTeam2Pk, 'ST2', 'Team 2', 1)

INSERT dbo.GlbGroupLink (GK_PK, GK_GS, GK_GG) VALUES (NEWID(), @SalesStaff1Pk, @SalesTeam1Pk)
INSERT dbo.GlbGroupLink (GK_PK, GK_GS, GK_GG) VALUES (NEWID(), @SalesStaff1Pk, @SalesTeam2Pk)
INSERT dbo.GlbGroupLink (GK_PK, GK_GS, GK_GG) VALUES (NEWID(), @SalesStaff2Pk, @SalesTeam2Pk)

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org1Pk}', 'TESTORGAAA', 'Test Organisation AAA', 'NZAKL')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org2Pk}', 'TESTORGBBB', 'Test Organisation BBB', 'AUSYD')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org3Pk}', 'TESTORGCCC', 'Test Organisation CCC', 'AUSYD')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org4Pk}', 'TESTORGDDD', 'Test Organisation DDD', 'AUSYD')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org5Pk}', 'TESTORGEEE', 'Test Organisation EEE', 'AUSYD')

INSERT INTO dbo.OrgColdCallRegister(O1_PK, O1_LeadUniqueReference, O1_EnquiryType, O1_LeadStatus, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) VALUES('{inquiryPk}', 'TSTINQ001', 'INQ', 'OPN', GetUtcDate(), 'E', GetUtcDate(), 'E');
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_GS_NKPrimarySalesPerson, P8_EstimatedCloseDate, P8_ClosedDate, P8_EstimatedValue, P8_RX_NKEstimatedValueCurrency, P8_LostReason, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser, P8_Outcome, P8_PackageType, P8_Source, P8_SourceDetails, P8_O1_Enquiry, P8_DiscountAmount, P8_RentalMultiplier)
VALUES ('{opp1Pk}', 'WITHOUT_CA', '{org2Pk}', @CompanyPk, 'XXX', 'A1', 'CRT', 'Test Opportunity 1', 'SS1', '2020-07-15', '2020-08-26', 2000, 'AUD', 'PRO', '2019-10-15 11:10', 'E', '2019-10-15 13:20', 'E', 'OUT', 'AAA', 'OTH', 'Test Details', '{inquiryPk}', 19, 83)
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_GS_NKPrimarySalesPerson, P8_ClosedDate, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES ('{opp2Pk}', 'WITH_DETAILS', '{org3Pk}', @CompanyPk, 'YYY', 'A3', 'WON', 'Test Opportunity 2', 'SS2', '2020-02-02', '2020-01-10 13:40', 'E', '2020-01-15 14:50', 'E')
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_GS_NKPrimarySalesPerson, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES ('{opp3Pk}', 'WITH_CA_AND_NORATE', '{org1Pk}', @CompanyPk, 'ZZZ', 'A2', 'ABA', 'Test Opportunity 3', 'SS1', '2020-02-07 09:00', 'E', '2020-02-07 09:00', 'E')
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_GS_NKPrimarySalesPerson, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES ('{opp4Pk}', 'WITH_CA_AND_RATE', '{org4Pk}', @CompanyPk, 'XXX', 'A1', 'CRT', 'Test Opportunity 4', 'SS1', '2020-02-08 09:00', 'E', '2020-02-09 09:00', 'E')
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_GS_NKPrimarySalesPerson, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES ('{opp5Pk}', 'WITH_CA_ZERO_FIXED', '{org5Pk}', @CompanyPk, 'XXX', 'A1', 'CAN', 'Test Opportunity 5', 'SS3', '2020-02-08 09:00', 'E', '2020-02-09 09:00', 'E')
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_GS_NKPrimarySalesPerson, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES ('{opp6Pk}', 'WITH_CA_AND_FIXED', '{org5Pk}', @CompanyPk, 'XXX', 'A1', 'CAN', 'Test Opportunity 6', 'SS3', '2020-02-08 09:00', 'E', '2020-02-09 09:00', 'E')

INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement1Pk, '#1', '{opp2Pk}', '{org1Pk}', 'MAN', '2020-01-17', 'PRF', '2020-01-11 17:43', '2020-02-17', NULL, '2020-01-11 17:43', 'XX','2020-01-11 17:43', 'XX', 'AAA')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement2Pk, '#2', '{opp3Pk}', '{org2Pk}', 'MAN', '2020-02-10', 'REV', '2020-02-02 07:11', NULL, NULL, '2020-02-02 07:11', 'XX','2020-02-02 07:11', 'XX', 'AAA')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement3Pk, '#3', '{opp4Pk}', '{org3Pk}', 'MAN', '2020-02-10', 'REV', '2020-02-08 07:11', NULL, NULL, '2020-02-08 07:11', 'XX','2020-02-02 07:11', 'XX', 'AAA')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement4Pk, '#4', '{opp5Pk}', '{org5Pk}', 'MAN', '2020-02-10', 'REV', '2020-02-08 07:11', NULL, NULL, '2020-02-08 07:11', 'XX','2020-02-02 07:11', 'XX', 'AAA')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement5Pk, '#5', '{opp6Pk}', '{org5Pk}', 'MAN', '2020-02-10', 'REV', '2020-02-08 07:11', NULL, NULL, '2020-02-08 07:11', 'XX','2020-02-02 07:11', 'XX', 'AAA')

INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk1, @Agreement2Pk, NULL, 'WP1', 'PCT', 0, 0, '2020-05-11', '2020-01-11 12:12', 'E', '2020-01-11 12:12', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk2, @Agreement3Pk, NULL, 'SS1', 'PCT', 3, 0, '2020-04-16', '2019-10-16 11:11', 'E', '2019-10-16 11:11', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk3, @Agreement3Pk, NULL, 'SS1', 'PCT', 3, 0, '2020-05-16', '2010-10-16 11:11', 'E', '2019-10-16 11:11', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk4, @Agreement4Pk, NULL, 'SS3', 'PCT', 3, 0, '2020-05-16', '2010-10-16 11:11', 'E', '2019-10-16 11:11', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk5, @Agreement5Pk, NULL, 'SS3', 'PCT', 3, 0, '2020-05-16', '2010-10-16 11:11', 'E', '2019-10-16 11:11', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk6, @Agreement5Pk, NULL, 'SS3', 'PCT', 3, 0, '2020-05-16', '2010-10-16 11:11', 'E', '2019-10-16 11:11', 'E')

INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_CommissionPercentage, CAT_CommissionAmount, CAT_CommissionPeriod, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser)
VALUES (@RatePk1, @RecipientPk1, 0.00, 0, 'NEW', '2020-01-11 12:12', 'E', '2020-01-11 12:12', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_CommissionPercentage, CAT_CommissionAmount, CAT_CommissionPeriod, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser)
VALUES (@RatePk2, @RecipientPk2, 5.00, 0, 'EXS', '2020-01-14 12:12', 'E', '2020-01-14 12:12', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_CommissionPercentage, CAT_CommissionAmount, CAT_CommissionPeriod, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser)
VALUES (@RatePk3, @RecipientPk3, 0.00, 0, 'NEW', '2020-01-14 12:12', 'E', '2020-01-14 12:12', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_CommissionPercentage, CAT_CommissionAmount, CAT_CommissionPeriod, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser)
VALUES (@RatePk4, @RecipientPk4, 0.00, 0, 'NEW', '2020-01-14 12:12', 'E', '2020-01-14 12:12', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_CommissionPercentage, CAT_CommissionAmount, CAT_CommissionPeriod, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser)
VALUES (@RatePk5, @RecipientPk5, 0.00, 5, 'NEW', '2020-01-14 12:12', 'E', '2020-01-14 12:12', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_CommissionPercentage, CAT_CommissionAmount, CAT_CommissionPeriod, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser)
VALUES (@RatePk6, @RecipientPk6, 0.00, 0, 'NEW', '2020-01-14 12:12', 'E', '2020-01-14 12:12', 'E')
";
			TestConnection.ExecuteNonQuery(commandText);
		}

		readonly Guid org1Pk = Guid.NewGuid();
		readonly Guid org2Pk = Guid.NewGuid();
		readonly Guid org3Pk = Guid.NewGuid();
		readonly Guid org4Pk = Guid.NewGuid();
		readonly Guid org5Pk = Guid.NewGuid();
		readonly Guid inquiryPk = Guid.NewGuid();
		readonly Guid opp1Pk = Guid.NewGuid();
		readonly Guid opp2Pk = Guid.NewGuid();
		readonly Guid opp3Pk = Guid.NewGuid();
		readonly Guid opp4Pk = Guid.NewGuid();
		readonly Guid opp5Pk = Guid.NewGuid();
		readonly Guid opp6Pk = Guid.NewGuid();

		class ReportFilter
		{
			public IEnumerable<string> StaffCodes { get; set; } = Enumerable.Empty<string>();
			public IEnumerable<Guid> OppOrgPks { get; set; } = Enumerable.Empty<Guid>();
			public string OppStatusList { get; set; } = "";
			public DateTime? OppCreateFrom { get; set; }
			public DateTime? OppCreateTo { get; set; }
			public DateTime CurrentDate { get; set; } = DateTime.Today;
		}
	}
}
