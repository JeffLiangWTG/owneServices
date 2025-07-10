using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Accounting.ElectronicMessaging.Registry;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.SaudiArabia.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForSaudiArabia))]
	internal class EInvoicingBatchCreatorForSaudiArabiaTest : EInvoicingBatchCreatorBaseTest
	{
		public void TestTransactionBatching_WhenThrottlingIsOff()
		{
			for (int i = 0; i < 10; i++)
			{
				var transaction = ObjectCreator.CreateARInvoice<ARInvoice>(FormattableString.Invariant($"100{i}"), ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI);
				ObjectCreator.CreateEInvoicingTransactionPivot(transaction, status: EInvoicingPivotState.Queued);
			}
			Factory.Save();

			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingBatchState.Ready);
			AssertEquals("No invoice batches in DB.", 0, invoiceBatches.Length);
			var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals("6 transactions ready for batching.", 10, transactionPivots.Length);

			using (AccountingElectronicMessagingRegistry.Instance.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 0))
			{
				var logger = new DetailedLoggerForTest();
				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				processor.PerformBatching(logger);

				invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingBatchState.Ready);
				AssertEquals("Invoice batch is not created.", 10, invoiceBatches.Length);
				transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingPivotState.Batched);
				AssertEquals("6 transactions are not batched.", 10, transactionPivots.Length);
			}
		}

		public void TestTransactionBatching_WhenThrottlingIsON()
		{
			for (int i = 0; i < 10; i++)
			{
				var transaction = ObjectCreator.CreateARInvoice<ARInvoice>(FormattableString.Invariant($"100{i}"), ObjectCreator.AUD, 1.0m, ObjectCreator.AALSHI);
				ObjectCreator.CreateEInvoicingTransactionPivot(transaction, status: EInvoicingPivotState.Queued);
			}
			Factory.Save();

			var invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingBatchState.Ready);
			AssertEquals("No invoice batches in DB.", 0, invoiceBatches.Length);
			var transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingPivotState.Queued, ZGuid.Empty);
			AssertEquals("6 transactions ready for batching.", 10, transactionPivots.Length);

			using (AccountingElectronicMessagingRegistry.Instance.LimitNumberOfInvoicesCanSimultaneouslyBeSentToXT.SetTemporaryValue(Env.CurrentCompanyPK, Guid.Empty, Guid.Empty, 5))
			{
				var logger = new DetailedLoggerForTest();
				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				processor.PerformBatching(logger);				
				invoiceBatches = EInvoicingTestHelper.LoadInvoiceBatchesForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingBatchState.Ready);
				AssertEquals("Invoice batch is not created.", 5, invoiceBatches.Length);
				transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingPivotState.Batched);
				AssertEquals("6 transactions are not batched.", 5, transactionPivots.Length);

				transactionPivots = EInvoicingTestHelper.LoadTransactionPivotsForCompany(GlbCompany.CurrentCompany.PK, Core.Constants.EInvoicingPivotState.Queued);
				AssertEquals("6 transactions are not batched.", 5, transactionPivots.Length);
			}
		}
		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForSaudiArabia(company);
	}
}
