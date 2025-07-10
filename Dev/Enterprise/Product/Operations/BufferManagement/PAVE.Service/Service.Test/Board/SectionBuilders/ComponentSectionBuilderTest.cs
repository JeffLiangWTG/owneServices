using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.PAVE.Common.DTO;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.BufferManagement.Integration;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business.Test;
using Enterprise.MasterFiles.Business;
using Enterprise.ProcessManagement.Integration;
using Enterprise.VisualBoards.Business.Test;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Service.Test
{
	public class ComponentSectionBuilderTest : ComponentSectionBuilderTestBase
	{
		#region Task

		[TestDate(2019, 1, 1)]
		public void TestWorkflowIndex_WithCCPM_Workflow_ShouldReturnCorrectValue()
		{
			var config = TestConfigsHelper.CreateSchematicTestConfig(Factory);
			var resourceWithTasks = BMSTestHelper.CreateStaffInCurrentBranchDept(Factory, "AAA", "Resource with tasks");
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow_shownOnDiagram = BMSTestHelper.CreateWorkflow(jobHeader, "Workflow the First", config.Buffer, releaseGroupPK: config.ReleaseGroup.PK);
			BMSTestHelper.CreateTask(workflow_shownOnDiagram, staffCode: "AAA", lowEstMinutes: 60 * BMConstants.WorkingHoursPerDay, estVariationFactor: 1);

			var diagram = NetworkTestCase.CreateDiagram(jobHeader);
			var shape = NetworkTestCase.CreateShape(workflow_shownOnDiagram, diagram);
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow.AddHours(-2);
			var networkViewModel = NetworkTestCase.CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			networkViewModel.SuggestAndAcceptAllBuffers();
			networkViewModel.ToggleApproval();

			Factory.Save();

			using (DummySecondaryServerConnectionProvider.TemporarilyEnableDummyProvider())
			{
				BMSTestCaseWithFactory.RunCCPMAndNCNTagRules(Factory);
				NetworkTestCase.RunBufferPenetrationUpdater();
			}

			NetworkTestCase.AssertCcpmRtrTagApplied(jobHeader);

			var board = config.System.Boards.AddNew();
			var bufferSection = board.Sections.AddNew();
			var sectionPK = bufferSection.PK.ToGuid();

			bufferSection.MS_FC_Component = config.Buffer.PK;

			VisualBoardsTestHelper.CreatePrimaryChannelForSection(bufferSection, ChannelTypeList.Codes.Resource, resourceWithTasks.PK);

			Factory.Save();

			ISection sectionDTO = null;

			using (VisualBoardsTestCase.DisableAsyncBehaviour())
			{
				sectionDTO = BuildSectionDTO(bufferSection);
			}

			var componentSectionDTO = sectionDTO as ComponentSectionDTO;
			var index = componentSectionDTO?.Workflows.First(workflowDTO => workflowDTO.PK == workflow_shownOnDiagram.PK.ToGuid()).Index;

			AssertEquals("Index should be 25%: 2 hours consumed of 8", 25.00m, index);
		}

		#endregion

		#region Jobs

		[TestDate(2020, 3, 26)]
		public void TestGetData_ShouldNotThrowException_WhenNoJobIsFound()
		{
			var system = BMSTestHelper.CreateSystem(Factory, "WKI");
			system.FS_Name = "System";

			var buffer = BMSTestHelper.CreateBuffer(system, "buffer", 300);
			var board = system.Boards.AddNew();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "ST1";

			var bufferSection = board.Sections.AddNew();
			bufferSection.MS_FC_Component = buffer.PK;

			var workItem1 = Factory.New<IWorkItem>();
			var header1 = ProcessJobHeader.GetForParent(workItem1 as IWorkflowProvider, Factory);
			var workflow1 = BMSTestHelper.CreateProcessHeader(header1, buffer, "Workflow 1", TestDateAttribute.Date.AddDays(-9));
			var task1 = VisualBoardsTestCase.CreateTask(workflow1, staff1.GS_Code, 300, taskStatus: ProcessTaskStatusCodeList.Codes.Open, sequence: 1, description: "Task 1");

			Factory.Save();

			var deletedCount = TestConnection.ExecuteNonQuery($"delete dbo.WorkItem where WKI_PK = '{workItem1.PK}'");
			AssertEquals("Should have deleted 1 WorkItem", 1, deletedCount);

			var newFactory = new BusinessObjectFactory();
			bufferSection = newFactory.Load<BMBoardSection>(bufferSection.PK);

			var serviceSectionDTO = BuildSectionDTO(bufferSection) as ComponentSectionDTO;

			AssertNull("WorkItem was deleted, properties should be null", serviceSectionDTO.Jobs.First().Properties);
			AssertEquals("Should have loaded workflow", workflow1.PK, serviceSectionDTO.Workflows.First().PK);
			AssertEquals("Should have loaded task", task1.PK, serviceSectionDTO.Tasks.First().PK);
		}

		#endregion

		protected override ISection BuildSectionDTO(BMBoardSection section) => new ComponentSectionBuilder(section).Build();
	}
}
