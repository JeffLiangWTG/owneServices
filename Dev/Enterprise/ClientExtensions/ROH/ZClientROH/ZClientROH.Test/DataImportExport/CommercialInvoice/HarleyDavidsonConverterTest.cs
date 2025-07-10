using System.IO;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.Rohlig.HarleyDavidson.Testing
{
	public class HarleyDavidsonConverterTest : TestCaseWithFactory
	{
		public void TestMapImport()
		{
			NotificationBuffer notify = new NotificationBuffer();
			HarleyDavidsonConverter converter = new HarleyDavidsonConverter(notify, Factory);
			Xsd.InvoiceHeader header = new Xsd.InvoiceHeader();
			using (var resourceRetriever = new EmbeddedResourceRetriever(GetType().Assembly))
			{
				var testingFile = resourceRetriever.SaveResourceToFile("HarleyCommercialInvoice.csv");
				using (StreamReader reader = new StreamReader(testingFile))
				{
					converter.ImportFlatFile(header, new HarleyDavidsonFlatFileFormat(), reader);
				}
			}

			AssertEquals("Header should have an InoivceNumber of 'Invoice'", "Invoice", header.InvoiceNumber);
			AssertEquals("InvoiceHeader Amount should be ", 103.29m, header.InvoiceAmount.Value);
			AssertEquals("InvoiceHeader Amount Currency should be AUD", Core.Constants.CurrencyCodes.Australia, header.InvoiceAmount.CurrencyCode);
			AssertEquals("Header should have 4 Invoice Lines", 4, header.InvoiceLines.Count);
			AssertInvoiceLine(header.InvoiceLines[0], "CA0006.T", "SCREW, CLUTCH LEVER", 1, 0.18m);
			AssertInvoiceLine(header.InvoiceLines[1], "N0520.02A8", "FOOTPEG ASSY, RIDER, LH", 2, 25.02m);
			AssertInvoiceLine(header.InvoiceLines[2], "N0521.02A8", "FOOTPEG ASSY, RIDER, RH", 4, 50.04m);
			AssertInvoiceLine(header.InvoiceLines[3], "N0553.02A8", "FOOTPEG, PASSENGER, RH", 3, 28.05m);
		}

		void AssertInvoiceLine(Xsd.InvoiceLine line, ZString partNumber, ZString description, ZDecimal quantity, ZDecimal price)
		{
			AssertEquals("PartNumber", partNumber, line.ProductNumber);
			AssertEquals("Description", description, line.ProductDescription);
			AssertEquals("Quantity", quantity, line.InvoiceQty.Value);
			AssertEquals("UnitPrice", price, line.LinePrice.Value);
			AssertEquals("UnitPriceCurrency", GlbCompany.CurrentCompany.LocalCurrency.RX_Code, line.LinePrice.CurrencyCode);
		}
	}
}
