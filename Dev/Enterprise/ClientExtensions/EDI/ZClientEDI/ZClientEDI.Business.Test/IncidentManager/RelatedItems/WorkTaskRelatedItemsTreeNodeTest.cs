using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business.Test;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	public class WorkTaskRelatedItemsTreeNodeTest : TestCaseWithFactory
	{
		readonly ZDateTime agreedDeliveryDate = ZDateTime.Now;

		public void TestWorkTaskRelatedItemsTreeNodeProperties()
		{
			var workItem = CreateWorkItem();
			var treeModel = new WorkTaskRelatedItemsTreeModel(workItem);
			var node1 = (WorkTaskRelatedItemsTreeNode)treeModel.RootNodes.First().ChildNodes.First();
			AssertEquals("WKP PJ00000001", node1.Group);
			AssertEquals("Closed as Completed", node1.StatusDescription);
			AssertEquals("Project", node1.Type);
			AssertEquals("PJ Description", node1.Description);
			AssertEquals("THEORG", node1.OrganisationCode);
			AssertEquals("Organization Name", node1.OrganisationName);
			AssertEquals(agreedDeliveryDate, node1.AgreedDeliveryDate.ToUniversalBranchTime());
			AssertEquals("WRK", node1.CurrentTaskStatus);
			AssertEquals("Task test", node1.CurrentTaskDescription);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, node1.CurrentTaskAssigned);
			AssertEquals("686 - Capability desc", node1.CurrentTaskCapabilityCodeDescription);
			AssertEquals("1AA", node1.SelectionCriterion1Code);
			AssertEquals("2AA", node1.SelectionCriterion2Code);
			AssertEquals("3AA", node1.SelectionCriterion3Code);
			AssertEquals("LOW", node1.SelectionCriterion4Code);
			AssertEquals(string.Empty, node1.SelectionCriterion5Code);
			AssertEquals(true, node1.IsClosedOrCancelled);
		}

		EDIWorkItem CreateWorkItem()
		{
			var workItemParent = Factory.NewWithValidTestData<EDIWorkItem>();
			workItemParent.WKI_WorkItemNumber = "WI00000001";

			var project1 = workItemParent.RelatedItems.AddNew(typeof(EDIProject)) as EDIProject;
			project1.WKP_ProjectNumber = "PJ00000001";
			SetupProject(project1, agreedDeliveryDate);

			var project2 = workItemParent.RelatedItems.AddNew(typeof(EDIProject)) as EDIProject;
			project2.WKP_ProjectNumber = "PJ00000002";

			project1.RelatedItems.AddNew(typeof(EDIWorkItem));
			project1.RelatedItems.AddNew(typeof(EDIWorkItem));
			project2.RelatedItems.AddNew(typeof(EDIWorkItem));

			return workItemParent;
		}

		void SetupProject(EDIProject project, ZDateTime agreedDeliveryDate)
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, project.WorkflowType);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Capability desc";

			var jobHeader = helper.GetJobHeaderForParent(project, Factory, false);
			var workflow = helper.CreateWorkflow(jobHeader, "Exported Workflow");
			var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 2, description: "Be exported");
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_Description = "Task test";
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			project.WKP_ProjectNumber = "PJ00000001";
			project.WKP_Status = ProcessTaskStatusCodeList.Codes.Closed;
			project.WKP_Summary = "PJ Description";
			project.JobWorkflow.FH_AgreedDeliveryDate = agreedDeliveryDate;
			project.WKP_Type = "1AA";
			project.WKP_SubType = "2AA";
			project.WKP_Module = "3AA";
			project.WKP_Priority = "LOW";

			var organization = ProcessMgmtTestHelper.CreateOrganizationAndContact(Factory).ParentOrg;
			organization.OH_Code = "THEORG";
			organization.OH_FullName = "Organization Name";
			project.ClientOrganisationPK = organization.PK;
		}
	}
}
