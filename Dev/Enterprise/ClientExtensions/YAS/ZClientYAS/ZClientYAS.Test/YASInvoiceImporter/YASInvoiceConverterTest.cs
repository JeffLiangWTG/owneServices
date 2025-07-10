using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DataTransfer.Business;
using Enterprise.ZArchitecture;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.Client.YAS.YASInvoiceImporter.Testing
{
	public class YASInvoiceConverterTest : TestCaseWithFactory
	{
		public void TestImport()
		{
			YASInvoiceConverterForTest converter = new YASInvoiceConverterForTest(new NotificationBuffer(), Factory);
			Xsd.InvoiceHeaderCollection invoices = new Xsd.InvoiceHeaderCollection();
			FlatFileDataRowCollection collection = new FlatFileDataRowCollection();
			AddInvoiceLineToCollection(collection, "Z", "PART3", "NOT 1 But 3", "3", "333");
			AddInvoiceLineToCollection(collection, "1", "PART1", "ITS A PART", "3", "100");
			AddInvoiceLineToCollection(collection, "a", "PART2", "Its two good", "2", "220");
			AddInvoiceLineToCollection(collection, "1", "PART4", "its part of one", "1", "100");
			AddInvoiceLineToCollection(collection, "", "", "", "", "");
			converter.MapImportForTest(invoices, collection);
			AssertEquals("Invoice Collection should have Header", 3, invoices.Count);
			AssertEquals("Invoice Header should have Number", "1", invoices[0].InvoiceNumber);
			AssertEquals("Invoice Header should have Invoice Total", 4m, invoices[0].InvoiceAmount.Value);
			AssertEquals("Invoice Header Should have a Line", 2, invoices[0].InvoiceLines.Count);
			Xsd.InvoiceLine line = GetLineFromCollection(invoices[0].InvoiceLines, "PART1");
			AssertNotNull(line);
			AssertEquals("Invoice Line should have Part", "ITS A PART", line.ProductDescription);
			AssertEquals("Invoice Line should have Qty", 3m, line.InvoiceQty.Value);
			AssertEquals("Invoice Line should have Value", 3m, line.LinePrice.Value);
		}

		Xsd.InvoiceLine GetLineFromCollection(Xsd.InvoiceLineCollection lines, ZString productNumber)
		{
			Xsd.InvoiceLine result = null;
			foreach (Xsd.InvoiceLine line in lines)
			{
				if (line.ProductNumber == productNumber)
				{
					result = line;
					break;
				}
			}

			return result;
		}

		void AddInvoiceLineToCollection(FlatFileDataRowCollection collection, ZString invoiceNumber, ZString partNo, ZString partDescription, ZString qty, ZString value)
		{
			FlatFileDataRow row = new FlatFileDataRow(InvoiceConstants.NoOfFields);
			row.SetField(InvoiceConstants.FixedFieldPosition.InvoiceNo, invoiceNumber);
			row.SetField(InvoiceConstants.FixedFieldPosition.PartDescription, partDescription);
			row.SetField(InvoiceConstants.FixedFieldPosition.PartNo, partNo);
			row.SetField(InvoiceConstants.FixedFieldPosition.Qty, qty);
			row.SetField(InvoiceConstants.FixedFieldPosition.Value, value);
			collection.Add(row);
		}
	}
}
