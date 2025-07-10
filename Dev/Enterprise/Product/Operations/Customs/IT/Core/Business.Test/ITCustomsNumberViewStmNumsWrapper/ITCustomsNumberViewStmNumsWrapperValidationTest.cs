using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.IT.Registry.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.IT.Business.Testing;

sealed class ITCustomsNumberViewStmNumsWrapperValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
{
	public void TestCheckYearOfApplicability()
	{
		var stmNum = Factory.New<CustomsNumberViewStmNums>();
		var wrapper = new ITCustomsNumberViewStmNumsWrapper(stmNum);
		wrapper.YearOfApplicability = ZDate.Today.Year - 1;
		AssertHasError(wrapper.YearOfApplicabilityInfo, "Year of applicability should not be earlier than current year.");
		wrapper.Validation.ValidateAll();
		AssertHasError(wrapper.YearOfApplicabilityInfo, "Year of applicability should not be earlier than current year.");
		Factory.Save();
		wrapper.Validation.ValidateAll();
		AssertNoErrors(wrapper.YearOfApplicabilityInfo);
	}

	public void TestCheckAppliesTo()
	{
		var currentCompany = GlbCompany.CurrentCompany;
		Factory.New<OrgHeader>().OH_Code = "CODE1";
		Factory.Save();
		new AccountCollectionTestBuilder(currentCompany.PK)
			.AppendAccount("12345678901-123", "1234").AppendAccountDetail("1234-CODE1", "CODE1")
			.AppendAccount("23456789012-234", "2345").AppendAccountDetail("2345-CODE1", "CODE1")
			.Build();

		var stmNumsProvider = CustomsNumberViewStmNumsBusinessProviderHelper.GetProvider(Factory, currentCompany.GC_RN_NKCountryCode, currentCompany.PK);
		var stmNums = CustomsNumberViewStmNumsHelper.NewStmNums(Factory, stmNumsProvider, currentCompany.PK);
		var stmNumsWrapper = new ITCustomsNumberViewStmNumsWrapper(stmNums);

		stmNumsWrapper.AppliesTo = ZString.Empty;
		AssertHasErrorContaining(stmNumsWrapper.AppliesToInfo, MandatoryValidation.MustBeEntered);
		AssertNoErrorContaining(stmNumsWrapper.AppliesToInfo, ListValidation.InvalidCodeError);
		stmNumsWrapper.AppliesTo = "12345678901";
		AssertNoErrorContaining(stmNumsWrapper.AppliesToInfo, MandatoryValidation.MustBeEntered);
		AssertNoErrorContaining(stmNumsWrapper.AppliesToInfo, ListValidation.InvalidCodeError);
		stmNumsWrapper.AppliesTo = "12345678912";
		AssertNoErrorContaining(stmNumsWrapper.AppliesToInfo, MandatoryValidation.MustBeEntered);
		AssertHasErrorContaining(stmNumsWrapper.AppliesToInfo, ListValidation.InvalidCodeError);
		stmNumsWrapper.AppliesTo = "23456789012";
		AssertNoErrorContaining(stmNumsWrapper.AppliesToInfo, MandatoryValidation.MustBeEntered);
		AssertNoErrorContaining(stmNumsWrapper.AppliesToInfo, ListValidation.InvalidCodeError);
	}
}
