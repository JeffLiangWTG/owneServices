using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.CH.Business;
using Enterprise.Customs.CH.Business.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using CusAuthorizationHeaderTypeList = Enterprise.Customs.CH.Business.CusAuthorizationHeaderTypeList;
using TransportMeansList = Enterprise.Customs.CH.Business.TransportMeansList;

namespace Enterprise.Customs.CH.DeclarationActivation.Business.Testing;

[TestedType(typeof(DeclarationActivationReportValidation))]
sealed class DeclarationActivationReportValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckCER_Type()
	{
		ValidationTestHelper.AssertInvalidCodeOrEmptyMessageError(DeclarationActivationReport.CER_TypeInfo, ["XXX"], [ActivationTypeList.Codes.Edec, ActivationTypeList.Codes.Passar]);
	}

	public void TestCheckNextProcedure()
	{
		RefCusCodeTestHelper.CreateNextProcedureList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(DeclarationActivationReport.NextProcedureInfo, RefCusCodeTestHelper.InvalidNextProcedureCode, RefCusCodeTestHelper.ValidNextProcedureCode);
	}

	public void TestCheckCommunicationLanguage()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(DeclarationActivationReport.CommunicationLanguageInfo, ["XX"], new SwissCustomsLanguageList().GetAllCodesZString());
	}

	public void TestCheckCER_Location()
	{
		var declarant1 = Factory.NewWithValidTestData<OrgHeader>();
		declarant1.OH_Code = "CH123";
		var appliesTo1 = Factory.NewWithValidTestData<OrgHeader>();
		appliesTo1.OH_Code = "APT01";

		var authorization1 = Factory.New<CusAuthorisationHeader>();
		authorization1.CPH_Type = CusAuthorizationHeaderTypeList.Codes.AuthorizedLocationCodeForPassar;
		authorization1.CPH_OH_PermitHolder = declarant1.PK;
		authorization1.CPH_Number = "123";
		authorization1.CPH_PermitDescription = "DESC123";
		authorization1.CPH_OA_AppliesTo = appliesTo1.MainAddress.PK;
		authorization1.CPH_StartDate = ZDate.Today.AddDays(-10);

		GlbCompany.CurrentCompany.GC_OH_OrgProxy = declarant1.PK;
		DeclarationActivationReport.CER_Type = ActivationTypeList.Codes.Passar;
		ValidationTestHelper.AssertInvalidCodeMessageError(DeclarationActivationReport.CER_LocationInfo, "999", "123");
	}

	public void TestCheckCER_TransportMode()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(DeclarationActivationReport.CER_TransportModeInfo, "XXx", TransportTypeList.Codes.Air);
	}

	public void TestCheckCER_TransportType()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(DeclarationActivationReport.CER_TransportTypeInfo, "88", TransportMeansList.Codes.Code40);
	}

	public void TestCheckCER_RN_NKTransportNationality()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(DeclarationActivationReport.CER_RN_NKTransportNationalityInfo, "XX", DeclarationActivationReport.CER_RN_NKTransportNationality);
	}

	public void TestCheckCER_OfficeOfExport()
	{
		RefCusCodeTestHelper.CreateCustomsOfficesList(Factory);
		ValidationTestHelper.AssertInvalidCodeMessageError(DeclarationActivationReport.CER_OfficeOfExportInfo, "56", "CH001251");
	}

	public void TestCheckCER_AdditionalDeclarationType()
	{
		new RefDataTestHelper(Factory).CreateCodeList(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Ensub).CreateCode("3").Save();
		ValidationTestHelper.AssertInvalidCodeMessageError(DeclarationActivationReport.CER_AdditionalDeclarationTypeInfo, "9", "3");
	}

	DeclarationActivationReport DeclarationActivationReport => declarationActivationReport ??= Factory.New<DeclarationActivationHeader>().Report;
	DeclarationActivationReport declarationActivationReport;
}
