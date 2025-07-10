using System.Globalization;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Divide))]
	sealed class DivideTest : MathOperationProviderTest
	{
		protected override void CreateRows(DummyDocumentSupportable dummy)
		{
			var values = new decimal[][]
			{
				new decimal[] { 8, 2 },
				new decimal[] { 8, 5 },
				new decimal[] { 0.35m, 0.7m },
				new decimal[] { -3.5m, -1.25m },
				new decimal[] { -0.7m, 5 },
				new decimal[] { 0.7m, -5 },
			};

			for (var index = 0; index < values.Length; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Code = "Test";
				child.Z0_Money = values[index][0];
				child.Z0_AnotherDecimal = values[index][1];
			}
		}

		protected override string TemplateString
		{
			get
			{
				return
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{C}-[<Divide(""<Collection.Money>"",""<Collection.AnotherDecimal>"")>]
{A}-[#EndOfReport]";
			}
		}

		protected override string ResultString
		{
			get
			{
				return
@"{C}-[4]
{C}-[1.6]
{C}-[0.5]
{C}-[2.8]
{C}-[-0.14]
{C}-[-0.14]";
			}
		}

		protected override string NestedMacroTemplateString
		{
			get
			{
				return
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{C}-[<FormatNumber(<Divide(""<Collection.AnotherDecimal>"",""<Divide(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>"")>, AUD)>]
{C}-[<Divide(""3"",""<Divide(""-6"",""4"")>"")>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NestedMacroResultString
		{
			get
			{
				return
@"{C}-[8.00]
{C}-[-2]
{C}-[8.00]
{C}-[-2]
{C}-[0.35]
{C}-[-2]
{C}-[-3.50]
{C}-[-2]
{C}-[-0.70]
{C}-[-2]
{C}-[0.70]
{C}-[-2]";
			}
		}

		protected override string NParamtersTemplateString
		{
			get
			{
				return
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{C}-[<FormatNumber(<Divide(""<Collection.AnotherDecimal>"",""<Collection.AnotherDecimal>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NParamtersMacroResultString
		{
			get
			{
				return
@"{C}-[0.13]
{C}-[0.13]
{C}-[2.86]
{C}-[-0.29]
{C}-[-1.43]
{C}-[1.43]";
			}
		}

		protected override string NParamtersNestedMacroTemplateString
		{
			get
			{
				return
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{C}-[<FormatNumber(<Divide(""<Collection.AnotherDecimal>"",""<Subtract(""3"",""4"",""1"")>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NParamtersNestedMacroResultString
		{
			get
			{
				return
@"{C}-[-0.13]
{C}-[-0.31]
{C}-[-1.00]
{C}-[-0.18]
{C}-[3.57]
{C}-[3.57]";
			}
		}

		protected override string NestedMacroTemplateStringFrenchFormat
		{
			get
			{
				return
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{C}-[<Divide(""<Collection.AnotherDecimal>"",""<Divide(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>"")>]
{C}-[<Divide(""3,"",""<Divide(""-8,00"",""4,0"")>"")>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NestedMacroResultStringFrenchFormat
		{
			get
			{
				return
@"{C}-[8]
{C}-[-1,5]
{C}-[8]
{C}-[-1,5]
{C}-[0,35]
{C}-[-1,5]
{C}-[-3,5000000000000000000000000004]
{C}-[-1,5]
{C}-[-0,7]
{C}-[-1,5]
{C}-[0,7]
{C}-[-1,5]";
			}
		}

		protected override string[] ReplacementTexts
		{
			get
			{
				return new string[] {
					@"<Divide(""123.3232"",""23.3232"",""20"")>",
					@"<Divide(""1000000000000"",""0.00000000001"",""0.00000000001"")>",
					@"<Divide(""0.00000000001"",""1000000000000"",""1000000000000"")>",
					@"<Divide(""-2.77556E-17"",""-1"")>",
					@"<Divide("""",""-1"")>",
					@"<Divide("""","""")>",
					@"<Divide(""52"",""0"")>",
					@"<Divide(""5"",""0"",""2"")>",
				};
			}
		}

		protected override string[] ReplacementValues
		{
			get
			{
				return new string[] {
					"0.2643788159429237840433559717",
					string.Empty,
					"0",
					"0.0000000000000000277556",
					string.Empty,
					string.Empty,
					string.Empty,
					string.Empty,
				};
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Divide();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TotalOSAmount", "160"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("OSOutstandingAmount", "2"));
		}

		public void TestDivideByZeroReportsErrorAndReturnsEmpty()
		{
			CombineAssertions(() =>
			{
				AssertDivisionByZeroError("<Divide(\"1\", \"0\")>");
				AssertDivisionByZeroError("<Divide(\"0\", \"0\")>");
			});
		}

		void AssertDivisionByZeroError(string macro)
		{
			var expectedErrorMessage = string.Format(CultureInfo.InvariantCulture, @"Severity: [Warning (without error report)] Message: [Error in Divide Macro: Attempted to divide by zero. Input macro: [{0}]]", macro);

			var result = new MacroTranslator(Report).GetValue(macro, Passes.FirstPass);
			AssertEquals("Division by zero should report error", expectedErrorMessage, Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			AssertEquals("Division by zero should result in the empty string", string.Empty, result);

			Report.ErrorManager.ClearErrors();
		}
	}
}
