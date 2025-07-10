using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance.Testing;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(MalaysiaComplianceInfoEInvoicingExtension))]
	public class MalaysiaComplianceInfoEInvoicingExtensionTest : MalaysiaComplianceInfoTest
	{
		#region IEInvoicingTransactionValidation

		public void TestGetValidationMessageForAfterPostAction_StatusCheck()
		{
			var statusCheckMessage = "The transaction is not eligible for requests because the E-Reporting Status of the transaction is not 'DLV - Delivered' or 'IMP - In Processing' or 'FAL - Failed' with E-Reporting Govt #.";
			var reverseCheckMessage = "The transaction is not eligible for requests as the transaction has been reversed.";

			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.Malaysia, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-1)))
			{
				var transactionHeaderReversed = Factory.NewWithValidTestData<AccTransactionHeader>();
				transactionHeaderReversed.AH_IsCancelled = true;

				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);

				var pivotSubmit = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Succeed);
				pivotSubmit.AIP_ParentID = transactionHeaderReversed.PK;

				var batchSubmit = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotSubmit, 1, EInvoicingBatchState.Sent);
				Factory.Save();

				AssertEquals(reverseCheckMessage, Extension.GetValidationMessageForAfterPostAction(new[] { pivotSubmit }, EInvoicingPivotActionType.StatusCheck));

				transactionHeaderReversed.AH_IsCancelled = false;
				AssertEquals(statusCheckMessage, Extension.GetValidationMessageForAfterPostAction(new[] { pivotSubmit }, EInvoicingPivotActionType.StatusCheck));

				pivotSubmit.AIP_Status = EInvoicingPivotState.InProcessing;
				AssertNullOrEmpty(Extension.GetValidationMessageForAfterPostAction(new[] { pivotSubmit }, EInvoicingPivotActionType.StatusCheck));

				pivotSubmit.AIP_Status = EInvoicingPivotState.Delivered;
				AssertNullOrEmpty(Extension.GetValidationMessageForAfterPostAction(new[] { pivotSubmit }, EInvoicingPivotActionType.StatusCheck));

				pivotSubmit.AIP_Status = EInvoicingPivotState.Failed;
				AssertEquals(statusCheckMessage, Extension.GetValidationMessageForAfterPostAction(new[] { pivotSubmit }, EInvoicingPivotActionType.StatusCheck));

				transactionHeaderReversed.AH_GovernmentAllocatedID = "F9D425P6DS7D8IU";
				AssertNullOrEmpty(Extension.GetValidationMessageForAfterPostAction(new[] { pivotSubmit }, EInvoicingPivotActionType.StatusCheck));
			}
		}

		public void TestGetValidationMessageForAfterPostAction_DocumentAction()
		{
			var transactionHeader = Factory.NewWithValidTestData<AccTransactionHeader>();
			var pivot1 = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot1.AIP_ActionType = ZString.Empty;
			pivot1.AIP_Status = ZString.Empty;
			pivot1.AIP_ParentID = transactionHeader.PK;
			var pivot2 = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			Pivots = new List<AccEInvoicingTransactionPivot> { pivot1, pivot2 };

			var transactionHeaderReversed = Factory.NewWithValidTestData<AccTransactionHeader>();
			transactionHeaderReversed.AH_IsCancelled = true;
			var pivot3 = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot3.AIP_ActionType = ZString.Empty;
			pivot3.AIP_Status = ZString.Empty;
			pivot3.AIP_ParentID = transactionHeaderReversed.PK;
			PivotsWithReversedParentTransaction = new List<AccEInvoicingTransactionPivot> { pivot3 };

			PivotStatus = new List<ZString> {
				EInvoicingPivotState.Queued,
				EInvoicingPivotState.Batched,
				EInvoicingPivotState.BatchedWithError,
				EInvoicingPivotState.Sent,
				EInvoicingPivotState.Delivered,
				EInvoicingPivotState.Succeed,
				EInvoicingPivotState.Failed,
				EInvoicingPivotState.Discarded,
				EInvoicingPivotState.Pending,
				EInvoicingPivotState.AwaitingReview,
				EInvoicingPivotState.InProcessing
			};

			foreach (var actionType in EInvoicingPivotActionType.CommandActionTypes)
			{
				foreach (var status in PivotStatus)
				{
					AssertForGetDocMessage(actionType, status);
				}
			}
		}

		void AssertForGetDocMessage(ZString actionType, ZString status)
		{
			Pivots[1].AIP_ActionType = actionType;
			Pivots[1].AIP_Status = status;
			PivotsWithReversedParentTransaction[0].AIP_ActionType = actionType;
			PivotsWithReversedParentTransaction[0].AIP_Status = status;

			var gUIActionProvider = Extension as IEInvoicingTransactionValidation;

			AssertErrorMessage(Pivots);
			AssertErrorMessage(PivotsWithReversedParentTransaction);

			void AssertErrorMessage(List<AccEInvoicingTransactionPivot> pivots)
			{
				if (pivots.FirstOrDefault().ParentTransactionHeader.IsReversed)
				{
					AssertEquals("The transaction is not eligible for requests as the transaction has been reversed.", gUIActionProvider.GetValidationMessageForAfterPostAction(pivots, EInvoicingPivotActionType.DocumentAction));
				}
				else if (actionType == EInvoicingPivotActionType.Submit && status == EInvoicingPivotState.Succeed)
				{
					AssertEquals(string.Empty, gUIActionProvider.GetValidationMessageForAfterPostAction(pivots, EInvoicingPivotActionType.DocumentAction));
				}
				else
				{
					AssertEquals("The transaction is not eligible for requests as the E-Reporting Status of the transaction is not equal to SUC.", gUIActionProvider.GetValidationMessageForAfterPostAction(pivots, EInvoicingPivotActionType.DocumentAction));
				}
			}
		}

		List<AccEInvoicingTransactionPivot> Pivots;
		List<AccEInvoicingTransactionPivot> PivotsWithReversedParentTransaction;
		List<ZString> PivotStatus;

		#endregion

		#region IExistActivePivotCheckProvider

		public void TestCheckExistActivePivot()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.Malaysia, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-1)))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);

				var pivotSubmit = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Succeed);
				var batchSubmit = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotSubmit, 1, EInvoicingBatchState.Sent);
				Factory.Save();
				var pivotQuery = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Sent);
				var batchQuery = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuery, 2, EInvoicingBatchState.Sent);
				Factory.Save();

				var utcNow = ZDateTime.UtcNow;
				pivotSubmit.AIP_LastSentTimeUtc = utcNow;

				AssertEquals(true, Extension.CheckExistActivePivot(pivotQuery));

				pivotQuery.AIP_LastSentTimeUtc = utcNow.AddMinutes(-20);
				AssertEquals(true, Extension.CheckExistActivePivot(pivotQuery));

				pivotQuery.AIP_LastSentTimeUtc = utcNow.AddMinutes(-40);
				AssertEquals(false, Extension.CheckExistActivePivot(pivotQuery));
			}
		}

		public void TestCheckCanExistSucceedPivot()
		{
			using (TestObjectCreator.SetUpForTestingEInvoicing(CountryCodes.Malaysia, true))
			using (AccountingMasterFilesRegistry.Instance.EReportingComplianceDate.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, DateTime.Today.AddDays(-1)))
			{
				var arInvoice = TestObjectCreator.CreateARInvoice<ARInvoice>("001", TestObjectCreator.KRW, 1m, TestObjectCreator.Debtor);
				TestObjectCreator.CreateInvoiceLine(arInvoice, arInvoice.TransactionCurrency, 1.0m, 200m, 20m, 0m, taxRate: TestObjectCreator.GST1);

				var pivotSubmit = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.Submit, EInvoicingPivotState.Succeed);
				var batchSubmit = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotSubmit, 1, EInvoicingBatchState.Sent);

				var pivotQuerySucceed = arInvoice.CreateNewEInvoicingPivot(Factory, EInvoicingPivotActionType.StatusCheck, EInvoicingPivotState.Succeed);
				var batchQuerySucceed = TestObjectCreator.CreateEInvoicingBatchForPivot(pivotQuerySucceed, 2, EInvoicingBatchState.Sent);

				Factory.Save();

				AssertEquals((true, ZString.Empty), Extension.CanExistSucceedPivot());
			}
		}

		#endregion

		public void TestGetCantReverseErrorMessageOfTransactionsWithDifferentEReportingStatus()
		{
			var expectedError = "Reversing is not allowed if the transaction is not taken as Invalid. If applicable, please Amend the Original Transaction in Job Billing module.";

			var arInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var apInvoice1 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);

			var arInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var apInvoice2 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP_INV2", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			TestObjectCreator.CreateEInvoicingTransactionPivot((ARInvoice)arInvoice2, status: EInvoicingPivotState.Discarded);
			TestObjectCreator.CreateEInvoicingTransactionPivot((APInvoice)apInvoice2, status: EInvoicingPivotState.Discarded);

			var arInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV3", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var apInvoice3 = TestObjectCreator.CreateInvoiceWithLine(typeof(APInvoice), "AP_INV3", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			TestObjectCreator.CreateEInvoicingTransactionPivot((ARInvoice)arInvoice3, status: EInvoicingPivotState.Queued);
			TestObjectCreator.CreateEInvoicingTransactionPivot((APInvoice)apInvoice3, status: EInvoicingPivotState.Queued);

			Factory.Save();

			using (TestObjectCreator.SetUpForTestingEInvoicing(Core.Constants.CountryCodes.Malaysia, true))
			{
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(arInvoice1));
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(apInvoice1));

				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(arInvoice2));
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(apInvoice2));

				AssertEquals(expectedError, Extension.GetCantReverseErrorMessage(arInvoice3));
				AssertEquals(expectedError, Extension.GetCantReverseErrorMessage(apInvoice3));

				TestObjectCreator.CreateEInvoicingTransactionPivot((ARInvoice)arInvoice3, actionType: EInvoicingPivotActionType.DocumentDetail);
				TestObjectCreator.CreateEInvoicingTransactionPivot((APInvoice)apInvoice3, actionType: EInvoicingPivotActionType.DocumentDetail);

				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(arInvoice3));
				AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(apInvoice3));
			}
		}

		public void TestGetCantReverseErrorMessageWithReversedTransaction()
		{
			var expectedError = "This transaction cannot be reversed because it has already been reversed or is a reversal of another transaction.";

			var arInvoice = TestObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "AR_INV1", TestObjectCreator.AUD, 1M, 10M, 0M, 10M, 0M);
			var job = TestObjectCreator.CreateAndSaveTestShipmentJob(JobHeaderStatus.Working.Code);
			var arCreditNote = TestObjectCreator.CreateARCreditNoteWithLine("ARCRD001", TestObjectCreator.Debtor, TestObjectCreator.AUD, 1.0m, "Credit Note", job, TestObjectCreator.CC1, 100.00m, ZDateTime.Today, false);

			string cantReverseMessage;
			TestObjectCreator.ReverseTransaction(arInvoice, out cantReverseMessage);
			TestObjectCreator.ReverseTransaction(arCreditNote, out cantReverseMessage);
			Factory.Save();

			var arInvoiceList = new List<InvoicingBase> { arInvoice, arCreditNote };

			arInvoiceList.ForEach(invoice => Assert("Precondition", invoice.IsReversed));

			using (TestObjectCreator.SetUpForTestingEInvoicing(Core.Constants.CountryCodes.Malaysia, true))
			{
				arInvoiceList.ForEach(invoice =>
				{
					AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(invoice));
					InvoicingBaseReversing reversing = new InvoicingBaseReversing(invoice);
					Assert("Should not be able to reverse transaction", !reversing.CanReverseTransaction);
					AssertEquals("Can't reverse reason", expectedError, reversing.CantReverseErrorMessage);
				});
			}

			using (TestObjectCreator.SetUpForTestingEInvoicing(Core.Constants.CountryCodes.Malaysia, false))
			{
				arInvoiceList.ForEach(invoice =>
				{
					AssertNullOrEmpty(Extension.GetCantReverseErrorMessage(invoice));
					InvoicingBaseReversing reversing = new InvoicingBaseReversing(invoice);
					Assert("Should not be able to reverse transaction", !reversing.CanReverseTransaction);
					AssertEquals("Can't reverse reason", expectedError, reversing.CantReverseErrorMessage);
				});
			}
		}

		TestObjectCreator TestObjectCreator => testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator testObjectCreator;

		MalaysiaComplianceInfoEInvoicingExtension Extension => TestObject as MalaysiaComplianceInfoEInvoicingExtension;
	}
}
