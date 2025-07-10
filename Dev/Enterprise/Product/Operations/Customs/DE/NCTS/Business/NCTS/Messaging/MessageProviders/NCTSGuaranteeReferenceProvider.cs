using CargoWise.Common;
using CargoWise.Customs.DE.MessageContracts.NCTS;
using CargoWise.Types;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.DE.NCTS.Business
{
	public class NCTSGuaranteeReferenceProvider : INCTSGuaranteeReference
	{
		public NCTSGuaranteeReferenceProvider(NctsGuarantee guarantee)
		{
			this.guarantee = Argument.NotNull(guarantee, nameof(guarantee));
		}

		public string GRN => guarantee.PW_BondNumber.ValueOrNullIfEmpty();

		public string AccessCode => guarantee.PW_Password.ValueOrNullIfEmpty();

		public decimal Amount
		{
			get
			{
				decimal result;
				var currency = guarantee.PW_RX_NKCurrency;
				var bondAmount = guarantee.PW_BondAmount;
				if (currency.IsEmpty || currency == Core.Constants.CurrencyCodes.EuropeanUnion)
				{
					result = bondAmount.FormatDecimal(2);
				}
				else
				{
					var exchangeRate = NCTSProviderHelpers.GetEffectiveExchangeRate(currency, guarantee.Factory);
					result = exchangeRate != null ? new ZDecimal(bondAmount / exchangeRate.RE_SellRate).FormatDecimal(2) : ZDecimal.Zero;
				}
				return result;
			}
		}

		public string Currency => Core.Constants.CurrencyCodes.EuropeanUnion;

		readonly NctsGuarantee guarantee;
	}
}
