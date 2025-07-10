using CargoWise.EntityFramework.Testing;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Client.DFD.Export.Testing
{
	public class DFDFinancialInvoiceTransactionExportFilterTest : TestCaseWithFactory
	{
		public void TestCreateDefaultQuery()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			ARCreditNote aRCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			ARAdjustmentNote aRAdjustmentNote1 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoice1.Logs.AddNew(Events.DataExport);
			aRCreditNote1.Logs.AddNew(Events.DataExport, DFDConstants.Export.DEXEventReference + "via ZD3 on 3-May-08");
			aRInvoice2.Logs.AddNew(Events.DataExport, "Other");
			Factory.Save();
			TransactionExportFilterProvider transactionHeaderExportFilterProvider = new TransactionExportFilterProvider(Factory);
			transactionHeaderExportFilterProvider.IncludeARAdjustmentNotes = true;
			transactionHeaderExportFilterProvider.IncludeARCreditNotes = true;
			transactionHeaderExportFilterProvider.IncludeARInvoices = true;
			DFDFinancialInvoiceTransactionExportFilter transactionHeaderExportFilter = new DFDFinancialInvoiceTransactionExportFilter(Factory, transactionHeaderExportFilterProvider);
			AccTransactionHeader[] transactionHeaders = (AccTransactionHeader[])Factory.Load(typeof(AccTransactionHeader), transactionHeaderExportFilter.Filter);
			AssertEquals(3, transactionHeaders.Length);
		}
	}
}
