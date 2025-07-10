using CargoWise.EntityFramework;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(CurrencyLanguageName))]
	sealed class CurrencyLanguageNameTest : ValueProviderWithLoadControlFactoryTest<CurrencyLanguageName>
	{
		public override void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should match <CurrencyLanguageName( USD, ZH-CN)>", ValueProviderToTest.IsResponsibleForReplacing("<CurrencyLanguageName( USD, ZH-CN)>", Passes.FirstPass));
			Assert("should match < Currency Language Name ( USD , ZH-CN )   >", ValueProviderToTest.IsResponsibleForReplacing("<Currency Language Name ( USD , ZH-CN )  >", Passes.FirstPass));
			Assert("should match <   currency languagename ( USD , CHS )   >", ValueProviderToTest.IsResponsibleForReplacing("<   currency languagename ( USD , ZH-CN )   >", Passes.SecondPass));
			Assert("should match <   currency languagename ( USD ,  )   >>", ValueProviderToTest.IsResponsibleForReplacing("<   Currency Languagename ( USD ,  )   >", Passes.SecondPass));
		}

		public override void TestReplacement()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_Desc", currency.PK, "ZH-CN", "RX", "美元");
			Factory.Save();
			PrepareRenderer();
			SetupReport();

			var returnedValue = ValueProviderToTest.GetReplacement("<CurrencyLanguageName(USD,ZH-CN)>", Report);
			AssertEquals("美元", returnedValue);

			var emptyValue = ValueProviderToTest.GetReplacement("<CurrencyLanguageName(, ZH-CN)>", Report);
			AssertEquals("Should return an empty string if no value is passed", "", emptyValue.ToString());

			var returnedValueOfOldLanguageCode = ValueProviderToTest.GetReplacement("<CurrencyLanguageName(USD,CHS)>", Report);
			AssertEquals("美元", returnedValueOfOldLanguageCode);
			AssertEquals(
				"Error when processing the macro <CurrencyLanguageName(USD,CHS)>: the language code CHS has been depreciated. Please use the ISO language code instead: ZH-CN.",
				((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors()[0].Message);
		}

		public void TestReplacementWithCADCurrency()
		{
			var currency = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "CAD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_Desc", currency.PK, "ZH-CN", "RX", "加元");
			Factory.Save();
			PrepareRenderer();
			SetupReport();

			var returnedValue = ValueProviderToTest.GetReplacement("<CurrencyLanguageName(CAD,ZH-CN)>", Report);
			AssertNotEquals("美元", returnedValue);
			AssertEquals("加元", returnedValue);
		}

		void SetupReport()
		{
			var testSectionBodyArea = new SectionBodyArea(1, 2, Report, "#SectionBody:DATA=Tbl");
			var testGroupByAreaArea = new GroupByArea(3, 4, Report, "#GroupBy:Fld") { DataParent = testSectionBodyArea };
			testSectionBodyArea.ExpandForDataRows(4);
			testSectionBodyArea.SetWorksheetForFormulaProvider(Report.WorkSheetCurrentlyBeingProcessed);
			testSectionBodyArea.FormulaProvider.AddColumn(1, "Tbl.Tst", 0);
			Report.Renderer.CurrentAreaToProcess = testGroupByAreaArea;
		}

		protected override void PrepareDataForExamplesEvaluate()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("CurrencyCode", "CAD"));
		}
	}
}
