using NUnit.Framework;

namespace Enterprise.Client.WCB.Testing
{
	public class WCBExceptionTest : TestCase
	{
		public static void TestConstructor()
		{
			WCBException ex = new WCBException(WCBException.WCBExceptionType.NoInvoiceHeaderFound, "OBL123456\t90130");
			AssertEquals("Invoice Header Key (OBL123456\t90130) Not Found", ex.Message);
			AssertEquals(WCBException.WCBExceptionType.NoInvoiceHeaderFound, ex.Type);
			ex = new WCBException(WCBException.WCBExceptionType.NoPortCodeFound, "EWEFKJHSDF");
			AssertEquals("No Port Code Found in Line (EWEFKJHSDF)", ex.Message);
			AssertEquals(WCBException.WCBExceptionType.NoPortCodeFound, ex.Type);
			ex = new WCBException(WCBException.WCBExceptionType.InvalidFileFormat);
			AssertEquals("Invalid Header and/or Footer Records", ex.Message);
			AssertEquals(WCBException.WCBExceptionType.InvalidFileFormat, ex.Type);
			ex = new WCBException(WCBException.WCBExceptionType.NoInvoiceLinesForHeader, "EWEFKJHSDF");
			AssertEquals("No Invoice Lines For Header (EWEFKJHSDF)", ex.Message);
			AssertEquals(WCBException.WCBExceptionType.NoInvoiceLinesForHeader, ex.Type);
		}
	}
}
