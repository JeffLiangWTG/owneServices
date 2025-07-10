using CargoWise.Types;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Customs.EU.NCTS.Business.Testing;

public static class NctsFunctionalityTestContextExtensions
{
	public static void SetNctsTransitionPeriod(this FunctionalityTestContext testContext,
		bool enabled,
		ZString grouping = default,
		ZDateTime effectiveDate = default)
	{
		testContext.SetFunctionality(
			code: Universal.Constants.FunctionalityTypes.NCTSTransitionPeriod,
			grouping: grouping.IfEmptyUse(() => GlbCompany.CurrentCompany.Country.Code),
			effectiveDate: effectiveDate.IfEmptyUse(() => ZDate.Today),
			enabled);
	}
}
