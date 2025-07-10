using System;
using System.Data;
using CargoWise.ComponentModel;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class DependentRelationshipTest : TestCaseWithFactory
	{
		public void TestAddToRelationship_OnAddNew()
		{
			DummyDependantBusinessObject child = Collection.AddNew();
			AssertEquals("Foreign key set on AddNew()", Master.PK, child.ZD1_Z0);
		}

		public void TestAddToRelationship_OnAdd()
		{
			DummyDependantBusinessObject child = Factory.New<DummyDependantBusinessObject>();
			Collection.Add(child);
			AssertEquals("Foreign key set on AddNew()", Master.PK, child.ZD1_Z0);
		}

		public void TestRemoveFromRelationship()
		{
			DummyDependantBusinessObject child = Collection.AddNew();
			AssertEquals("Foreign key set on add to collection", Master.PK, child.ZD1_Z0);

			Collection.RemoveFromRelationship(child);
			AssertEquals("Foreign key reset on remove from collection", ZGuid.Empty, child.ZD1_Z0);
		}

		public void TestRelationshipFilter_ClusterKey()
		{
			var master = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			var relationship = new DependentRelationship(master, typeof(DummyDependantWithClusterKeyBusinessObject));
			CombineAssertions(() =>
			{
				AssertEquals("Use FK when master not in db.", "ZD1_Z0 = @CWO1_", relationship.RelationshipFilter.FilterString);
				Factory.Save();
				AssertEquals("RelationshipFilter cached.", "ZD1_Z0 = @CWO1_", relationship.RelationshipFilter.FilterString);
				master.Z0_Number = 123;
				AssertEquals("Use Cluster Key and FK when both children and parent are ClusterKey enabled, and RelationshipFilter refreshed by masterClusterKeyEntity.ClusterKeyPty.ValueChanged.", "ZD1_Number = @CWO1_ and ZD1_Z0 = @CWO2_", relationship.RelationshipFilter.FilterString);
			});
		}

		public void TestRelationshipFilter_FK()
		{
			var relationship = new DependentRelationship(Master, typeof(DummyDependantBusinessObject));
			CombineAssertions(() =>
			{
				AssertEquals("Use FK when either children or parent are not ClusterKey enabled.", "ZD1_Z0 = @CWO1_", relationship.RelationshipFilter.FilterString);
			});
		}

		public void TestAddToRelationship_ParentTableCode()
		{
			var relationship = new DependentRelationship(Master, typeof(DummyDependantBusinessObjectWithParentID), new ZQuery(), new SchemaGuidColumn(DummyDependentBizoSchema.Instance, "ZD1_ParentID", 1, DBNull.Value, isNullable: true));
			var child = Factory.New<DummyDependantBusinessObjectWithParentID>();
			((ICollectionRelationship)relationship).AddToRelationship(child);
			AssertEquals(Master.TablePrefix, child.ZD1_ParentTableCode);
			AssertEquals(Master.TableName, child.ZD1_ParentTable);
		}

		public void TestRemoveFromRelationship_ParentTableCode()
		{
			var relationship = new DependentRelationship(Master, typeof(DummyDependantBusinessObjectWithParentID), new ZQuery(), new SchemaGuidColumn(DummyDependentBizoSchema.Instance, "ZD1_ParentID", 1, DBNull.Value, isNullable: true));
			var child = Factory.New<DummyDependantBusinessObjectWithParentID>();
			((ICollectionRelationship)relationship).AddToRelationship(child);
			AssertEquals(Master.TablePrefix, child.ZD1_ParentTableCode);
			AssertEquals(Master.TableName, child.ZD1_ParentTable);
			((ICollectionRelationship)relationship).RemoveFromRelationship(child);
			AssertEquals(ZString.Empty, child.ZD1_ParentTableCode);
			AssertEquals(ZString.Empty, child.ZD1_ParentTable);
		}

		internal class DummyDependantBusinessObjectWithParentID : DummyDependantBusinessObject
		{
			public DummyDependantBusinessObjectWithParentID(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
			{
			}

			public ZGuid ZD1_ParentID { get; set; }

			[MaxLength(3)]
			public ZString ZD1_ParentTableCode { get; set; }
			public ZPropertyInfo ZD1_ParentTableCodeInfo => GetZPropertyInfo(nameof(ZD1_ParentTableCode));

			[MaxLength(35)]
			public ZString ZD1_ParentTable { get; set; }
			public ZPropertyInfo ZD1_ParentTableInfo => GetZPropertyInfo(nameof(ZD1_ParentTable));
		}

		public void TestAddToRelationship_ClusterKey()
		{
			var master = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			master.Z0_Number = 123;
			var relationship = new DependentRelationship(master, typeof(DummyDependantWithClusterKeyBusinessObject));
			var collection = new ActiveBusinessObjectCollection<DummyDependantWithClusterKeyBusinessObject>(Factory, relationship);
			CombineAssertions(() =>
			{
				var dummyChild1 = collection.AddNew();
				AssertEquals("ClusterKey should be set from parent.", 123, dummyChild1.ZD1_Number);
				var dummyChild2 = Factory.New<DummyDependantWithClusterKeyBusinessObject>();
				dummyChild2.ZD1_Number = 456;
				collection.Add(dummyChild2);
				AssertEquals("ClusterKey should be updated to match new parent.", 123, dummyChild2.ZD1_Number);
				master.Delete();
				var dummyChild3 = collection.AddNew();
				AssertEquals("ClusterKey should not default from a deleted parent.", 0, dummyChild3.ZD1_Number);
			});
		}

		public void TestRemoveFromRelationship_ClusterKey()
		{
			var master = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			master.Z0_Number = 123;
			var relationship = new DependentRelationship(master, typeof(DummyDependantWithClusterKeyBusinessObject));
			var collection = new ActiveBusinessObjectCollection<DummyDependantWithClusterKeyBusinessObject>(Factory, relationship);
			CombineAssertions(() =>
			{
				var dummyChild1 = collection.AddNew();
				AssertEquals("ClusterKey should be set from parent.", 123, dummyChild1.ZD1_Number);
				collection.RemoveFromRelationship(dummyChild1);
				AssertEquals("ClusterKey should be set to 0 after remove", ZInt.Zero, dummyChild1.ZD1_Number);
			});
		}

		public void TestOnRelationshipFilterChanged_ClusterKey()
		{
			var master = Factory.New<DummyWithDependentsAndClusterKeyBusinessObject>();
			master.Z0_Number = 0;
			var relationship = new DependentRelationship(master, typeof(DummyDependantWithClusterKeyBusinessObject));
			var collection = new ActiveBusinessObjectCollection<DummyDependantWithClusterKeyBusinessObject>(Factory, relationship);
			CombineAssertions(() =>
			{
				var dummyChild1 = collection.AddNew();
				AssertEquals("ClusterKey should be set from parent.", 0, dummyChild1.ZD1_Number);

				master.Z0_Number = 123;
				dummyChild1.ZD1_Number = 123;
				AssertCollectionContains("Collection membership should be retained after the cluster key is populated on save.", dummyChild1, collection);

				var dummyChild2 = collection.AddNew();
				AssertEquals("Adding another child should set ClusterKey from parent.", 123, dummyChild2.ZD1_Number);
				AssertEquals("And should match the relationship filter.", true, relationship.MatchesRelationshipFilter(dummyChild2, true, true));
				AssertCollectionContains("And be in the collection.", dummyChild2, collection);
			});
		}

		#region Equals / GetHashCode

		public void TestEquals()
		{
			AssertEquals(
				new DependentRelationship(Master, typeof(DummyDependantBusinessObject)),
				new DependentRelationship(Master, typeof(DummyDependantBusinessObject)));
			AssertNotEquals(
				new DependentRelationship(Master, typeof(DummyDependantBusinessObject)),
				new DependentRelationship(Master2, typeof(DummyDependantBusinessObject)));
			AssertNotEquals(
				new DependentRelationship(Master, typeof(DummyBusinessObject)),
				new DependentRelationship(Master, typeof(DummyDependantBusinessObject)));
		}

		public void TestEquals_WhenOverriddenWithAdditionalContext()
		{
			var relationship1 = new DependentRelationshipWithAdditionalContextForEquals(Master, typeof(DummyDependantBusinessObject));
			var relationship2 = new DependentRelationshipWithAdditionalContextForEquals(Master, typeof(DummyDependantBusinessObject));

			AssertEquals(relationship1, relationship2);

			relationship1.IsReallyEqual = false;

			AssertEquals(relationship1, relationship2);
			AssertEquals(relationship2, relationship1);

			relationship2.IsReallyEqual = false;

			AssertNotEquals(relationship1, relationship2);
			AssertNotEquals(relationship2, relationship1);

			relationship1.IsReallyEqual = true;
			relationship2.IsReallyEqual = true;

			AssertEquals(relationship1, relationship2);
			AssertEquals(relationship2, relationship1);
		}

		class DependentRelationshipWithAdditionalContextForEquals : DependentRelationship
		{
			internal DependentRelationshipWithAdditionalContextForEquals(BusinessObject master, Type elementType)
				: base(master, elementType)
			{
			}

			internal bool? IsReallyEqual { get; set; }

			protected override bool IsAdditionalContextEqual(DependentRelationship other)
			{
				return base.IsAdditionalContextEqual(other) && (IsReallyEqual ?? true);
			}
		}

		#endregion

		#region Implementation

		ActiveBusinessObjectCollection<DummyDependantBusinessObject> Collection
		{
			get { return collection ?? (collection = new ActiveBusinessObjectCollection<DummyDependantBusinessObject>(Factory, Relationship)); }
		}
		ActiveBusinessObjectCollection<DummyDependantBusinessObject> collection;

		DependentRelationship Relationship
		{
			get { return relationship ?? (relationship = new DependentRelationship(Master, typeof(DummyDependantBusinessObject))); }
		}
		DependentRelationship relationship;

		DummyWithDependentsBusinessObject Master
		{
			get { return master ?? (master = Factory.New<DummyWithDependentsBusinessObject>()); }
		}
		DummyWithDependentsBusinessObject master;

		DummyWithDependentsBusinessObject Master2
		{
			get { return master2 ?? (master2 = Factory.New<DummyWithDependentsBusinessObject>()); }
		}
		DummyWithDependentsBusinessObject master2;

		#endregion
	}
}
