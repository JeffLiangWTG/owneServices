using System;
using System.IO;
using System.Text;
using System.Threading;
using CargoWise.Data;
using CargoWise.Data.Testing;
using Enterprise.Integration;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Environment.Testing
{
	public class GridLayoutRegistryTest : TransactionedTestCase
	{
		public void TestGridLayoutRegistry()
		{
			GridLayoutRegistry registry = new GridLayoutRegistry();
			byte[] testBytes = Encoding.ASCII.GetBytes("test");
			AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", Guid.Empty));

			registry.SetGridLayout("TestGrid", Guid.Empty, new MemoryStream(testBytes));
			AssertEquals("HasLayoutInDB", true, registry.HasLayoutInDB("TestGrid", Guid.Empty));

			byte[] retrievedBytes = registry.GetGridLayout("TestGrid", Guid.Empty).ToArray();
			AssertEquals("GetGridLayout", "test", Encoding.ASCII.GetString(retrievedBytes));
		}

		public void TestWithGridLayoutNull()
		{
			GridLayoutRegistry registry = new GridLayoutRegistryWithGridLayoutNull();
			byte[] testBytes = Encoding.ASCII.GetBytes("test");
			AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", Guid.Empty));

			registry.SetGridLayout("TestGrid", Guid.Empty, new MemoryStream(testBytes));
			AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", Guid.Empty));
			AssertEquals("GetGridLayout", null, registry.GetGridLayout("TestGrid", Guid.Empty));
		}

		public void TestDeleteValue_NoValueInRegistry()
		{
			GridLayoutRegistry registry = new GridLayoutRegistry();
			AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", Guid.Empty));
			registry.DeleteLayout("TestGrid", Guid.Empty);
		}

		public void TestDeleteValue()
		{
			Guid categoryPK = Guid.NewGuid();
			GridLayoutRegistry registry = new GridLayoutRegistry();
			AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", Guid.Empty));
			AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", categoryPK));

			byte[] testBytes = Encoding.ASCII.GetBytes("test");
			using (MemoryStream ms = new MemoryStream(testBytes))
			{
				registry.SetGridLayout("TestGrid", Guid.Empty, ms);
				AssertEquals("HasLayoutInDB", true, registry.HasLayoutInDB("TestGrid", Guid.Empty));
				AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", categoryPK));

				registry.SetGridLayout("TestGrid", categoryPK, ms);
				AssertEquals("HasLayoutInDB", true, registry.HasLayoutInDB("TestGrid", Guid.Empty));
				AssertEquals("HasLayoutInDB", true, registry.HasLayoutInDB("TestGrid", categoryPK));
			}

			registry.DeleteLayout("TestGrid", Guid.Empty);
			AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", Guid.Empty));
			AssertEquals("HasLayoutInDB", true, registry.HasLayoutInDB("TestGrid", categoryPK));

			registry.DeleteLayout("TestGrid", categoryPK);
			AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", Guid.Empty));
			AssertEquals("HasLayoutInDB", false, registry.HasLayoutInDB("TestGrid", categoryPK));

			AssertNull("Removed from Cache", registry.GetGridLayout("TestGrid"));
			AssertNull("Removed from Cache", registry.GetGridLayout("TestGrid", categoryPK));
		}

		protected class GridLayoutRegistryWithGridLayoutNull : GridLayoutRegistry
		{
			protected override IRegistryItem GridLayout
			{
				get { return null; }
			}
		}

		public void TestCategoryPK()
		{
			Guid category1 = Guid.NewGuid();
			Guid category2 = Guid.NewGuid();
			GridLayoutRegistry registry = new GridLayoutRegistry();

			byte[] testBytes1 = Encoding.ASCII.GetBytes("test1");
			registry.SetGridLayout("TestGrid", category1, new MemoryStream(testBytes1));
			byte[] testBytes2 = Encoding.ASCII.GetBytes("test2");
			registry.SetGridLayout("TestGrid", category2, new MemoryStream(testBytes2));

			byte[] retrievedBytes1 = registry.GetGridLayout("TestGrid", category1).ToArray();
			AssertEquals("GetGridLayout", "test1", Encoding.ASCII.GetString(retrievedBytes1));
			byte[] retrievedBytes2 = registry.GetGridLayout("TestGrid", category2).ToArray();
			AssertEquals("GetGridLayout", "test2", Encoding.ASCII.GetString(retrievedBytes2));
		}

		public void TestUsingCache()
		{
			GridLayoutRegistry registry = new GridLayoutRegistry();
			Guid contextPK = Guid.NewGuid();
			byte[] testBytes1 = Encoding.ASCII.GetBytes("test1");
			string gridName = "TestCachedGridData";

			registry.SetGridLayout(gridName, contextPK, new MemoryStream(testBytes1));
			byte[] retrievedBytes1 = registry.GetGridLayout(gridName, contextPK).ToArray();
			AssertEquals("GetGridLayout", "test1", Encoding.ASCII.GetString(retrievedBytes1));

			GridLayoutRegistry anotherRegistry = new GridLayoutRegistry();
			byte[] testBytes2 = Encoding.ASCII.GetBytes("test2");
			anotherRegistry.SetGridLayout(gridName, contextPK, new MemoryStream(testBytes2));

			retrievedBytes1 = registry.GetGridLayout(gridName, contextPK).ToArray();
			AssertEquals("GetGridLayout should retrieve cached value", "test1", Encoding.ASCII.GetString(retrievedBytes1));
		}

		[UseSnapshotProtection]
		public void TestUsingCacheFromThreads()
		{
			AssertNotNull("TestRegistry", TestRegistry);

			Thread thread1 = new Thread(new ThreadStart(ThreadProcForTest));
			Thread thread2 = new Thread(new ThreadStart(ThreadProcForTest));
			Thread thread3 = new Thread(new ThreadStart(ThreadProcForTest));

			thread1.Start();
			thread2.Start();
			thread3.Start();

			thread1.Join();
			thread2.Join();
			thread3.Join();

			AssertNotNull("TestRegistry", TestRegistry);
		}

		#region MultiThread access implementation

		GridLayoutRegistry TestRegistry
		{
			get
			{
				if (fTestRegistry == null)
				{
					fTestRegistry = new GridLayoutRegistry();
				}
				return fTestRegistry;
			}
		}
		GridLayoutRegistry fTestRegistry;

		void ThreadProcForTest()
		{
			using (Db.DisposableActionForDbConnection())
			{
				Guid contextPK = Guid.NewGuid();
				string gridName = "TestCachedGridData";

				for (int i = 0; i < 500; i++)
				{
					byte[] testBytes = Encoding.ASCII.GetBytes("test" + i.ToString());
					TestRegistry.SetGridLayout(gridName, contextPK, new MemoryStream(testBytes));
					Thread.Sleep(0);
					byte[] retrievedBytes = TestRegistry.GetGridLayout(gridName, contextPK).ToArray();
				}
			}
		}

		#endregion

	}
}
