using CargoWise.Types;

namespace Enterprise.Client.EDI.Billing.Business
{
	public interface IDiscountable
	{
		ZString SystemCode { get; }
		ZDecimal AmountToDiscount { get; }
	}
}
