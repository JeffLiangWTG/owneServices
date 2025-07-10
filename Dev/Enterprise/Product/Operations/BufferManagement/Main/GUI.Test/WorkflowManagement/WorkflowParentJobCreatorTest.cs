using CargoWise.Definitions;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.BufferManagement.GUI.Test
{
	class WorkflowParentJobCreatorTest : BMSTestCaseWithFactory
	{
		public void TestNullJobForm()
		{
			Env.Security.OrganisationNew.IsAllowed = false;

			var jobCreator = new WorkflowParentJobCreator("ORG");

			jobCreator.CreateJob(Factory);
			AssertExceptionThrown<SecurityAccessDeniedException>("A new form should not have been created.", () => jobCreator.ShowNewForm(() => { }));
		}

		public void TestPopulatePropertiesOnSubDiagramJob()
		{
			using (OverrideClientAssemblyAndSecurityCheckpointsForTest(Clients.EDI))
			{
				var workItem = Factory.New<IWorkItem>();
				workItem.WKI_SystemCreateUser = GlbStaff.CurrentUser.GS_Code;
				workItem.WKI_ActivityType = "UDF";
				workItem.WKI_WorkItemType = "UDF";

				var jobHeader = ProcessJobHeader.GetForParent((IWorkflowProvider)workItem, Factory, false);
				var mainDiagram = NetworkTestCase.CreateDiagram(jobHeader);

				var subDiagram = NetworkTestCase.CreateShape(mainDiagram);
				subDiagram.BNS_JobType = "WKI";

				var networkViewModel = NetworkTestCase.CreateNetworkViewModel(mainDiagram);
				var network = networkViewModel.GetJobNetwork();

				Factory.Save();

				var fireSaveButtonInvoker = new ZFormModaliser.PreShowInvoker(form =>
				{
					var zForm = (ZForm)form;
					((IWorkItem)zForm.BusinessEntity).WKI_Summary = "Test Description";
					zForm.FireSaveButton();
				});
				ZFormModaliser.SetDelegateToCallBeforeShowingFormsOrDialogs(fireSaveButtonInvoker);

				var action = new CreateJobAction(networkViewModel, linkOnly: true);
				action.ExecuteForEntityWithoutAccessCheck(subDiagram);
				Factory.Save();

				AssertNotNull("ProcessHeader", subDiagram.ProcessHeader);
				AssertEquals("Parent for ProcessHeader should exist", false, subDiagram.ProcessHeader.FH_ParentId.IsEmpty);
				var subWorkItem = Factory.Load<IWorkItem>(subDiagram.ProcessHeader.FH_ParentId);
				AssertNotNull("subWorkItem", subWorkItem);

				AssertEquals(workItem.WKI_ActivityType, subWorkItem.WKI_ActivityType);
				AssertEquals(workItem.WKI_WorkItemType, subWorkItem.WKI_WorkItemType);
			}
		}
	}
}
