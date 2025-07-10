using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.DbUpgrader.Scripts.Definitions.CommissionManagement;
using NUnit.Framework;

namespace Enterprise.Build.Database.Script.Public.CommissionManagement.Testing
{
	[TestedType(typeof(Report_WolfPackRecipientAddReport_List))]
	class Report_WolfPackRecipientAddReport_ListTest : DbCreateScriptTest
	{
		public void TestReportFilter_StaffCodes()
		{
			SetupReportData();
			var filter = new ReportFilter() { StaffCodes = new[] { "SS1", "WP2" }, StaffRole = "WP" };
			var expectedRecipients = new (string, string)[] { ("TSTOPP001#2", "SS1"), ("TSTOPP002#3", "WP2"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("StaffRole: WP | StaffCodes: SS1,WP2", filter, expectedRecipients);

			filter.StaffCodes = new[] { "SS2" };
			expectedRecipients = new (string, string)[] { ("TSTOPP003#4", "SS2") };
			AssertReportFilter("StaffRole: WP | StaffCodes: SS2", filter, expectedRecipients);

			filter.StaffCodes = new[] { "WP1", "WP3" };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP003#5", "WP3") };
			AssertReportFilter("StaffRole: WP | StaffCodes: WP1,WP3", filter, expectedRecipients);

			filter = new ReportFilter() { StaffCodes = new[] { "SS1", "WP2" }, StaffRole = "SR" };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", "") };
			AssertReportFilter("StaffRole: SR | StaffCodes: SS1,WP2", filter, expectedRecipients);

			filter.StaffCodes = new[] { "SS2" };
			expectedRecipients = new (string, string)[] { ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("StaffRole: SR | StaffCodes: SS2", filter, expectedRecipients);

			filter.StaffCodes = new[] { "WP1", "WP3" };
			expectedRecipients = Array.Empty<(string, string)>();
			AssertReportFilter("StaffRole: SR | StaffCodes: WP1,WP3", filter, expectedRecipients);

			filter = new ReportFilter() { StaffCodes = new[] { "SS1", "WP2" }, StaffRole = "WPorSR" };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP002#3", "WP2"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("StaffRole: WPorSR | StaffCodes: SS1,WP2", filter, expectedRecipients);

			filter.StaffCodes = new[] { "SS2" };
			expectedRecipients = new (string, string)[] { ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", ""), ("TSTOPP003#4", "SS2") };
			AssertReportFilter("StaffRole: WPorSR | StaffCodes: SS2", filter, expectedRecipients);

			filter.StaffCodes = new[] { "WP1", "WP3" };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP003#5", "WP3") };
			AssertReportFilter("StaffRole: WPorSR | StaffCodes: WP1,WP3", filter, expectedRecipients);

			filter = new ReportFilter() { StaffCodes = new[] { "SS1", "WP2" }, StaffRole = "WPandSR" };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#2", "SS1") };
			AssertReportFilter("StaffRole: WPandSR | StaffCodes: SS1,WP2", filter, expectedRecipients);

			filter.StaffCodes = new[] { "SS2", "WP1", "WP3" };
			expectedRecipients = Array.Empty<(string, string)>();
			AssertReportFilter("StaffRole: WPandSR | StaffCodes: SS2,WP1,WP3", filter, expectedRecipients);
		}

		public void TestReportFilter_StaffOrgPks()
		{
			SetupReportData();
			var filter = new ReportFilter() { StaffOrgPks = new[] { org1Pk } };
			var expectedRecipients = new (string, string)[] { ("TSTOPP001#2", "") };
			AssertReportFilter("StaffOrgs: Org1", filter, expectedRecipients);

			filter.StaffOrgPks = new[] { org1Pk, org2Pk };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#2", ""), ("TSTOPP003#4", "") };
			AssertReportFilter("StaffOrgs: Org1, Org2", filter, expectedRecipients);

			filter.StaffOrgPks = new[] { org3Pk };
			expectedRecipients = new (string, string)[] { ("TSTOPP002#3", "") };
			AssertReportFilter("StaffOrgs: Org3", filter, expectedRecipients);
		}

		public void TestReportFilter_SalesTeamCodes()
		{
			SetupReportData();
			var filter = new ReportFilter() { SalesTeamCodes = new[] { "ST1" } };

			var expectedRecipients = new (string, string)[] { ("TSTOPP001#2", "SS1"), ("TSTOPP002#3", "WP2"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("SalesTeamCodes: ST1", filter, expectedRecipients);

			filter.SalesTeamCodes = new[] { "ST2" };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("SalesTeamCodes: ST2", filter, expectedRecipients);

			filter.SalesTeamCodes =  new[] { "ST3" };

			expectedRecipients = Array.Empty<(string, string)>();
			AssertReportFilter("SalesTeamCodes: ST3 - Should return no results as ST3 is not a sales team", filter, expectedRecipients);

			filter.SalesTeamCodes = new[] { "ST2", "ST3" };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("SalesTeamCodes: ST2, ST3", filter, expectedRecipients);
		}

		public void TestReportFilter_OppOrgPks()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgPks = new[] { org1Pk } };
			var expectedRecipients = new (string, string)[] { ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", ""), ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("OppOrgs: Org1", filter, expectedRecipients);

			filter.OppOrgPks = new[] { org2Pk, org3Pk };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("OppOrgs: Org2, Org3", filter, expectedRecipients);

			filter.OppOrgPks = new[] { org3Pk };
			expectedRecipients = new (string, string)[] { ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("OppOrgs: Org3", filter, expectedRecipients);
		}

		public void TestReportFilter_OppOrgCountryCodes()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgCountryCodes = new[] { "AU" }, OppOrgPks = new[] { org1Pk, org2Pk, org3Pk } };
			var expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("OppOrgCountryCodes: AU", filter, expectedRecipients);

			filter.OppOrgCountryCodes = new[] { "NZ" };
			expectedRecipients = new (string, string)[] { ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", ""), ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("OppOrgCountryCodes: NZ", filter, expectedRecipients);

			filter.OppOrgCountryCodes = new[] { "AU", "NZ" };
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", ""), ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", ""), ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("OppOrgCountryCodes: AU, NZ - Should return all test recipients", filter, expectedRecipients);
		}

		public void TestReportFilter_AgreementDraft()
		{
			SetupReportData();
			var filter = new ReportFilter() { AgreementDraft = "Not-Draft", OppOrgPks = new[] { org1Pk, org2Pk, org3Pk } };
			var expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", ""), ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", ""), ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("AgreementDraft: Not-Draft", filter, expectedRecipients);

			filter.AgreementDraft = "Draft";
			expectedRecipients = new (string, string)[] { ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", "") };
			AssertReportFilter("AgreementDraft: Draft", filter, expectedRecipients);
		}

		public void TestReportFilter_OppStatusList()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppStatusList = "CRT", OppOrgPks = new[] { org1Pk, org2Pk, org3Pk } };
			var expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", ""), ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("OppStatuses: CRT", filter, expectedRecipients);

			filter.OppStatusList = "WON";
			expectedRecipients = new (string, string)[] { ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("OppStatuses: WON", filter, expectedRecipients);

			filter.OppStatusList = "CRT, WON";
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", ""), ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", ""), ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("OppStatuses: CRT, WON - Should return all test recipients", filter, expectedRecipients);
		}

		public void TestReportFilter_OppCreateDates()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgPks = new[] { org1Pk, org2Pk, org3Pk } };
			filter.OppCreateFrom = new DateTime(2020, 01, 01);
			var expectedRecipients = new (string, string)[] { ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", ""), ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", ""), ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("From: 2020-01-01", filter, expectedRecipients);

			filter.OppCreateFrom = null;
			filter.OppCreateTo = new DateTime(2020, 02, 05);
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("To: 2020-02-05", filter, expectedRecipients);

			filter.OppCreateFrom = new DateTime(2020, 01, 01);
			filter.OppCreateTo = new DateTime(2020, 02, 05);
			expectedRecipients = new (string, string)[] { ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("From: 2020-01-01 | To: 2020-02-05", filter, expectedRecipients);
		}

		public void TestReportFilter_ActualCloseDates()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgPks = new[] { org1Pk, org2Pk, org3Pk } };
			filter.ActualCloseFrom = new DateTime(2020, 06, 01);
			var expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", "") };
			AssertReportFilter("From: 2020-06-01", filter, expectedRecipients);

