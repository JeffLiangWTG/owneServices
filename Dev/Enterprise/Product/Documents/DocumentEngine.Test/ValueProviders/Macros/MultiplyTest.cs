using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Multiply))]
	sealed class MultiplyTest : MathOperationProviderTest
	{
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
{C}-[<FormatNumber(<Multiply(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string ResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[3.13]
{C}-[12.50]
{C}-[28.13]";
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
{C}-[<FormatNumber(<Multiply(""<Collection.AnotherDecimal>"",""<Multiply(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>"")>, AUD)>]
{C}-[<Multiply(""3"",""<Multiply(""-6"",""4"")>"")>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NestedMacroResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[-72]
{C}-[7.81]
{C}-[-72]
{C}-[62.50]
{C}-[-72]
{C}-[210.94]
{C}-[-72]";
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
{C}-[<FormatNumber(<Multiply(""<Collection.AnotherDecimal>"",""<Collection.AnotherDecimal>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NParamtersMacroResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[7.81]
{C}-[62.50]
{C}-[210.94]";
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
{C}-[<FormatNumber(<Multiply(""<Collection.AnotherDecimal>"",""<Subtract(""3"",""4"",""1"")>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NParamtersNestedMacroResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[-6.25]
{C}-[-25.00]
{C}-[-56.25]";
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
{C}-[<Multiply(""<Collection.AnotherDecimal>"",""<Multiply(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>"")>]
{C}-[<Multiply(""3,"",""<Multiply(""-8,00"",""4,0"")>"")>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NestedMacroResultStringFrenchFormat
		{
			get
			{
				return
@"{C}-[0]
{C}-[-96]
{C}-[7,8125]
{C}-[-96]
{C}-[62,5]
{C}-[-96]
{C}-[210,9375]
{C}-[-96]";
			}
		}

		protected override string[] ReplacementTexts
		{
			get
			{
				return new string[] {
					@"<Multiply(""123.3232"",""23.3232"",""20"")>",
					@"<Multiply(""0.00000000001"",""0.00000000001"",""0.00000000001"")>",
					@"<Multiply(""1000000000000"",""1000000000000"",""1000000000000"")>",
					@"<Multiply(""-2.77556E-17"",""-1"")>",
					@"<Multiply("""",""-1"")>",
					@"<Multiply("""","""")>",
				};
			}
		}

		protected override string[] ReplacementValues
		{
			get
			{
				return new string[] {
					"57525.8331648",
					"0",
					string.Empty,
					"0.0000000000000000277556",
					string.Empty,
					string.Empty
				};
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Multiply();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TotalOSAmount", "10"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("OSOutstandingAmount", "20"));
		}
	}
}
