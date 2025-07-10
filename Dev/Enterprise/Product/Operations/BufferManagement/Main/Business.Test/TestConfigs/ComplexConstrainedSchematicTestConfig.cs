using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.VisualBoards.Business.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	public class ComplexConstrainedSchematicTestConfig : ConstrainedSchematicTestConfig
	{
		protected ComplexConstrainedSchematicTestConfig(BusinessObjectFactory factory, string workflowType, bool makeResourcesPartOfReleaseGroup, bool createWorkflowsAndTasks, bool shouldUseExistingSystem)
			: base(factory, workflowType, makeResourcesPartOfReleaseGroup, shouldUseExistingSystem)
		{
			Staffs.Add(CCR);
			Staffs.Add(NonCCR1);
			Staffs.Add(NonCCR2);
			Board = VisualBoardsTestHelper.CreateBoard(System);
			Section = VisualBoardsTestHelper.CreateBoardSection(component: Buffer, board: Board, row: 0, col: 0, rowHeightPercent: 0, colWidthPercent: 0);

			var sectionConfiguration = Section.SectionConfiguration;
			sectionConfiguration.Subsections = 1;
			sectionConfiguration.CellsPerSubsection = 13;
			sectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
			sectionConfiguration.LastCell = LastCellList.Codes.Right;
			sectionConfiguration.ChannelBy = ChannelTypeList.Codes.Resource;
			sectionConfiguration.ReleaseGroupPK = ReleaseGroup.PK;
			sectionConfiguration.OverrideChannels = true;

			foreach (var staff in Staffs)
			{
				BMSTestHelper.CreatePrimaryChannelForSection(Section, ChannelTypeList.Codes.Resource, staff.PK, overrideChannels: true);
			}

			Section.SectionConfiguration.FadeBackgroundAtPercentage = 80;

			if (createWorkflowsAndTasks)
			{
				CreateWorkflowsAndTasks(factory);
			}
		}

		public static ComplexConstrainedSchematicTestConfig Create(BusinessObjectFactory factory, string workflowType, bool makeResourcesPartOfReleaseGroup, bool createWorkflowsAndTasks, bool shouldUseExistingSystem)
		{
			return new ComplexConstrainedSchematicTestConfig(factory, workflowType, makeResourcesPartOfReleaseGroup, createWorkflowsAndTasks, shouldUseExistingSystem);
		}

		public BMBoardSectionViewModel ResetViewModel()
		{
			sectionViewModel = null;
			return SectionViewModel;
		}

		#region Implementation

		void AllocateTasks(BMBoardSectionViewModel sectionViewModel)
		{
			var tasks = Workflows.SelectMany(w => w.Parent.WorkflowItems.Where(t => t.IsOpen).Select(t => t));
			sectionViewModel.ComponentGrid.AllocateTasks_ForTest(Section, sectionViewModel, tasks.ToArray());
		}

		void CreateWorkflowsAndTasks(BusinessObjectFactory factory)
		{
			foreach (var staff in new[] { CCR, NonCCR1, NonCCR2 })
			{
				var completionStatement = string.Format("workflow for {0}", staff.GS_Code);
				var workflow = BMSTestHelper.CreateWorkflowAndTask(factory, completionStatement: completionStatement, currentComponent: Buffer, releaseDateTime: ZDateTime.Now, staffCode: staff.GS_Code, lowEstMinutes: 15, description: "complex task for " + staff.GS_Code);
				Workflows.Add(workflow);
			}
		}

		#endregion

		public BMBoardSection Section { get; private set; }

		public BMBoard Board { get; private set; }

		public BMBoardSectionViewModel SectionViewModel
		{
			get
			{
				if (sectionViewModel == null)
				{
					sectionViewModel = BMSTestHelper.CreateViewModel(Section);
					AllocateTasks(sectionViewModel);
				}
				return sectionViewModel;
			}
		}
		BMBoardSectionViewModel sectionViewModel;

		public List<GlbStaff> Staffs
		{
			get { return staffs; }
		}
		readonly List<GlbStaff> staffs = new List<GlbStaff>();

		public List<ProcessHeader> Workflows
		{
			get { return workflows; }
		}
		readonly List<ProcessHeader> workflows = new List<ProcessHeader>();
	}
}
