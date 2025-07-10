using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class APAccQueryClaimCollection : AccQueryClaimCollection
	{
		public APAccQueryClaimCollection(BusinessObjectFactory factory)
			: this(factory, new ZQuery())
		{
		}

		public APAccQueryClaimCollection(BusinessObjectFactory factory, GlbCompany company)
			: base(factory, new ZQuery(), company)
		{
		}

		public APAccQueryClaimCollection(BusinessObjectFactory factory, ZQuery filter)
			: base(factory, filter)
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

		protected override void AddAdditionalFilters(ZDBOnlyQuery relatedTablesQuery)
		{
			ZGuid companyInContext = fCompany == null ? GlbCompany.CurrentCompany.PK : fCompany.PK;
			relatedTablesQuery.AddSubQuery(GetTransactionQuery(LedgerTypes.AccountsPayable, false, companyInContext), JoinCondition.And);
		}
	}
}

