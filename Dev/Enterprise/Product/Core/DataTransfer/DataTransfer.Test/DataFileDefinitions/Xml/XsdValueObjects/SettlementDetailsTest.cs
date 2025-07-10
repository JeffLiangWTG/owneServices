using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.SettlementDetails))]
	sealed class SettlementDetailsTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.SettlementDetails settlementDetails = new Xsd.SettlementDetails();
			AssertEquals("Should not be specified by default", false, settlementDetails.IsSpecified);

			settlementDetails.StandardInvoiceTerms = "fuc";
			AssertEquals("Should be specified if there is a address", true, settlementDetails.IsSpecified);
		}
	}
}
