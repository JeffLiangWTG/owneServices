using NUnit.Framework;

namespace Enterprise.Client.Wow.ServiceTasks.DeclarationInvoice.Export.Testing
{
	public class DeclarationInvoiceExportFlatFileDataRowBodyTest : TestCase
	{
		public void TestFlatFileDataRowBody()
		{
			var row = new DeclarationInvoiceExportFlatFileDataRowBody();
			row.IndicateBodyRecord = 1;
			row.JobNumber = "S00000001";
			row.InvoiceNumberForLine = "9999";
			row.IndentPONumber = "1234";
			row.MaterialProductCode = "Laptop";
			row.UnitPrice = 1.234m;
			row.Quantity = 3;
			row.PopulateFields();
			var expectedString = "1S000000019999              1234        Laptop            00000001.2340000000003";
			AssertEquals("expectedString:", expectedString, row.ToString());
		}
	}
}
