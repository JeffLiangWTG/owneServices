using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class AUCLREGContactInfoProviderAddInfoLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestLookups()
		{
			var organisation = Factory.New<OrgHeader>();
			var wrapper = new OrgHeaderWrapper(organisation);

			AssertEquals(typeof(RefUNLOCOCollection), wrapper.CLREGInfoProvider.CLREGContactInfoProvider.AddInfoLookups.RefUNLOCOs.GetType());
		}

		public void TestStateLists()
		{
			var organisation = Factory.New<OrgHeader>();
			var wrapper = new OrgHeaderWrapper(organisation);

			var contactDataProvider = wrapper.CLREGInfoProvider.CLREGContactInfoProvider;
			contactDataProvider.ZA_ContPort = "AUSYD";
			contactDataProvider.ZA_ContPostPort = "AUSYD";

			Assert("Lookup should contains only AU states", contactDataProvider.AddInfoLookups.OA_StateListForContactAddress.CodesAsString.Contains("NSW"));
			Assert("Lookup should contains only AU states", contactDataProvider.AddInfoLookups.OA_StateListForContactAddress.CodesAsString.Contains("NS"));
			Assert("Lookup should contains only AU states", !contactDataProvider.AddInfoLookups.OA_StateListForContactAddress.CodesAsString.Contains("DC"));

			Assert("Lookup should contains only AU states", contactDataProvider.AddInfoLookups.OA_StateListForContactPostalAddress.CodesAsString.Contains("NSW"));
			Assert("Lookup should contains only AU states", contactDataProvider.AddInfoLookups.OA_StateListForContactPostalAddress.CodesAsString.Contains("NS"));
			Assert("Lookup should contains only AU states", !contactDataProvider.AddInfoLookups.OA_StateListForContactPostalAddress.CodesAsString.Contains("DC"));

			contactDataProvider.ZA_ContPort = "USLAX";
			contactDataProvider.ZA_ContPostPort = "USLAX";

			Assert("Lookup should contains only US states", contactDataProvider.AddInfoLookups.OA_StateListForContactAddress.CodesAsString.Contains("DC"));
			Assert("Lookup should contains only US states", contactDataProvider.AddInfoLookups.OA_StateListForContactAddress.CodesAsString.Contains("TX"));
			Assert("Lookup should contains only US states", !contactDataProvider.AddInfoLookups.OA_StateListForContactAddress.CodesAsString.Contains("NSW"));

			Assert("Lookup should contains only US states", contactDataProvider.AddInfoLookups.OA_StateListForContactPostalAddress.CodesAsString.Contains("DC"));
			Assert("Lookup should contains only US states", contactDataProvider.AddInfoLookups.OA_StateListForContactPostalAddress.CodesAsString.Contains("TX"));
			Assert("Lookup should contains only US states", !contactDataProvider.AddInfoLookups.OA_StateListForContactPostalAddress.CodesAsString.Contains("NSW"));
		}
	}
}
