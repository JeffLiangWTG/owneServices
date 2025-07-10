using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI.Workflow;
using Enterprise.Client.EDI.TfsRest;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules.Testing;
using Moq;

namespace Enterprise.Client.EDI.IncidentManager.GUI.Testing
{
	class EDITaskWithDetailsAndFilterControlTest : TestCaseWithFactory
	{
		public void TestSetupSkillsTab()
		{
			using (ZForm form = new ZForm())
			using (EDITaskWithDetailsAndFilterControl control = new EDITaskWithDetailsAndFilterControl())
			{
				AssertEquals("SkillsControl.BindTo must be set to TasksView", "TasksView", control.SkillsControl.BindTo);
				AssertNotNull("SkillsControl.CreateSkillLearningTaskFunction should be assigned", control.SkillsControl.CreateSkillLearningTaskFunction);
			}
		}

		public void TestSetupNoteContextMenu()
		{
			AddTeamProjectCollection("http://tfs.wtg.zone:8080/tfs/CargoWise");
			using (ZForm form = new ZForm())
			{
				var staff1 = Factory.New<GlbStaff>();
				staff1.GS_Code = "1";
				staff1.GS_LoginName = "1";
				var staff2 = Factory.New<GlbStaff>();
				staff2.GS_Code = "2";
				staff2.GS_LoginName = "2";
				var workItem = Factory.New<NewWorkItem>();
				workItem.WKI_WorkItemNumber = "WI00010001";
				var collection = workItem.WorkflowItems;
				var processTaskView = new WorkItemProcessTaskCollectionViewFilter(collection);
				var processTask = collection.AddNew();
				using (var control = new EDITaskWithDetailsAndFilterControlForTest())
				{
					form.ControllerID = DummyControllerIDs.Dummy;
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(processTaskView, "");
					var richTextBox = control.SimpleNotesRichTextBox.GetRichTextBoxForTest();
					control.SimpleNotesRichTextBox.ContextMenuStripOpeningForTest();
					ToolStripMenuItem pullRequestItem = GetPullRequestItem(richTextBox.ContextMenuStrip.Items);
					AssertNotNull(pullRequestItem);
					Assert(!pullRequestItem.Visible);
					processTask.P9_Type = EDITaskTypes.TaskShelfTest;
					processTask.P9_GS_NKAssignedStaffMember = "1";
					control.SimpleNotesRichTextBox.ContextMenuStripOpeningForTest();
					pullRequestItem = GetPullRequestItem(richTextBox.ContextMenuStrip.Items);
					AssertNotNull(pullRequestItem);
					Assert(pullRequestItem.Visible);
					AssertCollectionsEqualByElements(new[] { "" }, pullRequestItem.DropDown.Items);
					pullRequestItem.DropDown.Show();
					AssertCollectionsEqualByElements(new[] { "WI00010001", "", "WI00010002" }, pullRequestItem.DropDown.Items);
					pullRequestItem.DropDown.Items[0].PerformClick();
					Assert(string.Compare(DevOpsContextForTest.ExpectedWebUrlForTest, richTextBox.Text, StringComparison.OrdinalIgnoreCase) == 0);
					processTask.P9_GS_NKAssignedStaffMember = "2";
					control.SimpleNotesRichTextBox.ContextMenuStripOpeningForTest();
					pullRequestItem = GetPullRequestItem(richTextBox.ContextMenuStrip.Items);
					AssertNotNull(pullRequestItem);
					Assert(pullRequestItem.Visible);
					AssertCollectionsEqualByElements(new[] { "" }, pullRequestItem.DropDown.Items);
					pullRequestItem.DropDown.Show();
					AssertCollectionsEqualByElements(new[] { "No Pull Requests Found for User 2" }, pullRequestItem.DropDown.Items);
					pullRequestItem.DropDown.Items[0].PerformClick();
					processTask.P9_GS_NKAssignedStaffMember = "";
					control.SimpleNotesRichTextBox.ContextMenuStripOpeningForTest();
					pullRequestItem = GetPullRequestItem(richTextBox.ContextMenuStrip.Items);
					AssertNotNull(pullRequestItem);
					Assert(pullRequestItem.Visible);
					AssertCollectionsEqualByElements(new[] { "" }, pullRequestItem.DropDown.Items);
					pullRequestItem.DropDown.Show();
					AssertCollectionsEqualByElements(new[] { "No Staff is Assigned" }, pullRequestItem.DropDown.Items);
				}
			}
		}

