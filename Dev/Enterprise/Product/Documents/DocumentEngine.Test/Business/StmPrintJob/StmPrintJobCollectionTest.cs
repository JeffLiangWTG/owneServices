using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.DocumentEngine.Scheduler.Business.Testing
{
	[TestedType(typeof(StmPrintJobCollection))]
	sealed class StmPrintJobCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmPrintJobCollection(Factory);
		}
	}
}
