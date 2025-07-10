using CargoWise.EntityFramework;
using Enterprise.Customs.DE.Business.Declaration;

namespace Enterprise.Customs.DE.Business
{
	public class InvoiceLineCusLinkPackageCollection : Customs.Business.BaseCusLinkPackageCollection
	{
		public InvoiceLineCusLinkPackageCollection(JobComInvoiceLine invoiceLine)
			: base(invoiceLine)
		{
		}

		public new InvoiceLineCusLinkPackage this[int index]
		{
			get { return (InvoiceLineCusLinkPackage)base[index]; }
		}

		public new InvoiceLineCusLinkPackage AddNew()
		{
			return (InvoiceLineCusLinkPackage)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceLineCusLinkPackage((JobComInvoiceLine)Supporter);
		}
	}
}
