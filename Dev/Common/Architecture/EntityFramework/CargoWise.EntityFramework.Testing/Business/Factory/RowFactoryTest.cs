using System;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Threading;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class RowFactoryTest : TestCaseWithFactory
	{
		public void TestLoadFromDataTableIfPossible_WhenFilterHasReLoadExistingRowsTrue()
		{
			var dummy1 = Factory.New<DummyBusinessObject>();
			dummy1.Z0_Code = "DM1";
			dummy1.Z0_Description = "Desc1";

			var dummy2 = Factory.New<DummyBusinessObject>();
			dummy2.Z0_Code = "DM2";
			dummy2.Z0_Description = "Desc2";
			Factory.Save();

			dummy1.Z0_Description = "Desc1Updated";
			dummy2.Z0_Description = "Desc2Updated";

			var filter = new ZQuery(DummyBizoSchema.PK, dummy1.PK)
			{
				MaximumRows = 2
			};
			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.PK, dummy2.PK);

			var rows = RowFactory.Load(DummyBizoSchema.Constants.TableName, filter);

			AssertEquals(2, rows.Length);

			filter.ReLoadExistingRows = true;
			RowFactory.AddFetchHint(new FetchHint(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid()));
			rows = RowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			var orderedRows = rows.OrderBy(r => r[DummyBizoSchema.Constants.Z0_Code]).ToArray();

			AssertEquals(2, orderedRows.Length);
			AssertEquals(dummy1.PK.ToGuid(), orderedRows[0][DummyBizoSchema.Constants.PK]);
			AssertEquals("Desc1", orderedRows[0][DummyBizoSchema.Constants.Z0_Description]);
			AssertEquals(dummy2.PK.ToGuid(), orderedRows[1][DummyBizoSchema.Constants.PK]);
			AssertEquals("Desc2", orderedRows[1][DummyBizoSchema.Constants.Z0_Description]);
		}

		public void TestUberFactoryRememberToLock_MultithreadedNRE()
		{
			var thread_amount = 100;
			var threads = new Thread[thread_amount];
			for (var j = 0; j < thread_amount; ++j)
			{
				threads[j] = new Thread(() =>
				{
					for (var i = 0; i < 100000; ++i)
					{
						RowFactory.ResetCacheAfterDbUpgrade();
					}
				});
			}
			for (var j = 0; j < thread_amount; ++j)
			{
				threads[j].Start();
			}
			try
			{
				for (var i = 0; i < 100000; ++i)
				{
					AssertNotNull(RowFactory.UberFactoryRememberToLock);
				}
			}
			finally
			{
				for (var j = 0; j < thread_amount; ++j)
				{
					threads[j].Join();
				}
			}
		}

		public void TestClearSpecificTableFromUberFactory()
		{
			using (RowFactory.SetCachedTables(DummyBizoSchema.Constants.TableName))
			{
				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Code = "BOB";
				Factory.Save();
				var factory2 = new BusinessObjectFactory();
				AssertNotNull(factory2.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, dummy.Z0_Code)));

				foreach (var uber in PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Where(s => s.IsOwnedByCurrentThread && s.RowFactory.IsUberFactory))
				{
					uber.DeactivateActiveCollectionsAndCaches();
				}

				RowFactory.ClearSpecificTableFromUberFactory(DummyBizoSchema.Constants.TableName);

				var factory3 = new BusinessObjectFactory();
				AssertNotNull(factory3.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, dummy.Z0_Code)));
			}
		}

		public void TestClearTableFromUberFactory_WithThePowerOfUpdates()
		{
			using (RowFactory.SetCachedTables(DummyBizoSchema.Constants.TableName))
			{
				var dummy = Factory.New<DummyBusinessObject>();
				dummy.Z0_Code = "BOB";
				Factory.Save();
				var factory2 = new BusinessObjectFactory();
				AssertNotNull(factory2.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, dummy.Z0_Code)));

				foreach (var uber in PersistentFactoryCacheManager.Instance.GetBusinessObjectFactories().Where(s => s.RowFactory.IsUberFactory))
				{
					uber.DeactivateActiveCollectionsAndCaches();
				}

				dummy.Z0_Date = ZDate.Today;
				Factory.Save();

				var factory3 = new BusinessObjectFactory();
				AssertNotNull(factory3.LoadTop1<DummyBusinessObject>(new ZQuery(DummyBizoSchema.Z0_Code, dummy.Z0_Code)));
			}
		}

		public void TestEachThreadGetsADifferentConnection()
		{
			object instance1 = null, instance2 = null, instance3 = null;

			RowFactory factory = new RowFactory();
			instance1 = factory.DbConnection;

			AssertEquals(instance1, Db.Connection);

			ThreadStart threadstart2 = new ThreadStart(delegate
			{
				using (Db.DisposableActionForDbConnection())
				{
					instance2 = factory.DbConnection;
					AssertEquals(instance2, Db.Connection);
				}
			});

			ThreadStart threadstart3 = new ThreadStart(delegate
			{
				using (Db.DisposableActionForDbConnection())
				{
					instance3 = factory.DbConnection;
					AssertEquals(instance3, Db.Connection);
				}
			});

			Thread thread2 = new Thread(threadstart2);
			Thread thread3 = new Thread(threadstart3);

			thread2.Start();
			thread3.Start();

			thread2.Join(1000);
			thread3.Join(1000);

			AssertNotNull(instance1);
			AssertNotNull(instance2);
			AssertNotNull(instance3);
			AssertNotEquals("Each thread should get it's own instance", instance1, instance2);
			AssertNotEquals("Each thread should get it's own instance", instance2, instance3);
			AssertNotEquals("Each thread should get it's own instance", instance1, instance3);
		}

		public void TestEachThreadGetsADifferentConnectionIfMainConnectionAssigned()
		{
			object instance1 = null, instance2 = null, instance3 = null;

			DbConnection connection = Db.Connection;
			RowFactory factory = new RowFactory(connection, Db.DatabaseName);
			instance1 = factory.DbConnection;

			AssertEquals(instance1, Db.Connection);

			ThreadStart threadstart2 = new ThreadStart(delegate
			{
				using (Db.DisposableActionForDbConnection())
				{
					instance2 = factory.DbConnection;
					AssertEquals(instance2, Db.Connection);
				}
			});

			ThreadStart threadstart3 = new ThreadStart(delegate
			{
				using (Db.DisposableActionForDbConnection())
				{
					instance3 = factory.DbConnection;
					AssertEquals(instance3, Db.Connection);
				}
			});

			Thread thread2 = new Thread(threadstart2);
			Thread thread3 = new Thread(threadstart3);

			thread2.Start();
			thread3.Start();

			thread2.Join(1000);
			thread3.Join(1000);

			AssertNotNull(instance1);
			AssertNotNull(instance2);
			AssertNotNull(instance3);
			AssertNotEquals("Each thread should get it's own instance", instance1, instance2);
			AssertNotEquals("Each thread should get it's own instance", instance2, instance3);
			AssertNotEquals("Each thread should get it's own instance", instance1, instance3);
		}

		public void TestEachThreadGetsTheSameConnectionIfExtraConnectionAssigned()
		{
			object instance1 = null, instance2 = null, instance3 = null;

			using (DbConnection connection = Db.NewExtraConnectionToMainDb())
			{
				RowFactory factory = new RowFactory(connection, Db.DatabaseName);
				instance1 = factory.DbConnection;

				ThreadStart threadstart2 = new ThreadStart(delegate
				{ instance2 = factory.DbConnection; });
				ThreadStart threadstart3 = new ThreadStart(delegate
				{ instance3 = factory.DbConnection; });

				Thread thread2 = new Thread(threadstart2);
				Thread thread3 = new Thread(threadstart3);

				thread2.Start();
				thread3.Start();

				thread2.Join(1000);
				thread3.Join(1000);

				AssertEquals("Each thread should get the same instance", connection, instance1);
				AssertEquals("Each thread should get the same instance", connection, instance2);
				AssertEquals("Each thread should get the same instance", connection, instance3);
			}
		}

		public void TestSelect()
		{
			DummyBusinessObject dummy = Factory.New<DummyBusinessObject>();
			dummy.Z0_Code = "Code";
			Factory.Save();

			RowFactory rowFactory = new RowFactory();
			ZQuery query = new ZQuery(DummyBizoSchema.Z0_Code, "Code");
			rowFactory.Load(DummyBizoSchema.Constants.TableName, query);
			DataRow[] results = rowFactory.Select(DummyBizoSchema.Constants.TableName, query);
			AssertEquals(1, results.Length);

			query.IsNoResultQuery = true;
			results = rowFactory.Select(DummyBizoSchema.Constants.TableName, query);
			AssertEquals(0, results.Length);
		}

		public void TestBooleanSelect()
		{
			DataRow row1 = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row1);
			row1["Z0_Bool"] = true;
			row1.Table.Rows.Add(row1);
			DataRow row2 = RowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row2);
			row2["Z0_Bool"] = false;
			row2.Table.Rows.Add(row2);
			DataRow[] rows = RowFactory.Select(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Bool, true));
			AssertEquals(row1, rows[0]);
			rows = RowFactory.Select(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Bool, false));
			AssertEquals(row2, rows[0]);
			rows = RowFactory.Select(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.NotEqual, false));
			AssertEquals(row1, rows[0]);
			rows = RowFactory.Select(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Bool, SQLComparisonOperator.NotEqual, true));
			AssertEquals(row2, rows[0]);
		}

		public void TestBooleanSelect_In()
		{
			var factory = new BusinessObjectFactory();
			var dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Bool = true;
			var dummy2 = factory.NewWithValidTestData<DummyBusinessObject>();
			dummy2.Z0_Bool = false;

			var query = new ZQuery(DummyBizoSchema.Z0_Bool, new object[] { ZBool.False, true });

			AssertContainsExactElementsInAnyOrder(new[] { dummy, dummy2 }, factory.Load<DummyBusinessObject>(query));
		}

		public void TestNew()
		{
			RowFactory factory = new RowFactory();

			DataRow row = factory.New(DummyBizoSchema.Constants.TableName);

			Assert(row.Table.DataSet.Tables[DummyBizoSchema.Constants.TableName] != null);
			Assert(row != null);
			Assert(row.RowState == DataRowState.Detached);
			Assert(row.Table.Columns.Count > 0);
			Assert(row.Table.Rows.Count == 0); // row has NOT been added to table yet, since it does not meet table constraints
		}

		public void TestDBOnlyQueryCached()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.NewZGuid());
			RowFactory factory = new RowFactory();
			int initialCount = factory.Load("DummyBizO", query).Length;
			string sQL = "insert into dbo.dummybizo (Z0_pk) values (newid())";
			Db.Connection.ExecuteNonQuery(sQL);
			AssertEquals("Count after forced insert into db", initialCount, factory.Load("DummyBizO", query).Length);
			factory.ClearQueryCache();
			AssertEquals("Count after query cache clear", initialCount + 1, factory.Load("DummyBizO", query).Length);
		}

		public void TestCachedPKQueryDoesNotHitDB()
		{
			Guid dummyPK = Guid.NewGuid();
			string description = "~Desc~";
			CreateDummyInDB(dummyPK, description);

			RowFactory factory = new RowFactory();
			factory.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Description, description)); // Seed RowFactory with job
			AssertEquals("Precondition", 1, factory.DatabaseLoadCount);

			ZQuery pkFilter = new ZQuery(DummyBizoSchema.PK, dummyPK);
			factory.Load(DummyBizoSchema.Constants.TableName, pkFilter);
			AssertEquals("DB Hit after subset query", 1, factory.DatabaseLoadCount);
		}

		public void TestSubsetQueryDoesNotHitDB()
		{
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.Z0_Code, "ABC");
			RowFactory factory = new RowFactory();
			AssertEquals(0, factory.DatabaseLoadCount);
			factory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("DB Hit after query", 1, factory.DatabaseLoadCount);
			filter.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Decimal, SQLComparisonOperator.Equal, new ZDecimal(3.2));
			AssertEquals("DB Hit after subset query", 1, factory.DatabaseLoadCount);
		}

		public void TestDBOnlyQueryReLoadExistingRows()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.NewZGuid());
			RowFactory factory = new RowFactory();
			int initialCount = factory.Load("DummyBizO", query).Length;
			string sQL = "insert into dbo.dummybizo (Z0_pk) values (newid())";
			Db.Connection.ExecuteNonQuery(sQL);
			AssertEquals("Precondition: Count without Reload", initialCount, factory.Load("DummyBizO", query).Length);
			query.ReLoadExistingRows = true;
			AssertEquals("Precondition: Count with Reload", initialCount + 1, factory.Load("DummyBizO", query).Length);
		}

		public void TestDBOnlyQueryCacheOrderBy()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.NewZGuid());
			RowFactory factory = new RowFactory();
			int initialCount = factory.Load("DummyBizO", query).Length;
			string sQL = "insert into dbo.dummybizo (Z0_pk) values (newid())";
			Db.Connection.ExecuteNonQuery(sQL);
			AssertEquals("Precondition: Count without orderby", initialCount, factory.Load("DummyBizO", query).Length);
			query.OrderBy = "Z0_PK";
			AssertEquals("Precondition: Count with Reload", initialCount + 1, factory.Load("DummyBizO", query).Length);
		}

		public void TestDBOnlyQueryCacheMaximumRows()
		{
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query.AddToFilter(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.NewZGuid());
			RowFactory factory = new RowFactory();
			int initialCount = factory.Load("DummyBizO", query).Length;
			string sQL = "insert into dbo.dummybizo (Z0_pk) values (newid())";
			Db.Connection.ExecuteNonQuery(sQL);
			AssertEquals("Precondition: Count without orderby", initialCount, factory.Load("DummyBizO", query).Length);
			query.MaximumRows = 123;
			AssertEquals("Precondition: Count with Reload", initialCount + 1, factory.Load("DummyBizO", query).Length);
		}

		public void TestSeedQueryCache()
		{
			RowFactory factory = new RowFactory();
			ZGuid guid1 = ZGuid.NewZGuid();
			ZGuid guid2 = ZGuid.NewZGuid();
			factory.SeedQueryCache(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Guid, new ZGuid[] { guid1, guid2 }));
			AssertEquals(0, factory.DatabaseLoadCount);
			factory.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Guid, guid1));
			AssertEquals(0, factory.DatabaseLoadCount);
			factory.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Guid, guid2));
			AssertEquals(0, factory.DatabaseLoadCount);
			factory.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Guid, ZGuid.NewZGuid()));
			AssertEquals(1, factory.DatabaseLoadCount);
		}

		public void TestFetchOnlyFromLocalCache()
		{
			RowFactory factory = new RowFactory();

			Guid dummyPK = Guid.NewGuid();
			CreateDummyInDB(dummyPK, "~Desc~");

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.PK, dummyPK);
			filter.FetchOnlyFromLocalCache = true;
			DataRow[] row = factory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("Row length with FetchOnlyFromLocalCache = true", 0, row.Length);
			filter.FetchOnlyFromLocalCache = false;
			DataRow[] row2 = factory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("Row length with FetchOnlyFromLocalCache = false", 1, row2.Length);
		}

		public void TestPrimaryKeyHintIsNotEnteredInQueryCacheWhenBusinessObjectDoesExistForPK()
		{
			ZGuid zGuid = Factory.New<DummyBusinessObject>().PK;

			ZQuery queryFK = new ZQuery(DummyBizoSchema.Z0_Guid, zGuid);
			Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, zGuid);
			Factory.ExecuteAllFetchHints();
			AssertEquals("Precondition", true, Factory.RowFactory.QueryCache.IsCached(DummyBizoSchema.Constants.TableName, queryFK));

			ZQuery queryPK = new ZQuery(DummyBizoSchema.PK, zGuid);
			Factory.AddFetchHint(DummyBizoSchema.PK, zGuid);
			Factory.ExecuteAllFetchHints();
			AssertEquals(false, Factory.RowFactory.QueryCache.IsCached(DummyBizoSchema.Constants.TableName, queryPK));
		}

		public void TestPrimaryKeyHintIsEnteredInQueryCacheWhenBusinessObjectDoesNotExistForPK()
		{
			ZGuid zGuid = ZGuid.NewZGuid();

			ZQuery queryFK = new ZQuery(DummyBizoSchema.Z0_Guid, zGuid);
			Factory.AddFetchHint(DummyBizoSchema.Z0_Guid, zGuid);
			Factory.ExecuteAllFetchHints();
			AssertEquals("Precondition", true, Factory.RowFactory.QueryCache.IsCached(DummyBizoSchema.Constants.TableName, queryFK));

			ZQuery queryPK = new ZQuery(DummyBizoSchema.PK, zGuid);
			Factory.AddFetchHint(DummyBizoSchema.PK, zGuid);
			Factory.ExecuteAllFetchHints();
			AssertEquals(true, Factory.RowFactory.QueryCache.IsCached(DummyBizoSchema.Constants.TableName, queryPK));
		}

		[ExpectNoExceptions]
		public void TestDBOnlyQueryWithChangedDataDoesNotError()
		{
			RowFactory rowFactory = new RowFactory();
			DataRow row = rowFactory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row);
			row[DummyBizoSchema.Z0_Code.Name] = "ABC";
			row.Table.Rows.Add(row); // this is done by bizO factory, once defaults are set

			rowFactory.Save();
			ZDBOnlyQuery query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			ZDBOnlySubQuery subquery = new ZDBOnlySubQuery(typeof(DummyDependantBusinessObject), DummyDependentBizoSchema.ZD1_Z0);
			query.AddSubQuery(subquery, JoinCondition.And);

			row[DummyBizoSchema.Z0_Code.Name] = "XYZ";
			row[DummyBizoSchema.Z0_Code.Name] = "ABC";  // Leaves rowstate changed

			rowFactory.Load(DummyBizoSchema.Constants.TableName, query);
		}
		public void TestLoadWithPK()
		{
			RowFactory factory = new RowFactory();

			DataRow row = factory.LoadFromPK(DummyBizoSchema.Constants.TableName, Guid.Empty);
			Assert(row == null);

			Guid dummyPK = Guid.NewGuid();
			CreateDummyInDB(dummyPK, "~~~");

			row = factory.LoadFromPK(DummyBizoSchema.Constants.TableName, dummyPK);
			Assert(row != null);
			Assert(!row.Table.DataSet.HasChanges());
			Assert(row.RowState == DataRowState.Unchanged);
			Assert(row.Table.Columns.Count > 0);
			Assert(row.Table.Rows.Count == 1); // row is not added to table as it meets DB constraints
			AssertEquals("~~~", row[DummyBizoSchema.Constants.Z0_Description]);

			string sQL = @"UPDATE dbo.JobShipment SET JS_TransportMode=@desc WHERE JS_PK=@pk";
			Db.Connection.ExecuteNonQuery(sQL, cmd =>
			{
				cmd.AddParameterBasedOnDbColumn("@desc", "$$$", DummyBizoSchema.Z0_Description);
				cmd.AddParameterBasedOnDbColumn("@pk", dummyPK, DummyBizoSchema.PK);
			});

			row = factory.LoadFromPK(DummyBizoSchema.Constants.TableName, dummyPK);
			AssertEquals("~~~", row[DummyBizoSchema.Constants.Z0_Description]);
		}

		[ExpectException(typeof(ApplicationException))]
		public void TestLoadFromNaturalKeyFailsOnColumnWithoutUniqueIndex()
		{
			RowFactory factory = new RowFactory();
			factory.LoadFromNaturalKey("DummyBizo", DummyBizoSchema.Z0_VarCharMax, new ZString("Any value"), false);
		}

		public void TestLoadWithConsistentViewOfDataSetAndDatabase()
		{
			RowFactory factory = new RowFactory();
			DataRow row = factory.New(DummyBusinessObject.Schema.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row);
			row.Table.Rows.Add(row); // this is done by bizO factory, once defaults are set

			Assert("Should load non-persistent dummy", factory.LoadFromPK(DummyBusinessObject.Schema.TableName, (Guid)row["Z0_PK"]) != null);
		}

		public void TestReloadWithPK()
		{
			RowFactory factory = new RowFactory();

			Guid dummyPK = Guid.NewGuid();
			CreateDummyInDB(dummyPK, "~~~");

			DataRow row = factory.LoadFromPK(DummyBizoSchema.Constants.TableName, dummyPK);
			AssertEquals("~~~", row[DummyBizoSchema.Constants.Z0_Code]);

			string sQL = @"UPDATE dbo.DummyBizo SET Z0_Description=@desc WHERE Z0_PK=@pk";
			Db.Connection.ExecuteNonQuery(sQL, cmd =>
			{
				cmd.AddParameterBasedOnDbColumn("@desc", "$$$", DummyBizoSchema.Z0_Description);
				cmd.AddParameterBasedOnDbColumn("@pk", dummyPK, DummyBizoSchema.PK);
			});

			int firstCount = factory.DatabaseLoadCount;
			row = factory.Reload(DummyBizoSchema.Constants.TableName, dummyPK);
			row = factory.Reload(DummyBizoSchema.Constants.TableName, dummyPK);
			AssertEquals("$$$", row[DummyBizoSchema.Constants.Z0_Description]);
			AssertEquals(firstCount + 2, factory.DatabaseLoadCount);
		}

		public void TestLoadFromNaturalKeyWithNotUniqueInTable()
		{
			RowFactory factory = new RowFactory();

			CreateDummyInDB(Guid.NewGuid(), "ABC", "12345");
			CreateDummyInDB(Guid.NewGuid(), "ABC", "23456");
			CreateDummyInDB(Guid.NewGuid(), "CED", "34567");

			AssertNoExceptionThrown("Should not make exception", () => factory.LoadFromNaturalKey("DummyBizo", DummyBizoSchema.Z0_Code, new ZString("ABC"), false));
			AssertEquals("Z0_Code 'ABC' is not unique in table 'DummyBizo'!Rows count : 2  First row returned. Same PK: False.", CargoWise.Common.ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestLoadWithWhereClause()
		{
			RowFactory factory = new RowFactory();

			CreateDummyInDB(Guid.NewGuid(), "~~~", "~Comrade~");
			CreateDummyInDB(Guid.NewGuid(), "~~~", "~Noodle~");
			CreateDummyInDB(Guid.NewGuid(), "$$$", "~Noodle~");

			DataRow[] rows = factory.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "~~~"));
			AssertEquals("RowCount after loading '~~~'", 2, rows.Length);
			AssertEquals("RowCount in Rows[0].Table", 2, rows[0].Table.Rows.Count);

			ZQuery sQLFilter;
			sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Code, "~~~");
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Description, "~Noodle~");
			rows = factory.Load(DummyBizoSchema.Constants.TableName, sQLFilter);
			Assert(rows.Length == 1);
			Assert(rows[0].Table.Rows.Count == 2);

			rows = factory.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "$$$"));
			Assert(rows.Length == 1);
			Assert(rows[0].Table.Rows.Count == 3);
		}

		public void TestLoadTopN()
		{
			RowFactory factory = new RowFactory();

			CreateDummyInDB(Guid.NewGuid(), "~~~", "~Comrade~");
			CreateDummyInDB(Guid.NewGuid(), "~~~", "~Noodle~");
			CreateDummyInDB(Guid.NewGuid(), "~~~", "~Bag~");

			ZQuery sQLFilter = new ZQuery(DummyBizoSchema.Z0_Code, "~~~");
			DataRow[] rows = factory.Load(DummyBizoSchema.Constants.TableName, sQLFilter);
			Assert(rows.Length == 3);

			sQLFilter.MaximumRows = 1;
			rows = factory.Load(DummyBizoSchema.Constants.TableName, sQLFilter);
			Assert(rows.Length == 1);

			sQLFilter.MaximumRows = 2;
			rows = factory.Load(DummyBizoSchema.Constants.TableName, sQLFilter);
			Assert(rows.Length == 2);

			sQLFilter.MaximumRows = 5;
			rows = factory.Load(DummyBizoSchema.Constants.TableName, sQLFilter);
			Assert(rows.Length == 3);
		}

		public void TestLoadWithWhereAndSortClause()
		{
			RowFactory factory = new RowFactory();

			ZQuery sQLFilter = new ZQuery();
			sQLFilter.MaximumRows = 1000;  // This allows the second query to go to the db again
			sQLFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "ABC");
			sQLFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "BCD");
			sQLFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "CED");
			DataRow[] rows = factory.Load(DummyBizoSchema.Constants.TableName, sQLFilter);
			AssertEquals("Pre-Insert Row Count", 0, rows.Length);

			CreateDummyInDB(Guid.NewGuid(), "ABC", "12345");
			CreateDummyInDB(Guid.NewGuid(), "BCD", "23456");
			CreateDummyInDB(Guid.NewGuid(), "CED", "34567");

			sQLFilter.OrderBy = DummyBizoSchema.Constants.Z0_Code;

			rows = factory.Load(DummyBizoSchema.Constants.TableName, sQLFilter);
			AssertEquals("Count", 3, rows.Length);
			AssertEquals("First Row", "ABC", rows[0][DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Second Row", "BCD", rows[1][DummyBizoSchema.Constants.Z0_Code]);
			AssertEquals("Third Row", "CED", rows[2][DummyBizoSchema.Constants.Z0_Code]);
		}

		public void TestLoadWithWhereClauseAndParams()
		{
			RowFactory factory = new RowFactory();

			CreateDummyInDB(Guid.NewGuid(), "~~~", "~Comrade~");
			CreateDummyInDB(Guid.NewGuid(), "~~~", "~Noodle~");
			CreateDummyInDB(Guid.NewGuid(), "$$$", "~Noodle~");

			DataRow[] rows = factory.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "~~~"));
			Assert(rows.Length == 2);
			Assert(rows[0].Table.Rows.Count == 2);

			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Code, "~~~");
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Description, "~Comrade~");

			rows = factory.Load(DummyBizoSchema.Constants.TableName, sQLFilter);
			Assert(rows.Length == 1);
			Assert(rows[0].Table.Rows.Count == 2);

			rows = factory.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Code, "$$$"));
			Assert(rows.Length == 1);
			Assert(rows[0].Table.Rows.Count == 3);
		}

		public void TestLoadWithNoResultFilter()
		{
			CreateDummyInDB(Guid.NewGuid(), "ABC", "12345");
			CreateDummyInDB(Guid.NewGuid(), "BCD", "23456");
			CreateDummyInDB(Guid.NewGuid(), "CED", "34567");

			var rowFactory = new RowFactory();
			var table = rowFactory.GetTable(DummyBizoSchema.Constants.TableName, true);

			var filter = new ZQuery();
			filter.IsNoResultQuery = true;
			var rows = rowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("Nothing should be found", 0, rows.Length);
			AssertEquals("Nothing should be loaded into memory", 0, table.Rows.Count);

			filter.IsNoResultQuery = false;
			rows = rowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(3, rows.Length);
			AssertEquals(3, table.Rows.Count);
		}

		public void TestQuotingAndEscaping()
		{
			RowFactory factory = new RowFactory();

			CreateDummyInDB(Guid.NewGuid(), "~~~", "~'Comrade~");
			CreateDummyInDB(Guid.NewGuid(), "~~~", "~No''odle~");
			CreateDummyInDB(Guid.NewGuid(), "$$$", "\"'''-~`");

			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyBizoSchema.Z0_Description, "~'Comrade~");
			sQLFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "~No''odle~");
			sQLFilter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "\"'''-~`");
			DataRow[] rows = factory.Load(DummyBizoSchema.Constants.TableName, sQLFilter);
			Assert(rows.Length == 3);
			Assert(rows[0].Table.Rows.Count == 3);
		}

		public void TestSaveInsert()
		{
			RowFactory factory = new RowFactory();

			DataRow dummy = factory.New("DummyBizo");
			Guid pK = DummyBusinessObject.SetDataRowDefaultValues(dummy);
			dummy.Table.Rows.Add(dummy);

			factory.Save();

			RowFactory factory2 = new RowFactory();
			DataRow loadedDummy = factory2.LoadFromPK(DummyBusinessObject.Schema.TableName, pK);

			AssertNotNull(loadedDummy);
			AssertEquals("Loaded OK", pK, loadedDummy["Z0_PK"]);
		}

		public void TestSaveUpdate()
		{
			RowFactory factory = new RowFactory();

			DataRow dummy = factory.New(DummyBusinessObject.Schema.TableName);
			Guid pK = DummyBusinessObject.SetDataRowDefaultValues(dummy);
			dummy["Z0_Description"] = "FirstVersion";

			dummy.Table.Rows.Add(dummy);

			factory.Save();

			RowFactory factory2 = new RowFactory();
			DataRow loadedDummy = factory2.LoadFromPK(DummyBusinessObject.Schema.TableName, pK);

			AssertNotNull(loadedDummy);
			AssertEquals("Loaded OK", pK, (Guid)loadedDummy["Z0_PK"]);
			AssertEquals("Loaded OK", "FirstVersion", loadedDummy["Z0_Description"].ToString().Trim());

			loadedDummy["Z0_Description"] = "SecondVersion";
			factory2.Save();

			RowFactory factory3 = new RowFactory();
			DataRow reloadedDummy = factory3.LoadFromPK(DummyBusinessObject.Schema.TableName, pK);

			AssertNotNull(reloadedDummy);
			AssertEquals("Loaded OK", pK, (Guid)reloadedDummy["Z0_PK"]);
			AssertEquals("Got updated!", "SecondVersion", reloadedDummy["Z0_Description"].ToString().Trim());
		}

		public void TestSaveDelete()
		{
			RowFactory factory = new RowFactory();

			DataRow dummy = factory.New(DummyBusinessObject.Schema.TableName);
			Guid pK = DummyBusinessObject.SetDataRowDefaultValues(dummy);
			dummy.Table.Rows.Add(dummy);

			factory.Save();

			RowFactory factory2 = new RowFactory();
			DataRow loadedDummy = factory2.LoadFromPK(DummyBusinessObject.Schema.TableName, pK);

			AssertNotNull(loadedDummy);
			AssertEquals("Loaded OK", pK, loadedDummy["Z0_PK"]);

			loadedDummy.Delete();
			factory2.Save();

			RowFactory factory3 = new RowFactory();
			DataRow reloadedDummy = factory3.LoadFromPK(DummyBusinessObject.Schema.TableName, pK);
			AssertNull("Load should fail as Dummy has been deleted!", reloadedDummy);
		}

		#region TestClearQueryCache

		public void TestClearQueryCache_ForSpecificTable()
		{
			var rowFactory = new RowFactory();
			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "AAA");
			var query2 = new ZQuery(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.StartsWith, "BBB");

			int dBhits = rowFactory.DatabaseLoadCount;
			rowFactory.Load(DummyBizoSchema.Constants.TableName, query1);
			rowFactory.Load(DummyDependentBizoSchema.Constants.TableName, query2);
			AssertEquals(dBhits + 2, rowFactory.DatabaseLoadCount);

			rowFactory.Load(DummyBizoSchema.Constants.TableName, query1);
			rowFactory.Load(DummyDependentBizoSchema.Constants.TableName, query2);
			AssertEquals("Queries should be cached after 1-st run, no new DB hits should happen.", dBhits + 2, rowFactory.DatabaseLoadCount);

			rowFactory.ClearQueryCache(DummyBizoSchema.Constants.TableName);
			rowFactory.Load(DummyBizoSchema.Constants.TableName, query1);
			rowFactory.Load(DummyDependentBizoSchema.Constants.TableName, query2);
			AssertEquals("Cache for 1 query was cleared, only 1 additional DB hit expected.", dBhits + 3, rowFactory.DatabaseLoadCount);
		}

		public void TestClearQueryCache_ForSpecificTable_DBOnlyQuery()
		{
			var rowFactory = new RowFactory();
			var query1 = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			query1.AddToFilter(DummyBizoSchema.Z0_Code, SQLComparisonOperator.StartsWith, "AAA");
			var query2 = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			query2.AddToFilter(DummyDependentBizoSchema.ZD1_Code, SQLComparisonOperator.StartsWith, "BBB");

			int dBhits = rowFactory.DatabaseLoadCount;
			rowFactory.Load(DummyBizoSchema.Constants.TableName, query1);
			rowFactory.Load(DummyDependentBizoSchema.Constants.TableName, query2);
			AssertEquals(dBhits + 2, rowFactory.DatabaseLoadCount);

			rowFactory.Load(DummyBizoSchema.Constants.TableName, query1);
			rowFactory.Load(DummyDependentBizoSchema.Constants.TableName, query2);
			AssertEquals("Queries should be cached after 1-st run, no new DB hits should happen.", dBhits + 2, rowFactory.DatabaseLoadCount);

			rowFactory.ClearQueryCache(DummyBizoSchema.Constants.TableName);
			rowFactory.Load(DummyBizoSchema.Constants.TableName, query1);
			rowFactory.Load(DummyDependentBizoSchema.Constants.TableName, query2);
			AssertEquals("Cache for 1 query was cleared, only 1 additional DB hit expected.", dBhits + 3, rowFactory.DatabaseLoadCount);
		}

		#endregion

		public void TestSaveTogetherSuccessful()
		{
			RowFactory factory1 = new RowFactory();
			DataRow dummy1 = factory1.New(DummyBusinessObject.Schema.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(dummy1);
			dummy1.Table.Rows.Add(dummy1);

			RowFactory factory2 = new RowFactory();
			DataRow dummy2 = factory2.New(DummyBusinessObject.Schema.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(dummy2);
			dummy2.Table.Rows.Add(dummy2);

			RowFactory factory3 = new RowFactory();
			DataRow dummy3 = factory3.New(DummyBusinessObject.Schema.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(dummy3);
			dummy3.Table.Rows.Add(dummy3);

			RowFactory.SaveTogether(factory1, factory2, factory3);

			RowFactory loader = new RowFactory();
			DataRow[] rows = loader.Load(DummyBusinessObject.Schema.TableName, new ZQuery()); // yeah, we want themn all!
			AssertEquals("All saved and loaded OK!", 3, rows.Length);
		}

		public void TestLoadingDeletedRows()
		{
			RowFactory factory = new RowFactory();

			DataRow dummy = factory.New(DummyBusinessObject.Schema.TableName);
			Guid pK = DummyBusinessObject.SetDataRowDefaultValues(dummy);
			dummy.Table.Rows.Add(dummy);

			factory.Save();

			RowFactory factory2 = new RowFactory();
			DataRow loadedDummy = factory2.LoadFromPK(DummyBusinessObject.Schema.TableName, pK);

			AssertNotNull(loadedDummy);
			AssertEquals("Loaded OK", pK, loadedDummy["Z0_PK"]);

			loadedDummy.Delete();

			DataRow deletedDummy = factory2.LoadFromPK(DummyBusinessObject.Schema.TableName, pK);
			AssertNull("Shouldn't be able to load deleted row!", deletedDummy);

			RowFactory factory3 = new RowFactory();
			deletedDummy = factory3.LoadFromPK(DummyBusinessObject.Schema.TableName, pK);
			AssertNotNull("Should be able to load row as delete has not been written to the DB", deletedDummy);
		}

		public void TestGetDatabaseCount()
		{
			AssertEquals("GetDatabaseCount()", 0, RowFactory.GetDatabaseCount(AutoDummyBizo.Schema.TableName));

			DataRow dummyRow = RowFactory.New(AutoDummyBizo.Schema.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(dummyRow);
			dummyRow.Table.Rows.Add(dummyRow);

			RowFactory.Save();
			AssertEquals("GetDatabaseCount()", 1, RowFactory.GetDatabaseCount(AutoDummyBizo.Schema.TableName));
		}

		public void TestEnsureConnectionIsOpen()
		{
			using (DbConnection connection = new ConnectionWithTinyTimeoutForTest(Db.ServerName, Db.DatabaseName))
			{
				RowFactory testRowFactory = new RowFactory(connection, Db.DatabaseName);
				testRowFactory.EnsureConnectionIsOpen();
				AssertEquals(ConnectionState.Open, connection.State);
				connection.CloseConnection();
				AssertEquals(ConnectionState.Closed, connection.State);
				testRowFactory.EnsureConnectionIsOpen();
				AssertEquals(ConnectionState.Open, connection.State);
			}
		}

		public void TestAndSQLMatchDoesNotHitDB()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "X");
			RowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(1, RowFactory.DatabaseLoadCount);

			filter.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "Y");
			RowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("And : should not hit db again", 1, RowFactory.DatabaseLoadCount);
		}

		public void TestOrFilterMatchDoesHitDB()
		{
			ZQuery filter = new ZQuery(DummyBizoSchema.Z0_Code, "X");
			RowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(1, RowFactory.DatabaseLoadCount);

			filter.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "Z");
			RowFactory.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals("Or : should hit db as cannot be worked out", 2, RowFactory.DatabaseLoadCount);
		}

		public void TestLoadWithBlobsYes()
		{
			LoadWithBlobsHelper(true);
		}

		public void TestLoadWithBlobsNo()
		{
			LoadWithBlobsHelper(false);
		}

		void LoadWithBlobsHelper(bool loadWithBlob)
		{
			RowFactory factory = new RowFactory();

			DataRow dummy = factory.New(DummyBusinessObject.Schema.TableName);
			Guid pK = DummyBusinessObject.SetDataRowDefaultValues(dummy);
			dummy[DummyBizoSchema.Z0_VarCharMax.Name] = new string('z', 1050);
			dummy[DummyBizoSchema.Z0_NVarCharMax.Name] = new string('z', 1050);
			dummy.Table.Rows.Add(dummy);

			factory.Save();

			RowFactory factory2 = new RowFactory();

			ZQuery query = new ZQuery(DummyBizoSchema.PK, pK);
			if (loadWithBlob)
			{
				query.IncludeBlob(DummyBizoSchema.Z0_VarCharMax);
			}
			DataRow loadedDummy = factory2.Load(DummyBusinessObject.Schema.TableName, query)[0];
			AssertEquals(!loadWithBlob, LazyLoading.LoadRequired(loadedDummy[DummyBizoSchema.Z0_VarCharMax.Name]));
			AssertEquals(true, LazyLoading.LoadRequired(loadedDummy[DummyBizoSchema.Z0_NVarCharMax.Name]));
		}

		public void TestEnableTableHitQueryCollection()
		{
			var bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_Short = (short)4;
			Factory.Save();

			var rowFactory = new RowFactory();

			using (rowFactory.EnableTableHitQueryCollection(new[] { DummyBizoSchema.Constants.TableName }))
			{
				var query1 = new ZQuery(DummyBizoSchema.Z0_Short, (short)4);
				AssertEquals("Precondition: Loaded row.", 1, rowFactory.Load(DummyBizoSchema.Constants.TableName, query1).Length);
			}

			var tableSelect = rowFactory.TableSelects.Single();
			AssertEquals(nameof(tableSelect.TableName), DummyBizoSchema.Constants.TableName, tableSelect.TableName);
			AssertEquals(nameof(tableSelect.Value), 1, tableSelect.Value);

			var tableSelectQuery = tableSelect.Queries.Single();
			AssertEndsWith(nameof(tableSelectQuery.Query), "WHERE Z0_Short = 4", tableSelectQuery.Query);

			var query2 = new ZQuery(DummyBizoSchema.Z0_Short, (short)2);
			AssertEquals("Precondition: Loaded no rows.", 0, rowFactory.Load(DummyBizoSchema.Constants.TableName, query2).Length);
			AssertEquals("Should not have appended a new query without query collection enabled.", 1, rowFactory.TableSelects.Single().Queries.Count());
		}

		public void TestEnableTableHitQueryCollection_WithUberFactory()
		{
			var bizO = Factory.New<DummyBusinessObject>();
			bizO.Z0_Short = (short)4;
			Factory.Save();

			using (RowFactory.SetCachedTables(DummyBizoSchema.Constants.TableName))
			{
				AssertEquals("Precondition: Should be cached.", true, RowFactory.IsCachedTable(DummyBizoSchema.Constants.TableName));

				var rowFactory1 = new RowFactory();
				var rowFactory2 = new RowFactory();

				rowFactory2.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Short, (short)1));

				using (rowFactory1.EnableTableHitQueryCollection(new[] { DummyBizoSchema.Constants.TableName }))
				{
					var query1 = new ZQuery(DummyBizoSchema.Z0_Short, (short)4);
					AssertEquals("Precondition: Loaded row.", 1, rowFactory1.Load(DummyBizoSchema.Constants.TableName, query1).Length);

					rowFactory2.Load(DummyBizoSchema.Constants.TableName, new ZQuery(DummyBizoSchema.Z0_Short, (short)2));
				}

				var rowFactory1TableSelect = rowFactory1.TableSelects.Single(ts => ts.TableName == DummyBizoSchema.Constants.TableName);
				AssertEquals(nameof(rowFactory1TableSelect.TableName), DummyBizoSchema.Constants.TableName, rowFactory1TableSelect.TableName);
				AssertEquals(nameof(rowFactory1TableSelect.Value), 1, rowFactory1TableSelect.Value);

				var rowFactory1TableSelectQuery = rowFactory1TableSelect.Queries.Single();
				AssertEndsWith(nameof(rowFactory1TableSelectQuery.Query), "WHERE Z0_Short = 4", rowFactory1TableSelectQuery.Query);

				var rowFactory2TableSelect = rowFactory2.TableSelects.Single(ts => ts.TableName == DummyBizoSchema.Constants.TableName);
				AssertEquals(nameof(rowFactory2TableSelect.TableName), DummyBizoSchema.Constants.TableName, rowFactory2TableSelect.TableName);
				AssertEquals(nameof(rowFactory2TableSelect.Value), 2, rowFactory2TableSelect.Value);
				AssertEquals("Should not collected any queries on rowFactory2.", 0, rowFactory2TableSelect.Queries.Count());

				var uberRowFactory = RowFactory.UberFactoryRememberToLock.RowFactory;
				var uberRowFactoryTableSelect = uberRowFactory.TableSelects.Single(ts => ts.TableName == DummyBizoSchema.Constants.TableName);
				AssertEquals(nameof(uberRowFactoryTableSelect.TableName), DummyBizoSchema.Constants.TableName, uberRowFactoryTableSelect.TableName);
				AssertEquals(nameof(uberRowFactoryTableSelect.Value), 3, uberRowFactoryTableSelect.Value);
				AssertEquals("Should have collected one query on the uberRowFactory.", 1, uberRowFactoryTableSelect.Queries.Count());
				AssertEndsWith("Should have collected one query on the uberRowFactory.", "WHERE Z0_Short = 4", uberRowFactoryTableSelect.Queries.Single().Query);
			}
		}

		[ExpectNoExceptions]
		public void TestMultipleFetchHintsThatShareACommonParameterisedFilterInSubQuery()
		{
			var factory = new BusinessObjectFactory();

			// 1st Query with filter on Z0_Code
			var query1 = new ZQuery(DummyBizoSchema.Z0_Code, "111");
			factory.AddFetchHint(DummyBizoSchema.Instance, query1);

			// 2nd Query with a filter on a different field. ZD1_Code appears on the sql script before Z0_Code
			var query2 = new ZDBOnlyQuery(typeof(DummyDependantBusinessObject));
			query2.AddToFilter(DummyDependentBizoSchema.ZD1_Code, "222");

			// SubQuery of query 2 which uses the same filter as query 1
			var subQuery = new ZDBOnlySubQuery(typeof(DummyBusinessObject), DummyBizoSchema.Z0_FK_Code);
			subQuery.AddToFilter(query1);
			query2.AddSubQuery(subQuery, JoinCondition.And);

			factory.AddFetchHint(DummyDependentBizoSchema.Instance, query2);

			using (var executedCommands = TestConnection.TrackExecutedCommands(false, false))
			{
				factory.ExecuteAllFetchHints();
				CombineAssertions("The test failed to reproduce a condition where a parameter gets shared between two queries.", () =>
				{
					Assert(TestConnection.ExecutedCommands.Any(x => x.Contains("Z0_Code = @CWO1_") && x.Contains("@CWO1_: '111'")));
					Assert(TestConnection.ExecutedCommands.Any(x => x.Contains("Z0_Code = @CWO2_") && x.Contains("@CWO2_: '111'")));
				});
			}
		}

		public void TestIsUberFactory()
		{
			var rowFactory = new RowFactory();
			var uberRowFactory = RowFactory.UberFactoryRememberToLock.RowFactory;
			Assert(!rowFactory.IsUberFactory);
			Assert(uberRowFactory.IsUberFactory);
		}

		#region Test LoadFromCacheFindsUnloadedBlob

		public void TestLoadFromCacheFindsUnloadedBlob()
		{
			RowFactory factory1 = new RowFactory();
			DataRow row = CreateNewDummyRowWithBlobInDB(factory1, "Loxodon Hierarch");

			RowFactory factory2 = new RowFactory();
			ZGuid pK = (Guid)row[DummyBizoSchema.Constants.PK];
			factory2.LoadFromPK(DummyBizoSchema.Constants.TableName, pK); // important - caches the row without the blob

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.PK, pK);
			filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.Contains, "Loxodon");

			DataRow[] loadedRows = factory2.Load(DummyBizoSchema.Constants.TableName, filter);
			AssertEquals(1, loadedRows.Length);
			AssertEquals(pK, loadedRows[0][DummyBizoSchema.Constants.PK]);
			AssertEquals("Loxodon Hierarch", loadedRows[0][DummyBizoSchema.Constants.Z0_VarCharMax]);
		}

		public void TestLoadFromDbWithBlobFilter()
		{
			RowFactory factory = new RowFactory();
			DataRow row = CreateNewDummyRowWithBlobInDB(factory, "Mindleech Mass");

			RowFactory factory2 = new RowFactory();
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.PK, (Guid)row[DummyBizoSchema.Constants.PK]);
			filter.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.Contains, "Mindleech");

			AssertEquals("Should have loaded a single row matching the blob filter.", 1, factory2.Load(DummyBizoSchema.Constants.TableName, filter).Length);
		}

		public void TestLoadFromDbWithBlobFilterWhenFilteringWithAnotherFieldThatHasChangesInRowAndBlobNotLoaded()
		{
			RowFactory factory = new RowFactory();
			DataRow row = CreateNewDummyRowWithBlobInDB(factory, "Tolsimir Wolfblood");

			RowFactory factory2 = new RowFactory();
			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyBizoSchema.PK, (Guid)row[DummyBizoSchema.Constants.PK]);
			filter.AddToFilter(DummyBizoSchema.PK, SQLComparisonOperator.NotEqual, ZGuid.NewZGuid());   // stop optimisations

			DataRow[] matchingRows = factory2.Load(DummyBizoSchema.Constants.TableName, filter);
			matchingRows[0]["Z0_Short"] = 1;
			ZQuery filter2 = new ZQuery();
			filter2.AddToFilter(DummyBizoSchema.Z0_VarCharMax, SQLComparisonOperator.Contains, "Wolfblood");
			filter2.AddToFilter(DummyBizoSchema.Z0_Short, (short)1);

			AssertEquals("Should have loaded a single row matching the blob filter.", 1, factory2.Load(DummyBizoSchema.Constants.TableName, filter2).Length);
		}

		DataRow CreateNewDummyRowWithBlobInDB(RowFactory factory, ZString blobText)
		{
			DataRow row = factory.New(DummyBizoSchema.Constants.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(row);
			row[DummyBizoSchema.Constants.Z0_VarCharMax] = blobText;
			row.Table.Rows.Add(row);
			factory.Save();

			return row;
		}

		#endregion

		#region Test Select with LIKE Operator

		public void TestSelectWithLikeOperator()
		{
			AssertSelectWithLikeOperator();
		}

		public void TestSelectWithLikeOperatorWithIndexes()
		{
			int originalMaximumRowsBeforeUsingIndex = Factory.RowFactory.MaximumRowsBeforeUsingIndex;
			try
			{
				Factory.RowFactory.MaximumRowsBeforeUsingIndex = 1;
				AssertSelectWithLikeOperator();
			}
			finally
			{
				Factory.RowFactory.MaximumRowsBeforeUsingIndex = originalMaximumRowsBeforeUsingIndex;
			}
		}

		void AssertSelectWithLikeOperator()
		{
			CreateTestData();

			AssertLoad("A%", true, false, Factory, new[] { "A", "AB", "ABC", "AABB", "AxB" });
			AssertLoad("%B", true, false, Factory, new[] { "B", "AB", "AABB", "AxB" });
			AssertLoad("A%B", true, false, Factory, new[] { "AB", "AABB", "AxB" });
			AssertLoad("%x%", true, false, Factory, new[] { "AxB", "xABx" });
			AssertLoad("A_B", true, false, Factory, new[] { "AxB" });
			AssertLoad("A__", true, false, Factory, new[] { "ABC", "AxB" });
			AssertLoad(new[] { "A%", "%B" }, JoinCondition.And, true, false, Factory, new[] { "AB", "AABB", "AxB" });
			AssertLoad(new[] { "A%", "%B" }, JoinCondition.Or, true, false, Factory, new[] { "A", "AB", "ABC", "AABB", "AxB", "B" });

			AssertLoad("A%", false, true, Factory, Array.Empty<string>());
			AssertLoad("%B", false, true, Factory, Array.Empty<string>());
			AssertLoad("A%B", false, true, Factory, Array.Empty<string>());
			AssertLoad("%x%", false, true, Factory, Array.Empty<string>());
			AssertLoad("A_B", false, true, Factory, Array.Empty<string>());

			BusinessObjectFactory otherFactory = new BusinessObjectFactory { RefreshEnabled = false };
			otherFactory.RowFactory.MaximumRowsBeforeUsingIndex = Factory.RowFactory.MaximumRowsBeforeUsingIndex;
			AddDummy("XYZ", "AzzzB", otherFactory);

			AssertLoad("A%B", false, false, otherFactory, new[] { "AzzzB" });

			Factory.Save();
			AssertLoad("A%", true, false, Factory, new[] { "A", "AB", "ABC", "AABB", "AxB" });
			AssertLoad("%B", true, false, Factory, new[] { "B", "AB", "AABB", "AxB" });
			AssertLoad("A%B", true, false, Factory, new[] { "AB", "AABB", "AxB" });
			AssertLoad("%x%", true, false, Factory, new[] { "AxB", "xABx" });
			AssertLoad("A_B", true, false, Factory, new[] { "AxB" });
			AssertLoad("A__", true, false, Factory, new[] { "ABC", "AxB" });
			AssertLoad(new[] { "A%", "%B" }, JoinCondition.And, true, false, Factory, new[] { "AB", "AABB", "AxB" });
			AssertLoad(new[] { "A%", "%B" }, JoinCondition.Or, true, false, Factory, new[] { "A", "AB", "ABC", "AABB", "AxB", "B" });

			AssertLoad("A%", false, true, Factory, new[] { "A", "AB", "ABC", "AABB", "AxB" });
			AssertLoad("%B", false, true, Factory, new[] { "B", "AB", "AABB", "AxB" });
			AssertLoad("A%B", false, true, Factory, new[] { "AB", "AABB", "AxB" });
			AssertLoad("%x%", false, true, Factory, new[] { "AxB", "xABx" });
			AssertLoad("A_B", false, true, Factory, new[] { "AxB" });
			AssertLoad("A__", false, true, Factory, new[] { "ABC", "AxB" });
			AssertLoad(new[] { "A%", "%B" }, JoinCondition.And, false, true, Factory, new[] { "AB", "AABB", "AxB" });
			AssertLoad(new[] { "A%", "%B" }, JoinCondition.Or, false, true, Factory, new[] { "A", "AB", "ABC", "AABB", "AxB", "B" });

			otherFactory.ClearQueryCache();

			AssertLoad("A%B", true, false, otherFactory, new[] { "AzzzB" });
			AssertLoad("A%B", false, true, otherFactory, new[] { "AB", "AABB", "AxB" });
			AssertLoad("A%B", false, false, otherFactory, new[] { "AzzzB", "AB", "AABB", "AxB" });

			AssertLoad("%z%z%z%", true, false, otherFactory, new[] { "AzzzB" });
			AssertLoad("%z%z%z%", false, true, otherFactory, Array.Empty<string>());
			AssertLoad("%z%z%z%", false, false, otherFactory, new[] { "AzzzB" });
		}

		public void TestSelectWithComplicatedLikeOperators_OR()
		{
			CreateTestData();
			AssertLoad(new[] { "A_", "_B" }, JoinCondition.Or, true, false, Factory, new[] { "AB" });
		}

		public void TestSelectWithComplicatedLikeOperators()
		{
			CreateTestData();

			//Local only

			AssertLoad(new[] { "A_", "_B" }, JoinCondition.And, true, false, Factory, new[] { "AB" });
			AssertLoad(new[] { "Z_", "_B" }, JoinCondition.And, true, false, Factory, Array.Empty<string>());
			AssertLoad(new[] { "A_", "_B", "A_B" }, JoinCondition.And, true, false, Factory, Array.Empty<string>());
			AssertLoad(new[] { "A__", "__B", "A_B" }, JoinCondition.And, true, false, Factory, new string[] { "AxB" });

			AssertLoad(new[] { "A_", "_B" }, JoinCondition.Or, true, false, Factory, new[] { "AB" });
			AssertLoad(new[] { "Z_", "_B" }, JoinCondition.Or, true, false, Factory, new[] { "AB" });
			AssertLoad(new[] { "A_", "_B", "A_B" }, JoinCondition.Or, true, false, Factory, new[] { "AB", "AxB" });
			AssertLoad(new[] { "A__", "__B", "A_B" }, JoinCondition.Or, true, false, Factory, new string[] { "ABC", "AxB" });

			//DB only

			Factory.Save();
			AssertLoad(new[] { "A_", "_B" }, JoinCondition.And, false, true, Factory, new[] { "AB" });
			AssertLoad(new[] { "Z_", "_B" }, JoinCondition.And, false, true, Factory, Array.Empty<string>());
			AssertLoad(new[] { "A_", "_B", "A_B" }, JoinCondition.And, false, true, Factory, Array.Empty<string>());
			AssertLoad(new[] { "A__", "__B", "A_B" }, JoinCondition.And, false, true, Factory, new string[] { "AxB" });

			AssertLoad(new[] { "A_", "_B" }, JoinCondition.Or, false, true, Factory, new[] { "AB" });
			AssertLoad(new[] { "Z_", "_B" }, JoinCondition.Or, false, true, Factory, new[] { "AB" });
			AssertLoad(new[] { "A_", "_B", "A_B" }, JoinCondition.Or, false, true, Factory, new[] { "AB", "AxB" });
			AssertLoad(new[] { "A__", "__B", "A_B" }, JoinCondition.Or, false, true, Factory, new string[] { "ABC", "AxB" });
		}

		public void TestSelectWithComplicatedLikeOperators_Contains()
		{
			CreateTestData();

			var query = new ZQuery() { FetchOnlyFromLocalCache = true };

			var subQuery = new ZQuery() { FetchOnlyFromLocalCache = true };

			query.AddToFilter(DummyBizoSchema.Z0_Code, "XYZ");

			subQuery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Contains, "C");
			subQuery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_B");

			query.AddToFilter(subQuery);

			var expectedResults = new[] { "ABC", "AxB" };
			DummyBusinessObject[] dummies = Factory.Load<DummyBusinessObject>(query);

			AssertEquals(expectedResults.Length, dummies.Length);
			AssertContainsExactElementsInAnyOrder(expectedResults, from dummy in dummies select dummy.Z0_Description.ToString());
		}

		public void TestSelectWithComplicatedLikeOperators_BracketOrAnd()
		{
			// ie. (((A or B) and C)

			CreateTestData();

			var query = new ZQuery() { FetchOnlyFromLocalCache = true };

			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_");
			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_C");
			query.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "XYZ");

			DummyBusinessObject[] dummies = Factory.Load<DummyBusinessObject>(query);

			var expectedResults = new[] { "AB", "ABC" };
			AssertEquals(expectedResults.Length, dummies.Length);
			AssertContainsExactElementsInAnyOrder(expectedResults, from dummy in dummies select dummy.Z0_Description.ToString());
		}

		public void TestSelectWithComplicatedLikeOperators_BracketOrAnd2()
		{
			// ie. (((A or B) and C)

			CreateTestData();

			var query = new ZQuery() { FetchOnlyFromLocalCache = true };

			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_");
			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_C");
			query.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "ABC");

			DummyBusinessObject[] dummies = Factory.Load<DummyBusinessObject>(query);

			var expectedResults = new[] { "ABC" };
			AssertEquals(expectedResults.Length, dummies.Length);
			AssertContainsExactElementsInAnyOrder(expectedResults, from dummy in dummies select dummy.Z0_Description.ToString());
		}

		public void TestSelectWithComplicatedLikeOperators_BracketOrAnd3()
		{
			// ie. (((A or B) and C)

			CreateTestData();

			var subQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			subQuery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_");
			subQuery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_C");

			var query = new ZQuery() { FetchOnlyFromLocalCache = true };
			query.AddToFilter(subQuery);
			query.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "ABC");

			DummyBusinessObject[] dummies = Factory.Load<DummyBusinessObject>(query);

			var expectedResults = new[] { "ABC" };
			AssertEquals(expectedResults.Length, dummies.Length);
			AssertContainsExactElementsInAnyOrder(expectedResults, from dummy in dummies select dummy.Z0_Description.ToString());
		}

		public void TestSelectWithComplicatedLikeOperators_Mix()
		{
			CreateTestData();

			var query = new ZQuery() { FetchOnlyFromLocalCache = true };

			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "ZZZ");
			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_");
			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Equal, "xABx");
			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_C");
			query.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Contains, "x");
			query.AddToFilter(JoinCondition.And, DummyBizoSchema.Z0_Code, SQLComparisonOperator.Equal, "XYZ");

			DummyBusinessObject[] dummies = Factory.Load<DummyBusinessObject>(query);

			var expectedResults = new[] { "AB", "ABC", "xABx", "AxB" };
			AssertEquals(expectedResults.Length, dummies.Length);
			AssertContainsExactElementsInAnyOrder(expectedResults, from dummy in dummies select dummy.Z0_Description.ToString());
		}

		public void TestSelectWithComplicatedLikeOperators_SpecialCase()
		{
			// This reproduces the problem of Work Item WI00278225 (Query from BatchImportManager.GetImportMailFilter())

			CreateTestData();

			var query = new ZQuery() { FetchOnlyFromLocalCache = true };
			query.AddToFilter(DummyBizoSchema.Z0_Code, "XYZ");

			var containsQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			containsQuery.AddToFilter(DummyBizoSchema.Z0_Description, SQLComparisonOperator.Contains, "[Z");

			var likeQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
			likeQuery.AddToFilter(JoinCondition.Or, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, "A_B");

			var subQuery = new ZQuery(containsQuery, JoinCondition.Or, likeQuery) { FetchOnlyFromLocalCache = true };

			query.AddToFilter(subQuery);

			var expectedResults = new[] { "AxB" };
			DummyBusinessObject[] dummies = Factory.Load<DummyBusinessObject>(query);

			AssertEquals(expectedResults.Length, dummies.Length);
			AssertContainsExactElementsInAnyOrder(expectedResults, from dummy in dummies select dummy.Z0_Description.ToString());
		}

		void CreateTestData()
		{
			AddDummy("XYZ", "A", Factory);
			AddDummy("XYZ", "B", Factory);
			AddDummy("XYZ", "AB", Factory);
			AddDummy("XYZ", "ABC", Factory);
			AddDummy("XYZ", "AABB", Factory);
			AddDummy("XYZ", "AxB", Factory);
			AddDummy("XYZ", "xABx", Factory);
		}

		void AddDummy(string code, string description, BusinessObjectFactory factory)
		{
			DummyBusinessObject dummy = factory.NewWithValidTestData<DummyBusinessObject>();
			dummy.Z0_Code = code;
			dummy.Z0_Description = description;
		}

		void AssertLoad(string template, bool localOnly, bool dbOnly, BusinessObjectFactory factory, string[] expectedResults)
		{
			AssertLoad(new[] { template }, JoinCondition.And, localOnly, dbOnly, factory, expectedResults);
		}

		void AssertLoad(string[] templates, JoinCondition joinCondition, bool localOnly, bool dbOnly, BusinessObjectFactory factory, string[] expectedResults)
		{
			ZQuery query;
			ZQuery subQuery;
			if (dbOnly)
			{
				query = new ZDBOnlyQuery(typeof(DummyBusinessObject));
				subQuery = new ZDBOnlyQuery(typeof(DummyBusinessObject));
			}
			else
			{
				query = new ZQuery();
				query.FetchOnlyFromLocalCache = localOnly;

				subQuery = new ZQuery();
				subQuery.FetchOnlyFromLocalCache = localOnly;
			}
			query.AddToFilter(DummyBizoSchema.Z0_Code, "XYZ");

			foreach (var template in templates)
			{
				subQuery.AddToFilter(joinCondition, DummyBizoSchema.Z0_Description, SQLComparisonOperator.Like, template);
			}

			query.AddToFilter(subQuery);

			DummyBusinessObject[] dummies = factory.Load<DummyBusinessObject>(query);

			AssertEquals(expectedResults.Length, dummies.Length);
			AssertContainsExactElementsInAnyOrder(expectedResults, from dummy in dummies select dummy.Z0_Description.ToString());
		}

		#endregion

		#region Culture Tests

		[ExpectNoExceptions]
		public void TestDanishSortAndFilter()
		{
			const string pkText = "7aa8da11-f6f8-4ede-b519-646b44be2b8a";
			Guid pk = new Guid(pkText);

			BusinessObjectFactory factory = new BusinessObjectFactory();

			factory.NewWithPrimaryKey<DummyBusinessObject>(pk).Z0_Code = "aa";
			factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid("7008d011-f6f8-4ede-b519-646b44be2b80")).Z0_Code = "a";
			factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid("70a8da11-f6f8-4ede-b519-646b44be2b8d")).Z0_Code = "b";
			factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid("7a08da11-f6f8-4ede-b519-646b44be2b8f")).Z0_Code = "c";
			factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid("7af8da11-f6f8-4ede-b519-646b44be2b8c")).Z0_Code = "x";
			factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid("7fa8da11-f6f8-4ede-b519-646b44be2b8b")).Z0_Code = "y";
			factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid("7ff8df11-f6f8-4ede-b519-646b44be2b81")).Z0_Code = "z";

			AssertEquals("Standard order", "a,aa,b,c,x,y,z,",
				factory.Load<DummyBusinessObject>(new ZQuery { OrderBy = "Z0_Code", FetchOnlyFromLocalCache = true }).Aggregate("", (current, dummy) => current + (dummy.Z0_Code + ",")));

			DataTable dummiesTable = ((INeedDataSet)factory).Data.Tables[DummyBizoSchema.Constants.TableName];

			AssertEquals(1, dummiesTable.Select("Z0_Code = 'aa'").Length);

			//note: using SqlGuid sort now, which prioritizes the last bytes, not the first bytes
			AssertEquals("Standard order", "700,7ff,7aa,7fa,7af,70a,7a0,",
				factory.Load<DummyBusinessObject>(new ZQuery { OrderBy = "Z0_PK", FetchOnlyFromLocalCache = true }).Aggregate("", (current, dummy) => current + (dummy.PK.ToString().Substring(0, 3) + ",")));

			AssertEquals(1, dummiesTable.Select(string.Format("Z0_PK = '{0}'", pk.ToString()), "", ZDataUtils.CurrentRowsNoDeletedFilter).Length);
			AssertEquals(1, dummiesTable.Select(string.Format("Z0_PK = '{0}'", pkText), "", ZDataUtils.CurrentRowsNoDeletedFilter).Length);

			dummiesTable.Locale = CultureInfo.GetCultureInfo("da-DK");

			AssertEquals("Danish order", "a,b,c,x,y,z,aa,",
				factory.Load<DummyBusinessObject>(new ZQuery { OrderBy = "Z0_Code", FetchOnlyFromLocalCache = true }).Aggregate("", (current, dummy) => current + (dummy.Z0_Code + ",")));

			AssertEquals(1, dummiesTable.Select("Z0_Code = 'aa'").Length);

			AssertEquals("Guids are always in standard order", "700,7ff,7aa,7fa,7af,70a,7a0,",
				factory.Load<DummyBusinessObject>(new ZQuery { OrderBy = "Z0_PK", FetchOnlyFromLocalCache = true }).Aggregate("", (current, dummy) => current + (dummy.PK.ToString().Substring(0, 3) + ",")));

			AssertEquals(1, dummiesTable.Select(string.Format("Z0_PK = CONVERT('{0}', 'System.Guid')", pk.ToString()), "", ZDataUtils.CurrentRowsNoDeletedFilter).Length);
			AssertEquals(1, dummiesTable.Select(string.Format("Z0_PK = CONVERT('{0}', 'System.Guid')", pkText), "", ZDataUtils.CurrentRowsNoDeletedFilter).Length);
		}

		public void TestDanishGetRow()
		{
			const string problemPkText = "7aa8da11-f6f8-4ede-b519-646b44be2b8a"; // Main part is "aa"
			const string otherPkText = "7ff8df11-f6f8-4ede-b519-646b44be2b8f"; // Main part is "ff" in same position as "aa" above

			BusinessObjectFactory factory = new BusinessObjectFactory();
			factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid(otherPkText));
			factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid(problemPkText));

			DataTable dummiesTable = ((INeedDataSet)factory).Data.Tables[DummyBizoSchema.Constants.TableName];

			AssertNotNull(dummiesTable.Rows.Find(new Guid(problemPkText)));
			AssertNotNull(dummiesTable.Rows.Find(new ZGuid(problemPkText)));
			AssertNotNull(dummiesTable.Rows.Find(problemPkText));
			AssertNotNull(factory.RowFactory.GetRow(DummyBizoSchema.Constants.TableName, new ZGuid(problemPkText)));

			CultureInfo currentCulture = Thread.CurrentThread.CurrentCulture;
			Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("da-DK");
			try
			{
				factory = new BusinessObjectFactory();
				factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid(otherPkText));
				factory.NewWithPrimaryKey<DummyBusinessObject>(new Guid(problemPkText));

				// Problematic PK
				AssertNotNull(dummiesTable.Rows.Find(new Guid(problemPkText)));
				AssertNull(dummiesTable.Rows.Find(new ZGuid(new ZGuid(problemPkText))));
				AssertNull(dummiesTable.Rows.Find(problemPkText));
				AssertNotNull(factory.RowFactory.GetRow(DummyBizoSchema.Constants.TableName, new ZGuid(problemPkText)));

				// Other PK
				AssertNotNull(dummiesTable.Rows.Find(new Guid(otherPkText)));
				AssertNotNull(dummiesTable.Rows.Find(new ZGuid(otherPkText)));
				AssertNotNull(dummiesTable.Rows.Find(otherPkText));
				AssertNotNull(factory.RowFactory.GetRow(DummyBizoSchema.Constants.TableName, new ZGuid(otherPkText)));
			}
			finally
			{
				Thread.CurrentThread.CurrentCulture = currentCulture;
			}
		}

		#endregion

		#region Implementation

		RowFactory RowFactory
		{
			get
			{
				if (fRowFactory == null)
				{
					fRowFactory = Factory.RowFactory;
				}
				return fRowFactory;
			}
		}
		RowFactory fRowFactory;

		#region Create Shipments in DB

		static void CreateDummyInDB(Guid pk, ZString description)
		{
			CreateDummyInDB(pk, "~~~", description);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1052:DoNotCastFactoryMethod", Justification = "Baseline")]
		static void CreateDummyInDB(Guid pk, ZString code, ZString description)
		{
			BusinessObjectFactory factory = new BusinessObjectFactory();
			DummyBusinessObject dummy = (DummyBusinessObject)factory.New(typeof(DummyBusinessObject), pk);
			dummy.Z0_Code = code;
			dummy.Z0_Description = description;
			factory.Save();
		}

		#endregion

		#endregion
	}

	#region RowFactoryCommitAndRollbackTests

	sealed class RowFactoryRollbackTests : TestCase
	{
		public void TestSaveTogetherRollback()
		{
			try
			{
				RowFactory factory1 = new RowFactory();
				DataRow dummy1 = factory1.New(DummyBusinessObject.Schema.TableName);
				DummyBusinessObject.SetDataRowDefaultValues(dummy1);
				dummy1.Table.Rows.Add(dummy1);

				RowFactory factory2 = new RowFactory();
				DataRow dummy2 = factory2.New(DummyBusinessObject.Schema.TableName);
				// BAD ROW WITH NO PK
				DummyBusinessObject.SetDataRowDefaultValues(dummy2);
				dummy2["Z0_PK"] = DBNull.Value;
				dummy2.Table.Rows.Add(dummy2);

				RowFactory factory3 = new RowFactory();
				DataRow dummy3 = factory3.New(DummyBusinessObject.Schema.TableName);
				DummyBusinessObject.SetDataRowDefaultValues(dummy3);
				dummy3.Table.Rows.Add(dummy3);

				try
				{
					RowFactory.SaveTogether(factory1, factory2, factory3);
					throw new Exception("Row factory save should have failed. Dummy2 has no PK!");
				}
				catch
				{
					// goody, save has failed!
				}

				// We should be all rolled back!
				AssertEquals(0, Db.Connection.AppTransactionCount);

				RowFactory loader = new RowFactory();
				DataRow[] rows = loader.Load(DummyBusinessObject.Schema.TableName, new ZQuery()); // yeah, we want themn all!
				AssertEquals("None saved! Should be all rolled back!", 0, rows.Length);
			}
			finally
			{
				while (Db.Connection.AppTransactionCount > 0)
				{
					Db.Connection.RollbackTransaction();
				}
			}
		}
	}

	sealed class RowFactoryCommitTests : TransactionedTestCase
	{
		public void TestSaveTogetherCommit()
		{
			RowFactory factory1 = new RowFactory();
			DataRow dummy1 = factory1.New(DummyBusinessObject.Schema.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(dummy1);
			dummy1.Table.Rows.Add(dummy1);

			RowFactory factory2 = new RowFactory();
			DataRow dummy2 = factory2.New(DummyBusinessObject.Schema.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(dummy2);
			dummy2.Table.Rows.Add(dummy2);

			RowFactory factory3 = new RowFactory();
			DataRow dummy3 = factory3.New(DummyBusinessObject.Schema.TableName);
			DummyBusinessObject.SetDataRowDefaultValues(dummy3);
			dummy3.Table.Rows.Add(dummy3);

			RowFactory.SaveTogether(factory1, factory2, factory3);

			AssertEquals("We should be all committed. Only the TransactionedTestCase transaction should be open.", 1, Db.Connection.AppTransactionCount);

			RowFactory loader = new RowFactory();
			DataRow[] rows = loader.Load(DummyBusinessObject.Schema.TableName, new ZQuery()); // yeah, we want them all!
			AssertEquals("Should be 3 rows posted!", 3, rows.Length);
		}
	}

	#endregion
}
