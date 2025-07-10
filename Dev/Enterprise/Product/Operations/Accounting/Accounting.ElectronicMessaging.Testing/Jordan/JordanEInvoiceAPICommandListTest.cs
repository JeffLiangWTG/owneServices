using Enterprise.Messaging.Integration;
using NUnit.Framework;

namespace Enterprise.Accounting.ElectronicMessaging.Jordan.Testing
{
	public class JordanEInvoiceAPICommandListTest : TestCase
	{
		public void TestSubmitTransaction()
		{
			AssertEquals(EInvoiceAPICommandList.Codes.SubmitTransaction, JordanEInvoiceAPICommandList.Codes.SubmitTransaction);
		}
	}
}
