using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class CusNctsContainerGenPivotTypeDeciderTest : TestCaseWithFactory
	{
		public void TestGetTypeForLoad()
		{
			var pivot = Factory.New<NctsCusInBondContainerPackageGenPivot>();
			var row = ((INeedRow)pivot).Row;
			var typeDecider = new CusNctsContainerGenPivotTypeDecider();
			AssertEquals(typeof(NctsCusInBondContainerPackageGenPivot), typeDecider.GetTypeForLoad(row, Factory));
			pivot.XX_Relation1TableCode = "!@";
			AssertEquals(typeof(GenPivot), typeDecider.GetTypeForLoad(row, Factory));
		}
	}
}

