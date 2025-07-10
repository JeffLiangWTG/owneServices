using System;
using CargoWise.Types;
using Enterprise.Customs.Universal;

namespace Enterprise.Customs.CH.Business.Testing;

public static class FuncsTestHelper
{
	public static IDisposable TemporarilySetFunctionality(string name, bool active) => ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(name, Core.Constants.CountryCodes.Switzerland, active ? ZDate.Today : ZDate.Today.AddDays(1), true);
}
