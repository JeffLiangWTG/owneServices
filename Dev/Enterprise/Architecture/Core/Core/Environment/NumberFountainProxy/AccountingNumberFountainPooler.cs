namespace Enterprise.ZArchitecture.Environment
{
	public static class AccountingNumberFountainPoolerExtensions
	{
		public static AccountingNumberFountainPooler Wrap(this NumberFountain.AccountingNumberFountainPooler inner)
		{
			return new AccountingNumberFountainPooler(inner);
		}
	}

	public class AccountingNumberFountainPooler
	{
		internal AccountingNumberFountainPooler(NumberFountain.AccountingNumberFountainPooler inner)
		{
			this.inner = inner;
		}

		public AccountingNumberFountainPooler(string fountainName, string prefix, long minValue)
		{
			inner = new NumberFountain.AccountingNumberFountainPooler(fountainName, prefix, minValue);
		}

		readonly NumberFountain.AccountingNumberFountainPooler inner;

		public INumberFountainProxy GetPeriodFountain(string voucherPeriod)
		{
			var company = EnvProxy.Instance.CurrentCompany;
			return inner.GetPeriodFountain(voucherPeriod, company.Code, company.PK).Wrap();
		}

		public INumberFountainProxy GetPeriodFountain(string voucherPeriod, int transactionNumDigits)
		{
			var company = EnvProxy.Instance.CurrentCompany;
			return inner.GetPeriodFountain(voucherPeriod, transactionNumDigits, company.Code, company.PK).Wrap();
		}

		public INumberFountainProxy GetTodaysPeriodFountain()
		{
			var company = EnvProxy.Instance.CurrentCompany;
			return inner.GetTodaysPeriodFountain(company.Code, company.PK).Wrap();
		}

		public INumberFountainProxy GetTodaysPeriodFountain(int transactionNumDigits)
		{
			var company = EnvProxy.Instance.CurrentCompany;
			return inner.GetTodaysPeriodFountain(transactionNumDigits, company.Code, company.PK).Wrap();
		}
	}
}
