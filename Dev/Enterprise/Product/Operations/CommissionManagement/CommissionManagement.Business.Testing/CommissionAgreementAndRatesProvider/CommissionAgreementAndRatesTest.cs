using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.CommissionManagement.Business.Testing
{
	internal class CommissionAgreementAndRatesTest : TestCaseWithFactory
	{
		public void TestRecipientRatePairs()
		{
			var scwStaff = Factory.NewWithValidTestData<GlbStaff>();
			scwStaff.GS_Code = "SCW";
			var adlStaff = Factory.NewWithValidTestData<GlbStaff>();
			adlStaff.GS_Code = "ADL";

			var agreement = Factory.NewWithValidTestData<OrgCommissionAgreement>();
			var scwRecipient = agreement.Recipients.AddNew();
			scwRecipient.CAR_GS_NKStaff = "SCW";
			var adlRecipient = agreement.Recipients.AddNew();
			adlRecipient.CAR_GS_NKStaff = "ADL";

			var scwRecipientRate_2001_2002 = scwRecipient.Rates.AddNew();
			scwRecipientRate_2001_2002.CAT_CommissionPeriod = "";
			scwRecipientRate_2001_2002.CAT_CommissionStartDate = new ZDate(2001, 1, 1);
			scwRecipientRate_2001_2002.CAT_CommissionEndDate = new ZDate(2001, 12, 31);

			var scwRecipientRate_2002_2003 = scwRecipient.Rates.AddNew();
			scwRecipientRate_2002_2003.CAT_CommissionPeriod = "";
			scwRecipientRate_2002_2003.CAT_CommissionStartDate = new ZDate(2002, 1, 1);
			scwRecipientRate_2002_2003.CAT_CommissionEndDate = new ZDate(2002, 12, 31);

			var scwRecipientRate_2003_2004 = scwRecipient.Rates.AddNew();
			scwRecipientRate_2003_2004.CAT_CommissionPeriod = "";
			scwRecipientRate_2003_2004.CAT_CommissionStartDate = new ZDate(2003, 1, 1);
			scwRecipientRate_2003_2004.CAT_CommissionEndDate = new ZDate(2003, 12, 31);

			var adlRecipientRate_2001_2003 = adlRecipient.Rates.AddNew();
			adlRecipientRate_2001_2003.CAT_CommissionPeriod = "";
			adlRecipientRate_2001_2003.CAT_CommissionStartDate = new ZDate(2001, 1, 1);
			adlRecipientRate_2001_2003.CAT_CommissionEndDate = new ZDate(2002, 12, 31);

			var adlRecipientRate_2002_2003 = adlRecipient.Rates.AddNew();
			adlRecipientRate_2002_2003.CAT_CommissionPeriod = "";
			adlRecipientRate_2002_2003.CAT_CommissionStartDate = new ZDate(2002, 1, 1);
			adlRecipientRate_2002_2003.CAT_CommissionEndDate = new ZDate(2002, 12, 31);

			var adlRecipientRate_2000_2004 = adlRecipient.Rates.AddNew();
			adlRecipientRate_2000_2004.CAT_CommissionPeriod = "";
			adlRecipientRate_2000_2004.CAT_CommissionStartDate = new ZDate(2000, 1, 1);
			adlRecipientRate_2000_2004.CAT_CommissionEndDate = new ZDate(2003, 12, 31);

			var agreementAndRatesProvider = new CommissionAgreementAndRates(agreement, new ZDate(2002, 6, 1));
			AssertContainsExactElementsInAnyOrder(
				new[]
				{
					new KeyValuePair<OrgCommissionAgreementRecipient, OrgCommissionAgreementRecipientRate>(scwRecipient, scwRecipientRate_2002_2003),
					new KeyValuePair<OrgCommissionAgreementRecipient, OrgCommissionAgreementRecipientRate>(adlRecipient, adlRecipientRate_2002_2003)	// there are multiple rates that cover this date for adl, but 2002_2003 is the latest one
				},
				agreementAndRatesProvider.RecipientRatePairs.Select(x => new KeyValuePair<OrgCommissionAgreementRecipient, OrgCommissionAgreementRecipientRate>(x.Recipient, x.Rate)));

			adlRecipient.CAR_EndDate = new ZDate(2002, 1, 1);
			agreementAndRatesProvider = new CommissionAgreementAndRates(agreement, new ZDate(2002, 6, 1));
			AssertContainsExactElementsInAnyOrder("Should no longer include adl recipient rate",
				new[]
				{
					new KeyValuePair<OrgCommissionAgreementRecipient, OrgCommissionAgreementRecipientRate>(scwRecipient, scwRecipientRate_2002_2003)
				},
				agreementAndRatesProvider.RecipientRatePairs.Select(x => new KeyValuePair<OrgCommissionAgreementRecipient, OrgCommissionAgreementRecipientRate>(x.Recipient, x.Rate)));
		}
	}

	public class CommissionAgreementAndRatesForTesting : ICommissionAgreementAndRates
	{
		public OrgCommissionAgreement CommissionAgreement
		{
			get;
			set;
		}

		public IReadOnlyCollection<RecipientRatePair> RecipientRatePairs
		{
			get;
			set;
		}

		public IReadOnlyCollection<RecipientRatePair> GetRecipientRatePairs(ZGuid chargeCodePK)
		{
			return RecipientRatePairs;
		}
	}
}
