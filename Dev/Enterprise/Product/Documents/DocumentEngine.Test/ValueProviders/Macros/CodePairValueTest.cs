using System;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CodePairValue))]
	sealed class CodePairValueTest : ValueProviderTest
	{
		public void TestReplacementWithIllegalPropertyName()
		{
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			ValueProviderToTest.GetReplacement("<CodePairValue(StorageClass, SomeField, 1)>", Report);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in CodePairValue Macro: Invalid property name - SomeField]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestReplacementWithIllegalProviderName()
		{
			Assert("Precondition - report.ErrorManager.HasErrors is false", !Report.ErrorManager.HasErrors);
			var replacedValue = (string)ValueProviderToTest.GetReplacement("<CodePairValue(IllegalProviderName, SomeField, 1)>", Report);
			AssertEquals("replaced value is empty", string.Empty, replacedValue);
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in CodePairValue Macro: Invalid provider name - IllegalProviderName]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<codepairvalue>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   Code Pair Value   >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   CodePairValue somefield1, somefield2, somefield3   >", Passes.FirstPass));
			Assert("should match <CodePairValue(SomeProvider, Code, 1)>", ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(SomeProvider, Code, 1)>", Passes.FirstPass));
			Assert("should match <CodePairValue(SomeProvider, Code, Code)>", ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(SomeProvider, Code, Code)>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<   <CodePairValue(SomeProvider, Code)   >", Passes.SecondPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(,,)>", Passes.FirstPass));
			Assert("should match <CodePairValue(SomeProvider, Description, )>", ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(SomeProvider, Description, )>", Passes.FirstPass));
			Assert("should match <CodePairValue(SomeProvider, Code, 1, TreatIndexAsString)>", ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(SomeProvider, Code, 1, TreatIndexAsString)>", Passes.FirstPass));
			Assert("should match <CodePairValue(SomeProvider, Code, , TreatIndexAsString)>", ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(SomeProvider, Code, , TreatIndexAsString)>", Passes.FirstPass));
			Assert("should not match <CodePairValue(SomeProvider, Code, 1, TreatIndexAsStraing)>", !ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(SomeProvider, Code, 1, TreatIndexAsStraing)>", Passes.FirstPass));
			Assert("should not match <CodePairValue(SomeProvider, Code, Code, )>", !ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(SomeProvider, Code, Code, )>", Passes.FirstPass));
			Assert("should match <CodePairValue(SomeProvider, Description, aaa bbb)>", ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(SomeProvider, Description, aaa bbb)>", Passes.FirstPass));
			Assert("should match <CodePairValue(SomeProvider, Description, \"aaa bbb\")>", ValueProviderToTest.IsResponsibleForReplacing("<CodePairValue(SomeProvider, Description, \"aaa bbb\")>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("20F", ValueProviderToTest.GetReplacement("<CodePairValue(StorageClass, Code, 1)>", Report));
			AssertEquals("Twenty Foot Equivalent Unit", ValueProviderToTest.GetReplacement("<CodePairValue(StorageClass, Description, 1)>", Report));
			AssertEquals(String.Empty, ValueProviderToTest.GetReplacement("<CodePairValue(StorageClass, Description, 1, TreatIndexAsString)>", Report));
			AssertEquals("Twenty Foot Equivalent Unit", ValueProviderToTest.GetReplacement("<CodePairValue(StorageClass, Description, 20F)>", Report));
			AssertEquals("Twenty Foot Equivalent Unit", ValueProviderToTest.GetReplacement("<CodePairValue(StorageClass, Description, 20F, TreatIndexAsString)>", Report));
			AssertEquals("20F, 20R, 20H, 40F, 40R, 40H, 45F, GEN", ValueProviderToTest.GetReplacement("<CodePairValue(StorageClass, Code, All)>", Report));
			AssertEquals(String.Empty, ValueProviderToTest.GetReplacement("<CodePairValue(StorageClass, Code, MY1)>", Report));

			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Some Value", "20F"));
			var areaToTest = new ConfigArea(4, 12, Report, "");
			AssertEquals("Twenty Foot Equivalent Unit", areaToTest.ReplaceMacros("<CodePairValue(StorageClass, Description, <Some Value>)>"));
			AssertEquals("Twenty Foot Equivalent Unit", areaToTest.ReplaceMacros("<CodePairValue(StorageClass, Description, \"<Some Value>\")>"));
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CodePairValue();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Storage Amount", "20F"));
		}
	}
}
