using System.Windows.Forms;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Module.Test
{
	[TestedType(typeof(ProcessHeaderController))]
	class ProcessHeaderControllerTest : BMControllerTest
	{
		public void TestShowForm_JobIsNotSupportsBufferManagement()
		{
			// see OrgPartRelationWorkflowDescriptor.cs for the SupportsBufferManagement override!
			var quotedBook = Factory.NewWithValidTestData(typeof(OrgPartRelation));
			var jobHeader = ProcessJobHeader.GetForParent(quotedBook as IWorkflowProvider, Factory);

			var header = jobHeader.ProcessHeaders.AddNew();

			var staff = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "DAN", "Daniel");
			var task = BMSTestHelper.CreateTask(header, staff.GS_Code, 120, estVariationFactor: 1);

			Factory.Save();

			var localFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var loadedJobHeader = localFactory.Load<ProcessHeader>(jobHeader.PK);

			var controller = new ProcessHeaderControllerForTest();
			IZForm form = null;
			AssertNoExceptionThrown(() => form = controller.GetForm_ForTest(loadedJobHeader));

			AssertNull("GIVEN job that is not supporting buffer management, WHEN executing GetForm SHOULD return NULL.", form);

			AssertEquals("GIVEN job that is not supporting buffer management, WHEN executing GetForm SHOULD not throw exception and SHOULD show not-supported message.",
				@"Unable to find the job this workflow belongs to.
Technical details: parent table code = OU",
				UnitTestUserNotification.Instance.LastMessage.Text);

			ErrorReporter.Clear();
		}

		public void TestGetForm_MultiOpenOfOwnedFormDoesntExplode()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, string.Empty, 60);

			Factory.Save();

			var controller = new ProcessHeaderControllerForTest();

			using (var form = controller.ShowViewForm(workflow))
			{
				Application.DoEvents();

				var secondForm = controller.ShowEditForm(workflow);
				Application.DoEvents();

				AssertEquals(form, secondForm);
			}
		}

		public void TestGetForm_WhenCurrentUserIsRestrictedFromModule_ShouldNotThrowException()
		{
			var resource = Factory.NewWithValidTestData<GlbStaff>();
			var system = BMSTestHelper.CreateSystem(Factory, "INQ");

			var jobHeader = BMSTestHelper.CreateJobHeader<SalesEnquiry>(Factory);
			var workflow = jobHeader.ProcessHeaders[0];
			var task = BMSTestHelper.CreateTask(workflow, resource.GS_Code, 60);

			Factory.Save();

			using (Env.SetTemporaryUserContext(resource.PK.ToGuid(), Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				Env.Security.WorkflowTasks.IsAllowed = true;
				AssertEquals(false, Env.Security.InquiryManager.IsAllowed);

				var controller = new ProcessHeaderControllerForTest();

				using (var form = controller.ShowViewForm(workflow))
				{
					AssertNull(form);
				}

				AssertMultilineASCIIEquals("", @"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Manage -> Client Relationship Management -> Inquiry Manager -> View", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestGetForm_HandleWithTemplate()
		{
			var template = BMSTestHelper.CreateWorkflowTemplate(Factory, "ORG");
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			var jobHeader = ProcessJobHeader.GetForParent(orgHeader, Factory);
			jobHeader.FH_P0_Template = template.PK;
			jobHeader.FH_ParentTableCode = "";
			Factory.Save();

			var controller = new ProcessHeaderControllerForTest();
			using (var form = controller.GetForm_ForTest(jobHeader))
			{
				AssertNull(form);
				AssertNotNull(controller.LastShownForm);
				AssertEquals("Edit Workflow Template", ((ZForm)controller.LastShownForm).Text);
			}
			controller.LastShownForm.Dispose();
		}

		public void TestShowFormForNewEntity_ForTemplate()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var templateJobHeader = ProcessJobHeader.GetForParent(template, Factory, false);
			templateJobHeader.FH_P0_Template = template.PK;
			var templateWorkflow = templateJobHeader.ProcessHeaders.AddNew();
			templateWorkflow.FH_CompletionStatement = "workflow1";
			templateWorkflow.FH_P0_Template = template.PK;

			Factory.Save();

			using (var form = new ProcessHeaderController().ShowFormForNewEntity(templateWorkflow))
			{
				AssertStartsWith("The form to edit the template should have been loaded, and yet...", "Edit Workflow Template", ((Form)form).Text);
			}
		}

		public void TestSecurityCheckpointForDelete()
		{
			var controller = new ProcessHeaderControllerForTest();
			AssertEquals("SecurityCheckPoint incorrect", Env.Security.WorkflowHeadersDelete, controller.GetCheckPointForDelete(null));
		}

		public void TestSecurityCheckpointForNew()
		{
			var controller = new ProcessHeaderControllerForTest();
			AssertEquals("SecurityCheckPoint incorrect", Env.Security.WorkflowHeaders, controller.GetCheckPointForNew(null));
		}

		class ProcessHeaderControllerForTest : ProcessHeaderController
		{
			public IZForm GetForm_ForTest(IBusiness businessEntity)
			{
				return GetForm(businessEntity);
			}
		}

		public void TestShowEditForm_ShouldNotSetControllerID()
		{
			var job = Factory.NewWithValidTestData<OrgHeader>();
			var header = ProcessJobHeader.GetForParent(job, Factory).ProcessHeaders[0];
			var task = job.WorkflowItems.AddNew();
			task.P9_FH_ProcessHeader = header.PK;
			Factory.Save();

			var controller = new ProcessHeaderController();
			using (var form = controller.ShowEditForm(header))
			{
				AssertNotNull(form);
				AssertEquals(ControllerIDs.Organisation, form.ControllerID);
			}
		}

		protected override ControllerID GetControllerID()
		{
			return ControllerIDs.ProcessHeader;
		}

		protected override BusinessObject GetBusinessObjectThatIsInTheDatabase()
		{
			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = "ORG";
			var jobHeader = ProcessJobHeader.GetForParent(template, Factory);
			jobHeader.FH_P0_Template = template.PK;
			var processHeader = jobHeader.ProcessHeaders.AddNew();
			processHeader.FH_CompletionStatement = "workflow1";
			processHeader.FH_P0_Template = template.PK;
			Factory.Save();
			return processHeader;
		}

		public void TestGetForm()
		{
			using (var form = new ProcessHeaderController().ShowFormForNewEntity(GetBusinessObjectThatIsInTheDatabase()))
			{
				AssertNotNull(form);
				//The GetForm() method in ProcessHeaderController.cs file returns null even if the form is not null.
				//This fix was suggested by Mykola after discussion about the call stack below.
				//Ensures that when the Parent Object (workitem, project etc) is opened, it is the only form added to OpenedFormCache in ZController.PrepareNewlyCreateForm().
				//Parent object is responsible for showing the form and adding to OpenedFormCache.
				//Prevents the processHeader (child) form from being added to OpenedFormCache
				//Previous configuration of code caused an uncessary repetion in call stack, this caused the form to be added into the OpenedFormCache twice
				//Call Stack

				//Enterprise.ZArchitecture.GUI.dll!Enterprise.ZArchitecture.Modules.ZController.PrepareNewlyCreateForm(CargoWise.EntityFramework.IBusiness BusinessEntity)
				//Enterprise.ZArchitecture.GUI.dll!Enterprise.ZArchitecture.Modules.ZController.ShowLoadedForm(CargoWise.EntityFramework.IBusiness SourceEntity, Enterprise.ZArchitecture.GUI.FormAction Action)
				//Enterprise.ZArchitecture.GUI.dll!Enterprise.ZArchitecture.Modules.ZController.ShowEditForm(CargoWise.EntityFramework.BusinessObject SourceEntity)
				//Enterprise.MasterFiles.GUI.dll!Enterprise.MasterFiles.GUI.WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(Enterprise.MasterFiles.Business.IWorkflowProvider workflowProvider)
				//Enterprise.MasterFiles.GUI.dll!Enterprise.MasterFiles.GUI.WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(Enterprise.BufferManagement.Integration.IProcessHeader processHeader, Enterprise.MasterFiles.Business.ProcessTask task)
				//Enterprise.MasterFiles.GUI.dll!Enterprise.MasterFiles.GUI.WorkflowParentFormFactory.ShowFormAndNavigateToWorkflowItem(Enterprise.BufferManagement.Integration.IProcessHeader processHeader)
				//Enterprise.BufferManagement.Module.dll!Enterprise.BufferManagement.Module.ProcessHeaderController.GetForm(CargoWise.EntityFramework.IBusiness businessEntity)
				//Enterprise.ZArchitecture.GUI.dll!Enterprise.ZArchitecture.Modules.ZController.PrepareNewlyCreateForm(CargoWise.EntityFramework.IBusiness BusinessEntity)
				//Enterprise.ZArchitecture.GUI.dll!Enterprise.ZArchitecture.Modules.ZController.ShowLoadedForm(CargoWise.EntityFramework.IBusiness SourceEntity, Enterprise.ZArchitecture.GUI.FormAction Action)
				//Enterprise.ZArchitecture.GUI.dll!Enterprise.ZArchitecture.Modules.ZController.ShowEditForm(CargoWise.EntityFramework.BusinessObject SourceEntity)
				//Enterprise.ZArchitecture.GUI.dll!Enterprise.ZArchitecture.Modules.ZFilterModule.ShowEditForm.AnonymousMethod__0()

				//Returning null prevents the form from being added to the OpenedFormCache twice, which will cause the exception. Therefore Save buttons are activated.

				var buttonsProvider = form as IPostingButtonsProvider;
				AssertEquals("&New", buttonsProvider.CommandButtonApply.Text);
			}
		}

		public override void TestNewForm()
		{
			AssertNotNull(Controller.ShowFormForNewEntity(GetBusinessObjectThatIsInTheDatabase()));
		}

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
		}
	}
}
