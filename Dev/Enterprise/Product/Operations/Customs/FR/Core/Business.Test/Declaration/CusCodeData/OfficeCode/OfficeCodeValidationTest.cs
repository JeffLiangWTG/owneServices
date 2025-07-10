using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.FR.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;

namespace Enterprise.Customs.FR.Business.Declaration.Testing;

class OfficeCodeValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckNoDuplicateCodeDataPairsForOfficeOfDispatch()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		var grouping = helper.CreateNewOrGetExistingDataGrouping(Core.Constants.Customs.Universal.RefDataGrouping.Codes.EuropeanUnionEUN);

		helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.France, parent: grouping);
		helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "CustomsOffice");
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000001", "bureau", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
		helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000002", "desc", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));

		var codeFR000001 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000001", "Central Community Transit Office", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(codeFR000001.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfDispatch);
		var codeFR000002 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000002", "Central Community Transit Office1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(codeFR000002.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfDispatch);
		var codeFR000003 = helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.France, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.CustomsOffice, "FR000001", "Central Community Transit Office2", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
		helper.CreateNewOrGetExistingCusCodeListAttribute(codeFR000003.PK, RefCusCodeListAttributeTypes.Codes.ROLE, EuOfficeCodesTypes.Codes.OfficeOfDischarge);

		Factory.Save();

		var errorMsg = "Office FR000001 is already used with code 'DIS'";
		var declaration = Factory.New<JobDeclaration>().WithFlux(EUJobMessageTypeList.Codes.Import);
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaIE;
		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDispatch, "FR000001");

		var office1 = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDispatch, "FR000002");
		AssertNoMessageError("Unique entries of (Office Of Dispatch + CY_Data) should not trigger validation error for UCC6Import.", office1.CY_DataInfo, errorMsg);

		var office2 = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDispatch, "FR000001");
		AssertHasMessageError("Repeated entries of (Office Of Dispatch + CY_Data) should trigger validation error for UCC6Import.", office2.CY_DataInfo, errorMsg);

		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDischarge, "FR000001");
		var office3 = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDischarge, "FR000001");
		AssertNoMessageError("Repeated entries of other offices having same CY_Data should not trigger validation error for UCC6Import.", office3.CY_DataInfo, errorMsg);

		office1.Delete();
		office2.Delete();
		office3.Delete();
		declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.DeltaG;

		office1 = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDispatch, "FR000002");
		AssertNoMessageError("Unique entries of (Office Of Dispatch + CY_Data) should not trigger validation error for non UCC6Import.", office1.CY_DataInfo, errorMsg);

		office2 = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDispatch, "FR000001");
		AssertNoMessageError("Repeated entries of (Office Of Dispatch + CY_Data) should not trigger validation error for non UCC6Import.", office2.CY_DataInfo, errorMsg);

		declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDischarge, "FR000001");
		office3 = declaration.CustomsOffices.AddNew(EuOfficeCodesTypes.Codes.OfficeOfDischarge, "FR000001");
		AssertNoMessageError("Repeated entries of other offices having same CY_Data should not trigger validation error for non UCC6Import.", office3.CY_DataInfo, errorMsg);
	}
}
