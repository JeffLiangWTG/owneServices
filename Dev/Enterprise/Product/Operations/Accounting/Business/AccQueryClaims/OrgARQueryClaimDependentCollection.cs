using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class OrgARQueryClaimDependentCollection : OrgQueryClaimDependentCollection
	{
		public OrgARQueryClaimDependentCollection(OrgHeader parent, BusinessObjectFactory factory)
			: base(parent, factory)
		{
		}

		public new ARAccQueryClaim this[int index]
		{
			get { return (ARAccQueryClaim)Elements[index]; }
		}

		public new ARAccQueryClaim AddNew()
		{
			return (ARAccQueryClaim)base.AddNew();
		}

		protected override void AddLedgerFilter(ZDBOnlyQuery relatedTablesQuery)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccQueryClaimSchema.AY_AH);
			subQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsReceivable);
			relatedTablesQuery.AddSubQuery(subQuery, JoinCondition.And);
			relatedTablesQuery.AddToFilter(JoinCondition.Or, AccQueryClaimSchema.AY_AH, null);
		}
	}
}

