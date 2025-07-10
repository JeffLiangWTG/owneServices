using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	[TestedType(typeof(StmMenuDocumentConfigForm))]
	sealed class StmMenuDocumentConfigFormTest : ZFormBasherTest
	{
		public void TestEndToEndWithGenericSectionsAndUserTemplate()
		{
			var systemTemplate = StmTemplateBase.GetDocBuilderTemplate(Factory, DocBuilderTemplateType.System);
			var userTemplate = Factory.New<StmTemplateBase>();
			userTemplate.SO_Name = SectionRepositoryTemplateNames.User;
			userTemplate.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(ConfigurableTemplateTestHelper.ConfigurableStripsForTesting);

			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(systemTemplate);
			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);

			var documentSupportable = Factory.NewWithValidTestData<OrgHeader>();

			using (var form = new StmMenuDocumentConfigForm(temporaryConfig, documentSupportable))
			{
				form.Show();

				AssertEquals("Pre-condition: form.BusinessEntity.ConfigItems.Count", 0, form.BusinessEntity.ConfigItems.Count);

				var position = -1;
				for (var index = 0; index < form.SectionsGrid.ListManager.List.Count; index++)
				{
					TemplateSection templateSection = (TemplateSection)form.SectionsGrid.ListManager.List[index];
					if (templateSection.SectionName == "Generic Section 1")
					{
						position = index;
						break;
					}
				}

				Assert("Generic Section 1 should be found.", position >= 0);

				form.SectionsGrid.ListManager.Position = position;
				form.AddButton.PerformClick();

				AssertEquals("form.BusinessEntity.ConfigItems.Count", 1, form.BusinessEntity.ConfigItems.Count);

				var configItemAdded = form.BusinessEntity.ConfigItems[0];
				CombineAssertions(() =>
				{
					AssertEquals("configItemAdded.S4_SectionType", GenericSectionUsageList.Codes.BodySection, configItemAdded.S4_SectionType);
					AssertEquals("configItemAdded.S4_SectionItemName", "Generic Section 1", configItemAdded.S4_SectionItemName);
				});
			}
		}

		public void TestEndToEndWithGenericSections()
		{
			var template = ConfigurableTemplateTestHelper.SetupSystemTemplateForTesting(Factory);
			var config = ConfigurableTemplateTestHelper.CreateDocumentConfig(template);
			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(config);

			AssertEquals("Pre-condition: temporaryConfig.ConfigItems.Count", 10, temporaryConfig.AvailableSections.Count);

			var documentSupportable = Factory.New<DummyBODocSupportable>();

			using (var form = new StmMenuDocumentConfigForm(temporaryConfig, documentSupportable))
			{
				form.Show();

				AssertEquals("Pre-condition: form.BusinessEntity.ConfigItems.Count", 0, form.BusinessEntity.ConfigItems.Count);
				AssertEquals("Pre-condition: form.SectionsGrid.ListManager.List.Count", 10, form.SectionsGrid.ListManager.List.Count);

				form.SectionsGrid.ListManager.Position = 2;
				form.AddButton.PerformClick();

				AssertEquals("form.BusinessEntity.ConfigItems.Count", 1, form.BusinessEntity.ConfigItems.Count);

				var configItemAdded = form.BusinessEntity.ConfigItems[0];
				CombineAssertions(() =>
				{
					AssertEquals("configItemAdded.S4_SectionType", GenericSectionUsageList.Codes.BodySection, configItemAdded.S4_SectionType);
					AssertEquals("configItemAdded.S4_SectionItemName", "Generic Section 1", configItemAdded.S4_SectionItemName);
				});
			}
		}

		public void TestAddAndRemoveConfigItem()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.BusinessEntity.AvailableSections.Add(new TemplateSection(ConfigurableSectionTypeList.Codes.PageFooter + ",AAA", 60, 1));
				((IBindingList)form.BusinessEntity.AvailableSections).RemoveSort();

				form.SectionsGrid.ListManager.Position = 1;
				form.AddButton.PerformClick();
				AssertEquals("BusinessEntity.ConfigItems.Count", 1, form.BusinessEntity.ConfigItems.Count);
				AssertEquals("BusinessEntity.ConfigItems[0].S4_SectionType", ConfigurableSectionTypeList.Codes.PageHeader, form.BusinessEntity.ConfigItems[0].S4_SectionType);
				AssertEquals("ConfigItemsGrid.ListManager.Position", 0, form.ConfigItemsGrid.ListManager.Position);
				AssertEquals("ConfigItemsGrid.IsSelected(0)", true, form.ConfigItemsGrid.IsSelected(0));

				form.SectionsGrid.UnSelectAll();
				form.SectionsGrid.Select(2);
				form.SectionsGrid.Select(3);
				form.AddButton.PerformClick();
				AssertEquals("BusinessEntity.ConfigItems.Count", 3, form.BusinessEntity.ConfigItems.Count);
				AssertEquals("BusinessEntity.ConfigItems[0].S4_SectionType", ConfigurableSectionTypeList.Codes.PageHeader, form.BusinessEntity.ConfigItems[0].S4_SectionType);
				AssertEquals("BusinessEntity.ConfigItems[1].S4_SectionType", ConfigurableSectionTypeList.Codes.BodySection, form.BusinessEntity.ConfigItems[1].S4_SectionType);
				AssertEquals("BusinessEntity.ConfigItems[2].S4_SectionType", ConfigurableSectionTypeList.Codes.PageFooter, form.BusinessEntity.ConfigItems[2].S4_SectionType);
				AssertEquals("ConfigItemsGrid.ListManager.Position", 1, form.ConfigItemsGrid.ListManager.Position);
				AssertEquals("ConfigItemsGrid.IsSelected(0)", false, form.ConfigItemsGrid.IsSelected(0));
				AssertEquals("ConfigItemsGrid.IsSelected(1)", true, form.ConfigItemsGrid.IsSelected(1));
				AssertEquals("ConfigItemsGrid.IsSelected(2)", true, form.ConfigItemsGrid.IsSelected(2));

				var configItem1 = form.BusinessEntity.ConfigItems[0];
				var configItem2 = form.BusinessEntity.ConfigItems[1];
				var configItem3 = form.BusinessEntity.ConfigItems[2];

				form.ConfigItemsGrid.UnSelectAll();
				form.ConfigItemsGrid.ListManager.Position = 1;
				form.RemoveButton.PerformClick();
				AssertEquals("BusinessEntity.ConfigItems.Count", 2, form.BusinessEntity.ConfigItems.Count);
				AssertEquals("BusinessEntity.ConfigItems[0].S4_SectionType", ConfigurableSectionTypeList.Codes.PageHeader, form.BusinessEntity.ConfigItems[0].S4_SectionType);
				AssertEquals("BusinessEntity.ConfigItems[1].S4_SectionType", ConfigurableSectionTypeList.Codes.PageFooter, form.BusinessEntity.ConfigItems[1].S4_SectionType);
				AssertEquals("BusinessEntity.ConfigItems[0].S4_PrintOrder", 1, form.BusinessEntity.ConfigItems[0].S4_PrintOrder);
				AssertEquals("BusinessEntity.ConfigItems[1].S4_PrintOrder", 2, form.BusinessEntity.ConfigItems[1].S4_PrintOrder);

				form.ConfigItemsGrid.SelectAllElements();
				form.RemoveButton.PerformClick();
				AssertEquals("BusinessEntity.ConfigItems.Count", 0, form.BusinessEntity.ConfigItems.Count);
				AssertEquals("configItem1.IsDeleted", true, configItem1.IsDeleted);
				AssertEquals("configItem2.IsDeleted", true, configItem2.IsDeleted);
				AssertEquals("configItem3.IsDeleted", true, configItem3.IsDeleted);

				form.BusinessEntity.AvailableSections.Sort(new SortInfo(TemplateSection.Schema.SectionName, ListSortDirection.Ascending));
				form.SectionsGrid.SelectAllElements();
				form.AddButton.PerformClick();
				form.ConfigItemsGrid.SelectAllElements();
				form.RemoveButton.PerformClick();
			}
		}

		[RequiresSTA]
		public void TestPreview()
		{
			DocumentSupportable.Z0_Code = "Oinky";
			var dummyTemplate = Factory.New<StmTemplateBase>();
			var dummyPivot = Factory.New<StmMenuTemplatePivotBase>();

			dummyPivot.SI_SU = Source.MenuTemplatePivot.MenuItem.PK;
			dummyPivot.SI_SO = dummyTemplate.PK;
			dummyTemplate.SO_DataContext = Source.MenuTemplatePivot.Template.SO_DataContext;
			dummyTemplate.SO_Template = DocumentEngineTestHelper.CreateTemplateFromString(
@"{A}-[#Config]
{A}-[Name=TestPreview]
{A}-[#SectionBody]
{B}-[This is a preview.]
{A}-[#EndOfReport]");

			Factory.Save();

			using (var form = GetFormToBash())
			{
				form.Show();
				//form.BusinessEntity.S3_GC = GlbCompany.CurrentCompany.PK;
				form.AddButton.PerformClick();
				form.PreviewButton.PerformClick();

				var previewForms = Application.OpenForms.OfType<XLSPreviewForm>().ToList();

				AssertEquals("There should only be one preview form.", 1, previewForms.Count);

				using (var previewForm = previewForms[0])
				{
					AssertEquals("previewForm.DeliverButton.Visible", false, previewForm.Controls.Find("DeliverButton", true)[0].Visible);
					AssertEquals("previewForm.OpenInExcelButton.Visible", false, previewForm.Controls.Find("OpenInExcelButton", true)[0].Visible);

					var flexCelPreview = (FlexCelPreviewWithCulture)previewForm.Controls.Find("PreviewMain", true)[0];
					var excelFile = flexCelPreview.Document.Workbook;
					AssertEquals("There should only be 1 row in the preview.", 1, excelFile.RowCount);
					AssertEquals("Added section should be in the preview.", "Oinky", excelFile.GetCellValue(1, 2));
				}
			}
		}

		public void TestPreviewWithInvalidTemplate()
		{
			using (var form = new StmMenuDocumentConfigForm(TemporaryStmMenuDocumentConfig.New(Source), Factory.New<OrgHeader>()))
			{
				form.Show();
				form.SectionsGrid.SelectAllElements();
				form.AddButton.PerformClick();
				form.PreviewButton.PerformClick();
				AssertNull("The preview form should not be shown.", ZFormModaliser.ActiveForm);
			}
		}

		public void TestDoNotAllowUserToPreviewIfThereAreErrors()
		{
			using (var form = GetFormToBash())
			{
				form.BusinessEntity.Source.Factory.Save();
				form.Show();

				form.PreviewButton.PerformClick();
				Assert("There should be some errors.", form.BusinessEntity.Notifications.GetErrors().Count() > 0);
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "There are errors - can't preview.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertNull("The preview form should not be shown.", ZFormModaliser.ActiveForm);
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.AddButton.PerformClick();
				form.PreviewButton.PerformClick();
				AssertEquals("form.BusinessEntity.Notifications.GetErrors().Count()", 0, form.BusinessEntity.Notifications.GetErrors().Count());
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);

				var previewForms = new List<XLSPreviewForm>();
				foreach (var openForm in Application.OpenForms)
				{
					if (openForm is XLSPreviewForm)
					{
						previewForms.Add(openForm as XLSPreviewForm);
					}
				}

				AssertEquals("One preview form should be shown.", 1, previewForms.Count);
				previewForms[0].Dispose();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestSave()
		{
			using (StmMenuDocumentConfigForm form = GetFormToBash())
			{
				form.Show();
				AssertEquals("Source.S3_GC", ZGuid.Empty, Source.S3_GC);
				form.OkButton.PerformClick();
				Assert("There should be no errors.", form.BusinessEntity.Notifications.GetErrors().Count() == 0);
				AssertNull("CommittedDocConfig", form.CommittedDocConfig);

				var configItem = form.BusinessEntity.ConfigItems.AddNew();
				configItem.S4_SectionType = ConfigurableSectionTypeList.Codes.BodySection;
				form.OkButton.PerformClick();
				AssertNoErrors((BusinessObject)form.LastDataSourceForTest);
				AssertEquals("CommittedDocConfig", Source, form.CommittedDocConfig);
			}
		}

		public void TestDoNotPrintInDocumentPack()
		{
			var temporaryConfig = TemporaryStmMenuDocumentConfig.New(Source);
			temporaryConfig.S3_IsSystem = true;
			using (var form = new StmMenuDocumentConfigForm(temporaryConfig, Factory.New<OrgHeader>()))
			{
				form.Show();
				Assert("do not print check box should be editable", !form.ExcludedFromDocPackCheckBox.ReadOnly);
			}

			temporaryConfig.S3_IsSystem = false;
			using (var form = new StmMenuDocumentConfigForm(temporaryConfig, Factory.New<OrgHeader>()))
			{
				form.Show();
				Assert("do not print check box should not be editable", form.ExcludedFromDocPackCheckBox.ReadOnly);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var docConfig = TemporaryStmMenuDocumentConfig.New(Source);
			docConfig.S3_Description = "Not Blank";
			docConfig.HasChanges = false;
			return new StmMenuDocumentConfigForm(docConfig, DocumentSupportable);
		}

		DummyBODocSupportable documentSupportable;
		DummyBODocSupportable DocumentSupportable => documentSupportable ?? (documentSupportable = Factory.NewWithValidTestData<DummyBODocSupportable>());

		StmMenuDocumentConfig source;
		StmMenuDocumentConfig Source => source ?? (source = Business.Testing.TemporaryStmMenuDocumentConfigTestHelper.GetNewSource(Factory));

		new StmMenuDocumentConfigForm GetFormToBash() => (StmMenuDocumentConfigForm)base.GetFormToBash();
	}
}
