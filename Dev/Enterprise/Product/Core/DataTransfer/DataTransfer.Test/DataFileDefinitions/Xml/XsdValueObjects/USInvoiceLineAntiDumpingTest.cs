using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USInvoiceLineAntiDumping))]
	sealed class USInvoiceLineAntiDumpingTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USInvoiceLineAntiDumping antiDumping = new Xsd.USInvoiceLineAntiDumping();
			AssertEquals(false, antiDumping.IsSpecified);

			antiDumping.CaseNo = "AD456";
			AssertEquals(true, antiDumping.IsSpecified);

			antiDumping.CaseNo = ZString.Empty;
			AssertEquals(false, antiDumping.IsSpecified);
		}
	}
}
