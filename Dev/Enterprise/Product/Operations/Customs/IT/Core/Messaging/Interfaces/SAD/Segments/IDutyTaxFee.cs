using CargoWise.Types;

namespace Enterprise.Customs.IT.Messaging.SAD;

public interface IDutyTaxFee
{
	ZString Type { get; }
	ZDecimal Base { get; }
	ZString CalculationFactor1 { get; }
	ZDecimal? Rate1 { get; }
	ZString CalculationFactor2 { get; }
	ZDecimal? Rate2 { get; }
	ZString CalculationFactor3 { get; }
	ZDecimal? Rate3 { get; }
	ZString CalculationFactor4 { get; }
	ZDecimal Amount { get; }
	ZString MethodOfPayment { get; }
}
