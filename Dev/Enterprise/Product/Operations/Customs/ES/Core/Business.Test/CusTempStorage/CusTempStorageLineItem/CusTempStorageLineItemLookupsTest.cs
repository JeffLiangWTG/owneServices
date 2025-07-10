using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.ES.Business.CusTempStorage.Testing
{
	class CusTempStorageLineItemLookupsTest : BusinessObjectLookupsTestCase
	{
		public void TestWeightUQList()
		{
			var lineItem = Factory.New<CusTempStorageLineItem>();
			var weightUnits = lineItem.Lookups.WeightUQList;
			var list = Factory.GetCachedCodeDescriptionPairList(OLookUpEditType.Weight);

			AssertSame(list, weightUnits);
			Assert(weightUnits.ContainsCode(Core.Constants.Weight.Kilograms));
		}
	}
}
