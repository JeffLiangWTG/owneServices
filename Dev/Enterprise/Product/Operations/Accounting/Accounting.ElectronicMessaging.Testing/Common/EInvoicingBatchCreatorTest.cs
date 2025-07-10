using System;
using CargoWise.Common;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	[TestedType(typeof(EInvoicingBatchCreator))]
	public class EInvoicingBatchCreatorTest : EInvoicingBatchCreatorBaseTest
	{
		public void TestTransactionGrouping_CreatesNoBatches_WhenNoQueuedPivots()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();

			var anyBatchesCreated = CreateBatchProcessor(company).PerformBatching(new DetailedLoggerForTest());
			Assert("No batches are created when zero pivots queued", !anyBatchesCreated);
			AssertEquals("No pivots are batched", 0, EInvoicingTestHelper.LoadTransactionPivotsForCompany(company.PK, EInvoicingPivotState.Batched).Length);
			AssertEquals("No batches are created", 0, EInvoicingTestHelper.LoadInvoiceBatchesForCompany(company.PK, EInvoicingBatchState.Ready).Length);
		}

		public void TestTransactionGrouping_CreatesOneBatch_WhenOneQueuedPivot()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();
			CreateFakePivots(1, company);

			var anyBatchesCreated = CreateBatchProcessor(company).PerformBatching(new DetailedLoggerForTest());
			Assert("A batch is created when any pivots available as queued", anyBatchesCreated);
			AssertPivotsAndBatches(company, expectedPivots: 1, expectedBatches: 1);
		}

		public void TestTransactionGrouping_CreatesOneBatch_WhenTwoQueuedPivots()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();
			CreateFakePivots(2, company);

			var anyBatchesCreated = CreateBatchProcessor(company).PerformBatching(new DetailedLoggerForTest());
			Assert("A batch is created when any pivots available as queued", anyBatchesCreated);
			AssertPivotsAndBatches(company, expectedPivots: 2, expectedBatches: 1);
		}

		public void TestTransactionGrouping_CreatesOneBatch_WhenFiveQueuedPivots()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();
			CreateFakePivots(5, company);

			var anyBatchesCreated = CreateBatchProcessor(company).PerformBatching(new DetailedLoggerForTest());
			Assert("A batch is created when any pivots available as queued", anyBatchesCreated);
			AssertPivotsAndBatches(company, expectedPivots: 5, expectedBatches: 1);
		}

		public void TestTransactionGrouping_CreatesTwoBatches_WhenSixQueuedPivots()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();
			CreateFakePivots(6, company);

			var anyBatchesCreated = CreateBatchProcessor(company).PerformBatching(new DetailedLoggerForTest());
			Assert("A batch is created when any pivots available as queued", anyBatchesCreated);
			AssertPivotsAndBatches(company, expectedPivots: 6, expectedBatches: 2);
		}

		public void TestTransactionGrouping_CreatesThreeBatches_WhenThirtyQueuedPivots_AndBatchSizeTen()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();
			CreateFakePivots(30, company);

			var anyBatchesCreated = CreateBatchProcessor(company, maximumBatchSize: 10).PerformBatching(new DetailedLoggerForTest());
			Assert("A batch is created when any pivots available as queued", anyBatchesCreated);
			AssertPivotsAndBatches(company, expectedPivots: 30, expectedBatches: 3);
		}

		public void TestMaximumBatchSizeFromRegistryOrCountryDefault()
		{
			var company1 = ObjectCreator.CreateCompanyAndBranch("AUSYD");
			var company2 = ObjectCreator.CreateCompanyAndBranch("NZAKL");

			var actualCompany1DefaultRegistry = EInvoicingBatchCreator.MaximumBatchSizeFromRegistryOrCountryDefault(company1, 100);
			AssertEquals(100, actualCompany1DefaultRegistry);
			var actualCompany2DefaultRegistry = EInvoicingBatchCreator.MaximumBatchSizeFromRegistryOrCountryDefault(company1, 200);
			AssertEquals(200, actualCompany2DefaultRegistry);

			using (AccountingElectronicMessagingRegistry.Instance.EInvoicingBatchSize.SetTemporaryValue(company1.PK.ToGuid(), Guid.Empty, Guid.Empty, 10))
			using (AccountingElectronicMessagingRegistry.Instance.EInvoicingBatchSize.SetTemporaryValue(company2.PK.ToGuid(), Guid.Empty, Guid.Empty, 5))
			{
				var actualCompany1WithRegistry = EInvoicingBatchCreator.MaximumBatchSizeFromRegistryOrCountryDefault(company1, 100);
				AssertEquals(10, actualCompany1WithRegistry);
				var actualCompany2WithRegistry = EInvoicingBatchCreator.MaximumBatchSizeFromRegistryOrCountryDefault(company2, 200);
				AssertEquals(5, actualCompany2WithRegistry);
			}
		}

		public void TestConstructorRaisesDeveloperError_WhenBatchSizeIsAboveArchitectureLimit()
		{
			ErrorReporter.Clear();

			var company = ObjectCreator.CreateCompanyAndBranch("AUSYD");
			var batchProcessor = CreateBatchProcessor(company, maximumBatchSize: 100);

			AssertEquals("Due to architectural limitations the maximum batch size should be clamped to 98", 98, batchProcessor.MaximumBatchSize);
			CombineAssertions("A developer error should be raised when batch size exceeds limit", () =>
			{
				AssertEquals(1, ErrorReporter.TotalErrorCount);
				AssertEquals("EInvoicingBatchCreator_BatchSizeExceedsArchitecturalLimitation", ErrorReporter.LastKeyReported);
				AssertEquals("Due to a limitation when importing XUE messages, batches greater than 98 are not supported. Requested maximum batch size 100 is reduced to 98.", ErrorReporter.LastMessageReported);
			});
			ErrorReporter.Clear();
		}

		public virtual void TestTransactionGrouping_CreateDifferentBatches_TransactionsWithTheSameUniqueKeys()
		{
			var company = CreateCompanyAndAssertPreconditionsForPivotsAndBatches();
			AssertTransactionGroupingWithTheSameUniqueKeys(company.Country.Code, 1);
		}

		#region Helpers

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
			=> CreateBatchProcessor(company);

		static EInvoicingBatchCreator CreateBatchProcessor(GlbCompany company, int maximumBatchSize = 5)
			=> new EInvoicingBatchCreator(company, maximumBatchSize: maximumBatchSize);

		#endregion
	}
}
