using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.JobInvoicing.Testing
{
	[TestedType(typeof(ConsolJobDocumentPrintItem))]
	public class ConsolJobDocumentPrintItemTest : JobDocumentPrintItemTest
	{
		public void TestConsolProperties()
		{
			AssertEquals(false, ((ConsolJobDocumentPrintItem)JobDocPrintItem).PrintContainerPackingSummary);
			AssertEquals(false, ((ConsolJobDocumentPrintItem)JobDocPrintItem).PrintJobByJobSummary);
			((ConsolJobDocumentPrintItem)JobDocPrintItem).Printer.PrintContainerPackingSummary = true;
			((ConsolJobDocumentPrintItem)JobDocPrintItem).Printer.PrintJobByJobSummary = true;
			AssertEquals(true, ((ConsolJobDocumentPrintItem)JobDocPrintItem).PrintContainerPackingSummary);
			AssertEquals(true, ((ConsolJobDocumentPrintItem)JobDocPrintItem).PrintJobByJobSummary);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			ForwardingConsol consol = Factory.New<ForwardingConsol>();
			ConsolJobDocumentPrinter jobDocPrinter = new ConsolJobDocumentPrinter(Factory);
			JobDocPrintItem = new ConsolJobDocumentPrintItem(jobDocPrinter, consol, Factory);
			return JobDocPrintItem;
		}

		#endregion
	}
}