		public void TestLoadPullRequestsFromMultipleTeamProjectCollections()
		{
			AddTeamProjectCollection("http://tfs.wtg.zone:8080/tfs/CargoWise");
			AddTeamProjectCollection("https://devops.wisetechglobal.com/wtg");
			using (ZForm form = new ZForm())
			{
				var staff1 = Factory.New<GlbStaff>();
				staff1.GS_Code = "USR";
				staff1.GS_LoginName = "user";
				var workItem = Factory.New<NewWorkItem>();
				workItem.WKI_WorkItemNumber = "WI00010001";
				var collection = workItem.WorkflowItems;
				var processTaskView = new WorkItemProcessTaskCollectionViewFilter(collection);
				var processTask = collection.AddNew();
				var mockDevOps1 = new Mock<IDevOpsRestContext>();
				mockDevOps1.Setup(o => o.GetUserId("user")).Returns("u1");
				mockDevOps1.Setup(o => o.GetPullRequestsByOwner("u1")).Returns(new[] { new PullRequestInfo()
				{ title = "WI00343738", createdBy = new PullRequestInfo.Createdby()
				{ id = "u1" } } });
				var mockDevOps2 = new Mock<IDevOpsRestContext>();
				mockDevOps2.Setup(o => o.GetUserId("user")).Returns("u2");
				mockDevOps2.Setup(o => o.GetPullRequestsByOwner("u2")).Returns(new[] { new PullRequestInfo()
				{ title = "WI00343739", createdBy = new PullRequestInfo.Createdby()
				{ id = "u2" } } });
				var mockDevOpsFactory = new Func<Uri, IDevOpsRestContext>(uri =>
				{
					if (uri.ToString() == "http://tfs.wtg.zone:8080/tfs/CargoWise")
					{
						return mockDevOps1.Object;
					}
					else if (uri.ToString() == "https://devops.wisetechglobal.com/wtg")
					{
						return mockDevOps2.Object;
					}
					else
					{
						throw new InvalidOperationException("Unknown team project collection url: " + uri);
					}
				});
				using (var control = new EDITaskWithDetailsAndFilterControlForTest(mockDevOpsFactory))
				{
					form.ControllerID = DummyControllerIDs.Dummy;
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(processTaskView, "");
					var richTextBox = control.SimpleNotesRichTextBox.GetRichTextBoxForTest();
					control.SimpleNotesRichTextBox.ContextMenuStripOpeningForTest();
					ToolStripMenuItem pullRequestItem = GetPullRequestItem(richTextBox.ContextMenuStrip.Items);
					AssertNotNull(pullRequestItem);
					Assert(!pullRequestItem.Visible);
					processTask.P9_Type = EDITaskTypes.TaskShelfTest;
					processTask.P9_GS_NKAssignedStaffMember = "USR";
					control.SimpleNotesRichTextBox.ContextMenuStripOpeningForTest();
					pullRequestItem = GetPullRequestItem(richTextBox.ContextMenuStrip.Items);
					AssertNotNull(pullRequestItem);
					Assert(pullRequestItem.Visible);
					AssertCollectionsEqualByElements(new[] { "" }, pullRequestItem.DropDown.Items);
					pullRequestItem.DropDown.Show();
					AssertCollectionsEqualByElements(new[] { "WI00343738", "WI00343739" }, pullRequestItem.DropDown.Items);
				}
			}
		}

