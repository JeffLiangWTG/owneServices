using System;
using Enterprise.DocumentEngine.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Scheduler.Testing
{
	[TestedType(typeof(DateScheduleForm))]
	sealed class DateScheduleFormTest : ScheduleFormTest<DateScheduleForm>
	{
		protected override Schedule GetNewSchedule()
		{
			DateSchedule result = (DateSchedule)Activator.CreateInstance(BusinessEntityType);
			result.ScheduleTask = Factory.NewWithValidTestData<ReportScheduleTask>();
			result.ByWeek = true;
			return result;
		}

		protected override Type BusinessEntityType
		{
			get { return typeof(DateSchedule); }
		}
	}
}
