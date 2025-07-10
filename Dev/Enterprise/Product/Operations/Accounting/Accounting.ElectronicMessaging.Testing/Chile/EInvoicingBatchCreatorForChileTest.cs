using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.EInvoicing;
using Enterprise.Accounting.ElectronicMessaging.Common;
using Enterprise.Accounting.ElectronicMessaging.Common.Testing;
using Enterprise.Integration;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.ElectronicMessaging.Chile.Testing
{
	[TestedType(typeof(EInvoicingBatchCreatorForChile))]
	public class EInvoicingBatchCreatorForChileTest : EInvoicingDependentBatchCreatorTest
	{
		protected override string Country => CountryCodes.Chile;

		protected override EInvoicingBatchCreatorBase GetBatchProcessor(GlbCompany company) => new EInvoicingBatchCreatorForChile(company);

		#region Cancellations

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedCancellationPivotStatus)[] CancellationSetUpAndExpectedValues
			=> new[] {
				//Batch cancellation
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Failed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingBatchState.Ready, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.BatchedWithError, EInvoicingPivotState.BatchedWithError, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Batched, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				//Discard cancellation
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded)
			};

		protected override string[] OriginalTransactionPivotStatusToWaitForCancellations => new[]
		{
			EInvoicingPivotState.BatchedWithError,
			EInvoicingPivotState.Delivered,
			EInvoicingPivotState.Sent,
			EInvoicingPivotState.Pending,
			EInvoicingPivotState.Queued,
			EInvoicingPivotState.Batched
		};

		protected override string ExpectedDependentCancelTransactionStatusWhenOriginalTransactionHasNotPivot => EInvoicingPivotState.Discarded;

		#endregion

		#region Amending

		protected override (string OriginalTransactionPivotStatus, string ExpectedOriginalTransactionPivotStatus, string ExpectedOriginalTransactionBatchStatus, string ExpectedAmendingPivotStatus)[] AmendingSetUpAndExpectedValues
			=> new[] {
				//Sent amending
				(EInvoicingPivotState.Succeed, EInvoicingPivotState.Succeed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				(EInvoicingPivotState.Failed, EInvoicingPivotState.Failed, EInvoicingBatchState.Sent, EInvoicingPivotState.Batched),
				//Wait for original transaction
				(EInvoicingPivotState.Queued, EInvoicingPivotState.Batched, EInvoicingBatchState.Ready, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Batched, EInvoicingPivotState.Batched, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Sent, EInvoicingPivotState.Sent, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Delivered, EInvoicingPivotState.Delivered, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				(EInvoicingPivotState.Pending, EInvoicingPivotState.Pending, EInvoicingBatchState.Sent, EInvoicingPivotState.Queued),
				//Discard amending
				(EInvoicingPivotState.Discarded, EInvoicingPivotState.Discarded, EInvoicingBatchState.Sent, EInvoicingPivotState.Discarded),
			};

		protected override string[] OriginalTransactionPivotStatusToWaitForAmendings => new[]
		{
			EInvoicingPivotState.BatchedWithError,
			EInvoicingPivotState.Delivered,
			EInvoicingPivotState.Sent,
			EInvoicingPivotState.Pending,
			EInvoicingPivotState.Queued,
			EInvoicingPivotState.Batched
		};

		protected override bool SupportsWaitForOriginalTransactionForAmending => true;

		protected override string ExpectedPivotStatusWhenAmendingTransactionDoesNotHaveOriginalTransaction => EInvoicingPivotState.Discarded;

		protected override string ExpectedDependentAmendmentTransactionStatusWhenOriginalTransactionHasNotPivot => EInvoicingPivotState.Discarded;

		public void TestCreditNoteWithoutOriginalTransactionButWithManualOriginalTransactionNumber()
			=> AssertCreditNoteWithoutOriginalTransaction("99-999", ZDate.Empty);

		public void TestCreditNoteWithoutOriginalTransactionButWithManualOriginalTransactionDate()
			=> AssertCreditNoteWithoutOriginalTransaction(null, new ZDate(2024, 12, 31));

		public void TestCreditNoteWithoutOriginalTransactionButWithManualOriginalTransactionNumberAndDate()
			=> AssertCreditNoteWithoutOriginalTransaction("99-999", new ZDate(2024, 12, 31));

		void AssertCreditNoteWithoutOriginalTransaction(string manualTransactionNumber, ZDate manualTransactionDate)
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Country))
			{
				var arCreditNote = Factory.NewWithValidTestData<ARCreditNote>();
				arCreditNote.AH_OriginalTransactionNum = manualTransactionNumber;
				arCreditNote.AH_OriginalInvoiceDate = manualTransactionDate;

				Helper.ObjectCreator.CreateEInvoicingTransactionPivot(arCreditNote, actionType: EInvoicingPivotActionType.Amend);
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
				AssertEquals(EInvoicingPivotState.Batched, creditPivot.AIP_Status);
			}
		}

		#endregion
	}
}
