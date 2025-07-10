using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using NUnit.Framework;

namespace Enterprise.Customs.FR.Business.NCTS.Testing
{
	[TestedType(typeof(NctsPackage))]
	sealed class NctsPackageTest : CusInvPackTest<NctsDepartureCargoDesc>
	{
		public void TestValidationTypePhase4()
		{
			AssertType<NctsPackagePhase4Validation>(package.Validation);
		}

		public void TestContainersPivotsForBindingOnly()
		{
			AssertType<NonPersistentContainerPivotPhase5Collection>(package.ContainersPivotsForBindingOnly);
		}

		public void TestValidationTypePhase5()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			package = Factory.New<NctsPackage>();
			goodsItem.Packages.Add(package);

			AssertType<NctsPackagePhase5Validation>(package.Validation);
		}

		public void TestHasContainer()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			package = Factory.New<NctsPackage>();
			goodsItem.Packages.Add(package);

			AssertEquals("Prerequisite: the package should have no containers.", 0, package.ContainersPivotsForBindingOnly.Count);
			AssertEquals("HasContainer should return false when there are no containers.", false, package.HasContainer);

			nctsHeader.DepartureHeaderContainers.AddNew();
			AssertEquals("Prerequisite: the package should have a container.", 1, package.ContainersPivotsForBindingOnly.Count);
			AssertEquals("HasContainer should return true when there is at least one container.", true, package.HasContainer);
		}

		public void TestHasSelectedContainer()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
			package = Factory.New<NctsPackage>();
			goodsItem.Packages.Add(package);

			AssertEquals("Prerequisite: the package should have no containers.", 0, package.ContainersPivotsForBindingOnly.Count);
			AssertEquals("HasSelectedContainer should return false when there are no containers.", false, package.HasSelectedContainer);

			nctsHeader.DepartureHeaderContainers.AddNew();
			AssertEquals("Prerequisite: the package should have a container.", 1, package.ContainersPivotsForBindingOnly.Count);
			AssertEquals("HasSelectedContainer should return false when there are no selected containers.", false, package.HasSelectedContainer);

			package.ContainersPivotsForBindingOnly[0].ContainerSelected = true;
			AssertEquals("HasSelectedContainer should return true when there is at least one selected container.", true, package.HasSelectedContainer);
		}

		public void TestNewPackageHasAnEmptyQty()
		{
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			var package = goodsItem.Packages.AddNew();

			AssertEquals(0, package.B5_UnitCount);
		}

		public void TestAllowZeroUnitInValidation()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();

			var package = goodsItem.Packages.AddNew();
			package.B5_UnitCount = 0;
			package.B5_UnitType = "1A";
			package.B5_MarksAndNumbers = "M-1234";

			package.Validation.ValidateB5_UnitCount();
			AssertNoNotifications(package.B5_UnitCountInfo);
		}

		public void TestItemsPackagesAreCustomsCompliant()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			var nctsBill = nctsHeader.Bills.AddNew();
			var (goodsItem1, package1) = AddGoodsItemWithPackage();
			var (goodsItem2, package2) = AddGoodsItemWithPackage();
			var (goodsItem3, package3) = AddGoodsItemWithPackage();

			Assert("None of the packages have B5_UnitCount > 0.", !package1.ItemsPackagesAreCustomsCompliant);

			package1.B5_UnitCount = 4;
			package2.B5_UnitCount = 1;
			Assert("When all packages are not set against 1st GoodsItem, they should have B5_UnitCount > 0.", !package1.ItemsPackagesAreCustomsCompliant);

			package3.B5_UnitCount = 2;
			Assert("All packages have B5_UnitCount > 0.", package1.ItemsPackagesAreCustomsCompliant);

			package2.B5_UnitCount = 0;
			package3.B5_UnitCount = 0;
			Assert("All packages are set against GoodsItem1.", package1.ItemsPackagesAreCustomsCompliant);

			package2.B5_UnitCount = 2;
			package3.B5_UnitCount = 0;
			Assert("GoodsItem 1 does not contain any packages with B5_UnitCount > 0.", !package1.ItemsPackagesAreCustomsCompliant);

			(NctsDepartureCargoDesc, NctsPackage) AddGoodsItemWithPackage()
			{
				var goodsItem = nctsBill.GoodsItems.AddNew();
				var package = goodsItem.Packages.AddNew();
				package.B5_UnitType = "CT";
				package.B5_UnitCount = 0;
				return (goodsItem, package);
			}
		}

		protected override NctsDepartureCargoDesc GetNewParent()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader.MovementHeader.GoodsItems.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();
			var nctsHeader = Factory.NewWithValidTestData<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			nctsHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItem = nctsHeader.MovementHeader.GoodsItems.AddNew();
			package = Factory.New<NctsPackage>();
			goodsItem.Packages.Add(package);
		}
		NctsPackage package;
	}
}
