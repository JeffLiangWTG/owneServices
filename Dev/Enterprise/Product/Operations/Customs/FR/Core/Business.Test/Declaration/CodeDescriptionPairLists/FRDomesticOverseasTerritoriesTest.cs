using CargoWise.EntityFramework.Testing;
using CargoWise.Types;

namespace Enterprise.Customs.FR.Business.Declaration.Testing
{
	public class FRDomesticOverseasTerritoriesTest : TestCaseWithFactory
	{
		public void TestGetRegionOrTerritoryOfDestinationCombined()
		{
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CORSE", "METRO" }, FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined("CORSE"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "CONTI", "METRO" }, FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined("CONTI"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "MARTI", "DPDOM", "MGPRE" }, FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined("MARTI"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "GUADE", "DPDOM", "MGPRE" }, FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined("GUADE"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "REUNI", "DPDOM", "MGPRE" }, FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined("REUNI"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "GUYAN", "DPDOM" }, FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined("GUYAN"));
			AssertContainsExactElementsInAnyOrder(new ZString[] { "MAYOT", "DPDOM" }, FRDomesticOverseasTerritories.GetRegionOrTerritoryOfDestinationCombined("MAYOT"));
		}
	}
}
