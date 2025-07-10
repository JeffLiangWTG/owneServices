using System;

namespace Enterprise.Integration.Accounting
{
	public interface IOrgCreditLimitAndBalanceDetails
	{
		string OrgCode { get; }
		string CompanyCode { get; }
		bool OnCreditHold(string ledger);
		bool IsOverCreditLimit(string ledger);
		bool IsOverCreditTerms(string ledger);
		decimal CreditLimit(string ledger);
		decimal OutstandingBalance(string ledger);
		decimal Claim(string ledger);
		decimal OutstandingBalanceOverdue(string ledger);
		decimal OutstandingBalanceNotOverdue(string ledger);
		decimal UnpostedRevenueRecognised(string ledger);
		decimal UnpostedRevenueUnrecognised(string ledger);
		decimal UnpostedRevenue(string ledger);

		bool IsARGlobalCreditApproved { get; }
		bool OnARGlobalCreditHold { get; }
		bool IsOverARGlobalCreditLimit { get; }
		string ARGlobalCreditCurrencyCode { get; }
		decimal ARGlobalCreditLimit { get; }
		decimal ARGlobalOutstandingBalance { get; }
		decimal ARGlobalClaim { get; }
		bool UnableToCalculateARGlobalOutstandingBalance { get; }
		decimal ARGlobalUnpostedRevenueRecognised { get; }
		decimal ARGlobalUnpostedRevenueUnrecognised { get; }
		bool UnableToCalculateARGlobalUnpostedRevenue { get; }

		string GetErrorMessage(string errorContext, Exception exception);
		void FetchCreditLimitAndBalanceFromWebService(string ledger, params string[] options);
	}

	public interface IOrgCreditLimitAndBalanceDetailsProvider
	{
		IOrgCreditLimitAndBalanceDetails GetOrgCreditLimitAndBalanceDetails(string orgCode);
	}
}
