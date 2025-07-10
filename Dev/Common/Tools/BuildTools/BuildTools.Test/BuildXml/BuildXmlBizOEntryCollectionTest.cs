using NUnit.Framework;

namespace CargoWise.BuildTools.Testing
{
	sealed class BuildXmlBizOEntryCollectionTest : TestCase
	{
		public void TestAdd()
		{
			AssertEquals("Pre-Condition", Collection.Count, 0);

			Collection.Add(Entry1);
			AssertEquals("BuildXmlBizOEntryCollection.Add()", Collection.Count, 1);
			AssertEquals("BuildXmlBizOEntryCollection.Add()", Entry1, Collection[Entry1.TableName]);
		}

		public void TestRemove()
		{
			Collection.Add(Entry1);
			AssertEquals("Pre-Condition", Collection.Count, 1);
			AssertEquals("Pre-Condition", Entry1, Collection[Entry1.TableName]);

			Collection.Remove(Entry1);
			AssertEquals("BuildXmlBizOEntryCollection.Remove()", Collection.Count, 0);
			AssertEquals("BuildXmlBizOEntryCollection.Remove()", null, Collection[Entry1.TableName]);
		}

		public void TestMasterFileReference()
		{
			Collection.Add(Entry1);
			Collection.Add(Entry2);

			AssertEquals(false, Collection[Entry1.TableName].MasterFileReference);
			AssertEquals(true, Collection[Entry2.TableName].MasterFileReference);
		}

		public void TestRemoveViaTableName()
		{
			Collection.Add(Entry1);
			AssertEquals("Pre-Condition", Collection.Count, 1);
			AssertEquals("Pre-Condition", Entry1, Collection[Entry1.TableName]);

			Collection.Remove(Entry1.TableName);
			AssertEquals("BuildXmlBizOEntryCollection.Remove()", Collection.Count, 0);
			AssertEquals("BuildXmlBizOEntryCollection.Remove()", null, Collection[Entry1.TableName]);
		}

		public void TestIndexer()
		{
			AssertEquals("Pre-Condition", Collection.Count, 0);

			AssertEquals("BuildXmlBizOEntryCollection.Add()", null, null);

			Collection.Add(Entry1);
			AssertEquals("BuildXmlBizOEntryCollection.Add()", Entry1, Collection[Entry1.TableName]);
		}

		public void TestContains()
		{
			AssertEquals("Pre-Condition", Collection.Count, 0);
			Assert("Pre-Condition", !Collection.Contains(Entry1.TableName));

			Collection.Add(Entry1);
			Assert("BuildXmlBizOEntryCollection.Contains()", Collection.Contains(Entry1.TableName));
			Assert("BuildXmlBizOEntryCollection.Contains()", !Collection.Contains(null));
			Assert("BuildXmlBizOEntryCollection.Contains()", !Collection.Contains("espresso"));
		}

		public void TestCount()
		{
			AssertEquals("Pre-Condition", Collection.Count, 0);

			Collection.Add(Entry1);
			AssertEquals("BuildXmlBizOEntryCollection.Add()", Collection.Count, 1);

			Collection.Remove(Entry1);
			AssertEquals("BuildXmlBizOEntryCollection.Add()", Collection.Count, 0);
		}

		public void TestEnumerating()
		{
			Collection.Add(Entry1);
			Collection.Add(Entry2);

			BuildXmlBizOEntryCollection newCollection = new BuildXmlBizOEntryCollection();
			foreach (BuildXmlBizOEntry entry in Collection)
			{
				newCollection.Add(entry);
			}

			AssertEquals("BuildXmlBizOEntryCollection Enumeration", Collection.Count, 2);
			Assert("BuildXmlBizOEntryCollection Enumeration", Collection.Contains(Entry1.TableName));
			Assert("BuildXmlBizOEntryCollection Enumeration", Collection.Contains(Entry2.TableName));
		}

		#region Implementation

		protected override void SetUp()
		{
			base.SetUp();

			Collection = new BuildXmlBizOEntryCollection();
			Entry1 = new BuildXmlBizOEntryOnMainDbForTesting("Entry1", "Solution1", false, true, false);
			Entry2 = new BuildXmlBizOEntryOnMainDbForTesting("Entry2", "Solution2", true, true, false);
		}

		BuildXmlBizOEntryCollection Collection;
		BuildXmlBizOEntry Entry1;
		BuildXmlBizOEntry Entry2;

		#endregion
	}
}
