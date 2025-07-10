using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class InstallationResultCollectionTest : TestCase
	{
		InstallationResultCollection TestList;

		protected override void SetUp()
		{
			base.SetUp();
			TestList = new InstallationResultCollection();
		}

		public void TestAddAndEnumerator()
		{
			List<InstallationResult> items = new List<InstallationResult>();
			items.Add(InstallationResult.OK());
			items.Add(InstallationResult.OK());
			items.Add(InstallationResult.OK());

			TestList.Add(items[0]);
			TestList.Add(items[1]);
			TestList.Add(items[2]);

			int count = 0;
			foreach (InstallationResult item in TestList)
			{
				AssertEquals(count.ToString(), items[count], item);
				count++;
			}

			AssertEquals("Count", 3, count);
		}

		[ExpectNoExceptions]
		public void TestEnumeratorWhenEmpty()
		{
			foreach (InstallationResult item in TestList)
			{
				Fail("Should not have returned any items");
				Equals(item, item); // suppress warning about Item not being used
			}
		}

		public void TestCount()
		{
			AssertEquals(0, TestList.Count);
			TestList.Add(InstallationResult.OK());
			AssertEquals(1, TestList.Count);
			TestList.Add(InstallationResult.Warning("warning"));
			AssertEquals(2, TestList.Count);
		}

		public void TestWarningCount()
		{
			AssertEquals(0, TestList.WarningCount);
			AddTestItems();
			AssertEquals(3, TestList.WarningCount);
		}

		public void TestErrorCount()
		{
			AssertEquals(0, TestList.ErrorCount);
			AddTestItems();
			AssertEquals(5, TestList.ErrorCount);
		}

		public void TestOKCount()
		{
			AssertEquals(0, TestList.OKCount);
			AddTestItems();
			AssertEquals(4, TestList.OKCount);
		}

		void AddTestItems()
		{
			TestList.Add(InstallationResult.OK());
			TestList.Add(InstallationResult.OK());
			TestList.Add(InstallationResult.OK());
			TestList.Add(InstallationResult.OK());

			TestList.Add(InstallationResult.Warning("warning"));
			TestList.Add(InstallationResult.Warning("warning"));
			TestList.Add(InstallationResult.Warning("warning"));

			TestList.Add(InstallationResult.Error("error"));
			TestList.Add(InstallationResult.Error("error"));
			TestList.Add(InstallationResult.Error("error"));
			TestList.Add(InstallationResult.Error("error"));
			TestList.Add(InstallationResult.Error("error"));
		}

		public void TestIndexer()
		{
			InstallationResult item1 = InstallationResult.OK();
			InstallationResult item2 = InstallationResult.OK();

			TestList.Add(item1);
			TestList.Add(item2);

			AssertSame(item1, TestList[0]);
			AssertSame(item2, TestList[1]);
		}

		public void TestGetErrorAndWarningMessages()
		{
			AssertEquals("GetErrorMessages()", string.Empty, TestList.GetErrorMessages());
			AssertEquals("GetWarningMessages()", string.Empty, TestList.GetWarningMessages());
			TestList.Add(InstallationResult.OK());
			TestList.Add(InstallationResult.Error("x"));
			TestList.Add(InstallationResult.Warning("y"));
			TestList.Add(InstallationResult.Error("z"));
			AssertEquals("GetErrorMessages()", "x\r\n\r\nz", TestList.GetErrorMessages());
			AssertEquals("GetWarningMessages()", "y", TestList.GetWarningMessages());
		}
	}
}