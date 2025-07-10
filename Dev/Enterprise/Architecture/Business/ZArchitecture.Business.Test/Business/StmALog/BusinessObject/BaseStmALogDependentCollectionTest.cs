using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(BaseStmALogDependentCollection<BaseStmALog>))]
	sealed class BaseStmALogDependentCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			return new BaseStmALogDependentCollection<BaseStmALog>(Factory.New<DummyEnterpriseBusinessObject>());
		}

		public void TestAddingLogSetsSL_Table()
		{
			var log = GetNewAdminLog();
			AssertEquals("SL_Table should be empty", "", log.SL_Table);

			Dummy.Logs.Add(log);
			AssertEquals("SL_Table should be set to Dummy.TableName", Dummy.TableName, log.SL_Table);
		}

		public void TestCancelAll()
		{
			var dummyBusinessObject = Factory.New<DummyEnterpriseBusinessObject>();
			var collection = new BaseStmALogDependentCollection<BaseStmALog>(dummyBusinessObject);
			var log = collection.AddNew();
			Assert(!log.SL_IsCancelled);
			collection.CancelAll();
			Assert(log.SL_IsCancelled);
		}

		public void TestMasterIsSetOnCreateAndLoad()
		{
			var dummyBusinessObject = Factory.New<DummyEnterpriseBusinessObject>();
			var collection = new BaseStmALogDependentCollection<BaseStmALog>(dummyBusinessObject);
			var log = collection.AddNew();
			AssertEquals("Master after create", dummyBusinessObject, log.Master);

			Factory.Save();
			var factory2 = new BusinessObjectFactory();
			var dummyBusinessObject2 = factory2.Load<DummyEnterpriseBusinessObject>(dummyBusinessObject.PK);
			var collection2 = new BaseStmALogDependentCollection<BaseStmALog>(dummyBusinessObject2);
			collection2.Load();
			AssertEquals("Master after load", dummyBusinessObject2, collection2[0].Master);
		}

		public void TestCollectionIsSortedByPostedTimeDesc()
		{
			var collection = new BaseStmALogDependentCollection<BaseStmALog>(Dummy);
			Factory.Save();

			CreateLogInDB(Dummy);
			System.Threading.Thread.Sleep(1000);
			CreateLogInDB(Dummy);
			System.Threading.Thread.Sleep(1000);
			CreateLogInDB(Dummy);

			Factory.Save();
			collection.Load();

			Assert("Precondition : Times must be different", collection[0].SL_PostedTimeUtc != collection[1].SL_PostedTimeUtc);
			Assert("Precondition : Times must be different", collection[1].SL_PostedTimeUtc != collection[2].SL_PostedTimeUtc);

			Assert(collection[0].SL_PostedTimeUtc > collection[1].SL_PostedTimeUtc);
			Assert(collection[1].SL_PostedTimeUtc > collection[2].SL_PostedTimeUtc);
		}

		#region Implementation

		void CreateLogInDB(DummyEnterpriseBusinessObject parent)
		{
			var savingFactory = new BusinessObjectFactory();
			var logger = savingFactory.Load<DummyEnterpriseBusinessObject>(parent.PK);
			logger.Logs.AddNew(Events.CustomisableEvent00);
			savingFactory.Save();
		}

		StmALog GetNewAdminLog()
		{
			var log = Factory.New<StmALog>();
			using (log.LockForUpdatingKeyFieldsForTesting())
			{
				log.SL_SE_NKEvent = Events.Login.Code;
			}
			return log;
		}

		DummyEnterpriseBusinessObject Dummy;
		protected override void SetUp()
		{
			Dummy = Factory.New<DummyEnterpriseBusinessObject>();
			base.SetUp();
		}

		#endregion
	}
}
