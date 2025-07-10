using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.EFTA.TemporaryStorageRegister.Business.Testing;

[TestedType(typeof(TSCustomsNumberViewStmNumsSetting))]
sealed class TSCustomsNumberViewStmNumsSettingTest : NonPersistentBusinessObjectTestCase
{
	public void TestSettings()
	{
		CombineAssertions(() =>
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			var provider = premises.NumberProvider;
			_ = provider.CustomsNumbers.AddNew();
			var wrapper1 = provider.CustomsNumberWrappers[0];
			wrapper1.IsActive = true;
			wrapper1.NumberPadding = 2;

			var setting = new TSCustomsNumberViewStmNumsSetting(premises, ZString.Empty);
			AssertEquals("RequiredDigit equals to NumberPadding", 2, setting.RequiredDigit());

			wrapper1.IsActive = false;
			AssertEquals("RequiredDigit equals to Default", 1, setting.RequiredDigit());
		});
	}

	public void TestGenerateCustomsNumber()
	{
		CombineAssertions(() =>
		{
			var premises = Factory.New<CusTempStorageRegPremises>();
			var provider = premises.NumberProvider;
			var stmNums = provider.CustomsNumbers.AddNew();
			stmNums.SN_MinimumValue = 1000;
			stmNums.SN_MaximumValue = 1001;
			var wrapper1 = provider.CustomsNumberWrappers[0];
			wrapper1.NumberPrefix = "P";
			wrapper1.NumberSuffix = "S";
			wrapper1.IsActive = true;

			var setting = new TSCustomsNumberViewStmNumsSetting(premises, ZString.Empty);
			AssertEquals("Formatted as it has a valid wrapper", "P001S", setting.GenerateCustomsNumber(stmNums, "001"));

			wrapper1.NumberSuffix = "Z";
			AssertEquals("Format changes if wrapper changes", "P001Z", setting.GenerateCustomsNumber(stmNums, "001"));

			wrapper1.IsActive = false;
			AssertEquals("Return the same number as it hasn't a valid wrapper", "001", setting.GenerateCustomsNumber(stmNums, "001"));
		});
	}

	protected override BusinessObject GetNewBusinessObject()
		=> new TSCustomsNumberViewStmNumsSetting(Factory.New<CusTempStorageRegPremises>(), ZString.Empty);
}
