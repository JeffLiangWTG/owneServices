using System;
using System.ComponentModel;
using System.Data;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static CargoWise.EntityFramework.ManyToManyBusinessObjectCollection;

namespace CargoWise.EntityFramework.Testing
{
	sealed class ManyToManyBusinessObjectCollectionTest : TestCaseWithDummy
	{
		public void TestCollectionWorksWithNullBOAsAParent()
		{
			DummyBusinessObject nullDummyBO = Factory.GetNull<DummyBusinessObject>();
			ManyToManyBusinessObjectCollection collection = new DummyMToNCollection(nullDummyBO);
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ collection.AddNew(); });
			AssertExceptionThrown(typeof(NotSupportedException), delegate
			{ collection.Add(Factory.New<DummyDependantBusinessObject>()); });
		}

		#region TestLoadingElementFromDbThatHasNoPivotRowDoesNotCreatePivotRow

		class DummyMToNCollectionWithNoRelationshipFilter : DummyMToNCollection
		{
			public DummyMToNCollectionWithNoRelationshipFilter(DummyBaseBusinessObject associatedObj)
				: base(associatedObj)
			{
			}

			protected override ZQuery CreateRelationshipFilter()
			{
				ZQuery result = new ZQuery();
				result.OrderBy = DummyDependentBizoSchema.Constants.ZD1_Code;
				return result;
			}
		}

		public void TestLoadingElementFromDbThatHasNoPivotRowDoesNotCreatePivotRow()
		{
			//try
			//{
			DummyMToNCollectionWithNoRelationshipFilter badCollection = new DummyMToNCollectionWithNoRelationshipFilter(AssociatedObject);
			DummyDependantBusinessObject badDummy = Factory.NewWithValidTestData<DummyDependantBusinessObject>();
			Factory.Save();

			badCollection.Load();
			AssertEquals("No pivot link should have been created.", 0, Factory.Load<DummyPivot>(new ZQuery(DummyPivotSchema.ZDP_Z0, badDummy.PK)).Length);
			AssertEquals("No pivot link exists, element should not have been added to the M to N collection.", 0, badCollection.Count);

			//    string ExpectedError =
			//        "Attempted to create a new pivot link of type 'DummyPivot' for 'DummyDependentBizo, " + BadDummy.PK +
			//        "' business object during a load that has no pivot link in the factory.RelationshipFilter: AdditionalFilter: ";

			//    AssertEquals("Dev error should have been reported.", ExpectedError, ErrorReporter.LastMessageReported.Replace("\n", ""));
			//}
			//finally
			//{
			//    ErrorReporter.Clear();
			//}
		}

		#endregion

		public void TestLoadRelationshipBusinessObjectForDoesNotHitDBForPivotWhenObjectsAreNotInDatabase()
		{
			new DummyMToNCollection(AssociatedObject).AddNew();
			AssertEquals(0, Factory.DatabaseLoadCount);
		}

		public void TestConstructingMToNCollectionDoesNotCreateFetchHint()
		{
			Factory.DropHints();
			new DummyMToNCollection(AssociatedObject);
			AssertEquals(0, Factory.ActiveTableFetchHints);
		}

		public void TestConstructingMToNCollectionCreatesFetchHint()
		{
			Factory.DropHints();
			Factory.Save();
			new DummyMToNCollection(AssociatedObject);
			AssertEquals(1, Factory.ActiveTableFetchHints);
		}

		public void TestAddNonCommittedAndCancelDoesNotCauseHasChangesToBecomeTrue()
		{
			AssertEquals("Initial state", 0, Collection.Count);
			Assert("Initial state", !AssociatedObject.HasChanges);
			Assert("Initial state", !Collection.HasChanges);

			BusinessObject @new = (BusinessObject)((IBindingList)Collection).AddNew();
			Assert("Has changes is false", !AssociatedObject.HasChanges);
			Assert("Has changes is false", !Collection.HasChanges);
			Assert("Has changes is false", !Collection.GetRelationshipBusinessObject(@new).HasChanges);

			((IBindingList)Collection).RemoveAt(0);
			Assert("Has changes is false", !AssociatedObject.HasChanges);
			Assert("Has changes is false", !Collection.HasChanges);
		}

		public void TestStandardAddCausesHasChangesToBecomeTrue()
		{
			Assert("Initial state", !AssociatedObject.HasChanges);
			Assert("Initial state", !Collection.HasChanges);

			DummyDependantBusinessObject kid = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			Collection.Add(kid);
			Assert("Has changes is true", AssociatedObject.HasChanges);
			Assert("Has changes is true", Collection.HasChanges);
		}

		public void TestRemoveOfSavedRelationshipCausesHasChangesToBecomeTrue()
		{
			DummyDependantBusinessObject kid = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			Collection.Add(kid);

			Factory.Save();
			Assert("No changes", !AssociatedObject.HasChanges);
			Assert("No changes", !Collection.HasChanges);

			Collection.Remove(kid);

			Assert("Has changes is true", AssociatedObject.HasChanges);
			Assert("Has changes is true", Collection.HasChanges);
		}

		public void TestRemoveOfObjectWithChangesCausesHasChangesToBecomeTrue()
		{
			DummyDependantBusinessObject kid = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			Collection.Add(kid);

			Collection.HasChanges = false;
			kid.HasChanges = false;

			Assert("No changes", !AssociatedObject.HasChanges);
			Assert("No changes", !Collection.HasChanges);

			kid.ZD1_Number = 234324; // make a change
			Collection.Remove(kid);

			Assert("Has changes is true", AssociatedObject.HasChanges);
			Assert("Has changes is true", Collection.HasChanges);
		}

		public void TestAddNewDoesNotCauseHasChangesToBeTrue()
		{
			Assert("Initial state", !AssociatedObject.HasChanges);
			Assert("Initial state", !Collection.HasChanges);

			Collection.AddNew();
			Assert("Has changes is false", !AssociatedObject.HasChanges);
			Assert("Has changes is false", !Collection.HasChanges);
		}

		#region Delete Checker Tests

		public void TestObjectsAreNotRemovedIfTheirLinkRowsHaveADeleteCheckerAndRemoveIsCalled()
		{
			DeleteCheckedCollection collection = new DeleteCheckedCollection(Dummy);
			BusinessObject newElement = collection.AddNew();

			bool gotException = false;
			try
			{
				collection.Remove(newElement);
			}
			catch (CannotDeleteException)
			{
				gotException = true;
				AssertEquals("Not removed", 1, collection.Count);
				AssertEquals("Not deleted", false, newElement.IsDeleted);
			}
			Assert("Delete checker Exception thrown", gotException);
		}

		public void TestObjectsAreNotRemovedIfTheirLinkRowsHaveADeleteCheckerAndTheyAreDeleted()
		{
			DeleteCheckedCollection collection = new DeleteCheckedCollection(Dummy);
			BusinessObject newElement = collection.AddNew();

			bool gotException = false;
			try
			{
				newElement.Delete();
			}
			catch (CannotDeleteException)
			{
				gotException = true;
				AssertEquals("Not removed", 1, collection.Count);
				AssertEquals("Not deleted", false, newElement.IsDeleted);
			}
			Assert("Delete checker Exception thrown", gotException);
		}

		class PivotWithDeleteChecker : DummyPivot
		{
			public PivotWithDeleteChecker(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			protected override IBusinessObjectStrategy[] GetStrategies()
			{
				return new IBusinessObjectStrategy[] { new PivotDeleteChecker() };
			}
		}

		class PivotDeleteChecker : DeleteChecker
		{
			public override DeleteDetails DeleteDetails(BusinessObject businessObjectToBeDeleted)
			{
				return new DeleteDetails.Disallow("go away - this pivot is immortal");
			}
		}

		class DeleteCheckedCollection : DummyMToNCollection
		{
			public DeleteCheckedCollection(DummyBaseBusinessObject assocObject)
				: base(assocObject)
			{
			}

			protected override Type TypeOfRelationshipBusinessObject
			{
				get
				{
					return typeof(PivotWithDeleteChecker);
				}
			}
		}

		#endregion

		public void TestMastersAreInDatabase()
		{
			AssociatedObject.SetIsInDatabase(false);
			AssertEquals("master in DB", false, ((IBusinessObjectCollectionInternals)Collection).MastersAreInDatabase);
			AssociatedObject.SetIsInDatabase(true);
			AssertEquals("master not in DB", true, ((IBusinessObjectCollectionInternals)Collection).MastersAreInDatabase);
		}

		public void TestHasChangesOnAddOfChildWithNoExistingRelationship()
		{
			AssertEquals("Initial collection HasChanges", false, Collection.HasChanges);

			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			AssertEquals("Initial Child HasChanges", false, child.HasChanges);

			Collection.Add(child);
			AssertEquals("Child HasChanges", true, child.HasChanges);
			AssertEquals("Collection HasChanges", true, Collection.HasChanges);
		}

		public void TestHasChangesOnAddOfChildWithExistingRelationship()
		{
			AssertEquals("Initial collection HasChanges", false, Collection.HasChanges);

			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			AssertEquals("Initial Child HasChanges", false, child.HasChanges);

			CreateNewPivot(AssociatedObject.PK, child.PK);

			Collection.Add(child);
			AssertEquals("Child HasChanges", false, child.HasChanges);
			AssertEquals("Collection HasChanges", false, Collection.HasChanges);
		}

		public void TestSetupNewElementButDoNotAddItDoesNotSetHasChanges()
		{
			AssertEquals("Initial collection HasChanges", false, Collection.HasChanges);

			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			AssertEquals("Initial Child HasChanges", false, child.HasChanges);

			Collection.SetupNewElementButDoNotAddIt(child, true);
			AssertEquals("Child HasChanges", false, child.HasChanges);
			AssertEquals("Collection HasChanges", false, Collection.HasChanges);
		}

		public void TestConstruction()
		{
			AssertNotNull(Dummy);
			AssertNotNull(Collection);
			AssertEquals(0, Collection.Count);
		}

		public void TestRemoveCollectionRelationships()
		{
			Collection.Load();
			AssertEquals("No junk in DB to confuse test", 0, Collection.Count);

			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			Collection.Add(child);
			Collection.Load();
			AssertEquals("One item in collection with relationship", 1, Collection.Count);

			Collection.Remove(child);
			Collection.Load();
			AssertEquals("Relationship should be gone", 0, Collection.Count);
		}

		public void TestRelationshipPersists()
		{
			Collection.Load();
			AssertEquals("No junk in DB to confuse test", 0, Collection.Count);

			BusinessObject child1 = Factory.New(typeof(DummyDependantBusinessObject));
			Collection.Add(child1);
			_ = Collection.AddNew();
			AssertEquals("2 added", 2, Collection.Count);

			Factory.Save();

			DummyMToNCollection collection2 = new DummyMToNCollection(AssociatedObject);
			collection2.Load();
			AssertEquals("Relationship saved and loaded OK", 2, collection2.Count);
		}

		public void TestLoadPivotFiltering()
		{
			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			BusinessObject child2 = Factory.New(typeof(DummyDependantBusinessObject));

			CreateNewPivot(AssociatedObject.PK, child.PK);
			Collection.Load();
			AssertEquals("Count", 1, Collection.Count);
			AssertEquals("PK", child.PK, Collection[0].PK);

			CreateNewPivot(AssociatedObject.PK, child2.PK);
			Collection.Load();
			AssertEquals("Count", 2, Collection.Count);
		}

		public void TestLoadAssociatedObjectFiltering()
		{
			DummyBusinessObject associatedObject2 = (DummyBusinessObject)Factory.New(typeof(DummyBusinessObject));
			DummyMToNCollection collection2 = new DummyMToNCollection(associatedObject2);

			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			BusinessObject child2 = Factory.New(typeof(DummyDependantBusinessObject));

			CreateNewPivot(AssociatedObject.PK, child.PK);
			CreateNewPivot(associatedObject2.PK, child2.PK);

			Collection.Load();
			AssertEquals("Count", 1, Collection.Count);
			AssertEquals("Got the right child", child.PK, Collection[0].PK);

			collection2.Load();
			AssertEquals("Count", 1, collection2.Count);
			AssertEquals("Got the right child", child2.PK, collection2[0].PK);
		}

		public void TestLoadWithConstructorFilter()
		{
			DummyMToNCollection collection = new DummyMToNCollection(AssociatedObject, new ZQuery(DummyDependentBizoSchema.ZD1_Number, 5));

			DummyDependantBusinessObject child = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child.ZD1_Number = 5;

			DummyDependantBusinessObject child2 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child2.ZD1_Number = 6;

			CreateNewPivot(AssociatedObject.PK, child.PK);
			collection.Load();
			AssertEquals("Count", 1, collection.Count);
			AssertEquals("PK", child.PK, collection[0].PK);
		}

		public void TestLoadWithFilterAndParams()
		{
			DummyMToNCollection collection = new DummyMToNCollection(AssociatedObject, new ZQuery(DummyDependentBizoSchema.ZD1_Number, 5));

			DummyDependantBusinessObject child0 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child0.ZD1_Number = 0;

			DummyDependantBusinessObject child1 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child1.ZD1_Number = 1;

			DummyDependantBusinessObject child2 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child2.ZD1_Number = 2;

			DummyDependantBusinessObject child3 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child3.ZD1_Number = 3;

			DummyDependantBusinessObject child4 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child4.ZD1_Number = 4;

			CreateNewPivot(AssociatedObject.PK, child0.PK);
			CreateNewPivot(AssociatedObject.PK, child1.PK);
			CreateNewPivot(AssociatedObject.PK, child2.PK);
			CreateNewPivot(AssociatedObject.PK, child4.PK);

			ZQuery filter = new ZQuery();
			filter.AddToFilter(DummyDependentBizoSchema.ZD1_Number, SQLComparisonOperator.LessThan, 4);
			collection.Load(filter);

			AssertEquals(true, collection.Contains(child0.PK));
			AssertEquals(true, collection.Contains(child1.PK));
			AssertEquals(true, collection.Contains(child2.PK));
			AssertEquals(collection.Count, 3);
		}

		public void TestLoadWithFilterNoParams()
		{
			DummyMToNCollection collection = new DummyMToNCollection(AssociatedObject, new ZQuery(DummyDependentBizoSchema.ZD1_Number, 5));

			DummyDependantBusinessObject child0 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child0.ZD1_Number = 0;

			DummyDependantBusinessObject child1 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child1.ZD1_Number = 1;

			DummyDependantBusinessObject child2 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child2.ZD1_Number = 2;

			DummyDependantBusinessObject child3 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child3.ZD1_Number = 3;

			DummyDependantBusinessObject child4 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child4.ZD1_Number = 4;

			CreateNewPivot(AssociatedObject.PK, child0.PK);
			CreateNewPivot(AssociatedObject.PK, child1.PK);
			CreateNewPivot(AssociatedObject.PK, child2.PK);
			CreateNewPivot(AssociatedObject.PK, child4.PK);

			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyDependentBizoSchema.ZD1_Number, SQLComparisonOperator.LessThan, 4);
			collection.Load(sQLFilter);

			AssertEquals(true, collection.Contains(child0.PK));
			AssertEquals(true, collection.Contains(child1.PK));
			AssertEquals(true, collection.Contains(child2.PK));
			AssertEquals(collection.Count, 3);
		}

		public void TestLoadWithSQLFilter()
		{
			DummyMToNCollection collection = new DummyMToNCollection(AssociatedObject, new ZQuery(DummyDependentBizoSchema.ZD1_Number, 5));

			DummyDependantBusinessObject child0 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child0.ZD1_Number = 0;

			DummyDependantBusinessObject child1 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child1.ZD1_Number = 1;

			DummyDependantBusinessObject child2 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child2.ZD1_Number = 2;

			DummyDependantBusinessObject child3 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child3.ZD1_Number = 3;

			DummyDependantBusinessObject child4 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			child4.ZD1_Number = 4;

			DummyDependantBusinessObject childWithNoPivot = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			childWithNoPivot.ZD1_Number = 0;

			CreateNewPivot(AssociatedObject.PK, child0.PK);
			CreateNewPivot(AssociatedObject.PK, child1.PK);
			CreateNewPivot(AssociatedObject.PK, child2.PK);
			CreateNewPivot(AssociatedObject.PK, child4.PK);

			ZQuery sQLFilter = new ZQuery();
			sQLFilter.AddToFilter(DummyDependentBizoSchema.ZD1_Number, SQLComparisonOperator.LessThan, 4);
			collection.Load(sQLFilter);

			AssertEquals("In collection", true, collection.Contains(child0.PK));
			AssertEquals("In collection", true, collection.Contains(child1.PK));
			AssertEquals("In collection", true, collection.Contains(child2.PK));
			AssertEquals("Didn't load any other trash", collection.Count, 3);
		}

		public void TestGetRelationshipBusinessObject()
		{
			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			DummyPivot createdPivot = CreateNewPivot(AssociatedObject.PK, child.PK);
			createdPivot.ZDP_AddInfo = "AddInfoSET!";
			Collection.Load();
			AssertEquals("Count", 1, Collection.Count);
			AssertEquals("PK", child.PK, Collection[0].PK);

			DummyPivot pivot = (DummyPivot)Collection.GetRelationshipBusinessObject(child);
			AssertNotNull(pivot);
			AssertEquals("AddInfoSET!", pivot.ZDP_AddInfo);

			BusinessObject child2 = Factory.New(typeof(DummyDependantBusinessObject));
			DummyPivot pivot2 = (DummyPivot)Collection.GetRelationshipBusinessObject(child2);
			AssertNull(pivot2);
		}

		public void TestAddFromDatabaseIfMatchLastLoadedFilter()
		{
			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			Factory.Save();

			AssertEquals("Count", 0, Collection.Count);
			AssertEquals("AddFromDatabaseIfMatchLastLoadedFilter", false, Collection.AddFromDatabaseIfMatchLastLoadedFilter(child.PK));
			AssertEquals("Count", 0, Collection.Count);
		}

		public void TestAddFromDatabaseIfMatchLastLoadedFilterWorksWithDataRefreshBus()
		{
			Factory.Save();

			DummyDependantBusinessObject child0 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			CreateNewPivot(AssociatedObject.PK, child0.PK);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBaseBusinessObject copyOfAssociatedBusinessObject = (DummyBaseBusinessObject)factory2.Load(typeof(DummyBaseBusinessObject), AssociatedObject.PK);
			DummyMToNCollection collection2 = new DummyMToNCollection(copyOfAssociatedBusinessObject);
			copyOfAssociatedBusinessObject.RegisterEditableChildObject(collection2);
			collection2.Load();
			AssertEquals("IsLoaded", true, collection2.IsLoaded);
			AssertEquals("Count", 1, collection2.Count);

			DummyDependantBusinessObject child = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			CreateNewPivot(AssociatedObject.PK, child.PK);
			Factory.Save();

			AssertEquals("Count", 2, collection2.Count);

			DummyDependantBusinessObject child2 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			CreateNewPivot(AssociatedObject.PK, child2.PK);
			Factory.Save();

			AssertEquals("Count", 3, collection2.Count);
		}

		public void TestNotifyDelete()
		{
			TestCaseHelper.ClearTable(DummyPivot.Schema.TableName);

			DummyMToNCollection collection2 = new DummyMToNCollection(AssociatedObject);
			BusinessObject bizObj = Collection.AddNew();

			collection2.Add(Factory.Load(typeof(AnotherDummyDependantBusinessObject), bizObj.PK));
			BusinessObject bizObj2 = collection2[0];

			AssertEquals("Collection count", 1, Collection.Count);
			AssertEquals("Collection2 count", 1, collection2.Count);

			AssertEquals("Same Row in BizObjs", Collection[0].Row, collection2[0].Row);

			AssertEquals("Same relationship BusinessObject", Collection.GetRelationshipBusinessObject(Collection[0]), collection2.GetRelationshipBusinessObject(collection2[0]));

			BusinessObject[] pivots = Factory.Load(typeof(DummyPivot), new ZQuery());
			AssertEquals("Shared pivot", 1, pivots.Length);

			Collection[0].Delete();

			AssertEquals("Collection count", 0, Collection.Count);
			AssertEquals("Collection2 count", 0, collection2.Count);

			Assert("Deleted", bizObj.IsDeleted);
			Assert("Deleted", bizObj2.IsDeleted);

			pivots = Factory.Load(typeof(DummyPivot), new ZQuery());
			AssertEquals("Shared pivot should be deleted", 0, pivots.Length);
		}

		public void TestDetachAndDeletionOfPivotDetachesFromSecondCollection()
		{
			TestCaseHelper.ClearTable(DummyPivot.Schema.TableName);

			DummyMToNCollection collection2 = new DummyMToNCollection(AssociatedObject);
			BusinessObject bizObj = Collection.AddNew();

			collection2.Add(Factory.Load(typeof(AnotherDummyDependantBusinessObject), bizObj.PK));
			BusinessObject bizObj2 = collection2[0];

			AssertEquals("Collection count", 1, Collection.Count);
			AssertEquals("Collection2 count", 1, collection2.Count);

			AssertEquals("Same Row in BizObjs", Collection[0].Row, collection2[0].Row);

			AssertEquals("Same relationship BusinessObject", Collection.GetRelationshipBusinessObject(Collection[0]), collection2.GetRelationshipBusinessObject(collection2[0]));

			BusinessObject[] pivots = Factory.Load(typeof(DummyPivot), new ZQuery());
			AssertEquals("Shared pivot", 1, pivots.Length);

			Collection.Remove(bizObj);

			AssertEquals("Collection count", 0, Collection.Count);
			AssertEquals("Collection2 count", 0, collection2.Count);

			Assert("Not Deleted", !bizObj.IsDeleted);
			Assert("Not Deleted", !bizObj2.IsDeleted);

			pivots = Factory.Load(typeof(DummyPivot), new ZQuery());
			AssertEquals("Shared pivot should be deleted", 0, pivots.Length);
		}

		public void TestDeletingSameLinkFromDifferentCollectionsWillNotCauseDBConcurrencyError()
		{
			Dummy.Factory.Save();

			Collection.Load();
			AssertEquals("No junk in DB to confuse test", 0, Collection.Count);

			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			Collection.Add(child);
			Collection.Load();
			AssertEquals("One item in collection with relationship", 1, Collection.Count);
			Factory.Save();

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBusinessObject dummy2 = (DummyBusinessObject)factory2.Load(Dummy.GetType(), Dummy.PK);
			DummyMToNCollection collection2 = new DummyMToNCollection(dummy2);

			collection2.Load();
			AssertEquals("One item in collection with relationship", 1, Collection.Count);

			Collection[0].Delete();
			Factory.Save();

			AssertEquals("Collection2.Count", 0, collection2.Count);

			factory2.Save(); //should not throw exceptions.
		}

		public void TestHasChangesTrueWhenRemovingPivot()
		{
			BusinessObject child = Factory.New(typeof(DummyDependantBusinessObject));
			Collection.Add(child);
			Collection.Load();
			AssertEquals("One item in collection with relationship", 1, Collection.Count);
			Factory.Save();

			AssertEquals("HasChanges", false, Collection.HasChanges);
			Collection.Remove(child);
			AssertEquals("HasChanges", true, Collection.HasChanges);
		}

		public void TestPivotsWorkWithDataRefreshBusDelete()
		{
			Factory.Save();

			DummyDependantBusinessObject child0 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			CreateNewPivot(AssociatedObject.PK, child0.PK);
			Factory.Save();

			Collection.Load();
			AssertEquals("Count", 1, Collection.Count);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBaseBusinessObject copyOfAssociatedBusinessObject = (DummyBaseBusinessObject)factory2.Load(typeof(DummyBaseBusinessObject), AssociatedObject.PK);
			DummyMToNCollection collection2 = new DummyMToNCollection(copyOfAssociatedBusinessObject);
			collection2.Load();
			AssertEquals("IsLoaded", true, collection2.IsLoaded);
			AssertEquals("Count", 1, collection2.Count);

			Collection.Remove(child0);
			AssertEquals("Count", 0, Collection.Count);

			Factory.Save();
			AssertEquals("Count", 0, collection2.Count);
		}

		public void TestPivotsWorkWithDataRefreshBusDelete_ForcingIsUpdatingByDataRefreshBusFlagOnMainCollection()
		{
			var parent = Factory.New<DummyBusinessObject>();
			var collection = new DummyMToNCollectionForDataRefreshBusTesting(parent);

			var child1 = collection.AddNew();
			var child2 = collection.AddNew();
			var child3 = collection.AddNew();

			collection.Remove(child1);
			AssertEquals(false, collection.StateOfIsUpdatingByDataRefreshBusFlagWhenRemoveWasCalled);

			var pivot = collection.GetRelationshipBusinessObject(child2);
			Factory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { child2, child3 }, collection);

			var anotherFactory = new BusinessObjectFactory();
			var pivotReloaded = anotherFactory.Load<DummyPivot>(pivot.PK);

			AssertEquals("Precondition", true, Factory.RefreshEnabled);
			AssertEquals("Precondition", true, anotherFactory.RefreshEnabled);
			pivotReloaded.Delete();
			anotherFactory.Save();

			AssertContainsExactElementsInAnyOrder(new[] { child3 }, collection);
			AssertEquals(true, collection.StateOfIsUpdatingByDataRefreshBusFlagWhenRemoveWasCalled);

			collection.Remove(child3);
			AssertEquals(false, collection.StateOfIsUpdatingByDataRefreshBusFlagWhenRemoveWasCalled);
		}

		public void TestPivotsWorkWithDataRefreshBusDelete_WillNotTriggerHasChangesChangedOnParent()
		{
			var parent = Factory.New<DummyBusinessObject>();
			var collection = new DummyMToNCollection(parent);

			var child = collection.AddNew();
			var pivot = collection.GetRelationshipBusinessObject(child);
			Factory.Save();

			parent.HasChangesChanged += (s, e) =>
			{
				Fail("Update caused by DataRefreshBus should not trigger HasChangesChanged on parent object (by playing with HasChanges/HasChangesFromDelete on collection)");
			};

			var anotherFactory = new BusinessObjectFactory();
			var pivotReloaded = anotherFactory.Load<DummyPivot>(pivot.PK);

			AssertEquals("Precondition", true, Factory.RefreshEnabled);
			AssertEquals("Precondition", true, anotherFactory.RefreshEnabled);
			pivotReloaded.Delete();
			anotherFactory.Save();

			AssertEquals(false, parent.HasChanges);
			AssertEquals(false, ((IBusinessObjectCollectionInternals)collection).HasChangesFromDelete);

			AssertEquals(true, pivot.IsDeleted);
			AssertEquals(false, collection.Contains(child));
		}

		class DummyMToNCollectionForDataRefreshBusTesting : DummyMToNCollection
		{
			public DummyMToNCollectionForDataRefreshBusTesting(DummyBaseBusinessObject parent)
				: base(parent)
			{
			}

			public override void Remove(BusinessObject elementToRemove)
			{
				StateOfIsUpdatingByDataRefreshBusFlagWhenRemoveWasCalled = IsUpdatingByDataRefreshBus;
				base.Remove(elementToRemove);
			}

			public bool StateOfIsUpdatingByDataRefreshBusFlagWhenRemoveWasCalled;
		}

		public void TestPivotsAreNotIncorrectlyCreatedWhenRefreshingAnotherFactory()
		{
			DummyBaseBusinessObject bizO1 = (DummyBaseBusinessObject)Factory.New(typeof(DummyBaseBusinessObject));
			DummyBaseBusinessObject bizO2 = (DummyBaseBusinessObject)Factory.New(typeof(DummyBaseBusinessObject));

			DummyMToNCollection collection1 = new DummyMToNCollection(bizO1);
			collection1.Load();
			DummyMToNCollection collection2 = new DummyMToNCollection(bizO2);
			collection2.Load();

			Factory.Save();

			BusinessObjectFactory otherFactory = new BusinessObjectFactory();
			DummyBaseBusinessObject bizO1InOtherFactory = (DummyBaseBusinessObject)otherFactory.Load(typeof(DummyBaseBusinessObject), bizO1.PK);

			DummyMToNCollection collection1InOtherFactory = new DummyMToNCollection(bizO1InOtherFactory);
			collection1InOtherFactory.AddNew();

			otherFactory.Save();

			AssertEquals(1, collection1.Count);
			AssertEquals(0, collection2.Count);
		}

		public void TestPivotsWorkWithDataRefreshBusInsert()
		{
			Factory.Save();

			Collection.Load();
			AssertEquals("Count", 0, Collection.Count);

			BusinessObjectFactory factory2 = new BusinessObjectFactory();
			DummyBaseBusinessObject copyOfAssociatedBusinessObject = (DummyBaseBusinessObject)factory2.Load(typeof(DummyBaseBusinessObject), AssociatedObject.PK);
			DummyMToNCollection collection2 = new DummyMToNCollection(copyOfAssociatedBusinessObject);
			collection2.Load();
			AssertEquals("Count", 0, collection2.Count);

			DummyDependantBusinessObject child0 = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			Factory.Save();

			CreateNewPivot(AssociatedObject.PK, child0.PK);
			Factory.Save();

			AssertEquals("Count", 1, collection2.Count);
		}

		public void TestNewManyToManyCollectionsDoNotHitDatabase()
		{
			DummyBaseBusinessObject copyOfAssociatedBusinessObject = (DummyBaseBusinessObject)Factory.New(typeof(DummyBaseBusinessObject));
			DummyMToNCollection mnNCollection = new DummyMToNCollection(copyOfAssociatedBusinessObject);
			Assert("Fetch From Local Cache should be set to prevent the collection from hitting the database", mnNCollection.RelationshipBusinessObjectsFilter.FetchOnlyFromLocalCache);
		}

		class DummyMToNCollectionThatUsesMaster : DummyMToNCollection
		{
			public DummyMToNCollectionThatUsesMaster(DummyBaseBusinessObject assocObj)
				: base(assocObj)
			{
			}

			protected override ZQuery CreateRelationshipFilter()
			{
				if (((DummyBaseBusinessObject)fAssociatedObject).Z0_AnotherNumber != 0)
				{
					//dont care as long as Master is touched here.
				}
				return base.CreateRelationshipFilter();
			}
		}

		[ExpectNoExceptions()]
		public void TestAddFromDatabaseIfMatchLastLoadedFilterWhenMasterIsDeleted()
		{
			DummyMToNCollectionThatUsesMaster collection = new DummyMToNCollectionThatUsesMaster(Dummy);
			collection.Load();

			AssertEquals("IsInDatabase", false, Dummy.IsInDatabase);
			Dummy.Delete();

			DummyDependantBusinessObject dummyChild = (DummyDependantBusinessObject)Factory.New(typeof(DummyDependantBusinessObject));
			dummyChild.ZD1_Code = "TTT";

			collection.AddFromDatabaseIfMatchLastLoadedFilter(dummyChild.PK);
		}

		public void TestShouldNotAddElementsOnDataRefreshIfThereIsNoRelationshipObject()
		{
			var dummyMaster = Factory.NewWithValidTestData<DummyBusinessObject>();
			var m2nCollection = new DummyMToNCollection(dummyMaster);
			m2nCollection.Load();

			var dummyChild = Factory.New<DummyDependantBusinessObject>();
			m2nCollection.AddFromDataRefresh(new[] { dummyChild });

			AssertEquals("Should not add element on data referesh as there is no relationship object", 0, m2nCollection.Count);

			var dummyPivot = Factory.New<DummyPivot>();
			dummyPivot.ZDP_Z0 = dummyMaster.PK;
			dummyPivot.ZDP_ZD1 = dummyChild.PK;

			AssertEquals("Should not add yet", 0, m2nCollection.Count);

			m2nCollection.AddFromDataRefresh(new[] { dummyChild });

			AssertEquals("Should add when tere is a relationship object", 1, m2nCollection.Count);
		}

		#region Uncommitted elements

		public void TestAddNewUncommittedBizoCreatesUncommittedPivot()
		{
			var bizo = (BusinessObject)((IBindingList)Collection).AddNew();

			AssertNotNull(bizo);
			Assert(((IBusinessObjectInternals)bizo).IsUnCommittedRow);

			var pivot = Collection.GetRelationshipBusinessObject(bizo);

			AssertNotNull(pivot);
			Assert(((IBusinessObjectInternals)pivot).IsUnCommittedRow);
		}

		public void TestSaveCollectionWithUncommittedBizoAndPivotDoesNotSaveUncommittedElements()
		{
			var bizo = (BusinessObject)((IBindingList)Collection).AddNew();
			var pivot = Collection.GetRelationshipBusinessObject(bizo);

			Assert(((IBusinessObjectInternals)bizo).IsUnCommittedRow);
			Assert(((IBusinessObjectInternals)pivot).IsUnCommittedRow);

			Assert(!bizo.IsInDatabase);
			Assert(!pivot.IsInDatabase);

			Collection.Factory.Save();

			Assert(!bizo.IsInDatabase);
			Assert(!pivot.IsInDatabase);
		}

		public void TestCommittingBizoCommitsPivot()
		{
			var bizo = (BusinessObject)((IBindingList)Collection).AddNew();
			var pivot = Collection.GetRelationshipBusinessObject(bizo);

			bizo.HasChanges = true;

			Assert(((IBusinessObjectInternals)bizo).IsUnCommittedRow);
			Assert(((IBusinessObjectInternals)pivot).IsUnCommittedRow);

			AssertSame(bizo, Collection[Collection.Count - 1]);
			((ICancelAddNew)Collection).EndNew(Collection.Count - 1);

			AssertEquals(DataRowState.Added, bizo.Row.RowState);
			AssertEquals(DataRowState.Added, pivot.Row.RowState);

			AssertSame("Should not add new pivot when committed", pivot, Collection.GetRelationshipBusinessObject(bizo));

			Assert(!bizo.IsInDatabase);
			Assert(!pivot.IsInDatabase);

			Collection.Factory.Save(); // Should not save anything and not cause any errors

			Assert(bizo.IsInDatabase);
			Assert(pivot.IsInDatabase);
		}

		public void TestDeletingUncommittedBizoDeletesPivot()
		{
			var bizo = (BusinessObject)((IBindingList)Collection).AddNew();
			var pivot = Collection.GetRelationshipBusinessObject(bizo);

			bizo.HasChanges = true;

			Assert(((IBusinessObjectInternals)bizo).IsUnCommittedRow);
			Assert(((IBusinessObjectInternals)pivot).IsUnCommittedRow);

			AssertSame(bizo, Collection[Collection.Count - 1]);
			((ICancelAddNew)Collection).CancelNew(Collection.Count - 1);

			Assert(pivot.IsDeleted);

			Collection.Factory.Save(); // Should not save anything and not cause any errors

			Assert(bizo.IsDeleted);
			Assert(pivot.IsDeleted);
		}

		public void TestAddNewUncommittedBizoCreatesUncommittedPivot_WithCollectionView()
		{
			var view = new DummyViewCollection(Collection);
			var bizo = (BusinessObject)((IBindingList)view).AddNew();

			AssertNotNull(bizo);
			Assert(((IBusinessObjectInternals)bizo).IsUnCommittedRow);

			var pivot = Collection.GetRelationshipBusinessObject(bizo);

			AssertNotNull(pivot);
			Assert(((IBusinessObjectInternals)pivot).IsUnCommittedRow);
		}

		class DummyViewCollection : BusinessObjectCollectionView<BusinessObject>
		{
			public DummyViewCollection(BusinessObjectCollection collectionToFilter) : base(collectionToFilter) { }

			protected override bool IsThisPartOfTheCollection(BusinessObject element)
			{
				return true;
			}
		}

		#endregion

		#region Implementation

		DummyMToNCollection Collection;
		DummyBusinessObject AssociatedObject;

		DummyPivot CreateNewPivot(ZGuid zDP_Z0, ZGuid zDP_ZD1)
		{
			DummyPivot pivot = Factory.New<DummyPivot>();
			pivot.ZDP_Z0 = zDP_Z0;
			pivot.ZDP_ZD1 = zDP_ZD1;
			pivot.HasChanges = false;

			return pivot;
		}

		protected override void SetUp()
		{
			// We want to ensure the ProcessFieldChangeRules are pre-loaded in the UberCache so that HitCounts are accurate
			new BusinessObjectFactory().New<DummyBusinessObject>();
			base.SetUp();
			AssociatedObject = Dummy;
			Collection = new DummyMToNCollection(AssociatedObject);
		}

		internal class AnotherDummyDependantBusinessObject : DummyDependantBusinessObject
		{
			public AnotherDummyDependantBusinessObject(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}
		}

		#endregion
	}

	[TestedType(typeof(PivotCollection))]
	sealed class PivotCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var dummyBO = Factory.NewWithValidTestData<DummyBusinessObject>();
			var collection = new DummyMToNCollection(dummyBO);

			return new PivotCollection(dummyBO, collection);
		}
	}
}
