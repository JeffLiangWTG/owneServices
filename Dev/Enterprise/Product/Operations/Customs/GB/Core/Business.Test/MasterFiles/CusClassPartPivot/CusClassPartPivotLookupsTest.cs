using CargoWise.EntityFramework.Testing;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using NUnit.Framework;

namespace Enterprise.Customs.GB.Business.MasterFiles.Testing
{
	[TestedType(typeof(CusClassPartPivotLookups))]
	public class CusClassPartPivotLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestGoodsCategoryList()
		{
			var pivot = Factory.New<CusClassPartPivot>();
			AssertType<GoodsCategoryList>(pivot.Lookups.GoodsCategoryList);
		}
	}
}
