using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using NUnit.Framework;
using static Enterprise.Customs.ES.Business.UniversalReferenceConstants;

namespace Enterprise.Customs.ES.NCTS.Business.Testing;

[TestedType(typeof(NctsPackage))]
public class NctsPackageTest : CusInvPackTest<NctsCommonCargoDesc>
{
	public void TestEffectiveUnitType()
	{
		CombineAssertions(() =>
		{
			package = GetArrivalPackage();
			package.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
			package.PackDifference.B5_UnitType = "2B";
			package.B5_UnitType = "1D";

			AssertEquals("Package is DIF", "2B", package.EffectiveUnitType);

			package.UnloadedStatus = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("Package is not DIF", "1D", package.EffectiveUnitType);
		});
	}

	public void TestEffectiveMarksAndNumbers()
	{
		CombineAssertions(() =>
		{
			package = GetArrivalPackage();
			package.UnloadedStatus = NctsUnloadedStateList.Codes.DIF;
			package.PackDifference.B5_MarksAndNumbers = "2B";
			package.PackDifference.B5_PackageID = "P";
			package.PackDifference.B5_Brand = "B";
			package.PackDifference.B5_Model = "M";
			package.B5_MarksAndNumbers = "1D";

			AssertEquals("Package is DIF", "2B", package.EffectiveMarksAndNumbers);

			package.PackDifference.B5_UnitType = "FR";
			AssertEquals("Package is DIF and FR", "P:B:M", package.EffectiveMarksAndNumbers);

			package.UnloadedStatus = NctsUnloadedStateList.Codes.NEW;
			AssertEquals("Package is not DIF", "1D", package.EffectiveMarksAndNumbers);

			package.B5_PackageID = "A";
			package.B5_Brand = "B";
			package.B5_Model = "C";
			package.B5_UnitType = "FR";
			AssertEquals("Package is not DIF and FR", "A:B:C", package.EffectiveMarksAndNumbers);
		});
	}

	public void TestB5_BrandMaxLength()
	{
		AssertEquals("B5_BrandMaxLength is equals to 35", 35, package.B5_BrandInfo.MaxLength);
	}

	public void TestB5_ModelMaxLength()
	{
		AssertEquals("B5_ModelMaxLength is equals to 35", 35, package.B5_ModelInfo.MaxLength);
	}

	public void TestB5_PackageIDCaption()
	{
		var captionResourceString = DataBoundResourceStrings.GetDataForProperty(packagePivot.B5_PackageIDInfo);
		CombineAssertions(() =>
		{
			AssertEquals("Caption", "VIN", captionResourceString.Caption);
			AssertEquals("MediumCaption", "VIN", captionResourceString.MediumCaption);
			AssertEquals("ShortCaption", "VIN", captionResourceString.ShortCaption);
			AssertEquals("Description", "Vehicle VIN", captionResourceString.FullDescription);
		});
	}

