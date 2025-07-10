using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.DocumentEngine.Scheduler.Business
{
	public class SchedulableReportCollection : StmMenuItemCollection
	{
		public SchedulableReportCollection(BusinessObjectFactory factory) : base(factory)
		{
		}

		public SchedulableReportCollection(BusinessObjectFactory factory, ZQuery additionalFilter) : base(factory, additionalFilter)
		{
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1161:ResGetString", Justification = "Filter related")]
		protected override ZQuery CreateRelationshipFilter()
		{
			var result = base.CreateRelationshipFilter();
			var reportOnlyFilter = new ZQuery(StmMenuItemSchema.SU_BusinessContext, SQLComparisonOperator.StartsWith, "Rep");
			result.AddToFilter(reportOnlyFilter);

			ZQuery publishedFilter = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, "");
			ZQuery privateFilter = new ZQuery(StmMenuItemSchema.SU_GS_NKStaffCode, GlbStaff.CurrentUser.GS_Code);
			publishedFilter.AddToFilter(privateFilter, JoinCondition.Or);
			result.AddToFilter(publishedFilter, JoinCondition.And);

			return result;
		}
	}
}
