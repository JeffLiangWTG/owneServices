using System.Threading;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class CurrentThreadBusinessObjectLoaderTest : NonTransactionedTestCase
	{
		public void TestShouldReturnBizO_WhenBizOIsCreatedOnCurrentThread()
		{
			var loader = new CurrentThreadBusinessObjectLoader();
			var factory = new BusinessObjectFactory();
			var dummy = factory.New<DummyBusinessObject>();

			var loaded = loader.GetOnCurrentThread(dummy);
			AssertEquals(dummy, loaded);
		}

		public void TestShouldReloadBizO_WhenBizOIsCreatedOnAnotherThread()
		{
			var loader = new CurrentThreadBusinessObjectLoader();

			BusinessObject dummy = null;

			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory = new BusinessObjectFactory();
					dummy = factory.New<DummyBusinessObject>();
					factory.Save();
				}
			});
			thread.Start();
			thread.Join();

			var loaded = loader.GetOnCurrentThread(dummy);
			AssertNotNull(loaded);
			AssertNotEquals(dummy, loaded);
			AssertEquals(dummy.PK, loaded.PK);
		}

		public void TestShouldReturnNull_WhenBizOIsCreatedOnAnotherThread_AndNotInTheDatabase()
		{
			var loader = new CurrentThreadBusinessObjectLoader();

			BusinessObject dummy = null;

			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factory = new BusinessObjectFactory();
					dummy = factory.New<DummyBusinessObject>();
				}
			});
			thread.Start();
			thread.Join();

			var loaded = loader.GetOnCurrentThread(dummy);
			AssertNull(loaded);
		}
	}
}
