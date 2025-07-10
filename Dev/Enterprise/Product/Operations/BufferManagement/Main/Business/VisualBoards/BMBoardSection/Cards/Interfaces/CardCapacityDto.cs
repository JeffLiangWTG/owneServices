using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	class CardCapacityDto : ICardCapacityDto
	{
		internal CardCapacityDto(ProcessTask task, ProcessHeader workflow, BMBoardSectionViewModel viewModel)
		{
			RelevantEstimateHours = task.RelevantEstimateHours;
			AssignedResourceCode = task.P9_GS_NKAssignedStaffMember;

			if (viewModel != null && viewModel.IsInConstrainedMode)
			{
				IsInCCRWorkflow = task.IsInWorkflowWithCCR();
				PenetratedComponentsPK = workflow.GetPenetratedComponents(task).Select(c => c.PK).ToArray();
				IsAssignedToCCR = ConstrainedModeHelper.IsDesignatedCapacityConstrainedResource(task.Factory, AssignedResourceCode, viewModel.ComponentPK);
			}
			else
			{
				PenetratedComponentsPK = Enumerable.Empty<ZGuid>();
			}

			if (workflow != null && viewModel != null)
			{
				AssignedStaff = workflow.GetAssignedStaff(viewModel.Cache);
				ComponentPK = workflow.FH_FC_CurrentComponent;
			}

			Sequence = task.P9_Sequence;
		}

		public int Sequence { get; }
		public bool IsAssignedToCCR { get; }
		public IEnumerable<ZGuid> PenetratedComponentsPK { get; }
		public bool IsInCCRWorkflow { get; }

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:UseTimespanForDuration", Justification = "Baseline")]
		public decimal RelevantEstimateHours { get; }
		public ZString AssignedResourceCode { get; }
		public ZGuid ComponentPK { get; }
		public IEnumerable<ZString> AssignedStaff { get; }
	}
}
