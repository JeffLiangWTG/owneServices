using System;
using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Schema;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.Actions;
using Enterprise.ArchiveManager.Business.ArchiveEligibility;
using Enterprise.ArchiveManager.Business.StageDescriptors.PDO;
using Enterprise.ArchiveManager.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.StageDescriptors.PDO
{
	class PDOPurgeStageDescriptorTest : JobHeaderArchiveStageDescriptorTest
	{
		protected override void ApplyCommonFiltersToQuery(ZQuery query, bool includeDeclarations)
		{
			var filters = GetArchiveableFilters(includeDeclarations);

			foreach (var filter in filters)
			{
				filter.ApplyTo(query);
			}
		}

		IEnumerable<JobHeaderArchiveableFilter> GetArchiveableFilters(bool includeDeclarations)
		{
			yield return JobHeaderArchiveableFilter.GetDateColumnFilter((SchemaDateTimeColumn)ExpectedMainArchiveDateFilterColumn, TestDate);

			yield return includeDeclarations
				? JobHeaderArchiveableFilter.ParentTableCodeIsOperationalJobOrDeclaration
				: JobHeaderArchiveableFilter.ParentTableCodeIsOperationalJob;

			yield return JobHeaderArchiveableFilter.AccountingPeriod;
			yield return JobHeaderArchiveableFilter.NoOpenAssociatedHotCheques;
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

		protected override IArchiveStageDescriptor StageDescriptor
			=> new PDOPurgeStageDescriptor();

		protected override IArchiveSystemDescriptor SystemDescriptor
			=> new PDOPurgeSystemDescriptor();

		protected override string ExpectedName
			=> "Purge Documents of Operational Records";

		protected override SchemaColumn ExpectedMainArchivePKColumn
			=> JobHeaderSchema.PK;

		protected override SchemaColumn ExpectedMainArchiveNKColumn
			=> JobHeaderSchema.JH_JobNum;

		protected override SchemaColumn ExpectedMainArchiveDateFilterColumn { get; set; } = JobHeaderSchema.JH_SystemCreateTimeUtc;

		protected override ZQuery ExpectedMainArchiveFilterWithoutDeclarations
		{
			get
			{
				var query = new ZQuery();
				ApplyCommonFiltersToQuery(query, includeDeclarations: false);
				query.OrderBy = $"{ExpectedMainArchiveDateFilterColumn.Name}, {ExpectedMainArchiveNKColumn.Name}, {ExpectedMainArchivePKColumn.Name}";
				return query;
			}
		}

		protected override ZQuery ExpectedMainArchiveFilterWithDeclarations
		{
			get
			{
				var query = new ZQuery();
				ApplyCommonFiltersToQuery(query, includeDeclarations: true);
				query.OrderBy = $"{ExpectedMainArchiveDateFilterColumn.Name}, {ExpectedMainArchiveNKColumn.Name}, {ExpectedMainArchivePKColumn.Name}";
				return query;
			}
		}

		protected override Type[] ExpectedPreparationActions
			=> new Type[] { typeof(ArchiveImageDeletionAction) };

		protected override Type[] ExpectedArchiveActions
			=> Array.Empty<Type>();
	}
}
