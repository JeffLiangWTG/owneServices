using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business.Testing
{
	sealed class BusinessObjectDataSourceProviderTest : TestCaseWithFactory
	{
		public void TestGetProperty()
		{
			OrgContactDependentCollection collection = (OrgContactDependentCollection)Provider.GetProperty("Contacts", OrgContactSchema.Constants.OC_ContactName + " DESC");
			AssertEquals(2, collection.Count);
			AssertEquals("T Mart", collection[0].OC_ContactName);
			AssertEquals("Bob Jane", collection[1].OC_ContactName);

			collection = (OrgContactDependentCollection)Provider.GetProperty("Contacts", OrgContactSchema.Constants.OC_ContactName);
			AssertEquals(2, collection.Count);
			AssertEquals("Bob Jane", collection[0].OC_ContactName);
			AssertEquals("T Mart", collection[1].OC_ContactName);

			collection = (OrgContactDependentCollection)Provider.GetProperty("Contacts", "");
			AssertEquals(2, collection.Count);
		}

		public void TestGetProperty_NotACollection()
		{
			ZString name = (ZString)Provider.GetProperty(OrgHeaderSchema.Constants.OH_FullName, OrgHeaderSchema.Constants.OH_FullName);
			AssertEquals("Some Company", name);
		}

		public void TestGetProperty_ChildObjects()
		{
			RefCountryStatesDependentCollection collection = (RefCountryStatesDependentCollection)Provider.GetProperty("UNLOCO.Country.States", RefCountryStatesSchema.Constants.RW_Code);
			AssertEquals(8, collection.Count);
			AssertEquals("ACT", collection[0].RW_Code);
			AssertEquals("NSW", collection[1].RW_Code);
			AssertEquals("NT", collection[2].RW_Code);
			AssertEquals("QLD", collection[3].RW_Code);
			AssertEquals("SA", collection[4].RW_Code);
			AssertEquals("TAS", collection[5].RW_Code);
			AssertEquals("VIC", collection[6].RW_Code);
			AssertEquals("WA", collection[7].RW_Code);
		}

		BusinessObjectDataSourceProvider Provider
		{
			get
			{
				if (provider == null)
				{
					OrgHeader org = Factory.New<OrgHeader>();
					org.OH_FullName = "Some Company";
					OrgContact contact1 = org.Contacts.AddNew();
					contact1.OC_ContactName = "Bob Jane";
					OrgContact contact2 = org.Contacts.AddNew();
					contact2.OC_ContactName = "T Mart";
					org.OH_RL_NKClosestPort = "AUSYD";

					provider = new BusinessObjectDataSourceProvider(org);
				}
				return provider;
			}
		}

		BusinessObjectDataSourceProvider provider;
	}
}
