using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Scheduler.Business.Testing
{
	[TestedType(typeof(StmReportRunCollection))]
	sealed class StmReportRunCollectionTest : ActiveBusinessObjectCollectionTestCase<StmReportRunCollection>
	{
		protected override StmReportRunCollection GetCollectionToTest()
		{
			return new StmReportRunCollection(Factory);
		}
	}
}
