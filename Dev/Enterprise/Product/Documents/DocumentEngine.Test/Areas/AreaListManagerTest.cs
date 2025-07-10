using System.Collections.Generic;
using System.IO;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Areas.Testing
{
	sealed class AreaListManagerTest : TestCaseWithFactory
	{
		public void TestAreaListManagerDoesNotContainSectionForeachArea()
		{
			var listManager = new AreaListManager();
			Assert("We don't need to contain SectionForeachArea as this special area is only used inside SectionBodyArea", !listManager.Contains(typeof(SectionForeachArea)));
		}

		public void TestTerminologyAndGrammarUsedInDocumentationAttributes()
		{
			foreach (ValueProviderDocumenter documenter in new AreaListManager().GetDocumenters())
			{
				string explanationWithBadSectionReferencesHighlighted = documenter.Explanation.ToString().ToLower()
					.Replace("body section", "Body Section")
					.Replace("sectionforeach", "SectionForeach")
					.Replace("sectionbody", "SectionBody")
					.Replace("section", "***<<<SECTION>>>***")
					.Replace("#***<<<SECTION>>>***", "#Section");
				AssertNotContains("areaDocumentation.Explanation should use the keyword 'body section' or 'area', not 'section'.\r\n" + documenter.Useage
					, "SECTION", explanationWithBadSectionReferencesHighlighted);
				string explanationWithLowerCaseAreaWordHighlighted = documenter.Explanation.Replace("body section", "Body Section").Replace("area", "***<<<AREA>>>***");
				AssertNotContains("areaDocumentation.Explanation should have all Area references in proper case.\r\n" + documenter.Useage
					, "AREA", explanationWithLowerCaseAreaWordHighlighted);
			}
		}

		public void TestGetDocumentationAttributesArePresentAndUniqueish()
		{
			var listManager = new AreaListManager();
			var documenters = listManager.GetDocumenters();
			AssertNotNull("listManager.GetDocumenters()", documenters);
			AssertEquals("documentationAttributes.Count", listManager.Count + 3, documenters.Count);
			List<string> useages = new List<string>();
			foreach (var documenter in documenters)
			{
				AssertNotNull("documenter", documenter);
				AssertEquals("documenter.Useage.IsEmpty", false, documenter.Useage.IsEmpty);
				AssertEquals("documenter.Explanation.IsEmpty", false, documenter.Explanation.IsEmpty);
				if (useages.Contains(documenter.Useage))
				{
					Fail("Useage is duplicated between 2 Documenters: " + documenter.Useage);
				}
				useages.Add(documenter.Useage);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		[ExpectNoExceptions]
		public void TestPopulateDuplicatedAreas()
		{
			var mainBO = Factory.New<DummyBusinessObject>();
			var excelTemplate = new ExcelTemplateForUnitTesting("MultiSectionTemplateOutput.xls", TestFilesSubFolder.AreaTestFiles);
			using (var report = GetNewReport(BODocDataProvider.Get(mainBO), excelTemplate))
			{
				report.PrepareForRender();
				report.Analyser.Areas[report.Analyser.Areas.Count - 3] = null;

				foreach (var section in report.Analyser.Sections)
				{
					section.PopulateRows(report);
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultiSectionTemplateOutput1Page()
		{
			var mainBO = Factory.New<DummyBusinessObject>();
			mainBO.Z0_VarCharMax = "Document";
			AddChildRows(mainBO, 1, "A");
			AddChildRows(mainBO, 2, "B");

			var excelTemplate = new ExcelTemplateForUnitTesting("MultiSectionTemplateOutput.xls", TestFilesSubFolder.AreaTestFiles);
			using (var report = GetNewReport(BODocDataProvider.Get(mainBO), excelTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var xlInterface = new ExcelInterface())
				{
					xlInterface.LoadExcelFile(outputStream);
					AssertMultilineASCIIEquals("", ExpectedMultiSectionTemplateOutput1Page.Trim(), xlInterface.WorkSheets[0].ToString());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMultiSectionTemplateOutput3Pages()
		{
			var mainBO = Factory.New<DummyBusinessObject>();
			mainBO.Z0_VarCharMax = "Document";
			AddChildRows(mainBO, 15, "A");
			AddChildRows(mainBO, 20, "B");
			AddChildRows(mainBO, 25, "C");

			var excelTemplate = new ExcelTemplateForUnitTesting("MultiSectionTemplateOutput.xls", TestFilesSubFolder.AreaTestFiles);
			using (var report = GetNewReport(BODocDataProvider.Get(mainBO), excelTemplate))
			using (var outputStream = new MemoryStream())
			{
				report.Save(outputStream);
				using (var xlInterface = new ExcelInterface())
				{
					xlInterface.LoadExcelFile(outputStream);
					AssertMultilineASCIIEquals("", ExpectedMultiSectionTemplateOutput3Pages.Trim(), xlInterface.WorkSheets[0].ToString());
				}
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsAreaInCorrectOrder()
		{
			using (var report = GetNewReport(null, new ExcelTemplateForUnitTesting("DummyBusinessObjectAsDataSource.xls", TestFilesSubFolder.DocumentTestFiles)))
			{
				var headerArea = new PageHeaderArea(0, 0, report, "#PageHeader");
				var groupByArea = new GroupByArea(0, 0, report, "#GroupBY:Hirsuitidity");

				AssertEquals(true, ListManager.IsAreaInCorrectOrder(headerArea, groupByArea));
				AssertEquals(false, ListManager.IsAreaInCorrectOrder(groupByArea, headerArea));
				AssertEquals(true, ListManager.IsAreaInCorrectOrder(groupByArea, groupByArea));
				AssertEquals(false, ListManager.IsAreaInCorrectOrder(headerArea, headerArea));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestIsSectionArea()
		{
			using (var report = GetNewReport(null, new ExcelTemplateForUnitTesting("DummyBusinessObjectAsDataSource.xls", TestFilesSubFolder.DocumentTestFiles)))
			{
				var headerArea = new PageHeaderArea(0, 0, report, "#PageHeader");
				var groupByArea = new GroupByArea(0, 0, report, "#GroupBY:Hirsuitidity");

				AssertEquals(true, ListManager.IsSectionArea(groupByArea));
				AssertEquals(false, ListManager.IsSectionArea(headerArea));
			}
		}

		public void TestManagerContainsAllAreaTypes()
		{
			var count = 0;
			foreach (var type in Assembly.Load("Enterprise.DocumentEngine").GetTypes())
			{
				if (type.IsSubclassOf(typeof(Area)) && !type.IsAbstract && !type.IsNested && type.Name != nameof(SectionForeachArea))
				{
					AssertCollectionContains("ListManager contains [" + type.FullName + "]", type, ListManager);
					count++;
				}
			}
			AssertEquals("ListManager.Count", count, ListManager.Count);
		}

		void AddChildRows(DummyBusinessObject mainBO, int count, string groupCode)
		{
			for (var index = 1; index <= count; index++)
			{
				var child = mainBO.Collection.AddNew();
				child.Z0_FK_Code = groupCode;
				child.Z0_Code = index.ToString().PadLeft(2, '0');
			}
		}

		Report GetNewReport(IBODocDataProvider topLevelDataSource, ExcelTemplateForUnitTesting excelTemplate)
		{
			var stmMenuItem = Factory.New<DocumentCommand>();
			var documentPack = new DocumentPack(stmMenuItem);
			if (topLevelDataSource == null)
			{
				return new Report(documentPack, excelTemplate);
			}
			return new Report(documentPack, excelTemplate, topLevelDataSource, "Test_" + excelTemplate.TemplateName, null, DocumentDirection.ANY, false);
		}

		AreaListManager fListManager;
		AreaListManager ListManager => fListManager ?? (fListManager = new AreaListManager());

		const string ExpectedMultiSectionTemplateOutput1Page = @"
{B}-[DocumentHeader]
{B}-[PageHeader]
{B}-[SectionHeader1-A]
{B}-[GroupByHeader1-A]
{B}-[SectionBody1-A-01]
{B}-[GroupByFooter1-A]
{B}-[GroupByHeader1-B]
{B}-[SectionBody1-B-01]
{B}-[SectionBody1-B-02]
{B}-[GroupByFooter1-B]
{B}-[SectionFooter1-A]
{B}-[SectionHeader2-A]
{B}-[GroupByHeader2-A]
{B}-[SectionBody2-A-01]
{B}-[GroupByFooter2-A]
{B}-[GroupByHeader2-B]
{B}-[SectionBody2-B-01]
{B}-[SectionBody2-B-02]
{B}-[GroupByFooter2-B]
{B}-[SectionFooter2-A]
{B}-[DocumentFooter]

{B}-[OnlyOnePageFooter]
{B}-[BackPage]
";

		const string ExpectedMultiSectionTemplateOutput3Pages = @"
{B}-[DocumentHeader]
{B}-[PageHeader]
{B}-[SectionHeader1-A]
{B}-[GroupByHeader1-A]
{B}-[SectionBody1-A-01]
{B}-[SectionBody1-A-02]
{B}-[SectionBody1-A-03]
{B}-[SectionBody1-A-04]
{B}-[SectionBody1-A-05]
{B}-[SectionBody1-A-06]
{B}-[SectionBody1-A-07]
{B}-[SectionBody1-A-08]
{B}-[SectionBody1-A-09]
{B}-[SectionBody1-A-10]
{B}-[SectionBody1-A-11]
{B}-[SectionBody1-A-12]
{B}-[SectionBody1-A-13]
{B}-[SectionBody1-A-14]
{B}-[SectionBody1-A-15]
{B}-[GroupByFooter1-A]
{B}-[GroupByHeader1-B]
{B}-[SectionBody1-B-01]
{B}-[SectionBody1-B-02]
{B}-[SectionBody1-B-03]
{B}-[SectionBody1-B-04]
{B}-[SectionBody1-B-05]
{B}-[SectionBody1-B-06]
{B}-[SectionBody1-B-07]
{B}-[SectionBody1-B-08]
{B}-[SectionBody1-B-09]
{B}-[SectionBody1-B-10]
{B}-[SectionBody1-B-11]
{B}-[SectionBody1-B-12]
{B}-[SectionBody1-B-13]
{B}-[SectionBody1-B-14]
{B}-[SectionBody1-B-15]
{B}-[SectionBody1-B-16]
{B}-[SectionBody1-B-17]
{B}-[SectionBody1-B-18]
{B}-[SectionBody1-B-19]
{B}-[SectionBody1-B-20]
{B}-[GroupByFooter1-B]
{B}-[GroupByHeader1-C]
{B}-[SectionBody1-C-01]
{B}-[SectionBody1-C-02]
{B}-[SectionBody1-C-03]
{B}-[SectionBody1-C-04]
{B}-[SectionBody1-C-05]
{B}-[SectionBody1-C-06]
{B}-[SectionBody1-C-07]
{B}-[SectionBody1-C-08]
{B}-[SectionBody1-C-09]
{B}-[SectionBody1-C-10]

{B}-[SectionPageFooter1-A]
{B}-[FirstPageFooter]
{B}-[BackPage]
{B}-[PageHeader]
{B}-[SectionPageHeader1-C]
{B}-[SectionBody1-C-11]
{B}-[SectionBody1-C-12]
{B}-[SectionBody1-C-13]
{B}-[SectionBody1-C-14]
{B}-[SectionBody1-C-15]
{B}-[SectionBody1-C-16]
{B}-[SectionBody1-C-17]
{B}-[SectionBody1-C-18]
{B}-[SectionBody1-C-19]
{B}-[SectionBody1-C-20]
{B}-[SectionBody1-C-21]
{B}-[SectionBody1-C-22]
{B}-[SectionBody1-C-23]
{B}-[SectionBody1-C-24]
{B}-[SectionBody1-C-25]
{B}-[GroupByFooter1-C]
{B}-[SectionFooter1-A]
{B}-[SectionHeader2-A]
{B}-[GroupByHeader2-A]
{B}-[SectionBody2-A-01]
{B}-[SectionBody2-A-02]
{B}-[SectionBody2-A-03]
{B}-[SectionBody2-A-04]
{B}-[SectionBody2-A-05]
{B}-[SectionBody2-A-06]
{B}-[SectionBody2-A-07]
{B}-[SectionBody2-A-08]
{B}-[SectionBody2-A-09]
{B}-[SectionBody2-A-10]
{B}-[SectionBody2-A-11]
{B}-[SectionBody2-A-12]
{B}-[SectionBody2-A-13]
{B}-[SectionBody2-A-14]
{B}-[SectionBody2-A-15]
{B}-[GroupByFooter2-A]
{B}-[GroupByHeader2-B]
{B}-[SectionBody2-B-01]
{B}-[SectionBody2-B-02]
{B}-[SectionBody2-B-03]
{B}-[SectionBody2-B-04]
{B}-[SectionBody2-B-05]
{B}-[SectionBody2-B-06]
{B}-[SectionBody2-B-07]
{B}-[SectionBody2-B-08]
{B}-[SectionBody2-B-09]
{B}-[SectionBody2-B-10]
{B}-[SectionBody2-B-11]
{B}-[SectionBody2-B-12]
{B}-[SectionBody2-B-13]
{B}-[SectionBody2-B-14]
{B}-[SectionBody2-B-15]

{B}-[SectionPageFooter2-A]
{B}-[PageFooter]
{B}-[BackPage]
{B}-[PageHeader]
{B}-[SectionPageHeader2-B]
{B}-[SectionBody2-B-16]
{B}-[SectionBody2-B-17]
{B}-[SectionBody2-B-18]
{B}-[SectionBody2-B-19]
{B}-[SectionBody2-B-20]
{B}-[GroupByFooter2-B]
{B}-[GroupByHeader2-C]
{B}-[SectionBody2-C-01]
{B}-[SectionBody2-C-02]
{B}-[SectionBody2-C-03]
{B}-[SectionBody2-C-04]
{B}-[SectionBody2-C-05]
{B}-[SectionBody2-C-06]
{B}-[SectionBody2-C-07]
{B}-[SectionBody2-C-08]
{B}-[SectionBody2-C-09]
{B}-[SectionBody2-C-10]
{B}-[SectionBody2-C-11]
{B}-[SectionBody2-C-12]
{B}-[SectionBody2-C-13]
{B}-[SectionBody2-C-14]
{B}-[SectionBody2-C-15]
{B}-[SectionBody2-C-16]
{B}-[SectionBody2-C-17]
{B}-[SectionBody2-C-18]
{B}-[SectionBody2-C-19]
{B}-[SectionBody2-C-20]
{B}-[SectionBody2-C-21]
{B}-[SectionBody2-C-22]
{B}-[SectionBody2-C-23]
{B}-[SectionBody2-C-24]
{B}-[SectionBody2-C-25]
{B}-[GroupByFooter2-C]
{B}-[SectionFooter2-A]
{B}-[DocumentFooter]

{B}-[LastPageFooter]
{B}-[BackPage]
";
	}
}
