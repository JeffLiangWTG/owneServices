using System;
using System.Threading;
using CargoWise.Async;
using CargoWise.Data;
using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DataRefreshBusWithThreadsTest : TestCase
	{
		#region Publish

		[UseSnapshotProtection]
		public void TestPublish()
		{
			var sql = "SELECT COUNT(*) FROM dbo.DummyBizo WHERE Z0_Code = @code";
			var count = (int)Db.Connection.ExecuteScalar(sql,
				cmd => cmd.AddParameterBasedOnDbColumn("@code", "CDA", DummyBizoSchema.Z0_Code));
			AssertEquals(count, 0);

			var factoryDst = new BusinessObjectFactory();
			var dummyDst = factoryDst.New<DummyBusinessObject>();
			dummyDst.Z0_Code = "CDA";
			factoryDst.Save();

			count = (int)Db.Connection.ExecuteScalar(sql,
				cmd => cmd.AddParameterBasedOnDbColumn("@code", "CDA", DummyBizoSchema.Z0_Code));
			AssertEquals(count, 1);

			var factorySrc = new BusinessObjectFactory();
			var dummySrc = factorySrc.Load<DummyBusinessObject>(dummyDst.PK);
			dummySrc.Z0_Code = "CDB";
			factorySrc.Save();

			count = (int)Db.Connection.ExecuteScalar(sql,
				cmd => cmd.AddParameterBasedOnDbColumn("@code", "CDA", DummyBizoSchema.Z0_Code));
			AssertEquals(count, 0);

			count = (int)Db.Connection.ExecuteScalar(sql,
				cmd => cmd.AddParameterBasedOnDbColumn("@code", "CDB", DummyBizoSchema.Z0_Code));
			AssertEquals(count, 1);

			var published = (dummyDst.Z0_Code == "CDB");
			Assert(published);
		}

		[UseSnapshotProtection]
		public void TestPublishInBackground()
		{
			var published = false;

			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factoryDst = new BusinessObjectFactory();
					var dummyDst = factoryDst.New<DummyBusinessObject>();
					dummyDst.Z0_Code = "CDA";
					factoryDst.Save();

					var factorySrc = new BusinessObjectFactory();
					var dummySrc = factorySrc.Load<DummyBusinessObject>(dummyDst.PK);
					dummySrc.Z0_Code = "CDB";
					factorySrc.Save();

					published = (dummyDst.Z0_Code == "CDB");
				}
			});

			thread.Start();
			thread.Join();

			Assert(published);
		}

		[UseSnapshotProtection, GuiTest]
		public void TestPublishFromBackgroundToForegroundIsPrevented()
		{
			using (var testSyncContext = SynchronizationContextForTest.Enable())
			{
				var factoryDst = new BusinessObjectFactory();
				var dummyDst = factoryDst.New<DummyBusinessObject>();
				dummyDst.Z0_Code = "CDA";
				factoryDst.Save();

				var factorySrc = new BusinessObjectFactory();
				factorySrc.ThreadSentry.RelinquishThreadOwnership();

				Action assert;
				var thread = RunThreadWithErrorHandling(out assert, () =>
				 {
					 using (Db.DisposableActionForDbConnection())
					 {
						 factorySrc.ThreadSentry.TakeThreadOwnership();

						 var dummySrc = factorySrc.Load<DummyBusinessObject>(dummyDst.PK);
						 dummySrc.Z0_Code = "CDB";
						 factorySrc.Save();
					 }
				 });

				thread.Start();
				thread.Join();
				testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));

				var published = dummyDst.Z0_Code == "CDB";
				AssertEquals(false, published);
				assert();
			}
		}

		[UseSnapshotProtection]
		public void TestPublishFromForegroundToBackground()
		{
			var published = true;
			var pk = new ZGuid();
			int step = 1;

			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factoryDst = new BusinessObjectFactory();
					var dummyDst = factoryDst.New<DummyBusinessObject>();
					dummyDst.Z0_Code = "CDA";
					factoryDst.Save();
					pk = dummyDst.PK;

					step = 2;

					while (step != 3)
					{
						Thread.Sleep(1000);
					}

					published = (dummyDst.Z0_Code == "CDB");
				}
			});

			thread.Start();

			while (step != 2)
			{
				Thread.Sleep(1000);
			}

			var factorySrc = new BusinessObjectFactory();
			var dummySrc = factorySrc.Load<DummyBusinessObject>(pk);
			dummySrc.Z0_Code = "CDB";
			factorySrc.Save();

			step = 3;

			thread.Join();

			Assert(!published);
		}

		[UseSnapshotProtection, GuiTest]
		public void TestPublishFromBackgroundToBackground()
		{
			using (var testSyncContext = SynchronizationContextForTest.Enable())
			{
				var published = true;

				var thread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var factoryDst = new BusinessObjectFactory();
						var dummyDst = factoryDst.New<DummyBusinessObject>();
						dummyDst.Z0_Code = "CDA";
						factoryDst.Save();

						var childThread = new Thread(() =>
						{
							using (Db.DisposableActionForDbConnection())
							{
								var factorySrc = new BusinessObjectFactory();
								var dummySrc = factorySrc.Load<DummyBusinessObject>(dummyDst.PK);
								dummySrc.Z0_Code = "CDB";
								factorySrc.Save();
							}
						});

						childThread.Start();
						childThread.Join();

						published = (dummyDst.Z0_Code == "CDB");
					}
				});

				thread.Start();

				testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));

				Assert(!published);
			}
		}

		#endregion

		#region Delete

		[UseSnapshotProtection]
		public void TestDeleteFromForegroundToBackground()
		{
			var deleted = false;
			int step = 1;

			var thread = new Thread(() =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factoryDst = new BusinessObjectFactory();
					var dummyDst = factoryDst.New<DummyBusinessObject>();
					dummyDst.Delete();
					factoryDst.Save();

					step = 2;

					while (step != 3)
					{
						Thread.Sleep(1000);
					}

					deleted = dummyDst.IsDeleted;
					Assert(deleted);
				}
			});

			thread.Start();

			while (step != 2)
			{
				Thread.Sleep(1000);
			}

			step = 3;

			Assert(!deleted);
			thread.Join();

			Assert(deleted);
		}

		[UseSnapshotProtection, SnailTest, GuiTest]
		public void TestDeleteFromBackgroundToForeground()
		{
			using (var testSyncContext = SynchronizationContextForTest.Enable())
			{
				var factoryDst = new BusinessObjectFactory();
				var dummyDst = factoryDst.New<DummyBusinessObject>();
				factoryDst.Save();

				var factorySrc = new BusinessObjectFactory();
				factorySrc.ThreadSentry.RelinquishThreadOwnership();

				var thread = new Thread(() =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						factorySrc.ThreadSentry.TakeThreadOwnership();

						var dummySrc = factorySrc.Load<DummyBusinessObject>(dummyDst.PK);
						dummySrc.Delete();
						Assert(dummySrc.IsDeleted);
						factorySrc.Save();
					}
				});

				thread.Start();
				thread.Join();

				testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));

				AssertEquals("DataRefreshBus doens't post betwen threads anymore.", false, dummyDst.IsDeleted);
			}
		}

		[UseSnapshotProtection, GuiTest]
		public void TestLoadManyToManyInBackgroundSaveDependentInForeground()
		{
			Action assert;
			using (var testSyncContext = SynchronizationContextForTest.Enable())
			using (var foregroundWaitEvent = new AutoResetEvent(false))
			using (var backgroundWaitEvent = new AutoResetEvent(false))
			using (Db.DisposableActionForDbConnection())
			{
				DummyBusinessObject master = null;
				var thread = RunThreadWithErrorHandling(out assert, () =>
				{
					using (Db.DisposableActionForDbConnection())
					using (var threadTestSyncContext = SynchronizationContextForTest.Enable())
					{
						BusinessObjectFactory factory = new BusinessObjectFactory();
						master = factory.New<DummyBusinessObject>();
						factory.Save();

						DummyMToNCollection collection = new DummyMToNCollection(master, null);
						master.RegisterEditableChildObject(collection);
						collection.Load();
						foregroundWaitEvent.Set();

						//Waiting for save in foreground
						backgroundWaitEvent.WaitOne(A_NICE_AMOUNT_TO_WAIT);
						testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));
						foregroundWaitEvent.Set();

						//Waiting for IsMatching callback
						backgroundWaitEvent.WaitOne(A_NICE_AMOUNT_TO_WAIT);
						testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));
						foregroundWaitEvent.Set();
					}
				});
				thread.Start();

				try
				{
					Assert("Waiting for background load", foregroundWaitEvent.WaitOne(A_NICE_AMOUNT_TO_WAIT));
					BusinessObjectFactory childFactory = new BusinessObjectFactory();
					DummyDependantBusinessObject child = childFactory.New<DummyDependantBusinessObject>();
					child.ZD1_Z0 = master.PK;
					childFactory.Save();
					backgroundWaitEvent.Set();

					Assert("Waiting for IsMatching check", foregroundWaitEvent.WaitOne(A_NICE_AMOUNT_TO_WAIT));
					testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));
					backgroundWaitEvent.Set();
				}
				finally
				{
					thread.Join();
				}
			}
			assert();
		}

		[UseSnapshotProtection, GuiTest]
		public void TestLoadManyToManyInForegroundSaveDependentInBackground()
		{
			Action assert;
			using (var testSyncContext = SynchronizationContextForTest.Enable())
			using (var foregroundWaitEvent = new AutoResetEvent(false))
			using (var backgroundWaitEvent = new AutoResetEvent(false))
			using (Db.DisposableActionForDbConnection())
			{
				BusinessObjectFactory childFactory = null;
				BusinessObjectFactory factory = new BusinessObjectFactory();
				var master = factory.New<DummyBusinessObject>();
				factory.Save();

				DummyMToNCollection collection = new DummyMToNCollection(master, null);
				master.RegisterEditableChildObject(collection);
				collection.Load();

				var thread = RunThreadWithErrorHandling(out assert, () =>
				 {
					 using (Db.DisposableActionForDbConnection())
					 using (var threadTestSyncContext = SynchronizationContextForTest.Enable())
					 {
						 childFactory = new BusinessObjectFactory();
						 DummyDependantBusinessObject child = childFactory.New<DummyDependantBusinessObject>();
						 child.ZD1_Z0 = master.PK;
						 childFactory.Save();
						 foregroundWaitEvent.Set();

						 //Waiting for IsMatching check
						 backgroundWaitEvent.WaitOne(A_NICE_AMOUNT_TO_WAIT);
						 testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));
						 foregroundWaitEvent.Set();
					 }
				 });

				thread.Start();
				try
				{
					Assert("Waiting for save in background", foregroundWaitEvent.WaitOne(A_NICE_AMOUNT_TO_WAIT));
					testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));
					backgroundWaitEvent.Set();

					Assert("Waiting for IsMatching callback", foregroundWaitEvent.WaitOne(A_NICE_AMOUNT_TO_WAIT));
					testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));
				}
				finally
				{
					thread.Join();
				}
				assert();
			}
		}

		[UseSnapshotProtection]
		public void TestLoadManyToMany()
		{
			using (Db.DisposableActionForDbConnection())
			{
				var factory1 = new BusinessObjectFactory();
				factory1.Save();

				var assosciatedObject = factory1.New<DummyBusinessObject>();
				var child0 = factory1.New<DummyDependantBusinessObject>();
				CreateNewPivot(factory1, assosciatedObject.PK, child0.PK);
				factory1.Save();

				using (var testSyncContext = SynchronizationContextForTest.Enable())
				{
					var factory2 = new BusinessObjectFactory();
					var copyOfBase = factory2.Load<DummyBaseBusinessObject>(assosciatedObject.PK);
					var collection2 = new DummyMToNCollection(copyOfBase);
					copyOfBase.RegisterEditableChildObject(collection2);
					collection2.Load();

					AssertEquals("IsLoaded", true, collection2.IsLoaded);
					AssertEquals("Count", 1, collection2.Count);

					var child = factory1.New<DummyDependantBusinessObject>();
					CreateNewPivot(factory1, assosciatedObject.PK, child.PK);
					factory1.Save();

					AssertEquals("Count", 2, collection2.Count);

					var child2 = factory1.New<DummyDependantBusinessObject>();
					CreateNewPivot(factory1, assosciatedObject.PK, child2.PK);
					factory1.Save();

					AssertEquals("Count", 3, collection2.Count);
				}
			}
		}

		Thread RunThreadWithErrorHandling(out Action assert, Action a)
		{
			Exception exception = null;
			assert = () => AssertNull(exception);
			return new Thread(() =>
			{
				try
				{
					a();
				}
				catch (Exception e)
				{
					exception = e;
				}
			});
		}

		const int A_NICE_AMOUNT_TO_WAIT = 100000;

		DummyPivot CreateNewPivot(BusinessObjectFactory factory, ZGuid zDP_Z0, ZGuid zDP_ZD1)
		{
			DummyPivot pivot = factory.New<DummyPivot>();
			pivot.ZDP_Z0 = zDP_Z0;
			pivot.ZDP_ZD1 = zDP_ZD1;
			pivot.HasChanges = false;

			return pivot;
		}

		[UseSnapshotProtection, GuiTest]
		public void TestDeleteFromBackgroundToBackground()
		{
			using (var testSyncContext = SynchronizationContextForTest.Enable())
			{
				var deleted = true;
				Action assert;
				Action otherAssert = null;
				var thread = RunThreadWithErrorHandling(out assert, () =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						var factoryDst = new BusinessObjectFactory();
						var dummyDst = factoryDst.New<DummyBusinessObject>();
						dummyDst.Z0_Code = "CDA";
						factoryDst.Save();

						var childThread = RunThreadWithErrorHandling(out otherAssert, () =>
						{
							using (Db.DisposableActionForDbConnection())
							{
								var factorySrc = new BusinessObjectFactory();
								var dummySrc = factorySrc.Load<DummyBusinessObject>(dummyDst.PK);
								dummySrc.Delete();
								factorySrc.Save();
							}
						});

						childThread.Start();
						childThread.Join();

						deleted = dummyDst.IsDeleted;
						Assert(!deleted);
					}
				});

				thread.Start();

				testSyncContext.WaitAndExecuteCallbacks(TimeSpan.FromSeconds(1));

				Assert(!deleted);
				assert();
				otherAssert();
			}
		}

		[UseSnapshotProtection]
		public void TestDeleteInBackground()
		{
			var deleted = false;

			Action assert;
			var thread = RunThreadWithErrorHandling(out assert, () =>
			{
				using (Db.DisposableActionForDbConnection())
				{
					var factoryDst = new BusinessObjectFactory();
					var dummyDst = factoryDst.New<DummyBusinessObject>();
					dummyDst.Z0_Code = "CDA";
					factoryDst.Save();

					var factorySrc = new BusinessObjectFactory();
					var dummySrc = factorySrc.Load<DummyBusinessObject>(dummyDst.PK);
					dummySrc.Delete();
					factorySrc.Save();

					deleted = dummyDst.IsDeleted;
				}
			});

			thread.Start();
			thread.Join();

			Assert(deleted);
			assert();
		}

		#endregion

		protected override void SetUp()
		{
			GC.Collect();
			Db.Connection.ExecuteNonQuery(
				"DELETE FROM dbo.DummyBizo WHERE Z0_Code = @code1 OR Z0_Code = @code2",
				cmd =>
				{
					cmd.AddParameterBasedOnDbColumn("@code1", "CDA", DummyBizoSchema.Z0_Code);
					cmd.AddParameterBasedOnDbColumn("@code2", "CDB", DummyBizoSchema.Z0_Code);
				});
		}
	}
}
