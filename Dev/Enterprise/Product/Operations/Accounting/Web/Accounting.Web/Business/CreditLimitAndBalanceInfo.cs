using System;

namespace Enterprise.Accounting.Web.Business
{
	[Serializable]
	public class CreditLimitAndBalanceInfo : CreditLimitAndBalanceRequest
	{
		public bool OnCreditHold { get; set; }
		public bool OnCreditHoldHasValue { get; set; }
		public bool IsOverCreditLimit { get; set; }
		public bool IsOverCreditLimitHasValue { get; set; }
		public bool IsOverCreditTerms { get; set; }
		public bool IsOverCreditTermsHasValue { get; set; }
		public string CurrencyCode { get; set; }
		public string SettlementGroupLegacySystemCode { get; set; }
		public string SettlementGroupOrgCode { get; set; }
		public string SettlementGroupExternalDebtorCode { get; set; }
		public string SettlementGroupExternalCreditorCode { get; set; }
		public decimal CreditLimit { get; set; }
		public bool CreditLimitHasValue { get; set; }
		public decimal AccountBalanceTotal { get; set; }
		public bool AccountBalanceTotalHasValue { get; set; }
		public decimal ClaimTotal { get; set; }
		public bool ClaimTotalHasValue { get; set; }
		public decimal AccountBalanceNotOverdue { get; set; }
		public bool AccountBalanceNotOverdueHasValue { get; set; }
		public decimal OverdueTotal { get; set; }
		public bool OverdueTotalHasValue { get; set; }
		public decimal AccountBalanceOverdueLessThanOnePeriod { get; set; }
		public bool AccountBalanceOverdueLessThanOnePeriodHasValue { get; set; }
		public decimal AccountBalanceOverdueOnePeriodAndOver { get; set; }
		public bool AccountBalanceOverdueOnePeriodAndOverHasValue { get; set; }
		public decimal AccountBalanceOverdueTwoPeriodsAndOver { get; set; }
		public bool AccountBalanceOverdueTwoPeriodsAndOverHasValue { get; set; }
		public decimal AccountBalanceOverdueThreePeriodsAndOver { get; set; }
		public bool AccountBalanceOverdueThreePeriodsAndOverHasValue { get; set; }
		public decimal UnpostedRevenueRecognisedTotal { get; set; }
		public bool UnpostedRevenueRecognisedTotalHasValue { get; set; }
		public decimal UnpostedRevenueUnrecognisedTotal { get; set; }
		public bool UnpostedRevenueUnrecognisedTotalHasValue { get; set; }
		public string LegacySystemCode { get; set; }
		public string ExternalDebtorCode { get; set; }
		public string ExternalCreditorCode { get; set; }

		public bool IsGlobalCreditApproved { get; set; }
		public bool IsGlobalCreditApprovedHasValue { get; set; }
		public bool OnGlobalCreditHold { get; set; }
		public bool OnGlobalCreditHoldHasValue { get; set; }
		public bool IsOverGlobalCreditLimit { get; set; }
		public bool IsOverGlobalCreditLimitHasValue { get; set; }
		public string GlobalCreditCurrencyCode { get; set; }
		public string GlobalCreditGroupOrgCode { get; set; }
		public decimal GlobalCreditLimit { get; set; }
		public bool GlobalCreditLimitHasValue { get; set; }

		public decimal GlobalBalanceTotal { get; set; }
		public bool GlobalBalanceTotalHasValue { get; set; }
		public decimal GlobalClaimTotal { get; set; }
		public bool GlobalClaimTotalHasValue { get; set; }
		public decimal GlobalUnpostedRevenueRecognisedTotal { get; set; }
		public bool GlobalUnpostedRevenueRecognisedTotalHasValue { get; set; }
		public decimal GlobalUnpostedRevenueUnrecognisedTotal { get; set; }
		public bool GlobalUnpostedRevenueUnrecognisedTotalHasValue { get; set; }

		public bool InvalidGlobalCreditCurrencyOrMissingExRate { get; set; }
		public bool InvalidGlobalCreditCurrencyOrMissingExRateHasValue { get; set; }
	}
}
