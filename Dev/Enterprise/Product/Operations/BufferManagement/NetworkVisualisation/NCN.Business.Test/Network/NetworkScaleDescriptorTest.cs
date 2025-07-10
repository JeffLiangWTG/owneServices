using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using CargoWise.NetworkVisualisation.Integration;
using CargoWise.PAVE.Common.Interfaces;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.Business.Test;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.VisualBoards.Business.Test;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class NetworkScaleDescriptorTest : NetworkTestCase
	{
		#region Label

		public void TestShowTimeDetails_WithHoliday()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var branch = Factory.Load<GlbBranch>(GlbBranch.CurrentBranch.PK);
			var holiday = branch.GlbHolidays.AddNew();
			holiday.GH_Date = new ZDateTime(2014, 7, 29);
			holiday.GH_Recurring = true;

			var diagram = CreateDiagram(Factory);
			diagram.ScheduledStartTimeUtc = new ZDateTime(2014, 7, 21);

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();
			var descriptor = network.ScaleDescriptor;
			var set = descriptor.GetScaleSetForColumns(7);

			AssertEquals("21-Jul-14 10:00", set.ScalePoints[0].Label);
			AssertEquals("22-Jul-14 10:00", set.ScalePoints[1].Label);
			AssertEquals("23-Jul-14 10:00", set.ScalePoints[2].Label);
			AssertEquals("24-Jul-14 10:00", set.ScalePoints[3].Label);
			AssertEquals("25-Jul-14 10:00", set.ScalePoints[4].Label);
			AssertEquals("Should skip weekend", "28-Jul-14 10:00", set.ScalePoints[5].Label);
			AssertEquals("Should skip branch holiday", "30-Jul-14 10:00", set.ScalePoints[6].Label);
		}

		[TestDate(2015, 10, 6)]
		public void TestGetScaleSet_MakesEnoughPoints()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagram = CreateDiagram(Factory);
			diagram.ScheduledStartTimeUtc = new ZDateTime(2015, 10, 1);

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();
			var descriptor = network.ScaleDescriptor;
			AssertEquals(10, descriptor.GetScaleSetForColumns(10).ScalePoints.Count);
		}

		[TestDate(2015, 10, 6)]
		public void TestShowTimeDetails_CheckIfIsFirstIndexInThePresent()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagram = CreateDiagram(Factory);
			diagram.ScheduledStartTimeUtc = new ZDateTime(2015, 10, 1);

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();
			var descriptor = network.ScaleDescriptor;
			var set = descriptor.GetScaleSetForColumns(10);
			AssertEquals(3, set.IndexOfColumnInPresent);

			AssertEquals("01-Oct-15 10:00", set.ScalePoints[0].Label);
			AssertEquals("02-Oct-15 10:00", set.ScalePoints[1].Label);
			AssertEquals("05-Oct-15 10:00", set.ScalePoints[2].Label);
			AssertEquals("06-Oct-15 10:00", set.ScalePoints[3].Label);
			AssertEquals("07-Oct-15 10:00", set.ScalePoints[4].Label);
		}

		[TestDate(2015, 10, 6)]
		public void TestShowTimeDetails_CheckIfIsFirstIndexInThePresent_AdjustScales()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagram = CreateDiagram(Factory);
			diagram.ScheduledStartTimeUtc = new ZDateTime(2015, 10, 1);

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();
			var descriptor = network.ScaleDescriptor;

			AssertEquals("Default scale is 8 hours", 8, diagram.Scale.Hour);

			var set = descriptor.GetScaleSetForColumns(10);
			AssertEquals(3, set.IndexOfColumnInPresent);
			AssertEquals("05-Oct-15 10:00", set.ScalePoints[2].Label);
			AssertEquals("06-Oct-15 10:00", set.ScalePoints[3].Label);
			AssertEquals("07-Oct-15 10:00", set.ScalePoints[4].Label);

			diagram.Scale = new ZInt(960).GetDateTimeFromMinutes(); // 16 hours
			Factory.Save();

			set = descriptor.GetScaleSetForColumns(10);
			AssertEquals(2, set.IndexOfColumnInPresent);
			AssertEquals("05-Oct-15 10:00", set.ScalePoints[1].Label);
			AssertEquals("07-Oct-15 10:00", set.ScalePoints[2].Label);
			AssertEquals("09-Oct-15 10:00", set.ScalePoints[3].Label);

			diagram.ScheduledStartTimeUtc = new ZDateTime(2015, 10, 2);
			Factory.Save();
			set = descriptor.GetScaleSetForColumns(10);
			AssertEquals(2, set.IndexOfColumnInPresent);
			AssertEquals("05-Oct-15 10:00", set.ScalePoints[1].Label);
			AssertEquals("07-Oct-15 10:00", set.ScalePoints[2].Label);
			AssertEquals("09-Oct-15 10:00", set.ScalePoints[3].Label);

			diagram.Scale = new ZInt(240).GetDateTimeFromMinutes(); // 4 hours
			Factory.Save();

			set = descriptor.GetScaleSetForColumns(10);
			AssertEquals(6, set.IndexOfColumnInPresent);
			AssertEquals("05-Oct-15 14:00", set.ScalePoints[5].Label);
			AssertEquals("06-Oct-15 10:00", set.ScalePoints[6].Label);
			AssertEquals("06-Oct-15 14:00", set.ScalePoints[7].Label);
			AssertEquals("07-Oct-15 10:00", set.ScalePoints[8].Label);

			diagram.Scale = new ZInt(2400).GetDateTimeFromMinutes(); // 40 hours
			Factory.Save();

			set = descriptor.GetScaleSetForColumns(3);
			AssertEquals(1, set.IndexOfColumnInPresent);
			AssertEquals("01-Oct-15 10:00", set.ScalePoints[0].Label);
			AssertEquals("08-Oct-15 10:00", set.ScalePoints[1].Label);
			AssertEquals("15-Oct-15 10:00", set.ScalePoints[2].Label);
		}

		[TestDate(2015, 7, 14)]
		public void TestScaleLabels_WhenDiagramCountsBackFromScheduledFinishTime_ShouldUseDiagramDuration()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			var shape1 = NetworkTestCase.CreateShape(diagram);
			var shape2 = NetworkTestCase.CreateShape(diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);

			diagram.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(12);

			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();

			NetworkTestCase.SetShapeRectangle(network, shape1, diagram, 300, 100, 100, 100);
			NetworkTestCase.SetShapeRectangle(network, shape2, diagram, 300, 100, 400, 100);

			network.RefreshSchedules();
			Factory.Save();

			AssertEquals("Diagram should be 7 days in duration (in working hours) - 3 days for each shape and one day for the gap at the start", 7 * 8m, diagram.ExplicitDurationHours);

			var descriptor = network.ScaleDescriptor;
			var points = descriptor.GetScaleSetForColumns(7).ScalePoints.Select(p => p.Label).ToArray();

			AssertArrayEqualsByElements(new[]
			{
				"19-Jul-15 10:00",
				"20-Jul-15 10:00",
				"21-Jul-15 10:00",
				"22-Jul-15 10:00",
				"23-Jul-15 10:00",
				"24-Jul-15 10:00",
				"25-Jul-15 10:00",
			}, points);
		}

		[TestDate(2015, 7, 14)]
		public void TestScaleLabels_WhenDiagramCountsBackFromScheduledFinishTime_AndDiagramDurationIsZero()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(12);

			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();
			network.RefreshSchedules();
			Factory.Save();

			AssertEquals("Diagram should have no duration as there are no shapes", 0m, diagram.ExplicitDurationHours);

			var descriptor = network.ScaleDescriptor;
			var points = descriptor.GetScaleSetForColumns(3);

			CombineAssertions("Scale labels", () =>
			{
				AssertEquals("Index=0", "1 day", points.ScalePoints[0].Label);
				AssertEquals("Index=1", "2 days", points.ScalePoints[1].Label);
				AssertEquals("Index=2", "3 days", points.ScalePoints[2].Label);
			});
		}

		[TestDate(2015, 7, 14)]
		public void TestScaleLabels_WhenDiagramHasScheduledStartAndFinishDate()
		{
			WorkingDaysTestHelper.UpdateEveryDayTo9To5(Factory, Env.CurrentDepartmentPK);

			var diagram = NetworkTestCase.CreateDiagram(Factory);
			diagram.ScheduledStartTimeUtc = ZDateTime.UtcNow;
			diagram.ScheduledFinishTimeUtc = ZDateTime.UtcNow.AddDays(12);

			var network = NetworkTestCase.CreateNetwork(diagram);
			network.SwitchToScaled();
			network.RefreshSchedules();
			Factory.Save();

			AssertEquals("Diagram should be 12 days in duration (in working hours)", 12 * 8m, diagram.ExplicitDurationHours);

			var descriptor = network.ScaleDescriptor;
			var points = descriptor.GetScaleSetForColumns(7);

			CombineAssertions("Scale labels", () =>
			{
				AssertEquals("Index=0", "14-Jul-15 10:00", points.ScalePoints[0].Label);
				AssertEquals("Index=1", "15-Jul-15 10:00", points.ScalePoints[1].Label);
				AssertEquals("Index=2", "16-Jul-15 10:00", points.ScalePoints[2].Label);
				AssertEquals("Index=3", "17-Jul-15 10:00", points.ScalePoints[3].Label);
				AssertEquals("Index=4", "18-Jul-15 10:00", points.ScalePoints[4].Label);
				AssertEquals("Index=5", "19-Jul-15 10:00", points.ScalePoints[5].Label);
				AssertEquals("Index=6", "20-Jul-15 10:00", points.ScalePoints[6].Label);
			});
		}

		public void TestGetLabel()
		{
			AssertScale(1, "1 minute", "2 minutes", "3 minutes");
			AssertScale(60, "1 hour", "2 hours", "3 hours", "4 hours");
			AssertScale(120, "2 hours", "4 hours", "6 hours", "1 day", "1.25 days");
			AssertScale(60 * 8, "1 day", "2 days", "3 days", "4 days");
			AssertScale(60 * 8 * 5, "1 week", "2 weeks", "3 weeks", "4 weeks");
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1050:Use System.TimeSpan Type For A Duration", Justification = "Baseline")]
		void AssertScale(int scaleMinutes, params string[] scales)
		{
			var diagram = Factory.New<BMNCNShape>();
			var network = CreateNetwork(diagram);
			diagram.IsScaled = true;
			diagram.Scale = new ZInt(scaleMinutes).GetDateTimeFromMinutes();

			var descriptor = network.ScaleDescriptor;
			AssertNotNull(descriptor);
			var set = descriptor.GetScaleSetForColumns(scales.Length);

			for (var i = 0; i < scales.Length; i++)
			{
				AssertEquals(scales[i], set.ScalePoints[i].Label);
			}
		}

		#endregion

		#region Background Color

		[TestDate(2014, 7, 24, 0, 0, 0)]
		public void TestBackgroundColor_ForDiagramWithStartTime()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagram = CreateDiagram(Factory);
			diagram.BackColor = string.Empty;
			diagram.ScheduledStartTimeUtc = new ZDateTime(2014, 7, 21);

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();
			var descriptor = network.ScaleDescriptor;

			AssertScaleDetails(descriptor, 0, Color.FromArgb(255, 221, 221, 221), string.Empty);
			AssertScaleDetails(descriptor, 1, Color.FromArgb(255, 221, 221, 221), string.Empty);
			AssertScaleDetails(descriptor, 2, Color.FromArgb(255, 221, 221, 221), string.Empty);
			AssertScaleDetails(descriptor, 3, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty);
		}

		[TestDate(2014, 7, 24, 9, 0, 0)]
		public void TestBackgroundColor_ForDiagramWithStartTime_DifferentTimeZone()
		{
			//Current department TimeZone = UTC + 10
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagram = CreateDiagram(Factory);
			diagram.BackColor = string.Empty;
			diagram.ScheduledStartTimeUtc = new ZDateTime(2014, 7, 21);

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();
			var descriptor = network.ScaleDescriptor;

			AssertScaleDetails(descriptor, 0, Color.FromArgb(255, 221, 221, 221), string.Empty);
			AssertScaleDetails(descriptor, 1, Color.FromArgb(255, 221, 221, 221), string.Empty);
			AssertScaleDetails(descriptor, 2, Color.FromArgb(255, 221, 221, 221), string.Empty);
			AssertScaleDetails(descriptor, 3, Color.FromArgb(255, 221, 221, 221), string.Empty); //UTC 9:00 = 19:00 department local time (UTC + 10), which has advanced this day
			AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty);

			// change branch/dep to a new TimeZone with UTC + 0, ie. London "GBLON"			
			GlbBranch branch1 = Factory.NewWithValidTestData<GlbBranch>();
			branch1.GB_GC = GlbCompany.CurrentCompany.PK;
			branch1.GB_RL_NKHomePort = "GBLON"; //London - UTC + 0 port.Code;

			GlbDepartment department1 = Factory.NewWithValidTestData<GlbDepartment>();
			department1.GE_Code = "DP1";
			AssertNotNull(department1.WorkTimes.ParentID); // this is required to force WorkTimes to be loaded/create

			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, department1.PK);

			var schedule = diagram.GetOrCreateSchedule();
			schedule.BNC_GB_Branch = branch1.PK;
			schedule.BNC_GE_Department = department1.PK;
			diagram.ScheduledStartTimeUtc = new ZDateTime(2014, 7, 21, 10, 0, 0); //start at 10 UTC

			network.FullRefresh();
			descriptor = network.ScaleDescriptor;

			AssertScaleDetails(descriptor, 1, Color.FromArgb(255, 221, 221, 221), string.Empty);
			AssertScaleDetails(descriptor, 2, Color.FromArgb(255, 221, 221, 221), string.Empty);
			AssertScaleDetails(descriptor, 3, Color.Empty, string.Empty); //UTC 9:00 = 9:00 department local time (UTC + 0), which has not advanced this day
			AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty);

			//Now login to the new branch with UTC + 0
			var createUser = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();
			using (Env.SetTemporaryUserContext(createUser.GS_LoginName, branch1.PK.ToGuid(), department1.PK.ToGuid()))
			{
				network.FullRefresh();

				descriptor = network.ScaleDescriptor;
				AssertScaleDetails(descriptor, 0, Color.FromArgb(255, 221, 221, 221), string.Empty);
				AssertScaleDetails(descriptor, 1, Color.FromArgb(255, 221, 221, 221), string.Empty);
				AssertScaleDetails(descriptor, 2, Color.FromArgb(255, 221, 221, 221), string.Empty);
				AssertScaleDetails(descriptor, 3, Color.Empty, string.Empty); //UTC 9:00 = 9:00 department local time (UTC + 0), which has not advanced this day
				AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty);
				AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty);
			}
		}

		#endregion

		#region Leveling Rules

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestConcurrencyLevelingRuleViolation()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 2);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 150, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 0, 300, 300, 100);

			AssertScaleDetails(descriptor, 0, Color.Chartreuse, string.Empty); // 3 shapes on this timeslot -- exceeds specified concurrency value
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty); // 2 shapes on this timeslot
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty); // 1 shape on this timeslot
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestConcurrencyLevelingRuleViolation_SingleApplicableChannel()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1);

			var channel1 = NetworkTestCase.CreateChannel(diagramShape, "ABC", height: 400);
			var channel2 = NetworkTestCase.CreateChannel(diagramShape, "BBC", height: 200);

			NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel1);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 50, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 250, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 0, 450, 300, 100);

			AssertScaleDetails(descriptor, 0, Color.Chartreuse, string.Empty); // 3 shapes on this timeslot, but only shape 1 and 2 contribute -- exceeds specified concurrency value
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty); // 2 shapes on this timeslot, but only shape 2 contributes -- no violation
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty); // 1 shape on this timeslot -- shape 3 only -- no violation
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestConcurrencyLevelingRuleViolation_MultipleApplicableChannels()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1);

			var channel1 = NetworkTestCase.CreateChannel(diagramShape, "ABC", height: 200);
			var channel2 = NetworkTestCase.CreateChannel(diagramShape, "BBC", height: 200);
			var channel3 = NetworkTestCase.CreateChannel(diagramShape, "CBC", height: 200);

			NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel1);
			NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel3);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 50, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 250, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 0, 450, 300, 100);

			AssertScaleDetails(descriptor, 0, Color.Chartreuse, string.Empty); // 3 shapes on this timeslot, but only shape 1 and 3 contribute -- exceeds specified concurrency value
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty); // 2 shapes on this timeslot, but only shape 3 contributes -- no violation
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty); // 1 shape on this timeslot -- shape 3 only -- no violation
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestStartLevelingRuleViolation()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, value: 2);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 200, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 100, 150, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 200, 300, 100, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty); // no shapes precede childShape1
			AssertScaleDetails(descriptor, 1, Color.Chartreuse, string.Empty); // the start of childshape1 precedes the start of childShape2 by one timeslot
			AssertScaleDetails(descriptor, 2, Color.Chartreuse, string.Empty); // the start of childshape2 precedes the start of childShape3 by one timeslot
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestStartLevelingRuleViolation_SingleApplicableChannel()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, value: 2);

			var channel1 = NetworkTestCase.CreateChannel(diagramShape, "ABC", height: 400);
			var channel2 = NetworkTestCase.CreateChannel(diagramShape, "BBC", height: 200);

			NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel1);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 50, 200, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 100, 250, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 200, 450, 100, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty); // no shapes precede childShape1
			AssertScaleDetails(descriptor, 1, Color.Chartreuse, string.Empty); // the start of childshape1 precedes the start of childShape2 by one timeslot
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty); // the start of childshape2 precedes the start of childShape3 by one timeslot, but doesn't violate since since childShape3 is in a non-violating channel
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestStartLevelingRuleViolation_MultipleApplicableChannels()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, value: 2);

			var channel1 = NetworkTestCase.CreateChannel(diagramShape, "ABC", height: 200);
			var channel2 = NetworkTestCase.CreateChannel(diagramShape, "BBC", height: 200);
			var channel3 = NetworkTestCase.CreateChannel(diagramShape, "CBC", height: 200);

			NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel1);
			NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel2);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 50, 200, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 100, 250, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 200, 450, 100, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty); // no shapes precede childShape1
			AssertScaleDetails(descriptor, 1, Color.Chartreuse, string.Empty); // the start of childshape1 precedes the start of childShape2 by one timeslot
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty); // the start of childshape2 precedes the start of childShape3 by one timeslot, but doesn't violate since childShape3 is in a non-applicable channel
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestGapLevelingRuleViolation_OneTimescale()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 1);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 100, 150, 200, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty); // no shapes precede childShape1
			AssertScaleDetails(descriptor, 1, Color.Chartreuse, string.Empty); // the start of childShape2 aligns with the end of childShape2 and a gap of 1 timeslot is required
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty);
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestGapLevelingRuleViolation_MultipleTimescales()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 2);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 100, 150, 200, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty); // no shapes precede childShape1
			AssertScaleDetails(descriptor, 1, Color.Chartreuse, string.Empty); // the start of childShape2 aligns with the end of childShape2 and a gap of 2 timeslots is required
			AssertScaleDetails(descriptor, 2, Color.Chartreuse, string.Empty); // a gap of 2 timeslots is required, so highlight both timeslots
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestGapLevelingRuleViolation_OverlappingShapes_OneOverlap()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 1);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 150, 300, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 300, 300, 300, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty); // child shapes 1 and 2 overlap here -- no violation
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty); // no shapes overlap here and not start of shape 2 -- no violation
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty); // no shapes overlap here and not start of shape 2  -- no violation
			AssertScaleDetails(descriptor, 3, Color.Chartreuse, string.Empty); // violation since start of shape 3 is too close to shape 2 and does not overlap with any shape
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestGapLevelingRuleViolation_OverlappingShapes_TwoOverlaps()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 1);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 150, 300, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 200, 300, 300, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty); // child shapes 1 and 2 overlap here -- no violation
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty); // no shapes overlap here and not start of shape 2 -- no violation
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty); // shapes 2 and 3 overlap here -- no violation
			AssertScaleDetails(descriptor, 3, Color.Empty, string.Empty); // no shapes overlap here and not start of shape 3 -- no violation 
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestGapLevelingRuleViolation_OverlappingShapes_OneShapeCompletelyOverlapsAnotherShape_ShouldNotCauseViolation()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 1);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 800, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 100, 150, 600, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 3, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 6, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 7, Color.Empty, string.Empty);
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestGapLevelingRuleViolation_OverlappingShapes_OneShapeCompletelyOverlapsTwoOtherShapes_ShouldCauseViolation()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 1);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 300, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 300, 150, 300, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 100, 300, 400, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 3, Color.Chartreuse, string.Empty);
			AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty);
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestGapLevelingRuleViolation_SingleApplicableChannel()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 2);

			var channel1 = NetworkTestCase.CreateChannel(diagramShape, "ABC", height: 400);
			var channel2 = NetworkTestCase.CreateChannel(diagramShape, "BBC", height: 200);

			NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel1);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 50, 200, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 200, 250, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 400, 450, 200, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty); // no violation
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty); // no violation
			AssertScaleDetails(descriptor, 2, Color.Chartreuse, string.Empty); // violation because shape 2 within two units of shape 1 and in applicable channel
			AssertScaleDetails(descriptor, 3, Color.Chartreuse, string.Empty); // violation because shape 2 within two units of shape 1 and in applicable channel
			AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty); // no violation because shape 3 within two units of shape 2, but not in applicable channel
			AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty); // no violation because shape 3 within two units of shape 2, but not in applicable channel
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestGapLevelingRuleViolation_MultipleApplicableChannels()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 2);

			var channel1 = NetworkTestCase.CreateChannel(diagramShape, "ABC", height: 200);
			var channel2 = NetworkTestCase.CreateChannel(diagramShape, "BBC", height: 200);
			var channel3 = NetworkTestCase.CreateChannel(diagramShape, "CBC", height: 200);

			NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel1);
			NetworkTestCase.CreateLevelingRuleChannelLink(rule, channel2);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 50, 200, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 200, 250, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 400, 450, 200, 100);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty); // no violation
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty); // no violation
			AssertScaleDetails(descriptor, 2, Color.Chartreuse, string.Empty); // violation because shape 2 within two units of shape 1 and in applicable channel
			AssertScaleDetails(descriptor, 3, Color.Chartreuse, string.Empty); // violation because shape 2 within two units of shape 1 and in applicable channel
			AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty); // no violation because shape 3 within two units of shape 2, but not in applicable channel
			AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty); // no violation because shape 3 within two units of shape 2, but not in applicable channel
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestMultipleLevelingRuleViolations_ShouldRenderSingleViolationInTimeslot()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var jobHeader4 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule1 = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, name: "Zutroy", colorName: Color.HotPink.Name);
			var rule2 = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, name: "Worker", colorName: Color.Red.Name);
			var rule3 = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, name: "Parasite", colorName: Color.AliceBlue.Name);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 150, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 0, 300, 300, 100);
			var childShape4 = NetworkTestCase.CreateShapeAtLocation(jobHeader4, diagram, 300, 450, 100, 100);

			AssertScaleDetails(descriptor, 0, Color.AliceBlue, string.Empty); // Concurrency and start violations in this timeslot -- Parasite (Alice Blue) takes precedence
			AssertScaleDetails(descriptor, 1, Color.HotPink, string.Empty); // Concurrency violation in this timeslot -- Zutroy (Hot Pink)
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty); // no violation
			AssertScaleDetails(descriptor, 3, Color.Red, string.Empty); // Gap violation in this timeslot -- -- Worker (Red)
			AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty); // no violation
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestMultipleLevelingRuleViolations_MultipleApplicableChannels()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var jobHeader4 = CreateJobHeader<OrgHeader>();
			var jobHeader5 = CreateJobHeader<OrgHeader>();
			var jobHeader6 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var channel1 = NetworkTestCase.CreateChannel(diagramShape, "ABC", height: 300);
			var channel2 = NetworkTestCase.CreateChannel(diagramShape, "BBC", height: 300);
			var channel3 = NetworkTestCase.CreateChannel(diagramShape, "CBC", height: 300);

			var ruleConAbcCbc = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1, colorName: Color.Orange.Name, name: "Don't go CONNIN My Heart");
			var ruleGapBbc = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 2, colorName: Color.Blue.Name, name: "GAPPIN on the Ritz");
			var ruleSrtBbcCbc = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, value: 1, colorName: Color.Chocolate.Name, name: "Gotta be STARTIN Something");
			var ruleGapAbc = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 2, colorName: Color.Red.Name, name: "Never gonna GAP you up");

			NetworkTestCase.CreateLevelingRuleChannelLink(ruleConAbcCbc, channel1);
			NetworkTestCase.CreateLevelingRuleChannelLink(ruleConAbcCbc, channel3);

			NetworkTestCase.CreateLevelingRuleChannelLink(ruleSrtBbcCbc, channel2);
			NetworkTestCase.CreateLevelingRuleChannelLink(ruleSrtBbcCbc, channel3);

			NetworkTestCase.CreateLevelingRuleChannelLink(ruleGapAbc, channel1);

			NetworkTestCase.CreateLevelingRuleChannelLink(ruleGapBbc, channel2);

			var abcShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 50, 200, 100);
			var abcShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 200, 200, 200, 100);
			var bbcShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 400, 350, 200, 100);
			var bbcShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader4, diagram, 600, 500, 200, 100);
			var cbcShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader5, diagram, 0, 650, 200, 100);
			var cbcShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader6, diagram, 400, 650, 200, 100);

			AssertScaleDetails(descriptor, 0, Color.Orange, string.Empty); // abcShape1 and cbcShape1
			AssertScaleDetails(descriptor, 1, Color.Orange, string.Empty); // abcShape1 and cbcShape1
			AssertScaleDetails(descriptor, 2, Color.Red, string.Empty); // abcShape1 and abcShape2 
			AssertScaleDetails(descriptor, 3, Color.Red, string.Empty); // abcShape1 and abcShape2 
			AssertScaleDetails(descriptor, 4, Color.Chocolate, string.Empty); // bbcShape1 and cbcShape2
			AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 6, Color.Blue, string.Empty); // bbcShape1 and bbcShape2
			AssertScaleDetails(descriptor, 7, Color.Blue, string.Empty); // bbcShape1 and bbcShape2
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestRefreshLevelingViolations()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 2);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 150, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 0, 300, 300, 100);

			AssertScaleDetails(descriptor, 0, Color.Chartreuse, string.Empty); // 3 shapes on this timeslot -- exceeds specified concurrency value
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty); // 2 shapes on this timeslot
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty); // 1 shape on this timeslot

			childShape2.X = 500;

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty);
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestLevelingRulesOverrideAffinities()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var jobHeader3 = CreateJobHeader<OrgHeader>();

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var affinity = diagramShape.ShapeAffinities.AddNew();
			affinity.Color = "Brown";
			affinity.Name = "Worshipful Company of Liver Peddlers";

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 2);

			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 150, 200, 100);
			var childShape3 = NetworkTestCase.CreateShapeAtLocation(jobHeader3, diagram, 0, 300, 300, 100);

			((IDiagramEntity)diagram).CreateAffinityLink(childShape1, affinity);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape2, affinity);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape3, affinity);

			AssertScaleDetails(descriptor, 0, Color.Chartreuse, "The affinity Worshipful Company of Liver Peddlers is overloaded at this position."); // render leveling rule colour here, even though there is a concurrency violation by affinity as well.
			AssertScaleDetails(descriptor, 1, Color.Brown, "The affinity Worshipful Company of Liver Peddlers is overloaded at this position."); // render affinity colour here.
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty);
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestLevelingRulesNotApplied_ForAnnotationsNestedAndClosedShapes()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			jobHeader1.FH_Status = "CLS";
			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 150, 100, 100);

			var jobHeader3 = CreateJobHeader<OrgHeader>();
			var childChildShape = NetworkTestCase.CreateShapeAtLocation(jobHeader3, childShape2, 0, 150, 100, 100);

			var annotation = networkViewModel.CreateNewAnnotation(diagram);
			annotation.X = 0;
			annotation.Y = 300;
			annotation.Width = 100;
			annotation.Height = 100;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
		}

		[TestDate(2018, 11, 19, 0, 0, 0)]
		public void TestLevelingRulesNotApplied_ForBuffers()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2018, 11, 19);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;

			var jobHeader = CreateJobHeader<OrgHeader>();
			var workflow = jobHeader.ProcessHeaders.AddNew();
			var task = VisualBoardsTestHelper.CreateTask(workflow, GlbStaff.CurrentUser.GS_Code, 60);
			var childShape = NetworkTestCase.CreateShapeAtLocation(jobHeader, diagram, 0, 0, 100, 100);

			networkViewModel.SuggestAndAcceptAllBuffers();

			var bufferShape = network.Shapes.OfType<BMNCNBufferShape>().Single();
			AssertEquals(WorkStatus.None, ((IProposedNetworkEntity)bufferShape).Status);

			var bufferEntity = network.Entities.GetInstance(bufferShape);
			bufferEntity.X = diagram.X;
			bufferEntity.Y = diagram.Y + 150;

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MaximumConcurrentEntities, value: 1);
			var descriptor = network.ScaleDescriptor;

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
		}

		[TestDate(2019, 1, 9, 0, 0, 0)]
		public void TestStartLevelingRuleViolation_WhenShapeIsClosed_ShouldNotApply()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2019, 1, 9);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			jobHeader1.FH_Status = "CLS";
			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 0, 150, 100, 100);

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumEntityStartGapSize, value: 1);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);

			jobHeader1.FH_Status = "OPN";

			AssertScaleDetails(descriptor, 0, Color.Chartreuse, string.Empty);
		}

		[TestDate(2019, 1, 9, 0, 0, 0)]
		public void TestGapLevelingRuleViolation_WhenShapeIsClosed_ShouldNotApply()
		{
			WorkingDaysTestHelper.UpdateWeekDaysTo9To5(Factory, Env.CurrentDepartment.PK);

			var diagramShape = NetworkTestCase.CreateDiagram(Factory);
			diagramShape.BackColor = string.Empty;
			diagramShape.IsScaled = true;
			diagramShape.Scale = diagramShape.ResolutionIncrement = new ZInt(60 * BMConstants.WorkingHoursPerDay).GetDateTimeFromMinutes();
			diagramShape.ScheduledStartTimeUtc = new ZDateTime(2019, 1, 9);

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			var descriptor = network.ScaleDescriptor;

			var jobHeader1 = CreateJobHeader<OrgHeader>();
			jobHeader1.FH_Status = "CLS";
			var childShape1 = NetworkTestCase.CreateShapeAtLocation(jobHeader1, diagram, 0, 0, 100, 100);

			var jobHeader2 = CreateJobHeader<OrgHeader>();
			var childShape2 = NetworkTestCase.CreateShapeAtLocation(jobHeader2, diagram, 100, 150, 100, 100);

			var rule = NetworkTestCase.CreateLevelingRule(diagramShape, type: LevelingRuleTypeList.Codes.MinimumGapSizeBetweenEntities, value: 1);

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty);

			jobHeader1.FH_Status = "OPN";

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.Chartreuse, string.Empty);
		}

		#endregion

		#region Affinities

		public void TestOverloadedAffinity()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			diagramShape.BackColor = string.Empty;

			var affinity1 = diagramShape.ShapeAffinities.AddNew();
			var affinity2 = diagramShape.ShapeAffinities.AddNew();
			affinity1.Name = "affinity1";
			affinity2.Name = "affinity2";
			affinity1.Color = Color.Lavender.Name;
			affinity2.Color = Color.HotPink.Name;

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			network.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);
			var childShape4 = networkViewModel.CreateNewShape(diagram);
			var childShape5 = networkViewModel.CreateNewShape(diagram);

			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";
			childShape3.Name = "childShape3";
			childShape4.Name = "childShape4";
			childShape5.Name = "childShape5";

			((IDiagramEntity)diagram).CreateAffinityLink(childShape1, affinity1);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape1, affinity2);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape2, affinity2);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape3, affinity2);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape4, affinity1);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape5, affinity1);

			((INetworkEntity)childShape1).Width = 200;
			((INetworkEntity)childShape2).Width = 200;
			((INetworkEntity)childShape3).Width = 200;
			((INetworkEntity)childShape4).Width = 200;
			((INetworkEntity)childShape5).Width = 200;

			foreach (var entity in network.Entities)
			{
				AssertEquals(200d, entity.Width);
			}

			((INetworkEntity)childShape1).X = 0;
			((INetworkEntity)childShape2).X = 100; // Overlapping with childShape1
			((INetworkEntity)childShape3).X = 300;
			((INetworkEntity)childShape4).X = 400; // Overlapping with childShape3
			((INetworkEntity)childShape5).X = 600; // Not involved in an overlap

			var descriptor = network.ScaleDescriptor;

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.HotPink, "The affinity affinity2 is overloaded at this position.");
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 3, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 4, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 5, Color.Empty, string.Empty);
		}

		public void TestOverloadedAffinity_ShouldHighlightMostOverloadedAffinity()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			diagram.BackColor = Color.Empty;

			var affinity1 = diagram.ShapeAffinities.AddNew();
			var affinity2 = diagram.ShapeAffinities.AddNew();
			affinity1.Name = "affinity1";
			affinity2.Name = "affinity2";
			affinity1.Color = Color.Lavender.Name;
			affinity2.Color = Color.HotPink.Name;

			network.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);

			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";
			childShape3.Name = "childShape3";

			((IDiagramEntity)diagram).CreateAffinityLink(childShape1, affinity1);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape1, affinity2);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape2, affinity1);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape2, affinity2);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape3, affinity2);

			((INetworkEntity)childShape1).Width = 200;
			((INetworkEntity)childShape2).Width = 200;
			((INetworkEntity)childShape3).Width = 200;

			foreach (var entity in network.Entities)
			{
				AssertEquals(200d, entity.Width);
			}

			// All shapes are overlapping
			((INetworkEntity)childShape1).X = 0;
			((INetworkEntity)childShape2).X = 0;
			((INetworkEntity)childShape3).X = 0;

			var descriptor = network.ScaleDescriptor;

			AssertScaleDetails(descriptor, 0, Color.HotPink, "The affinity affinity2 is overloaded at this position.");
			AssertScaleDetails(descriptor, 1, Color.HotPink, "The affinity affinity2 is overloaded at this position.");

			((IDiagramEntity)diagram).RemoveAffinityLink(childShape2, affinity2);
			((IDiagramEntity)diagram).RemoveAffinityLink(childShape3, affinity2);

			descriptor.ClearCachedData();

			AssertScaleDetails(descriptor, 0, Color.Lavender, "The affinity affinity1 is overloaded at this position.");
			AssertScaleDetails(descriptor, 1, Color.Lavender, "The affinity affinity1 is overloaded at this position.");
		}

		[TestDate(2015, 10, 6)]
		public void TestOverloadedAffinity_ShouldHighlightMostOverloadedAffinityEvenIfScaleTimeIsInThePast()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			diagram.BackColor = Color.Empty;
			diagram.ScheduledStartTimeLocal = new ZDateTime(2015, 10, 1);

			var affinity1 = diagram.ShapeAffinities.AddNew();
			var affinity2 = diagram.ShapeAffinities.AddNew();
			affinity1.Name = "affinity1";
			affinity2.Name = "affinity2";
			affinity1.Color = Color.Lavender.Name;
			affinity2.Color = Color.HotPink.Name;

			network.SwitchToScaled();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);

			childShape1.Name = "childShape1";
			childShape2.Name = "childShape2";
			childShape3.Name = "childShape3";

			((IDiagramEntity)diagram).CreateAffinityLink(childShape1, affinity1);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape1, affinity2);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape2, affinity1);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape2, affinity2);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape3, affinity2);

			((INetworkEntity)childShape1).Width = 200;
			((INetworkEntity)childShape2).Width = 200;
			((INetworkEntity)childShape3).Width = 200;

			foreach (var entity in network.Entities)
			{
				AssertEquals(200d, entity.Width);
			}

			// All shapes are overlapping
			((INetworkEntity)childShape1).X = 0;
			((INetworkEntity)childShape2).X = 0;
			((INetworkEntity)childShape3).X = 0;

			var descriptor = network.ScaleDescriptor;

			AssertScaleDetails(descriptor, 0, Color.HotPink, "The affinity affinity2 is overloaded at this position.");
			AssertScaleDetails(descriptor, 1, Color.HotPink, "The affinity affinity2 is overloaded at this position.");
			AssertScaleDetails(descriptor, 2, Color.FromArgb(255, 221, 221, 221), string.Empty);

			((IDiagramEntity)diagram).RemoveAffinityLink(childShape2, affinity2);
			((IDiagramEntity)diagram).RemoveAffinityLink(childShape3, affinity2);

			descriptor.ClearCachedData();

			AssertScaleDetails(descriptor, 0, Color.Lavender, "The affinity affinity1 is overloaded at this position.");
			AssertScaleDetails(descriptor, 1, Color.Lavender, "The affinity affinity1 is overloaded at this position.");
			AssertScaleDetails(descriptor, 2, Color.FromArgb(255, 221, 221, 221), string.Empty);
		}

		public void TestOverloadedAffinity_MultipleConcurrencyAllowed()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			diagramShape.BackColor = string.Empty;

			var affinity = diagramShape.ShapeAffinities.AddNew();
			affinity.Name = "Cumberbitches";
			affinity.AllowedConcurrency = 2;
			affinity.Color = Color.HotPink.Name;

			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();
			var diagram = network.DiagramEntity;

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(diagram);

			((IDiagramEntity)diagram).CreateAffinityLink(childShape1, affinity);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape2, affinity);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape3, affinity);

			((INetworkEntity)childShape1).Width = 200;
			((INetworkEntity)childShape2).Width = 200;
			((INetworkEntity)childShape3).Width = 200;

			foreach (var entity in network.Entities)
			{
				AssertEquals(200d, entity.Width);
			}

			((INetworkEntity)childShape1).X = 0;
			((INetworkEntity)childShape2).X = 100; // Overlapping with childShape1
			((INetworkEntity)childShape3).X = 200;

			var descriptor = network.ScaleDescriptor;

			for (var scaleIndex = 0; scaleIndex < 4; scaleIndex++)
			{
				AssertScaleDetails(descriptor, scaleIndex, Color.Empty, string.Empty);
			}

			((INetworkEntity)childShape3).X = 100; // Overlapping with childShape1 and childShape2

			descriptor.ClearCachedData();

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.HotPink, "The affinity Cumberbitches is overloaded at this position.");
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 3, Color.Empty, string.Empty);
		}

		public void TestOverloadedAffinity_ForSubDiagramChildren()
		{
			var diagramShape = Factory.New<BMNCNShape>();
			var networkViewModel = CreateNetworkViewModel(diagramShape);
			var network = networkViewModel.GetJobNetwork();
			var diagram = network.DiagramEntity;
			diagram.BackColor = Color.Empty;

			var subDiagram = CreateShape(diagram);
			subDiagram.BackColor = Color.Lavender;

			var affinity = diagram.ShapeAffinities.AddNew();
			affinity.Name = "Cumberbitches";
			affinity.AllowedConcurrency = 2;
			affinity.Color = Color.HotPink.Name;

			network.SwitchToScaled();
			networkViewModel.Refresh();

			var childShape1 = networkViewModel.CreateNewShape(diagram);
			var childShape2 = networkViewModel.CreateNewShape(diagram);
			var childShape3 = networkViewModel.CreateNewShape(subDiagram);

			((IDiagramEntity)diagram).CreateAffinityLink(childShape1, affinity);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape2, affinity);
			((IDiagramEntity)diagram).CreateAffinityLink(childShape3, affinity);

			((INetworkEntity)childShape1).Width = 200;
			((INetworkEntity)childShape2).Width = 200;
			((INetworkEntity)subDiagram).Width = 200;
			((INetworkEntity)childShape3).Width = 200;

			foreach (var entity in network.Entities)
			{
				AssertEquals(200d, entity.Width);
			}

			((INetworkEntity)childShape1).X = 0;
			((INetworkEntity)childShape2).X = 100; // Overlapping with childShape1
			((INetworkEntity)childShape3).X = 200;

			var descriptor = network.ScaleDescriptor;

			for (var scaleIndex = 0; scaleIndex < 4; scaleIndex++)
			{
				AssertScaleDetails(descriptor, scaleIndex, Color.Empty, string.Empty);
			}

			((INetworkEntity)childShape3).X = 100; // Overlapping with childShape1 and childShape2

			descriptor.ClearCachedData();

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.HotPink, "The affinity Cumberbitches is overloaded at this position.");
			AssertScaleDetails(descriptor, 2, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 3, Color.Empty, string.Empty);
		}

		public void TestOverloadedAffinity_ShouldIgnoreCompletedShapes()
		{
			var diagram = CreateDiagram(Factory);
			var shape1 = CreateShape(diagram);
			var shape2 = CreateShape(diagram);

			var affinity = CreateAffinity(diagram);
			affinity.AllowedConcurrency = 1;

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();

			LinkAffinity(network, shape1, affinity);
			LinkAffinity(network, shape2, affinity);

			var descriptor = network.ScaleDescriptor;

			AssertScaleDetails(descriptor, 0, Color.Red, "The affinity Lemon is overloaded at this position.");
			AssertScaleDetails(descriptor, 1, Color.Red, "The affinity Lemon is overloaded at this position.");

			descriptor.ClearCachedData();

			shape1.BNS_Status = ShapeStatusList.Codes.Closed;

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty);
		}

		public void TestOverloadedAffinity_ShouldIgnoreShapesLinkedToCompletedEntities()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);
			var workflow1 = BMSTestHelper.CreateWorkflow(jobHeader, "Bendy");
			var workflow2 = BMSTestHelper.CreateWorkflow(jobHeader, "Baps");

			var task1 = BMSTestHelper.CreateTask(workflow1, GlbStaff.CurrentUser.GS_Code);
			var task2 = BMSTestHelper.CreateTask(workflow2, GlbStaff.CurrentUser.GS_Code);

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);

			var affinity = CreateAffinity(diagram);
			affinity.AllowedConcurrency = 1;

			var network = CreateNetwork(diagram);
			network.SwitchToScaled();

			LinkAffinity(network, shape1, affinity);
			LinkAffinity(network, shape2, affinity);

			var descriptor = network.ScaleDescriptor;

			AssertScaleDetails(descriptor, 0, Color.Red, "The affinity Lemon is overloaded at this position.");
			AssertScaleDetails(descriptor, 1, Color.Red, "The affinity Lemon is overloaded at this position.");

			descriptor.ClearCachedData();

			task1.P9_Status = ProcessTaskStatusCodeList.Codes.Closed;
			Factory.Save();

			AssertScaleDetails(descriptor, 0, Color.Empty, string.Empty);
			AssertScaleDetails(descriptor, 1, Color.Empty, string.Empty);
		}

		void AssertScaleDetails(INetworkScaleDescriptor descriptor, int columnIndex, Color backColor, string tooltip)
		{
			var set = descriptor.GetScaleSetForColumns(columnIndex + 1);
			var point = set.ScalePoints.Last();
			var lastBackground = set.BackgroundPoints.LastOrDefault();

			var lastColor = lastBackground != null && (lastBackground.EndColumn > columnIndex) ? lastBackground.Color : Color.Empty;

			var message = string.Format("Scale details for column index {0}, with label {1}", columnIndex, point.Label);
			CombineAssertions(message, () =>
			{
				AssertEquals("Background color at index " + columnIndex, backColor, lastColor);
				AssertEquals("Tool tip at index " + columnIndex, tooltip, point.ToolTip);
			});
		}

		#endregion

		#region Performance

		[TestDate(2017, 10, 11)]
		public void TestGetOverloadedAffinities_ShouldNotWalkDependencyNetworks()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = CreateWorkflow(jobHeader, "Workflow1");
			var workflow2 = CreateWorkflow(jobHeader, "Workflow2");
			var workflow3 = CreateWorkflow(jobHeader, "Workflow3");
			var workflow4 = CreateWorkflow(jobHeader, "Workflow4");

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram);
			var shape2 = CreateShape(workflow2, diagram);
			var shape3 = CreateShape(workflow3, diagram);
			var shape4 = CreateShape(workflow4, diagram);

			shape1.MakeVisiblePrerequisiteOf(shape2, diagram);
			shape2.MakeVisiblePrerequisiteOf(shape3, diagram);
			shape4.MakeVisiblePrerequisiteOf(shape3, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);

			networkViewModel = CreateNetworkViewModel(loadedDiagram);
			network = networkViewModel.GetJobNetwork();
			new SuggestBufferAction(networkViewModel).ExecuteForEntityWithoutAccessCheck(network.DiagramEntity);

			newFactory.ResetDatabaseLoadCount();
			network.ScaleDescriptor.GetScaleSetForColumns(20);

			// Getting scale info should incur no extra db hits
			// as it is optimised to use cached properties for whether workflows are closed.
			AssertDbHits(new Dictionary<string, int>(), newFactory);

			newFactory.ResetDatabaseLoadCount();
			network.ScaleDescriptor.GetScaleSetForColumns(20);

			// Getting scale info again should incur no extra db hits at all.
			AssertDbHits(new Dictionary<string, int>(), newFactory);
		}

		[TestDate(2017, 10, 11)]
		public void TestGetLevelingRules_DBHits()
		{
			var jobHeader = BMSTestHelper.CreateJobHeader<OrgHeader>(Factory, addDefaultProcessHeaderIfNone: false);

			var workflow1 = CreateWorkflow(jobHeader, "Workflow1");

			var diagram = CreateDiagram(jobHeader);
			var shape1 = CreateShape(workflow1, diagram);

			var networkViewModel = CreateNetworkViewModel(diagram, createNodeViewModels: false);
			var network = networkViewModel.GetJobNetwork();
			network.SwitchToScaled();

			var rule1 = CreateLevelingRule(diagram, name: "Won", value: 1);
			var rule2 = CreateLevelingRule(diagram, name: "Too", value: 2);
			var rule3 = CreateLevelingRule(diagram, name: "Tree", value: 3);
			var rule4 = CreateLevelingRule(diagram, name: "Fore", value: 4);
			var rule5 = CreateLevelingRule(diagram, name: "Farve", value: 5);
			var rule6 = CreateLevelingRule(diagram, name: "Sicks", value: 6);
			var rule7 = CreateLevelingRule(diagram, name: "Sehr Van", value: 7);
			var rule8 = CreateLevelingRule(diagram, name: "Ate", value: 8);
			var rule9 = CreateLevelingRule(diagram, name: "Nighne", value: 9);
			var rule10 = CreateLevelingRule(diagram, name: "Tenne", value: 10);

			Factory.Save();

			var newFactory = Factory.CreateNewFactory();
			var loadedDiagram = newFactory.Load<BMNCNShape>(diagram.PK);

			networkViewModel = CreateNetworkViewModel(loadedDiagram, createNodeViewModels: false);
			network = networkViewModel.GetJobNetwork();
			newFactory.ResetDatabaseLoadCount();
			network.ScaleDescriptor.GetScaleSetForColumns(20);

			// Getting scale info should only load the leveling rules once
			var hits = new Dictionary<string, int>();
			hits.Add(BMNCNLevelingRuleSchema.Constants.TableName, 1);
			AssertDbHits(hits, newFactory);

			newFactory.ResetDatabaseLoadCount();
			network.ScaleDescriptor.GetScaleSetForColumns(20);

			// Getting scale info again should incur no extra db hits at all.
			AssertDbHits(new Dictionary<string, int>(), newFactory);
		}

		#endregion
	}
}
