using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.InvoiceCountryPayload))]
	sealed class InvoiceCountryPayloadTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.InvoiceCountryPayload invoiceCountryPayload = new Xsd.InvoiceCountryPayload();
			AssertEquals(false, invoiceCountryPayload.IsSpecified);

			invoiceCountryPayload.USInvoice.IsSpecified = true;
			AssertEquals(true, invoiceCountryPayload.IsSpecified);

			invoiceCountryPayload.USInvoice.IsSpecified = false;
			AssertEquals(false, invoiceCountryPayload.IsSpecified);
		}
	}
}
