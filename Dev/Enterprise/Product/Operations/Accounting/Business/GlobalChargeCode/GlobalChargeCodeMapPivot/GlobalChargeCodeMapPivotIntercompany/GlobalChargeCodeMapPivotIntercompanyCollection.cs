using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.GlobalChargeCode
{
	public class GlobalChargeCodeMapPivotIntercompanyCollection : BusinessObjectCollection<GlobalChargeCodeMapPivotIntercompany>
	{
		public GlobalChargeCodeMapPivotIntercompanyCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public GlobalChargeCodeMapPivotIntercompanyCollection(BusinessObjectFactory factory, ZGuid accGlobalChargeCodeMapPK)
			: base(factory)
		{
			this.AccGlobalChargeCodeMapPK = accGlobalChargeCodeMapPK;
		}

		readonly ZGuid AccGlobalChargeCodeMapPK;

		protected override void SetDefaultsForNewChild(BusinessObject child)
		{
			base.SetDefaultsForNewChild(child);
			((GlobalChargeCodeMapPivotIntercompany)child).YP_YG = AccGlobalChargeCodeMapPK;
		}

		protected override ZQuery CreateRelationshipFilter()
		{
			ZDBOnlyQuery relationshipFilter = new ZDBOnlyQuery(typeof(GlobalChargeCodeMapPivot));
			ZDBOnlySubQuery glbCompanyFilterSubQuery = new ZDBOnlySubQuery(typeof(AccChargeCode), AccChargeCodeSchema.PK);
			glbCompanyFilterSubQuery.AddToFilter(AccChargeCodeSchema.AC_GC, GlbCompany.CurrentCompany.PK);
			relationshipFilter.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_AC, glbCompanyFilterSubQuery, JoinCondition.And);
			ZDBOnlySubQuery intercompanyFilterSubQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMap), AccGlobalChargeCodeMapSchema.PK);
			intercompanyFilterSubQuery.AddToFilter(AccGlobalChargeCodeMapSchema.YG_OH, null);
			relationshipFilter.AddSubQuery(AccGlobalChargeCodeMapPivotSchema.YP_YG, intercompanyFilterSubQuery, JoinCondition.And);
			if (!AccGlobalChargeCodeMapPK.IsEmpty && AccGlobalChargeCodeMapPK.IsValid)
			{
				relationshipFilter.AddToFilter(new ZQuery(AccGlobalChargeCodeMapPivotSchema.YP_YG, AccGlobalChargeCodeMapPK));
			}
			return relationshipFilter;
		}

		public ZString GetCodeOfAPChargeCode()
		{
			List<ZString> codes = new List<ZString>();
			foreach (GlobalChargeCodeMapPivot pivot in this)
			{
				if (pivot.YP_TYPE == LedgerTypes.AccountsPayable && !codes.Contains(pivot.ChargeCode.AC_Code))
				{
					codes.Add(pivot.ChargeCode.AC_Code);
				}
			}
			return string.Join(", ", codes);
		}

		public ZString GetCodesOfARChargeCodes()
		{
			List<ZString> codes = new List<ZString>();
			foreach (GlobalChargeCodeMapPivot pivot in this)
			{
				if (pivot.YP_TYPE == LedgerTypes.AccountsReceivable && !codes.Contains(pivot.ChargeCode.AC_Code))
				{
					codes.Add(pivot.ChargeCode.AC_Code);
				}
			}
			return string.Join(", ", codes);
		}
	}
}

