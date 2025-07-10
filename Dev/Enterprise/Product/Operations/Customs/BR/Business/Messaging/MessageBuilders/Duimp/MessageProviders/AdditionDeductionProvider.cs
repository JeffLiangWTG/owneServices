using CargoWise.Customs.BR.MessageContracts.Duimp.Outgoing;
using CargoWise.Types;

namespace Enterprise.Customs.BR.Business.Duimp
{
	public class AdditionDeductionProvider : IAdditionDeduction
	{
		AdditionDeductionProvider(ZString chargeType, ZString currencyCode, ZDecimal amount)
		{
			this.chargeType = chargeType;
			CurrencyCode = currencyCode;
			Amount = (double)amount;
		}
		readonly ZString chargeType;

		public static AdditionDeductionProvider New(ZString chargeType, ZString currencyCode, ZDecimal amount) =>
			amount.IsEmpty ? null : new AdditionDeductionProvider(chargeType, currencyCode, amount);

		public string Type => ImportChargesProvider.IsAdditions(chargeType) ? "ACRESCIMO" : "DEDUCAO";

		public int ChargeCode => ZInt.ParseSafe(ImportCustomsChargeTypeList.GetImportCustomsChargeTypeValue(chargeType), ZInt.Zero);

		public string CurrencyCode { get; private set; }

		public double Amount { get; private set; }
	}
}
