using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

public interface IUcc6A93NumberPayment
{
	IUcc6A93NumberAmountCalculator AmountCalculator { get; }

	ZString PaymentType { get; }

	ZDateTime PaymentDate { get; }

	ZString PaymentResponseNo { get; }
}
