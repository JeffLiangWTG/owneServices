using CargoWise.Types;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITCustomsNumberViewStmNumsWrapperLookupsTest : CargoWise.EntityFramework.Testing.BusinessObjectLookupsTestCase
{
	public void TestTypeList()
	{
		var stmNum = Factory.New<CustomsNumberViewStmNums>();
		stmNum.Provider = Factory.Load<GlbCompany>(GlbCompany.CurrentCompany.PK).CustomsNumberProvider;
		AssertEquals("NumberRangeTypeList", "R, ENS, RENS, DIV, EXS, REXS, AP", stmNum.Lookups.TypeList.CodesAsString);
	}

	public void TestAppliesToListForCustomsDeclarationType()
	{
		Factory.New<OrgHeader>().OH_Code = "CODE1";
		Factory.Save();

		var currentCompany = GlbCompany.CurrentCompany;

		new AccountCollectionTestBuilder(currentCompany.PK)
			.AppendAccount("23456789012-234", "1234").AppendAccountDetail("1234-CODE1", "CODE1")
			.AppendAccount("12345678901-123", "2345").AppendAccountDetail("2345-CODE1", "CODE1")
			.AppendAccount("12345678901-999", "3456").AppendAccountDetail("3456-CODE1", "CODE1")
			.Build();

		var newCompany = Factory.New<GlbCompany>();
		new AccountCollectionTestBuilder(newCompany.PK)
			.AppendAccount("11111111111-001", "1111").AppendAccountDetail("1111-CODE1", "CODE1")
			.AppendAccount("22222222222-002", "2222").AppendAccountDetail("2222-CODE1", "CODE1")
			.Build();

		CombineAssertions("Assert AppliesToList against current company", () => AssertAppliesToListAgainstOwnerCompany(currentCompany, "12345678901, 23456789012", NumberRangeTypeList.Codes.CustomsDeclarations));
		CombineAssertions("Assert AppliesToList against new company", () => AssertAppliesToListAgainstOwnerCompany(newCompany, "11111111111, 22222222222", NumberRangeTypeList.Codes.CustomsDeclarations));

		var newCompanyWithoutAnyRegisteredAccount = Factory.New<GlbCompany>();
		CombineAssertions("Assert AppliesToList against new company without any registered account", () => AssertAppliesToListAgainstOwnerCompany(newCompanyWithoutAnyRegisteredAccount, "", NumberRangeTypeList.Codes.CustomsDeclarations));
	}

	#region Implementation

	void AssertAppliesToListAgainstOwnerCompany(GlbCompany company, ZString expectedCodesAsString, ZString stmNumsWrapperType)
	{
		var stmNumsProvider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, company.GC_RN_NKCountryCode, company.PK);
		var stmNums = CustomsNumberViewStmNumsHelper.NewStmNums(Factory, stmNumsProvider, company.PK);
		var stmNumsWrapper = new ITCustomsNumberViewStmNumsWrapper(stmNums);
		stmNumsWrapper.SN_Type = stmNumsWrapperType;
		var appliesToList1 = stmNumsWrapper.Lookups.AppliesToList;
		var appliesToList2 = stmNumsWrapper.Lookups.AppliesToList;
		AssertEquals("AppliesToList cached", true, object.ReferenceEquals(appliesToList1, appliesToList2));
		AssertEquals("AppliesToList.CodesAsString", expectedCodesAsString, appliesToList1.CodesAsString);
	}

	#endregion
}
