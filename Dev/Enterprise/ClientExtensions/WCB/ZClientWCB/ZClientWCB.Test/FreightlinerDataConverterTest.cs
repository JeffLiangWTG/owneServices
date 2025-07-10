using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.WCB.Testing
{
	public class FreightlinerDataConverterTest : TestCaseWithFactory
	{
		public void TestMapImport()
		{
			FreightlinerDataConverterTestClass converter = new FreightlinerDataConverterTestClass(Factory);
			string[] dataHeader = { "SUMMARY NUMBER", "B/L", "DATE ARRIVED", "DATE", "ID CODE", "AIR/OCEAN", "CARRIER", "FREIGHT", "PICK TICKET", "PO", "FTL INVOICE", "LINE", "PART NUMBER", "ALTERNATE PART", "QTY", "DES ENG", "DESC SPN", "DESC SPN 2", "UOM", "STD PK", "COUNTRY", "UNIT PRICE", "EXT PRICE", "WEIGHT", "ETD", "ETA", "FLIGHT/VOY", "NUMBER OF BOXES", "INV DATE", "BOX NUMBER", "QTY RCVD", "QTY SHIPPED", "NOTES", "AWB", "OUTBOUND CARRIER", "SHIP TO", "YN", "SID", "HTS_USA", "HTSALL" };
			FlatFileDataRow columnHeader = new FlatFileDataRow(dataHeader);
			string[] stringLine1 = { "OE30526", "009647591    ABF", "4/21/2005 0:00:00", "4/6/2005 0:00:00", "EX601", "O", "019 ABF", "0.00", "02A2902319", "#0000040738-00", "Q30680", "370", "97HA 13008 AA", "", "6.00", "LAMP ASY", "", "", "EA", "1.00", "US", "32.81", "196.86", "4.50", "5/22/2005 0:00:00", "6/10/2005 0:00:00", "815", "", "", "819", "3.00", "3.00", "", "LOA03030526", "COLUMBUS WAIKATO", "A", "0", "EX601/30526/SEA", "N/A", "N/A" };
			FlatFileDataRow fileLine1 = new FlatFileDataRow(stringLine1);
			string[] stringLine2 = { "OE30526", "009643903-ABFS", "4/27/2005 0:00:00", "4/21/2005 0:00:00", "EX601", "O", "066-ABFS", "0.00", "02A2924323", "#0000055075-00", "R25184", "590", "18-41054-104", "", "10.00", "SWITCH PANEL", "", "", "EA", "1.00", "US", "38.46", "384.60", "0.25", "5/22/2005 0:00:00", "6/10/2005 0:00:00", "815", "", "", "857", "2.00", "2.00", "", "LOA03030526", "COLUMBUS WAIKATO", "A", "0", "EX601/30526/SEA", "", "" };
			FlatFileDataRow fileLine2 = new FlatFileDataRow(stringLine2);
			string[] stringLine3 = { "OE30526", "428993/601SEA", "4/5/2005 0:00:00", "4/4/2005 0:00:00", "EX601", "O", "FL-XSP", "0.00", "1143FLJ58", "#69521OCEAN-01", "1000042183", "000010", "DMS FJ126DDBSNNN", "", "1.00", "EZ RIDE RD CLTH HIBK", "", "", "EA", "1.00", "", "450.64", "450.64", "54.15", "5/22/2005 0:00:00", "6/10/2005 0:00:00", "815", "", "", "841", "1.00", "1.00", "", "LOA03030526", "COLUMBUS WAIKATO", "SURFACE", "0", "EX601/30526/SEA", "", "" };
			FlatFileDataRow fileLine3 = new FlatFileDataRow(stringLine3);
			FlatFileDataRowCollection fileLines = new FlatFileDataRowCollection();
			fileLines.Add(columnHeader);
			fileLines.Add(fileLine1);
			fileLines.Add(fileLine2);
			fileLines.Add(fileLine3);
			Xsd.InvoiceHeader invoiceHeader = new Xsd.InvoiceHeader();
			converter.MapImport(invoiceHeader, fileLines);
			AssertEquals("Invoice Line Count equals 3 as the 1st row of file is the data header", 3, invoiceHeader.InvoiceLines.Count);
			AssertEquals("Invoice number", "Q30680", invoiceHeader.InvoiceNumber.ToString());
			AssertEquals("Total Price of Invoice Line 1", "98.43", invoiceHeader.InvoiceLines[0].LinePrice.Value.ToString());
			AssertEquals("Total Price of Invoice Line 2", "76.92", invoiceHeader.InvoiceLines[1].LinePrice.Value.ToString());
			AssertEquals("Total Price of Invoice Line 3", "450.64", invoiceHeader.InvoiceLines[2].LinePrice.Value.ToString());
			AssertEquals("Total Line Amount", 625.99m, invoiceHeader.InvoiceAmount.Value);
			AssertEquals("Order Number of Invoice Line 1", "#0000040738-00", invoiceHeader.InvoiceLines[0].OrderNumber);
			AssertEquals("Order Number of Invoice Line 2", "#0000055075-00", invoiceHeader.InvoiceLines[1].OrderNumber);
			AssertEquals("Order Number of Invoice Line 3", "#69521OCEAN-01", invoiceHeader.InvoiceLines[2].OrderNumber);
			AssertEquals("Part Number of Invoice Line 1", "97HA 13008 AA", invoiceHeader.InvoiceLines[0].ProductNumber);
			AssertEquals("Part Number of Invoice Line 2", "18-41054-104", invoiceHeader.InvoiceLines[1].ProductNumber);
			AssertEquals("Part Number of Invoice Line 3", "DMS FJ126DDBSNNN", invoiceHeader.InvoiceLines[2].ProductNumber);
			AssertEquals("Goods Origin ", "US", invoiceHeader.InvoiceLines[0].LineClassification.OriginOfGoods);
			AssertEquals("Goods Origin ", "US", invoiceHeader.InvoiceLines[1].LineClassification.OriginOfGoods);
			AssertEquals("Goods Origin ", "", invoiceHeader.InvoiceLines[2].LineClassification.OriginOfGoods);
			AssertEquals("Invoice Quantity of Invoice Line 1", 3m, invoiceHeader.InvoiceLines[0].InvoiceQty.Value);
			AssertEquals("Invoice Quantity of Invoice Line 2", 2m, invoiceHeader.InvoiceLines[1].InvoiceQty.Value);
			AssertEquals("Invoice Quantity of Invoice Line 3", 1m, invoiceHeader.InvoiceLines[2].InvoiceQty.Value);
		}
	}
}
