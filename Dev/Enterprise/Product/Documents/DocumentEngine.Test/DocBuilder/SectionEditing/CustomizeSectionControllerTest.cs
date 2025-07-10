using System.Text;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.FlexCelInterface;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;

namespace Enterprise.DocumentEngine.DocBuilder.SectionEditing.Testing
{
	sealed class CustomizeSectionControllerTest : TestCaseWithFactory
	{
		public void TestCopyAndOverrideSection()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN:Category 1, My Section]
{B}-[My Section 1]
{A}-[#EndOfReport]");

			var customizedTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Customized Document Elements [UR-PK]",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Customized Document Elements [UR-PK]]
{A}-[#ConfigurableSection:GEN:Category 1, My Section]
{B}-[I should be overriden]
{A}-[#EndOfReport]");

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.Urdu);
			var view = new DummyCustomizeSectionView();
			view.SetShouldOverrideExistingSection(true);
			var controller = new CustomizeSectionController(manager, view);

			controller.CopySection(template.TemplateSections.Find("My Section"), manager.Language);

			AssertMultilineASCIIEquals("view.Log.ToString()",
@"Section [My Section] in Template [Customized Document Elements [UR-PK]] is being overriden.
Template [Customized Document Elements [UR-PK]] edited at Row: 3 Column: 0",
				view.Log.ToString());

			using (var excelInterface = new ExcelInterface(customizedTemplate.SO_Template))
			{
				AssertMultilineASCIIEquals("Customized Template",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Customized Document Elements [UR-PK]]
{A}-[#ConfigurableSection:GEN:Category 1, My Section]
{B}-[My Section 1]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestCopySection()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN:Category 1, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN:Category 2, My Section 2]
{B}-[My Section 2]
{A}-[#EndOfReport]");

			var customizedTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized, Enterprise.Core.SharedConstants.Languages.Urdu);
			AssertNull("There should be no customized template for Urdu.", customizedTemplate);

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.Urdu);
			var view = new DummyCustomizeSectionView();
			var controller = new CustomizeSectionController(manager, view);

			controller.CopySection(template.TemplateSections.Find("My Section 1"), manager.Language);
			AssertMultilineASCIIEquals("view.Log.ToString()",
@"Template [Customized Document Elements [UR-PK]] edited at Row: 5 Column: 0",
				view.Log.ToString());

			customizedTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized, Enterprise.Core.SharedConstants.Languages.Urdu);
			AssertNotNull("There should now be a customized template for Urdu.", customizedTemplate);

			using (var excelInterface = new ExcelInterface(customizedTemplate.SO_Template))
			{
				AssertMultilineASCIIEquals("Customized Template",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [UR-PK]]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#ConfigurableSection:GEN:Category 1, My Section 1]
{B}-[My Section 1]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}

			view.Log = new StringBuilder();
			view.SetShouldOverrideExistingSection(false);
			controller.CopySection(template.TemplateSections.Find("My Section 1"), manager.Language);
			AssertMultilineASCIIEquals("view.Log.ToString()",
@"Section [My Section 1] in Template [Customized Document Elements [UR-PK]] is being edited.
Template [Customized Document Elements [UR-PK]] edited at Row: 5 Column: 0",
				view.Log.ToString());

			using (var excelInterface = new ExcelInterface(customizedTemplate.SO_Template))
			{
				AssertMultilineASCIIEquals("Customized Template",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [UR-PK]]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#ConfigurableSection:GEN:Category 1, My Section 1]
{B}-[My Section 1]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}

			view.Log = new StringBuilder();
			view.SetShouldOverrideExistingSection(true);
			controller.CopySection(template.TemplateSections.Find("My Section 1"), manager.Language);
			AssertMultilineASCIIEquals("view.Log.ToString()",
@"Section [My Section 1] in Template [Customized Document Elements [UR-PK]] is being overriden.
Template [Customized Document Elements [UR-PK]] edited at Row: 5 Column: 0",
				view.Log.ToString());

			using (var excelInterface = new ExcelInterface(customizedTemplate.SO_Template))
			{
				AssertMultilineASCIIEquals("Customized Template",
@"{A}-[#Config]
{A}-[Name=Customized Document Elements [UR-PK]]
{A}-[HideColumnIf]   {BU}-[1==1]   {BV}-[1==1]   {BW}-[1==1]   {BX}-[1==1]   {BY}-[1==1]   {BZ}-[1==1]
{A}-[DataContext=GenericFreightJob]
{A}-[EmailSubject=<ReportName> - <JobNumber>]
{A}-[#ConfigurableSection:GEN:Category 1, My Section 1]
{B}-[My Section 1]
{A}-[#EndOfReport]",
					excelInterface.WorkSheets[0].ToString());
			}
		}

		public void TestCopySectionCustomizeLanguageBuilderTemplateWithSelectedLanguage_WI00024334()
		{
			var customizedTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized, Enterprise.Core.SharedConstants.Languages.Urdu);
			AssertNull("There should be no customized template for Urdu.", customizedTemplate);

			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateFromString(Factory,
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Test]
{A}-[#ConfigurableSection:GEN:Category 1, My Section 1]
{B}-[My Section 1]
{A}-[#ConfigurableSection:GEN:Category 2, My Section 2]
{B}-[My Section 2]
{A}-[#ConfigurableSection:GEN:Category 3, My Section 3]
{B}-[My Section 3]
{A}-[#EndOfReport]");

			var customURDTemplate = DocumentEngineTestHelper.CreateTemplateFromString(Factory, "Customized Document Elements [UR-PK]",
@"{A}-[#Config]
{A}-[DataContext=.DummyBODocSupportable]
{A}-[Name=Customized Document Elements [UR-PK]]
{A}-[#ConfigurableSection:GEN:Category Urdu 1, Urdu Section]
{B}-[This should be customized section]
{A}-[#EndOfReport]");

			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			AssertEquals("There should now be three customized templates plus two configurations for system English template.", 5, systemTemplate.TemplateSections.Count);

			customizedTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.Customized, Enterprise.Core.SharedConstants.Languages.Urdu);
			AssertNotNull("There should be a customized template for Urdu.", customizedTemplate);
			AssertEquals("There should now be a customized template plus two configurations for Urdu.", 3, customizedTemplate.TemplateSections.Count);

			var manager = new CustomizeSectionManager(Factory, Enterprise.Core.SharedConstants.Languages.Urdu);
			var view = new DummyCustomizeSectionView();
			view.SetShouldOverrideExistingSection(true);
			var controller = new CustomizeSectionController(manager, view);

			AssertNoExceptionThrown("Expect not throw exception using CopySectop when only found customized Template", () => controller.CopySection(customURDTemplate.TemplateSections.Find("Urdu Section"), manager.Language));
		}

		#region Implementation

		class DummyCustomizeSectionView : ICustomizeSectionView
		{
			public StringBuilder Log = new StringBuilder();
			bool shouldOverrideExistingSection;

			internal void SetShouldOverrideExistingSection(bool value)
			{
				shouldOverrideExistingSection = value;
			}

			public ITemplateEditor GetTemplateEditor(StmTemplateBase template)
			{
				return new DummyTemplateEditor(template, Log);
			}

			public bool ShouldOverrideExistingSection(ZString sectionName, ZString templateName)
			{
				if (shouldOverrideExistingSection)
				{
					Log.AppendLine(string.Format("Section [{0}] in Template [{1}] is being overriden.", sectionName, templateName));
				}
				else
				{
					Log.AppendLine(string.Format("Section [{0}] in Template [{1}] is being edited.", sectionName, templateName));
				}

				return shouldOverrideExistingSection;
			}

			public void ShowEditOnlyMessage(ZString sectionName, ZString templateName)
			{
				Log.AppendLine(string.Format("Section [{0}] is being edited in [{1}].", sectionName, templateName));
			}
		}

		class DummyTemplateEditor : ITemplateEditor
		{
			internal DummyTemplateEditor(StmTemplateBase template, StringBuilder log)
			{
				this.template = template;
				this.log = log;
			}

			readonly StmTemplateBase template;
			readonly StringBuilder log;

			public void Edit()
			{
				log.AppendLine(string.Format("Template [{0}] edited with no row and no column", template.SO_Name));
			}

			public void Edit(int row, int column)
			{
				log.AppendLine(string.Format("Template [{0}] edited at Row: {1} Column: {2}", template.SO_Name, row, column));
			}
		}

		#endregion
	}
}
