using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppDomainWrappers.Net;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Core.Environment.Internal;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	sealed class RegistryItemDictionaryTest : TransactionedTestCase
	{
		public void TestAddGetItemAndUpdate()
		{
			AssertNull("GetItem(\"x\")", Dictionary.GetItem("x"));
			AssertNull("GetItem(\"y\")", Dictionary.GetItem("y"));

			StringRegistryItem item1 = new StringRegistryItem("x", null, null, null, RegistryStorageFlags.System);
			StringRegistryItem item2 = new StringRegistryItem("y", null, null, null, RegistryStorageFlags.System);

			AssertNull("GetItem(\"x\")", Dictionary.GetItem("x"));
			AssertNull("GetItem(\"y\")", Dictionary.GetItem("y"));

			Dictionary.Add(item1);
			Dictionary.Add(item2);

			AssertEquals("GetItem(\"x\")", item1, Dictionary.GetItem("x"));
			AssertEquals("GetItem(\"y\")", item2, Dictionary.GetItem("y"));

			RegistryItemHolder holder1 = DictionaryInternals.GetHolder("x");
			RegistryItemHolder holder2 = DictionaryInternals.GetHolder("y");

			Assert(holder1.ElapsedSinceLastUse.TotalSeconds < 1);
			Assert(holder2.ElapsedSinceLastUse.TotalSeconds < 1);

			TimeSpan span = new TimeSpan(0, 5, 0);
			holder1.ElapsedSinceLastUse = span;
			holder2.ElapsedSinceLastUse = span;

			AssertEquals("GetItem(\"x\") to update ElapsedSinceLastUse.", item1, Dictionary.GetItem("x"));
			Assert("GetHolder(\"x\").ElapsedSinceLastUse", holder1.ElapsedSinceLastUse.TotalSeconds < 1);
			AssertEquals("GetHolder(\"y\").ElapsedSinceLastUse", span, holder2.ElapsedSinceLastUse);
		}

		public void TestInstance()
		{
			var instance = RegistryItemDictionary.Instance;
			AssertEquals("Instance should be cached.", instance, RegistryItemDictionary.Instance);
			var task = new Task(() => AssertEquals("Instance should be cached.", instance, RegistryItemDictionary.Instance));
			task.Start();
			task.Wait();
		}

		public void TestPurge()
		{
			StringRegistryItem item1 = new StringRegistryItem("x", null, null, null, RegistryStorageFlags.System);
			StringRegistryItem item2 = new StringRegistryItem("y", null, null, null, RegistryStorageFlags.System);
			Dictionary.Add(item1);
			Dictionary.Add(item2);
			DictionaryInternals.Purge("x");
			AssertEquals("Count", 1, DictionaryInternals.Count);
			AssertNotNull("GetHolder(\"y\")", DictionaryInternals.GetHolder("y"));
		}

		public void TestPurgeAll()
		{
			StringRegistryItem item = new StringRegistryItem("x", null, null, null, RegistryStorageFlags.System);
			Dictionary.Add(item);
			AssertEquals("Count", 1, DictionaryInternals.Count);
			Dictionary.PurgeAll();
			AssertEquals("Count", 0, DictionaryInternals.Count);
		}

		public void TestPurgeAllIfUpdatedByUser()
		{
			StringRegistryItem item = new StringRegistryItem("x", null, null, null, RegistryStorageFlags.System);
			Dictionary.Add(item);
			AssertEquals("Count", 1, DictionaryInternals.Count);
			RawDataRegistry rawRegistry = new RawDataRegistry();
			int currentVersion = rawRegistry.RegistryUserUpdateVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty);
			Dictionary.PurgeAllIfUpdatedByUser();
			AssertEquals("Registry Version should be the same", currentVersion, rawRegistry.RegistryUserUpdateVersion.GetValueWithoutFallback(Guid.Empty, Guid.Empty, Guid.Empty));
			AssertEquals("Was not purged because Version still the same", 1, DictionaryInternals.Count);

			rawRegistry.RegistryUserUpdateVersion.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, currentVersion + 1);
			Dictionary.PurgeAllIfUpdatedByUser();
			AssertEquals("Should be purged after Registry Version has been changed", 0, DictionaryInternals.Count);
		}

		public void TestPurgeExpired()
		{
			for (var i = 0; i < DictionaryInternals.ItemThreshold; i++)
			{
				Dictionary.Add(new StringRegistryItem("DUMMY_ITEM_" + i, null, null, null, RegistryStorageFlags.System));
			}

			Dictionary.Add(new StringRegistryItem("Bow", null, null, null, RegistryStorageFlags.System));
			Dictionary.Add(new StringRegistryItem("Wow", null, null, null, RegistryStorageFlags.System));

			Dictionary.PurgeExpired();
			AssertEquals("Count", DictionaryInternals.ItemThreshold + 2, DictionaryInternals.Count);

			DictionaryInternals.GetHolder("DUMMY_ITEM_23").ElapsedSinceLastUse = new TimeSpan(0, DictionaryInternals.TimeoutMinutes - 4, 0);
			DictionaryInternals.GetHolder("DUMMY_ITEM_9").ElapsedSinceLastUse = new TimeSpan(0, DictionaryInternals.TimeoutMinutes - 3, 0);
			DictionaryInternals.GetHolder("DUMMY_ITEM_15").ElapsedSinceLastUse = new TimeSpan(0, DictionaryInternals.TimeoutMinutes - 2, 0);
			DictionaryInternals.GetHolder("DUMMY_ITEM_0").ElapsedSinceLastUse = new TimeSpan(0, DictionaryInternals.TimeoutMinutes - 1, 0);
			DictionaryInternals.GetHolder("DUMMY_ITEM_5").ElapsedSinceLastUse = new TimeSpan(0, DictionaryInternals.TimeoutMinutes + 1, 0);
			DictionaryInternals.GetHolder("DUMMY_ITEM_4").ElapsedSinceLastUse = new TimeSpan(0, DictionaryInternals.TimeoutMinutes + 2, 0);
			DictionaryInternals.GetHolder("Bow").ElapsedSinceLastUse = new TimeSpan(0, DictionaryInternals.TimeoutMinutes + 3, 0);

			Dictionary.PurgeExpired();
			AssertEquals("Count", DictionaryInternals.ItemThreshold, DictionaryInternals.Count);

			AssertNotNull("GetHolder(\"DUMMY_ITEM_23\")", DictionaryInternals.GetHolder("DUMMY_ITEM_23"));
			AssertNotNull("GetHolder(\"DUMMY_ITEM_9\")", DictionaryInternals.GetHolder("DUMMY_ITEM_9"));
			AssertNotNull("GetHolder(\"DUMMY_ITEM_15\")", DictionaryInternals.GetHolder("DUMMY_ITEM_15"));
			AssertNotNull("GetHolder(\"DUMMY_ITEM_0\")", DictionaryInternals.GetHolder("DUMMY_ITEM_0"));
			AssertNotNull("GetHolder(\"DUMMY_ITEM_5\")", DictionaryInternals.GetHolder("DUMMY_ITEM_5"));
			AssertNotNull("GetHolder(\"Wow\")", DictionaryInternals.GetHolder("Wow"));

			AssertNull("GetHolder(\"DUMMY_ITEM_4\")", DictionaryInternals.GetHolder("DUMMY_ITEM_4"));
			AssertNull("GetHolder(\"Bow\")", DictionaryInternals.GetHolder("Bow"));
		}

		[ExpectNoExceptions]
		public void TestPurgeExpiredMultiThread()
		{
			var endTime = DateTime.Now + TimeSpan.FromSeconds(5);
			var i = 0;

			void AddItems()
			{
				using (Db.DisposableActionForDbConnection())
				{
					while (DateTime.Now < endTime)
					{
						Dictionary.Add(new StringRegistryItem("DUMMY_ITEM_" + Interlocked.Increment(ref i), null, null, null, RegistryStorageFlags.System));
					}
				}
			}

			var threads = Enumerable.Range(0, 10).Select(_ => new Thread(AddItems)).ToArray();
			threads.ForEach(t => t.Start());

			while (DateTime.Now < endTime)
			{
				Dictionary.PurgeExpired();
				Thread.Sleep(15);
			}

			threads.ForEach(t => t.Join());
		}

		public void TestPurgeIfOlderThan()
		{
			var span = new TimeSpan(0, 0, 1);

			StringRegistryItem item1 = new StringRegistryItem("DUMMY_ITEM_1", null, null, null, RegistryStorageFlags.System);
			Dictionary.Add(item1);
			StringRegistryItem item2 = new StringRegistryItem("DUMMY_ITEM_2", null, null, null, RegistryStorageFlags.System);
			Dictionary.Add(item2);

			Dictionary.PurgeIfOlderThan(item1.Name, span);
			AssertNotNull("First item should not be purged.", Dictionary.GetItem(item1.Name));
			Dictionary.PurgeIfOlderThan(item2.Name, span);
			AssertNotNull("Second item should not be purged.", Dictionary.GetItem(item2.Name));

			DictionaryInternals.GetHolder(item1.Name).ElapsedSinceLastUse = span;
			Dictionary.PurgeIfOlderThan(item1.Name, span);
			AssertNull("First item should be purged.", Dictionary.GetItem(item1.Name));

			DictionaryInternals.GetHolder(item2.Name).ElapsedSinceLastUse = span;
			Dictionary.PurgeIfOlderThan(item2.Name, TimeSpan.Zero);
			AssertNull("Second item should be purged.", Dictionary.GetItem(item2.Name));
		}

		public void TestMultipleThreadsDoesNotCrash()
		{
			Thread[] threads = new Thread[10];

			for (int i = 0; i < threads.Length; i++)
			{
				ThreadStart threadStart = () =>
				{
					using (Db.DisposableActionForDbConnection())
					{
						BashDictionary();
					}
				};

				threads[i] = new Thread(threadStart);
			}
			RunMultiThreadTest(threads);
		}

		[UseSnapshotProtection]
		public void TestNoTypeInitializerException()
		{
#pragma warning disable CW1157 // WI00669071 - Do not use System.AppDomain.
			var domainData = new Dictionary<string, object>
			{
				{ "ServerName", Db.ServerName },
				{ "DatabaseName", Db.DatabaseName },
			};

			using (var appDomainWrapper = new AppDomainWrapper())
			{
				using (var adminConnection = Db.NewAdminConnection())
				{
					AssertEquals(DbLockoutState.AquiredLockout, adminConnection.AcquireLockout());
					try
					{
						appDomainWrapper.RunActionInAppDomain(() =>
						{
							Db.InitializeDatabaseDetails(
								(string)AppDomain.CurrentDomain.GetData("ServerName"),
								(string)AppDomain.CurrentDomain.GetData("DatabaseName"));

							EnterpriseApplicationConfiguration.ConfigureObjectFactory();
							AssertExceptionThrown(typeof(DatabaseUpgradeInProgressException), () => RegistryItemDictionary.Instance.GetItem("test"));
						}, domainData);
					}
					finally
					{
						adminConnection.ResetLockout();
					}

					appDomainWrapper.RunActionInAppDomain(() =>
					{
						AssertNoExceptionThrown(() => RegistryItemDictionary.Instance.GetItem("test"));
					}, domainData);
				}
			}
#pragma warning restore CW1157 // WI00669071 - Do not use System.AppDomain.
		}

		#region Implementation

		void RunMultiThreadTest(Thread[] threads)
		{
			try
			{
				foreach (Thread thread in threads)
				{
					thread.Start();
				}
			}
			finally
			{
				foreach (Thread thread in threads)
				{
					thread.Join();
				}
			}

			lock (syncRoot)
			{
				if (lastThreadException == null)
				{
					Assert(true);
				}
				else
				{
					Fail("An exception occurred in one of the threads: " + lastThreadException.ToString());
				}
			}
		}

		void BashDictionary()
		{
			try
			{
				for (int i = 0; i < 10; i++)
				{
					RegistryItemDictionary.Instance.GetItem("x");
					RegistryItemDictionary.Instance.Add(new StringRegistryItem("x", null, null, null, RegistryStorageFlags.System));
					RegistryItemDictionary.Instance.PurgeExpired();
					RegistryItemDictionary.Instance.Add(new StringRegistryItem("y", null, null, null, RegistryStorageFlags.System));
					RegistryItemDictionary.Instance.PurgeAll();
					RegistryItemDictionary.Instance.GetItem("y");
				}
			}
			catch (Exception ex) when (!ex.IsCriticalException())
			{
				lock (syncRoot)
				{
					lastThreadException = ex;
				}
			}
		}

		RegistryItemDictionary Dictionary
		{
			get { return dictionary ?? (dictionary = (RegistryItemDictionary)Activator.CreateInstance(typeof(RegistryItemDictionary), true)); }
		}

		IRegistryItemDictionaryInternals DictionaryInternals
		{
			get { return Dictionary; }
		}

		RegistryItemDictionary dictionary;
		Exception lastThreadException;
		readonly object syncRoot = new object();

		protected override void SetUp()
		{
			base.SetUp();
			dictionary = null;
			lastThreadException = null;
		}

		#endregion
	}
}
