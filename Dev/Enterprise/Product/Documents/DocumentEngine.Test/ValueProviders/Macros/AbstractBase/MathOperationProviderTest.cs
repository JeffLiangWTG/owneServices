using System.Globalization;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Macros;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Environment;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	abstract class MathOperationProviderTest : ValueProviderTest
	{
		public void TestMacroCalculation()
		{
			MacroCalculation(TemplateString, ResultString, null);
		}

		public void TestNestedMacroCalculation()
		{
			MacroCalculation(NestedMacroTemplateString, NestedMacroResultString, null);
		}

		public void TestNParametersMacroCalculation()
		{
			MacroCalculation(NParamtersTemplateString, NParamtersMacroResultString, null);
		}

		public void TestNParametersNestedMacroCalculation()
		{
			MacroCalculation(NParamtersNestedMacroTemplateString, NParamtersNestedMacroResultString, null);
		}

		public void TestMacroCalculationInFrenchCulture()
		{
			MacroCalculation(NestedMacroTemplateStringFrenchFormat, NestedMacroResultStringFrenchFormat, Core.Constants.Languages.French);
		}

		void MacroCalculation(string templateString, string resultString, ZString languageCode)
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test", templateString, "UnitTest");

			var dummy = factory.New<DummyDocumentSupportable>();
			CreateRows(dummy);

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;
			document.Template.IsDocBuilderStyleForTest = true;

			var printJob = languageCode == (ZString)null ?
				DeliveryTestHelper.DeliverDocument(documentCommand).First() : DeliveryTestHelper.DeliverDocument(documentCommand, languageCode).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("Calculation should correct", resultString, workSheet.ToString());
			}
		}

		protected virtual void CreateRows(DummyDocumentSupportable dummy)
		{
			for (var index = 0; index < 4; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Code = "Test";
				child.Z0_Money = index * 1.25;
				child.Z0_AnotherDecimal = child.Z0_Money * 2;
			}
		}

		protected abstract string TemplateString { get; }
		protected abstract string ResultString { get; }
		protected abstract string NestedMacroTemplateString { get; }
		protected abstract string NestedMacroResultString { get; }
		protected abstract string NParamtersTemplateString { get; }
		protected abstract string NParamtersMacroResultString { get; }
		protected abstract string NParamtersNestedMacroTemplateString { get; }
		protected abstract string NParamtersNestedMacroResultString { get; }
		protected abstract string NestedMacroTemplateStringFrenchFormat { get; }
		protected abstract string NestedMacroResultStringFrenchFormat { get; }

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert(string.Format("should match < {0} () >", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format("< {0} () >", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format("should match < {0} (1.2231123) >", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format("< {0} (1.2231123) >", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format("should match < {0} (1.223,) >", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format("< {0} (1.223,) >", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format("should match < {0} (1.223,,2) >", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format("< {0} (1.223,,2) >", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format("should match <{0}( 12.33, 2.33)>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format("<{0}( 12.33, 2.33)>", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0}( ""-2.77556E-17"", ""1"")>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0}( ""-2.77556E-17"", ""2.33"")>", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0}( ""12.33"", ""2.33"")>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0}( ""12.33"", ""2.33"")>", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0}(""12.33"" , ""-2.33"")>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0}(""12.33"" , ""2.33"")>", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0}(""12.33"", ""2.33"" )>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0}(""12.33"", ""2.33"" )>", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match < {0}(""12.33"", ""-2.33"")>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"< {0}(""12.33"", ""2.33"")>", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0} (""12.33"", ""2.33"")>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0} (""12.33"", ""2.33"")>", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0}(""12.33"", ""-2.33"") >", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0}(""12.33"", ""2.33"") >", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0}(""12"", ""2"")>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0}(""12"", ""2"") >", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0}(""0"", ""-0.2"")>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0}(""0"", ""0.2"") >", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0}(""12"", ""2"", ""3"")>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0}(""12"", ""2"", ""3"")>", OperationProvider.OperationName), Passes.SecondPass));
			Assert(string.Format(@"should match <{0}(""12"", ""2"", ""-3"", ""4"", ""-5"")>", OperationProvider.OperationName), ValueProviderToTest.IsResponsibleForReplacing(string.Format(@"<{0}(""12"", ""2"", ""3"", ""4"", ""5"")>", OperationProvider.OperationName), Passes.SecondPass));
		}

		public void TestReplacement()
		{
			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.France)))
			{
				var textsAndValues = ReplacementTexts.Zip(ReplacementValues, (t, v) => new { Text = t, Value = v });
				CombineAssertions(() =>
				{
					foreach (var textAndValue in textsAndValues)
					{
						AssertEquals(textAndValue.Value, ValueProviderToTest.GetReplacement(textAndValue.Text, Report));
					}
				});
			}
		}

		public void TestInvalidInputValuesReportAnError()
		{
			var macro1 = $"<{OperationProvider.OperationName}(\"1\", \"A\")>";
			var macro2 = $"<{OperationProvider.OperationName}(\"2\", \"3\")>";
			var macro3 = $"<{OperationProvider.OperationName}(load'o'crap, crap2)>";

			ValueProviderToTest.GetReplacement(macro1, Report);
			ValueProviderToTest.GetReplacement(macro2, Report);
			ValueProviderToTest.GetReplacement(macro3, Report);

			var expectedErrorMessage = string.Format(CultureInfo.InvariantCulture, @"Severity: [Warning (without error report)] Message: [Error in {0} Macro: Invalid parameters provided: {1}, expected format: <{0}(""value1"",""value2"",...)>.]
Severity: [Warning (without error report)] Message: [Error in {0} Macro: Invalid parameters provided: {2}, expected format: <{0}(""value1"",""value2"",...)>.]", OperationProvider.OperationName, macro1, macro3);

			AssertEquals("report.ErrorManager.ToString()", expectedErrorMessage, Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));

			Report.ErrorManager.ClearErrors();

			AssertMacroReportsError($"<{OperationProvider.OperationName}()>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(123)>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(123,234)>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"00:10\")>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"1,10\",\"a\")>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"1.1.1\",\"10\")>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"5\", 1, \"2\")>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"1\", \"\", \"2\")>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"1\", \"\"\", \"2\")>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"1\", \"\")>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"\", \"2\")>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"1.0.0.0\", \"1\")>");
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"1\", ,\"1\")>");

			var innerMacro = $"<{OperationProvider.OperationName}(\"0\",\"1\")>";
			AssertMacroReportsError($"<{OperationProvider.OperationName}(\"{innerMacro}\",\"APPLE\")>", $"<{OperationProvider.OperationName}(\"{ValueProviderToTest.GetReplacement(innerMacro, Report)}\",\"APPLE\")>");
		}

		public void TestNonMatchingMacroDoesntHang()
		{
			var longNumber = new string('1', 20);
			var macro = $"<{OperationProvider.OperationName}(\"{longNumber}\",\"{longNumber}\">"; // Missing closing parenthesis

			Assert("Precondition", !OperationProvider.Regex.IsMatch(macro)); // This input shouldn't cause the regex to hang
		}

		protected abstract string[] ReplacementTexts { get; }
		protected abstract string[] ReplacementValues { get; }

		protected MathOperationProvider OperationProvider
		{
			get
			{
				if (operationProvider == null)
				{ operationProvider = (MathOperationProvider)GetNewValueProvider(); }
				return operationProvider;
			}
		}
		MathOperationProvider operationProvider;

		void AssertMacroReportsError(string macro, string evaluatedNestedMacro = null)
		{
			new MacroTranslator(Report).GetValue(macro, Passes.FirstPass);
			var expectedErrorMessage = string.Format(CultureInfo.InvariantCulture, @"Severity: [Warning (without error report)] Message: [Error in {0} Macro: Invalid parameters provided: {1}, expected format: <{0}(""value1"",""value2"",...)>. Input macro: [{2}]]", OperationProvider.OperationName, evaluatedNestedMacro ?? macro, macro);
			AssertEquals("report.ErrorManager.ToString()", expectedErrorMessage, Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			Report.ErrorManager.ClearErrors();
		}
	}
}
