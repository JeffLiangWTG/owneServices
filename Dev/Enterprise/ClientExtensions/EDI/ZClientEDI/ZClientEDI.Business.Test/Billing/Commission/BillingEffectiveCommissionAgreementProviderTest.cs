using System.Collections.Generic;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Client.EDI.Billing.Business.Test
{
	public class BillingEffectiveCommissionAgreementProviderTest : TestCaseWithFactory
	{
		public void TestGetByStream()
		{
			var customer = Factory.NewWithValidTestData<OrgHeader>();

			var opportunity = OrgCommissionAgreementTestHelper.GetNewEffectiveOpportunity(Factory, customer);

			var agreement1 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement1.CA0_Name = "#1";
			agreement1.CA0_OH_Customer = customer.PK;
			agreement1.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement1.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement1.FillWithValidTestData();

			var agreement2 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement2.CA0_Name = "#2";
			agreement2.CA0_OH_Customer = customer.PK;
			agreement2.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement2.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement2.FillWithValidTestData();

			var agreement3 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement3.CA0_Name = "#3";
			agreement3.CA0_OH_Customer = customer.PK;
			agreement3.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement3.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement3.FillWithValidTestData();

			var agreement4 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement4.CA0_Name = "#4";
			agreement4.CA0_OH_Customer = customer.PK;
			agreement4.CA0_CommissionStream = "WBP";
			agreement4.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement4.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement4.FillWithValidTestData();

			var agreement5 = (EdiCommissionAgreement)opportunity.ApprovedCommissionAgreements.AddNew();
			agreement5.CA0_Name = "#5";
			agreement5.CA0_OH_Customer = customer.PK;
			agreement5.CA0_CommissionStream = "ABC";
			agreement5.CA0_CommissionTriggerType = OrgCommissionAgreementTriggerTypes.Codes.Manual;
			agreement5.CA0_EffectiveDate = new ZDate(1900, 1, 1);
			agreement5.FillWithValidTestData();

			ZGuid clientCompanyPk = ZGuid.NewZGuid();
			ZGuid licenceDatabasePk = ZGuid.NewZGuid();

			var responsibleAgreements = new Dictionary<(string Stream, ZGuid PivotPk), EdiCommissionAgreement>();
			responsibleAgreements[(string.Empty, clientCompanyPk)] = agreement1;
			responsibleAgreements[(string.Empty, licenceDatabasePk)] = agreement2;
			responsibleAgreements[(string.Empty, ZGuid.Empty)] = agreement3;
			responsibleAgreements[("WBP", clientCompanyPk)] = agreement4;
			responsibleAgreements[("ABC", licenceDatabasePk)] = agreement5;

			var provider = BillingEffectiveCommissionAgreementProvider.New(clientCompanyPk, licenceDatabasePk, ZDate.Today, responsibleAgreements);
			var result = provider.ByStream;
			AssertEquals("Number of Streams", 2, result.Keys.Count);
			Assert("Empty Stream mapped", result.ContainsKey(string.Empty));
			AssertEquals("Correct Agreement for empty stream and client company specified", agreement1.PK, result[string.Empty].CommissionAgreement.PK);
			Assert("WBP Stream mapped", result.ContainsKey("WBP"));
			AssertEquals("Correct Agreement for WBP stream and client company specified", agreement4.PK, result["WBP"].CommissionAgreement.PK);

			provider = BillingEffectiveCommissionAgreementProvider.New(ZGuid.Empty, licenceDatabasePk, ZDate.Today, responsibleAgreements);
			result = provider.ByStream;
			AssertEquals("Number of Streams", 2, result.Keys.Count);
			Assert("Empty Stream mapped", result.ContainsKey(string.Empty));
			AssertEquals("Correct Agreement for empty stream and client company not specified", agreement2.PK, result[string.Empty].CommissionAgreement.PK);
			Assert("ABC Stream mapped", result.ContainsKey("ABC"));
			AssertEquals("Correct Agreement for ABC stream and client company not specified", agreement5.PK, result["ABC"].CommissionAgreement.PK);

			provider = BillingEffectiveCommissionAgreementProvider.New(ZGuid.Empty, ZGuid.Empty, ZDate.Today, responsibleAgreements);
			result = provider.ByStream;
			AssertEquals("Number of Streams", 1, result.Keys.Count);
			Assert("Empty Stream mapped", result.ContainsKey(string.Empty));
			AssertEquals("Correct Agreement for empty stream neither pivots specified", agreement3.PK, result[string.Empty].CommissionAgreement.PK);
		}
	}
}
