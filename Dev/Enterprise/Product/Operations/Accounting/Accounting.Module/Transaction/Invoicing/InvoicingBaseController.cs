using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.Module
{
	public abstract partial class InvoicingBaseController : AccountingTransactionController, IBusinessEntityFactorySettings
	{
		public IZForm ShowImportedDataForm(InvoicingBase invoice)
		{
			try
			{
				IZForm formToReturn = ShowFormForNewEntity(invoice);

				if (formToReturn != null)
				{
					formToReturn.DisplayMode = ZArchitecture.Core.ODisplayMode.Edit;
					invoice.IsImportedFromFile = true;
					return formToReturn;
				}
				else
				{
					invoice.ReleaseAllMutexOnInvoice();
					return null;
				}
			}
			catch (SecurityAccessDeniedException)
			{
				invoice.ReleaseAllMutexOnInvoice();
				throw;
			}
		}

		public override IZForm ShowNewForm()
		{
			InvoicingBase invoicingBase = (InvoicingBase)GetNewBusinessEntityInLocalFactory();
			invoicingBase.SubmittedFromInvoicingForm = true;
			return ShowFormForNewEntity(invoicingBase);
		}

		bool IBusinessEntityFactorySettings.ShouldUseSourceEntityFactory { get; set; }
	}
}
