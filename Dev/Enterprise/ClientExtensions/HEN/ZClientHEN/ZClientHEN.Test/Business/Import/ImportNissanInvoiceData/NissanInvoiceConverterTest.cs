using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.HEN.Nissan.Testing
{
	sealed class NissanInvoiceConverterTest : TestCaseWithFactory
	{
		public void TestMapImport()
		{
			var invoiceHeader = new Xsd.InvoiceHeader();
			var pathToTestFile = resourceRetriever.SaveResourceToFile("ImportNissanInvoiceData.TestFiles.PROFORMA3.TXT");
			using (TextReader reader = new StreamReader(pathToTestFile))
			{
				var converter = new NissanInvoiceConverter(new NotificationBuffer(), Factory);
				converter.ImportFlatFile(invoiceHeader, new NissanInvoiceFlatFileFormat(), reader);
			}

			AssertNotNull(invoiceHeader);
			AssertEquals("20090206", invoiceHeader.InvoiceNumber);
			AssertEquals("AUD", invoiceHeader.InvoiceAmount.CurrencyCode);
			AssertEquals((ZDecimal)11545.07, invoiceHeader.InvoiceAmount.Value);
			AssertEquals((ZDecimal)2100.00, invoiceHeader.Weight.Value);
			AssertEquals(2, invoiceHeader.InvoiceLines.Count);
			AssertEquals("B52404M400", invoiceHeader.InvoiceLines[0].ProductNumber);
			AssertEquals((ZDecimal)2, invoiceHeader.InvoiceLines[0].InvoiceQty.Value);
			AssertEquals((ZDecimal)11.98, invoiceHeader.InvoiceLines[0].LinePrice.Value);
			AssertEquals((ZDecimal)1.00, invoiceHeader.InvoiceLines[0].Weight.Value);
			AssertEquals("4298NZ", invoiceHeader.InvoiceLines[0].OrderNumber);
			AssertEquals("JP", invoiceHeader.InvoiceLines[0].LineClassification.OriginOfGoods);
		}

		protected override void SetUp()
		{
			base.SetUp();
			resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly);
		}
		protected override void TearDown()
		{
			base.TearDown();
			resourceRetriever.Dispose();
		}

		EmbeddedResourceRetriever resourceRetriever;
	}
}
