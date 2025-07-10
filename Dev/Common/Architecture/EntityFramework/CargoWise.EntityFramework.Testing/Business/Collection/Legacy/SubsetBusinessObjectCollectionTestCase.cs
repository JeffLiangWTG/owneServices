using System.Threading;
using CargoWise.Common;
using WTG.StaticAnalysis.Annotation;

namespace CargoWise.EntityFramework.Testing
{
	abstract class SubsetBusinessObjectCollectionTestCase<TBusinessObject> : SubsetBusinessObjectCollectionTest<TBusinessObject>
			where TBusinessObject : BusinessObject
	{
		public void TestHasChangesFromDelete()
		{
			var child = Dummy.Collection.AddNew();
			child.Z0_Description = "INVIEW";

			var coll = GetNewCollection(Dummy.Collection);
			AssertEquals("PreCondition:Contains child", true, coll.Contains(child));

			var collHasChangesFired = false;
			coll.HasChangesChanged += (sender, e) =>
			{
				collHasChangesFired = true;
			};

			var dummyCollectionHasChangesFired = false;
			Dummy.Collection.HasChangesChanged += (sender, e) =>
			{
				dummyCollectionHasChangesFired = true;
			};

			coll.RemoveAndDelete(child);
			AssertEquals("Child is deleted", true, child.IsDeleted);
			AssertEquals("subset has changes", true, coll.HasChanges);
			AssertEquals("Dummy.Collection.HasChanges", true, Dummy.Collection.HasChanges);
			AssertEquals(true, collHasChangesFired);
			AssertEquals(true, dummyCollectionHasChangesFired);
		}

		public void TestMastersAreInDatabaseAndMastersAreDeletedForDepedentCollection()
		{
			DummyDependentBusinessObjectCollection collectionToFilter = new DummyDependentBusinessObjectCollection(Dummy, Factory);
			AssertEquals("PreCondition: Master IsInDatabase", false, Dummy.IsInDatabase);
			AssertEquals("PreCondition: Master IsDeleted", false, Dummy.IsDeleted);

			DummyDependantBusinessObjectSubsetCollection subsetColl = new DummyDependantBusinessObjectSubsetCollection(collectionToFilter);

			AssertEquals("MastersAreInDatabase", false, ((IBusinessObjectCollectionInternals)subsetColl).MastersAreInDatabase);
			AssertEquals("MastersAreDeleted", false, ((IBusinessObjectCollectionInternals)subsetColl).MastersAreDeleted);

			Factory.Save();

			AssertEquals("MastersAreInDatabase", true, ((IBusinessObjectCollectionInternals)subsetColl).MastersAreInDatabase);
			AssertEquals("MastersAreDeleted", false, ((IBusinessObjectCollectionInternals)subsetColl).MastersAreDeleted);

			Dummy.Delete();
			AssertEquals("MastersAreDeleted", true, ((IBusinessObjectCollectionInternals)subsetColl).MastersAreDeleted);
		}

		public void TestTypeOfElements()
		{
			var testCollection = GetNewCollection(Dummy.Collection);
			AssertEquals("TypeOfElements", testCollection.TypeOfElements, Dummy.Collection.TypeOfElements);
		}

		public void TestLoad()
		{
			ErrorReporter.Clear();

			try
			{
				var testView = GetNewCollection(this.Dummy.Collection);
				testView.Load();

				Assert("Error should be reported for Load", !string.IsNullOrEmpty(ErrorReporter.LastMessageReported));
			}
			finally
			{
				ErrorReporter.Clear();
			}
		}

		public void TestRebuildsOnConstruction()
		{
			var child = Dummy.Collection.AddNew();
			child.Z0_Description = "INVIEW";

			var testCollection = GetNewCollection(Dummy.Collection);
			AssertEquals("Count", 1, testCollection.Count);
			AssertEquals("Correct dummy", true, testCollection.Contains(child));
		}

		public void TestRebuild()
		{
			Dummy.Z0_Description = "INVIEW";

			var testBO = Factory.New<DummyBusinessObject>();
			testBO.Z0_Description = "INVIEW";

			var filteredBO = Factory.New<DummyBusinessObject>();
			filteredBO.Z0_Description = "NOTINVIEW";

			var filteredBO2 = Factory.New<DummyBusinessObject>();
			filteredBO2.Z0_Description = "NOTINVIEW";

			Dummy.Collection.Load();
			AssertEquals("Objects in filtered collection", 4, Dummy.Collection.Count);

			var testCollection = GetNewCollection(this.Dummy.Collection);
			AssertEquals("Objects in view collection", 2, testCollection.Count);
			testCollection.Rebuild();
			AssertEquals("Objects in view collection", 2, testCollection.Count);
		}

		#region Thread safe issue

		public static void ThreadThatClearingTheCollection(object bizo)
		{
			Interlocked.Increment(ref threadCount);

			workThreadForASignal.WaitOne();

			var dummy = bizo as DummyBusinessObject;

			dummy.Collection.RemoveAll();

			mainThreadForASignal.Set();

			Interlocked.Decrement(ref threadCount);
		}

		[ThreadSafe(ThreadSafeAttribute.Mechanism.Interlocked)]
		static long threadCount = 0;

		public void TestTryToEnumerateCollectionAlredyClearedByAnotherThread()
		{
			mainThreadForASignal.Reset();
			workThreadForASignal.Reset();

			Dummy.Z0_Description = "INVIEW";

			var testBO = Factory.New<DummyBusinessObject>();
			testBO.Z0_Description = "INVIEW";

			Dummy.Collection.Load();
			AssertEquals("Objects in filtered collection", 2, Dummy.Collection.Count);
			Interlocked.Exchange(ref SubsetBusinessObjectCollection<TBusinessObject>.NeedsSignalStuffForTest, 1L);

			try
			{
				var helpThread = new Thread(new ParameterizedThreadStart(ThreadThatClearingTheCollection));
				helpThread.Start(Dummy);

				while (Interlocked.Read(ref threadCount) < 1)
				{
					Thread.Sleep(500);
				}

				var testCollection = GetNewCollection(this.Dummy.Collection);

				AssertEquals("Objects in view collection", 0, testCollection.Count);
			}
			finally
			{
				Interlocked.Exchange(ref SubsetBusinessObjectCollection<TBusinessObject>.NeedsSignalStuffForTest, 0L);
			}
		}

		#endregion

		public void TestSwapCollection()
		{
			var collection1 = new DummyBusinessObjectCollection(Factory);
			var factory2 = new BusinessObjectFactory();
			var collection2 = new DummyBusinessObjectCollection(factory2);

			var child1 = collection1.AddNew();
			var child2 = collection2.AddNew();

			child1.Z0_Description = "INVIEW";
			child2.Z0_Description = "INVIEW";

			var testCollection = GetNewCollection(collection1);
			AssertEquals(1, testCollection.Count);
			AssertEquals(child1, ((System.Collections.IList)testCollection)[0]);

			testCollection.SwapCollectionToFilter(collection2);
			AssertEquals(1, testCollection.Count);
			AssertEquals(child2, ((System.Collections.IList)testCollection)[0]);

			AssertEquals(factory2, testCollection.Factory);
		}

		public void TestDeleteRow()
		{
			var collection1 = new DummyBusinessObjectCollection(Factory);
			var child1 = collection1.AddNew();
			var child2 = collection1.AddNew();

			child1.Z0_Description = "INVIEW";
			child2.Z0_Description = "INVIEW";

			var testCollection = GetNewCollection(collection1);
			AssertEquals(2, testCollection.Count);
			child1.Row.Delete();
			testCollection.Rebuild();
			AssertEquals(1, testCollection.Count);
		}

		protected abstract ISubsetBusinessObjectCollection GetNewCollection(BusinessObjectCollection collectionToFilter);
	}
}
