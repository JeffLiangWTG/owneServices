using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(StmALogCollection))]
	sealed class StmALogCollectionTest : BusinessObjectCollectionTestCase
	{
		public void TestCancelAll()
		{
			StmALogCollection collection = new StmALogCollection(Factory);
			StmALog log = collection.AddNew();
			Assert(!log.SL_IsCancelled);
			collection.CancelAll();
			Assert(log.SL_IsCancelled);
		}

		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new StmALogCollection(Factory);
		}
	}
}
