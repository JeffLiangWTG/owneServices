using System;
using CargoWise.Customs.IT.MessageContracts.Declaration.Import;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.IT.Business.Declaration;
using Enterprise.Customs.IT.Business.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.MessageSending.AidaXml.Import.Testing;

sealed class LocationOfGoodsWrapperTest : TestCaseWithFactory
{
	public void TestNewOrNull()
	{
		AssertExceptionThrown<ArgumentNullException>("Exception expected when argument is null", () => LocationOfGoodsWrapper.NewOrNull(null));
		AssertNull(nameof(LocationOfGoodsWrapper.NewOrNull), NewLocationOfGoodsWrapper);

		declaration.JE_LocationQualifier = "XY";
		declaration.JE_MessageType = "EXP";
		AssertNull(nameof(LocationOfGoodsWrapper.NewOrNull), NewLocationOfGoodsWrapper);

		declaration.JE_LocationQualifier = "";
		declaration.JE_MessageType = "IMP";
		AssertNull(nameof(LocationOfGoodsWrapper.NewOrNull), NewLocationOfGoodsWrapper);

		declaration.JE_LocationQualifier = "XY";
		AssertNotNull(nameof(LocationOfGoodsWrapper.NewOrNull), NewLocationOfGoodsWrapper);
	}

	public void TestAdditionalCode()
	{
		declaration.JE_LocationQualifier = "D";
		AssertEquals("When LocationQalifier = D, AdditionalCode", "", NewLocationOfGoodsWrapper.AdditionalCode);
		declaration.JE_LocationQualifier = "F";
		AssertEquals("When LocationQalifier = F, AdditionalCode", "", NewLocationOfGoodsWrapper.AdditionalCode);
		declaration.JE_LocationQualifier = "FC";
		AssertEquals("When LocationQalifier = FC, AdditionalCode", "", NewLocationOfGoodsWrapper.AdditionalCode);
		declaration.JE_LocationQualifier = "LB";
		declaration.ImportJE_LocationOtherInformation = "ABCXY";
		AssertEquals("When LocationQalifier = LB, AdditionalCode", "ABCXY", NewLocationOfGoodsWrapper.AdditionalCode);
		declaration.JE_LocationQualifier = "LC";
		AssertEquals("When LocationQalifier = LC, AdditionalCode", "ABCXY", NewLocationOfGoodsWrapper.AdditionalCode);
		declaration.JE_LocationQualifier = "XX";
		AssertEquals("When LocationQalifier Unknown, Add", "", NewLocationOfGoodsWrapper.AdditionalCode);
	}

	public void TestAddress()
	{
		declaration.JE_LocationQualifier = "D";
		AssertEquals("When LocationQalifier = D, Address", "", NewLocationOfGoodsWrapper.Address);

		declaration.JE_LocationQualifier = "F";
		AssertEquals("When LocationQalifier = F, with empty Address", "", NewLocationOfGoodsWrapper.Address);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.OA_Address1 = "Line 1";
		orgAddress.OA_Address2 = "Cross 5";
		orgHeader.Addresses.Add(orgAddress);
		declaration.GoodsLocationAddress.OrganisationPK = orgHeader.PK;
		declaration.GoodsLocationAddress.E2_OA_Address = orgAddress.PK;
		AssertEquals("When LocationQalifier = F, Address", "Line 1Cross 5", NewLocationOfGoodsWrapper.Address);

		declaration.JE_LocationQualifier = "FC";
		AssertEquals("When LocationQalifier = FC, Address", "", NewLocationOfGoodsWrapper.Address);
		declaration.JE_LocationQualifier = "LB";
		AssertEquals("When LocationQalifier = LB, Address", "", NewLocationOfGoodsWrapper.Address);
		declaration.JE_LocationQualifier = "LC";
		AssertEquals("When LocationQalifier = LC, Address", "", NewLocationOfGoodsWrapper.Address);
		declaration.JE_LocationQualifier = "XX";
		AssertEquals("When LocationQalifier Unknown, Address", "", NewLocationOfGoodsWrapper.Address);
	}

