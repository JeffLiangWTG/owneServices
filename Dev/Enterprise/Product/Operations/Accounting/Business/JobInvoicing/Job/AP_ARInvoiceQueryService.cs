using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Extensions;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.Accounting.Integration;
using Enterprise.Customs.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.JobInvoicing
{
	public class AP_ARInvoiceQueryService : IAccountingAP_ARInvoiceQuery
	{
		public AP_ARInvoiceQueryResult GetPostingDetails(ICustomsJobInfo declaration, ZGuid[] customsDSBCharges, bool postAPCustomsDSB, bool postARCustomsDSB)
		{
			AP_ARInvoiceQueryResult result = new AP_ARInvoiceQueryResult();

			Job job = GetJob(declaration);

			if (job != null)
			{
				List<ZGuid> dSBCharges = new List<ZGuid>(customsDSBCharges);

				List<Charge> wIPs = new List<Charge>();

				foreach (Charge charge in job.Charges)
				{
					if (!charge.IsCostPosted && !charge.IsCommentChargeCode && !charge.IsRevenueCharge && !charge.JR_IsApportioned)
					{
						if (!dSBCharges.Contains(charge.JR_AC) || postAPCustomsDSB)
						{
							result.NumberOfUnpostedAPCharges++;

							if (charge.HasValidDataForCostPosting || dSBCharges.Contains(charge.JR_AC))//AP Invoice number, Amount and Date will be auto-populated for Customs DSB
							{
								result.NumberOfUnpostedAPChargesReadyForPosting++;
							}
						}
					}

					if (!charge.IsRevenuePosted && !charge.IsOverheadCharge)
					{
						if (!dSBCharges.Contains(charge.JR_AC) || postARCustomsDSB)
						{
							result.NumberOfUnpostedARCharges++;

							if (charge.HasValidDataForRevenuePosting)
							{
								result.NumberOfUnpostedARChargesReadyForPosting++;
							}

							wIPs.Add(charge);
						}
					}
				}

				IReceivablesPostingChargeCollection coll = new IReceivablesPostingChargeCollection();
				coll.AddRange(new TypedEnumerable<IReceivablesPostingCharge>(wIPs));
				PostingChargeCollection chargesReadyForARPosting = new PostingChargeDistributor().DistributeCharges(coll);
				result.NumberOfARInvoicesToBeIssued = chargesReadyForARPosting.Count;
			}

			return result;
		}

		public AP_ARInvoiceQueryResult GetInvoiceAmount(ICustomsJobInfo declaration, List<ZGuid> chargeCodesToMatch)
		{
			AP_ARInvoiceQueryResult result = new AP_ARInvoiceQueryResult();

			Job job = GetJob(declaration);

			if (job != null)
			{
				ZDecimal aPPostedAmount = 0m, aPUnpostedAmount = 0m, aRPostedAmount = 0m, aRUnpostedAmount = 0m;
				ZBool? aPFullyPaid = null;

				foreach (Charge charge in job.Charges)
				{
					if (chargeCodesToMatch.Contains(charge.JR_AC))
					{
						if (charge.IsCostPosted)
						{
							aPPostedAmount += charge.JR_LocalCostAmt;
						}
						else
						{
							aPUnpostedAmount += charge.JR_LocalCostAmt;
						}

						if (charge.IsRevenuePosted)
						{
							aRPostedAmount += charge.JR_LocalSellAmt;
						}
						else
						{
							aRUnpostedAmount += charge.JR_LocalSellAmt;
						}

						if (charge.JR_LocalCostAmt != 0m)
						{
							MasterFiles.Business.AccTransactionHeader apInvoice = charge.APLine != null ? charge.APLine.TransactionHeader : null;

							bool fullyPaid = apInvoice != null && apInvoice.AH_OutstandingAmount == 0m;
							if (!aPFullyPaid.HasValue)
							{
								aPFullyPaid = fullyPaid;
							}

							aPFullyPaid &= fullyPaid;
						}
					}
				}

				if (!aPFullyPaid.HasValue)
				{
					aPFullyPaid = false;
				}

				result.SetAmounts(aPPostedAmount, aPUnpostedAmount, aPFullyPaid.Value, aRPostedAmount, aRUnpostedAmount);
			}

			return result;
		}

		public AP_ARInvoiceQueryResult GetTotalInvoicedDetails(ICustomsJobInfo declaration, List<ZGuid> chargeCodesToMatch, string descToMatch = "")
		{
			var factory = declaration.Factory;
			var declarationJobCacheService = DeclarationJobCacheService.GetDeclarationJobCacheService(factory);
			var isDeclarationAlreadyLoaded = declarationJobCacheService.CheckServiceContainsLoadedDeclaration(declaration.PK);
			if (!isDeclarationAlreadyLoaded)
			{
				var declarationPKsToExclude = declarationJobCacheService.NotLoadedDeclarationPKs.Union(declarationJobCacheService.LoadedDeclarationPKs);
				var loadDeclarationsQuery = new ZQuery() { FetchOnlyFromLocalCache = true };
				if (declarationPKsToExclude.Any())
				{
					loadDeclarationsQuery.AddToFilter(JobDeclarationSchema.PK, SQLComparisonOperator.NotEqual, declarationPKsToExclude);
				}
				var newDeclarationsInFactory = factory.Load<BaseJobDeclaration>(loadDeclarationsQuery);
				newDeclarationsInFactory.ForEach(x => AddDeclarationJob(x, declarationJobCacheService));
			}

			var job = GetJob(declaration);
			var result = new AP_ARInvoiceQueryResult();

			if (job != null)
			{
				var jobPKsToLoad = isDeclarationAlreadyLoaded ? new ZGuid[] { job.PK } : declarationJobCacheService.NotLoadedJobPKs;

				var lineQuery = new ZQuery();
				if (chargeCodesToMatch != null)
				{
					lineQuery.AddToFilter(AccTransactionLinesSchema.AL_AC, chargeCodesToMatch);
				}
				lineQuery.AddToFilter(AccTransactionLinesSchema.AL_LineType, ZArchitecture.Core.TransactionLineTypes.Revenue);
				lineQuery.AddToFilter(AccTransactionLinesSchema.AL_AH, SQLComparisonOperator.NotEqual, null);
				lineQuery.AddToFilter(AccTransactionLinesSchema.AL_JH, jobPKsToLoad);
				lineQuery.AddToFilter(AccTransactionLinesSchema.AL_GC, job.JH_GC);
				if (!string.IsNullOrEmpty(descToMatch))
				{
					lineQuery.AddToFilter(AccTransactionLinesSchema.AL_Desc, SQLComparisonOperator.Contains, descToMatch);
				}
				lineQuery.FetchOnlyFromLocalCache = isDeclarationAlreadyLoaded;

				IEnumerable<TransactionLine> lines;

				var externalFetchHintSupporter = (IExternalFetchHintSupporter)factory;
				using (externalFetchHintSupporter.SetupCreator())
				{
					externalFetchHintSupporter.AddTableFetchHintCreator(AccTransactionLinesSchema.Instance, GetTransactionLineFetchHints);
					lines = factory.Load<TransactionLine>(lineQuery).Where(x => x.AL_JH == job.PK);
				}

				InvoicingBase[] disbursementInvoices;

				if (lines.Any())
				{
					result.TotalBilledAmount = lines.Sum(x => x.AL_LineAmount);
					disbursementInvoices = factory.Load<InvoicingBase>(new ZQuery(AccTransactionHeaderSchema.PK, lines.Where(x => x.TransactionHeader.AH_Ledger == LedgerTypes.AccountsReceivable).Select(x => x.AL_AH).Distinct()));
				}
				else
				{
					var headerQuery = new ZQuery(AccTransactionHeaderSchema.AH_JH, jobPKsToLoad);
					headerQuery.AddToFilter(AccTransactionHeaderSchema.AH_Ledger, Enterprise.ZArchitecture.Core.LedgerTypes.AccountsReceivable);
					headerQuery.AddToFilter(AccTransactionHeaderSchema.AH_TransactionCategory, Enterprise.ZArchitecture.Core.InvoiceTypesList.Codes.DisbursementInvoice);
					headerQuery.AddToFilter(AccTransactionHeaderSchema.AH_GC, job.JH_GC);
					headerQuery.AddToFilter(AccTransactionHeaderSchema.AH_JH, job.PK);
					headerQuery.FetchOnlyFromLocalCache = isDeclarationAlreadyLoaded;
					disbursementInvoices = factory.Load<InvoicingBase>(headerQuery).ToArray();
				}

				result.TotalOutstandingAmount = disbursementInvoices.Sum(x => x.AH_OutstandingAmount);
				result.TotalInvoicedAmount = disbursementInvoices.Sum(x => x.AH_InvoiceAmount);

				if (!isDeclarationAlreadyLoaded)
				{
					declarationJobCacheService.MoveAllToLoaded();
				}
			}

			return result;
		}

		static IEnumerable<IFetchHint> GetTransactionLineFetchHints(IColumnIndexer line)
		{
			var headerPK = line.GetValue(AccTransactionLinesSchema.AL_AH);
			if (headerPK.IsValid)
			{
				// TransactionLineTypeDecider loads AccTransactionHeader to determine type of the loaded line.
				// At that point fetch strategy for line is not yet created.
				yield return new FetchHint(AccTransactionHeaderSchema.PK, headerPK);
			}
		}

		public void ClearServiceCache(BusinessObjectFactory factory)
		{
			var declarationJobCacheService = DeclarationJobCacheService.GetDeclarationJobCacheService(factory);
			declarationJobCacheService.ClearServiceCache();
		}

		void AddDeclarationJob(ICustomsJobInfo declaration, DeclarationJobCacheService declarationJobCacheService)
		{
			if (!declarationJobCacheService.CheckServiceContainsNotLoadedDeclaration(declaration.PK))
			{
				var job = GetJob(declaration);
				if (job != null)
				{
					declarationJobCacheService.AddDeclarationJob(declaration.PK, job.PK);
				}
			}
		}

		Job GetJob(ICustomsJobInfo declaration)
		{
			Job result = null;
			if (declaration != null)
			{
				result = new Job.Loader(declaration.TopLevelObjectForJobToReference).Load();
			}
			return result;
		}

		class DeclarationJobCacheService : IService
		{
			public static DeclarationJobCacheService GetDeclarationJobCacheService(BusinessObjectFactory factory)
			{
				var service = factory.ServiceContainer.GetService<DeclarationJobCacheService>();
				if (service == null)
				{
					factory.ServiceContainer.AddService(new DeclarationJobCacheService());
					service = factory.ServiceContainer.GetService<DeclarationJobCacheService>();
				}
				return service;
			}

			DeclarationJobCacheService()
			{
				loadedDeclarationJobs = new Dictionary<ZGuid, ZGuid>();
				notLoadedDeclarationJobs = new Dictionary<ZGuid, ZGuid>();
			}

			readonly Dictionary<ZGuid, ZGuid> loadedDeclarationJobs;
			readonly Dictionary<ZGuid, ZGuid> notLoadedDeclarationJobs;

			public ZGuid[] LoadedDeclarationPKs
			{
				get { return loadedDeclarationJobs.Keys.ToArray(); }
			}

			public ZGuid[] NotLoadedDeclarationPKs
			{
				get { return notLoadedDeclarationJobs.Keys.ToArray(); }
			}

			public ZGuid[] NotLoadedJobPKs
			{
				get { return notLoadedDeclarationJobs.Values.ToArray(); }
			}

			public void ClearServiceCache()
			{
				loadedDeclarationJobs.Clear();
			}

			public void AddDeclarationJob(ZGuid declarationPK, ZGuid jobPK)
			{
				notLoadedDeclarationJobs.Add(declarationPK, jobPK);
			}

			public void MoveAllToLoaded()
			{
				notLoadedDeclarationJobs.ForEach(x => loadedDeclarationJobs.Add(x.Key, x.Value));
				notLoadedDeclarationJobs.Clear();
			}

			public bool CheckServiceContainsLoadedDeclaration(ZGuid declarationPK)
			{
				return loadedDeclarationJobs.Keys.Contains(declarationPK);
			}

			public bool CheckServiceContainsNotLoadedDeclaration(ZGuid declarationPK)
			{
				return notLoadedDeclarationJobs.Keys.Contains(declarationPK);
			}
		}
	}
}