using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ProcessTaskForJobController))]
	public class ProcessTaskForJobControllerTest : BMControllerTest
	{
		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ProcessTaskForJob;
		}

		public void TestGetForm_NavigateToTask()
		{
			AssertNull(controller.GetForm_ForTest(task3));

			using (var form = controller.LastShownForm)
			{
				AssertTaskSelected("Should select correct task on uncached form", task3.PK, form);
			}
		}

		public void TestGetIDForFormCache_ShouldFindCachedForm()
		{
			AssertNull(controller.GetForm_ForTest(task3));

			using (var form = controller.LastShownForm)
			{
				Assert("Should find form in cache from any of the job's tasks.", controller.IsFormShownFor(task1));
				Assert("Should find form in cache from any of the job's tasks.", controller.IsFormShownFor(task2));
				Assert("Should find form in cache from any of the job's tasks.", controller.IsFormShownFor(task3));
			}
		}

		public void TestSwitchToFormFor_NavigateToTask()
		{
			AssertNull(controller.GetForm_ForTest(task3));

			using (var form = controller.LastShownForm)
			{
				AssertTaskSelected("Precondition", task3.PK, form);

				controller.SwitchToFormFor(task2);
				AssertTaskSelected("Should select correct task on cached form", task2.PK, form);
			}
		}

		public void TestShowEditForm_WhenEditAllowed_ShouldOpenInBrowseMode()
		{
			AssertShowFormInMode("ShowEditForm with correct security checkpoints should open forms in browse mode",
									ODisplayMode.Browse, "Edit Inquiry");
		}

		public void TestShowEditForm_WhenViewAllowedEditDenied_ShouldOpeninReadOnlyMode()
		{
			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.InquiryManagerView.IsAllowed = true;
				AssertEquals("Precondition", false, Env.Security.InquiryManagerEdit.IsAllowed);

				AssertShowFormInMode("ShowEditForm without correct security checkpoints for edit should open forms in ReadOnly mode",
										ODisplayMode.ReadOnly, "View Inquiry");
			}
		}

		public void TestShowEditForm_WhenCurrentUserIsRestrictedFromModule_ShouldNotThrowException()
		{
			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasks.IsAllowed = true;
				AssertEquals("Precondition", false, Env.Security.InquiryManager.IsAllowed);

				using (var form = controller.ShowEditForm(task3))
				{
					AssertNull(form);
				}

				AssertMultilineASCIIEquals("", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Client Relationship Management -> Inquiry Manager -> View", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetForm_MultiOpenOfOwnedFormDoesntExplode()
		{
			using (var form = controller.ShowViewForm(task3))
			{
				AssertTaskSelected("Precondition", task3.PK, form);

				var secondForm = controller.ShowEditForm(task2);

				AssertEquals("Should have found form in form cache", form, secondForm);
				AssertTaskSelected("Should have selected second task on cached form", task2.PK, form);
			}
		}

		public void TestGetForm_ShowTaskForm_WhenStandaloneTask()
		{
			var taskStandalone = Factory.New<ProcessTask>();

			Factory.Save();

			AssertNull(controller.GetForm_ForTest(taskStandalone));

			using (var form = controller.LastShownForm)
			{
				AssertType<TaskManagementForm>(form);
			}
		}

		public void TestShowEditForm_ShouldNotSetControllerID()
		{
			using (var form = controller.ShowEditForm(task3))
			{
				AssertNotNull(form);
				AssertEquals(ControllerIDs.SalesEnquiry, form.ControllerID);
			}
		}

		public void TestShowForm_JobDoesNotSupportBufferManagement()
		{
			// see OrgPartRelationWorkflowDescriptor.cs for the SupportsBufferManagement override!
			var quotedBook = Factory.NewWithValidTestData(typeof(OrgPartRelation));
			var jobHeader = ProcessJobHeader.GetForParent(quotedBook as IWorkflowProvider, Factory);

			var header = jobHeader.ProcessHeaders.AddNew();

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "Daniel");
			var task = BMSTestHelper.CreateTask(header, staff.GS_Code, 120, estVariationFactor: 1);

			Factory.Save();

			var localFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedTask = localFactory.Load<ProcessTask>(task.PK);

			IZForm form = null;
			AssertNoExceptionThrown(() => form = controller.GetForm_ForTest(loadedTask));

			AssertNull("GIVEN job that is not supporting buffer management, WHEN executing GetForm SHOULD return NULL.", form);

			AssertEquals("GIVEN job that is not supporting buffer management, WHEN executing GetForm SHOULD not throw exception and SHOULD show not-supported message.",
				@"Unable to find the job this task belongs to.
Technical details: parent table code = OU",
				UnitTestUserNotification.Instance.LastMessage.Text);

			ErrorReporter.Clear();
		}

		#region Not Supported

		public void TestShowNewForm_ShouldThrowModuleFeatureNotSupported()
		{
			AssertExceptionThrown<ModuleFeatureNotSupportedException>(() => controller.ShowNewForm());
		}

		public void TestShowDeleteForm_ShouldThrowModuleFeatureNotSupported()
		{
			AssertExceptionThrown<ModuleFeatureNotSupportedException>(() => controller.ShowDeleteForm(null));
		}

		#endregion

		#region Implementation

		class ProcessTaskForJobControllerForTest : ProcessTaskForJobController
		{
			public IZForm GetForm_ForTest(IBusiness businessEntity)
			{
				return GetForm(businessEntity);
			}
		}

		void AssertShowFormInMode(string msg, ODisplayMode formMode, string formText)
		{
			using (var form = controller.ShowEditForm(task3))
			{
				AssertNotNull(form);
				AssertEquals(msg, formMode, form.DisplayMode);
				AssertEquals("Form Text did not match expected form DisplayMode", formText, ((ZForm)form).Text);
				Assert(UnitTestUserNotification.Instance.LastMessage.WasNone);
				AssertTaskSelected("The correct task was not selected on the form", task3.PK, form);
			}
		}

		void AssertTaskSelected(string msg, ZGuid taskPK, IZForm form)
		{
			Application.DoEvents();
			AssertNotNull(form);
			var grid = ((Form)form).FindSingleOrDefault<ZGrid>(x => x.Name == "TasksGrid");

			AssertNotNull("Precondition", grid);
			AssertGreaterThanOrEqualTo("Precondition", grid.CurrentRowIndex, 0);

			var selected = (BusinessObject)grid.ListManager.Current;

			AssertEquals(msg, taskPK, selected.PK);
		}

		ProcessTaskForJobControllerForTest controller;
		ProcessTask task1, task2, task3;
		GlbStaff resource;

		void MakeSystem()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");
			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);
			var workflow1 = jobHeader.ProcessHeaders[0];
			var workflow2 = jobHeader.ProcessHeaders.AddNew();

			resource = Factory.NewWithValidTestData<GlbStaff>();
			task1 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 60);
			task2 = BMSTestHelper.CreateTask(workflow1, resource.GS_Code, 60);
			task3 = BMSTestHelper.CreateTask(workflow2, resource.GS_Code, 60);

			Factory.Save();
		}

		#endregion

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			MakeSystem();
			controller = new ProcessTaskForJobControllerForTest();
		}
	}
}
