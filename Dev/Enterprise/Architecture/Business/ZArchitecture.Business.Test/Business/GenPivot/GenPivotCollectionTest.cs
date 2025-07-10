using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.ZArchitecture.Business.Testing
{
	[TestedType(typeof(GenPivotCollection))]
	sealed class GenPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<GenPivotCollection>
	{
		#region Add

		public void TestAddChild()
		{
			var bizo = Factory.New<DummyBaseBusinessObject>();
			var childBizo = (BusinessObject)Factory.New<DummyDependantBusinessObject>();
			var collection = new GenPivotCollection(bizo, "AAA");
			var pivot = collection.AddChild(childBizo);

			CombineAssertions(() =>
			{
				AssertEquals("XX_Relation1TableCode", bizo.TablePrefix, pivot.XX_Relation1TableCode);
				AssertEquals("XX_Relation1ID", bizo.PK, pivot.XX_Relation1ID);
				AssertEquals("XX_Relation2TableCode", childBizo.TablePrefix, pivot.XX_Relation2TableCode);
				AssertEquals("XX_Relation2ID", childBizo.PK, pivot.XX_Relation2ID);
			});
		}

		public void TestAddParent()
		{
			var bizo = Factory.New<DummyBaseBusinessObject>();
			var bizo2 = Factory.New<DummyDependantBusinessObject>();
			var collection = new GenPivotCollection(bizo, "AAA");
			var pivot = collection.AddParent(bizo2);

			CombineAssertions(() =>
			{
				AssertEquals("XX_Relation1TableCode", bizo2.TablePrefix, pivot.XX_Relation1TableCode);
				AssertEquals("XX_Relation1ID", bizo2.PK, pivot.XX_Relation1ID);
				AssertEquals("XX_Relation2TableCode", bizo.TablePrefix, pivot.XX_Relation2TableCode);
				AssertEquals("XX_Relation2ID", bizo.PK, pivot.XX_Relation2ID);
			});
		}

		#endregion

		public void TestDefaultsForNewElement()
		{
			var bizo = Factory.New<DummyBaseBusinessObject>();
			var collection = new GenPivotCollection(bizo, "AAA");
			var pivot = collection.AddNew();
			var collection2 = new GenPivotCollection(bizo, "AAA", false, true);
			var pivot2 = collection2.AddNew();

			CombineAssertions(() =>
			{
				AssertEquals("XX_RelationType", "AAA", pivot.XX_RelationType);
				AssertEquals("XX_Relation1TableCode", bizo.TablePrefix, pivot.XX_Relation1TableCode);
				AssertEquals("XX_Relation1ID", bizo.PK, pivot.XX_Relation1ID);
				AssertEquals("XX_Relation2TableCode", "", pivot.XX_Relation2TableCode);
				AssertEquals("XX_Relation2ID", ZGuid.Empty, pivot.XX_Relation2ID);
			});

			CombineAssertions(() =>
			{
				AssertEquals("XX_RelationType", "AAA", pivot2.XX_RelationType);
				AssertEquals("XX_Relation1TableCode", "", pivot2.XX_Relation1TableCode);
				AssertEquals("XX_Relation1ID", ZGuid.Empty, pivot2.XX_Relation1ID);
				AssertEquals("XX_Relation2TableCode", bizo.TablePrefix, pivot2.XX_Relation2TableCode);
				AssertEquals("XX_Relation2ID", bizo.PK, pivot2.XX_Relation2ID);
			});
		}

		public void TestFindRelated()
		{
			var bizo = Factory.New<DummyBaseBusinessObject>();
			var child1 = Factory.New<DummyBaseBusinessObject>();
			var child2 = Factory.New<DummyBaseBusinessObject>();
			var parent1 = Factory.New<DummyBaseBusinessObject>();
			var parent2 = Factory.New<DummyBaseBusinessObject>();
			var collection1 = new GenPivotCollection(bizo, "AAA");
			var collection2 = new GenPivotCollection(bizo, "BBB");
			collection1.AddChild(child1);
			collection1.AddChild(child2);
			collection1.AddParent(parent1);
			collection1.AddParent(parent2);
			collection2.AddChild(child2);
			collection2.AddParent(parent1);

			AssertEquals(child1.PK, collection1.FindRelated(child1.PK).Relation2ID);
			AssertEquals(child2.PK, collection1.FindRelated(child2.PK).Relation2ID);
			AssertEquals(parent1.PK, collection1.FindRelated(parent1.PK).Relation1ID);
			AssertEquals(parent2.PK, collection1.FindRelated(parent2.PK).Relation1ID);

			AssertEquals(child2.PK, collection2.FindRelated(child2.PK).Relation2ID);
			AssertEquals(parent1.PK, collection2.FindRelated(parent1.PK).Relation1ID);

			AssertNull(collection1.FindRelated(ZGuid.NewZGuid()));
			AssertNull(collection2.FindRelated(child1.PK));
			AssertNull(collection2.FindRelated(parent2.PK));
		}

		public void TestAddRelatedIfNotExist()
		{
			var bizo = Factory.New<DummyBaseBusinessObject>();
			var child1 = Factory.New<DummyBaseBusinessObject>();
			var child2 = Factory.New<DummyBaseBusinessObject>();
			var parent1 = Factory.New<DummyBaseBusinessObject>();
			var parent2 = Factory.New<DummyBaseBusinessObject>();
			var collection1 = new GenPivotCollection(bizo, "AAA");
			var collection2 = new GenPivotCollection(bizo, "BBB", false);

			collection1.AddRelatedIfNotExist(child1);
			collection1.AddRelatedIfNotExist(child1);
			collection1.AddRelatedIfNotExist(child2);
			collection1.AddRelatedIfNotExist(child2);
			collection1.AddRelatedIfNotExist(child1);
			collection1.AddRelatedIfNotExist(child2);

			collection2.AddRelatedIfNotExist(parent1);
			collection2.AddRelatedIfNotExist(parent1);
			collection2.AddRelatedIfNotExist(parent2);
			collection2.AddRelatedIfNotExist(parent2);
			collection2.AddRelatedIfNotExist(parent1);
			collection2.AddRelatedIfNotExist(parent2);

			AssertEquals(2, collection1.Count);
			AssertEquals(child1.PK, collection1.FindRelated(child1.PK).Relation2ID);
			AssertEquals(child2.PK, collection1.FindRelated(child2.PK).Relation2ID);

			AssertEquals(2, collection2.Count);
			AssertEquals(parent1.PK, collection2.FindRelated(parent1.PK).Relation1ID);
			AssertEquals(parent2.PK, collection2.FindRelated(parent2.PK).Relation1ID);
		}

		protected override GenPivotCollection GetCollectionToTest()
		{
			return new GenPivotCollection(ParentBizo, "AAA");
		}

		protected override BusinessObject GetNewElementToAddToTheCollection()
		{
			BusinessObject bizo = Factory.NewWithValidTestData<DummyBaseBusinessObject>();
			var pivot = Factory.New<GenPivot>();
			pivot.XX_Relation1TableCode = ParentBizo.TablePrefix;
			pivot.XX_Relation1ID = ParentBizo.PK;
			pivot.XX_Relation2TableCode = bizo.TablePrefix;
			pivot.XX_Relation2ID = bizo.PK;
			pivot.XX_RelationType = "AAA";

			return pivot;
		}

		BusinessObject ParentBizo
		{
			get { return bizo ?? (bizo = Factory.NewWithValidTestData<DummyBaseBusinessObject>()); }
		}
		BusinessObject bizo;
	}
}
