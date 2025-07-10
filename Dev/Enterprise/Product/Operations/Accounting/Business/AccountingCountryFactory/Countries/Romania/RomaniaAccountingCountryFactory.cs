using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class RomaniaAccountingCountryFactory :
		IAccountingCountryFactory,
		IInstanceProvider<IEInvoicingEligibilityDecider>,
		IInstanceProvider<IBatchQueueInvoicesForEInvoicingProvider>,
		IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>,
		IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>,
		IInstanceProvider<ISupportResetStatusToDelivered>,
		IInstanceProvider<IEInvoicingAuthorizationBehaviourProvider>
	{
		IEInvoicingEligibilityDecider IInstanceProvider<IEInvoicingEligibilityDecider>.Get() => new RomaniaEInvoicingEligibilityDecider();

		IBatchQueueInvoicesForEInvoicingProvider IInstanceProvider<IBatchQueueInvoicesForEInvoicingProvider>.Get() => new BatchQueueInvoicesForEInvoicingProvider();

		IEInvoicingPivotsToRequeueFilterProvider IInstanceProvider<IEInvoicingPivotsToRequeueFilterProvider>.Get() => new RomaniaEInvoicingPivotsToRequeueInfo();

		IEInvoicingRequeuePivotsStatusProvider IInstanceProvider<IEInvoicingRequeuePivotsStatusProvider>.Get() => new RomaniaEInvoicingRequeuePivotsStatusProvider();

		ISupportResetStatusToDelivered IInstanceProvider<ISupportResetStatusToDelivered>.Get() => new RomaniaSupportResetStatusToDeliveredProvider();

		IEInvoicingAuthorizationBehaviourProvider IInstanceProvider<IEInvoicingAuthorizationBehaviourProvider>.Get() => new RomaniaEInvoicingAuthorizationBehaviourProvider();
	}
}
