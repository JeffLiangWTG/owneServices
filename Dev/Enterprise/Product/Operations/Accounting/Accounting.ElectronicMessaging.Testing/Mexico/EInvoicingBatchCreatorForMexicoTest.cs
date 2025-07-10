using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Mexico.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForMexico))]
	public class EInvoicingBatchCreatorForMexicoTest : EInvoicingDependentBatchCreatorTest
	{
		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForMexico(company);

		protected override string Country => CountryCodes.Mexico;

		#region Cancellations

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedCancellationPivotStatus)[] CancellationSetUpAndExpectedValues
			=> new[] {
				//Batch cancellation
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingBatchState.Ready, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.BatchedWithError, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Failed, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Batched, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				//Discard cancelation
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded),
			};

		protected override string[] OriginalTransactionPivotStatusToWaitForCancellations => new[] { EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Failed, EInvoicingPivotState.Delivered, EInvoicingPivotState.Sent, EInvoicingPivotState.Pending, EInvoicingPivotState.Queued, EInvoicingPivotState.Batched };

		#endregion

		#region Amendings

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedAmendingPivotStatus)[] AmendingSetUpAndExpectedValues
			=> new[] {
				//Batch amending
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.BatchedWithError, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Failed, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingBatchState.Ready, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Batched, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				//Discard amending
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded),
			};

		protected override string[] OriginalTransactionPivotStatusToWaitForAmendings => new[] { EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Failed, EInvoicingPivotState.Delivered, EInvoicingPivotState.Sent, EInvoicingPivotState.Pending, EInvoicingPivotState.Queued, EInvoicingPivotState.Batched };

		protected override bool SupportsWaitForOriginalTransactionForAmending => true;

		protected override string ExpectedPivotStatusWhenAmendingTransactionDoesNotHaveOriginalTransaction => EInvoicingPivotState.Discarded;

		public void TestDiscardTransactionIfMoreThanFourDaysHavePassed()
		{
			AssertDiscardTransactionIfMoreThanFourDaysHavePassed(EInvoicingPivotState.Failed);
			AssertDiscardTransactionIfMoreThanFourDaysHavePassed(EInvoicingPivotState.BatchedWithError);

			void AssertDiscardTransactionIfMoreThanFourDaysHavePassed(ZString originalTransactionPivotStatus)
			{
				var complianceDate = ZDateTime.Today.AddDays(-10);
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
				{
					GlbCompany.CurrentCompany.Factory.Save();

					// Queue and batch a ar transaction.
					var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
					var pivot = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Queued);
					arInvoice.Factory.Save();

					var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
					processor.PerformBatching(new DetailedLoggerForTest());

					var pivotAfterProcess = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
					AssertNotNull(pivotAfterProcess);

					var batch = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterProcess.AIP_AIB));
					AssertNotNull(batch);

					pivotAfterProcess.AIP_Status = originalTransactionPivotStatus;
					pivotAfterProcess.Factory.Save();
					batch.AIB_Status = EInvoicingBatchState.Sent;
					batch.Factory.Save();

					// Create a reversal and batch again.
					var arCreditNote = (ARCreditNote)(Helper.ObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice)).amendTransaction;
					AssertNotNull("CreditNote should not be null", arCreditNote);
					Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Amend, status: EInvoicingPivotState.Queued);
					arCreditNote.Factory.Save();

					processor.PerformBatching(new DetailedLoggerForTest());
					AssertAfterRunServiceTask(EInvoicingPivotState.Queued);

					arCreditNote.AH_InvoiceDate = ZDateTime.Today.AddDays(5);
					arCreditNote.Factory.Save();

					processor.PerformBatching(new DetailedLoggerForTest());
					AssertAfterRunServiceTask(EInvoicingPivotState.Queued);

					arCreditNote.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
					arCreditNote.Factory.Save();

					processor.PerformBatching(new DetailedLoggerForTest());
					AssertAfterRunServiceTask(EInvoicingPivotState.Discarded);

					void AssertAfterRunServiceTask(string creditPivotStatus)
					{
						var pivotAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
						AssertNotNull(pivotAfterCreditNoteProcessed);

						var batchAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterCreditNoteProcessed.AIP_AIB));
						AssertNotNull(batchAfterCreditNoteProcessed);

						var creditPivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
						AssertNotNull(creditPivot);

						AssertEquals(originalTransactionPivotStatus, pivotAfterCreditNoteProcessed.AIP_Status);
						AssertEquals(EInvoicingBatchState.Sent, batchAfterCreditNoteProcessed.AIB_Status);
						AssertEquals(creditPivotStatus, creditPivot.AIP_Status);
					}
				}
			}
		}

		public void TestDiscardReversalTransactionIfMoreThanFourDaysHavePassed()
		{
			AssertDiscardReversalTransactionIfMoreThanFourDaysHavePassed(EInvoicingPivotState.Failed);
			AssertDiscardReversalTransactionIfMoreThanFourDaysHavePassed(EInvoicingPivotState.BatchedWithError);

			void AssertDiscardReversalTransactionIfMoreThanFourDaysHavePassed(ZString originalTransactionPivotStatus)
			{
				var complianceDate = ZDateTime.Today.AddDays(-10);
				using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
				{
					GlbCompany.CurrentCompany.Factory.Save();

					// Queue and batch a ar transaction.
					var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
					var pivot = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Queued);
					arInvoice.Factory.Save();

					var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
					processor.PerformBatching(new DetailedLoggerForTest());

					var pivotAfterProcess = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
					AssertNotNull(pivotAfterProcess);

					var batch = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterProcess.AIP_AIB));
					AssertNotNull(batch);

					pivotAfterProcess.AIP_Status = originalTransactionPivotStatus;
					pivotAfterProcess.Factory.Save();
					batch.AIB_Status = EInvoicingBatchState.Sent;
					batch.Factory.Save();

					// Create a Cancel and batch again.
					var arCreditNote = (ARCreditNote)Helper.ObjectCreator.ReverseTransaction(arInvoice, out _);
					AssertNotNull("CreditNote should not be null", arCreditNote);
					var pivotCreditNote = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Cancel, status: EInvoicingPivotState.Queued);
					arCreditNote.Factory.Save();

					processor.PerformBatching(new DetailedLoggerForTest());
					AssertAfterRunServiceTask(EInvoicingPivotState.Queued);

					arCreditNote.AH_InvoiceDate = ZDateTime.Today.AddDays(5);
					arCreditNote.Factory.Save();

					processor.PerformBatching(new DetailedLoggerForTest());
					AssertAfterRunServiceTask(EInvoicingPivotState.Queued);

					arCreditNote.AH_InvoiceDate = ZDateTime.Today.AddDays(-10);
					arCreditNote.Factory.Save();

					processor.PerformBatching(new DetailedLoggerForTest());
					AssertAfterRunServiceTask(EInvoicingPivotState.Discarded);

					void AssertAfterRunServiceTask(string creditPivotStatus)
					{
						var pivotAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
						AssertNotNull(pivotAfterCreditNoteProcessed);

						var batchAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterCreditNoteProcessed.AIP_AIB));
						AssertNotNull(batchAfterCreditNoteProcessed);

						var creditPivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
						AssertNotNull(creditPivot);

						AssertEquals(originalTransactionPivotStatus, pivotAfterCreditNoteProcessed.AIP_Status);
						AssertEquals(EInvoicingBatchState.Sent, batchAfterCreditNoteProcessed.AIB_Status);
						AssertEquals(creditPivotStatus, creditPivot.AIP_Status);
					}
				}
			}
		}

		#endregion

		protected override string ExpectedDependentCancelTransactionStatusWhenOriginalTransactionHasNotPivot => EInvoicingPivotState.Discarded;
		protected override string ExpectedDependentAmendmentTransactionStatusWhenOriginalTransactionHasNotPivot => EInvoicingPivotState.Discarded;
	}
}
