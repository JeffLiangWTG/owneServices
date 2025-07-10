using CargoWise.Types;
using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USInvoiceLineCountervailing))]
	sealed class USInvoiceLineCountervailingTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USInvoiceLineCountervailing counterv = new Xsd.USInvoiceLineCountervailing();
			AssertEquals(false, counterv.IsSpecified);

			counterv.CaseNo = "CV456";
			AssertEquals(true, counterv.IsSpecified);

			counterv.CaseNo = ZString.Empty;
			AssertEquals(false, counterv.IsSpecified);
		}
	}
}
