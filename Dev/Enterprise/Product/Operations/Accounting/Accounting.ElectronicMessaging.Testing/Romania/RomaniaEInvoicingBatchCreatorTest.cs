using System;
using System.Globalization;
using System.Linq;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Romania.Testing
{
	[TestedType(typeof(RomaniaEInvoicingBatchCreator))]
	public class RomaniaEInvoicingBatchCreatorTest : EInvoicingBatchCreatorBaseTest
	{
		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company)
		{
			return new RomaniaEInvoicingBatchCreator(company);
		}

		[TestDate(2024, 01, 17, 2, 30, 00)]
		public void TestPerformBatching_RomaniaEReportingDelayTimeForQueryInvoiceRequest()
		{
			using (AccountingElectronicMessagingRegistry.Instance.RomaniaEReportingDelayTimeForQueryInvoiceRequest.SetTemporaryValue(Guid.Empty, Guid.Empty, Guid.Empty, 15))
			{
				var batchCreator = new RomaniaEInvoicingBatchCreator(GlbCompany.CurrentCompany);

				var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
				var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice1, status: EInvoicingPivotState.Delivered);
				var batch1 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot1, 001, EInvoicingBatchState.Ready);
				Factory.Save();

				pivot1.AIP_LastResponseReceivedUtc = new ZDateTime(2024, 01, 17, 2, 16, 00);
				Factory.Save();
				AssertEquals(false, batchCreator.PerformBatching(new DetailedLoggerForTest()));

				pivot1.AIP_LastResponseReceivedUtc = new ZDateTime(2024, 01, 17, 2, 15, 00);
				Factory.Save();
				AssertEquals(false, batchCreator.PerformBatching(new DetailedLoggerForTest()));

				pivot1.AIP_LastResponseReceivedUtc = new ZDateTime(2024, 01, 17, 2, 14, 00);
				Factory.Save();
				AssertEquals("Batch pivot when it have passed 15 mins since last receive time.", true, batchCreator.PerformBatching(new DetailedLoggerForTest()));
			}
		}

		public void TestPerformBatching_WhenAllPivotsAreQueued()
		{
			var batchCreator = new RomaniaEInvoicingBatchCreator(GlbCompany.CurrentCompany);

			var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice1, status: EInvoicingPivotState.Queued);

			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000002", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice2, status: EInvoicingPivotState.Queued);

			Factory.Save();

			AssertEquals("Precondition", true, pivot1.AIP_AIB.IsEmpty);
			AssertEquals("Precondition", true, pivot2.AIP_AIB.IsEmpty);

			AssertEquals(true, batchCreator.PerformBatching(new DetailedLoggerForTest()));
			Factory.ReloadAll<AccEInvoicingTransactionPivot>();

			AssertEquals("AIP_Status should be updated from Queued to Batched", EInvoicingPivotState.Batched, pivot1.AIP_Status);
			AssertEquals("AIP_Status should be updated from Queued to Batched", EInvoicingPivotState.Batched, pivot2.AIP_Status);
			var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(GlbCompany.CurrentCompany.PK, EInvoicingBatchState.Ready);
			AssertContainsExactElementsInAnyOrder("There should be two new batches with status: Ready ", new [] { pivot1.AIP_AIB, pivot2.AIP_AIB }, batches.Select(x => x.PK).ToArray());
		}

		public void TestPerformBatching_WhenAllPivotsAreDelivered()
		{
			var batchCreator = new RomaniaEInvoicingBatchCreator(GlbCompany.CurrentCompany);

			var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice1, status: EInvoicingPivotState.Delivered);
			var batch1 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot1, 001, EInvoicingBatchState.Ready);
			pivot1.AIP_LastResponseReceivedUtc = ZDateTime.Now.AddDays(-1);

			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000002", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice2, status: EInvoicingPivotState.Delivered);
			var batch2 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot2, 002, EInvoicingBatchState.Ready);
			pivot2.AIP_LastResponseReceivedUtc = ZDateTime.Now.AddDays(-1);

			Factory.Save();

			AssertEquals(true, batchCreator.PerformBatching(new DetailedLoggerForTest()));
			Factory.ReloadAll<AccEInvoicingTransactionPivot>();

			AssertEquals("AIP_Status should still be Delivered", EInvoicingPivotState.Delivered, pivot1.AIP_Status);
			AssertEquals("AIP_Status should still be Delivered", EInvoicingPivotState.Delivered, pivot2.AIP_Status);
			var batches = Factory.Load<AccEInvoicingBatch>(new ZQuery());
			AssertContainsExactElementsInAnyOrder("There should be no new batches created", new[] { pivot1.AIP_AIB, pivot2.AIP_AIB }, batches.Select(x => x.PK).ToArray());
		}

		public void TestPerformBatching_WhenPivotsAreQueuedOrDelivered()
		{
			var batchCreator = new RomaniaEInvoicingBatchCreator(GlbCompany.CurrentCompany);

			var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000001", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice1, status: EInvoicingPivotState.Delivered);
			var batch1 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot1, NextBatchNumber, EInvoicingBatchState.Ready);
			pivot1.AIP_LastResponseReceivedUtc = ZDateTime.Now.AddDays(-1);

			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000002", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice2, status: EInvoicingPivotState.Queued);

			Factory.Save();

			AssertEquals("Precondition", false, pivot1.AIP_AIB.IsEmpty);
			AssertEquals("Precondition", true, pivot2.AIP_AIB.IsEmpty);

			AssertEquals(true, batchCreator.PerformBatching(new DetailedLoggerForTest()));
			Factory.ReloadAll<AccEInvoicingTransactionPivot>();

			AssertEquals("AIP_Status should still be Delivered", EInvoicingPivotState.Delivered, pivot1.AIP_Status);
			AssertEquals("AIP_Status should be updated from Queued to Batched", EInvoicingPivotState.Batched, pivot2.AIP_Status);
			var batches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(GlbCompany.CurrentCompany.PK, EInvoicingBatchState.Ready);
			AssertContainsExactElementsInAnyOrder("There should be two batches with status: Ready ", new[] { pivot1.AIP_AIB, pivot2.AIP_AIB }, batches.Select(x => x.PK).ToArray());
		}

		public void TestPerformBatching_ResetEInvoicingToDelivered()
		{
			var batchCreator = new RomaniaEInvoicingBatchCreator(GlbCompany.CurrentCompany);
			var logger = new DetailedLoggerForTest();

			var arInvoice1 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000010", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot1 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice1, status: EInvoicingPivotState.Delivered);
			var originBatch1 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot1, NextBatchNumber, EInvoicingBatchState.Discarded);
			pivot1.AIP_AIB = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, pivot1.AIP_AIB);

			var arInvoice2 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000011", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot2 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice2, status: EInvoicingPivotState.Delivered);
			var originBatch2 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot2, NextBatchNumber, EInvoicingBatchState.Discarded);
			var originBatch3 = TestObjectCreator.CreateEInvoicingBatchForPivot(pivot2, NextBatchNumber, EInvoicingBatchState.Discarded);
			pivot2.AIP_AIB = ZGuid.Empty;
			AssertEquals(ZGuid.Empty, pivot2.AIP_AIB);

			var arInvoice3 = TestObjectCreator.CreateARInvoice<ARInvoice>("00000012", TestObjectCreator.EUR, 1m, TestObjectCreator.Debtor);
			var pivot3 = TestObjectCreator.CreateEInvoicingTransactionPivot(arInvoice3, status: EInvoicingPivotState.Delivered);
			pivot3.SetCompanyAndCountryCode(TestObjectCreator.NonCurrentCompany);
			AssertEquals(ZGuid.Empty, pivot3.AIP_AIB);
			Factory.Save();

			AssertEquals(true, batchCreator.PerformBatching(logger));
			AssertCollectionContains(logger.Logs, x => x.Item2 == "Batching sub task: Reset Status to Delivered completed.");

			pivot1.Reload();
			AssertNotEquals("New batch should be created.",originBatch1, pivot1.AIP_AIB);
			AssertEquals(EInvoicingBatchState.Ready, pivot1.Batch.AIB_Status);
			AssertEquals(EInvoicingPivotState.Delivered, pivot1.AIP_Status);

			pivot2.Reload();
			AssertNotEquals("New batch should be created.", originBatch2, pivot2.AIP_AIB);
			AssertNotEquals("New batch should be created.", originBatch3, pivot2.AIP_AIB);
			AssertEquals(EInvoicingBatchState.Ready, pivot2.Batch.AIB_Status);
			AssertEquals(EInvoicingPivotState.Delivered, pivot2.AIP_Status);

			pivot3.Reload();
			AssertEquals("Pivot in another company should be skipped.", ZGuid.Empty, pivot3.AIP_AIB);
		}

		ZInt NextBatchNumber => int.Parse(AccountingNumberFountainWrapperFactory.Instance.AccEInvoicingBatchNumber.GetNext(Db.Connection), CultureInfo.InvariantCulture);

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;
	}
}
