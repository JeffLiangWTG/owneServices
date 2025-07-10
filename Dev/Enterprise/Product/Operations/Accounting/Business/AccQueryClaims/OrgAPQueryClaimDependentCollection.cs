using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class OrgAPQueryClaimDependentCollection : OrgQueryClaimDependentCollection
	{
		public OrgAPQueryClaimDependentCollection(OrgHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public new APAccQueryClaim this[int index]
		{
			get { return (APAccQueryClaim)Elements[index]; }
		}

		public new APAccQueryClaim AddNew()
		{
			return (APAccQueryClaim)base.AddNew();
		}

		protected override void AddLedgerFilter(ZDBOnlyQuery relatedTablesQuery)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccQueryClaimSchema.AY_AH);
			subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsPayable);
			relatedTablesQuery.AddSubQuery(subQuery, JoinCondition.And);
		}
	}
}

