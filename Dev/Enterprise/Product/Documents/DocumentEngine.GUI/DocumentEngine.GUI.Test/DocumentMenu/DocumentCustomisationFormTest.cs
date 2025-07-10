using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.Definitions;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Core.Forms;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.DocumentEngine.GUI.ReflectiveFieldMap;
using Enterprise.DocumentEngine.Testing;
using Enterprise.DocumentEngine.Testing.UtilityClasses;
using Enterprise.DocumentEngineCore;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineIntegration;
using Enterprise.Environment;
using Enterprise.Integration.Licensing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	[TestedType(typeof(DocumentCustomisationForm))]
	sealed class DocumentCustomisationFormTest : MenuCustomisationFormAbstractTest
	{
		public void TestMenuDeliveryRestrictionsTabPage()
		{
			AssertMenuDeliveryRestrictionsTabPage(Factory.New<CustomDummyBODocSupportable>(), true);
			AssertMenuDeliveryRestrictionsTabPage(Factory.New<DummyDeliveryRestrictionBODocSupportable>(), true);
		}

		public void TestDeliveryRestrictionDropEditLastSelectedItemChanged()
		{
			using (var form = new DocumentCustomisationForm(DocumentMenuCustomisation.New(Factory.New<DummyDeliveryRestrictionBODocSupportable>(), null)))
			{
				form.PivotAndChildMenuTabControl.SelectedIndex = 0;

				var customisation = form.BusinessEntity;
				var menuItem = customisation.Menus.AddNew();
				menuItem.SU_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
				var restriction = menuItem.DeliveryRestrictions.AddNew();
				restriction.SDR_RN_NKOriginCountryCode = "AU";
				restriction.SDR_DeliveryRestrictionType = nameof(DeliveryRestrictionType.NON);
				restriction.SDR_IsActive = true;

				form.Show();
				UnitTestUserNotification.Instance.AddOKAnswer();

				form.DeliveryRestrictionDropEdit.Focus();
				SendKey(form.DeliveryRestrictionDropEdit, Keys.C);
				AssertEquals("DeliveryRestrictionDropEdit Text", "CNH", form.DeliveryRestrictionDropEdit.CodeBox.Text);

				form.MenusGrid.Focus();
				Application.DoEvents();
				AssertEquals("menuItem DeliveryRestrictionType should be CNH", "CNH", menuItem.SU_DeliveryRestrictionType);
				AssertEquals("DeliveryRestrictions should read only", true, menuItem.DeliveryRestrictions.ReadOnly);
				AssertEquals("DeliveryRestrictions can not be activated", false, restriction.SDR_IsActive);
				var expectedMessage = @"The Delivery Restrictions tab is not available when the CNH - Movement Restricted/Credit on Hold restriction has been set on the Details tab.

Are you sure you want to continue?";
				AssertEquals(expectedMessage, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Confirm Delivery Restriction Type Change", UnitTestUserNotification.Instance.LastMessage.Caption);

				form.DeliveryRestrictionDropEdit.Focus();
				SendKey(form.DeliveryRestrictionDropEdit, Keys.N);
				AssertEquals("DeliveryRestrictionDropEdit Text", "NON", form.DeliveryRestrictionDropEdit.CodeBox.Text);

				form.MenusGrid.Focus();
				Application.DoEvents();
				AssertEquals("menuItem DeliveryRestrictionType should be NON", "NON", menuItem.SU_DeliveryRestrictionType);
				AssertEquals("DeliveryRestrictions should not read only", false, menuItem.DeliveryRestrictions.ReadOnly);

				UnitTestUserNotification.Instance.AddAnswer(ZDialogResult.Cancel);

				restriction.SDR_IsActive = true;
				form.DeliveryRestrictionDropEdit.Focus();
				SendKey(form.DeliveryRestrictionDropEdit, Keys.C);
				AssertEquals("DeliveryRestrictionDropEdit Text", "CNH", form.DeliveryRestrictionDropEdit.CodeBox.Text);

				form.MenusGrid.Focus();
				Application.DoEvents();
				AssertEquals("menuItem DeliveryRestrictionType should be NON", "NON", menuItem.SU_DeliveryRestrictionType);
				AssertEquals("DeliveryRestrictions should not read only", false, menuItem.DeliveryRestrictions.ReadOnly);
				AssertEquals("DeliveryRestrictions should be still activated", true, restriction.SDR_IsActive);
			}
		}

		public void TestConfigItemsGridIsReadOnly()
		{
			using (var form = GetFormToBash())
			{
				AssertEquals(
					"Don't allow users to edit config items from here, they must click on the edit button.",
					true,
					form.configItemsGrid.ReadOnly);
			}
		}

		public void TestMapTreeMenuGetsAdded()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.Show();
				AssertNotNull("form.Menu.MenuItems.FindByText(MapTreeMenuManager.MenuKey)", form.Menu.MenuItems.FindByText("Common Data Source Maps"));
			}
		}

		public void TestMacroAndFilterMenuItems()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.Show();
				AssertNotNull("Evaluate Filter", form.MenusGrid.ContextMenu.MenuItems.FindByText("Evaluate Filter"));
				AssertNotNull("Evaluate Macro", form.MenusGrid.ContextMenu.MenuItems.FindByText("Evaluate Macro"));
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadNonExcelTemplateDoesNotThrowExceptionAndNotifiesUser()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				string templateFilePath = form.testTemplateFilePath = UnitTestingConstants.TestFilesDir + "TIFPage.tif";

				StmTemplateBase template = form.CurrentTemplate;
				AssertNotNull("CurrentTemplate", template);

				template.SO_DataContext = nameof(Core.Constants.DataContext.None);
				template.SO_IsSystemDefined = false;
				template.SO_ExcelTemplatePath = templateFilePath;
				template.SO_Template = ZBlob.Empty;
				template.IsCheckedOutByMe = true;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.loadTemplateButton.PerformClick();

				AssertEquals(ExcelInterfaceExceptionBase.ErrorMessageFileFormatNotSupported, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("template.SO_Template.IsEmpty", true, template.SO_Template.IsEmpty);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadNewCustomisableSectionTemplateLink()
		{
			using (var form = new DocumentCustomisationForm(DocumentMenuCustomisation.New(Factory.New<DummyBODocSupportable>(), null)))
			using (var resourceRetriever = new EmbeddedResourceRetriever(typeof(PrintTaskTest).Assembly))
			{
				form.PivotAndChildMenuTabControl.SelectedIndex = 1;

				DocumentMenuCustomisation customisation = form.BusinessEntity;
				DocumentCommand menuItem = customisation.Menus.AddNew();
				int startingTemplateCount = customisation.AvailableTemplates.Count;
				string testFileName = resourceRetriever.SaveResourceToFile("Enterprise.DocumentEngine.Test.Testing.DocumentTestFiles.CustomisableSectionTest.xls");
				form.Show();
				form.testTemplateFilePath = testFileName;
				AssertNoExceptionThrown(form.newTemplateButton.PerformClick);
				AssertEquals(startingTemplateCount + 1, customisation.AvailableTemplates.Count);
				StmTemplateBase template = customisation.AvailableTemplates[startingTemplateCount];
				Assert("currentTemplate.SO_ExcelTemplatePath.Length > 10", template.SO_ExcelTemplatePath.Length > 10);
				Assert("testFileName.EndsWith(currentTemplate.SO_ExcelTemplatePath) - [" + template.SO_ExcelTemplatePath + "]", testFileName.EndsWith(template.SO_ExcelTemplatePath));

				AssertEquals("form.CurrentMenu", menuItem, form.CurrentMenu);
				AssertEquals("form.CurrentTemplate", template, form.CurrentTemplate);
				AssertEquals("form.CurrentTemplatePivot", null, form.CurrentTemplatePivot);

				AssertNoExceptionThrown(form.addTemplatePivotButton.PerformClick);
				AssertNotEquals("form.CurrentTemplatePivot", null, form.CurrentTemplatePivot);
			}
		}

		public void TestAddAndRemoveChildMenu()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuChildMenusTabPage;
				TestAddAndRemovePivot(
					form,
					form.availableChildMenusGrid, form.childMenusUsedGrid,
					form.addChildMenuPivotButton, form.removeChildMenuPivotButton,
					StmMenuMenuPivotSchema.SF_SU_Inward, StmMenuMenuPivotSchema.SF_SU_Outward);
			}
		}

		public void TestAddAndRemoveSystemChildMenu()
		{
			TestRemainingPivotAfterAddAndRemoveChildMenu(true, MenuEditingMode.AllowAll, true);
			TestRemainingPivotAfterAddAndRemoveChildMenu(true, MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, false);
			TestRemainingPivotAfterAddAndRemoveChildMenu(true, MenuEditingMode.AllowEditingOfSystemDefinedOnly, true);
			TestRemainingPivotAfterAddAndRemoveChildMenu(true, MenuEditingMode.AllowEditingOfClientSpecificOnly, false);

			TestRemainingPivotAfterAddAndRemoveChildMenu(false, MenuEditingMode.AllowAll, true);
			TestRemainingPivotAfterAddAndRemoveChildMenu(false, MenuEditingMode.NotAllowEditingOfSystemOrClientMenus, true);
			TestRemainingPivotAfterAddAndRemoveChildMenu(false, MenuEditingMode.AllowEditingOfSystemDefinedOnly, true);
			TestRemainingPivotAfterAddAndRemoveChildMenu(false, MenuEditingMode.AllowEditingOfClientSpecificOnly, true);
		}

		public void TestAddAndRemoveDocType()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuDocTypesTabPage;
				TestAddAndRemovePivot(
					form,
					form.availableDocTypesGrid, form.docTypesUsedGrid,
					form.addDocTypePivotButton, form.removeDocTypePivotButton,
					StmMenuEDocsSchema.SX_SU, StmMenuEDocsSchema.SX_RT_DocType);
			}
		}

		public void TestAvailableChildMenusGridTracking()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				TestGridTracking(
					form, form.menuChildMenusTabPage, form.addChildMenuPivotButton,
					form.availableChildMenusGrid, form.childMenusUsedGrid,
					StmMenuMenuPivotSchema.SF_SU_Outward);
			}
		}

		public void TestAvailableDocTypesGridTracking()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				TestGridTracking(
					form, form.menuDocTypesTabPage, form.addDocTypePivotButton,
					form.availableDocTypesGrid, form.docTypesUsedGrid,
					StmMenuEDocsSchema.SX_RT_DocType);
			}
		}

		public void TestAddAndRemoveChildMenusWithMultipleRowSelected()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuChildMenusTabPage;
				TestAddAndRemovePivotWithMultipleRowSelected(
					form,
					form.availableChildMenusGrid, form.childMenusUsedGrid,
					form.addChildMenuPivotButton, form.removeChildMenuPivotButton,
					StmMenuMenuPivotSchema.SF_SU_Inward, StmMenuMenuPivotSchema.SF_SU_Outward);
			}
		}

		public void TestAddAndRemoveDocTypesMultipleRowSelected()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuDocTypesTabPage;
				TestAddAndRemovePivotWithMultipleRowSelected(
					form,
					form.availableDocTypesGrid, form.docTypesUsedGrid,
					form.addDocTypePivotButton, form.removeDocTypePivotButton,
					StmMenuEDocsSchema.SX_SU, StmMenuEDocsSchema.SX_RT_DocType);
			}
		}

		public void TestCopyDocConfig()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.Show();

				form.PivotAndChildMenuTabControl.SelectedIndex = 1;

				StmMenuTemplatePivotBase templatePivot = form.BusinessEntity.Menus[1].Documents[0];
				templatePivot.Template.SO_Name = SectionRepositoryTemplateNames.User;
				templatePivot.Template.Factory.Save();

				templatePivot.DocConfigs.RemoveAll();
				StmMenuDocumentConfig docConfig = templatePivot.DocConfigs.AddNew();

				StmMenuDocumentConfigItem configItem = docConfig.ConfigItems.AddNew();
				configItem.S4_SectionItemName = "No!";

				form.MenusGrid.ListManager.Position = 1;
				form.copyDocConfigButton.PerformClick();

				Assert("Last form should be the Customizable Document Configuration form", ZFormModaliser.LastFormShownDialogForTest != null && ZFormModaliser.LastFormShownDialogForTest.GetType() == typeof(StmMenuDocumentConfigForm));

				var configForm = (StmMenuDocumentConfigForm)ZFormModaliser.LastFormShownDialogForTest;

				AssertEquals("Config form should have shown the existing config item.", templatePivot.DocConfigs[1].PK, ((BusinessObject)configForm.LastDataSourceForTest).PK);

				AssertEquals("docConfigsGrid.ListManager.Position", 1, form.docConfigsGrid.ListManager.Position);
				AssertEquals("docConfigsGrid.IsSelected(1)", true, form.docConfigsGrid.IsSelected(1));

				StmMenuDocumentConfig docConfigCopy = templatePivot.DocConfigs[1];
				AssertNotEquals("docConfigCopy.PK", docConfig.PK, docConfigCopy.PK);
				AssertEquals("docConfigCopy.ConfigItems[0].S4_SectionItemName", "No!", docConfigCopy.ConfigItems[0].S4_SectionItemName);
			}
		}

		public void TestDocConfigsGrid()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				AssertEquals("docConfigsGrid.ReadOnly", false, form.docConfigsGrid.ReadOnly);
				foreach (ZGridColumnInfo columnInfo in form.docConfigsGrid.ColumnStyles)
				{
					AssertEquals(columnInfo.ColumnName + ".IsReadOnly", true, columnInfo.IsReadOnly);
				}
			}
		}

		public void TestDocConfigsVisiblity()
		{
			var dummy = Factory.New<DummyBODocSupportable>();
			Factory.Save();

			var customisationFactory = new BusinessObjectFactory();

			var documentCommand1 = customisationFactory.New<DocumentCommand>();
			documentCommand1.SU_MenuName = "Non-DocBuilder Document";
			documentCommand1.SU_BusinessContext = nameof(BusinessContext.Test);
			documentCommand1.SU_MenuIndex = 1;

			var document1 = documentCommand1.Documents.AddNew();
			document1.SI_SU = documentCommand1.PK;
			document1.SI_SO = customisationFactory.New<StmTemplateBase>().PK;
			document1.Template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.UnitTest);

			var documentCommand2 = customisationFactory.New<DocumentCommand>();
			documentCommand2.SU_MenuName = "DocBuilder Document";
			documentCommand2.SU_BusinessContext = nameof(BusinessContext.Test);
			documentCommand2.SU_MenuIndex = 2;

			var document2 = documentCommand2.Documents.AddNew();
			document2.SI_SU = documentCommand2.PK;
			document2.SI_SO = customisationFactory.New<StmTemplateBase>().PK;
			document2.Template.SO_DataContext = nameof(Enterprise.Core.Constants.DataContext.UnitTest);
			document2.Template.SO_Name = SectionRepositoryTemplateNames.System + " [ZH-CN]";

			var customisation = DocumentMenuCustomisation.New(dummy, null, customisationFactory);
			using (var form = new DocumentCustomisationForm(customisation))
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedIndex = 1;

				form.MenusGrid.SelectSingleElement(documentCommand1);
				AssertEquals("docConfigsGroupBox.Visible", false, form.docConfigsGroupBox.Visible);
				AssertEquals("docConfigsSplitter.Visible", false, form.docConfigsSplitter.Visible);

				form.MenusGrid.SelectSingleElement(documentCommand2);
				AssertEquals("docConfigsGroupBox.Visible", true, form.docConfigsGroupBox.Visible);
				AssertEquals("docConfigsSplitter.Visible", true, form.docConfigsSplitter.Visible);

				form.MenusGrid.SelectSingleElement(documentCommand1);
				AssertEquals("docConfigsGroupBox.Visible", false, form.docConfigsGroupBox.Visible);
				AssertEquals("docConfigsSplitter.Visible", false, form.docConfigsSplitter.Visible);

				var availableTemplatesGrid = form.Controls.Find("availableTemplatesGrid", true)[0] as ZGrid;
				var addTemplatePivotButton = form.Controls.Find("addTemplatePivotButton", true)[0] as ZButton;
				var docBuilderTemplate = documentCommand2.Documents[0].Template;

				availableTemplatesGrid.SelectSingleElement(docBuilderTemplate);
				addTemplatePivotButton.PerformClick();

				AssertEquals("docConfigsGroupBox.Visible", true, form.docConfigsGroupBox.Visible);
				AssertEquals("docConfigsSplitter.Visible", true, form.docConfigsSplitter.Visible);
			}
		}

		public void TestDocConfigsGridSizeEqualsTemplatesUsedGridSizeOnFormResize()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.Show();

				form.PivotAndChildMenuTabControl.SelectedIndex = 1;

				StmTemplate template1 = form.BusinessEntity.Menus[0].Documents[0].Template;
				StmTemplate template2 = form.BusinessEntity.Menus[2].Documents[0].Template;

				template2.SO_Name = SectionRepositoryTemplateNames.User;
				template2.Factory.Save();
				form.MenusGrid.ListManager.Position = 2;
				AssertEquals("docConfigsGroupBox.Visible", true, form.docConfigsGroupBox.Visible);

				form.ResizeRedrawForTesting = true;
				form.Size = new System.Drawing.Size(2000, 1000);
				Assert("docConfigsGrid.Height should equal TemplatesUsedGrid.Height.", Math.Abs(form.TemplatesUsedGrid.Height - form.docConfigsGrid.Height) <= 1);
			}
		}

		public void TestEditAndNewDocConfig()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.Show();

				form.PivotAndChildMenuTabControl.SelectedIndex = 1;

				StmMenuTemplatePivotBase templatePivot = form.BusinessEntity.Menus[1].Documents[0];
				templatePivot.Template.SO_Name = SectionRepositoryTemplateNames.User;
				templatePivot.Template.Factory.Save();
				templatePivot.DocConfigs.RemoveAll();
				StmMenuDocumentConfig existingConfigItem = templatePivot.DocConfigs.AddNew();

				form.MenusGrid.ListManager.Position = 1;
				form.newDocConfigButton.PerformClick();
				StmMenuDocumentConfigForm configForm = (StmMenuDocumentConfigForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertNotEquals("Config form should have shown a new config item.", existingConfigItem.PK, ((BusinessObject)configForm.LastDataSourceForTest).PK);

				form.editDocConfigButton.PerformClick();
				configForm = (StmMenuDocumentConfigForm)ZFormModaliser.LastFormShownDialogForTest;
				AssertEquals("Config form should have shown the existing config item.", existingConfigItem.PK, ((BusinessObject)configForm.LastDataSourceForTest).PK);
			}
		}

		[ExpectNoExceptions]
		public void TestLicenceConsumptionOnSave()
		{
			var mockRepository = new MockRepository(MockBehavior.Default);
			var licenceConsumptionLogCreatorMock = mockRepository.Create<ILicenceConsumptionLogCreator>(MockBehavior.Strict);

			licenceConsumptionLogCreatorMock.Setup(m => m.CreateLog(Env.Licence.DocumentCustomisation));

			using (ObjectFactory.Substitute(licenceConsumptionLogCreatorMock.Object))
			using (var form = GetFormToBash())
			{
				form.FireSaveButton();
				form.FireSaveButton();
			}
			licenceConsumptionLogCreatorMock.Verify(m => m.CreateLog(Env.Licence.DocumentCustomisation), Times.Exactly(2));
		}

		public void TestDefaultSelectedTabAndDataSourceMaps()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				AssertEquals(form.menuTemplatesTabPage, form.PivotAndChildMenuTabControl.SelectedTab);
				AssertNotNull(form.MenusGrid.ContextMenu.MenuItems.FindByText("Document Data Source Maps"));
			}
		}

		[RequiresSTA]
		public void TestGenericDataContextsLoading()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				AssertNotNull(form.DocumentDataSourceMapsMenuItem.MenuItems.FindByText("No Data Source Map found"));

				for (var i = 0; i < form.MenusGrid.ListManager.Count; i++)
				{
					form.MenusGrid.ListManager.Position = i;
					form.DocumentDataSourceMapsMenuItem.PerformSelect();

					var wrappers = new HashSet<(ZString dataContextIdentifier, DocumentWrapper wrapper)>();
					var dataContexts = new HashSet<ZString>();
					form.CombineAllSelectedDataContextsTo(dataContexts);
					dataContexts.ForEach(d =>
					{
						form.BusinessEntity.Parent.DocumentSupporter
							.GetDocumentWrappers(new DataContextValue(d).DataContext, form.CurrentMenu)
							.ForEach(
								w =>
								{
									wrappers.Add((d + " | " + w.GetType().Name, w));
								});
					});

					if (wrappers.Any())
					{
						AssertEquals(wrappers.Count, form.DocumentDataSourceMapsMenuItem.MenuItems.Count);

						AssertContainsExactElementsInAnyOrder(wrappers.Select(x => x.dataContextIdentifier), form.DocumentDataSourceMapsMenuItem.MenuItems.Cast<MenuItem>().Select(x => x.Text));

						var openedFormsCount = Application.OpenForms.Count;

						foreach (MenuItem menuItem in form.DocumentDataSourceMapsMenuItem.MenuItems)
						{
							menuItem.PerformClick();
							AssertEquals(openedFormsCount + 1, Application.OpenForms.Count);
							AssertEquals(typeof(MapTreeForm), Application.OpenForms[openedFormsCount].GetType());

							Application.OpenForms[openedFormsCount].Close();
						}
					}
					else
					{
						AssertEquals(1, form.DocumentDataSourceMapsMenuItem.MenuItems.Count);
						AssertEquals("No Data Source Map found", form.DocumentDataSourceMapsMenuItem.MenuItems[0].Text);
					}
				}
			}
		}

		public void TestRenameDuplicateIdentifier()
		{
			var list = new List<(ZString dataContextIdentifier, ZString dataContextOnly, DocumentWrapper wrapper)> {
				("IdentifierA | DocumentWrapperForTesting","IdentifierA", new DocumentWrapperForTesting("TestValue1")),
				("IdentifierA | DocumentWrapperForTesting","IdentifierA", new DocumentWrapperForTesting("TestValue2")),
				("IdentifierA | DocumentWrapperForTesting","IdentifierA", new DocumentWrapperForTesting("TestValue3")),
				("IdentifierB | DocumentWrapperForTesting","IdentifierB", new DocumentWrapperForTesting("TestValue4")),
				("IdentifierB | DocumentWrapperForTesting","IdentifierB", new DocumentWrapperForTesting("TestValue5")),
				("IdentifierC | DocumentWrapperForTesting","IdentifierC", new DocumentWrapperForTesting("TestValue6"))
			};

			using (var form = GetFormToBash())
			{
				var renamedList = form.RenameDuplicateIdentifier(list);

				AssertEquals(6, renamedList.Count);
				AssertNotNull(renamedList.First(x => x.dataContextIdentifier == "IdentifierA | DocumentWrapperForTesting1"));
				AssertNotNull(renamedList.First(x => x.dataContextIdentifier == "IdentifierA | DocumentWrapperForTesting2"));
				AssertNotNull(renamedList.First(x => x.dataContextIdentifier == "IdentifierA | DocumentWrapperForTesting3"));
				AssertNotNull(renamedList.First(x => x.dataContextIdentifier == "IdentifierB | DocumentWrapperForTesting1"));
				AssertNotNull(renamedList.First(x => x.dataContextIdentifier == "IdentifierB | DocumentWrapperForTesting2"));
				AssertNotNull(renamedList.First(x => x.dataContextIdentifier == "IdentifierC | DocumentWrapperForTesting"));
			}
		}

		public void TestCustomDataContextsLoading()
		{
			var dummy = Factory.New<CustomDummyBODocSupportable>();
			dummy.SupportedDataContexts = new[] { Core.Constants.DataContext.GenericFreightJob };
			dummy.BusinessContext = BusinessContext.Shipment;

			using (var form = new DocumentCustomisationForm(DocumentMenuCustomisation.New(dummy, null)))
			{
				form.Show();

				var menus = (form.MenusGrid.DataSource as DocumentMenuCustomisation).Menus.Cast<DocumentCommand>();
				var firstMenuContainsDocConfigsIndex = menus.IndexOf(menu => menu.Documents.Cast<StmMenuTemplatePivotBase>().Any(d => d.DocConfigs.Any()));
				form.MenusGrid.ListManager.Position = firstMenuContainsDocConfigsIndex;
				form.CurrentMenu.Documents.OfType<StmMenuTemplatePivotBase>().First().DocConfigs.OfType<StmMenuDocumentConfig>()
					.First().S3_OverrideDataContext = nameof(Core.Constants.DataContext.GenericBasicLabelAll);
				form.DocumentDataSourceMapsMenuItem.PerformSelect();

				AssertEquals(6, form.DocumentDataSourceMapsMenuItem.MenuItems.Count);
				AssertNotNull(form.DocumentDataSourceMapsMenuItem.MenuItems.FindByText("GenericFreightJob | DummyDocumentWrapper1"));
				AssertNotNull(form.DocumentDataSourceMapsMenuItem.MenuItems.FindByText("GenericFreightJob | DummyDocumentWrapper2"));
				AssertNotNull(form.DocumentDataSourceMapsMenuItem.MenuItems.FindByText("GenericFreightJob | DocumentWrapperForTesting"));
				AssertNotNull(form.DocumentDataSourceMapsMenuItem.MenuItems.FindByText("GenericBasicLabelAll | DummyDocumentWrapper1"));
				AssertNotNull(form.DocumentDataSourceMapsMenuItem.MenuItems.FindByText("GenericBasicLabelAll | DummyDocumentWrapper2"));
				AssertNotNull(form.DocumentDataSourceMapsMenuItem.MenuItems.FindByText("GenericBasicLabelAll | DummyDocumentWrapper3"));
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionThrownWhenNoBusinessContext()
		{
			var dummy = Factory.New<CustomDummyBODocSupportable>();
			dummy.SupportedDataContexts = new[] { Core.Constants.DataContext.CompanyCampaign };

			using (var form = new DocumentCustomisationForm(DocumentMenuCustomisation.New(dummy, null)))
			{
				form.Show();
				form.DocumentDataSourceMapsMenuItem.PerformSelect();
			}
		}

		[ExpectNoExceptions]
		public void TestNoExceptionWhenGetDocumentWrappersReturnNull()
		{
			var dummy = Factory.New<CustomDummyBODocSupportable>();
			dummy.SupportedDataContexts = new[] { Core.Constants.DataContext.StatementSummary };
			dummy.BusinessContext = BusinessContext.Shipment;
			using (var form = new DocumentCustomisationForm(DocumentMenuCustomisation.New(dummy, null)))
			{
				form.Show();
				form.MenusGrid.ListManager.Position = 0;
				form.CurrentMenu.Documents.RemoveAll();
				form.DocumentDataSourceMapsMenuItem.PerformSelect();
				AssertEquals("No Data Source Map found", form.DocumentDataSourceMapsMenuItem.MenuItems[0].Text);
			}
		}

		public void TestMacroColumnStyleInFilters()
		{
			using (DocumentCustomisationForm form = GetFormToBash())
			{
				form.Show();

				var macroMenusGridFilterList = (ZMacrosFindBoxColumnStyleInfo)form.MenusGrid.GetColumnStyle("SU_FilterList");
				AssertEquals(typeof(ZMacrosFindBoxColumnStyleInfo), macroMenusGridFilterList.GetType());
				AssertEquals(true, macroMenusGridFilterList.IsUsedForExpressions);
				AssertEquals(true, macroMenusGridFilterList.ShouldEscapeAllSpecialCharacters);

				var macroAvailableMenusGridFilterList = (ZMacrosFindBoxColumnStyleInfo)form.availableChildMenusGrid.GetColumnStyle("SU_FilterList");
				AssertEquals(typeof(ZMacrosFindBoxColumnStyleInfo), macroAvailableMenusGridFilterList.GetType());
				AssertEquals(true, macroAvailableMenusGridFilterList.IsUsedForExpressions);
				AssertEquals(true, macroAvailableMenusGridFilterList.ShouldEscapeAllSpecialCharacters);

				var macroMenusUsedGridFilter = (ZMacrosFindBoxColumnStyleInfo)form.childMenusUsedGrid.GetColumnStyle("SF_Filter");
				AssertEquals(typeof(ZMacrosFindBoxColumnStyleInfo), macroMenusUsedGridFilter.GetType());
				AssertEquals(true, macroMenusUsedGridFilter.IsUsedForExpressions);
				AssertEquals(true, macroMenusUsedGridFilter.ShouldEscapeAllSpecialCharacters);

				var macroTemplatesUsedGridFilter = (ZMacrosFindBoxColumnStyleInfo)form.TemplatesUsedGrid.GetColumnStyle("SI_MenuTemplateFilter");
				AssertEquals(typeof(ZMacrosFindBoxColumnStyleInfo), macroTemplatesUsedGridFilter.GetType());
				AssertEquals(true, macroTemplatesUsedGridFilter.IsUsedForExpressions);
				AssertEquals(true, macroTemplatesUsedGridFilter.ShouldEscapeAllSpecialCharacters);
			}
		}

		protected override Form GetFormToBashCore()
		{
			var dummy = Factory.NewWithValidTestData<OrgHeader>();
			Factory.Save();

			var customisation = DocumentMenuCustomisation.New(dummy, null);
			return new DocumentCustomisationForm(customisation);
		}

		protected override string FilePathForLoadTemplate => UnitTestingConstants.TestDocumentsFilesDir + @"\DummyOrgTemplate.xls";

		protected override string FilePathForLoadTemplateXlsx => UnitTestingConstants.TestDocumentsFilesDir + @"\DummyOrgTemplate.xlsx";

		protected override string OriginalDataContext => nameof(Core.Constants.DataContext.Organisation);

		void AssertMenuDeliveryRestrictionsTabPage(DummyBODocSupportable dummy, bool tabVisible)
		{
			using (var form = new DocumentCustomisationForm(DocumentMenuCustomisation.New(dummy, null)))
			{
				AssertEquals(tabVisible, form.menuDeliveryRestrictionsTabPage.TabVisible);
			}
		}

		void SendKey(ZDropEdit dropEdit, Keys key)
		{
			KeySender.PostKeyDown(dropEdit.CodeBox, key);
			Application.DoEvents();
			UserIdleWorker.Flush();
		}

		void TestRemainingPivotAfterAddAndRemoveChildMenu(ZBool isSystemDefined, MenuEditingMode menuEditingMode, ZBool canDeleted)
		{
			using (var form = GetFormToBash())
			{
				UnitTestUserNotification.Instance.ClearMessages();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuChildMenusTabPage;
				form.Show();

				var childMenu = (StmMenuItemBase)form.availableChildMenusGrid.ListManager.Current;

				var result = form.CurrentMenu.ChildMenus.AddNew();
				result.SF_SU_Inward = form.CurrentMenu.PK;
				result.SF_OverriddenBusinessContext = childMenu.SU_BusinessContext;
				result.SF_SU_Outward = childMenu.PK;
				result.SF_IsSystemDefined = isSystemDefined;
				result.EditingMode = menuEditingMode;

				form.removeChildMenuPivotButton.PerformClick();
				AssertEquals(canDeleted ? 0 : 1, form.childMenusUsedGrid.ListManager.Count);

				if (!canDeleted)
				{
					AssertEquals($"You cannot remove the following system-defined document.\r\n{result.SF_Calc_ChildName}\r\n", UnitTestUserNotification.Instance.LastMessage.Text);
				}
			}
		}

		new DocumentCustomisationForm GetFormToBash() => (DocumentCustomisationForm)base.GetFormToBash();

		sealed class CustomDummyBODocSupportable : DummyBODocSupportable
		{
			public CustomDummyBODocSupportable(BusinessObjectFactory factory, DataRow dataRow) : base(factory, dataRow)
			{
			}

			public override DocumentSupporter DocumentSupporter => new CustomDummyBODocSupportableDocumentSupporter(this);

			public Core.Constants.DataContext[] SupportedDataContexts { get; set; }

			public BusinessContext BusinessContext { get; set; }
		}

		sealed class CustomDummyBODocSupportableDocumentSupporter : DummyBODocSupportableDocumentSupporter
		{
			public CustomDummyBODocSupportableDocumentSupporter(CustomDummyBODocSupportable docDummyBusinessObject) : base(docDummyBusinessObject)
			{
				SupportedDataContexts = docDummyBusinessObject.SupportedDataContexts;
				BusinessContext = docDummyBusinessObject.BusinessContext;
			}

			protected override Core.Constants.DataContext[] GetSupportedDataContexts()
			{
				return SupportedDataContexts;
			}

			Core.Constants.DataContext[] SupportedDataContexts { get; }

			public override BusinessContext BusinessContext { get; }

			protected override DocumentWrapper[] GetDocumentWrappersInternal(Core.Constants.DataContext dataContext, IStmMenuItem commandBeingRun)
			{
				var result = new List<DocumentWrapper>();
				switch (dataContext)
				{
					case Core.Constants.DataContext.GenericFreightJob:
						result.Add(new DummyDocumentWrapper(Parent, Parent.Factory));
						result.Add(new DummyDocumentWrapper(Parent, Parent.Factory));
						result.Add(new DocumentWrapperForTesting("Testing"));
						break;
					case Core.Constants.DataContext.GenericBasicLabelAll:
						result.Add(new DummyDocumentWrapper(Parent, Parent.Factory));
						result.Add(new DummyDocumentWrapper(Parent, Parent.Factory));
						result.Add(new DummyDocumentWrapper(Parent, Parent.Factory));
						break;
					case Core.Constants.DataContext.StatementSummary:
						return null;
				}
				return result.ToArray();
			}
		}

		sealed class DummyDeliveryRestrictionBODocSupportable : DummyBODocSupportable, IOriginDestinationForDocumentDeliveryRestriction
		{
			public DummyDeliveryRestrictionBODocSupportable(BusinessObjectFactory factory, DataRow dataRow) : base(factory, dataRow)
			{
			}

			public override DocumentSupporter DocumentSupporter => new DummyDeliveryRestrictionBODocSupportableDocumentSupporter(this);

			public string OriginCountryCode { get; }

			public string DestinationCountryCode { get; }
		}

		sealed class DummyDeliveryRestrictionBODocSupportableDocumentSupporter : DummyBODocSupportableDocumentSupporter
		{
			public DummyDeliveryRestrictionBODocSupportableDocumentSupporter(DummyDeliveryRestrictionBODocSupportable docDummyBusinessObject) : base(docDummyBusinessObject)
			{
			}
		}
	}
}
