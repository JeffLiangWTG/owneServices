using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.DataProviders;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.Environment;
using Enterprise.Environment.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.ValueProviders.Testing
{
	[TestsSubclassesOf(typeof(ValueProvider), ExcludePrivate = true)]
	public abstract class ValueProviderTest : TestCaseWithFactory
	{
		public virtual void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro()
		{
			if (ValueProviderToTest.NeedCheckForScientificNotationExposed())
			{
				Assert("if NeedCheckForScientificNotation is true, the test case should be overridden.", false);
			}
			Assert(true);
		}

		public virtual void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro_WhenCultureChanges_ShouldConvertToStringWithoutScientificNotation()
		{
			if (ValueProviderToTest.NeedCheckForScientificNotationExposed())
			{
				Assert("if NeedCheckForScientificNotation is true, the test case should be overridden.", false);
			}
			Assert(true);
		}

		public void RunInNullEnvironment(Action assertions)
		{
			var previous = Env.GetCurrentProvider();
			try
			{
				using (var provider = new NullEnvProvider())
				{
					provider.Enable();

					assertions();
				}
			}
			finally
			{
				previous.Enable();
			}
		}

		public virtual void TestDocumentation()
		{
			var currencyCAD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "CAD"));
			var currencyUSD = Factory.LoadTop1<RefCurrency>(new ZQuery(RefCurrencySchema.RX_Code, "USD"));
			var helper = new RefCurrencyTestHelper(Factory);
			helper.CreateRefLanguageText("RX_Desc", currencyCAD.PK, "ZH-CN", "RX", "加元");
			helper.CreateRefLanguageText("RX_Desc", currencyUSD.PK, "ZH-CN", "RX", "美元");
			helper.CreateRefLanguageText("RX_UnitName", currencyUSD.PK, "AR-AE", "RX", "دولار");
			helper.CreateRefLanguageText("RX_SubUnitName", currencyUSD.PK, "AR-AE", "RX", "سنتات");
			Factory.Save();

			var macroName = ValueProviderToTest.GetType().Name;
			var examplesAndResults = ValueProviderToTest.Documentation.ExamplesAndResults;
			AssertNotNull("Value Provider should implement ValueProvider", ValueProviderToTest);
			AssertNotNullOrEmpty("GetDocumentation() on [class " + macroName + "] must be implemented to show the Useage.", ValueProviderToTest.Documentation.Useage);
			AssertNotNullOrEmpty("GetDocumentation() on [class " + macroName + "] must be implemented to show an Explanation.", ValueProviderToTest.Documentation.Explanation);
			Assert("GetDocumentation() on [class " + macroName + "] must be implemented to show at least one example.", examplesAndResults?.Count > 0);

			PrepareDataForExamplesEvaluate();
			CombineAssertions
			(() =>
				{
					var regexToMatch = ValueProviderToTest is INonVisualisableValueProvider nonVisualisableValueProvider ? nonVisualisableValueProvider.RegexToReplaceMacro : ValueProviderToTest.Regex;
					var examplesNotMatch = examplesAndResults.Select(e => e.example).Where(q => !regexToMatch.IsMatch(q));
					Assert($"Examples: {string.Join(" ", examplesNotMatch)} don't match the regex of current macro.", !examplesNotMatch.Any());
					if (!(ValueProviderToTest is INonVisualisableValueProvider))
					{
						foreach (var exampleAndResultPair in examplesAndResults)
						{
							AssertExamplesAreReplacedAsExpected(exampleAndResultPair.example, exampleAndResultPair.expectedResult);
						}
					}
				});
		}

		protected virtual void PrepareDataForExamplesEvaluate() { }

		protected virtual void AssertExamplesAreReplacedAsExpected(string example, object expectedResult)
		{
			using (((ReportRenderer)Report.Renderer).TemporarilySwitchCurrentPassForTest(PassToReplaceExample))
			{
				var actualResult = Report.MacroTranslator.GetValue(example, PassToReplaceExample);
				AssertEquals($"Example {example} should be replaced correctly", expectedResult, actualResult);
			}
		}

		protected virtual Passes PassToReplaceExample => ValueProviderToTest.PassToStartReplacingOn;

		public virtual void TestExistsInValueProviderCollection()
		{
			if (!ValueProviderType.IsAbstract)
			{
				AssertEquals(string.Format("Provider of type {0} could not be found in ValueProviderCollection. Please add to class ValueProviderCollector.", ValueProviderType.Name), true, ExistsInValueProviderCollection);
			}
			else
			{
				Assert("ValueProvider cannot exist in collection if type is abstract", true);
			}
		}

		bool ExistsInValueProviderCollection
		{
			get
			{
				ValueProviderCollector collector = new ValueProviderCollector();
				foreach (ValueProvider provider in collector.ValueProviders.Providers)
				{
					if (provider.GetType() == ValueProviderType)
					{
						return true;
					}
				}
				return false;
			}
		}

		public void TestRegexIsPreCompiled()
		{
			Assert("Regex should have RegexOptions.Compiled as its options.", ((int)GetNewValueProvider().Regex.Options & (int)RegexOptions.Compiled) != 0);
		}

		public void TestRegexIsStatic()
		{
			ValueProvider vP1 = GetNewValueProvider();
			ValueProvider vP2 = GetNewValueProvider();
			Assert("GetValueProvider should return a new object each time.", vP1 != vP2);
			AssertSame(vP1.Regex, vP2.Regex);
		}

		public void TestMacroRegexIsCorrect()
		{
			string regexToTest = ValueProviderToTest.Regex.ToString().Trim();
			if (ValueProviderToTest.Regex.Options.HasFlag(RegexOptions.IgnorePatternWhitespace))
			{
				regexToTest = Regex.Replace(regexToTest, "#.*", "").Replace(" ", "").Replace("\r", "").Replace("\n", "").Replace("\t", "");
			}
			AssertEquals("The regex " + regexToTest + " should contain the start '^' and end '$' character.", true, regexToTest.StartsWith("^") && regexToTest.EndsWith("$"));
			AssertEquals("The regex " + regexToTest + " should contain only one start '^' and one end '$' character.", true, !regexToTest.StartsWith("^^") && !regexToTest.EndsWith("$$"));
		}

		public virtual void TestTemplateVisualiserComponentType()
		{
			AssertEquals(VisualiserComponentTypes.StaticText, GetNewValueProvider().ComponentType);
		}

		public void TestRegexIsCultureInvariant()
		{
			AssertEquals(RegexOptions.CultureInvariant, GetNewValueProvider().Regex.Options & RegexOptions.CultureInvariant);
		}

		public void TestINonVisualisableValueProviderThatModifyDocumentLayout()
		{
			var valueProvider = GetNewValueProvider();
			if (valueProvider is INonVisualisableValueProviderThatModifyDocumentLayout)
			{
				Assert(valueProvider.Documentation.Explanation.ToString().Contains("This macro will not work if it is used together with TFormula macro"));
			}
			else
			{
				AssertNullOrEmpty(valueProvider.GetType().GetProperty("AdditionalDocumentation", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(valueProvider).ToString());
			}
		}

		public virtual void TestProviderResetProperlyAndShouldNotHoldData()
		{
			FieldInfo[] fields = GetNewValueProvider().GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
			if (fields.Length > 0)
			{
				foreach (FieldInfo fieldInfo in fields)
				{
					Assert(string.Format(@"
Missing field {0} in FieldCollection.
Please override FieldCollection in your test class when there is field(s) in your value provider class. This class is to make developer be aware that the data hold in field(s) of value provider could affect the macro translation 
because now they are shared and reused between different reports when using TextMacroProcessor. Override ValueProvider.Reset() method to clear the data hold in field(s) when necessary to prevent this issue from happening.
To make this test pass, just add FieldInfo of all the fields to FieldCollection, and reset field in ValueProvider.Reset() if needed.", fieldInfo.Name), FieldCollection.Exists(f => f.Name == fieldInfo.Name && f.FieldType == fieldInfo.FieldType));
				}
			}
			else
			{
				Assert(true);
			}
		}

		protected virtual List<FieldInfo> FieldCollection { get { return new List<FieldInfo>(); } }

		public void TestTryParseWithReportOnFail_CustomMessage()
		{
			var goodDecimalResult = TestTryParseWithReportOnFailCore(Report, 9001m, () => decimal.Parse("9"), "this is a message", false);
			AssertEquals(9m, goodDecimalResult);
			AssertEquals(false, Report.ErrorManager.HasErrors);

			var badDecimalResult = TestTryParseWithReportOnFailCore(Report, 9001m, () => decimal.Parse("9a"), "this is a message", false);
			AssertEquals(9001m, badDecimalResult);
			AssertEquals(true, Report.ErrorManager.HasErrors);
			AssertContains("Error in TestValueProvider Macro: this is a message", ((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors()[0].Message);
		}

		public void TestTryParseWithReportOnFail_ReportException()
		{
			var goodDecimalResult = TestTryParseWithReportOnFailCore(Report, 9001m, () => decimal.Parse("9"), string.Empty, true);
			AssertEquals(9m, goodDecimalResult);
			AssertEquals(false, Report.ErrorManager.HasErrors);

			var badDecimalResult = TestTryParseWithReportOnFailCore(Report, 9001m, () => decimal.Parse("9a"), string.Empty, true);
			AssertEquals(9001m, badDecimalResult);
			AssertEquals(true, Report.ErrorManager.HasErrors);
#if NET
			AssertContains("Error in TestValueProvider Macro: The input string '9a' was not in a correct format.",
				((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors()[0].Message);
#else
			AssertContains("Error in TestValueProvider Macro: Input string was not in a correct format.",
				((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors()[0].Message);
#endif

		}

		public void TestTryParseWithReportOnFail_ReportFormattedException()
		{
			var goodDecimalResult = TestTryParseWithReportOnFailCore(Report, 9001m, () => decimal.Parse("9"), "error: {0}", true);
			AssertEquals(9m, goodDecimalResult);
			AssertEquals(false, Report.ErrorManager.HasErrors);

			var badDecimalResult = TestTryParseWithReportOnFailCore(Report, 9001m, () => decimal.Parse("9a"), "error: {0}", true);
			AssertEquals(9001m, badDecimalResult);
			AssertEquals(true, Report.ErrorManager.HasErrors);
#if NET
			AssertContains("Error in TestValueProvider Macro: error: The input string '9a' was not in a correct format.",
				((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors()[0].Message);
#else
			AssertContains("Error in TestValueProvider Macro: error: Input string was not in a correct format.",
				((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors()[0].Message);
#endif
		}

		public void TestTryParseWithReportOnFail_Bool()
		{
			var goodDecimalResult = TestTryParseWithReportOnFailCore(Report, false, () => bool.Parse("true"), string.Empty, false);
			AssertEquals(true, goodDecimalResult);
			AssertEquals(false, Report.ErrorManager.HasErrors);

			var badDecimalResult = TestTryParseWithReportOnFailCore(Report, false, () => bool.Parse("garbage"), string.Empty, false);
			AssertEquals(false, badDecimalResult);
			AssertEquals(true, Report.ErrorManager.HasErrors);
			AssertContains("Error in TestValueProvider Macro:", ((IHaveReportProcessingErrorsForGUI)Report.ErrorManager).GetErrors()[0].Message);
		}

		T TestTryParseWithReportOnFailCore<T>(Report report, T defaultValue, Func<T> func, string errorMessage, bool useExceptionMessage)
		{
			return new TestValueProvider().TryParseWithReportOnFail_ForTest(report, defaultValue, func, errorMessage, useExceptionMessage);
		}

		#region Assertions

		protected void AssertIsResponsibleForReplacing(string pattern, Passes pass = Passes.FirstPass)
		{
			AssertEquals("Should be responsible for replacing: " + pattern, true, ValueProviderToTest.IsResponsibleForReplacing(pattern, pass));
		}

		protected void AssertNotResponsibleForReplacing(string pattern, Passes pass = Passes.FirstPass)
		{
			AssertEquals("Should not be responsible for replacing: " + pattern, false, ValueProviderToTest.IsResponsibleForReplacing(pattern, pass));
		}

		protected void AssertIsReplacedWith(object expectedResult, string macro, Passes pass = Passes.FirstPass)
		{
			AssertIsReplacedWith("\"" + macro + "\" produced an unexpected result", expectedResult, macro, pass);
		}

		protected void AssertIsReplacedWith(string message, object expectedResult, string macro, Passes pass = Passes.FirstPass)
		{
			AssertIsResponsibleForReplacing(macro, pass);
			AssertEquals(message, expectedResult, ValueProviderToTest.GetReplacement(macro, Report));
		}

		protected void AssertMacroHasError(string expectedError, string macro, Passes pass = Passes.FirstPass)
		{
			AssertIsResponsibleForReplacing(macro, pass);
			ValueProviderToTest.GetReplacement(macro, Report);
			Assert(expectedError, Report.ErrorManager.HasErrors);
			Assert(expectedError, Report.ErrorManager.ToString().Contains(expectedError));
			Report.ErrorManager.ClearErrors();
		}

		#endregion

		#region Test Classes

		protected class DummyDataProvider : IDataProvider
		{
			IDataRowSource IDataProvider.GetDataRowSource(string tableIdentifier)
			{
				return ((IDataProvider)this).GetDataRowSource(tableIdentifier, false);
			}

			IDataRowSource IDataProvider.GetDataRowSource(string tableIdentifier, bool isForDataSection)
			{
				return null;
			}

			IDataRowSource IDataProvider.GetDataRowSource(string tableIdentifier, bool isForDataSection, int maximumNumberOfRows)
			{
				return null;
			}

			object IDataProvider.GetColumnValue(IDataRowSource dataSource, int rowIndex, string columnName, bool onlyForCurrentSource, Area area)
			{
				byte[] uncompressedBytes = { (byte)'a', (byte)'b', (byte)'c' };
				byte[] compressedBytes = Compressor.Compress(uncompressedBytes);
				if (columnName == "Col1")
				{
					return rowIndex + 1;
				}
				return compressedBytes;
			}

			public IDataRowSource DataSource
			{
				get { return new DummyDataSource(); }
			}
		}

		protected class DummyDataSource : IDataRowSource
		{
			public int RowCount
			{
				get { return 3; }
			}

			public IDataRowSource[] GroupBy(string[] columnName)
			{
				return null;
			}

			public IDataRowSource Filter(string expressions)
			{
				return null;
			}

			public IDataRowSource GetFirstNRows(int rowsToKeep)
			{
				return null;
			}

			public IDataRowSource Split(int rowsToKeep)
			{
				return null;
			}

			public int GroupCount(string[] columnNames)
			{
				return 0;
			}

			public IDataRowSource GetRowsFromIndexes(int[] indexes)
			{
				return null;
			}
		}

		class TestValueProvider : ValueProvider
		{
			public override Regex Regex
			{
				get
				{
					throw new NotImplementedException();
				}
			}

			protected override ValueProviderDocumenter GetDocumentation()
			{
				throw new NotImplementedException();
			}

			protected override object GetReplacementCore(string macro, Report report)
			{
				throw new NotImplementedException();
			}

			public T TryParseWithReportOnFail_ForTest<T>(Report report, T defaultValue, Func<T> func, string errorMessage, bool useExceptionMessage)
			{
				return TryParseWithReportOnFail(report, defaultValue, func, errorMessage, useExceptionMessage);
			}
		}

		#endregion

		#region Implementation

		protected abstract ValueProvider GetNewValueProvider();

		protected virtual Type ValueProviderType
		{
			get { return ValueProviderToTest.GetType(); }
		}

		protected ValueProvider ValueProviderToTest
		{
			get
			{
				if (fValueProviderToTest == null)
				{
					fValueProviderToTest = GetNewValueProvider();
				}
				return fValueProviderToTest;
			}
		}
		ValueProvider fValueProviderToTest;

		protected Report Report
		{
			get
			{
				if (fReport == null)
				{
					fReport = new Report(Pack, ExcelTemplate);
				}
				return fReport;
			}
			set
			{
				fReport = value;
			}
		}
		Report fReport;

		internal ExcelTemplateForUnitTesting ExcelTemplate
		{
			get
			{
				if (excelTemplate == null)
				{
					embeddedResourceRetriever = new EmbeddedResourceRetriever();
					var tempFileName = embeddedResourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.ReportTestFiles.NewStyleTemplate.xls", "NewStyleTemplate.xls");
					excelTemplate = new ExcelTemplateForUnitTesting("NewStyleTemplate.xls", Path.GetFullPath(tempFileName));
				}
				return excelTemplate;
			}
		}
		ExcelTemplateForUnitTesting excelTemplate;

		protected DocumentPack Pack
		{
			get
			{
				if (fPack == null)
				{
					var menuItem = Factory.New<StmMenuItem>();
					fPack = new DocumentPack(menuItem);
				}

				return fPack;
			}
		}

		DocumentPack fPack;

		protected void PrepareRenderer()
		{
			PrepareRenderer(Report);
		}

		protected void PrepareRenderer(Report report)
		{
			TestData.CreateJobTestTable();
			TestData.CreateHeaderTestTable();
			TestData.CreateLinesTestTable();
			TestData.CreateDocEngineTestTable();
			temporarilyUseMainConnection = Report.TemporarilyUseMainConnection();

			report.PrepareForRender();

			System.Data.DataTable tbl = new System.Data.DataTable("UnitTest");
			tbl.Columns.Add(new System.Data.DataColumn("Col1", typeof(int)));
			tbl.Columns.Add(new System.Data.DataColumn("CompressedBytes", typeof(byte[])));
			byte[] uncompressedBytes = new byte[] { (byte)'a', (byte)'b', (byte)'c' };
			byte[] compressedBytes = Compressor.Compress(uncompressedBytes);
			tbl.Rows.Add(new object[] { 1, compressedBytes });
			tbl.Rows.Add(new object[] { 2, compressedBytes });
			tbl.Rows.Add(new object[] { 3, compressedBytes });
			DummyDataProvider tstDataProvider = new DummyDataProvider();

			SectionBodyArea testArea1 = new SectionBodyArea(10, 11, report, "#SectionBody:DATA=Tbl");
			Area testArea2 = new ConfigArea(1, 1, report, "");

			testArea1.ExpandForDataRows(4);
			testArea1.FormulaRelatedAreas.Add(testArea2);
			report.Renderer.CurrentAreaToProcess = testArea1;
			report.Renderer.CurrentAreaToProcess.SetWorksheetForFormulaProvider(report.WorkSheetCurrentlyBeingProcessed);
			report.Renderer.CurrentAreaToProcess.FormulaProvider.AddColumn(1, "Tbl.Tst", 0);
		}

		protected override void TearDown()
		{
			base.TearDown();
			fReport?.Dispose();
			embeddedResourceRetriever?.Dispose();
			temporarilyUseMainConnection?.Dispose();
		}

		IDisposable temporarilyUseMainConnection;
		EmbeddedResourceRetriever embeddedResourceRetriever;

		#endregion
	}
}
