using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Business;

namespace Enterprise.Client.ZClientPOW.Suzuki.Testing
{
	public class DataTransferTest : TestCaseWithFactory
	{
		public void TestImportSuzukiFileNotExist()
		{
			var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			bool result = DataTransfer.ImportInvoices(JobDec, "ZClientPOW.Test.notHere.txt");
			AssertEquals(0, JobDec.Invoices.Count);
			AssertEquals(0, JobDec.InvoiceLines.Count);
			AssertEquals(false, result);
		}

		public void TestImportSuzukiInvoice()
		{
			bool result = DataTransfer.ImportInvoices(JobDec, PathToValidTestFile);
			AssertEquals(1, JobDec.Invoices.Count);
			AssertEquals(38, JobDec.InvoiceLines.Count);
			AssertEquals(true, result);
		}

		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}
		#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			JobDec = Factory.New<BaseJobDeclaration>();
			DataTransfer = new SuzukiDataTransfer();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			PathToValidTestFile = resourceRetriever.SaveResourceToFile("SuzukiTestData.txt");
		}
		EmbeddedResourceRetriever resourceRetriever;
		BaseJobDeclaration JobDec;
		SuzukiDataTransfer DataTransfer;
		string PathToValidTestFile;
		#endregion
	}
}
