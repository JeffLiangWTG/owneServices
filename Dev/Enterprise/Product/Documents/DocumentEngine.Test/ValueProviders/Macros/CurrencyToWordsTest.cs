using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Integration;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrencyToWords))]
	sealed class CurrencyToWordsTest : ValueProviderTest
	{
		public void TestIsResponsibleForReplacing()
		{
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<currency To Words(12, USD,,SAF)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<currency To Words(12, USD, ,SAF)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<currency To Words(12, USD,,,SAF)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<currency To Words(12, USD, , , SAF)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<currency To Words(12, USD, EN-US, SAF)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<currency To Words(12, USD, EN-US)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<currency To Words(12, USD, ZH-CN)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<currency To Words(12, USD)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CUrrencyToWords(11.22, RLS)>", Passes.FirstPass));
			Assert(ValueProviderToTest.IsResponsibleForReplacing("<CURRENCY TO WORDS(12, EUR)>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<CURRENCY To Words( )>", Passes.FirstPass));
			Assert(!ValueProviderToTest.IsResponsibleForReplacing("<Currency To Words(0)>", Passes.FirstPass));
		}

		public void TestParseDoubleReplacement()
		{
			Culture.Default.NumberFormat = Culture.GetCulture("US").NumberFormat;
			AssertEquals("", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(439.842.112,52, USD)>", Report));
		}

		public void TestReplacement()
		{
			AssertEquals("twelve dollars only", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12, USD)>", Report));
			AssertEquals("one dollar only", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(1, USD)>", Report));
			AssertEquals("one dollar and 50 cents", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(1.5, USD)>", Report));
			AssertEquals("zero dollars only", ValueProviderToTest.GetReplacement("<currency TO WORDS(0, USD)>", Report));
			AssertEquals("thirty two dollars only", ValueProviderToTest.GetReplacement("<currency TO WORDS(32, USD)>", Report));
			AssertEquals("twelve dollars and 20 cents", ValueProviderToTest.GetReplacement("<currency TO WORDS(12.2, USD)>", Report));
			AssertEquals("twelve dollars and 20 cents", ValueProviderToTest.GetReplacement("<currency TO WORDS(12.2, USD, 'EN-US')>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<currency TO WORDS( , )>", Report));
			AssertEquals("one hundred and twenty three dollars and fifty six cents", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(123.56, USD, ENG, LTR)>", Report));
			AssertEquals("cent vingt-trois ariarys et cinquante-six ", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(123.56, MGA, FR_FR, LTR)>", Report));
			AssertEquals("cent vingt-trois ariarys et cinquante-six ", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(123.56, MGA, FR_FR, lTr)>", Report));
			AssertEquals("twelve dollars and 20 cents", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12.2, USD,,TEST)>", Report));
		}

		public void TestReplacementNestedMacro()
		{
			Report.MacroTranslator.RegisterValueProvider(new Enterprise.DocumentEngine.ValueReplacers.FixedValueProvider("Amount", 12));

			Report.MacroTranslator.RegisterValueProvider(new Enterprise.DocumentEngine.ValueReplacers.FixedValueProvider("Currency", "USD"));
			AssertEquals("twelve dollars only", Report.MacroTranslator.GetValue("<CURRENCY TO WORDS(<Amount>, <Currency>)>", Passes.FirstPass));
		}

		public void TestReplacementWithOldLanguageCodes()
		{
			AssertEquals("zwölf dollar", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12, USD, GRM)>", Report));
			AssertEquals(
				 "Error when processing the macro <CURRENCY TO WORDS(12, USD, GRM)>: the language code GRM has been depreciated. Please use the ISO language code instead: DE-DE.",
				((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors()[0].Message);
		}

		public void TestReplacementDifferentLanguages()
		{
			AssertEquals("twelve dollars only", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12, USD, EN-US)>", Report));
			AssertEquals("zwölf dollar", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12, USD, DE-DE)>", Report));
			AssertEquals("zwölf dollar", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12,USD,DE-DE)>", Report));
			AssertEquals("zwölf dollar", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12,USD, DE-DE)>", Report));
			AssertEquals("zwölf dollar", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12, USD,DE-DE)>", Report));
			// AssertEquals("Ba M\u01B0\u01A1i N\u0103m", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(35, USD, VI-VN)>", Report));	VTN does not work because there are no VTN currency translations in the system
		}

		public void TestReplacementWithTotalMacro()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
	@"{A}-[#Config]
{A}-[Name=Test]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Text>]   {C}-[<Collection.Decimal>]
{A}-[#SectionFooter]
{B}-[Total in words : <CurrencyToWords(<Total Collection.Decimal>, EUR)>]  {C}-[<Total Collection.Decimal>]
{A}-[#EndOfReport]");
			template.SO_DataContext = "UnitTest";

			var dummy = Factory.New<DummyDocumentSupportable>();
			for (var index = 0; index < 10; index++)
			{
				var child = dummy.Collection.AddNew();
				child.Z0_VarCharMax = index.ToString();
				child.Z0_Decimal = 1.0;
			}

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var pivot = documentCommand.Documents.AddNew();
			pivot.SI_SO = template.PK;
			pivot.SI_SU = documentCommand.PK;

			var printJob = DeliveryTestHelper.DeliverDocument(documentCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				Assert(excelInterface.WorkSheets.First().ToString().Contains("[Total in words : ten euros only]"));
			}
		}

		public void TestReplacementSouthAsiaPattern()
		{
			AssertEquals("twelve only", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12, INR, EN, SAF)>", Report));
			AssertEquals("one only", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(1, INR, EN, SAF)>", Report));
			AssertEquals("one and paise fifty", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(1.5, INR, EN-US, SAF)>", Report));
			AssertEquals("twelve and paise twenty four", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(12.24, INR, EN-US, SAF)>", Report));
			AssertEquals("one lakh, twenty three thousand, five hundred and thirty four and paise seventy eight", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(123534.78, INR, EN-US, SAF)>", Report));
			AssertEquals("ten lakh only", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(1000000, INR, EN-US, SAF)>", Report));
			AssertEquals("nine crore, ninety nine lakh, ninety nine thousand, nine hundred and ninety nine only", ValueProviderToTest.GetReplacement("<CURRENCY TO WORDS(99999999, INR, EN-US, SAF)>", Report));
		}

		public void TestReplacementSouthAsiaPatternWorksOnlyWhenReportLanguageIsEnglish()
		{
			Report.Parent.Language = Core.SharedConstants.Languages.EnglishAmerican;
			AssertEquals("one lakh, twenty three thousand, five hundred and thirty four and paise seventy eight", ValueProviderToTest.GetReplacement("<CurrencyToWords(123534.78, INR, EN-US, SAF)>", Report));
			AssertEquals("one lakh, twenty three thousand, five hundred and thirty four and paise seventy eight", ValueProviderToTest.GetReplacement("<CurrencyToWords(123534.78, INR, , SAF)>", Report));
			AssertEquals("one lakh, twenty three thousand, five hundred and thirty four and paise seventy eight", ValueProviderToTest.GetReplacement("<CurrencyToWords(123534.78, INR,,SAF)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.French;
			AssertEquals("cent vingt-trois mille cinq cent trente-quatre rupees et 78 paise", ValueProviderToTest.GetReplacement("<CurrencyToWords(123534.78, INR, , SAF)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Dutch;
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "INR"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_UnitName", currency.PK, "NL-NL", "RX", "roepie");
			Factory.Save();
			AssertEquals("honderd-en-drieëntwintigduizend vijfhonderd-en-vierendertig roepie en 78 paise", ValueProviderToTest.GetReplacement("<CurrencyToWords(123534.78, INR, , SAF)>", Report));
		}

		public void TestReplacementInReportLanguage()
		{
			Report.Parent.Language = Core.SharedConstants.Languages.Spanish;
			AssertEquals("diecinueve con 00/100 euros", ValueProviderToTest.GetReplacement("<CurrencyToWords(19, EUR)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Portuguese;
			AssertEquals("dezenove euros", ValueProviderToTest.GetReplacement("<CurrencyToWords(19, EUR)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Italian;
			AssertEquals("diciannove euros", ValueProviderToTest.GetReplacement("<CurrencyToWords(19, EUR)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.French;
			AssertEquals("dix-neuf euros", ValueProviderToTest.GetReplacement("<CurrencyToWords(19, EUR)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Dutch;
			AssertEquals("negentien euro", ValueProviderToTest.GetReplacement("<CurrencyToWords(19, EUR)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Turkish;
			AssertEquals("yalnizondokuzeuro", ValueProviderToTest.GetReplacement("<CurrencyToWords(19, EUR)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.Bulgarian;
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "EUR"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_UnitName", currency.PK, "BG-BG", "RX", "евро");
			helper.CreateRefLanguageText("RX_Desc", currency.PK, "ZH-CN", "RX", "欧元");
			helper.CreateRefLanguageText("RX_Desc", currency.PK, "ZH-TW", "RX", "歐元");
			Factory.Save();
			AssertEquals("деветнадесет евро", ValueProviderToTest.GetReplacement("<CurrencyToWords(19, EUR)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.ChineseSimplified;
			AssertEquals("欧元壹拾贰元整", ValueProviderToTest.GetReplacement("<CurrencyToWords(12, EUR)>", Report));
			Report.Parent.Language = Core.SharedConstants.Languages.ChineseTraditional;
			AssertEquals("歐元壹拾貳元整", ValueProviderToTest.GetReplacement("<CurrencyToWords(12, EUR)>", Report));
		}

		public void TestAllEnglishVariants()
		{
			foreach (ICodeDescription language in new AvailableDocBuilderLanguageList(Factory))
			{
				if (Res.IsEnglish(language.Code))
				{
					using (Res.TemporarilySwitchLanguage(language.Code))
					{
						AssertEquals("twelve dollars only", ValueProviderToTest.GetReplacement("<CurrencyToWords(12, USD)>", Report));
					}
				}
			}
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new CurrencyToWords();
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new Enterprise.DocumentEngine.ValueReplacers.FixedValueProvider("Charges.Value", 12.5));
			Report.MacroTranslator.RegisterValueProvider(new Enterprise.DocumentEngine.ValueReplacers.FixedValueProvider("Charges.CurrencyCode", "USD"));
		}
	}
}
