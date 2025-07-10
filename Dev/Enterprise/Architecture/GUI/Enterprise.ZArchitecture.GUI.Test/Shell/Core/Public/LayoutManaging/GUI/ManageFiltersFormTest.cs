using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Internal;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Modules.Testing;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.GUI.Internal.Testing
{
	[TestedType(typeof(ManageLayoutsForm))]
	public class ManageFiltersFormTest : ZFormBasherTest
	{
		public void TestRenameLayoutDialogIsCentered()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "RenameDialogTest", false, false, false);
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form =
				new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				form.Show();
				Application.DoEvents();
				SelectItem(form, "RenameDialogTest");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.RenameSelectedLayoutExposed();
				AssertEquals(FormStartPosition.CenterParent, ZFormModaliser.LastFormShownDialogForTest.StartPosition);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return new ManageLayoutsForm(new DummyFilterStripBusinessObject(), true, true, false);
		}

		public void TestSelectedLayoutItem()
		{
			var translationsEditor = new Mock<ICustomizableDataTranslationEditor>();
			ObjectFactory.Substitute(Mock.Of<ICustomizableDataTranslationEditor>());

			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZTestGridForm(dummy))
			{
				form.Show();
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				var filter = Factory.New<StmModuleFilter>();
				filter.S9_ModuleID = gridID;
				filter.S9_IsPublished = true;
				filter.S9_FilterName = "Test";
				Factory.Save();

				using (var manageForm = new ManageLayoutsForm(gridLayoutManageable, false, false, false))
				{
					manageForm.Show();
					var filterLayouts1 = manageForm.FiltersTreeView.Nodes["My Filter Layouts"];
					manageForm.FiltersTreeView.PerformSelect(filterLayouts1);
					AssertEquals("GIVEN a non-layout node WHEN selected THEN button should be disabled", false, manageForm.editLocalLanguageValuesButton.Enabled);
					var filterLayouts2 = manageForm.FiltersTreeView.Nodes["Shared Filter Layouts"];
					manageForm.FiltersTreeView.PerformSelect(filterLayouts2);
					AssertEquals("GIVEN a non-layout node WHEN selected THEN button should be disabled", false, manageForm.editLocalLanguageValuesButton.Enabled);
				}
			}
		}

		public void TestSaveGridColourAndSaveColumnCheckBoxVisibility()
		{
			using (var form = new ManageLayoutsForm(new DummyFilterStripBusinessObject(), true, true, false))
			{
				form.Show();
				AssertEquals("SaveColumnsCheckBox should be visible", true, form.SaveColumnsCheckBox.Visible);
				AssertEquals("SaveGridColourCheckBox should be visible", true, form.SaveGridColourCheckBox.Visible);
				AssertEquals("SaveGridColourNameTextBox should be visible", true, form.SaveGridColourNameTextBox.Visible);
			}

			using (var form = new ManageLayoutsForm(new DummyFilterStripBusinessObject(), false, false, false))
			{
				form.Show();
				AssertEquals("SaveColumnsCheckBox should not be visible", false, form.SaveColumnsCheckBox.Visible);
				AssertEquals("SaveGridColourCheckBox should not be visible", false, form.SaveGridColourCheckBox.Visible);
				AssertEquals("SaveGridColourNameTextBox should not be visible", false, form.SaveGridColourNameTextBox.Visible);
			}
		}

		public void TestListViewIncludesDefaultLayout()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZTestGridForm(dummy))
			{
				form.Show();

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = false;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = true;

				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);

				//should have serialised correctly
				new DataGridLayoutManager().SavePreconfiguredLayout(gridLayoutManageable, "DescriptionVisible", false, false, SaveColumnLayout.Ignore);

				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Number].IsVisible = true;
				form.TabGrid.Columns[DummyBizoSchema.Constants.Z0_Description].IsVisible = false;
				form.TabGrid.Columns.HasLayoutChanged = true;//default layout is saved
			}

			using (var form = new ZTestGridForm(dummy))
			{
				form.Show();
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);

				using (var manageForm = new TestManageLayoutsForm(gridLayoutManageable, false, false, false))
				{
					manageForm.Show();
					manageForm.OnShownExposed();

					AssertEquals("Three list items are expected including 'Default' layout", 3, manageForm.FiltersTreeViewExposed.Nodes.Count);

					AssertEquals("DescriptionVisible", manageForm.FiltersTreeViewExposed.Nodes[0].Nodes[0].Text);
					AssertEquals(StmDataGridLayoutStorage.DefaultLayoutName, manageForm.FiltersTreeViewExposed.Nodes[2].Text);

					AssertNotEquals(default(ZGuid), (manageForm.FiltersTreeViewExposed.Nodes[0].Nodes[0] as LayoutsTreeNode).Layout.PK);
					AssertNotEquals(default(ZGuid), (manageForm.FiltersTreeViewExposed.Nodes[2] as LayoutsTreeNode).Layout.PK);

					AssertRenameIsNotAllowedForDefaultLayout(manageForm);

					AssertDeleteIsNotAllowedForDefaultLayout(manageForm);
				}
			}
		}

		public void TestEditLocalLanguagesButton()
		{
			var translationsEditor = new Mock<ICustomizableDataTranslationEditor>();
			ObjectFactory.Substitute(translationsEditor.Object);

			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZTestGridForm(dummy))
			{
				form.Show();
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				var filter = Factory.New<StmModuleFilter>();
				filter.S9_ModuleID = gridID;
				filter.S9_IsPublished = true;
				filter.S9_FilterName = "Test";
				Factory.Save();

				using (var manageForm = new ManageLayoutsForm(gridLayoutManageable, false, false, false))
				{
					manageForm.Show();
					AssertEquals(true, manageForm.editLocalLanguageValuesButton.Enabled);
					manageForm.editLocalLanguageValuesButton.PerformClick();
				}

				translationsEditor.Verify(o => o.EditTranslations(filter.S9_FilterNameInfo.CustomizableDataResourceStrings, (ResourceString)filter.S9_FilterNameMultilingual, filter));
			}
		}

		[ExpectNoExceptions]
		public void TestEditLocalLanguagesButtonUsesSelectedFilter()
		{
			var translationsEditor = new Mock<ICustomizableDataTranslationEditor>();
			ObjectFactory.Substitute(translationsEditor.Object);

			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZTestGridForm(dummy))
			{
				form.Show();
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);
				var filter1 = Factory.New<StmModuleFilter>();
				filter1.S9_ModuleID = gridID;
				filter1.S9_IsPublished = true;
				filter1.S9_FilterName = "Test 1";
				var filter2 = Factory.New<StmModuleFilter>();
				filter2.S9_ModuleID = gridID;
				filter2.S9_IsPublished = true;
				filter2.S9_FilterName = "Test 2";
				Factory.Save();

				using (var manageForm = new ManageLayoutsForm(gridLayoutManageable, false, false, false))
				{
					manageForm.Show();
					Application.DoEvents();
					manageForm.FiltersTreeView.SelectedNode = FindInTreeNodeList(manageForm.FiltersTreeView.Nodes.Cast<TreeNode>(), "Test 1");
					manageForm.editLocalLanguageValuesButton.PerformClick();

					translationsEditor.Verify(o => o.EditTranslations(filter1.S9_FilterNameInfo.CustomizableDataResourceStrings, (ResourceString)filter1.S9_FilterNameMultilingual, filter1));

					manageForm.FiltersTreeView.SelectedNode = FindInTreeNodeList(manageForm.FiltersTreeView.Nodes.Cast<TreeNode>(), "Test 2");
					manageForm.editLocalLanguageValuesButton.PerformClick();
					translationsEditor.Verify(o => o.EditTranslations(filter2.S9_FilterNameInfo.CustomizableDataResourceStrings, (ResourceString)filter2.S9_FilterNameMultilingual, filter2));
				}
			}
		}

		public void TestEditLocalLanguagesButtonWithNoFilters()
		{
			var dummy = Factory.New<DummyBusinessObject>();
			using (var form = new ZTestGridForm(dummy))
			{
				form.Show();
				var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
				var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);

				using (var manageForm = new ManageLayoutsForm(gridLayoutManageable, false, false, false))
				{
					manageForm.Show();
					manageForm.editLocalLanguageValuesButton.PerformClick();
				}

				AssertEquals("No published layouts found for the current module.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestEditLocalLanguagesButtonSecurity()
		{
			EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed = false;
			try
			{
				var dummy = Factory.New<DummyBusinessObject>();
				using (var form = new ZTestGridForm(dummy))
				{
					form.Show();
					var gridID = new DataGridLayoutContextKeyProvider(form.TabGrid).ContextKeyForStmModuleFilter;
					var gridLayoutManageable = new ZGridLayoutModification(form.TabGrid, form.TabGrid.Columns, Factory, null);

					using (var manageForm = new ManageLayoutsForm(gridLayoutManageable, false, false, false))
					{
						manageForm.Show();
						AssertEquals(false, manageForm.editLocalLanguageValuesButton.Enabled);
					}
				}
			}
			finally
			{
				EnvProxy.Instance.Security.PublishGlobalFilterLayouts.IsAllowed = true;
			}
		}

		public void TestSaveButton_DeleteLayout()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "First", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Second", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "First [+]", "Second [+]");
				AssertEquals(false, form.SaveButtonExposed.Enabled);

				SelectItem(form, "First [+]");
				form.DeleteSelectedLayoutExposed();

				AssertEquals(true, form.SaveButtonExposed.Enabled);
				form.SaveButtonExposed.PerformClick();
				Assert(!form.IsDisposed);
			}
		}

		public void TestSaveButton_RenameLayout()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "First", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Second", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "First [+]", "Second [+]");
				AssertEquals(false, form.SaveButtonExposed.Enabled);

				SelectItem(form, "First [+]");
				form.NewLayoutNameForRenaming = "A";
				form.RenameSelectedLayoutExposed();
				AssertLayoutList(form, "A [+]", "Second [+]");
				var layoutsTreeNodeA = (LayoutsTreeNode)FindItemWithText(form, "A [+]");
				AssertNotNull("'A' should have a layout", layoutsTreeNodeA.Layout);

				AssertEquals(true, form.SaveButtonExposed.Enabled);
				form.SaveButtonExposed.PerformClick();
				Assert(!form.IsDisposed);
			}
		}

		public void TestSaveButton_SaveColumnsCheckBox()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "First", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Second", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "First [+]", "Second [+]");
				AssertEquals(false, form.SaveColumnsCheckBox.Checked);
				AssertEquals(false, form.SaveButtonExposed.Enabled);

				form.SaveColumnsCheckBox.Checked = true;

				AssertEquals(true, form.SaveButtonExposed.Enabled);
				form.SaveButtonExposed.PerformClick();
				Assert(!form.IsDisposed);
			}
		}

		public void TestSaveButton_SaveGridColourCheckBox()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "First", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Second", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "First [+]", "Second [+]");
				AssertEquals(false, form.SaveGridColourCheckBox.Checked);
				AssertEquals(false, form.SaveButtonExposed.Enabled);

				form.SaveGridColourCheckBox.Checked = true;

				AssertEquals(true, form.SaveButtonExposed.Enabled);
				form.SaveButtonExposed.PerformClick();
				Assert(!form.IsDisposed);
			}
		}

		public void TestSaveCloseButton_DeleteLayout()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "First", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Second", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "First [+]", "Second [+]");
				AssertEquals(false, form.SaveCloseButtonExposed.Enabled);

				SelectItem(form, "First [+]");
				form.DeleteSelectedLayoutExposed();

				AssertEquals(true, form.SaveCloseButtonExposed.Enabled);
				form.SaveCloseButtonExposed.PerformClick();
				Assert(form.IsDisposed);
			}
		}

		public void TestSaveCloseButton_RenameLayout()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "First", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Second", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "First [+]", "Second [+]");
				AssertEquals(false, form.SaveCloseButtonExposed.Enabled);

				SelectItem(form, "First [+]");
				form.NewLayoutNameForRenaming = "A";
				form.RenameSelectedLayoutExposed();
				AssertLayoutList(form, "A [+]", "Second [+]");
				var layoutsTreeNodeA = (LayoutsTreeNode)FindItemWithText(form, "A [+]");
				AssertNotNull("'A' should have a layout", layoutsTreeNodeA.Layout);

				AssertEquals(true, form.SaveCloseButtonExposed.Enabled);
				form.SaveCloseButtonExposed.PerformClick();
				Assert(form.IsDisposed);
			}
		}

		public void TestSaveCloseButton_SaveColumnsCheckBox()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "First", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Second", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "First [+]", "Second [+]");
				AssertEquals(false, form.SaveColumnsCheckBox.Checked);
				AssertEquals(false, form.SaveCloseButtonExposed.Enabled);

				form.SaveColumnsCheckBox.Checked = true;

				AssertEquals(true, form.SaveCloseButtonExposed.Enabled);
				form.SaveCloseButtonExposed.PerformClick();
				Assert(form.IsDisposed);
			}
		}

		public void TestSaveCloseButton_SaveGridColourCheckBox()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "First", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Second", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "First [+]", "Second [+]");
				AssertEquals(false, form.SaveGridColourCheckBox.Checked);
				AssertEquals(false, form.SaveCloseButtonExposed.Enabled);

				form.SaveGridColourCheckBox.Checked = true;

				AssertEquals(true, form.SaveCloseButtonExposed.Enabled);
				form.SaveCloseButtonExposed.PerformClick();
				Assert(form.IsDisposed);
			}
		}
		public void TestNonUserDefinedFilter_WhenSelected_ShouldEnableSaveColumnCheckBox()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "First", true, true, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Second", false, false, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.DummyDependent, "Other Module", false, false, false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "First", "Second");

				SelectItem(form, "First");
				AssertEquals(false, form.SaveColumnsCheckBox.ReadOnly);
				AssertEquals(false, form.SaveGridColourCheckBox.ReadOnly);
				AssertEquals(true, form.SaveGridColourNameTextBox.ReadOnly);
				form.SaveColumnsCheckBox.Checked = true;
				form.SaveGridColourCheckBox.Checked = true;

				SelectItem(form, "Second");
				AssertEquals(false, form.SaveColumnsCheckBox.ReadOnly);
				AssertEquals(false, form.SaveGridColourCheckBox.ReadOnly);
				AssertEquals(true, form.SaveGridColourNameTextBox.ReadOnly);
				form.SaveColumnsCheckBox.Checked = true;
				form.SaveGridColourCheckBox.Checked = true;
			}
		}

		public void TestUserDefinedFilter_WhenSelected_ShouldDisableSaveColumnCheckBox()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Third", true, true, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Fourth", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.DummyDependent, "Other Module UDFS", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "Third [+]", "Fourth [+]");

				SelectItem(form, "Third [+]");
				AssertEquals(true, form.SaveColumnsCheckBox.ReadOnly);
				AssertEquals(false, form.SaveColumnsCheckBox.Checked);
				AssertEquals(true, form.SaveGridColourCheckBox.ReadOnly);
				AssertEquals(false, form.SaveGridColourCheckBox.Checked);
				AssertEquals(true, form.SaveGridColourNameTextBox.ReadOnly);

				SelectItem(form, "Fourth [+]");
				AssertEquals(true, form.SaveColumnsCheckBox.ReadOnly);
				AssertEquals(false, form.SaveColumnsCheckBox.Checked);
				AssertEquals(true, form.SaveGridColourCheckBox.ReadOnly);
				AssertEquals(false, form.SaveGridColourCheckBox.Checked);
				AssertEquals(true, form.SaveGridColourNameTextBox.ReadOnly);
			}
		}

		public void TestUserDefinedFilter_AreWellConstructedInTree()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "A/B", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "A/C", false, false, true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "D", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "A", "B [+]", "C [+]", "D [+]");
			}
		}

		public void TestDeleteLayout_WhenUserDefinedFilter_UsedInLayout_ShouldShowWarning()
		{
			SetUpFiltersForLayoutAdjustmentTests();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "Me filters [+]", "Me other filters [+]");

				SelectItem(form, "Me filters [+]");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.DeleteSelectedLayoutExposed();

				const string warning = @"The user-defined filter [Me filters] is included in the filter layout [Me other filters]:

