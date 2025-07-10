using CargoWise.Application;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.CountryCompliance.Interfaces;

namespace Enterprise.Accounting.Business.AccountingPresentationProviders
{
	public interface ITransactionReasonFormPresentationProvider
	{
		bool IsAmendInFullVisible(InvoicingBase invoicingBase);
	}

	public class TransactionReasonFormPresentationProvider : ITransactionReasonFormPresentationProvider
	{
		public bool IsAmendInFullVisible(InvoicingBase invoicingBase)
		{
			if (invoicingBase == null || !invoicingBase.IsAmendingWithARCreditNote)
			{
				return false;
			}

			var provider = (ObjectFactory.Get<IAccountingCountryComplianceGlobalFactory>().GetFeatureInterface<ITransactionReasonFormProvider>(invoicingBase.Company.GC_RN_NKCountryCode));
			return provider?.ShouldShowAmendInFull() ?? false;
		}
	}
}
