using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(FormatNumber))]
	sealed class FormatNumberTest : ValueProviderWithLoadControlFactoryTest<FormatNumber>
	{
		public void TestTotalMacroInsideNumberFormatMacro()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[HideColumnIf]   {D}-[1==1]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{C}-[<FormatNumber(<Collection.Decimal>, AUD)>]   {D}-[<Collection.Decimal>]
{A}-[#GroupBy:Collection.Code]
{B}-[Total]   {C}-[<FormatNumber(<Total Collection.Decimal>, AUD)>]   {D}-[<Total Collection.Decimal>]
{A}-[#EndOfReport]", "UnitTest");

			var dummy = factory.New<DummyDocumentSupportable>();
			for (var index = 0; index < 4; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_Code = "Test";
				child.Z0_Decimal = index + 1;
			}

			var documentCommand = factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("Total should be calculated correctly.",
@"{C}-[1.00]
{C}-[2.00]
{C}-[3.00]
{C}-[4.00]
{B}-[Total]   {C}-[10.00]",
					workSheet.ToString());
			}
		}

		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("should not match <   Format  Number (1.2231123)   >", !ValueProviderToTest.IsResponsibleForReplacing("<   Format  Number (1.2231123)   >", Passes.SecondPass));
			Assert("should match <FormatNumber(12.33, USD)>", ValueProviderToTest.IsResponsibleForReplacing("<FormatNumber(12.33, USD)>", Passes.SecondPass));
			Assert("should match <FormatNumber(12.3456, 3)>", ValueProviderToTest.IsResponsibleForReplacing("<FormatNumber(12.3456, 3)>", Passes.SecondPass));
			Assert("should match <FormatNumber(12.3456, -1)>", ValueProviderToTest.IsResponsibleForReplacing("<FormatNumber(12.3456, -1)>", Passes.SecondPass));
		}

		public void TestFormatAmountWithInvalidDecimalPlaces()
		{
			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.France)))
			{
				Assert(!Report.ErrorManager.HasErrors);

				GlbCompany.CurrentCompany.GC_IsReciprocal = true;
				AssertEquals("12 412 351,345679", ValueProviderToTest.GetReplacement("<FormatNumber(12412351.3456789, -1)>", Report));
				AssertNull(ValueProviderToTest.GetReplacement("<FormatNumber(12412351.3456789, 100)>", Report));

				GlbCompany.CurrentCompany.GC_IsReciprocal = false;
				AssertEquals("12 412 351,345679", ValueProviderToTest.GetReplacement("<FormatNumber(12412351.3456789, -1)>", Report));
				AssertNull(ValueProviderToTest.GetReplacement("<FormatNumber(12412351.3456789, 100)>", Report));

				Assert(Report.ErrorManager.HasErrors);
				AssertContains("Decimal places can not be greater than 99.", Report.ErrorManager.ToString());
			}
		}

		public void TestFormatAmountShouldNotChangeCurrentNumberFormat()
		{
			var currentNumberFormat = Culture.CurrentCompanyCountryCulture.NumberFormat;

			AssertEquals("$", currentNumberFormat.CurrencySymbol);
			AssertEquals(2, currentNumberFormat.CurrencyDecimalDigits);

			var formatResult = ValueProviderToTest.GetReplacement("<FormatNumber(12412351.33, 1)>", Report);
			AssertEquals("12,412,351.3", formatResult);

			AssertEquals("$", currentNumberFormat.CurrencySymbol);
			AssertEquals(2, currentNumberFormat.CurrencyDecimalDigits);
		}

		public override void TestReplacement()
		{
			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.France)))
			{
				AssertEquals("12 412 351,33", ValueProviderToTest.GetReplacement("<FormatNumber(12412351.33, USD)>", Report));
				AssertEquals("load'o'crap", ValueProviderToTest.GetReplacement("<FormatNumber(load'o'crap, USD)>", Report));
				AssertEquals("12 412 351,346", ValueProviderToTest.GetReplacement("<FormatNumber(12412351.34567, 3)>", Report));
			}

			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.HongKong)))
			{
				AssertEquals("0.00", ValueProviderToTest.GetReplacement("<FormatNumber(4.540197384717E-11, HKD)>", Report));
			}
		}

		public void TestNonBreakingSpacesAreReplacedWithClassicSpaces()
		{
			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.Australia)))
			{
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(1234.56m, "", "PLN", "Arial", "1,234.56");
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(1234.56m, "", "PLN", "Lucida Console", "1,234.56");
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(1234.56m, "", "AU", "Arial", "1,234.56");
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(1234.56m, "", "AU", "Lucida Console", "1,234.56");
			}

			using (Env.CurrentCompany.Country.SetCultureForTest(Culture.GetCulture(Core.Constants.CountryCodes.Poland)))
			{
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(1234.56m, "Amount should contain non-breaking spaces", "PLN", "Arial", string.Format("1{0}234,56", (char)160));
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(1234.56m, "Amount should contain non-breaking spaces", "AU", "Arial", string.Format("1{0}234,56", (char)160));
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(1234.56m, "Amount should contain classic spaces", "PLN", "Lucida Console", string.Format("1{0}234,56", (char)32));
				AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(1234.56m, "Amount should contain classic spaces", "AU", "Lucida Console", string.Format("1{0}234,56", (char)32));
			}
		}

		void AssertNonBreakingSpacesAreReplacedWithClassicSpacesWhenRequired(decimal amount, string assertComment, string currencyCode, string font, string expectedResult)
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
string.Format(@"{{A}}-[#Config]
{{A}}-[Name=Test]
{{A}}-[DataContext=UnitTest]
{{A}}-[#SectionBody:Data=Collection]
{{B}}-[<FormatNumber(<Collection.AnotherDecimal>, {0})>]]
{{A}}-[#EndOfReport]", currencyCode), "UnitTest");

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(template.SO_Template);
				var workSheet = excelInterface.WorkSheets.First();
				var format = workSheet.GetCellFormat(4, 1);
				format.FontName = font;
				workSheet.SetCellFormat(4, 1, format);

				using (var stream = new MemoryStream())
				{
					excelInterface.SaveToStream(stream);
					template.SO_Template = stream.CopyToByteArray();
				}
			}

			var dummy = Factory.New<DummyDocumentSupportable>();
			var child = dummy.Collection.AddNew();
			child.Z0_AnotherDecimal = amount;

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();
				AssertMultilineASCIIEquals(assertComment, string.Format("{{B}}-[{0}]", expectedResult), workSheet.ToString());
			}
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Amount", 12412351.33));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("CurrencyCode", "USD"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("DecimalPlaces", 1));
		}
	}
}
