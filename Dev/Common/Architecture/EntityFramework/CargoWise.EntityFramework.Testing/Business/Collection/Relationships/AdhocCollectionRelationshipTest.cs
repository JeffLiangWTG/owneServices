using System;
using System.Data;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class AdhocCollectionRelationshipTest : TestCaseWithFactory
	{
		public void TestAddNew()
		{
			DummyBusinessObject dummy = Collection.AddNew();
			AssertEquals(true, Collection.Contains(dummy));
		}

		public void TestObjectFromAnotherFactory()
		{
			var bizOFactory2 = new BusinessObjectFactory().New<DummyBusinessObject>();
			var relationship = new AdhocCollectionRelationship(typeof(DummyBusinessObject));

			ICollectionRelationship interaceImp = relationship;
			interaceImp.AddToRelationship(bizOFactory2);

			AssertEquals(0, relationship.LoadBusinessObjects(Factory, new ZQuery()).Length);
		}

		public void TestSorted()
		{
			DummyBusinessObject dummy2 = Collection.AddNew();
			DummyBusinessObject dummy1 = Collection.AddNew();
			dummy1.Z0_Code = "1";
			dummy2.Z0_Code = "2";

			Collection.ApplySort(DummyBizoSchema.Z0_Code.Name, System.ComponentModel.ListSortDirection.Ascending);
			AssertEquals("1", Collection[0].Z0_Code);
			AssertEquals("2", Collection[1].Z0_Code);

			Collection.ApplySort(DummyBizoSchema.Z0_Code.Name, System.ComponentModel.ListSortDirection.Descending);
			AssertEquals("2", Collection[0].Z0_Code);
			AssertEquals("1", Collection[1].Z0_Code);

			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			dummy3.Z0_Code = "3";
			Collection.Add(dummy3);
			AssertEquals("3", Collection[0].Z0_Code);
			AssertEquals("2", Collection[1].Z0_Code);
			AssertEquals("1", Collection[2].Z0_Code);

			dummy2.Delete();
			AssertEquals("3", Collection[0].Z0_Code);
			AssertEquals("1", Collection[1].Z0_Code);
		}

		#region Equals / GetHashCode

		public void TestEquals()
		{
			TestAdhocCollectionRelationship relationship1 = new TestAdhocCollectionRelationship(typeof(DummyBusinessObject));
			TestAdhocCollectionRelationship relationship2 = new TestAdhocCollectionRelationship(typeof(DummyBusinessObject));
			AssertEquals("Relationships equal by object reference", true, relationship1.Equals(relationship1));
			AssertEquals("Relationships equal by object reference", false, relationship1.Equals(relationship2));
		}

		#endregion

		#region Clone

		public void TestClone()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			((ICollectionRelationship)Relationship).AddToRelationship(dummy1);
			((ICollectionRelationship)Relationship).AddToRelationship(dummy2);
			AssertEquals("2 business objects added", 2, Relationship.LoadBusinessObjects(Factory, new ZQuery()).Length);

			ICollectionRelationship clonedRelationship = Relationship.Clone();
			AssertEquals("Relationship contains same business objects when cloned", 2, clonedRelationship.LoadBusinessObjects(Factory, new ZQuery()).Length);

			DummyBusinessObject dummy3 = Factory.New<DummyBusinessObject>();
			((ICollectionRelationship)Relationship).AddToRelationship(dummy3);
			AssertEquals("Adding to the original relationship doesn't affect the cloned relationship", 2, clonedRelationship.LoadBusinessObjects(Factory, new ZQuery()).Length);
			AssertEquals("Copy on write of the pk list allows new objects to be added to the original relationship", 3, Relationship.LoadBusinessObjects(Factory, new ZQuery()).Length);

			clonedRelationship = Relationship.Clone();
			DummyBusinessObject dummy4 = Factory.New<DummyBusinessObject>();
			clonedRelationship.AddToRelationship(dummy4);
			AssertEquals("Adding to the cloned relationship doesn't affect the original relationship", 3, Relationship.LoadBusinessObjects(Factory, new ZQuery()).Length);
			AssertEquals("Copy on write of the pk list allows new objects to be added to the cloned relationship", 4, clonedRelationship.LoadBusinessObjects(Factory, new ZQuery()).Length);
		}

		#endregion

		#region MatchesRelationshipFilter

		public void TestMatchesRelationshipFilter()
		{
			var dummy1 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			var dummy2 = Factory.New<DummyBusinessObjectWithActiveFilter>();
			dummy1.IsActive = true;
			dummy2.IsActive = true;
			var relationship = new TestAdhocCollectionRelationship(typeof(DummyBusinessObjectWithActiveFilter));

			((ICollectionRelationship)relationship).AddToRelationship(dummy1);
			AssertEquals("Precondition: 1 business object added.", 1, relationship.LoadBusinessObjects(Factory, new ZQuery()).Length);

			AssertEquals("Dummy1 should match filter.", true, relationship.MatchesRelationshipFilter(dummy1, false, false));
			AssertEquals("Dummy1 should match filter.", true, relationship.MatchesRelationshipFilter(dummy1, false, true));
			AssertEquals("Dummy1 should match filter.", true, relationship.MatchesRelationshipFilter(dummy1, true, false));
			AssertEquals("Dummy1 should match filter.", true, relationship.MatchesRelationshipFilter(dummy1, true, true));

			AssertEquals("Dummy2 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy2, false, false));
			AssertEquals("Dummy2 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy2, false, true));
			AssertEquals("Dummy2 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy2, true, false));
			AssertEquals("Dummy2 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy2, true, true));

			dummy1.IsActive = false;
			AssertEquals("Dummy1 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy1, false, false));
			AssertEquals("Dummy1 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy1, false, true));
			AssertEquals("Dummy1 should match filter.", true, relationship.MatchesRelationshipFilter(dummy1, true, false));
			AssertEquals("Dummy1 should match filter.", true, relationship.MatchesRelationshipFilter(dummy1, true, true));

			dummy1.IsActive = true;
			((ICollectionRelationship)relationship).AddToRelationship(dummy2);
			AssertEquals("Precondition: 2 business objects.", 2, relationship.LoadBusinessObjects(Factory, new ZQuery()).Length);

			dummy1.Delete();
			dummy2.Row.AcceptChanges();
			dummy2.Delete();
			AssertEquals("Precondition: RowState.", DataRowState.Detached, dummy1.Row.RowState);
			AssertEquals("Precondition: RowState.", DataRowState.Deleted, dummy2.Row.RowState);

			AssertEquals("Dummy1 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy1, false, false));
			AssertEquals("Dummy1 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy1, false, true));
			AssertEquals("Dummy1 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy1, true, false));
			AssertEquals("Dummy1 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy1, true, true));

			AssertEquals("Dummy2 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy2, false, false));
			AssertEquals("Dummy2 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy2, false, true));
			AssertEquals("Dummy2 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy2, true, false));
			AssertEquals("Dummy2 should *not* match filter.", false, relationship.MatchesRelationshipFilter(dummy2, true, true));
		}

		#endregion

		#region AddToRelationship / RemoveFromRelationship / Clear

		public void TestAddToRemoveFromRelationship()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();

			Collection.Add(dummy1);
			AssertEquals("1 item in the collection", 1, Collection.Count);
			AssertEquals(true, Collection.Contains(dummy1));
			AssertEquals(false, Collection.Contains(dummy2));

			Collection.RemoveFromRelationship(dummy1);
			AssertEquals("No items in the collection", 0, Collection.Count);
			AssertEquals(false, Collection.Contains(dummy1));
			AssertEquals(false, Collection.Contains(dummy2));
		}

		public void TestClear()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new AdhocCollectionRelationship(typeof(DummyBusinessObject)));

			dummy1.Z0_Bool = true;
			collection.Add(dummy1);
			collection.Add(dummy2);

			AssertEquals("PreCondition", 2, collection.Count);
			((System.Collections.IList)collection).Clear();
			AssertEquals("Elements should have been cleared", 0, collection.Count);
		}

		public void TestDelete()
		{
			DummyBusinessObject dummy1 = Factory.New<DummyBusinessObject>();
			DummyBusinessObject dummy2 = Factory.New<DummyBusinessObject>();
			var collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, new AdhocCollectionRelationship(typeof(DummyBusinessObject)));

			collection.Add(dummy1);
			collection.Add(dummy2);
			AssertEquals("PreCondition", 2, collection.Count);

			dummy1.Delete();
			AssertEquals("Element should be removed from relationship", 1, collection.Count);
		}

		#endregion

		#region Test Classes

		class TestAdhocCollectionRelationship : AdhocCollectionRelationship
		{
			public TestAdhocCollectionRelationship(Type elementType)
				: base(elementType)
			{
			}

			public new AdhocCollectionRelationship Clone()
			{
				return (AdhocCollectionRelationship)base.Clone();
			}
		}

		#endregion

		#region Implementation

		ActiveBusinessObjectCollection<DummyBusinessObject> Collection
		{
			get { return collection ?? (collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, Relationship)); }
		}
		ActiveBusinessObjectCollection<DummyBusinessObject> collection;

		TestAdhocCollectionRelationship Relationship
		{
			get { return relationship ?? (relationship = new TestAdhocCollectionRelationship(typeof(DummyBusinessObject))); }
		}
		TestAdhocCollectionRelationship relationship;

		#endregion
	}
}
