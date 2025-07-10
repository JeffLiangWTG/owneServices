using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class ARAccQueryClaimCollection : AccQueryClaimCollection
	{
		public ARAccQueryClaimCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public ARAccQueryClaimCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, new ZQuery(), company)
		{
		}

		public ARAccQueryClaimCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
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

		protected override void AddAdditionalFilters(ZDBOnlyQuery relatedTablesQuery)
		{
			ZDBOnlySubQuery subQuery = new ZDBOnlySubQuery(typeof(AccQueryClaim), AccQueryClaimSchema.PK);

			ZDBOnlySubQuery assignedBranchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), AccQueryClaimSchema.AY_GB);

			ZGuid companyInContext = fCompany == null ? GlbCompany.CurrentCompany.PK : fCompany.PK;
			assignedBranchQuery.AddToFilter(GlbBranchSchema.GB_GC, companyInContext);
			subQuery.AddSubQuery(assignedBranchQuery, JoinCondition.And);
			subQuery.AddToFilter(JoinCondition.And, AccQueryClaimSchema.AY_AH, null);
			subQuery.AddSubQuery(GetTransactionQuery(LedgerTypes.AccountsReceivable, false, companyInContext), JoinCondition.Or);
			ZDBOnlySubQuery intercompanyClaimsQuery = new ZDBOnlySubQuery(typeof(AccQueryClaim), AccQueryClaimSchema.PK);
			intercompanyClaimsQuery.AddSubQuery(GetTransactionQuery(LedgerTypes.AccountsPayable, true, companyInContext), JoinCondition.Or);
			intercompanyClaimsQuery.AddSubQuery(assignedBranchQuery, JoinCondition.And);
			subQuery.AddSubQuery(intercompanyClaimsQuery, JoinCondition.Or);
			relatedTablesQuery.AddSubQuery(subQuery, JoinCondition.And);
		}
	}
}

