using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.APAutomation.APReconciliation;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.Business.APReconciliation
{
	internal class DraftInvoiceHeaderBasedAPReconciliationAccrualSource(AccDraftInvoiceHeader draftInvoiceHeader) : APReconciliationAccrualSourceBase
	{
		readonly AccDraftInvoiceHeader DraftInvoiceHeader = draftInvoiceHeader;

		public override BusinessObjectFactory Factory => DraftInvoiceHeader.Factory;

		public override ZGuid CompanyPK => DraftInvoiceHeader.AIH_GC_Company;

		public override IEnumerable<ZGuid> GetJobParentPKs()
		{
			var jobParentPks = new List<ZGuid>();

			var draftOpJobs = DraftInvoiceHeader.JobClusters.OfType<AccDraftInvoiceJobCluster>()
				.SelectMany(jc => jc.OperationalJobs).OfType<AccDraftInvoiceJob>();

			//non-consol job parents
			var filteredOpJobs = draftOpJobs.Where(op => APReconciliationNodeProvider.GetAccrualSourceType(op.AIJ_ParentTableCode) == AccrualSourceTypes.Job).ToList();
			if (filteredOpJobs.Count > 0)
			{
				jobParentPks.AddRange(filteredOpJobs.Select(djh => djh.AIJ_ParentID));
			}

			//consol related job parents
			filteredOpJobs = draftOpJobs.Where(op => APReconciliationNodeProvider.GetAccrualSourceType(op.AIJ_ParentTableCode) == AccrualSourceTypes.Consol).ToList();
			if (filteredOpJobs.Count > 0)
			{
				//PKs of apportioned shipments
				jobParentPks.AddRange(Consols.SelectMany(c => c.CostSupporter.ShipmentsListPKs));

				//PKs of consol used to load gateway consol jobs 
				jobParentPks.AddRange(filteredOpJobs.Select(j => j.AIJ_ParentID));
			}

			return jobParentPks.Distinct().ToList();
		}

		public override IEnumerable<ZGuid> GetConsolPKs() =>
			 GetConsolDraftInvoiceJobs().Select(djh => djh.AIJ_ParentID).ToArray();

		internal Job GetJob(ZGuid jobParentId) =>
				Jobs.SingleOrDefault(j => j.JH_ParentID == jobParentId);

		internal IJobCostingPlugIn GetConsol(ZGuid consolId) =>
				Consols.SingleOrDefault(c => c.PK == consolId);

		internal List<ZGuid> GetSettlementGroupCreditorPKs()
		{
			if (settlementGroupCreditorPKs == null)
			{
				var creditorHasValidSettlementGroup = DraftInvoiceHeader.Creditor.APSettlementGroup != null &&
													DraftInvoiceHeader.Creditor.APSettlementGroup.OH_IsActive;
				settlementGroupCreditorPKs = creditorHasValidSettlementGroup
						? GetCreditorsInSameSettlementGroup()
						: new List<ZGuid>();
			}

			return settlementGroupCreditorPKs;

			List<ZGuid> GetCreditorsInSameSettlementGroup()
			{
				var settlementGroupFilterQuery = new ZQuery();
				settlementGroupFilterQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_RelatedParty, DraftInvoiceHeader.Creditor.APSettlementGroupPK);
				settlementGroupFilterQuery.AddToFilter(OrgRelatedPartySchema.PR_PartyType, RelatedPartyTypeList.Codes.APSettlementGroup);
				settlementGroupFilterQuery.AddToFilter(OrgRelatedPartySchema.PR_GC, CompanyPK);
				settlementGroupFilterQuery.AddToFilter(OrgRelatedPartySchema.PR_OH_Parent, SQLComparisonOperator.NotEqual, DraftInvoiceHeader.AIH_OH_Creditor);

				return Factory.Load<OrgRelatedParty>(settlementGroupFilterQuery)
					.Select(p => p.PR_OH_Parent)
					.ToList();
			}
		}

		List<ZGuid> settlementGroupCreditorPKs;

		List<Job> Jobs
		{
			get
			{
				if (jobs == null)
				{
					jobs = LoadJobs().ToList();
				}
				return jobs;

				Job[] LoadJobs()
				{
					var jobParentPks = GetJobParentPKs();
					var filter = new ZQuery(JobHeaderSchema.JH_ParentID, jobParentPks);
					filter.AddToFilter(JobHeaderSchema.JH_GC, CompanyPK);
					return Factory.Load<Job>(filter);
				}
			}
		}
		List<Job> jobs;

		List<IJobCostingPlugIn> Consols
		{
			get
			{
				if (consols == null)
				{
					consols = new List<IJobCostingPlugIn>();
					var draftOpJobsGroups = GetConsolDraftInvoiceJobs()
						.GroupBy(op => op.AIJ_ParentTableCode)
						.ToList();

					if (draftOpJobsGroups.Count > 0)
					{
						foreach (var jobGroup in draftOpJobsGroups)
						{
							consols.AddRange(GetConsols(jobGroup.Key, jobGroup.Select(j => j.AIJ_ParentID).ToArray()));
						}
					}
				}
				return consols;
			}
		}
		List<IJobCostingPlugIn> consols;

		AccDraftInvoiceJob[] GetConsolDraftInvoiceJobs() =>
			DraftInvoiceHeader.JobClusters
				.OfType<AccDraftInvoiceJobCluster>()
				.SelectMany(jc => jc.OperationalJobs).OfType<AccDraftInvoiceJob>()
				.Where(op => APReconciliationNodeProvider.GetAccrualSourceType(op.AIJ_ParentTableCode) == AccrualSourceTypes.Consol)
				.ToArray();
	}
}
