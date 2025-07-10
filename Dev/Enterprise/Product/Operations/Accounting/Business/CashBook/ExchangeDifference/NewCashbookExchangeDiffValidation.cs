using System.Globalization;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.CashBook.ExchangeDifference
{
	public class NewCashbookExchangeDiffValidation : CashbookExchangeDiffValidation
	{
		public NewCashbookExchangeDiffValidation(NewCashbookExchangeDiff parent)
			: base(parent)
		{
			Parent = parent;
		}

		protected new NewCashbookExchangeDiff Parent;

		protected override void CheckAH_ExchangeRate()
		{
			base.CheckAH_ExchangeRate();
			var defaultNewExchangeRate = 0m;
			var modifyNewExchangeRateSecurity = Environment.Env.Security.ModifyCashBookBankCurrencyAdjustmentNewExchangeRate;
			if (!modifyNewExchangeRateSecurity.IsAllowed && Parent.AH_ExchangeRate != (defaultNewExchangeRate = Parent.GetNewExchangeRate()))
			{
				var defaultNewExchangeRateAsString = defaultNewExchangeRate.ToString(string.Format(CultureInfo.InvariantCulture, "F{0}", Parent.ExchangeRateDecimalPlaces), CultureInfo.InvariantCulture);
				Parent.AH_ExchangeRateInfo.AddError(Res.GetString("8b72f93f-c0a1-48e6-a810-39ee5f77d427", "You have not been granted security rights to {0}. Please set this to {1}.", modifyNewExchangeRateSecurity.DisplayTextPathToSecurityRight.ToString(), defaultNewExchangeRateAsString));
			}
			else if (Parent.AH_ExchangeRate.IsEmpty)
			{
				var exchangeRateType = AccountingConfigurationRegistry.Instance.BankCurrencyAdjustmentExchangeRateType.Value;
				var exchangeRateNotFoundMessage = exchangeRateType == Constants.ExchangeRateTypes.Code.PeriodEndRate ?
					Res.GetString("6b2f6e64-5f32-4a08-a04b-57857b71d5cd", "Period End Rate and BUY exchange rate not found with reference to post date.") :
					Res.GetString("3cdfb55c-d448-4750-b197-41a08b89441e", "{0} exchange rate not found with reference to post date.", exchangeRateType);
				Parent.AH_ExchangeRateInfo.AddError(exchangeRateNotFoundMessage);
			}
		}

		protected override void CheckAH_AB()
		{
			base.CheckAH_AB();
			if (!(Parent.BankAccount?.AB_IsActive ?? false))
			{
				Parent.AH_ABInfo.AddError(Res.GetString("a4b7999d-216f-48ee-8ac5-0b11712c8c49", @"This bank account cannot be chosen because it is inactive. Please un-select this bank account by unticked the ‘Include’ checkbox."));
			}
			else if (Parent.BankCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
			{
				Parent.AH_ABInfo.AddError(Res.GetString("cf6dad77-5531-4d61-83b1-b13265cc019a", @"This bank account cannot be chosen because it is a local currency bank account of which currency adjustment is not applicable. Please un-select this bank account by unticked the ‘Include’ checkbox."));
			}
		}
	}
}
