using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Integration;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class EDIWorkflowUserControlTest : TestCaseWithFactory
	{
		public void TestFilterControlForWorkflowOnlyLayout_WorkItem()
		{
			var workItem = Factory.New<NewWorkItem>();
			using (var form = new ZForm(workItem))
			using (var workflowControl = new EDIWorkflowUserControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(workflowControl);
				form.Show();
				var tabControl = (ZTabControl)workflowControl.Controls[0];
				var filterTab = tabControl.TabPages.Cast<ZTabPage>().SingleOrDefault(x => x is EDITaskWithDetailsAndFilterTab);
				AssertEquals("Should be Work Item specific type", typeof(EDITaskWithDetailsAndFilterTab), filterTab.GetType());
				AssertEquals("Should be Work Item specific type", typeof(EDITaskWithDetailsAndFilterControl), ((EDITaskWithDetailsAndFilterTab)filterTab).FilterControl.GetType());
			}
		}

		public void TestFilterControlForWorkflowOnlyLayout_Other()
		{
			var supportIncident = Factory.New<SupportIncident>();
			using (var form = new ZForm(supportIncident))
			using (var workflowControl = new EDIWorkflowUserControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(workflowControl);
				form.Show();
				var tabControl = (ZTabControl)workflowControl.Controls[0];
				var filterTab = tabControl.TabPages.Cast<ZTabPage>().SingleOrDefault(x => x is TaskWithDetailsAndFilterTab);
				AssertEquals("Should be standard task tab", typeof(TaskWithDetailsAndFilterTab), filterTab.GetType());
			}
		}

		public void TestWorkflowTaskDetailsControlContainsGitPullRequestMenuItem()
		{
			var processJobHeaderProviderMock = new Mock<IProcessJobHeaderProvider>();
			processJobHeaderProviderMock.Setup(x => x.BufferManagementEnabledForWorkflowProvider(It.IsAny<IWorkflowProviderCore>(), It.IsAny<BusinessObjectFactory>())).Returns(true);
			ITaskDetailsControl attachedITaskDetailsControl = null;
			var menuItemMock = new Mock<ITaskDetailsMenuItem>();
			menuItemMock.Setup(x => x.AttachToTaskDetailsControl(It.IsAny<ITaskDetailsControl>())).Callback((ITaskDetailsControl control) =>
			{
				AssertNotNull(control);
				attachedITaskDetailsControl = control;
			});
			var jobHeader = Factory.NewWithValidTestData<OrgHeader>();
			var workItem = Factory.New<NewWorkItem>();
			var task = jobHeader.WorkflowItems.AddNew();
			Factory.Save();
			using (ObjectFactory.Substitute(processJobHeaderProviderMock.Object))
			using (var form = new ZForm(workItem))
			using (var workflowControl = new EDIWorkflowUserControl())
			{
				form.ControllerID = DummyControllerIDs.Dummy;
				workflowControl.GetTaskDetailsMenuItemMethod = () => menuItemMock.Object;
				form.Controls.Add(workflowControl);
				form.Show();
				var tabControl = (ZTabControl)workflowControl.Controls[0];
				var workflowManagementTabPage = tabControl.TabPages.Cast<ZTabPage>().SingleOrDefault(x => x is IWorkflowManagementTabPage);
				CombineAssertions(() =>
				{
					AssertNotNull(workflowManagementTabPage);
					Assert(workflowManagementTabPage is ITaskDetailsMenuStripHostControl);
					AssertNotNull(attachedITaskDetailsControl);
				});
			}
		}

		protected override void SetUp()
		{
			base.SetUp();
			ObjectFactory.Get<IBMSRegistry>().AlwaysViewWorkflowManagementTab = true;
		}
	}
}