Test Module -> Me other filters -> Me filters

Deleting [Me filters] will remove it from that layout and may affect its query results. Are you sure you want to delete [Me filters]?";

				AssertMultilineASCIIEquals("A warning should have been displayed, and yet...", warning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Me filters [+]", "Me other filters [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.DeleteSelectedLayoutExposed();

				AssertMultilineASCIIEquals("A warning should have been displayed, and yet...", warning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Me other filters [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				SelectItem(form, "Layout With UDFS");
				form.DeleteSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Me other filters [+]");

				form.CancelButtonExposed.PerformClick();
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "Me filters [+]", "Me other filters [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				SelectItem(form, "Layout With UDFS");
				form.DeleteSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				SelectItem(form, "Me other filters [+]");
				form.DeleteSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				AssertLastStmModuleFilterEventHasBeenLogged("ADD", "Me other filters");
				form.SaveButtonExposed.PerformClick();
				AssertLastStmModuleFilterEventHasBeenLogged("DEL", "Me other filters");
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "Me filters [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				SelectItem(form, "Me filters [+]");
				form.DeleteSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				form.SaveButtonExposed.PerformClick();
				AssertLastStmModuleFilterEventHasBeenLogged("DEL", "Me filters");
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, Array.Empty<string>());
			}

			AssertLastStmModuleFilterEventHasBeenLogged("DEL", "Me filters");
		}

		void AssertLastStmModuleFilterEventHasBeenLogged(string eventType, string layoutName)
		{
			var query = new ZQuery(StmALogSchema.SL_Table, "StmModuleFilter");
			query.OrderBy = StmALogSchema.Constants.SL_PostedTimeUtc + OrderByClause.Descending;

			var lastFilterLog = Factory.LoadTop1<StmALog>(query);

			CombineAssertions(() =>
			{
				AssertNotNull("Should have found 1 log about StmModuleFilter in StmALog", lastFilterLog);
				AssertEquals("SL_Reference should show filter name", $"Layout Name: '{layoutName}'", lastFilterLog.SL_Reference);
				AssertEquals("Event type should be 'deleted'", eventType, lastFilterLog.SL_SE_NKEvent);
			});
		}

		public void TestRenameLayout_WhenUserDefinedFilter_UsedInLayout_ShouldShowWarning()
		{
			SetUpFiltersForLayoutAdjustmentTests();

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true) { NewLayoutNameForRenaming = "Renamed" })
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "Me filters [+]", "Me other filters [+]");

				SelectItem(form, "Me filters [+]");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.RenameSelectedLayoutExposed();

				const string warning = @"The user-defined filter [Me filters] is included in the filter layout [Me other filters]:
