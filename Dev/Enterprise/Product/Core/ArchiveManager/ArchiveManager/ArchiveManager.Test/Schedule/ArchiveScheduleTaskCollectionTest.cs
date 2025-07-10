using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ArchiveManager.Business.Schedule;
using NUnit.Framework;

namespace Enterprise.ArchiveManager.Test.Schedule
{
	[TestedType(typeof(ArchiveScheduleTaskCollection))]
	class ArchiveScheduleTaskCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObject GetNewElementToAddToTheCollection()
			=> Factory.New<ArchiveScheduleTask>();

		protected override BusinessObjectCollection GetCollectionToTest()
			=> new ArchiveScheduleTaskCollection(Factory);
	}
}
