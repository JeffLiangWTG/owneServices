using System.Linq;
using System.Windows.Forms;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;

namespace Enterprise.BufferManagement.GUI.Test
{
	class MoveToComponentMenuItemProviderTest : BMSTestCaseWithFactory
	{
		#region MoveToComponent

		public void TestMoveToComponent_SingleWorkflow()
		{
			workflow1.FH_FC_CurrentComponent = buffer2.PK;

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new ProcessHeader[] { workflow1 };
			module.GridCollection.Add(workflow1);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var moveToComponentMenu = (MoveToComponentMenuItemMenuTree)menuItems[0];
			moveToComponentMenu.OnPopup();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformMoveToComponentClick(moveToComponentMenu, bucket1);
			AssertEquals(bucket1.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move workflow Organization (XVBQP68SIYXQ) - Job Workflow from component [Buffer2] to component [Bucket1]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformMoveToComponentClick(moveToComponentMenu, buffer1);
			AssertEquals(buffer1.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move workflow Organization (XVBQP68SIYXQ) - Job Workflow from component [Bucket1] to component [Buffer1]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformMoveToComponentClick(moveToComponentMenu, buffer2);
			AssertEquals(buffer2.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move workflow Organization (XVBQP68SIYXQ) - Job Workflow from component [Buffer1] to component [Buffer2]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMoveToComponent_MultipleWorkflows()
		{
			workflow1.FH_FC_CurrentComponent = workflow2.FH_FC_CurrentComponent = workflow3.FH_FC_CurrentComponent = buffer2.PK;

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new ProcessHeader[] { workflow1, workflow2, workflow3 };
			module.GridCollection.AddRange(new[] { workflow1, workflow2, workflow3 });

			var menuItems = provider.GetMenuItems(module).ToArray();
			var moveToComponentMenu = (MoveToComponentMenuItemMenuTree)menuItems[0];
			moveToComponentMenu.OnPopup();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformMoveToComponentClick(moveToComponentMenu, bucket1);
			AssertEquals(bucket1.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(bucket1.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(bucket1.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move 3 workflows to component [Bucket1]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformMoveToComponentClick(moveToComponentMenu, buffer1);
			AssertEquals(buffer1.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer1.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(buffer1.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move 3 workflows to component [Buffer1]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformMoveToComponentClick(moveToComponentMenu, buffer2);
			AssertEquals(buffer2.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer2.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(buffer2.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move 3 workflows to component [Buffer2]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMoveToComponent_JobHeaderWithMultipleWorkflows()
		{
			workflow1.FH_FC_CurrentComponent = workflow2.FH_FC_CurrentComponent = workflow3.FH_FC_CurrentComponent = buffer2.PK;

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new ProcessHeader[] { jobHeader };
			module.GridCollection.Add(jobHeader);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var moveToComponentMenu = (MoveToComponentMenuItemMenuTree)menuItems[0];
			moveToComponentMenu.OnPopup();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformMoveToComponentClick(moveToComponentMenu, bucket1);
			AssertEquals(bucket1.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(bucket1.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(bucket1.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move 3 workflows to component [Bucket1]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformMoveToComponentClick(moveToComponentMenu, buffer1);
			AssertEquals(buffer1.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer1.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(buffer1.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move 3 workflows to component [Buffer1]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
			PerformMoveToComponentClick(moveToComponentMenu, buffer2);
			AssertEquals(buffer2.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer2.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(buffer2.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move 3 workflows to component [Buffer2]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		public void TestMoveToComponent_JobHeaderWithMultipleWorkflows_UserCancelled()
		{
			workflow1.FH_FC_CurrentComponent = workflow2.FH_FC_CurrentComponent = workflow3.FH_FC_CurrentComponent = buffer2.PK;

			Factory.Save();

			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new ProcessHeader[] { jobHeader };
			module.GridCollection.Add(jobHeader);

			var menuItems = provider.GetMenuItems(module).ToArray();
			var moveToComponentMenu = (MoveToComponentMenuItemMenuTree)menuItems[0];
			moveToComponentMenu.OnPopup();

			UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
			PerformMoveToComponentClick(moveToComponentMenu, bucket1);
			AssertEquals(buffer2.PK, workflow1.FH_FC_CurrentComponent);
			AssertEquals(buffer2.PK, workflow2.FH_FC_CurrentComponent);
			AssertEquals(buffer2.PK, workflow3.FH_FC_CurrentComponent);
			AssertEquals("Are you sure to move 3 workflows to component [Bucket1]? Note that based on transfer rules, your workflows may move back to their original components.", UnitTestUserNotification.Instance.LastMessage.Text);
		}

		#endregion

		#region MenuItems

		public void TestMenuItems()
		{
			module.IDOverride = ModuleIDs.ProcessHeader;
			module.SelectedBusinessObjectsOverride = new ProcessHeader[] { workflow1 };

			var menuItems = provider.GetMenuItems(module).ToArray();
			AssertEquals(1, menuItems.Length);

			var moveToComponentMenu = (MoveToComponentMenuItemMenuTree)menuItems[0];
			moveToComponentMenu.OnPopup();

			AssertCollectionContains(moveToComponentMenu.MenuItems.Cast<ZMenuItem>(), m => m.Text == bucket1.FC_Name);
			AssertCollectionContains(moveToComponentMenu.MenuItems.Cast<ZMenuItem>(), m => m.Text == buffer1.FC_Name);
			AssertCollectionContains(moveToComponentMenu.MenuItems.Cast<ZMenuItem>(), m => m.Text == buffer2.FC_Name);
		}

		#endregion

		#region Implementation

		static void PerformMoveToComponentClick(MoveToComponentMenuItemMenuTree menuItem, BMComponent component)
		{
			menuItem.MenuItems.Cast<ZMenuItem>().Single(m => m.Text == component.FC_Name).PerformClick();
		}

		protected override void SetUp()
		{
			base.SetUp();
			module = new DummyModuleForVisualBoards();
			provider = new MoveToComponentMenuItemProvider();
			system = BMSTestHelper.CreateSystem(Factory, "ORG");
			bucket1 = BMSTestHelper.CreateBucket(system, "Bucket1");
			buffer1 = BMSTestHelper.CreateBuffer(system, "Buffer1");
			buffer2 = BMSTestHelper.CreateBuffer(system, "Buffer2");
			jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory);
			workflow1 = jobHeader.ProcessHeaders[0];
			workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");
			workflow3 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow3");
		}

		protected override void TearDown()
		{
			base.TearDown();
			module.Dispose();
		}

		IFilterGridMenuItemProvider provider;
		DummyModuleForVisualBoards module;
		BMSystem system;
		BMComponent bucket1;
		BMComponent buffer1;
		BMComponent buffer2;
		ProcessJobHeader jobHeader;
		ProcessHeader workflow1;
		ProcessHeader workflow2;
		ProcessHeader workflow3;

		#endregion
	}
}
