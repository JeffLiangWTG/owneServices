using CargoWise.Types;
using Enterprise.Accounting.Integration;

namespace Enterprise.Accounting.Business.ARAP.PaymentApproval
{
	public interface IeNettPayment
	{
		ZString PaymentType { get; }
		ZString CurrencyCode { get; }
		eNettWebServiceResult PayWithCreditCardViaENett();
		void PayWithEnettDirectDebitFX();
	}
}
