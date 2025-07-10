namespace Enterprise.BufferManagement.Business.Test
{
	public class ComponentGridBasherTestCaseBuilder
	{
		public virtual SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(4, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(4, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(4, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(4, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(4, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(4, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(4, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(4, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(4, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(4, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(4, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(4, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(4, 12),
			};
		}

		public virtual SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(5, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(5, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(5, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(5, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(5, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(5, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(5, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(5, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(5, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(5, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(5, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(5, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(5, 12),
			};
		}

		public virtual SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 0)
					.WithAging(postCCRWorkflow.PK, 12)
					.ExpectFadeAt(4, 1)
					.ExpectFadeAt(5, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 1)
					.WithAging(postCCRWorkflow.PK, 12)
					.ExpectFadeAt(4, 1)
					.ExpectFadeAt(5, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 2)
					.WithAging(postCCRWorkflow.PK, 10)
					.ExpectFadeAt(4, 2)
					.ExpectFadeAt(5, 10),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 3)
					.WithAging(postCCRWorkflow.PK, 9)
					.ExpectFadeAt(4, 3)
					.ExpectFadeAt(5, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 4)
					.WithAging(postCCRWorkflow.PK, 8)
					.ExpectFadeAt(4, 4)
					.ExpectFadeAt(5, 8),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 5)
					.WithAging(postCCRWorkflow.PK, 7)
					.ExpectFadeAt(4, 5)
					.ExpectFadeAt(5, 7),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 6)
					.WithAging(postCCRWorkflow.PK, 6)
					.ExpectFadeAt(4, 6)
					.ExpectFadeAt(5, 6),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 7)
					.WithAging(postCCRWorkflow.PK, 5)
					.ExpectFadeAt(4, 7)
					.ExpectFadeAt(5, 5),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 8)
					.WithAging(postCCRWorkflow.PK, 4)
					.ExpectFadeAt(4, 8)
					.ExpectFadeAt(5, 4),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 9)
					.WithAging(postCCRWorkflow.PK, 3)
					.ExpectFadeAt(4, 9)
					.ExpectFadeAt(5, 3),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 10)
					.WithAging(postCCRWorkflow.PK, 2)
					.ExpectFadeAt(4, 10)
					.ExpectFadeAt(5, 2),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 11)
					.WithAging(postCCRWorkflow.PK, 1)
					.ExpectFadeAt(4, 11)
					.ExpectFadeAt(5, 1),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 12)
					.WithAging(postCCRWorkflow.PK, 0)
					.ExpectFadeAt(4, 12)
					.ExpectFadeAt(5, 1),
			};
		}
	}
}
