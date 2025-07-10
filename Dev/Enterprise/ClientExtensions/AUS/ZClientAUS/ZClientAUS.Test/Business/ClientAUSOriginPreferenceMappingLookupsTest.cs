using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Client.AUS.Business.Testing
{
	internal class ClientAUSOriginPreferenceMappingLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestOriginList()
		{
			ClientAUSOriginPreferenceMapping testMapping = Factory.New<ClientAUSOriginPreferenceMapping>();
			RefCountryCollection originList = testMapping.OriginList;
			AssertEquals("Origin Collection", typeof(RefCountryCollection), originList.GetType());
			Assert("Origin List should have been loaded with country data", originList.Count > 0);
		}

		public void TestPreferenceRulesList()
		{
			ClientAUSOriginPreferenceMapping testMapping = Factory.New<ClientAUSOriginPreferenceMapping>();
			CodeDescriptionPairList pRTList = testMapping.PreferenceRuleList;
			AssertEquals("Preference Rules Collection", typeof(CodeDescriptionPairList), pRTList.GetType());
		}

		public void TestPreferenceSchemeList()
		{
			ClientAUSOriginPreferenceMapping testMapping = Factory.New<ClientAUSOriginPreferenceMapping>();
			testMapping.T7_RN_NKPreferenceOrigin = "SG";
			CodeDescriptionPairList pSTList = testMapping.PreferenceSchemeList;
			AssertEquals("Preference Scheme Collection", typeof(CodeDescriptionPairList), pSTList.GetType());
		}
	}
}
