using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(KeyedCodePairValue))]
	sealed class KeyedCodePairValueTest : ValueProviderTest
	{
		public void TestBlankCodeReturnsBlankString()
		{
			var macro = "<KeyedCodePairValue(dynamicorderlinestatus, DESCRIPTION,   , ce34345b-43b3-4329-8a3f-1e644428e4f1)>";

			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			AssertIsResponsibleForReplacing(macro);
			AssertEquals("ValueProviderToTest.GetReplacement(macro, Report)", "(empty)", ValueProviderToTest.GetReplacement(macro, Report));
			AssertEquals("report.ErrorManager.ToString()", ReportErrorManager.HasNoErrors, Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));

			Report.ErrorManager.ClearErrors();
		}

		public void TestReplacementWithIllegalParameters()
		{
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			ValueProviderToTest.GetReplacement("<KeyedCodePairValue(dynamicorderheaderstatus, SomeField, 2,e8f803f8-e6da-4e36-978c-39ad9242ccdd)>", Report);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in KeyedCodePairValue Macro: Invalid field [SomeField]. Should be CODE or DESCRIPTION.]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();

			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			ValueProviderToTest.GetReplacement("<KeyedCodePairValue(dynamicorderheaderstatus, Code, MY1,Iamnotaguid)>", Report);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", true, Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false).StartsWith(
						@"Severity: [Warning (without error report)] Message: [Error in KeyedCodePairValue Macro: [Iamnotaguid] Is not a valid GUID"));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<codepairvaluepk>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   Code Pair Value pk  >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   CodePairValuePk somefield1, somefield2, somefield3   >", Passes.FirstPass));
			Assert("should match <KeyedCodePairValue(SomeProvider, Code, 1)>", ValueProviderToTest.IsResponsibleForReplacing("<KeyedCodePairValue(SomeProvider, Code, 1,e8f803f8-e6da-4e36-978c-39ad9242ccd)>", Passes.FirstPass));
			Assert("should match <KeyedCodePairValue(SomeProvider, Code, Code)>", ValueProviderToTest.IsResponsibleForReplacing("<KeyedCodePairValue(SomeProvider, Code, Code,e8f803f8-e6da-4e36-978c-39ad9242ccd)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   <KeyedCodePairValue(SomeProvider, Code)   >", Passes.SecondPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<KeyedCodePairValue(,,)>", Passes.FirstPass));
		}

		[ExpectNoExceptions]
		public void TestReplacement()
		{
			AssertEquals("INC", ValueProviderToTest.GetReplacement("<KeyedCodePairValue(dynamicorderheaderstatus, Code, 1,e8f803f8-e6da-4e36-978c-39ad9242ccdd)>", Report));
			AssertEquals("Incomplete", ValueProviderToTest.GetReplacement("<KeyedCodePairValue(dynamicorderheaderstatus, Description, 1,e8f803f8-e6da-4e36-978c-39ad9242ccdd)>", Report));
			AssertEquals("Incomplete", ValueProviderToTest.GetReplacement("<KeyedCodePairValue(dynamicorderheaderstatus, Description, INC,e8f803f8-e6da-4e36-978c-39ad9242ccdd)>", Report));
			AssertEquals("INC, PLC, CNF, SHP, PRT, DLV, CAN", ValueProviderToTest.GetReplacement("<KeyedCodePairValue(dynamicorderheaderstatus, Code, All,e8f803f8-e6da-4e36-978c-39ad9242ccdd)>", Report));
			AssertEquals("(empty)", ValueProviderToTest.GetReplacement("<KeyedCodePairValue(dynamicorderheaderstatus, Code, MY1,e8f803f8-e6da-4e36-978c-39ad9242ccdd)>", Report));

			object value = ValueProviderToTest.GetReplacement("<KeyedCodePairValue(dynamicorderheaderstatus, Code, MY1,Iamnotaguid)>", Report);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new KeyedCodePairValue();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Key", "e8f803f8-e6da-4e36-978c-39ad9242ccdd"));
		}
	}
}
