using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class KoreaSouthEInvoicingPenaltyTaxDetail : NonPersistentBusinessObject
	{
		public KoreaSouthEInvoicingPenaltyTaxDetail()
		{
		}

		public KoreaSouthEInvoicingPenaltyTaxDetail(ZString type, ZString explanation, ZString supplier, ZString receiver)
		{
			Type = type;
			Explanation = explanation;
			Supplier = supplier;
			Receiver = receiver;
		}

		public ZString Type { get; }
		public ZString Explanation { get; }
		public ZString Supplier { get; }
		public ZString Receiver { get; }
	}
}
