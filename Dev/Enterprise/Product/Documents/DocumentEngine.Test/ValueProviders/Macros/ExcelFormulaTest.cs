using System.IO;
using System.Linq;
using CargoWise.Common;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ExcelFormula))]
	sealed class ExcelFormulaTest : ValueProviderTest
	{
		public void TestReplacementDoesNotReplaceCell()
		{
			AssertEquals(60.00, ValueProviderToTest.GetReplacement("<ExcelFormula(\"=60\")>", Report));
			AssertEquals("#config", Report.WorkSheetCurrentlyBeingProcessed.ParentExcelInterface.Xls.GetCellValue(1, 1));
		}

		public void TestReplacementByUsingRecalcCell()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
				@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{C}-[<ExcelFormula(""=IF(B4="""","""",B4)"")>] {D}-[D4Value]
{B}-[B5Value] {C}-[<If(""<Z0_Code>""==""TST"",""<ExcelFormula(""= MAX(<Z0_Number>, 40)"")>"", ""2"")>] {D}-[D5Value]
{D}-[D6Value]
{D}-[D7Value]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_Number = 45;
			dummy.Z0_Code = "TST";

			using (var documentPack = new DocumentPack(GetDocumentCommand(dummy, template), dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						var workSheet = excelInterface.WorkSheets.First();

						AssertEquals(@"{D}-[D4Value]
{B}-[B5Value]   {C}-[45]   {D}-[D5Value]
{D}-[D6Value]
{D}-[D7Value]", workSheet.ToString());
					}
				}
			}
		}

		public void TestIsResponsibleForReplacing()
		{
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<ExcelFormula(\"=blah\")>", Passes.SecondPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<excelformula(\"=blah\")>", Passes.SecondPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("< ExcelFormula (\"=blah\") >", Passes.SecondPass));
			Assert("Should match", ValueProviderToTest.IsResponsibleForReplacing("<ExcelFormula(\"=blah\r\nhello\")>", Passes.SecondPass));

			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<Excel formula(=blah)>", Passes.SecondPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<ExcelFormula>", Passes.SecondPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<>", Passes.SecondPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<ExcelFormula(blah)>", Passes.SecondPass));
			Assert("Should not match", !ValueProviderToTest.IsResponsibleForReplacing("<excelformula(<blah>)>", Passes.SecondPass));
		}

		public void TestReplacement()
		{
			AssertEquals("", ValueProviderToTest.GetReplacement("<ExcelFormula(\"\")>", Report));
			AssertEquals(60.00, ValueProviderToTest.GetReplacement("<ExcelFormula(\"=60\")>", Report));

			AssertEquals("", ValueProviderToTest.GetReplacement("<ExcelFormula(\"InvalidFormula\")>", Report));
			AssertMultilineASCIIEquals("", @"Severity: [Warning (without error report)] Message: [Error Replacing Macros in [<ExcelFormula("""")>] - Formula must start with ""="" or ""{="": """"'] Cell: [N/A]
Severity: [Warning (without error report)] Message: [Error Replacing Macros in [<ExcelFormula(""InvalidFormula"")>] - Formula must start with ""="" or ""{="": ""InvalidFormula""'] Cell: [N/A]".Trim(), Report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false));
		}

		public void TestReplaceFormulaInDocument()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<Z0_VarCharMax>]
{B}-[<ShrinkToFit><Z0_VarCharMax><Z0_VarCharMax>]
{B}-[<If(1==1,""<ExcelFormula(""= SUM(30,40,50)"")>"", ""2"")>]
{B}-[<If(1==1,""<ShrinkToFit><ExcelFormula(""= AVERAGE(70,80)"")>"", ""2"")>]
{B}-[<ShrinkToFit><If(1==1,""<ExcelFormula(""= MAX(<Z0_Decimal>, 40)"")>"", ""2"")>]
{B}-[<ShrinkToFit><If(1==1,""<ExcelFormula(""<Z0_VarCharMax>"")>"", ""2"")>]
{B}-[<ExcelFormula(""<Z0_VarCharMax>"")><ExcelFormula(""<Z0_VarCharMax>"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_VarCharMax = @"= 50";
			dummy.Z0_Decimal = 50;

			using (var documentPack = new DocumentPack(GetDocumentCommand(dummy, template), dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						var workSheet = excelInterface.WorkSheets.First();

						AssertEquals("= 50", workSheet[0, 1].ToString());
						AssertEquals("= 50= 50", workSheet[1, 1].ToString());
						AssertEquals("Formula '= SUM(30,40,50)' should have been calculated", "120", workSheet[2, 1].ToString());
						AssertEquals("Formula '= AVERAGE(70,80)' should have been calculated", "75", workSheet[3, 1].ToString());
						AssertEquals("Formula '= MAX(<Z0_Decimal>, 40)' should have been calculated", "50", workSheet[4, 1].ToString());
						AssertEquals("Formula should have been calculated", "50", workSheet[5, 1].ToString());
						AssertEquals("Formula should have been calculated", "5050", workSheet[6, 1].ToString());
					}
				}
			}
		}

		public void TestExcelFormulaReturnEmptyStringIFSheetIsNull()
		{
			Report dummyReportForGettingDefaults;
			var tempPack = new DocumentPack();

			dummyReportForGettingDefaults = new Report(tempPack, null, new System.Guid(), Core.Constants.DataContext.Shipment);
			AssertEquals("", ValueProviderToTest.GetReplacement("<ExcelFormula(\"\")>", dummyReportForGettingDefaults));
		}

		public void TestExcelFormulaReplaceNestedMacrosWhileIrresponsible()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[#SectionBody:Data=Collection]
{B}-[<Collection.Code>]     {C}-[<AutoHeight><Collection.Text>]     {D}-[<ExcelFormula(""=<Collection.Number> + 10"")>]
{A}-[#EndOfReport]");
			var dummy = Factory.New<DummyDocumentSupportable>();
			var child1 = dummy.Collection.AddNew("Long", number: 10);
			child1.Z0_VarCharMax = "This Is Very Long!!!!";

			var child2 = dummy.Collection.AddNew("Short", number: 20);
			child2.Z0_VarCharMax = "Short";

			using (var documentPack = new DocumentPack(GetDocumentCommand(dummy, template), dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);
						var workSheet = excelInterface.WorkSheets.First();
						var expected = "20";
						AssertEquals(expected, workSheet[0, 3].ToString());
						expected = "30";
						AssertEquals(expected, workSheet[3, 3].ToString());
					}
				}
			}
		}

		public void TestReplaceRelativeFormula()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<Z0_AnotherDecimal>]
{B}-[<ExcelFormula(""=ROUNDUP(INDIRECT(""R[-1]C"", FALSE), 0)"")>]
{B}-[<If(1==1,""<ExcelFormula(""=ROUNDUP(INDIRECT(""R[-2]C"", FALSE), 0)"")>"", ""1"")>]
{B}-[<If(1==1,""<ShrinkToFit><ExcelFormula(""=ROUNDUP(INDIRECT(""R[-3]C"", FALSE), 0)"")>"", ""1"")>]
{B}-[<ShrinkToFit><If(1==1,""<ExcelFormula(""=ROUNDUP(INDIRECT(""R[-4]C"", FALSE), 0)"")>"", ""1"")>]
{B}-[<ExcelFormula(""=ROUNDUP(INDIRECT(""R[-5]C"", FALSE), 0)"")><ExcelFormula(""=ROUNDUP(INDIRECT(""R[-5]C"", FALSE), 0)"")>]
{B}-[<Z0_AnotherDecimal>]
{B}-[<Z0_AnotherDecimal>]
{B}-[<Z0_AnotherDecimal>]
{B}-[<ExcelFormula(""=SUM(B10:B12)"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_AnotherDecimal = 1.5;

			using (var documentPack = new DocumentPack(GetDocumentCommand(dummy, template), dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						var workSheet = excelInterface.WorkSheets.First();

						AssertEquals("1.5", workSheet[0, 1].ToString());
						AssertEquals("2", workSheet[1, 1].ToString());
						AssertEquals("Formula should have been calculated", "2", workSheet[2, 1].ToString());
						AssertEquals("Formula should have been calculated", "2", workSheet[3, 1].ToString());
						AssertEquals("Formula should have been calculated", "2", workSheet[4, 1].ToString());
						AssertEquals("Formula should have been calculated", "22", workSheet[5, 1].ToString());
						AssertEquals("4.5", workSheet[9, 1].ToString());
					}
				}
			}
		}

		public void TestExcelFormulaSumValueInDifferentLanguage()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[TranslateLegacyDocument]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<ExcelFormula(""= SUM(<Z0_AnotherDecimal>)"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();
			dummy.Z0_AnotherDecimal = 1.100;

			using (var documentPack = new DocumentPack(GetDocumentCommand(dummy, template), dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				documentPack.Language = Core.Constants.Languages.Bulgarian;
				report.PrepareForRender();

				AssertEquals("language", Core.Constants.Languages.Bulgarian, report.Language);

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						var workSheet = excelInterface.WorkSheets.First();
						{
							AssertEquals("Formula '= SUM(1.100)' should have been calculated correctly", "1.1", workSheet[0, 1].ToString());
						}
					}
				}
			}
		}

		public void TestExcelFormulaBoundValueInDifferentLanguage()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[TranslateLegacyDocument]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<ExcelFormula(""= ROUND(""1.2345"", 3)"")>]
{B}-[<ExcelFormula(""= ROUND(1.2345, 3)"")>]
{A}-[#EndOfReport]");

			var dummy = Factory.New<DummyDocumentSupportable>();

			using (var documentPack = new DocumentPack(GetDocumentCommand(dummy, template), dummy, null, null))
			{
				var report = documentPack.GetFirstReport();
				documentPack.Language = Core.Constants.Languages.French;
				report.PrepareForRender();

				AssertEquals("language", Core.Constants.Languages.French, report.Language);

				using (var stream = new MemoryStream())
				{
					report.Save(stream);

					using (var excelInterface = new ExcelInterface())
					{
						excelInterface.LoadExcelFile(stream);

						var workSheet = excelInterface.WorkSheets.First();
						{
							AssertEquals("Formula '= ROUND(\"1.2345\", 3)' should have been calculated correctly", "1.235", workSheet[0, 1].ToString());
							AssertEquals("Formula '= ROUND(1.2345, 3)' should have been calculated correctly", "1.235", workSheet[1, 1].ToString());
						}
					}
				}
			}
		}

		public void TestCorrectFormula()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<ExcelFormula(""=1"")>]     {C}-[<ExcelFormula(""=4"")>]
{B}-[<ExcelFormula(""=2"")>]     {C}-[<ExcelFormula(""=5"")>]
{B}-[<ExcelFormula(""=3"")>]     {C}-[<ExcelFormula(""=6"")>]
{B}-[<ExcelFormula(""=SUMIFS(B4:B6, C4:C6, ""5"")"")>]
{A}-[#EndOfReport]");
			var dummy = Factory.New<DummyDocumentSupportable>();
			using var documentPack = new DocumentPack(GetDocumentCommand(dummy, template), dummy, null, null);
			var report = documentPack.GetFirstReport();
			using var stream = new MemoryStream();

			report.Save(stream);

			using var excelInterface = new ExcelInterface();
			excelInterface.LoadExcelFile(stream);
			var workSheet = excelInterface.WorkSheets[0];
			AssertEquals("SUMIFS return the correct value", "2", workSheet[3, 1].ToString());
			AssertEquals("ReportErrorManager has no errors", report.ErrorManager.ToString());
			AssertEquals("No issues were reported", string.Empty, ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			report.ErrorManager.ClearErrors();
		}

		public void TestIncorrectFormulaAndIsSystemDefined()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<ExcelFormula(""=1"")>]     {C}-[<ExcelFormula(""=4"")>]
{B}-[<ExcelFormula(""=2"")>]     {C}-[<ExcelFormula(""=5"")>]
{B}-[<ExcelFormula(""=3"")>]     {C}-[<ExcelFormula(""=6"")>]
{B}-[<ExcelFormula(""=SUMIFS(B4:B6, ""5"", C4:C6)"")>]
{A}-[#EndOfReport]");
			var dummy = Factory.New<DummyDocumentSupportable>();
			using var documentPack = new DocumentPack(GetDocumentCommand(dummy, template, isSystemDefined: true), dummy, null, null);
			var report = documentPack.GetFirstReport();
			using var stream = new MemoryStream();
			((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;

			report.Save(stream);

			AssertMultilineASCIIEquals(
				"ReportErrorManager has error",
				"Severity: [Error] Message: [Cannot evaluate <ExcelFormula(\"=SUMIFS(B4:B6, \"5\", C4:C6)\")>. Unable to cast object of type 'System.Double' to type 'FlexCel.Core.TreeNode'.] Cell: [B7]",
				report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false)
			);
			report.ErrorManager.ReportErrors();
			AssertContains("Report a issue to system", "Severity: [Error] Message: [Cannot evaluate <ExcelFormula(\"=SUMIFS(B4:B6, \"5\", C4:C6)\")>. Unable to cast object of type 'System.Double' to type 'FlexCel.Core.TreeNode'.] Cell: [B7]", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
			report.ErrorManager.ClearErrors();
		}

		public void TestIncorrectFormulaAndIsNotSystemDefined()
		{
			var clientEmai = "postmaster@sample.org";
			CreateClientInfo(clientEmai);
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test",
@"{A}-[#Config]
{A}-[DataContext=UnitTest]
{A}-[#SectionBody]
{B}-[<ExcelFormula(""=1"")>]     {C}-[<ExcelFormula(""=4"")>]
{B}-[<ExcelFormula(""=2"")>]     {C}-[<ExcelFormula(""=5"")>]
{B}-[<ExcelFormula(""=3"")>]     {C}-[<ExcelFormula(""=6"")>]
{B}-[<ExcelFormula(""=SUMIFS(B4:B6, ""5"", C4:C6)"")>]
{A}-[#EndOfReport]");
			var dummy = Factory.New<DummyDocumentSupportable>();
			using var documentPack = new DocumentPack(GetDocumentCommand(dummy, template, isSystemDefined: false), dummy, null, null);
			var report = documentPack.GetFirstReport();
			using var stream = new MemoryStream();
			((IReportForUnitTesting)report).GenerateRegardlessOfAnyErrors = true;

			report.Save(stream);

			AssertMultilineASCIIEquals(
				"ReportErrorManager has error",
				"Severity: [Error (without error report)] Message: [Cannot evaluate <ExcelFormula(\"=SUMIFS(B4:B6, \"5\", C4:C6)\")>. Unable to cast object of type 'System.Double' to type 'FlexCel.Core.TreeNode'.] Cell: [B7]",
				report.ErrorManager.ToString("Severity: [{0}] Message: [{1}] Cell: [{2}]", false)
			);
			report.ErrorManager.ReportErrors();
			AssertEquals("No issues were reported to system", string.Empty, ErrorReporter.LastMessageReported);
			AssertEquals("Emails to Client", 1, Env.OutgoingMailManager.EmailsCreated.Count);
			var email = Env.OutgoingMailManager.EmailsCreated[0];
			AssertEquals("email.Recipients", clientEmai, email.Recipients.RecipientsAsDelimitedString());
			ErrorReporter.Clear();
			report.ErrorManager.ClearErrors();
		}

		void CreateClientInfo(string clientEmai)
		{
			var postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
			var postMaster = postMasterGroup.Staff.AddNew();
			postMaster.GS_EmailAddress = clientEmai;
			postMaster.GS_Code = "_O_";
			postMaster.GS_LoginName = "postmastersample";
			Factory.Save();
		}

		DocumentCommand GetDocumentCommand(DummyDocumentSupportable dummy, StmTemplateBase template, bool isSystemDefined = false)
		{
			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;
			documentCommand.SU_IsSystemDefined = isSystemDefined;

			var document = documentCommand.Documents.AddNew();
			document.SI_SU = documentCommand.PK;
			document.SI_SO = template.PK;
			document.SI_IsSystemDefined = isSystemDefined;

			template.SO_IsSystemDefined = isSystemDefined;
			return documentCommand;
		}

		protected override ValueProvider GetNewValueProvider()
		{
			return new ExcelFormula();
		}
	}
}
