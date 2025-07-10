using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotIntercompanyWithoutLocalClientOverrideCollection : GlobalChargeCodeMapPivotIntercompanyCollection
	{
		public GlobalChargeCodeMapPivotIntercompanyWithoutLocalClientOverrideCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public GlobalChargeCodeMapPivotIntercompanyWithoutLocalClientOverrideCollection(BusinessObjectFactory factory, ZGuid accGlobalChargeCodeMapPK)
			: base(factory, accGlobalChargeCodeMapPK)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			ZDBOnlyQuery relationshipFilter = (ZDBOnlyQuery)base.CreateRelationshipFilter();
			relationshipFilter.AddToFilter(new ZQuery(AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride, SQLComparisonOperator.Equal, null));
			return relationshipFilter;
		}
	}
}

