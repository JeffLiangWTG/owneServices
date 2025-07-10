using CargoWise.EntityFramework;
using CargoWise.Types;

namespace Enterprise.Accounting.Business.Base.Matching
{
	public class SettlementOrganisation : NonPersistentBusinessObject, IObsoleteValidation
	{
		public SettlementOrganisation(MatchingBase matching)
			: base(matching.Factory)
		{
			this.MatchingFilterBizO = matching.MatchingFilterBizO;
		}

		public ZBool IsPrimaryOrgValidAndNonEmpty
		{
			get { return MatchingFilterBizO.PrimaryOrganization.IsValid; }
		}

		public ZPropertyInfo PrimaryOrganizationInfo
		{
			get { return MatchingFilterBizO.PrimaryOrganizationInfo; }
		}

		public MatchingFilterBusinessObject MatchingFilterBizO { get; }

		public ZBool IncludeAllAR
		{
			get { return fIncludeAllAR; }
			set
			{
				fIncludeAllAR = value;
				IncludeAllARInfo.RefreshBinding();
				SettlementOrgInfos.SetARLedgerOfAllElements(value);
			}
		}

		ZBool fIncludeAllAR;

		public ZPropertyInfo IncludeAllARInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeAllAR)); }
		}

		public ZBool IncludeAllAP
		{
			get { return fIncludeAllAP; }
			set
			{
				fIncludeAllAP = value;
				IncludeAllAPInfo.RefreshBinding();
				SettlementOrgInfos.SetAPLedgerOfAllElements(value);
			}
		}
		ZBool fIncludeAllAP;

		public ZPropertyInfo IncludeAllAPInfo
		{
			get { return GetZPropertyInfo(nameof(IncludeAllAP)); }
		}

		public OrgLedgerFilterCollection SettlementOrgInfos
		{
			get
			{
				if (settlementOrgInfos == null)
				{
					settlementOrgInfos = new OrgLedgerFilterCollection(Factory);
				}
				return settlementOrgInfos;
			}
		}
		OrgLedgerFilterCollection settlementOrgInfos;
	}
}
