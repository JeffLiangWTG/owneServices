using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Accounting.DataTransfer.Invoices.FlatFile.Testing
{
	[TestedType(typeof(DataImporterBusinessObject))]
	public class SingleInvoiceDataImporterBusinessObjectTest : DataImporterBusinessObjectTest
	{
		public void TestMessageOverrides()
		{
			SingleInvoiceDataImporterBusinessObject importerBusinessObject = new SingleInvoiceDataImporterBusinessObject(Factory);
			ZString expected = "The import process is complete, please close this form to edit the transaction";
			AssertEquals("Message", expected, importerBusinessObject.ProgressMessageForFatalError_ForTestOnly);
		}
	}
}
