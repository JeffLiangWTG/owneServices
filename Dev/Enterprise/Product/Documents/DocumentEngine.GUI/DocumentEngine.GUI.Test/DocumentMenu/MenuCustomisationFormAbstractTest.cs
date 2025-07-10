using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CargoWise.BrandManager;
using CargoWise.BuildTools;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.DocumentEngine.Business;
using Enterprise.DocumentEngine.DocBuilder;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.DocumentMenu.Testing
{
	abstract class MenuCustomisationFormAbstractTest : ZArchitecture.GUI.Testing.ZFormBasherTest
	{
		public void TestViewTemplateHistoryMenuItem()
		{
			using (var form = GetFormToBash())
			{
				var menuItem = form.availableTemplatesGrid.ContextMenu.MenuItems.FindByText("&View Template History...");

				Assert(menuItem.Visible);
			}
		}

		public void TestEditTemplate()
		{
			using (ControlDpiScalingHelper.OverrideDPI_ForTesting(120, 120))
			using (DocumentMenu.MenuCustomisationForm form = GetFormToBash())
			{
				var templateEditor = new DummyTemplateEditor();
				form.dummyTemplateEditorForTesting = templateEditor;
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				var template = form.CurrentTemplate;
				template.SO_Name = "Test Template";

				template.ReadOnly = false;
				template.IsCheckedOutByMe = false;
				AssertEquals("Precondition: template.CanModifyExcelTemplate", true, template.CanModifyExcelTemplate);
				form.editTemplateButton.PerformClick();
				AssertEquals("form.DummyTemplateEditorForTesting.log",
					"Template [Test Template] edited with no row and no column",
					string.Join(System.Environment.NewLine, form.dummyTemplateEditorForTesting.Log.ToArray()));
				AssertEquals("Templates should be edited with Windows scaling set to 100%. Please revert to scaling 100%, log off and log back in, then edit the template.", UnitTestUserNotification.Instance.PreviousMessages.First(n => n.WasInformation).Text);
				AssertEquals("No Warnings", false, UnitTestUserNotification.Instance.PreviousMessages.Any(n => n.WasWarning));
				form.dummyTemplateEditorForTesting.Log.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				template.ReadOnly = true;
				template.IsCheckedOutByMe = false;
				AssertEquals("Precondition: template.CanModifyExcelTemplate", false, template.CanModifyExcelTemplate);
				form.editTemplateButton.PerformClick();
				AssertEquals("form.DummyTemplateEditorForTesting.log",
					"Template [Test Template] edited with no row and no column",
					string.Join(System.Environment.NewLine, form.dummyTemplateEditorForTesting.Log.ToArray()));
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", $"This template is read-only, so it will be shown in Excel in read-only mode.\r\n\r\nAny changes you make will not be saved back to {BrandingFactory.Instance.ProductName}.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.dummyTemplateEditorForTesting.Log.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

				form.availableTemplatesGrid.BindTo = string.Empty;
				AssertNull("Precondition: form.CurrentTemplate is null", form.CurrentTemplate);
				form.editTemplateButton.PerformClick();
				AssertEquals("UnitTestUserNotification.Instance.LastMessage.Text", "You need to select a template record before using the Edit button.", UnitTestUserNotification.Instance.LastMessage.Text);
				form.dummyTemplateEditorForTesting.Log.Clear();
				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			}
		}

		public void TestRemovingPivotTellsYouIfYouDontHaveOneSelected()
		{
			using (DocumentMenu.MenuCustomisationForm form = GetFormToBash())
			{
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;
				form.Show();

				form.TemplatesUsedGrid.ListManager.RemoveAt(0);
				AssertEquals("Precondition: form.TemplatesUsedGrid.ListManager.Count", 0, form.TemplatesUsedGrid.ListManager.Count);

				AssertEquals("Precondition: LastMessage.Text", null, UnitTestUserNotification.Instance.LastMessage.Text);
				form.removeTemplatePivotButton.PerformClick();
				AssertEquals("LastMessage.Text", "Please select a template link to remove.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestAddAndRemoveTemplatePivot()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				TestAddAndRemovePivot(
					form,
					form.availableTemplatesGrid, form.TemplatesUsedGrid,
					form.addTemplatePivotButton, form.removeTemplatePivotButton,
					StmMenuTemplatePivotSchema.SI_SU, StmMenuTemplatePivotSchema.SI_SO);
			}
		}

		[RequiresSTA]
		public void TestAddAndRemoveTemplateWithMultipleRowSelected()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				TestAddAndRemovePivotWithMultipleRowSelected(
					form,
					form.availableTemplatesGrid, form.TemplatesUsedGrid,
					form.addTemplatePivotButton, form.removeTemplatePivotButton,
					StmMenuTemplatePivotSchema.SI_SU, StmMenuTemplatePivotSchema.SI_SO);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestAddNewTemplate()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				var templateFilePath = form.testTemplateFilePath = form.BusinessEntity.AvailableTemplates[0].ExcelTemplateFullPath;
				var menuItem = form.BusinessEntity.Menus.AddNew();

				var countBefore = form.BusinessEntity.AvailableTemplates.Count;
				form.newTemplateButton.PerformClick();
				var countAfter = form.BusinessEntity.AvailableTemplates.Count;
				AssertEquals("Available Templates Count", countBefore + 1, countAfter);

				var template = form.BusinessEntity.AvailableTemplates[countAfter - 1];
				AssertEquals("template.ExcelTemplateFullPath", templateFilePath, template.ExcelTemplateFullPath);
				AssertEquals("template.SO_DataContext", form.BusinessEntity.AvailableTemplates[0].SO_DataContext, template.SO_DataContext);
			}
		}

		public void TestAvailableTemplatesGridColour()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;
				form.testingGridColour = true;
				StmTemplateBase template = form.BusinessEntity.AvailableTemplates.AddNew();
				using (TempFile tempFile = TempFile.NewWithExtension("xls"))
				{
					template.SO_ExcelTemplatePath = tempFile.Filename;
					AssertEquals("Precondition: ExcelTemplateFullPath should be the same as the temp file's path.", tempFile.Filename, template.ExcelTemplateFullPath);

					File.SetAttributes(template.ExcelTemplateFullPath, FileAttributes.Normal);
					template.IsCheckedOutByMe = true;
					AssertEquals("Colour when checked out", Color.Crimson, form.GetColour(template));

					template.IsCheckedOutByMe = false;
					AssertEquals("Colour when not checked out", Color.Empty, form.GetColour(template));

					File.SetAttributes(template.ExcelTemplateFullPath, FileAttributes.ReadOnly);
					AssertEquals("Colour when not checked out", Color.Empty, form.GetColour(template));
				}
			}
		}

		public void TestAvailableTemplatesGridTracking()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				TestGridTracking(
					form, form.menuTemplatesTabPage, form.addTemplatePivotButton,
					form.availableTemplatesGrid, form.TemplatesUsedGrid,
					StmMenuTemplatePivotSchema.SI_SO);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestMarkTemplateEditable()
		{
			var mock = new Mock<ISourceControl>();
			SourceControl.SetEnterpriseInstanceForTesting(mock.Object);

			using (var form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				form.availableTemplatesGrid.ListManager.Position = 0;

				mock.Setup(m => m.IsFileCheckedOutByMe(It.IsAny<string>())).Returns(false);
				mock.Setup(m => m.CheckOut(It.IsAny<string>(), It.IsAny<bool>())).Throws(new SourceControlException("Blah."));
				form.markTemplateEditableButton.PerformClick();
			}

			AssertEquals("Last Message Shown.", "Blah.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMarkSystemDocumentElementsEditable()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				var templates = new StmTemplateBaseCollection(Factory);
				var commonTemplate = templates.AddNew();
				commonTemplate.SO_Name = "Common Template";
				var systemDocElements = templates.AddNew();
				systemDocElements.SO_Name = "System Document Elements";

				form.availableTemplatesGrid.SetDataBinding(templates, null);

				form.availableTemplatesGrid.ListManager.Position = 1;
				AssertEquals("MarkTemplateEditableButton should be disabled when System Document Elements selected.", false, form.markTemplateEditableButton.Enabled);

				form.availableTemplatesGrid.ListManager.Position = 0;
				AssertEquals("MarkTemplateEditableButton should be enabled when any template selected except System Document Elements.", true, form.markTemplateEditableButton.Enabled);
			}
		}

		public void TestGridCursorsAreCorrectOnLoad()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				StmMenuTemplatePivot templatePivot = form.BusinessEntity.Menus[0].Documents[0];
				AssertEquals("CurrentTemplatePivot.PK", templatePivot.PK, form.CurrentTemplatePivot.PK);
				AssertEquals("CurrentTemplate.PK", templatePivot.Template.PK, form.CurrentTemplate.PK);
			}
		}

		public virtual void TestButtonsAreDisplayedCorrectly()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				AssertEquals("Precondition: form.BusinessEntity.HasChanges", false, form.BusinessEntity.HasChanges);
				AssertEquals("\'Save\' button should be disabled initially.", false, form.zPostingButtonsUserControl1.SaveButton.Enabled);
				AssertEquals("The OK button should have \'Save & Close\' written on it.", "Save && Close", form.zPostingButtonsUserControl1.SaveAndCloseButton.Text);
				AssertEquals("\'Cancel\' button should have \'Close\' written on it initially.", "Close", form.zPostingButtonsUserControl1.CloseButton.Text);

				form.BusinessEntity.HasChanges = true;

				AssertEquals("\'Save\' button should be enabled after a change to the form.", true, form.zPostingButtonsUserControl1.SaveButton.Enabled);
				AssertEquals("\'Cancel\' button should have \'Cancel\' written on it after a change to the form.", "&Cancel", form.zPostingButtonsUserControl1.CloseButton.Text);

				form.zPostingButtonsUserControl1.SaveButton.PerformClick();

				AssertEquals("\'Save\' button should be disabled after saving.", false, form.zPostingButtonsUserControl1.SaveButton.Enabled);

				form.BusinessEntity.HasChanges = true;
				form.zPostingButtonsUserControl1.SaveButton.PerformClick();
				AssertEquals("form.BusinessEntity.HasChanges should be set to false after the \'Save\' button is clicked, but it was true. This indicates the save button is not working correctly.", false, form.BusinessEntity.HasChanges);

				AssertEquals("\'Cancel\' button should have \'Close\' written on it after save.", "Close", form.zPostingButtonsUserControl1.CloseButton.Text);
			}
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadTemplate()
		{
			AssertLoadTemplate(FilePathForLoadTemplate, OriginalDataContext);
		}

		[DatCapabilityRequirement("SOURCE_CODE")]
		public void TestLoadTemplateAsXlsx()
		{
			AssertLoadTemplate(FilePathForLoadTemplateXlsx, OriginalDataContext);
		}

		public void TestCtrlSNoException() //WI00065760
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				form.Show();
				var saveMenu = ((IFileMenuItemsProvider)form).FileMenuItem.MenuItems.FindByText("&Save");
				AssertNoExceptionThrown(delegate
				{
					if (saveMenu.Enabled)
					{
						saveMenu.PerformClick();
					}
				});
			}
		}

		public void TestCtrlNewNoException()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				var businessEntity = form.BusinessEntity;

				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				Assert("Menus must be more than 1", businessEntity.Menus.Count > 0);
				var menu = businessEntity.Menus[0];
				menu.SU_EmailSenderOverride = "test@test.com";
				AssertEquals("bizO should has change", true, businessEntity.HasChanges);

				form.zPostingButtonsUserControl1.SaveButton.PerformClick();

				var newMenu = ((IFileMenuItemsProvider)form).FileMenuItem.MenuItems.FindByText("&New");
				AssertNoExceptionThrown(delegate
				{
					if (newMenu.Enabled)
					{
						newMenu.PerformClick();
					}
				});
			}
		}

		public void TestCloseButtonWhenNoChanges()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				var businessEntity = form.BusinessEntity;

				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				businessEntity.HasChanges = false;

				Assert("Precondition. The business entity does not contain changes", !businessEntity.HasChanges);
				form.zPostingButtonsUserControl1.CloseButton.PerformClick();
				Assert("Check whether the form has been closed and disposed of", form.IsDisposed);
			}
		}

		public void TestCancelButtonWhenChangesPresent()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				var factorySavedSuccessfullyCount = 0;

				form.BusinessEntity.Factory.Saved += (factory, savedSuccessfully) =>
				{
					if (savedSuccessfully)
					{
						factorySavedSuccessfullyCount++;
					}
				};

				var businessEntity = form.BusinessEntity;

				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				businessEntity.HasChanges = true;
				var dialogBox = UnitTestUserNotification.Instance;
				dialogBox.AddAnswer(DialogResult.Cancel);

				form.zPostingButtonsUserControl1.CloseButton.PerformClick();
				AssertEquals("A popup dialog box appears asking the user whether or not they want to save", "This record has been modified.\r\nWould you like to save the changes?", dialogBox.LastMessage.Text);
				Assert("Check whether the form is still open", !form.IsDisposed);
				AssertEquals("businessEntity.HasChanges", true, businessEntity.HasChanges);

				dialogBox.ClearMessagesAndAnswers();
				dialogBox.AddAnswer(DialogResult.Yes);
				form.zPostingButtonsUserControl1.CloseButton.PerformClick();

				AssertEquals("Factory.Saved count", 1, factorySavedSuccessfullyCount);
			}
		}

		[RequiresSTA]
		public void TestXButtonWhenChangesPresent()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				var businessEntity = form.BusinessEntity;

				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				businessEntity.HasChanges = true;

				var dialogBox = UnitTestUserNotification.Instance;
				dialogBox.AddAnswer(DialogResult.Cancel);

				form.Close();
				AssertEquals("A popup dialog box appears asking the user whether or not they want to save", "This record has been modified.\r\nWould you like to save the changes?", dialogBox.LastMessage.Text);
				Assert("Check whether the form is still open", !form.IsDisposed);
			}
		}

		[RequiresSTA]
		public void TestHasChangesChanged_ShouldClearTemplateCache()
		{
			var helper = new Enterprise.DocumentEngine.DocBuilder.Testing.TemplateCacheTestHelper(Factory);

			if (Directory.Exists(helper.FileSystemCacheFullPath))
			{
				Directory.Delete(helper.FileSystemCacheFullPath, true);
			}

			AssertEquals("Cache should be empty", 0, Directory.Exists(helper.FileSystemCacheFullPath) ? Directory.GetFiles(helper.FileSystemCacheFullPath).Length : 0);

			using (var form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				TemplateCache.Instance.PurgeOldRecords();

				var templateCachePath = helper.FileSystemCacheFullPath;
				if (!Directory.Exists(templateCachePath))
				{
					Directory.CreateDirectory(templateCachePath);
				}

				Func<int> numberOfTemplatesInCache = () => Directory.Exists(templateCachePath) ? Directory.GetFiles(templateCachePath).Length : 0;

				try
				{
					File.WriteAllText(Path.Combine(templateCachePath, "Something.xls"), "Blah blah blah");
					AssertEquals("Should be an item in the cache", 1, numberOfTemplatesInCache());

					AssertEquals(false, form.BusinessEntity.HasChanges);
					form.BusinessEntity.HasChanges = false;
					AssertEquals("Setting HasChanges to false should not affect template cache", 1, numberOfTemplatesInCache());

					form.BusinessEntity.HasChanges = true;
					AssertEquals("Setting HasChanges should clear template cache", 0, numberOfTemplatesInCache());
				}
				finally
				{
					if (Directory.Exists(templateCachePath))
					{
						Directory.Delete(templateCachePath, true);
					}
				}
			}
		}

		public void TestSaveButtonClick_ShouldIncrementCustomisationVersionInRegistry()
		{
			var startingCustomisationVersion = RawDataRegistry.Instance.DocumentCustomisationVersionNumber.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);

			using (var form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;
				try
				{
					form.BusinessEntity.HasChanges = true;
					form.zPostingButtonsUserControl1.SaveAndCloseButton.PerformClick();
					AssertEquals(startingCustomisationVersion + 1, RawDataRegistry.Instance.DocumentCustomisationVersionNumber.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
				}
				finally
				{
					CargoWise.Common.ErrorReporter.Clear();
				}
			}
		}

		public void TestFilterTextBoxStyle()
		{
			using (var form = GetFormToBash())
			{
				form.Show();

				AssertEquals(true, form.filterTextBox.ShouldEscapeAllSpecialCharacters);
			}
		}

		public void TestMenuShortcutList()
		{
			using (MenuCustomisationForm form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;

				string templateFilePath = form.testTemplateFilePath = form.BusinessEntity.AvailableTemplates[0].ExcelTemplateFullPath;

				var menuShortcuts = form.BusinessEntity.Menus[0].MenuShortcutList.GetAllCodes();

				Assert(!menuShortcuts.Contains("CtrlShiftS"));
				Assert(!menuShortcuts.Contains("CtrlShiftD"));
				Assert(!menuShortcuts.Contains("CtrlShiftR"));
			}
		}

		public virtual void TestEmailSubjectTextBox_CharacterLimitIs512()
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuDetailsTab;
				var emailSubjectTextBox = form.PivotAndChildMenuTabControl.Controls.Find("emailSubjectTextBox", true).FirstOrDefault() as ZTextBox;

				AssertEquals(512, emailSubjectTextBox.MaxLength);
			}
		}

		protected abstract string FilePathForLoadTemplate { get; }

		protected abstract string FilePathForLoadTemplateXlsx { get; }

		protected abstract string OriginalDataContext { get; }

		protected void TestAddAndRemovePivot(MenuCustomisationForm form, ZGrid availableElementsGrid, ZGrid pivotGrid, ZButton addPivotButton, ZButton removePivotButton, SchemaColumn menusFKColumn, SchemaColumn availableElementsFKColumn)
		{
			form.Show();

			int pivotCount = pivotGrid.ListManager.Count;

			form.MenusGrid.ListManager.Position = 1;
			availableElementsGrid.UnSelectAll();

			availableElementsGrid.ListManager.Position = 2;
			availableElementsGrid.Select(2);
			addPivotButton.PerformClick();

			AssertEquals(pivotGrid + ".ListManager.Count", pivotCount + 1, pivotGrid.ListManager.Count);
			BusinessObject pivot = (BusinessObject)pivotGrid.ListManager.List[pivotCount];
			AssertEquals("pivot." + menusFKColumn.Name, ((BusinessObject)form.MenusGrid.ListManager.List[1]).PK, pivot[menusFKColumn]);
			AssertEquals("pivot." + availableElementsFKColumn.Name, ((BusinessObject)availableElementsGrid.ListManager.List[2]).PK, pivot[availableElementsFKColumn]);

			removePivotButton.PerformClick();
			AssertEquals(pivotGrid + ".ListManager.Count", pivotCount, pivotGrid.ListManager.Count);
			AssertEquals(pivotGrid + ".ListManager.List.Contains(pivot)", false, pivotGrid.ListManager.List.Contains(pivot));
		}

		protected void TestAddAndRemovePivotWithMultipleRowSelected(MenuCustomisationForm form, ZGrid availableElementsGrid, ZGrid pivotGrid, ZButton addPivotButton, ZButton removePivotButton, SchemaColumn menusFKColumn, SchemaColumn availableElementsFKColumn)
		{
			form.Show();

			var pivotCount = pivotGrid.ListManager.Count;

			form.MenusGrid.ListManager.Position = 1;
			availableElementsGrid.UnSelectAll();

			var count = Math.Min(availableElementsGrid.ListManager.Count, 5);
			for (var i = 0; i < count; i++)
			{
				availableElementsGrid.ListManager.Position = i;
				availableElementsGrid.Select(i);
			}
			var allCount = pivotCount + count;

			addPivotButton.PerformClick();
			AssertEquals(pivotGrid + ".ListManager.Count", allCount, pivotGrid.ListManager.Count);

			var addedList = new List<BusinessObject>();
			for (var i = 0; i < count; i++)
			{
				var pivot = (BusinessObject)pivotGrid.ListManager.List[i + pivotCount];
				addedList.Add(pivot);
				AssertEquals(i + " pivot." + menusFKColumn.Name, ((BusinessObject)form.MenusGrid.ListManager.List[1]).PK, pivot[menusFKColumn]);
				AssertEquals(i + " pivot." + availableElementsFKColumn.Name, ((BusinessObject)availableElementsGrid.ListManager.List[i]).PK, pivot[availableElementsFKColumn]);
			}

			removePivotButton.PerformClick();
			AssertEquals(pivotGrid + ".ListManager.Count", pivotCount, pivotGrid.ListManager.Count);
			addedList.ForEach(x =>
			{
				AssertEquals(pivotGrid + ".ListManager.List.Contains(pivot)", false, pivotGrid.ListManager.List.Contains(x));
			});
		}

		protected void TestGridTracking(MenuCustomisationForm form, ZTabPage tabPage, ZButton addButton, ZGrid availableElementsGrid, ZGrid pivotGrid, SchemaColumn foreignKeyColumn)
		{
			form.Show();
			form.PivotAndChildMenuTabControl.SelectedTab = tabPage;
			int menusGridCount = Math.Min(form.MenusGrid.ListManager.Count, 10);

			for (int i = 0; i < menusGridCount; i++)
			{
				form.MenusGrid.ListManager.Position = i;
				for (int j = 0; j < Math.Min(availableElementsGrid.ListManager.Count, 10); j++)
				{
					availableElementsGrid.ListManager.Position = j;
					addButton.PerformClick();
				}
			}

			for (int i = 0; i < menusGridCount; i++)
			{
				form.MenusGrid.ListManager.Position = i;
				AssertSelectedElement(availableElementsGrid, pivotGrid, foreignKeyColumn);

				for (int j = 0; j < pivotGrid.ListManager.Count; j++)
				{
					pivotGrid.ListManager.Position = j;
					AssertSelectedElement(availableElementsGrid, pivotGrid, foreignKeyColumn);
				}
			}

			if (form.PivotAndChildMenuTabControl.TabPages.Count > 1)
			{
				form.PivotAndChildMenuTabControl.SelectedIndex =
					(form.PivotAndChildMenuTabControl.SelectedIndex == form.PivotAndChildMenuTabControl.TabPages.Count - 1) ?
					form.PivotAndChildMenuTabControl.SelectedIndex - 1 : form.PivotAndChildMenuTabControl.SelectedIndex + 1;
				form.MenusGrid.ListManager.Position = 0;
				form.PivotAndChildMenuTabControl.SelectedTab = tabPage;
				AssertSelectedElement(availableElementsGrid, pivotGrid, foreignKeyColumn);
			}
		}

		void AssertLoadTemplate(string templateFilePath, string originalDataContext)
		{
			using (var form = GetFormToBash())
			{
				form.Show();
				form.PivotAndChildMenuTabControl.SelectedTab = form.menuTemplatesTabPage;
				form.testTemplateFilePath = templateFilePath;

				var template = form.CurrentTemplate;
				AssertNotNull("CurrentTemplate", template);

				template.SO_DataContext = originalDataContext;
				template.SO_IsSystemDefined = false;
				template.SO_ExcelTemplatePath = templateFilePath;
				template.SO_Template = ZBlob.Empty;
				template.IsCheckedOutByMe = true;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				form.loadTemplateButton.PerformClick();

				AssertEquals("Last Message Shown", "Do you want to re-load from '" + templateFilePath + "'? Click 'No' to select another file.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("template.SO_Template.IsEmpty", false, template.SO_Template.IsEmpty);
			}
		}

		static Control FindControl(Control control, string name)
		{
			foreach (Control child in control.Controls)
			{
				if (child.Name == name)
				{
					return child;
				}
				Control childFinding = FindControl(child, name);
				if (childFinding != null)
				{
					return childFinding;
				}
			}
			return null;
		}

		void AssertSelectedElement(ZGrid availableElementsGrid, ZGrid pivotGrid, SchemaColumn foreignKeyColumn)
		{
			BusinessObject pivot = (BusinessObject)pivotGrid.ListManager.GetCurrent();
			AssertEquals("Selected PK", pivot[foreignKeyColumn], ((BusinessObject)availableElementsGrid.ListManager.GetCurrent()).PK);
			AssertEquals("SelectedRowCount", 1, availableElementsGrid.SelectedRowCount);
		}

		new MenuCustomisationForm GetFormToBash() => (MenuCustomisationForm)base.GetFormToBash();
	}
}
