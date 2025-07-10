using System.Collections;
using CargoWise.IO;
using CargoWise.Types;
using NUnit.Framework;
using InvoiceLineConstants = Enterprise.Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceLineFields;
using RecordType = Enterprise.Customs.DataTransfer.FlatFileInvoiceDataImporter.Constants.InvoiceRecordType;

namespace Enterprise.Client.ZClientPOW.Suzuki.Testing
{
	public class SuzukiInvoiceFileDataReaderTest : TestCase
	{
		public void TestRecords()
		{
			AssertEquals("Incorrect number of Invoice Lines", 39, Reader.Records.Length);
		}

		#region Data Conversion and Processing
		public void ProcessRecord()
		{
			string testLine = "       6591         S10474KK130-33028    BEARING-NEEDLE,72KTV030              5     10                           52.50";
			ArrayList records = new ArrayList();
			Reader.ProcessRecord(testLine, records);
			AssertEquals("Header not found", RecordType.Head, ((string)records[0])[0]);
			AssertEquals("Should have been the product number", "K130-33028", ((string)records[1])[0]);
			AssertEquals("Should have been the quantity", "10", ((string)records[1])[1]);
			AssertEquals("Should have been the total price", "52.50", ((string)records[1])[2]);
			AssertEquals("One Record", 1, records.Count);
		}

		public void TestExtractInvoiceLineFromDataLine()
		{
			string testLine = "       6591         S10474KK130-33028    BEARING-NEEDLE,72KTV030              5     10                           52.50";
			string[] result = Reader.ExtractInvoiceLineFromDataLine(testLine);
			AssertEquals("The ProductNumber was not extracted correctly", "KK130-33028", result[0]);
			AssertEquals("The Quantity was not extraced correctly", "10", result[1]);
			AssertEquals("The TotalPrice was not extracted correctly", "52.50", result[2]);
		}

		public void TestConvertDataLineToEDICsvLine()
		{
			string[] dataLine = new string[3];
			dataLine[0] = "product123";
			dataLine[1] = "500";
			dataLine[2] = "965.32";
			string[] result = Reader.ConvertDataLineToEDICsvLine(dataLine);
			AssertEquals("should have been LINE", RecordType.Line, result[InvoiceLineConstants.Type]);
			AssertEquals("could not find product", "product123", result[InvoiceLineConstants.ProductCode]);
			AssertEquals("Quanity is incorrect", "500", result[InvoiceLineConstants.Quantity]);
			AssertEquals("LinePrice is incorrect", "965.32", result[InvoiceLineConstants.LinePrice]);
		}

		#endregion
		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}
		#region Setup
		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
			testFilePath = resourceRetriever.SaveResourceToFile("SuzukiTestData.txt");
			Reader = new SuzukiInvoiceFileDataReaderTestClass(testFilePath);
		}

		EmbeddedResourceRetriever resourceRetriever;
		SuzukiInvoiceFileDataReaderTestClass Reader;
		string testFilePath;
		class SuzukiInvoiceFileDataReaderTestClass : SuzukiInvoiceDataFileReader
		{
			public SuzukiInvoiceFileDataReaderTestClass(string uri) : base(uri)
			{
			}

			public new void ProcessRecord(string lineFromFile, ArrayList recordsArray)
			{
				base.ProcessRecord(lineFromFile, recordsArray);
			}

			public new string[] ExtractInvoiceLineFromDataLine(ZString dataLine)
			{
				return base.ExtractInvoiceLineFromDataLine(dataLine);
			}

			public new string[] ConvertDataLineToEDICsvLine(string[] dataLine)
			{
				return base.ConvertDataLineToEDICsvLine(dataLine);
			}
		}
		#endregion
	}
}
