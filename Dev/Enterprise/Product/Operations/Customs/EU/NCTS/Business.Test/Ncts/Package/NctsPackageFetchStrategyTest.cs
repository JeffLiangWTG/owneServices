using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Customs.EU.NCTS.Business.Testing
{
	sealed class NctsPackageFetchStrategyTest : TestCaseWithFactory
	{
		public void TestFetchForLoadChildEditableObjects()
		{
			var nctsHeader = Factory.New<NctsHeaderForTest>();
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			var bill = nctsHeader.Bills.AddNew();
			var goodsItem = bill.GoodsItems.AddNew();

			var package1 = goodsItem.Packages.AddNew();
			var package2 = goodsItem.Packages.AddNew();
			var packagePks = new[] { package1.PK, package2.PK };
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			foreach (var pk in packagePks)
			{
				var package = newFactory.Load<NctsPackage>(pk);
				package.FetchStrategy.FetchForLoadChildEditableObjects();
			}

			newFactory.ResetDatabaseLoadCount();

			foreach (var pk in packagePks)
			{
				var package = newFactory.Load<NctsPackage>(pk);
				package.LoadChildEditableObjects();
			}
			AssertEquals("FetchForLoadChildEditableObjects Hints should be added for GenPivot", 1, newFactory.GetTableHitCount(GenPivotSchema.Constants.TableName));
		}
	}
}
