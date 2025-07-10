using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	sealed class MacroTranslatorTest : TestCaseWithFactory
	{
		public void TestShouldEscapeAngleBracketsForModifiable()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[DataContext=None]
{A}-[Name=Blah]
{A}-[Data:ReportData=select 'This is <Blah> Test' as BlahText]
{A}-[#SectionBody:Data=ReportData]
{B}-[<Modifiable(<ReportData.BlahText>)>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("Expected template",
					@"{B}-[This is <Blah> Test]
", workSheet.ToString());
			}
		}

		public void TestEscapeAngleBracketsForNestedMacro()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[DataContext=None]
{A}-[Name=Blah]
{A}-[Data:ReportData=select 'This is <Blah> Test' as BlahText]
{A}-[#SectionBody:Data=ReportData]
{B}-[<Upper(""<ReportData.BlahText>"")>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("Expected template",
					@"{B}-[THIS IS <BLAH> TEST]
", workSheet.ToString());
			}
		}

		public void TestEscapeAngleBracketsForIf()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[DataContext=None]
{A}-[Name=Blah]
{A}-[Data:ReportData=select 'This is <Blah<br />> Test' as BlahText]
{A}-[#SectionBody:Data=ReportData]
{B}-[<If(""<Upper(""<ReportData.BlahText>"")>"" == ""THIS IS \<BLAH\<BR /\>\> TEST"", ""Test true"", ""Test false"")>]
{A}-[#EndOfReport]");

			var reportCommand = Factory.New<ReportCommand>();
			var pivot = reportCommand.Documents.AddNew();
			pivot.SI_SU = reportCommand.PK;
			pivot.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				var workSheet = excelInterface.WorkSheets.First();

				AssertMultilineASCIIEquals("Expected template",
					@"{B}-[Test true]
", workSheet.ToString());
			}
		}

		public void TestGetFormulaResult()
		{
			var excelTemplate = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
@"{A}-[#Config]
{A}-[#EndOfReport]");

			using (var documentPack = new DocumentPack())
			using (var report = new Report(documentPack, excelTemplate))
			{
				var translator = new MacroTranslator(report);

				AssertEquals("translator.GetFormulaResult(\"= 68\")", 68.0, translator.GetFormulaResult("= 68"));
				AssertEquals("translator.GetFormulaResult(\"= 1 + 1\")", 2.0, translator.GetFormulaResult("= 1 + 1"));
			}
		}

		public void TestRegisterValueProviderWorksIfMacroHasSomeSpacesAtEitherEnd()
		{
			MacroTranslator instance = new MacroTranslator(new Report(null, null));
			Assert(instance.GetValue("  <Datetimestart> ", Passes.FirstPass) is DateTime);
			instance.RegisterValueProvider(new FixedValueProvider("Test", 110));
			AssertEquals(110, instance.GetValue("   <test> ", Passes.FirstPass));
		}

		public void TestRegisterValueProvider()
		{
			MacroTranslator instance = new MacroTranslator(new Report(null, null));
			Assert(instance.GetValue("<Datetimestart>", Passes.FirstPass) is DateTime);
			instance.RegisterValueProvider(new FixedValueProvider("Test", 110));
			AssertEquals(110, instance.GetValue("<test>", Passes.FirstPass));
		}

		public void TestDoubleQuotesNotConvertedToSingleQuotes()
		{
			var instance = new MacroTranslator(new Report(null, null));
			instance.RegisterValueProvider(new FixedValueProvider("Test", "\"Something\""));
			AssertEquals("\"Something\"", instance.GetValue("<Test>", Passes.FirstPass));
		}

		public void TestGetValueWithSingleAngleBracket()
		{
			var instance = new MacroTranslator(new Report(null, null));
			instance.RegisterValueProvider(new FixedValueProvider("Test1", "AB<"));
			instance.RegisterValueProvider(new FixedValueProvider("Test2", "AB>"));
			instance.RegisterValueProvider(new FixedValueProvider("Test3", "AB<>"));
			AssertEquals("AB<", instance.GetValue("<Test1>", Passes.FirstPass, true));
			AssertEquals("AB>", instance.GetValue("<Test2>", Passes.FirstPass, true));
			AssertEquals("AB<>", instance.GetValue("<Test3>", Passes.FirstPass, true));
		}

		public void TestMacroWithHtmlBreakLine()
		{
			using (Report testReport = new Report(null, null, "TestReport", null, false))
			{
				AssertEquals("pre-condition", false, testReport.Renderer.IsProcessingMacros);
				var instance = new MacroTranslator(testReport);
				instance.RegisterValueProvider(new FixedValueProvider("Test", "<Html Break Line <br /> Test>"));
				instance.RegisterValueProvider(new FixedValueProvider("Test2", "Html Break Line br Test>"));
				instance.RegisterValueProvider(new FixedValueProvider("Test3", "<Html Break Line <br> Test"));
				instance.RegisterValueProvider(new FixedValueProvider("Test4", "<Html Break Line <br > Test>"));
				instance.RegisterValueProvider(new FixedValueProvider("MacroCachingTest", "1 \n <br> <br > <br /> <hr /> <area /> <base /> <img /> <input /> <link /> <meta /> <col /> <frame /> <embed /> <keygen />"));

				AssertEquals("\\<Html Break Line <br /> Test\\>", instance.GetValue("<Test>", Passes.FirstPass));
				AssertEquals("Html Break Line br Test\\>", instance.GetValue("<Test2>", Passes.FirstPass));
				AssertEquals("\\<Html Break Line \\<br\\> Test", instance.GetValue("<Test3>", Passes.FirstPass));
				AssertEquals("\\<Html Break Line \\<br \\> Test\\>", instance.GetValue("<Test4>", Passes.FirstPass));
				AssertEquals("1 \n \\<br\\> \\<br \\> <br /> \\<hr /\\> \\<area /\\> \\<base /\\> \\<img /\\> \\<input /\\> \\<link /\\> \\<meta /\\> \\<col /\\> \\<frame /\\> \\<embed /\\> \\<keygen /\\>", instance.GetValue("<MacroCachingTest>", Passes.FirstPass));
			}

			using (var stream = new MemoryStream())
			{
				var template = DocumentEngineTestHelper.CreateExcelTemplateFromString("Test", string.Empty,
					@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Z0_Description>]
{B}-[<Collection.CollectionOwnProperty>]
{A}-[#EndOfReport]");
				var menuItem = Factory.New<StmMenuItem>();
				menuItem.SU_IsSystemDefined = true;
				menuItem.SU_MenuName = "TestMacroWithHtmlBreakLine";
				menuItem.SU_BusinessContext = ".DummyBusinessObject";
				var dummy = Factory.New<DummyDocumentSupportable>();
				dummy.Collection.AddNew("AAA", "AAA Description");
				dummy.Collection.AddNew("BBB", "BBB<br />Description");
				using (var documentPack = new DocumentPack(menuItem))
				using (var testReport = new Report(documentPack, template, BODocDataProvider.Get(dummy), null, null, null, DocumentDirection.ANY, false))
				{
					AssertNoExceptionThrown("macro with HtmlBreakLine Exception won't be thrown", () => testReport.Save(stream));
				}
			}
		}

		public void TestNonExistingMacro()
		{
			MacroTranslator instance = new MacroTranslator(new Report(null, null));
			AssertEquals("<Non Existing Macro>", instance.GetValue("<Non Existing Macro>", Passes.FirstPass));
		}

		public void TestNowMacro()
		{
			MacroTranslator instance = new MacroTranslator(new Report(null, null));
			Assert(instance.GetValue("<Now>", Passes.FirstPass).GetType() == typeof(ZDateTime));
		}

		public void TestRecursiveValueProvider()
		{
			MacroTranslator instance = new MacroTranslator(new Report(null, null));
			instance.RegisterValueProvider(new FixedValueProvider("Test", 12));
			AssertEquals("twelve", instance.GetValue("<NUMBER TO WORDS(<Test>)>", Passes.FirstPass));
		}

		public void TestRecursiveValueProvider_MoreThanOneLevelOfRecursion()
		{
			MacroTranslator instance = new MacroTranslator(new Report(null, null));
			instance.RegisterValueProvider(new FixedValueProvider("Blah", 20));
			ExcelHyperlink hyperlink = (ExcelHyperlink)instance.GetValue("<UrlHyperlink(www.edi.com.au, <NUMBER TO WORDS(<Blah>)>, we'll do it my way)>", Passes.FirstPass);
			AssertEquals("twenty", hyperlink.TextToShow);
			AssertEquals("www.edi.com.au", hyperlink.LinkLocation);
			AssertEquals("we'll do it my way", hyperlink.Tooltip);
		}

		[ExpectNoExceptions]
		public void TestConcurrentResetProviders()
		{
			MacroTranslator instance = new MacroTranslator(new Report(null, null));
			instance.RegisterValueProvider(new Image());
			Thread[] threads = new Thread[10];
			for (int i = 0; i < 10; ++i)
			{
				threads[i] = new Thread(() =>
				{
					for (int j = 0; j < 10; ++j)
					{
						instance.ResetProviders();
					}
				});
			}
			for (int i = 0; i < 10; ++i)
			{
				threads[i].Start();
			}
			for (int i = 0; i < 10; ++i)
			{
				threads[i].Join();
			}
		}

		public void TestExceptionThrownWhenMacroDoesNotExist()
		{
			using (Report testReport = new Report(null, null, "TestReport", null, false))
			{
				MacroTranslator instance = new MacroTranslator(testReport);
				instance.RegisterValueProvider(new FixedValueProvider("Test", 12));
				AssertExceptionThrown<FieldNotFoundException>("Blah", "Field <Blah> not found on DataSource.", () => instance.GetValue("<Blah>", Passes.SecondPass));
			}
		}

		public void TestExceptionThrownWhenMacroHasDataSourceError()
		{
			using (Report testReport = new Report(null, null, "TestReport", null, false))
			{
				MacroTranslator instance = new MacroTranslator(testReport);
				instance.RegisterValueProvider(new FixedValueProvider("Test", 12));
				AssertExceptionThrown<FieldNotFoundException>("_DataSource", MacroDataSource.EmptyDataSourceTypeError, () => instance.GetValue("<_DataSource>", Passes.SecondPass));
				AssertExceptionThrown<FieldNotFoundException>("_DataSource.", MacroDataSource.EmptyDataSourceTypeError, () => instance.GetValue("<_DataSource.>", Passes.SecondPass));
			}
		}

		public void TestIsMacroCached()
		{
			using (Report testReport = new Report(null, null, "TestReport", null, false))
			{
				Hashtable calledMacros = new Hashtable();
				MacroTranslator instance = new MacroTranslator(testReport);
				instance.RegisterValueProvider(new FixedValueProvider("MacroCachingTest", 10));
				instance.MacroCalled += (macroWithoutAngleBrackets, valueProvider) =>
				{
					string macroTitleForTesting =
						(valueProvider is DBOrBOValueProvider || valueProvider is DelegateValueProvider ||
						 valueProvider is FixedValueProvider)
							? macroWithoutAngleBrackets
							: valueProvider.GetType().Name;
					calledMacros[macroTitleForTesting] = macroTitleForTesting;
				};
				object macroValue = instance.GetValue("<MacroCachingTest>", Passes.FirstPass);
				AssertEquals(10, macroValue);
				AssertEquals(true, calledMacros.ContainsKey("MacroCachingTest"));
			}
		}

		class NullReturner : ValueProvider
		{
			protected override object GetReplacementCore(string macro, Report report)
			{
				return null;
			}

			protected override ValueProviderDocumenter GetDocumentation()
			{
				return new ValueProviderDocumenter("Dummy", (NoResString)"Returns an empty value. Internal use only.");
			}

			public override Regex Regex
			{
				get { return new Regex("Dummy"); }
			}

			protected override bool IsResponsibleForReplacingCore(string macro, Passes currentPass)
			{
				return macro == "<GetMeNull>";
			}
		}

		public void TestNullIsTranslatedToEmptyString()
		{
			MacroTranslator instance = new MacroTranslator(new Report(null, null));
			instance.RegisterValueProvider(new NullReturner());
			AssertEquals("", instance.GetValue("<GetMeNull>", Passes.FirstPass));
		}

		class ValueProviderWithReset : ValueProvider
		{
			protected override ValueProviderDocumenter GetDocumentation()
			{
				throw new NotImplementedException();
			}

			protected override object GetReplacementCore(string macro, Report report)
			{
				return "";
			}

			public override Regex Regex
			{
				get
				{
					return new Regex("Dummy");
				}
			}

			protected override void ResetCore()
			{
				IsReset = true;
			}

			public bool IsReset;
		}

		public void TestReset()
		{
			MacroTranslator instance = new MacroTranslator(null);
			ValueProviderWithReset provider = new ValueProviderWithReset();
			instance.RegisterValueProvider(provider);
			AssertEquals("should'nt be reset", false, provider.IsReset);
			instance.ResetProviders();
			AssertEquals("should be reset", true, provider.IsReset);
		}
	}
}
