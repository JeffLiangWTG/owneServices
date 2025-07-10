using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationOrganisationsNotifyParty))]
	sealed class USDeclarationOrganisationsNotifyPartyTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationOrganisationsNotifyParty uSDeclarationOrganisationsNotifyParty = new Xsd.USDeclarationOrganisationsNotifyParty();
			AssertEquals(false, uSDeclarationOrganisationsNotifyParty.IsSpecified);

			uSDeclarationOrganisationsNotifyParty.Item = "TEST";
			AssertEquals(true, uSDeclarationOrganisationsNotifyParty.IsSpecified);

			uSDeclarationOrganisationsNotifyParty.Item = "";
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.EDICode = "TST";
			uSDeclarationOrganisationsNotifyParty.Item = organisation;
			AssertEquals(true, uSDeclarationOrganisationsNotifyParty.IsSpecified);

			organisation.EDICode = "";
			AssertEquals(false, uSDeclarationOrganisationsNotifyParty.IsSpecified);
		}
	}
}
