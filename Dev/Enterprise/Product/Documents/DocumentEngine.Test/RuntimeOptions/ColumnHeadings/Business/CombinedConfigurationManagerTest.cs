using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.DocumentEngine.RuntimeOptions.Testing
{
	[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
	sealed class CombinedConfigurationManagerTest : ColumnConfigurationManagerAbstractTest<CombinedConfigurationManager>
	{
		public override void TestNotEquals()
		{
			CombinedConfigurationManager a = GetNewSetting(HeadingManager);
			CombinedConfigurationManager b = new CombinedConfigurationManager(HeadingManager, "NotTest");

			AssertNotEquals("Not the same becaue of description", a, b);
		}

		public void TestSort()
		{
			CombinedConfigurationManager first = new CombinedConfigurationManager(HeadingManager, "AAA");
			CombinedConfigurationManager second = new CombinedConfigurationManager(HeadingManager, "BBB");
			List<ColumnConfigurationManager> managers = new List<ColumnConfigurationManager>();

			managers.Add(second);
			managers.Add(first);

			ColumnConfigurationManager[] mangersAsArray = managers.ToArray();
			AssertEquals("Not in order yet", false, mangersAsArray[0].Equals(first));
			AssertEquals("Not in order yet", false, mangersAsArray[1].Equals(second));

			managers.Sort();
			mangersAsArray = managers.ToArray();
			AssertEquals("in order", first, mangersAsArray[0]);
			AssertEquals("in order", second, mangersAsArray[1]);
		}

		public void TestClientAndUniqueDescription()
		{
			var descriptionManager = new CombinedConfigurationManager(HeadingManager, "Test1");
			AssertEquals("Description must be 'Test1'", "Test1", descriptionManager.Description);
			AssertEquals("UniqueDescription must be 'Test1'", "Test1", descriptionManager.UniqueDescription);
			AssertEquals("LinkPK must be empty", ZGuid.Empty, descriptionManager.LinkPK);
			AssertEquals("LinkCode must be empty", "", descriptionManager.LinkCode);
			AssertEquals("CompanyCode must be empty", "", descriptionManager.CompanyCode);
			AssertNull("Company should be null", descriptionManager.Company);
			AssertEquals("CompanyName must be empty", "", descriptionManager.CompanyName);

			var client = Factory.New<OrgHeader>();
			client.OH_Code = "TestClient";
			Factory.Save();
			HeadingManager.SaveToFilterField = "Client";
			var linkDescriptionManager = new CombinedConfigurationManager(HeadingManager, client.PK, client.OH_Code, "", "Test2");
			AssertEquals("Description must be 'TestClient (as Client) - Test2'", "TestClient (as Client) - Test2", linkDescriptionManager.Description);
			AssertEquals("UniqueDescription must be 'Test2'", "Test2", linkDescriptionManager.UniqueDescription);
			AssertEquals("LinkPK can't be empty", client.PK, linkDescriptionManager.LinkPK);
			AssertEquals("LinkCode must be 'TestClient'", "TestClient", linkDescriptionManager.LinkCode);
			AssertEquals("CompanyCode must be empty", "", linkDescriptionManager.CompanyCode);
			AssertNull("Company should be null", linkDescriptionManager.Company);
			AssertEquals("CompanyName must be empty", "", linkDescriptionManager.CompanyName);

			var company = Factory.NewWithValidTestData<GlbCompany>();
			company.GC_Code = "AAA";
			company.GC_Name = "AAA Company";
			Factory.Save();

			var otherCompanyManager = new CombinedConfigurationManager(HeadingManager, client.PK, client.OH_Code, "", "Test3", company.GC_Code);
			AssertEquals("Description must be 'AAA Company(AAA) - Test3'", "AAA Company(AAA) - Test3", otherCompanyManager.Description);
			AssertEquals("UniqueDescription must be 'Test3'", "Test3", otherCompanyManager.UniqueDescription);
			AssertEquals("LinkPK can't be empty", client.PK, otherCompanyManager.LinkPK);
			AssertEquals("LinkCode must be 'TestClient'", "TestClient", otherCompanyManager.LinkCode);
			AssertEquals("CompanyCode must be 'AAA'", "AAA", otherCompanyManager.CompanyCode);
			AssertNotNull("Company should be not null", otherCompanyManager.Company);
			AssertEquals("CompanyName must be 'AAA Company'", "AAA Company", otherCompanyManager.CompanyName);
		}

		protected override string ExpectedToString => "Test";

		protected override CombinedConfigurationManager GetNewSetting(ColumnConfigurationsManager headingManager) => new CombinedConfigurationManager(headingManager, "Test");
	}
}
