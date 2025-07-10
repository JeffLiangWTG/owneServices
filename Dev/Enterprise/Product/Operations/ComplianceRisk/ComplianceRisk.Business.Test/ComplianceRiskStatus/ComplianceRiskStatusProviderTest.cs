using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ComplianceRisk.Business.Test
{
	public class ComplianceRiskStatusProviderTest : TestCaseWithFactory
	{
		public void TestInterfaceImplementation()
		{
			var dummyBizO = new DummyBizObjThatImplementPartyAndLocationAndCommodityProvider(Factory);

			var complianceRiskStatus = dummyBizO.ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus;
			complianceRiskStatus.COR_OverallRisk = "PSK";
			complianceRiskStatus.COR_PartyRisk = "CLR";
			complianceRiskStatus.COR_LocationRisk = "CLR";
			complianceRiskStatus.COR_CommodityRisk = "PSK";
			Factory.Save();

			var org = Factory.NewWithValidTestData<OrgHeader>();
			var country = Factory.LoadTop1<RefCountry>(new ZQuery(RefCountrySchema.RN_Code, "AU"));

			dummyBizO.AddPartiesForTest(new ScreeningParty(org, "Party", org));
			dummyBizO.AddCountriesForTest(new ScreeningParty(country, "Country", country));
			dummyBizO.AddCommoditiesForTest(new ComplianceCommodity("121212", "AU", "Source", dummyBizO.ParentID, string.Empty, "Packing", "Goods Description"));

			CombineAssertions(() =>
			{
				AssertEquals("CLR", dummyBizO.ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.COR_PartyRisk);
				AssertEquals("CLR", dummyBizO.ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.COR_LocationRisk);
				AssertEquals("PSK", dummyBizO.ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.COR_CommodityRisk);
				AssertEquals("PSK", dummyBizO.ComplianceRiskPlugInBusinessObjectForTest.ComplianceRiskStatus.COR_OverallRisk);
				AssertEquals(1, dummyBizO.Parties.Count());
				var firstParty = dummyBizO.Parties.First() as ScreeningParty;
				AssertEquals(org, firstParty.Header);
				AssertEquals("Party", firstParty.Description);
				AssertEquals(1, dummyBizO.Locations.Count());
				AssertEquals(country, dummyBizO.Locations.First().Country);
				AssertEquals("AU: Country", dummyBizO.Locations.First().ParentsDescription);
				AssertEquals(1, dummyBizO.Commodities.Count());
				AssertEquals("121212", dummyBizO.Commodities.First().HarmonizedCode);
				AssertEquals("AU", dummyBizO.Commodities.First().GroupingOrCountry);
				AssertEquals("Source", dummyBizO.Commodities.First().Source);
				AssertEquals("Packing", dummyBizO.Commodities.First().CommoditySource);
			});
		}
	}
}
