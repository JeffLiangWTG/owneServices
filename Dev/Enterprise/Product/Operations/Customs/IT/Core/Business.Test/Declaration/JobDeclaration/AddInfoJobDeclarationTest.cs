using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

[TestedType(typeof(JobDeclaration))]
sealed class AddInfoJobDeclarationTest : EU.Business.Declaration.Testing.AddInfoJobDeclarationBOTest
{
	public void TestZG_CTStatusID_ReadOnly()
	{
		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = ZString.Empty;
		Assert("For empty message type expected not readonly", !declaration.ZG_CTStatusIDInfo.ReadOnly);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		Assert("For IMP message type expected readonly", declaration.ZG_CTStatusIDInfo.ReadOnly);

		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Export;
		Assert("For EXP message type expected not readonly", !declaration.ZG_CTStatusIDInfo.ReadOnly);
	}

	public void TestSetZG_AuthorisationNumberDefaultJE_LocationOfGoods()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var authorisation = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: organisation.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisation.PK;
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		AssertEquals("Empty ZG_AuthorisationNumber", ZString.Empty, declaration.JE_LocationOfGoods);

		declaration.ZG_AuthorisationNumber = "1111111";
		AssertEquals("No 'LOC' rules for the selected authorisation", ZString.Empty, declaration.JE_LocationOfGoods);

		var authorisationRule1 = authorisation.CusAuthorisationRules.AddNew();
		authorisationRule1.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Location;
		authorisationRule1.CPR_ValueFrom = "123456A";
		Factory.ClearCachedValue<CodeDescriptionPairList>($"{authorisation.PK}|{declaration.JE_CustomsOffice}");
		declaration.ZG_AuthorisationNumberInfo.ClearValue();
		declaration.ZG_AuthorisationNumber = "1111111";
		AssertEquals("JE_LocationOfGoods is defaulted from the single existing 'LOC' rule", "123456A", declaration.JE_LocationOfGoods);

		declaration.JE_LocationOfGoods = "XXXXXXX";
		declaration.ZG_AuthorisationNumberInfo.ClearValue();
		declaration.ZG_AuthorisationNumber = "1111111";
		AssertEquals("JE_LocationOfGoods is already set, and its value gets overwritten", "123456A", declaration.JE_LocationOfGoods);

		declaration.JE_LocationOfGoods = ZString.Empty;
		var authorisationRule2 = authorisation.CusAuthorisationRules.AddNew();
		authorisationRule2.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Location;
		authorisationRule2.CPR_ValueFrom = "123456B";
		Factory.ClearCachedValue<CodeDescriptionPairList>($"{authorisation.PK}|{declaration.JE_CustomsOffice}");
		declaration.ZG_AuthorisationNumberInfo.ClearValue();
		declaration.ZG_AuthorisationNumber = "1111111";
		AssertEquals("JE_LocationOfGoods is not defaulted as there are 2 'LOC' rules", ZString.Empty, declaration.JE_LocationOfGoods);
	}

	public void TestSetZG_AuthorisationNumberWithEmptyAuthorizationOrLocations()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var authorisation = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: organisation.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		declaration.ZG_AuthorisationNumber = "fakeit";
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.JE_OH_Importer = organisation.PK;

		declaration.JE_LocationQualifier = "AA";
		declaration.JE_SubLocationOfGoods = "IT";
		declaration.GoodsLocationAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
		declaration.JE_LocationOfGoods = "BB";
		declaration.JE_LocationOtherInformation = "OTH";

		declaration.ZG_AuthorisationNumberInfo.ClearValue();
		declaration.ZG_AuthorisationNumber = ZString.Empty;

		CombineAssertions("When authorization is null Goods Locations fields get emptied", () =>
		{
			AssertEquals(ZString.Empty, declaration.JE_LocationQualifier);
			AssertEquals(ZString.Empty, declaration.JE_SubLocationOfGoods);
			AssertEquals(ZGuid.Empty, declaration.GoodsLocationAddress.OrganisationPK);
			AssertEquals(ZString.Empty, declaration.JE_LocationOfGoods);
			AssertEquals(ZString.Empty, declaration.JE_LocationOtherInformation);
		});

		var authorisationRule1 = authorisation.CusAuthorisationRules.AddNew();
		authorisationRule1.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Document;
		authorisationRule1.CPR_ValueFrom = "1234";
		Factory.ClearCachedValue<CodeDescriptionPairList>($"{authorisation.PK}|{declaration.JE_CustomsOffice}");
		declaration.ZG_AuthorisationNumberInfo.ClearValue();

		declaration.JE_LocationQualifier = "AA";
		declaration.JE_SubLocationOfGoods = "IT";
		declaration.GoodsLocationAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
		declaration.JE_LocationOfGoods = "BB";
		declaration.JE_LocationOtherInformation = "OTH";

		declaration.ZG_AuthorisationNumber = "1111111";

		CombineAssertions("When authorization is not null, but there are no locations, Goods Locations fields get emptied", () =>
		{
			AssertEquals("AA", declaration.JE_LocationQualifier);
			AssertEquals(ZString.Empty, declaration.JE_SubLocationOfGoods);
			AssertEquals(ZGuid.Empty, declaration.GoodsLocationAddress.OrganisationPK);
			AssertEquals(ZString.Empty, declaration.JE_LocationOfGoods);
			AssertEquals(ZString.Empty, declaration.JE_LocationOtherInformation);
		});
	}

	protected override BusinessObject GetNewBusinessObject()
	{
		return Factory.New<JobDeclaration>();
	}
}
