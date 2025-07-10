using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(ReportScheduleTaskDependentCollection))]
	sealed class ReportScheduleTaskDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			ReportCommand command = Factory.New<ReportCommand>();
			return new ReportScheduleTaskDependentCollection(command);
		}
	}
}
