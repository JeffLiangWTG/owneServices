using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.VisualBoards.Business.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	public class SubCmptHeadingFadeTest
	{
		public readonly List<WorkflowAging> WorkflowAgings = new List<WorkflowAging>();
		public readonly List<VisualBoardPositionForTest<int>> ExpectedFadePositions = new List<VisualBoardPositionForTest<int>>();

		public SubCmptHeadingFadeTest(IEnumerable<WorkflowAging> workflowAging, IEnumerable<VisualBoardPositionForTest<int>> expectedFadePositions)
		{
			if (workflowAging != null)
			{
				WorkflowAgings.AddRange(workflowAging);
			}

			if (expectedFadePositions != null)
			{
				ExpectedFadePositions.AddRange(expectedFadePositions);
			}
		}
	}

	public class WorkflowAging
	{
		public readonly ZGuid WorkflowPK;
		public readonly ZInt? AgingInDays;

		public WorkflowAging(ZGuid workflowPK, ZInt? agingInDays)
		{
			WorkflowPK = workflowPK;
			AgingInDays = agingInDays;
		}
	}
}
