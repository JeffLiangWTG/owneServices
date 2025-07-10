using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.GUI;
using Enterprise.VisualBoards.GUI.Test;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardFormShowCurrentItemsTest : BMSGUITestCase
	{
		public void TestRefresh_ShowCurrentItemsFilterByDefault_IsTrue()
		{
			AssertAction(
				ActionType.Refresh,
				showCurrentItemsFilterByDefault: true,
				expectedCurrentItemsOnShowForm: true,
				expectedCurrentItemsOnAction: true,
				expectedCurrentItemsOnTogglingCheckbox: false, // When toggling and refresh should keep toggle value
				expectedCurrentItemsOnRetogglingCheckbox: true);
		}

		public void TestRefresh_ShowCurrentItemsFilterByDefault_IsFalse()
		{
			AssertAction(
				ActionType.Refresh,
				showCurrentItemsFilterByDefault: false,
				expectedCurrentItemsOnShowForm: false,
				expectedCurrentItemsOnAction: false,
				expectedCurrentItemsOnTogglingCheckbox: true, // When toggling and refresh should keep toggle value
				expectedCurrentItemsOnRetogglingCheckbox: false);
		}

		public void TestReload_ShowCurrentItemsFilterByDefault_IsTrue()
		{
			AssertAction(
				ActionType.Reload,
				showCurrentItemsFilterByDefault: true,
				expectedCurrentItemsOnShowForm: true,
				expectedCurrentItemsOnAction: true,
				expectedCurrentItemsOnTogglingCheckbox: true, // When toggling and reload should keep configuration value
				expectedCurrentItemsOnRetogglingCheckbox: true);
		}

		public void TestReload_ShowCurrentItemsFilterByDefault_IsFalse()
		{
			AssertAction(
				ActionType.Reload,
				showCurrentItemsFilterByDefault: false,
				expectedCurrentItemsOnShowForm: false,
				expectedCurrentItemsOnAction: false,
				expectedCurrentItemsOnTogglingCheckbox: false,  // When toggling and reload should keep configuration value
				expectedCurrentItemsOnRetogglingCheckbox: false);
		}

		#region Implementation

		void AssertAction(
			ActionType actionType,
			bool showCurrentItemsFilterByDefault,
			bool expectedCurrentItemsOnShowForm,
			bool expectedCurrentItemsOnAction,
			bool expectedCurrentItemsOnTogglingCheckbox,
			bool expectedCurrentItemsOnRetogglingCheckbox)
		{
			var config = SetupTest(showCurrentItemsFilterByDefault);

			Factory.Save();

			using (var form = new VisualBoardForm(VisualBoardFormBasherTest.GetViewModel(config.BufferSection.Board)))
			{
				form.Show();
				Application.DoEvents();

				AssertShowCurrentItems($"WHEN SHOW form", form, expectedCurrentItemsOnShowForm);

				RefreshOrReload(actionType, form);
				AssertShowCurrentItems($"WHEN {actionType.ToString()}", form, expectedCurrentItemsOnAction);

				ClickShowCurrentItems(form);
				RefreshOrReload(actionType, form);
				AssertShowCurrentItems($"WHEN toggle ShowCurrenItem menu-item and {actionType.ToString()}", form, expectedCurrentItemsOnTogglingCheckbox);

				ClickShowCurrentItems(form);
				RefreshOrReload(actionType, form);
				AssertShowCurrentItems($"WHEN retoggle ShowCurrentItem menu-item and {actionType.ToString()}", form, expectedCurrentItemsOnRetogglingCheckbox);
			}
		}

		void AssertShowCurrentItems(string message, VisualBoardForm form, bool expectedShowCurrentItemsChecked)
		{
			var componentControl = form.FindAll<BMComponentControl>().Single();

			var taskPanel = form.FindAll<TaskPanel>().First();
			var contextMenu = (LazyContextMenuStrip)taskPanel.ContextMenuStrip;
			contextMenu.AddItems_ForTest(taskPanel);
			AssertNotNull("Precondition: contextMenu", contextMenu);

			var menuItem = contextMenu.Items[0];

			var extraMessage = expectedShowCurrentItemsChecked
				? "should only show current item"
				: "should show all items";

			CombineAssertions($"{message} THEN {extraMessage}", () =>
			{
				AssertEquals("Menu-item should show be 'Show Startable Items'", "Show Startable Items", menuItem.Text);
				AssertEquals($"ShowCurrentItems should be {expectedShowCurrentItemsChecked}", expectedShowCurrentItemsChecked, ((ZToolStripMenuItem)menuItem).Checked);

				var taskCards = form
					.FindAll<TaskCardControl>()
					.Where(taskCardControl => taskCardControl.Visible)
					.ToArray();

				if (expectedShowCurrentItemsChecked)
				{
					AssertEquals("Total cards", 2, taskCards.Length);

					var expectedCards = new[]
					{
						"Task: [Task Prereq Workflow]; Workflow: [Prereq Workflow]",
						"Task: [Task Workflow]; Workflow: [Workflow]"
					};
					AssertContainsExactElementsInAnyOrder(expectedCards, taskCards.Select(taskCard => taskCard.CardContent.DisplayTextForDebugging));
				}
				else
				{
					AssertEquals("Total cards", 3, taskCards.Length);

					var expectedCards = new[]
					{
						"Task: [Task Prereq Workflow]; Workflow: [Prereq Workflow]",
						"Task: [Task Postreq Workflow]; Workflow: [Postreq Workflow]",
						"Task: [Task Workflow]; Workflow: [Workflow]"
					};
					AssertContainsExactElementsInAnyOrder(expectedCards, taskCards.Select(taskCard => taskCard.CardContent.DisplayTextForDebugging));
				}
			});
		}

		void RefreshOrReload(ActionType actionType, VisualBoardForm form)
		{
			switch (actionType)
			{
				case ActionType.Refresh:
					form.RefreshBoard();
					break;
				case ActionType.Reload:
					form.ReloadBoard();
					break;
			}

			Application.DoEvents();
		}

		void ClickShowCurrentItems(VisualBoardForm form)
		{
			var componentControl = form.FindAll<BMComponentControl>().Single();
			var currentTaskMenuItem = componentControl.ContextMenuStrip.Items[0];
			AssertEquals("Show Startable Items", currentTaskMenuItem.Text);
			currentTaskMenuItem.PerformClick();
		}

		VisualBoardTestConfig SetupTest(bool enableShowCurrentItemsFilterByDefault)
		{
			var config = TestConfigsHelper.CreateVisualBoardTestConfig(Factory);
			BMSTestHelper.CreatePrimaryChannelForSection(config.BufferSection, ChannelTypeList.Codes.Resource, GlbStaff.CurrentUser.PK, overrideChannels: true);
			config.BufferSection.SectionConfiguration.CardType = CardTypeList.Codes.Workflow;

			config.BufferSection.SectionConfiguration.EnableShowCurrentItemsFilterByDefault = enableShowCurrentItemsFilterByDefault;

			var prereqJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Prereq Job");
			var prereqWorkflow = BMSTestHelper.CreateProcessHeaderAndTask(prereqJobHeader, config.Buffer, 1, GlbStaff.CurrentUser.GS_Code, completionStatement: "Prereq Workflow");
			var postreqJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Postreq Job");
			var postreqWorkflow = BMSTestHelper.CreateProcessHeaderAndTask(postreqJobHeader, config.Buffer, 1, GlbStaff.CurrentUser.GS_Code, completionStatement: "Postreq Workflow");
			prereqJobHeader.MakePrerequisiteOf(postreqJobHeader);

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, description: "Job");
			var workflow = BMSTestHelper.CreateProcessHeaderAndTask(jobHeader, config.Buffer, 1, GlbStaff.CurrentUser.GS_Code, completionStatement: "Workflow");

			return config;
		}

		protected override bool ShouldDisableAsyncBehaviour => true;

		enum ActionType { Refresh, Reload }

		#endregion
	}
}
