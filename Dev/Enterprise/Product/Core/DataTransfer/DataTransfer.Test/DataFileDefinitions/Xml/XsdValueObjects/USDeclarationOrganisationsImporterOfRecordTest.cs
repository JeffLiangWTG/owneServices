using Enterprise.DataTransfer.Xml.Testing;
using NUnit.Framework;
using Xsd = Enterprise.DataTransfer.Xml.XsdVersion1;

namespace Enterprise.DataTransfer.Xml.XsdVersion1.Testing
{
	[TestedType(typeof(Xsd.USDeclarationOrganisationsImporterOfRecord))]
	sealed class USDeclarationOrganisationsImporterOfRecordTest : ValueObjectTestCase
	{
		public void TestIsSpecified()
		{
			Xsd.USDeclarationOrganisationsImporterOfRecord uSDeclarationOrganisationsImporterOfRecord = new Xsd.USDeclarationOrganisationsImporterOfRecord();
			AssertEquals(false, uSDeclarationOrganisationsImporterOfRecord.IsSpecified);

			uSDeclarationOrganisationsImporterOfRecord.Item = "TEST";
			AssertEquals(true, uSDeclarationOrganisationsImporterOfRecord.IsSpecified);

			uSDeclarationOrganisationsImporterOfRecord.Item = "";
			Xsd.Organisation organisation = new Xsd.Organisation();
			organisation.EDICode = "TST";
			uSDeclarationOrganisationsImporterOfRecord.Item = organisation;
			AssertEquals(true, uSDeclarationOrganisationsImporterOfRecord.IsSpecified);

			organisation.EDICode = "";
			AssertEquals(false, uSDeclarationOrganisationsImporterOfRecord.IsSpecified);
		}
	}
}
