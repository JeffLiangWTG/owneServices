using CargoWise.EntityFramework.Testing;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Service.Shared.Templates.Dtos;
using Enterprise.BufferManagement.Service.Templates;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ProcessManagement.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Service.Test
{
	public class TemplateServiceTest : TestCaseWithFactory
	{
		TemplateService Service;

		protected override void SetUp()
		{
			base.SetUp();
			BMSTestHelper.EnableBMSInRegistry();
			Globals.IsUserInteractive = false;
			TestConfigsHelper.CreateSchematicTestConfig(Factory);
			Service = new TemplateService();
		}

		protected override void TearDown()
		{
			MasterFilesTestHelper.ClearIterationReasonsFromRegistry("ORG");
			base.TearDown();
		}

		public void TestApplyTaskNoteTemplate()
		{
			MasterFilesTestHelper.AddTaskTypesToRegistry(WorkflowDescriptors.WorkItemWorkflowDescriptorCode, "OPA");
			var workItem = (IWorkItem)BMSTestHelper.CreateJob<IWorkItem>(Factory);
			workItem.WKI_WorkItemNumber = "WI00000101";
			workItem.WKI_Summary = "O loco Meu!";

			var jobHeader = BMSTestHelper.CreateJobHeader((IWorkflowProvider)workItem);
			var workflow = BMSTestHelper.CreateWorkflow(jobHeader, "Day");
			var task = BMSTestHelper.CreateTask(workflow, taskType: "OPA", sequence: 1, description: "Dessa vida nada se leva só a vida que se leva!");

			Factory.Save();

			var template =
@"Number: <WKI_WorkItemNumber>
Summary: <WKI_Summary>

TASK
Type: <P9_Type>
Sequence: <P9_Sequence>
Description: <P9_Description>

Something empty: <noopp>


more things here";

			var result = Service.ApplyTaskNoteTemplate(task.PK.ToGuid(), new ApplyTemplateRequest
			{
				Template = template
			});

			AssertEquals(result,
@"Number: WI00000101
Summary: O loco Meu!

TASK
Type: OPA
Sequence: 1
Description: Dessa vida nada se leva só a vida que se leva!

Something empty: 


more things here");
		}

		public void TestApplyWorkItemDescriptionTemplate()
		{
			var service = new TemplateService();
			var workItem = (IWorkItem)BMSTestHelper.CreateJob<IWorkItem>(Factory);
			workItem.WKI_WorkItemNumber = "WI00000101";
			workItem.WKI_WorkItemType = "BLA";
			workItem.WKI_Summary = "Summary summary summary";

			var jobHeader = BMSTestHelper.CreateJobHeader((IWorkflowProvider)workItem);

			Factory.Save();

			var template = "This is a template! <WKI_WorkItemNumber> <WKI_Summary> " +
				"And also the WI type: <WKI_WorkItemType>";

			var result = service.ApplyWorkItemNoteTemplate(workItem.PK.ToGuid(), new ApplyTemplateRequest
			{
				Template = template
			});

			AssertEquals($"This is a template! {workItem.WKI_WorkItemNumber} {workItem.WKI_Summary} " +
				$"And also the WI type: {workItem.WKI_WorkItemType}", result);
		}
	}
}
