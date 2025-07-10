using System.Collections.Generic;
using NUnit.Framework;

namespace CargoWise.Loader.Common.Testing
{
	class InstallationItemCollectionTest : TestCase
	{
		InstallationItemCollection TestList;

		protected override void SetUp()
		{
			base.SetUp();
			TestList = new InstallationItemCollection();
		}

		public void TestAddAndEnumerator()
		{
			List<DummyInstallationItem> items = new List<DummyInstallationItem>();
			items.Add(new DummyInstallationItem(""));
			items.Add(new DummyInstallationItem(""));
			items.Add(new DummyInstallationItem(""));

			TestList.Add(items[0]);
			TestList.Add(items[1]);
			TestList.Add(items[2]);

			int count = 0;
			foreach (InstallationItem item in TestList)
			{
				AssertEquals(count.ToString(), items[count], item);
				count++;
			}

			AssertEquals("Count", 3, count);
		}

		public void TestRecursiveDepthFirstDependencyEnumerator()
		{
			// Set up a hierarchy with a few levels
			TestList.Add(new DummyInstallationItem("first"));
			TestList.Add(new DummyInstallationItem("second"));
			TestList.Add(new DummyInstallationItem("third"));
			TestList.Add(new DummyInstallationItem("fourth"));
			TestList[2].Dependencies.Add(new DummyInstallationItem("third/first"));
			TestList[2].Dependencies.Add(new DummyInstallationItem("third/second"));
			TestList[2].Dependencies.Add(new DummyInstallationItem("third/third"));
			TestList[0].Dependencies.Add(new DummyInstallationItem("first/first"));
			TestList[0].Dependencies.Add(new DummyInstallationItem("first/second"));
			TestList[2].Dependencies[1].Dependencies.Add(new DummyInstallationItem("third/second/first"));
			TestList[2].Dependencies[1].Dependencies.Add(new DummyInstallationItem("third/second/second"));
			TestList[2].Dependencies[1].Dependencies[1].Dependencies.Add(new DummyInstallationItem("third/second/second/first"));

			int count = 0;
			foreach (DummyInstallationItem dummy in TestList.GetDepthFirstEnumerable())
			{
				switch (count)
				{
					case 0:
						AssertEquals("first/first", dummy.DummyString);
						break;
					case 1:
						AssertEquals("first/second", dummy.DummyString);
						break;
					case 2:
						AssertEquals("first", dummy.DummyString);
						break;
					case 3:
						AssertEquals("second", dummy.DummyString);
						break;
					case 4:
						AssertEquals("third/first", dummy.DummyString);
						break;
					case 5:
						AssertEquals("third/second/first", dummy.DummyString);
						break;
					case 6:
						AssertEquals("third/second/second/first", dummy.DummyString);
						break;
					case 7:
						AssertEquals("third/second/second", dummy.DummyString);
						break;
					case 8:
						AssertEquals("third/second", dummy.DummyString);
						break;
					case 9:
						AssertEquals("third/third", dummy.DummyString);
						break;
					case 10:
						AssertEquals("third", dummy.DummyString);
						break;
					case 11:
						AssertEquals("fourth", dummy.DummyString);
						break;
					default:
						Fail("Too many objects returned");
						break;
				}
				count++;
			}
			AssertEquals("Should have enumerated all objects", 12, count);
		}

		public void TestClear()
		{
			TestCount();
			AssertEquals("Count before clear", 2, TestList.Count);
			TestList.Clear();
			AssertEquals("Count after clear", 0, TestList.Count);
		}

		[ExpectNoExceptions]
		public void TestEnumeratorWhenEmpty()
		{
			foreach (InstallationItem item in TestList)
			{
				Fail("Should not have returned any items");
				Equals(item, item); // suppress warning about Item not being used
			}
		}

		public void TestCount()
		{
			AssertEquals(0, TestList.Count);
			TestList.Add(new DummyInstallationItem(""));
			AssertEquals(1, TestList.Count);
			TestList.Add(new DummyInstallationItem(""));
			AssertEquals(2, TestList.Count);
		}

		public void TestIndexer()
		{
			InstallationItem item1 = new DummyInstallationItem("");
			InstallationItem item2 = new DummyInstallationItem("");

			TestList.Add(item1);
			TestList.Add(item2);

			AssertSame(item1, TestList[0]);
			AssertSame(item2, TestList[1]);
		}
	}
}
