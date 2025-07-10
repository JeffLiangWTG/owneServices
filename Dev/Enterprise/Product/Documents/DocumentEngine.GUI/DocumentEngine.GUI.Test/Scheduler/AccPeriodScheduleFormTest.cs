using System;
using Enterprise.DocumentEngine.Scheduler.Business;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.GUI.Scheduler.Testing
{
	[TestedType(typeof(AccPeriodScheduleForm))]
	sealed class AccPeriodScheduleFormTest : ScheduleFormTest<AccPeriodScheduleForm>
	{
		protected override Type BusinessEntityType
		{
			get { return typeof(AccPeriodSchedule); }
		}
	}
}
