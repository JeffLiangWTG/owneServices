using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Universal;
using RefDataGroupingCodes = Enterprise.Core.Constants.Customs.Universal.RefDataGrouping.Codes;

namespace Enterprise.Customs.DE.Business.Testing
{
	sealed class UniversalValidationHelperTest : TestCaseWithFactory
	{
		public void TestIsInAESTransitionPeriod_True()
		{
			CombineAssertions(() =>
			{
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, Core.Constants.CountryCodes.Germany, ZDate.Today, true))
				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
				{
					AssertEquals("Functionality is enabled for Germany", true, UniversalValidationHelper.IsInAESTransitionPeriod);
				}

				using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, true))
				{
					AssertEquals("Functionality is enabled for EUN", true, UniversalValidationHelper.IsInAESTransitionPeriod);
				}
			});
		}

		public void TestIsInAESTransitionPeriod_False()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Constants.FunctionalityTypes.AESTransitionPeriod, RefDataGroupingCodes.EuropeanUnionEUN, ZDate.Today, false))
			{
				AssertEquals("Functionality is disabled", false, UniversalValidationHelper.IsInAESTransitionPeriod);
			}
		}
	}
}
