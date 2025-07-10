using System;
using System.Linq;
using System.Reflection;
using CargoWise.EntityFramework.Testing;
using CargoWise.Schema;
using CargoWise.Types;
using Enterprise.ArchiveManager.Business;
using Enterprise.ArchiveManager.Business.ArchiveEligibility;
using Enterprise.ArchiveManager.Engine;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.ArchiveManager.Test.ArchiveEligibility
{
	public class JobHeaderArchiveableFilterTest : TestCaseWithFactory
	{
		public void TestGetArchiveableFiltersWithoutDeclarations()
			=> TestGetArchiveableFiltersCore(includeDeclarations: false,
			[
				JobHeaderArchiveableFilter.JobIsClosed,
				TestDateColumnFilter,

				JobHeaderArchiveableFilter.ParentTableCodeIsOperationalJob,
				JobHeaderArchiveableFilter.AccountingPeriod,
				JobHeaderArchiveableFilter.NoOpenAssociatedHotCheques,
				JobHeaderArchiveableFilter.NoAssociatedRateAttachments,
				JobHeaderArchiveableFilter.NoAssociatedJobShipments,
				JobHeaderArchiveableFilter.NoAssociatedHVLVScanningSummary,
				JobHeaderArchiveableFilter.NoAssociatedCusUSLVConsignment,
				JobHeaderArchiveableFilter.NoAssociatedJobDeclarations,
				JobHeaderArchiveableFilter.NoAssociatedCusSCAHouse,
				JobHeaderArchiveableFilter.NoAssociatedCusHawb,
				JobHeaderArchiveableFilter.NoAssociatedCusSCADepotHouse,
				JobHeaderArchiveableFilter.NoAssociatedCusSCAOceanBill,
				JobHeaderArchiveableFilter.NoAssociatedCusCAeMHMaster_JobConShipLink,
				JobHeaderArchiveableFilter.NoAssociatedCusCAeMHMaster_JobConsol,
				JobHeaderArchiveableFilter.NoAssociatedAsycudaManifestHeader_JobConShipLink,
				JobHeaderArchiveableFilter.NoAssociatedAsycudaManifestHeader_JobConsol,
				JobHeaderArchiveableFilter.NoAssociatedAsycudaBill,
				JobHeaderArchiveableFilter.NoAssociatedCusDecHouseBill,
				JobHeaderArchiveableFilter.NoAssociatedCusOutturn,
				JobHeaderArchiveableFilter.NoAssociatedJobConsolLinkedToJobDeclaration
			]);

		public void TestGetArchiveableFiltersWithDeclarations()
			=> TestGetArchiveableFiltersCore(includeDeclarations: true,
			[
				JobHeaderArchiveableFilter.JobIsClosed,
				TestDateColumnFilter,

				JobHeaderArchiveableFilter.ParentTableCodeIsOperationalJobOrDeclaration,
				JobHeaderArchiveableFilter.AccountingPeriod,
				JobHeaderArchiveableFilter.NoOpenAssociatedHotCheques,
				JobHeaderArchiveableFilter.NoAssociatedRateAttachments,
				JobHeaderArchiveableFilter.NoAssociatedJobShipments,
				JobHeaderArchiveableFilter.NoAssociatedHVLVScanningSummary
			]);

		void TestGetArchiveableFiltersCore(bool includeDeclarations, JobHeaderArchiveableFilter[] expectedFilters)
		{
			var config = new ArchiveConfiguration(ArchiveManagerBirthday, 10, ZDateTime.Now, false, includeDeclarations);
			var actualFilters = CommonArchiveStageDescriptor.GetCommonArchiveableFiltersForConfig(config, JobHeaderCloseColumn).ToArray();

			AssertArrayEqualsByElements(expectedFilters, actualFilters);
		}

		public void TestExceptionThrownIfBizoNotInDatabase()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();

			Assert("Precondition - Bizo is not in database", !jobHeader.IsInDatabase);
			var exception = AssertExceptionThrown<InvalidOperationException>(() =>
			{
				_ = JobHeaderArchiveableFilter.JobIsClosed.ExecuteFor(jobHeader);
			});

			AssertEquals(exception.Message, "Bizo must exist in database");
		}

		public void TestAllFiltersAreValidSql()
		{
			var jobHeader = Factory.NewJobWithValidTestDataForTesting<JobHeader>();
			Factory.Save();

			var filters = typeof(JobHeaderArchiveableFilter)
				.GetFields(BindingFlags.Static | BindingFlags.Public)
				.Select(f => f.GetValue(null) as JobHeaderArchiveableFilter)
				.Append(TestDateColumnFilter)
				.ToArray();

			Assert("Precondition - Bizo is in database", jobHeader.IsInDatabase);
			AssertEquals("Precondition - Filter count", 24, filters.Length);

			foreach (var filter in filters)
			{
				AssertNoExceptionThrown(filter.ToString(), () => filter.ExecuteFor(jobHeader));
			}
		}

		public void TestGetDateColumnFilter()
		{
			AssertEquals(TestDateColumnFilter.FilterName, "Job Close Date on or before 19-Jan-09");
			AssertEquals(TestDateColumnFilter.FilterClause, "JH_A_JCL < '2009-01-20 00:00:00.000'");
		}

		static JobHeaderArchiveableFilter TestDateColumnFilter
			=> JobHeaderArchiveableFilter.GetDateColumnFilter(JobHeaderCloseColumn, ArchiveManagerBirthday);

		static SchemaDateTimeColumn JobHeaderCloseColumn
			=> JobHeaderSchema.JH_A_JCL;

		static ZDateTime ArchiveManagerBirthday
			=> new(2009, 1, 19);
	}
}
