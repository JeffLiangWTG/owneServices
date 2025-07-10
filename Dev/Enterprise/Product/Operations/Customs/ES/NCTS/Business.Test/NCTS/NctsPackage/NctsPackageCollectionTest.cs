using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsPackageCollection<>))]
sealed class NctsPackageCollectionTest : BusinessObjectCollectionTestCase
{
	public void TestAllowNew() => CombineAssertions(() =>
	{
		var movement = nctsHeader.ArrivalMovementHeader;
		movement.BM_NoChangesToReport = false;
		AssertEquals("Prereq: NoChanges equals to UnloadingDifferenceDataReadOnly", expected: false, movement.UnloadingDifferenceDataReadOnly);
		AssertEquals("Allow new when NoChangesToReport is false", expected: true, goodsItem.Packages.AllowNew);

		movement.BM_NoChangesToReport = true;
		AssertEquals("Prereq: NoChanges equals to UnloadingDifferenceDataReadOnly", expected: true, movement.UnloadingDifferenceDataReadOnly);
		AssertEquals("Not Allow new when NoChangesToReport is true", expected: false, goodsItem.Packages.AllowNew);

		movement.BM_NoChangesToReport = false;
		nctsHeader.EffectiveMessageStatus = Common.EU.LogicalStatusList.Codes.Sent;
		AssertEquals("Prereq: NoChanges equals to UnloadingDifferenceDataReadOnly", expected: false, movement.UnloadingDifferenceDataReadOnly);
		AssertEquals("Not Allow new when header is sent", expected: false, goodsItem.Packages.AllowNew);
	});

	public void TestChild_DefaultValue()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Departure);
		NctsDepartureCargoDesc goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		CombineAssertions(() =>
		{
			goodsItem.IsVehicles = true;
			var package = goodsItem.Packages.AddNew();
			AssertEquals("B5_UnitType Equals to FR when is departure and IsVehicles is true", RefCusCodeList.PackageType.Frame, package.B5_UnitType);

			goodsItem.IsVehicles = false;
			package = goodsItem.Packages.AddNew();
			AssertEquals("B5_UnitType is Empty when is departure and is IsVehicles is false", ZString.Empty, package.B5_UnitType);

			var arrivalPackage = this.goodsItem.Packages.AddNew();
			AssertEquals("B5_UnitType is Empty when is arrival", ZString.Empty, arrivalPackage.B5_UnitType);
		});
	}

	protected override BusinessObjectCollection GetCollectionToTest() => goodsItem.Packages;

	protected override void SetUp()
	{
		nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(EU.NCTS.Business.NctsMovementType.Codes.Arrival);
		goodsItem = nctsHeader.Bills.AddNew().ArrivalGoodsItems.AddNew();
	}
	NctsHeader nctsHeader;
	NctsArrivalCargoDesc goodsItem;
}
