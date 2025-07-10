using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.ReportErrorManagement;
using Enterprise.DocumentEngine.Scheduler.Business;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.DocumentEngine.Visualisation;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(If))]
	sealed class IfTest : ValueProviderTest
	{
		public override void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro()
		{
			double value1 = 0.000001d;
			float value2 = 0.000001F;
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue1", value1));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue2", value2));
			AssertEquals(value1, Report.MacroTranslator.GetValue("<If(<TestValue1> == 0.000001, \"<TestValue1>\",\"false\")>", Passes.FirstPass));
			Assert(!Report.ErrorManager.HasErrors);

			AssertEquals("false", Report.MacroTranslator.GetValue("<If(<TestValue2> == 0.000002, \"<TestValue2>\",\"false\")>", Passes.FirstPass));
			Assert(!Report.ErrorManager.HasErrors);

			AssertEquals(value1, Report.MacroTranslator.GetValue("<If(<TestValue1> == 0.000001, \"<If(<TestValue2> == 0.000001, \"<TestValue1>\",\"false1\")>\",\"false2\")>", Passes.FirstPass));
			Assert(!Report.ErrorManager.HasErrors);
		}

		public override void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro_WhenCultureChanges_ShouldConvertToStringWithoutScientificNotation()
		{
			using (Culture.SetTemporarily(CultureInfo.GetCultureInfo("pt-BR")))
			{
				var value1 = 765.217D;
				Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue1", value1));
				AssertEquals(value1, Report.MacroTranslator.GetValue("<If(<TestValue1> == 765.217, \"<TestValue1>\",\"false\")>", Passes.FirstPass));

				var value2 = 765.217F;
				Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("TestValue2", value2));
				AssertEquals(value2, Report.MacroTranslator.GetValue("<If(<TestValue2> == 765.217, \"<TestValue2>\",\"false\")>", Passes.FirstPass));
				Assert(!Report.ErrorManager.HasErrors);
			}
		}

		public void TestBoFieldAndFixedValueInIfMacro()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Hello", "Hello World"));
			AssertReplacedWithoutErrors("<If(\"1\" == \"1\", \"<Hello>\",\"false\")>", "Hello World");

			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<If('<Collection.Description>'=='', ""<Collection.Code>"", ""<Collection.Description>"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");
			dummy.Collection.AddNew("BBB");

			var printJob = GetPrintJob(template, dummy);

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				AssertEquals("Should evaluate to correct Value",
					@"{B}-[AAA Description]
{B}-[BBB]", excelInterface.WorkSheets.First().ToString());
			}

			template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<If('<Collection.Description>'=='', ""<Collection.Code>"", ""<Z0_AnotherDecimal>"")>]
{A}-[#EndOfReport]");
			dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");
			dummy.Z0_AnotherDecimal = 1.5m;
			printJob = GetPrintJob(template, dummy);

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				AssertEquals("Should evaluate to correct Value",
					@"{B}-[1.5]", excelInterface.WorkSheets.First().ToString());
			}
		}

		public void TestOnlyMatchedConditionBeEvaluated()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<If('<Collection.Description>'=='', ""<Collection.Code>"", ""<Collection.WhatEverField>"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA");

			var printJob = GetPrintJob(template, dummy);

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				AssertEquals("Should evaluate to correct Value", @"{B}-[AAA]", excelInterface.WorkSheets.First().ToString());
			}
		}

		public void TestOnlyMatchedConditionBeEvaluatedWithNestedIf1()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<If('<Collection.Description>'=='', ""<If('<Collection.Description>'!='', ""<Collection.WhatEverField>"", ""<Collection.Code>"")>"", ""hahaha"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA");

			var printJob = GetPrintJob(template, dummy);

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				AssertEquals("Should evaluate to correct Value", @"{B}-[AAA]", excelInterface.WorkSheets.First().ToString());
			}
		}

		public void TestOnlyMatchedConditionBeEvaluatedWithNestedIf2()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<If('<Collection.Description>'=='', ""<If('<Collection.Description>'!='', ""<Collection.WhatEverField>"", ""<Collection.Code>"")>"", ""<Collection.WhatEverField>"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA");

			var printJob = GetPrintJob(template, dummy);

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				AssertEquals("Should evaluate to correct Value", @"{B}-[AAA]", excelInterface.WorkSheets.First().ToString());
			}
		}

		public void TestFieldMacroDoesNotExistInIfMacro()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<If('<Collection.Description>'=='', ""<Collection.Code>"", ""<WhatEver> asdf"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "AAA Description");

			try
			{
				GetPrintJob(template, dummy);
			}
			catch (DocumentEngineException e)
			{
				Assert(e.Message.Contains("Field <WhatEver> not found on any of the DataSource Types"));
			}
		}

		public void TestComplexNestedIfMacro()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Hello", "Hello"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Wise", "Wise"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Tech", "Tech"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Global", "Global"));

			AssertReplacedWithoutErrors("<If(\"<Hello>\" == \"Hello\", \"<If(\"<Wise>\" == \"<Tech>\", \"IM WRONG\",\"<If(\"1\" == \"1\", \"<Hello>, <Wise><Tech> <GLobal>\",\"NOT RETURN ME\")>\")>\",\"false\")>", "Hello, WiseTech Global");
			AssertReplacedWithoutErrors("<If(\"<Hello>\" == \"Hello\", \"<If(\"<Wise>\" == \"<Tech>\", \"IM WRONG\",\"<If(\"1\" == \"1\", \"I'm Beginning, <If(\"<Wise>\" == \"Wise\", \"<Hello> Wise\",\"whatever <WhatEver>\")><If(\"2\" == \"2\", \"<Tech>\",\"NOT ME\")> <GLobal>\",\"NOT RETURN ME\")>\")>\",\"false\")>", "I'm Beginning, Hello WiseTech Global");
		}

		public void TestIfMacroWorksWithTotalMacro()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Code>]   {C}-[<Collection.Decimal>]
{A}-[#SectionFooter]
{C}-[<If('Justin'=='Justin', ""GRAND TOTAL <Total Collection.Decimal>"", ""Not Me"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Collection.AddNew("AAA", "", 0, 10);
			dummy.Collection.AddNew("AAA", "", 0, 10);
			dummy.Collection.AddNew("AAA", "", 0, 10);

			var printJob = GetPrintJob(template, dummy);

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				Assert("Total macro shoule be translated correctly", excelInterface.WorkSheets.First().ToString().Contains("{C}-[GRAND TOTAL 30]"));
			}
		}

		public void TestIfMacroWithLineBreak()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Hello", "Hello"));

			AssertReplacedWithoutErrors(
				@"<If(""<Hello>"" == ""Hello Whatever"", """", ""
 <Hello>:
"")>",

				@"
 Hello:
");
		}

		StmPrintJob GetPrintJob(StmTemplateBase template, DummyDocumentSupportable dummy)
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			return DeliveryTestHelper.DeliverDocument(documentCommand).First();
		}

		public void TestNestedIfMacroInIfMacroWithTrailingSpace()
		{
			var factory = new BusinessObjectFactory();
			var template = DocumentEngineTestHelper.CreateTemplateFromString(factory, "Test",
@"{A}-[#Config]
{A}-[Name=Test]
{A}-[#SectionBody]
{B}-[<If(""True"" == ""True"", ""<If(""True"" == ""True"", ""Hello"", ""GoodBye"")> World"", ""False"")> ]
{A}-[#EndOfReport]");

			var reportCommand = factory.New<ReportCommand>();
			var document = reportCommand.Documents.AddNew();
			document.SI_SU = document.PK;
			document.SI_SO = template.PK;

			var printJob = DeliveryTestHelper.DeliverReport(reportCommand).First();

			using (var excelInterface = new ExcelInterface(printJob.SP_CustomProperties))
			{
				AssertEquals("Should evaluate to 'Hello World' with a trailing space.", "{B}-[Hello World ]", excelInterface.WorkSheets.First().ToString());
			}
		}

		public void TestReplacementWithEscapedDoubleQuoteInsideExpressionEvaluateProperly()
		{
			AssertEquals("Precondition: Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors, false);

			string bill1, bill2, bill3;
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_1", bill1 = "BILL < \"CLINTON\""));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_2", bill2 = "BI<LL \"CLINTON\""));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_3", bill3 = "BILL \"CLI>NTON"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_4", "A\\<"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_5", "50"));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_6", "<<<-\"->>>"));
			GlbStaff.CurrentUser.GS_FullName = "<LoginFullName>";

			AssertReplacedWithoutErrors("<If(\"<Bill_1>\" == \"<Bill_1>\", \"true\",\"false\")>", "true");
			AssertReplacedWithoutErrors("<If(\"<Bill_1>\" == \"<Bill_2>\", \"true\",\"false\")>", "false");
			AssertReplacedWithoutErrors("<If(\"<Bill_1>\" == \"<Bill_3>\", \"true\",\"false\")>", "false");

			AssertReplacedWithoutErrors("<If(\"<Bill_2>\" == \"<Bill_1>\", \"true\",\"false\")>", "false");
			AssertReplacedWithoutErrors("<If(\"<Bill_2>\" == \"<Bill_2>\", \"true\",\"false\")>", "true");
			AssertReplacedWithoutErrors("<If(\"<Bill_2>\" == \"<Bill_3>\", \"true\",\"false\")>", "false");

			AssertReplacedWithoutErrors("<If(\"<Bill_3>\" == \"<Bill_1>\", \"true\",\"false\")>", "false");
			AssertReplacedWithoutErrors("<If(\"<Bill_3>\" == \"<Bill_2>\", \"true\",\"false\")>", "false");
			AssertReplacedWithoutErrors("<If(\"<Bill_3>\" == \"<Bill_3>\", \"true\",\"false\")>", "true");

			AssertReplacedWithoutErrors("<If(\"<Bill_4>\" == \"A\\\\\\<\", \"true\",\"false\")>", "true", Passes.FirstPass);
			AssertReplacedWithoutErrors(@"<If(1==1,""<ExcelFormula(""= MAX(<Bill_5>, 40)"")>"", ""2"")>", "50");
			AssertReplacedWithoutErrors(@"<If(1==1,""<ExcelFormula(""= MAX(<Bill_5>, 40)"")>"", ""2"")>", @"<If(1==1,""<ExcelFormula(""= MAX(<Bill_5>, 40)"")>"", ""2"")>", Passes.FirstPass);
			AssertReplacedWithoutErrors("<If(1==0, \"whatever\", \"<Bill_6>\")>", "\\<\\<\\<-\"-\\>\\>\\>");

			AssertReplacedWithoutErrors("<If(\"<LoginFullName>\" == \"<LoginFullName>\", \"<LoginFullName>\",\"false\")>", "\\<LoginFullName\\>");

			AssertReplacedWithoutErrors("<If(1 == 1, \"<Bill_1>\",\"<Bill_2>\")>", bill1.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(1 != 1, \"<Bill_1>\",\"<Bill_2>\")>", bill2.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(1 != 1, \"<Bill_2>\",\"<Bill_3>\")>", bill3.EscapeAngleBrackets());

			AssertReplacedWithoutErrors("<If(1 > 1, \"<Bill_2>\",\"<Bill_3>\")>", bill3.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(1 < 1, \"<Bill_1>\",\"<Bill_2>\")>", bill2.EscapeAngleBrackets());
		}

		public void TestReplacementWithRogueSlash_ShouldEvaluateProperly()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_1", "BILL < \"CLINTON\"\\"));

			AssertReplacedWithoutErrors("<If(\"<Bill_1>\" == \"<Bill_1>\", \"<Bill_1>\",\"false\")>", "BILL \\< \"CLINTON\"\\");
		}

		void AssertReplacedWithoutErrors(string inputString, string expectedResult, Passes pass = Passes.SecondPass)
		{
			using (((ReportRenderer)Report.Renderer).TemporarilySwitchCurrentPassForTest(pass))
			{
				string result = "(not evaluated)";
				CombineAssertions(delegate
				{
					AssertNoExceptionThrown("Exception evaluating [" + inputString + "]", delegate
					{ result = Report.MacroTranslator.GetValue(inputString, pass).ToString(); });
					AssertEquals("Result from evaluating [" + inputString + "]", expectedResult, result);
					AssertMultilineASCIIEquals("Report.ErrorManager.HasErrors after evaluating [" + inputString + "]", ReportErrorManager.HasNoErrors, Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
				});
			}
		}

		public void TestReplacementWithIllegalExpression()
		{
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			AssertEquals("", ValueProviderToTest.GetReplacement("<If(\"AB CD !=\", \"Yes\", \"No\")>", Report));
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);
			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in If Macro: Result of ""AB CD !="" is not a True/False expression]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		public override void TestTemplateVisualiserComponentType()
		{
			AssertEquals("ValueProviderToTest.ComponentType", VisualiserComponentTypes.TextEdit, ValueProviderToTest.ComponentType);
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should match", ValueProviderToTest.IsResponsibleForReplacing("<If(condition,\"true\",\"false\")>", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< If ( condition , \"true\" , \"false\" ) >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< If ( condition, \"true\", \"false\") >", Passes.FirstPass));
			Assert("should NOT match ", !ValueProviderToTest.IsResponsibleForReplacing("< If ( condition, \"true\")>", Passes.FirstPass));
			Assert("should NOT match ", !ValueProviderToTest.IsResponsibleForReplacing("< If ( condition, true, false)>", Passes.FirstPass));
		}

		public void TestReplacement()
		{
			AssertEquals("true and\",yes\"", ValueProviderToTest.GetReplacement("<If(1==1, \"true and\",yes\"\", \"false\")>", Report));
			AssertEquals("\"False with quotes\"", ValueProviderToTest.GetReplacement("<If(1==2, \"true and\",yes\"\", \"\"False with quotes\"\")>", Report));
			AssertEquals("false", ValueProviderToTest.GetReplacement("<If(1==2,\"true\",\"false\")>", Report));
			AssertEquals("true", ValueProviderToTest.GetReplacement("<If(1 == 1,\"true\",\"false\")>", Report));
			AssertEquals("no", ValueProviderToTest.GetReplacement("<If(\"meh\"==\"MEH\",\"yes\",\"no\")>", Report));
			AssertEquals("yes", ValueProviderToTest.GetReplacement("<If(\"meh\" ==\"meh\",\"yes\",\"no\")>", Report));
			AssertEquals("yes", ValueProviderToTest.GetReplacement(@"<If(""MEH""==""MEH"",""yes"",""n
				o"")>", Report));
			AssertEquals("false", ValueProviderToTest.GetReplacement("<If(1 > 1,\"true\",\"false\")>", Report));
		}

		public void TestExpressionEvaluationError_ShouldNotReportErrorForTemplateWithCustomisedSections()
		{
			Report.Template.ContainsCustomisedSections = true;
			AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, Report.ErrorManager.HasErrors);
			AssertEquals("", ValueProviderToTest.GetReplacement("<If(\"AB CD !=\", \"Yes\", \"No\")>", Report));
			AssertEquals("report.ErrorManager.HasErrors is true", true, Report.ErrorManager.HasErrors);

			AssertEquals("report.ErrorManager.IsWarningOnly is true", true, Report.ErrorManager.HasWarningsOnly);
			AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in If Macro: Result of ""AB CD !="" is not a True/False expression]",
									Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
		}

		public void TestNestedMacroIsEvaluatedWhenPutTogetherWithAutoHeight()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[#SectionBody:Data=Collection]
{B}-[<If('<Collection.Description>'=='', ""<AutoHeight><Collection.Code>"", ""<AutoHeight><Collection.Description>"")>]
{A}-[#EndOfReport]");
			var dummy = Factory.New<DummyDocumentSupportable>();
			var child1 = dummy.Collection.AddNew("AAA", @"SomethingLoooooongHere");
			var child2 = dummy.Collection.AddNew("BBB", @"SomethingElse");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;

			using (var documentPack = new DocumentPack(documentCommand, dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						var workSheet = excelInterface.WorkSheets.First();

						AssertEquals(@"{B}-[Somethin]
{B}-[gLooooo]
{B}-[ongHere]
{B}-[Somethin]
{B}-[gElse]", workSheet.ToString());
					}
				}
			}
		}

		public void TestGreaterThanAndLessThan()
		{
			AssertEquals("Precondition: Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors, false);

			var bill1 = "BILL < \"CLINTON\"";
			var bill2 = "BI<LL \"CLINTON\"";
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_1", bill1));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_2", bill2));

			//To do 'greater than' use '&gt;' instead of '>'.
			AssertReplacedWithoutErrors("<If(2 &gt; 1, \"<Bill_1>\",\"<Bill_2>\")>", bill1.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(1 &gt; 2, \"<Bill_1>\",\"<Bill_2>\")>", bill2.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(1 &gt; 1, \"<Bill_1>\",\"<Bill_2>\")>", bill2.EscapeAngleBrackets());

			//To do 'less than' use '&lt;' instead of '<'.
			AssertReplacedWithoutErrors("<If(2 &lt; 1, \"<Bill_1>\",\"<Bill_2>\")>", bill2.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(1 &lt; 2, \"<Bill_1>\",\"<Bill_2>\")>", bill1.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(2 &lt; 2, \"<Bill_1>\",\"<Bill_2>\")>", bill2.EscapeAngleBrackets());

			//To do 'less than or equal to' use '&lt;=' instead of '<='.
			AssertReplacedWithoutErrors("<If(2 &lt;= 1, \"<Bill_1>\",\"<Bill_2>\")>", bill2.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(1 &lt;= 2, \"<Bill_1>\",\"<Bill_2>\")>", bill1.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(2 &lt;= 2, \"<Bill_1>\",\"<Bill_2>\")>", bill1.EscapeAngleBrackets());

			//To do 'greater than or equal to' use '&gt;=' instead of '>='.
			AssertReplacedWithoutErrors("<If(2 &gt;= 1, \"<Bill_1>\",\"<Bill_2>\")>", bill1.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(1 &gt;= 2, \"<Bill_1>\",\"<Bill_2>\")>", bill2.EscapeAngleBrackets());
			AssertReplacedWithoutErrors("<If(1 &gt;= 1, \"<Bill_1>\",\"<Bill_2>\")>", bill1.EscapeAngleBrackets());

			AssertEquals("Report.ErrorManager.HasErrors", false, Report.ErrorManager.HasErrors);
		}

		public void TestNonNestedMacro_ReturnString()
		{
			Assert(ValueProviderToTest.GetReplacement("<If(1 == 1, \"a test string\", \"false\")>", Report) is string);
		}

		public void TestNestedMacro_ReturnObject()
		{
			Assert(ValueProviderToTest.GetReplacement("<If(1 == 1, \"<Now>\", \"false\")>", Report) is ZDateTime);
			Assert(ValueProviderToTest.GetReplacement("<If(1 == 1, \"<CurrentCompany>\", \"false\")>", Report) is ZGuid);
			Assert(ValueProviderToTest.GetReplacement("<If(1 == 1, \"<UrlHyperlink(test.url, OASite, click here)>\", \"false\")>", Report) is ExcelHyperlink);
			Assert(ValueProviderToTest.GetReplacement("<If(1 == 0, \"1\", \"<If(\"<If(\"2\" == \"3\", \"1\", \"2\")> \" == \"1\", \"1\", \"<UrlHyperlink(test.url, OASite, click here)>\")>\")>", Report) is ExcelHyperlink);
			Assert(ValueProviderToTest.GetReplacement("<If(\"<if(1 == 0, \"0\" ,\"1\")>\" == \"0\", \"1\", \"<If(\"<If(\"2\" == \"3\", \"1\", \"2\")> \" == \"1\", \"1\", \"<UrlHyperlink(test.url, OASite, click here)>\")>\")>", Report) is ExcelHyperlink);
		}

		public void TestTwoOrMoreConsecutiveNestedMacros_ReturnString()
		{
			Assert(ValueProviderToTest.GetReplacement("<If(1 == 1, \"<Now><GoodsDescription>\", \"false\")>", Report) is string);
			Assert(ValueProviderToTest.GetReplacement("<If(1 == 1, \"<CurrentCompany><GoodsDescription>\", \"false\")>", Report) is string);
			Assert(ValueProviderToTest.GetReplacement("<If(1 == 1, \"<UrlHyperlink(test.url, OASite, click here)><UrlHyperlink(test.url, OASite, click here)>\", \"false\")>", Report) is string);
			Assert(ValueProviderToTest.GetReplacement("<If(1 == 0, \"1\", \"<If(\"<If(\"2\" == \"3\", \"1\", \"2\")> \" == \"1\", \"1\", \"<UrlHyperlink(test.url, OASite, click here)><GoodsDescription>\")>\")>", Report) is string);
			Assert(ValueProviderToTest.GetReplacement("<If(\"<if(1 == 0, \"0\" ,\"1\")>\" == \"0\", \"1\", \"<If(\"<If(\"2\" == \"3\", \"1\", \"2\")> \" == \"1\", \"1\", \"<UrlHyperlink(test.url, OASite, click here)><GoodsDescription>\")>\")>", Report) is string);
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new If();
		}

		protected override void SetUp()
		{
			Report.Renderer.CurrentAreaToProcess = new ConfigArea(0, 0, Report, ""); // make <CurrentPage> = 0
		}

		protected override Passes PassToReplaceExample => Passes.SecondPass;
	}
}
