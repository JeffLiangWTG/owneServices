using System;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.PaymentApproval;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.Base.Transaction;
using Enterprise.Accounting.Business.Riba;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Accounting.Business.Base.Matching.Testing
{
	class MatchingValidationTest : TestCaseWithFactory
	{
		public void TestPayAmountDoesntExceedSpecificCurrencyAmountWhenMatchingInvoiceToComPayPayment()
		{
			APPaymentApprovalWithoutAuthorisation approval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval.AV_PaymentType = ReceiptTypes.eNettDirectDebit;
			approval.AV_RX_NKPaymentCurrency = "USD";
			approval.AV_Amount = 100m;
			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;
			APInvoiceLine line = (APInvoiceLine)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			line.AL_OSExTaxAmount = 50m;
			Factory.Save();
			IMatchingCollection collection = new IMatchingCollection(Factory) { invoice, approval };
			AssertNotNull(invoice.PaymentApprovalCurrentlyBeingMatched);
			IMatching invoiceAsIMatching = invoice;
			invoiceAsIMatching.OSPartialPaymentAmount = -50m;
			((MatchingValidation)invoice.Validation).ValidateOSPartialPaymentAmount();
			AssertHasError(invoiceAsIMatching.OSPartialPaymentAmountInfo, "When matching a ComPay payment, you can only pay an invoice up to a total of its lines that match the payment currency.");

			line.AL_RX_NKTransactionCurrency = "USD";
			invoiceAsIMatching.OSPartialPaymentAmount = -50m;
			((MatchingValidation)invoice.Validation).ValidateOSPartialPaymentAmount();
			AssertNoError(invoiceAsIMatching.OSPartialPaymentAmountInfo, "When matching a ComPay payment, you can only pay an invoice up to a total of its lines that match the payment currency.");
		}

		public void TestCheckAH_TransactionTypeWithApproval()
		{
			APPaymentApprovalWithoutAuthorisation approval = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			approval.AV_PaymentType = ReceiptTypes.eNettDirectDebit;
			approval.AV_RX_NKPaymentCurrency = "AUD";
			approval.AV_Amount = 100m;

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			APCreditNote creditNote = Factory.NewWithValidTestData<APCreditNote>();
			creditNote.AH_OH = TestObjectCreator.AALSHI.PK;

			Factory.Save();

			IMatchingCollection collection = new IMatchingCollection(Factory) { invoice, approval, creditNote };
			AssertNotNull(invoice.PaymentApprovalCurrentlyBeingMatched);

			((MatchingValidation)creditNote.Validation).ValidateAH_TransactionType();
			AssertHasError(creditNote.AH_TransactionTypeInfo, "A ComPay payment can only be matched to AP invoices.");

			((MatchingValidation)invoice.Validation).ValidateAH_TransactionType();
			AssertNoError("Compay supports invoices", invoice.AH_TransactionTypeInfo, "A ComPay payment can only be matched to AP invoices.");
		}

		public void TestCheckAH_TransactionTypeWithPayment()
		{
			APPayment payment = TestObjectCreator.CreateAPPayment(1.0m, 10m, ZDateTime.Today, ZDateTime.Today, TestObjectCreator.ABIGAS.PK, TestObjectCreator.AUDBankAccount.PK);
			payment.AH_ReceiptType = ReceiptTypes.eNettDirectDebit;

			APInvoice invoice = Factory.NewWithValidTestData<APInvoice>();
			invoice.AH_OH = TestObjectCreator.AALSHI.PK;

			APCreditNote creditNote = Factory.NewWithValidTestData<APCreditNote>();
			creditNote.AH_OH = TestObjectCreator.AALSHI.PK;

			Factory.Save();

			IMatchingCollection collection = new IMatchingCollection(Factory) { invoice, payment, creditNote };
			AssertNotNull(invoice.PaymentCurrentlyBeingMatched);

			((MatchingValidation)creditNote.Validation).ValidateAH_TransactionType();
			AssertHasError(creditNote.AH_TransactionTypeInfo, "A ComPay payment can only be matched to AP invoices.");

			((MatchingValidation)invoice.Validation).ValidateAH_TransactionType();
			AssertNoError("Compay supports invoices", invoice.AH_TransactionTypeInfo, "A ComPay payment can only be matched to AP invoices.");

			((MatchingValidation)payment.Validation).ValidateAH_TransactionType();
			AssertNoError("if matched to APPayment should give error", payment.AH_TransactionTypeInfo, "A ComPay payment can only be matched to AP invoices.");

			//Make Payment sent to ENett
			var message = Factory.New<eNettEDIMessage>();
			message.EM_MessageType = "ENE";
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.eNett;
			message.EM_MessageSubType = eNettMessageSubTypeList.Codes.GetNewPayments;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			message.EM_Status = EDIMessage.Status.Sent;
			message.EM_LinkTable = AccTransactionHeaderSchema.Constants.TableName;
			message.EM_LinkUniqueID = payment.PK;

			Factory.Save();

			((MatchingValidation)creditNote.Validation).ValidateAH_TransactionType();
			AssertNoError(creditNote.AH_TransactionTypeInfo, "A ComPay payment can only be matched to AP invoices.");
		}

		public void TestValidateOSOutstandingAmountWhenAttachedToBatch()
		{
			OrgHeader org1 = TestObjectCreator.AALSHI;
			ARInvoice invoice1 = Factory.NewWithValidTestData<ARInvoice>();
			invoice1.AH_OH = org1.PK;
			invoice1.AH_InvoiceAmount = 100m;
			invoice1.AH_OutstandingAmount = 100m;
			Factory.Save();

			IMatchingCollection matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(invoice1);

			IMatching transactionAsMatching = invoice1;
			((MatchingValidation)invoice1.Validation).ValidateOSOutstandingAmount();
			AssertNoError(transactionAsMatching.OSOutstandingAmountInfo, "This transaction has been attached to an active collection batch, thus it can not be matched here.");

			var bank = Factory.NewWithValidTestData<AccBankAccount>();
			var batch = Factory.New<AccCollectionBatch>();
			batch.ACB_TotalAmount = 100m;
			batch.ACB_AB = bank.PK;
			batch.ACB_GC = GlbCompany.CurrentCompany.PK;
			batch.ACB_BatchNumber = "0001000";

			var order = Factory.New<AccCollectionOrder>();
			order.ACO_ACB = batch.PK;
			order.ACO_CollectionDate = ZDateTime.Today.Date;
			order.ACO_OH_Debtor = TestObjectCreator.AALSHI.PK;
			order.ACO_OrderNumber = "000001";
			order.IncludeInBatch = true;
			order.ACO_Amount = 100m;

			var orderline = Factory.New<AccCollectionOrderLine>();
			orderline.AOL_ACO = order.PK;
			orderline.AOL_AH = invoice1.PK;
			orderline.AOL_IsCancelled = false;
			orderline.IncludeInOrder = true;
			Factory.Save();
			Assert(invoice1.IsUsedByActiveCollectionOrderLine);

			((MatchingValidation)invoice1.Validation).ValidateOSOutstandingAmount();
			AssertHasError(transactionAsMatching.OSOutstandingAmountInfo, "This transaction has been attached to an active collection batch, thus it can not be matched here.");
		}

		[TestDate(2010, 07, 05)]
		public void TestValidateOSOutstandingAmount()
		{
			BusinessObjectFactory newFactory = new BusinessObjectFactory();
			OrgHeader org1 = TestObjectCreator.AALSHI;
			APInvoice aPInvoiceToTest = Factory.NewWithValidTestData<APInvoice>();
			aPInvoiceToTest.AH_OH = org1.PK;
			aPInvoiceToTest.AH_InvoiceAmount = -90M;
			aPInvoiceToTest.AH_OutstandingAmount = -90M;

			Factory.Save();

			AccBankAccount bankAccount = newFactory.NewWithValidTestData<AccBankAccount>();
			bankAccount.AB_Code = "TestBank";
			AccChequeBook checkBook = newFactory.NewWithValidTestData<AccChequeBook>();
			checkBook.AK_AB = bankAccount.PK;
			checkBook.AK_Code = "TestBook";

			ARPaymentApprovalWithAuthorisation payment1 = newFactory.NewWithValidTestData<ARPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 40M;
			payment1.AV_AB = bankAccount.PK;
			PaymentApprovalItem item1 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = 40M;
			item1.A2_AH = aPInvoiceToTest.PK;
			item1.A2_AV = payment1.PK;

			ARPaymentApprovalWithAuthorisation payment2 = newFactory.NewWithValidTestData<ARPaymentApprovalWithAuthorisation>();
			payment2.AV_Amount = 50M;
			payment2.AV_AB = bankAccount.PK;
			payment2.AV_AK = checkBook.PK;
			payment2.AV_ChequeOrReference = "Test223";
			PaymentApprovalItem item2 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item2.A2_PaymentThisRun = 50M;
			item2.A2_AH = aPInvoiceToTest.PK;
			item2.A2_AV = payment2.PK;

			newFactory.Save();

			IMatchingCollection matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(aPInvoiceToTest);

			IMatching transactionAsMatching = aPInvoiceToTest;

			((MatchingValidation)aPInvoiceToTest.Validation).ValidateOSOutstandingAmount();
			Assert(transactionAsMatching.OSOutstandingAmountInfo.GetErrors().ContainsNotificationContaining("This transaction is fully paid by the following Unapproved payment(s):"));
			Assert(transactionAsMatching.OSOutstandingAmountInfo.GetErrors().ContainsNotificationContaining("Pay. Date   Bank Account   Check Book   Check/Reference   Amount"));
			Assert(transactionAsMatching.OSOutstandingAmountInfo.GetErrors().ContainsNotificationContaining("05-Jul-10  TestBank          TestBook       Test223     50.00 AUD"));
			Assert(transactionAsMatching.OSOutstandingAmountInfo.GetErrors().ContainsNotificationContaining("05-Jul-10  TestBank               N/A                N/A          40.00 AUD"));
		}

		public void TestValidateOSPartialPaymentAmount()
		{
			OrgHeader org1 = TestObjectCreator.AALSHI;
			APInvoice aPInvoiceToTest = Factory.NewWithValidTestData<APInvoice>();
			aPInvoiceToTest.AH_OH = org1.PK;
			aPInvoiceToTest.AH_OutstandingAmount = 90M;

			IMatchingCollection matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(aPInvoiceToTest);

			IMatching transactionAsMatching = aPInvoiceToTest;

			transactionAsMatching.OSPartialPaymentAmount = 50M;

			Assert("Validation should be Matching Validation", aPInvoiceToTest.Validation.GetType().IsAssignableFrom(typeof(MatchingValidation)));

			((MatchingValidation)aPInvoiceToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(!transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between 0 and 90.00"));

			transactionAsMatching.OSPartialPaymentAmount = 120M;

			((MatchingValidation)aPInvoiceToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between 0 and 90.00"));

			transactionAsMatching.OSPartialPaymentAmount = -120M;

			((MatchingValidation)aPInvoiceToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between 0 and 90.00"));

			aPInvoiceToTest.AH_OutstandingAmount = -90M;

			transactionAsMatching.OSPartialPaymentAmount = -50M;

			((MatchingValidation)aPInvoiceToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(!transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between -90.00 and 0"));

			transactionAsMatching.OSPartialPaymentAmount = -120M;

			((MatchingValidation)aPInvoiceToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between -90.00 and 0"));

			transactionAsMatching.OSPartialPaymentAmount = 120M;

			((MatchingValidation)aPInvoiceToTest.Validation).ValidateOSPartialPaymentAmount();
			Assert(transactionAsMatching.OSPartialPaymentAmountInfo.HasError("Pay Amount must be between -90.00 and 0"));

			((ISupportMatchingOfMyLines)aPInvoiceToTest).LineTotalPaidAmount = 42m;
			((MatchingValidation)aPInvoiceToTest.Validation).ValidateOSPartialPaymentAmount();
			AssertHasWarning(transactionAsMatching.OSPartialPaymentAmountInfo, "Pay amount was set on the by the line basis. To edit, right click and select 'Pay Lines'.");

			string expectedWarning = @"You are part paying this transaction without matching lines. 
You will not be able to match this transaction at the line level in the future if you proceed.
Alternatively, use the 'Match Transaction Lines' option by right clicking now to allow matching at the line level in the future.";
			transactionAsMatching.OSPartialPaymentAmount = 50M;
			AssertHasError(transactionAsMatching.OSPartialPaymentAmountInfo, "Pay Amount must be between -90.00 and 0");
			AssertNoWarning(transactionAsMatching.OSPartialPaymentAmountInfo, expectedWarning);

			((ISupportMatchingOfMyLines)aPInvoiceToTest).LineTotalPaidAmount = 0m;
			transactionAsMatching.OSPartialPaymentAmount = -50M;
			AssertNoError(transactionAsMatching.OSPartialPaymentAmountInfo, "Pay Amount must be between -90.00 and 0");
			AssertHasWarning(transactionAsMatching.OSPartialPaymentAmountInfo, expectedWarning);
		}

		public void TestValidateMatchStatus()
		{
			var org = TestObjectCreator.AALSHI;
			var aPInvoiceToTest = Factory.NewWithValidTestData<APInvoice>();
			aPInvoiceToTest.AH_OH = org.PK;
			aPInvoiceToTest.AH_OutstandingAmount = 90M;

			var matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(aPInvoiceToTest);

			IMatching transactionAsMatching = aPInvoiceToTest;
			transactionAsMatching.MatchStatus = "XXX";

			Assert(!((ICodeDescriptionPairListProvider)AccountingConfigurationRegistry.Instance.MatchStatus).CodeDescriptionPairList.ContainsCode("XXX"));
			Assert("Validation should be Matching Validation", aPInvoiceToTest.Validation.GetType().IsAssignableFrom(typeof(MatchingValidation)));

			((MatchingValidation)aPInvoiceToTest.Validation).ValidateMatchStatus();
			AssertHasError(transactionAsMatching.MatchStatusInfo, "Enter a valid Match Status Code.");
		}

		public void TestValidateMatchStatusReasonCode()
		{
			var org = TestObjectCreator.AALSHI;
			var aPInvoiceToTest = Factory.NewWithValidTestData<APInvoice>();
			aPInvoiceToTest.AH_OH = org.PK;
			aPInvoiceToTest.AH_OutstandingAmount = 90M;

			var matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(aPInvoiceToTest);

			IMatching transactionAsMatching = aPInvoiceToTest;
			transactionAsMatching.MatchStatusReasonCode = "XXX";

			Assert(!((ICodeDescriptionPairListProvider)AccountingConfigurationRegistry.Instance.MatchStatusReason).CodeDescriptionPairList.ContainsCode("XXX"));
			Assert("Validation should be Matching Validation", aPInvoiceToTest.Validation.GetType().IsAssignableFrom(typeof(MatchingValidation)));

			using (aPInvoiceToTest.SuspendValidationTesting())
			{
				var validator = (MatchingValidation)aPInvoiceToTest.Validation;
				var matchStatusReasonCodeInfo = transactionAsMatching.MatchStatusReasonCodeInfo;

				matchStatusReasonCodeInfo.ClearAllNotifications();
				validator.ValidateMatchStatusReasonCode();
				AssertHasError(matchStatusReasonCodeInfo, "Enter a valid Match Status Reason Code.");

				matchStatusReasonCodeInfo.ClearAllNotifications();
				transactionAsMatching.MatchStatus = "UAC";
				transactionAsMatching.MatchStatusReasonCode = ZString.Empty;
				validator.ValidateMatchStatusReasonCode();
				AssertHasError(matchStatusReasonCodeInfo, "Please enter a value.");

				Assert(((ICodeDescriptionPairListProvider)AccountingConfigurationRegistry.Instance.MatchStatusReason).CodeDescriptionPairList.ContainsCode("ADV"));

				matchStatusReasonCodeInfo.ClearAllNotifications();
				transactionAsMatching.MatchStatus = ZString.Empty;
				transactionAsMatching.MatchStatusReasonCode = "ADV";
				validator.ValidateMatchStatusReasonCode();
				AssertHasError(matchStatusReasonCodeInfo, "If a match status is not specified, a reason must not be specified.");
			}
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1103:DoNotUseStringLiteralsForDateFormats", Justification = "Testing")]
		public void TestDueDateAndInvoiceDateAndPostDateShouldNotBeRangeChecked()
		{
			OrgHeader org1 = TestObjectCreator.AALSHI;
			APInvoice aPInvoiceToTest = Factory.NewWithValidTestData<APInvoice>();
			aPInvoiceToTest.AH_OH = org1.PK;

			ZDateTime testDate = ZDateTime.Today.AddYears(-11);
			aPInvoiceToTest.AH_DueDate = testDate;
			aPInvoiceToTest.AH_InvoiceDate = testDate;
			aPInvoiceToTest.AH_PostDate = testDate;

			string errorMessage = string.Format("The date '{0}' is more than 10 years old and thus is not valid.", testDate.ToString("dd-MMM-yyyy"));
			Assert("There should be error", aPInvoiceToTest.AH_DueDateInfo.HasError(errorMessage));
			Assert("There should be error", aPInvoiceToTest.AH_InvoiceDateInfo.HasError(errorMessage));
			Assert("There should be error", aPInvoiceToTest.AH_PostDateInfo.HasError(errorMessage));

			IMatchingCollection matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(aPInvoiceToTest);
			Assert("Validation should be Matching Validation", aPInvoiceToTest.Validation.GetType().IsAssignableFrom(typeof(MatchingValidation)));

			testDate = ZDateTime.Today.AddYears(-12);
			aPInvoiceToTest.AH_DueDate = testDate;
			aPInvoiceToTest.AH_InvoiceDate = testDate;
			aPInvoiceToTest.AH_PostDate = testDate;

			Assert("There should be no error", !aPInvoiceToTest.AH_DueDateInfo.HasErrors());
			Assert("There should be no error", !aPInvoiceToTest.AH_InvoiceDateInfo.HasErrors());
			Assert("There should be no error", !aPInvoiceToTest.AH_PostDateInfo.HasErrors());
		}

		[TestDate(2010, 07, 05)]
		public void TestValidateOSPartialPaymentAmountAddWarningForUnpostedPaymentOutstandingAmount()
		{
			OrgHeader organisation = TestObjectCreator.AALSHI;
			APInvoice invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("Inv001", TestObjectCreator.USD, 2M, 90M, 0M, 0M, 45M, 0M, 0M, organisation);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			APPaymentApprovalWithAuthorisation payment1 = newFactory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 40M;
			payment1.AV_RX_NKPaymentCurrency = TestObjectCreator.GBP.RX_Code;
			TestObjectCreator.GBP.RX_SubUnitRatio = 1;
			payment1.AV_PayExRate = 2M;
			payment1.AV_PaymentDate = ZDateTime.BrettsBirthday;
			PaymentApprovalItem item1 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = -10M;
			item1.A2_AH = invoice.PK;
			item1.A2_AV = payment1.PK;

			APPaymentApprovalWithoutAuthorisation payment2 = newFactory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			payment2.AV_Amount = 50M;
			payment2.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			payment1.AV_PayExRate = 2M;
			payment2.AV_ChequeOrReference = "Test223";
			payment2.AV_PaymentDate = ZDateTime.Now;
			PaymentApprovalItem item2 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item2.A2_PaymentThisRun = -20M;
			item2.A2_AH = invoice.PK;
			item2.A2_AV = payment2.PK;

			newFactory.Save();

			APPaymentApprovalWithoutAuthorisation unSavedPayment = Factory.NewWithValidTestData<APPaymentApprovalWithoutAuthorisation>();
			unSavedPayment.AV_Amount = 30M;
			unSavedPayment.AV_RX_NKPaymentCurrency = TestObjectCreator.USD.RX_Code;
			unSavedPayment.AV_PayExRate = 2M;
			unSavedPayment.AV_PaymentDate = ZDateTime.Now;
			PaymentApprovalItem unsavedItem = Factory.NewWithValidTestData<PaymentApprovalItem>();
			unsavedItem.A2_PaymentThisRun = -15M;
			unsavedItem.A2_AH = invoice.PK;
			unsavedItem.A2_AV = unSavedPayment.PK;

			AssertEquals("Precondition: invoice.ExistingPaymentApprovalItems should include unsaved payment.", 3, invoice.ExistingPaymentApprovalItems.Count);

			IMatchingCollection matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(invoice);

			IMatching transactionAsMatching = invoice;
			transactionAsMatching.OSPartialPaymentAmount = transactionAsMatching.OSOutstandingAmount;

			((MatchingValidation)invoice.Validation).ValidateOSPartialPaymentAmount();

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"This transaction has an outstanding amount of AUD -45.00. There are 2 unposted Payments matched to this transaction to the value of AUD -30.00, leaving AUD -15.00 available to match in this session. This can be found in the relevant Ledger's 'Payment Processing' Module. The details of the unposted payments are:");

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 05-Jul-10
- Check / Reference: Test223
- Matched Amount: AUD -20.00
- Payment Amount: USD 50.00");

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 18-Sep-71
- Check / Reference: 
- Matched Amount: AUD -10.00
- Payment Amount: GBP 40");
		}

		[TestDate(2010, 07, 05)]
		public void TestValidateOSPartialPaymentAmountAddWarningForUnpostedPaymentOutstandingAmount_OnePayment()
		{
			OrgHeader organisation = TestObjectCreator.AALSHI;
			APInvoice invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("Inv001", TestObjectCreator.USD, 2M, 90M, 0M, 0M, 45M, 0M, 0M, organisation);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			APPaymentApprovalWithAuthorisation payment1 = newFactory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 40M;
			payment1.AV_RX_NKPaymentCurrency = TestObjectCreator.GBP.RX_Code;
			payment1.AV_PayExRate = 2M;
			payment1.AV_PaymentDate = ZDateTime.BrettsBirthday;
			PaymentApprovalItem item1 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = -10M;
			item1.A2_AH = invoice.PK;
			item1.A2_AV = payment1.PK;

			newFactory.Save();

			IMatchingCollection matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(invoice);

			IMatching transactionAsMatching = invoice;
			transactionAsMatching.OSPartialPaymentAmount = transactionAsMatching.OSOutstandingAmount;

			string expectedWarningText =
@"This transaction has an outstanding amount of AUD -45.00. There is an unposted Payment matched to this transaction to the value of AUD -10.00, leaving AUD -35.00 available to match in this session. This can be found in the relevant Ledger's 'Payment Processing' Module. The details of the unposted payment are:
- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 18-Sep-71
- Check / Reference: 
- Matched Amount: AUD -10.00
- Payment Amount: GBP 40.00
";
			((MatchingValidation)invoice.Validation).ValidateOSPartialPaymentAmount();
			AssertHasWarning(transactionAsMatching.OSPartialPaymentAmountInfo, expectedWarningText);

			GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 1;

			expectedWarningText =
@"This transaction has an outstanding amount of AUD -45. There is an unposted Payment matched to this transaction to the value of AUD -10, leaving AUD -35 available to match in this session. This can be found in the relevant Ledger's 'Payment Processing' Module. The details of the unposted payment are:
- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 18-Sep-71
- Check / Reference: 
- Matched Amount: AUD -10
- Payment Amount: GBP 40.00
";
			((MatchingValidation)invoice.Validation).ValidateOSPartialPaymentAmount();
			AssertHasWarning(transactionAsMatching.OSPartialPaymentAmountInfo, expectedWarningText);
		}

		[TestDate(2010, 07, 05)]
		public void TestValidateOSPartialPaymentAmountAddWarningForUnpostedPaymentOutstandingAmount_FourPayments()
		{
			OrgHeader organisation = TestObjectCreator.AALSHI;
			APInvoice invoice = TestObjectCreator.CreateAPInvoice<APInvoice>("Inv001", TestObjectCreator.USD, 2M, 90M, 0M, 0M, 45M, 0M, 0M, organisation);

			Factory.Save();

			BusinessObjectFactory newFactory = new BusinessObjectFactory();

			APPaymentApprovalWithAuthorisation payment1 = newFactory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 40M;
			payment1.AV_RX_NKPaymentCurrency = TestObjectCreator.GBP.RX_Code;
			payment1.AV_PayExRate = 2M;
			payment1.AV_PaymentDate = ZDateTime.BrettsBirthday;
			PaymentApprovalItem item1 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = -10M;
			item1.A2_AH = invoice.PK;
			item1.A2_AV = payment1.PK;

			payment1 = newFactory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 40M;
			payment1.AV_RX_NKPaymentCurrency = TestObjectCreator.GBP.RX_Code;
			payment1.AV_PayExRate = 2M;
			payment1.AV_PaymentDate = ZDateTime.BrettsBirthday;
			item1 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = -10M;
			item1.A2_AH = invoice.PK;
			item1.A2_AV = payment1.PK;

			payment1 = newFactory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 40M;
			payment1.AV_RX_NKPaymentCurrency = TestObjectCreator.GBP.RX_Code;
			payment1.AV_PayExRate = 2M;
			payment1.AV_PaymentDate = ZDateTime.BrettsBirthday;
			item1 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = -10M;
			item1.A2_AH = invoice.PK;
			item1.A2_AV = payment1.PK;

			payment1 = newFactory.NewWithValidTestData<APPaymentApprovalWithAuthorisation>();
			payment1.AV_Amount = 40M;
			payment1.AV_RX_NKPaymentCurrency = TestObjectCreator.GBP.RX_Code;
			payment1.AV_PayExRate = 2M;
			payment1.AV_PaymentDate = ZDateTime.BrettsBirthday;
			item1 = newFactory.NewWithValidTestData<PaymentApprovalItem>();
			item1.A2_PaymentThisRun = -10M;
			item1.A2_AH = invoice.PK;
			item1.A2_AV = payment1.PK;

			newFactory.Save();

			IMatchingCollection matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(invoice);

			IMatching transactionAsMatching = invoice;
			transactionAsMatching.OSPartialPaymentAmount = transactionAsMatching.OSOutstandingAmount;
			((MatchingValidation)invoice.Validation).ValidateOSPartialPaymentAmount();

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"This transaction has an outstanding amount of AUD -45.00. There are 4 unposted Payments matched to this transaction to the value of AUD -40.00, leaving AUD -5.00 available to match in this session. This can be found in the relevant Ledger's 'Payment Processing' Module. The details of the unposted payments are:");

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 18-Sep-71
- Check / Reference: 
- Matched Amount: AUD -10.00
- Payment Amount: GBP 40.00");
			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 18-Sep-71
- Check / Reference: 
- Matched Amount: AUD -10.00
- Payment Amount: GBP 40.00");

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 18-Sep-71
- Check / Reference: 
- Matched Amount: AUD -10.00
- Payment Amount: GBP 40.00");

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo, @"To see more unposted payments use Payment Processing module.");

			GlbCompany.CurrentCompany.LocalCurrency.RX_SubUnitRatio = 1;
			((MatchingValidation)invoice.Validation).ValidateOSPartialPaymentAmount();

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"This transaction has an outstanding amount of AUD -45. There are 4 unposted Payments matched to this transaction to the value of AUD -40, leaving AUD -5 available to match in this session. This can be found in the relevant Ledger's 'Payment Processing' Module. The details of the unposted payments are:");

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 18-Sep-71
- Check / Reference: 
- Matched Amount: AUD -10
- Payment Amount: GBP 40.00");

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 18-Sep-71
- Check / Reference: 
- Matched Amount: AUD -10
- Payment Amount: GBP 40.00");

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo,
@"- Ledger: AP
- Status: Fully Approved (APP)
- Payment Date: 18-Sep-71
- Check / Reference: 
- Matched Amount: AUD -10
- Payment Amount: GBP 40.00");

			AssertHasWarningContaining(transactionAsMatching.OSPartialPaymentAmountInfo, @"To see more unposted payments use Payment Processing module.");
		}

		public void TestValidateOSPartialPaymentAmountForClearingJournal()
		{
			AccountingConfigurationRegistry.Instance.ClearingJournalConfiguration.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, AccountingConstants.ClearingJournalConfigurationTypes.LineBranchPerHeader.Code);
			var invoice = TestObjectCreator.CreateInvoice(typeof(ARInvoice), "INV3", TestObjectCreator.USD, 0.5M);
			var line1 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 0.5M, 100M, TestObjectCreator.NonCurrentBranch.PK);
			var line2 = TestObjectCreator.CreateInvoiceLine(invoice, TestObjectCreator.USD, 0.5M, 200M, GlbBranch.CurrentBranch.PK);
			IMatchingCollection collection = new IMatchingCollection(Factory) { invoice };
			((IMatching)invoice).OSPartialPaymentAmount = 50M;
			AssertHasWarningContaining(((IMatching)invoice).OSPartialPaymentAmountInfo, "This transaction with different branches in lines is being partly paid without matching lines. ");
		}

		public void TestBranchDepartmentCombinationValidation_MatchingValidation()
		{
			InvoicingBase invoice = Factory.New<APInvoice>();
			invoice.AH_TransactionNum = "00001000";
			new IMatchingCollection(Factory) { invoice };
			InvoicingLineBase line = (InvoicingLineBase)invoice.Lines.AddNew();
			line.AL_AG = TestObjectCreator.GLHeader1.PK;
			Factory.Save();
			GlbBranchCombinationValidationTest.ValidateBranchDepartmentCombinationsForBizObjInDatabase(Factory, invoice,
				() => { invoice.Validation.ValidateAll(); }, invoice.AH_GEInfo);
		}

		public void TestValidateAll()
		{
			var arInvoice = Factory.NewWithValidTestData<ARInvoice>();
			arInvoice.AH_OH = TestObjectCreator.AALSHI.PK;
			arInvoice.AH_InvoiceAmount = 100m;
			arInvoice.AH_OutstandingAmount = 100m;
			arInvoice.AH_OSTotal = 100m;

			var matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(arInvoice);

			var invoiceAsIMatching = arInvoice as IMatching;
			invoiceAsIMatching.OSPartialPaymentAmount = 100m;
			arInvoice.AH_OutstandingAmount = 90m;

			var arInvoiceValidation = arInvoice.Validation as MatchingValidation;

			AssertNoError("Not validated yet, no error", invoiceAsIMatching.OSPartialPaymentAmountInfo, "Pay Amount must be between 0 and 90.00");
			AssertNoError("Not validated yet, no error", invoiceAsIMatching.OSOutstandingAmountInfo, "This transaction has been attached to an active collection batch, thus it can not be matched here.");

			arInvoiceValidation.ValidateAll();
			AssertHasError("Pay > outstanding amount, error", invoiceAsIMatching.OSPartialPaymentAmountInfo, "Pay Amount must be between 0 and 90.00");
			AssertEquals("not Used By Active Collection Order Line", false, arInvoiceValidation.Parent_ForTestOnly.IsUsedByActiveCollectionOrderLine);
			AssertNoError("not Used By Active Collection Order Line, no error", invoiceAsIMatching.OSOutstandingAmountInfo, "This transaction has been attached to an active collection batch, thus it can not be matched here.");

			var batch = TestObjectCreator.CreateCollectionBatch(TestObjectCreator.AUDBankAccount, GlbCompany.CurrentCompany, "0001000", 100m, false);
			var order = TestObjectCreator.CreateCollectionOrder(batch, ZDateTime.Today.Date, TestObjectCreator.AALSHI, "000001", 100m, false);
			order.IncludeInBatch = true;
			var orderLine = TestObjectCreator.CreateCollectionOrderLine(order, arInvoice, false);
			orderLine.IncludeInOrder = true;

			ClearNotifications(arInvoice);
			arInvoiceValidation.ValidateAll();
			AssertHasError("Pay > outstanding amount, error", invoiceAsIMatching.OSPartialPaymentAmountInfo, "Pay Amount must be between 0 and 90.00");
			AssertEquals("Used By Active Collection Order Line", true, arInvoiceValidation.Parent_ForTestOnly.IsUsedByActiveCollectionOrderLine);
			AssertHasError("Used By Active Collection Order Line, error", invoiceAsIMatching.OSOutstandingAmountInfo, "This transaction has been attached to an active collection batch, thus it can not be matched here.");

			arInvoice.AH_OutstandingAmount = 0m;

			ClearNotifications(arInvoice);
			arInvoiceValidation.ValidateAll();
			AssertEquals("Before save to DB, if Outstanding amount is 0m, the validation will be passed", false, invoiceAsIMatching.OSPartialPaymentAmountInfo.HasNotifications());

			AssertEquals("Not Saved to DB yet", false, arInvoiceValidation.Parent_ForTestOnly.IsInDatabase);
			AssertEquals("Is Readonly", true, arInvoiceValidation.Parent_ForTestOnly.IsTransactionInDatabaseReadOnly);
			arInvoice.AH_OutstandingAmount = 100m;
			Factory.Save();
			AssertEquals("Already Saved to DB", true, arInvoiceValidation.Parent_ForTestOnly.IsInDatabase);
			AssertEquals("Is Readonly", true, arInvoiceValidation.Parent_ForTestOnly.IsTransactionInDatabaseReadOnly);

			arInvoice.AH_OutstandingAmount = 90m;

			ClearNotifications(arInvoice);
			arInvoiceValidation.ValidateAll();
			AssertHasError("Pay > outstanding amount, error", invoiceAsIMatching.OSPartialPaymentAmountInfo, "Pay Amount must be between 0 and 90.00");
			AssertHasError("Used By Active Collection Order Line, error", invoiceAsIMatching.OSOutstandingAmountInfo, "This transaction has been attached to an active collection batch, thus it can not be matched here.");

			arInvoice.AH_OutstandingAmount = 0m;
			ClearNotifications(arInvoice);
			arInvoiceValidation.ValidateAll();
			AssertEquals("After save to DB, if Outstanding amount is 0m, the validation will be passed", false, invoiceAsIMatching.OSPartialPaymentAmountInfo.HasNotifications());
		}

		void ClearNotifications(ARInvoice arInvoice)
		{
			var invoiceAsIMatching = arInvoice as IMatching;
			using (arInvoice.SuspendValidationTesting())
			{
				invoiceAsIMatching.OSPartialPaymentAmountInfo.ClearAllNotifications();
				invoiceAsIMatching.OSOutstandingAmountInfo.ClearAllNotifications();
			}
			AssertEquals(false, invoiceAsIMatching.OSPartialPaymentAmountInfo.HasNotifications());
			AssertEquals(false, invoiceAsIMatching.OSOutstandingAmountInfo.HasNotifications());
		}

		public void TestValidateAH_InvoicePaymentReferenceCode()
		{
			var configurationCollection = InvoiceRemittanceConfigurationCollectionTest.GetConfigurationCollectionForTest(Factory);
			AccountingMasterFilesRegistry.Instance.InvoiceRemittanceConfiguration.SetValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, configurationCollection);
			Factory.Save();

			var aRInvoiceToTest = Factory.NewWithValidTestData<ARInvoice>();
			aRInvoiceToTest.AH_OH = TestObjectCreator.AALSHI.PK;
			aRInvoiceToTest.AH_TransactionNum = "ARINV001";
			aRInvoiceToTest.AH_InvoicePaymentReferenceCode = "AAA";

			var matchingCollection = new IMatchingCollection(Factory);
			matchingCollection.Add(aRInvoiceToTest);

			IMatching transactionAsMatching = aRInvoiceToTest;
			transactionAsMatching.MatchStatus = "XXX";

			((MatchingValidation)aRInvoiceToTest.Validation).ValidateAH_InvoicePaymentReferenceCode();
			AssertEquals(aRInvoiceToTest.AH_InvoicePaymentReferenceCodeInfo.HasErrors(), false);

			aRInvoiceToTest.AH_InvoicePaymentReferenceCode = "CCC";

			((MatchingValidation)aRInvoiceToTest.Validation).ValidateAH_InvoicePaymentReferenceCode();
			AssertHasError(aRInvoiceToTest.AH_InvoicePaymentReferenceCodeInfo, "The invoice remittance type 'CCC' recorded against transaction number(s) ARINV001 is invalid.\r\nPlease check the invoice remittance configuration under registry 'Accounting > Receivables Defaults > Default Settings > Invoice Remittance Configuration' and update the invoice remittance type via Receivables Transactions > Actions > Override Invoice Remittance Type before matching.");
		}

		#region Implementation

		TestObjectCreator fTestObjectCreator;
		protected TestObjectCreator TestObjectCreator
		{
			get { return fTestObjectCreator ?? (fTestObjectCreator = new TestObjectCreator(Factory)); }
		}

		#endregion
	}
}
