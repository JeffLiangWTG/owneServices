using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty))]
	sealed class USOrganisationSpecificDetailsImporterOfRecordDetailsNotifyPartyTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty uSOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty
				= new Xsd.USOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty();
			AssertEquals(false, uSOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty.IsSpecified);

			uSOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty.Item = "TEST";
			AssertEquals(true, uSOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty.IsSpecified);

			uSOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty.Item = "";
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.EDICode = "TST";
			uSOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty.Item = organisation;
			AssertEquals(true, uSOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty.IsSpecified);

			organisation.EDICode = "";
			AssertEquals(false, uSOrganisationSpecificDetailsImporterOfRecordDetailsNotifyParty.IsSpecified);
		}
	}
}
