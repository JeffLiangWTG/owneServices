using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.STI.Navision.Testing
{
	public class AccLinesFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			AccLinesFlatFileDataRow row = new AccLinesFlatFileDataRow();
			AssertEquals("Row Should have 29 Fields", 29, row.FieldCount);
			row.DocumentType = "Credit Note";
			row.SellToCustomerNumber = "SellToCode";
			row.DocumentNumber = "S00050669";
			row.LineNumber = "30000";
			row.Type = "G/L Account";
			row.GLAccountNumber = "5498654";
			row.LocationCode = "BNE";
			row.PostingGroup = "POSTGROUP";
			row.ShipmentDate = new ZDateTime(2006, 11, 11, 11, 11, 11);
			row.Description = "I Feel Like falling Asleep at the desk, very very very bored!!!!";
			row.UnitOfMeasure = "Kilograms";
			row.Quantity = 1;
			row.UnitPrice = 89475.9235m;
			row.UnitCost = 549.375957m;
			row.GSTPercentage = 23m;
			row.AmountExcludingGST = 100m;
			row.AmountIncludingGST = 110m;
			row.ShortCutDimension1Code = "111111111111";
			row.ShortCutDimension2Code = "222222222222";
			row.CustomerPriceGroup = "Exclamation";
			row.JobNumber = "S000035435";
			row.ReasonCode = "XEX";
			row.GenBusinessPostingGroup = "GENBUS";
			row.GenProdPostingGroup = "GENPROD";
			row.VATBusinessPostingGroup = "VATBUS";
			row.VATProdPostingGroup = "VATPROD";
			row.Currency = "USD";
			row.UnitOfMeasureCode = "KG";
			row.ChargeCode = "GRPES";
			AssertEquals("Document Type", "CreditNote", row.DocumentType);
			AssertEquals("Sell To customer Number", "SellToCode", row.SellToCustomerNumber);
			AssertEquals("Document Number", "S00050669", row.DocumentNumber);
			AssertEquals("Line Number", "30000", row.LineNumber);
			AssertEquals("Type", "G/L Account", row.Type);
			AssertEquals("GL Account Number", "5498654", row.GLAccountNumber);
			AssertEquals("Location Code", "BNE", row.LocationCode);
			AssertEquals("Posting Group", "POSTGROUP", row.PostingGroup);
			AssertEquals("Shipment Date", new ZDateTime(2006, 11, 11), row.ShipmentDate);
			AssertEquals("Description", "I Feel Like falling Asleep at the desk very very v", row.Description);
			AssertEquals("Unit of Measure", "Kilograms", row.UnitOfMeasure);
			AssertEquals("Quantity", 1, row.Quantity);
			AssertEquals("Unit Price", 89475.92m, row.UnitPrice);
			AssertEquals("Unit Cost", 549.38m, row.UnitCost);
			AssertEquals("GST Percentage", 23m, row.GSTPercentage);
			AssertEquals("Amount Excluding GST", 100m, row.AmountExcludingGST);
			AssertEquals("Amount Including GST", 110m, row.AmountIncludingGST);
			AssertEquals("Short Cut Dimension 1 Code", "111111111111", row.ShortCutDimension1Code);
			AssertEquals("Short Cut Dimension 2 Code", "222222222222", row.ShortCutDimension2Code);
			AssertEquals("Customer Price Group", "Exclamatio", row.CustomerPriceGroup);
			AssertEquals("Job Number", "S000035435", row.JobNumber);
			AssertEquals("Reason Code", "XEX", row.ReasonCode);
			AssertEquals("Gen Business Posting Group", "GENBUS", row.GenBusinessPostingGroup);
			AssertEquals("Gen Prod Posting Group", "GENPROD", row.GenProdPostingGroup);
			AssertEquals("VAT Business Posting Group", "VATBUS", row.VATBusinessPostingGroup);
			AssertEquals("VAT Prod Posting Group", "VATPROD", row.VATProdPostingGroup);
			AssertEquals("Currency", "USD", row.Currency);
			AssertEquals("Unit of Measure Code", "KG", row.UnitOfMeasureCode);
			AssertEquals("Charge Code", "GRPES", row.ChargeCode);
		}
	}
}
