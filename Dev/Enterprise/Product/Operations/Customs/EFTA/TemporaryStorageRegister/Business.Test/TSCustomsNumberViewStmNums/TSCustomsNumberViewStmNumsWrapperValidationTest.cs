using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

sealed class TSCustomsNumberViewStmNumsWrapperValidationTest : CargoWise.EntityFramework.Testing.BusinessObjectValidationTestCase
{
	const string NumberSeparatorMessageError = "Character '@' is not allowed for Prefix or Suffix.";

	public void TestCheckNumberPrefix()
	{
		var stmNum = Factory.New<CustomsNumberViewStmNums>();
		var wrapper = new TSCustomsNumberViewStmNumsWrapper(stmNum) { NumberPrefix = "ADT@@" };
		wrapper.Validation.ValidateNumberPrefix();
		AssertHasErrorContaining(wrapper.NumberPrefixInfo, NumberSeparatorMessageError);
		wrapper.NumberPrefix = "ADT";
		wrapper.Validation.ValidateNumberPrefix();
		AssertNoErrorContaining(wrapper.NumberPrefixInfo, NumberSeparatorMessageError);
	}

	public void TestCheckNumberSuffix()
	{
		var stmNum = Factory.New<CustomsNumberViewStmNums>();
		var wrapper = new TSCustomsNumberViewStmNumsWrapper(stmNum) { NumberSuffix = "ADT@@" };
		wrapper.Validation.ValidateNumberSuffix();
		AssertHasErrorContaining(wrapper.NumberSuffixInfo, NumberSeparatorMessageError);
		wrapper.NumberSuffix = "ADT";
		wrapper.Validation.ValidateNumberSuffix();
		AssertNoErrorContaining(wrapper.NumberSuffixInfo, NumberSeparatorMessageError);
	}

	public void TestCheckIsActive()
	{
		CombineAssertions(() =>
		{
			const string expectedError = "There is already one configuration active. Only one configuration active is allowed per premises.";
			var stmNumsProvider = Factory.New<CusTempStorageRegPremises>().NumberProvider;
			var stmNums1 = stmNumsProvider.CustomsNumbers.AddNew();
			var wrapper1 = (TSCustomsNumberViewStmNumsWrapper)stmNumsProvider.GetOrCreateWrapper(stmNums1);
			wrapper1.IsActive = true;

			AssertNoErrorContaining("Only 1 wrapper active", wrapper1.IsActiveInfo, expectedError);

			var stmNums2 = stmNumsProvider.CustomsNumbers.AddNew();
			var wrapper2 = (TSCustomsNumberViewStmNumsWrapper)stmNumsProvider.GetOrCreateWrapper(stmNums2);
			wrapper2.IsActive = true;

			AssertHasErrorContaining("2 wrappers active", wrapper2.IsActiveInfo, expectedError);

			wrapper1.IsActive = false;

			wrapper2.Validation.ValidateIsActive();

			AssertNoErrorContaining("Only 1 wrapper active #1", wrapper1.IsActiveInfo, expectedError);
			AssertNoErrorContaining("Only 1 wrapper active #2", wrapper2.IsActiveInfo, expectedError);

			var stmNumsProvider2 = Factory.New<CusTempStorageRegPremises>().NumberProvider;
			var stmNums3 = stmNumsProvider2.CustomsNumbers.AddNew();
			var wrapper3 = (TSCustomsNumberViewStmNumsWrapper)stmNumsProvider2.GetOrCreateWrapper(stmNums3);
			wrapper3.IsActive = true;

			AssertNoErrorContaining("Only 1 wrapper active per Provider #1", wrapper1.IsActiveInfo, expectedError);
			AssertNoErrorContaining("Only 1 wrapper active per Provider #2", wrapper2.IsActiveInfo, expectedError);
			AssertNoErrorContaining("Only 1 wrapper active per Provider #3", wrapper3.IsActiveInfo, expectedError);
		});
	}
}
