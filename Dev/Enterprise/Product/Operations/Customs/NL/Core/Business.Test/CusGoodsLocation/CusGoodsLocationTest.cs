using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.NL.Business.Declaration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.NL.Business.Testing;

[TestedType(typeof(CusGoodsLocation))]
sealed class CusGoodsLocationTest : EnterpriseBusinessObjectTestCase
{
	public void TestLookups()
	{
		AssertType<CusGoodsLocationLookups>(cusGoodsLocation.Lookups);
	}

	public void TestUnlocodeList()
	{
		AssertEquals("Unlocode: List", "Lookups.UnlocodeList", cusGoodsLocation.UnlocodeInfo.GetAttribute<ListAttribute>().ListDataSourceMember);
	}

	public void TestDefaultValuesForExport()
	{
		var declaration = Factory.New<BaseJobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var instruction = Factory.New<Declaration.CusEntryInstruction>();
		instruction.CEI_JE = declaration.PK;

		instruction.CEI_OA_Warehouse = ZGuid.Empty;
		OrgHeader supplier = Factory.NewWithValidTestData<OrgHeader>();
		supplier.MainAddress.OA_Address1 = "Address 1";
		supplier.MainAddress.OA_PostCode = "Pincode 1";
		supplier.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Netherlands;
		declaration.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
		var goodsLocation = instruction.GoodsLocation;

		AssertEquals("Qualifier", "T", goodsLocation.CGL_Qualifier);
		AssertEquals("Type", "A", goodsLocation.CGL_Type);
		AssertEquals("Additional Identifier", "Address 1", goodsLocation.CGL_AdditionalIdentifier);
		AssertEquals("Postcode", "Pincode 1", goodsLocation.Address.E2_Postcode);
		AssertEquals("Country Code", "NL", goodsLocation.Address.E2_RN_NKCountryCode);

		var declaration2 = Factory.New<BaseJobDeclaration>();
		declaration2.JE_MessageType = MessageTypeList.Codes.Export;
		var instruction2 = Factory.New<Declaration.CusEntryInstruction>();
		instruction2.CEI_JE = declaration2.PK;

		var warehouse = Factory.NewWithValidTestData<OrgHeader>();
		warehouse.MainAddress.OA_Address1 = "Address 2";
		warehouse.MainAddress.OA_PostCode = "Pincode 2";
		warehouse.MainAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Belgium;
		instruction2.CEI_OA_Warehouse = warehouse.MainAddress.PK;
		declaration2.SupplierDocumentaryAddress.OrganisationPK = supplier.PK;
		var goodsLocation2 = instruction2.GoodsLocation;

		AssertEquals("Additional Identifier", "Address 2", goodsLocation2.CGL_AdditionalIdentifier);
		AssertEquals("Postcode", "Pincode 2", goodsLocation2.Address.E2_Postcode);
		AssertEquals("Country Code", "BE", goodsLocation2.Address.E2_RN_NKCountryCode);
	}

