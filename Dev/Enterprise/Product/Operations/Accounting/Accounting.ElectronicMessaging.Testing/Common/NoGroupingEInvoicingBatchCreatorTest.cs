using CargoWise.Data.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	[TestedType(typeof(NoGroupingEInvoicingBatchCreator))]
	public class NoGroupingEInvoicingBatchCreatorTest : EInvoicingBatchCreatorBaseTest
	{
		public void TestTransactionGrouping_CreatesNoBatches_WhenNoQueuedPivots()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();

			var anyBatchesCreated = GetBatchProcessor(company).PerformBatching(new DetailedLoggerForTest());
			Assert("No batches are created when zero pivots queued", !anyBatchesCreated);
			AssertEquals("No pivots are batched", 0, EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Batched).Length);
			AssertEquals("No batches are created", 0, EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, EInvoicingBatchState.Ready).Length);
		}

		public void TestTransactionGrouping_CreatesOneBatch_WhenOneQueuedPivot()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();
			CreateFakePivots(1, company);

			var anyBatchesCreated = GetBatchProcessor(company).PerformBatching(new DetailedLoggerForTest());
			Assert("A batch is created when any pivots available as queued", anyBatchesCreated);
			AssertPivotsAndBatches(company, expectedPivots: 1, expectedBatches: 1);
		}

		public void TestTransactionGrouping_CreatesFiveBatches_WhenFiveQueuedPivots()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();
			CreateFakePivots(5, company);

			var anyBatchesCreated = GetBatchProcessor(company).PerformBatching(new DetailedLoggerForTest());
			Assert("A batch is created when any pivots available as queued", anyBatchesCreated);
			AssertPivotsAndBatches(company, expectedPivots: 5, expectedBatches: 5);
		}

		#region Obsolete - asserting country specific behavior should be done in country specific test cases

		[UseSnapshotProtection]
		[TestDate(2020, 4, 29)]
		public void TestCompanySpecificBatchNumbers_Fiji() => RunForCountry(Core.Constants.CountryCodes.Fiji, 2);

		[UseSnapshotProtection]
		[TestDate(2020, 4, 29)]
		public void TestCompanySpecificBatchNumbers_Italy() => RunForCountry(Core.Constants.CountryCodes.Italy, 3);

		[UseSnapshotProtection]
		[TestDate(2020, 4, 29)]
		public void TestCompanySpecificBatchNumbers_Hungary() => RunForCountry(Core.Constants.CountryCodes.Hungary, 2);

		void RunForCountry(string countryCode, int numberOfInvCrdAdjForThisCountry)
		{
			var company1 = Helper.CreateCompanyAndBranch(countryCode + "1", "BR1", countryCode, true);
			Helper.AddCustomsCodeForCountryIfMissing(company1.FirstActiveBranch.OrgProxy, countryCode, "VAT");
			Helper.AddCustomsCodeForCountryIfMissing(company1.FirstActiveBranch.OrgProxy, countryCode, "GST");
			var company2 = Helper.CreateCompanyAndBranch(countryCode + "2", "BR2", countryCode, true);
			Helper.AddCustomsCodeForCountryIfMissing(company2.FirstActiveBranch.OrgProxy, countryCode, "VAT");
			Helper.AddCustomsCodeForCountryIfMissing(company2.FirstActiveBranch.OrgProxy, countryCode, "GST");
			Helper.AddCustomsCodeForCountryIfMissing(ObjectCreator.AALSHI, countryCode, "VAT");
			Helper.AddCustomsCodeForCountryIfMissing(ObjectCreator.AALSHI, countryCode, "GST");

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, numberOfInvCrdAdjForThisCountry * 1, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, numberOfInvCrdAdjForThisCountry * 2, numberOfInvCrdAdjForThisCountry);
			AssertCompanySpecificBatchNumbers(company2, numberOfInvCrdAdjForThisCountry * 1, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, numberOfInvCrdAdjForThisCountry * 3, numberOfInvCrdAdjForThisCountry);
			AssertCompanySpecificBatchNumbers(company2, numberOfInvCrdAdjForThisCountry * 2, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company1.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company1, numberOfInvCrdAdjForThisCountry * 4, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company2, numberOfInvCrdAdjForThisCountry * 3, numberOfInvCrdAdjForThisCountry);

			Helper.CreateARAPINVCRDADJTransactions(company2.FirstActiveBranch, ObjectCreator.AALSHI, ObjectCreator.ABIGAS);
			AssertCompanySpecificBatchNumbers(company2, numberOfInvCrdAdjForThisCountry * 4, numberOfInvCrdAdjForThisCountry);
		}

		void AssertCompanySpecificBatchNumbers(GlbCompany company, int batchNumber, int numberOfInvCrdAdjForThisCountry)
		{
			var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, Core.Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals($"Number of transactions ready for batching (company '{company.GC_Code}').", numberOfInvCrdAdjForThisCountry, transactionPivots.Length);

			var processor = GetBatchProcessor(company);
			processor.PerformBatching(new DetailedLoggerForTest());

			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, Core.Constants.EInvoicingBatchState.Ready);
			AssertEquals($"Batch number starts as expected (company '{company.GC_Code}').", batchNumber, invoiceBatches[invoiceBatches.Length - 1].AIB_BatchNumber);
		}

		#endregion

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new NoGroupingEInvoicingBatchCreator(company);
		}
	}
}
