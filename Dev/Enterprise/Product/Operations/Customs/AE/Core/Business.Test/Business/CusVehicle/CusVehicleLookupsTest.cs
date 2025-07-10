using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.AE.Business.Testing;

sealed class CusVehicleLookupsTest : Customs.Business.Testing.CusVehicleCollectionTestBaseOnly
{
	public void TestVehicleBrandList() => CombineAssertions(() =>
	{
		declaration.JE_ApplicationCode = Core.Constants.CurrencyCodes.UnitedArabEmirates;
		var cached = lookups.VehicleBrandList;
		AssertRefCodeList(Core.Constants.CurrencyCodes.UnitedArabEmirates, AEConstants.RefCusCodeList.CodeTypes.VehicleBrand, cached);
		AssertSame(cached, lookups.VehicleBrandList);
	});

	public void TestCarTypeList() => CombineAssertions(() =>
	{
		declaration.JE_ApplicationCode = Core.Constants.CurrencyCodes.UnitedArabEmirates;
		var cached = lookups.CarTypeList;
		AssertRefCodeList(Core.Constants.CurrencyCodes.UnitedArabEmirates, AEConstants.RefCusCodeList.CodeTypes.VehicleType, cached);
		AssertSame(cached, lookups.CarTypeList);
	});

	public void TestPayloadUQList() => CombineAssertions(() =>
	{
		var cached = lookups.PayloadUQList;
		AssertRefCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.UnitedNationsRecommendations, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.UnitedNationsPackageTypes, cached);
		AssertSame(cached, lookups.PayloadUQList);
	});

	public void TestDriveSideList() => CombineAssertions(() =>
	{
		var cached = lookups.DriveSideList;
		AssertContainsExactElementsInExactOrder(["L", "R", "N"], cached.GetAllCodes());
		AssertSame(cached, lookups.DriveSideList);
	});

	public void TestSpecificationStandardList() => CombineAssertions(() =>
	{
		var cached = lookups.SpecificationStandardList;
		AssertContainsExactElementsInExactOrder(["1", "2"], cached.GetAllCodes());
		AssertSame(cached, lookups.SpecificationStandardList);
	});

	void AssertRefCodeList(string datagGroupingCode, string codeType, ZZRefCusCodeListCombinedCollection codeListCombined)
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var otherCodeType = "TEST";
		helper.CreateNewOrGetExistingDataGrouping(datagGroupingCode);
		helper.CreateNewOrGetExistingCusCodeType(codeType, codeType, datagGroupingCode);
		helper.CreateNewOrGetExistingCusCodeType(otherCodeType, otherCodeType, datagGroupingCode);
		helper.CreateNewOrGetExistingCusCodeList(datagGroupingCode, codeType, "TEST1", "matched",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(datagGroupingCode, codeType, "TEST2", "matched",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(datagGroupingCode, otherCodeType, "TEST3", "Other Code Type",
			ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.SouthAfrica, codeType, "TEST4",
			"South Africa", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CurrencyCodes.SouthAfrica, otherCodeType, "TEST5",
			"South Africa && Other Code Type", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		Factory.Save();

		codeListCombined.Load();
		var codeList = codeListCombined.Select(x => x.ZZD_Code);

		AssertEquals(true, codeList.Contains("TEST1"));
		AssertEquals(true, codeList.Contains("TEST2"));
		AssertEquals(false, codeList.Contains("TEST3"));
		AssertEquals(false, codeList.Contains("TEST4"));
		AssertEquals(false, codeList.Contains("TEST5"));
	}

	protected override void SetUp()
	{
		base.SetUp();
		declaration = Factory.New<JobDeclaration>();
		invoiceHeader = declaration.Invoices.AddNew();
		invoiceLine = (JobComInvoiceLine)invoiceHeader.InvoiceLines.AddNew();
		vehicle = invoiceLine.Vehicles.AddNew();
		lookups = vehicle.Lookups;
	}

	JobDeclaration declaration;
	JobComInvoiceHeader invoiceHeader;
	JobComInvoiceLine invoiceLine;
	CusVehicle vehicle;
	CusVehicleLookups lookups;
}
