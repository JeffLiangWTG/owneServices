using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationOrganisationsUltimateConsignee))]
	sealed class USDeclarationOrganisationsUltimateConsigneeTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationOrganisationsUltimateConsignee uSDeclarationOrganisationsUltimateConsignee = new Xsd.USDeclarationOrganisationsUltimateConsignee();
			AssertEquals(false, uSDeclarationOrganisationsUltimateConsignee.IsSpecified);

			uSDeclarationOrganisationsUltimateConsignee.Item = "TEST";
			AssertEquals(true, uSDeclarationOrganisationsUltimateConsignee.IsSpecified);

			uSDeclarationOrganisationsUltimateConsignee.Item = "";
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.EDICode = "TST";
			uSDeclarationOrganisationsUltimateConsignee.Item = organisation;
			AssertEquals(true, uSDeclarationOrganisationsUltimateConsignee.IsSpecified);

			organisation.EDICode = "";
			AssertEquals(false, uSDeclarationOrganisationsUltimateConsignee.IsSpecified);
		}
	}
}
