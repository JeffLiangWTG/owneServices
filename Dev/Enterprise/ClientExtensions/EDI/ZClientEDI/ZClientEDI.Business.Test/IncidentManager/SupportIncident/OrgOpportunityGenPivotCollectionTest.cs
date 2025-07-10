using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;

namespace Enterprise.Client.EDI.IncidentManager.Business.Test
{
	[TestedType(typeof(OrgOpportunityGenPivotCollection))]
	public class OrgOpportunityGenPivotCollectionTest : ActiveBusinessObjectCollectionTestCase<OrgOpportunityGenPivotCollection>
	{
		protected override OrgOpportunityGenPivotCollection GetCollectionToTest()
		{
			return new OrgOpportunityGenPivotCollection(ParentBizo);
		}

		BusinessObject ParentBizo => bizo ?? (bizo = Factory.NewWithValidTestData<OrgOpportunity>());
		BusinessObject bizo;

		public void TestAddChild()
		{
			var bizo = Factory.New<OrgOpportunity>();
			var childBizo = (BusinessObject)Factory.New<SupportIncident>();
			var collection = new OrgOpportunityGenPivotCollection(bizo);
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
