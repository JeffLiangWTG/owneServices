using System;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ApprovedJobScheduledTimeFilter : ApprovedWorkflowScheduledTimeFilter
	{
		public static ApprovedWorkflowScheduledTimeFilter GetScheduledStartTimeFilter(Type businessObjectType)
		{
			return new ApprovedJobScheduledTimeFilter(ViewApprovedWorkflowScheduleSchema.VWS_ScheduledStartTimeUtc, ProcessHeader.ModuleFilterConstants.ApprovedScheduledStartTime, ScheduledStartTimeDescription, businessObjectType);
		}

		public static ApprovedWorkflowScheduledTimeFilter GetScheduledFinishTimeFilter(Type businessObjectType)
		{
			return new ApprovedJobScheduledTimeFilter(ViewApprovedWorkflowScheduleSchema.VWS_ScheduledFinishTimeUtc, ProcessHeader.ModuleFilterConstants.ApprovedScheduledFinishTime, ScheduledFinishTimeDescription, businessObjectType);
		}

		ApprovedJobScheduledTimeFilter(SchemaDateTimeColumn columnOnApprovedScheduleView, ZString description, MultilingualString multilingualDescription, Type businessObjectType)
			: base(columnOnApprovedScheduleView, description, multilingualDescription)
		{
			this.businessObjectType = Argument.NotNull(businessObjectType, "businessObjectType");
		}

		readonly Type businessObjectType;

		protected override ZQuery GetQueryUsingFilterColumns()
		{
			var query = new ZDBOnlyQuery(businessObjectType);
			var workflowSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_ParentId);
			AddViewApprovedWorkflowScheduleSubQuery(workflowSubQuery);
			query.AddSubQuery(workflowSubQuery, JoinCondition.And);

			return query;
		}
	}
}
