using System;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ZArchitecture.Web.Business
{
	public class OrgContactAutoCompleteHelper : DependentBizOAutoCompleteHelper
	{
		public OrgContactAutoCompleteHelper(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		protected override Type BusinessObjectType
		{
			get { return typeof(OrgContact); }
		}

		protected override SchemaColumn TextColumn
		{
			get { return OrgContactSchema.OC_ContactName; }
		}

		protected override SchemaColumn KeyColumn
		{
			get { return OrgContactSchema.PK; }
		}

		protected override ZQuery GetListFilter(ZString key)
		{
			ZDBOnlyQuery filter = new ZDBOnlyQuery(BusinessObjectType);
			filter.AddToFilter(OrgContactSchema.OC_OH, ParentPK);
			filter.AddToFilter(OrgContactSchema.OC_ContactName, SQLComparisonOperator.StartsWith, key.SubstringSafe(0, OrgContactSchema.OC_ContactName.MaxLength));
			filter.AddToFilter(JoinCondition.And, OrgContactSchema.OC_IsActive, ZBool.True);
			return filter;
		}

		protected override ZQuery GetKeyFilter(ZString text)
		{
			ZQuery filter = base.GetKeyFilter(text);
			filter.AddToFilter(OrgContactSchema.OC_OH, ParentPK);

			return filter;
		}
	}
}
