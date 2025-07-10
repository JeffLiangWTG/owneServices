using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public abstract class APReconciliationAccrualSourceBase : IAPReconciliationAccrualSource
	{
		public abstract IEnumerable<ZGuid> GetJobParentPKs();

		public abstract IEnumerable<ZGuid> GetConsolPKs();

		public IEnumerable<Charge> GetCharges(ZGuid jobParentPK)
		{
			var chargeAccruals = ChargeAccruals.Where(ca => ca.JobParentId == jobParentPK);
			if (chargeAccruals.Any())
			{
				return chargeAccruals.Single().Accruals;
			}
			return [];
		}

		public IEnumerable<AccrualsForAPReconciliation<Charge>> GetChargeAccruals() => ChargeAccruals;

		public IEnumerable<JobConsolCost> GetConsolCosts(ZGuid consolId)
		{
			var consolCostAccruals = ConsolCostAccruals.Where(ca => ca.JobParentId == consolId);
			if (consolCostAccruals.Any())
			{
				return consolCostAccruals.Single().Accruals;
			}
			return [];
		}

		public IEnumerable<AccrualsForAPReconciliation<JobConsolCost>> GetConsolCostAccruals() => ConsolCostAccruals;

		public abstract BusinessObjectFactory Factory { get; }

		public abstract ZGuid CompanyPK { get; }

		List<AccrualsForAPReconciliation<Charge>> ChargeAccruals
		{
			get
			{
				if (chargeAccruals == null)
				{
					chargeAccruals = [];

					var filteredOpJobParentPks = GetJobParentPKs();
					if (filteredOpJobParentPks.Any())
					{
						chargeAccruals.AddRange(LoadCharges(filteredOpJobParentPks));
					}
				}
				return chargeAccruals;
			}
		}
		List<AccrualsForAPReconciliation<Charge>> chargeAccruals;

		protected virtual AccrualsForAPReconciliation<Charge> GetChargeAccrualInstance() => new AccrualsForAPReconciliation<Charge>();

		protected virtual List<AccrualsForAPReconciliation<Charge>> LoadCharges(IEnumerable<ZGuid> jobParentPKs)
		{
			var accruals = new List<AccrualsForAPReconciliation<Charge>>();
			foreach (var pks in AccountingUtils.ChunksOf(jobParentPKs, 100))
			{
				var jobHeaderQuery = new ZQuery(JobHeaderSchema.JH_ParentID, pks);
				jobHeaderQuery.AddToFilter(JobHeaderSchema.JH_GC, CompanyPK);

				var jobHeadersCache = Factory.Load<JobHeader>(jobHeaderQuery);

				var jobHeaderSubFilter = new ZDBOnlySubQuery(typeof(JobHeader), JobHeaderSchema.PK, JobChargeSchema.JR_JH);
				// Add the primary keys from jobHeadersResult to the filter
				var jobHeaderPKs = jobHeadersCache.Select(jh => jh.PK).ToList();
				jobHeaderSubFilter.AddToFilter(JobHeaderSchema.PK, jobHeaderPKs);

				var accTransactionLineSubFilter = new ZDBOnlySubQuery(typeof(AccTransactionLines), AccTransactionLinesSchema.PK, JobChargeSchema.JR_AL_APLine);
				accTransactionLineSubFilter.AddToFilter(AccTransactionLinesSchema.AL_LineType, TransactionLineTypes.Accrual);

				var jobChargeFilter = new ZDBOnlyQuery(typeof(JobCharge));
				jobChargeFilter.AddSubQuery(jobHeaderSubFilter, JoinCondition.And);

				var accrualFilter = new ZDBOnlyQuery(typeof(JobCharge));
				accrualFilter.AddToFilter(JobChargeSchema.JR_AL_APLine, null);
				accrualFilter.AddSubQuery(accTransactionLineSubFilter, JoinCondition.Or);
				jobChargeFilter.AddToFilter(accrualFilter, JoinCondition.And);
				jobChargeFilter.AddToFilter(JobChargeSchema.JR_E6, null);

				/* above ZQuery would generate following sql query

				SELECT JR.*
				FROM JobCharge JR
				WHERE
				(
					JR_JH IN
					(
						SELECT JH_PK FROM dbo.JobHeader WHERE JH_ParentID = '4A7476D0-DAFD-4A4F-8524-A65BE979AA2C'
						AND
						JH_GC = '42BC088E-A7BE-4737-8ECE-1616F9BE49C6'
					)
				)
				AND
				(
					JR_AL_APLine is null
					OR
					JR_AL_APLine IN
					(
						SELECT AL_PK FROM dbo.AccTransactionLines WHERE AL_LineType = 'ACR'
					)
				)
				AND
				JR_E6 IS NULL*/

				var charges = Factory.Load<Charge>(jobChargeFilter);
				var chargesByJob = charges.ToLookup(c => new
				{
					ParentId = c.Job.JH_ParentID,
					JobNumber = c.Job.JH_JobNum,
					ParentTableCode = c.Job.JH_ParentTableCode
				});

				foreach (var job in jobHeadersCache)
				{
					var jobKey = new
					{
						ParentId = job.JH_ParentID,
						JobNumber = job.JH_JobNum,
						ParentTableCode = job.JH_ParentTableCode
					};

					var jobCharges = chargesByJob.Contains(jobKey)
						? chargesByJob[jobKey].ToList()
						: new List<Charge>();

					var jobResult = GetChargeAccrualInstance();
					jobResult.JobParentId = job.JH_ParentID;
					jobResult.JobNumber = job.JH_JobNum;
					jobResult.JobParentTableCode = job.JH_ParentTableCode;
					jobResult.AccrualType = AccrualSourceTypes.Job;
					jobResult.Accruals = jobCharges;
					accruals.Add(jobResult);
				}
			}

			return accruals;
		}

		List<AccrualsForAPReconciliation<JobConsolCost>> ConsolCostAccruals
		{
			get
			{
				if (consolCostAccruals == null)
				{
					consolCostAccruals = new List<AccrualsForAPReconciliation<JobConsolCost>>();
					var consolJobParentPKs = GetConsolPKs();
					if (consolJobParentPKs.Any())
					{
						consolCostAccruals.AddRange(LoadConsolCosts(consolJobParentPKs));
					}
				}
				return consolCostAccruals;

				List<AccrualsForAPReconciliation<JobConsolCost>> LoadConsolCosts(IEnumerable<ZGuid> consolJobParentPKs)
				{
					var accruals = new List<AccrualsForAPReconciliation<JobConsolCost>>();
					foreach (var pks in AccountingUtils.ChunksOf(consolJobParentPKs, 100))
					{
						var jobConsolCostFilter = new ZQuery(JobConsolCostSchema.E6_ParentID, pks);
						jobConsolCostFilter.AddToFilter(JobConsolCostSchema.E6_GC, CompanyPK);
						var consolCosts = Factory.Load<JobConsolCost>(jobConsolCostFilter);

						//Query will be something like below. I will implemented batching by E6_ParentID.

						//SELECT * 
						//FROM JobConsolCost E6
						//WHERE E6_ParentID IN ('68A4D9F3-6A28-45AA-9CA5-0058A79A209E',...)

						var groupedconsolCosts = consolCosts.GroupBy(c =>
							new {
								parentId = c.E6_ParentID,
								parentTableCode = c.E6_ParentTableCode
							});

						accruals.AddRange(groupedconsolCosts.Select(gc =>
							new AccrualsForAPReconciliation<JobConsolCost>()
							{
								JobParentId = gc.Key.parentId.ToGuid(),
								JobParentTableCode = gc.Key.parentTableCode,
								AccrualType = AccrualSourceTypes.Consol,
								Accruals = gc
							}));
					}

					return accruals;
				}
			}
		}
		List<AccrualsForAPReconciliation<JobConsolCost>> consolCostAccruals;

		protected IJobCostingPlugIn[] GetConsols(string parentTableCode, params ZGuid[] parentIds)
		{
			var entSchema = ObjectFactory.Get<IApplicationSchemaResolver>();
			var bizoType = BusinessObjectFactory.GetBusinessObjectBaseTypeFromTablePrefix(parentTableCode, reportUnknownPrefix: false);
			var pkColumn = entSchema.GetTableSchemaFromColumnNamePrefix(parentTableCode).PK;
			var filter = new ZQuery(pkColumn, parentIds);
			var jConsols = Factory.Load(bizoType, filter).Cast<IJobCostingPlugIn>().ToArray();
			return jConsols;
		}
	}
}
