namespace Enterprise.BufferManagement.Business.Test
{
	#region Hidden Sub Components
	public class ComponentGridBasherStackedSubComponentTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(4, 1).ExpectFadeAt(6, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(4, 1).ExpectFadeAt(6, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(4, 2).ExpectFadeAt(6, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(4, 3).ExpectFadeAt(6, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(4, 4).ExpectFadeAt(6, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(4, 5).ExpectFadeAt(6, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(4, 6).ExpectFadeAt(6, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(4, 7).ExpectFadeAt(6, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(4, 8).ExpectFadeAt(6, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(4, 9).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(4, 10).ExpectFadeAt(6, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(4, 11).ExpectFadeAt(6, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(4, 12).ExpectFadeAt(6, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(5, 1).ExpectFadeAt(7, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(5, 1).ExpectFadeAt(7, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(5, 2).ExpectFadeAt(7, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(5, 3).ExpectFadeAt(7, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(5, 4).ExpectFadeAt(7, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(5, 5).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(5, 6).ExpectFadeAt(7, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(5, 7).ExpectFadeAt(7, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(5, 8).ExpectFadeAt(7, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(5, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(5, 10).ExpectFadeAt(7, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(5, 11).ExpectFadeAt(7, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(5, 12).ExpectFadeAt(7, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 0)
					.WithAging(postCCRWorkflow.PK, 12)
					.ExpectFadeAt(4, 1)
					.ExpectFadeAt(5, 12)
					.ExpectFadeAt(6, 1)
					.ExpectFadeAt(7, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 1)
					.WithAging(postCCRWorkflow.PK, 12)
					.ExpectFadeAt(4, 1)
					.ExpectFadeAt(5, 12)
					.ExpectFadeAt(6, 1)
					.ExpectFadeAt(7, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 2)
					.WithAging(postCCRWorkflow.PK, 10)
					.ExpectFadeAt(4, 2)
					.ExpectFadeAt(5, 10)
					.ExpectFadeAt(6, 2)
					.ExpectFadeAt(7, 10),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 3)
					.WithAging(postCCRWorkflow.PK, 9)
					.ExpectFadeAt(4, 3)
					.ExpectFadeAt(5, 9)
					.ExpectFadeAt(6, 3)
					.ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 4)
					.WithAging(postCCRWorkflow.PK, 8)
					.ExpectFadeAt(4, 4)
					.ExpectFadeAt(5, 8)
					.ExpectFadeAt(6, 4)
					.ExpectFadeAt(7, 8),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 5)
					.WithAging(postCCRWorkflow.PK, 7)
					.ExpectFadeAt(4, 5)
					.ExpectFadeAt(5, 7)
					.ExpectFadeAt(6, 5)
					.ExpectFadeAt(7, 7),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 6)
					.WithAging(postCCRWorkflow.PK, 6)
					.ExpectFadeAt(4, 6)
					.ExpectFadeAt(5, 6)
					.ExpectFadeAt(6, 6)
					.ExpectFadeAt(7, 6),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 7)
					.WithAging(postCCRWorkflow.PK, 5)
					.ExpectFadeAt(4, 7)
					.ExpectFadeAt(5, 5)
					.ExpectFadeAt(6, 7)
					.ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 8)
					.WithAging(postCCRWorkflow.PK, 4)
					.ExpectFadeAt(4, 8)
					.ExpectFadeAt(5, 4)
					.ExpectFadeAt(6, 8)
					.ExpectFadeAt(7, 4),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 9)
					.WithAging(postCCRWorkflow.PK, 3)
					.ExpectFadeAt(4, 9)
					.ExpectFadeAt(5, 3)
					.ExpectFadeAt(6, 9)
					.ExpectFadeAt(7, 3),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 10)
					.WithAging(postCCRWorkflow.PK, 2)
					.ExpectFadeAt(4, 10)
					.ExpectFadeAt(5, 2)
					.ExpectFadeAt(6, 10)
					.ExpectFadeAt(7, 2),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 11)
					.WithAging(postCCRWorkflow.PK, 1)
					.ExpectFadeAt(4, 11)
					.ExpectFadeAt(5, 1)
					.ExpectFadeAt(6, 11)
					.ExpectFadeAt(7, 1),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 12)
					.WithAging(postCCRWorkflow.PK, 0)
					.ExpectFadeAt(4, 12)
					.ExpectFadeAt(5, 1)
					.ExpectFadeAt(6, 12)
					.ExpectFadeAt(7, 1),
			};
		}

		#endregion
	}

	public class ComponentGridBasherSubComponentWithGapTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 0)
					.WithAging(postCCRWorkflow.PK, 12)
					.ExpectFadeAt(4, 1)
					.ExpectFadeAt(5, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 1)
					.WithAging(postCCRWorkflow.PK, 11)
					.ExpectFadeAt(4, 1)
					.ExpectFadeAt(5, 11),
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

		#endregion

		#region Implementation

		readonly int subCmptPrimaryAxis = 4;

		#endregion
	}

	public class ComponentGridBasherSubComponentWith11DaysPreCCRBufferAnd1DayPostCCRBufferTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis + 1, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis + 1, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis + 1, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis + 1, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis + 1, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis + 1, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis + 1, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis + 1, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis + 1, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis + 1, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis + 1, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis + 1, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis + 1, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
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

		#endregion

		#region Implementation

		readonly int subCmptPrimaryAxis = 4;

		#endregion
	}

	public class ComponentGridBasherPreConstraintsOverlappedTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			// Both subcomponents are considered pre-CCR components and should fade
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(4, 1).ExpectFadeAt(5, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(4, 1).ExpectFadeAt(5, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(4, 2).ExpectFadeAt(5, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(4, 3).ExpectFadeAt(5, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(4, 4).ExpectFadeAt(5, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(4, 5).ExpectFadeAt(5, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(4, 6).ExpectFadeAt(5, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(4, 7).ExpectFadeAt(5, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(4, 8).ExpectFadeAt(5, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(4, 9).ExpectFadeAt(5, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(4, 10).ExpectFadeAt(5, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(4, 11).ExpectFadeAt(5, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(4, 12).ExpectFadeAt(5, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			// Both subcomponents are considered pre-CCR components so it will behave the same as GetTestsForSubComponentZoneHeadingFade_PreCCRAging i.e. ignoring post-CCR workflow
			return GetTestsForSubComponentZoneHeadingFade_PreCCRAging(preCCRWorkflow);
		}

		#endregion
	}

	public class ComponentGridBasherSubComponentOverlappedTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			// Pre-CCR subcomponent Secondary-axis 1 and 2 does not exist because its Offset = 2 * 8 * 60
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

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
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

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
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

		#endregion
	}

	public class ComponentGridBasherSubComponentWithMultiplePostConstraintBuffersTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			var subCmptPrimaryAxis = 4;

			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(5, 1).ExpectFadeAt(6, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(5, 1).ExpectFadeAt(6, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(5, 2).ExpectFadeAt(6, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(5, 3).ExpectFadeAt(6, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(5, 4).ExpectFadeAt(6, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(5, 5).ExpectFadeAt(6, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(5, 6).ExpectFadeAt(6, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(5, 7).ExpectFadeAt(6, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(5, 8).ExpectFadeAt(6, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(5, 9).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(5, 10).ExpectFadeAt(6, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(5, 11).ExpectFadeAt(6, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(5, 12).ExpectFadeAt(6, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 0)
					.WithAging(postCCRWorkflow.PK, 12)
					.ExpectFadeAt(4, 1)
					.ExpectFadeAt(5, 12)
					.ExpectFadeAt(6, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 1)
					.WithAging(postCCRWorkflow.PK, 11)
					.ExpectFadeAt(4, 1)
					.ExpectFadeAt(5, 11)
					.ExpectFadeAt(6, 11),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 2)
					.WithAging(postCCRWorkflow.PK, 10)
					.ExpectFadeAt(4, 2)
					.ExpectFadeAt(5, 10)
					.ExpectFadeAt(6, 10),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 3)
					.WithAging(postCCRWorkflow.PK, 9)
					.ExpectFadeAt(4, 3)
					.ExpectFadeAt(5, 9)
					.ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 4)
					.WithAging(postCCRWorkflow.PK, 8)
					.ExpectFadeAt(4, 4)
					.ExpectFadeAt(5, 8)
					.ExpectFadeAt(6, 8),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 5)
					.WithAging(postCCRWorkflow.PK, 7)
					.ExpectFadeAt(4, 5)
					.ExpectFadeAt(5, 7)
					.ExpectFadeAt(6, 7),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 6)
					.WithAging(postCCRWorkflow.PK, 6)
					.ExpectFadeAt(4, 6)
					.ExpectFadeAt(5, 6)
					.ExpectFadeAt(6, 6),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 7)
					.WithAging(postCCRWorkflow.PK, 5)
					.ExpectFadeAt(4, 7)
					.ExpectFadeAt(5, 5)
					.ExpectFadeAt(6, 5),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 8)
					.WithAging(postCCRWorkflow.PK, 4)
					.ExpectFadeAt(4, 8)
					.ExpectFadeAt(5, 4)
					.ExpectFadeAt(6, 4),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 9)
					.WithAging(postCCRWorkflow.PK, 3)
					.ExpectFadeAt(4, 9)
					.ExpectFadeAt(5, 3)
					.ExpectFadeAt(6, 3),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 10)
					.WithAging(postCCRWorkflow.PK, 2)
					.ExpectFadeAt(4, 10)
					.ExpectFadeAt(5, 2)
					.ExpectFadeAt(6, 2),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 11)
					.WithAging(postCCRWorkflow.PK, 1)
					.ExpectFadeAt(4, 11)
					.ExpectFadeAt(5, 1)
					.ExpectFadeAt(6, 1),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 12)
					.WithAging(postCCRWorkflow.PK, 0)
					.ExpectFadeAt(4, 12)
					.ExpectFadeAt(5, 1)
					.ExpectFadeAt(6, 1),
			};
		}

		#endregion
	}

	public class ComponentGridBasherSubComponentWith1DayPreCCRBufferAnd11DaysPostCCRBufferTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis + 1, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis + 1, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis + 1, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis + 1, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis + 1, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis + 1, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis + 1, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis + 1, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis + 1, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis + 1, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis + 1, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis + 1, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis + 1, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
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

		#endregion

		#region Implementation

		readonly int subCmptPrimaryAxis = 4;

		#endregion
	}

	#endregion

	#region Visible Child Components

	public class ComponentGridBasherAdditionalComponentLongerThanPrimaryComponentSubComponentTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(4, 1).ExpectFadeAt(6, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(4, 2).ExpectFadeAt(6, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(4, 3).ExpectFadeAt(6, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(4, 4).ExpectFadeAt(6, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(4, 6).ExpectFadeAt(6, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(4, 7).ExpectFadeAt(6, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(4, 8).ExpectFadeAt(6, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(4, 10).ExpectFadeAt(6, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(4, 11).ExpectFadeAt(6, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(4, 12).ExpectFadeAt(6, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(4, 13).ExpectFadeAt(6, 13),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(4, 13).ExpectFadeAt(6, 13),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(4, 13).ExpectFadeAt(6, 13),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(5, 1).ExpectFadeAt(7, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(5, 2).ExpectFadeAt(7, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(5, 3).ExpectFadeAt(7, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(5, 4).ExpectFadeAt(7, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(5, 6).ExpectFadeAt(7, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(5, 7).ExpectFadeAt(7, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(5, 8).ExpectFadeAt(7, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(5, 10).ExpectFadeAt(7, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(5, 11).ExpectFadeAt(7, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(5, 12).ExpectFadeAt(7, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(5, 13).ExpectFadeAt(7, 13),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(5, 13).ExpectFadeAt(7, 13),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(5, 13).ExpectFadeAt(7, 13),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 0)
					.WithAging(postCCRWorkflow.PK, 12)
					.ExpectFadeAt(4, 1)
					.ExpectFadeAt(5, 13)
					.ExpectFadeAt(6, 1)
					.ExpectFadeAt(7, 13),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 1)
					.WithAging(postCCRWorkflow.PK, 12)
					.ExpectFadeAt(4, 2)
					.ExpectFadeAt(5, 13)
					.ExpectFadeAt(6, 2)
					.ExpectFadeAt(7, 13),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 2)
					.WithAging(postCCRWorkflow.PK, 10)
					.ExpectFadeAt(4, 3)
					.ExpectFadeAt(5, 13)
					.ExpectFadeAt(6, 3)
					.ExpectFadeAt(7, 13),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 3)
					.WithAging(postCCRWorkflow.PK, 9)
					.ExpectFadeAt(4, 4)
					.ExpectFadeAt(5, 12)
					.ExpectFadeAt(6, 4)
					.ExpectFadeAt(7, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 4)
					.WithAging(postCCRWorkflow.PK, 8)
					.ExpectFadeAt(4, 6)
					.ExpectFadeAt(5, 11)
					.ExpectFadeAt(6, 6)
					.ExpectFadeAt(7, 11),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 5)
					.WithAging(postCCRWorkflow.PK, 7)
					.ExpectFadeAt(4, 7)
					.ExpectFadeAt(5, 10)
					.ExpectFadeAt(6, 7)
					.ExpectFadeAt(7, 10),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 6)
					.WithAging(postCCRWorkflow.PK, 6)
					.ExpectFadeAt(4, 8)
					.ExpectFadeAt(5, 8)
					.ExpectFadeAt(6, 8)
					.ExpectFadeAt(7, 8),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 7)
					.WithAging(postCCRWorkflow.PK, 5)
					.ExpectFadeAt(4, 10)
					.ExpectFadeAt(5, 7)
					.ExpectFadeAt(6, 10)
					.ExpectFadeAt(7, 7),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 8)
					.WithAging(postCCRWorkflow.PK, 4)
					.ExpectFadeAt(4, 11)
					.ExpectFadeAt(5, 6)
					.ExpectFadeAt(6, 11)
					.ExpectFadeAt(7, 6),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 9)
					.WithAging(postCCRWorkflow.PK, 3)
					.ExpectFadeAt(4, 12)
					.ExpectFadeAt(5, 4)
					.ExpectFadeAt(6, 12)
					.ExpectFadeAt(7, 4),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 10)
					.WithAging(postCCRWorkflow.PK, 2)
					.ExpectFadeAt(4, 13)
					.ExpectFadeAt(5, 3)
					.ExpectFadeAt(6, 13)
					.ExpectFadeAt(7, 3),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 11)
					.WithAging(postCCRWorkflow.PK, 1)
					.ExpectFadeAt(4, 13)
					.ExpectFadeAt(5, 2)
					.ExpectFadeAt(6, 13)
					.ExpectFadeAt(7, 2),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 12)
					.WithAging(postCCRWorkflow.PK, 0)
					.ExpectFadeAt(4, 13)
					.ExpectFadeAt(5, 1)
					.ExpectFadeAt(6, 13)
					.ExpectFadeAt(7, 1),
			};
		}

		#endregion
	}

	public class ComponentGridBasherStackedVisibleChildComponentTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(6, 1).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(6, 1).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(6, 2).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(6, 3).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(6, 4).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(6, 5).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(6, 6).ExpectFadeAt(7, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(6, 7).ExpectFadeAt(7, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(6, 8).ExpectFadeAt(7, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(6, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(6, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(6, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(7, 10).ExpectFadeAt(8, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(7, 11).ExpectFadeAt(8, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(7, 12).ExpectFadeAt(8, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(6, 1).ExpectFadeAt(7, 5)
					.WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(7, 12).ExpectFadeAt(8, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(6, 1).ExpectFadeAt(7, 5)
					.WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(7, 11).ExpectFadeAt(8, 11),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(6, 2).ExpectFadeAt(7, 5)
					.WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(7, 10).ExpectFadeAt(8, 10),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(6, 3).ExpectFadeAt(7, 5)
					.WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(6, 4).ExpectFadeAt(7, 5)
					.WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(6, 5).ExpectFadeAt(7, 5)
					.WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(6, 6).ExpectFadeAt(7, 6)
					.WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(6, 7).ExpectFadeAt(7, 7)
					.WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(6, 8).ExpectFadeAt(7, 8)
					.WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(6, 9)
					.WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(6, 10)
					.WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(6, 11)
					.WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(6, 12)
					.WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(7, 9).ExpectFadeAt(8, 9),
			};
		}

		#endregion
	}

	public class ComponentGridBasherVisibleChildComponentWithGapTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(6, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(6, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(6, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1)
					.WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1)
					.WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 11),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2)
					.WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 10),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3)
					.WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4)
					.WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5)
					.WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6)
					.WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7)
					.WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8)
					.WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 9)
					.WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 10)
					.WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 11)
					.WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 12)
					.WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 9),
			};
		}

		#endregion

		#region Implementation

		readonly int subCmptPrimaryAxis = 6;

		#endregion
	}

	public class ComponentGridBasherVisibleChildComponentWith11DaysPreCCRBufferAnd1DayPostCCRBufferTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1)
					.WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1)
					.WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2)
					.WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3)
					.WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4)
					.WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5)
					.WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6)
					.WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7)
					.WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8)
					.WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 9)
					.WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 10)
					.WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 11)
					.WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12)
					.WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 12),
			};
		}

		#endregion

		#region Implementation

		readonly int subCmptPrimaryAxis = 6;

		#endregion
	}

	public class ComponentGridBasherVisibleChildPreConstraintsOverlappedTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			// Both subcomponents are considered pre-CCR components and should fade
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(6, 1).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(6, 1).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(6, 2).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(6, 3).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(6, 4).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(6, 5).ExpectFadeAt(7, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(6, 6).ExpectFadeAt(7, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(6, 7).ExpectFadeAt(7, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(6, 8).ExpectFadeAt(7, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(6, 10).ExpectFadeAt(7, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(6, 11).ExpectFadeAt(7, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(6, 12).ExpectFadeAt(7, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			// Both subcomponents are considered pre-CCR components so it will behave the same as GetTestsForSubComponentZoneHeadingFade_PreCCRAging i.e. ignoring post-CCR workflow
			return GetTestsForSubComponentZoneHeadingFade_PreCCRAging(preCCRWorkflow);
		}

		#endregion
	}

	public class ComponentGridBasherVisibleChildComponentOverlappedTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			// Pre-CCR subcomponent Secondary-axis 1 and 2 does not exist because its Offset = 2 * 8 * 60
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(6, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(6, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(6, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(6, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(6, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(6, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(6, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(6, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(6, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(6, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(6, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(6, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(6, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(7, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(7, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(7, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(6, 3)
					.WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(7, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(6, 3)
					.WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(7, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(6, 3)
					.WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(7, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(6, 3)
					.WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(6, 4)
					.WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(6, 5)
					.WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(6, 6)
					.WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(6, 7)
					.WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(6, 8)
					.WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9).ExpectFadeAt(6, 9)
					.WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10).ExpectFadeAt(6, 10)
					.WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11).ExpectFadeAt(6, 11)
					.WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12).ExpectFadeAt(6, 12)
					.WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(7, 9),
			};
		}

		#endregion
	}

	public class ComponentGridBasherVisibleChildComponentWithMultiplePostConstraintBuffersTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			var subCmptPrimaryAxis = 6;

			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			var subCmptPrimaryAxis1 = 6;
			var subCmptPrimaryAxis2 = 7;

			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis1, 9).ExpectFadeAt(subCmptPrimaryAxis2, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis1, 10).ExpectFadeAt(subCmptPrimaryAxis2, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis1, 11).ExpectFadeAt(subCmptPrimaryAxis2, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis1, 12).ExpectFadeAt(subCmptPrimaryAxis2, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(6, 1)
					.WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(6, 12).ExpectFadeAt(7, 12),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(6, 1)
					.WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(6, 11).ExpectFadeAt(7, 11),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 2).ExpectFadeAt(6, 2)
					.WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(6, 10).ExpectFadeAt(7, 10),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 3).ExpectFadeAt(6, 3)
					.WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 4).ExpectFadeAt(6, 4)
					.WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 5).ExpectFadeAt(6, 5)
					.WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 6).ExpectFadeAt(6, 6)
					.WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 7).ExpectFadeAt(6, 7)
					.WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 8).ExpectFadeAt(6, 8)
					.WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 9)
					.WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 10)
					.WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 11)
					.WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
				new SubCmptHeadingFadeTestBuilder()
					.WithAging(preCCRWorkflow.PK, 12)
					.WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(6, 9).ExpectFadeAt(7, 9),
			};
		}

		#endregion
	}

	public class ComponentGridBasherVisibleChildComponentWith1DayPreCCRBufferAnd11DaysPostCCRBufferTestCaseBuilder : ComponentGridBasherTestCaseBuilder
	{
		#region SubComponentZoneHeading Background FadeColor

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PreCCRAging(ProcessHeader preCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_PostCCRAging(ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis, 12),
			};
		}

		public override SubCmptHeadingFadeTest[] GetTestsForSubComponentZoneHeadingFade_FadeIndependently(ProcessHeader preCCRWorkflow, ProcessHeader postCCRWorkflow)
		{
			return new SubCmptHeadingFadeTest[] {
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 1)
					.WithAging(postCCRWorkflow.PK, 12).ExpectFadeAt(subCmptPrimaryAxis, 12),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 1)
					.WithAging(postCCRWorkflow.PK, 11).ExpectFadeAt(subCmptPrimaryAxis, 11),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 2)
					.WithAging(postCCRWorkflow.PK, 10).ExpectFadeAt(subCmptPrimaryAxis, 10),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 3)
					.WithAging(postCCRWorkflow.PK, 9).ExpectFadeAt(subCmptPrimaryAxis, 9),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 4)
					.WithAging(postCCRWorkflow.PK, 8).ExpectFadeAt(subCmptPrimaryAxis, 8),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 5)
					.WithAging(postCCRWorkflow.PK, 7).ExpectFadeAt(subCmptPrimaryAxis, 7),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 6)
					.WithAging(postCCRWorkflow.PK, 6).ExpectFadeAt(subCmptPrimaryAxis, 6),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 7)
					.WithAging(postCCRWorkflow.PK, 5).ExpectFadeAt(subCmptPrimaryAxis, 5),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 8)
					.WithAging(postCCRWorkflow.PK, 4).ExpectFadeAt(subCmptPrimaryAxis, 4),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 9)
					.WithAging(postCCRWorkflow.PK, 3).ExpectFadeAt(subCmptPrimaryAxis, 3),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 10)
					.WithAging(postCCRWorkflow.PK, 2).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 11)
					.WithAging(postCCRWorkflow.PK, 1).ExpectFadeAt(subCmptPrimaryAxis, 2),
				new SubCmptHeadingFadeTestBuilder().WithAging(preCCRWorkflow.PK, 12)
					.WithAging(postCCRWorkflow.PK, 0).ExpectFadeAt(subCmptPrimaryAxis, 2),
			};
		}

		#endregion

		#region Implementation

		readonly int subCmptPrimaryAxis = 6;

		#endregion
	}

	#endregion
}
