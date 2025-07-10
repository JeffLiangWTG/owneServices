using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.NCTS.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants.Customs.Universal;
using static Enterprise.Customs.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.CH.NCTS.Business.Testing;

[TestedType(typeof(InventoryConsignmentItemPackagingDataProvider))]
sealed class InventoryConsignmentItemPackagingDataProviderTest : BaseArrivalDataProviderTest<InventoryConsignmentItemPackagingDataProvider, NctsHeaderArrivalMessageSendingObject>
{
	protected override InventoryConsignmentItemPackagingDataProvider CreateDataProvider() => InventoryConsignmentItemPackagingDataProvider.NewCollection(ArrivalGoodsItem.Packages).First();

	public void TestNewCollection() => CombineAssertions(() =>
	{
		AssertNull("null", ConsignmentItemPackagingDataProvider.NewCollection(null));

		var package1 = ArrivalGoodsItem.Packages.AddNew();
		var package2 = ArrivalGoodsItem.Packages.AddNew();
		var package3 = ArrivalGoodsItem.Packages.AddNew();
		var package4 = ArrivalGoodsItem.Packages.AddNew();
		package1.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		package2.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		package3.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
		package4.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;

		var providers = InventoryConsignmentItemPackagingDataProvider.NewCollection(ArrivalGoodsItem.Packages);

		AssertEquals("NEW", true, providers.Any(x => x.SequenceNumber == package1.B5_SequenceNumber));
		AssertEquals("MIS", true, providers.Any(x => x.SequenceNumber == package2.B5_SequenceNumber));
		AssertEquals("DIF", true, providers.Any(x => x.SequenceNumber == package3.B5_SequenceNumber));
		AssertEquals("DEC", false, providers.Any(x => x.SequenceNumber == package4.B5_SequenceNumber));
	});

