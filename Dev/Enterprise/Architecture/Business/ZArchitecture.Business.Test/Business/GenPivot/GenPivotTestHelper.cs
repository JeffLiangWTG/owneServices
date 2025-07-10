using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Business.Testing
{
	public sealed class GenPivotTestHelper : TestCaseWithFactory
	{
		public void AssertPivotExists(string relation1TableCode, ZGuid relation1ID, string relation2TableCode, ZGuid relation2ID, string relationType)
		{
			AssertPivotExists(relation1TableCode, relation1ID, relation2TableCode, relation2ID, relationType, Factory);
		}

		public void AssertPivotExists(string relation1TableCode, ZGuid relation1ID, string relation2TableCode, ZGuid relation2ID, string relationType, BusinessObjectFactory factory)
		{
			ZQuery query = new ZQuery();
			query.AddToFilter(GenPivotSchema.XX_Relation1TableCode, relation1TableCode);
			query.AddToFilter(GenPivotSchema.XX_Relation1ID, relation1ID);
			query.AddToFilter(GenPivotSchema.XX_Relation2TableCode, relation2TableCode);
			query.AddToFilter(GenPivotSchema.XX_Relation2ID, relation2ID);
			query.AddToFilter(GenPivotSchema.XX_RelationType, relationType);
			var pivot = factory.LoadTop1(typeof(GenPivot), query);
			Assert("GenPivots exists.", pivot != null);
		}
	}
}
