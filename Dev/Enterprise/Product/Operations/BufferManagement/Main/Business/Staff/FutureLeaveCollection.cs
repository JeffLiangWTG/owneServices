using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class FutureLeaveCollection : ActiveBusinessObjectCollection<GlbStaffHoliday>
	{
		public FutureLeaveCollection(GlbStaff staff, ZQuery filter)
			: base(staff.Factory, staff, filter, GlbStaffHolidaySchema.GA_GS)
		{
		}

		public static ZQuery GetFilterForFutureLeaveWithinXDays(int howNear)
		{
			var query = new ZQuery();

			var startAfterFilter = new ZQuery(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);
			var startBeforeFilter = new ZQuery(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now.AddDays(howNear));
			startAfterFilter.AddToFilter(startBeforeFilter, JoinCondition.And);

			var finishAfterFilter = new ZQuery(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now.AddDays(howNear));
			var finishBeforeFilter = new ZQuery(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now);
			finishAfterFilter.AddToFilter(finishBeforeFilter, JoinCondition.And);

			var leftOuterBoundsFilter = new ZQuery(GlbStaffHolidaySchema.GA_StartTime, SQLComparisonOperator.LessThanOrEqualTo, ZDateTime.Now);
			var rightOuterBoundsFilter = new ZQuery(GlbStaffHolidaySchema.GA_EndTime, SQLComparisonOperator.GreaterThanOrEqualTo, ZDateTime.Now.AddDays(howNear));
			leftOuterBoundsFilter.AddToFilter(rightOuterBoundsFilter, JoinCondition.And);

			query.AddToFilter(startAfterFilter, JoinCondition.Or);
			query.AddToFilter(finishAfterFilter, JoinCondition.Or);
			query.AddToFilter(leftOuterBoundsFilter, JoinCondition.Or);

			return query;
		}
	}
}
