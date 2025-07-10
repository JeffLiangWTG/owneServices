using System.Data;
using System.Text;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(WorkTaskRelatedItemsTreeWrapper))]
	public class WorkTaskRelatedItemsTreeWrapperTest : NonPersistentBusinessObjectTestCase
	{
		readonly ZDateTime agreedDeliveryDate = ZDateTime.Now;

		public void TestTreeWrapperProperties()
		{
			var wrapper = (WorkTaskRelatedItemsTreeWrapper)GetNewBusinessObject();

			AssertEquals("WKI WI00000001", wrapper.Group);
			AssertEquals("Assigned", wrapper.StatusDescription);
			AssertEquals("Work Item", wrapper.Type);
			AssertEquals("Value cannot be null.Parameter name: source", wrapper.Description);
			AssertEquals("", wrapper.OrganisationCode);
			AssertEquals("", wrapper.OrganisationName);
			AssertEquals(agreedDeliveryDate, wrapper.AgreedDeliveryDate.ToUniversalBranchTime());
			AssertEquals("WRK", wrapper.CurrentTaskStatus);
			AssertEquals("Task test", wrapper.CurrentTaskDescription);
			AssertEquals(GlbStaff.CurrentUser.GS_Code, wrapper.CurrentTaskAssigned);
			AssertEquals("686 - Capability desc", wrapper.CurrentTaskCapabilityCodeDescription);
			AssertEquals("1AA", wrapper.SelectionCriterion1Code);
			AssertEquals("2AA", wrapper.SelectionCriterion2Code);
			AssertEquals("3AA", wrapper.SelectionCriterion3Code);
			AssertEquals("4AA", wrapper.SelectionCriterion4Code);
			AssertEquals("LOW", wrapper.SelectionCriterion5Code);
			AssertEquals(false, wrapper.IsClosedOrCancelled);
		}

		protected override BusinessObject GetNewBusinessObject()
		{
			var workItemParent = Factory.NewWithValidTestData<WorkItemForTest>();
			workItemParent.WKI_WorkItemNumber = "WI00000001";
			SetupWorkItem(workItemParent, agreedDeliveryDate);

			var treeModel = new WorkTaskRelatedItemsTreeModel(workItemParent);
			return new WorkTaskRelatedItemsTreeWrapper(treeModel, workItemParent, null, false);
		}

		void SetupWorkItem(WorkItemForTest workItem, ZDateTime agreedDeliveryDate)
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
			task.P9_Status = ProcessTaskStatusCodeList.Codes.Working;

			var sbDescriptionWithNewLine = new StringBuilder();
			sbDescriptionWithNewLine.AppendLine("Value cannot be null.");
			sbDescriptionWithNewLine.AppendLine("Parameter name: source");

			workItem.WKI_WorkItemNumber = "WI00000001";
			workItem.WKI_Status = ProcessTaskStatusCodeList.Codes.Assigned;
			workItem.WKI_Summary = sbDescriptionWithNewLine.ToString();
			workItem.JobWorkflowExposed.FH_AgreedDeliveryDate = agreedDeliveryDate;
			workItem.WKI_WorkItemType = "1AA";
			workItem.WKI_WorkItemArea = "2AA";
			workItem.WKI_ActivityType = "3AA";
			workItem.WKI_ActivitySubtype = "4AA";
			workItem.WKI_Priority = "LOW";
		}

		class WorkItemForTest : EDIWorkItem
		{
			public WorkItemForTest(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public IProcessJobHeader JobWorkflowExposed => JobWorkflow;
		}
	}
}
