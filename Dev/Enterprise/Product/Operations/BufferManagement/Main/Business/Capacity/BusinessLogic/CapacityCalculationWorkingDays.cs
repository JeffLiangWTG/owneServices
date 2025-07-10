using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	static class CapacityCalculationWorkTimeArithmetic
	{
		public static IWorkTimeArithmetic GetInstance(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK = default(ZGuid))
		{
			ICalendarDataSource dataSource = new CapacityCalculationCalendarDataSource(factory, departmentPK, branchPK, staffPK);
			dataSource = new CachedCalendarDataSource(dataSource);
			var creator = new Func<WorkTimeArithmetic>(() => new WorkTimeArithmetic(dataSource));
			return WorkingDays.GetInstance(factory, departmentPK, branchPK, staffPK, creator);
		}

		class CapacityCalculationCalendarDataSource : CalendarArithmeticDataSource
		{
			public CapacityCalculationCalendarDataSource(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK = default(ZGuid))
				: base(factory, departmentPK, branchPK, staffPK)
			{
			}

			public override IEnumerable<DateTimeRange> GetStaffHolidaysFromRange(DateTimeRange range)
			{
				if (!staffPK.IsValid)
				{
					return Enumerable.Empty<DateTimeRange>();
				}

				var query = GetStaffHolidaysInRangeQuery(range, checkAvailabilityPercentage: false);

				return factory.Load<GlbStaffHoliday>(query)
					.Select(
					x => x.GA_WorkHolidayType == BMConstants.BMSLeaveType && x.GA_RecordType == BMConstants.BMSLeaveType
						? new DateTimeRange(x.GA_StartTime.ToDateTime(), x.GA_EndTime.ToDateTime(), x.GA_AvailabilityPercentage)
						: new DateTimeRange(x.GA_StartTime.ToDateTime(), x.GA_EndTime.ToDateTime()));
			}
		}
	}
}
