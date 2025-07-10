using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
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

		protected override void OnAdded(BusinessObject bizOAdded)
		{
			base.OnAdded(bizOAdded);
			PackagesLimitHelper.HookCheckPackUnitCountEvent((Customs.Business.BaseCusLinkPackage)bizOAdded, Supporter, this);
		}

		protected override void ActionAfterRebuilt()
		{
			base.ActionAfterRebuilt();
			if (!IsValidationSuspended)
			{
				PackagesLimitHelper.CheckPackageUnitCount(Supporter, this);
			}
		}
	}
}
