using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class AccHeaderFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			AccHeaderFlatFileDataRow row = new AccHeaderFlatFileDataRow();
			AssertEquals("Row should have 39 Fields", 39, row.FieldCount);
			row.DocumentType = "Credit Note";
			row.SellToCustomerNumber = "DTRR665456";
			row.InvoiceNumber = "00065456";
			row.BillToCustomerNumber = "DTRR665456";
			row.BillToName = "debtor name";
			row.BillToName2 = "debtor name 2";
			row.BillToAddress = "1 Elvin Skin Boots";
			row.BillToAddress2 = "2 Blades of Aclarity";
			row.BillToCity = "Felwood Forest";
			row.BillToContact = "Stealth Assassin";
			row.YourReference = "SA";
			row.OrderDate = new ZDateTime(2006, 2, 9, 10, 4, 12);
			row.PostingDate = new ZDateTime(2006, 2, 9, 10, 5, 51);
			row.ShipmentDate = new ZDateTime(2006, 2, 8, 13, 38, 29);
			row.PostingDescription = "The Butterfly - Magically Created for use in the war of the Magi...";
			row.PaymentTermsCode = "Perserverance";
			row.DueDate = new ZDateTime(2006, 1, 1, 1, 1, 1);
			row.ShipmentMethodCode = "LJKF64";
			row.ShortCutDimension1Code = "IUHFS98D";
			row.ShortCutDimension2Code = "823DIEA";
			row.CustomerPostingGroup = "Scourge";
			row.SellToCustomerName = "debtor name";
			row.SellToCustomerName2 = "debtor name 2";
			row.SellToAddress = "1 Elvin Skin Boots";
			row.SellToAddress2 = "2 Blades of Aclarity";
			row.SellToCity = "Felwood Forest";
			row.SellToContact = "Stealth Assassin";
			row.BillToPostCode = "90210";
			row.BillToCounty = "US West";
			row.BillToCountryCode = "ZZ";
			row.SellToPostCode = "90210";
			row.SellToCounty = "US West";
			row.SellToCountryCode = "ZZ";
			row.DocumentDate = new ZDateTime(2006, 2, 13, 13, 13, 13);
			row.ExternalDocumentNumber = "985737498546";
			row.VATBusinessPostingGroup = "HKHLKH93827493";
			row.AdjustmentAppliesTo = "S00019884 Description";
			AssertEquals("Document Type", "CreditNote", row.DocumentType);
			AssertEquals("Sell To Customer Number", "DTRR665456", row.SellToCustomerNumber);
			AssertEquals("Invoice Number", "00065456", row.InvoiceNumber);
			AssertEquals("Bill To Customer Number", "DTRR665456", row.BillToCustomerNumber);
			AssertEquals("Bill To Name", "debtor name", row.BillToName);
			AssertEquals("Bill To Name2", "debtor name 2", row.BillToName2);
			AssertEquals("Bill To Address", "1 Elvin Skin Boots", row.BillToAddress);
			AssertEquals("Bill To Address 2", "2 Blades of Aclarity", row.BillToAddress2);
			AssertEquals("Bill To City", "Felwood Forest", row.BillToCity);
			AssertEquals("Bill To Contact", "Stealth Assassin", row.BillToContact);
			AssertEquals("Your Reference", "SA", row.YourReference);
			AssertEquals("Order Date", new ZDateTime(2006, 2, 9), row.OrderDate);
			AssertEquals("Posting Date", new ZDateTime(2006, 2, 9), row.PostingDate);
			AssertEquals("Shipment Date", new ZDateTime(2006, 2, 8), row.ShipmentDate);
			AssertEquals("Posting Description", "The Butterfly - Magically Created for use in the w", row.PostingDescription);
			AssertEquals("Payment Terms Code", "Perservera", row.PaymentTermsCode);
			AssertEquals("Due Date", new ZDateTime(2006, 1, 1), row.DueDate);
			AssertEquals("Shipment Method Code", "LJKF64", row.ShipmentMethodCode);
			AssertEquals("Short Cut Dimension 1 Code", "IUHFS98D", row.ShortCutDimension1Code);
			AssertEquals("Short Cut Dimension 2 Code", "823DIEA", row.ShortCutDimension2Code);
			AssertEquals("Customer Posting Group", "Scourge", row.CustomerPostingGroup);
			AssertEquals("Sell To Customer Number", "DTRR665456", row.SellToCustomerNumber);
			AssertEquals("Sell To Name", "debtor name", row.SellToCustomerName);
			AssertEquals("Sell To Name2", "debtor name 2", row.SellToCustomerName2);
			AssertEquals("Sell To Address", "1 Elvin Skin Boots", row.SellToAddress);
			AssertEquals("Sell To Address 2", "2 Blades of Aclarity", row.SellToAddress2);
			AssertEquals("Sell To City", "Felwood Forest", row.SellToCity);
			AssertEquals("Sell To Contact", "Stealth Assassin", row.SellToContact);
			AssertEquals("Bill To Post Code", "90210", row.SellToPostCode);
			AssertEquals("Bill To County", "US West", row.SellToCounty);
			AssertEquals("Bill To Country Code", "ZZ", row.BillToCountryCode);
			AssertEquals("Sell To Post Code", "90210", row.SellToPostCode);
			AssertEquals("Sell To County", "US West", row.SellToCounty);
			AssertEquals("Sell To Country Code", "ZZ", row.SellToCountryCode);
			AssertEquals("Document Date", new ZDateTime(2006, 2, 13), row.DocumentDate);
			AssertEquals("External Document Number", "985737498546", row.ExternalDocumentNumber);
			AssertEquals("VAT Business Posting Group, last column should be blank space", " ", row.VATBusinessPostingGroup);
			AssertEquals("Adjustment Applies to", "S00019884 Descriptio", row.AdjustmentAppliesTo);
		}
	}
}
