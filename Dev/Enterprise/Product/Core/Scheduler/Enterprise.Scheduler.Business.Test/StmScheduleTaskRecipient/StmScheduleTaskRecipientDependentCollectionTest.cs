using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	[TestedType(typeof(StmScheduleTaskRecipientDependentCollection))]
	public class StmScheduleTaskRecipientDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			StmScheduleTask scheduleTask = Factory.NewWithValidTestData<StmScheduleTask>();
			return new StmScheduleTaskRecipientDependentCollection(scheduleTask);
		}
	}
}
