using System;
using System.Globalization;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using Moq;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class EInvoicingDependentBatchCreatorTest : EInvoicingBatchCreatorBaseTest
	{
		protected abstract string Country { get; }

		protected virtual (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedCancellationPivotStatus)[] CancellationSetUpAndExpectedValues
			=> new[] {
				//Batch cancellation
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				//Discard cancellation
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Failed, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded),
				//Discard both
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded),
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Discarded, EInvoicingBatchState.Discarded, EInvoicingPivotState.Discarded)
			};

		protected virtual (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedAmendingPivotStatus)[] AmendingSetUpAndExpectedValues
			=> new[] {
				//Batch amending
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Batched, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.BatchedWithError, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingBatchState.Ready, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Failed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
			};

		protected virtual string[] OriginalTransactionPivotStatusToWaitForCancellations => new[] { EInvoicingPivotState.Sent, EInvoicingPivotState.Pending };

		protected virtual string[] OriginalTransactionPivotStatusToWaitForAmendings => Array.Empty<string>();

		protected virtual bool SupportsWaitForOriginalTransactionForAmending => false;

		protected virtual string ExpectedPivotStatusWhenAmendingTransactionDoesNotHaveOriginalTransaction => EInvoicingPivotState.Batched;

		protected virtual string ExpectedDependentCancelTransactionStatusWhenOriginalTransactionHasNotPivot => EInvoicingPivotState.Batched;

		protected virtual string ExpectedDependentAmendmentTransactionStatusWhenOriginalTransactionHasNotPivot => EInvoicingPivotState.Batched;

		protected virtual string ExpectedDependentTransactionStatusWhenOriginalTransactionHasDCDPivotBecauseEInvoicingDisabled => EInvoicingPivotState.Discarded;

		protected virtual bool ExpectPopulateGovernmentAllocatedNumberWithOriginalTransactionGvtNumber => true;

		const string ProcessingMessageCancellations = "Processing cancellation pivots.";
		const string ProcessingMessageAmendings = "Processing amending pivots.";
		const string ProcessingMessageQueued = "Processing all queued pivots.";

		#region Cancellations

		public void TestCancellationBatchBehavior()
		{
			foreach (var cancellationCase in CancellationSetUpAndExpectedValues)
			{
				AssertTransactionPivot(EInvoicingPivotActionType.Cancel, cancellationCase.OriginalTransactionPivotStatus, cancellationCase.ExpectedOriginalTransactionPivotStatus, cancellationCase.ExpectedOriginalTransactionBatchStatus, cancellationCase.ExpectedCancellationPivotStatus);
			}
		}

		public void TestOriginalTransactionHasNotPivot_ForCancellations()
		{
			AssertOriginalTransactionHasNotPivotStatus(EInvoicingPivotActionType.Cancel, ExpectedDependentCancelTransactionStatusWhenOriginalTransactionHasNotPivot);
		}

		public void TestOriginalTransactionHasDCDPivotBecauseEInvoicingDisabled_ForCancellations()
		{
			AssertOriginalTransactionHasDCDPivotStatusBecauseEInvoicingDisabled(EInvoicingPivotActionType.Cancel);
		}

		public void TestWaitForOriginalTransaction_ForCancellations()
		{
			foreach (var originalTransactionPivotStatus in OriginalTransactionPivotStatusToWaitForCancellations)
			{
				AssertWaitForOriginalTransaction(EInvoicingPivotActionType.Cancel, originalTransactionPivotStatus);
			}
		}

		#endregion

		#region Amendings

		public void TestAmendingBatchBehavior()
		{
			foreach (var amendingCase in AmendingSetUpAndExpectedValues)
			{
				AssertTransactionPivot(EInvoicingPivotActionType.Amend, amendingCase.OriginalTransactionPivotStatus, amendingCase.ExpectedOriginalTransactionPivotStatus, amendingCase.ExpectedOriginalTransactionBatchStatus, amendingCase.ExpectedAmendingPivotStatus);
			}
		}

		public void TestOriginalTransactionHasNotPivot_ForAmendings()
		{
			AssertOriginalTransactionHasNotPivotStatus(EInvoicingPivotActionType.Amend, ExpectedDependentAmendmentTransactionStatusWhenOriginalTransactionHasNotPivot);
		}

		public void TestOriginalTransactionHasDCDPivotBecauseEInvoicingDisabled_ForAmendings()
		{
			AssertOriginalTransactionHasDCDPivotStatusBecauseEInvoicingDisabled(EInvoicingPivotActionType.Amend);
		}

		public void TestWaitForOriginalTransaction_ForAmendings()
		{
			if (OriginalTransactionPivotStatusToWaitForAmendings.Length > 0)
			{
				foreach (var originalTransactionPivotStatus in OriginalTransactionPivotStatusToWaitForAmendings)
				{
					AssertWaitForOriginalTransaction(EInvoicingPivotActionType.Amend, originalTransactionPivotStatus);
				}
			}
			else
			{
				Assert(true);
			}
		}

		public void TestEnableTheBehaviourWaitForOriginalTransaction_ForAmendings()
		{
			AssertEnableTheBehaviourWaitForOriginalTransaction(false);
			AssertEnableTheBehaviourWaitForOriginalTransaction(true);
		}

		public void TestAmendingTransactionDoesNotHaveOriginalTransaction()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
			{
				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Amend, status: EInvoicingPivotState.Queued);
				arCreditNote.Factory.Save();

				var strategyMock = new Mock<IBatchCreatorStrategy>();
				strategyMock.Setup(x => x.SupportsWaitForOriginalTransactionForAmending).Returns(true);

				var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
				globalFactoryMock.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(strategyMock.Object);
				ObjectFactory.Substitute(globalFactoryMock.Object);

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();
				processor.PerformBatching(serviceLogger);

				var creditPivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
				AssertNotNull(creditPivot);
				AssertEquals(ExpectedPivotStatusWhenAmendingTransactionDoesNotHaveOriginalTransaction, creditPivot.AIP_Status);

				var creditBatch = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, creditPivot.AIP_AIB));
				if (ExpectedPivotStatusWhenAmendingTransactionDoesNotHaveOriginalTransaction == EInvoicingPivotState.Batched)
				{
					AssertNotNull(creditBatch);
					AssertEquals(EInvoicingBatchState.Ready, creditBatch.AIB_Status);
				}
				else
				{
					AssertNull(creditBatch);
				}
			}
		}

		#endregion

		public void TestTransactionsProcessingOrder()
		{
			AssertTransactionsProcessingOrder(true);
			AssertTransactionsProcessingOrder(false);
		}

		#region Implementation

		void AssertTransactionPivot(ZString pivotActionType, ZString originalTransactionPivotStatus, ZString expectedOriginalTransactionPivotStatus, ZString expectedOriginalTransactionBatchStatus, ZString expectedCancellationPivotStatus)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				ObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: ObjectCreator.GST1);
				AssertTransactionPivot(arInvoice, EInvoicingPivotActionType.Submit);

				if (pivotActionType == EInvoicingPivotActionType.Cancel)
				{
					// Update original invoice pivot status to Succeed to have AmendingPivotCanBeBatched as true.
					var arInvoicePivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
					arInvoicePivot.AIP_Status = EInvoicingPivotState.Succeed;
					arInvoicePivot.Factory.Save();

					// Reverse an amending transaction
					var arCreditNote = Helper.ObjectCreator.CreateARCreditNoteReverseTransaction(arInvoice);
					ObjectCreator.CreateInvoiceLine(arCreditNote, arCreditNote.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: ObjectCreator.GST1);
					var creditNoteActionType = SupportsWaitForOriginalTransactionForAmending ? EInvoicingPivotActionType.Amend : EInvoicingPivotActionType.Submit;
					AssertTransactionPivot(arCreditNote, creditNoteActionType);
				}
			}

			void AssertTransactionPivot(InvoicingBase invoice, ZString actionType)
			{
				// Pivot and batch transaction.
				var pivot = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(invoice, actionType: actionType, status: EInvoicingPivotState.Queued);
				var batch = Helper.ObjectCreator.CreateEInvoicingBatchForPivot(pivot, GetNextBatchNumber, EInvoicingBatchState.Sent);

				var expectedOriginalTransactionPivotMessage = $"[{originalTransactionPivotStatus}] Sometimes, original transaction pivots have error messages.";
				pivot.AIP_ErrorDescription = expectedOriginalTransactionPivotMessage;
				pivot.AIP_Status = originalTransactionPivotStatus;
				Factory.Save();

				// Create a reversal and batch again.
				var arReversal = CreateCancellationOrAmendingCreditNote(pivotActionType, invoice);
				AssertNotNull("CreditNote should not be null", arReversal);
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arReversal, actionType: pivotActionType, status: EInvoicingPivotState.Queued);
				Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();
				processor.PerformBatching(serviceLogger);

				var waitForOriginal = pivotActionType == EInvoicingPivotActionType.Amend && SupportsWaitForOriginalTransactionForAmending || pivotActionType == EInvoicingPivotActionType.Cancel;
				var originalTransactionShouldProcessed = !waitForOriginal || originalTransactionPivotStatus == EInvoicingPivotState.Queued && expectedOriginalTransactionPivotStatus == EInvoicingPivotState.Batched;
				var originalTransactionProccesingMessage = GetTransactionLogMessage(actionType, SupportsWaitForOriginalTransactionForAmending);

				AssertEquals(1, serviceLogger.Logs.Count(x => (x.Item1 == LogType.Debug) && x.Item2 == GetTransactionLogMessage(pivotActionType, SupportsWaitForOriginalTransactionForAmending)));
				AssertEquals(originalTransactionShouldProcessed ? 1 : 0, serviceLogger.Logs.Count(x => (x.Item1 == LogType.Debug) && x.Item2 == originalTransactionProccesingMessage));
				AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Information && x.Item2 == "Batching completed."));

				var pivotAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, invoice.PK));
				AssertNotNull(pivotAfterCreditNoteProcessed);
				AssertEquals(pivot.PK, pivotAfterCreditNoteProcessed.PK);
				AssertEquals(expectedOriginalTransactionPivotStatus, pivotAfterCreditNoteProcessed.AIP_Status);
				AssertEquals(expectedOriginalTransactionPivotMessage, pivotAfterCreditNoteProcessed.AIP_ErrorDescription);

				var batchAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterCreditNoteProcessed.AIP_AIB));
				AssertNotNull(batchAfterCreditNoteProcessed);
				AssertEquals(expectedOriginalTransactionBatchStatus, batchAfterCreditNoteProcessed.AIB_Status);

				var creditPivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arReversal.PK));
				AssertNotNull(creditPivot);
				AssertEquals(expectedCancellationPivotStatus, creditPivot.AIP_Status);
			}
		}

		void AssertOriginalTransactionHasNotPivotStatus(ZString pivotActionType, string expectedDependentTransactionStatusWhenOriginalTransactionHasNotPivot)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
			{
				// AR transaction was created before einvoicing was implemented for this country
				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				ObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: ObjectCreator.GST1);
				Factory.Save();

				if (!arInvoice.EInvoicingStatus.IsEmpty)
				{
					// In the event the above ARInvoice is eligible, delete the pivot so the transaction looks like it was created before eInvoicing was implemented.
					var pivot = arInvoice.GetMostRecentEInvoicingTransactionPivot();
					pivot.Delete();
					Factory.Save();
				}

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();
				processor.PerformBatching(serviceLogger);
				Factory.ReloadAll<AccEInvoicingTransactionPivot>();

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
				{
					// Create a Reversal/Amendment (pivotActionType) and batch again.
					var arCreditNote = CreateCancellationOrAmendingCreditNote(pivotActionType, arInvoice);
					var pivotCreditNote = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: pivotActionType, status: EInvoicingPivotState.Queued);
					arCreditNote.Factory.Save();

					processor.PerformBatching(serviceLogger);
					Factory.ReloadAll<AccEInvoicingTransactionPivot>();

					var pivotAfterCreditNoteProcessed = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
					AssertNull(pivotAfterCreditNoteProcessed);

					var creditPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
					AssertNotNull(creditPivot);
					AssertEquals(expectedDependentTransactionStatusWhenOriginalTransactionHasNotPivot, creditPivot.AIP_Status);

					var creditBatch = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, creditPivot.AIP_AIB));
					if (expectedDependentTransactionStatusWhenOriginalTransactionHasNotPivot == EInvoicingPivotState.Batched)
					{
						AssertNotNull(creditBatch);
						AssertEquals(EInvoicingBatchState.Ready, creditBatch.AIB_Status);
					}
					else
					{
						AssertNull(creditBatch);
					}
				}
			}
		}

		void AssertOriginalTransactionHasDCDPivotStatusBecauseEInvoicingDisabled(ZString pivotActionType)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
			{
				// AR transaction was created when EnableEInvoicingFunctionality is off
				ARInvoice arInvoice = null;
				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
				{
					arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
					ObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: ObjectCreator.GST1);
					arInvoice.Factory.Save();

					if (arInvoice.EInvoicingStatus.IsEmpty)
					{
						// In the event the above ARInvoice is ineligible, create a pivot so the transaction looks like it was, but got discarded anyway.
						var pivot = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, actionType: EInvoicingPivotActionType.Submit, status: EInvoicingPivotState.Discarded);
						pivot.AIP_ErrorDescription = AccEInvoicingTransactionPivot.ErrorDescriptionWhenEInvoicingDisabled();
						pivot.Factory.Save();
					}
					AssertEquals("Precondition: Discarded pivot", EInvoicingPivotState.Discarded, arInvoice.EInvoicingStatus);
				}

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();
				processor.PerformBatching(serviceLogger);

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
				{
					// Create a reversal and batch again.
					var arCreditNote = CreateCancellationOrAmendingCreditNote(pivotActionType, arInvoice);
					var pivotCreditNote = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Cancel, status: EInvoicingPivotState.Queued);
					arCreditNote.Factory.Save();

					processor.PerformBatching(serviceLogger);

					var pivotAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
					AssertNotNull(pivotAfterCreditNoteProcessed);
					AssertEquals(EInvoicingPivotState.Discarded, pivotAfterCreditNoteProcessed.AIP_Status);

					var creditPivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
					AssertNotNull(creditPivot);
					AssertEquals(ExpectedDependentTransactionStatusWhenOriginalTransactionHasDCDPivotBecauseEInvoicingDisabled, creditPivot.AIP_Status);
				}
			}
		}

		void AssertWaitForOriginalTransaction(ZString pivotActionType, ZString originalTransactionPivotStatus)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			var governmentAllocatedNumber = "123456";

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				ObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: ObjectCreator.GST1);
				AssertWaitForOriginalTransaction(arInvoice, EInvoicingPivotActionType.Submit);

				if (pivotActionType == EInvoicingPivotActionType.Cancel)
				{
					// Cancel an amending transaction
					var arCreditNote = Helper.ObjectCreator.CreateARCreditNoteReverseTransaction(arInvoice);
					ObjectCreator.CreateInvoiceLine(arCreditNote, arCreditNote.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: ObjectCreator.GST1);
					var creditNoteActionType = SupportsWaitForOriginalTransactionForAmending ? EInvoicingPivotActionType.Amend : EInvoicingPivotActionType.Submit;
					AssertWaitForOriginalTransaction(arCreditNote, creditNoteActionType);
				}
			}

			void AssertWaitForOriginalTransaction(InvoicingBase invoice, ZString actionType)
			{
				// Pivot
				var pivot = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(invoice, actionType: actionType, status: originalTransactionPivotStatus);
				Factory.Save();

				// Create a reversal and batch.
				var arCreditNote = CreateCancellationOrAmendingCreditNote(pivotActionType, invoice);
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: pivotActionType, status: EInvoicingPivotState.Queued);
				Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();
				processor.PerformBatching(serviceLogger);
				Factory.ReloadAll<AccEInvoicingTransactionPivot>();

				var creditPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
				AssertEquals(EInvoicingPivotState.Queued, creditPivot.AIP_Status);

				//Change original transaction pviot status and gvt number
				pivot.AIP_Status = EInvoicingPivotState.Succeed;
				if (originalTransactionPivotStatus == EInvoicingPivotState.Queued)
				{
					//For Queued we need to get the AccEInvoicingTransactionPivot from database instead to create it manually since PerformBatching already create it.
					var batch = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivot.AIP_AIB));
					batch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
				}
				else
				{
					var batch = Helper.ObjectCreator.CreateEInvoicingBatchForPivot(pivot, GetNextBatchNumber, EInvoicingBatchState.Sent);
					batch.AIB_GovernmentAllocatedNumber = governmentAllocatedNumber;
				}
				Factory.Save();

				processor.PerformBatching(serviceLogger);
				Factory.ReloadAll<AccEInvoicingTransactionPivot>();

				creditPivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
				AssertEquals(EInvoicingPivotState.Batched, creditPivot.AIP_Status);

				var creditBatch = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, creditPivot.AIP_AIB));
				AssertNotNull(creditBatch);
				AssertEquals(EInvoicingBatchState.Ready, creditBatch.AIB_Status);

				var expectedReversalGvtAllocatedNumber = ExpectPopulateGovernmentAllocatedNumberWithOriginalTransactionGvtNumber ? governmentAllocatedNumber : string.Empty;
				AssertEquals(expectedReversalGvtAllocatedNumber, creditBatch.AIB_GovernmentAllocatedNumber);
			}
		}

		void AssertTransactionsProcessingOrder(bool waitForOriginalTransaction)
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);

			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				GlbCompany.CurrentCompany.Factory.Save();

				// Create two ar transaction.
				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				ObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: ObjectCreator.GST1);
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Sent);
				arInvoice.Factory.Save();

				var otherArInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				ObjectCreator.CreateInvoiceLine(otherArInvoice, otherArInvoice.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: ObjectCreator.GST1);
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(otherArInvoice, status: EInvoicingPivotState.Queued);
				otherArInvoice.Factory.Save();

				// Create a cancellation.
				ARCreditNote arCreditNoteCan = (ARCreditNote)(Helper.ObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice)).amendTransaction;
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNoteCan, actionType: EInvoicingPivotActionType.Cancel, status: EInvoicingPivotState.Queued);
				arCreditNoteCan.Factory.Save();

				// Create a amend.
				ARCreditNote arCreditNoteAmd = (ARCreditNote)(Helper.ObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice)).amendTransaction;
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNoteAmd, actionType: EInvoicingPivotActionType.Amend, status: EInvoicingPivotState.Queued);
				arCreditNoteAmd.Factory.Save();

				var strategyMock = new Mock<IBatchCreatorStrategy>();
				strategyMock.Setup(x => x.SupportsWaitForOriginalTransactionForAmending).Returns(waitForOriginalTransaction);

				var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
				globalFactoryMock.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(strategyMock.Object);

				using (ObjectFactory.Substitute(globalFactoryMock.Object))
				{
					var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
					var serviceLogger = new DetailedLoggerForTest();
					processor.PerformBatching(serviceLogger);

					AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == ProcessingMessageQueued));
					AssertEquals(1, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == ProcessingMessageCancellations));
					AssertEquals(waitForOriginalTransaction ? 1 : 0, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == ProcessingMessageAmendings));

					strategyMock.Verify(x => x.SupportsWaitForOriginalTransactionForAmending, Times.Once);
					globalFactoryMock.Verify(x => x.GetCountryEInvoicingBatchCreatorStrategy(Country), Times.Once);
				}
			}
		}

		void AssertEnableTheBehaviourWaitForOriginalTransaction(bool waitForOriginalTransactionForAmending)
		{
			var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
			ObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1m, 100m, 10m, 0, taxRate: ObjectCreator.GST1);
			var pivot = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arInvoice, status: EInvoicingPivotState.Queued);
			arInvoice.Factory.Save();

			var pivotAfterProcess = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
			AssertNotNull(pivotAfterProcess);

			var batch = Helper.ObjectCreator.CreateEInvoicingBatchForPivot(pivotAfterProcess, GetNextBatchNumber, EInvoicingBatchState.Sent);
			batch.Factory.Save();
			pivotAfterProcess.AIP_Status = EInvoicingBatchState.Sent;
			pivotAfterProcess.Factory.Save();

			// Create a reversal and batch again.
			var arCreditNote = CreateCancellationOrAmendingCreditNote(EInvoicingPivotActionType.Amend, arInvoice);
			Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Amend, status: EInvoicingPivotState.Queued);
			arCreditNote.Factory.Save();

			var strategyMock = new Mock<IBatchCreatorStrategy>();
			strategyMock.Setup(x => x.SupportsWaitForOriginalTransactionForAmending).Returns(waitForOriginalTransactionForAmending);

			var globalFactoryMock = new Mock<IGlobalEInvoicingObjectFactory>();
			globalFactoryMock.Setup(x => x.GetCountryEInvoicingBatchCreatorStrategy(It.IsAny<ZString>())).Returns(strategyMock.Object);

			using (ObjectFactory.Substitute(globalFactoryMock.Object))
			{
				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				var serviceLogger = new DetailedLoggerForTest();
				processor.PerformBatching(serviceLogger);

				AssertEquals(waitForOriginalTransactionForAmending ? 1 : 0, serviceLogger.Logs.Count(x => x.Item1 == LogType.Debug && x.Item2 == ProcessingMessageAmendings));
			}
		}

		InvoicingBase CreateCancellationOrAmendingCreditNote(ZString pivotActionType, InvoicingBase arInvoice)
		{
			if (pivotActionType == EInvoicingPivotActionType.Cancel)
			{
				return (InvoicingBase)Helper.ObjectCreator.ReverseTransaction(arInvoice, out _);
			}
			else if (pivotActionType == EInvoicingPivotActionType.Amend)
			{
				return (InvoicingBase)(Helper.ObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, arInvoice)).amendTransaction;
			}
			else
			{
				return null;
			}
		}

		string GetTransactionLogMessage(ZString pivotActionType, bool supportsWaitForOriginalTransactionForAmending)
		{
			switch (pivotActionType)
			{
				case EInvoicingPivotActionType.Cancel:
					return ProcessingMessageCancellations;
				case EInvoicingPivotActionType.Amend:
					return supportsWaitForOriginalTransactionForAmending ? ProcessingMessageAmendings : ProcessingMessageQueued;
				case EInvoicingPivotActionType.Submit:
					return ProcessingMessageQueued;
				default:
					return string.Empty;
			}
		}

		ZInt GetNextBatchNumber => int.Parse(AccountingNumberFountainWrapperFactory.Instance.AccEInvoicingBatchNumber.GetNext(Db.Connection), CultureInfo.InvariantCulture);

		#endregion
	}
}
