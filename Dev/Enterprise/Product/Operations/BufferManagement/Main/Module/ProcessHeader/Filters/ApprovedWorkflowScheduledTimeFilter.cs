using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ApprovedWorkflowScheduledTimeFilter : ModuleDateFilter
	{
		public static ApprovedWorkflowScheduledTimeFilter GetScheduledStartTimeFilter()
		{
			return new ApprovedWorkflowScheduledTimeFilter(ViewApprovedWorkflowScheduleSchema.VWS_ScheduledStartTimeUtc, ProcessHeader.ModuleFilterConstants.ApprovedScheduledStartTime, ScheduledStartTimeDescription);
		}

		public static ApprovedWorkflowScheduledTimeFilter GetScheduledFinishTimeFilter()
		{
			return new ApprovedWorkflowScheduledTimeFilter(ViewApprovedWorkflowScheduleSchema.VWS_ScheduledFinishTimeUtc, ProcessHeader.ModuleFilterConstants.ApprovedScheduledFinishTime, ScheduledFinishTimeDescription);
		}

		protected ApprovedWorkflowScheduledTimeFilter(SchemaDateTimeColumn columnOnApprovedScheduleView, ZString description, MultilingualString multilingualDescription)
			: base(description, columnOnApprovedScheduleView, convertFromLocalToUTC: true)
		{
			MultilingualDescription = multilingualDescription;
		}

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			AddViewApprovedWorkflowScheduleSubQuery(query);

			return query;
		}

		protected void AddViewApprovedWorkflowScheduleSubQuery(ZDBOnlyQuery parentQuery)
		{
			var subQuery = new ZDBOnlySubQuery(typeof(ViewApprovedWorkflowSchedule), ViewApprovedWorkflowScheduleSchema.PK, notIn: IsPropertySearchUsingHasNoDateEntered);

			if (!IsPropertySearchUsingHasNoDateEntered)
			{
				AddFilterParameters(subQuery);
			}

			parentQuery.AddSubQuery(ProcessHeaderSchema.PK, subQuery, JoinCondition.And);
		}

		protected static MultilingualString ScheduledStartTimeDescription
		{
			get { return ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ApprovedScheduledStartTime", "Approved Scheduled Start Time"); }
		}

		protected static MultilingualString ScheduledFinishTimeDescription
		{
			get { return ResString.GetMultilingualString("BufferManagement|ProcessHeaderFilterBusinessObject|ApprovedScheduledFinishTime", "Approved Scheduled Finish Time"); }
		}
	}
}
