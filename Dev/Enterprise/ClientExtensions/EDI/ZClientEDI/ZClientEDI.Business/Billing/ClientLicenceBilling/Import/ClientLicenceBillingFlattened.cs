using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class ClientLicenceBillingFlattened : AutoClientLicenceBillingFlattened
	{
		public override ZDecimal CurrentPrepaymentBalance
		{
			get => base.CurrentPrepaymentBalance;
			set
			{
				base.CurrentPrepaymentBalance = value;
				IsCurrentPrepaymentBalanceProvided = true;
			}
		}

		public override ZString CurrentPrepaymentCurrency
		{
			get => base.CurrentPrepaymentCurrency;
			set
			{
				base.CurrentPrepaymentCurrency = value;
				IsCurrentPrepaymentCurrencyProvided = true;
			}
		}

		public override ZDecimal FuturePrepaymentBalance
		{
			get => base.FuturePrepaymentBalance;
			set
			{
				base.FuturePrepaymentBalance = value;
				IsFuturePrepaymentBalanceProvided = true;
			}
		}

		public override ZString FuturePrepaymentCurrency
		{
			get => base.FuturePrepaymentCurrency;
			set
			{
				base.FuturePrepaymentCurrency = value;
				IsFuturePrepaymentCurrencyProvided = true;
			}
		}

		public bool IsCurrentPrepaymentBalanceProvided { get; private set; }
		public bool IsCurrentPrepaymentCurrencyProvided { get; private set; }
		public bool IsFuturePrepaymentBalanceProvided { get; private set; }
		public bool IsFuturePrepaymentCurrencyProvided { get; private set; }
	}
}


