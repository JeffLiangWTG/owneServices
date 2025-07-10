using System.Globalization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Areas;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.DocumentEngine.ValueReplacers;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(HideRowIf))]
	sealed class HideRowIfTest : ValueProviderTest
	{
		public override void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro()
		{
			double value1 = 0.0000000001d;
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Amount", value1));
			AssertType<RowHider>(Report.MacroTranslator.GetValue("<HideRowIf(<Amount>==0.0000000001)>", Passes.FirstPass));
			Assert(!Report.ErrorManager.HasErrors);

			float value2 = 0.0000000001F;
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Amount", value2));
			AssertType<RowHider>(Report.MacroTranslator.GetValue("<HideRowIf(<Amount>==0.0000000001)>", Passes.FirstPass));
			Assert(!Report.ErrorManager.HasErrors);
		}

		public override void TestGetValueWithDoubleOrFloatValueInEvaluatedNestedMacro_WhenCultureChanges_ShouldConvertToStringWithoutScientificNotation()
		{
			using(Culture.SetTemporarily(CultureInfo.GetCultureInfo("pt-BR")))
			{
				var value1 = 765.217D;
				Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Amount", value1));
				AssertType<RowHider>(Report.MacroTranslator.GetValue("<HideRowIf(<Amount>==765.217)>", Passes.FirstPass));
				Assert(!Report.ErrorManager.HasErrors);

				var value2 = 765.217F;
				Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Amount", value2));
				AssertType<RowHider>(Report.MacroTranslator.GetValue("<HideRowIf(<Amount>==765.217)>", Passes.FirstPass));
				Assert(!Report.ErrorManager.HasErrors);
			}
		}

		public void TestReplacementWithEscapedDoubleQuoteInsideExpressionEvaluateProperly()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_1", "BILL < \"CLINTON\""));
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill_2", "BI<LL \"CLINTON\""));
			AssertEquals("Precondition: Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors, false);

			AssertEquals("Replaced Value when 2 Equal Values Specified", typeof(RowHider), ValueProviderToTest.GetReplacement("<Hide RowIf(\"<Bill_1>\" == \"<Bill_1>\")>", Report).GetType());
			AssertEquals("Report.ErrorManager.HasErrors", false, Report.ErrorManager.HasErrors);

			AssertEquals("Replaced Value when 2 Inequal Values Specified", "", ValueProviderToTest.GetReplacement("<Hide RowIf(\"<Bill_1>\" == \"<Bill_2>\")>", Report));
			AssertEquals("Report.ErrorManager.HasErrors", false, Report.ErrorManager.HasErrors);
		}

		public void TestReplacementWithBackslashInsideExpressionEvaluatesProperly()
		{
			Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Bill", "BILL FROST\\"));
			Assert(Report.ErrorManager.ToString(), !Report.ErrorManager.HasErrors);

			using (var worksheet = Report.XlInterface.WorkSheets[0])
			{
				worksheet[1, 1] = "<Hide RowIf(\"<Bill>\" == \"\")>";
				new CellContentReplacer(Report, 1, 1).ReplaceMacros();
			}

			Assert(Report.ErrorManager.ToString(), !Report.ErrorManager.HasErrors);
		}

		[NUnit.Framework.DatCapabilityRequirement("SOURCE_CODE")]
		public void TestReplacementWithIllegalExpression()
		{
			var factory = new BusinessObjectFactory();
			var command = factory.New<ReportCommand>();
			var excelTemplate = new ExcelTemplateForUnitTesting("Test.xls", TestFilesSubFolder.DocumentTestFiles);
			var pack = new DocumentPack(command);

			var postMasterGroup = factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = "postmaster@sample.org";
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";
			factory.Save();

			using (var report = new Report(pack, excelTemplate, System.Guid.Empty, Enterprise.Core.Constants.DataContext.None, "My Delivery Order"))
			{
				report.MenuItem.SU_BusinessContext = "Custom";
				report.MenuItem.SU_MenuName = "Delivery Order";
				report.MenuItem.SU_MenuPath = "Misc/";
				report.MenuItem.SU_FilterList = "CTY=GB";
				report.MenuItem.SU_IsSystemDefined = true;
				report.MenuItem.SU_IsClientSpecific = true;

				report.StTemplate = factory.New<StmTemplate>();
				report.StTemplate.SO_Name = "DA893";
				report.StTemplate.SO_IsSystemDefined = false;
				report.StTemplate.SO_IsClientSpecific = false;
				report.StTemplate.SO_DataContext = "Shipping";
				report.StTemplate.SO_ExcelTemplatePath = @"abc.xls";

				AssertEquals("Precondition - report.ErrorManager.HasErrors is false", false, report.ErrorManager.HasErrors);
				AssertEquals("", ValueProviderToTest.GetReplacement("<Hide RowIf(\"AB CD !=\")>", report));
				AssertEquals("report.ErrorManager.HasErrors is true", true, report.ErrorManager.HasErrors);
				AssertEquals("report.ErrorManager.IsWarningOnly is true", true, report.ErrorManager.HasWarningsOnly);
				AssertEquals("report.ErrorManager.ToString()", @"Severity: [Warning (without error report)] Message: [Error in HideRowIf Macro: Result of ""AB CD !="" is not a True/False expression. Input macro: [<Hide RowIf(""AB CD !="")>]]",
					report.ErrorManager.ToString("Severity: [{0}] Message: [{1}]", false));
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("should not match <>", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< Hide Row If  >", Passes.FirstPass));
			Assert("should not match ", !ValueProviderToTest.IsResponsibleForReplacing("< HideRow If >", Passes.FirstPass));
			Assert("should match ", ValueProviderToTest.IsResponsibleForReplacing("< Hide Row If  (Condition)>", Passes.FirstPass));
		}

		public void TestRegexToFindMacroAnyWhereInString()
		{
			AssertNoMatch(HideRowIf.RegexToFindMacroAnyWhereInString, "<>");
			AssertNoMatch(HideRowIf.RegexToFindMacroAnyWhereInString, "< Hide Row If  >");
			AssertNoMatch(HideRowIf.RegexToFindMacroAnyWhereInString, "< HideRow If >");
			AssertMatch(HideRowIf.RegexToFindMacroAnyWhereInString, "blah < Hide Row If  (Condition)>\nbblala");
			AssertMatch(HideRowIf.RegexToFindMacroAnyWhereInString, "blah < Hide Row If  (Condi\ntion)>\nbblala");
		}

		public void TestReplacement()
		{
			AssertEquals("", ValueProviderToTest.GetReplacement("<Hide RowIf(1==2)>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<Hide RowIf(1\n==2)>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<Hide RowIf(1\r\n==2)>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<Hide RowIf(1\r==2)>", Report));
			AssertEquals(typeof(RowHider), ValueProviderToTest.GetReplacement("<Hide RowIf(1==1)>", Report).GetType());
			AssertEquals(typeof(RowHider), ValueProviderToTest.GetReplacement("<Hide RowIf(\n1\n==1)>", Report).GetType());
			AssertEquals(typeof(RowHider), ValueProviderToTest.GetReplacement("<Hide RowIf(\"Test 1\" == \"Test 1\")>", Report).GetType());
		}

		public void TestReplaceNestedMacroShouldNotCareAboutCulture()
		{
			ZDecimal value = 100.00m;
			using (Culture.SetTemporarily(Culture.GetCultureForLanguage(Core.SharedConstants.Languages.French)))
			{
				Report.MacroTranslator.RegisterValueProvider(new FixedValueProvider("Amount", value));
				AssertEquals("", Report.MacroTranslator.GetValue("<HideRowIf(<Amount>==0)>", Passes.FirstPass));
			}
		}

		public void TestIsINonVisualisableValueProviderThatModifyDocumentLayout()
		{
			var provider = GetNewValueProvider() as INonVisualisableValueProvider;

			AssertNotNull(provider);
			AssertEquals(HideRowIf.RegexToFindMacroAnyWhereInString, provider.RegexToReplaceMacro);
		}

		public void TestGreaterThanAndLessThan()
		{
			AssertEquals("Precondition: Report.ErrorManager.HasErrors", Report.ErrorManager.HasErrors, false);

			//To do 'greater than' use '&gt;' instead of '>'.
			AssertEquals(typeof(RowHider), ValueProviderToTest.GetReplacement("<HideRowIf(20 &gt; 10)>", Report).GetType());
			AssertEquals("", ValueProviderToTest.GetReplacement("<HideRowIf(10 &gt; 20)>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<HideRowIf(10 &gt; 10)>", Report));

			//To do 'less than' use '&lt;' instead of '<'.
			AssertEquals(typeof(RowHider), ValueProviderToTest.GetReplacement("<HideRowIf(10 &lt; 20)>", Report).GetType());
			AssertEquals("", ValueProviderToTest.GetReplacement("<HideRowIf(20 &lt; 10)>", Report));
			AssertEquals("", ValueProviderToTest.GetReplacement("<HideRowIf(20 &lt; 20)>", Report));

			//To do 'less than or equal to' use '&lt;=' instead of '<='.
			AssertEquals(typeof(RowHider), ValueProviderToTest.GetReplacement("<HideRowIf(10 &lt;= 20)>", Report).GetType());
			AssertEquals("", ValueProviderToTest.GetReplacement("<HideRowIf(20 &lt;= 10)>", Report));
			AssertEquals(typeof(RowHider), ValueProviderToTest.GetReplacement("<HideRowIf(10 &lt;= 10)>", Report).GetType());

			//To do 'greater than or equal to' use '&gt;=' instead of '>='.
			AssertEquals(typeof(RowHider), ValueProviderToTest.GetReplacement("<HideRowIf(20 &gt;= 10)>", Report).GetType());
			AssertEquals("", ValueProviderToTest.GetReplacement("<HideRowIf(10 &gt;= 20)>", Report));
			AssertEquals(typeof(RowHider), ValueProviderToTest.GetReplacement("<HideRowIf(10 &gt;= 10)>", Report).GetType());

			AssertEquals("Report.ErrorManager.HasErrors", false, Report.ErrorManager.HasErrors);
		}

		public override void TestDocumentation()
		{
			base.TestDocumentation();
			AssertContains("Please note that we might get some extra blank lines when some macros are used together with this macro.\r\nE.g: <HideRowIf(<TotalPages> == 2)>\r\nE.g: <HideRowIf(<CurrencyMajorUnit(USD)> == \"dollar\")>\r\nE.g: <HideRowIf(<DeliveryCount> == 3)>",
				ValueProviderToTest.Documentation.Explanation.ToString());
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new HideRowIf();
		}
	}
}
