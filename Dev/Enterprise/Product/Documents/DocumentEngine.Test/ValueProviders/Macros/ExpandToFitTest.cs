using System;
using System.Linq;
using CargoWise.Types;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngine.ValueProviders.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.MacroValueProviders.Testing
{
	[TestedType(typeof(ExpandToFit))]
	sealed class ExpandToFitTest : ValueProviderTest<ExpandToFit>
	{
		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestEndToEnd()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(GetGeneratedDocumentBlob(@"CargoWise
Unit 3a, 72 O'Riordan Street,
Alexandria NSW 2015
Australia
PO Box 6390
Alexandria NSW 2015
Australia", "ExpandToFit.xls"));

				var workSheet = excelInterface.WorkSheets[0];
				var expected = new[] { 1899, 300, 1299, 300, 300, 300, 1899, 300, 1299, 300, 300, 300, 1899, 300, 300, 300, 1299, 300, 564 };

				CombineAssertions(() =>
				{
					for (var row = 0; row < expected.Length; row++)
					{
						var message = string.Format("Row {0}: {1}", row, workSheet[row, 1]);
						AssertEquals(message, expected[row], workSheet.GetRowHeight(row));
					}
				});
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[GuiTest]
		public void TestExpandToFitCatersForCellPadding()
		{
			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.LoadExcelFile(GetGeneratedDocumentBlob("PART:RT2781HB AV RECEIVER WITH HDMI SWITCH", "ExpandToFitPadding.xls"));
				var workSheet = excelInterface.WorkSheets[0];
				AssertEquals(757, workSheet.GetRowHeight(1));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestOnlyExpandsIfRowIsAlreadyVisible()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("ExpandToFit.xls", TestFilesSubFolder.DocumentTestFiles);
			var template = TemplateTestHelper.CreateTemplate(Factory, excelTemplate.TemplateName, excelTemplate.GetAsByteArray(), ".DummyBODocSupportable");

			using (var excelInterface = new ExcelInterface(excelTemplate.GetAsByteArray()))
			{
				var workSheet = excelInterface.WorkSheets[0];
				workSheet.SetRowHeight(0, 0);
				ExpandToFit.Expand(excelInterface, 0, 0, 0, RowToExpand.First, "Something");

				AssertEquals("Should not expand row to fit contents since the row has already been hidden", 0, workSheet.GetRowHeight(0));
			}
		}

		public void TestGetRowToExpand()
		{
			AssertEquals(RowToExpand.First, ExpandToFit.GetRowToExpand("<ExpandToFit>"));
			AssertEquals(RowToExpand.First, ExpandToFit.GetRowToExpand("<ExpandToFit(First)>"));
			AssertEquals(RowToExpand.Last, ExpandToFit.GetRowToExpand("<ExpandToFit(Last)>"));
			AssertEquals(RowToExpand.First, ExpandToFit.GetRowToExpand("<ExpandToFit(Middle)>"));
		}

		public void TestResponsibleForReplacing()
		{
			AssertNotResponsibleForReplacing("<>");

			AssertIsResponsibleForReplacing(@"<ExpandToFit>");
			AssertIsResponsibleForReplacing(@"<ExpandToFit(First)>");
			AssertIsResponsibleForReplacing(@"<ExpandToFit(Last)>");
			AssertIsResponsibleForReplacing(@"<ExpandToFit( First)>");
			AssertIsResponsibleForReplacing(@"<ExpandToFit( Last)>");
			AssertIsResponsibleForReplacing(@"<ExpandToFit(First )>");
			AssertIsResponsibleForReplacing(@"<ExpandToFit(Last )>");
			AssertIsResponsibleForReplacing(@"<ExpandToFit( First )>");
			AssertIsResponsibleForReplacing(@"<ExpandToFit( Last )>");

			AssertNotResponsibleForReplacing(@"<ExpandToFit(Firs)>");
			AssertNotResponsibleForReplacing(@"<ExpandToFit(Las)>");
			AssertNotResponsibleForReplacing(@"<ExpandToFit(Middle)>");
		}

		public void TestReplacement()
		{
			AssertIsReplacedWith(string.Empty, @"<ExpandToFit>");
			AssertIsReplacedWith(string.Empty, @"<ExpandToFit(First)>");
			AssertIsReplacedWith(string.Empty, @"<ExpandToFit(Last)>");
		}

		public void TestIsReplacedOnSecondPass()
		{
			AssertEquals("Should hide rows on first pass", Passes.FirstPass, new HideRowIf().PassToStartReplacingOn);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestDoesntExpandBeyondMaxLimitOfFlexCel()
		{
			var excelTemplate = new ExcelTemplateForUnitTesting("ExpandToFit.xls", TestFilesSubFolder.DocumentTestFiles);

			using (var excelInterface = excelTemplate.GetNewExcelInterface())
			{
				var reallyTallString = new String('w', Int32.MaxValue / 1000000).Aggregate("", (accumulator, c) =>
				{
					return accumulator + c + System.Environment.NewLine;
				});
				ExpandToFit.Expand(excelInterface, 0, 4, 1, RowToExpand.First, reallyTallString);
				AssertEquals(ExpandToFit.MaximumRowHeight, excelInterface.GetRowHeight(1, 4));
			}
		}

		public void TestIsINonVisualisableValueProviderThatModifyDocumentLayout()
		{
			var provider = GetNewValueProvider() as INonVisualisableValueProvider;

			AssertNotNull(provider);
			AssertEquals(ExpandToFit.RegexToFindMacroAnyWhereInString, provider.RegexToReplaceMacro);
		}

		ZBlob GetGeneratedDocumentBlob(string textForExpandToFit, string templatePath)
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			dummy.Z0_VarCharMax = textForExpandToFit;

			var excelTemplate = new ExcelTemplateForUnitTesting(templatePath, TestFilesSubFolder.DocumentTestFiles);
			var template = TemplateTestHelper.CreateTemplate(Factory, excelTemplate.TemplateName, excelTemplate.GetAsByteArray(), ".DummyBODocSupportable");

			var documentCommand = Factory.New<DocumentCommand>();
			documentCommand.Parent = dummy;

			var document = documentCommand.Documents.AddNew();
			document.SI_SO = template.PK;
			document.SI_SU = documentCommand.PK;

			var printJobs = DeliveryTestHelper.DeliverDocument(documentCommand);
			AssertEquals("printJobs.Length", 1, printJobs.Length);

			return printJobs[0].SP_CustomProperties;
		}
	}
}
