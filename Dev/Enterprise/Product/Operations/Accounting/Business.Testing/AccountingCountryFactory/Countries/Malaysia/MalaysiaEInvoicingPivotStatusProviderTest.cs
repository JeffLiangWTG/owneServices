using System;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Utility.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.AccountingCountryFactory.Testing
{
	public class MalaysiaEInvoicingPivotStatusProviderTest : EInvoicingPivotStatusProviderTestBase<MalaysiaEInvoicingPivotStatusProvider>
	{
		protected override string CountryCode => CountryCodes.Malaysia;

		public void TestGetInitialPivotStatus_APLedger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var apInv = TestObjectCreator.CreateAPInvoice<APInvoice>("INV001", TestObjectCreator.AUD, 1.0m, 100m, 10m, 0m, 100m, 10m, 0m, TestObjectCreator.Debtor);

				AssertInitialPivotStatusEquals(apInv, string.Empty, "EInvoicing Functionality For Payable is enabled");

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionalityForPayables.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					AssertInitialPivotStatusEquals(apInv, EInvoicingPivotState.Discarded, "EInvoicing Functionality For Payable is disabled");
				}
			}
		}

		public void TestGetInitialPivotStatus_ARLedger()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, true))
			{
				var arInv = TestObjectCreator.CreateARInvoice<ARInvoice>("11111", TestObjectCreator.AUD, 1.0m, TestObjectCreator.AALSHI);

				AssertInitialPivotStatusEquals(arInv, string.Empty, "EInvoicing Functionality is enabled");

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, false))
				{
					AssertInitialPivotStatusEquals(arInv, EInvoicingPivotState.Discarded, "EInvoicing Functionality is disabled");
				}
			}
		}
	}
}
