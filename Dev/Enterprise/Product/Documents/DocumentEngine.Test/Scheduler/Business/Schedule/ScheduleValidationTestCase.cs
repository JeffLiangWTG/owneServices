using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestsSubclassesOf(typeof(ScheduleValidation))]
	abstract class ScheduleValidationTestCase<T> : BusinessObjectValidationTestCase where T : Schedule, new()
	{
		public void TestValidateAll()
		{
			using (Schedule.SuspendValidationTesting())
			{
				PrepareScheduleForTestValidateAll();
			}
			Schedule.Validation.ValidateAll();
			AssertNoNotifications(Schedule);
		}

		public virtual void TestValidatePeriodCount()
		{
			AssertNoErrors("Precondition: PeriodCount should not have errors.", Schedule.PeriodCountInfo);

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			Schedule.PeriodCount = 0;

			AssertHasError(Schedule.PeriodCountInfo, $"Please enter a 'Period Count' within the range 1 to {Business.Schedule.MaxPeriodCount}.");

			Schedule.PeriodCount = Business.Schedule.MaxPeriodCount + 1;
			AssertHasError(Schedule.PeriodCountInfo, $"Please enter a 'Period Count' within the range 1 to {Business.Schedule.MaxPeriodCount}.");

			Schedule.PeriodCount = 1;
			AssertNoErrors(Schedule.PeriodCountInfo);

			Schedule.PeriodCount = Business.Schedule.MaxPeriodCount;
			AssertNoErrors(Schedule.PeriodCountInfo);

			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			Schedule.PeriodCount = 0;
			AssertNoErrors(Schedule.PeriodCountInfo);

			Schedule.PeriodScope = "!";
			Schedule.Validation.ValidatePeriodCount();
			AssertNoErrors(Schedule.PeriodCountInfo);
		}

		public virtual void TestValidatePeriodScope()
		{
			AssertNoErrors("Precondition: PeriodScope should not have errors.", Schedule.PeriodScopeInfo);

			Schedule.PeriodScope = "";
			AssertHasError(Schedule.PeriodScopeInfo, "Please enter a value.");

			Schedule.PeriodScope = "!";
			AssertHasError(Schedule.PeriodScopeInfo, "Enter a valid selection.");

			Schedule.PeriodScope = PeriodScopeList.Codes.Next;
			AssertNoErrors(Schedule.PeriodScopeInfo);
		}

		protected T Schedule
		{
			get
			{
				if (schedule == null)
				{
					schedule = new T();
				}
				return schedule;
			}
		}

		protected virtual void PrepareScheduleForTestValidateAll()
		{
			Schedule.PeriodScope = PeriodScopeList.Codes.This;
			Schedule.PeriodCountInfo.AddError("x");
			Schedule.PeriodScopeInfo.AddError("x");
		}

		T schedule;
	}
}
