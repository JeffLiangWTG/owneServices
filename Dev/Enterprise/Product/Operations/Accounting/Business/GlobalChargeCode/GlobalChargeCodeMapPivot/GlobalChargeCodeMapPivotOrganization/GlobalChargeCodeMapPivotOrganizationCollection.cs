using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotOrganizationCollection : BusinessObjectCollection<GlobalChargeCodeMapPivotOrganization>
	{
		public GlobalChargeCodeMapPivotOrganizationCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlobalChargeCodeMapPivotOrganizationCollection(BusinessObjectFactory factory, ZGuid accGlobalChargeCodeMapPK)
			: base(factory)
		{
			this.AccGlobalChargeCodeMapPK = accGlobalChargeCodeMapPK;
		}

		readonly ZGuid AccGlobalChargeCodeMapPK;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((GlobalChargeCodeMapPivotOrganization)child).YP_YG = AccGlobalChargeCodeMapPK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZDBOnlyQuery relationshipFilter = new ZDBOnlyQuery(typeof(GlobalChargeCodeMapPivot));
			ZDBOnlySubQuery glbCompanyFilterSubQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.PK);
			glbCompanyFilterSubQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			relationshipFilter.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_AC, glbCompanyFilterSubQuery, JoinCondition.And);
			ZDBOnlySubQuery organizationFilterSubQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMap), AccGlobalChargeCodeMapSchema.PK);
			organizationFilterSubQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_OH, SQLComparisonOperator.NotEqual, null);
			relationshipFilter.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_YG, organizationFilterSubQuery, JoinCondition.And);
			if (!AccGlobalChargeCodeMapPK.IsEmpty && AccGlobalChargeCodeMapPK.IsValid)
			{
				relationshipFilter.AddToFilter(new ZQuery(AccGlobalChargeCodeMapPivotSchema.YP_YG, AccGlobalChargeCodeMapPK));
			}
			return relationshipFilter;
		}

		public ZGuid OrganisationPK { get; set; }

		public ZString GetCodeOfAPChargeCode()
		{
			ZString code = ZString.Empty;
			foreach (GlobalChargeCodeMapPivotOrganization pivot in this)
			{
				if (pivot.YP_TYPE == LedgerTypes.AccountsPayable)
				{
					code = pivot.ChargeCode.AC_Code;
					break;
				}
			}
			return code;
		}

		public ZString GetCodesOfARChargeCodes()
		{
			ZString codes = ZString.Empty;
			foreach (GlobalChargeCodeMapPivotOrganization pivot in this)
			{
				if (pivot.YP_TYPE == LedgerTypes.AccountsReceivable)
				{
					codes += pivot.ChargeCode.AC_Code + ", ";
				}
			}
			codes = codes.TrimEnd(", ".ToCharArray());
			return codes;
		}
	}
}

