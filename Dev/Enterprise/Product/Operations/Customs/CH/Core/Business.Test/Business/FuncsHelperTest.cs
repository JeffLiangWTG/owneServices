using System;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;

namespace Enterprise.Customs.CH.Business.Testing;
 
[TestedType(typeof(FuncsHelper))]
public sealed class FuncsHelperTest : TestCaseWithFactory
{
	public void TestIsCHNE015V3Active() => AssertFuncs(FunctionalityTypes.CHNE015V3, () => FuncsHelper.IsCHNE015V3Active);

	public void TestIsCHNT015V4Active() => AssertFuncs(FunctionalityTypes.CHNT015V4, () => FuncsHelper.IsCHNT015V4Active);

	public void TestIsCHNT515V4Active() => AssertFuncs(FunctionalityTypes.CHNT515V4, () => FuncsHelper.IsCHNT515V4Active);

	public void TestIsCHNT044V4Active() => AssertFuncs(FunctionalityTypes.CHNT044V4, () => FuncsHelper.IsCHNT044V4Active);

	void AssertFuncs(string code, Func<bool> funcs) => CombineAssertions(() =>
	{
		var today = ZDate.Today;
		var tomorrow = today.AddDays(1);

		AssertEquals($"{code}: Funcs does not exist", false, funcs());

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(code, Core.Constants.CountryCodes.Switzerland, today, true))
		{
			AssertEquals($"{code}: Current date is within start/end date", true, funcs());
		}

		using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(code, Core.Constants.CountryCodes.Switzerland, tomorrow, true))
		{
			AssertEquals($"{code}: Current date is outside start/end date", false, funcs());
		}
	});
}