	public void TestB5_PackageID_ReadOnly()
	{
		var arrivalPackage = GetArrivalPackage();
		goodsItem.IsVehicles = true;
		package = goodsItem.Packages.AddNew();
		package.B5_UnitType = "BX";
		CombineAssertions(() =>
		{
			AssertEquals("VIN is not ReadOnly when is not Arrival", false, package.B5_PackageIDInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "BX";
			AssertEquals("VIN is ReadOnly when is Arrival and Package type is not FR", true, arrivalPackage.B5_PackageIDInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "FR";
			AssertEquals("VIN is not ReadOnly when is Arrival and Package type is FR", false, arrivalPackage.B5_PackageIDInfo.ReadOnly);

			package = goodsItem.Packages.AddNew();
			AssertEquals("VIN is not ReadOnly when good item is Vehicles", false, package.B5_PackageIDInfo.ReadOnly);

			goodsItem.IsVehicles = false;
			package = goodsItem.Packages.AddNew();
			AssertEquals("VIN is ReadOnly when good item is not Vehicles", true, package.B5_PackageIDInfo.ReadOnly);
		});
	}

	public void TestB5_Brand_ReadOnly()
	{
		var arrivalPackage = GetArrivalPackage();
		goodsItem.IsVehicles = true;
		package = goodsItem.Packages.AddNew();
		package.B5_UnitType = "BX";
		CombineAssertions(() =>
		{
			AssertEquals("Brand is not ReadOnly when is not Arrival", false, package.B5_BrandInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "BX";
			AssertEquals("Brand is ReadOnly when is Arrival and Package type is not FR", true, arrivalPackage.B5_BrandInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "FR";
			AssertEquals("Brand is not ReadOnly when is Arrival and Package type is FR", false, arrivalPackage.B5_BrandInfo.ReadOnly);

			package = goodsItem.Packages.AddNew();
			AssertEquals("Brand is not ReadOnly when good item is Vehicles", false, package.B5_PackageIDInfo.ReadOnly);

			goodsItem.IsVehicles = false;
			package = goodsItem.Packages.AddNew();
			AssertEquals("Brand is ReadOnly when good item is not Vehicles", true, package.B5_PackageIDInfo.ReadOnly);
		});
	}

	public void TestB5_Model_ReadOnly()
	{
		var arrivalPackage = GetArrivalPackage();
		goodsItem.IsVehicles = true;
		package = goodsItem.Packages.AddNew();
		package.B5_UnitType = "BX";
		CombineAssertions(() =>
		{
			AssertEquals("Model is not ReadOnly when is not Arrival", false, package.B5_ModelInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "BX";
			AssertEquals("Model is ReadOnly when is Arrival and Package type is not FR", true, arrivalPackage.B5_ModelInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "FR";
			AssertEquals("Model is not ReadOnly when is Arrival and Package type is FR", false, arrivalPackage.B5_ModelInfo.ReadOnly);

			package = goodsItem.Packages.AddNew();
			AssertEquals("Model is not ReadOnly when good item is Vehicles", false, package.B5_ModelInfo.ReadOnly);

			goodsItem.IsVehicles = false;
			package = goodsItem.Packages.AddNew();
			AssertEquals("Model is ReadOnly when good item is not Vehicles", true, package.B5_ModelInfo.ReadOnly);
		});
	}

	public void TestB5_UnitCount_ReadOnly()
	{
		var arrivalPackage = GetArrivalPackage();
		goodsItem.IsVehicles = false;
		package = goodsItem.Packages.AddNew();
		package.B5_UnitType = "FR";
		CombineAssertions(() =>
		{
			AssertEquals("UnitCount is not ReadOnly when is not Arrival", false, package.B5_UnitCountInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "FR";
			AssertEquals("UnitCount is ReadOnly when is Arrival and Package type is FR", true, arrivalPackage.B5_UnitCountInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "BX";
			AssertEquals("UnitCount is not ReadOnly when is Arrival and Package type is not FR", false, arrivalPackage.B5_UnitCountInfo.ReadOnly);

			package = goodsItem.Packages.AddNew();
			AssertEquals("UnitCount is not ReadOnly when good item is not Vehicles", false, package.B5_UnitCountInfo.ReadOnly);

			goodsItem.IsVehicles = true;
			package = goodsItem.Packages.AddNew();
			AssertEquals("UnitCount is ReadOnly when good item is Vehicles", true, package.B5_UnitCountInfo.ReadOnly);
		});
	}

	public void TestB5_UnitType_ReadOnly()
	{
		CombineAssertions(() =>
		{
			goodsItem.IsVehicles = true;
			package = goodsItem.Packages.AddNew();
			AssertEquals("B5_UnitType is ReadOnly when good item is Vehicles", true, package.B5_UnitTypeInfo.ReadOnly);

			goodsItem.IsVehicles = false;
			package = goodsItem.Packages.AddNew();
			AssertEquals("B5_UnitType is not ReadOnly when good item is not Vehicles", false, package.B5_UnitTypeInfo.ReadOnly);
		});
	}

	public void TestB5_UnitType_DefaultValue()
	{
		CombineAssertions(() =>
		{
			goodsItem.IsVehicles = true;
			package = goodsItem.Packages.AddNew();
			AssertEquals("B5_UnitType Equals to FR when is departure and IsVehicles is true", RefCusCodeList.PackageType.Frame, package.B5_UnitType);

			goodsItem.IsVehicles = false;
			package = goodsItem.Packages.AddNew();
			AssertEquals("B5_UnitType is Empty when is departure and is IsVehicles is false", ZString.Empty, package.B5_UnitType);

			var arrivalPackage = GetArrivalPackage();
			AssertEquals("B5_UnitType is Empty when is arrival", ZString.Empty, arrivalPackage.B5_UnitType);
		});
	}

	public void TestB5_MarksAndNumbers_ReadOnly()
	{
		var arrivalPackage = GetArrivalPackage();
		goodsItem.IsVehicles = false;
		package = goodsItem.Packages.AddNew();
		package.B5_UnitType = "FR";
		CombineAssertions(() =>
		{
			AssertEquals("MarksAndNumbers is not ReadOnly when is not Arrival", false, package.B5_MarksAndNumbersInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "FR";
			AssertEquals("MarksAndNumbers is ReadOnly when is Arrival and Package type is FR", true, arrivalPackage.B5_MarksAndNumbersInfo.ReadOnly);

			arrivalPackage.B5_UnitType = "BX";
			AssertEquals("MarksAndNumbers is not ReadOnly when is Arrival and Package type is not FR", false, arrivalPackage.B5_MarksAndNumbersInfo.ReadOnly);

			package = goodsItem.Packages.AddNew();
			AssertEquals("MarksAndNumbers is not ReadOnly when good item is not Vehicles", false, package.B5_MarksAndNumbersInfo.ReadOnly);

			goodsItem.IsVehicles = true;
			package = goodsItem.Packages.AddNew();
			AssertEquals("MarksAndNumbers is ReadOnly when good item is Vehicles", true, package.B5_MarksAndNumbersInfo.ReadOnly);
		});
	}

	public void TestPackDifferencee_ReadOnly()
	{
		var arrivalPackage = GetArrivalPackage();
		arrivalPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.NEW;
		CombineAssertions(() =>
		{
			AssertEquals("Package is ReadOnly when is Arrival is NEW", false, arrivalPackage.B5_UnitTypeInfo.ReadOnly);

			arrivalPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DIF;
			AssertEquals("Package is ReadOnly when is Arrival not NEW", true, arrivalPackage.B5_UnitTypeInfo.ReadOnly);
			AssertEquals("PackDifference is not ReadOnly", false, arrivalPackage.PackDifference.B5_UnitTypeInfo.ReadOnly);
		});
	}

	public void TestUnitTypeFRPackageQuantity()
	{
		var arrivalPackage = GetArrivalPackage();
		package.B5_UnitCount = 55;
		package.B5_UnitType = "FR";
		CombineAssertions(() =>
		{
			AssertEquals("Unit Count is not 1 when is not Arrival", 55, package.B5_UnitCount);

			arrivalPackage.B5_UnitCount = 55;
			arrivalPackage.B5_UnitType = "FR";
			AssertEquals("Unit Count is 1 when is Arrival and Package type is FR", 1, arrivalPackage.B5_UnitCount);

			arrivalPackage.B5_UnitCount = 55;
			arrivalPackage.B5_UnitType = "BX";
			AssertEquals("Unit Count is not change to 1 when is Arrival and Package type is not FR", 55, arrivalPackage.B5_UnitCount);
		});
	}

	public void TestSetUnitTypeToFRDeleteMarks()
	{
		var arrivalPackage = GetArrivalPackage();
		package.B5_MarksAndNumbers = "Marks";
		package.B5_UnitType = "FR";
		CombineAssertions(() =>
		{
			AssertEquals("Marks is not deleted when is not Arrival", "Marks", package.B5_MarksAndNumbers);

			arrivalPackage.B5_MarksAndNumbers = "Marks";
			arrivalPackage.B5_UnitType = "FR";
			AssertEquals("Marks is deleted when is Arrival and Package type is FR", ZString.Empty, arrivalPackage.B5_MarksAndNumbers);

			arrivalPackage.B5_MarksAndNumbers = "Marks";
			arrivalPackage.B5_UnitType = "BX";
			AssertEquals("Marks is not deleted when is Arrival and Package type is not FR", "Marks", arrivalPackage.B5_MarksAndNumbers);
		});
	}

	public void TestSetUnitTypeNotToFRDeleteVINBrandModel()
	{
		var arrivalPackage = GetArrivalPackage();
		package.B5_PackageID = "Vin";
		package.B5_Brand = "Brand";
		package.B5_Model = "Model";
		package.B5_UnitType = "BX";
		CombineAssertions(() =>
		{
			AssertEquals("VIN is not deleted when is not Arrival", "Vin", package.B5_PackageID);
			AssertEquals("Brand is not deleted when is not Arrival", "Brand", package.B5_Brand);
			AssertEquals("Model is not deleted when is not Arrival", "Model", package.B5_Model);

			arrivalPackage.B5_PackageID = "Vin";
			arrivalPackage.B5_Brand = "Brand";
			arrivalPackage.B5_Model = "Model";
			arrivalPackage.B5_UnitType = "BX";
			AssertEquals("VIN is deleted when is Arrival and Package type is not FR", ZString.Empty, arrivalPackage.B5_PackageID);
			AssertEquals("Brand is deleted when is Arrival and Package type is not FR", ZString.Empty, arrivalPackage.B5_Brand);
			AssertEquals("Model is deleted when is Arrival and Package type is not FR", ZString.Empty, arrivalPackage.B5_Model);

			arrivalPackage.B5_PackageID = "Vin";
			arrivalPackage.B5_Brand = "Brand";
			arrivalPackage.B5_Model = "Model";
			arrivalPackage.B5_UnitType = "FR";
			AssertEquals("VIN are is deleted when is Arrival and Package type is FR", "Vin", arrivalPackage.B5_PackageID);
			AssertEquals("Brand are is deleted when is Arrival and Package type is FR", "Brand", arrivalPackage.B5_Brand);
			AssertEquals("Model are is deleted when is Arrival and Package type is FR", "Model", arrivalPackage.B5_Model);
		});
	}

	public void TestGetShouldPropertiesBeReadOnly()
	{
		GetShouldPropertiesBeReadOnly(ESNCTS5ArrivalCustomsStatusList.Codes.UnloadPermissionGranted);
		GetShouldPropertiesBeReadOnly(ESNCTS5ArrivalCustomsStatusList.Codes.DiscrepancyResolutionNoRelease, true);
		GetShouldPropertiesBeReadOnly(ESNCTS5ArrivalCustomsStatusList.Codes.ClosedFullRelease, true);
	}

	public void GetShouldPropertiesBeReadOnly(ZString customsStatus, bool alwaysReadOnlyForThisStatus = false) => CombineAssertions(() =>
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMovementHeader.BM_Phase = NCTS5ArrivalPhaseList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Failed;
		nctsHeader.ArrivalMovementHeader.BM_CustomsStatus = customsStatus;
		var bill = nctsHeader.Bills.AddNew();
		var arrivalGoodItem = bill.ArrivalGoodsItems.AddNew();
		var arrivalPackage = arrivalGoodItem.Packages.AddNew();
		arrivalPackage.B5_TypeOfDifference = NctsUnloadedStateList.Codes.DEC;
		arrivalPackage.B5_GrossWeight = 20;

		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = true;
		AssertEquals("GrossWeight is not readOnly when BM_NoChangesToReport = true and BM_CustomsStatus = " + customsStatus, false, arrivalPackage.B5_GrossWeightInfo.ReadOnly);
		AssertEquals("TypeOfDifference is readOnly when BM_NoChangesToReport = true and BM_CustomsStatus = " + customsStatus, true, arrivalPackage.B5_TypeOfDifferenceInfo.ReadOnly);
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		AssertEquals("GrossWeight readOnly when BM_NoChangesToReport = false and BM_CustomsStatus = " + customsStatus, false, arrivalPackage.B5_GrossWeightInfo.ReadOnly);
		AssertEquals("TypeOfDifference readOnly when BM_NoChangesToReport = false and BM_CustomsStatus = " + customsStatus, alwaysReadOnlyForThisStatus, arrivalPackage.B5_TypeOfDifferenceInfo.ReadOnly);

		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		AssertEquals("GrossWeight readOnly when BM_NoChangesToReport = false and BM_MessageStatus = SNT and BM_CustomsStatus = " + customsStatus, false, arrivalPackage.B5_GrossWeightInfo.ReadOnly);
		AssertEquals("TypeOfDifference is readOnly when BM_NoChangesToReport = false and BM_MessageStatus = SNT and BM_CustomsStatus = " + customsStatus, true, arrivalPackage.B5_TypeOfDifferenceInfo.ReadOnly);
	});

	public void TestIsUnloadingRemarksReadOnlySpainNullParent()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.BH_ApplicationCode = Common.CusInBondApplicationCodeList.Codes.NCTS5;
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMovementHeader.BM_Phase = NCTS5ArrivalPhaseList.Codes.UnloadingRemarks;
		nctsHeader.ArrivalMovementHeader.BM_MessageStatus = LogicalStatusList.Codes.Sent;
		var bill = nctsHeader.Bills.AddNew();

		var arrivalGoodItem = bill.ArrivalGoodsItems.AddNew();
		var package = Factory.New<NctsPackage>();

		CombineAssertions(() =>
		{
			AssertEquals("If Parent is null, then readonly false", false, package.B5_UnitTypeInfo.ReadOnly);

			package.B5_ParentTableCode = arrivalGoodItem.TablePrefix;
			package.B5_ParentID = arrivalGoodItem.PK;
			AssertEquals("If Parent is not null, then readonly is checked", true, package.B5_UnitTypeInfo.ReadOnly);
		});
	}

	public void TestValidation()
	{
		AssertType<NctsPackagePhase5Validation>(package.Validation);
	}

	public void TestLookups()
	{
		AssertType<NctsPackageLookups>(package.Lookups);
	}

	public void TestContainersPivotsForBindingOnly()
	{
		AssertType<NonPersistentContainerPivotPhase5Collection>(package.ContainersPivotsForBindingOnly);
	}

	NctsPackage GetArrivalPackage()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Arrival);
		nctsHeader.ArrivalMovementHeader.BM_NoChangesToReport = false;
		var bill = nctsHeader.Bills.AddNew();
		var arrivalGoodItem = bill.ArrivalGoodsItems.AddNew();
		return arrivalGoodItem.Packages.AddNew();
	}

	protected override NctsCommonCargoDesc GetNewParent()
	{
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		return nctsHeader.Bills.AddNew().GoodsItems.AddNew();
	}

	protected override BusinessObject GetNewBusinessObject() => package;

	protected override void SetUp()
	{
		base.SetUp();
		var nctsHeader = Factory.New<NctsHeader>();
		nctsHeader.SetMovementType(NctsMovementType.Codes.Departure);
		goodsItem = nctsHeader.Bills.AddNew().GoodsItems.AddNew();
		package = goodsItem.Packages.AddNew();
	}

	NctsDepartureCargoDesc goodsItem;
	NctsPackage package;
}
