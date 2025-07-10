using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Client.WCB.DaimlerChrysler.Testing
{
	public class DecInvoiceLineDataRowTest : TestCase
	{
		public static void TestProperties()
		{
			DecInvoiceLineDataRow row = new DecInvoiceLineDataRow();
			AssertEquals("Row should have 7 fields", 7, row.FieldCount);
			row.RecordType = "D2";
			row.InvoiceOrderNo = "1234/7895462";
			row.CommissionNo = "COM38372";
			row.ChassisNo = "CCN34223344";
			row.Model = "987564";
			row.Colour = "456";
			row.FOBAmount = 6.40m;
			AssertEquals("RecordType", "D2", row.RecordType);
			AssertEquals("InvoiceOrderNo", "1234/7895462", row.InvoiceOrderNo);
			AssertEquals("InvoiceNumber", "1234", row.InvoiceNumber);
			AssertEquals("OrderNumber", "7895462", row.OrderNumber);
			AssertEquals("CommissionNo", "COM38372", row.CommissionNo);
			AssertEquals("ChassisNo", "CCN342", row.ChassisNo);
			AssertEquals("Model", "987564", row.Model);
			AssertEquals("Colour", "456", row.Colour);
			AssertEquals("FOBAmount", 6.40m, row.FOBAmount);
			row.InvoiceOrderNo = "1234";
			AssertEquals("InvoiceNumber", "1234", row.InvoiceNumber);
			AssertEquals("OrderNumber", ZString.Empty, row.OrderNumber);
			row.InvoiceOrderNo = ZString.Empty;
			AssertEquals("InvoiceNumber", ZString.Empty, row.InvoiceNumber);
			AssertEquals("OrderNumber", ZString.Empty, row.OrderNumber);
		}
	}
}
