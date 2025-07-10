using System;
using System.Linq;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class NctsArrivalInternalPackagesInfoCommonWrapperTest : Customs.Business.Testing.DataProviderTestCase<NctsArrivalInternalPackagesInfoCommonWrapper>
	{
		public void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("NctsMovementDetail", () => new NctsArrivalInternalPackagesInfoCommonWrapper(null));
		}

		public void TestPackages()
		{
			var package1 = goodsItem.Packages.AddNew();
			package1.B5_MarksAndNumbers = InternalPackage1.Marks;
			package1.B5_UnitType = InternalPackage1.Type;
			package1.B5_UnitCount = InternalPackage1.NumberOfPackages;
			var wrappedPackage = wrapper.Packages.Single();

			CombineAssertions(() =>
			{
				AssertEquals("Tag", InternalPackage1.Marks, wrappedPackage.Tag);
				AssertEquals("ElementsType", InternalPackage1.Type, wrappedPackage.ElementsType);
				AssertEquals("NumberOfElements", InternalPackage1.NumberOfPackages, wrappedPackage.NumberOfElements);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
			goodsItem = nctsHeader.ArrivalMovementHeader.GoodsItems.AddNew();
			wrapper = new NctsArrivalInternalPackagesInfoCommonWrapper(goodsItem);
		}

		NctsArrivalAndUnloadingCargoDesc goodsItem;
		NctsArrivalInternalPackagesInfoCommonWrapper wrapper;

		protected override NctsArrivalInternalPackagesInfoCommonWrapper GetProvider() => wrapper;
	}
}
