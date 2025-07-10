using System.Linq;
using System.Windows.Forms;
using CargoWise.Common;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.VisualBoards.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class VisualBoardDependencyMenuItemTest : DependencyMenuItemBaseTest
	{
		#region DependencyMenuItemBaseTest Overrides

		protected override bool ShouldSaveAfterActions => true;
		protected override bool ShouldUpdateNumberOfOpenPrereqsAfterActions => false;

		protected override string InvalidLink_ImmediateErrorMessage_StandardDirection => @"This link is invalid so cannot be added.

Error - FP_FH_HeaderFrom: This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (DIDNOTHING) - and snap!
Organization (DIDNOTHING) - You bend...
Organization (WRONGSYD) - it works every time!
Organization (WRONGSYD) - Job Organization (H5ZX52PAMCOI) is complete.
Organization (WRONGSYD) - The bend and snap
Error - FP_FH_HeaderFrom: This is a dependency link between workflows that are also involved in a Parent-Child relationship.
Error - FP_FH_HeaderTo: This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (DIDNOTHING) - and snap!
Organization (DIDNOTHING) - You bend...
Organization (WRONGSYD) - it works every time!
Organization (WRONGSYD) - Job Organization (H5ZX52PAMCOI) is complete.
Organization (WRONGSYD) - The bend and snap
Error - FP_FH_HeaderTo: This is a dependency link between workflows that are also involved in a Parent-Child relationship.";

		protected override string InvalidLink_ImmediateErrorMessage_OppositeDirection => @"This link is invalid so cannot be added.

Error - FP_FH_HeaderFrom: This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (DIDNOTHING) - and snap!
Organization (DIDNOTHING) - Job Organization (XVBQP68SIYXQ) is complete.
Organization (DIDNOTHING) - You bend...
Organization (WRONGSYD) - it works every time!
Organization (WRONGSYD) - The bend and snap
Error - FP_FH_HeaderFrom: This is a dependency link between workflows that are also involved in a Parent-Child relationship.
Error - FP_FH_HeaderTo: This link is part of a looped dependency. The following workflows are involved in a loop:
Organization (DIDNOTHING) - and snap!
Organization (DIDNOTHING) - Job Organization (XVBQP68SIYXQ) is complete.
Organization (DIDNOTHING) - You bend...
Organization (WRONGSYD) - it works every time!
Organization (WRONGSYD) - The bend and snap
Error - FP_FH_HeaderTo: This is a dependency link between workflows that are also involved in a Parent-Child relationship.";

		protected override ZToolStripMenuItem GetMenuItem(ProcessHeader workflow, string menuItemName)
		{
			return GetMenuItem(workflow, menuItemName, disposables, config);
		}

		#endregion

		#region Implementation

		internal static ZToolStripMenuItem GetMenuItem(ProcessHeader workflow, string menuItemName, DisposableList disposables, VisualBoardTestConfig config)
		{
			disposables.Add(DisableAsyncBehaviour());

			TaskCardControl.AutoGenerateTaskMenuItems.Value = true;

			var board = config.BucketBoard;

			workflow.Factory.Save();

			var form = new VisualBoardForm(BMSTestHelper.CreateSlideshowViewModel(board));

			disposables.Add(form);

			form.Show();
			Application.DoEvents();

			var task = workflow.Tasks.Single();
			var taskTicket = BMSGUITestCase.FindTaskCardControl(form, task);

			var menuItem = taskTicket.ContextMenuStrip.Items.OfType<ZToolStripMenuItem>().Single(i => i.Text == menuItemName);

			menuItem.ShowDropDown();
			menuItem.HideDropDown();

			return menuItem;
		}

		#endregion
	}

	class VisualBoardParentChildMenuItemTest : ParentChildMenuItemBaseTest
	{
		#region ParentChildMenuItemBaseTest Overrides

		protected override bool ShouldSaveAfterActions => true;
		protected override bool ShouldUpdateNumberOfOpenPrereqsAfterActions => false;

		protected override ZToolStripMenuItem GetMenuItem(ProcessHeader workflow, string menuItemName)
		{
			return VisualBoardDependencyMenuItemTest.GetMenuItem(workflow, menuItemName, disposables, config);
		}

		#endregion
	}
}