	public void TestCity()
	{
		declaration.JE_LocationQualifier = "D";
		AssertEquals("When LocationQalifier = D, City", "", NewLocationOfGoodsWrapper.City);

		declaration.JE_LocationQualifier = "F";
		AssertEquals("When LocationQalifier = F and empty address, City", "", NewLocationOfGoodsWrapper.City);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.City = "Rome";
		orgHeader.Addresses.Add(orgAddress);
		declaration.GoodsLocationAddress.OrganisationPK = orgHeader.PK;
		declaration.GoodsLocationAddress.E2_OA_Address = orgAddress.PK;
		AssertEquals("When LocationQalifier = F, City", "Rome", NewLocationOfGoodsWrapper.City);

		orgAddress.City = "";
		AssertEquals("When LocationQalifier = F and empty city, City", "", NewLocationOfGoodsWrapper.City);

		declaration.JE_LocationQualifier = "FC";
		AssertEquals("When LocationQalifier = FC, City", "", NewLocationOfGoodsWrapper.City);
		declaration.JE_LocationQualifier = "LB";
		AssertEquals("When LocationQalifier = LB, City", "", NewLocationOfGoodsWrapper.City);
		declaration.JE_LocationQualifier = "LC";
		AssertEquals("When LocationQalifier = LC, City", "", NewLocationOfGoodsWrapper.City);
		declaration.JE_LocationQualifier = "XX";
		AssertEquals("When LocationQalifier Unknown, City", "", NewLocationOfGoodsWrapper.City);
	}

	public void TestCode()
	{
		declaration.JE_LocationQualifier = "D";
		declaration.JE_LocationOfGoods = "IT234543";
		AssertEquals("When LocationQalifier = D, Code", "IT234543", NewLocationOfGoodsWrapper.Code);
		declaration.JE_LocationQualifier = "F";
		AssertEquals("When LocationQalifier = F, Code", "", NewLocationOfGoodsWrapper.Code);

		declaration.JE_LocationQualifier = "FC";
		declaration.JE_LocationOfGoods = "AW3545";
		AssertEquals("When LocationQalifier = FC, Code", "AW3545", NewLocationOfGoodsWrapper.Code);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var cusAuthorisationHeader = CusAuthorisationHeaderTestHelper.CreateAuthorisationHeader(Factory, type: CusAuthorizationHeaderTypeList.Codes.CustomsWarehousingCWP, permitHolder: orgHeader.PK, "123ABD", startDate: ZDate.Today.AddDays(-10), endDate: ZDate.Today.AddDays(10));
		var cusAuthorisationRule = cusAuthorisationHeader.CusAuthorisationRules.AddNew();
		cusAuthorisationRule.CPR_ValueFrom = "1234ER";
		cusAuthorisationRule.CPR_RuleCode = "LOC";

		declaration.ZG_AuthorisationNumber = "123ABD";
		declaration.JE_LocationQualifier = "LB";
		declaration.JE_LocationOfGoods = "1234ER";
		AssertEquals("When LocationQalifier = LB, Code", "123ABD.1234ER", NewLocationOfGoodsWrapper.Code);

		declaration.JE_LocationQualifier = "LC";
		AssertEquals("When LocationQalifier = LC, Code", "123ABD.1234ER", NewLocationOfGoodsWrapper.Code);

		declaration.JE_LocationOfGoods = "";
		AssertEquals("When LocationQalifier = LC and LocationOfGoods empty, Code", "123ABD", NewLocationOfGoodsWrapper.Code);

		declaration.ZG_AuthorisationNumber = "";
		declaration.JE_LocationOfGoods = "1234ER";
		declaration.JE_LocationQualifier = "LC";
		AssertEquals("When LocationQalifier = LC and AuthorisationNumber is empty, Code", "1234ER", NewLocationOfGoodsWrapper.Code);

		declaration.JE_LocationOfGoods = "";
		AssertEquals("When LocationQalifier = LC, LocationOfGoods and AuthorisationNumber are empty, Code", "", NewLocationOfGoodsWrapper.Code);

		declaration.JE_LocationQualifier = "XX";
		AssertEquals("When LocationQalifier Unknown, Code", "", NewLocationOfGoodsWrapper.Code);
	}

	public void TestCountryCode()
	{
		declaration.JE_LocationQualifier = "D";
		declaration.JE_LocationOfGoods = "ES134";
		AssertEquals("When LocationQalifier = D, CountryCode", "ES", NewLocationOfGoodsWrapper.CountryCode);
		declaration.JE_LocationQualifier = "F";
		AssertEquals("When LocationQalifier = F, CountryCode", "IT", NewLocationOfGoodsWrapper.CountryCode);

		declaration.JE_LocationQualifier = "FC";
		declaration.ImportJE_SubLocationOfGoods = "DE";
		AssertEquals("When LocationQalifier = FC, CountryCode", "DE", NewLocationOfGoodsWrapper.CountryCode);

		declaration.JE_LocationQualifier = "LB";
		AssertEquals("When LocationQalifier = LB, CountryCode", "DE", NewLocationOfGoodsWrapper.CountryCode);
		declaration.JE_LocationQualifier = "LC";
		AssertEquals("When LocationQalifier = LC, CountryCode", "DE", NewLocationOfGoodsWrapper.CountryCode);
		declaration.JE_LocationQualifier = "XX";
		AssertEquals("When LocationQalifier Unknown, CountryCode", "", NewLocationOfGoodsWrapper.CountryCode);
	}

