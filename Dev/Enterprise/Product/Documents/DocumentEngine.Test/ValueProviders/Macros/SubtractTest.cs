using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Subtract))]
	sealed class SubtractTest : MathOperationProviderTest
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
{C}-[<FormatNumber(<Subtract(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string ResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[1.25]
{C}-[2.50]
{C}-[3.75]";
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
{C}-[<FormatNumber(<Subtract(""<Collection.AnotherDecimal>"",""<Add(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>"")>, AUD)>]
{C}-[<Subtract(""3"",""<Subtract(""3"",""4"")>"")>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NestedMacroResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[4]
{C}-[-1.25]
{C}-[4]
{C}-[-2.50]
{C}-[4]
{C}-[-3.75]
{C}-[4]";
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
{C}-[<FormatNumber(<Subtract(""<Collection.AnotherDecimal>"",""<Collection.AnotherDecimal>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NParamtersMacroResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[-1.25]
{C}-[-2.50]
{C}-[-3.75]";
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
{C}-[<FormatNumber(<Subtract(""<Collection.AnotherDecimal>"",""<Add(""-3"",""4"",""1"")>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NParamtersNestedMacroResultString
		{
			get
			{
				return
@"{C}-[-2.00]
{C}-[-0.75]
{C}-[0.50]
{C}-[1.75]";
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
{C}-[<Subtract(""<Collection.AnotherDecimal>"",""<Add(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>"")>]
{C}-[<Subtract(""2,25"",""<Subtract(""3,25"",""5,00000"")>"")>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NestedMacroResultStringFrenchFormat
		{
			get
			{
				return
@"{C}-[0]
{C}-[4]
{C}-[-1,25]
{C}-[4]
{C}-[-2,5]
{C}-[4]
{C}-[-3,75]
{C}-[4]";
			}
		}

		protected override string[] ReplacementTexts
		{
			get { return new string[] { @"<Subtract(""123.3232"",""23.3232"",""20"")>" }; }
		}

		protected override string[] ReplacementValues
		{
			get { return new string[] { "80" }; }
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Subtract();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TotalOSAmount", 100));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("OSOutstandingAmount", 20));
		}
	}
}
