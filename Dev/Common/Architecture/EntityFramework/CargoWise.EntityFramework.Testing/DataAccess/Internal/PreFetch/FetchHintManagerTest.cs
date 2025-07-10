using System;
using System.Data;
using CargoWise.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing.DataAccess
{
	sealed class FetchHintManagerTest : TransactionedTestCase
	{
		public void TestAddFetchHintForField()
		{
			AssertEquals(0, Manager.TableFetchers.Count);

			FetchHint bizOHint = new FetchHint(DummyBizoSchema.PK, ZGuid.Empty);
			Manager.AddFetchHint(bizOHint);
			AssertEquals("Table Count after first fetch", 1, Manager.TableFetchers.Count);
			AssertEquals("PreCachedColumnValuePairs", 1, Manager.TableFetchers[DummyBizoSchema.Constants.TableName].PreCachedFetchHints);

			FetchHint dummyBizOHint1 = new FetchHint(DummyDependentBizoSchema.PK, ZGuid.NewZGuid());
			Manager.AddFetchHint(dummyBizOHint1);
			AssertEquals("Table Count after second fetch", 2, Manager.TableFetchers.Count);

			FetchHint dummyBizOHint2 = new FetchHint(DummyDependentBizoSchema.PK, ZGuid.NewZGuid());
			Manager.AddFetchHint(dummyBizOHint2);
			AssertEquals("Table Count after third fetch", 2, Manager.TableFetchers.Count);
		}

		public void TestFetchTableDecreasesRowFactoryDBHits()
		{
			BusinessObjectFactory boFactory = new BusinessObjectFactory();
			DummyBaseBusinessObject dummy1 = boFactory.New<DummyBaseBusinessObject>();
			DummyBaseBusinessObject dummy2 = boFactory.New<DummyBaseBusinessObject>();
			boFactory.Save();
			FetchHint hint1 = new FetchHint(DummyBizoSchema.PK, dummy1.PK);
			FetchHint hint2 = new FetchHint(DummyBizoSchema.PK, dummy2.PK);
			Factory.AddFetchHint(hint1);
			Factory.AddFetchHint(hint2);

			AssertEquals("Precondition : DatabaseSelectCount", 0, Factory.DatabaseLoadCount);
			AssertNotNull(Factory.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy1.PK));
			AssertEquals("DatabaseSelectCount after first load", 1, Factory.DatabaseLoadCount);
			AssertNotNull(Factory.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy2.PK));
			AssertEquals("DatabaseSelectCount after second load", 1, Factory.DatabaseLoadCount);
		}

		public void TestHasFetchHints()
		{
			AssertEquals("Should not have any Dummy table fetch hints.", false, Manager.HasFetchHints(DummyBizoSchema.Constants.TableName));

			Manager.AddFetchHint(new FetchHint(DummyBizoSchema.PK, ZGuid.Empty));
			AssertEquals("Should not have a 'DodgyTable' table fetch hint.", false, Manager.HasFetchHints("DodgyTable"));
			AssertEquals("Should have Dummy table fetch hints.", true, Manager.HasFetchHints(DummyBizoSchema.Constants.TableName));
		}

		public void TestHasFetchHintsCaseInsensitive()
		{
			AssertEquals("Should not have any Dummy table fetch hints.", false, Manager.HasFetchHints(DummyBizoSchema.Constants.TableName));
			Manager.AddFetchHint(new FetchHint(DummyBizoSchema.PK, ZGuid.Empty));
			AssertEquals("Should have Dummy table fetch hints.", true, Manager.HasFetchHints("DUMMYBIZO"));
		}

		public void TestLoadWithVarcharMax()
		{
			BusinessObjectFactory boFactory = new BusinessObjectFactory();
			DummyBaseBusinessObject dummy1 = boFactory.New<DummyBaseBusinessObject>();
			DummyBaseBusinessObject dummy2 = boFactory.New<DummyBaseBusinessObject>();
			dummy1.Z0_VarCharMax = dummy2.Z0_VarCharMax = new string('z', 1050);
			boFactory.Save();
			FetchHint hint1 = new FetchHint(DummyBizoSchema.PK, dummy1.PK, DummyBizoSchema.Z0_VarCharMax);
			FetchHint hint2 = new FetchHint(DummyBizoSchema.PK, dummy2.PK);
			Factory.AddFetchHint(hint1);
			Factory.AddFetchHint(hint2);

			AssertEquals("Precondition : DatabaseSelectCount", 0, Factory.DatabaseLoadCount);
			DataRow row1 = Factory.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy1.PK);
			AssertEquals("DatabaseSelectCount after first load", 2, Factory.TableSelects[0].Value);
			AssertEquals(false, LazyLoading.LoadRequired(row1[DummyBizoSchema.Z0_VarCharMax.Name]));
			DataRow row2 = Factory.LoadFromPK(DummyBizoSchema.Constants.TableName, dummy2.PK);
			AssertEquals("DatabaseSelectCount after second load", 2, Factory.TableSelects[0].Value);
			AssertEquals(true, LazyLoading.LoadRequired(row2[DummyBizoSchema.Z0_VarCharMax.Name]));
		}

		public void TestLoadWhenFetchTableThrowException_LockTimeoutExpired()
		{
			var factoryCreate = new BusinessObjectFactory();
			var bizO = DummyBusinessObject.New(factoryCreate);
			bizO.Z0_Code = "123";
			factoryCreate.Save();

			var boFactory = new BusinessObjectFactory();
			boFactory.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, bizO.PK));

			AssertEquals("Precondition : DatabaseSelectCount", 0, boFactory.DatabaseLoadCount);

			ZQuery query = new ZQuery(DummyBizoSchema.PK, bizO.PK);
			query.AddToFilter(DummyBizoSchema.Z0_Code, bizO.Z0_Code);

			((IDbConnected)boFactory).Connection.OnBeforeExecute += ThrowLockRequestTimeOutPeriodExceeded;

			AssertExceptionThrown<Exception>(() =>
			{
				boFactory.RowFactory.fetcher.FetchTable(DummyBizoSchema.Constants.TableName);
			});

			((IDbConnected)boFactory).Connection.OnBeforeExecute -= ThrowLockRequestTimeOutPeriodExceeded;

			AssertEquals("DatabaseSelectCount is 0 when FetchTable throw SQL exception", 0, boFactory.DatabaseLoadCount);

			var test = boFactory.Load(typeof(DummyBusinessObject), query);

			AssertEquals("DatabaseSelectCount after load", 1, boFactory.DatabaseLoadCount);
			AssertEquals(1, test.Length);

			void ThrowLockRequestTimeOutPeriodExceeded(DbCommand command)
			{
				if (command.CommandText.Contains("DummyBizo"))
				{
					throw Data.Testing.SqlExceptionBuilder.CreateSqlException(1222, "Lock request time out period exceeded.");
				}
			}
		}

		public void TestLoadWhenFetchAllTablesThrowException_LockTimeoutExpired()
		{
			var factoryCreate = new BusinessObjectFactory();
			var bizO = factoryCreate.New<DummyBusinessObject>();
			bizO.Z0_Code = "123";
			var biz1 = factoryCreate.New<DummyDependantBusinessObject>();
			biz1.ZD1_Code = "456";
			factoryCreate.Save();
			var boFactory = new BusinessObjectFactory();
			boFactory.AddFetchHint(new ImmediateFetchHint(typeof(DummyBusinessObject), DummyBizoSchema.PK, bizO.PK));
			boFactory.AddFetchHint(new ImmediateFetchHint(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.PK, biz1.PK));
			AssertEquals("Precondition : DatabaseLoadCount", 0, boFactory.DatabaseLoadCount);

			((IDbConnected)boFactory).Connection.OnBeforeExecute += ThrowLockRequestTimeOutPeriodExceeded;
			AssertExceptionThrown<Exception>(() =>
			{
				boFactory.RowFactory.fetcher.FetchAllTables();
			});
			((IDbConnected)boFactory).Connection.OnBeforeExecute -= ThrowLockRequestTimeOutPeriodExceeded;
			AssertEquals("DatabaseLoadCount is 0 when FetchTable throw SQL exception", 0, boFactory.DatabaseLoadCount);

			ZQuery query = new ZQuery(DummyBizoSchema.PK, bizO.PK);
			ZQuery query1 = new ZQuery(DummyDependentBizoSchema.PK, biz1.PK);
			var test = boFactory.Load(typeof(DummyBusinessObject), query);
			var test1 = boFactory.Load(typeof(DummyDependantBusinessObject), query1);
			AssertEquals(1, test1.Length);
			AssertEquals(1, test.Length);
			AssertEquals("DatabaseLoadCount after load", 2, boFactory.DatabaseLoadCount);

			void ThrowLockRequestTimeOutPeriodExceeded(DbCommand command)
			{
				if (command.CommandText.Contains(DummyBizoSchema.Constants.TableName))
				{
					throw Data.Testing.SqlExceptionBuilder.CreateSqlException(1222, "Lock request time out period exceeded.");
				}
			}
		}

		#region Implementation

		FetchHintManager Manager
		{
			get
			{
				if (fManager == null)
				{
					fManager = new FetchHintManager(Factory);
				}
				return fManager;
			}
		}

		RowFactory Factory
		{
			get
			{
				if (fFactory == null)
				{
					fFactory = new RowFactory();
				}
				return fFactory;
			}
		}

		FetchHintManager fManager;
		RowFactory fFactory;

		#endregion
	}
}
