using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.AccountingCountryFactory
{
	class KoreaSouthAccountingCountryFactory : IAccountingCountryFactory,
		IInstanceProvider<IReverseDateValidation>,
		IInstanceProvider<IInvoiceDateValidation>,
		IInstanceProvider<ITransactionLinesValidation>,
		IInstanceProvider<IEInvoicingActionProvider>,
		IInstanceProvider<IEInvoicingPreEligibilityProvider>,
		IInstanceProvider<IAmendStatusCodeProvider>,
		IInstanceProvider<IAmendStatusCodeValidationProvider>,
		IInstanceProvider<IEInvoicingPivotStatusProvider>
	{
		IReverseDateValidation IInstanceProvider<IReverseDateValidation>.Get() => new KoreaSouthEInvoicingValidation();

		IInvoiceDateValidation IInstanceProvider<IInvoiceDateValidation>.Get() => new KoreaSouthEInvoicingValidation();

		ITransactionLinesValidation IInstanceProvider<ITransactionLinesValidation>.Get() => new KoreaSouthEInvoicingValidation();

		IEInvoicingActionProvider IInstanceProvider<IEInvoicingActionProvider>.Get() => new KoreaSouthEInvoicingActionProvider();

		IEInvoicingPreEligibilityProvider IInstanceProvider<IEInvoicingPreEligibilityProvider>.Get() => new KoreaSouthEInvoicingPreEligibilityProvider();

		IAmendStatusCodeProvider IInstanceProvider<IAmendStatusCodeProvider>.Get() => new KoreaSouthAmendStatusCodeProvider();

		IAmendStatusCodeValidationProvider IInstanceProvider<IAmendStatusCodeValidationProvider>.Get() => new KoreaSouthAmendStatusCodeValidationProvider();

		IEInvoicingPivotStatusProvider IInstanceProvider<IEInvoicingPivotStatusProvider>.Get() => new KoreaSouthEInvoicingPivotStatusProvider();
	}
}
