using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CusVehicleValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCheckCVH_VehicleIdentificationNumber_Mandatory()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(vehicle.CVH_VehicleIdentificationNumberInfo);
	}

	public void TestCheckCVH_BrandName()
	{
		CreateVehicleBrands();
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(vehicle.CVH_BrandNameInfo, "XXX", "1");
	}

	public void TestCheckCVH_CarType()
	{
		CreateCarTypes();
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(vehicle.CVH_CarTypeInfo, "XXX", "2WD");
	}

	public void TestCheckCVH_Color()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(vehicle.CVH_ColorInfo);
	}

	public void TestCheckCVH_DriveSide()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(vehicle.CVH_DriveSideInfo, "X", "L");
	}

	public void TestCheckCVH_ModelName()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(vehicle.CVH_ModelNameInfo);
	}

	public void TestCheckCVH_ModelYear_Mandatory()
	{
		ValidationTestHelper.AssertYouHaveNotEnteredMessageError(vehicle.CVH_ModelYearInfo);
	}

	public void TestCheckCVH_ModelYear_Format() => CombineAssertions(() =>
	{
		vehicle.CVH_ModelYear = "987";
		AssertHasMessageErrorContaining("987 is invalid for Model Year", vehicle.CVH_ModelYearInfo, "Please enter a valid Model Year.");

		vehicle.CVH_ModelYear = "1987";
		AssertNoMessageErrorContaining("1987 is valid for Model Year", vehicle.CVH_ModelYearInfo, "Please enter a valid Model Year.");

		vehicle.CVH_ModelYear = "2014";
		AssertNoMessageErrorContaining("2014 is valid for Model Year", vehicle.CVH_ModelYearInfo, "Please enter a valid Model Year.");
	});

	public void TestCheckCVH_Payload()
	{
		ValidationTestHelper.AssertValueCannotBeNegativeMessageError(vehicle.CVH_PayloadInfo);
	}

	public void TestCheckCVH_PayloadUQ()
	{
		CreatePayloadUQs();
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(vehicle.CVH_PayloadUQInfo, "XX", "1A");
	}

	public void TestCheckCheckCVH_SpecificationStandard()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(vehicle.CVH_SpecificationStandardInfo, "3", "1");
	}

	protected override void SetUp()
	{
		base.SetUp();
		vehicle = Factory.New<CusVehicle>();
		declaration = Factory.New<JobDeclaration>();
		declaration.JE_ApplicationCode = Core.Constants.CurrencyCodes.UnitedArabEmirates;
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
		vehicle = invoiceLine.Vehicles.AddNew();
	}

	void CreateVehicleBrands()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(AEConstants.RefCusCodeList.CodeTypes.VehicleBrand, AEConstants.RefCusCodeList.CodeTypes.VehicleBrand);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.UnitedArabEmirates, AEConstants.RefCusCodeList.CodeTypes.VehicleBrand, "1", "ACURA", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CurrencyCodes.UnitedArabEmirates);
		Factory.Save();
	}

	void CreateCarTypes()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(AEConstants.RefCusCodeList.CodeTypes.VehicleType, AEConstants.RefCusCodeList.CodeTypes.VehicleType);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.UnitedArabEmirates, AEConstants.RefCusCodeList.CodeTypes.VehicleType, "2WD", "2WD", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CurrencyCodes.UnitedArabEmirates);
		Factory.Save();
	}

	void CreatePayloadUQs()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, "1A", "Drum, steel", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations);
		Factory.Save();
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusVehicle vehicle;
}
