using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.Base.Reversing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CountryCompliance.Testing.Vietnam;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Core;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.EInvoicing.Testing
{
	[TestedType(typeof(VietnamComplianceInfoEInvoicingExtension))]
	public class VietnamComplianceInfoEInvoicingExtensionTest : VietnamComplianceInfoTest
	{
		public void TestReversingOriginalTransactionWithAmendments()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.DisallowReversingOriginalTransactionsWhenAmendmentsAreNotReversed.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, false))
			{
				var originalInvoice = ObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), ObjectCreator.AUD, 1m, ObjectCreator.ABIGAS);
				ObjectCreator.CreateEInvoicingTransactionPivot(originalInvoice, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				var amendingCreditNote = (CreditNote)ObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, originalInvoice).amendTransaction;
				AssertNotNull(amendingCreditNote);

				Factory.Save();

				var amendingTransactions = originalInvoice.GetRelatedAmendingTransactions();
				AssertEquals("Amending transaction available", 1, amendingTransactions.Count);
				Assert(amendingTransactions.Contains(amendingCreditNote));
				Factory.Save();

				var reversing = new InvoicingBaseReversing(amendingCreditNote);
				AssertEquals("Precondition", true, reversing.CanReverseTransaction);
				AssertNullOrEmpty("Precondition", reversing.GenerateCantReverseErrorMessage_ForTestOnly());

				ObjectCreator.CreateEInvoicingTransactionPivot(amendingCreditNote);
				Factory.Save();

				AssertEquals("Should not be able to reverse transaction", false, reversing.CanReverseTransaction);
				AssertEquals("Error message", "The credit note (adjustment invoice) has been submitted for E-Reporting and cannot be reversed.", reversing.GenerateCantReverseErrorMessage_ForTestOnly());
			}
		}

		[SuspendCriticalValidation]
		public void TestAmendARTransaction_PreventSecondAmendWithCRD()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(Core.Constants.CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testObjectCreator = new TestObjectCreator(Factory);
				testObjectCreator.CreateTestPeriods(ZDateTime.Today.AddDays(-10));
				var shipment = testObjectCreator.CreateShipment("S00001000");
				var job = testObjectCreator.CreateJob(shipment, testObjectCreator.LocalClient, 0, testObjectCreator.Agent, 0);

				var charge = testObjectCreator.CreateCharge(job, testObjectCreator.CC1, "Desc 1", testObjectCreator.AUD, 50M, null, testObjectCreator.AUD, -100, testObjectCreator.Debtor);
				testObjectCreator.Debtor.CompanyData.OB_ARVATConfig = AccountingMasterFilesConstants.OrganisationTaxConfiguartionTypes.CashBasis.Code;
				Factory.Save();

				var invoiceWithLine = testObjectCreator.CreateInvoiceWithLine(typeof(ARInvoice), "001", testObjectCreator.AUD, 1.0m, 100.00m, 10.00m, 100.00m, 10.00m);
				invoiceWithLine.AH_OH = testObjectCreator.Debtor.PK;
				invoiceWithLine.Lines[0].AL_GE = job.Department.PK;
				invoiceWithLine.AH_TransactionCategory = "FIN";
				invoiceWithLine.AH_JH = job.PK;
				invoiceWithLine.Lines[0].AL_AC = testObjectCreator.CC1.PK;
				invoiceWithLine.Lines[0].AL_JH = job.PK;
				Factory.Save();

				var pivot = testObjectCreator.CreateEInvoicingTransactionPivot(invoiceWithLine, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				var resultMessage = ZString.Empty;
				var resultCaption = ZString.Empty;
				var securityHelper = new JobInvoicingSecurityHelper(shipment.InvoicingSupporter.JobInvoicingSecurity);
				var creditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
					(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

				AssertEquals("Original transaction", invoiceWithLine.PK, creditNote.OriginalTransaction.PK);
				Assert(resultMessage.IsEmpty);
				Assert(resultCaption.IsEmpty);

				creditNote.Factory.Save();

				testObjectCreator.CreateEInvoicingTransactionPivot((CreditNote)creditNote);
				Factory.Save();

				var secondCreditNote = CreditNoteAmendingHelper.AmendARTransaction(TransactionTypes.CreditNote, invoiceWithLine, securityHelper,
					(message, caption) => SetMessageAndCaption(message, caption), (message, caption) => SetMessageAndCaption(message, caption));

				AssertNull(secondCreditNote);
				AssertEquals("Message", "The selected invoice is linked to an amendment credit note and a second amendment with credit note is not allowed.", resultMessage);
				AssertEquals("Caption", "Cannot amend transaction", resultCaption);

				void SetMessageAndCaption(ZString message, ZString caption)
				{
					resultMessage = message;
					resultCaption = caption;
				}
			}
		}

		public void TestGetValidationMessageForAfterPostAction()
		{
			var extension = new VietnamComplianceInfoEInvoicingExtension();
			var pivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			pivot.AIP_ActionType = EInvoicingPivotActionType.Adjustment;
			var pivots = new List<AccEInvoicingTransactionPivot>();
			pivots.Add(pivot);
			var message = extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);

			AssertEquals(extension.NotEligibleForRequestsMessage, message);

			var submitSucceedPivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			submitSucceedPivot.AIP_ActionType = EInvoicingPivotActionType.Submit;
			submitSucceedPivot.AIP_Status = EInvoicingPivotState.Succeed;
			pivots.Add(submitSucceedPivot);

			message = extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertNullOrEmpty(message);

			var cancelSucceedPivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			cancelSucceedPivot.AIP_ActionType = EInvoicingPivotActionType.Cancel;
			cancelSucceedPivot.AIP_Status = EInvoicingPivotState.Succeed;
			pivots.Remove(submitSucceedPivot);
			pivots.Add(cancelSucceedPivot);

			message = extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertNullOrEmpty(message);

			var approveSucceedPivot = Factory.NewWithValidTestData<AccEInvoicingTransactionPivot>();
			approveSucceedPivot.AIP_ActionType = EInvoicingPivotActionType.Approve;
			approveSucceedPivot.AIP_Status = EInvoicingPivotState.Succeed;
			pivots.Remove(cancelSucceedPivot);
			pivots.Add(approveSucceedPivot);

			message = extension.GetValidationMessageForAfterPostAction(pivots, ZString.Empty);
			AssertNullOrEmpty(message);
		}

		public void TestIsCircular78()
		{
			var extension = new VietnamComplianceInfoEInvoicingExtension();
			var sequenceNumberMaxDigits = 8;

			AssertEquals("IsCircular78 is ture when sequenceNumberMaxDigits is 8", true, extension.IsCircular78(sequenceNumberMaxDigits));

			sequenceNumberMaxDigits = 7;

			AssertEquals("IsCircular78 is false when sequenceNumberMaxDigits is not 8", false, extension.IsCircular78(sequenceNumberMaxDigits));
		}

		public void TestGetCantAmendErrorMessageOfAmendWithInvoice()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var extension = new VietnamComplianceInfoEInvoicingExtension();
				var originalInvoiceWithStatusNotSucceed = ObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), ObjectCreator.AUD, 1m, ObjectCreator.ABIGAS);
				ObjectCreator.CreateEInvoicingTransactionPivot(originalInvoiceWithStatusNotSucceed, status: EInvoicingPivotState.Queued);
				Factory.Save();

				var amendingInvoiceWithStatusNotSucceed = ObjectCreator.AmendARTransaction(TransactionTypes.Invoice, originalInvoiceWithStatusNotSucceed).amendTransaction;
				AssertNull(amendingInvoiceWithStatusNotSucceed);
				AssertEquals("Error Message", "Invoice cannot be amended until it has been submitted with status 'SUC'.", extension.GetCantAmendErrorMessage(originalInvoiceWithStatusNotSucceed, TransactionTypes.Invoice));

				var originalInvoiceWithStatusSucceed = ObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), ObjectCreator.AUD, 1m, ObjectCreator.ABIGAS);
				ObjectCreator.CreateEInvoicingTransactionPivot(originalInvoiceWithStatusSucceed, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				var amendingInvoice = (InvoicingBase)ObjectCreator.AmendARTransaction(TransactionTypes.Invoice, originalInvoiceWithStatusSucceed).amendTransaction;
				AssertNotNull(amendingInvoice);
				Factory.Save();

				ObjectCreator.CreateEInvoicingTransactionPivot(amendingInvoice, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				AssertEquals("Error Message", "The selected invoice is linked to an amendment invoice and a second amendment with invoice is not allowed.", extension.GetCantAmendErrorMessage(originalInvoiceWithStatusSucceed, TransactionTypes.Invoice));
			}
		}

		public void TestGetCantAmendErrorMessageWhenAlreadyHaveAmendmentOfDifferentType()
		{
			using (GlbCompany.CurrentCompany.TemporarilySetCountry(CountryCodes.VietNam))
			using (AccountingMasterFilesRegistry.Instance.EnableEInvoicingFunctionality.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamIssuePositiveAdjustmentViaAmendWithInvoice.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			using (AccountingConfigurationRegistry.Instance.VietnamEInvoicingAdjustment.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var extension = new VietnamComplianceInfoEInvoicingExtension();
				var originalInvoiceWithStatusSucceedForCreditNote = ObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), ObjectCreator.AUD, 1m, ObjectCreator.ABIGAS);
				ObjectCreator.CreateEInvoicingTransactionPivot(originalInvoiceWithStatusSucceedForCreditNote, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				var amendingCreditNote = (InvoicingBase)ObjectCreator.AmendARTransaction(TransactionTypes.CreditNote, originalInvoiceWithStatusSucceedForCreditNote).amendTransaction;
				AssertNotNull(amendingCreditNote);
				Factory.Save();

				ObjectCreator.CreateEInvoicingTransactionPivot(amendingCreditNote, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				AssertEquals("Error Message", "The selected invoice is linked to an amendment credit note and a second amendment with invoice is not allowed.", extension.GetCantAmendErrorMessage(originalInvoiceWithStatusSucceedForCreditNote, TransactionTypes.Invoice));

				var originalInvoiceWithStatusSucceedForInvoice = ObjectCreator.CreateARInvoice<ARInvoice>(TestObjectCreator.GetRandomString(3), ObjectCreator.AUD, 1m, ObjectCreator.ABIGAS);
				ObjectCreator.CreateEInvoicingTransactionPivot(originalInvoiceWithStatusSucceedForInvoice, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				var amendingInvoice = (InvoicingBase)ObjectCreator.AmendARTransaction(TransactionTypes.Invoice, originalInvoiceWithStatusSucceedForInvoice).amendTransaction;
				AssertNotNull(amendingInvoice);
				Factory.Save();

				ObjectCreator.CreateEInvoicingTransactionPivot(amendingInvoice, status: EInvoicingPivotState.Succeed);
				Factory.Save();

				AssertEquals("Error Message", "The selected invoice is linked to an amendment invoice and a second amendment with credit note is not allowed.", extension.GetCantAmendErrorMessage(originalInvoiceWithStatusSucceedForInvoice, TransactionTypes.CreditNote));
			}
		}

		TestObjectCreator ObjectCreator => fObjectCreator ?? (fObjectCreator = new TestObjectCreator(Factory));
		TestObjectCreator fObjectCreator;
	}
}
