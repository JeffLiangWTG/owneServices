using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.AccQueryClaims
{
	public class APAccQueryClaimLookups : AccQueryClaimLookups
	{
		public APAccQueryClaimLookups(APAccQueryClaim parent)
			: base(parent)
		{
			if (!(parent is APAccQueryClaim))
			{
				throw new NotSupportedException("Please pass in an AccQuery claim. Currently constructor allows an auto until bug in generator is fixed.");
			}
		}

		APAccQueryClaim Claim
		{
			get { return (APAccQueryClaim)Parent; }
		}

		#region Collections

		#region Branches

		public override GlbBranchCollection Branches
		{
			get
			{
				GlbBranchNotCurrentCompanyRelatedCollection branchCollection = new GlbBranchNotCurrentCompanyRelatedCollection(Factory, new ZQuery(GlbBranchSchema.GB_IsActive, ZBool.True));
				ZQuery companyFilter = new ZQuery(GlbBranchSchema.GB_GC, Env.CurrentCompany.PK);
				if (Claim.AY_OH_Debtor.IsValid)
				{
					ZDBOnlyQuery orgProxyFilter = new ZDBOnlyQuery(typeof(GlbBranch));
					ZDBOnlySubQuery orgQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK);
					orgQuery.AddToFilter(GlbCompanySchema.GC_OH_OrgProxy, Claim.AY_OH_Debtor);
					orgProxyFilter.AddSubQuery(GlbBranchSchema.GB_GC, orgQuery, JoinCondition.And);

					companyFilter.AddToFilter(orgProxyFilter, JoinCondition.Or);

					orgProxyFilter = new ZDBOnlyQuery(typeof(GlbBranch));
					orgQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.PK);
					orgQuery.AddToFilter(GlbBranchSchema.GB_OH_OrgProxy, Claim.AY_OH_Debtor);
					orgProxyFilter.AddSubQuery(orgQuery, JoinCondition.And);

					companyFilter.AddToFilter(orgProxyFilter, JoinCondition.Or);
				}
				branchCollection.AdditionalRelationshipFilter = companyFilter;
				return branchCollection;
			}
		}

		#endregion

		#region TransactionHeaders

		public override AccTransactionHeaderCollection TransactionHeaders
		{
			get
			{
				var collection =  Claim.IsCreatedFromCASS ? new APTransactionHeaderCollection(Claim.Factory)  : new APInvoiceForClaimCollection(Claim, TransactionUniqueFilter);
				collection.SetOverrideNotificationWhenAdditionalFilterNotMet(Claim.Validation.GetErrorWhenTransactionHeadersAdditionalFilterNotMet);
				return collection;
			}
		}

		#endregion

		#endregion
	}
}

