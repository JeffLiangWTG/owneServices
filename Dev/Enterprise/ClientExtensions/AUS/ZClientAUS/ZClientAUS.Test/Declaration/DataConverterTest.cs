using CargoWise.EntityFramework.Testing;
using Enterprise.DataTransfer.Business;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.AUS.Testing
{
	public class DataConverterTest : TestCaseWithFactory
	{
		public void TestMapImportForAUSInvoiceFormat()
		{
			AUSDataConverterTestClass converter = new AUSDataConverterTestClass(Factory);
			string line1 = @"76314875  251959465B     FAN WHEEL                     0000001000002055000084ED";
			string line2 = @"76314875  1J0881805JJLGL COVER BACKREST LEATHER BLACK  0000025000025500000228CSI";
			string line3 = @"76314875  6Q0881805CDMTP BACKREST COVER, HEATER ELEMENT000002500000720000015.0SI";
			FlatFileDataRowCollection fileLines = new FlatFileDataRowCollection();
			AUSFlatFileFormat flatFileFormater = new AUSFlatFileFormat();
			FlatFileDataRow fileLine1 = flatFileFormater.ConvertToRow(line1);
			fileLines.Add(fileLine1);
			FlatFileDataRow fileLine2 = flatFileFormater.ConvertToRow(line2);
			fileLines.Add(fileLine2);
			FlatFileDataRow fileLine3 = flatFileFormater.ConvertToRow(line3);
			fileLines.Add(fileLine3);
			Xsd.InvoiceHeaderCollection testInvoiceHeaderCollection = new Xsd.InvoiceHeaderCollection();
			converter.MapImport(testInvoiceHeaderCollection, fileLines);
			Xsd.InvoiceHeader invoiceHeader = testInvoiceHeaderCollection[0];
			AssertEquals("Invoice Line Count equals 3", 3, invoiceHeader.InvoiceLines.Count);
			AssertEquals("Invoice number", "76314875", invoiceHeader.InvoiceNumber.ToString());
			AssertEquals("Part Number - Invoice Line 1", "251959465B", invoiceHeader.InvoiceLines[0].ProductNumber);
			AssertEquals("Part Description - Invoice Line 1", "FAN WHEEL", invoiceHeader.InvoiceLines[0].ProductDescription);
			AssertEquals("Invoice Quantity - Invoice Line 1", 1m, invoiceHeader.InvoiceLines[0].InvoiceQty.Value);
			AssertEquals("Line Price - Invoice Line 1", 20.55m, invoiceHeader.InvoiceLines[0].LinePrice.Value);
			AssertEquals("Part Number - Invoice Line 2", "1J0881805JJLGL", invoiceHeader.InvoiceLines[1].ProductNumber);
			AssertEquals("Part Description - Invoice Line 2", "COVER BACKREST LEATHER BLACK", invoiceHeader.InvoiceLines[1].ProductDescription);
			AssertEquals("Invoice Quantity - Invoice Line 2", 25m, invoiceHeader.InvoiceLines[1].InvoiceQty.Value);
			AssertEquals("Line Price - Invoice Line 2", 6375m, invoiceHeader.InvoiceLines[1].LinePrice.Value);
			AssertEquals("Part Number of Invoice Line 3", "6Q0881805CDMTP", invoiceHeader.InvoiceLines[2].ProductNumber);
			AssertEquals("Part Description - Invoice Line 3", "BACKREST COVER, HEATER ELEMENT", invoiceHeader.InvoiceLines[2].ProductDescription);
			AssertEquals("Invoice Quantity - Invoice Line 3", 25m, invoiceHeader.InvoiceLines[2].InvoiceQty.Value);
			AssertEquals("Line Price - Invoice Line 3", 1800m, invoiceHeader.InvoiceLines[2].LinePrice.Value);
			AssertEquals("Total Line Amount", 8195.55m, invoiceHeader.InvoiceAmount.Value);
		}
	}
}
