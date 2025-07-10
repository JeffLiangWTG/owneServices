using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.CalendarArithmetic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Integration;

namespace Enterprise.ZArchitecture.GUI
{
	class WorkingDaysHelper
	{
		public static DateTime GetRelativeTime(int numberOfDaysToAdd, TimeOfDay timeOfDay, DataSourceOption dataSourceOption)
		{
			var factory = EnvironmentFactory ?? new ReadOnlyBusinessObjectFactory { NameForDebugging = nameof(WorkingDaysHelper) };
			var workTimeArithmetic = (dataSourceOption == DataSourceOption.Staff)
				? GetWorktimeInstance(factory, Env.CurrentDepartmentPK, Env.CurrentBranchPK, Env.CurrentUserPK)
				: GetWorktimeInstance(factory, Env.CurrentDepartmentPK, Env.CurrentBranchPK);

			DateTime relativeWorkDay;
			var now = ZDateTime.Now.ToDateTime();

			if (numberOfDaysToAdd == 0)
			{
				if (dataSourceOption == DataSourceOption.Staff && !workTimeArithmetic.IsWorkDay(now.Date, checkStaffHolidays: false, checkBranchHolidays: false))
				{
					workTimeArithmetic = GetWorktimeInstance(factory, Env.CurrentDepartmentPK, Env.CurrentBranchPK);
				}

				relativeWorkDay = now.Date;
			}
			else
			{
				relativeWorkDay = workTimeArithmetic.GetAnotherStandardWorkingDay(now.Date, numberOfDaysToAdd, checkStaffHolidays: false, checkBranchHolidays: true);
			}

			var workingTimes = workTimeArithmetic.GetWorkingRangesForDate(relativeWorkDay, checkBranchHolidays: false);

			if (workingTimes.Any())
			{
				return timeOfDay == TimeOfDay.Start
					? workingTimes.First().Start
					: workingTimes.Last().End;
			}
			else
			{
				return now;
			}
		}

		static IWorkTimeArithmetic GetWorktimeInstance(BusinessObjectFactory factory, ZGuid departmentPK, ZGuid branchPK, ZGuid staffPK = default)
		{
			return ObjectFactory.Get<IWorkingDaysProvider>().GetWorkingDays(factory, departmentPK, branchPK, staffPK);
		}

		static BusinessObjectFactory EnvironmentFactory => (Env.CurrentUser as BusinessObject)?.Factory;

		public enum TimeOfDay
		{
			Start, End
		}

		public enum DataSourceOption
		{
			Staff, Department
		}
	}
}
