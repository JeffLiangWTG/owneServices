using CargoWise.Types;

namespace Enterprise.Customs.GB.CDS.Messaging
{
	public interface IPayment
	{
		ZString MethodCode { get; }
		IAmountAndCurrency PaymentAmount { get; }
		IAmountAndCurrency TaxAssessedAmount { get; }
	}

	class PaymentWrapper : IPayment
	{
		PaymentWrapper(ZString methodCode, IAmountAndCurrency paymentAmount, IAmountAndCurrency taxAssessedAmount)
		{
			this.methodCode = methodCode;
			this.paymentAmount = paymentAmount;
			this.taxAssessedAmount = taxAssessedAmount;
		}

		public static PaymentWrapper New(ZString methodCode, IAmountAndCurrency paymentAmount, IAmountAndCurrency taxAssessedAmount)
		{
			return new PaymentWrapper(methodCode, paymentAmount, taxAssessedAmount);
		}

		ZString IPayment.MethodCode => methodCode;

		IAmountAndCurrency IPayment.PaymentAmount => paymentAmount;

		IAmountAndCurrency IPayment.TaxAssessedAmount => taxAssessedAmount;

		readonly ZString methodCode;
		readonly IAmountAndCurrency paymentAmount;
		readonly IAmountAndCurrency taxAssessedAmount;
	}
}
