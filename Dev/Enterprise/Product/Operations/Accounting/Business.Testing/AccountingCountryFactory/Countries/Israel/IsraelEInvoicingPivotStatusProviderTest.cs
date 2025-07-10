using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class IsraelEInvoicingPivotStatusProviderTest : EInvoicingPivotStatusProviderTestBase<IsraelEInvoicingPivotStatusProvider>
	{
		protected override string CountryCode => CountryCodes.Israel;

		public void TestGetInitialPivotStatus()
		{
			AssertInitialPivotStatusEquals(null, "", "Null transaction should give us empty value.");

			AssertTransactions();
		}

		public void TestGetInitialPivotStatus_OverriddenStatusRegistriesDoesNotAffectResult()
		{
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatus.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
			using (AccountingMasterFilesRegistry.Instance.EReportingSubmitPivotDefaultStatusForPayables.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, EInvoicingPivotState.Pending))
			{
				AssertTransactions();
			}
		}

		void AssertTransactions()
		{
			AssertInitialPivotStatusEquals(Factory.NewWithValidTestData<APInvoice>(), EInvoicingPivotState.Queued, "Payable invoice pivot should get QUE status.");
			AssertInitialPivotStatusEquals(Factory.NewWithValidTestData<APCreditNote>(), EInvoicingPivotState.Queued, "Payable credit note pivot should get QUE status.");
			AssertInitialPivotStatusEquals(Factory.NewWithValidTestData<APAdjustmentNote>(), EInvoicingPivotState.Queued, "Payable adjustment note pivot should get QUE status.");

			AssertInitialPivotStatusEquals(Factory.NewWithValidTestData<ARInvoice>(), EInvoicingPivotState.Queued, "Receivable invoice pivot should get QUE status.");
			AssertInitialPivotStatusEquals(Factory.NewWithValidTestData<ARCreditNote>(), EInvoicingPivotState.Queued, "Receivable credit note pivot should get QUE status.");
			AssertInitialPivotStatusEquals(Factory.NewWithValidTestData<ARAdjustmentNote>(), EInvoicingPivotState.Queued, "Receivable adjustment note pivot should get QUE status.");
		}
	}
}
