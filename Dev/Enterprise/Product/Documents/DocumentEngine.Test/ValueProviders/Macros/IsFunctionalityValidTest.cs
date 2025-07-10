using CargoWise.Application;
using CargoWise.Types;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Integration.DocumentEngine;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(IsFunctionalityValid))]
	sealed class IsFunctionalityValidTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			CombineAssertions(() =>
			{
				AssertIsResponsibleForReplacing("<IsFunctionalityValid(Risk)>");
				AssertIsResponsibleForReplacing("< IsFunctionalityValid(Risk)>");
				AssertIsResponsibleForReplacing("<IsFunctionalityValid(Other)>");
				AssertNotResponsibleForReplacing("<IsFunctionalityValid>");
				AssertNotResponsibleForReplacing("<AutoHeight>");
			});
		}

		[TestDate(2021, 08, 31)]
		public void TestReplacement()
		{
			var zzCustomsFunctionalityEffectiveDate = ObjectFactory.Get<IZZCustomsFunctionalityEffectiveDate>();
			var code = "RISK";
			using (zzCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(code, Core.Constants.CountryCodes.Australia, ZDate.Today, false))
			{
				AssertEquals("Macro should return 'N'", "N", ValueProviderToTest.GetReplacement("<IsFunctionalityValid(RISK)>", Report));
			}
			using (zzCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(code, Core.Constants.CountryCodes.Australia, ZDate.Today, true))
			{
				AssertEquals("Macro should return 'Y'", "Y", ValueProviderToTest.GetReplacement("<IsFunctionalityValid(RISK)>", Report));
			}
		}

		public void TestReportError()
		{
			new MacroTranslator(Report).GetValue("<IsFunctionalityValid(RISK)(RISK2)>", Passes.FirstPass);
			Assert(Report.ErrorManager.HasErrors);
			AssertEquals("Severity: [Warning (without error report)] Message: [Error in IsFunctionalityValid Macro: Wrong number of arguments. Expected Macro: [<IsFunctionalityValid(code)>], where code is the only argument. Input macro: [<IsFunctionalityValid(RISK)(RISK2)>]] Cell: [N/A] Sheetname: [(unknown)]", Report.ErrorManager.ToString());
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new IsFunctionalityValid();
		}
	}
}
