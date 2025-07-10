using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.Rohlig.Bellin.Testing
{
	public class BellinFlatFileDataRowTest : TestCase
	{
		public void TestProperties()
		{
			BellinFlatFileDataRow row = new BellinFlatFileDataRow();
			row.InvoiceNumber = "00002345987279CWPRCEOUIPOUP7N0932Q954IWAC98258978POCA98O7A5987AC59APC5N9AC5N4TAP8C9J5495Q9D5JP";
			row.CounterpartNumber = "O10923470320790179349";
			row.InvoiceDate = new ZDateTime(2005, 12, 28, 16, 19, 23);
			row.MaturityDate = new ZDateTime(2005, 12, 29, 4, 4, 54);
			row.Amount = 873.245m;
			row.Currency = "blah";
			row.InvoiceFlag = "Yea";
			row.Comment = "howzat!!";
			row.DeliveryNoteNumber = "Note";
			row.ExternalDocumentNumber = "092374";
			row.BookingDate = ZDateTime.Empty;
			row.Remark = "no remarks";
			AssertEquals("Invoice Number", "00002345987279CWPRCEOUIPOUP7N0932Q954IWAC98258978P", row.InvoiceNumber);
			AssertEquals("CounterpartNumber", "O1092347032079017934", row.CounterpartNumber);
			AssertEquals("Invoice Date", new ZDateTime(2005, 12, 28), row.InvoiceDate);
			AssertEquals("Maturity Date", new ZDateTime(2005, 12, 29), row.MaturityDate);
			AssertEquals("Amount", 873.25m, row.Amount);
			AssertEquals("Currency", "bla", row.Currency);
			AssertEquals("Invoice Flag", "Yea", row.InvoiceFlag);
			AssertEquals("Comment", "howzat!!", row.Comment);
			AssertEquals("DeliveryNoteNumber", "Note", row.DeliveryNoteNumber);
			AssertEquals("ExternalDocumentNumber", "092374", row.ExternalDocumentNumber);
			AssertEquals("Booking Date", ZDateTime.Empty, row.BookingDate);
			AssertEquals("Remark", "no remarks", row.Remark);
		}
	}
}