	public void TestQualifier()
	{
		declaration.JE_LocationQualifier = "D";
		AssertEquals("When LocationQalifier = D, Qualifier", "V", NewLocationOfGoodsWrapper.Qualifier);
		declaration.JE_LocationQualifier = "F";
		AssertEquals("When LocationQalifier = F, Qualifier", "Z", NewLocationOfGoodsWrapper.Qualifier);
		declaration.JE_LocationQualifier = "FC";
		AssertEquals("When LocationQalifier = FC, Qualifier", "Y", NewLocationOfGoodsWrapper.Qualifier);
		declaration.JE_LocationQualifier = "LB";
		AssertEquals("When LocationQalifier = LB, Qualifier", "Y", NewLocationOfGoodsWrapper.Qualifier);
		declaration.JE_LocationQualifier = "LC";
		AssertEquals("When LocationQalifier = LC, Qualifier", "Y", NewLocationOfGoodsWrapper.Qualifier);
		declaration.JE_LocationQualifier = "XX";
		AssertEquals("When LocationQalifier Unknown, Qualifier", "", NewLocationOfGoodsWrapper.Qualifier);
	}

	public void TestRole()
	{
		declaration.JE_LocationQualifier = "D";
		AssertEquals("When LocationQalifier = D, Role", "D", NewLocationOfGoodsWrapper.Role);
		declaration.JE_LocationQualifier = "F";
		AssertEquals("When LocationQalifier = F, Role", "D", NewLocationOfGoodsWrapper.Role);
		declaration.JE_LocationQualifier = "FC";
		AssertEquals("When LocationQalifier = FC, Role", "D", NewLocationOfGoodsWrapper.Role);
		declaration.JE_LocationQualifier = "LB";
		AssertEquals("When LocationQalifier = LB, Role", "B", NewLocationOfGoodsWrapper.Role);
		declaration.JE_LocationQualifier = "LC";
		AssertEquals("When LocationQalifier = LC, Role", "C", NewLocationOfGoodsWrapper.Role);
		declaration.JE_LocationQualifier = "XX";
		AssertEquals("When LocationQalifier Unknown, Role", "", NewLocationOfGoodsWrapper.Role);
	}

	public void TestZipCode()
	{
		declaration.JE_LocationQualifier = "D";
		AssertEquals("When LocationQalifier = D, ZipCode", "", NewLocationOfGoodsWrapper.ZipCode);

		declaration.JE_LocationQualifier = "F";
		AssertEquals("When LocationQalifier = F and address empty, ZipCode", "", NewLocationOfGoodsWrapper.ZipCode);

		var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
		var orgAddress = Factory.New<OrgAddress>();
		orgAddress.Postcode = "23454";
		orgHeader.Addresses.Add(orgAddress);
		declaration.GoodsLocationAddress.OrganisationPK = orgHeader.PK;
		declaration.GoodsLocationAddress.E2_OA_Address = orgAddress.PK;
		AssertEquals("When LocationQalifier = F, ZipCode", "23454", NewLocationOfGoodsWrapper.ZipCode);

		orgAddress.Postcode = "";
		AssertEquals("When LocationQalifier = F and postcode empty, ZipCode", "", NewLocationOfGoodsWrapper.ZipCode);

		declaration.JE_LocationQualifier = "FC";
		AssertEquals("When LocationQalifier = FC, ZipCode", "", NewLocationOfGoodsWrapper.ZipCode);
		declaration.JE_LocationQualifier = "LB";
		AssertEquals("When LocationQalifier = LB, ZipCode", "", NewLocationOfGoodsWrapper.ZipCode);
		declaration.JE_LocationQualifier = "LC";
		AssertEquals("When LocationQalifier = LC, ZipCode", "", NewLocationOfGoodsWrapper.ZipCode);
		declaration.JE_LocationQualifier = "XX";
		AssertEquals("When LocationQalifier Unknown, ZipCode", "", NewLocationOfGoodsWrapper.ZipCode);
	}

	protected override void SetUp()
	{
		base.SetUp();

		declaration = Factory.New<JobDeclaration>();
		declaration.JE_MessageType = "IMP";
	}

	JobDeclaration declaration;
	ILocationOfGoods NewLocationOfGoodsWrapper => GetNewLocationOfGoodsOrNull();

	ILocationOfGoods GetNewLocationOfGoodsOrNull() => LocationOfGoodsWrapper.NewOrNull(declaration);
}
