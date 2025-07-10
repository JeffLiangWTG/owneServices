using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	public class KoreaSouthEInvoicingPenaltyTaxDetailCollection : NonPersistentBusinessObjectCollection<KoreaSouthEInvoicingPenaltyTaxDetail>
	{
		public override bool ReadOnly
		{
			get { return true; }
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new KoreaSouthEInvoicingPenaltyTaxDetail();
		}
	}
}
