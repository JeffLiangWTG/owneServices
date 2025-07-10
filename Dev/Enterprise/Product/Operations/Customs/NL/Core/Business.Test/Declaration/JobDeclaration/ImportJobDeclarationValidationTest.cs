using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.EU.Business;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Declaration.Testing;

[TestedType(typeof(ImportJobDeclarationValidation))]
class ImportJobDeclarationValidationTest : JobDeclarationValidationAbstractTest<ImportJobDeclarationValidation>
{
	protected override string MessageType => MessageTypeList.Codes.Import;

	protected override ImportJobDeclarationValidation GetValidation() => new ImportJobDeclarationValidation(jobDeclaration);

	public void TestIntracomReceiverValidation()
	{
		var orgHeaderEU = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderEU, Core.Constants.CountryCodes.Belgium, "123456789");
		orgHeaderEU.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Belgium;
		OrgAddress addressEU = Factory.New<OrgAddress>();
		addressEU.OA_Code = "BBB";
		addressEU.OA_OH = orgHeaderEU.PK;
		addressEU.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;

		var orgHeaderNL = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderNL, Core.Constants.CountryCodes.Netherlands, "123456789");
		orgHeaderNL.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Netherlands;
		OrgAddress addressNL = Factory.New<OrgAddress>();
		addressNL.OA_Code = "AAA";
		addressNL.OA_OH = orgHeaderNL.PK;
		addressNL.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;

		var orgHeaderNonEU = Factory.NewWithValidTestData<OrgHeader>();
		AddCustomsCodeForTest(orgHeaderNonEU, Core.Constants.CountryCodes.SouthAfrica, "123456789");
		orgHeaderNonEU.OH_RL_NKClosestPort = Core.Constants.CountryCodes.SouthAfrica;
		OrgAddress addressZA = Factory.New<OrgAddress>();
		addressZA.OA_Code = "CCC";
		addressZA.OA_OH = orgHeaderNonEU.PK;
		addressZA.OA_RN_NKCountryCode = Core.Constants.CountryCodes.SouthAfrica;

		var orgHeaderEUNoVat = Factory.NewWithValidTestData<OrgHeader>();
		orgHeaderEUNoVat.OH_RL_NKClosestPort = Core.Constants.CountryCodes.Belgium;
		OrgAddress addressEUNoEori = Factory.New<OrgAddress>();
		addressEUNoEori.OA_Code = "DDD";
		addressEUNoEori.OA_OH = orgHeaderEUNoVat.PK;
		addressEUNoEori.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OA_Representative = addressEU.PK;

		const string invalidEORIMessage = "EORI number from representative is required.";
		const string fiscalRepMessage = "The fiscal rep should be based in an EU country.";
		CombineAssertions(() =>
		{
			AssertNoMessageErrors(declaration.JE_OA_RepresentativeInfo);

			declaration.JE_OA_Representative = addressNL.PK;
			AssertNoMessageErrorContaining("In NL - Eori", declaration.JE_OA_RepresentativeInfo, invalidEORIMessage);

			declaration.JE_OA_Representative = addressZA.PK;
			AssertHasMessageErrorContaining("Not In EU", declaration.JE_OA_RepresentativeInfo, fiscalRepMessage);
			AssertNoMessageErrorContaining("Not In EU - Eori", declaration.JE_OA_RepresentativeInfo, invalidEORIMessage);

			declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = "IMP";
			declaration.JE_OA_Representative = addressEUNoEori.PK;
			AssertHasMessageErrorContaining("In EU - No Eori", declaration.JE_OA_RepresentativeInfo, invalidEORIMessage);
		});
	}

	public void TestCustomsOfficeValueNotInListValidation()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_CustomsOffice = "ABC";
		var expectedMessageError = ListValidation.InvalidCodeMessageError;
		AssertHasMessageErrorContaining(declaration.JE_CustomsOfficeInfo, expectedMessageError);

		declaration.JE_CustomsOffice = CustomsOfficesList.Codes.RotterdamHavenKantoorMaasvlakte;
		AssertNoErrorContaining(declaration.JE_CustomsOfficeInfo, expectedMessageError);
	}

	OrgCusCode AddCustomsCodeForTest(OrgHeader organisation, ZString country, ZString customsRegNo, string codeType = null)
	{
		OrgCusCode taxCode = organisation.CustomsCodes.AddNew();
		taxCode.OK_RN_NKCodeCountry = country;
		taxCode.OK_CodeType = codeType ?? OrgCusCode.EuropeanUnionSharedCodeTypes.Eori;
		taxCode.OK_CustomsRegNo = customsRegNo;
		return taxCode;
	}

	public void TestCheckJE_TransportModeInland()
	{
		ValidationTestHelper.AssertInvalidCodeMessageError(jobDeclaration.JE_TransportModeInlandInfo, "AAA", TransportTypeList.Codes.Air);
	}
}
