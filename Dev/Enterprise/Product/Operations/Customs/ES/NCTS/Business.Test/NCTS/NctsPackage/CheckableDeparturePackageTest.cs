using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Business.Testing
{
	[TestedType(typeof(NctsPackage))]
	class CheckableDeparturePackageTest : CusInvPackTest<NctsDepartureCargoDesc>
	{
		public void TestGetRelatedEntryPackagesEmptyWhenNullGoodsItem()
		{
			var packages = GetPackagesAsArray(Factory.New<NctsPackage>());
			AssertEquals("packages is an empty array", false, packages.Any());
		}

		public void TestGetOtherPackages_Phase4()
		{
			var nctsDeparture = Factory.NewDepartureNctsHeader();
			nctsDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS4;
			var goodsItemCollection = nctsDeparture.MovementHeader.GoodsItems;

			var package1 = GetNewPackagePivot(goodsItemCollection, 1, "CT", 1, "M1");

			CombineAssertions("Expected other packages", () =>
			{
				var otherPackages = GetPackagesAsArray(package1);
				AssertEquals("other packages number with just one line", 1, otherPackages.Length);

				var package2 = GetNewPackagePivot(goodsItemCollection, 2, "CT", 0, "M1");
				otherPackages = GetPackagesAsArray(package2);
				AssertEquals("other packages number with two lines", 2, otherPackages.Length);
				AssertEquals("Second package", 1, otherPackages[0].UnitCount);

				var package3 = GetNewPackagePivot(goodsItemCollection, 3, "CT", 0, "M1");
				otherPackages = GetPackagesAsArray(package3);
				AssertEquals("other packages number", 3, otherPackages.Length);
				AssertEquals("First package", 1, otherPackages[0].UnitCount);
			});
		}

		public void TestGetOtherPackages_Phase5()
		{
			var nctsDeparture = Factory.NewDepartureNctsHeader();
			nctsDeparture.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			var bill = nctsDeparture.Bills.AddNew();
			var goodsItemCollection = bill.GoodsItems;

			var package1 = GetNewPackagePivot(goodsItemCollection, 1, "CT", 1, "M1");

			CombineAssertions("Expected other packages", () =>
			{
				var otherPackages = GetPackagesAsArray(package1);
				AssertEquals("other packages number with just one line", 1, otherPackages.Length);

				var package2 = GetNewPackagePivot(goodsItemCollection, 2, "CT", 0, "M1");
				otherPackages = GetPackagesAsArray(package2);
				AssertEquals("other packages number with two lines", 2, otherPackages.Length);
				AssertEquals("Second package", 1, otherPackages[0].UnitCount);

				var package3 = GetNewPackagePivot(goodsItemCollection, 3, "CT", 0, "M1");
				otherPackages = GetPackagesAsArray(package3);
				AssertEquals("other packages number", 3, otherPackages.Length);
				AssertEquals("First package", 1, otherPackages[0].UnitCount);
			});
		}

		public void TestPackage_UnitType()
		{
			var nctsDeparture = Factory.NewDepartureNctsHeader();
			var goodsItemCollection = nctsDeparture.MovementHeader.GoodsItems;

			var package1 = GetNewPackagePivot(goodsItemCollection, 1, "CT", 0, "M1");

			AssertEquals("When B5_UnitType is CT, Package_UnitType is CT", "CT", (package1).UnitType);
		}

		public void TestPackage_MarksAndNumbers()
		{
			var nctsDeparture = Factory.NewDepartureNctsHeader();
			var goodsItemCollection = nctsDeparture.MovementHeader.GoodsItems;

			var package1 = GetNewPackagePivot(goodsItemCollection, 1, "CT", 0, "M1");

			AssertEquals("When B5_MarksAndNumbers is M1, Package_MarksAndNumbers is M1", "M1", (package1).MarksAndNumbers);
		}

		public void TestPackage_UnitCount()
		{
			var nctsDeparture = Factory.NewDepartureNctsHeader();
			var goodsItemCollection = nctsDeparture.MovementHeader.GoodsItems;

			var package1 = GetNewPackagePivot(goodsItemCollection, 1, "CT", 1, "M1");
			AssertEquals("When B5_UnitCount is 1, Package_UnitCount is 1", 1, (package1).UnitCount);

			var package2 = GetNewPackagePivot(goodsItemCollection, 1, "CT", 0, "M1");
			AssertEquals("When B5_UnitCount is 0, Package_UnitCount is 0", 0, (package2).UnitCount);
		}

		public void TestPackage_UnitCountInfo()
		{
			var nctsDeparture = Factory.NewDepartureNctsHeader();
			var goodsItemCollection = nctsDeparture.MovementHeader.GoodsItems;

			var package1 = GetNewPackagePivot(goodsItemCollection, 1, "CT", 1, "M1");
			AssertEquals("Package_UnitCountInfo of the interface must be B5_UnitCountInfo", package1.UnitCountInfo, (package1).UnitCountInfo);
		}

		protected override NctsDepartureCargoDesc GetNewParent()
		{
			var nctsHeader = Factory.New<NctsHeader>();
			nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
			return nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var goodsItem = GetNewParent();
			return goodsItem.Packages.AddNew();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "A");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"VG", "VG", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.Bulk, "");
			helper.CreateCusCodeListWithAttribute(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes,
				"NE", "NE", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue, Customs.Business.UniversalReferenceConstants.PackageUnitAttributes.BreakBulk, "");
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
			Factory.Save();
		}

		ICheckablePackage GetNewPackagePivot(EU.NCTS.Business.INctsDepartureCargoDescCollection<NctsDepartureCargoDesc> goodsItemCollection, ZShort lineNo, ZString unitType, ZLong unitCount, ZString marksAndNumbers) => PackageTestHelper.GetNewDeparturePackageInNewItem(goodsItemCollection, lineNo, unitType, unitCount, marksAndNumbers);

		ICheckablePackage[] GetPackagesAsArray(ICheckablePackage package) => package.GetRelatedEntryPackages().ToArray();
	}
}
