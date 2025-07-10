using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.AU.Declaration.Business;
using Enterprise.DataTransfer.Business;
using Enterprise.DataTransfer.DataAdapters;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.AUS.Testing
{
	public class AUSDataImporterTest : TestCaseWithFactory
	{
		public void TestAUSDataImporterForMultipleInvoices()
		{
			AUSDataConverterTestClass converter = new AUSDataConverterTestClass(Factory);
			string line1 = @"76314875  251959465B     FAN WHEEL                     0000001000002055000084ED";
			string line2 = @"76314875  1J0881805JJLGL COVER BACKREST LEATHER BLACK  0000025000025500000228CSI";
			string line3 = @"76314875  6Q0881805CDMTP BACKREST COVER, HEATER ELEMENT000002500000720000015.0SI";
			string line4 = @"76314962  6Q0881805CDMTP BACKREST COVER, HEATER ELEMENT00000250000072000001560SI";
			string line5 = @"76314962  251959465B     FAN WHEEL                     0000001000002055000084ED";
			FlatFileDataRowCollection fileLines = new FlatFileDataRowCollection();
			AUSFlatFileFormat flatFileFormater = new AUSFlatFileFormat();
			FlatFileDataRow fileLine1 = flatFileFormater.ConvertToRow(line1);
			fileLines.Add(fileLine1);
			FlatFileDataRow fileLine2 = flatFileFormater.ConvertToRow(line2);
			fileLines.Add(fileLine2);
			FlatFileDataRow fileLine3 = flatFileFormater.ConvertToRow(line3);
			fileLines.Add(fileLine3);
			FlatFileDataRow fileLine4 = flatFileFormater.ConvertToRow(line4);
			fileLines.Add(fileLine4);
			FlatFileDataRow fileLine5 = flatFileFormater.ConvertToRow(line5);
			fileLines.Add(fileLine5);
			Xsd.InvoiceHeaderCollection testInvoiceHeaderCollection = new Xsd.InvoiceHeaderCollection();
			JobDeclaration testDec = Factory.NewWithValidTestData<JobDeclaration>();
			AUInvoiceValueObjectDataAdapter dataAdapter = new AUInvoiceValueObjectDataAdapter(testDec);
			Declaration.AUSDataImporter testImporter = new Declaration.AUSDataImporter(testDec);
			ValueObjectImportContext importContext = new ValueObjectImportContext(Factory, new NotificationBuffer());
			converter.MapImport(testInvoiceHeaderCollection, fileLines);
			AssertEquals("Invoices created", 2, testInvoiceHeaderCollection.Count);
			foreach (Xsd.InvoiceHeader invoiceHeader in testInvoiceHeaderCollection)
			{
				if (invoiceHeader.InvoiceNumber.ToString() == "76314875")
				{
					AssertEquals("Invoice Line Count equals 3", 3, invoiceHeader.InvoiceLines.Count);
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
				else if (invoiceHeader.InvoiceNumber.ToString() == "76314962")
				{
					AssertEquals("Invoice Line Count equals 2", 2, invoiceHeader.InvoiceLines.Count);
					AssertEquals("Part Number of Invoice Line 1", "6Q0881805CDMTP", invoiceHeader.InvoiceLines[0].ProductNumber);
					AssertEquals("Part Description - Invoice Line 1", "BACKREST COVER, HEATER ELEMENT", invoiceHeader.InvoiceLines[0].ProductDescription);
					AssertEquals("Invoice Quantity - Invoice Line 1", 25m, invoiceHeader.InvoiceLines[0].InvoiceQty.Value);
					AssertEquals("Line Price - Invoice Line 1", 1800m, invoiceHeader.InvoiceLines[0].LinePrice.Value);
					AssertEquals("Part Number - Invoice Line 2", "251959465B", invoiceHeader.InvoiceLines[1].ProductNumber);
					AssertEquals("Part Description - Invoice Line 2", "FAN WHEEL", invoiceHeader.InvoiceLines[1].ProductDescription);
					AssertEquals("Invoice Quantity - Invoice Line 2", 1m, invoiceHeader.InvoiceLines[1].InvoiceQty.Value);
					AssertEquals("Line Price - Invoice Line 2", 20.55m, invoiceHeader.InvoiceLines[1].LinePrice.Value);
					AssertEquals("Total Line Amount", 1820.55m, invoiceHeader.InvoiceAmount.Value);
				}
				else
				{
					Assert("Invoices not created as expected", false);
				}
			}
		}
	}
}
