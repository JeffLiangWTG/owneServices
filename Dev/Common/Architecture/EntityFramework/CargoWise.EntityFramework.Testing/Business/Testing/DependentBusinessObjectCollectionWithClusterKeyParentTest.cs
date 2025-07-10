using System.Linq;
using CargoWise.Types;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DependentBusinessObjectCollectionWithClusterKeyParentTest : TestCaseWithFactory
	{
		public void TestRelationshipFilter_ClusterKey()
		{
			var dummyParent = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			var collection = new DependentBusinessObjectCollectionWithClusterKeyParent(dummyParent, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("Use FK when both children and parent are ClusterKey enabled and parent not in db (cluster key = 0)", "ZD1_Z0 = @CWO1_", collection.RelationshipFilter.FilterString);
				dummyParent.Z0_Number = 123;
				Factory.Save();
				AssertEquals("Use Cluster Key and FK when both children and parent are ClusterKey enabled and parent in db (cluster key > 0)", "ZD1_Number = @CWO1_ and ZD1_Z0 = @CWO2_", collection.RelationshipFilter.FilterString);
			});
		}

		public void TestRelationshipFilter_FK()
		{
			var dummyParent = Factory.New<DummyWithDependentsBusinessObject>();
			var collection = new DependentBusinessObjectCollectionForFKTest(dummyParent, Factory);
			CombineAssertions(() =>
			{
				AssertEquals("Use FK when children is not ClusterKey enabled", "ZD1_Z0 = @CWO1_", collection.RelationshipFilter.FilterString);
			});
		}

		public void TestSetCollectionRelationships()
		{
			var dummyParent = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			var collection = new DependentBusinessObjectCollectionWithClusterKeyParent(dummyParent, Factory);
			dummyParent.Z0_Number = 123;

			CombineAssertions(() =>
			{
				var dummyChild1 = collection.AddNew();
				AssertEquals("new child default cluster key from parent", 123, dummyChild1.ZD1_Number);
				var dummyChild2 = Factory.New<DummyDependantWithClusterKeyBusinessObject>();
				dummyChild2.ZD1_Number = 456;
				collection.Add(dummyChild2);
				AssertEquals("set child cluster key from parent", 123, dummyChild2.ZD1_Number);
				dummyParent.Delete();
				var dummyChild3 = collection.AddNew();
				AssertEquals("no default cluster key from deleted parent", 0, dummyChild3.ZD1_Number);
			});
		}

		public void TestRemoveCollectionRelationships()
		{
			var dummyParent = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			var collection = new DependentBusinessObjectCollectionWithClusterKeyParent(dummyParent, Factory);
			dummyParent.Z0_Number = 123;

			CombineAssertions(() =>
			{
				var dummyChild1 = collection.AddNew();
				var dummyChild2 = Factory.New<DummyDependantWithClusterKeyBusinessObject>();
				dummyChild2.ZD1_Number = 456;
				collection.Add(dummyChild2);

				collection.Remove(dummyChild1);
				AssertEquals("dummyChild1.ZD1_Number set to 0 after removed", ZInt.Zero, dummyChild1.ZD1_Number);
				collection.RemoveAll();
				AssertEquals("dummyChild2.ZD1_Number set to 0 after removed", ZInt.Zero, dummyChild2.ZD1_Number);
			});
		}

		public void TestRemoveCollectionRelationships_DoNotRemoveClusterKeyWhenDeleting()
		{
			var dummyParent = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			var collection = new DependentBusinessObjectCollectionWithClusterKeyParent(dummyParent, Factory);
			dummyParent.Z0_Number = 123;

			CombineAssertions(() =>
			{
				var dummyChild1 = collection.AddNew();
				dummyChild1.OnDeleting += DummyChild_OnDeleting;
				var dummyChild2 = Factory.New<DummyDependantWithClusterKeyBusinessObject>();
				dummyChild2.OnDeleting += DummyChild_OnDeleting;
				collection.Add(dummyChild2);

				collection.RemoveAndDelete(dummyChild1);
				collection.RemoveAndDeleteAll();
			});

			void DummyChild_OnDeleting(object sender, System.EventArgs e)
			{
				AssertEquals("ClusterKey should still be set", 123, (sender as DummyDependantWithClusterKeyBusinessObject).ZD1_Number);
			}
		}

		public void TestLoadAllWhenMasterInDb()
		{
			var dummyParent = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			var collection = new DependentBusinessObjectCollectionWithClusterKeyParent(dummyParent, Factory);
			dummyParent.Z0_Number = 123;
			var dummyChild1 = collection.AddNew();

			var dummyChild2 = Factory.New<DummyDependantWithClusterKeyBusinessObject>();
			dummyChild2.ZD1_Z0 = dummyParent.PK;
			dummyChild2.ZD1_Number = 123;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var newDummyParent = newFactory.Load<DummyWithDependentsAndClusterKeyBusinessObject>(dummyParent.PK);
			var newCollection = new DependentBusinessObjectCollectionWithClusterKeyParent(newDummyParent, newFactory);
			newCollection.Load();

			CombineAssertions(() =>
			{
				Assert("dummyChild1 added to collection in new factory", newCollection.Any(x => x.PK == dummyChild1.PK));
				Assert("dummyChild2 created and linked to parent, should load to collection in new factory", newCollection.Any(x => x.PK == dummyChild2.PK));

				var dummyChild3 = newFactory.New<DummyDependantWithClusterKeyBusinessObject>();
				dummyChild3.ZD1_Z0 = dummyParent.PK;
				dummyChild3.ZD1_Number = dummyParent.Z0_Number;
				AssertCollectionNotContains("dummyChild3 before load to collection", dummyChild3, newCollection);

				newCollection.Load();
				AssertCollectionContains("dummyChild3 created and linked to parent in new factory but not saved to db, should also load to collection", dummyChild3, newCollection);
			});
		}
	}
}
