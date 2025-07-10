using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public abstract class CashAdvanceFilterBusinessObjectForMatchingBase : CashAdvanceFilterBusinessObject
	{
		public CashAdvanceFilterBusinessObjectForMatchingBase()
			: this(null)
		{
		}

		public CashAdvanceFilterBusinessObjectForMatchingBase(MatchingFilterBusinessObject matchingFilterBizO)
		: base()
		{
			MatchingFilterBizO = matchingFilterBizO;
			QueryObjectType = typeof(AccCashAdvanceRequestHeader);
			((IFilterStripBusinessObjectInternals)this).LayoutContext = LayoutContextName;
		}

		MatchingFilterBusinessObject MatchingFilterBizO { get; }

		protected override void AddStatusFilter(ModuleFilterCollection moduleFilters)
		{
			base.AddStatusFilter(moduleFilters);
			var statusFilter = moduleFilters[StatusFilterDescription] as ModuleTextFilter;
			if (statusFilter != null)
			{
				statusFilter.Visibility = FilterVisibility.AlwaysAppliedAndHidden;
				statusFilter.Property = CashAdvanceStatusCodes.RequestHeader.Requested;
				statusFilter.IsActive = true;
			}
		}

		public override ZQuery Filter
		{
			get
			{
				ZQuery query = base.Filter;
				query.AddToFilter(GetOrgQuery());
				return query;
			}
		}

		ZQuery GetOrgQuery()
		{
			if (MatchingFilterBizO != null)
			{
				var (receivableOrgs, payableOrgs, _) = MatchingFilterBizO.GroupSettlementOrgsByLedger();
				if (LedgerType == LedgerTypes.AccountsReceivable && receivableOrgs.Count > 0)
				{
					var orgQuery = new ZQuery();
					orgQuery.AddToFilter(AccCashAdvanceRequestHeaderSchema.CAH_OH_Organization, receivableOrgs.ToArray());
					return orgQuery;
				}
				if (LedgerType == LedgerTypes.AccountsPayable && payableOrgs.Count > 0)
				{
					var orgQuery = new ZQuery();
					orgQuery.AddToFilter(AccCashAdvanceRequestHeaderSchema.CAH_OH_Organization, payableOrgs.ToArray());
					return orgQuery;
				}
			}
			return ZQuery.NoResultQuery;
		}

		protected abstract string LayoutContextName { get; }
	}
}
