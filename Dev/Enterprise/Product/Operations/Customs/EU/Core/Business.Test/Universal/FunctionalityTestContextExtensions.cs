using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.Business.Testing;

public static class FunctionalityTestContextExtensions
{
	public static void SetAESTransitionPeriod(this FunctionalityTestContext testContext,
		bool enabled,
		ZString grouping = default,
		ZDateTime effectiveDate = default)
	{
		testContext.SetFunctionality(
			code: Universal.Constants.FunctionalityTypes.AESTransitionPeriod,
			grouping: grouping.IfEmptyUse(() => GlbCompany.CurrentCompany.Country.Code),
			effectiveDate: effectiveDate.IfEmptyUse(() => ZDate.Today),
			enabled);
	}
}
