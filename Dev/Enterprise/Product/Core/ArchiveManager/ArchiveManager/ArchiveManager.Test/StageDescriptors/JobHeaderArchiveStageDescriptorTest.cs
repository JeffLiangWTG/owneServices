using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business.ArchiveEligibility;

namespace Enterprise.ArchiveManager.Test.StageDescriptors
{
	abstract class JobHeaderArchiveStageDescriptorTest : CommonArchiveStageDescriptorTest
	{
		protected virtual void ApplyCommonFiltersToQuery(ZQuery query, bool includeDeclarations)
		{
			foreach (var filter in GetCommonArchiveableFilters(includeDeclarations))
			{
				filter.ApplyTo(query);
			}
		}

		IEnumerable<JobHeaderArchiveableFilter> GetCommonArchiveableFilters(bool includeDeclarations)
		{
			yield return JobHeaderArchiveableFilter.JobIsClosed;
			yield return JobHeaderArchiveableFilter.GetDateColumnFilter((SchemaDateTimeColumn)ExpectedMainArchiveDateFilterColumn, TestDate);

			yield return includeDeclarations
				? JobHeaderArchiveableFilter.ParentTableCodeIsOperationalJobOrDeclaration
				: JobHeaderArchiveableFilter.ParentTableCodeIsOperationalJob;

			yield return JobHeaderArchiveableFilter.AccountingPeriod;
			yield return JobHeaderArchiveableFilter.NoOpenAssociatedHotCheques;
			yield return JobHeaderArchiveableFilter.NoAssociatedRateAttachments;
			yield return JobHeaderArchiveableFilter.NoAssociatedJobShipments;
			yield return JobHeaderArchiveableFilter.NoAssociatedHVLVScanningSummary;

			if (!includeDeclarations)
			{
				yield return JobHeaderArchiveableFilter.NoAssociatedCusUSLVConsignment;
				yield return JobHeaderArchiveableFilter.NoAssociatedJobDeclarations;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusSCAHouse;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusHawb;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusSCADepotHouse;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusSCAOceanBill;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusCAeMHMaster_JobConShipLink;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusCAeMHMaster_JobConsol;
				yield return JobHeaderArchiveableFilter.NoAssociatedAsycudaManifestHeader_JobConShipLink;
				yield return JobHeaderArchiveableFilter.NoAssociatedAsycudaManifestHeader_JobConsol;
				yield return JobHeaderArchiveableFilter.NoAssociatedAsycudaBill;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusDecHouseBill;
				yield return JobHeaderArchiveableFilter.NoAssociatedCusOutturn;
				yield return JobHeaderArchiveableFilter.NoAssociatedJobConsolLinkedToJobDeclaration;
			}
		}
	}
}
