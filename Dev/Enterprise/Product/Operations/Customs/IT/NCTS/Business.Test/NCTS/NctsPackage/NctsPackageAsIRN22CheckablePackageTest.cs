using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.IT.Business.Declaration;
using NUnit.Framework;

namespace Enterprise.Customs.IT.NCTS.Business.Testing;

[TestedType(typeof(NctsPackage))]
sealed class NctsPackageAsIRN22CheckablePackageTest : CusInvPackTest<NctsDepartureCargoDesc>
{
	public void TestGetPreviousPackages()
	{
		var nctsDeparture = Factory.NewDepartureNctsHeader();
		var goodsItemCollection = nctsDeparture.MovementHeader.GoodsItems;

		var package1 = GetNewPackagePivot(goodsItemCollection, 1, "CT", 1, "M1");

		CombineAssertions("Expected previous packages", () =>
		{
			var previousPackages = GetPreviousPackaesAsArray(package1);

			AssertEquals("previous packages number with just one line", 0, previousPackages.Length);
		});

		var package2 = GetNewPackagePivot(goodsItemCollection, 2, "CT", 0, "M1");

		CombineAssertions("Expected previous packages", () =>
		{
			var previousPackages = GetPreviousPackaesAsArray(package2);

			AssertEquals("previous packages number with two lines", 1, previousPackages.Length);
			AssertEquals("Second package", 1, previousPackages[0].UnitCount);
		});

		var package3 = GetNewPackagePivot(goodsItemCollection, 3, "CT", 0, "M1");

		CombineAssertions("Expected previous packages", () =>
		{
			var previousPackages = GetPreviousPackaesAsArray(package3);

			AssertEquals("previous packages number", 2, previousPackages.Length);
			AssertEquals("First package", 0, previousPackages[0].UnitCount);
			AssertEquals("Second package", 1, previousPackages[1].UnitCount);
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

	IRN22CheckablePackage GetNewPackagePivot(NctsDepartureCargoDescCollection goodsItemCollection, ZShort lineNo, ZString unitType, ZLong unitCount, ZString marksAndNumbers) => PackageTestHelper.GetNewPackageInNewItem(goodsItemCollection, lineNo, unitType, unitCount, marksAndNumbers);

	IRN22CheckablePackage[] GetPreviousPackaesAsArray(IRN22CheckablePackage package) => package.GetRelatedEntryPreviousPackages().ToArray();
}
