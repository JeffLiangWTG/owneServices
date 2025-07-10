using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class OrgFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			OrgFlatFileDataRow row = new OrgFlatFileDataRow();
			AssertEquals("Should be 33 Fields", 33, row.FieldCount);
			row.OrgCode = "STISYD";
			row.OrgName = "Strang International Pty. Ltd.";
			row.SearchName = "Strang International";
			row.Name2 = "Someone's Second Name";
			row.Address = "185-189 O'Riordan Street";
			row.Address2 = "Level 2 184 Bourke Road Alexandria";
			row.City = "Mascot";
			row.Contact = "Bill Blogs";
			row.PhoneNumber = "61 2 9669 1099";
			row.GlobalDimension1Code = "blah blah black sheep";
			row.GlobalDimension2Code = "have you any wool?";
			row.CreditLimit = 10000m;
			row.CustomerPostingGroup = "CUST";
			row.CustomerPriceGroup = "CATHOLIC";
			row.PaymentTermsCode = "INV30";
			row.SalesPersonCode = "BAKER";
			row.ShipmentMethodCode = "FOB";
			row.InvoiceDisc = "10006934";
			row.CountryCode = "AU";
			row.Blocked = "All";
			row.PrintStatements = "YES";
			row.BillToCustomerNumber = "10000702";
			row.ApplicationMethod = "MANUAL";
			row.FaxNumber = "61 2 9317 4514";
			row.GenBusinessPostingGroup = "DEFAULT";
			row.PostCode = "2020";
			row.County = "NSW";
			row.Email = "info@sowters.com.au";
			row.HomePage = "www.sowters.com.au";
			row.GSTBusinessPostingGroup = "DOMESTIC";
			row.ResposibilityCentre = "RSPSBLTYCNTRE";
			row.ABN = "26 004 243 887";
			row.ABNDivisionPartNumber = "57646843574";
			AssertEquals("Org Code", "STISYD", row.OrgCode);
			AssertEquals("Org Name", "Strang International Pty. Ltd.", row.OrgName);
			AssertEquals("Search Name", "Strang International", row.SearchName);
			AssertEquals("Name2", "Someones Second Name", row.Name2);
			AssertEquals("Address", "185-189 ORiordan Street", row.Address);
			AssertEquals("Address2", "Level 2 184 Bourke Road Alexan", row.Address2);
			AssertEquals("City", "Mascot", row.City);
			AssertEquals("Contact", "Bill Blogs", row.Contact);
			AssertEquals("Phone Number", "61 2 9669 1099", row.PhoneNumber);
			AssertEquals("Global Dimension 1 Code", "blah blah black shee", row.GlobalDimension1Code);
			AssertEquals("Global Dimension 2 Code", "have you any wool?", row.GlobalDimension2Code);
			AssertEquals("Credit Limit", 10000.00m, row.CreditLimit);
			AssertEquals("Customer Posting Group", "CUST", row.CustomerPostingGroup);
			AssertEquals("Customer Price Group", "CATHOLIC", row.CustomerPriceGroup);
			AssertEquals("Payment Terms Code", "INV30", row.PaymentTermsCode);
			AssertEquals("Sales Person Code", "BAKER", row.SalesPersonCode);
			AssertEquals("Shipment Method Code", "FOB", row.ShipmentMethodCode);
			AssertEquals("Invoice Disc", "10006934", row.InvoiceDisc);
			AssertEquals("Country Code", "AU", row.CountryCode);
			AssertEquals("Blocked", "All", row.Blocked);
			AssertEquals("Print Statements", "YES", row.PrintStatements);
			AssertEquals("BillToCusomterNumber", "10000702", row.BillToCustomerNumber);
			AssertEquals("Application Method", "MANUAL", row.ApplicationMethod);
			AssertEquals("Fax Number", "61 2 9317 4514", row.FaxNumber);
			AssertEquals("GenBusinessPostingGroup", "DEFAULT", row.GenBusinessPostingGroup);
			AssertEquals("Post Code", "2020", row.PostCode);
			AssertEquals("County", "NSW", row.County);
			AssertEquals("Email", "info@sowters.com.au", row.Email);
			AssertEquals("Home Page", "www.sowters.com.au", row.HomePage);
			AssertEquals("GST Business Posting Group", "DOMESTIC", row.GSTBusinessPostingGroup);
			AssertEquals("Responsibility Centre", "RSPSBLTYCN", row.ResposibilityCentre);
			AssertEquals("ABN", "26 004 243 887", row.ABN);
			AssertEquals("ABN Division Part Number, last column should be blank space", " ", row.ABNDivisionPartNumber);
		}
	}
}
