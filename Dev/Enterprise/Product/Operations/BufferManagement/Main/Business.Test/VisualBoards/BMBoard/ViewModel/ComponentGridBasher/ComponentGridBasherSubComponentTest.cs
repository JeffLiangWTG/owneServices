using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.BufferManagement.Business.Test
{
	#region Test Sub Components in Additional Primary Components

	public class ComponentGridBasherAdditionalComponentLongerThanPrimaryComponentSubComponentTest : ComponentGridBasherTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherAdditionalComponentLongerThanPrimaryComponentSubComponentTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.Buffer.FC_BufferTimespanInMinutes = 72 * 60;

			Config.PreConstraintBuffer.FC_BufferTimespanInMinutes = 48 * 60;
			Config.Constraint.FC_OffsetInMinutes = 48 * 60;
			Config.PostConstraintBuffer.FC_BufferTimespanInMinutes = 24 * 60;
			Config.PostConstraintBuffer.FC_OffsetInMinutes = 48 * 60;

			var additionalBuffer = BMSTestHelper.CreateBuffer(Config.System, "loongu baffaa");
			BMSTestHelper.CreateSubBuffer(additionalBuffer, "Long-Pre-Constraint", timespanMinutes: 64 * 60, offsetMinutes: 0);
			BMSTestHelper.CreateConstraint(additionalBuffer, "Long-Constraint", offsetMinutes: 64 * 60);
			BMSTestHelper.CreateSubBuffer(additionalBuffer, "Long-Post-Constraint", timespanMinutes: 32 * 60, offsetMinutes: 64 * 60);

			BMSTestHelper.CreateAdditionalComponent(Config.Section, additionalBuffer);
			ConstrainedModeHelper.SwitchToConstrainedMode(Config.ReleaseGroup, additionalBuffer.PK);

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
		}

		protected override void AssertChannelStatus_OutsideTargetZone_InitialCondition(IVisualBoardChannel channel)
		{
			AssertEquals("Outside target zone, Idle", channel.Status);
		}

		[RequiresSTA]
		public void TestSubComponentHeading()
		{
			Config.Section.SectionConfiguration.ShowChildComponentZones = false;

			var subComponentHeadingPrimaryAxis = 4;

			CombineAssertions(() =>
			{
				AssertOverlappingSubComponentHeading("Pre-Constraint", subComponentHeadingPrimaryAxis,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 2, 2, 1, 1, 1, 0, 0, 0, 0, 0 }));

				AssertOverlappingSubComponentHeading("Post-Constraint", subComponentHeadingPrimaryAxis + 1,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 1, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 1, 0 }));

				AssertOverlappingSubComponentHeading("Long-Pre-Constraint", subComponentHeadingPrimaryAxis + 2,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 2, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 2, 2, 1, 1, 1, 0, 0, 0, 0, 0 }));

				AssertOverlappingSubComponentHeading("Long-Post-Constraint", subComponentHeadingPrimaryAxis + 3,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 3, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 1, 0 }));
			});
		}

		[RequiresSTA]
		public void TestSubComponentHeading_ShowChildComponentZones()
		{
			Config.Section.SectionConfiguration.ShowChildComponentZones = true;

			var subComponentHeadingPrimaryAxis = 6;

			CombineAssertions(() =>
			{
				AssertOverlappingSubComponentHeading("Long-Pre-Constraint", subComponentHeadingPrimaryAxis,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 2, 2, 1, 1, 1 }));

				AssertOverlappingSubComponentHeading("Long-Post-Constraint", subComponentHeadingPrimaryAxis,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis, startingSecondaryAxis: 9, expectedZones: new[] { 3, 2, 2, 1, 0 }));
			});
		}
	}

	#endregion

	#region Test Stacked SubComponents

	/// <summary>
	/// https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherStackedSubComponentTest​
	/// </summary>
	public class ComponentGridBasherStackedSubComponentTest : ComponentGridBasherTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherStackedSubComponentTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			var preConstraintBuffer2 = BMSTestHelper.CreateSubBuffer(Config.Buffer, "Pre-Constraint-2", timespanMinutes: 32 * 60, offsetMinutes: 32 * 60);
			var postConstraintBuffer2 = BMSTestHelper.CreateSubBuffer(Config.Buffer, "Post-Constraint-2", timespanMinutes: 24 * 60, offsetMinutes: 64 * 60);
			Config.PreConstraintBuffer.FC_DisplaySequence = 1;
			preConstraintBuffer2.FC_DisplaySequence = 2;
			Config.Constraint.FC_DisplaySequence = 3;
			postConstraintBuffer2.FC_DisplaySequence = 4;
			Config.PostConstraintBuffer.FC_DisplaySequence = 5;

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
		}

		[TestDate(2014, 1, 1)]
		public void TestSubComponentHeading()
		{
			Config.Section.SectionConfiguration.ShowChildComponentZones = false;

			var subComponentHeadingPrimaryAxis = 4;

			AssertOverlappingSubComponentHeading("Pre-Constraint", subComponentHeadingPrimaryAxis,
				CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 2, 2, 1, 1, 1, 0, 0, 0, 0, 0 }));

			AssertOverlappingSubComponentHeading("Post-Constraint", subComponentHeadingPrimaryAxis + 1,
				CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 1, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 2, 1, 0 }));

			AssertOverlappingSubComponentHeading("Pre-Constraint-2", subComponentHeadingPrimaryAxis + 2,
				CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 2, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 3, 3, 2, 2, 1, 0, 0, 0, 0, 0 }));

			AssertOverlappingSubComponentHeading("Post-Constraint-2", subComponentHeadingPrimaryAxis + 3,
				CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 3, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 3, 3, 3, 3, 3, 3, 2, 1, 0, 0 }));
		}

		[TestDate(2014, 1, 1)]
		public void TestSubComponentHeading_ShowChildComponentZones()
		{
			Config.Section.SectionConfiguration.ShowChildComponentZones = true;

			var subComponentHeadingPrimaryAxis = 6;

			CombineAssertions(() =>
			{
				AssertOverlappingSubComponentHeading("Pre-Constraint", subComponentHeadingPrimaryAxis,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis, startingSecondaryAxis: 1, expectedZones: new[] { 3, 3, 3, 2, 2, 1, 1, 1, 0, 0, 0, 0, 0 }));

				AssertOverlappingSubComponentHeading("Pre-Constraint-2", subComponentHeadingPrimaryAxis + 1,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 1, startingSecondaryAxis: 5, expectedZones: new[] { 3, 2, 2, 1 }));

				AssertOverlappingSubComponentHeading("Post-Constraint-2", subComponentHeadingPrimaryAxis + 1,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 1, startingSecondaryAxis: 9, expectedZones: new[] { 3, 2, 1, 0, 0 }));

				AssertOverlappingSubComponentHeading(ZString.Empty, subComponentHeadingPrimaryAxis + 1,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 1, startingSecondaryAxis: 1, expectedZones: new[] { -1, -1, -1, -1 }));

				AssertOverlappingSubComponentHeading("Post-Constraint", subComponentHeadingPrimaryAxis + 2,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 2, startingSecondaryAxis: 9, expectedZones: new[] { 3, 2, 2, 1, 0 }));

				AssertOverlappingSubComponentHeading(ZString.Empty, subComponentHeadingPrimaryAxis + 2,
					CreateExpectedSubComponentHeadingTests(primaryAxis: subComponentHeadingPrimaryAxis + 2, startingSecondaryAxis: 1, expectedZones: new[] { -1, -1, -1, -1, -1, -1, -1, -1 }));
			});
		}

		#region Test Channel Status: High Risk (Post CCR)

		protected override void AssertCCRStatus_Zone2PostCCRBuffer(IVisualBoardChannel channel)
		{
			AssertEquals("post-ccr-task should not impact CCR status", "Idle", channel.Status);
		}

		#endregion
	}

	public class ComponentGridBasherStackedVisibleChildComponentTest : ComponentGridBasherStackedSubComponentTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherStackedVisibleChildComponentTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.ShowChildComponentZones = true;
		}
	}

	public class ComponentGridBasherStackedSubComponentTestFlowUpTest : ComponentGridBasherStackedSubComponentTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Up;
		}
	}

	public class ComponentGridBasherStackedSubComponentTestFlowRightTest : ComponentGridBasherStackedSubComponentTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Right;
		}
	}

	public class ComponentGridBasherStackedSubComponentTestFlowDownTest : ComponentGridBasherStackedSubComponentTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
		}
	}

	public class ComponentGridBasherStackedSubComponentTestFlowLeftTest : ComponentGridBasherStackedSubComponentTest
	{
		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Left;
		}
	}

	#endregion

	public class ComponentGridBasherSubComponentWithGapTest : ComponentGridBasherTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherSubComponentWithGapTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
			Config.PreConstraintBuffer.FC_BufferTimespanInMinutes = 4 * 8 * 60;
		}
	}

	public class ComponentGridBasherVisibleChildComponentWithGapTest : ComponentGridBasherSubComponentWithGapTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherVisibleChildComponentWithGapTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.ShowChildComponentZones = true;
		}
	}

	/// <summary>
	/// https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherSubComponentWith1DayPreCCRBufferAnd11DaysPostCCRBufferTest
	/// </summary>
	public class ComponentGridBasherSubComponentWith1DayPreCCRBufferAnd11DaysPostCCRBufferTest : ComponentGridBasherTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherSubComponentWith1DayPreCCRBufferAnd11DaysPostCCRBufferTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.PreConstraintBuffer.FC_OffsetInMinutes = 0;
			Config.PreConstraintBuffer.FC_BufferTimespanInMinutes = 1 * 8 * 60;

			Config.PostConstraintBuffer.FC_OffsetInMinutes = 1 * 8 * 60;
			Config.PostConstraintBuffer.FC_BufferTimespanInMinutes = 11 * 8 * 60;

			Config.Constraint.FC_OffsetInMinutes = 1 * 8 * 60;

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
		}

		#region Test Channel Status

		protected override void AssertChannelStatus_OutsideTargetZone_InitialCondition(IVisualBoardChannel channel)
		{
			AssertEquals("High risk (late ccr task) because constraint is on 1 days and worklfow age is at 9 days",
				"Outside target zone, Idle",
				channel.Status);
		}

		protected override void AssertChannelStatus_OutsideTargetZone_MadeCCRTaskNonCurrent(IVisualBoardChannel channel)
		{
			AssertEquals("The 'complex task for CCR' has aged past the constraint risk line,",
				"Outside target zone, Idle",
				channel.Status);
		}

		#endregion

		protected override void AssertCCRStatus_Zone2PostCCRBuffer(IVisualBoardChannel channel)
		{
			AssertEquals("post-ccr-task should not impact CCR status", "Idle", channel.Status);
		}
	}

	public class ComponentGridBasherVisibleChildComponentWith1DayPreCCRBufferAnd11DaysPostCCRBufferTest : ComponentGridBasherSubComponentWith1DayPreCCRBufferAnd11DaysPostCCRBufferTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherVisibleChildComponentWith1DayPreCCRBufferAnd11DaysPostCCRBufferTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.ShowChildComponentZones = true;
		}
	}

	/// <summary>
	/// https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherSubComponentWith11DaysPreCCRBufferAnd1DayPostCCRBufferTest
	/// </summary>
	public class ComponentGridBasherSubComponentWith11DaysPreCCRBufferAnd1DayPostCCRBufferTest : ComponentGridBasherTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherSubComponentWith11DaysPreCCRBufferAnd1DayPostCCRBufferTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.PreConstraintBuffer.FC_OffsetInMinutes = 0;
			Config.PreConstraintBuffer.FC_BufferTimespanInMinutes = 11 * 8 * 60;

			Config.PostConstraintBuffer.FC_OffsetInMinutes = 11 * 8 * 60;
			Config.PostConstraintBuffer.FC_BufferTimespanInMinutes = 1 * 8 * 60;

			Config.Constraint.FC_OffsetInMinutes = 11 * 8 * 60;

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
		}

		#region Test Channel Status

		protected override void AssertChannelStatus_OutsideTargetZone_InitialCondition(IVisualBoardChannel channel)
		{
			AssertEquals("Not High risk (late ccr task) but 'Not enough work' because constraint is on 11 days and worklfow age is at 9 days",
				"Not enough work, Idle",
				channel.Status);
		}

		protected override void AssertChannelStatus_OutsideTargetZone_AddedNewCCRWorkflowAndTaskWith9DaysAging(IVisualBoardChannel channel)
		{
			AssertEquals("Not High risk (late ccr task) because constraint is at 11 days while workflow age is at 9 days",
				"Idle",
				channel.Status);
		}

		protected override void AssertChannelStatus_OutsideTargetZone_MadeCCRTaskNonCurrent(IVisualBoardChannel channel)
		{
			AssertEquals("Not High risk (late ccr task) but 'Not enough work' because CCR-tasks is not current",
				"Not enough work, Idle",
				channel.Status);
		}

		#endregion

		#region Test Channel Status: High Risk (Post CCR)

		protected override void AssertCCRStatus_WithIncrementWorkflowAging(int agingIndex, IVisualBoardChannel channel)
		{
			AssertEquals(string.Format("Aging={0}, post-ccr-task should not impact CCR status", agingIndex), "Not enough work, Idle", channel.Status);
		}

		protected override void AssertCCRStatus_Zone1PostCCRBuffer_WithClosedCCRTask(IVisualBoardChannel channel)
		{
			Assert("This test trigger Not enough work because long span of pre-constraint-buffer", true);
		}

		protected override void AssertCCRStatus_Zone1PostCCRBuffer_WithClosedCCRTask_ButPreCCRWorkflow(IVisualBoardChannel channel)
		{
			Assert("This test trigger Not enough work because long span of pre-constraint-buffer", true);
		}

		protected override void AssertCCRStatus_Zone1PostCCRBuffer_WithOpenCCRTask(IVisualBoardChannel channel)
		{
			Assert("This test trigger Not enough work because long span of pre-constraint-buffer", true);
		}

		protected override void AssertCCRStatus_Zone1PostCCRBuffer_WithLowerSequenceNumberClosedCCRTask_AndHigherSequenceNumberOpenCCRTask(IVisualBoardChannel channel)
		{
			Assert("This test trigger Not enough work because long span of pre-constraint-buffer", true);
		}

		protected override void AssertCCRStatus_Zone2PostCCRBuffer(IVisualBoardChannel channel)
		{
			Assert("This test trigger Not enough work because long span of pre-constraint-buffer", true);
		}

		#endregion
	}

	public class ComponentGridBasherVisibleChildComponentWith11DaysPreCCRBufferAnd1DayPostCCRBufferTest : ComponentGridBasherSubComponentWith11DaysPreCCRBufferAnd1DayPostCCRBufferTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherVisibleChildComponentWith11DaysPreCCRBufferAnd1DayPostCCRBufferTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.ShowChildComponentZones = true;
		}
	}

	/// <summary>
	///	https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherPreConstraintsOverlappedTest
	/// </summary>
	public class ComponentGridBasherPreConstraintsOverlappedTest : ComponentGridBasherTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherPreConstraintsOverlappedTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.PostConstraintBuffer.FC_OffsetInMinutes = 4 * 8 * 60;
			Config.PostConstraintBuffer.FC_BufferTimespanInMinutes = 8 * 8 * 60;

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
		}

		#region Test Channel Status

		protected override void AssertChannelStatus_OutsideTargetZone_MadeCCRTaskNonCurrent(IVisualBoardChannel channel)
		{
			AssertEquals("'Outside target zone' because CCR n-th percentage is outside the target zone (higher in buffer penetration), regardless CCR tasks are current",
				"Outside target zone, Idle",
				channel.Status);
		}

		#endregion

		#region Test Channel Status: High Risk (Post CCR)

		protected override void AssertCCRStatus_Zone1PostCCRBuffer_WithClosedCCRTask(IVisualBoardChannel channel)
		{
			Assert("This test doesn't have post-constraint buffer because all components-offset < constraint-offset. ", true);
		}

		protected override void AssertCCRStatus_Zone1PostCCRBuffer_WithOpenCCRTask(IVisualBoardChannel channel)
		{
			Assert("This test doesn't have post-constraint buffer because all components-offset < constraint-offset. ", true);
		}

		#endregion
	}

	public class ComponentGridBasherVisibleChildPreConstraintsOverlappedTest : ComponentGridBasherPreConstraintsOverlappedTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherVisibleChildPreConstraintsOverlappedTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.ShowChildComponentZones = true;
		}
	}

	/// <summary>
	/// https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherSubComponentOverlappedTest
	/// </summary>
	public class ComponentGridBasherSubComponentOverlappedTest : ComponentGridBasherTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherSubComponentOverlappedTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.PreConstraintBuffer.FC_OffsetInMinutes = 2 * 8 * 60;

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
		}
	}

	public class ComponentGridBasherVisibleChildComponentOverlappedTest : ComponentGridBasherSubComponentOverlappedTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherVisibleChildComponentOverlappedTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.ShowChildComponentZones = true;
		}
	}

	/// <summary>
	/// https://wisetechglobal.sharepoint.com/Development/Development%20Wiki/Testing%20Visual%20Board.aspx#ComponentGridBasherSubComponentWithMultiplePostConstraintBuffersTest
	/// </summary>
	public class ComponentGridBasherSubComponentWithMultiplePostConstraintBuffersTest : ComponentGridBasherTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherSubComponentWithMultiplePostConstraintBuffersTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.PostConstraintBuffer.FC_BufferTimespanInMinutes = 8 * 8 * 60;

			var postConstraintBuffer2 = BMSTestHelper.CreateSubBuffer(Config.Buffer, "Post-Constraint-2", offsetMinutes: 8 * 8 * 60, timespanMinutes: 4 * 8 * 60);
			postConstraintBuffer2.FC_DisplaySequence = 4;

			Config.Section.SectionConfiguration.FlowDirection = FlowDirectionList.Codes.Down;
		}
	}

	public class ComponentGridBasherVisibleChildComponentWithMultiplePostConstraintBuffersTest : ComponentGridBasherSubComponentWithMultiplePostConstraintBuffersTest
	{
		protected override ComponentGridBasherTestCaseBuilder GetTestCaseBuilder()
		{
			return new ComponentGridBasherVisibleChildComponentWithMultiplePostConstraintBuffersTestCaseBuilder();
		}

		protected override void SetUp()
		{
			base.SetUp();

			Config.Section.SectionConfiguration.ShowChildComponentZones = true;
		}
	}
}
