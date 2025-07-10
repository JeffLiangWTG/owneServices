using CargoWise.Types;
using Enterprise.Customs.Common;
using Enterprise.Customs.ES.Business.Testing;
using Enterprise.Customs.ES.NCTS.Business.MessageWrappers;
using Enterprise.Customs.EU.NCTS.Business;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	class ArrivalLineWrapperForDepartureTest : WrapperHelperTest<ArrivalLineWrapperForDeparture>
	{
		public void TestGoodsCustomsProcedureCategory3()
		{
			AssertEquals(ZString.Empty, wrapper.GoodsCustomsProcedureCategory3);
		}

		public void TestGoodsCustomsProcedureCategory4()
		{
			AssertEquals(ZString.Empty, wrapper.GoodsCustomsProcedureCategory4);
		}

		public void TestGoodsCustomsProcedureCategory5()
		{
			AssertEquals(ZString.Empty, wrapper.GoodsCustomsProcedureCategory5);
		}

		public void TestTotalGoodValueInEuros()
		{
			goodsItem.BY_MonetaryValue = 23.45m;
			AssertEquals(23.45m, wrapper.TotalGoodValueInEuros);
		}

		public void TestExternalPackages()
		{
			foreach (string tag in ContainerTagsWithEmpty)
			{
				AddContainerForTest(nctsHeader, tag);
			}
			var externalPackages = wrapper.ExternalPackages;

			CombineAssertions(() =>
			{
				AssertEquals("Expected filled ExternalPackages", ContainerTags.Length, externalPackages.NumberOfPackages);
				AssertSame("Cached ExternalPackages", wrapper.ExternalPackages, externalPackages);
			});

			void AddContainerForTest(NctsHeader nctsHeader, ZString containerTag)
			{
				var nctsHeaderContainer = nctsHeader.DepartureHeaderContainers.AddNew();
				nctsHeaderContainer.BC_ContainerNum = containerTag;
				var nonPersistentContainerPivot = goodsItem.ContainersPivots.AddNew();
				nonPersistentContainerPivot.Container = nctsHeaderContainer;
				nonPersistentContainerPivot.ContainerSelected = true;
			}
		}

		public void TestInternalPackagesIsVehicles()
		{
			goodsItem.IsVehicles = true;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			var internalPackages = wrapper.InternalPackages;

			CombineAssertions(() =>
			{
				AssertEquals("Expected filled InternalPackages.Packages with vehicles", 2, internalPackages.Packages.Count);
				AssertSame("Cached InternalPackages", wrapper.InternalPackages, internalPackages);
			});
		}

		public void TestInternalPackagesIsNotVehicles()
		{
			goodsItem.IsVehicles = false;
			goodsItem.Packages.AddNew();
			goodsItem.Packages.AddNew();
			var internalPackages = wrapper.InternalPackages;
			CombineAssertions(() =>
			{
				AssertEquals("Expected filled InternalPackages.Packages with packages", 2, internalPackages.Packages.Count);
				AssertSame("Cached InternalPackages", wrapper.InternalPackages, internalPackages);
			});
		}

		protected override void SetUp()
		{
			base.SetUp();

			nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
			goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			wrapper = new ArrivalLineWrapperForDeparture(goodsItem);
		}

		NctsHeader nctsHeader;
		NctsDepartureCargoDesc goodsItem;
		ArrivalLineWrapperForDeparture wrapper;

		protected override ArrivalLineWrapperForDeparture GetProvider() => wrapper;
	}
}
