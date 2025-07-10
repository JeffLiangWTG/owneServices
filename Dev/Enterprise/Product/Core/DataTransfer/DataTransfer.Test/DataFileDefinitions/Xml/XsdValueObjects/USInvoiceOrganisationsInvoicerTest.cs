using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USInvoiceOrganisationsInvoicer))]
	sealed class USInvoiceOrganisationsInvoicerTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USInvoiceOrganisationsInvoicer uSInvoiceOrganisationsInvoicer = new Xsd.USInvoiceOrganisationsInvoicer();
			AssertEquals(false, uSInvoiceOrganisationsInvoicer.IsSpecified);

			uSInvoiceOrganisationsInvoicer.Item = "TEST";
			AssertEquals(true, uSInvoiceOrganisationsInvoicer.IsSpecified);

			uSInvoiceOrganisationsInvoicer.Item = "";
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.EDICode = "TST";
			uSInvoiceOrganisationsInvoicer.Item = organisation;
			AssertEquals(true, uSInvoiceOrganisationsInvoicer.IsSpecified);

			organisation.EDICode = "";
			AssertEquals(false, uSInvoiceOrganisationsInvoicer.IsSpecified);
		}
	}
}