		public void TestLoadPullRequestsHandlesWebException()
		{
			AddTeamProjectCollection("http://tfs.wtg.zone:8080/tfs/CargoWise");
			AddTeamProjectCollection("https://devops.wisetechglobal.com/wtg");
			using (ZForm form = new ZForm())
			{
				var staff1 = Factory.New<GlbStaff>();
				staff1.GS_Code = "USR";
				staff1.GS_LoginName = "user";
				var workItem = Factory.New<NewWorkItem>();
				workItem.WKI_WorkItemNumber = "WI00010001";
				var collection = workItem.WorkflowItems;
				var processTaskView = new WorkItemProcessTaskCollectionViewFilter(collection);
				var processTask = collection.AddNew();
				var mockDevOps1 = new Mock<IDevOpsRestContext>();
				mockDevOps1.Setup(o => o.GetUserId("user")).Throws(new WebException("The remote server returned an error: (401) Unauthorized."));
				var mockDevOps2 = new Mock<IDevOpsRestContext>();
				mockDevOps2.Setup(o => o.GetUserId("user")).Returns("u2");
				mockDevOps2.Setup(o => o.GetPullRequestsByOwner("u2")).Returns(new[] { new PullRequestInfo()
				{ title = "WI00343739", createdBy = new PullRequestInfo.Createdby()
				{ id = "u2" } } });
				var mockDevOpsFactory = new Func<Uri, IDevOpsRestContext>(uri =>
				{
					if (uri.ToString() == "http://tfs.wtg.zone:8080/tfs/CargoWise")
					{
						return mockDevOps1.Object;
					}
					else if (uri.ToString() == "https://devops.wisetechglobal.com/wtg")
					{
						return mockDevOps2.Object;
					}
					else
					{
						throw new InvalidOperationException("Unknown team project collection url: " + uri);
					}
				});
				using (var control = new EDITaskWithDetailsAndFilterControlForTest(mockDevOpsFactory))
				{
					form.ControllerID = DummyControllerIDs.Dummy;
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(processTaskView, "");
					var richTextBox = control.SimpleNotesRichTextBox.GetRichTextBoxForTest();
					control.SimpleNotesRichTextBox.ContextMenuStripOpeningForTest();
					ToolStripMenuItem pullRequestItem = GetPullRequestItem(richTextBox.ContextMenuStrip.Items);
					AssertNotNull(pullRequestItem);
					Assert(!pullRequestItem.Visible);
					processTask.P9_Type = EDITaskTypes.TaskShelfTest;
					processTask.P9_GS_NKAssignedStaffMember = "USR";
					control.SimpleNotesRichTextBox.ContextMenuStripOpeningForTest();
					pullRequestItem = GetPullRequestItem(richTextBox.ContextMenuStrip.Items);
					AssertNotNull(pullRequestItem);
					Assert(pullRequestItem.Visible);
					AssertCollectionsEqualByElements(new[] { "" }, pullRequestItem.DropDown.Items);
					pullRequestItem.DropDown.Show();
					AssertCollectionsEqualByElements(new[] { "WI00343739" }, pullRequestItem.DropDown.Items);
				}
			}
		}

		public void TestDisplayOrHideReleaseInfoControls_DAT()
		{
			using (ZForm form = new ZForm())
			{
				var workItem = Factory.New<NewWorkItem>();
				workItem.WKI_WorkItemNumber = "WI00010001";
				var collection = workItem.WorkflowItems;
				var processTaskView = new WorkItemProcessTaskCollectionViewFilter(collection);
				var processTask = collection.AddNew();
				processTask.P9_Type = "CH0";
				processTask.P9_GS_NKAssignedStaffMember = "DAT";
				processTask.P9_Status = "CLS";

				using (var control = new EDITaskWithDetailsAndFilterControlForTest())
				{
					form.ControllerID = DummyControllerIDs.Dummy;
					form.Controls.Add(control);
					form.Show();
					control.SetDataBinding(processTaskView, "");
					AssertEquals(1, control.TasksGrid.List.Count);
					AssertEquals(true, control.ReleaseInfoTextBox.Visible);
					AssertEquals(true, control.ReleaseInfoTextBox.ReadOnly);
					AssertEquals(true, control.VersionNoTextBox.Visible);
					AssertEquals(true, control.VersionNoTextBox.ReadOnly);
					AssertEquals(true, control.RetrieveReleaseInfoButton.Visible);
				}
			}
		}

