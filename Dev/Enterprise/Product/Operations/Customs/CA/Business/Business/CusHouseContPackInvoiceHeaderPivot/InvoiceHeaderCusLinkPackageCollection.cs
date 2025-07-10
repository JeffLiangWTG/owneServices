using CargoWise.EntityFramework;

namespace Enterprise.Customs.CA.Business
{
	public class InvoiceHeaderCusLinkPackageCollection : Customs.Business.BaseCusLinkPackageCollection
	{
		public InvoiceHeaderCusLinkPackageCollection(JobComInvoiceHeader invoice)
			: base(invoice)
		{
		}

		public new InvoiceHeaderCusLinkPackage this[int index]
		{
			get { return (InvoiceHeaderCusLinkPackage)base[index]; }
		}

		public new InvoiceHeaderCusLinkPackage AddNew()
		{
			return (InvoiceHeaderCusLinkPackage)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceHeaderCusLinkPackage((JobComInvoiceHeader)Supporter);
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
