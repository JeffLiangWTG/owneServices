using CargoWise.EntityFramework.Testing;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.DocBuilder.BulkTemplateUpdating.Testing
{
	sealed class ConfigurableTemplateManipulatorTest : TestCaseWithFactory
	{
		public void TestRemoveAllSectionAreaIdentifiers()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestRemoveAllTags]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:PFT, Continued Over...]
{A}-[#PageFooter]
{A}-[#if <TotalPages> > 1]
{B}-[Continued Over...]
{A}-[#endif]
{A}-[#PageFooter]
{A}-[#LastPageFooter]
{A}-[#OnlyOnePageFooter]
{A}-[#EndOfReport]");

			Factory.Save();

			var manipulator = new ConfigurableTemplateManipulator(template);
			manipulator.RemoveAllSectionAreaIdentifiers("Continued Over...");

			AssertMultilineASCIIEquals("RemoveAllSectionAreaIdentifiers",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestRemoveAllTags]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:PFT, Continued Over...]
{A}-[#if <TotalPages> > 1]
{B}-[Continued Over...]
{A}-[#endif]
{A}-[#EndOfReport]", template.ToContents());
		}

		public void TestRenameSectionAndConfigItems()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestRenameSection]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 2");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 2");

			AssertMultilineASCIIEquals("Pre-condition: config.ConfigItems",
@"1: BDY, Generic Section 1 []
2: BDY, Generic Section 2 []
3: BDY, Generic Section 1 []
4: BDY, Generic Section 2 []", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));

			Factory.Save();

			var manipulator = new ConfigurableTemplateManipulator(template);
			manipulator.RenameSectionAndConfigItems("Generic Section 2", "Renamed Generic Section 2");

			AssertMultilineASCIIEquals("RenameSectionAndConfigItems",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestRenameSection]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:GEN, Renamed Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]", template.ToContents());

			AssertMultilineASCIIEquals("config.ConfigItems",
@"1: BDY, Generic Section 1 []
2: BDY, Renamed Generic Section 2 []
3: BDY, Generic Section 1 []
4: BDY, Renamed Generic Section 2 []", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));
		}

		public void TestChangeSectionType()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestChangeSectionType]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 2");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 2");

			AssertMultilineASCIIEquals("Pre-condition: config.ConfigItems",
@"1: BDY, Generic Section 1 []
2: BDY, Generic Section 2 []
3: BDY, Generic Section 1 []
4: BDY, Generic Section 2 []", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));

			Factory.Save();

			var manipulator = new ConfigurableTemplateManipulator(template);
			manipulator.ChangeSectionAndConfigType("Generic Section 2", ConfigurableSectionTypeList.Codes.BodySectionExpanding, GenericSectionUsageList.Codes.BodySectionExpanding);
			AssertMultilineASCIIEquals("ChangeSectionType",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestChangeSectionType]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:BEX, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]", template.ToContents());

			AssertMultilineASCIIEquals("config.ConfigItems",
@"1: BDY, Generic Section 1 []
2: BEX, Generic Section 2 []
3: BDY, Generic Section 1 []
4: BEX, Generic Section 2 []", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));
		}

		public void TestRemoveSection()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestRemoveSection]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#ConfigurableSection:GEN, Generic Section 2]
{B}-[This is Generic Section 2.]
{A}-[#EndOfReport]");
			template.SO_DataContext = ".DummyBODocSupportable";

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 2");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 1");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section 2");

			AssertMultilineASCIIEquals("Pre-condition: config.ConfigItems",
@"1: BDY, Generic Section 1 []
2: BDY, Generic Section 2 []
3: BDY, Generic Section 1 []
4: BDY, Generic Section 2 []", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));

			Factory.Save();

			var manipulator = new ConfigurableTemplateManipulator(template);
			manipulator.RemoveSection("Generic Section 2");

			AssertMultilineASCIIEquals("RemoveSection",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestRemoveSection]
{A}-[#ConfigurableSection:GEN, Generic Section 1]
{B}-[This is Generic Section 1.]
{A}-[#EndOfReport]", template.ToContents());

			AssertMultilineASCIIEquals("config.ConfigItems",
@"1: BDY, Generic Section 1 []
2: BDY, Generic Section 1 []", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));
		}

		public void TestSplitSectionAndConfigItems()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=TestSplitSection]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:BOD, Section Body To Split]
{A}-[#SectionBody]
{A}-[#if ""<Declaration.Country.Code>"" == ""AU""]
{B}-[This declaration is from Australia.]
{A}-[#else]
{B}-[This declaration is NOT from Australia.]
{A}-[#endif]

{A}-[#ConfigurableSection:BOD, Section Body 2]
{A}-[#SectionBody]
{B}-[This is Section Body 2.]

{A}-[#EndOfReport]");

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Section Body 1");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Section Body To Split");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Section Body 2");
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Section Body To Split");

			AssertMultilineASCIIEquals("Pre-condition: config.ConfigItems",
@"1: BOD, Section Body 1 []
2: BOD, Section Body To Split []
3: BOD, Section Body 2 []
4: BOD, Section Body To Split []", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));

			Factory.Save();

			var manipulator = new ConfigurableTemplateManipulator(template);
			manipulator.SplitSectionAndConfigItems("Section Body To Split", 3, "Split Section Body 1", "Split Section Body 2");

			AssertMultilineASCIIEquals("SplitSectionAndConfigItems",
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=TestSplitSection]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:BOD, Split Section Body 1]
{A}-[#SectionBody]
{A}-[#if ""<Declaration.Country.Code>"" == ""AU""]
{B}-[This declaration is from Australia.]
{A}-[#ConfigurableSection:BOD, Split Section Body 2]
{A}-[#else]
{B}-[This declaration is NOT from Australia.]
{A}-[#endif]

{A}-[#ConfigurableSection:BOD, Section Body 2]
{A}-[#SectionBody]
{B}-[This is Section Body 2.]

{A}-[#EndOfReport]", template.ToContents());

			AssertMultilineASCIIEquals("Pre-condition: config.ConfigItems",
@"1: BOD, Section Body 1 []
2: BOD, Split Section Body 1 []
3: BOD, Split Section Body 2 []
4: BOD, Section Body 2 []
5: BOD, Split Section Body 1 []
6: BOD, Split Section Body 2 []", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));
		}

		public void TestRemoveConditionalStatementsAndPutInFilter()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=TestRemoveConditionalStatementsAndPutInFilter]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:GEN, Generic Section To Strip]
{A}-[#if ""<CartageInfo.PrintTwoJourneys>""==""Y""]
{B}-[This is the Generic Section To Strip.]

{A}-[#endif]
{A}-[#ConfigurableSection:BOD, Section Body 2]
{A}-[#SectionBody]
{B}-[This is Section Body 2.]

{A}-[#EndOfReport]");

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			ConfigurableTemplateTestHelper.AddFromTemplateSection(config, "Generic Section To Strip");

			AssertMultilineASCIIEquals("Pre-condition: config.ConfigItems",
@"1: BDY, Generic Section To Strip []", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));

			Factory.Save();

			var manipulator = new ConfigurableTemplateManipulator(template);
			manipulator.RemoveConditionalStatementsAndPutInFilter("Generic Section To Strip", @"""<CartageInfo.PrintTwoJourneys>""==""Y""");

			AssertMultilineASCIIEquals("RemoveConditionalStatementsAndPutInFilter",
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=TestRemoveConditionalStatementsAndPutInFilter]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:GEN, Generic Section To Strip]
{B}-[This is the Generic Section To Strip.]

{A}-[#ConfigurableSection:BOD, Section Body 2]
{A}-[#SectionBody]
{B}-[This is Section Body 2.]

{A}-[#EndOfReport]", template.ToContents());

			AssertMultilineASCIIEquals("Pre-condition: config.ConfigItems",
@"1: BDY, Generic Section To Strip [""<CartageInfo.PrintTwoJourneys>""==""Y""]", ConfigurableTemplateTestHelper.GetConfigItemsString(config.ConfigItems));
		}

		public void TestStripAreaTagIfOnlyTagAndIsFirstRow()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=TestStripAreaTagIfOnlyTagAndIsFirstRow]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:BOD, Section Body To Strip]
{A}-[#SectionBody]
{B}-[This is the Section Body To Strip.]

{A}-[#ConfigurableSection:BOD, Section Body That Can't Be Striped]
{A}-[#SectionBody]
{B}-[This is Section Body 2.]
{A}-[#SectionPageFooter]
{B}-[This is the Page Footer for Section Body 2.]

{A}-[#EndOfReport]");

			var manipulator = new ConfigurableTemplateManipulator(template);
			manipulator.StripAreaTagIfOnlyTagAndIsFirstRow("Section Body 1", "#DocumentHeader");
			manipulator.StripAreaTagIfOnlyTagAndIsFirstRow("Section Body To Strip", "#SectionBody");
			manipulator.StripAreaTagIfOnlyTagAndIsFirstRow("Section Body That Can't Be Striped", "#SectionBody");
			AssertMultilineASCIIEquals("templateUpdater.SplitSection",
@"{A}-[#Config]
{A}-[DataContext=GenericFreightJob]
{A}-[Name=TestStripAreaTagIfOnlyTagAndIsFirstRow]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:BOD, Section Body To Strip]
{B}-[This is the Section Body To Strip.]

{A}-[#ConfigurableSection:BOD, Section Body That Can't Be Striped]
{A}-[#SectionBody]
{B}-[This is Section Body 2.]
{A}-[#SectionPageFooter]
{B}-[This is the Page Footer for Section Body 2.]

{A}-[#EndOfReport]", template.ToContents());
		}

		public void TestRemoveAllTags()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestRemoveAllTags]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:PFT, Continued Over...]
{A}-[#PageFooter]
{B}-[Continued Over...]
{A}-[#LastPageFooter]
{A}-[#OnlyOnePageFooter]
{A}-[#EndOfReport]");

			Factory.Save();

			var manipulator = new ConfigurableTemplateManipulator(template);
			manipulator.RemoveAllTags("Continued Over...");

			AssertMultilineASCIIEquals("ttemplateUpdater.RemoveConditionalStatementsAndPutInFilter",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=TestRemoveAllTags]

{A}-[#ConfigurableSection:BOD, Section Body 1]
{A}-[#SectionBody]
{B}-[This is Section Body 1.]

{A}-[#ConfigurableSection:PFT, Continued Over...]
{B}-[Continued Over...]
{A}-[#EndOfReport]", template.ToContents());
		}
	}
}
