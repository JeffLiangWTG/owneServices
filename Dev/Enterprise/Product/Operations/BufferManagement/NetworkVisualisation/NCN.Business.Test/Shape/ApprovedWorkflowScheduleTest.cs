using CargoWise.Types;
using Enterprise.BufferManagement.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.NetworkVisualisation.Business.Test
{
	class ApprovedWorkflowScheduleTest : NetworkTestCase
	{
		public void TestApprovedScheduleType()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader, isScaled: true);
			Factory.Save();

			AssertNull(GetScheduleInNewFactory(jobHeader.PK));

			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			AssertEquals(ApprovedDiagramTypeList.Codes.NCNApprovedDiagram, GetScheduleInNewFactory(jobHeader.PK).VWS_ApprovedScheduleType);

			diagram.IsBuffered = true;
			Factory.Save();

			AssertEquals(ApprovedDiagramTypeList.Codes.CCPMApprovedDiagram, GetScheduleInNewFactory(jobHeader.PK).VWS_ApprovedScheduleType);
		}

		public void TestNCNReleaseOffset()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			diagram.NCNReleaseOffsetMinutes = 150;
			Factory.Save();

			AssertNull(GetScheduleInNewFactory(jobHeader.PK));

			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			AssertEquals(150, GetScheduleInNewFactory(jobHeader.PK).VWS_NCNReleaseOffsetMinutes);

			diagram.IsBuffered = true;
			Factory.Save();

			AssertEquals(0, GetScheduleInNewFactory(jobHeader.PK).VWS_NCNReleaseOffsetMinutes);
		}

		public void TestScheduleTimes()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			diagram.ScheduledStartTimeUtc = new ZDateTime(2014, 7, 12);
			diagram.ScheduledFinishTimeUtc = new ZDateTime(2014, 7, 14);
			Factory.Save();

			var schedule = GetScheduleInNewFactory(jobHeader.PK);
			AssertNull(schedule);

			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);
			Factory.Save();

			schedule = GetScheduleInNewFactory(jobHeader.PK);
			AssertEquals(new ZDateTime(2014, 7, 12), schedule.VWS_ScheduledStartTimeUtc);
			AssertEquals(new ZDateTime(2014, 7, 14), schedule.VWS_ScheduledFinishTimeUtc);
		}

		public void TestScheduleTimes_WithNCNOffsets()
		{
			var jobHeader = CreateJobHeader<OrgHeader>();
			var diagram = CreateDiagram(jobHeader);
			diagram.ScheduledStartTimeUtc = new ZDateTime(2014, 7, 12);
			diagram.ScheduledFinishTimeUtc = new ZDateTime(2014, 7, 14);
			((IApprovable)diagram).Approve(GlbStaff.CurrentUser.GS_Code);

			Factory.Save();

			var schedule = GetScheduleInNewFactory(jobHeader.PK);
			AssertEquals(new ZDateTime(2014, 7, 12), schedule.VWS_ScheduledStartTimeUtc);
			AssertEquals(new ZDateTime(2014, 7, 14), schedule.VWS_ScheduledFinishTimeUtc);

			diagram.NCNReleaseOffsetMinutes = 60;
			Factory.Save();

			schedule = GetScheduleInNewFactory(jobHeader.PK);
			AssertEquals(new ZDateTime(2014, 7, 11, 23, 0, 0), schedule.VWS_ScheduledStartTimeUtc);
			AssertEquals(new ZDateTime(2014, 7, 14), schedule.VWS_ScheduledFinishTimeUtc);

			diagram.NCNReleaseOffsetMinutes = 1440;
			Factory.Save();

			schedule = GetScheduleInNewFactory(jobHeader.PK);
			AssertEquals(new ZDateTime(2014, 7, 11), schedule.VWS_ScheduledStartTimeUtc);
			AssertEquals(new ZDateTime(2014, 7, 14), schedule.VWS_ScheduledFinishTimeUtc);
		}
	}
}
