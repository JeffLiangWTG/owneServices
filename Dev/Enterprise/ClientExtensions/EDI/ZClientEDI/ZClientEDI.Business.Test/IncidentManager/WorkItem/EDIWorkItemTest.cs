using System.Data;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	abstract class EDIWorkItemRelatedItemTestCase : WorkItemRelatedItemTestCase
	{
	}

	[TestedType(typeof(EDIWorkItem))]
	public class EDIWorkItemTest : WorkItemCommonTest<EDIWorkItem>
	{
		public void TestEDTLogReferenceIsUpdated()
		{
			var workItem = Factory.New<EDIWorkItem>();
			workItem.WKI_WorkItemType = "ENT";
			workItem.WKI_WorkItemArea = "ARC";
			workItem.WKI_ActivityType = "COR";
			Factory.Save();

			workItem.WKI_WorkItemType = "ENT";
			workItem.WKI_WorkItemArea = "PAV";
			workItem.WKI_ActivityType = "BUF";
			Factory.Save();

			Assert("References message should match", workItem.Logs.HasLogWith(StmALogSchema.SL_Reference, "Reassigned from ENT/ARC/COR to ENT/PAV/BUF"));
		}

		#region IWorkTaskTreeNode

		public void TestIWorkTaskTreeNodeMembers()
		{
			var workItem = Factory.NewWithValidTestData<WorkItemForTest>();
			var project = Factory.NewWithValidTestData<EDIProject>();
			var project2 = Factory.NewWithValidTestData<EDIProject>();
			var parentProject = Factory.NewWithValidTestData<EDIProject>();
			var agreedDeliveryDate = ZDateTime.Now;
			SetupWorkItem(workItem, agreedDeliveryDate, project, project2, parentProject);

			AssertEquals(agreedDeliveryDate, workItem.AgreedDeliveryDate.ToUniversalBranchTime());
			AssertEquals("Task test", workItem.CurrentTaskDescription);
			AssertEquals("686 - Capability desc", workItem.CurrentTaskCapabilityCodeDescription);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, workItem.CurrentTaskAssigned);

			AssertEquals(2, workItem.ChildrenOnlyRelatedItems.Count);
			AssertEquals(1, workItem.ChildrenOnlyRelatedItems.Count(a => a.PK == project.PK));
			AssertEquals(1, workItem.ChildrenOnlyRelatedItems.Count(a => a.PK == project2.PK));
			AssertEquals(1, workItem.ParentsOnlyRelatedItems.Count);
			AssertEquals(parentProject.PK, workItem.ParentsOnlyRelatedItems.First().PK);
		}

		void SetupWorkItem(WorkItemForTest workItem, ZDateTime agreedDeliveryDate, EDIProject project, EDIProject project2, EDIProject parentProject)
		{
			var helper = ObjectFactory.Get<IBMTestHelper>();
			helper.EnableBMSInRegistry();
			helper.CreateSystem(Factory, workItem.WorkflowType);

			var capability = Factory.NewWithValidTestData<GlbCapability>();
			capability.G4_Description = "Capability desc";

			var jobHeader = helper.GetJobHeaderForParent(workItem, Factory, addDefaultProcessHeaderIfNone: false);
			var workflow = helper.CreateWorkflow(jobHeader, "Exported Workflow");
			var task = (ProcessTask)helper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 1, taskStatus: ProcessTaskStatusCodeList.Codes.Working, sequence: 2, description: "Be exported");
			task.P9_G4_RequiredCapability = capability.PK;
			task.P9_Description = "Task test";

			workItem.JobWorkflowExposed.FH_AgreedDeliveryDate = agreedDeliveryDate;
			workItem.RelatedItems.Add(project);
			workItem.RelatedItems.Add(project2);
			parentProject.RelatedItems.Add(workItem);
		}

		class WorkItemForTest : EDIWorkItem
		{
			public WorkItemForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public IProcessJobHeader JobWorkflowExposed => JobWorkflow;
		}

		#endregion
	}

	[TestedType(typeof(EDIWorkItem))]
	sealed class EDIWorkItemRelatedItemTest : EDIWorkItemRelatedItemTestCase
	{
	}

	public abstract class EDIWorkItemRelatedItemSourceTestCase : WorkItemRelatedItemSourceTestCase
	{
	}

	[TestedType(typeof(EDIWorkItem))]
	sealed class EDIWorkItemRelatedItemSourceTest : EDIWorkItemRelatedItemSourceTestCase
	{
	}
}
