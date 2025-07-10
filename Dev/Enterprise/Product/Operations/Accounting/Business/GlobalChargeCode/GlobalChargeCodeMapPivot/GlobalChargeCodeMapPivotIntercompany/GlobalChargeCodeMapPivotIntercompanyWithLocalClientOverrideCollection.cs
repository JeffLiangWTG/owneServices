using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection : GlobalChargeCodeMapPivotIntercompanyCollection
	{
		public GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection(BusinessObjectFactory factory)
			: base(factory)
		{ }

		public GlobalChargeCodeMapPivotIntercompanyWithLocalClientOverrideCollection(BusinessObjectFactory factory, ZGuid accGlobalChargeCodeMapPK)
			: base(factory, accGlobalChargeCodeMapPK)
		{ }

		protected override ZQuery CreateRelationshipFilter()
		{
			ZDBOnlyQuery relationshipFilter = (ZDBOnlyQuery)base.CreateRelationshipFilter();
			relationshipFilter.AddToFilter(new ZQuery(AccGlobalChargeCodeMapPivotSchema.YP_OH_LocalClientOverride, SQLComparisonOperator.NotEqual, null));
			return relationshipFilter;
		}

		public override void Add(BusinessObject businessObject)
		{
			((GlobalChargeCodeMapPivotIntercompany)businessObject).SupportLocalClientOverride = true;
			base.Add(businessObject);
		}

		protected override BusinessObject CreateBusinessObjectFromRow(System.Data.DataRow row)
		{
			GlobalChargeCodeMapPivotIntercompany pivot = (GlobalChargeCodeMapPivotIntercompany)base.CreateBusinessObjectFromRow(row);
			pivot.SupportLocalClientOverride = true;
			return pivot;
		}
	}
}

