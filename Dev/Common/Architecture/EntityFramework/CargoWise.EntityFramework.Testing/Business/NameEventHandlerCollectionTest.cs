using System;
using CargoWise.Common;

namespace CargoWise.EntityFramework.Testing
{
	sealed class NameEventHandlerCollectionTest : TestCaseWithDummy
	{
		public NameEventHandlerCollectionTest()
		{
		}

		#region Add

		public void TestAdd()
		{
			AssertEquals("Collection is empty", 0, CollectionForTest.Count);
			CollectionForTest.Add("Test1", Handler1);

			AssertEquals("Collection has one element", 1, CollectionForTest.Count);
		}

		public void TestAddNull()
		{
			ErrorReporter.Clear();
			CollectionForTest.Add(null, Handler1);
			AssertEquals("Developer Error Reported", NameEventHandlerCollection.DeveloperErrorMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			CollectionForTest.Add("Test", null);
			AssertEquals("Developer Error Reported", NameEventHandlerCollection.DeveloperErrorMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
			CollectionForTest.Add(null, null);
			AssertEquals("Developer Error Reported", NameEventHandlerCollection.DeveloperErrorMessage, ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		#endregion

		#region Indexers

		public void TestStringIndexer()
		{
			AssertEquals("Collection is empty", 0, CollectionForTest.Count);

			CollectionForTest.Add("Test1", Handler1);
			AssertEquals("Collection has one element", 1, CollectionForTest.Count);

			CollectionForTest.Add("Test2", Handler2);
			AssertEquals("Collection has two elements", 2, CollectionForTest.Count);

			EventHandler newHandler = CollectionForTest["Test2"];
			Assert("NewHander is Test2", newHandler.Equals(Handler2));
			newHandler(this, EventArgs.Empty);
			Assert("Handler Target Called", Target2Hit);

			newHandler = CollectionForTest["Test1"];
			Assert("NewHander is Test1", newHandler.Equals(Handler1));
			newHandler(this, EventArgs.Empty);
			Assert("Handler Target Called", Target1Hit);
		}

		public void TestIntIndexer()
		{
			AssertEquals("Collection is empty", 0, CollectionForTest.Count);

			CollectionForTest.Add("Test1", Handler1);
			AssertEquals("Collection has one element", 1, CollectionForTest.Count);

			CollectionForTest.Add("Test2", Handler2);
			AssertEquals("Collection has two elements", 2, CollectionForTest.Count);

			EventHandler newHandler = CollectionForTest[1];
			Assert("NewHander is Test2", newHandler.Equals(Handler2));
			newHandler(this, EventArgs.Empty);
			Assert("Handler Target Called", Target2Hit);

			newHandler = CollectionForTest[0];
			Assert("NewHander is Test1", newHandler.Equals(Handler1));
			newHandler(this, EventArgs.Empty);
			Assert("Handler Target Called", Target1Hit);
		}

		public void TestNullIndex()
		{
			AssertNull("Ficticious entry reference should return null", CollectionForTest["scootch"]);
		}

		#endregion

		#region Clear

		public void TestClear()
		{
			AssertEquals("Collection is empty", 0, CollectionForTest.Count);
			CollectionForTest.Add("Test1", Handler1);
			AssertEquals("Collection has one element", 1, CollectionForTest.Count);

			CollectionForTest.Clear();

			AssertEquals("Collection is empty", 0, CollectionForTest.Count);
		}

		#endregion

		#region Remove

		public void TestRemove()
		{
			AssertEquals("Collection is empty", 0, CollectionForTest.Count);

			CollectionForTest.Add("Test1", Handler1);
			AssertEquals("Collection has one element", 1, CollectionForTest.Count);

			CollectionForTest.Add("Test2", Handler2);
			AssertEquals("Collection has two elements", 2, CollectionForTest.Count);

			CollectionForTest.Remove("Test1");
			AssertEquals("Collection has one element", 1, CollectionForTest.Count);
			AssertNull("Collection does not have Test1", CollectionForTest["Test1"]);

			CollectionForTest.Remove("Test2");
			AssertEquals("Collection is empty", 0, CollectionForTest.Count);
			AssertNull("Collection does not have Test2", CollectionForTest["Test2"]);
		}

		#endregion

		#region Set Up

		NameEventHandlerCollection CollectionForTest;
		EventHandler Handler1;
		EventHandler Handler2;

		protected override void SetUp()
		{
			base.SetUp();

			CollectionForTest = new NameEventHandlerCollection();
			Handler1 = new EventHandler(TestTarget1);
			Handler2 = new EventHandler(TestTarget2);
		}

		bool Target1Hit;
		void TestTarget1(object sender, EventArgs e)
		{
			Target1Hit = true;
		}

		bool Target2Hit;
		void TestTarget2(object sender, EventArgs e)
		{
			Target2Hit = true;
		}

		#endregion
	}
}
