using System;
using CargoWise.EntityFramework.Testing;
using CargoWiseOne.ResourceStrings;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using FlexCel.Core;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	sealed class StripManipulatorTest : TestCaseWithFactory
	{
		public void TestCopyAndInsertWithFormulasWithMultipleCellReferencesThatNeedTranslations()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
@"{B}-[Hello]
{B}-[and goodbye]");

			workSheet[2, 1] = new TFormula("=CONCATENATE(CONCATENATE(CONCATENATE(B1, \" \"), B2), \" world\")");
			workSheet.ReCalc();
			AssertMultilineASCIIEquals("Pre-condition: Translate Formula with Cell References",
@"{B}-[Hello]
{B}-[and goodbye]
{B}-[Hello and goodbye world]",
				workSheet.ToString());

			using (var resourceStrings = Res.UseMockData())
			{
				try
				{
					var templateSection = new TemplateSection("#ConfigurableSection:GEN:Test, My Section", 0, 0, language: Core.SharedConstants.Languages.French);

					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "Hello"), new ResourceStringData("", "Bonjour"));
					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "and goodbye"), new ResourceStringData("", "et au revoir"));
					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "world"), new ResourceStringData("", "du monde"));

					var manipulator = new StripManipulator(workSheet);

					manipulator.CopyAndInsertRows(workSheet, 0, 3, templateSection);
					manipulator.RemoveUnnecessaryRows();

					AssertMultilineASCIIEquals("Translate Formula with Cell References",
	@"{B}-[Bonjour]
{B}-[et au revoir]
{B}-[Bonjour et au revoir du monde]",
						workSheet.ToString());
				}
				finally
				{
					workSheet.ParentExcelInterface.Dispose();
				}
			}
		}

		public void TestCopyAndInsertWithFormulasWithCellReferencesThatNeedTranslations()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
@"{B}-[Hello]");

			workSheet[1, 1] = new TFormula("=CONCATENATE(B1, \" world\")");
			workSheet.ReCalc();
			AssertMultilineASCIIEquals("Pre-condition: Translate Formula with Cell References",
@"{B}-[Hello]
{B}-[Hello world]",
				workSheet.ToString());

			using (var resourceStrings = Res.UseMockData())
			{
				try
				{
					var templateSection = new TemplateSection("#ConfigurableSection:GEN:Test, My Section", 0, 0, language: Core.SharedConstants.Languages.French);

					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "Hello"), new ResourceStringData("", "Bonjour"));
					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "world"), new ResourceStringData("", "tout le monde"));

					var manipulator = new StripManipulator(workSheet);

					manipulator.CopyAndInsertRows(workSheet, 0, 2, templateSection);
					manipulator.RemoveUnnecessaryRows();

					AssertMultilineASCIIEquals("Translate Formula with Cell References",
	@"{B}-[Bonjour]
{B}-[Bonjour tout le monde]",
						workSheet.ToString());
				}
				finally
				{
					workSheet.ParentExcelInterface.Dispose();
				}
			}
		}

		public void TestCopyAndInsertWithFormulasThatNeedTranslations()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
@"{B}-[]");

			workSheet[0, 1] = new TFormula("=CONCATENATE(\"Hello\", \" world\")");

			AssertEquals("Pre-condition: workSheet.GetCell(0, 1).IsFormula", true, workSheet.GetCell(0, 1).IsFormula);
			workSheet.ReCalc();
			AssertMultilineASCIIEquals("Pre-condition: Translate Formula",
@"{B}-[Hello world]",
				workSheet.ToString());

			using (var resourceStrings = Res.UseMockData())
			{
				try
				{
					var templateSection = new TemplateSection("#ConfigurableSection:GEN:Test, My Section", 0, 0, language: Core.SharedConstants.Languages.French);

					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "Hello"), new ResourceStringData("", "Bonjour"));
					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "world"), new ResourceStringData("", "tout le monde"));

					var manipulator = new StripManipulator(workSheet);

					manipulator.CopyAndInsertRows(workSheet, 0, 1, templateSection);
					manipulator.RemoveUnnecessaryRows();

					AssertEquals("Pre-condition: workSheet.GetCell(0, 1).IsFormula", true, workSheet.GetCell(0, 1).IsFormula);
					AssertMultilineASCIIEquals("Translate Formula",
	@"{B}-[Bonjour tout le monde]",
						workSheet.ToString());
				}
				finally
				{
					workSheet.ParentExcelInterface.Dispose();
				}
			}
		}

		public void TestCopyAndInsertWithTranslations()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
