using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Customs.IT.Business.Declaration.Testing;

internal sealed class JobDeclarationGoodsLocationFieldsDefaulterTest : TestCaseWithFactory
{
	public void TestConstructor()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception when declaration is null", () => new JobDeclarationGoodsLocationFieldsDefaulter(null));
		AssertNoExceptionThrown("If declaration is not null, no exception is thrown", () => new JobDeclarationGoodsLocationFieldsDefaulter(Factory.New<JobDeclaration>()));
	}

	public void TestAssignOfficeOfPresentationToLocationOfGoodsIfAvailable()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_LocationOfGoods = ZString.Empty;
		declaration.JE_CustomsOffice = ZString.Empty;

		var defaulter = new JobDeclarationGoodsLocationFieldsDefaulter(declaration);

		CombineAssertions("If available, office of presentation must be assigned to JE_LocationOfGoods", () =>
		{
			defaulter.AssignOfficeOfPresentationToLocationOfGoodsIfAvailable();
			AssertEquals(ZString.Empty, declaration.JE_LocationOfGoods);

			declaration.JE_CustomsOffice = "IT123456";
			defaulter.AssignOfficeOfPresentationToLocationOfGoodsIfAvailable();
			AssertEquals("IT123456", declaration.JE_LocationOfGoods);
		});
	}

	public void TestEmptyGoodsLocationFields()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_LocationQualifier = "AA";
		declaration.JE_SubLocationOfGoods = "IT";
		declaration.GoodsLocationAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
		declaration.JE_LocationOfGoods = "BB";
		declaration.JE_LocationOtherInformation = "OTH";

		var defaulter = new JobDeclarationGoodsLocationFieldsDefaulter(declaration);
		defaulter.EmptyGoodsLocationFields();

		CombineAssertions("All Goods location fields must be empty", () =>
		{
			AssertEquals(ZString.Empty, declaration.JE_LocationQualifier);
			AssertEquals(ZString.Empty, declaration.JE_SubLocationOfGoods);
			AssertEquals(ZGuid.Empty, declaration.GoodsLocationAddress.OrganisationPK);
			AssertEquals(ZString.Empty, declaration.JE_LocationOfGoods);
			AssertEquals(ZString.Empty, declaration.JE_LocationOtherInformation);
		});
	}

	public void TestEmptyGoodsLocationSubFields()
	{
		var declaration = Factory.New<JobDeclaration>();

		declaration.JE_LocationQualifier = "AA";
		declaration.JE_SubLocationOfGoods = "IT";
		declaration.GoodsLocationAddress.OrganisationPK = Factory.New<OrgHeader>().PK;
		declaration.JE_LocationOfGoods = "BB";
		declaration.JE_LocationOtherInformation = "OTH";

		var defaulter = new JobDeclarationGoodsLocationFieldsDefaulter(declaration);
		defaulter.EmptyGoodsLocationSubFields();

		CombineAssertions("All Goods location fields but JE_LocationQualifier must be empty", () =>
		{
			AssertEquals("AA", declaration.JE_LocationQualifier);
			AssertEquals(ZString.Empty, declaration.JE_SubLocationOfGoods);
			AssertEquals(ZGuid.Empty, declaration.GoodsLocationAddress.OrganisationPK);
			AssertEquals(ZString.Empty, declaration.JE_LocationOfGoods);
			AssertEquals(ZString.Empty, declaration.JE_LocationOtherInformation);
		});
	}

	public void TestDefaultLocationOfGoodsIfNeeded()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		var authorisation = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: organisation.PK, "1111111", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = Common.EU.EUJobMessageTypeList.Codes.Import;
		declaration.ZG_AuthorisationNumber = ZString.Empty;
		declaration.JE_OH_Importer = organisation.PK;
		declaration.ZG_AuthorisationNumber = ZString.Empty;

		var defaulter = new JobDeclarationGoodsLocationFieldsDefaulter(declaration);
		defaulter.DefaultLocationOfGoodsIfNeeded();
		AssertEquals("Empty ZG_AuthorisationNumber", ZString.Empty, declaration.JE_LocationOfGoods);

		declaration.ZG_AuthorisationNumber = "1111111";
		defaulter.DefaultLocationOfGoodsIfNeeded();
		AssertEquals("No 'LOC' rules for the selected authorisation", ZString.Empty, declaration.JE_LocationOfGoods);

		var authorisationRule1 = authorisation.CusAuthorisationRules.AddNew();
		authorisationRule1.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Location;
		authorisationRule1.CPR_ValueFrom = "123456A";
		Factory.ClearCachedValue<CodeDescriptionPairList>($"{authorisation.PK}|{declaration.JE_CustomsOffice}");
		defaulter.DefaultLocationOfGoodsIfNeeded();
		AssertEquals("JE_LocationOfGoods is defaulted from the single existing 'LOC' rule", "123456A", declaration.JE_LocationOfGoods);

		declaration.JE_LocationOfGoods = "XXXXXXX";
		defaulter.DefaultLocationOfGoodsIfNeeded();
		AssertEquals("JE_LocationOfGoods is overwritten", "123456A", declaration.JE_LocationOfGoods);

		declaration.JE_LocationOfGoods = ZString.Empty;
		var authorisationRule2 = authorisation.CusAuthorisationRules.AddNew();
		authorisationRule2.CPR_RuleCode = ITCusAuthorisationRuleTypeList.Codes.Location;
		authorisationRule2.CPR_ValueFrom = "123456B";
		Factory.ClearCachedValue<CodeDescriptionPairList>($"{authorisation.PK}|{declaration.JE_CustomsOffice}");
		defaulter.DefaultLocationOfGoodsIfNeeded();
		AssertEquals("JE_LocationOfGoods is not defaulted as there are 2 'LOC' rules", ZString.Empty, declaration.JE_LocationOfGoods);
	}

	public void TestDefaultLocationOfGoodsIfNeededWithEmptyAuthorizationOrLocations()
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

		var defaulter = new JobDeclarationGoodsLocationFieldsDefaulter(declaration);
		defaulter.DefaultLocationOfGoodsIfNeeded();

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

		defaulter.DefaultLocationOfGoodsIfNeeded();

		CombineAssertions("When authorization is not null, but there are no locations, Goods Locations fields get emptied", () =>
		{
			AssertEquals(ZString.Empty, declaration.JE_LocationQualifier);
			AssertEquals(ZString.Empty, declaration.JE_SubLocationOfGoods);
			AssertEquals(ZGuid.Empty, declaration.GoodsLocationAddress.OrganisationPK);
			AssertEquals(ZString.Empty, declaration.JE_LocationOfGoods);
			AssertEquals(ZString.Empty, declaration.JE_LocationOtherInformation);
		});
	}

	public void TestSetDefaultLocationQualifierToLB_WithAuthorizationType()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		SetupAuthorisationsForOrg(organisation);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = organisation.PK;

		declaration.ZG_AuthorisationNumber = "1111CWP";
		AssertEquals("When AuthorizationType=CWP, LocationQualifier", "LB", declaration.JE_LocationQualifier);

		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.ZG_AuthorisationNumber = "1111CW1";
		AssertEquals("When AuthorizationType=CW1, LocationQualifier", "LB", declaration.JE_LocationQualifier);

		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.ZG_AuthorisationNumber = "1111CW2";
		AssertEquals("When AuthorizationType=CW2, LocationQualifier", "LB", declaration.JE_LocationQualifier);

		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.ZG_AuthorisationNumber = "1111ALI";
		AssertNotEquals("When AuthorizationType=ALI, LocationQualifier", "LB", declaration.JE_LocationQualifier);

		declaration.ZG_AuthorisationNumber = "someXYZ";
		AssertEquals("When Authorization is invalid, LocationQualifier", ZString.Empty, declaration.JE_LocationQualifier);
	}

	public void TestSetDefaultLocationQualifierToLC_WithAuthorizationType()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		SetupAuthorisationsForOrg(organisation);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = organisation.PK;

		declaration.ZG_AuthorisationNumber = "1111ALI";
		AssertEquals("When AuthorizationType=ALI, LocationQualifier", "LC", declaration.JE_LocationQualifier);

		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.ZG_AuthorisationNumber = "1111CWP";
		AssertNotEquals("When AuthorizationType=CWP, LocationQualifier", "LC", declaration.JE_LocationQualifier);

		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.ZG_AuthorisationNumber = "1111CW1";
		AssertNotEquals("When AuthorizationType=CW1, LocationQualifier", "LC", declaration.JE_LocationQualifier);

		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.ZG_AuthorisationNumber = "1111CW2";
		AssertNotEquals("When AuthorizationType=CW2, LocationQualifier", "LC", declaration.JE_LocationQualifier);

		declaration.ZG_AuthorisationNumber = "someXYZ";
		AssertEquals("When Authorization is invalid, LocationQualifier", ZString.Empty, declaration.JE_LocationQualifier);
	}

	public void TestSetDefaultLocationQualifierIfNotEmpty()
	{
		var organisation = Factory.NewWithValidTestData<OrgHeader>();
		SetupAuthorisationsForOrg(organisation);

		var declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
		declaration.JE_OH_Importer = organisation.PK;
		declaration.JE_LocationQualifier = "XY";

		declaration.ZG_AuthorisationNumber = "1111ALI";
		AssertEquals("When LocationQualifier is already filled and AuthorizationType=ALI, LocationQualifier", "XY", declaration.JE_LocationQualifier);

		declaration.ZG_AuthorisationNumber = "1111CWP";
		AssertEquals("When LocationQualifier is already filled and AuthorizationType=CWP, LocationQualifier", "XY", declaration.JE_LocationQualifier);

		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.ZG_AuthorisationNumber = "1111ALI";
		AssertEquals("When LocationQualifier is empty and AuthorizationType=ALI, LocationQualifier", "LC", declaration.JE_LocationQualifier);

		declaration.JE_LocationQualifier = ZString.Empty;
		declaration.ZG_AuthorisationNumber = "1111CWP";
		AssertEquals("When LocationQualifier is empty and AuthorizationType=CWP, LocationQualifier", "LB", declaration.JE_LocationQualifier);
	}

	void SetupAuthorisationsForOrg(OrgHeader organisation)
	{
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, permitHolder: organisation.PK, "1111CWP", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW1, permitHolder: organisation.PK, "1111CW1", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCW2, permitHolder: organisation.PK, "1111CW2", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.ApprovedLocationForImport, permitHolder: organisation.PK, "1111ALI", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
	}
}
