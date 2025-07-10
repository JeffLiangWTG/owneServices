using Enterprise.DocumentEngine.ValueReplacers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(Add))]
	sealed class AddTest : MathOperationProviderTest
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
{C}-[<FormatNumber(<Add(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string ResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[3.75]
{C}-[7.50]
{C}-[11.25]";
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
{C}-[<FormatNumber(<Add(""<Collection.AnotherDecimal>"",""<Add(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>"")>, AUD)>]
{C}-[<Add(""3"",""<Add(""-6"",""4"")>"")>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NestedMacroResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[1]
{C}-[6.25]
{C}-[1]
{C}-[12.50]
{C}-[1]
{C}-[18.75]
{C}-[1]";
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
{C}-[<FormatNumber(<Add(""<Collection.AnotherDecimal>"",""<Collection.AnotherDecimal>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NParamtersMacroResultString
		{
			get
			{
				return
@"{C}-[0.00]
{C}-[6.25]
{C}-[12.50]
{C}-[18.75]";
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
{C}-[<FormatNumber(<Add(""<Collection.AnotherDecimal>"",""<Subtract(""3"",""4"",""1"")>"",""<Collection.Money>"")>, AUD)>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NParamtersNestedMacroResultString
		{
			get
			{
				return
@"{C}-[-2.00]
{C}-[1.75]
{C}-[5.50]
{C}-[9.25]";
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
{C}-[<Add(""<Collection.AnotherDecimal>"",""<Add(""<Collection.AnotherDecimal>"",""<Collection.Money>"")>"")>]
{C}-[<Add(""3,2"",""<Add(""-8,4"",""4,2"")>"")>]
{A}-[#EndOfReport]";
			}
		}

		protected override string NestedMacroResultStringFrenchFormat
		{
			get
			{
				return
@"{C}-[0]
{C}-[-1]
{C}-[6,25]
{C}-[-1]
{C}-[12,5]
{C}-[-1]
{C}-[18,75]
{C}-[-1]";
			}
		}

		protected override string[] ReplacementTexts
		{
			get { return new string[] { @"<Add(""123.3232"",""23.3232"",""20"")>" }; }
		}

		protected override string[] ReplacementValues
		{
			get { return new string[] { "166.6464" }; }
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new Add();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("GenericTransactionHeader.TotalOSAmount", 40));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("GenericTransactionHeader.OSOutstandingAmount", 30));
		}
	}
}