Test Module -> Me other filters -> Me filters
Renaming [Me filters] will remove it from that layout and may affect its query results. Are you sure you want to rename [Me filters]?";

				AssertMultilineASCIIEquals("A warning should have been displayed, and yet...", warning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Me filters [+]", "Me other filters [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.RenameSelectedLayoutExposed();

				AssertMultilineASCIIEquals("A warning should have been displayed, and yet...", warning, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Renamed [+]", "Me other filters [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				form.NewLayoutNameForRenaming = "Other renamed";
				SelectItem(form, "Layout With UDFS");
				form.RenameSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Renamed [+]", "Me other filters [+]");

				form.CancelButtonExposed.PerformClick();
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "Me filters [+]", "Me other filters [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				SelectItem(form, "Layout With UDFS");
				form.DeleteSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				SelectItem(form, "Me other filters [+]");
				form.DeleteSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				form.SaveButtonExposed.PerformClick();
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true) { NewLayoutNameForRenaming = "Renamed" })
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "Me filters [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				SelectItem(form, "Me filters [+]");
				form.RenameSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);

				form.SaveButtonExposed.PerformClick();
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "Renamed [+]");
			}
		}

		public void TestRenameAndDeleteLayouts_WithMultipleLevels_ShouldShowsCorrectTreeView_WithCorrectLayouts()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "A/B", false, false, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "C/D", false, false, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "A", false, false, false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "A", "B", "C", "D"); // A/B and C/D

				SelectItem(form, "B");
				form.NewLayoutNameForRenaming = "A/E";
				form.RenameSelectedLayoutExposed();
				AssertLayoutList(form, "A", "E", "C", "D"); // A/E and C/D
				var layoutsTreeNodeA = (LayoutsTreeNode)FindItemWithText(form, "A");
				AssertNotNull("'A' should have a layout", layoutsTreeNodeA.Layout);

				SelectItem(form, "A");
				form.NewLayoutNameForRenaming = "F";
				form.RenameSelectedLayoutExposed();
				AssertLayoutList(form, "A", "F", "E", "C", "D"); // A/E, C/D and F
				var layoutsTreeNodeF = (LayoutsTreeNode)FindItemWithText(form, "F");
				AssertNotNull("The layout should be with 'F' after renaming A to F", layoutsTreeNodeF.Layout);
				layoutsTreeNodeA = (LayoutsTreeNode)FindItemWithText(form, "A");
				AssertNull("The layout for 'A' should be gone after renaming A to F", layoutsTreeNodeA.Layout);

				SelectItem(form, "F");
				form.DeleteSelectedLayoutExposed();
				AssertLayoutList(form, "E", "A", "C", "D"); // A/E and C/D

				SelectItem(form, "E");
				form.DeleteSelectedLayoutExposed();
				AssertLayoutList(form, "C", "D"); // only C/D

				SelectItem(form, "D");
				form.DeleteSelectedLayoutExposed();
				AssertLayoutList(form); // nothing left
			}
		}

		static void SetUpFiltersForLayoutAdjustmentTests()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Me filters", false, false, true);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("[USR]Me filters");

				FilterStripsTestHelper.SaveFilterLayout(filterBizo, "Me other filters", false, false, isUserDefinedFilter: true);
			}
		}

		public void TestDeleteUserDefinedFilter_UsedInFiltersMatchOperator_ShouldShowWarning()
		{
			AssertAdjustUserDefinedFilter_UsedInFiltersMatchOperator_ShouldShowWarning(form => form.DeleteSelectedLayoutExposed(), false);
		}

		public void TestRenameUserDefinedFilter_UsedInFiltersMatchOperator_ShouldShowWarning()
		{
			AssertAdjustUserDefinedFilter_UsedInFiltersMatchOperator_ShouldShowWarning(form => form.RenameSelectedLayoutExposed(), true);
		}

		void AssertAdjustUserDefinedFilter_UsedInFiltersMatchOperator_ShouldShowWarning(Action<TestManageLayoutsForm> adjustLayoutAction, bool isRename)
		{
			FilterStripsTestHelper.CreateUserDefinedFilterStrip("Me filters", ModuleIDs.GlbStaff, "Code", "ABC", isPublished: false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.SalesEnquiry))
			{
				var filterBizo = module.FilterBusinessObject;
				var strip = filterBizo.FilterStrips.AddNew("Assigned Staff");
				var filter = (ModuleNkFilter)strip.CurrentModuleFilter;
				filter.ComparisonOperator = ModuleTextFilter.ComparisonConstants.FiltersMatch;
				filter.SelectedFilters.AddFilterStrip<ModuleFilter>("[USR]Me filters");

				FilterStripsTestHelper.SaveFilterLayout(filterBizo, "My Sales Enquiry layout", false, false, isUserDefinedFilter: false);
			}

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.GlbStaff))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "Me filters [+]");

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				SelectItem(form, "Me filters [+]");
				adjustLayoutAction(form);

				var expectedWarning = isRename
					? @"The user-defined filter [Me filters] is included in the filter layout [My Sales Enquiry layout]:

