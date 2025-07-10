using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPackageCollection))]
	sealed class NctsPackageCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestAllowNew()
		{
			CombineAssertions(() =>
			{
				for (int i = 0; i < 99; i++)
				{
					AssertEquals($"Allowed New Package {i}", true, goodsItem.Packages.AllowNew);
					goodsItem.Packages.AddNew();
				}

				AssertEquals("Maximum 100", false, goodsItem.Packages.AllowNew);
			});
		}

		protected override BusinessObjectCollection GetCollectionToTest() => goodsItem.Packages;

		protected override void SetUp()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
		}
		NctsArrivalCargoDesc goodsItem;
	}
}
