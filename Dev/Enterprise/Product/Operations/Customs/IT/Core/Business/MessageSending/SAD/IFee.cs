using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IFee
{
	ZString ChargeType { get; }
	ZDecimal BaseValue { get; }
	ZDecimal Rate { get; }
	ZString MethodOfCalculation { get; }
	ZDecimal Amount { get; }
	ZString MethodOfPayment { get; }
}
