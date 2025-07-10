using CargoWise.Application;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Presentation.GUI
{
	public interface IInvoiceFormPresentationProvider
	{
		ZBool GetIsReversalStatusCodeVisible(InvoicingBase invoicing);
		ZBool IsSupplyTypeColumnVisible();
		ZBool IsTaxBranchColumnVisible();
		bool IsEInvoicingColumnsAvailable(ZString ledgerType);
	}

	class InvoiceFormPresentationProvider : IInvoiceFormPresentationProvider
	{
		ZBool IInvoiceFormPresentationProvider.GetIsReversalStatusCodeVisible(InvoicingBase invoicing)
		{
			var isReversal = invoicing?.IsReversalTransaction ?? false;

			if (isReversal)
			{
				return invoicing.IsReversalStatusCodeAllowed;
			}

			return false;
		}

		ZBool IInvoiceFormPresentationProvider.IsSupplyTypeColumnVisible()
		{
			return AccountingMasterFilesRegistry.Instance.EnableSupplyTypeClassificationCodes.Value;
		}

		ZBool IInvoiceFormPresentationProvider.IsTaxBranchColumnVisible()
		{
			return AccountingMasterFilesRegistry.Instance.EnableTaxBranchReporting.Value;
		}

		bool IInvoiceFormPresentationProvider.IsEInvoicingColumnsAvailable(ZString ledgerType)
		{
			return ObjectFactory.Get<IElectronicInvoicingAccountingObjectFactory>().GetEInvoicingConfigurationChecks().IsEInvoicingSupportedForCurrentCountry(ledgerType);
		}
	}
}
