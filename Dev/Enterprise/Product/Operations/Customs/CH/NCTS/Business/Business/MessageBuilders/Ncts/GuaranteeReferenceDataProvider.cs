using CargoWise.Customs.CH.MessageContracts.Passar.Outgoing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.CH.NCTS.Business;

public class GuaranteeReferenceDataProvider : IGuaranteeReference
{
	public static GuaranteeReferenceDataProvider New(NctsGuarantee guarantee, int sequenceNumber) => guarantee != null ? new GuaranteeReferenceDataProvider(guarantee, sequenceNumber) : null;

	GuaranteeReferenceDataProvider(NctsGuarantee guarantee, int sequenceNumber)
	{
		this.guarantee = guarantee;
		this.SequenceNumber = sequenceNumber;
	}
	readonly NctsGuarantee guarantee;

	public int SequenceNumber { get; }

	public decimal AmountToBeCovered => amountToBeCovered ??= GetAmountToBeCovered();
	decimal? amountToBeCovered;

	decimal GetAmountToBeCovered()
	{
		var amount = ZDecimal.Zero;
		if (guarantee.PW_RX_NKCurrency == CurrencyCodes.Switzerland)
		{
			amount = guarantee.PW_BondAmount;
		}
		else
		{
			var exchangeRate = new RefExchangeRate.Loader(guarantee.Factory).GetEffectiveRateOn(ZDate.Today, guarantee.PW_RX_NKCurrency, ExchangeRateTypes.Code.CustomsRate, GlbCompany.CurrentCompany.PK);
			if (exchangeRate != null )
			{
				amount = guarantee.PW_BondAmount * exchangeRate.RE_SellRate;
			}
		}
		return amount.Round(2).Normalize();
	}

	public string GRN => guarantee.PW_BondNumber;

	public string AccessCode => guarantee.PW_Password;
}
