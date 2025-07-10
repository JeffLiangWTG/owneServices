using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.BE.Business.Testing;
using Enterprise.Customs.EU.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.BE.Business.Declaration.Testing;

sealed class ImportJobDeclarationValidationTest : BusinessObjectValidationTestCase
{
	public void TestCheckJE_CustomsOffice()
	{
		using (ConfigurationTestHelper.TemporarilySetDeclarationConfiguration(Factory, "IsUCC6Core", false))
		{
			var helper = new Universal.Testing.UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "FAC");
			var codeList = helper.CreateCusCodeListWithAttribute(Core.Constants.CountryCodes.Belgium, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.Facilities, "VALID", "desc", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTime, UniversalReferenceConstants.Type, UniversalReferenceConstants.DALocatie);
			helper.CreateCusCodeListAttribute(codeList.PK, UniversalReferenceConstants.SubType, UniversalReferenceConstants.Kantoor);
			Factory.Save();

			var declaration = GetImportDeclaration();

			CombineAssertions(() =>
			{
				declaration.JE_CustomsOffice = "VALID";
				AssertNoMessageError(declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
				declaration.JE_CustomsOffice = "INVALID";
				AssertHasMessageError(declaration.JE_CustomsOfficeInfo, ListValidation.InvalidCodeMessageError);
			});
		}
	}

	public void TestCheckJE_OH_ControllingCustomer()
	{
		const string messageError = "The Guarantee Provider Party requires a Economic Operators Registration and Identification number (EORI), (Edit Organization > Details > Details > Config > Registration Numbers / Codes)";
		var declaration = GetImportDeclaration();
		declaration.JE_OH_ControllingCustomer = ZGuid.Empty;
		var guaranteeProvider = Factory.New<OrgHeader>();

		CombineAssertions(() =>
		{
			AssertNoMessageError("no controlling customer filled in", declaration.JE_OH_ControllingCustomerInfo, messageError);

			declaration.JE_OH_ControllingCustomer = guaranteeProvider.PK;
			AssertHasMessageError("controlling customer filled in, no EORI", declaration.JE_OH_ControllingCustomerInfo, messageError);

			declaration.ControllingCustomer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "", Core.Constants.CountryCodes.Belgium);
			declaration.JE_OH_ControllingCustomer = guaranteeProvider.PK;
			AssertHasMessageError("controlling customer filled in, empty EORI", declaration.JE_OH_ControllingCustomerInfo, messageError);
			declaration.ControllingCustomer.CustomsCodes.RemoveAndDeleteAll();

			declaration.ControllingCustomer.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "FR0123456789", Core.Constants.CountryCodes.France);
			declaration.JE_OH_ControllingCustomer = guaranteeProvider.PK;
			AssertNoMessageError("controlling customer filled in, EORI filled in", declaration.JE_OH_ControllingCustomerInfo, messageError);
		});
	}

	JobDeclaration GetImportDeclaration()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.SetImport();
		return declaration;
	}
}
