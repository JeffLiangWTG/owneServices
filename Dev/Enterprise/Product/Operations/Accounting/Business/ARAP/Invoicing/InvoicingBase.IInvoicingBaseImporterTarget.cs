using CargoWise.EntityFramework;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	partial class InvoicingBase : IInvoicingBaseImporterTarget
	{
		public IInvoicingBaseImporterTarget AsInvoicingBaseImporterTarget => this;

		BusinessObjectFactory IInvoicingBaseImporterTarget.Factory => this.Factory;

		APInvoiceConsolCosting IInvoicingBaseImporterTarget.ConsolCosting => this.ConsolCosting;

		void IInvoicingBaseImporterTarget.ImportAllApportionmentsFromCosting_SuspendListChanged()
		{
			try
			{
				using (((ISingleElementListInternal)this).SuspendListChanged())
				{
					ImportAllApportionmentsFromCosting();

					if (!IsValidationSuspended)
					{
						Lines.RunPreSaveValidation();
					}
				}
			}
			finally
			{
				OnElementReset();
			}
		}
	}
}