using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.DataTransfer;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Client.DFD.Export.Testing
{
	public class DFDFinancialInvoiceTransactionExportFilterForBatchProcessTest : TestCaseWithFactory
	{
		[TestDate(2008, 5, 3)]
		public void TestCreateDefaultQuery()
		{
			ARInvoice aRInvoice1 = Factory.NewWithValidTestData<ARInvoice>();
			ARCreditNote aRCreditNote1 = Factory.NewWithValidTestData<ARCreditNote>();
			ARAdjustmentNote aRAdjustmentNote1 = Factory.NewWithValidTestData<ARAdjustmentNote>();
			ARInvoice aRInvoice2 = Factory.NewWithValidTestData<ARInvoice>();
			aRCreditNote1.Logs.AddNew(Events.DataExport, DFDConstants.Export.DEXEventReference + "via ZD3 on 29-Jan-07");
			aRInvoice2.Logs.AddNew(Events.DataExport, DFDConstants.Export.DEXEventReference + "3-May-08");
			Factory.Save();
			TransactionExportFilterProvider transactionHeaderExportFilterProvider = new TransactionExportFilterProvider(Factory);
			transactionHeaderExportFilterProvider.IncludeARAdjustmentNotes = true;
			transactionHeaderExportFilterProvider.IncludeARCreditNotes = true;
			transactionHeaderExportFilterProvider.IncludeARInvoices = true;
			DFDFinancialInvoiceTransactionExportFilterForBatchProcessForTest transactionHeaderExportFilter = new DFDFinancialInvoiceTransactionExportFilterForBatchProcessForTest(Factory, transactionHeaderExportFilterProvider);
			AccTransactionHeader[] transactionHeaders = (AccTransactionHeader[])Factory.Load(typeof(AccTransactionHeader), transactionHeaderExportFilter.Filter);
			AssertEquals(2, transactionHeaders.Length);
		}

		class DFDFinancialInvoiceTransactionExportFilterForBatchProcessForTest : DFDFinancialInvoiceTransactionExportFilterForBatchProcess
		{
			public DFDFinancialInvoiceTransactionExportFilterForBatchProcessForTest(BusinessObjectFactory factory, TransactionExportFilterProvider filterProvider) : base(factory, filterProvider, (new ZDateTime(2008, 5, 3)).AddDays(-3), (new ZDateTime(2008, 5, 3)).AddDays(3))
			{
			}

			public new ZDBOnlyQuery CreateDefaultQuery()
			{
				return base.CreateDefaultQuery();
			}
		}
	}
}
