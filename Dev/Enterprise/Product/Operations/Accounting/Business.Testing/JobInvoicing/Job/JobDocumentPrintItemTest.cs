using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(JobDocumentPrintItem))]
	public class JobDocumentPrintItemTest : NonPersistentBusinessObjectTestCase
	{
		public void TestProperties()
		{
			AssertEquals(false, JobDocPrintItem.PrintAPInvoiceAnalysis);
			AssertEquals(false, JobDocPrintItem.PrintARInvoiceAnalysis);
			AssertEquals(false, JobDocPrintItem.PrintJobRevenueJournalAnalysis);
			AssertEquals(false, JobDocPrintItem.PrintChargeDetail);
			AssertEquals(false, JobDocPrintItem.PrintChargeSummary);
			AssertEquals(false, JobDocPrintItem.PrintProfitRecognitionByDateSummary);
			JobDocPrintItem.Printer.PrintAPInvoiceAnalysis = true;
			JobDocPrintItem.Printer.PrintARInvoiceAnalysis = true;
			JobDocPrintItem.Printer.PrintJobRevenueJournalAnalysis = true;
			JobDocPrintItem.Printer.PrintChargeDetail = true;
			JobDocPrintItem.Printer.PrintChargeSummary = true;
			JobDocPrintItem.Printer.PrintProfitRecognitionByDateSummary = true;
			AssertEquals(true, JobDocPrintItem.PrintAPInvoiceAnalysis);
			AssertEquals(true, JobDocPrintItem.PrintARInvoiceAnalysis);
			AssertEquals(true, JobDocPrintItem.PrintJobRevenueJournalAnalysis);
			AssertEquals(true, JobDocPrintItem.PrintChargeDetail);
			AssertEquals(true, JobDocPrintItem.PrintChargeSummary);
			AssertEquals(true, JobDocPrintItem.PrintProfitRecognitionByDateSummary);
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();
			GetNewBusinessObject();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			Job job = Factory.NewJobForTesting<Job>();
			JobDocumentPrinter printer = new JobDocumentPrinter(Factory);
			JobDocPrintItem = new JobDocumentPrintItem(printer, job, Factory);
			return JobDocPrintItem;
		}

		protected JobDocumentPrintItem JobDocPrintItem;

		#endregion
	}
}
