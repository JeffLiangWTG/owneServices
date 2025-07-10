#if DEBUG

namespace Enterprise.Accounting.DataTransfer.Integration
{
	public partial class OrgCreditLimitAndBalanceDetails
	{
		public void GetCreditLimitAndBalanceFromWebService_ForTestOnly(string ledger, string balanceOverdueAgingOption)
		{
			var detailsFieldStrategies = GenerateDetailsFieldStrategies();
			GetCreditLimitAndBalanceFromWebService(ledger, balanceOverdueAgingOption, detailsFieldStrategies);
		}
	}
}

#endif
