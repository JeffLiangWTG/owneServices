using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;

namespace Enterprise.Customs.GB.CDS.Messaging.Wrappers.Testing
{
	public class GbCDSExportEntryLineWrapperTests : TestCaseWithFactory
	{
		public void TestGbCDSExportEntryLineWrapperDescription()
		{
			CombineAssertions(() =>
			{
				var declaration = Factory.NewWithValidTestData<JobDeclaration>();
				var invoice = declaration.Invoices.AddNew();
				invoice.FillWithValidTestData();
				var line1 = invoice.JobComInvoiceLines.AddNew();
				line1.FillWithValidTestData();
				line1.JI_Description = "LINE\r\n25";
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				var entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				AssertEquals("Description", new ZString("LINE\r\n25"), entryLine.Description);
				var wrapper = new GbCDSExportEntryLineWrapper(entryLine);
				AssertEquals("Description", new ZString("LINE 25"), ((ICommodity)wrapper).Description);

				line1.JI_Description = "1234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789" +
					"012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456789";
				AssertEquals(289, line1.JI_Description.Length);
				declaration.DoMerge(new Customs.Business.SendsMessagesToCustomsShutterUpperer());

				entryLine = declaration.CustomsEntryHeaders[0].MergedLines[0];
				wrapper = new GbCDSExportEntryLineWrapper(entryLine);
				AssertEquals(280, ((ICommodity)wrapper).Description.Length);
			});
		}
	}
}
