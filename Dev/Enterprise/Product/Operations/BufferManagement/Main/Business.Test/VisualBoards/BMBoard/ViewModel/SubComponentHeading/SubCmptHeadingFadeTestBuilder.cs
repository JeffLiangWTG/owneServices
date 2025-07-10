using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.VisualBoards.Business.Test;

namespace Enterprise.BufferManagement.Business.Test
{
	public class SubCmptHeadingFadeTestBuilder
	{
		readonly List<VisualBoardPositionForTest<int>> expectedFadePositions = new List<VisualBoardPositionForTest<int>>();
		readonly List<WorkflowAging> workflowAgings = new List<WorkflowAging>();

		public SubCmptHeadingFadeTest Build()
		{
			return new SubCmptHeadingFadeTest(workflowAgings, expectedFadePositions);
		}

		public SubCmptHeadingFadeTestBuilder WithAging(ZGuid workflowPK, ZInt agingInDays)
		{
			this.workflowAgings.Add(new WorkflowAging(workflowPK, agingInDays));
			return this;
		}

		public SubCmptHeadingFadeTestBuilder ExpectFadeAt(int primaryAxis = -1, int secondaryAxis = -1)
		{
			this.expectedFadePositions.Add(new VisualBoardPositionForTest<int>(primaryAxis, secondaryAxis));
			return this;
		}

		public static implicit operator SubCmptHeadingFadeTest(SubCmptHeadingFadeTestBuilder instance)
		{
			return instance.Build();
		}
	}
}
