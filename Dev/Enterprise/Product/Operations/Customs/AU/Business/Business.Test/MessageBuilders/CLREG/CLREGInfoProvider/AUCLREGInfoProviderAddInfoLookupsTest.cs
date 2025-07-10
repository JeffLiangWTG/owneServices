using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCLREGInfoProviderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var organisation = Factory.New<OrgHeader>();
			var wrapper = new OrgHeaderWrapper(organisation);
			var lookups = wrapper.CLREGInfoProvider.AddInfoLookups;

			AssertEquals(Factory.GetCachedValue<CMRGenderCodes>(), lookups.GenderList);
			AssertEquals(typeof(RefUNLOCOCollection), lookups.RefUNLOCOs.GetType());
			AssertEquals(typeof(OrgAddressDependentCollection), lookups.Addresses.GetType());
			AssertEquals(typeof(OrgContactDependentCollection), lookups.Contacts.GetType());
			Assert(lookups.ABNNominatedClientTypeList.ContainsOnly("I"));

			AssertEquals(2, lookups.CACTypeList.Count);
			AssertEquals("BA", lookups.CACTypeList[0].Code);
			AssertEquals("PA", lookups.CACTypeList[1].Code);
		}

		public void TestStateLists()
		{
			var organisation = Factory.New<OrgHeader>();
			var wrapper = new OrgHeaderWrapper(organisation);
			var dataProvider = wrapper.CLREGInfoProvider;
			dataProvider.ZA_BsnPort = "AUSYD";
			dataProvider.ZA_PostPort = "AUSYD";
			var lookups = dataProvider.AddInfoLookups;

			Assert("Lookup should contains only AU states", lookups.OA_StateListForBusinessAddress.CodesAsString.Contains("NSW"));
			Assert("Lookup should contains only AU states", lookups.OA_StateListForBusinessAddress.CodesAsString.Contains("NS"));
			Assert("Lookup should contains only AU states", !lookups.OA_StateListForBusinessAddress.CodesAsString.Contains("DC"));
			Assert("Lookup should contains only AU states", lookups.OA_StateListForPostalAddress.CodesAsString.Contains("NSW"));
			Assert("Lookup should contains only AU states", lookups.OA_StateListForPostalAddress.CodesAsString.Contains("NS"));
			Assert("Lookup should contains only AU states", !lookups.OA_StateListForPostalAddress.CodesAsString.Contains("DC"));

			dataProvider.ZA_BsnPort = "USLAX";
			dataProvider.ZA_PostPort = "USLAX";

			Assert("Lookup should contains only US states", lookups.OA_StateListForBusinessAddress.CodesAsString.Contains("DC"));
			Assert("Lookup should contains only US states", lookups.OA_StateListForBusinessAddress.CodesAsString.Contains("TX"));
			Assert("Lookup should contains only US states", !lookups.OA_StateListForBusinessAddress.CodesAsString.Contains("NSW"));

			Assert("Lookup should contains only US states", lookups.OA_StateListForPostalAddress.CodesAsString.Contains("DC"));
			Assert("Lookup should contains only US states", lookups.OA_StateListForPostalAddress.CodesAsString.Contains("TX"));
			Assert("Lookup should contains only US states", !lookups.OA_StateListForPostalAddress.CodesAsString.Contains("NSW"));
		}
	}
}