			filter.ActualCloseFrom = null;
			filter.ActualCloseTo = new DateTime(2020, 02, 01);
			expectedRecipients = Array.Empty<(string, string)>();
			AssertReportFilter("To: 2020-02-01", filter, expectedRecipients);

			filter.ActualCloseFrom = new DateTime(2020, 02, 01);
			filter.ActualCloseTo = new DateTime(2020, 10, 01);
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("From: 2020-02-01 | To: 2020-10-01", filter, expectedRecipients);
		}

		public void TestReportFilter_AgreementEffectiveStartDates()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgPks = new[] { org1Pk, org2Pk, org3Pk } };
			filter.AgreementEffectiveFrom = new DateTime(2020, 03, 01);
			var expectedRecipients = new (string, string)[] { ("TSTOPP003#6", "SS1") };
			AssertReportFilter("From: 2020-03-01", filter, expectedRecipients);

			filter.AgreementEffectiveFrom = null;
			filter.AgreementEffectiveTo = new DateTime(2020, 02, 01);
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("To: 2020-02-01", filter, expectedRecipients);

			filter.AgreementEffectiveFrom = new DateTime(2020, 02, 01);
			filter.AgreementEffectiveTo = new DateTime(2020, 03, 01);
			expectedRecipients = new (string, string)[] { ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", "") };
			AssertReportFilter("From: 2020-02-01 | To: 2020-03-01", filter, expectedRecipients);
		}

		public void TestReportFilter_AgreementStatus()
		{
			SetupReportData();
			var filter = new ReportFilter() { AgreementStatus = "All", OppOrgPks = new[] { org1Pk, org2Pk, org3Pk } };
			var expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", ""), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", ""), ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", ""), ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("AgreementStatus: All", filter, expectedRecipients);

			filter.AgreementStatus = "Reversed";
			expectedRecipients = new (string, string)[] { ("TSTOPP001#2", "SS1"), ("TSTOPP001#2", "") };
			AssertReportFilter("AgreementStatus: Reversed", filter, expectedRecipients);

			filter.AgreementStatus = "Expired";
			filter.CurrentDate = new DateTime(2020, 02, 01);
			expectedRecipients = Array.Empty<(string, string)>();
			AssertReportFilter("AgreementStatus: Expired | Date: 2020-02-01", filter, expectedRecipients);

			filter.CurrentDate = new DateTime(2020, 02, 20);
			expectedRecipients = new (string, string)[] { ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("AgreementStatus: Expired | Date: 2020-02-20", filter, expectedRecipients);

			filter.AgreementStatus = "Inactive";
			filter.CurrentDate = new DateTime(2020, 02, 01);
			expectedRecipients = new (string, string)[] { ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", ""), ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("AgreementStatus: Inactive | Date: 2020-02-01", filter, expectedRecipients);

			filter.CurrentDate = new DateTime(2020, 02, 20);
			expectedRecipients = new (string, string)[] { ("TSTOPP003#5", "WP3"), ("TSTOPP003#6", "SS1") };
			AssertReportFilter("AgreementStatus: Inactive | Date: 2020-02-20", filter, expectedRecipients);

			filter.AgreementStatus = "Active";
			filter.CurrentDate = new DateTime(2020, 02, 01);
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP002#3", "WP2"), ("TSTOPP002#3", "") };
			AssertReportFilter("AgreementStatus: Active | Date: 2020-02-01", filter, expectedRecipients);

			filter.CurrentDate = new DateTime(2020, 02, 20);
			expectedRecipients = new (string, string)[] { ("TSTOPP001#1", "WP1"), ("TSTOPP003#4", "SS2"), ("TSTOPP003#4", "") };
			AssertReportFilter("AgreementStatus: Active | Date: 2020-02-20", filter, expectedRecipients);
		}

		public void TestReportOutput()
		{
			SetupReportData();
			var filter = new ReportFilter() { OppOrgPks = new[] { org2Pk } };
			var result = GetReportResult(filter).AsEnumerable();
			AssertEquals("Precondition: Should have returned three results for selected opportunity organisation", 3, result.Count());

			var orderedResults = result.OrderBy(r => r["AgreementName"]).ThenByDescending(r => r["RecipientCode"]).ToList();

			CombineAssertions("Record 1 fields are incorrect", () =>
			{
				AssertEquals("P8_OpportunityId", "TSTOPP001", orderedResults[0]["P8_OpportunityId"]);
				AssertEquals("OpportunityPK", opp1Pk, orderedResults[0]["OpportunityPK"]);
				AssertEquals("OpportunityOrgCode", "TESTORGBBB", orderedResults[0]["OpportunityOrgCode"]);
				AssertEquals("OpportunityOrgPK", org2Pk, orderedResults[0]["OpportunityOrgPK"]);
				AssertEquals("OpportunityOrgName", "Test Organisation BBB", orderedResults[0]["OpportunityOrgName"]);
				AssertEquals("OpportunityCreateDate", new DateTime(2019, 10, 15, 11, 10, 0), orderedResults[0]["OpportunityCreateDate"]);
				AssertEquals("OpportunitySalesPersonName", "Sales Staff 1", orderedResults[0]["OpportunitySalesPersonName"]);
				AssertEquals("OpportunitySalesPersonTeams", "ST1, ST2", orderedResults[0]["OpportunitySalesPersonTeams"]);
				AssertEquals("P8_OpportunityDescription", "Test Opportunity 1", orderedResults[0]["P8_OpportunityDescription"]);
				AssertEquals("P8_Stage", "A1", orderedResults[0]["P8_Stage"]);
				AssertEquals("P8_Source", "OTH", orderedResults[0]["P8_Source"]);
				AssertEquals("P8_SourceDetails", "Test Details", orderedResults[0]["P8_SourceDetails"]);
				AssertEquals("OpportunityLastEditDate", new DateTime(2019, 10, 15, 13, 20, 0), orderedResults[0]["OpportunityLastEditDate"]);
				AssertEquals("P8_EstimatedCloseDate", new DateTime(2020, 07, 15), orderedResults[0]["P8_EstimatedCloseDate"]);
				AssertEquals("P8_ClosedDate", new DateTime(2020, 08, 26), orderedResults[0]["P8_ClosedDate"]);
				AssertEquals("P8_Outcome", "OUT", orderedResults[0]["P8_Outcome"]);
				AssertEquals("OpportunitySalesType", "AAA", orderedResults[0]["OpportunitySalesType"]);
				AssertEquals("OpportunityProductType", "AAA", orderedResults[0]["OpportunityProductType"]);
				AssertEquals("OpportunityStatus", "CRT", orderedResults[0]["OpportunityStatus"]);
				AssertEquals("OpportunityTotalEstimatedValue", 2000m, orderedResults[0]["OpportunityTotalEstimatedValue"]);
				AssertEquals("OpportunityCurrency", "AUD", orderedResults[0]["OpportunityCurrency"]);
				AssertEquals("OpportunityCloseReason", "PRO", orderedResults[0]["OpportunityCloseReason"]);
				AssertEquals("OpportunityOrgUNLOCO", "AUSYD", orderedResults[0]["OpportunityOrgUNLOCO"]);
				AssertEquals("OpportunityOrgCountryCode", "AU", orderedResults[0]["OpportunityOrgCountryCode"]);
				AssertEquals("OpportunityOrgCountryName", "Australia", orderedResults[0]["OpportunityOrgCountryName"]);
				AssertEquals("InquiryID", "TSTINQ001", orderedResults[0]["InquiryID"]);
				AssertEquals("InquiryPK", inquiryPk, orderedResults[0]["InquiryPK"]);
				AssertEquals("AgreementName", "TSTOPP001#1", orderedResults[0]["AgreementName"]);
				AssertEquals("CA0_CommissionStream", "AAA", orderedResults[0]["CA0_CommissionStream"]);
				AssertEquals("CustomerName", "Test Organisation AAA", orderedResults[0]["CustomerName"]);
				AssertEquals("CA0_CommissionTriggerType", "MAN", orderedResults[0]["CA0_CommissionTriggerType"]);
				AssertEquals("VCA_EffectiveDate", new DateTime(2020, 01, 15), orderedResults[0]["VCA_EffectiveDate"]);
				AssertEquals("EffectiveDateEnd", DBNull.Value, orderedResults[0]["EffectiveDateEnd"]);
				AssertEquals("StatusDescription", "Active", orderedResults[0]["StatusDescription"]);
				AssertEquals("CA0_CommissionBasis", "REV", orderedResults[0]["CA0_CommissionBasis"]);
				AssertEquals("CA0_LastApprovedDateUtc", new DateTime(2020, 01, 11, 11, 11, 0), orderedResults[0]["CA0_LastApprovedDateUtc"]);
				AssertEquals("CA0_SystemLastEditTimeUtc", new DateTime(2020, 01, 11, 11, 11, 0), orderedResults[0]["CA0_SystemLastEditTimeUtc"]);
				AssertEquals("DraftStatus", "Not-Draft", orderedResults[0]["DraftStatus"]);
				AssertEquals("CA0_ExpiredDate", DBNull.Value, orderedResults[0]["CA0_ExpiredDate"]);
				AssertEquals("CA0_ReversedDateUtc", DBNull.Value, orderedResults[0]["CA0_ReversedDateUtc"]);
				AssertEquals("RecipientCode", "WP1", orderedResults[0]["RecipientCode"]);
				AssertEquals("EntityTeams", "ST2", orderedResults[0]["EntityTeams"]);
				AssertEquals("EntityOrgName", DBNull.Value, orderedResults[0]["EntityOrgName"]);
				AssertEquals("CAR_CommissionType", "PCT", orderedResults[0]["CAR_CommissionType"]);
				AssertEquals("SharePercent", DBNull.Value, orderedResults[0]["SharePercent"]);
				AssertEquals("CAR_EndDate", new DateTime(2020, 05, 11), orderedResults[0]["CAR_EndDate"]);
				AssertEquals("CAR_SystemCreateTimeUtc", new DateTime(2020, 01, 11, 12, 12, 0), orderedResults[0]["CAR_SystemCreateTimeUtc"]);
				AssertEquals("CommissionPeriodList", "NEW, EXS", orderedResults[0]["CommissionPeriodList"]);
				AssertEquals("EntityRateLatestEndDate", new DateTime(2022, 01, 14), orderedResults[0]["EntityRateLatestEndDate"]);
				AssertEquals("P8_DiscountAmount", 19m, orderedResults[0]["P8_DiscountAmount"]);
				AssertEquals("P8_RentalMultiplier", 83m, orderedResults[0]["P8_RentalMultiplier"]);
				AssertEquals("OpportunityEstimatedValueShare", DBNull.Value, orderedResults[0]["OpportunityEstimatedValueShare"]);
			});

			CombineAssertions("Record 2 fields are incorrect", () =>
			{
				AssertEquals("OpportunityPK", opp1Pk, orderedResults[1]["OpportunityPK"]);
				AssertEquals("AgreementName", "TSTOPP001#2", orderedResults[1]["AgreementName"]);
				AssertEquals("CA0_CommissionStream", "AAA", orderedResults[1]["CA0_CommissionStream"]);
				AssertEquals("CustomerName", "Test Organisation BBB", orderedResults[1]["CustomerName"]);
				AssertEquals("CA0_CommissionTriggerType", "MAN", orderedResults[1]["CA0_CommissionTriggerType"]);
				AssertEquals("VCA_EffectiveDate", new DateTime(2020, 02, 22), orderedResults[1]["VCA_EffectiveDate"]);
				AssertEquals("EffectiveDateEnd", new DateTime(2020, 08, 22), orderedResults[1]["EffectiveDateEnd"]);
				AssertEquals("StatusDescription", "Reversed", orderedResults[1]["StatusDescription"]);
				AssertEquals("CA0_CommissionBasis", "REV", orderedResults[1]["CA0_CommissionBasis"]);
				AssertEquals("CA0_LastApprovedDateUtc", DBNull.Value, orderedResults[1]["CA0_LastApprovedDateUtc"]);
				AssertEquals("CA0_SystemLastEditTimeUtc", new DateTime(2019, 10, 16, 11, 11, 0), orderedResults[1]["CA0_SystemLastEditTimeUtc"]);
				AssertEquals("DraftStatus", "Draft", orderedResults[1]["DraftStatus"]);
				AssertEquals("CA0_ExpiredDate", new DateTime(2020, 08, 22), orderedResults[1]["CA0_ExpiredDate"]);
				AssertEquals("CA0_ReversedDateUtc", new DateTime(2020, 01, 15, 10, 40, 0), orderedResults[1]["CA0_ReversedDateUtc"]);
				AssertEquals("RecipientCode", "SS1", orderedResults[1]["RecipientCode"]);
				AssertEquals("EntityTeams", "ST1, ST2", orderedResults[1]["EntityTeams"]);
				AssertEquals("EntityOrgName", DBNull.Value, orderedResults[1]["EntityOrgName"]);
				AssertEquals("CAR_CommissionType", "PCT", orderedResults[1]["CAR_CommissionType"]);
				AssertEquals("SharePercent", 75m, orderedResults[1]["SharePercent"]);
				AssertEquals("CAR_EndDate", new DateTime(2020, 04, 16), orderedResults[1]["CAR_EndDate"]);
				AssertEquals("CAR_SystemCreateTimeUtc", new DateTime(2019, 10, 16, 11, 11, 0), orderedResults[1]["CAR_SystemCreateTimeUtc"]);
				AssertEquals("CommissionPeriodList", "", orderedResults[1]["CommissionPeriodList"]);
				AssertEquals("EntityRateLatestEndDate", new DateTime(2020, 06, 18), orderedResults[1]["EntityRateLatestEndDate"]);
				AssertEquals("P8_DiscountAmount", 19m, orderedResults[1]["P8_DiscountAmount"]);
				AssertEquals("P8_RentalMultiplier", 83m, orderedResults[1]["P8_RentalMultiplier"]);
				AssertEquals("OpportunityEstimatedValueShare", 1500m, orderedResults[1]["OpportunityEstimatedValueShare"]);
			});

			CombineAssertions("Record 3 fields are incorrect", () =>
			{
				AssertEquals("OpportunityPK", opp1Pk, orderedResults[2]["OpportunityPK"]);
				AssertEquals("AgreementName", "TSTOPP001#2", orderedResults[2]["AgreementName"]);
				AssertEquals("RecipientCode", "", orderedResults[2]["RecipientCode"]);
				AssertEquals("EntityTeams", DBNull.Value, orderedResults[2]["EntityTeams"]);
				AssertEquals("EntityOrgName", "Test Organisation AAA", orderedResults[2]["EntityOrgName"]);
				AssertEquals("CAR_CommissionType", "PCT", orderedResults[2]["CAR_CommissionType"]);
				AssertEquals("SharePercent", 25m, orderedResults[2]["SharePercent"]);
				AssertEquals("CAR_EndDate", new DateTime(2020, 04, 17), orderedResults[2]["CAR_EndDate"]);
				AssertEquals("CAR_SystemCreateTimeUtc", new DateTime(2019, 10, 17, 14, 21, 0), orderedResults[2]["CAR_SystemCreateTimeUtc"]);
				AssertEquals("CommissionPeriodList", "", orderedResults[2]["CommissionPeriodList"]);
				AssertEquals("EntityRateLatestEndDate", DBNull.Value, orderedResults[2]["EntityRateLatestEndDate"]);
				AssertEquals("P8_DiscountAmount", 19m, orderedResults[2]["P8_DiscountAmount"]);
				AssertEquals("P8_RentalMultiplier", 83m, orderedResults[2]["P8_RentalMultiplier"]);
				AssertEquals("OpportunityEstimatedValueShare", 500m, orderedResults[2]["OpportunityEstimatedValueShare"]);
			});
		}

		void AssertReportFilter(string message, ReportFilter filter, (string, string)[] expectedRecipients)
		{
			var rows = GetReportResult(filter).Rows.Cast<DataRow>();
			var actualRecipients = rows.Select(r => ((string)r["AgreementName"], (string)r["RecipientCode"]));
			AssertContainsExactElementsInAnyOrder(message, expectedRecipients, actualRecipients);
		}

		DataTable GetReportResult(ReportFilter filter)
		{
			var commissionPeriodTable = new DataTable();
			commissionPeriodTable.Locale = CultureInfo.InvariantCulture;
			commissionPeriodTable.Columns.Add("Code", typeof(string)); // Part of SQL code
			commissionPeriodTable.Columns.Add("Description", typeof(string)); // Part of SQL code
			commissionPeriodTable.Columns.Add("Start", typeof(int)); // Part of SQL code
			commissionPeriodTable.Columns.Add("End", typeof(int)); // Part of SQL code
			commissionPeriodTable.Columns.Add("IsEnabled", typeof(bool)); // Part of SQL code

			commissionPeriodTable.Rows.Add(new object[] { "NEW", "New client", 0, 12, true });
			commissionPeriodTable.Rows.Add(new object[] { "EXS", "Existing client", 12, 24, true });

			var command = TestConnection.Command("SELECT * FROM Report_WolfPackRecipientAddReport_List(@StaffCodes, @StaffCodesIsEmpty, @StaffRole, @StaffOrgPKs, @StaffOrgPKsIsEmpty, @EntitySalesTeamCodes, @EntitySalesTeamCodesIsEmpty, @OpportunityOrgPKs, @OpportunityOrgPKsIsEmpty, @OpportunityOrgCountryCodes, @OpportunityOrgCountryCodesIsEmpty, @AgreementDraft, @OpportunityStatusList, @OpportunityCreateFromDate, @OpportunityCreateToDate, @ActualCloseFromDate, @ActualCloseToDate, @AgreementEffectiveStartFromDate, @AgreementEffectiveStartToDate, @AgreementStatus, @CommissionPeriodRanges, @CurrentDate)");
			command.AddTableValuedParameter("@StaffCodes", "dbo.TVP_varchar_250", filter.StaffCodes);
			command.AddTableValuedParameter("@StaffOrgPKs", "dbo.TVP_uniqueidentifier", filter.StaffOrgPks);
			command.AddTableValuedParameter("@OpportunityOrgPKs", "dbo.TVP_uniqueidentifier", filter.OppOrgPks);
			command.AddTableValuedParameter("@OpportunityOrgCountryCodes", "dbo.TVP_varchar_250", filter.OppOrgCountryCodes);
			command.AddTableValuedParameter("@CommissionPeriodRanges", "dbo.TVP_CommissionPeriod", commissionPeriodTable);
			command.AddTableValuedParameter("@EntitySalesTeamCodes", "dbo.TVP_varchar_250", filter.SalesTeamCodes);

			command.AddParameter("@StaffCodesIsEmpty", SqlDbType.Bit, filter.StaffCodes.Any() ? 0 : 1);
			command.AddParameter("@StaffOrgPKsIsEmpty", SqlDbType.Bit, filter.StaffOrgPks.Any() ? 0 : 1);
			command.AddParameter("@OpportunityOrgPKsIsEmpty", SqlDbType.Bit, filter.OppOrgPks.Any() ? 0 : 1);
			command.AddParameter("@OpportunityOrgCountryCodesIsEmpty", SqlDbType.Bit, filter.OppOrgCountryCodes.Any() ? 0 : 1);
			command.AddParameter("@EntitySalesTeamCodesIsEmpty", SqlDbType.Bit, filter.SalesTeamCodes.Any() ? 0 : 1);

			command.AddParameter("@StaffRole", SqlDbType.VarChar, filter.StaffRole);
			command.AddParameter("@AgreementDraft", SqlDbType.VarChar, filter.AgreementDraft);
			command.AddParameter("@OpportunityStatusList", SqlDbType.VarChar, filter.OppStatusList);
			command.AddParameter("@OpportunityCreateFromDate", SqlDbType.SmallDateTime, (object)filter.OppCreateFrom ?? DBNull.Value);
			command.AddParameter("@OpportunityCreateToDate", SqlDbType.SmallDateTime, (object)filter.OppCreateTo ?? DBNull.Value);
			command.AddParameter("@ActualCloseFromDate", SqlDbType.SmallDateTime, (object)filter.ActualCloseFrom ?? DBNull.Value);
			command.AddParameter("@ActualCloseToDate", SqlDbType.SmallDateTime, (object)filter.ActualCloseTo ?? DBNull.Value);
			command.AddParameter("@AgreementEffectiveStartFromDate", SqlDbType.SmallDateTime, (object)filter.AgreementEffectiveFrom ?? DBNull.Value);
			command.AddParameter("@AgreementEffectiveStartToDate", SqlDbType.SmallDateTime, (object)filter.AgreementEffectiveTo ?? DBNull.Value);
			command.AddParameter("@AgreementStatus", SqlDbType.VarChar, filter.AgreementStatus);
			command.AddParameter("@CurrentDate", SqlDbType.Date, filter.CurrentDate);

			return DataUtils.GetDataTableFromCommand(command);
		}

		void SetupReportData()
		{
			var commandText = $@"
DECLARE @CompanyPk UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesStaff1Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesStaff2Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @WPStaff1Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @WPStaff2Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @WPStaff3Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesTeam1Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesTeam2Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @SalesTeam3Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @RatePk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @RatePk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @RatePk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement1Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement2Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement3Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement4Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement5Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @Agreement6Pk UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk1 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk2 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk3 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk4 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk5 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk6 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk7 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk8 UNIQUEIDENTIFIER = NEWID();
DECLARE @RecipientPk9 UNIQUEIDENTIFIER = NEWID();

INSERT INTO dbo.GlbCompany (GC_PK, GC_Code, GC_Name, GC_RN_NKCountryCode, GC_RX_NKLocalCurrency) VALUES (@CompanyPk, 'TES', 'AU company', 'AU', 'AUD')
INSERT dbo.GlbStaff(GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@SalesStaff1Pk, 'SS1', 'Sales Staff 1', 'SS1 Login', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT dbo.GlbStaff(GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@SalesStaff2Pk, 'SS2', 'Sales Staff 2', 'SS2 Login', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT dbo.GlbStaff(GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@WPStaff1Pk, 'WP1', 'Wolf Pack Staff 1', 'WP1 Login', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT dbo.GlbStaff(GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@WPStaff2Pk, 'WP2', 'Wolf Pack Staff 2', 'WP2 Login', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT dbo.GlbStaff(GS_PK, GS_Code, GS_FullName, GS_LoginName, GS_SystemCreateTimeUtc, GS_SystemCreateUser, GS_SystemLastEditTimeUtc, GS_SystemLastEditUser) VALUES (@WPStaff3Pk, 'WP3', 'Wolf Pack Staff 3', 'WP3 Login', GetUtcDate(), '~BP', GetUtcDate(), '~BP')
INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsSales) VALUES (@SalesTeam1Pk, 'ST1', 'Team 1', 1)
INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsSales) VALUES (@SalesTeam2Pk, 'ST2', 'Team 2', 1)
INSERT dbo.GlbGroup (GG_PK, GG_Code, GG_Desc, GG_IsSales) VALUES (@SalesTeam3Pk, 'ST3', 'Team 3', 0)
INSERT dbo.GlbGroupLink (GK_PK, GK_GS, GK_GG) VALUES (NEWID(), @SalesStaff1Pk, @SalesTeam1Pk)
INSERT dbo.GlbGroupLink (GK_PK, GK_GS, GK_GG) VALUES (NEWID(), @SalesStaff1Pk, @SalesTeam2Pk)
INSERT dbo.GlbGroupLink (GK_PK, GK_GS, GK_GG) VALUES (NEWID(), @SalesStaff2Pk, @SalesTeam3Pk)
INSERT dbo.GlbGroupLink (GK_PK, GK_GS, GK_GG) VALUES (NEWID(), @WPStaff1Pk, @SalesTeam2Pk)
INSERT dbo.GlbGroupLink (GK_PK, GK_GS, GK_GG) VALUES (NEWID(), @WPStaff2Pk, @SalesTeam1Pk)
INSERT dbo.GlbGroupLink (GK_PK, GK_GS, GK_GG) VALUES (NEWID(), @WPStaff2Pk, @SalesTeam3Pk)

INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org1Pk}', 'TESTORGAAA', 'Test Organisation AAA', 'NZAKL')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org2Pk}', 'TESTORGBBB', 'Test Organisation BBB', 'AUSYD')
INSERT INTO dbo.OrgHeader (OH_PK, OH_Code, OH_FullName, OH_RL_NKClosestPort) VALUES ('{org3Pk}', 'TESTORGCCC', 'Test Organisation CCC', 'AUSYD')
INSERT INTO dbo.OrgColdCallRegister(O1_PK, O1_LeadUniqueReference, O1_EnquiryType, O1_LeadStatus, O1_SystemCreateTimeUtc, O1_SystemCreateUser, O1_SystemLastEditTimeUtc, O1_SystemLastEditUser) VALUES('{inquiryPk}', 'TSTINQ001', 'INQ', 'OPN', GetUtcDate(), 'E', GetUtcDate(), 'E');
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_GS_NKPrimarySalesPerson, P8_EstimatedCloseDate, P8_ClosedDate, P8_EstimatedValue, P8_RX_NKEstimatedValueCurrency, P8_LostReason, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser, P8_Outcome, P8_PackageType, P8_Source, P8_SourceDetails, P8_O1_Enquiry, P8_DiscountAmount, P8_RentalMultiplier)
VALUES ('{opp1Pk}', 'TSTOPP001', '{org2Pk}', @CompanyPk, 'AAA', 'A1', 'CRT', 'Test Opportunity 1', 'SS1', '2020-07-15', '2020-08-26', 2000, 'AUD', 'PRO', '2019-10-15 11:10', 'E', '2019-10-15 13:20', 'E', 'OUT', 'AAA', 'OTH', 'Test Details', '{inquiryPk}', 19, 83)
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_GS_NKPrimarySalesPerson, P8_ClosedDate, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES ('{opp2Pk}', 'TSTOPP002', '{org3Pk}', @CompanyPk, 'AAA', 'A1', 'WON', 'Test Opportunity 2', 'SS2', '2020-02-02', '2020-01-10 13:40', 'E', '2020-01-15 14:50', 'E')
INSERT INTO dbo.OrgOpportunity (P8_PK, P8_OpportunityID, P8_OH, P8_GC, P8_OpportunityType, P8_Stage, P8_Status, P8_OpportunityDescription, P8_SystemCreateTimeUtc, P8_SystemCreateUser, P8_SystemLastEditTimeUtc, P8_SystemLastEditUser)
VALUES ('{opp3Pk}', 'TSTOPP003', '{org1Pk}', @CompanyPk, 'AAA', 'A1', 'CRT', 'Test Opportunity 3', '2020-02-07 09:00', 'E', '2020-02-07 09:00', 'E')

INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement1Pk, '#1', '{opp1Pk}', '{org1Pk}', 'MAN', '2020-01-15', 'REV', '2020-01-11 11:11', NULL, NULL, '2020-01-10 12:12', 'XX','2020-01-11 11:11', 'XX', 'AAA')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream, CA0_CA0_ParentVersion)
VALUES (@Agreement2Pk, '#2', '{opp1Pk}', '{org2Pk}', 'MAN', '2020-02-22', 'REV', NULL, '2020-08-22', '2020-01-15 10:40', '2019-10-15 12:12', 'XX','2019-10-16 11:11', 'XX', 'AAA', @Agreement1Pk)
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement3Pk, '#3', '{opp2Pk}', '{org1Pk}', 'MAN', '2020-01-17', 'PRF', '2020-01-11 17:43', '2020-02-17', NULL, '2020-01-11 17:43', 'XX','2020-01-11 17:43', 'XX', 'AAA')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement4Pk, '#4', '{opp3Pk}', '{org1Pk}', 'MAN', '2020-02-10', 'REV', '2020-02-02 07:11', NULL, NULL, '2020-02-02 07:11', 'XX','2020-02-02 07:11', 'XX', 'AAA')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement5Pk, '#5', '{opp3Pk}', '{org2Pk}', 'MAN', NULL, 'REV', NULL, NULL, NULL, '2020-02-02 07:11', 'XX','2020-02-02 07:11', 'XX', 'AAA')
INSERT INTO dbo.OrgCommissionAgreement (CA0_PK, CA0_Name, CA0_P8, CA0_OH_Customer, CA0_CommissionTriggerType, CA0_EffectiveDate, CA0_CommissionBasis, CA0_LastApprovedDateUtc, CA0_ExpiredDate, CA0_ReversedDateUtc, CA0_SystemCreateTimeUtc, CA0_SystemCreateUser, CA0_SystemLastEditTimeUtc, CA0_SystemLastEditUser, CA0_CommissionStream)
VALUES (@Agreement6Pk, '#6', '{opp3Pk}', '{org3Pk}', 'MAN', '2020-03-03', 'REV', '2020-02-02 07:11', NULL, NULL, '2020-02-02 07:11', 'XX','2020-02-02 07:11', 'XX', 'AAA')

INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk1, @Agreement1Pk, NULL, 'WP1', 'PCT', 0, 0, '2020-05-11', '2020-01-11 12:12', 'E', '2020-01-11 12:12', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk2, @Agreement2Pk, NULL, 'SS1', 'PCT', 3, 0, '2020-04-16', '2019-10-16 11:11', 'E', '2019-10-16 11:11', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk3, @Agreement2Pk, '{org1Pk}', '', 'PCT', 1, 0, '2020-04-17', '2019-10-17 14:21', 'E', '2019-10-17 14:21', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk4, @Agreement3Pk, NULL, 'WP2', 'PCT', 1, 0, '2020-05-13', '2020-01-11 17:43', 'E', '2020-01-11 17:43', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk5, @Agreement3Pk, '{org3Pk}', '', 'PCT', 1, 0, '2020-05-14', '2020-01-11 17:43', 'E', '2020-01-11 17:43', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk6, @Agreement4Pk, NULL, 'SS2', 'PCT', 1, 0, '2020-05-15', '2020-02-02 07:11', 'E', '2020-02-02 07:11', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk7, @Agreement4Pk, '{org2Pk}', '', 'PCT', 1, 0, '2020-05-16', '2020-02-02 07:11', 'E', '2020-02-02 07:11', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk8, @Agreement5Pk, NULL, 'WP3', 'PCT', 1, 0, '2020-05-17', '2020-02-02 07:11', 'E', '2020-02-02 07:11', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipient (CAR_PK, CAR_CA0, CAR_OH_Party, CAR_GS_NKStaff, CAR_CommissionType, CAR_Share, CAR_IsCommissionRateOverriden, CAR_EndDate, CAR_SystemCreateTimeUtc, CAR_SystemCreateUser, CAR_SystemLastEditTimeUtc, CAR_SystemLastEditUser)
VALUES (@RecipientPk9, @Agreement6Pk, NULL, 'SS1', 'PCT', 1, 0, '2020-05-18', '2020-02-02 07:11', 'E', '2020-02-02 07:11', 'E')

INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_CommissionPercentage, CAT_CommissionAmount, CAT_CommissionPeriod, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser)
VALUES (@RatePk1, @RecipientPk1, 5.00, 0, 'NEW', '2020-01-11 12:12', 'E', '2020-01-11 12:12', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_CommissionPercentage, CAT_CommissionAmount, CAT_CommissionPeriod, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser)
VALUES (@RatePk2, @RecipientPk1, 6.00, 0, 'EXS', '2020-01-11 12:12', 'E', '2020-01-11 12:12', 'E')
INSERT INTO dbo.OrgCommissionAgreementRecipientRate (CAT_PK, CAT_CAR, CAT_CommissionPercentage, CAT_CommissionAmount, CAT_CommissionPeriod, CAT_CommissionEndDateOverride, CAT_SystemCreateTimeUtc, CAT_SystemCreateUser, CAT_SystemLastEditTimeUtc, CAT_SystemLastEditUser)
VALUES (@RatePk3, @RecipientPk2, 5.00, 0, '', '2020-06-18', '2019-10-16 11:11', 'E', '2019-10-16 11:11', 'E')
";
			TestConnection.ExecuteNonQuery(commandText);
		}

		readonly Guid org1Pk = Guid.NewGuid();
		readonly Guid org2Pk = Guid.NewGuid();
		readonly Guid org3Pk = Guid.NewGuid();
		readonly Guid inquiryPk = Guid.NewGuid();
		readonly Guid opp1Pk = Guid.NewGuid();
		readonly Guid opp2Pk = Guid.NewGuid();
		readonly Guid opp3Pk = Guid.NewGuid();

		class ReportFilter
		{
			public IEnumerable<string> StaffCodes { get; set; } = Enumerable.Empty<string>();
			public IEnumerable<string> OppOrgCountryCodes { get; set; } = Enumerable.Empty<string>();
			public IEnumerable<Guid> StaffOrgPks { get; set; } = Enumerable.Empty<Guid>();
			public IEnumerable<Guid> OppOrgPks { get; set; } = Enumerable.Empty<Guid>();
			public string StaffRole { get; set; } = "";
			public IEnumerable<string> SalesTeamCodes { get; set; } = Enumerable.Empty<string>();
			public string AgreementDraft { get; set; } = "";
			public string OppStatusList { get; set; } = "";
			public DateTime? OppCreateFrom { get; set; }
			public DateTime? OppCreateTo { get; set; }
			public DateTime? ActualCloseFrom { get; set; }
			public DateTime? ActualCloseTo { get; set; }
			public DateTime? AgreementEffectiveFrom { get; set; }
			public DateTime? AgreementEffectiveTo { get; set; }
			public string AgreementStatus { get; set; } = "";
			public DateTime CurrentDate { get; set; } = DateTime.Today;
		}
	}
}

