using CargoWise.EntityFramework;

namespace Enterprise.Customs.FR.Business.Declaration
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