	public void TestSetDefaultsFromAuthorizationIfNeeded()
	{
		var authorizationHeader = Factory.New<CusAuthorisationHeader>();
		authorizationHeader.CPH_Number = "1523625B02";
		authorizationHeader.CPH_Type = Customs.Business.CusAuthorizationHeaderTypeList.Codes.CentralizedClearance;
		authorizationHeader.CPH_RN_NKCountryCode = "NL";
		var authorizationRule = authorizationHeader.CusAuthorisationRules.AddNew();
		authorizationRule.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorizationRule.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		authorizationRule.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		authorizationRule.GoodsLocation.AdditionalIdentifier = "42";
		authorizationRule.GoodsLocation.Address.E2_RN_NKCountryCode = "NL";
		authorizationRule.GoodsLocation.Address.E2_Postcode = "4950 LC";
		authorizationRule.CPR_ValueFrom = "T;B;4950 LC;42;NL";

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = MessageTypeList.Codes.Export;
		var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
		var goodsLocation = entryInstruction.GoodsLocation;

		CombineAssertions("No AuthorizationHeader passed to procedure", () =>
		{
			goodsLocation.SetDefaultsFromAuthorizationIfNeeded(null);
			AssertEquals("Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Type = 'A'", "A", goodsLocation.CGL_Type);
			AssertEquals("Additional Identifier is empty", string.Empty, goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("Address.E2_PostCode is empty", string.Empty, goodsLocation.Address.E2_Postcode);
			AssertEquals("Address.E2_NK_RNCountryCode is empty", string.Empty, goodsLocation.Address.E2_RN_NKCountryCode);
		});

		CombineAssertions("AuthorizationHeader passed to procedure on declaration", () =>
		{
			goodsLocation.SetDefaultsFromAuthorizationIfNeeded(authorizationHeader);
			AssertEquals("Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Type = 'B'", "B", goodsLocation.CGL_Type);
			AssertEquals("Additional Identifier = '42'", "42", goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("Address.E2_PostCode = '4950 LC'", "4950 LC", goodsLocation.Address.E2_Postcode);
			AssertEquals("Address.E2_NK_RNCountryCode = 'NL'", "NL", goodsLocation.Address.E2_RN_NKCountryCode);
		});

		goodsLocation.AdditionalIdentifier = string.Empty;
		goodsLocation.Address.E2_Postcode = string.Empty;
		goodsLocation.Address.E2_RN_NKCountryCode = string.Empty;

		var authorizationRule2 = authorizationHeader.CusAuthorisationRules.AddNew();
		authorizationRule2.CPR_RuleCode = Customs.Business.CusAuthorisationRuleTypeList.Codes.Location;
		authorizationRule2.GoodsLocation.CGL_Qualifier = Customs.Business.CusGoodsLocationQualifierList.Codes.PostcodeAddress;
		authorizationRule2.GoodsLocation.CGL_Type = Customs.Business.CusGoodsLocationTypeList.Codes.AuthorizedPlace;
		authorizationRule2.GoodsLocation.AdditionalIdentifier = "12";
		authorizationRule2.GoodsLocation.Address.E2_RN_NKCountryCode = "NL";
		authorizationRule2.GoodsLocation.Address.E2_Postcode = "4952 AK";
		authorizationRule2.CPR_ValueFrom = "T:B;4952 AK;12;NL";

		CombineAssertions("AuthorizationHeader with multiple locations (rules) passed to procedure on departure declaration", () =>
		{
			goodsLocation.SetDefaultsFromAuthorizationIfNeeded(authorizationHeader);
			AssertEquals("Qualifier = 'T'", "T", goodsLocation.CGL_Qualifier);
			AssertEquals("Type = 'B'", "B", goodsLocation.CGL_Type);
			AssertEquals("Additional Identifier is empty", string.Empty, goodsLocation.CGL_AdditionalIdentifier);
			AssertEquals("Address.E2_PostCode is empty", string.Empty, goodsLocation.Address.E2_Postcode);
			AssertEquals("Address.E2_NK_RNCountryCode is empty", string.Empty, goodsLocation.Address.E2_RN_NKCountryCode);
		});
	}

	public void TestCGL_QualifierReadonly()
	{
		var cusGoodsLocation = Factory.New<CusGoodsLocation>();
		CombineAssertions(() =>
		{
			AssertEquals("Qualifier should not be read only by default", false, cusGoodsLocation.CGL_QualifierInfo.ReadOnly);
			cusGoodsLocation.CGL_ParentTableCode = CusGoodsLocationUseList.Codes.CustomsPermitRule;
			AssertEquals("Qualifier should be read only in authorisation mode", true, cusGoodsLocation.CGL_QualifierInfo.ReadOnly);
		});
	}

	public void TestCGL_TypeReadonly()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Type should not be read only by default", false, cusGoodsLocation.CGL_TypeInfo.ReadOnly);
			cusGoodsLocation.CGL_ParentTableCode = CusGoodsLocationUseList.Codes.CustomsPermitRule;
			AssertEquals("Type should be read only in authorisation mode", true, cusGoodsLocation.CGL_TypeInfo.ReadOnly);
		});
	}

	public void TestContactPersonDataVisible()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Contact Person Data schould be visible by default", true, cusGoodsLocation.ContactPersonDataVisible);
			cusGoodsLocation.CGL_ParentTableCode = CusGoodsLocationUseList.Codes.CustomsPermitRule;
			AssertEquals("Contact Person Data schould not be visible in authorisation mode", false, cusGoodsLocation.ContactPersonDataVisible);
		});
	}

	public void TestAddress()
	{
		AssertType<CusGoodsLocationAddress>(cusGoodsLocation.Address);
	}

	public void TestValidation()
	{
		AssertType<CusGoodsLocationValidation>(cusGoodsLocation.Validation);
	}

	public void TestDisplayTextInAuthorisationMode()
	{
		CombineAssertions(() =>
		{
			AssertEquals("Display Text", "T;A", cusGoodsLocation.DisplayText);

			var cusAuthorisationRule = Factory.New<CusAuthorisationRule>();
			cusGoodsLocation = (CusGoodsLocation)cusAuthorisationRule.GoodsLocation;
			cusGoodsLocation.CGL_ParentTableCode = CusGoodsLocationUseList.Codes.CustomsPermitRule;
			cusGoodsLocation.Address.E2_Postcode = "2940 LL";
			cusGoodsLocation.CGL_AdditionalIdentifier = "59";
			AssertEquals("Display Text", "T;B;2940 LL;59;NL", cusGoodsLocation.DisplayText);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		var declaration = Factory.New<JobDeclaration>();
		var location = declaration.CustomsEntryInstructions.AddNew().GoodsLocation;
		location.CGL_LocationUse = "DEP";
		return location;
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory) => GetNewBusinessObject();

	protected override BusinessObject GetBusinessObjectForFetchForLoad() => GetNewBusinessObject();
	protected override void SetUp()
	{
		base.SetUp();
		cusGoodsLocation = (CusGoodsLocation)GetNewBusinessObject();
	}

	CusGoodsLocation cusGoodsLocation;
}
