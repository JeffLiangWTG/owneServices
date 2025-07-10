using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USInvoiceLineUltimateConsignee))]
	sealed class USInvoiceLineUltimateConsigneeTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USInvoiceLineUltimateConsignee uSInvoiceLineUltimateConsignee = new Xsd.USInvoiceLineUltimateConsignee();
			AssertEquals(false, uSInvoiceLineUltimateConsignee.IsSpecified);

			uSInvoiceLineUltimateConsignee.Item = "TEST";
			AssertEquals(true, uSInvoiceLineUltimateConsignee.IsSpecified);

			uSInvoiceLineUltimateConsignee.Item = "";
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.EDICode = "TST";
			uSInvoiceLineUltimateConsignee.Item = organisation;
			AssertEquals(true, uSInvoiceLineUltimateConsignee.IsSpecified);

			organisation.EDICode = "";
			AssertEquals(false, uSInvoiceLineUltimateConsignee.IsSpecified);
		}
	}
}
