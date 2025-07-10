using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Internal;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowRelationshipsUserControlTest : BMSTestCaseWithFactory
	{
		public void TestWorkflowLookupButtons_FiltersByJob_ByDefault()
		{
			var otherJobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			BMSTestHelper.CreateWorkflow(otherJobHeader, "workflow1");
			BMSTestHelper.CreateWorkflow(otherJobHeader, "workflow2");

			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow1");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "workflow2");

			Factory.Save();

			using (var form = new ZForm { Width = 800, Height = 800, ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowRelationshipsUserControlForTest())
			{
				control.SetDataBinding(workflow1, new ProcessHeaderRelationshipsViewModel(Factory));
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				EmbeddedModulePopup modulePopup = null;
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(dialog => modulePopup = (EmbeddedModulePopup)dialog);
				form.FindAndClickButton("addPostrequisiteButton");

				var filter = modulePopup.Module_ForTest.FilterBusinessObject.Filter;
				AssertContainsExactElementsInAnyOrder("Default filters are workign correctly.", new[] { workflow1, workflow2, jobHeader }, Factory.Load<ProcessHeader>(filter));
			}
		}

		public void TestRemoveMultipleLinks()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			workflow1.MakePrerequisiteOf(workflow2);
			workflow1.MakePrerequisiteOf(workflow3);

			var links = workflow1.LinksFromMeToOthers.ToArray();

			AssertEquals(2, links.Length);

			using (var form = new ZForm { Width = 800, Height = 800 })
			using (var control = new WorkflowRelationshipsUserControlForTest())
			{
				var viewModel = new ProcessHeaderRelationshipsViewModel(Factory);
				control.SetDataBinding(workflow1, viewModel);
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				control.PostrequisitesGrid_Exposed.Select(0);
				control.PostrequisitesGrid_Exposed.Select(1);

				Application.DoEvents();
				control.Find(c => c.Name == "removePostrequisiteButton").Cast<ZButton>().First().PerformClick();

				AssertEquals(0, workflow1.LinksFromMeToOthers.Count());
			}
		}

		public void TestRemoveLink_WhenApprovedArrowExists_ShouldDecoupleInstead()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link1_3 = workflow1.GetOrCreateDependencyLink(workflow3);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram, "shape3");

			var arrow1_2 = NetworkTestCase.CreateDependencyAttachment(diagram, link1_2, shape1, shape2);
			var arrow1_3 = NetworkTestCase.CreateDependencyAttachment(diagram, link1_3, shape1, shape3);

			arrow1_2.Approve(GlbStaff.CurrentUser.GS_Code);

			using (var form = new ZForm { Width = 800, Height = 800, ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowRelationshipsUserControlForTest())
			{
				control.SetDataBinding(workflow1, new ProcessHeaderRelationshipsViewModel(Factory));
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				control.PostrequisitesGrid_Exposed.Select(0);
				control.PostrequisitesGrid_Exposed.Select(1);

				AssertEquals(2, workflow1.PostrequisiteLinks.Count());
				AssertEquals(1, workflow2.PrerequisiteLinks.Count());

				PostMessage(control.PostrequisitesGrid_Exposed.Handle, WM_KEYDOWN, VK_DELETE, 0);
				Application.DoEvents();

				AssertEquals(2, workflow1.PostrequisiteLinks.Count());
				AssertEquals(1, workflow2.PrerequisiteLinks.Count());

				AssertEquals(false, arrow1_2.BNA_IsDecouple);
				AssertEquals(false, arrow1_3.IsDeleted);
				AssertEquals("Some of the selected links have an approved arrow on a project plan. The arrow will be decoupled. \r\nDecoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				Application.DoEvents();
				control.Find(c => c.Name == "removePostrequisiteButton").Cast<ZButton>().First().PerformClick();

				AssertEquals(0, workflow1.PostrequisiteLinks.Count());
				AssertEquals(0, workflow2.PrerequisiteLinks.Count());

				AssertEquals(true, arrow1_2.BNA_IsDecouple);
				AssertEquals(true, arrow1_3.IsDeleted);
				AssertEquals("Some of the selected links have an approved arrow on a project plan. The arrow will be decoupled. \r\nDecoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRemoveLink_WhenApprovedArrowExists_AnsweringCancel()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();
			var workflow3 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);
			var link1_3 = workflow1.GetOrCreateDependencyLink(workflow3);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");
			var shape3 = NetworkTestCase.CreateShape(workflow3, diagram, "shape3");

			var arrow1_2 = NetworkTestCase.CreateDependencyAttachment(diagram, link1_2, shape1, shape2);
			var arrow1_3 = NetworkTestCase.CreateDependencyAttachment(diagram, link1_3, shape1, shape3);

			arrow1_2.Approve(GlbStaff.CurrentUser.GS_Code);

			using (var form = new ZForm { Width = 800, Height = 800, ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowRelationshipsUserControlForTest())
			{
				control.SetDataBinding(workflow1, new ProcessHeaderRelationshipsViewModel(Factory));
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);

				control.PostrequisitesGrid_Exposed.Select(0);
				control.PostrequisitesGrid_Exposed.Select(1);

				Application.DoEvents();
				control.Find(c => c.Name == "removePostrequisiteButton").Cast<ZButton>().First().PerformClick();

				AssertEquals(2, workflow1.PostrequisiteLinks.Count());
				AssertEquals(1, workflow2.PrerequisiteLinks.Count());

				AssertEquals(false, arrow1_2.BNA_IsDecouple);
				AssertEquals(false, arrow1_3.IsDeleted);
				AssertEquals(@"Some of the selected links have an approved arrow on a project plan. The arrow will be decoupled. 
Decoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				PostMessage(control.PostrequisitesGrid_Exposed.Handle, WM_KEYDOWN, VK_DELETE, 0);
				Application.DoEvents();

				AssertEquals(2, workflow1.PostrequisiteLinks.Count());
				AssertEquals(1, workflow2.PrerequisiteLinks.Count());

				AssertEquals(false, arrow1_2.BNA_IsDecouple);
				AssertEquals(false, arrow1_3.IsDeleted);
				AssertEquals(@"Some of the selected links have an approved arrow on a project plan. The arrow will be decoupled. 
Decoupling causes the dependency link to be removed but the arrow to be preserved on the project plan.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestRemoveLink_WhenApprovedArrowExists_AndBufferExistsOnLink()
		{
			BMSTestHelper.CreateSystem(Factory, "ORG");
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			var link1_2 = workflow1.GetOrCreateDependencyLink(workflow2);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();

			var shape1 = NetworkTestCase.CreateShape(workflow1, diagram, "shape1");
			var shape2 = NetworkTestCase.CreateShape(workflow2, diagram, "shape2");

			var arrow1_2 = network.CreateRelationship(shape1, shape2).AsAttachment();

			network.SwitchToScaled();

			arrow1_2.Approve(GlbStaff.CurrentUser.GS_Code);

			new AddBufferAction.AddBufferForLinkAction(networkViewModel, arrow1_2).ExecuteForEntityWithoutAccessCheck(shape1);

			UnitTestUserNotification.Instance.AddOKAnswer();
			networkViewModel.ToggleApproval();

			using (var form = new ZForm { Width = 800, Height = 800, ControllerID = DummyControllerIDs.Dummy })
			using (var control = new WorkflowRelationshipsUserControlForTest())
			{
				control.SetDataBinding(workflow1, new ProcessHeaderRelationshipsViewModel(Factory));
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				control.PostrequisitesGrid_Exposed.Select(0);

				Application.DoEvents();
				control.Find(c => c.Name == "removePostrequisiteButton").Cast<ZButton>().First().PerformClick();

				AssertEquals(1, workflow1.PostrequisiteLinks.Count());
				AssertEquals(1, workflow2.PrerequisiteLinks.Count());

				AssertEquals(false, arrow1_2.BNA_IsDecouple);
				AssertEquals("The selected link has an approved arrow on a project plan and has an attached buffer. Please un-approve this diagram and remove this buffer prior to removing this link", UnitTestUserNotification.Instance.LastMessage.Text);

				UnitTestUserNotification.Instance.ClearMessages();

				PostMessage(control.PostrequisitesGrid_Exposed.Handle, WM_KEYDOWN, VK_DELETE, 0);
				Application.DoEvents();

				AssertEquals(1, workflow1.PostrequisiteLinks.Count());
				AssertEquals(1, workflow2.PrerequisiteLinks.Count());

				AssertEquals(false, arrow1_2.BNA_IsDecouple);
				AssertEquals("The selected link has an approved arrow on a project plan and has an attached buffer. Please un-approve this diagram and remove this buffer prior to removing this link", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		const UInt32 WM_KEYDOWN = 0x0100;
		const int VK_DELETE = 0x2E;

		[DllImport("user32.dll")]
		static extern bool PostMessage(IntPtr hWnd, UInt32 msg, int wParam, int lParam);

		public void TestDoubleClick_NullGridNotAccessOnDoubleClickWhenNoRowSelected()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var cmp = system.Components.AddNew();
			cmp.FC_Name = "Component1";

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "ORG1";
			job.OH_FullName = "Organisation 1";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader.ProcessHeaders[0];
			header1.FH_CompletionStatement = "Workflow1";

			using (var form = new ZForm())
			using (var control = new WorkflowRelationshipsUserControlForTest())
			{
				var viewModel = new ProcessHeaderRelationshipsViewModel(Factory);
				control.SetDataBinding(jobHeader, viewModel);
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(control);
				form.Show();

				AssertNoExceptionThrown(() => control.NavigateIfAllowed(() => WorkflowRelationshipsUserControl.GetCurrentlySelectedItem<ProcessHeader>(control.JobWorkflowsGrid_exposed), -1));
				AssertEquals("Job Organization (ORG1) is complete.", control.CompletionStatement_Exposed.Text);

				AssertNoExceptionThrown(() => control.NavigateIfAllowed(() => WorkflowRelationshipsUserControl.GetCurrentlySelectedItem<ProcessHeader>(control.JobWorkflowsGrid_exposed), 0));
				AssertEquals("Workflow1", control.CompletionStatement_Exposed.Text);
			}
		}

		public void TestFormContent()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "ORG");
			var cmp = system.Components.AddNew();
			cmp.FC_Name = "Component1";

			var job = Factory.NewWithValidTestData<OrgHeader>();
			job.OH_Code = "ORG1";
			job.OH_FullName = "Organisation 1";
			var jobHeader = ProcessJobHeader.GetForParent(job, Factory);
			var header1 = jobHeader.ProcessHeaders[0];
			header1.FH_CompletionStatement = "Workflow1";
			var header2 = jobHeader.ProcessHeaders.AddNew();
			header2.FH_CompletionStatement = "Workflow2";
			var header3 = jobHeader.ProcessHeaders.AddNew();
			header3.FH_CompletionStatement = "Workflow3";
			var header4 = jobHeader.ProcessHeaders.AddNew();
			header4.FH_CompletionStatement = "Workflow4";
			var header5 = jobHeader.ProcessHeaders.AddNew();
			header5.FH_CompletionStatement = "Workflow5";
			var header6 = jobHeader.ProcessHeaders.AddNew();
			header6.FH_CompletionStatement = "Workflow6";

			var link1To2 = header1.GetOrCreateDependencyLink(header2);
			var link1To3 = header1.GetOrCreateDependencyLink(header3);
			var link2To4 = header2.GetOrCreateDependencyLink(header4);
			var link3To4 = header3.GetOrCreateDependencyLink(header4);

			var link5ChildOf4 = header5.GetOrCreateLinkToParent(header4);

			using (var form = new ZForm())
			using (var control = new WorkflowRelationshipsUserControlForTest())
			{
				var viewModel = new ProcessHeaderRelationshipsViewModel(Factory);
				control.SetDataBinding(header4, viewModel);
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(control);
				form.Show();

				AssertEquals(2, control.PrerequisitesGrid_Exposed.ListManager.Count);
				Assert(control.PrerequisitesGrid_Exposed.List.Contains(link2To4));
				Assert(control.PrerequisitesGrid_Exposed.List.Contains(link3To4));

				AssertEquals(0, control.PostrequisitesGrid_Exposed.ListManager.Count);

				AssertEquals(0, control.ParentsGrid_Exposed.ListManager.Count);

				AssertEquals(1, control.ChildrenGrid_Exposed.ListManager.Count);
				Assert(control.ChildrenGrid_Exposed.List.Contains(link5ChildOf4));

				AssertEquals("ORG1", control.JobNumber_Exposed.Text);
				AssertEquals("Organisation 1", control.JobDescription_Exposed.Text);
				AssertEquals("Workflow4", control.CompletionStatement_Exposed.Text);
				AssertEquals("Component1", control.CurrentComponent_Exposed.Text);
			}
		}

		public void TestSelectedWorkflowContentsNotCutOff()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders[0];

			using (var form = new ZForm { Width = 1015, Height = 630 })
			using (var control = new WorkflowRelationshipsUserControlForTest())
			{
				var viewModel = new ProcessHeaderRelationshipsViewModel(Factory);
				control.SetDataBinding(workflow, viewModel);
				control.Size = new System.Drawing.Size(1015, 630);
				form.ControllerID = DummyControllerIDs.Dummy;
				form.Controls.Add(control);
				form.Show();

				Application.DoEvents();

				var parentControl = control.StatusDescription_Exposed.Parent;

				Assert(parentControl.Bottom > control.JobNumber_Exposed.Bottom);
				Assert(parentControl.Bottom > control.JobDescription_Exposed.Bottom);
				Assert(parentControl.Bottom > control.OpenJobFormButton_Exposed.Bottom);
				Assert(parentControl.Bottom > control.GoToJobButton_Exposed.Bottom);
				Assert(parentControl.Bottom > control.ZTextBox1_Exposed.Bottom);
				Assert(parentControl.Bottom > control.CompletionStatement_Exposed.Bottom);
				Assert(parentControl.Bottom > control.CurrentComponent_Exposed.Bottom);
				Assert(parentControl.Bottom > control.StaggerDelayTimeEdit_Exposed.Bottom);
				Assert(parentControl.Bottom > control.StaggerDelayFactorCalcEdit_Exposed.Bottom);
				Assert(parentControl.Bottom > control.StatusDescription_Exposed.Bottom);
			}
		}
	}

	class WorkflowRelationshipsUserControlForTest : WorkflowRelationshipsUserControl
	{
		public WorkflowRelationshipsUserControlForTest()
			: base()
		{
		}

		public ZGrid PrerequisitesGrid_Exposed
		{
			get { return base.prerequisitesGrid; }
		}

		public ZGrid PostrequisitesGrid_Exposed
		{
			get { return base.postrequisitesGrid; }
		}

		public ZGrid ParentsGrid_Exposed
		{
			get { return base.parentsGrid; }
		}

		public ZGrid ChildrenGrid_Exposed
		{
			get { return base.childrenGrid; }
		}

		public ZGrid JobWorkflowsGrid_exposed
		{
			get { return base.JobWorkflowsGrid; }
		}

		public ZTextBox JobNumber_Exposed
		{
			get { return base.jobNumber; }
		}

		public ZTextBox JobDescription_Exposed
		{
			get { return base.jobDescription; }
		}

		public ZTextBox CompletionStatement_Exposed
		{
			get { return base.completionStatement; }
		}

		public ZTextBox CurrentComponent_Exposed
		{
			get { return base.currentComponent; }
		}

		public ZButton GoToJobButton_Exposed
		{
			get { return base.GoToJobButton; }
		}

		public ZButton OpenJobFormButton_Exposed
		{
			get { return base.OpenJobFormButton; }
		}

		public ZTextBox ZTextBox1_Exposed
		{
			get { return base.zTextBox1; }
		}

		public ZCalcEdit StaggerDelayFactorCalcEdit_Exposed
		{
			get { return base.StaggerDelayFactorCalcEdit; }
		}

		public ZTimeEditEx StaggerDelayTimeEdit_Exposed
		{
			get { return base.StaggerDelayTimeEdit; }
		}

		public ZTextBox StatusDescription_Exposed
		{
			get { return base.StatusDescription; }
		}
	}
}
