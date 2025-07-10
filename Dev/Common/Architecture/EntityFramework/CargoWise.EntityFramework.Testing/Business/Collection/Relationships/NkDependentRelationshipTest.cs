using Enterprise.ZArchitecture.Schema;

namespace CargoWise.EntityFramework.Testing
{
	sealed class NkDependentRelationshipTest : TestCaseWithFactory
	{
		public void TestAddToRelationship_OnAddNew()
		{
			var child = Collection.AddNew();
			AssertEquals("Relationship set on AddNew()", Master.Z0_Code, child.Z0_FK_Code);
		}

		public void TestAddToRelationship_OnAdd()
		{
			var child = Factory.New<DummyBusinessObject>();
			Collection.Add(child);
			AssertEquals("Relationship set on AddNew()", Master.Z0_Code, child.Z0_FK_Code);
		}

		public void TestRemoveFromRelationship()
		{
			var child = Collection.AddNew();
			AssertEquals("Relationship set on add to collection", Master.Z0_Code, child.Z0_FK_Code);

			Collection.RemoveFromRelationship(child);
			Assert("Relationshi preset on remove from collection", child.Z0_FK_Code.IsEmpty);
		}

		#region Equals / GetHashCode

		public void TestEquals()
		{
			AssertEquals(
				new NkDependentRelationship(Master, typeof(DummyBusinessObject), null, DummyBizoSchema.Z0_FK_Code),
				new NkDependentRelationship(Master, typeof(DummyBusinessObject), null, DummyBizoSchema.Z0_FK_Code));

			AssertNotEquals(
				new NkDependentRelationship(Master, typeof(DummyBusinessObject), null, DummyBizoSchema.Z0_FK_Code),
				new NkDependentRelationship(Master2, typeof(DummyBusinessObject), null, DummyBizoSchema.Z0_FK_Code));

			AssertNotEquals(
				new NkDependentRelationship(Master, typeof(DummyBusinessObject), null, DummyBizoSchema.Z0_FK_Code),
				new NkDependentRelationship(Master, typeof(DummyDependantBusinessObject), null, DummyBizoSchema.Z0_FK_Code));
		}

		#endregion

		#region Implementation

		ActiveBusinessObjectCollection<DummyBusinessObject> Collection
		{
			get { return collection ?? (collection = new ActiveBusinessObjectCollection<DummyBusinessObject>(Factory, Relationship)); }
		}
		ActiveBusinessObjectCollection<DummyBusinessObject> collection;

		NkDependentRelationship Relationship
		{
			get { return relationship ?? (relationship = new NkDependentRelationship(Master, typeof(DummyBusinessObject), null, DummyBizoSchema.Z0_FK_Code)); }
		}
		NkDependentRelationship relationship;

		DummyBusinessObject Master
		{
			get
			{
				if (master == null)
				{
					master = Factory.New<DummyBusinessObject>();
					master.Z0_Code = "AAA";
				}
				return master;
			}
		}
		DummyBusinessObject master;

		DummyBusinessObject Master2
		{
			get
			{
				if (master2 == null)
				{
					master2 = Factory.New<DummyBusinessObject>();
					master2.Z0_Code = "BBB";
				}
				return master2;
			}
		}
		DummyBusinessObject master2;

		#endregion
	}
}
