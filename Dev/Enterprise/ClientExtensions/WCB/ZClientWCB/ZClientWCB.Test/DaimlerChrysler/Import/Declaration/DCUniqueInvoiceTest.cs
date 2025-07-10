using NUnit.Framework;

namespace Enterprise.Client.WCB.DaimlerChrysler.Testing
{
	public class DCUniqueInvoiceTest : TestCase
	{
		public static void TestInvoiceHeader()
		{
			DecInvoiceHeaderDataRow header = new DecInvoiceHeaderDataRow();
			DCUniqueInvoice uniqueInvoice = new DCUniqueInvoice(header);
			AssertNotNull(uniqueInvoice.InvoiceHeader);
			AssertEquals("Correct header", header, uniqueInvoice.InvoiceHeader);
			uniqueInvoice.InvoiceHeader = null;
			AssertNull(null, uniqueInvoice.InvoiceHeader);
		}

		public static void TestInvoiceLineCollection()
		{
			DecInvoiceHeaderDataRow header = new DecInvoiceHeaderDataRow();
			DCUniqueInvoice uniqueInvoice = new DCUniqueInvoice(header);
			AssertNotNull(uniqueInvoice.InvoiceLineCollection);
			uniqueInvoice.InvoiceLineCollection = null;
			AssertNull(null, uniqueInvoice.InvoiceLineCollection);
		}
	}
}
