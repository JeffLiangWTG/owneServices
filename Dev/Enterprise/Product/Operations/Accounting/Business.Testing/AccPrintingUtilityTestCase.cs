using System;
using System.Linq;
using CargoWise.Application;
using CargoWise.Data;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP;
using Enterprise.Accounting.Business.ARAP.HotCheque;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.ARAP.Journal;
using Enterprise.Accounting.Business.ARAP.ReceiptPayment;
using Enterprise.Accounting.Business.CashBook.DepositBatch;
using Enterprise.Accounting.Business.CashBook.DirectPayment;
using Enterprise.Accounting.Business.CashBook.DirectReceipt;
using Enterprise.Accounting.Business.CashBook.Transfer;
using Enterprise.Accounting.Business.GeneralLedger.GLJournals;
using Enterprise.DocumentEngine;
using Enterprise.DocumentEngineCore.DocumentSupport;
using Enterprise.DocumentEngineCore.DocWrappers;
using Enterprise.DocumentEngineCore.DocWrappers.Testing;
using Enterprise.DocumentEngineCore.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using Moq.Protected;
using NUnit.Framework;
using Constants = Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.Testing
{
	public class AccPrintingUtilityTestCase : TestCaseWithFactory
	{
		public void TestCheckMenuItemExistsForChequeTemplate()
		{
			APPayment cheque1 = Factory.NewWithValidTestData<APPayment>();
			AccHotCheque cheque2 = Factory.NewWithValidTestData<AccHotCheque>();
			DirectPayment cheque3 = Factory.NewWithValidTestData<DirectPayment>();

			AssertEquals(true, AccPrintingUtility.CheckMenuItemExistsForChequeTemplate("Standard", cheque1));
			AssertEquals(true, AccPrintingUtility.CheckMenuItemExistsForChequeTemplate("Standard", cheque2));
			AssertEquals(true, AccPrintingUtility.CheckMenuItemExistsForChequeTemplate("Standard", cheque3));

			StmTemplate clientChequeTemplate = Factory.NewWithValidTestData<StmTemplate>();
			clientChequeTemplate.SO_Name = "Client Cheque Template";
			clientChequeTemplate.SO_DataContext = "Cheques";
			clientChequeTemplate.SO_IsClientSpecific = true;
			Factory.Save();

			AssertEquals(false, AccPrintingUtility.CheckMenuItemExistsForChequeTemplate(clientChequeTemplate.SO_Name, cheque1));
			AssertEquals(false, AccPrintingUtility.CheckMenuItemExistsForChequeTemplate(clientChequeTemplate.SO_Name, cheque2));
			AssertEquals(false, AccPrintingUtility.CheckMenuItemExistsForChequeTemplate(clientChequeTemplate.SO_Name, cheque3));
		}

		public void TestPrintDocument_AllowMultipleCopiesControlsNumberOfCopiesReadOnly()
		{
			APPayment cheque = Factory.NewWithValidTestData<APPayment>();
			using (var utility = new AccPrintingUtility(Factory, Constants.DataContext.Cheques))
			{
				utility.PrintDocument(cheque, "Standard", AllowedDeliveryOptions.All, ZGuid.Empty, true);
				AssertEquals("Test_NumberOfCopiesIsReadOnly should NOT be read only", false, utility.Test_NumberOfCopiesIsReadOnly);
				utility.PrintDocument(cheque, "Standard", AllowedDeliveryOptions.All, ZGuid.Empty, false);
				AssertEquals("Test_NumberOfCopiesIsReadOnly should be read only", true, utility.Test_NumberOfCopiesIsReadOnly);
			}

			DepositBatch depositBatch = Factory.NewWithValidTestData<DepositBatch>();
			using (var utility = new AccPrintingUtility(Factory, Enterprise.Core.Constants.DataContext.DepositBatch))
			{
				utility.PrintDocuments(depositBatch, new string[] { "Deposit Slip Bank", "Deposit Slip Office" }, AllowedDeliveryOptions.All, true);
				AssertEquals("Test_NumberOfCopiesIsReadOnly should NOT be read only", false, utility.Test_NumberOfCopiesIsReadOnly);
				utility.PrintDocuments(depositBatch, new string[] { "Deposit Slip Bank", "Deposit Slip Office" }, AllowedDeliveryOptions.All, false);
				AssertEquals("Test_NumberOfCopiesIsReadOnly should NOT be read only", true, utility.Test_NumberOfCopiesIsReadOnly);
			}
		}

		[ExpectNoExceptions()]
		public void TestPrintDocumentWithChequeCanPopulatesRecipient()
		{
			APPayment cheque = Factory.NewWithValidTestData<APPayment>();
			OrgHeader org = Factory.NewWithValidTestData<OrgHeader>();
			cheque.AH_OH = org.PK;

			using (var utility = new AccPrintingUtility(Factory, Constants.DataContext.Cheques))
			{
				Assert("Precondition - Recipient PK should be empty", utility.DeliveryInstructionsPassedForPrintingHasEmptyRecipient());
				utility.PrintDocument(cheque, "Singapore", AllowedDeliveryOptions.All, ZGuid.Empty, true);
				Assert("Postcondition - Recipient PK should not be empty", !utility.DeliveryInstructionsPassedForPrintingHasEmptyRecipient());
			}
		}

		[ExpectNoExceptions()]
		public void TestInvoiceBatchPrinting()
		{
			APPayment aPPayment = Factory.NewWithValidTestData<APPayment>();
			APPayment aPPayment2 = Factory.NewWithValidTestData<APPayment>();
			using (var utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.TransactionHeader))
			{
				utility.AddPaymentDocumentToPack("Payment Voucher", aPPayment, true, true);
				utility.AddPaymentDocumentToPack("Remittance Advice", aPPayment, true, true);
				utility.AddPaymentDocumentToPack(aPPayment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Cheques, null), "USStandard", aPPayment, Constants.DataContext.Cheques);
				utility.AddPaymentDocumentToPack("Payment Voucher", aPPayment2, true, true);
				utility.AddPaymentDocumentToPack("Remittance Advice", aPPayment2, true, true);
				utility.AddPaymentDocumentToPack(aPPayment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Cheques, null), "USStandard", aPPayment2, Constants.DataContext.Cheques);
				AssertEquals("Should be 1 print task per each document type", 3, utility.PaymentDocumentPacks.Count);

				AssertNotNull(utility.PaymentDocumentPacks["Payment Voucher"]);
				AssertEquals(typeof(DocumentPrintSet), utility.PaymentDocumentPacks["Payment Voucher"].GetType());
				AssertEquals("DocumentPrintSet should contain 2 DocumentPacks", 2, ((DocumentPrintSet)utility.PaymentDocumentPacks["Payment Voucher"]).Count);

				AssertNotNull(utility.PaymentDocumentPacks["Remittance Advice"]);
				AssertEquals(typeof(DocumentPrintSet), utility.PaymentDocumentPacks["Remittance Advice"].GetType());
				AssertEquals("DocumentPrintSet should contain 2 DocumentPacks", 2, ((DocumentPrintSet)utility.PaymentDocumentPacks["Remittance Advice"]).Count);

				AssertNotNull(utility.PaymentDocumentPacks[nameof(Constants.DataContext.Cheques)]);
				AssertEquals(typeof(PrintTask), utility.PaymentDocumentPacks[nameof(Constants.DataContext.Cheques)].GetType());
				AssertEquals("PrintTask should contain 2 DocumentPacks", 2, utility.PaymentDocumentPacks[nameof(Constants.DataContext.Cheques)].Count);

				utility.PrintDocumentPacksForPaymentCollection(AllowedDeliveryOptions.All);
			}
		}

		[ExpectNoExceptions()]
		public void TestInvoiceBatchPrintingWithNonLegacyDocument()
		{
			APPayment aPPayment = Factory.NewWithValidTestData<APPayment>();
			APPayment aPPayment2 = Factory.NewWithValidTestData<APPayment>();
			using (var utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.TransactionHeader))
			{
				utility.AddPaymentDocumentToPack("Payment Voucher", aPPayment, false, true);
				utility.AddPaymentDocumentToPack("Remittance Advice", aPPayment, false, true);
				utility.AddPaymentDocumentToPack("Payment Voucher", aPPayment2, false, true);
				utility.AddPaymentDocumentToPack("Remittance Advice", aPPayment2, false, true);
				AssertEquals("Should be 1 print task per each document type", 2, utility.PaymentDocumentPacks.Count);

				AssertNotNull(utility.PaymentDocumentPacks["Payment Voucher"]);
				AssertEquals(typeof(DocumentPrintSet), utility.PaymentDocumentPacks["Payment Voucher"].GetType());
				AssertEquals("DocumentPrintSet should contain 2 DocumentPacks", 2, ((DocumentPrintSet)utility.PaymentDocumentPacks["Payment Voucher"]).Count);

				AssertNotNull(utility.PaymentDocumentPacks["Remittance Advice"]);
				AssertEquals(typeof(DocumentPrintSet), utility.PaymentDocumentPacks["Remittance Advice"].GetType());
				AssertEquals("DocumentPrintSet should contain 2 DocumentPacks", 2, ((DocumentPrintSet)utility.PaymentDocumentPacks["Remittance Advice"]).Count);

				utility.PrintDocumentPacksForPaymentCollection(AllowedDeliveryOptions.All);
			}
		}

		public void TestInvoiceBatchPrintingWithNonLegacyDocument_WhenMultipleMenuExistWithSameName()
		{
			var aPPayment = Factory.NewWithValidTestData<APPayment>();
			TestObjectCreator.CreateDuplicateMenuItem("Remittance Advice", aPPayment);
			TestObjectCreator.CreateDuplicateMenuItem("Payment Voucher", aPPayment);

			var glJournal = Factory.NewWithValidTestData<GLJournal>();
			TestObjectCreator.CreateDuplicateMenuItem("General Ledger Journal", glJournal);

			Factory.Save();

			using (var utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.TransactionHeader))
			{
				utility.AddPaymentDocumentToPack("Payment Voucher", aPPayment, false, true);
				utility.AddPaymentDocumentToPack("Remittance Advice", aPPayment, false, true);

				AssertNotNull(utility.PaymentDocumentPacks["Payment Voucher"]);
				AssertEquals(typeof(DocumentPrintSet), utility.PaymentDocumentPacks["Payment Voucher"].GetType());
				AssertEquals("DocumentPrintSet should contain 2 DocumentPacks", 1, ((DocumentPrintSet)utility.PaymentDocumentPacks["Payment Voucher"]).Count);

				AssertNotNull(utility.PaymentDocumentPacks["Remittance Advice"]);
				AssertEquals(typeof(DocumentPrintSet), utility.PaymentDocumentPacks["Remittance Advice"].GetType());
				AssertEquals("DocumentPrintSet should contain 2 DocumentPacks", 1, ((DocumentPrintSet)utility.PaymentDocumentPacks["Remittance Advice"]).Count);

				AssertNoExceptionThrown("Expect no exception even if there is duplicate menu", () => utility.PrintDocument(glJournal, "General Ledger Journal", AllowedDeliveryOptions.All));
			}
		}

		[ExpectNoExceptions()]
		public void TestPrintDocument_ValidTemplateName()
		{
			int menuItemCountBefore = GetTableCount(StmMenuItemSchema.Constants.TableName);
			int menuTemplatePivotCountBefore = GetTableCount(StmMenuTemplatePivotSchema.Constants.TableName);
			try
			{
				AccHotCheque cheque = Factory.NewWithValidTestData<AccHotCheque>();

				AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.Cheques);
				utility.PrintDocument(cheque, "Standard", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			}
			finally
			{
				AssertEquals("No MenuItem should be added", menuItemCountBefore, GetTableCount(StmMenuItemSchema.Constants.TableName));
				AssertEquals("No MenuTemplatePivot should be added", menuTemplatePivotCountBefore, GetTableCount(StmMenuTemplatePivotSchema.Constants.TableName));
			}
		}

		[ExpectException(typeof(ReportException))]
		public void TestPrintDocument_InvalidTemplateName()
		{
			int menuItemCountBefore = GetTableCount(StmMenuItemSchema.Constants.TableName);
			int menuTemplatePivotCountBefore = GetTableCount(StmMenuTemplatePivotSchema.Constants.TableName);
			try
			{
				AccHotCheque cheque = Factory.NewWithValidTestData<AccHotCheque>();

				AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.Cheques);
				utility.PrintDocument(cheque, "", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			}
			finally
			{
				AssertEquals("No MenuItem should be added", menuItemCountBefore, GetTableCount(StmMenuItemSchema.Constants.TableName));
				AssertEquals("No MenuTemplatePivot should be added", menuTemplatePivotCountBefore, GetTableCount(StmMenuTemplatePivotSchema.Constants.TableName));
			}
		}

		[ExpectNoExceptions()]
		public void TestPrintDocument_ValidDocumentCommand()
		{
			int menuItemCountBefore = GetTableCount(StmMenuItemSchema.Constants.TableName);
			int menuTemplatePivotCountBefore = GetTableCount(StmMenuTemplatePivotSchema.Constants.TableName);
			try
			{
				var cheque = Factory.NewWithValidTestData<AccHotCheque>();
				var docCommand = DocumentCommand.GetDocumentCommand(Factory, cheque, "Standard");
				AssertNotNull("DocCommand", docCommand);

				AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.Cheques);
				utility.PrintDocument(cheque, docCommand, AllowedDeliveryOptions.All, ZGuid.Empty, true);
			}
			finally
			{
				AssertEquals("No MenuItem should be added", menuItemCountBefore, GetTableCount(StmMenuItemSchema.Constants.TableName));
				AssertEquals("No MenuTemplatePivot should be added", menuTemplatePivotCountBefore, GetTableCount(StmMenuTemplatePivotSchema.Constants.TableName));
			}
		}

		[ExpectException(typeof(ReportException))]
		public void TestPrintDocument_NullDocumentCommand()
		{
			int menuItemCountBefore = GetTableCount(StmMenuItemSchema.Constants.TableName);
			int menuTemplatePivotCountBefore = GetTableCount(StmMenuTemplatePivotSchema.Constants.TableName);
			try
			{
				AccHotCheque cheque = Factory.NewWithValidTestData<AccHotCheque>();

				AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.Cheques);
				utility.PrintDocument(cheque, (DocumentCommand)null, AllowedDeliveryOptions.All, ZGuid.Empty, true);
			}
			finally
			{
				AssertEquals("No MenuItem should be added", menuItemCountBefore, GetTableCount(StmMenuItemSchema.Constants.TableName));
				AssertEquals("No MenuTemplatePivot should be added", menuTemplatePivotCountBefore, GetTableCount(StmMenuTemplatePivotSchema.Constants.TableName));
			}
		}

		public void TestPrintDocuments()
		{
			DocumentWrapperForTesting wrapper1 = new DocumentWrapperForTesting();
			DocumentWrapperForTesting wrapper2 = new DocumentWrapperForTesting();
			DocumentWrapperForTesting wrapper3 = new DocumentWrapperForTesting();

			DocumentWrapper[] wrappers = { wrapper1, wrapper2, wrapper3 };

			var utilityMock =
				new Mock<AccPrintingUtility>(Factory, Constants.DataContext.ARInvoice) { CallBase = true };
			AccPrintingUtility utility = utilityMock.Object;

			utilityMock
				.Protected()
				.Setup<DeliveryInstructionDestination>("RunPrintTask",
					ItExpr.IsAny<DocumentPack>(),
					ItExpr.IsNull<DocumentCommand>(),
					ItExpr.IsNull<DeliveryInstructions>(),
					AllowedDeliveryOptions.AllExceptPreview)
				.Returns(DeliveryInstructionDestination.DummyDestinationForTesting);
			AssertEquals("Should return whatever RunPrintTask returns", DeliveryInstructionDestination.DummyDestinationForTesting, utility.PrintDocuments(wrappers, "ARInvoice", AllowedDeliveryOptions.AllExceptPreview));
			utilityMock.Verify();
		}

		[ExpectNoExceptions()]
		public void TestDocumentCommandsForPaymentTypeTransactions()
		{
			BankTransferFromRow cBTransfer = Factory.New<BankTransferFromRow>();
			APPayment payment = Factory.New<APPayment>();
			DirectPayment directPayment = Factory.New<DirectPayment>();

			AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory);
			utility.PrintDocument(payment, "Remittance Advice", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(directPayment, "Remittance Advice", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(cBTransfer, "Remittance Advice", AllowedDeliveryOptions.All, ZGuid.Empty, true);

			utility.PrintDocument(payment, "Payment Voucher", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(directPayment, "Payment Voucher", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(cBTransfer, "Payment Voucher", AllowedDeliveryOptions.All, ZGuid.Empty, true);
		}

		[ExpectNoExceptions()]
		public void TestDocumentCommandsForRemittanceAdviceAndPaymentVoucherUnderDifferentDocBuilderRegistrySetting()
		{
			BankTransferFromRow cBTransfer = Factory.New<BankTransferFromRow>();
			APPayment payment = Factory.New<APPayment>();
			DirectPayment directPayment = Factory.New<DirectPayment>();

			AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory);
			DocumentsDataRegistry.Instance.UseNewDocBuilderRemittanceAdvice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			utility.PrintDocument(payment, "Remittance Advice", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(directPayment, "Remittance Advice", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(cBTransfer, "Remittance Advice", AllowedDeliveryOptions.All, ZGuid.Empty, true);

			DocumentsDataRegistry.Instance.UseNewDocBuilderPaymentVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			utility.PrintDocument(payment, "Payment Voucher", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(directPayment, "Payment Voucher", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(cBTransfer, "Payment Voucher", AllowedDeliveryOptions.All, ZGuid.Empty, true);

			DocumentsDataRegistry.Instance.UseNewDocBuilderRemittanceAdvice.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			utility.PrintDocument(payment, "Remittance Advice", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(directPayment, "Remittance Advice", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(cBTransfer, "Remittance Advice", AllowedDeliveryOptions.All, ZGuid.Empty, true);

			DocumentsDataRegistry.Instance.UseNewDocBuilderPaymentVoucher.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			utility.PrintDocument(payment, "Payment Voucher", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(directPayment, "Payment Voucher", AllowedDeliveryOptions.All, ZGuid.Empty, true);
			utility.PrintDocument(cBTransfer, "Payment Voucher", AllowedDeliveryOptions.All, ZGuid.Empty, true);
		}

		[ExpectNoExceptions()]
		public void TestDocWrappersForReceiptMatching()
		{
			//APs
			APAdjustmentNote aPAdjNote = Factory.New<APAdjustmentNote>();
			APCreditNote aPCreditNote = Factory.New<APCreditNote>();
			APContraRow aPContra = Factory.New<APContraRow>();
			APDiscount aPDiscount = Factory.New<APDiscount>();
			APExchangeDifference aPExchangeDiff = Factory.New<APExchangeDifference>();
			APInvoice aPInvoice = Factory.New<APInvoice>();
			APJournal aPJournal = Factory.New<APJournal>();
			APPayment aPPayment = Factory.New<APPayment>();
			APReceipt aPReceipt = Factory.New<APReceipt>();

			AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.TransactionHeader);

			utility.PrintDocument(aPAdjNote.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aPCreditNote.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aPContra.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aPDiscount.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aPExchangeDiff.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aPInvoice.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aPJournal.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aPPayment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aPReceipt.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);

			//ARs
			ARAdjustmentNote aRAdjNote = Factory.New<ARAdjustmentNote>();
			ARCreditNote aRCreditNote = Factory.New<ARCreditNote>();
			ARContraRow aRContra = Factory.New<ARContraRow>();
			ARDiscount aRDiscount = Factory.New<ARDiscount>();
			ARExchangeDifference aRExchangeDiff = Factory.New<ARExchangeDifference>();
			ARInvoice aRInvoice = Factory.New<ARInvoice>();
			ARJournal aRJournal = Factory.New<ARJournal>();
			ARPayment aRPayment = Factory.New<ARPayment>();
			ARReceipt aRReceipt = Factory.New<ARReceipt>();

			utility.PrintDocument(aRAdjNote.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aRCreditNote.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aRContra.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aRDiscount.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aRExchangeDiff.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aRInvoice.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aRJournal.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aRPayment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
			utility.PrintDocument(aRReceipt.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);

			//CBs
			DirectReceipt directReceipt = Factory.New<DirectReceipt>();
			utility.PrintDocument(directReceipt.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.TransactionHeader, null), "Receipt Matching", AllowedDeliveryOptions.All);
		}

		[ExpectNoExceptions]
		public void TestAutoPrintDocument()
		{
			BusinessObject stmPrintQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			APPayment aPPayment = Factory.New<APPayment>();
			ARPayment aRPayment = Factory.New<ARPayment>();

			Factory.Save();
			AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.Cheques);

			utility.PrintDocument(aPPayment, "USStandard", AllowedDeliveryOptions.All, stmPrintQueue.PK, false);
			utility.PrintDocument(aRPayment, "USStandard", AllowedDeliveryOptions.All, stmPrintQueue.PK, false);

			AssertNotEquals("Test_DeliveryInstructionsPassedForPrinting is not null", null, utility.Test_DeliveryInstructionsPassedForPrinting.FirstOrDefault());
			AssertEquals("Test_DeliveryInstructionsPassedForPrinting.Destination", DeliveryInstructionDestination.Print, utility.Test_DeliveryInstructionsPassedForPrinting.First().Destination);
			AssertEquals("Test_DeliveryInstructionsPassedForPrinting.PrintQueuePK", stmPrintQueue.PK, utility.Test_DeliveryInstructionsPassedForPrinting.First().PrinterDelivery.PrintQueuePK);
		}

		public void TestAutoChequePrintingOverriddenAddress()
		{
			var stmPrintQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());

			var orgHeader = TestObjectCreator.CreateOrgHeader("BORG", true, true);

			var apAddress = TestObjectCreator.CreateAddress(orgHeader, "APM main office");
			apAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);

			var arAddress = TestObjectCreator.CreateAddress(orgHeader, "ARM main office");
			arAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);

			ARPayment payment = Factory.New<ARPayment>();
			payment.AH_OH = orgHeader.PK;
			payment.AH_OA_InvoiceAddressOverride = arAddress.PK;

			Factory.Save();
			AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.Cheques);
			utility.PrintDocument(payment, "USStandard", AllowedDeliveryOptions.All, stmPrintQueue.PK, false);

			AssertNotNull("Test_DeliveryInstructionsPassedForPrinting is not null", utility.Test_DeliveryInstructionsPassedForPrinting.FirstOrDefault());

			AssertEquals(1, utility.Test_DeliveryInstructionsPassedForPrinting.First().Recipients.Count);
			AssertEquals("ARM main office", utility.Test_DeliveryInstructionsPassedForPrinting.First().Recipients[0].Address1);
		}

		public void TestAutoChequePrintingOverriddenAddress_ForPaymentCollection()
		{
			var stmPrintQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());

			var orgHeader = TestObjectCreator.CreateOrgHeader("BORG", true, true);

			var apAddress = TestObjectCreator.CreateAddress(orgHeader, "APM main office");
			apAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);

			var arAddress = TestObjectCreator.CreateAddress(orgHeader, "ARM main office");
			arAddress.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Receivables);

			ARPayment payment = Factory.New<ARPayment>();
			payment.AH_OH = orgHeader.PK;
			payment.AH_OA_InvoiceAddressOverride = arAddress.PK;

			Factory.Save();
			AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.Cheques);
			utility.AddPaymentDocumentToPack(payment.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Cheques, null), "USStandard", payment, Constants.DataContext.Cheques);
			utility.AutoPrintDocumentPacksForPaymentCollection(stmPrintQueue.PK);

			AssertNotNull("Test_DeliveryInstructionsPassedForPrinting is not null", utility.Test_DeliveryInstructionsPassedForPrinting.FirstOrDefault());

			AssertEquals(1, utility.Test_DeliveryInstructionsPassedForPrinting.First().Recipients.Count);
			AssertEquals("ARM main office", utility.Test_DeliveryInstructionsPassedForPrinting.First().Recipients[0].Address1);
		}

		public void TestAutoChequePrintingCorrectAddress_ForPaymentCollectionWithDifferentOrg()
		{
			var stmPrintQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());

			var orgHeader1 = TestObjectCreator.CreateOrgHeader("BORG", true, true);
			var orgHeader2 = TestObjectCreator.CreateOrgHeader("CORP", true, true);

			var apAddress1 = TestObjectCreator.CreateAddress(orgHeader1, "BORG APM main office");
			apAddress1.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);

			var apAddress2 = TestObjectCreator.CreateAddress(orgHeader2, "CORP APM main office");
			apAddress2.AddressCapability.SetCapabilityEnabled(OrgConstants.AddressType.Payables);

			APPayment payment1 = Factory.New<APPayment>();
			payment1.AH_OH = orgHeader1.PK;

			APPayment payment2 = Factory.New<APPayment>();
			payment2.AH_OH = orgHeader2.PK;

			Factory.Save();
			AccPrintingUtilitySubClassForTesting utility = new AccPrintingUtilitySubClassForTesting(Factory, Constants.DataContext.Cheques);
			utility.AddPaymentDocumentToPack(payment1.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Cheques, null), "USStandard", payment1, Constants.DataContext.Cheques);
			utility.AddPaymentDocumentToPack(payment2.DocumentSupporter.GetDocumentWrappers(Constants.DataContext.Cheques, null), "USStandard", payment2, Constants.DataContext.Cheques);
			utility.AutoPrintDocumentPacksForPaymentCollection(stmPrintQueue.PK);

			AssertEquals("2 instructions should be created", 2, utility.Test_DeliveryInstructionsPassedForPrinting.Count);

			AssertEquals(1, utility.Test_DeliveryInstructionsPassedForPrinting[0].Recipients.Count);
			AssertEquals("BORG APM main office", utility.Test_DeliveryInstructionsPassedForPrinting[0].Recipients[0].Address1);

			AssertEquals(1, utility.Test_DeliveryInstructionsPassedForPrinting[1].Recipients.Count);
			AssertEquals("CORP APM main office", utility.Test_DeliveryInstructionsPassedForPrinting[1].Recipients[0].Address1);
		}

		public void TestShouldNotThrownZCannotSaveExceptionWhenNotAutoPrintCheques()
		{
			var cheque = Factory.NewWithValidTestData<AccHotCheque>();
			var stmPrintQueue = Factory.NewWithValidTestData(ObjectFactory.GetType<Enterprise.Integration.DocumentEngine.IStmPrintQueue>());
			Factory.Save();

			var utility = new AccPrintingUtility(Factory, Constants.DataContext.Cheques);

			using (ObjectFactory.Substitute<IExceptionWhenRenderAndSave>(new ExceptionToThrow(new InvalidOperationException("Object is currently in use elsewhere. Secret Code X$%#XX"))))
			{
				AssertNoExceptionThrown(() => utility.PrintDocument(cheque, "Standard", AllowedDeliveryOptions.All, ZGuid.Empty, false));
				AssertExceptionThrown(typeof(ZCannotSaveException), () => utility.PrintDocument(cheque, "Standard", AllowedDeliveryOptions.All, stmPrintQueue.PK, false));
			}
		}

		#region Inner Class

		class ExceptionToThrow : IExceptionWhenRenderAndSave
		{
			readonly Exception ex;
			public ExceptionToThrow(Exception ex)
			{
				this.ex = ex;
			}

			public void Throw()
			{
				throw ex;
			}
		}

		#endregion

		#region Implementation

		int GetTableCount(string table)
		{
			DbCommand cmd = Db.Connection.Command(string.Format("SELECT COUNT(*) FROM {0}", table));
			return (int)cmd.ExecuteScalar();
		}

		TestObjectCreator TestObjectCreator
		{
			get { return testObjectCreator ?? (testObjectCreator = new TestObjectCreator(Factory)); }
		}
		TestObjectCreator testObjectCreator;

		#endregion
	}
}