Inquiry Manager -> My Sales Enquiry layout -> Assigned Staff -> Me filters

Renaming [Me filters] will remove it from that layout and may affect its query results. Are you sure you want to rename [Me filters]?"
					: @"The user-defined filter [Me filters] is included in the filter layout [My Sales Enquiry layout]:

Inquiry Manager -> My Sales Enquiry layout -> Assigned Staff -> Me filters

Deleting [Me filters] will remove it from that layout and may affect its query results. Are you sure you want to delete [Me filters]?";

				AssertMultilineASCIIEquals("The user should have been prompted about the effects of adjusting the filter, and yet...", expectedWarning, UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDeleteLayout_WhenNestedUserDefinedFilter_UsedOnFilterRule_ShouldShowErrorAndNotDelete()
		{
			AssertAdjustLayout_WhenUserDefinedFilter_UsedOnFilterRule_ShouldShowErrorAndNotChange(form => form.DeleteSelectedLayoutExposed(), false, true);
		}

		public void TestRenameLayout_WhenNestedUserDefinedFilter_UsedOnFilterRule_ShouldShowErrorAndNotRename()
		{
			AssertAdjustLayout_WhenUserDefinedFilter_UsedOnFilterRule_ShouldShowErrorAndNotChange(form => form.RenameSelectedLayoutExposed(), true, true);
		}

		public void TestDeleteLayout_WhenUserDefinedFilter_UsedOnFilterRule_ShouldShowErrorAndNotDelete()
		{
			AssertAdjustLayout_WhenUserDefinedFilter_UsedOnFilterRule_ShouldShowErrorAndNotChange(form => form.DeleteSelectedLayoutExposed(), false, false);
		}

		public void TestRenameLayout_WhenUserDefinedFilter_UsedOnFilterRule_ShouldShowErrorAndNotRename()
		{
			AssertAdjustLayout_WhenUserDefinedFilter_UsedOnFilterRule_ShouldShowErrorAndNotChange(form => form.RenameSelectedLayoutExposed(), true, false);
		}

		void AssertAdjustLayout_WhenUserDefinedFilter_UsedOnFilterRule_ShouldShowErrorAndNotChange(Action<TestManageLayoutsForm> adjustLayoutAction, bool isRename, bool adjustNested)
		{
			DummyBaseBusinessObject.TypeDecider.TypeForLoadOverride = typeof(DummyWithRelatedFilters);
			FilterStripsTestHelper.DropStmModuleFilterRuleConstraints();
			var udf1 = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Filter to change", true, true, true);
			var udf2 = FilterStripsTestHelper.AddUserDefinedFilterLayoutToAnotherFilterLayout(udf1.S9_FilterName, "Filter with filter to change");

			var dummy1 = Factory.New<DummyWithRelatedFilters>();
			dummy1.Z0_Description = "Thing with filter rule 1";
			var filterRuleLayout1 = dummy1.Filter;
			var dummy2 = Factory.New<DummyWithRelatedFilters>();
			dummy2.Z0_Description = "Thing with filter rule 2";
			var filterRuleLayout2 = dummy2.Filter;

			if (adjustNested)
			{
				FilterStripsTestHelper.AddFilterStrip<ModuleUserDefinedFilter>(filterRuleLayout1, ModuleUserDefinedFilter.GetPrefixedDescription(udf2));
				FilterStripsTestHelper.AddFilterStrip<ModuleUserDefinedFilter>(filterRuleLayout2, ModuleUserDefinedFilter.GetPrefixedDescription(udf2));
			}
			else
			{
				FilterStripsTestHelper.AddFilterStrip<ModuleUserDefinedFilter>(filterRuleLayout1, ModuleUserDefinedFilter.GetPrefixedDescription(udf1));
				FilterStripsTestHelper.AddFilterStrip<ModuleUserDefinedFilter>(filterRuleLayout2, ModuleUserDefinedFilter.GetPrefixedDescription(udf1));
			}

			Factory.Save();

			AssertEquals(DummyModuleIDs.Dummy.Name, filterRuleLayout1.S9_ModuleID);
			AssertEquals(DummyModuleIDs.Dummy.Name, filterRuleLayout2.S9_ModuleID);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true) { NewLayoutNameForRenaming = "Renamed" })
			{
				form.Show();
				Application.DoEvents();
				AssertLayoutList(form, "Filter to change [+]", "Filter with filter to change [+]");

				SelectItem(form, "Filter to change [+]");
				adjustLayoutAction(form);

				var expectedError = GetExpectedErrorMessage_TwoConsumingFilters(isRename, adjustNested);

				AssertContainsExactLinesInAnyOrder(expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Filter to change [+]", "Filter with filter to change [+]");

				dummy2.Delete();
				Factory.Save();

				SelectItem(form, "Filter to change [+]");
				adjustLayoutAction(form);

				expectedError = GetExpectedErrorMessage_OneConsumingFilter(isRename, adjustNested);

				AssertMultilineASCIIEquals("There should have been an error listing the filter rules that use the filter, and yet...", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Filter to change [+]", "Filter with filter to change [+]");

				dummy1.Delete();
				Factory.Save();

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				adjustLayoutAction(form);

				expectedError = GetExpectedErrorMessage_WeirdConsumingFilter(isRename);

				AssertMultilineASCIIEquals("There should have been a warning instead of an error, and yet...", expectedError, UnitTestUserNotification.Instance.LastMessage.Text);

				var expectedList = isRename
					? new[] { "Renamed [+]", "Filter with filter to change [+]" }
					: new[] { "Filter with filter to change [+]" };

				AssertLayoutList(form, expectedList);
			}
		}

		string GetExpectedErrorMessage_TwoConsumingFilters(bool isRename, bool adjustNested)
		{
			if (adjustNested)
			{
				return GetExpectedErrorMessage_WeirdConsumingFilter(isRename);
			}
			else
			{
				return isRename
					? @"The user-defined filter [Filter to change] cannot be renamed because it is included in the following filter rules:

Dummy Business Object: Thing with filter rule 1 -> Filter to change
Dummy Business Object: Thing with filter rule 2 -> Filter to change"
					: @"The user-defined filter [Filter to change] cannot be deleted because it is included in the following filter rules:

Dummy Business Object: Thing with filter rule 1 -> Filter to change
Dummy Business Object: Thing with filter rule 2 -> Filter to change";
			}
		}

		string GetExpectedErrorMessage_OneConsumingFilter(bool isRename, bool adjustNested)
		{
			if (adjustNested)
			{
				return GetExpectedErrorMessage_WeirdConsumingFilter(isRename);
			}
			else
			{
				return isRename
					? @"The user-defined filter [Filter to change] cannot be renamed because it is included in the following filter rule:

Dummy Business Object: Thing with filter rule 1 -> Filter to change"
					: @"The user-defined filter [Filter to change] cannot be deleted because it is included in the following filter rule:

Dummy Business Object: Thing with filter rule 1 -> Filter to change";
			}
		}

		string GetExpectedErrorMessage_WeirdConsumingFilter(bool isRename)
		{
			return isRename
					? @"The user-defined filter [Filter to change] is included in the filter layout [Filter with filter to change]:

Test Module -> Filter with filter to change -> Filter to change

Renaming [Filter to change] will remove it from that layout and may affect its query results. Are you sure you want to rename [Filter to change]?"
					: @"The user-defined filter [Filter to change] is included in the filter layout [Filter with filter to change]:

Test Module -> Filter with filter to change -> Filter to change

Deleting [Filter to change] will remove it from that layout and may affect its query results. Are you sure you want to delete [Filter to change]?";
		}

		public void TestRenameForUserDefinedFilter_WithExistingLayoutOrUserDefinedFilter_WithSameName_ShouldNotBeAllowed()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Caviar", false, false, isUserDefinedFilter: true);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Myanmar", false, false, isUserDefinedFilter: true);
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "Caviar [+]", "Myanmar [+]");

				// Try to rename the user-defined filter

				SelectItem(form, "Caviar [+]");
				form.NewLayoutNameForRenaming = "Myanmar";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.RenameSelectedLayoutExposed();

				AssertEquals("The user-defined filter 'Myanmar' already exists. Please enter a different name.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Caviar [+]", "Myanmar [+]");

				UnitTestUserNotification.Instance.ClearMessages();
				form.NewLayoutNameForRenaming = "Rosanne Barr";
				form.RenameSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Rosanne Barr [+]", "Myanmar [+]");

				// Try to rename the filter layout
				UnitTestUserNotification.Instance.ClearMessages();
				form.NewLayoutNameForRenaming = "Leather bar";
				form.RenameSelectedLayoutExposed();
				AssertEquals(null, UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Leather bar [+]", "Myanmar [+]");
			}
		}

		public void TestTrimSpacesInPath()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				AssertEquals("A/B/C/D/E/F", form.TrimSpacesInPath("A  /   B/ C   /   D    /   E   /     F"));
			}
		}

		public void TestRenameLayout_CannotRenameToExistingLayout()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "A/B/C", true, true, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "D/E/F", true, true, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "G/H/I", false, true, false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "A", "B", "C", "D", "E", "F", "G", "H", "I");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				SelectItem(form, "C");
				form.NewLayoutNameForRenaming = "A/  B  /  C";
				form.RenameSelectedLayoutExposed();
				AssertLayoutList(form, "A", "B", "C", "D", "E", "F", "G", "H", "I");
				AssertNull("Should allow to rename to the same name with spaces", UnitTestUserNotification.Instance.LastMessage.Text);

				SelectItem(form, "C");
				form.NewLayoutNameForRenaming = "G/H/I";
				form.RenameSelectedLayoutExposed();
				AssertLayoutList(form, "D", "E", "F", "G", "H", "I", "G", "H", "I");
				AssertNull("Should allow to rename to a name existing but with a different privacy", UnitTestUserNotification.Instance.LastMessage.Text);

				SelectItem(form, "I", true);
				form.NewLayoutNameForRenaming = "D/E/F";
				form.RenameSelectedLayoutExposed();

				AssertEquals("The filter layout 'D/E/F' already exists. Please enter a different name.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutListContains("Should have picked up a matching layout and denied the rename", form, "D", "E", "F", "G", "H", "I", "G", "H", "I");
				UnitTestUserNotification.Instance.ClearMessages();

				form.NewLayoutNameForRenaming = "D/ E /F";
				form.RenameSelectedLayoutExposed();

				AssertEquals("The filter layout 'D/ E /F' already exists. Please enter a different name.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutListContains("Should have picked up the spaces in the name and found a matching layout and denied the rename", form, "D", "E", "F", "G", "H", "I", "G", "H", "I");
				UnitTestUserNotification.Instance.ClearMessages();

				form.NewLayoutNameForRenaming = "D/E";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				form.RenameSelectedLayoutExposed();
				AssertLayoutListContains("Should be ok to rename to a node that exists already but is not a filter yet", form, "D", "E", "F", "G", "H", "I");
			}
		}

		static void SelectItem(TestManageLayoutsForm form, string itemText, bool? privacy = null)
		{
			form.FiltersTreeViewExposed.SelectedNode = FindItemWithText(form, itemText, privacy);
		}

		static TreeNode FindItemWithText(TestManageLayoutsForm form, string text, bool? privacy = null)
		{
			if (privacy.HasValue)
			{
				if (privacy.Value)
				{
					return FindInTreeNodeList(form.FiltersTreeViewExposed.Nodes[1].Nodes.Cast<TreeNode>(), text);
				}
				else
				{
					return FindInTreeNodeList(form.FiltersTreeViewExposed.Nodes[0].Nodes.Cast<TreeNode>(), text);
				}
			}
			else
			{
				return FindInTreeNodeList(form.FiltersTreeViewExposed.Nodes.Cast<TreeNode>(), text);
			}
		}

		static TreeNode FindInTreeNodeList(IEnumerable<TreeNode> treeNodes, string text)
		{
			if (treeNodes.Any(x => x.Text.StartsWith(text)))
			{
				return treeNodes.First(x => x.Text.StartsWith(text));
			}

			foreach (var subNodes in treeNodes)
			{
				var subLevelNode = FindInTreeNodeList(subNodes.Nodes.Cast<TreeNode>(), text);
				if (subLevelNode != null)
				{
					return subLevelNode;
				}
			}

			return null;
		}

		static void AssertLayoutList(TestManageLayoutsForm form, params string[] expectedLayouts)
		{
			AssertLayoutListContains(null, form, expectedLayouts);
		}

		static void AssertLayoutListContains(string message, TestManageLayoutsForm form, params string[] expectedLayouts)
		{
			var itemNames = FindLayoutsInList(form.FiltersTreeViewExposed.Nodes.Cast<TreeNode>());
			var allExpectedLayouts = expectedLayouts.Concat(new string[] { "My Filter Layouts", "Shared Filter Layouts" });
			AssertContainsExactElementsInAnyOrder(message, allExpectedLayouts, itemNames);
		}

		static IEnumerable<string> FindLayoutsInList(IEnumerable<TreeNode> treeNodes)
		{
			var result = new List<string>();
			result.AddRange(treeNodes.Select(x => x.Text));
			foreach (var subNodes in treeNodes)
			{
				result.AddRange(FindLayoutsInList(subNodes.Nodes.Cast<TreeNode>()));
			}

			return result;
		}

		public void TestZSaveConcurrencyException_IsHandled()
		{
			var data = Factory.NewWithValidTestData<StmData>();
			data.SD_Owner = EnvProxy.Instance.CurrentUser.PK;
			data.SD_DepartmentGuid = EnvProxy.Instance.CurrentCompany.PK;
			data.SD_Name = DummyModuleIDs.Dummy.Name;

			Factory.Save();

			CheckConcurrencyScenario(true, true);
			CheckConcurrencyScenario(true, false);
			CheckConcurrencyScenario(false, true);
			CheckConcurrencyScenario(false, false);
		}

		void CheckConcurrencyScenario(bool isRenameFirst, bool isRenameSecond)
		{
			var filter = FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "ConcurrencyTest", true, true, false);

			using (var module1 = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var module2 = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form1 = new TestManageLayoutsForm(module1.FilterBusinessObject, true, true, false))
			using (var form2 = new TestManageLayoutsForm(module2.FilterBusinessObject, true, true, false))
			{
				form1.FactoryExposed.RefreshEnabled = false;
				form2.FactoryExposed.RefreshEnabled = false;

				module1.FilterBusinessObject.LastUsedLayout = filter;
				module2.FilterBusinessObject.LastUsedLayout = filter;

				form1.Show();
				form2.Show();
				Application.DoEvents();

				AssertLayoutList(form1, "ConcurrencyTest");
				AssertLayoutList(form2, "ConcurrencyTest");

				if (isRenameFirst)
				{
					SelectItem(form1, "ConcurrencyTest");
					form1.NewLayoutNameForRenaming = "Concurrency1";
					form1.RenameSelectedLayoutExposed();
				}
				else
				{
					SelectItem(form1, "ConcurrencyTest");
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form1.DeleteSelectedLayoutExposed();
				}

				if (isRenameSecond)
				{
					SelectItem(form2, "ConcurrencyTest");
					form2.NewLayoutNameForRenaming = "Concurrency2";
					form2.RenameSelectedLayoutExposed();
				}
				else
				{
					SelectItem(form2, "ConcurrencyTest");
					UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
					form2.DeleteSelectedLayoutExposed();
				}

				form1.SaveButtonExposed.PerformClick();
				AssertNoExceptionThrown(() => form2.SaveButtonExposed.PerformClick());
				AssertStartsWith("Concurrency Error Message", "While you were editing your data, another user ", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("Save button should be disabled", false, form2.SaveButtonExposed.Enabled);

				form1.SaveCloseButtonExposed.PerformClick();
				AssertNoExceptionThrown(() => form2.SaveCloseButtonExposed.PerformClick());
				AssertStartsWith("Concurrency Error Message", "While you were editing your data, another user ", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertEquals("SaveClose button should be disabled", false, form2.SaveCloseButtonExposed.Enabled);
				AssertNoExceptionThrown(() => module2.FilterBusinessObject.SaveLastUsedLayout());

				var layout = Factory.Load<StmModuleFilter>(filter.PK);
				if (layout != null)
				{
					layout.Delete();
					Factory.Save();
				}
			}
		}

		public void TestRenamePublishedLayout_ToSameNameAsOtherPublishedLayout_ShouldNotAllow()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Pinot", true, true, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Noir", true, true, false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "Pinot", "Noir");

				SelectItem(form, "Pinot");
				form.NewLayoutNameForRenaming = "Noir";
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				form.RenameSelectedLayoutExposed();

				AssertEquals("The filter layout 'Noir' already exists. Please enter a different name.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Pinot", "Noir");
			}
		}

		public void TestRenamePublishedUserDefinedFilterStrip_WithInsufficientSecurity_ShouldShowError()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Komono You Didn't!", true, false, true);
			EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed = false;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "Komono You Didn't! [+]");

				SelectItem(form, "Komono You Didn't! [+]");
				form.NewLayoutNameForRenaming = "Renamed!";
				form.RenameSelectedLayoutExposed();

				AssertEquals("The selected layout is available to all users and can only be modified by an administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Komono You Didn't! [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed = true;
				form.RenameSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Renamed! [+]");
			}
		}

		public void TestDeletePublishedUserDefinedFilterStrip_WithInsufficientSecurity_ShouldShowError()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "Komono You Didn't!", true, false, true);
			EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed = false;

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, true))
			{
				form.Show();
				Application.DoEvents();

				AssertLayoutList(form, "Komono You Didn't! [+]");

				SelectItem(form, "Komono You Didn't! [+]");
				form.DeleteSelectedLayoutExposed();

				AssertEquals("The selected layout is available to all users and can only be modified by an administrator.", UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form, "Komono You Didn't! [+]");

				UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
				EnvProxy.Instance.Security.PublishUserDefinedFilters.IsAllowed = true;
				form.DeleteSelectedLayoutExposed();
				AssertNull(UnitTestUserNotification.Instance.LastMessage.Text);
				AssertLayoutList(form); // empty
			}
		}

		public void TestDelete_KeepsExpandedStates()
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "A/A1/A2", false, false, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "B/B1", false, false, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "B/B1/B2/B3", false, false, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "C/C1", false, false, false);
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "C/C2", false, false, false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				form.Show();
				Application.DoEvents();

				form.FiltersTreeView.ExpandAll();
				CombineAssertions(() =>
				{
					AssertEquals(7, form.treeNodesExpandedStates.Count);
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node A is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
					Assert("Node A1 is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].Nodes[0].IsExpanded);
					Assert("Node B is expanded", form.FiltersTreeView.Nodes[0].Nodes[1].IsExpanded);
					Assert("Node B1 is expanded", form.FiltersTreeView.Nodes[0].Nodes[1].Nodes[0].IsExpanded);
					Assert("Node B2 is expanded", form.FiltersTreeView.Nodes[0].Nodes[1].Nodes[0].Nodes[0].IsExpanded);
					Assert("Node C is expanded", form.FiltersTreeView.Nodes[0].Nodes[2].IsExpanded);
				});

				SelectItem(form, "A2");
				form.DeleteSelectedLayoutExposed();
				CombineAssertions(() =>
				{
					AssertEquals("The whole A should have been removed from the list as there is no other node in A tree", 5, form.treeNodesExpandedStates.Count);
					Assert(!form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A\A1"));
					Assert(!form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A"));
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node B is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
					Assert("Node B1 is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].Nodes[0].IsExpanded);
					Assert("Node B2 is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].Nodes[0].Nodes[0].IsExpanded);
					Assert("Node C is expanded", form.FiltersTreeView.Nodes[0].Nodes[1].IsExpanded);
				});

				SelectItem(form, "B3");
				form.DeleteSelectedLayoutExposed();
				CombineAssertions(() =>
				{
					AssertEquals("The B should not be removed from the list as there is node B1 in the tree", 3, form.treeNodesExpandedStates.Count);
					Assert(!form.treeNodesExpandedStates.Contains(@"My Filter Layouts\B\B1\B2"));
					Assert(!form.treeNodesExpandedStates.Contains(@"My Filter Layouts\B\B1"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts\B"));
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node B is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
					Assert("Node C is expanded", form.FiltersTreeView.Nodes[0].Nodes[1].IsExpanded);
				});

				SelectItem(form, "B1");
				form.DeleteSelectedLayoutExposed();
				CombineAssertions(() =>
				{
					AssertEquals("The whole B should have been removed from the list as there is no other node in B tree", 2, form.treeNodesExpandedStates.Count);
					Assert(!form.treeNodesExpandedStates.Contains(@"My Filter Layouts\B"));
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node C is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
				});

				SelectItem(form, "C1");
				form.DeleteSelectedLayoutExposed();
				CombineAssertions(() =>
				{
					AssertEquals("The C should not be removed from the list as there is node C2 in the tree", 2, form.treeNodesExpandedStates.Count);
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts\C"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts"));
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node C is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
				});

				SelectItem(form, "C2");
				form.DeleteSelectedLayoutExposed();
				CombineAssertions(() =>
				{
					AssertEquals("The whole C should have been removed from the list as there is no other node in C tree", 0, form.treeNodesExpandedStates.Count);
				});
			}
		}

		public void TestRename_ToSameLevel_KeepsExpandedStates()
		{
			PrepareFormAndRunRenameAssertions((TestManageLayoutsForm form) =>
			{
				SelectItem(form, "A2");
				form.NewLayoutNameForRenaming = "A/A1/A3";
				form.RenameSelectedLayoutExposed();
				CombineAssertions(() =>
				{
					AssertEquals("Stay the same", 3, form.treeNodesExpandedStates.Count);
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A\A1"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts"));
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node A is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
					Assert("Node A1 is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].Nodes[0].IsExpanded);
				});
			});
		}

		public void TestRename_ToParentLevel_KeepsExpandedStates()
		{
			PrepareFormAndRunRenameAssertions((TestManageLayoutsForm form) =>
			{
				SelectItem(form, "A2");
				form.NewLayoutNameForRenaming = "A/A1";
				form.RenameSelectedLayoutExposed();
				CombineAssertions(() =>
				{
					AssertEquals("Node A1 should have been removed form the list as A1 has no child node", 2, form.treeNodesExpandedStates.Count);
					Assert(!form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A\A1"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts"));
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node A is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
				});
			});
		}

		public void TestRename_ToChildLevel_KeepsExpandedStates()
		{
			PrepareFormAndRunRenameAssertions((TestManageLayoutsForm form) =>
			{
				SelectItem(form, "A2");
				form.NewLayoutNameForRenaming = "A/A1/A2/A3";
				form.RenameSelectedLayoutExposed();
				CombineAssertions(() =>
				{
					AssertEquals("Node A2 should have been added to the list as A2 has a child node", 4, form.treeNodesExpandedStates.Count);
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A\A1\A2"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A\A1"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts"));
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node A is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
					Assert("Node A1 is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].Nodes[0].IsExpanded);
					Assert("Node A2 is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].Nodes[0].Nodes[0].IsExpanded);
				});
			});
		}

		public void TestRename_ToOtherNode_KeepsExpandedStates()
		{
			PrepareFormAndRunRenameAssertions((TestManageLayoutsForm form) =>
			{
				SelectItem(form, "A2");
				form.NewLayoutNameForRenaming = "B/B1";
				form.RenameSelectedLayoutExposed();
				CombineAssertions(() =>
				{
					AssertEquals("The whole A should have been removed from the list as A/A1/A2 has been renamed to B/B1", 2, form.treeNodesExpandedStates.Count);
					Assert(!form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A\A1"));
					Assert(!form.treeNodesExpandedStates.Contains(@"My Filter Layouts\A"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts\B"));
					Assert(form.treeNodesExpandedStates.Contains(@"My Filter Layouts"));
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node B is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
				});
			});
		}

		void PrepareFormAndRunRenameAssertions(Action<TestManageLayoutsForm> action)
		{
			FilterStripsTestHelper.SaveFilterLayout(DummyModuleIDs.Dummy, "A/A1/A2", false, false, false);

			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(DummyModuleIDs.Dummy))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				form.Show();
				Application.DoEvents();

				form.FiltersTreeView.ExpandAll();
				CombineAssertions(() =>
				{
					AssertEquals(3, form.treeNodesExpandedStates.Count);
					Assert("Node My Filter Layouts is expanded", form.FiltersTreeView.Nodes[0].IsExpanded);
					Assert("Node A is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].IsExpanded);
					Assert("Node A1 is expanded", form.FiltersTreeView.Nodes[0].Nodes[0].Nodes[0].IsExpanded);
				});

				action(form);
			}
		}

		public void TestSystemDefinedLayoutAreGreyedOut()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				form.Show();
				Application.DoEvents();

				SelectItem(form, "Shipment Routing Board");
				AssertEquals(Color.Gray, form.FiltersTreeView.SelectedNode.ForeColor);
			}
		}

		public void TestRenameAndDeleteButtonsAreDisabledForSystemDefinedLayouts()
		{
			using (var module = (ZFilterGridModule)ZModuleFactory.Instance.Create(ModuleIDs.JobShipment))
			using (var form = new TestManageLayoutsForm(module.FilterBusinessObject, true, true, false))
			{
				form.Show();
				SelectItem(form, "Shipment Routing Board");
				Application.DoEvents();

				Assert(!form.DeleteLayoutButtonExposed.Enabled);
				Assert(!form.RenameLayoutButtonExposed.Enabled);
				Assert(!form.SaveColumnsCheckBox.Enabled);
				Assert(!form.SaveGridColourCheckBox.Enabled);
				Assert(!form.SaveGridColourNameTextBox.Enabled);
			}
		}

		void AssertRenameIsNotAllowedForDefaultLayout(TestManageLayoutsForm manageForm)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			manageForm.FiltersTreeViewExposed.SelectedNode = manageForm.FiltersTreeViewExposed.Nodes[2];

			manageForm.RenameSelectedLayoutExposed();

			AssertEquals("The selected layout cannot be renamed.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("SaveButton stays as disabled", false, manageForm.SaveButtonExposed.Enabled);
			AssertEquals("SaveCloseButton stays as disabled", false, manageForm.SaveCloseButtonExposed.Enabled);
		}

		void AssertDeleteIsNotAllowedForDefaultLayout(TestManageLayoutsForm manageForm)
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();

			manageForm.FiltersTreeViewExposed.SelectedNode = manageForm.FiltersTreeViewExposed.Nodes[2];

			manageForm.DeleteSelectedLayoutExposed();

			AssertEquals("The selected layout cannot be deleted.", UnitTestUserNotification.Instance.LastMessage.Text);
			AssertEquals("SaveButton stays as disabled", false, manageForm.SaveButtonExposed.Enabled);
			AssertEquals("SaveCloseButton stays as disabled", false, manageForm.SaveCloseButtonExposed.Enabled);
		}

		internal class TestManageLayoutsForm : ManageLayoutsForm
		{
			public TestManageLayoutsForm(IModifyModuleAndGridLayout layoutManageable, bool shouldShowSaveColumnsCheckBox, bool shouldSaveGridColourCheckBox, bool shouldShowUserDefinedFilter, GridColourScheme gridLastUsedColourScheme = null)
				: base(layoutManageable, shouldShowSaveColumnsCheckBox, shouldSaveGridColourCheckBox, shouldShowUserDefinedFilter, gridLastUsedColourScheme)
			{
			}

			public void SelectListItem(string listItemText)
			{
				FiltersTreeViewExposed.SelectedNode = FiltersTreeViewExposed.Nodes.Cast<TreeNode>().Single(item => item.Text == listItemText);
			}

			public ZTreeView FiltersTreeViewExposed
			{
				get { return FiltersTreeView; }
			}

			public ZButton SaveCloseButtonExposed
			{
				get { return this.SaveCloseButton; }
			}

			public ZButton SaveButtonExposed
			{
				get { return this.SaveButton; }
			}

			public ZToolStripButton RenameLayoutButtonExposed
			{
				get { return this.RenameLayoutButton; }
			}

			public ZToolStripButton DeleteLayoutButtonExposed
			{
				get { return this.DeleteLayoutButton; }
			}

			public ZButton CancelButtonExposed => CancelChangesButton;

			public void RenameSelectedLayoutExposed()
			{
				RenameSelectedLayout();
			}

			public void DeleteSelectedLayoutExposed()
			{
				DeleteSelectedLayout();
			}

			public void OnShownExposed()
			{
				this.OnShown(EventArgs.Empty);
			}

			protected override ZString RequestLayoutNameFromUser()
			{
				return NewLayoutNameForRenaming ?? base.RequestLayoutNameFromUser();
			}

			public string NewLayoutNameForRenaming { get; set; }
		}
	}
}
