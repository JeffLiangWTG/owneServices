using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.IT.Messaging.SAD;

namespace Enterprise.Customs.IT.Business;

public class SADDutyTaxFeeWrapper : IDutyTaxFee
{
	public SADDutyTaxFeeWrapper(IFee fee)
	{
		this.fee = Argument.NotNull(fee, nameof(fee));
	}
	readonly IFee fee;

	public ZString Type
	{
		get
		{
			var chargeType = fee.ChargeType;

			return chargeType == EU.Business.UniversalReferenceConstants.RefCusRateCodes.Vat
				? UniversalReferenceConstants.RefCusRateCodes.ItalianCustomsVatCode
				: chargeType;
		}
	}

	public ZDecimal Base => fee.BaseValue;
	public ZString CalculationFactor1 => SadCalculationFactor;
	public ZDecimal? Rate1 => fee.Rate;
	public ZString CalculationFactor2 => GetCalculationFactor();
	public ZDecimal? Rate2 => null;
	public ZString CalculationFactor3 => ZString.Empty;
	public ZDecimal? Rate3 => null;
	public ZString CalculationFactor4 => ZString.Empty;
	public ZDecimal Amount => fee.Amount;
	public ZString MethodOfPayment => fee.MethodOfPayment;

	#region Implementation

	const string SadCalculationFactor = "X";
	const string PercentageCalculationFactor = "%";

	ZString GetCalculationFactor()
	{
		var methodOfCalculation = fee.MethodOfCalculation;
		return methodOfCalculation == PercentageCalculationFactor ? methodOfCalculation : ZString.Empty;
	}

	#endregion
}
