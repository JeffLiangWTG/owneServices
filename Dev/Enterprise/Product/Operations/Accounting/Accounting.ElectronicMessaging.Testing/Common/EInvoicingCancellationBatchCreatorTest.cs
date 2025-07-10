using System;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Core;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Common.Testing
{
	public abstract class EInvoicingCancellationBatchCreatorTest : EInvoicingBatchCreatorBaseTest
	{
		public virtual void TestBatchCreatedForPivotsWithCancelActionType()
		{
			AssertTransactionPivotCreation(EInvoicingPivotState.Succeed, EInvoicingPivotState.Batched, EInvoicingPivotState.Succeed);
			AssertTransactionPivotCreation(EInvoicingPivotState.Delivered, EInvoicingPivotState.Batched, EInvoicingPivotState.Delivered);

			AssertTransactionPivotCreation(EInvoicingPivotState.Failed, EInvoicingPivotState.Discarded);
			AssertTransactionPivotCreation(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded);

			AssertTransactionPivotCreation(EInvoicingPivotState.Batched, EInvoicingPivotState.Discarded, expectedSUBBatchStatus: EInvoicingBatchState.Discarded);
			AssertTransactionPivotCreation(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.Discarded, expectedSUBBatchStatus: EInvoicingBatchState.Discarded);
			AssertTransactionPivotCreation(EInvoicingPivotState.Queued, EInvoicingPivotState.Discarded, expectedSUBBatchStatus: EInvoicingBatchState.Discarded);

			AssertTransactionPivotCreation(EInvoicingPivotState.Pending, EInvoicingPivotState.Queued, EInvoicingPivotState.Pending);
			AssertTransactionPivotCreation(EInvoicingPivotState.Sent, EInvoicingPivotState.Queued, EInvoicingPivotState.Sent);
		}

		protected void AssertTransactionPivotCreation(ZString subPivotStatus, ZString expectedCANPivotStatus, string expectedSUBPivotStatus = EInvoicingPivotState.Discarded, string expectedSUBBatchStatus = EInvoicingBatchState.Sent, string specificComplianceSubType = "TXI")
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
			{
				Helper.AddCustomsCodeForCountryIfMissing(GlbCompany.CurrentCompany.OrgProxy, CountryCode, CustomOrgCusCode, CustomRegNumber);
				GlbCompany.CurrentCompany.Factory.Save();

				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				arInvoice.AH_ComplianceSubType = specificComplianceSubType;
				arInvoice.AH_TransactionReference = "12345";
				Factory.Save();

				var pivot = Factory.LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivot);

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				processor.PerformBatching(new DetailedLoggerForTest());

				var pivotAfterProcess = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivotAfterProcess);

				var batch = Factory.LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterProcess.AIP_AIB));
				AssertNotNull(batch);

				pivotAfterProcess.AIP_Status = subPivotStatus;
				pivotAfterProcess.Factory.Save();
				batch.AIB_Status = EInvoicingBatchState.Sent;
				batch.Factory.Save();

				var arCreditNote = Helper.ObjectCreator.CreateARCreditNote("CRD001", Helper.ObjectCreator.AALSHI);
				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, Constants.EInvoicingPivotActionType.Cancel);
				arCreditNote.AH_TransactionBelongsToGroup = arInvoice.PK;
				arCreditNote.Factory.Save();

				processor.PerformBatching(new DetailedLoggerForTest());

				var pivotAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
				AssertNotNull(pivotAfterCreditNoteProcessed);
				AssertEquals(expectedSUBPivotStatus, pivotAfterCreditNoteProcessed.AIP_Status);

				var creditPivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
				AssertNotNull(creditPivot);
				AssertEquals(EInvoicingPivotActionType.Cancel, creditPivot.AIP_ActionType);
				AssertEquals(expectedCANPivotStatus, creditPivot.AIP_Status);

				var batchAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingBatch>(new ZQuery(AccEInvoicingBatchSchema.PK, pivotAfterCreditNoteProcessed.AIP_AIB));
				AssertNotNull(batchAfterCreditNoteProcessed);
				AssertEquals(expectedSUBBatchStatus, batchAfterCreditNoteProcessed.AIB_Status);
			}
		}

		public virtual void TestOriginalTransactionHasNotPivotStatus()
		{
			var complianceDate = ZDateTime.Today.AddDays(-10);
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCode))
			{
				var arInvoice = Helper.ObjectCreator.CreateARInvoice<ARInvoice>("0001", Helper.ObjectCreator.USD, 1m, Helper.ObjectCreator.AALSHI);
				arInvoice.Factory.Save();

				var processor = GetBatchProcessor(GlbCompany.CurrentCompany);
				processor.PerformBatching(new DetailedLoggerForTest());

				using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
				using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, complianceDate.ToDateTime()))
				{
					var arCreditNote = (ARCreditNote)Helper.ObjectCreator.ReverseTransaction(arInvoice, out _);
					var pivotCreditNote = Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Cancel, status: EInvoicingPivotState.Queued);
					arCreditNote.Factory.Save();

					processor.PerformBatching(new DetailedLoggerForTest());

					var pivotAfterCreditNoteProcessed = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arInvoice.PK));
					AssertNull(pivotAfterCreditNoteProcessed);

					var creditPivot = new BusinessObjectFactory().LoadTop1<AccEInvoicingTransactionPivot>(new ZQuery(AccEInvoicingTransactionPivotSchema.AIP_ParentID, arCreditNote.PK));
					AssertNotNull(creditPivot);
					AssertEquals(ExpectedStatusWithEmptyStatusOnOriginalTransaction, creditPivot.AIP_Status);
				}
			}
		}

		protected abstract ZString CountryCode { get; }

		protected virtual string ExpectedStatusWithEmptyStatusOnOriginalTransaction => EInvoicingPivotState.Queued;

		ZString CustomOrgCusCode => OrgCusCode.CodeTypes.VATCode;

		ZString CustomRegNumber => "0100233488";
	}
}
