using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.Exceptions;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.ExcelTemplates;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.DocBuilder.Testing
{
	[TestedType(typeof(TemplateSectionCollection))]
	sealed class TemplateSectionCollectionTest : NonPersistentBusinessObjectCollectionTestCase<TemplateSectionCollection>
	{
		public void TestGetNewCollection()
		{
			var systemTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "System Document Elements",
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=Test System Template]
{A}-[#ConfigurableSection:BOD, Body Section 1]
{A}-[#SectionBody]
{B}-[This is the system Body Section 1.]
{A}-[#ConfigurableSection:BOD, Body Section 2]
{A}-[#SectionBody]
{B}-[This is the system Body Section 2.]
{A}-[#EndOfReport]");

			var userTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Customized Document Elements",
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=Test User Configurable Template]
{A}-[#ConfigurableSection:BOD, Body Section A]
{A}-[#SectionBody]
{B}-[This is the user Body Section A.]
{A}-[#EndOfReport]");

			var systemSectionCollection = new TemplateSectionCollection(new ExcelTemplateReadFromStmTemplateTable(systemTemplate));
			var userSectionCollection = new TemplateSectionCollection(new ExcelTemplateReadFromStmTemplateTable(userTemplate));

			AssertEquals(4, systemSectionCollection.Count);
			AssertEquals(ConfigurableSectionTypeList.Codes.ConfigSection, systemSectionCollection[0].TypeCode);
			AssertEquals(ConfigurableSectionTypeList.Codes.BodySection, systemSectionCollection[1].TypeCode);
			AssertEquals("Body Section 1", systemSectionCollection[1].SectionName);
			AssertEquals(ConfigurableSectionTypeList.Codes.BodySection, systemSectionCollection[2].TypeCode);
			AssertEquals("Body Section 2", systemSectionCollection[2].SectionName);
			AssertEquals(ConfigurableSectionTypeList.Codes.EndOfReport, systemSectionCollection[3].TypeCode);

			AssertEquals(3, userSectionCollection.Count);
			AssertEquals(ConfigurableSectionTypeList.Codes.ConfigSection, userSectionCollection[0].TypeCode);
			AssertEquals(ConfigurableSectionTypeList.Codes.BodySection, userSectionCollection[1].TypeCode);
			AssertEquals("Body Section A", userSectionCollection[1].SectionName);
			AssertEquals(ConfigurableSectionTypeList.Codes.EndOfReport, userSectionCollection[2].TypeCode);

			Factory.Save();

			var cacheManager = TemplateSectionCollection.CacheManager.Get(Factory);
			cacheManager.GetCollection();
			cacheManager.InvalidateTemplateSectionsCache();

			AssertNoExceptionThrown("System.InvalidOperationException about adding a deleted business object SHOULD NOT BE THROWN!", delegate
			{ cacheManager.GetCollection(); });
		}

		public void TestGetNewConfigurableOnlyCollection()
		{
			PrepareTestTemplates();

			var systemSectionCollection = new TemplateSectionCollection(new ExcelTemplateReadFromStmTemplateTable(testSystemTemplate), true);
			var userSectionCollection = new TemplateSectionCollection(new ExcelTemplateReadFromStmTemplateTable(testUserTemplate), true);

			AssertEquals(2, systemSectionCollection.Count);
			AssertEquals(ConfigurableSectionTypeList.Codes.BodySection, systemSectionCollection[0].TypeCode);
			AssertEquals("Body Section 1", systemSectionCollection[0].SectionName);
			AssertEquals(ConfigurableSectionTypeList.Codes.BodySection, systemSectionCollection[1].TypeCode);
			AssertEquals("Body Section 2", systemSectionCollection[1].SectionName);

			AssertEquals(2, userSectionCollection.Count);
			AssertEquals(ConfigurableSectionTypeList.Codes.BodySection, userSectionCollection[0].TypeCode);
			AssertEquals("Body Section 1", userSectionCollection[0].SectionName);
			AssertEquals(ConfigurableSectionTypeList.Codes.BodySection, userSectionCollection[1].TypeCode);
			AssertEquals("Body Section A", userSectionCollection[1].SectionName);

			Factory.Save();

			var cacheManager = TemplateSectionCollection.CacheManager.Get(Factory);
			TemplateSectionCollection sections = cacheManager.GetConfigurableOnlyCollection();
			AssertEquals(4, sections.Count);
			var orderedSections = sections.Cast<TemplateSection>().ToList().OrderBy(p => p.SectionName).ToList();
			AssertEquals("Body Section 1", orderedSections[0].SectionName);
			AssertEquals("Body Section 1", orderedSections[1].SectionName);
			AssertEquals("Body Section 2", orderedSections[2].SectionName);
			AssertEquals("Body Section A", orderedSections[3].SectionName);
		}

		public void TestCacheManagerSystemSections()
		{
			PrepareTestTemplates();
			Factory.Save();

			var cacheManager = TemplateSectionCollection.CacheManager.Get(Factory);
			var systemSections = cacheManager.SystemTemplateSections;
			var allSections = cacheManager.GetCollection();

			AssertEquals(4, systemSections.Count);
			AssertEquals("Config Section", systemSections[0].SectionName);
			AssertEquals("Body Section 1", systemSections[1].SectionName);
			AssertEquals("Body Section 2", systemSections[2].SectionName);
			AssertEquals("End of Report", systemSections[3].SectionName);

			AssertEquals(5, allSections.Count);
			AssertEquals("Config Section", allSections[0].SectionName);
			AssertEquals("Body Section 1", allSections[1].SectionName);
			AssertEquals("Body Section A", allSections[2].SectionName);
			AssertEquals("End of Report", allSections[3].SectionName);
			AssertEquals("Body Section 2", allSections[4].SectionName);
		}

		public void TestContains()
		{
			var template = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Test Template",
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=Test Configurable Template]
{A}-[#ConfigurableSection:BOD, Body Section 1]
{A}-[#SectionBody]
{B}-[This is Body Section 1.]
{A}-[#EndOfReport]");
			var templateSections = new TemplateSectionCollection(new ExcelTemplateReadFromStmTemplateTable(template));

			AssertEquals("templateSections.Contains", false, templateSections.Contains("Body Section 2"));
			AssertEquals("templateSections.Contains", false, templateSections.Contains(string.Empty));
			AssertEquals("templateSections.Contains", false, templateSections.Contains(null));

			AssertEquals("templateSections.Contains", true, templateSections.Contains("Body Section 1"));
		}

		public void TestSectionsGetRightStartAndEnd()
		{
			ExcelTemplate template = new ExcelTemplateReadFromByteArray(null, null, SectionRepositoryTestHelper.GetSystemRepositoryTemplateBlob());
			SectionRepository systemRepository = new SectionRepository(template);
			ZStringBuilder result = new ZStringBuilder();
			foreach (TemplateSection systemSection in systemRepository.AllSections)
			{
				result.Append(systemSection.ToString());
			}
			AssertMultilineASCIIEquals("Sections Breakdown", @"
CON / Config Section - Starting: 1 Rows: 0 Ending: 1
AAA / AAA Section - Starting: 2 Rows: 2 Ending: 4
BBB / BBB Section - Starting: 5 Rows: 1 Ending: 6
END / End of Report - Starting: 7 Rows: 0 Ending: 7
".Trim(), result.ToStringWithNewLineBetweenAppends());
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestGetConfigAndEndOfReportSectionMethods()
		{
			ExcelTemplateForUnitTesting template = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", TestFilesSubFolder.DocumentTestFiles);
			TemplateSectionCollection collection = new TemplateSectionCollection(template);
			AssertEquals("Precondition: repository.AllSections.Count", 6, collection.Count);
			TemplateSection configSection = collection[0];
			TemplateSection endOfReportSection = collection[5];
			AssertEquals("Precondition: configSection.TypeCode", ConfigurableSectionTypeList.Codes.ConfigSection, configSection.TypeCode);
			AssertEquals("Precondition: endOfReportSection.TypeCode", ConfigurableSectionTypeList.Codes.EndOfReport, endOfReportSection.TypeCode);

			AssertEquals(configSection, collection.GetConfigSection());
			AssertEquals(endOfReportSection, collection.GetEndOfReportSection());

			collection.RemoveAll();
			AssertExceptionThrown(typeof(TemplateDefinitionException), delegate
			{ collection.GetConfigSection(); });
			AssertExceptionThrown(typeof(TemplateDefinitionException), delegate
			{ collection.GetEndOfReportSection(); });
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestFind()
		{
			ExcelTemplateForUnitTesting template = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", TestFilesSubFolder.DocumentTestFiles);
			TemplateSectionCollection collection = new TemplateSectionCollection(template);
			AssertEquals("Precondition: repository.AllSections.Count", 6, collection.Count);

			TemplateSection section1 = collection.Find("Brett's Farts STILL Stink");
			AssertEquals(ConfigurableSectionTypeList.Codes.BodySection, section1.TypeCode);
			AssertEquals("Brett's Farts STILL Stink", section1.SectionName);

			AssertNull(collection.Find(string.Empty));
			AssertNull(collection.Find(null));

			TemplateSection section2 = collection.Find(ConfigurableSectionTypeList.Descriptions.PageHeader);
			AssertEquals(ConfigurableSectionTypeList.Codes.PageHeader, section2.TypeCode);
			AssertEquals(ConfigurableSectionTypeList.Descriptions.PageHeader, section2.SectionName);

			section2 = collection.Find(ConfigurableSectionTypeList.Descriptions.PageHeader.GetUnresolvedString().ToUpperInvariant());
			AssertEquals(ConfigurableSectionTypeList.Codes.PageHeader, section2.TypeCode);
			AssertEquals(ConfigurableSectionTypeList.Descriptions.PageHeader, section2.SectionName);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMove()
		{
			ExcelTemplateForUnitTesting template = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", TestFilesSubFolder.DocumentTestFiles);
			TemplateSectionCollection collection = new TemplateSectionCollection(template);
			AssertEquals("Precondition: repository.AllSections.Count", 6, collection.Count);

			AssertEquals("Precondition: collection[0].StartingRow", 1, collection[0].StartingRowNumber);
			AssertEquals("Precondition: collection[1].StartingRow", 8, collection[1].StartingRowNumber);
			AssertEquals("Precondition: collection[2].StartingRow", 11, collection[2].StartingRowNumber);
			AssertEquals("Precondition: collection[3].StartingRow", 14, collection[3].StartingRowNumber);
			AssertEquals("Precondition: collection[4].StartingRow", 21, collection[4].StartingRowNumber);
			AssertEquals("Precondition: collection[5].StartingRow", 24, collection[5].StartingRowNumber);

			collection.Move(collection[3], 7);

			AssertEquals("collection[0].StartingRow", 1, collection[0].StartingRowNumber);
			AssertEquals("collection[1].StartingRow", 14, collection[1].StartingRowNumber);
			AssertEquals("collection[2].StartingRow", 17, collection[2].StartingRowNumber);
			AssertEquals("collection[3].StartingRow", 7, collection[3].StartingRowNumber);
			AssertEquals("collection[4].StartingRow", 21, collection[4].StartingRowNumber);
			AssertEquals("collection[5].StartingRow", 24, collection[5].StartingRowNumber);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestInstantiationWithCustomisableTemplateSpreadSheetLoadsSections()
		{
			ExcelTemplateForUnitTesting template = new ExcelTemplateForUnitTesting("CustomisableSectionTest.xls", TestFilesSubFolder.DocumentTestFiles);
			TemplateSectionCollection collection = new TemplateSectionCollection(template);

			ZStringBuilder result = new ZStringBuilder();
			foreach (TemplateSection systemSection in collection)
			{
				result.Append(systemSection.ToString());
			}
			string fullResult = result.ToStringWithNewLineBetweenAppends();
			AssertMultilineASCIIEquals("Sections Breakdown", @"
CON / Config Section - Starting: 1 Rows: 6 Ending: 7
DHD / My Head Hurts - Starting: 8 Rows: 2 Ending: 10
PHD / Page Header - Starting: 11 Rows: 2 Ending: 13
BOD / Brett's Farts STILL Stink - Starting: 14 Rows: 6 Ending: 20
PFT / Page Footer - Starting: 21 Rows: 2 Ending: 23
END / End of Report - Starting: 24 Rows: 0 Ending: 24
			".Trim(), fullResult);

			AssertEquals("collection.Count", 6, collection.Count);

			TemplateSection templateSection = collection[0];
			AssertEquals("collection[0].TypeCode", ConfigurableSectionTypeList.Codes.ConfigSection, templateSection.TypeCode);
			AssertEquals("collection[0].SectionName", ConfigurableSectionTypeList.Descriptions.ConfigSection, templateSection.SectionName);
			AssertEquals("collection[0].StartingRow", 1, templateSection.StartingRowNumber);
			AssertEquals("collection[0].RowCount", 6, templateSection.RowCount);

			templateSection = collection[1];
			AssertEquals("collection[1].TypeCode", ConfigurableSectionTypeList.Codes.DocumentHeader, templateSection.TypeCode);
			AssertEquals("collection[1].SectionName", "My Head Hurts", templateSection.SectionName);
			AssertEquals("collection[1].StartingRow", 8, templateSection.StartingRowNumber);
			AssertEquals("collection[1].RowCount", 2, templateSection.RowCount);

			templateSection = collection[2];
			AssertEquals("collection[2].TypeCode", ConfigurableSectionTypeList.Codes.PageHeader, templateSection.TypeCode);
			AssertEquals("collection[2].SectionName", ConfigurableSectionTypeList.Descriptions.PageHeader, templateSection.SectionName);
			AssertEquals("collection[2].StartingRow", 11, templateSection.StartingRowNumber);
			AssertEquals("collection[2].RowCount", 2, templateSection.RowCount);

			templateSection = collection[3];
			AssertEquals("collection[3].TypeCode", ConfigurableSectionTypeList.Codes.BodySection, templateSection.TypeCode);
			AssertEquals("collection[3].SectionName", "Brett's Farts STILL Stink", templateSection.SectionName);
			AssertEquals("collection[3].StartingRow", 14, templateSection.StartingRowNumber);
			AssertEquals("collection[3].RowCount", 6, templateSection.RowCount);

			templateSection = collection[4];
			AssertEquals("collection[4].TypeCode", ConfigurableSectionTypeList.Codes.PageFooter, templateSection.TypeCode);
			AssertEquals("collection[4].SectionName", ConfigurableSectionTypeList.Descriptions.PageFooter, templateSection.SectionName);
			AssertEquals("collection[4].StartingRow", 21, templateSection.StartingRowNumber);
			AssertEquals("collection[4].RowCount", 2, templateSection.RowCount);

			templateSection = collection[5];
			AssertEquals("collection[5].TypeCode", ConfigurableSectionTypeList.Codes.EndOfReport, templateSection.TypeCode);
			AssertEquals("collection[5].SectionName", ConfigurableSectionTypeList.Descriptions.EndOfReport, templateSection.SectionName);
			AssertEquals("collection[5].StartingRow", 24, templateSection.StartingRowNumber);
			AssertEquals("collection[5].RowCount", 0, templateSection.RowCount);
		}

		public void TestTemplateSectionCollectionLanguage()
		{
			var userTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Customized Document Elements [ZH-CN]",
				@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=Test User Configurable Template]
{A}-[#ConfigurableSection:BOD, Body Section A]
{A}-[#SectionBody]
{B}-[This is the user Body Section A.]
{A}-[#EndOfReport]");

			var excelTemplate = new ExcelTemplateReadFromStmTemplateTable(userTemplate);
			var userSectionCollection = new TemplateSectionCollection(excelTemplate);
			AssertEquals(3, userSectionCollection.Count);
			Assert(userSectionCollection.All(x => ((TemplateSection)x).Language == Core.SharedConstants.Languages.ChineseSimplified));
		}

		public override void TestAddNew()
		{
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ new TemplateSectionCollection(null).AddNew(); });
		}

		public override void TestTypedAddNew()
		{
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ new TemplateSectionCollection(null).AddNew(typeof(TemplateSection)); });
		}

		protected override TemplateSectionCollection GetCollectionToTest() => new TemplateSectionCollection(null);

		protected override BusinessObject GetNewElementToAddToTheCollection() => new TemplateSection(ConfigurableSectionTypeList.Codes.DocumentHeader + ",My Favourite Martian", 1, 2);

		StmTemplateBase testSystemTemplate;
		StmTemplateBase testUserTemplate;

		void PrepareTestTemplates()
		{
			testSystemTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "System Document Elements",
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=Test System Template]
{A}-[#ConfigurableSection:BOD, Body Section 1]
{A}-[#SectionBody]
{B}-[This is the system Body Section 1.]
{A}-[#ConfigurableSection:BOD, Body Section 2]
{A}-[#SectionBody]
{B}-[This is the system Body Section 2.]
{A}-[#EndOfReport]");

			testUserTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Customized Document Elements",
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=Test User Configurable Template]
{A}-[#ConfigurableSection:BOD, Body Section 1]
{A}-[#ConfigurableSection:BOD, Body Section A]
{A}-[#SectionBody]
{B}-[This is the user Body Section A.]
{A}-[#EndOfReport]");
		}
	}
}
