using CargoWise.Types;
using Enterprise.Customs.Universal;
using NUnit.Framework;
using static Enterprise.Customs.Universal.Constants;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.NCTS.Business.Testing
{
	static class NCTSConditionalFunctionalityTestHelper
	{
		public static void RunAssertionsInPhase5TransitionPeriod(AssertionWithHtml.VoidParameterlessDelegate assertions)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
			{
				AssertionWithHtml.CombineAssertions(assertions);
			}
		}

		public static void RunAssertionsOutsidePhase5TransitionPeriod(AssertionWithHtml.VoidParameterlessDelegate assertions)
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(FunctionalityTypes.NCTSTransitionPeriod,
						RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertionWithHtml.CombineAssertions(assertions);
			}
		}
	}
}
