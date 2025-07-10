using CargoWise.Types;

namespace Enterprise.Customs.IE.Messaging
{
	public interface ITaxTypeProvider
	{
		ZString TaxType { get; }
		ZString Unit { get; }
		ZDecimal Quantity { get; }
		ZDecimal Amount { get; }
		ZDecimal TaxRate { get; }
		ZDecimal TaxAmount { get; }
		ZString MethodOfPayment { get; }
	}
}
