
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.GlobalChargeCode;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Module
{
	public abstract class GlobalChargeCodeFilterBusinessObject : FilterStripBusinessObject
	{
		public GlobalChargeCodeFilterBusinessObject()
		{
		}

		#region Filters

		protected override ModuleFilterCollection GetModuleFiltersCore()
		{
			ModuleFilterCollection filters = new ModuleFilterCollection();
			filters.AddTextFilter("Code", AccGlobalChargeCodeMapSchema.YG_Code).MultilingualDescription = ResString.GetMultilingualString("Accounting|GlobalChargeCodeFilter|Code", "Code");
			filters.AddTextFilter("Description", AccGlobalChargeCodeMapSchema.YG_Desc).MultilingualDescription = ResString.GetMultilingualString("Accounting|GlobalChargeCodeFilter|Description", "Description");
			filters.AddGuidFilter("AP Charge Code", ModuleIDs.AccChargeCode, APChargeCodeFilter, ChargeCodes).MultilingualDescription = ResString.GetMultilingualString("Accounting|GlobalChargeCodeFilter|APChargeCode", "AP Charge Code");
			filters.AddGuidFilter("AR Charge Code", ModuleIDs.AccChargeCode, ARChargeCodeFilter, ChargeCodes).MultilingualDescription = ResString.GetMultilingualString("Accounting|GlobalChargeCodeFilter|ARChargeCode", "AR Charge Code");
			return filters;
		}

		ZQuery APChargeCodeFilter(ZGuid chargeCodePK)
		{
			return ChargeCodeFilter(chargeCodePK, ZArchitecture.Core.LedgerTypes.AccountsPayable);
		}

		ZQuery ARChargeCodeFilter(ZGuid chargeCodePK)
		{
			return ChargeCodeFilter(chargeCodePK, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
		}

		ZQuery ChargeCodeFilter(ZGuid chargeCodePK, string pivotType)
		{
			ZDBOnlyQuery globalChargeCodeQuery = new ZDBOnlyQuery(typeof(GlobalChargeCodeMap));

			ZDBOnlySubQuery pivotSubQuery = new ZDBOnlySubQuery(typeof(GlobalChargeCodeMapPivot), AccGlobalChargeCodeMapPivotSchema.YP_YG);
			pivotSubQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_TYPE, pivotType);
			pivotSubQuery.AddToFilter(AccGlobalChargeCodeMapPivotSchema.YP_AC, chargeCodePK);
			globalChargeCodeQuery.AddSubQuery(AccGlobalChargeCodeMapSchema.PK, pivotSubQuery, JoinCondition.And);

			return globalChargeCodeQuery;
		}

		#endregion

		#region LookUps

		public virtual AccChargeCodeCollection ChargeCodes
		{
			get { return FindboxLookupCollections.GetChargeCodeCollection(Factory); }
		}

		#endregion
	}
}
