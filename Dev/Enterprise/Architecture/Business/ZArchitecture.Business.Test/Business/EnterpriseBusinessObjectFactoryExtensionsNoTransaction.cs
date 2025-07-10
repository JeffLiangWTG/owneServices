using System;
using System.Linq;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[UseSnapshotProtection]
	sealed class EnterpriseBusinessObjectFactoryExtensionsNoTransaction : TestCase
	{
		public void TestInTransactionAction_FactoryIsSavedInOtherFactory()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var obj = factory.New<DummyBusinessObject>();
			bool wasSet = false;
			factory.AddInTransactionAction(() =>
			{
				var isolatedFactory = new BusinessObjectFactory() { RefreshEnabled = false };
				var obj2 = isolatedFactory.New<DummyBusinessObject>();
				isolatedFactory.Save();

				Assert(factory.IsInTransaction);
				wasSet = true;
			});

			factory.Save();
			Assert(wasSet);

			var reload = factory.Load<DummyBusinessObject>(new ZQuery());
			AssertEquals(2, reload?.Length);
		}

		public void TestInTransactionAction_SubFactoryFailureFailsEntireSave()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory() { RefreshEnabled = false };
			var obj = factory.New<DummyBusinessObject>();
			var isolatedFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var obj2 = isolatedFactory.New<DummyBusinessObject>();
			((INeedRow)obj2).Row["Z0_PK"] = obj.PK.ToGuid();
			bool wasSet = false;

			factory.AddInTransactionAction(() =>
			{
				isolatedFactory.Save();
				wasSet = true;
			});

			AssertExceptionThrown<ZSaveException>(() => factory.Save());
			AssertEquals(false, wasSet);

			AssertEquals(0, factory.Load<DummyBusinessObject>(new ZQuery()).Where(item => item.IsInDatabase).Count());
			AssertEquals(0, isolatedFactory.Load<DummyBusinessObject>(new ZQuery()).Where(item => item.IsInDatabase).Count());
			AssertEquals(0, new BusinessObjectFactory() { RefreshEnabled = false }.Load<DummyBusinessObject>(new ZQuery()).Length);
		}

		public void TestInTransactionAction_CircularReferenceDoesNotSaveOrStackOverflow()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var obj = factory.NewWithValidTestData<StmALog>(); // Need StmALog because it has no unique constraint, and we don't want to rely on the unique constraint
			var isolatedFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var obj2 = isolatedFactory.NewWithValidTestData<StmALog>();
			bool wasSet = false;

			factory.AddInTransactionAction(() =>
			{
				isolatedFactory.Save();
				Assert(factory.IsInTransaction);
				wasSet = true;
			});

			isolatedFactory.AddInTransactionAction(() => factory.Save());

			AssertExceptionThrown<InvalidOperationException>(() => factory.Save());
			AssertEquals(false, wasSet);

			AssertEquals(0, factory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Table, obj.SL_Table)).Where(item => item.IsInDatabase).Count());
			AssertEquals(0, isolatedFactory.Load<StmALog>(new ZQuery(StmALogSchema.SL_Table, obj.SL_Table)).Where(item => item.IsInDatabase).Count());
			AssertEquals(0, new BusinessObjectFactory() { RefreshEnabled = false }.Load<StmALog>(new ZQuery(StmALogSchema.SL_Table, obj.SL_Table)).Length);
		}

		public void TestInTransactionAction_NoFalseCircularReferenceDetectionOnSaveFailure()
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			var obj = factory.NewWithValidTestData<StmALog>(); // Need StmALog because it has no unique constraint, and we don't want to rely on the unique constraint
			var isolatedFactory = new BusinessObjectFactory() { RefreshEnabled = false };
			var obj2 = isolatedFactory.NewWithValidTestData<StmALog>();
			bool wasSet = false;

			factory.AddInTransactionAction(() =>
			{
				isolatedFactory.Save();
				Assert(factory.IsInTransaction);
				wasSet = true;
			});

			int count = 0;
			isolatedFactory.Saving += (_) =>
			{
				if (++count < 3)
				{
					var recoverableException = new ZDataException(new TransactionException("", OdysseyDataErrorType.TransactionRolledBack), ((INeedRow)obj2).Row, Db.Connection);
					recoverableException.SetFriendlyMessageForTest("I can recover with on retry");
					throw new ZSaveException(recoverableException, factory);
				}
			};

			AssertNoExceptionThrown(() => ZExceptionReporting.ProcessWithSaveExceptionHandling(() => factory.Save(), () => { }, true, false, 3));
			AssertEquals(true, wasSet);
			AssertEquals(2, new BusinessObjectFactory() { RefreshEnabled = false }.Load<StmALog>(new ZQuery(StmALogSchema.SL_Table, obj.SL_Table)).Length);
		}
	}
}
