using System;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.Accounting.DataTransfer.Testing
{
	//proper test class should be inherited from BaseAccountingDataAdapterTest<TransactionHeader, Xsd.FinancialInvoices>, but it requires a lot of setup
	class RemittanceFileImportAdapterBasicTest : TestCaseWithFactory
	{
		public void TestProcessWithSaveExceptionHandlingIniitalizationMustBeSetToDoImport()
		{
			var adapter = new RemittanceFileImportAdapter();
			AssertExceptionThrown(typeof(ArgumentNullException), "Value cannot be null.\r\nParameter name: ProcessWithSaveExceptionHandling", () => adapter.PerformRemittanceFileImport(null, null, null), true);
		}
	}
}
