using CargoWise.Common;
using CargoWise.Types;

namespace Enterprise.Customs.IT.Business;

sealed class Ucc6A93NumberPayment : IUcc6A93NumberPayment
{
	public Ucc6A93NumberPayment(ZString paymentType, ZDateTime paymentDate, ZString paymentResponseNo, IFee[] fees)
	{
		Argument.NotNull(fees, nameof(fees));

		PaymentType = Argument.NotNullOrEmpty(paymentType, nameof(paymentType));
		PaymentDate = paymentDate;
		PaymentResponseNo = Argument.NotNullOrEmpty(paymentResponseNo, nameof(paymentResponseNo));
		AmountCalculator = new Ucc6A93NumberAmountCalculator(fees);
	}

	public IUcc6A93NumberAmountCalculator AmountCalculator { get; }
	public ZString PaymentType { get; }
	public ZDateTime PaymentDate { get; }
	public ZString PaymentResponseNo { get; }
}
