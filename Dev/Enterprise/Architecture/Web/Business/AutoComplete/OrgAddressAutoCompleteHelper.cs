using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class OrgAddressAutoCompleteHelper : DependentBizOAutoCompleteHelper
	{
		public OrgAddressAutoCompleteHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type BusinessObjectType
		{
			get { return typeof(OrgAddress); }
		}

		protected override SchemaColumn TextColumn
		{
			get { return OrgAddressSchema.OA_Address1; }
		}

		protected override SchemaColumn KeyColumn
		{
			get { return OrgAddressSchema.PK; }
		}

		protected override SchemaColumn[] ColumnsToSelectForListFilter()
		{
			return new SchemaColumn[] {
				OrgAddressSchema.OA_Address2,
				OrgAddressSchema.OA_Code
			};
		}

		protected override ZQuery GetListFilter(ZString key)
		{
			var filter = new ZDBOnlyQuery(BusinessObjectType);
			filter.AddToFilter(OrgAddressSchema.OA_OH, ParentPK);

			var subQuery = GetTextSubQuery(key);
			filter.AddToFilter(subQuery);

			return filter;
		}

		protected ZQuery GetTextSubQuery(ZString key)
		{
			var subQuery = new ZQuery();

			subQuery.AddToFilter(JoinCondition.Or, TextColumn, SQLComparisonOperator.Contains, GetTextColumnFilterValue(key));
			subQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_Code, SQLComparisonOperator.Contains, key.SubstringSafe(0, OrgAddressSchema.OA_Code.MaxLength));
			subQuery.AddToFilter(JoinCondition.Or, OrgAddressSchema.OA_Address2, SQLComparisonOperator.Contains, key.SubstringSafe(0, OrgAddressSchema.OA_Address2.MaxLength));

			subQuery.AddToFilter(JoinCondition.And, OrgAddressSchema.OA_IsActive, ZBool.True);

			return subQuery;
		}

		protected override ZQuery GetKeyFilter(ZString text)
		{
			var filter = base.GetKeyFilter(text);
			filter.AddToFilter(OrgAddressSchema.OA_OH, ParentPK);

			return filter;
		}
	}
}
