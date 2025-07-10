using CargoWise.Types;
using Enterprise.Customs.ES.Business.Declaration;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.ES.Business.Testing;

public class DeclarationAESConsigneeWrapperTest : WrapperHelperTest<DeclarationAESConsigneeWrapper>
{
	public void TestGetNewDeclarationAESConsigneeWrapper()
	{
		CombineAssertions(() =>
		{
			AssertNull("Header null", GetWrapper(null));

			AssertNotNull("Header not null", GetWrapper(Factory.New<OrgHeader>()));
		});
	}

	public void TestId()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected empty Id", ZString.Empty, wrapper.Id);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.CodeTypes.PassportID, "GB333333333", "GB");
			AssertEquals("Expected PAS Id with country code when no NIF or EORI declared", "GB333333333", wrapper.Id);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.NaturalPersonIndividual;
			AssertEquals("Expected NIF Id", "NIF22222222", wrapper.Id);

			orgHeader.CustomsCodes.AddNew(OrgCusCode.SpainCodeTypes.NIF, "NIF22222222");
			orgHeader.OH_Category = OrgConstants.Category.Government;
			AssertEquals("Expected Country+NIF for non NAT Organizations", "ESNIF22222222", wrapper.Id);

			OrgCusCode eoriCusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "FR");
			AssertEquals("Expected EORI Id with country code", "FR22222222", wrapper.Id);

			eoriCusCode.OK_CustomsRegNo = "ES22222222";
			eoriCusCode.OK_RN_NKCodeCountry = "ES";
			AssertEquals("Expected EORI Id with country code not repeated", "ES22222222", wrapper.Id);

			wrapper = GetWrapper(orgHeader, dontSendImporterId: true);
			AssertEquals("Expected empty Id when dontSendImporterId is true, even when there is a cuscode with correct values to be Id", ZString.Empty, wrapper.Id);
		});
	}

	public void TestName()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Expected filled Name when no id declared (EOR, NIF or PAS)", "Org Name", wrapper.Name);

			OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "ES");
			AssertEquals("Expected empty Name when id declared EOR", ZString.Empty, wrapper.Name);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
			AssertEquals("Expected filled Name when id declared but not EOR nor NIF nor PAS", "Org Name", wrapper.Name);

			cusCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
			AssertEquals("Expected empty Name when id declared NIF", ZString.Empty, wrapper.Name);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
			AssertEquals("Expected filled Name when id declared but not EOR nor NIF nor PAS", "Org Name", wrapper.Name);

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			AssertEquals("Expected empty Name when id declared PAS", ZString.Empty, wrapper.Name);

			wrapper = GetWrapper(orgHeader, dontSendImporterId: true);
			AssertEquals("Expected filled Name when dontSendImporterId is true, even when there is a cuscode with correct values to be Id", "Org Name", wrapper.Name);
		});
	}

	public void TestAddress()
	{
		var helper = new UniversalReferenceTestDataHelper(Factory);
		helper.CreateCusMapType(RefCusMapTypeList.Codes.EUCTY, MapDirectionList.Codes.BTH, "Description", false);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "RS", "XS", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		helper.CreateCusMap(RefCusMapTypeList.Codes.EUCTY, "MQ", "FR", ZDateTime.BrettsBirthday, ZDateTime.Now.AddMonths(2), Core.Constants.CountryCodes.Spain);
		Factory.Save();

		CombineAssertions(() =>
		{
			var address = wrapper.Address;
			AssertNotNull("Expected filled Address when no id declared (EOR, NIF or PAS)", wrapper.Address);
			AssertSame("Cached Address", wrapper.Address, address);
			AssertEquals("Expected filled Address correctly with the OrgAddress given", "Address Other", address.Address);

			OrgCusCode cusCode = orgHeader.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "22222222", "ES");
			wrapper = GetWrapper(orgHeader);
			AssertNull("Expected null Address when id declared EOR", wrapper.Address);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.TraderExciseNumber;
			wrapper = GetWrapper(orgHeader);
			AssertNotNull("Expected filled Address when id declared but not EOR nor NIF nor PAS", wrapper.Address);

			cusCode.OK_CodeType = OrgCusCode.SpainCodeTypes.NIF;
			wrapper = GetWrapper(orgHeader);
			AssertNull("Expected null Address when id declared NIF", wrapper.Address);

			cusCode.OK_CodeType = OrgCusCode.EuropeanUnionSharedCodeTypes.ImportOneStopShopVatRegistration;
			wrapper = GetWrapper(orgHeader);
			AssertNotNull("Expected filled Address when id declared but not EOR nor NIF nor PAS", wrapper.Address);

			cusCode.OK_CodeType = OrgCusCode.CodeTypes.PassportID;
			wrapper = GetWrapper(orgHeader);
			AssertNull("Expected null Address when id declared PAS", wrapper.Address);

			wrapper = GetWrapper(orgHeader, dontSendImporterId: true);
			AssertNotNull("Expected filled Address when dontSendImporterId is true, even when there is a cuscode with correct values to be Id", wrapper.Address);

			orgAddress.OA_RN_NKCountryCode = "RS";
			wrapper = GetWrapper(orgHeader, dontSendImporterId: true);
			AssertEquals("Expected filled Country with Default Territory (XS)", "XS", wrapper.Address.Country);

			orgAddress.OA_RN_NKCountryCode = "ES";
			wrapper = GetWrapper(orgHeader, dontSendImporterId: true);
			AssertEquals("Expected filled Country with given code since there is no Default Territory", "ES", wrapper.Address.Country);

			orgAddress.OA_RN_NKCountryCode = "MQ";
			wrapper = GetWrapper(orgHeader, dontSendImporterId: true);
			AssertEquals("Expected filled Country with Default Territory (FR)", "FR", wrapper.Address.Country);

			orgAddress.OA_Address1 = "Address1";
			wrapper = GetWrapper(orgHeader, dontSendImporterId: true);
			AssertEquals("Expected address filled correctly.", "Address1", wrapper.Address.Address);

			orgAddress.OA_City = "City";
			wrapper = GetWrapper(orgHeader, dontSendImporterId: true);
			AssertEquals("Expected city filled correctly.", "City", wrapper.Address.City);

			orgAddress.OA_PostCode = "PostCode";
			wrapper = GetWrapper(orgHeader, dontSendImporterId: true);
			AssertEquals("Expected PostCode filled correctly.", "PostCode", wrapper.Address.PostCode);
		});
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();

		orgHeader = Factory.New<OrgHeader>();
		orgHeader.OH_FullName = "Org Name";
		orgHeader.OH_Code = "TestOrg";

		var orgAddressMain = orgHeader.MainAddress;
		orgAddressMain.OA_Address1 = "Address Main";

		orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_OH = orgHeader.PK;
		orgAddress.OA_Address1 = "Address Other";

		wrapper = GetWrapper(orgHeader);
	}
	OrgHeader orgHeader;
	OrgAddress orgAddress;
	DeclarationAESConsigneeWrapper wrapper;
	JobDeclaration declaration;

	DeclarationAESConsigneeWrapper GetWrapper(OrgHeader orgHeader, bool dontSendImporterId = false) => DeclarationAESConsigneeWrapper.New(orgHeader, orgAddress, isProvisionalPeriod: false, dontSendImporterId, declaration);

	protected override DeclarationAESConsigneeWrapper GetProvider() => wrapper;
}
