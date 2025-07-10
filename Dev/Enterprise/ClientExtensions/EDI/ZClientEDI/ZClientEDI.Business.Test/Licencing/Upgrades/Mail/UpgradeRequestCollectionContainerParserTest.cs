using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	public class UpgradeRequestCollectionContainerParserTest : TestCaseWithFactory
	{
		[ExpectNoExceptions]
		public void TestParse()
		{
			UpgradeRequestCollectionContainerParser parser = new UpgradeRequestCollectionContainerParser(Factory);
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "CDESYD";
			org.Contacts.AddNew();
			org.Contacts[0].OC_ContactName = "Test Contact";
			org.Contacts[0].OC_Email = "Test.Contact@edi.com";

			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = org.PK;
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			org.LicCompany.LC_LE = licEnt.PK;

			UpgradeRequestCollectionContainer upgrader = new UpgradeRequestCollectionContainer(Factory, org);

			parser.Parse(upgrader, "");
		}
	}
}