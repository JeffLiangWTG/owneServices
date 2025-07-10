using CargoWise.EntityFramework.Testing;

namespace Enterprise.ZArchitecture.Business.Testing
{
	sealed class GenPivotCollectionRelationshipTest : TestCaseWithFactory
	{
		public void TestFilter()
		{
			var biz1 = Factory.New<DummyBaseBusinessObject>();
			var biz2 = Factory.New<DummyDependantBusinessObject>();
			var biz3 = Factory.New<DummyDependantBusinessObject>();

			var pivot12 = Factory.New<GenPivot>();
			pivot12.XX_Relation1TableCode = biz1.TablePrefix;
			pivot12.XX_Relation1ID = biz1.PK;
			pivot12.XX_Relation2TableCode = biz2.TablePrefix;
			pivot12.XX_Relation2ID = biz2.PK;
			pivot12.XX_RelationType = "AAA";

			var pivot31 = Factory.New<GenPivot>();
			pivot31.XX_Relation1TableCode = biz3.TablePrefix;
			pivot31.XX_Relation1ID = biz3.PK;
			pivot31.XX_Relation2TableCode = biz1.TablePrefix;
			pivot31.XX_Relation2ID = biz1.PK;
			pivot31.XX_RelationType = "AAA";

			var pivot23 = Factory.New<GenPivot>();
			pivot23.XX_Relation1TableCode = biz2.TablePrefix;
			pivot23.XX_Relation1ID = biz2.PK;
			pivot23.XX_Relation2TableCode = biz3.TablePrefix;
			pivot23.XX_Relation2ID = biz3.PK;
			pivot23.XX_RelationType = "AAA";

			var pivot12Unrelated = Factory.New<GenPivot>();
			pivot12Unrelated.XX_Relation1TableCode = biz2.TablePrefix;
			pivot12Unrelated.XX_Relation1ID = biz2.PK;
			pivot12Unrelated.XX_Relation2TableCode = biz1.TablePrefix;
			pivot12Unrelated.XX_Relation2ID = biz1.PK;
			pivot12Unrelated.XX_RelationType = "BBB";

			var relationship = new GenPivotCollectionRelationship(biz1, "AAA", true, true);
			AssertContainsExactElementsInAnyOrder(
					new[] { pivot12, pivot31 },
					Factory.Load<GenPivot>(relationship.RelationshipFilter)
				);

			var childrenRelationship = new GenPivotCollectionRelationship(biz1, "AAA", true, false);
			AssertContainsExactElementsInAnyOrder(
					new[] { pivot12 },
					Factory.Load<GenPivot>(childrenRelationship.RelationshipFilter)
				);

			var parentsRelationship = new GenPivotCollectionRelationship(biz1, "AAA", false, true);
			AssertContainsExactElementsInAnyOrder(
					new[] { pivot31 },
					Factory.Load<GenPivot>(parentsRelationship.RelationshipFilter)
				);
		}
	}
}
