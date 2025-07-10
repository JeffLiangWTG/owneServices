using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.BufferManagement.Business;
using Enterprise.BufferManagement.NetworkVisualisation.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Module
{
	public class ApprovedWorkflowScheduleTypeFilter : ApprovedScheduleTypeFilter
	{
		public ApprovedWorkflowScheduleTypeFilter()
			: base(GetQuery)
		{
		}

		static ZQuery GetQuery(ZString value)
		{
			var query = new ZDBOnlyQuery(typeof(ProcessHeader));
			AddViewApprovedWorkflowScheduleSubQuery(query, value);

			return query;
		}
	}

	public class ApprovedJobScheduleTypeFilter : ApprovedScheduleTypeFilter
	{
		public ApprovedJobScheduleTypeFilter(Type bizoType)
			: base(v => GetQuery(v, bizoType))
		{
		}

		static ZQuery GetQuery(ZString value, Type bizoType)
		{
			var query = new ZDBOnlyQuery(bizoType);
			var workflowSubQuery = new ZDBOnlySubQuery(typeof(ProcessHeader), ProcessHeaderSchema.FH_ParentId);
			AddViewApprovedWorkflowScheduleSubQuery(workflowSubQuery, value);
			query.AddSubQuery(workflowSubQuery, JoinCondition.And);

			return query;
		}
	}

	public abstract class ApprovedScheduleTypeFilter : ModuleTextFilter
	{
		protected ApprovedScheduleTypeFilter(GetTextQuery queryGetter)
			: base(ProcessHeader.ModuleFilterConstants.ApprovedScheduleType, queryGetter, () => new ApprovedDiagramTypeList())
		{
			MultilingualDescription = ResString.GetMultilingualString("c56eec49-0ff7-4d4f-a902-69d10f0d2690", "Schedule Type");
		}

		protected static void AddViewApprovedWorkflowScheduleSubQuery(ZDBOnlyQuery parentQuery, ZString value)
		{
			var notIn = string.Equals(value, ApprovedDiagramTypeList.Codes.NonApprovedDiagram, StringComparison.OrdinalIgnoreCase);
			var subQuery = new ZDBOnlySubQuery(typeof(ViewApprovedWorkflowSchedule), ViewApprovedWorkflowScheduleSchema.PK, notIn);

			if (!notIn)
			{
				subQuery.AddToFilter(ViewApprovedWorkflowScheduleSchema.VWS_ApprovedScheduleType, value);
			}

			parentQuery.AddSubQuery(ProcessHeaderSchema.PK, subQuery, JoinCondition.And);
		}
	}
}
