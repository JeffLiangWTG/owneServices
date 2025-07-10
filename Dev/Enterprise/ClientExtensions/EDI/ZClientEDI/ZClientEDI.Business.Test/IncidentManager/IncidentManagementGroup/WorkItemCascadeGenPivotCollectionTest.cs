using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.ProcessManagement.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(WorkItemCascadeGenPivotCollection))]
	public class WorkItemCascadeGenPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<WorkItemCascadeGenPivotCollection>
	{
		protected override WorkItemCascadeGenPivotCollection GetCollectionToTest()
		{
			return new WorkItemCascadeGenPivotCollection(ParentBizo);
		}

		BusinessObject ParentBizo => bizo ?? (bizo = Factory.NewWithValidTestData<IncidentManagementGroup>());
		BusinessObject bizo;

		public void TestAddChild()
		{
			var bizo = Factory.New<IncidentManagementGroup>();
			var childBizo = (BusinessObject)Factory.New<WorkItem>();
			var collection = new WorkItemCascadeGenPivotCollection(bizo);
			var pivot = collection.AddChild(childBizo);

			CombineAssertions(() =>
			{
				AssertEquals("XX_Relation1TableCode", bizo.TablePrefix, pivot.XX_Relation1TableCode);
				AssertEquals("XX_Relation1ID", bizo.PK, pivot.XX_Relation1ID);
				AssertEquals("XX_Relation2TableCode", childBizo.TablePrefix, pivot.XX_Relation2TableCode);
				AssertEquals("XX_Relation2ID", childBizo.PK, pivot.XX_Relation2ID);
			});
		}
	}
}
