using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB.Testing
{
	public class DaimlerDataConverterTest : TestCaseWithFactory
	{
		public void TestMapImportForDaimlerInvFormat()
		{
			DaimlerDataConverterTestClass converter = new DaimlerDataConverterTestClass(Factory);
			string[] dataHeader = { "Invoice", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "Delivery", "Line", "Part", "N/A", "Qty", "Description", "N/A", "N/A", "N/A", "N/A", "Country of Orig", "Gross Sale", "Net Price/Line", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "Customs Num", "N/A", "N/A", "N/A", "N/A", "N/A", "Gross Price/Line", "Disc Code", "Fact Iden" };
			FlatFileDataRow columnHeader = new FlatFileDataRow(dataHeader);
			string[] stringLine1 = { "926846", "", "", "", "", "", "", "", "", "", "263733", "00001", "722589013100", "", "1", "X", "", "", "", "", "US", "207.81", "207.81", "", "", "", "", "", "", "", "", "", "0000820559", "", "", "", "", "", "207.81", "0", "88" };
			FlatFileDataRow fileLine1 = new FlatFileDataRow(stringLine1);
			string[] stringLine2 = { "926846", "", "", "", "", "", "", "", "", "", "263733", "00002", "210589003700", "", "1", "PLIERS", "", "", "", "", "000", "37.93", "37.93", "", "", "", "", "", "", "", "", "", "0000820320", "", "", "", "", "", "37.93", "0", "88" };
			FlatFileDataRow fileLine2 = new FlatFileDataRow(stringLine2);
			FlatFileDataRowCollection fileLines = new FlatFileDataRowCollection();
			fileLines.Add(columnHeader);
			fileLines.Add(fileLine1);
			fileLines.Add(fileLine2);
			Xsd.InvoiceHeader invoiceHeader = new Xsd.InvoiceHeader();
			converter.MapImport(invoiceHeader, fileLines);
			AssertEquals("Invoice Line Count equals 2 as the 1st row of file is the data header", 2, invoiceHeader.InvoiceLines.Count);
			AssertEquals("Invoice number", "926846", invoiceHeader.InvoiceNumber.ToString());
			AssertEquals("Total Price of Invoice Line 1", "207.81", invoiceHeader.InvoiceLines[0].LinePrice.Value.ToString());
			AssertEquals("Total Price of Invoice Line 2", "37.93", invoiceHeader.InvoiceLines[1].LinePrice.Value.ToString());
			AssertEquals("Total Line Amount", 245.74m, invoiceHeader.InvoiceAmount.Value);
			AssertEquals("Order Number of Invoice Line 1", "", invoiceHeader.InvoiceLines[0].OrderNumber);
			AssertEquals("Order Number of Invoice Line 2", "", invoiceHeader.InvoiceLines[1].OrderNumber);
			AssertEquals("Part Number of Invoice Line 1", "722589013100", invoiceHeader.InvoiceLines[0].ProductNumber);
			AssertEquals("Part Number of Invoice Line 2", "210589003700", invoiceHeader.InvoiceLines[1].ProductNumber);
			AssertEquals("Goods Origin ", "US", invoiceHeader.InvoiceLines[0].LineClassification.OriginOfGoods);
			AssertEquals("Goods Origin ", "", invoiceHeader.InvoiceLines[1].LineClassification.OriginOfGoods);
			AssertEquals("Invoice Quantity of Invoice Line 1", 1m, invoiceHeader.InvoiceLines[0].InvoiceQty.Value);
			AssertEquals("Invoice Quantity of Invoice Line 2", 1m, invoiceHeader.InvoiceLines[1].InvoiceQty.Value);
		}

		public void TestMapImportProblemDaimlerInvoiceValue()
		{
			DaimlerDataConverterTestClass converter = new DaimlerDataConverterTestClass(Factory);
			string[] dataHeader = { "Invoice", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "Delivery", "Line", "Part", "N/A", "Qty", "Description", "N/A", "N/A", "N/A", "N/A", "Country of Orig", "Gross Sale", "Net Price/Line", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "N/A", "Customs Num", "N/A", "N/A", "N/A", "N/A", "N/A", "Gross Price/Line", "Disc Code", "Fact Iden" };
			FlatFileDataRow columnHeader = new FlatFileDataRow(dataHeader);
			string[] stringLine1 = { "929635", "", "", "", "", "", "", "", "", "", "272706", "00001", "9018600702", "", "1", "LEATHER AIRBAG", "", "", "", "", "000", "860.13", "507.48", "", "", "", "", "", "", "", "", "", "0000870899", "", "", "", "", "", "860.13", "1", "88" };
			FlatFileDataRow fileLine1 = new FlatFileDataRow(stringLine1);
			string[] stringLine2 = { "929635", "", "", "", "", "", "", "", "", "", "272706", "00002", "2108600286", "", "1", "SAFETY BELT", "", "", "", "", "000", "249.66", "147.30", "", "", "", "", "", "", "", "", "", "0000870821", "", "", "", "", "", "249.66", "8", "88" };
			FlatFileDataRow fileLine2 = new FlatFileDataRow(stringLine2);
			string[] stringLine5 = { "929635", "", "", "", "", "", "", "", "", "", "272746", "00003", "0005800350", "", "9", "NOISE DAMPING S", "", "", "", "", "000", "354.22", "1880.91", "", "", "", "", "", "", "", "", "", "0000870899", "", "", "", "", "", "3187.98", "1", "88" };
			FlatFileDataRow fileLine5 = new FlatFileDataRow(stringLine5);
			string[] stringLine8 = { "929635", "", "", "", "", "", "", "", "", "", "273055", "00001", "0009860050", "", "4", "PAINT PIN", "", "", "", "", "000", "11.09", "26.16", "", "", "", "", "", "", "", "", "", "0000320820", "", "", "", "", "", "44.36", "8", "88" };
			FlatFileDataRow fileLine8 = new FlatFileDataRow(stringLine8);
			FlatFileDataRowCollection fileLines = new FlatFileDataRowCollection();
			fileLines.Add(columnHeader);
			fileLines.Add(fileLine1);
			fileLines.Add(fileLine2);
			fileLines.Add(fileLine5);
			fileLines.Add(fileLine8);
			Xsd.InvoiceHeader invoiceHeader = new Xsd.InvoiceHeader();
			converter.MapImport(invoiceHeader, fileLines);
			AssertEquals("Invoice Line Count equals 4 as the 1st row of file is the data header", 4, invoiceHeader.InvoiceLines.Count);
			AssertEquals("Total Price of Invoice Line 5", "1880.91", invoiceHeader.InvoiceLines[2].LinePrice.Value.ToString());
			AssertEquals("Total Price of Invoice Line 8", "26.16", invoiceHeader.InvoiceLines[3].LinePrice.Value.ToString());
		}
	}
}
