using System;
using System.Collections.Generic;
using System.Windows.Forms;
using CargoWise.Windows.UI;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Modules;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.GUI;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	[TestedType(typeof(EDIProjectForm))]
	internal sealed class EDIProjectFormTest : ZFormBasherTest
	{
		public void TestRelatedItemsTabShouldContainUnifiedControlsForRelatedItems()
		{
			using (var form = (EDIProjectForm)GetFormToBashCore())
			{
				form.Show();
				var relatedTab = form.TopLevelTabControl_Exposed.GetTabPageByNameOrText("Related Items");
				AssertNotNull(relatedTab);
				form.TopLevelTabControl_Exposed.SelectedTab = relatedTab;
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("RelatedItemGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("NetworkDiagramGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ParentWorkflowGroupBox"));
				AssertNotNull(relatedTab.FindSingleOrDefault<ZGroupBox>("ChildWorkflowGroupBox"));
			}
		}

		public void TestDisableActionMenuWhenViewOrDelete()
		{
			var project = Factory.NewWithValidTestData<EDIProject>();
			project.WKP_Type = project.InstallationProjectCode; // to display all the EDI actions
			Factory.Save();
			using (var viewForm = new EDIProjectFormForTest(project))
			{
				viewForm.DisplayMode = ODisplayMode.ReadOnly;
				viewForm.Show();
				AssertCustomisedActionsMenuAbility(false, viewForm);
			}

			using (var editForm = new EDIProjectFormForTest(project))
			{
				editForm.DisplayMode = ODisplayMode.Edit;
				editForm.Show();
				AssertCustomisedActionsMenuAbility(true, editForm);
			}

			using (var deleteForm = new EDIProjectFormForTest(project))
			{
				deleteForm.DisplayMode = ODisplayMode.Delete;
				deleteForm.Show();
				AssertCustomisedActionsMenuAbility(false, deleteForm);
			}
		}

		void AssertCustomisedActionsMenuAbility(bool expected, EDIProjectFormForTest form)
		{
			CombineAssertions(() =>
			{
				foreach (var menuItem in form.ProjectAction_Exposed)
				{
					AssertEquals(string.Format("{0}'s {1} is enabled", form.Text, menuItem.Text), expected, menuItem.Enabled);
				}
			});
		}

		public void TestActionsMenu()
		{
			var project = Factory.New<EDIProject>();
			project.WKP_Type = project.InstallationProjectCode;
			using (var form = new EDIProjectForm(project))
			{
				form.Show();
				MenuItem item = GetActionsMenuItem(form, "Add to &Log");
				item.PerformClick();
				AssertEquals(typeof(AddProjectLogPopupForm), ZFormModaliser.LastFormShownDialogForTest.GetType());
				((ZForm)ZFormModaliser.LastFormShownDialogForTest).Dispose();
				ZFormModaliser.LastFormShownDialogForTest = null;
				ZFormModaliser.LastIBusinessShownOnDialogForTest = null;
				item = GetActionsMenuItem(form, EmailSender.GetSendEmailMenuItemTextForTest());
				AssertNotNull("Send E-mail", item);
				item = GetActionsMenuItem(form, "Begin &Pre-Install");
				item.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				item = GetActionsMenuItem(form, "&Complete Pre-Install");
				item.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				item = GetActionsMenuItem(form, "Complete &Install");
				item.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				item = GetActionsMenuItem(form, "Begin &Training");
				item.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
				item = GetActionsMenuItem(form, "Complete T&raining");
				item.PerformClick();
				AssertNull(ZFormModaliser.LastFormShownDialogForTest);
			}
		}

		public void TestNoDuplicateShortcutInActionMenu()
		{
			var project = Factory.New<EDIProject>();
			List<Shortcut> alreadyUsedShortcuts = new List<Shortcut>();
			using (var form = new EDIProjectForm(project))
			{
				MenuItem actionsMenu = GetActionsMenu(form);
				foreach (MenuItem actionItem in actionsMenu.MenuItems)
				{
					bool isShortcutAlreadyUsed = actionItem.ShowShortcut && actionItem.Shortcut != Shortcut.None && alreadyUsedShortcuts.Contains(actionItem.Shortcut);
					AssertEquals("The Shortcut for the menu item Action->" + actionItem.Text + " is already used.", false, isShortcutAlreadyUsed);
					alreadyUsedShortcuts.Add(actionItem.Shortcut);
				}
			}
		}

		public void TestFormHasLicencePlugIn()
		{
			var project = Factory.New<EDIProject>();
			using (var form = new EDIProjectForm(project))
			{
				AssertNotNull("The form should contain the Licence PlugIn", form.PlugIns.GetPlugIn(ClientControllerRegistration.ProjectClientOrgLicence));
			}
		}

		MenuItem GetActionsMenuItem(Form form, string text)
		{
			MenuItem actionsMenu = GetActionsMenu(form);
			actionsMenu.OnPopup(EventArgs.Empty);
			foreach (MenuItem actionItem in actionsMenu.MenuItems)
			{
				if (actionItem.Text == text && actionItem.Visible)
				{
					return actionItem;
				}
			}

			return null;
		}

		MenuItem GetActionsMenu(Form form)
		{
			foreach (MenuItem item in form.Menu.MenuItems)
			{
				if (item.Text == "Actio&ns")
				{
					return item;
				}
			}

			return null;
		}

		#region Implementation
		protected override Form GetFormToBashCore()
		{
			var project = Factory.New<EDIProject>();
			var form = new EDIProjectForm(project);
			form.ControllerID = ControllerIDs.Project;
			return form;
		}
		#endregion
	}
}
