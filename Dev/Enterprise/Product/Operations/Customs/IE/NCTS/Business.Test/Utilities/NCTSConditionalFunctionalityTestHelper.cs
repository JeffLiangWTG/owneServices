using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.IE.NCTS.Business.Testing
{
	static class NCTSConditionalFunctionalityTestHelper
	{
		public static void CombineAssertionsInPhase5TransitionPeriod(AssertionWithHtml.VoidParameterlessDelegate assertions)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertionWithHtml.CombineAssertions(assertions);
			}
		}
	}
}