@"{B}-[Hello1]
{B}-[Hello2]
{B}-[Hello3]");

			using (var resourceStrings = Res.UseMockData())
			{
				try
				{
					var templateSection = new TemplateSection("#ConfigurableSection:GEN:Test, My Section", 0, 0, language: Core.SharedConstants.Languages.Italian);

					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "Hello1"), new ResourceStringData("", "Ciao1"));
					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "Hello2"), new ResourceStringData("", "Ciao2"));
					resourceStrings.Put(DocBuilderResourceStrings.GetKey(templateSection, "Hello3"), new ResourceStringData("", "Ciao3"));

					var manipulator = new StripManipulator(workSheet);

					manipulator.CopyAndInsertRows(workSheet, 0, 1, templateSection);
					manipulator.CopyAndInsertRows(workSheet, 2, 1, templateSection);
					manipulator.RemoveUnnecessaryRows();

					AssertMultilineASCIIEquals("Translate Formula",
	@"{B}-[Ciao1]
{B}-[Ciao3]",
						workSheet.ToString());
				}
				finally
				{
					workSheet.ParentExcelInterface.Dispose();
				}
			}
		}

		public void TestCopyAndInsertNegativeRowCount()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
@"{B}-[Hello1]");

			using (workSheet.ParentExcelInterface)
			{
				var templateSection = new TemplateSection("#ConfigurableSection:GEN:Test, My Section", 0, 0);

				var manipulator = new StripManipulator(workSheet);
				AssertNoExceptionThrown(() => manipulator.CopyAndInsertRows(workSheet, 1, -2, templateSection));
			}
		}

		public void TestInsertPageHeaderAll()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.PageHeaderAll,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#DocumentHeader]
{B}-[This is Generic Section 1.]
{A}-[#PageHeader:StartFromSecondPage]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertPageHeaderFirstPageOnly()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.PageHeaderFirstPageOnly,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#DocumentHeader]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertPageHeaderStartFromSecondPage()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.PageHeaderStartFromSecondPage,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#PageHeader:StartFromSecondPage]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertBodySection()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.BodySection,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#SectionBody]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertBodySectionExpanding()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.BodySectionExpanding,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{B}-[This is Generic Section 1.]");
		}

		public void TestInsertPageFooterFirstPageOnly()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.PageFooterFirstPageOnly,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#FirstPageFooter]
{B}-[This is Generic Section 1.]
{A}-[#OnlyOnePageFooter]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertPageFooterAllExceptLastPage()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.PageFooterAllExceptLastPage,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#FirstPageFooter]
{B}-[This is Generic Section 1.]
{A}-[#PageFooter]
{B}-[This is Generic Section 1.]
{A}-[#LastPageFooter]");
		}

		public void TestInsertPageFooterLastPage()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.PageFooterLastPage,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#LastPageFooter]
{B}-[This is Generic Section 1.]
{A}-[#OnlyOnePageFooter]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertPageFooterAll()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.PageFooterAll,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#PageFooter]
{B}-[This is Generic Section 1.]
{A}-[#FirstPageFooter]
{B}-[This is Generic Section 1.]
{A}-[#OnlyOnePageFooter]
{B}-[This is Generic Section 1.]
{A}-[#LastPageFooter]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertPageFooterFallbackDefault()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.PageFooterFallbackDefault,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#PageFooter]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertFirstPageFooterUnlessOnlyOnePage()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.FirstPageFooterUnlessOnlyOnePage,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#FirstPageFooter]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertFirstPageFooterWhenOnlyOnePage()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.FirstPageFooterWhenOnlyOnePage,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#OnlyOnePageFooter]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertBackPage()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.BackPage,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#BackPage]
{B}-[This is Generic Section 1.]");
		}

		public void TestInsertBackPageFirstPageOnly()
		{
			AssertInsertGenericStrip(
				GenericSectionUsageList.Codes.BackPageFirstPageOnly,
				ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory),
				"Generic Section 1",
@"{A}-[#BackPage:FirstPageOnly]
{B}-[This is Generic Section 1.]");
		}

		public void TestConstructor()
		{
			AssertExceptionThrown(typeof(ArgumentNullException), () =>
			{
				var stripInserterWithNullDestinationWorkSheet = new StripManipulator(null);
			});

			using (var excelInterface = new ExcelInterface())
			{
				excelInterface.NewExcelFile(1);
				var destinationWorkSheet = excelInterface.WorkSheets[0];

				AssertNoExceptionThrown(() =>
				{
					var stripInserter = new StripManipulator(destinationWorkSheet);
				});
			}
		}

		public void TestInsertStrip()
		{
			using (var sourceExcelInterface = new ExcelInterface())
			using (var destinationExcelInterface = new ExcelInterface())
			{
				var sourceTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Source",
@"{A}-[#config]
{A}-[Name=CustomisableSectionTest]
{A}-[#ConfigurableSection:PHD, Page Header 1]
{A}-[#PageHeader]
{B}-[This is Page Header 1 in Source.]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1 in Source.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2 in Source.]
{A}-[#ConfigurableSection:GEN, Generic Section 3]
{B}-[This is Generic Section 3 in Source.]
{A}-[#EndOfReport]");
				var sourceExcelTemplate = sourceTemplate.GetExcelTemplate();

				using (var sourceStream = sourceExcelTemplate.GetAsTemplateStream())
				{
					sourceExcelInterface.LoadExcelFile(sourceStream);
				}

				using (var destinationStream = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Destination",
@"{A}-[#config]
{A}-[Name=CustomisableSectionTest]
{A}-[#ConfigurableSection:PHD, Page Header 1]
{A}-[#PageHeader]
{B}-[This is Page Header 1 in Destination.]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1 in Destination.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2 in Destination.]
{A}-[#ConfigurableSection:GEN, Generic Section 3]
{B}-[This is Generic Section 3 in Destination.]
{A}-[#EndOfReport]").GetExcelTemplate().GetAsTemplateStream())
				{
					destinationExcelInterface.LoadExcelFile(destinationStream);
				}

				var sourceWorkSheet = sourceExcelInterface.WorkSheets[0];
				var destinationWorkSheet = destinationExcelInterface.WorkSheets[0];
				var sourceSectionRepository = new SectionRepository(sourceExcelTemplate);

				var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(sourceTemplate);

				var templateSection1 = sourceSectionRepository.AllSections.Find("Generic Section 2");
				var configItem1 = config.ConfigItems.AddFromTemplateSection(templateSection1);

				var stripInserter = new StripManipulator(destinationWorkSheet);
				stripInserter.InsertStrip(configItem1, sourceWorkSheet, templateSection1);

				AssertMultilineASCIIEquals("destinationWorkSheet.ToString()",
@"{A}-[#SectionBody]
{B}-[This is Generic Section 2 in Source.]
{A}-[#config]
{A}-[Name=CustomisableSectionTest]
{A}-[#ConfigurableSection:PHD, Page Header 1]
{A}-[#PageHeader]
{B}-[This is Page Header 1 in Destination.]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1 in Destination.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2 in Destination.]
{A}-[#ConfigurableSection:GEN, Generic Section 3]
{B}-[This is Generic Section 3 in Destination.]
{A}-[#EndOfReport]", destinationWorkSheet.ToString());
			}
		}

		public void TestRightToLeftFlipData()
		{
			var workSheet = DocumentEngineTestHelper.CreateExcelWorkSheetFromString(
@"{B}-[<A> <B>]
{B}-F[=""A "" & ""B""]
{B}-[<A> <B> <C>]
{B}-[<A> / <B> / <C>]
{B}-[<A> <B> <C> <D>]
{B}-[<A> Something <B>]
{B}-F[=CONCATENATE(""A "", ""B"")]
");

			using (Res.TemporarilySwitchLanguage(Core.SharedConstants.Languages.Arabic))
			{
				try
				{
					var templateSection = new TemplateSection("#ConfigurableSection:GEN:Test, My Section", 0, 0);

					var manipulator = new StripManipulator(workSheet);
					for (int i = 0; i < 7; i++)
					{
						manipulator.CopyAndInsertRows(workSheet, i, 1, templateSection);
					}
					manipulator.RemoveUnnecessaryRows();

					AssertMultilineASCIIEquals("Translated",
@"{B}-[<B> <A>]
{B}-[B A]
{B}-[<C> <B> <A>]
{B}-[<C> / <B> / <A>]
{B}-[<D> <C> <B> <A>]
{B}-[<A> Something <B>]
{B}-[B A]",
						workSheet.ToString());
				}
				finally
				{
					workSheet.ParentExcelInterface.Dispose();
				}
			}
		}

		#region Implementation

		void AssertInsertGenericStrip(string sectionType, StmTemplateBase sourceTemplate, string sectionName, string expected)
		{
			var sourceTemplateSection = sourceTemplate.TemplateSections.Find(sectionName);

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(sourceTemplate);
			var configItem = config.ConfigItems.AddFromTemplateSection(sourceTemplateSection);
			configItem.S4_SectionType = sectionType;

			using (var sourceExcelInterface = new ExcelInterface())
			using (var destinationExcelInterface = new ExcelInterface())
			{
				using (var sourceStream = sourceTemplate.GetExcelTemplate().GetAsTemplateStream())
				{
					sourceExcelInterface.LoadExcelFile(sourceStream);
				}

				destinationExcelInterface.NewExcelFile(1);

				var sourceWorkSheet = sourceExcelInterface.WorkSheets[0];
				var destinationWorkSheet = destinationExcelInterface.WorkSheets[0];

				var stripInserter = new StripManipulator(destinationWorkSheet);
				stripInserter.InsertStrip(configItem, sourceWorkSheet, sourceTemplateSection);

				AssertMultilineASCIIEquals(string.Format("Inserting section of type [{0}] is not correct.", sectionType), expected, destinationWorkSheet.ToString());
			}
		}

		#endregion
	}
}
