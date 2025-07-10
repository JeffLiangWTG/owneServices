using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.Customs.Business;

namespace Enterprise.Client.ZClientCCP.Kawasaki.Testing
{
	public class DataTransferTest : TestCaseWithFactory
	{
		public void TestImportKawasakiFileNotExist()
		{
				var fakeFilePath = Path.Combine(Temp.TempPath, "KawasakiTestDataBlankNotHere.txt");
				bool result = DataTransfer.ImportInvoices(JobDec, fakeFilePath);
				AssertEquals(0, JobDec.Invoices.Count);
				AssertEquals(0, JobDec.InvoiceLines.Count);
				AssertEquals(false, result);
		}

		public void TestImportKawasakiInvoice()
		{
			bool result = DataTransfer.ImportInvoices(JobDec, PathToValidTestFile);
			AssertEquals(6, JobDec.Invoices.Count);
			AssertEquals(0, JobDec.Invoices[0].Charges.Count);
			AssertEquals(36, JobDec.InvoiceLines.Count);
			AssertEquals(true, result);
		}

		public void TestImportWithEnoughFields()
		{
			string tempDirectory = Temp.TempPath;
			try
			{
				var tempPath = Temp.GetTempFileName(tempDirectory);
				File.WriteAllText(tempPath, @"""MAJN04"",""552278"",""0001"",1,""AIR FREIGHT"",""16165-S008"",1.07,1,""SEPARATOR,CRANKCASE OIL"",""JPN"",""840991""
""MAJN04"",""552278"",""0001"",1,""AIR FREIGHT"",""16165-S008"",1.07,1,""SEPARATOR,CRANKCASE OIL"",""JPN""
""MAJN04"",""872426"",""0001"",49,""AIR FREIGHT"",""92210-1184"",.87,2,""NUT"",""JPN"",
");
				bool result = DataTransfer.ImportInvoices(JobDec, tempPath);
				AssertEquals(true, result);
			}
			finally
			{
				TempDirectory.DeleteDirectory(tempDirectory);
			}
		}

		public void TestImportWithoutEnoughFields()
		{
			string tempDirectory = Temp.TempPath;
			try
			{
				var tempPath = Temp.GetTempFileName(tempDirectory);
				File.WriteAllText(tempPath, @"""MAJN04"",""552278"",""0001"",1,""AIR FREIGHT"",""16165-S008"",1.07,1,""SEPARATOR,CRANKCASE OIL"",""JPN"",""840991""
""MAJN04"",""552278"",""0001"",1,""AIR FREIGHT"",""16165-S008"",1.07,1,""SEPARATOR,CRANKCASE OIL"",""JPN""

""MAJN04"",""872426"",""0001"",49,""AIR FREIGHT"",""92210-1184"",.87,2,""NUT""
");
				bool result = DataTransfer.ImportInvoices(JobDec, tempPath);
				AssertEquals(false, result);
				AssertEquals(@"The following lines do not have enough fields:
Line 2
Line 3
Line 4", DataTransfer.ErrorMessage);
			}
			finally
			{
				TempDirectory.DeleteDirectory(tempDirectory);
			}
		}

		#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			JobDec = Factory.New<BaseJobDeclaration>();
			DataTransfer = new KawasakiDataTransfer();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			PathToValidTestFile = resourceRetriever.SaveResourceToFile("Enterprise.Client.ZClientCCP.Testing.Kawasaki.TestFiles.KawasakiCustomsData.txt");
		}
		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		BaseJobDeclaration JobDec;
		KawasakiDataTransfer DataTransfer;
		string PathToValidTestFile;
		EmbeddedResourceRetriever resourceRetriever;
		#endregion
	}
}
