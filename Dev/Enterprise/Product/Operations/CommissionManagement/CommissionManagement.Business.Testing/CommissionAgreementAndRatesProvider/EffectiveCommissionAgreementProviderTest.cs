using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class EffectiveCommissionAgreementProviderTest : TestCaseWithFactory
	{
		public void TestGeneralUsage()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_Code = "ADL";
			var customer = Factory.NewWithValidTestData<OrgHeader>();

			var opportunity = Factory.NewWithValidTestData<OrgOpportunity>();
			opportunity.P8_Status = "WON";

			var uneffectiveAgreement_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			uneffectiveAgreement_XXX_XXX_XXX.CA0_Name = "#1";
			uneffectiveAgreement_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			uneffectiveAgreement_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			uneffectiveAgreement_XXX_XXX_XXX.CA0_EffectiveDate = ZDate.Empty;
			uneffectiveAgreement_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(uneffectiveAgreement_XXX_XXX_XXX, "XXX", "XXX", "XXX");
			AssertEquals("Precondition: IsApproved", true, uneffectiveAgreement_XXX_XXX_XXX.IsApproved);

			var expiredAgreement_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			expiredAgreement_XXX_XXX_XXX.CA0_Name = "#2";
			expiredAgreement_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			expiredAgreement_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			expiredAgreement_XXX_XXX_XXX.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			expiredAgreement_XXX_XXX_XXX.CA0_ExpiredDate = new ZDate(1999, 1, 1);
			expiredAgreement_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(expiredAgreement_XXX_XXX_XXX, "XXX", "XXX", "XXX");
			AssertEquals("Precondition: IsApproved", true, expiredAgreement_XXX_XXX_XXX.IsApproved);

			var reversedAgreement_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			reversedAgreement_XXX_XXX_XXX.CA0_Name = "#3";
			reversedAgreement_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			reversedAgreement_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			reversedAgreement_XXX_XXX_XXX.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			reversedAgreement_XXX_XXX_XXX.Reverse();
			reversedAgreement_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(reversedAgreement_XXX_XXX_XXX, "XXX", "XXX", "XXX");
			AssertEquals("Precondition: IsApproved", true, reversedAgreement_XXX_XXX_XXX.IsApproved);
			AssertEquals("Precondition: IsReversed", true, reversedAgreement_XXX_XXX_XXX.IsReversed);

			var unapprovedAgreement_XXX_XXX_ALL = opportunity.CommissionAgreements.AddNew();
			unapprovedAgreement_XXX_XXX_ALL.CA0_Name = "#4";
			unapprovedAgreement_XXX_XXX_ALL.CA0_OH_Customer = customer.PK;
			unapprovedAgreement_XXX_XXX_ALL.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			unapprovedAgreement_XXX_XXX_ALL.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			unapprovedAgreement_XXX_XXX_ALL.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(unapprovedAgreement_XXX_XXX_ALL, "XXX", "XXX", "ALL");
			AssertEquals("Precondition: IsApproved", false, unapprovedAgreement_XXX_XXX_ALL.IsApproved);

			var agreement_XXX_ALL_ALL = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement_XXX_ALL_ALL.CA0_Name = "#5";
			agreement_XXX_ALL_ALL.CA0_OH_Customer = customer.PK;
			agreement_XXX_ALL_ALL.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement_XXX_ALL_ALL.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreement_XXX_ALL_ALL.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement_XXX_ALL_ALL, "XXX", "ALL", "ALL");
			AssertEquals("Precondition: IsApproved", true, agreement_XXX_ALL_ALL.IsApproved);

			var agreement_ALL_ALL_ALL = opportunity.ApprovedCommissionAgreements.AddNew();
			agreement_ALL_ALL_ALL.CA0_Name = "#6";
			agreement_ALL_ALL_ALL.CA0_OH_Customer = customer.PK;
			agreement_ALL_ALL_ALL.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement_ALL_ALL_ALL.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreement_ALL_ALL_ALL.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreement_ALL_ALL_ALL, "ALL", "ALL", "ALL");
			AssertEquals("Precondition: IsApproved", true, agreement_ALL_ALL_ALL.IsApproved);

			var staffRecipient = agreement_XXX_ALL_ALL.Recipients.AddNew();
			staffRecipient.CAR_GS_NKStaff = "ADL";
			var staffRate_2000_2002 = staffRecipient.Rates.AddNew();
			staffRate_2000_2002.CAT_CommissionPeriod = "";
			staffRate_2000_2002.CAT_CommissionStartDate = new ZDate(2000, 1, 1);
			staffRate_2000_2002.CAT_CommissionEndDate = new ZDate(2002, 1, 1);
			var staffRate_2002_2005 = staffRecipient.Rates.AddNew();
			staffRate_2002_2005.CAT_CommissionPeriod = "";
			staffRate_2002_2005.CAT_CommissionStartDate = new ZDate(2002, 1, 2);
			staffRate_2002_2005.CAT_CommissionEndDate = new ZDate(2005, 1, 1);

			var agreementForWbpStream_XXX_XXX_XXX = opportunity.ApprovedCommissionAgreements.AddNew();
			agreementForWbpStream_XXX_XXX_XXX.CA0_Name = "#10";
			agreementForWbpStream_XXX_XXX_XXX.CA0_OH_Customer = customer.PK;
			agreementForWbpStream_XXX_XXX_XXX.CA0_CommissionStream = "WBP";
			agreementForWbpStream_XXX_XXX_XXX.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreementForWbpStream_XXX_XXX_XXX.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreementForWbpStream_XXX_XXX_XXX.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItem(agreementForWbpStream_XXX_XXX_XXX, "XXX", "XXX", "XXX");
			AssertEquals("Precondition: IsApproved", true, agreementForWbpStream_XXX_XXX_XXX.IsApproved);

			var agreementWithMode = opportunity.ApprovedCommissionAgreements.AddNew();
			agreementWithMode.CA0_Name = "#11";
			agreementWithMode.CA0_OH_Customer = customer.PK;
			agreementWithMode.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreementWithMode.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreementWithMode.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItemWithConditions(agreementWithMode, "XXX", "YYY", "YYY", "SHP", "", "");

			var agreementWithModeAndOrigin = opportunity.ApprovedCommissionAgreements.AddNew();
			agreementWithModeAndOrigin.CA0_Name = "#12";
			agreementWithModeAndOrigin.CA0_OH_Customer = customer.PK;
			agreementWithModeAndOrigin.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreementWithModeAndOrigin.CA0_EffectiveDate = new ZDate(2000, 1, 1);
			agreementWithModeAndOrigin.FillWithValidTestData();
			OrgCommissionAgreementTestHelper.SetSingleCommissionAgreementOverallItemWithConditions(agreementWithModeAndOrigin, "XXX", "YYY", "YYY", "SHP", "AUSYD", "");

			Factory.Save();

			var args = new EffectiveCommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(2003, 1, 1), ZString.Empty, ZString.Empty, ZString.Empty);
			var provider = EffectiveCommissionAgreementProvider.New(Factory, args);
			AssertEquals(2, provider.ByStream.Count);

			var defaultStreamAgreementAndRates = provider.ByStream[""];
			AssertEquals("Should have returned XXX-ALL-ALL (not XXX-XXX-XXX since it is not an effective agreement, and not XXX-XXX-ALL since it is not approved)", agreement_XXX_ALL_ALL.PK, defaultStreamAgreementAndRates.CommissionAgreement.PK);
			AssertEquals(1, defaultStreamAgreementAndRates.RecipientRatePairs.Count);
			AssertEquals(staffRecipient.PK, defaultStreamAgreementAndRates.RecipientRatePairs.Single().Recipient.PK);
			AssertEquals(staffRate_2002_2005.PK, defaultStreamAgreementAndRates.RecipientRatePairs.Single().Rate.PK);

			var wbpStreamAgreementAndRates = provider.ByStream["WBP"];
			AssertEquals("Should have returned XXX-XXX-XXX", agreementForWbpStream_XXX_XXX_XXX.PK, wbpStreamAgreementAndRates.CommissionAgreement.PK);
			AssertEquals(0, wbpStreamAgreementAndRates.RecipientRatePairs.Count);

			opportunity.P8_Status = "OPN";
			args = new EffectiveCommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(2003, 1, 1), ZString.Empty, ZString.Empty, ZString.Empty);
			provider = EffectiveCommissionAgreementProvider.New(Factory, args);
			AssertEquals("Should have still returned agreement even though opportunity is no longer effective", 2, provider.ByStream.Count);

			args = new EffectiveCommissionItemArgs(customer.PK, "XXX", "", "", new ZDate(2003, 1, 1), "SHP", "USLAX", ZString.Empty);
			provider = EffectiveCommissionAgreementProvider.New(Factory, args);
			AssertEquals(2, provider.ByStream.Count);

			var value = provider.ByStream[""];
			AssertEquals(agreementWithMode.PK, value.CommissionAgreement.PK);

			args = new EffectiveCommissionItemArgs(customer.PK, "XXX", "", "", new ZDate(2003, 1, 1), "SHP", ZString.Empty, "AUSYD");
			provider = EffectiveCommissionAgreementProvider.New(Factory, args);
			AssertEquals(2, provider.ByStream.Count);

			args = new EffectiveCommissionItemArgs(customer.PK, "XXX", "", "", new ZDate(2003, 1, 1), "SHP", "AUSYD", "");
			provider = EffectiveCommissionAgreementProvider.New(Factory, args);
			AssertEquals(2, provider.ByStream.Count);

			value = provider.ByStream[""];
			AssertEquals(agreementWithModeAndOrigin.PK, value.CommissionAgreement.PK);

			args = new EffectiveCommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(1998, 12, 31), "", "", "");
			provider = EffectiveCommissionAgreementProvider.New(Factory, args);
			AssertEquals(1, provider.ByStream.Count);

			value = provider.ByStream[""];
			AssertEquals(expiredAgreement_XXX_XXX_XXX.PK, value.CommissionAgreement.PK);

			args = new EffectiveCommissionItemArgs(customer.PK, "XXX", "XXX", "XXX", new ZDate(1999, 1, 1), "", "", "");
			provider = EffectiveCommissionAgreementProvider.New(Factory, args);
			AssertEquals(1, provider.ByStream.Count);

			value = provider.ByStream[""];
			AssertEquals(expiredAgreement_XXX_XXX_XXX.PK, value.CommissionAgreement.PK);
		}
	}
}
