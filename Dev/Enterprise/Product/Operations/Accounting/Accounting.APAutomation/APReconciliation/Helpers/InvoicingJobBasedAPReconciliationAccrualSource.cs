using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Integration;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.APAutomation.APReconciliation
{
	public class InvoicingJobBasedAPReconciliationAccrualSource : APReconciliationAccrualSourceBase
	{
		public override BusinessObjectFactory Factory { get; } = new();

		public override ZGuid CompanyPK { get; }

		public InvoicingJobBasedAPReconciliationAccrualSource(IEnumerable<(ZGuid jobParentPk, ZString jobParentTableCode)> jobParentInfo, ZGuid companyPK)
		{
			if (jobParentInfo == null || !jobParentInfo.Any())
			{
				throw new ArgumentException("jobParentInfo cannot be null or empty.", nameof(jobParentInfo));
			}
			if (companyPK == ZGuid.Empty)
			{
				throw new ArgumentException("companyPK cannot be empty.", nameof(companyPK));
			}

			this.jobParentInfo = jobParentInfo;
			this.CompanyPK = companyPK;
			this.jobParentPks = new List<ZGuid>();
		}

		readonly IEnumerable<(ZGuid jobParentPk, ZString jobParentTableCode)> jobParentInfo;

		public override IEnumerable<ZGuid> GetJobParentPKs() => JobParentPks;

		public override IEnumerable<ZGuid> GetConsolPKs() => GetJobParentByAccrualSourceType(AccrualSourceTypes.Consol).Select(jpi => jpi.jobParentPk);

		List<ZGuid> JobParentPks
		{
			get
			{
				if (jobParentPks.Count == 0)
				{
					LoadConsolAndRelatedJobInfo();
				}
				return jobParentPks;
			}
		}
		List<ZGuid> jobParentPks;

		List<ConsolInfo> JobConsolInfo
		{
			get
			{
				if (jobConsolInfo == null)
				{
					LoadConsolAndRelatedJobInfo();
				}
				return jobConsolInfo;
			}
		}
		List<ConsolInfo> jobConsolInfo;

		List<IJobCostingPlugIn> Consols => consols ??= GetJobParentByAccrualSourceType(AccrualSourceTypes.Consol)
			.ToLookup(jp => jp.jobParentTableCode, jp => jp.jobParentPk)
			.SelectMany(x => GetConsols(parentTableCode: x.Key, parentIds: x.ToArray()))
			.ToList();
		List<IJobCostingPlugIn> consols;

		protected override List<AccrualsForAPReconciliation<Charge>> LoadCharges(IEnumerable<ZGuid> jobParentPKs)
		{
			var accrualGroupsWithConsolInfo = base.LoadCharges(jobParentPKs)
				.OfType<AccrualsForAPReconciliationWithConsolInfo<Charge>>()
				.ToList();

			var newAccrualGroups = new List<AccrualsForAPReconciliationWithConsolInfo<Charge>>();

			foreach (var grp in accrualGroupsWithConsolInfo)
			{
				foreach (var cInfo in JobConsolInfo)
				{
					if (cInfo.RelatedJobPks.Contains(grp.JobParentId))
					{
						grp.IsRelatedJob = true;
						if (grp.ConsolPk != null)
						{
							newAccrualGroups.Add(new AccrualsForAPReconciliationWithConsolInfo<Charge>
							{
								JobParentId = grp.JobParentId,
								JobNumber = grp.JobNumber,
								JobParentTableCode = grp.JobParentTableCode,
								AccrualType = grp.AccrualType,
								Accruals = grp.Accruals,
								IsRelatedJob = grp.IsRelatedJob,
								ConsolPk = cInfo.ConsolPk
							});
						}
						else
						{
							grp.ConsolPk = cInfo.ConsolPk;
						}
					}
				}
			}

			accrualGroupsWithConsolInfo.AddRange(newAccrualGroups);

			return accrualGroupsWithConsolInfo.Cast<AccrualsForAPReconciliation<Charge>>().ToList();
		}

		protected override AccrualsForAPReconciliation<Charge> GetChargeAccrualInstance() => new AccrualsForAPReconciliationWithConsolInfo<Charge>();

		void LoadConsolAndRelatedJobInfo()
		{
			jobConsolInfo = new List<ConsolInfo>();

			var filteredJobs = GetJobParentByAccrualSourceType(AccrualSourceTypes.Job);
			var pks = filteredJobs.Select(djh => djh.jobParentPk).ToList();

			foreach (var consol in Consols)
			{
				if (consol != null)
				{
					var relatedJobPks = consol.CostSupporter.ShipmentsListPKs ?? Array.Empty<ZGuid>();
					jobConsolInfo.Add(new ConsolInfo
					{
						ConsolPk = consol.PK,
						RelatedJobPks = relatedJobPks
					});

					pks.AddRange(relatedJobPks);
					pks.Add(consol.PK);
				}
			}

			jobParentPks = pks.Distinct().ToList();
		}

		(ZGuid jobParentPk, ZString jobParentTableCode)[] GetJobParentByAccrualSourceType(AccrualSourceTypes sourceType) =>
			jobParentInfo.Where(jpi => APReconciliationNodeProvider.GetAccrualSourceType(jpi.jobParentTableCode) == sourceType).ToArray();

		protected class ConsolInfo
		{
			public ZGuid ConsolPk { get; set; }
			public ZGuid[] RelatedJobPks { get; set; }
		}
	}
}