	public void TestProperties() => CombineAssertions(() =>
	{
		var helper = new RefDataTestHelper(Factory);
		var codeList = helper.CreateCodeList(RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, dataGrouping: RefDataGrouping.Codes.UnitedNationsRecommendations);
		var typeOfPackageBulk = codeList.CreateCode("PB").WithAttribute(PackageUnitAttributes.Bulk, PackageUnitAttributes.Bulk);
		Factory.Save();

		Package.B5_UnitType = "PT";
		Package.B5_UnitCount = 12;
		Package.B5_MarksAndNumbers = "m+n";

		AssertEquals("SequenceNumber", 1, DataProvider.SequenceNumber);

		ArrivalGoodsItem.BY_Description = "desc";
		ArrivalGoodsItem.BY_CusC4Number = "cus4";
		ArrivalGoodsItem.BY_HarmonisedTariff = "1000.10";
		ArrivalGoodsItem.BY_GrossWeight = 100;
		ArrivalGoodsItem.BY_NetWeight = 100;
		ArrivalGoodsItem.BY_UnloadedState = NctsUnloadedStateList.Codes.DIF;

		Package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;

		CopyAllToUnloaded();
		AssertEquals("TypeOfPackages -- same", "PT", DataProvider.TypeOfPackages);
		CopyAllToUnloaded(unitType: ZString.Empty);
		AssertEquals("TypeOfPackages -- empty", "PT", DataProvider.TypeOfPackages);
		CopyAllToUnloaded(marksAndnumbers: "xxx");
		AssertEquals("TypeOfPackages -- same, other difference", null, DataProvider.TypeOfPackages);
		CopyAllToUnloaded(unitType: ZString.Empty, marksAndnumbers: "xxx");
		AssertEquals("TypeOfPackages -- empty, other difference", null, DataProvider.TypeOfPackages);
		CopyAllToUnloaded(unitType: "XX");
		AssertEquals("TypeOfPackages -- change", "XX", DataProvider.TypeOfPackages);
		CopyAllToUnloaded(withGoodsItemDifference: true);
		AssertEquals("TypeOfPackages -- with GoodsItem difference", null, DataProvider.TypeOfPackages);

		CopyAllToUnloaded();
		AssertEquals("NumberOfPackages -- same", 12, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(unitCount: ZLong.Zero);
		AssertEquals("NumberOfPackages -- empty", 12, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(marksAndnumbers: "xxx");
		AssertEquals("NumberOfPackages -- same, other difference", null, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(unitCount: ZLong.Zero, marksAndnumbers: "xxx");
		AssertEquals("NumberOfPackages -- empty, other difference", null, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(unitCount: 99);
		AssertEquals("NumberOfPackages -- change", 99, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(withGoodsItemDifference: true);
		AssertEquals("NumberOfPackages -- with GoodsItem difference", null, DataProvider.NumberOfPackages);

		CopyAllToUnloaded(unitType: "PB");
		AssertEquals("NumberOfPackages -- same, diff is bulk", null, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(unitType: "PB", unitCount: ZLong.Zero);
		AssertEquals("NumberOfPackages -- empty, diff is bulk", null, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(unitType: "PB", unitCount: 99);
		AssertEquals("NumberOfPackages -- change, diff is bulk", null, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(unitType: "PB", withGoodsItemDifference: true);
		AssertEquals("NumberOfPackages -- with GoodsItem difference, diff is bulk", null, DataProvider.NumberOfPackages);

		Package.B5_UnitType = "PB";
		CopyAllToUnloaded();
		AssertEquals("NumberOfPackages -- same, bulk", null, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(unitCount: ZLong.Zero);
		AssertEquals("NumberOfPackages -- empty, bulk", null, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(unitCount: 99);
		AssertEquals("NumberOfPackages -- change, bulk", null, DataProvider.NumberOfPackages);
		CopyAllToUnloaded(withGoodsItemDifference: true);
		AssertEquals("NumberOfPackages -- with GoodsItem difference, bulk", null, DataProvider.NumberOfPackages);

		CopyAllToUnloaded();
		AssertEquals("ShippingMarks -- same", "m+n", DataProvider.ShippingMarks);
		CopyAllToUnloaded(marksAndnumbers: ZString.Empty);
		AssertEquals("ShippingMarks -- empty", "m+n", DataProvider.ShippingMarks);
		CopyAllToUnloaded(unitType: "XX");
		AssertEquals("ShippingMarks -- same, other difference", null, DataProvider.ShippingMarks);
		CopyAllToUnloaded(marksAndnumbers: ZString.Empty, unitType: "XX");
		AssertEquals("ShippingMarks -- empty, other difference", null, DataProvider.ShippingMarks);
		CopyAllToUnloaded(marksAndnumbers: "XX");
		AssertEquals("ShippingMarks -- change", "XX", DataProvider.ShippingMarks);
		CopyAllToUnloaded(withGoodsItemDifference: true);
		AssertEquals("ShippingMarks -- with GoodsItem difference", null, DataProvider.ShippingMarks);

		Package.B5_UnitType = "PT";
		Package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		ResetDataProvider();
		AssertEquals("TypeOfPackages -- NEW", "PT", DataProvider.TypeOfPackages);
		AssertEquals("NumberOfPackages -- NEW", 12, DataProvider.NumberOfPackages);
		AssertEquals("ShippingMarks -- NEW", "m+n", DataProvider.ShippingMarks);
		Package.B5_UnitType = "PB";
		ResetDataProvider();
		AssertEquals("NumberOfPackages -- NEW, bulk", null, DataProvider.NumberOfPackages);

		Package.B5_UnitType = "PT";
		Package.B5_TypeOfDifference = NctsUnloadedStateList.Codes.MIS;
		ResetDataProvider();
		AssertNull("TypeOfPackages -- MIS", DataProvider.TypeOfPackages);
		AssertNull("NumberOfPackages -- MIS", DataProvider.NumberOfPackages);
		AssertNull("ShippingMarks -- MIS", DataProvider.ShippingMarks);
		Package.B5_UnitType = "PB";
		ResetDataProvider();
		AssertNull("NumberOfPackages -- MIS, bulk", DataProvider.NumberOfPackages);

		void CopyAllToUnloaded(ZString? unitType = null, ZLong? unitCount = null, ZString? marksAndnumbers = null, bool withGoodsItemDifference = false)
		{
			Package.PackDifference.B5_UnitType = unitType ?? Package.B5_UnitType;
			Package.PackDifference.B5_UnitCount = unitCount ?? Package.B5_UnitCount;
			Package.PackDifference.B5_MarksAndNumbers = marksAndnumbers ?? Package.B5_MarksAndNumbers;
			ArrivalGoodsItem.UnloadedGoodsItem.BY_Description = withGoodsItemDifference ? ArrivalGoodsItem.BY_Description + "X" : ArrivalGoodsItem.BY_Description;
			ResetDataProvider();
		}
	});

	NctsPackage Package => package ?? (package = ArrivalGoodsItem.Packages.AddNew());
	NctsPackage package;
}
