using System;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestsSubclassesOf(typeof(ScheduleLookups))]
	abstract class ScheduleLookupsTestCase<T> : BusinessObjectLookupsTestCase where T : Schedule, new()
	{
		public void TestPeriodScopes()
		{
			AssertEquals("PeriodScopes.GetType()", typeof(PeriodScopeList), Schedule.Lookups.PeriodScopes.GetType());
		}

		protected T Schedule
		{
			get
			{
				if (schedule == null)
				{
					schedule = Activator.CreateInstance<T>();
				}
				return schedule;
			}
		}

		T schedule;
	}
}
