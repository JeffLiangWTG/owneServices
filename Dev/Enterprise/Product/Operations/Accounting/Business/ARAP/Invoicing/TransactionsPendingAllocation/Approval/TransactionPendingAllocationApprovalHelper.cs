using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.Accounting.Business.AccountingCountryFactory;
using Enterprise.Accounting.Business.TransactionApproval;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TransactionPendingAllocationApprovalHelper : TransactionApprovalHelper<TransactionPendingAllocationApprovalRequest, TransactionPendingAllocationApprovalDetails>
	{
		public TransactionPendingAllocationApprovalHelper(TransactionPendingAllocationApprovalRequest approvingRequest)
			: base(approvingRequest, (x, y) => new TransactionPendingAllocationApprovalRequestCollection(x, y))
		{
		}

		public static ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider GetITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider(BusinessObjectFactory factory, string countryCode)
			=> factory.GetCachedValue("ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider" + countryCode, 
				() => AccountingMasterFilesRegistry.Instance.EnableRejectionEInvoicingRequestInTransactionPendingAllocationApproval.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty)
				? (ObjectFactory.Get<IGlobalAccountingCountryFactory>().GetCountryFactory(countryCode) as IInstanceProvider<ITransactionPendingAllocationApprovalEInvoicingRequestsFunctionalityProvider>)?.Get()
				: null);
	}
}