		void AssertCollectionsEqualByElements(string[] expected, ToolStripItemCollection items)
		{
			var index = 0;
			foreach (ToolStripItem item in items)
			{
				Assert(expected[index++].Equals(item.Text, StringComparison.Ordinal));
			}
		}

		void AddTeamProjectCollection(string url)
		{
			var result = new List<Uri>();
			using (var connection = DbConnectionCrikey.GetAutoTesterUserTestsConnection())
			using (var command = connection.Command("insert into TeamProjectCollections (TPC_Url) values (@url)"))
			{
				command.AddParameter("url", SqlDbType.VarChar, 128, url);
				command.ExecuteNonQuery();
			}
		}

		ToolStripMenuItem GetPullRequestItem(ToolStripItemCollection items)
		{
			foreach (var item in items)
			{
				if (((ToolStripMenuItem)item).Text == "Pull Requests")
				{
					return item as ToolStripMenuItem;
				}
			}

			return null;
		}

		class EDITaskWithDetailsAndFilterControlForTest : EDITaskWithDetailsAndFilterControl
		{
			public EDITaskWithDetailsAndFilterControlForTest() : this(GetDevOpsContextForTest)
			{
			}

			public EDITaskWithDetailsAndFilterControlForTest(Func<Uri, IDevOpsRestContext> getDevOpsContext) : base()
			{
				GitPullRequestMenuItem.GetDevOpsContextMethod = getDevOpsContext;
			}

			static IDevOpsRestContext GetDevOpsContextForTest(Uri uri)
			{
				return new DevOpsContextForTest();
			}
		}

		class DevOpsContextForTest : IDevOpsRestContext
		{
			internal DevOpsContextForTest()
			{
				userId1 = Guid.NewGuid();
				userId2 = Guid.NewGuid();
				pullRequestsForTest = new List<PullRequestInfo>()
				{ new PullRequestInfo()
				{ title = "WI00010001", createdBy = new PullRequestInfo.Createdby()
				{ id = userId1.ToString() }, url = new Uri(pullRequestUrlForTest, UriKind.Absolute), repository = new PullRequestInfo.Repository()
				{ project = new PullRequestInfo.Project()
				{ name = "TestProject" } }, pullRequestUrl = new Uri(ExpectedWebUrlForTest) }, new PullRequestInfo()
				{ title = "WI00010002", createdBy = new PullRequestInfo.Createdby()
				{ id = userId1.ToString() } }, };
			}

			readonly Guid userId1;
			readonly Guid userId2;
			readonly List<PullRequestInfo> pullRequestsForTest;
			public string GetUserId(ZString loginName)
			{
				switch (loginName)
				{
					case "1":
						return userId1.ToString();
					case "2":
						return userId2.ToString();
					default:
						return null;
				}
			}

			public IEnumerable<PullRequestInfo> GetPullRequestsByOwner(string ownerId)
			{
				return pullRequestsForTest.Where(p => p.createdBy.id == ownerId);
			}

			public const string ExpectedWebUrlForTest = "http://tfs.wtg.zone:8080/tfs/CargoWise/TestProject/_git/29a4f052-42c7-460b-b310-48ac8f8122e5/pullrequest/1418?_a=overview";
			const string pullRequestUrlForTest = "http://tfs.wtg.zone:8080/tfs/CargoWise/_apis/git/repositories/29a4f052-42c7-460b-b310-48ac8f8122e5/pullRequests/1418";
		}
	}
}
