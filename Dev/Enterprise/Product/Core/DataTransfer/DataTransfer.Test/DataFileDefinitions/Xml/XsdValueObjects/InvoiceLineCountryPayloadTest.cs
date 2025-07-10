using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.InvoiceLineCountryPayload))]
	sealed class InvoiceLineCountryPayloadTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.InvoiceLineCountryPayload invoiceLineCountryPayload = new Xsd.InvoiceLineCountryPayload();
			AssertEquals(false, invoiceLineCountryPayload.IsSpecified);

			invoiceLineCountryPayload.USInvoiceLine.IsSpecified = true;
			AssertEquals(true, invoiceLineCountryPayload.IsSpecified);
		}
	}
}
