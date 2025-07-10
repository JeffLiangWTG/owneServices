using System;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Client.EDI.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.LicenceKeyBuilder.Business.Test
{
	class UpgradeRequestCollectionContainerHtmlParserTest : TestCaseWithFactory
	{
		public void TestCorrectWrapperAndBizOTypes()
		{
			var parser = new UpgradeRequestCollectionContainerHtmlParserForTesting(Factory);
			AssertEquals(typeof(DocUpgradeRequestCollectionContainer), parser.GetTypeOfWrapperForTesting());
		}

		[ExpectNoExceptions]
		public void TestParse()
		{
			UpgradeRequestCollectionContainerHtmlParser parser = new UpgradeRequestCollectionContainerHtmlParser(Factory);
			EDIOrgHeader org = Factory.New<EDIOrgHeader>();
			org.OH_Code = "CDESYD";
			org.Contacts.AddNew();
			org.Contacts[0].OC_ContactName = "Test Contact";
			org.Contacts[0].OC_Email = "Test.Contact@edi.com";

			LicenceCompany licenceCompany = Factory.NewWithValidTestData<LicenceCompany>();
			licenceCompany.LC_OH = org.PK;
			LicenceEnterprise licEnt = Factory.NewWithValidTestData<LicenceEnterprise>();
			org.LicCompany.LC_LE = licEnt.PK;

			UpgradeRequestCollectionContainerForTesting upgrader = new UpgradeRequestCollectionContainerForTesting(Factory, org);

			parser.Parse(upgrader, "");

			org.OH_FullName = "Organization name with tag <br />?!";
			upgrader.CurrentOrganisationOverrideForTesting = org;
			AssertEquals("Should be html encoded", "Organization name with tag &lt;br /&gt;?!", parser.Parse(upgrader, "(*ClientName*)"));
		}

		#region Implementation

		class UpgradeRequestCollectionContainerHtmlParserForTesting : UpgradeRequestCollectionContainerHtmlParser
		{
			internal UpgradeRequestCollectionContainerHtmlParserForTesting(BusinessObjectFactory factory)
				: base(factory)
			{
			}

			internal Type GetTypeOfWrapperForTesting()
			{
				return TypeOfWrapper;
			}
		}

		class UpgradeRequestCollectionContainerForTesting : UpgradeRequestCollectionContainer
		{
			internal UpgradeRequestCollectionContainerForTesting(BusinessObjectFactory factory, params EDIOrgHeader[] organisationsToUpgrade)
				: base(factory, organisationsToUpgrade)
			{
			}

			internal EDIOrgHeader CurrentOrganisationOverrideForTesting;
			public override EDIOrgHeader CurrentOrganisation
			{
				get { return CurrentOrganisationOverrideForTesting; }
			}
		}

		#endregion
	}
}
