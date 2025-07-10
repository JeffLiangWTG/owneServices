using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using CargoWise.Common;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class UnapprovedTransactionCandidateCollection : TransactionHeaderCollection
	{
		public UnapprovedTransactionCandidateCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public UnapprovedTransactionCandidateCollection(BusinessObjectFactory factory, ZQuery additionalFilters)
			: base(factory)
		{
			this.AdditionalFilters = additionalFilters;
		}
		readonly ZQuery AdditionalFilters;

		#region Overrides

		protected override bool AllowNewCore
		{
			get { return false; }
		}

		protected override BusinessObject AddNewCore()
		{
			throw new NotSupportedException();
		}

		protected override BusinessObject AddNewCore(Type bizOType)
		{
			throw new NotSupportedException();
		}

		protected override void OnLoaded()
		{
			base.OnLoaded();
			DoCollectionValidation();
		}

		#endregion

		#region Collection Validation

		public void DoCollectionValidation()
		{
			foreach (TransactionHeader transaction in this)
			{
				if (!transaction.IsValidationSuspended)
				{
					transaction.ClearRowNotifications();
				}
			}
			CalculateWarnings();
		}

		void CalculateWarnings()
		{
			foreach (TransactionHeader transaction in this)
			{
				if (transaction.IsValidationSuspended ||
					transaction.AH_Ledger != LedgerTypes.AccountsReceivable) //i.e. It isn't a sister company invoice
				{
					continue;
				}
				ZQuery existingTranQuery = new ZQuery();
				existingTranQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionNum, transaction.TransactionNumberPrefixed);
				existingTranQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
				existingTranQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, ZArchitecture.Core.LedgerTypes.AccountsPayable);
				existingTranQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, OrgProxiesForIssuingCompanyAndBranch(transaction));
				existingTranQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, transaction is ARInvoice ? ZArchitecture.Core.TransactionTypes.Invoice : ZArchitecture.Core.TransactionTypes.CreditNote);
				TransactionHeader[] existingInvoices = Factory.Load<TransactionHeader>(existingTranQuery);

				if (existingInvoices.Length > 0)
				{
					transaction.AddRowWarning(Res.GetString("e870fd1a-b36f-42c7-b49c-88047c234627", "AP {0} with the same Transaction Number already exists", transaction is ARInvoice ? Res.GetString("244f4ac9-9f0c-40a6-97eb-9c2564a10f02", "Invoice") : Res.GetString("4a8bf87d-4ded-450b-8597-dcabf55fda01", "Credit Note")));
				}

				var invoice = transaction as InvoicingBase;
				if (invoice == null)
				{
					continue;
				}

				List<ZString> jobNumbers = new List<ZString>();
				foreach (InvoicingLineBase line in invoice.Lines)
				{
					if (line.Job != null)
					{
						if (!jobNumbers.Contains(line.Job.JH_JobNum))
						{
							jobNumbers.Add(line.Job.JH_JobNum);
						}
					}
				}

				if (jobNumbers.Any())
				{
					var query = new ZQuery();
					query.AddToFilter(JobHeaderSchema.JH_GC, GlbCompany.CurrentCompany.PK);
					query.AddToFilter(JobHeaderSchema.JH_JobNum, jobNumbers);
					query.AddToFilter(JobHeaderSchema.JH_Status, JobHeaderStatus.Closed.Code);

					var closedJobs = invoice.Factory.Load<Job>(query);

					if (closedJobs.Any())
					{
						var closedJobNumbers = new ZStringBuilder(closedJobs.Select(x => x.JH_JobNum));
						string message = Res.GetString("23e4a480-bdc7-4a5d-9090-beb60ccbff56", "The following job(s) are closed: {0}. ", closedJobNumbers.ToStringWithDelimiterBetweenAppends(", "));

						if (!JobReopenSecurityCheckHelper.CanReopenJob_NonInteractiveSecurityCheck(invoice.Factory, closedJobs))
						{
							message += AccountingConstants.ReopenClosedJobSecurityMessages.ErrorMessage;
						}
						else
						{
							message += AccountingConstants.ReopenClosedJobSecurityMessages.WarningMessage;
						}

						transaction.AddRowWarning(message);
					}
				}
			}
		}

		ZGuid[] OrgProxiesForIssuingCompanyAndBranch(TransactionHeader transaction)
		{
			var orgs = new List<ZGuid>();
			if (transaction.Branch.GB_OH_OrgProxy.IsValid)
			{
				orgs.Add(transaction.Branch.GB_OH_OrgProxy);
			}
			if (transaction.Company.GC_OH_OrgProxy.IsValid)
			{
				orgs.Add(transaction.Company.GC_OH_OrgProxy);
			}
			return orgs.ToArray();
		}

		#endregion

		#region Filtering

		public string AH_DescFilter { get; set; }

		protected override ZQuery CreateRelationshipFilter()
		{
			var subQueryForAR = getBasicArQuery();
			subQueryForAR.AddToFilter(AccTransactionHeaderSchema.AH_FullyPaidDate, SQLComparisonOperator.Equal, null);

			var subQueryForAR_Canceled = getBasicArQuery();
			subQueryForAR_Canceled.AddToFilter(AccTransactionHeaderSchema.AH_IsCancelled, true);
			subQueryForAR_Canceled.AddToFilter(AccTransactionHeaderSchema.AH_OutstandingAmount, SQLComparisonOperator.Equal, 0);
			subQueryForAR_Canceled.AddToFilter(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, SQLComparisonOperator.NotEqual, null);
			subQueryForAR_Canceled.AddSubQuery(AccTransactionHeaderSchema.AH_TransactionBelongsToGroup, getPostedOriginalsQuery(), JoinCondition.And);

			var subQueryForUA = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
			subQueryForUA.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.UnapprovedPayableTransactions);
			subQueryForUA.AddToFilter(AccTransactionHeaderSchema.AH_GC, GlbCompany.CurrentCompany.PK);
			subQueryForUA.AddToFilter(AccTransactionHeaderSchema.AH_PostedInternal, false);

			var subQueryUnion = new ZDBOnlyQuery(typeof(AccTransactionHeader));

			if (AllBranchWithPayables.Any())
			{
				subQueryForUA.AddAsUnionQuery(subQueryForAR_Canceled, true);
				subQueryForUA.AddAsUnionQuery(subQueryForAR, true);
			}

			subQueryUnion.AddSubQuery(AccTransactionHeaderSchema.PK, subQueryForUA, JoinCondition.And);

			var result = new ZQuery(subQueryUnion);

			if (AdditionalFilters != null)
			{
				result.AddToFilter(AdditionalFilters);
			}

			result.AddOptionRecompileConditionally = true;
			return result;

			ZDBOnlySubQuery getBasicArQuery()
			{
				var arQuery = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
				arQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, LedgerTypes.AccountsReceivable);
				arQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionType, GetTransactionTypesToFilter(GlbCompany.CurrentCompany.PK));
				arQuery.AddToFilter(AccTransactionHeaderSchema.AH_OH, OrgProxiesOfCurrentCompanyAndItsBranches);
				arQuery.AddToFilter(AccTransactionHeaderSchema.AH_GB, AllBranchWithPayables);
				arQuery.AddToFilter(AccTransactionHeaderSchema.AH_PostedInternal, false);
				return arQuery;
			}

			ZDBOnlySubQuery getPostedOriginalsQuery()
			{
				var postedOriginals = new ZDBOnlySubQuery(typeof(AccTransactionHeader), AccTransactionHeaderSchema.PK);
				postedOriginals.AddToFilter(AccTransactionHeaderSchema.AH_PostedInternal, true);
				return postedOriginals;
			}
		}

		ZGuid[] OrgProxiesOfCurrentCompanyAndItsBranches
		{
			get
			{
				if (orgProxiesOfCurrentCompanyAndItsBranches == null)
				{
					var companyOrgProxy = GlbCompany.CurrentCompany.GC_OH_OrgProxy;
					var branchOrgProxies = GlbCompany.CurrentCompany.Branches.Select(x => x.GB_OH_OrgProxy);
					orgProxiesOfCurrentCompanyAndItsBranches = branchOrgProxies.Append(companyOrgProxy).Distinct().OrderBy(x => x).ToArray();
				}
				return orgProxiesOfCurrentCompanyAndItsBranches;
			}
		}
		ZGuid[] orgProxiesOfCurrentCompanyAndItsBranches;

		public static ZDBOnlyQuery GetBranchOrCompanySubQuery(ZDBOnlySubQuery companyQuery, ZDBOnlySubQuery branchQuery)
		{
			var branchOrCompanySubQuery = new ZDBOnlyQuery(typeof(AccTransactionHeader));
			branchOrCompanySubQuery.AddSubQuery(AccTransactionHeaderSchema.AH_GB, GlbBranchSchema.PK, branchQuery, JoinCondition.Or);
			branchOrCompanySubQuery.AddSubQuery(AccTransactionHeaderSchema.AH_GC, companyQuery, JoinCondition.Or);
			return branchOrCompanySubQuery;
		}

		#region GetCompanySubQuery

		public static ZDBOnlySubQuery GetCompanySubQuery(ZDBOnlySubQuery orgProxyQuery) => GetCompanySubQuery(orgProxyQuery, ZGuid.Empty);

		public static ZDBOnlySubQuery GetCompanySubQuery(ZGuid oh_pk) => GetCompanySubQuery(null, oh_pk);

		static ZDBOnlySubQuery GetCompanySubQuery(ZDBOnlySubQuery orgProxyQuery, ZGuid oh_pk)
		{
			var companyQuery = new ZDBOnlySubQuery(typeof(GlbCompany), GlbCompanySchema.PK);
			if (orgProxyQuery != null)
			{
				companyQuery.AddSubQuery(GlbCompanySchema.GC_OH_OrgProxy, orgProxyQuery, JoinCondition.And);
			}
			else
			{
				companyQuery.AddToFilter(GlbCompanySchema.GC_OH_OrgProxy, oh_pk);
			}

			return companyQuery;
		}

		#endregion

		#region GetBranchSubQuery

		public static ZDBOnlySubQuery GetBranchSubQuery(ZDBOnlySubQuery orgProxyQuery) => GetBranchSubQuery(orgProxyQuery, ZGuid.Empty);

		public static ZDBOnlySubQuery GetBranchSubQuery(ZGuid oh_pk) => GetBranchSubQuery(null, oh_pk);

		static ZDBOnlySubQuery GetBranchSubQuery(ZDBOnlySubQuery orgProxyQuery, ZGuid oh_pk)
		{
			var branchQuery = new ZDBOnlySubQuery(typeof(GlbBranch), GlbBranchSchema.GB_GC);
			if (orgProxyQuery != null)
			{
				branchQuery.AddSubQuery(GlbBranchSchema.GB_OH_OrgProxy, orgProxyQuery, JoinCondition.And);
			}
			else
			{
				branchQuery.AddToFilter(GlbBranchSchema.GB_OH_OrgProxy, oh_pk);
			}

			return branchQuery;
		}

		#endregion

		ZGuid[] AllBranchWithPayables => ValidBranchAndCompany.AllBranchWithPayables;

		(ZGuid[] AllCompaniesPKsExceptCurrent, ZGuid[] AllBranchWithPayables) ValidBranchAndCompany
		{
			get
			{
				if (validBranchAndCompany == default)
				{
					var branches = LoadValidBranchAndCompany(Factory);
					var allBranchWithPayables = branches.Select(x => x.GB_PK).Distinct().ToArray();
					var allCompaniesPKsExceptCurrent = branches.Select(x => x.GB_GC).Distinct().ToArray();
					validBranchAndCompany = (allCompaniesPKsExceptCurrent, allBranchWithPayables);
				}

				return validBranchAndCompany;
			}
		}
		(ZGuid[] allCompaniesPKsExceptCurrent, ZGuid[] allBranchWithPayables) validBranchAndCompany;

		IEnumerable<(ZGuid GB_PK, ZGuid GB_GC)> LoadValidBranchAndCompany(IDbConnected conn)
		{
			const string query = @"
SELECT GB_PK,GB_GC
FROM (
	SELECT GB_PK,GB_GC
	FROM dbo.GlbBranch
	JOIN dbo.OrgHeader ON OH_PK = GB_OH_OrgProxy AND OH_IsActive = 1
	JOIN dbo.OrgCompanyData 
		ON	OH_PK = OB_OH
		AND OB_GC = @currentCompanyPK 
		AND OB_IsCreditor = 1 
	WHERE GB_GC <> @currentCompanyPK 
	UNION ALL
	SELECT GB_PK,GB_GC
	FROM dbo.GlbBranch
	JOIN dbo.GlbCompany ON GC_PK = GB_GC 
	JOIN dbo.OrgHeader ON OH_PK = GC_OH_OrgProxy AND OH_IsActive = 1
	JOIN dbo.OrgCompanyData 
		ON	GC_OH_OrgProxy = OB_OH
		AND OB_GC = @currentCompanyPK 
		AND OB_IsCreditor = 1 
	WHERE GB_GC <> @currentCompanyPK 
) as QueryResult
GROUP BY  GB_PK,GB_GC
";

			var dynamicCollection = new DynamicBusinessObjectCollection(Factory);
			dynamicCollection.Load(query, new ZSqlParameter[] {
					ZSqlParameter.New("@currentCompanyPK" , GlbCompany.CurrentCompany.PK.ToGuid(),GlbCompanySchema.PK)
				});

			return dynamicCollection.Select(dyBo =>
			{
				return (new ZGuid(dyBo["GB_PK"]), new ZGuid(dyBo["GB_GC"]));
			}).ToArray();
		}

		string[] GetTransactionTypesToFilter(ZGuid currentCompanyPK)
		{
			if (AccountingMasterFilesUtils.ShouldPreventCreateCreditNote(LedgerTypes.AccountsPayable, currentCompanyPK))
			{
				return new[] { TransactionTypes.Invoice };
			}
			else
			{
				return new[] { TransactionTypes.Invoice, TransactionTypes.CreditNote };
			}
		}
		#endregion
	}
}
