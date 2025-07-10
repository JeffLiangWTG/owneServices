using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Accounting.Business;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Netting;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Customs.Common;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.Core.Testing;
using Enterprise.UniversalDataBuss.DataObjects;
using Enterprise.UniversalDataBuss.DataObjects.Accounting;
using Enterprise.UniversalDataBuss.DataObjects.Core;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Integration;
using Enterprise.UniversalDataBuss.Management.Testing;
using Enterprise.UniversalDataBuss.ServiceTasks;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;
using ServiceManager.Integration.ServiceTasks.CW.Test;

namespace Enterprise.Accounting.DataTransfer.Universal.Netting.Testing
{
	[MasterFiles.Integration.Test.MatchAgainstOnlineFlightsInUnitTest]
	public class NettingTransactionImporterTest : TestCaseWithFactory
	{
		[TestDate(2016, 03, 01)]
		public void TestNettingPeriod()
		{
			var firstPeriod = period;
			AssertNotNull("firstPeriod already setsup", firstPeriod);

			var secondPeriod = SetupNextNettingPeriod(ns, firstPeriod, "BBB");
			AssertNotNull("secondPeriod also setup", secondPeriod);
			Assert(firstPeriod.NSP_NettingExecutionDateUtc < secondPeriod.NSP_NettingExecutionDateUtc);
			Assert(firstPeriod.NSP_LatestUploadDateUtc < secondPeriod.NSP_LatestUploadDateUtc);
			Assert(firstPeriod.NSP_LatestApprovalDateUtc < secondPeriod.NSP_LatestApprovalDateUtc);

			var thirdPeriod = SetupNextNettingPeriod(ns, secondPeriod, "CCC");
			AssertNotNull("thirdPeriod also setup", thirdPeriod);
			Assert(secondPeriod.NSP_NettingExecutionDateUtc < thirdPeriod.NSP_NettingExecutionDateUtc);
			Assert(secondPeriod.NSP_LatestUploadDateUtc < thirdPeriod.NSP_LatestUploadDateUtc);
			Assert(secondPeriod.NSP_LatestApprovalDateUtc < thirdPeriod.NSP_LatestApprovalDateUtc);

			Factory.Save();

			var transactionDueDate = ZDateTime.Today.AddDays(-30); //transaction due date is before any valid netting cycle
			Assert("Precondition", transactionDueDate < firstPeriod.NSP_EarliestInvoiceDateUtc);
			var resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: true);
			AssertEquals(firstPeriod.PK, resultPeriod);

			Assert("Precondition", transactionDueDate < firstPeriod.NSP_EarliestInvoiceDateUtc);
			resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: false);
			AssertEquals(firstPeriod.PK, resultPeriod);

			transactionDueDate = ZDateTime.Today.AddDays(5); //transaction due date is in the first valid netting cycle
			Assert("Precondition", transactionDueDate < firstPeriod.NSP_LatestInvoiceDateUtc);
			resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: true);
			AssertEquals(firstPeriod.PK, resultPeriod);

			Assert("Precondition", transactionDueDate < firstPeriod.NSP_LatestInvoiceDateUtc);
			resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: false);
			AssertEquals(firstPeriod.PK, resultPeriod);

			transactionDueDate = transactionDueDate.AddDays(30); //transaction due date is in the second valid netting cycle
			Assert("Precondition", transactionDueDate > firstPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate < secondPeriod.NSP_LatestInvoiceDateUtc);
			resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: true);
			AssertEquals(secondPeriod.PK, resultPeriod);

			Assert("Precondition", transactionDueDate > firstPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate < secondPeriod.NSP_LatestInvoiceDateUtc);
			resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: false);
			AssertEquals(secondPeriod.PK, resultPeriod);

			transactionDueDate = transactionDueDate.AddDays(30); //transaction due date is in the thrid valid netting cycle
			Assert("Precondition", transactionDueDate > firstPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate > secondPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate < thirdPeriod.NSP_LatestInvoiceDateUtc);
			resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: true);
			AssertEquals(thirdPeriod.PK, resultPeriod);

			Assert("Precondition", transactionDueDate > firstPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate > secondPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate < thirdPeriod.NSP_LatestInvoiceDateUtc);
			resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: false);
			AssertEquals(thirdPeriod.PK, resultPeriod);

			transactionDueDate = transactionDueDate.AddDays(30); //transaction due date is outside any valid netting cycle
			Assert("Precondition", transactionDueDate > firstPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate > secondPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate > thirdPeriod.NSP_LatestInvoiceDateUtc);
			resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: true);
			AssertEquals(thirdPeriod.PK, resultPeriod);

			Assert("Precondition", transactionDueDate > firstPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate > secondPeriod.NSP_LatestInvoiceDateUtc);
			Assert("Precondition", transactionDueDate > thirdPeriod.NSP_LatestInvoiceDateUtc);
			resultPeriod = NettingHelper.GetNettingPeriod(ns, transactionDueDate, isReceivable: false);
			AssertEquals(thirdPeriod.PK, resultPeriod);
		}

		public void TestARTransactionTransactionType()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAR();

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arTransaction = null;
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			AssertNotNull("Transaction should be created", arTransaction);
			AssertEquals("Transaction type", TransactionTypes.Invoice, arTransaction.TransactionType);

			universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.TransactionType = TransactionType.CRD;

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			AssertNotNull("Transaction should be created", arTransaction);
			AssertEquals("Transaction type", TransactionTypes.CreditNote, arTransaction.TransactionType);

			universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.TransactionType = TransactionType.ADJ;

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			AssertNotNull("Transaction should be created", arTransaction);
			AssertEquals("Transaction type", TransactionTypes.AdjustmentNote, arTransaction.TransactionType);
		}

		public void TestAPTransactionTransactionType()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAP();
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());

			Assert(importer.ImportNettingTransaction(message, universalTransaction));

			NettingPayableTransaction apTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apTransaction);
			AssertEquals("Transaction type", TransactionTypes.Invoice, apTransaction.TransactionType);

			universalTransaction = GetUniversalTransactionForAP();
			universalTransaction.TransactionType = TransactionType.CRD;

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction));

			apTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);
			AssertNotNull("Transaction should be created", apTransaction);
			AssertEquals("Transaction type", TransactionTypes.CreditNote, apTransaction.TransactionType);

			universalTransaction = GetUniversalTransactionForAP();
			universalTransaction.TransactionType = TransactionType.ADJ;

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction));

			apTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);
			AssertNotNull("Transaction should be created", apTransaction);
			AssertEquals("Transaction type", TransactionTypes.AdjustmentNote, apTransaction.TransactionType);
		}

		[TestDate(2018, 12, 15)]
		public void TestImportARWithoutPeriods()
		{
			//Arrange
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAR();
			period.NSP_IsComplete = true;
			Factory.Save();

			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			NettingReceivableTransaction arTransaction = null;
			bool isImported = false;

			//Act
			AssertNoExceptionThrown(() => isImported = importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));

			//Assert
			AssertEquals(false, isImported);
			AssertNull("Transaction should NOT be created", arTransaction);
			AssertContains("Error - No Open Netting Period found for Netting System 'EDIWNS001' at UTC date:", logger.Logs);
			AssertImportTransactionDeleted(importer);
			AssertNull(importer.APNettingTransaction_ForTestOnly);
		}

		public void TestNettingImporterUniversalMessageProcessing()
		{
			//Arrange
			using (AccountingConfigurationRegistry.Instance.IsNettingSystem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var universalTransaction = GetUniversalTransactionForAP();
				universalTransaction.TransactionDate = ZDateTime.Now;
				universalTransaction.DataContext = DataContextFactory.New();
				universalTransaction.DataContext.SetWorkflowInfo(new WorkflowInfo()
				{
					RecipientRoles = new[] { new RecipientRoleDetail() { Type = RecipientRoleType.WNS } }
				});
				universalTransaction.OrganizationAddress.AddressType = "OFC";
				universalTransaction.BranchAddress = new OrganizationAddress
				{
					AddressType = "OFC",
					Address1 = "404 Missing Rd",
					City = "City",
					Country = new Country { Code = Core.Constants.CountryCodes.Bermuda },
					Postcode = "PostCode",
					Port = new UNLOCO { Code = "BM404" },
					OrganizationCode = "EDICUS",
					State = "State",
				};
				TestCaseWithFactoryAndMessagingHelpers.GetQueuedUniversalTransactionMessage(universalTransaction, senderEHubID, nettingSystemEHubID);

				//Act
				(new UMIServiceTask { ServiceLogger = new TestServiceLogger() }).RunTask();

				//Assert
				AssertNull(ErrorReporter.LastExceptionReported);
			}
		}

		public void TestUnmatchReversedTransaction()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);
			creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.FRT.PK);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arOriginalTransaction = null;
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arOriginalTransaction));
			Factory.Save();

			AssertNotNull("Transaction should be created", arOriginalTransaction);

			var apOriginalTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apOriginalTransaction);

			//simulating matching
			var pivot = Factory.New<NettingMatchPivot>();
			pivot.NMP_NSP_Period = period.PK;
			pivot.NMP_NRT_ReceivableTransaction = arOriginalTransaction.PK;
			pivot.NMP_NPT_PayableTransaction = apOriginalTransaction.PK;

			arOriginalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apOriginalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			invoice.GenerateReverseTransaction(true);

			var reversedInvoice = invoice.ReverseInvoice;
			reversedInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			reversedInvoice.AH_TransactionNum = "Reversed transaction";
			invoice.SetTransactionBelongsToGroupField(reversedInvoice.PK);

			Factory.Save();

			universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, reversedInvoice)));
			universalTransaction = writer.GetDataObject(reversedInvoice);
			universalTransaction.IsCancelled = true;

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arReversedTransaction = null;
			AssertNoExceptionThrown(() => importer.ImportNettingTransaction(message, universalTransaction, ref arReversedTransaction));

			Factory.Save();

			ReleaseFactory();

			var arOriginalTransaction2 = Factory.Load<NettingReceivableTransaction>(arOriginalTransaction.PK);
			AssertEquals("Approval status changed from MAT to REV", NettingTransactionApprovalStatus.Reversed, arOriginalTransaction2.ApprovalStatus);

			var arReversedTransaction2 = Factory.Load<NettingReceivableTransaction>(arReversedTransaction.PK);
			AssertEquals("Approval status is REV", NettingTransactionApprovalStatus.Reversed, arReversedTransaction2.ApprovalStatus);

			var apOriginalTransaction2 = Factory.Load<NettingPayableTransaction>(apOriginalTransaction.PK);
			AssertEquals("Approval status changed from MAT to APP", NettingTransactionApprovalStatus.Approved, apOriginalTransaction2.ApprovalStatus);

			var pivot2 = Factory.Load<NettingMatchPivot>(pivot.PK);
			AssertNull("Matching pivot should be deleted", pivot2);
		}

		public void TestTransactionReversal_OriginalTransactionApprovedAndNotSettled()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);
			creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.FRT.PK);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arOriginalTransaction = null;
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arOriginalTransaction));
			Factory.Save();

			AssertNotNull("Transaction should be created", arOriginalTransaction);

			var apOriginalTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apOriginalTransaction);

			arOriginalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Approved;
			apOriginalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Approved;

			Factory.Save();

			invoice.GenerateReverseTransaction(true);

			var reversedInvoice = invoice.ReverseInvoice;
			reversedInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			reversedInvoice.AH_TransactionNum = "Reversed transaction";
			invoice.SetTransactionBelongsToGroupField(reversedInvoice.PK);

			Factory.Save();

			universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, reversedInvoice)));
			universalTransaction = writer.GetDataObject(reversedInvoice);
			universalTransaction.IsCancelled = true;

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arReversedTransaction = null;
			AssertNoExceptionThrown(() => importer.ImportNettingTransaction(message, universalTransaction, ref arReversedTransaction));

			Factory.Save();

			ReleaseFactory();

			var arOriginalTransaction2 = Factory.Load<NettingReceivableTransaction>(arOriginalTransaction.PK);
			AssertEquals("Approval status changed from MAT to REV", NettingTransactionApprovalStatus.Reversed, arOriginalTransaction2.ApprovalStatus);

			var arReversedTransaction2 = Factory.Load<NettingReceivableTransaction>(arReversedTransaction.PK);
			AssertEquals("Approval status is REV", NettingTransactionApprovalStatus.Reversed, arReversedTransaction2.ApprovalStatus);

			var apOriginalTransaction2 = Factory.Load<NettingPayableTransaction>(apOriginalTransaction.PK);
			AssertEquals("Approval status changed from MAT to APP", NettingTransactionApprovalStatus.Approved, apOriginalTransaction2.ApprovalStatus);
		}

		public void TestTransactionReversal_OriginalTransactionNotApprovedAndNotSettled()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);
			creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.FRT.PK);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arOriginalTransaction = null;
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arOriginalTransaction));
			Factory.Save();

			AssertNotNull("Transaction should be created", arOriginalTransaction);

			var apOriginalTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apOriginalTransaction);

			arOriginalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Disputed;
			apOriginalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Disputed;

			Factory.Save();

			invoice.GenerateReverseTransaction(true);

			var reversedInvoice = invoice.ReverseInvoice;
			reversedInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			reversedInvoice.AH_TransactionNum = "Reversed transaction";
			invoice.SetTransactionBelongsToGroupField(reversedInvoice.PK);

			Factory.Save();

			universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, reversedInvoice)));
			universalTransaction = writer.GetDataObject(reversedInvoice);
			universalTransaction.IsCancelled = true;

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arReversedTransaction = null;
			AssertNoExceptionThrown(() => importer.ImportNettingTransaction(message, universalTransaction, ref arReversedTransaction));

			Factory.Save();

			ReleaseFactory();

			var arOriginalTransaction2 = Factory.Load<NettingReceivableTransaction>(arOriginalTransaction.PK);
			AssertEquals("Approval status changed from MAT to REV", NettingTransactionApprovalStatus.Disputed, arOriginalTransaction2.ApprovalStatus);

			var arReversedTransaction2 = Factory.Load<NettingReceivableTransaction>(arReversedTransaction.PK);
			AssertEquals("Approval status is REV", NettingTransactionApprovalStatus.Reversed, arReversedTransaction2.ApprovalStatus);

			var apOriginalTransaction2 = Factory.Load<NettingPayableTransaction>(apOriginalTransaction.PK);
			AssertEquals("Approval status changed from MAT to APP", NettingTransactionApprovalStatus.Disputed, apOriginalTransaction2.ApprovalStatus);
		}

		public void TestTransactionReversal_OriginalTransactionSettled()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);
			creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 120M, 0M, 0M, 120M, 0M, 0M, creator.FRT.PK);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arOriginalTransaction = null;
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arOriginalTransaction));
			Factory.Save();

			AssertNotNull("Transaction should be created", arOriginalTransaction);

			var apOriginalTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apOriginalTransaction);

			arOriginalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Setteled;
			apOriginalTransaction.ApprovalStatus = NettingTransactionApprovalStatus.SettledOutOfNetting;

			Factory.Save();

			invoice.GenerateReverseTransaction(true);

			var reversedInvoice = invoice.ReverseInvoice;
			reversedInvoice.IsManuallySetTransactionNumber_ForTestOnly = true;
			reversedInvoice.AH_TransactionNum = "Reversed transaction";
			invoice.SetTransactionBelongsToGroupField(reversedInvoice.PK);

			Factory.Save();

			universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance);
			writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, reversedInvoice)));
			universalTransaction = writer.GetDataObject(reversedInvoice);
			universalTransaction.IsCancelled = true;

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arReversedTransaction = null;
			AssertNoExceptionThrown(() => importer.ImportNettingTransaction(message, universalTransaction, ref arReversedTransaction));

			Factory.Save();

			ReleaseFactory();

			var arOriginalTransaction2 = Factory.Load<NettingReceivableTransaction>(arOriginalTransaction.PK);
			AssertEquals("Approval status changed from MAT to REV", NettingTransactionApprovalStatus.Setteled, arOriginalTransaction2.ApprovalStatus);

			var arReversedTransaction2 = Factory.Load<NettingReceivableTransaction>(arReversedTransaction.PK);
			AssertEquals("Approval status is REV", NettingTransactionApprovalStatus.Approved, arReversedTransaction2.ApprovalStatus);

			var apOriginalTransaction2 = Factory.Load<NettingPayableTransaction>(apOriginalTransaction.PK);
			AssertEquals("Approval status changed from MAT to APP", NettingTransactionApprovalStatus.SettledOutOfNetting, apOriginalTransaction2.ApprovalStatus);
		}

		[TestDate(2014, 12, 15)]
		public void TestImportARDoesNotOverwriteMatchedTransaction()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.TransactionDate = ZDateTime.Now;

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arTransaction = null;
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));

			var arNettingTransaction = arTransaction;

			AssertNotNull("Transaction should be created", arNettingTransaction);

			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apNettingTransaction);

			arNettingTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;
			apNettingTransaction.ApprovalStatus = NettingTransactionApprovalStatus.Matched;

			Factory.Save();

			importer.ImportNettingTransaction(message, universalTransaction);

			AssertEquals(arNettingTransaction.ApprovalStatus, NettingTransactionApprovalStatus.Matched);
			AssertEquals(apNettingTransaction.ApprovalStatus, NettingTransactionApprovalStatus.Matched);
		}

		[TestDate(2017, 08, 10)]
		public void TestImportReverseARInvoice()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.TransactionType = TransactionType.CRD;

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arNettingTransaction = null;
			AssertNoExceptionThrown(() => importer.ImportNettingTransaction(message, universalTransaction, ref arNettingTransaction));

			AssertNotNull("Transaction should be created", arNettingTransaction);
			AssertEquals("Original transaction is going to be empty since the original transaction did not come through netting", ZGuid.Empty, arNettingTransaction.OriginalTransaction);

			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apNettingTransaction);
			AssertEquals("Original transaction is going to be empty since the original transaction did not come through netting", ZGuid.Empty, arNettingTransaction.OriginalTransaction);
		}

		[TestDate(2017, 08, 10)]
		public void TestOSCurrencyIsNotSet()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.OSCurrency = null;
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arTransaction = null;

			AssertExceptionThrown<MalformedUniversalXmlException>(() => importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
		}

		[TestDate(2014, 12, 15)]
		public void TestImportAR_SingleJobInvoice()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.TransactionDate = ZDateTime.Now;
			universalTransaction.OSTotal = 120M;
			var currency = new Currency();
			currency.Code = "AUD";
			currency.Description = "Australian Dollar";
			var postingJournal = new PostingJournal
			{
				Job = new EntityReference() { Type = "Job", Key = "S001001" },
				ChargeCode = new ChargeCode() { Code = "FRT" },
				OSTotalAmount = 120M,
				OSCurrency = currency
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { postingJournal });
			var dataContext = DataContextFactory.New();
			dataContext.AddDataSource(DataContextType.ForwardingShipment, "S001001");
			var shipment = new Shipment()
			{
				DataContext = dataContext,
				WayBillNumber = "123456",
				WayBillType = new WayBillType() { Code = "HWB" }
			};
			universalTransaction.ShipmentCollection.Add(shipment);

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arTransaction = null;

			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction)); //simulate duplicate EDIMessage

			var arNettingTransaction = arTransaction;

			AssertNotNull("Transaction should be created", arNettingTransaction);

			AssertEquals("Recipient:", receiver.PK, arNettingTransaction.RecipientPK);
			AssertEquals("Netting system:", ns.PK, arNettingTransaction.NettingSystemPK);

			AssertEquals("Reference:", "00001000", arNettingTransaction.Reference);
			AssertEquals("Transaction date:", ZDateTime.Today, arNettingTransaction.Date);
			AssertEquals("Due date:", ZDateTime.Today, arNettingTransaction.DueDate);
			AssertEquals("Amount:", 120M, arNettingTransaction.Amount);
			AssertEquals("Currency:", "AUD", arNettingTransaction.Currency);
			AssertEquals("Transaction Type:", TransactionTypes.Invoice, arNettingTransaction.TransactionType);

			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apNettingTransaction);

			AssertEquals("Recipient:", arNettingTransaction.RecipientPK, apNettingTransaction.RecipientPK);
			AssertEquals("Netting system:", arNettingTransaction.NettingSystemPK, apNettingTransaction.NettingSystemPK);

			AssertEquals("Reference:", arNettingTransaction.Reference, apNettingTransaction.Reference);
			AssertEquals("Transaction date:", arNettingTransaction.Date, apNettingTransaction.Date);
			AssertEquals("Due date:", arNettingTransaction.DueDate, apNettingTransaction.DueDate);
			AssertEquals("Amount:", arNettingTransaction.Amount, apNettingTransaction.Amount);
			AssertEquals("Currency:", arNettingTransaction.Currency, apNettingTransaction.Currency);
			AssertEquals("Transaction Type:", arNettingTransaction.TransactionType, apNettingTransaction.TransactionType);

			var queryLine1 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			queryLine1.AddToFilter(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference, "S001001");
			var lines = Factory.Load<NettingReceivableTransactionLine>(queryLine1);
			AssertEquals("There only should be one line", 1, lines.Length);
			var line1 = lines[0];
			AssertEquals("Line1 amount", 120M, line1.NRL_Amount);
			AssertEquals("Line1 currency", "AUD", line1.NRL_RX_NKCurrency);
			AssertEquals("Is Approved", false, line1.IsApproved);

			var queryLine1Ref = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, line1.PK);
			queryLine1Ref.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.HouseBill });
			var refLine1 = Factory.Load<NettingReceivableLineReference>(queryLine1Ref);
			AssertNotNull(refLine1);
			AssertEquals("2 line references should be found", 2, refLine1.Length);
		}

		[TestDate(2014, 12, 15)]
		public void TestImportAR_InNonJobRelated()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.OSTotal = 120m;
			universalTransaction.TransactionDate = ZDateTime.Now;

			NettingReceivableTransaction arTransaction = null;
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction)); //simulate duplicate EDIMessage

			var arNettingTransaction = arTransaction;

			AssertNotNull("Transaction should be created", arNettingTransaction);

			AssertEquals("Recipient:", receiver.PK, arNettingTransaction.RecipientPK);
			AssertEquals("Netting system:", ns.PK, arNettingTransaction.NettingSystemPK);

			AssertEquals("Reference:", "00001000", arNettingTransaction.Reference);
			AssertEquals("Transaction date:", ZDateTime.Today, arNettingTransaction.Date);
			AssertEquals("Due date:", ZDateTime.Today, arNettingTransaction.DueDate);
			AssertEquals("Amount:", 120M, arNettingTransaction.Amount);
			AssertEquals("Currency:", "AUD", arNettingTransaction.Currency);

			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apNettingTransaction);

			AssertEquals("Recipient:", arNettingTransaction.RecipientPK, apNettingTransaction.RecipientPK);
			AssertEquals("Netting system:", arNettingTransaction.NettingSystemPK, apNettingTransaction.NettingSystemPK);

			AssertEquals("Reference:", arNettingTransaction.Reference, apNettingTransaction.Reference);
			AssertEquals("Transaction date:", arNettingTransaction.Date, apNettingTransaction.Date);
			AssertEquals("Due date:", arNettingTransaction.DueDate, apNettingTransaction.DueDate);
			AssertEquals("Amount:", arNettingTransaction.Amount, apNettingTransaction.Amount);
			AssertEquals("Currency:", arNettingTransaction.Currency, apNettingTransaction.Currency);

			var queryLine1 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			var line1 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine1);
			AssertNull("No line created as line not related to job", line1);

			var queryLine2 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			var line2 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine2);
			AssertNull("No line created as line not related to job", line2);
		}

		[TestDate(2014, 12, 15)]
		public void TestImportWhenMultipleOrgIsSetupWithSameEhubID()
		{
			// Arrange
			var org1 = Creator.CreateOrgHeader("TSTSND2", true, true);
			org1.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, senderEHubID);
			var org1Participant = Creator.CreateNettingOrganisation(ns, org1, "CUR");

			Factory.Save();

			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.OrganizationAddress = new OrganizationAddress() { OrganizationCode = "ZTSTSND2" };
			universalTransaction.BranchAddress = new OrganizationAddress() { OrganizationCode = "ZTSTSND1" };

			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			NettingReceivableTransaction arNettingTransaction = null;
			importer.ImportNettingTransaction(message, universalTransaction, ref arNettingTransaction);

			// Act
			importer.ImportNettingTransaction(message, universalTransaction, ref arNettingTransaction); //simulate duplicate EDIMessage

			// Assert
			AssertNotNull("Receivable Transaction should be created", arNettingTransaction);

			AssertNotEquals("Issuer and Recipient of the AR transaction should not be the same", arNettingTransaction.IssuerPK, arNettingTransaction.RecipientPK);
			AssertEquals(sender.PK, arNettingTransaction.IssuerPK);
			AssertEquals(org1Participant.PK, arNettingTransaction.RecipientPK);

			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);
			AssertNotNull("Payable Transaction should not created", apNettingTransaction);

			AssertNotEquals("Issuer and Recipient of the AP transaction should not be the same", apNettingTransaction.IssuerPK, apNettingTransaction.RecipientPK);
			AssertEquals(sender.PK, apNettingTransaction.IssuerPK);
			AssertEquals(org1Participant.PK, apNettingTransaction.RecipientPK);

			AssertContains("Information - Netting Participant ZTSTSND1 matched for branch address", logger.Logs);
			AssertContains("Information - Netting Participant ZTSTSND2 matched for organization address", logger.Logs);
			AssertContains("Information - AR Netting Transaction 00001000: Issuer ZTSTSND1 and Recipient ZTSTSND2", logger.Logs);
		}

		[TestDate(2014, 12, 15)]
		public void TestImportAR_MultipleShipmentLevelInvoice()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var shipment1 = creator.CreateShipment("S001001");
			shipment1.JS_HouseBill = "123456";
			var job1 = creator.CreateJob(shipment1, false);

			var carrierBookingReference = shipment1.Numbers.AddNew();
			carrierBookingReference.CE_EntryType = "BKG";
			carrierBookingReference.CE_EntryNum = "BKG001002";
			carrierBookingReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var agentReference = shipment1.Numbers.AddNew();
			agentReference.CE_EntryType = "OAG";
			agentReference.CE_EntryNum = "OAG001002";
			agentReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var order = shipment1.AttachedOrders.AddNew();
			order.BuyerPK = creator.Agent.PK;
			order.JD_OrderNumber = "ORD001002";

			var shipment2 = creator.CreateShipment("S001002");
			shipment2.JS_HouseBill = "234567";
			var job2 = creator.CreateJob(shipment2, false);

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);

			var invoiceLine1 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine1.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1, job1, creator.FRT);

			var invoiceLine2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine2.AL_JH = job2.PK;
			creator.CreateJobCharge(invoiceLine2, job2, creator.FRT);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			NettingReceivableTransaction arTransaction = null;
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Factory.Save();
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction)); //simulate duplicate EDIMessage
			Factory.Save();

			var arNettingTransaction = arTransaction;

			AssertNotNull("Transaction should be created", arNettingTransaction);

			AssertEquals("Recipient:", receiver.PK, arNettingTransaction.RecipientPK);
			AssertEquals("Netting system:", ns.PK, arNettingTransaction.NettingSystemPK);

			AssertEquals("Reference:", "00001000", arNettingTransaction.Reference);
			AssertEquals("Transaction date:", ZDateTime.Today, arNettingTransaction.Date);
			AssertEquals("Due date:", ZDateTime.Today, arNettingTransaction.DueDate);
			AssertEquals("Amount:", 120M, arNettingTransaction.Amount);
			AssertEquals("Currency:", "AUD", arNettingTransaction.Currency);

			var lineQuery = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, importer.ARNettingTransaction_ForTestOnly.PK);
			var lines = Factory.Load<NettingReceivableTransactionLine>(lineQuery);
			AssertEquals("2 lines should be there", 2, lines.Length);

			var query = new ZQuery(NettingReceivableTransactionRefSchema.NRR_NRT_Transaction, arNettingTransaction.PK);
			query.AddToFilter(NettingReceivableTransactionRefSchema.NRR_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber });
			var transactionReferences = Factory.Load<NettingReceivableTransactionRef>(query);
			AssertNotNull(transactionReferences);
			AssertEquals("1 header level references should be found", 1, transactionReferences.Length);

			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apNettingTransaction);

			AssertEquals("Issuer:", arNettingTransaction.IssuerPK, apNettingTransaction.IssuerPK);
			AssertEquals("Recipient:", arNettingTransaction.RecipientPK, apNettingTransaction.RecipientPK);
			AssertEquals("Netting system:", arNettingTransaction.NettingSystemPK, apNettingTransaction.NettingSystemPK);

			AssertEquals("Reference:", arNettingTransaction.Reference, apNettingTransaction.Reference);
			AssertEquals("Transaction date:", arNettingTransaction.Date, apNettingTransaction.Date);
			AssertEquals("Due date:", arNettingTransaction.DueDate, apNettingTransaction.DueDate);
			AssertEquals("Amount:", arNettingTransaction.Amount, apNettingTransaction.Amount);
			AssertEquals("Currency:", arNettingTransaction.Currency, apNettingTransaction.Currency);

			var queryLine1 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			queryLine1.AddToFilter(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference, "S001001");
			var line1 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine1);
			AssertEquals("Line1 amount", 60M, line1.NRL_Amount);
			AssertEquals("Line1 currency", "AUD", line1.NRL_RX_NKCurrency);
			AssertEquals("Is Approved", false, line1.IsApproved);

			var queryLine1Ref = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, line1.PK);
			queryLine1Ref.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.OrderReferences });
			var refLine1 = Factory.Load<NettingReceivableLineReference>(queryLine1Ref);
			AssertNotNull(refLine1);
			AssertEquals("5 line references should be found", 5, refLine1.Length);

			var queryLine2 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			queryLine2.AddToFilter(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference, "S001002");
			var line2 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine2);
			AssertEquals("Line2 amount", 60M, line2.NRL_Amount);
			AssertEquals("Line2 currency", "AUD", line2.NRL_RX_NKCurrency);
			AssertEquals("Is Approved", false, line2.IsApproved);

			var queryLine2Ref = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, line2.PK);
			queryLine2Ref.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.HouseBill });
			var refLine2 = Factory.Load<NettingReceivableLineReference>(queryLine2Ref);
			AssertNotNull(refLine2);
			AssertEquals("2 line references should be found", 2, refLine2.Length);
		}

		[TestDate(2014, 12, 15)]
		public void TestImportAR_SingleConsolLevelInvoice()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001002");
			consol.JK_UniqueConsignRef = "CZ1";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CCN001002";

			var shipment1 = creator.CreateShipment("S001001", consol);
			shipment1.JS_HouseBill = "123456";
			var packedContainer1 = shipment1.OuterPackLines.AddNew();
			packedContainer1.JL_RefNumber = "PCK001002";
			var job1 = creator.CreateJob(shipment1, false);

			var shipment2 = creator.CreateShipment("S001002", consol);
			shipment2.JS_HouseBill = "234567";
			var packedContainer2 = shipment2.OuterPackLines.AddNew();
			packedContainer2.JL_RefNumber = "PCK001003";
			var job2 = creator.CreateJob(shipment2, false);

			var carrierBookingReference = shipment1.Numbers.AddNew();
			carrierBookingReference.CE_EntryType = "BKG";
			carrierBookingReference.CE_EntryNum = "BKG001002";
			carrierBookingReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var agentReference = shipment1.Numbers.AddNew();
			agentReference.CE_EntryType = "OAG";
			agentReference.CE_EntryNum = "OAG001002";
			agentReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);

			var invoiceLine1 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine1.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1, job1, creator.FRT);

			var invoiceLine2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine2.AL_JH = job2.PK;
			creator.CreateJobCharge(invoiceLine2, job2, creator.FRT);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			NettingReceivableTransaction arTransaction = null;
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Factory.Save();
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction)); //simulate duplicate EDIMessage
			Factory.Save();

			var arNettingTransaction = arTransaction;

			AssertNotNull("Transaction should be created", arNettingTransaction);

			AssertEquals("Issuer:", GlbBranch.CurrentBranch.GB_OH_OrgProxy, arNettingTransaction.Issuer.NSO_OH_Organisation);
			AssertEquals("Recipient:", receiver.PK, arNettingTransaction.RecipientPK);
			AssertEquals("Netting system:", ns.PK, arNettingTransaction.NettingSystemPK);

			AssertEquals("Reference:", "00001000", arNettingTransaction.Reference);
			AssertEquals("Transaction date:", ZDateTime.Today, arNettingTransaction.Date);
			AssertEquals("Due date:", ZDateTime.Today, arNettingTransaction.DueDate);
			AssertEquals("Amount:", 120M, arNettingTransaction.Amount);
			AssertEquals("Currency:", "AUD", arNettingTransaction.Currency);

			var lineQuery = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, importer.ARNettingTransaction_ForTestOnly.PK);
			var lines = Factory.Load<NettingReceivableTransactionLine>(lineQuery);
			AssertEquals("2 lines should be there", 2, lines.Length);

			var query = new ZQuery(NettingReceivableTransactionRefSchema.NRR_NRT_Transaction, arNettingTransaction.PK);
			query.AddToFilter(NettingReceivableTransactionRefSchema.NRR_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ConsolNumber, AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber });
			var transactionReferences = Factory.Load<NettingReceivableTransactionRef>(query);
			AssertNotNull(transactionReferences);
			AssertEquals("2 header level references should be found", 2, transactionReferences.Length);

			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apNettingTransaction);

			AssertEquals("Issuer:", arNettingTransaction.IssuerPK, apNettingTransaction.IssuerPK);
			AssertEquals("Recipient:", arNettingTransaction.RecipientPK, apNettingTransaction.RecipientPK);
			AssertEquals("Netting system:", arNettingTransaction.NettingSystemPK, apNettingTransaction.NettingSystemPK);

			AssertEquals("Reference:", arNettingTransaction.Reference, apNettingTransaction.Reference);
			AssertEquals("Transaction date:", arNettingTransaction.Date, apNettingTransaction.Date);
			AssertEquals("Due date:", arNettingTransaction.DueDate, apNettingTransaction.DueDate);
			AssertEquals("Amount:", arNettingTransaction.Amount, apNettingTransaction.Amount);
			AssertEquals("Currency:", arNettingTransaction.Currency, apNettingTransaction.Currency);

			var queryLine1 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			queryLine1.AddToFilter(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference, "S001001");
			var line1 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine1);
			AssertEquals("Line1 amount", 60M, line1.NRL_Amount);
			AssertEquals("Line1 currency", "AUD", line1.NRL_RX_NKCurrency);
			AssertEquals("Is Approved", false, line1.IsApproved);

			var queryLine1Ref = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, line1.PK);
			queryLine1Ref.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var refLine1 = Factory.Load<NettingReceivableLineReference>(queryLine1Ref);
			AssertNotNull(refLine1);
			AssertEquals("6 line references should be found", 6, refLine1.Length);

			var queryLine2 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			queryLine2.AddToFilter(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference, "S001002");
			var line2 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine2);
			AssertEquals("Line2 amount", 60M, line2.NRL_Amount);
			AssertEquals("Line2 currency", "AUD", line2.NRL_RX_NKCurrency);
			AssertEquals("Is Approved", false, line2.IsApproved);

			var queryLine2Ref = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, line2.PK);
			queryLine2Ref.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var refLine2 = Factory.Load<NettingReceivableLineReference>(queryLine2Ref);
			AssertNotNull(refLine2);
			AssertEquals("4 line references should be found", 4, refLine2.Length);
		}

		[TestDate(2014, 12, 15)]
		public void TestImportAP_MultipleConsol()
		{
			var message = GetMessage(receiverEHubID, nettingSystemEHubID);

			var consol1 = creator.CreateConsol("AUSYD", "NZAKL", "C001002");
			var container = consol1.Containers.AddNew();
			container.JC_ContainerNum = "CCN001002";
			var shipment1 = creator.CreateShipment("S001001", consol1);
			shipment1.JS_HouseBill = "123456";
			var packedContainer = shipment1.OuterPackLines.AddNew();
			packedContainer.JL_RefNumber = "PCK001002";
			var job1 = creator.CreateJob(shipment1, false);

			var carrierBookingReference = shipment1.Numbers.AddNew();
			carrierBookingReference.CE_EntryType = "BKG";
			carrierBookingReference.CE_EntryNum = "BKG001002";
			carrierBookingReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var agentReference = shipment1.Numbers.AddNew();
			agentReference.CE_EntryType = "OAG";
			agentReference.CE_EntryNum = "OAG001002";
			agentReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var order = shipment1.AttachedOrders.AddNew();
			order.BuyerPK = creator.Agent.PK;
			order.JD_OrderNumber = "ORD001002";

			var consol2 = creator.CreateConsol("AUSYD", "NZAKL", "C001003");
			var shipment2 = creator.CreateShipment("S001002", consol2);
			shipment2.JS_HouseBill = "234567";
			var job2 = creator.CreateJob(shipment2, false);

			var invoice = creator.CreateInvoice(typeof(APInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);

			var invoiceLine1 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine1.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1, job1, creator.FRT);

			var invoiceLine2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine2.AL_JH = job2.PK;
			creator.CreateJobCharge(invoiceLine2, job2, creator.FRT);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			NettingReceivableTransaction arTransaction = null;
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Factory.Save();

			var nettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", nettingTransaction);
			AssertContains("Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Information - Netting Participant ZTSTRCV1 matched for organization address", logger.Logs);
			AssertContains("Information - AP Netting Transaction 00001000: Issuer ZTSTRCV1 and Recipient EDICUS", logger.Logs);
			AssertEquals("Recipient:", GlbBranch.CurrentBranch.GB_OH_OrgProxy, nettingTransaction.Recipient.NSO_OH_Organisation);
			AssertEquals("Netting system:", ns.PK, nettingTransaction.NettingSystemPK);

			AssertEquals("Reference:", "00001000", nettingTransaction.Reference);
			AssertEquals("Transaction date:", ZDateTime.Today, nettingTransaction.Date);
			AssertEquals("Due date:", ZDateTime.Today, nettingTransaction.DueDate);
			AssertEquals("Amount:", 120M, nettingTransaction.Amount);
			AssertEquals("Currency:", "AUD", nettingTransaction.Currency);

			var queryLine1 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine1.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001001");
			var line1 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine1);
			AssertEquals("Line1 amount", 60M, line1.NPL_Amount);
			AssertEquals("Line1 currency", "AUD", line1.NPL_RX_NKCurrency);
			AssertEquals("Is Approved", true, line1.IsApproved);

			var queryLine1Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, line1.PK);
			queryLine1Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ConsolNumber, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.OrderReferences });
			var refLine1 = Factory.Load<NettingPayableLineReference>(queryLine1Ref);
			AssertNotNull(refLine1);
			AssertEquals("7 line references should be found", 7, refLine1.Length);

			var queryLine2 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine2.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001002");
			var line2 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine2);
			AssertEquals("Line2 amount", 60M, line2.NPL_Amount);
			AssertEquals("Line2 currency", "AUD", line2.NPL_RX_NKCurrency);
			AssertEquals("Is Approved", true, line2.IsApproved);

			var queryLine2Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, line2.PK);
			queryLine2Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ConsolNumber, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.HouseBill });
			var refLine2 = Factory.Load<NettingPayableLineReference>(queryLine2Ref);
			AssertNotNull(refLine2);
			AssertEquals("3 reference should be found for the consol linked to second line", 3, refLine2.Length);
		}

		[TestDate(2014, 12, 15)]
		public void TestImportAPNettingSystem_UpdateExistingPayable()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001002");
			consol.JK_UniqueConsignRef = "CZ1";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CCN001002";

			var shipment1 = creator.CreateShipment("S001001", consol);
			shipment1.JS_HouseBill = "123456";
			var packedContainer1 = shipment1.OuterPackLines.AddNew();
			packedContainer1.JL_RefNumber = "PCK001002";
			var job1 = creator.CreateJob(shipment1, false);

			var shipment2 = creator.CreateShipment("S001002", consol);
			shipment2.JS_HouseBill = "234567";
			var packedContainer2 = shipment2.OuterPackLines.AddNew();
			packedContainer2.JL_RefNumber = "PCK001003";
			var job2 = creator.CreateJob(shipment2, false);

			var carrierBookingReference = shipment1.Numbers.AddNew();
			carrierBookingReference.CE_EntryType = "BKG";
			carrierBookingReference.CE_EntryNum = "BKG001002";
			carrierBookingReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var agentReference = shipment1.Numbers.AddNew();
			agentReference.CE_EntryType = "OAG";
			agentReference.CE_EntryNum = "OAG001002";
			agentReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);

			var invoiceLine1 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine1.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1, job1, creator.FRT);

			var invoiceLine2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine2.AL_JH = job2.PK;
			creator.CreateJobCharge(invoiceLine2, job2, creator.FRT);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			NettingReceivableTransaction arTransaction = null;
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Factory.Save();

			//import will create ar transaction entries but will only create ap header entries
			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);
			AssertNotNull(apNettingTransaction);
			AssertContains("Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Information - Netting Participant ZTSTRCV1 matched for organization address", logger.Logs);
			AssertContains("Information - AR Netting Transaction 00001000: Issuer EDICUS and Recipient ZTSTRCV1", logger.Logs);
			AssertEquals("Precondition: transaction date is not set", arTransaction.Date, apNettingTransaction.Date);
			AssertEquals("Precondition: due date is not set", arTransaction.DueDate, apNettingTransaction.DueDate);
			AssertEquals("Precondition: Approval status is 'ADD'", "ADD", apNettingTransaction.ApprovalStatus);

			AssertEquals("Invoice Type is set for the AR transactions", TransactionTypes.Invoice, arTransaction.NRT_TransactionType);
			AssertEquals("Invoice Type is set for the AP transactions", TransactionTypes.Invoice, apNettingTransaction.NPT_TransactionType);

			var queryLine1 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine1.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001001");
			var line1 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine1);
			AssertNotNull(line1);
			AssertEquals("Line1 amount", 60M, line1.NPL_Amount);
			AssertEquals("Line1 currency", "AUD", line1.NPL_RX_NKCurrency);
			AssertEquals("Is Approved", false, line1.IsApproved);

			var queryLine1Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, line1.PK);
			queryLine1Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var refLine1 = Factory.Load<NettingPayableLineReference>(queryLine1Ref);
			AssertNotNull(refLine1);
			AssertEquals("6 line references should be found", 6, refLine1.Length);

			var queryLine2 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine2.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001002");
			var line2 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine2);
			AssertNotNull(line2);
			AssertEquals("Line2 amount", 60M, line2.NPL_Amount);
			AssertEquals("Line2 currency", "AUD", line2.NPL_RX_NKCurrency);
			AssertEquals("Is Approved", false, line2.IsApproved);

			var queryLine2Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, line2.PK);
			queryLine2Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var refLine2 = Factory.Load<NettingPayableLineReference>(queryLine2Ref);
			AssertNotNull(refLine2);
			AssertEquals("4 line references should be found", 4, refLine2.Length);

			//Will import AP this time, from the receiver
			message = GetMessage(receiverEHubID, nettingSystemEHubID);

			invoice = creator.CreateInvoice(typeof(APInvoice), "00001000", creator.AUD, 1M, sender.Organisation);

			var invoiceLine1A = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 40M, 0M, 0M, 40M, 0M, 0M, creator.FRT.PK);
			invoiceLine1A.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1A, job1, creator.FRT);

			var invoiceLine1B = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 20M, 0M, 0M, 20M, 0M, 0M, creator.FRT.PK);
			invoiceLine1B.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1B, job1, creator.FRT);

			invoiceLine2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine2.AL_JH = job2.PK;
			creator.CreateJobCharge(invoiceLine2, job2, creator.FRT);

			Factory.Save();

			writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			universalTransaction = writer.GetDataObject(invoice);
			universalTransaction.BranchAddress = new OrganizationAddress { OrganizationCode = "ZTSTRCV1" };
			universalTransaction.OrganizationAddress = new OrganizationAddress { OrganizationCode = "EDICUS" };

			logger = new TestErrorLogger();
			importer = new NettingTransactionImporter(Factory, logger);
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Factory.Save();

			AssertContains("Information - Netting Participant ZTSTRCV1 matched for branch address", logger.Logs);
			AssertContains("Information - Netting Participant EDICUS matched for organization address", logger.Logs);
			AssertContains("Information - AP Netting Transaction 00001000: Issuer EDICUS and Recipient ZTSTRCV1", logger.Logs);
			AssertEquals("transaction date should be set", ZDateTime.Today, apNettingTransaction.Date);
			AssertEquals("Precondition: due date is not set", ZDateTime.Today, apNettingTransaction.DueDate);
			AssertEquals("Precondition: Approval status is 'APP'", "APP", apNettingTransaction.ApprovalStatus);

			var newFactory = new BusinessObjectFactory();
			var queryTransactionRef = new ZQuery(NettingPayableTransactionRefSchema.NPR_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryTransactionRef.AddToFilter(NettingPayableTransactionRefSchema.NPR_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber, AccountingConstants.TransactionReferenceTypes.JobInvoiceNumber, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var references = newFactory.Load<NettingPayableTransactionRef>(queryTransactionRef);
			AssertNotNull(references);
			AssertEquals("3 transaction references should be found", 3, references.Length);

			queryLine1 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine1.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001001");
			var lines = newFactory.Load<NettingPayableTransactionLine>(queryLine1);
			AssertNotNull(lines);
			AssertEquals("2 lines associated to job S001001 should be found", 2, lines.Length);
			var line1A = lines.First(x => x.NPL_Amount == 40);
			AssertEquals("Line1 amount", 40M, line1A.NPL_Amount);
			AssertEquals("Line1 currency", "AUD", line1A.NPL_RX_NKCurrency);
			AssertEquals("Is Approved", true, line1A.IsApproved);

			var line1B = lines.First(x => x.NPL_Amount == 20);
			AssertEquals("Line1 amount", 20M, line1B.NPL_Amount);
			AssertEquals("Line1 currency", "AUD", line1B.NPL_RX_NKCurrency);
			AssertEquals("Is Approved", true, line1B.IsApproved);

			queryLine1Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, line1A.PK);
			queryLine1Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			refLine1 = newFactory.Load<NettingPayableLineReference>(queryLine1Ref);
			AssertNotNull(refLine1);
			AssertEquals("6 line references should be found", 6, refLine1.Length);

			queryLine1Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, line1B.PK);
			queryLine1Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			refLine1 = newFactory.Load<NettingPayableLineReference>(queryLine1Ref);
			AssertNotNull(refLine1);
			AssertEquals("6 line references should be found", 6, refLine1.Length);

			queryLine2 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine2.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001002");
			line2 = newFactory.LoadTop1<NettingPayableTransactionLine>(queryLine2);
			AssertEquals("Line2 amount", 60M, line2.NPL_Amount);
			AssertEquals("Line2 currency", "AUD", line2.NPL_RX_NKCurrency);
			AssertEquals("Is Approved", true, line2.IsApproved);

			queryLine2Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, line2.PK);
			queryLine2Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			refLine2 = newFactory.Load<NettingPayableLineReference>(queryLine2Ref);
			AssertNotNull(refLine2);
			AssertEquals("4 line references should be found", 4, refLine2.Length);
		}

		[TestDate(2014, 12, 15)]
		public void TestImportAPNettingSystem_UpdateExistingPayable_CurrentPeriodIsClosed()
		{
			period.NSP_IsComplete = true;
			var nextPeriod = SetupNextNettingPeriod(ns, period);

			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001002");
			consol.JK_UniqueConsignRef = "CZ1";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CCN001002";

			var shipment1 = creator.CreateShipment("S001001", consol);
			shipment1.JS_HouseBill = "123456";
			var packedContainer1 = shipment1.OuterPackLines.AddNew();
			packedContainer1.JL_RefNumber = "PCK001002";
			var job1 = creator.CreateJob(shipment1, false);

			var shipment2 = creator.CreateShipment("S001002", consol);
			shipment2.JS_HouseBill = "234567";
			var packedContainer2 = shipment2.OuterPackLines.AddNew();
			packedContainer2.JL_RefNumber = "PCK001003";
			var job2 = creator.CreateJob(shipment2, false);

			var carrierBookingReference = shipment1.Numbers.AddNew();
			carrierBookingReference.CE_EntryType = "BKG";
			carrierBookingReference.CE_EntryNum = "BKG001002";
			carrierBookingReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var agentReference = shipment1.Numbers.AddNew();
			agentReference.CE_EntryType = "OAG";
			agentReference.CE_EntryNum = "OAG001002";
			agentReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);

			var invoiceLine1 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine1.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1, job1, creator.FRT);

			var invoiceLine2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine2.AL_JH = job2.PK;
			creator.CreateJobCharge(invoiceLine2, job2, creator.FRT);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			NettingReceivableTransaction arTransaction = null;
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Factory.Save();
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction)); //simulate duplicate EDIMessage
			Factory.Save();

			var arNettingTransaction = arTransaction;
			AssertNotNull("Transaction should be created", arNettingTransaction);
			AssertEquals("Transaction period is next period, as current period is closed", nextPeriod.PK, arNettingTransaction.NRT_NSP_Period);

			var queryLine1 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			queryLine1.AddToFilter(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference, "S001001");
			var receivableLine1 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine1);
			AssertNotNull(receivableLine1);
			AssertEquals("Transaction period is next period, as current period is closed", nextPeriod.PK, receivableLine1.NRL_NSP_Period);

			var queryLine1Ref = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, receivableLine1.PK);
			queryLine1Ref.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var receivableRefLine1 = Factory.Load<NettingReceivableLineReference>(queryLine1Ref);
			AssertEquals("6 line references should be found", 6, receivableRefLine1.Length);
			foreach (NettingReceivableLineReference lineRef in receivableRefLine1)
			{
				AssertEquals("Transaction period is next period, as current period is closed", nextPeriod.PK, lineRef.NR1_NSP_Period);
			}

			var queryLine2 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			queryLine2.AddToFilter(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference, "S001002");
			var receivableLine2 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine2);
			AssertNotNull(receivableLine2);
			AssertEquals("Transaction period is next period, as current period is closed", nextPeriod.PK, receivableLine2.NRL_NSP_Period);

			var queryLine2Ref = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, receivableLine2.PK);
			queryLine2Ref.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var receivableRefLine2 = Factory.Load<NettingReceivableLineReference>(queryLine2Ref);
			AssertEquals("4 line references should be found", 4, receivableRefLine2.Length);
			foreach (NettingReceivableLineReference lineRef in receivableRefLine1)
			{
				AssertEquals("Transaction period is next period, as current period is closed", nextPeriod.PK, lineRef.NR1_NSP_Period);
			}

			//import will create ar transaction entries but will only create ap header entries
			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);
			AssertNotNull(apNettingTransaction);
			AssertEquals("Transaction period is next period, as current period is closed", nextPeriod.PK, apNettingTransaction.NPT_NSP_Period);

			queryLine1 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine1.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001001");
			var payableLine1 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine1);
			AssertNotNull(payableLine1);

			queryLine2 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine2.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001002");
			var payableLine2 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine2);
			AssertNotNull(payableLine2);

			//Will import AP this time, from the receiver
			message = GetMessage(receiverEHubID, nettingSystemEHubID);

			invoice = creator.CreateInvoice(typeof(APInvoice), "00001000", creator.AUD, 1M, sender.Organisation);

			invoiceLine1 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine1.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1, job1, creator.FRT);

			invoiceLine2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine2.AL_JH = job2.PK;
			creator.CreateJobCharge(invoiceLine2, job2, creator.FRT);

			Factory.Save();

			writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			universalTransaction = writer.GetDataObject(invoice);

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Factory.Save();
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction)); //simulate duplicate EDIMessage
			Factory.Save();

			queryLine1 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine1.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001001");
			payableLine1 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine1);
			AssertNotNull(payableLine1);
			AssertEquals("Transaction period is next period, as current period is closed", nextPeriod.PK, payableLine1.NPL_NSP_Period);

			queryLine1Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, payableLine1.PK);
			queryLine1Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var payableRefLine1 = Factory.Load<NettingPayableLineReference>(queryLine1Ref);
			AssertEquals("6 line references should be found", 6, payableRefLine1.Length);
			foreach (NettingPayableLineReference lineRef in payableRefLine1)
			{
				AssertEquals("Transaction period is next period, as current period is closed", nextPeriod.PK, lineRef.NP1_NSP_Period);
			}

			queryLine2 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine2.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001002");
			payableLine2 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine2);
			AssertNotNull(payableLine2);
			AssertEquals("Transaction period is next period, as current period is closed", nextPeriod.PK, payableLine2.NPL_NSP_Period);

			queryLine2Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, payableLine2.PK);
			queryLine2Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var refLine2 = Factory.Load<NettingPayableLineReference>(queryLine2Ref);
			AssertNotNull(refLine2);
			AssertEquals("4 line references should be found", 4, refLine2.Length);
		}

		[TestDate(2014, 12, 15)]
		public void TestImportAPNettingSystem_UpdateExistingPayable_LatestApprovalDateHasElapsed()
		{
			period.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(-2); //latest approval date already elapsed
			period.NSP_IsComplete = false;

			var nextPeriod = SetupNextNettingPeriod(ns, period);

			Factory.Save();

			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var consol = creator.CreateConsol("AUSYD", "NZAKL", "C001002");
			consol.JK_UniqueConsignRef = "CZ1";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "CCN001002";

			var shipment1 = creator.CreateShipment("S001001", consol);
			shipment1.JS_HouseBill = "123456";
			var packedContainer1 = shipment1.OuterPackLines.AddNew();
			packedContainer1.JL_RefNumber = "PCK001002";
			var job1 = creator.CreateJob(shipment1, false);

			var shipment2 = creator.CreateShipment("S001002", consol);
			shipment2.JS_HouseBill = "234567";
			var packedContainer2 = shipment2.OuterPackLines.AddNew();
			packedContainer2.JL_RefNumber = "PCK001003";
			var job2 = creator.CreateJob(shipment2, false);

			var carrierBookingReference = shipment1.Numbers.AddNew();
			carrierBookingReference.CE_EntryType = "BKG";
			carrierBookingReference.CE_EntryNum = "BKG001002";
			carrierBookingReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var agentReference = shipment1.Numbers.AddNew();
			agentReference.CE_EntryType = "OAG";
			agentReference.CE_EntryNum = "OAG001002";
			agentReference.CE_Category = CusEntryNumber.Categories.AdditionalReferenceNumber;

			var invoice = creator.CreateInvoice(typeof(ARInvoice), "00001000", creator.AUD, 1M, receiver.Organisation);

			var invoiceLine1 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine1.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1, job1, creator.FRT);

			var invoiceLine2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine2.AL_JH = job2.PK;
			creator.CreateJobCharge(invoiceLine2, job2, creator.FRT);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			NettingReceivableTransaction arTransaction = null;
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Factory.Save();
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction)); //simulate duplicate EDIMessage
			Factory.Save();

			var arNettingTransaction = arTransaction;
			AssertNotNull("Transaction should be created", arNettingTransaction);
			AssertEquals("Transaction period is current period, as due date falls within current period range", period.PK, arNettingTransaction.NRT_NSP_Period);

			var queryLine1 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			queryLine1.AddToFilter(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference, "S001001");
			var receivableLine1 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine1);
			AssertNotNull(receivableLine1);
			AssertEquals("Transaction period is current period, as due date falls within current period range", period.PK, receivableLine1.NRL_NSP_Period);

			var queryLine1Ref = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, receivableLine1.PK);
			queryLine1Ref.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var receivableRefLine1 = Factory.Load<NettingReceivableLineReference>(queryLine1Ref);
			AssertEquals("6 line references should be found", 6, receivableRefLine1.Length);
			foreach (NettingReceivableLineReference lineRef in receivableRefLine1)
			{
				AssertEquals("Transaction period is current period, as due date falls within current period range", period.PK, lineRef.NR1_NSP_Period);
			}

			var queryLine2 = new ZQuery(NettingReceivableTransactionLineSchema.NRL_NRT_Transaction, arNettingTransaction.PK);
			queryLine2.AddToFilter(NettingReceivableTransactionLineSchema.NRL_PrimaryJobReference, "S001002");
			var receivableLine2 = Factory.LoadTop1<NettingReceivableTransactionLine>(queryLine2);
			AssertNotNull(receivableLine2);
			AssertEquals("Transaction period is current period, as due date falls within current period range", period.PK, receivableLine2.NRL_NSP_Period);

			var queryLine2Ref = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, receivableLine2.PK);
			queryLine2Ref.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var receivableRefLine2 = Factory.Load<NettingReceivableLineReference>(queryLine2Ref);
			AssertEquals("4 line references should be found", 4, receivableRefLine2.Length);

			//import will create ar transaction entries but will only create ap header entries
			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);
			AssertNotNull(apNettingTransaction);
			AssertEquals("Transaction period is next period, as latest approval date for current period has elapsed", nextPeriod.PK, apNettingTransaction.NPT_NSP_Period);

			queryLine1 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine1.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001001");
			var payableLine1 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine1);
			AssertNotNull(payableLine1);

			queryLine2 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine2.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001002");
			var payableLine2 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine2);
			AssertNotNull(payableLine2);

			//Will import AP this time, from the receiver
			message = GetMessage(receiverEHubID, nettingSystemEHubID);

			invoice = creator.CreateInvoice(typeof(APInvoice), "00001000", creator.AUD, 1M, sender.Organisation);

			invoiceLine1 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine1.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine1, job1, creator.FRT);

			invoiceLine2 = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine2.AL_JH = job2.PK;
			creator.CreateJobCharge(invoiceLine2, job2, creator.FRT);

			Factory.Save();

			writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			universalTransaction = writer.GetDataObject(invoice);

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));
			Factory.Save();
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction)); //simulate duplicate EDIMessage
			Factory.Save();

			queryLine1 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine1.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001001");
			payableLine1 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine1);
			AssertNotNull(payableLine1);
			AssertEquals("Transaction period is next period, as latest approval date for current period has elapsed", nextPeriod.PK, payableLine1.NPL_NSP_Period);

			queryLine1Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, payableLine1.PK);
			queryLine1Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.CarrierBookingReference, AccountingConstants.TransactionReferenceTypes.AgentReference, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var payableRefLine1 = Factory.Load<NettingPayableLineReference>(queryLine1Ref);
			AssertEquals("6 line references should be found", 6, payableRefLine1.Length);
			foreach (NettingPayableLineReference lineRef in payableRefLine1)
			{
				AssertEquals("Transaction period is next period, as latest approval date for current period has elapsed", nextPeriod.PK, lineRef.NP1_NSP_Period);
			}

			queryLine2 = new ZQuery(NettingPayableTransactionLineSchema.NPL_NPT_Transaction, importer.APNettingTransaction_ForTestOnly.PK);
			queryLine2.AddToFilter(NettingPayableTransactionLineSchema.NPL_PrimaryJobReference, "S001002");
			payableLine2 = Factory.LoadTop1<NettingPayableTransactionLine>(queryLine2);
			AssertNotNull(payableLine2);
			AssertEquals("Transaction period is next period, as latest approval date for current period has elapsed", nextPeriod.PK, payableLine2.NPL_NSP_Period);

			queryLine2Ref = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, payableLine2.PK);
			queryLine2Ref.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, new ZString[] { AccountingConstants.TransactionReferenceTypes.ShipmentNumber, AccountingConstants.TransactionReferenceTypes.PackedContainer, AccountingConstants.TransactionReferenceTypes.HouseBill, AccountingConstants.TransactionReferenceTypes.ConsolNumber });
			var refLine2 = Factory.Load<NettingPayableLineReference>(queryLine2Ref);
			AssertNotNull(refLine2);
			AssertEquals("4 line references should be found", 4, refLine2.Length);
		}

		[TestDate(2018, 12, 15)]
		public void TestImportAPWithoutPeriods()
		{
			//Arrange
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAP();
			period.NSP_IsComplete = true;
			Factory.Save();

			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var isImported = false;

			//Act
			AssertNoExceptionThrown(() => isImported = importer.ImportNettingTransaction(message, universalTransaction));

			//Assert
			AssertEquals(false, isImported);
			AssertContains("Error - No Open Netting Period found for Netting System 'EDIWNS001' at UTC date:", logger.Logs);
			AssertImportTransactionDeleted(importer, false);
		}

		public void TestImportAR_WithCommentInvoiceLine()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAR();
			var currency = new Currency();
			currency.Code = "AUD";
			currency.Description = "Australian Dollar";
			var postingJournal = new PostingJournal
			{
				Job = new EntityReference() { Type = "Job", Key = "S001001" },
				OSCurrency = currency,
				ChargeCode = new ChargeCode() { Code = "ZZCMT" }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { postingJournal });

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arTransaction = null;
			Assert(!importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));

			AssertNull("Transaction should not be created", arTransaction);
		}

		public void TestImportAR_WithCommentAndNormalInvoiceLine()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAR();
			var currency = new Currency();
			currency.Code = "AUD";
			currency.Description = "Australian Dollar";
			var postingJournalComment = new PostingJournal
			{
				Job = new EntityReference() { Type = "Job", Key = "S001001" },
				OSCurrency = currency,
				ChargeCode = new ChargeCode() { Code = "ZZCMT" }
			};
			var postingJournalFreight = new PostingJournal
			{
				Job = new EntityReference() { Type = "Job", Key = "S001001" },
				OSCurrency = currency,
				ChargeCode = new ChargeCode() { Code = "FRT" },
				OSTotalAmount = 120M
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { postingJournalComment, postingJournalFreight });

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			NettingReceivableTransaction arTransaction = null;
			Assert(importer.ImportNettingTransaction(message, universalTransaction, ref arTransaction));

			AssertNotNull("Transaction should be created", arTransaction);
			AssertEquals("Transaction type", TransactionTypes.Invoice, arTransaction.TransactionType);
		}

		public void TestImportAP_WithCommentInvoiceLine()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAP();
			var currency = new Currency();
			currency.Code = "AUD";
			currency.Description = "Australian Dollar";
			var postingJournal = new PostingJournal
			{
				Job = new EntityReference() { Type = "Job", Key = "S001001" },
				OSCurrency = currency,
				ChargeCode = new ChargeCode() { Code = "ZZCMT" }
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { postingJournal });

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());

			Assert(!importer.ImportNettingTransaction(message, universalTransaction));

			AssertNull("Transaction should not be created", importer.APNettingTransaction_ForTestOnly);
		}

		public void TestImportAP_WithCommentAndNormalInvoiceLine()
		{
			var message = GetMessage(senderEHubID, nettingSystemEHubID);

			var universalTransaction = GetUniversalTransactionForAP();
			var currency = new Currency();
			currency.Code = "AUD";
			currency.Description = "Australian Dollar";
			var postingJournalComment = new PostingJournal
			{
				Job = new EntityReference() { Type = "Job", Key = "S001001" },
				OSCurrency = currency,
				ChargeCode = new ChargeCode() { Code = "ZZCMT" }
			};
			var postingJournalFreight = new PostingJournal
			{
				Job = new EntityReference() { Type = "Job", Key = "S001001" },
				OSCurrency = currency,
				ChargeCode = new ChargeCode() { Code = "FRT" },
				OSTotalAmount = 120M
			};
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { postingJournalComment, postingJournalFreight });

			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());

			Assert(importer.ImportNettingTransaction(message, universalTransaction));

			NettingPayableTransaction apTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);

			AssertNotNull("Transaction should be created", apTransaction);
			AssertEquals("Transaction type", TransactionTypes.Invoice, apTransaction.TransactionType);
		}

		[TestDate(2019, 11, 15)]
		public void TestLinesAndReferencesGetOverwrittenByNewImportForSameInvoice()
		{
			var ediMessage = GetMessage(senderEHubID, nettingSystemEHubID);

			var consolNumber = "C001001";
			var consol = creator.CreateConsol("AUSYD", "NZAKL", consolNumber);
			consol.JK_UniqueConsignRef = consolNumber;

			var shipmentNumber = "S001001";
			var houseBillNumber = "123456";
			var shipment = creator.CreateShipment(shipmentNumber, consol);
			shipment.JS_HouseBill = houseBillNumber;
			var job1 = creator.CreateJob(shipment, false);

			var invoiceNumber = "00001000";
			var invoice = creator.CreateInvoice(typeof(ARInvoice), invoiceNumber, creator.AUD, 1M, receiver.Organisation);
			var invoiceLine = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine, job1, creator.FRT);

			Factory.Save();

			var writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			var universalTransaction = writer.GetDataObject(invoice);

			NettingReceivableTransaction arTransaction = null;
			var importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(ediMessage, universalTransaction, ref arTransaction));

			Factory.Save();

			var arNettingTransaction = arTransaction;
			var apNettingTransaction = (NettingPayableTransaction)importer.APNettingTransaction_ForTestOnly;

			AssertNotNull("AR Transaction should be created", arNettingTransaction);
			AssertNotNull("AP Transaction should be created", apNettingTransaction);

			var masterbillNumber = "MWB001";
			creator.AddNettingTransactionReference(arNettingTransaction, AccountingConstants.TransactionReferenceTypes.MasterBill, masterbillNumber);
			creator.AddNettingTransactionReference(apNettingTransaction, AccountingConstants.TransactionReferenceTypes.MasterBill, masterbillNumber);
			Factory.Save();

			AssertReceivableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber, invoiceNumber);
			AssertReceivableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertReceivableTransactionReference("Reference added manually, to simulate this reference will be deleted by next import of the same UXML", true, AccountingConstants.TransactionReferenceTypes.MasterBill, masterbillNumber);

			AssertPayableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber, invoiceNumber);
			AssertPayableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertPayableTransactionReference("Reference added manually, to simulate this reference will be deleted by next import of the same UXML", true, AccountingConstants.TransactionReferenceTypes.MasterBill, masterbillNumber);

			AssertEquals("1 line should be created for the AR transaction", 1, arNettingTransaction.Lines.Count);
			var arLine = arNettingTransaction.Lines[0];
			AssertEquals(shipmentNumber, arLine.NRL_PrimaryJobReference);

			AssertEquals("1 line should be created for the AP transaction", 1, apNettingTransaction.Lines.Count);
			var apLine = apNettingTransaction.Lines[0];
			AssertEquals(shipmentNumber, apLine.NPL_PrimaryJobReference);

			var orderReferenceNumber = "ORD123";
			creator.AddNettingLineReference(arLine, AccountingConstants.TransactionReferenceTypes.OrderReferences, orderReferenceNumber);
			creator.AddNettingLineReference(apLine, AccountingConstants.TransactionReferenceTypes.OrderReferences, orderReferenceNumber);
			Factory.Save();

			AssertReceivableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertReceivableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, shipmentNumber);
			AssertReceivableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.HouseBill, houseBillNumber);
			AssertReceivableTransactionLineReference("Reference added manually, to simulate this reference will be deleted by next import of the same UXML", true, AccountingConstants.TransactionReferenceTypes.OrderReferences, orderReferenceNumber);

			AssertPayableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertPayableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, shipmentNumber);
			AssertPayableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.HouseBill, houseBillNumber);
			AssertPayableTransactionLineReference("Reference added manually, to simulate this reference will be deleted by next import of the same UXML", true, AccountingConstants.TransactionReferenceTypes.OrderReferences, orderReferenceNumber);

			writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			universalTransaction = writer.GetDataObject(invoice);

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(ediMessage, universalTransaction, ref arTransaction));

			Factory.Save();

			arNettingTransaction = arTransaction;
			apNettingTransaction = (NettingPayableTransaction)importer.APNettingTransaction_ForTestOnly;

			AssertNotNull("AR Transaction should be created", arNettingTransaction);
			AssertNotNull("AP Transaction should be created", apNettingTransaction);

			AssertReceivableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber, invoiceNumber);
			AssertReceivableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertReceivableTransactionReference("Reference added in previous import is discarded after new import", false, AccountingConstants.TransactionReferenceTypes.MasterBill, masterbillNumber);

			AssertPayableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber, invoiceNumber);
			AssertPayableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertPayableTransactionReference("Reference for AP is only added if the AP transaction did not exist before", true, AccountingConstants.TransactionReferenceTypes.MasterBill, masterbillNumber);

			AssertEquals("1 line should be created for the AR transaction", 1, arNettingTransaction.Lines.Count);
			arLine = arNettingTransaction.Lines[0];
			AssertEquals(shipmentNumber, arLine.NRL_PrimaryJobReference);

			AssertEquals("1 line should be created for the AP transaction", 1, apNettingTransaction.Lines.Count);
			apLine = apNettingTransaction.Lines[0];
			AssertEquals(shipmentNumber, apLine.NPL_PrimaryJobReference);

			AssertReceivableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertReceivableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, shipmentNumber);
			AssertReceivableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.HouseBill, houseBillNumber);
			AssertReceivableTransactionLineReference("Reference added in previous import is discarded after new import", false, AccountingConstants.TransactionReferenceTypes.OrderReferences, orderReferenceNumber);

			AssertPayableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertPayableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, shipmentNumber);
			AssertPayableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.HouseBill, houseBillNumber);
			AssertPayableTransactionLineReference("Reference for AP is only added if the AP transaction did not exist before", true, AccountingConstants.TransactionReferenceTypes.OrderReferences, orderReferenceNumber);

			//Will import AP this time, from the receiver
			ediMessage = GetMessage(receiverEHubID, nettingSystemEHubID);

			invoice = creator.CreateInvoice(typeof(APInvoice), invoiceNumber, creator.AUD, 1M, sender.Organisation);
			invoiceLine = creator.CreateInvoiceLine(invoice, creator.AUD, 1M, 60M, 0M, 0M, 60M, 0M, 0M, creator.FRT.PK);
			invoiceLine.AL_JH = job1.PK;
			creator.CreateJobCharge(invoiceLine, job1, creator.FRT);

			Factory.Save();
			writer = new TransactionDataObjectWriter(new DataWritingManager(new ActionInfo(RecipientRoleType.IDB, invoice)));
			universalTransaction = writer.GetDataObject(invoice);
			universalTransaction.BranchAddress.OrganizationCode = "ZTSTRCV1";
			universalTransaction.OrganizationAddress.OrganizationCode = "EDICUS";

			importer = new NettingTransactionImporter(Factory, new TestErrorLogger());
			Assert(importer.ImportNettingTransaction(ediMessage, universalTransaction));
			Factory.Save();

			AssertPayableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.InvoiceTransactionNumber, invoiceNumber);
			AssertPayableTransactionReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertPayableTransactionReference("Reference added in previous import is discarded after new import", false, AccountingConstants.TransactionReferenceTypes.MasterBill, masterbillNumber);

			AssertEquals("1 line should be created for the AP transaction", 1, apNettingTransaction.Lines.Count);
			apLine = apNettingTransaction.Lines[0];
			AssertEquals(shipmentNumber, apLine.NPL_PrimaryJobReference);

			AssertPayableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ConsolNumber, consolNumber);
			AssertPayableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.ShipmentNumber, shipmentNumber);
			AssertPayableTransactionLineReference("Reference added from UXML import", true, AccountingConstants.TransactionReferenceTypes.HouseBill, houseBillNumber);
			AssertPayableTransactionLineReference("Reference added in previous import is discarded after new import", false, AccountingConstants.TransactionReferenceTypes.OrderReferences, orderReferenceNumber);

			void AssertReceivableTransactionReference(ZString message, bool shouldExist, ZString type, ZString reference)
			{
				var query = new ZQuery(NettingReceivableTransactionRefSchema.NRR_NRT_Transaction, arNettingTransaction.PK);
				query.AddToFilter(NettingReceivableTransactionRefSchema.NRR_Type, type);
				query.AddToFilter(NettingReceivableTransactionRefSchema.NRR_Reference, reference);

				var result = Factory.Exists(typeof(NettingReceivableTransactionRef), query);

				AssertEquals(message, shouldExist, result);
			}

			void AssertPayableTransactionReference(ZString message, bool shouldExist, ZString type, ZString reference)
			{
				var query = new ZQuery(NettingPayableTransactionRefSchema.NPR_NPT_Transaction, apNettingTransaction.PK);
				query.AddToFilter(NettingPayableTransactionRefSchema.NPR_Type, type);
				query.AddToFilter(NettingPayableTransactionRefSchema.NPR_Reference, reference);

				var result = Factory.Exists(typeof(NettingPayableTransactionRef), query);

				AssertEquals(message, shouldExist, result);
			}

			void AssertReceivableTransactionLineReference(ZString message, bool shouldExist, ZString type, ZString reference)
			{
				var query = new ZQuery(NettingReceivableLineReferenceSchema.NR1_NRL_Line, arLine.PK);
				query.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Type, type);
				query.AddToFilter(NettingReceivableLineReferenceSchema.NR1_Reference, reference);

				var result = Factory.Exists(typeof(NettingReceivableLineReference), query);

				AssertEquals(message, shouldExist, result);
			}

			void AssertPayableTransactionLineReference(ZString message, bool shouldExist, ZString type, ZString reference)
			{
				var query = new ZQuery(NettingPayableLineReferenceSchema.NP1_NPL_Line, apLine.PK);
				query.AddToFilter(NettingPayableLineReferenceSchema.NP1_Type, type);
				query.AddToFilter(NettingPayableLineReferenceSchema.NP1_Reference, reference);

				var result = Factory.Exists(typeof(NettingPayableLineReference), query);

				AssertEquals(message, shouldExist, result);
			}
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAP_LogsIssuerAndRecipient()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAP();

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Information - Netting Participant ZTSTRCV1 matched for organization address", logger.Logs);
			AssertContains("Information - AP Netting Transaction 00001000: Issuer ZTSTRCV1 and Recipient EDICUS", logger.Logs);

			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);
			AssertNotNull("Transaction should be created", apNettingTransaction);
			AssertEquals("Issuer:", receiver.Organisation.OH_Code, apNettingTransaction.Issuer.Organisation.OH_Code);
			AssertEquals("Recipient:", GlbBranch.CurrentBranch.OrgProxy.OH_Code, apNettingTransaction.Recipient.Organisation.OH_Code);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAP_NoBranchAddressInTransaction_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAP();
			universalTransaction.Branch = null;
			universalTransaction.BranchAddress = null;

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Error - No address was provided for the branch address in this Universal Transaction. Netting cannot continue.", logger.Logs);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAP_NoBranchNettingParticipant_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAP();
			universalTransaction.Branch.Code = "ABIGAS";
			universalTransaction.BranchAddress.OrganizationCode = "ABIGAS";

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Error - No Netting Participant found for branch (ABIGAS).", logger.Logs);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAP_NoBranchOrganisationFound_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAP();
			universalTransaction.BranchAddress = new OrganizationAddress
			{
				Address1 = "404 Missing Rd",
				City = "City",
				Country = new Country { Code = Core.Constants.CountryCodes.Bermuda },
				Postcode = "PostCode",
				Port = new UNLOCO { Code = "BM404" },
				OrganizationCode = "NOORG",
				State = "State",
			};

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			var expectedError = @"Error - No active organization can be found for matching branch address (NOORG).
404 Missing Rd, City, State, PostCode, BM (BM404).";
			AssertContains(expectedError, logger.Logs);
		}

		[TestDate(2018, 12, 15)]
		public void TestImportAP_NoNettingSystem_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, "NONET");
			var universalTransaction = GetUniversalTransactionForAP();

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Information - Netting Participant ZTSTRCV1 matched for organization address", logger.Logs);
			AssertContains("Information - AP Netting Transaction 00001000: Issuer ZTSTRCV1 and Recipient EDICUS", logger.Logs);
			AssertContains("Error - No Netting System is setup for eHub ID: 'NONET'", logger.Logs);
			AssertImportTransactionDeleted(importer);
			AssertNull(importer.ARNettingTransaction_ForTestOnly);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAP_NoOrgAddressInTransaction_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAP();
			universalTransaction.OrganizationAddress = null;

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Pre-condition", "Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Error - No address was provided for the organization address in this Universal Transaction. Netting cannot continue.", logger.Logs);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAP_NoOrgAddressNettingParticipant_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAP();
			universalTransaction.OrganizationAddress = new OrganizationAddress { OrganizationCode = "ABIGAS" };

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Pre-condition", "Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Error - No Netting Participant found for organization (ABIGAS).", logger.Logs);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAP_NoOrgAddressOrganisationFound_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAP();
			universalTransaction.OrganizationAddress = new OrganizationAddress
			{
				Address1 = "404 Missing Rd",
				City = "City",
				Country = new Country { Code = Core.Constants.CountryCodes.Bermuda },
				Postcode = "PostCode",
				Port = new UNLOCO { Code = "BM404" },
				OrganizationCode = "NOORG",
				State = "State",
			};

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Pre-condition", "Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			var expectedError = @"Error - No active organization can be found for matching organization address (NOORG).
404 Missing Rd, City, State, PostCode, BM (BM404).";
			AssertContains(expectedError, logger.Logs);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAR_LogsIssuerAndRecipient()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAR();

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Information - Netting Participant ZTSTRCV1 matched for organization address", logger.Logs);
			AssertContains("Information - AR Netting Transaction 00001000: Issuer EDICUS and Recipient ZTSTRCV1", logger.Logs);

			var apNettingTransaction = Factory.Load<NettingPayableTransaction>(importer.APNettingTransaction_ForTestOnly.PK);
			AssertNotNull("Transaction should be created", apNettingTransaction);
			AssertEquals("Issuer:", GlbBranch.CurrentBranch.OrgProxy.OH_Code, apNettingTransaction.Issuer.Organisation.OH_Code);
			AssertEquals("Recipient:", receiver.Organisation.OH_Code, apNettingTransaction.Recipient.Organisation.OH_Code);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAR_NoBranchAddressInTransaction_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.Branch = null;
			universalTransaction.BranchAddress = null;

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Error - No address was provided for the branch address in this Universal Transaction. Netting cannot continue.", logger.Logs);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAR_NoBranchNettingParticipant_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.Branch.Code = "ABIGAS";
			universalTransaction.BranchAddress.OrganizationCode = "ABIGAS";

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Error - No Netting Participant found for branch (ABIGAS).", logger.Logs);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAR_NoBranchOrganisationFound_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.BranchAddress = new OrganizationAddress
			{
				Address1 = "404 Missing Rd",
				City = "City",
				Country = new Country { Code = Core.Constants.CountryCodes.Bermuda },
				Postcode = "PostCode",
				Port = new UNLOCO { Code = "BM404" },
				OrganizationCode = "NOORG",
				State = "State",
			};

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			var expectedError = @"Error - No active organization can be found for matching branch address (NOORG).
404 Missing Rd, City, State, PostCode, BM (BM404).";
			AssertContains(expectedError, logger.Logs);
		}

		[TestDate(2018, 12, 15)]
		public void TestImportAR_NoNettingSystem_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, "NONET");
			var universalTransaction = GetUniversalTransactionForAR();

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Information - Netting Participant ZTSTRCV1 matched for organization address", logger.Logs);
			AssertContains("Information - AR Netting Transaction 00001000: Issuer EDICUS and Recipient ZTSTRCV1", logger.Logs);
			AssertContains("Error - No Netting System is setup for eHub ID: 'NONET'", logger.Logs);
			AssertImportTransactionDeleted(importer);
			AssertNull(importer.APNettingTransaction_ForTestOnly);
			AssertNull(importer.ARNettingTransaction_ForTestOnly);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAR_NoOrgAddressInTransaction_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.OrganizationAddress = null;

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Pre-condition", "Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Error - No address was provided for the organization address in this Universal Transaction. Netting cannot continue.", logger.Logs);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAR_NoOrgAddressNettingParticipant_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.OrganizationAddress = new OrganizationAddress { OrganizationCode = "ABIGAS" };

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Pre-condition", "Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			AssertContains("Error - No Netting Participant found for organization (ABIGAS).", logger.Logs);
		}

		[TestDate(2023, 01, 10)]
		public void TestImportAR_NoOrgAddressOrganisationFound_LogsError()
		{
			// Arrange
			var logger = new TestErrorLogger();
			var importer = new NettingTransactionImporter(Factory, logger);
			var message = GetMessage(senderEHubID, nettingSystemEHubID);
			var universalTransaction = GetUniversalTransactionForAR();
			universalTransaction.OrganizationAddress = new OrganizationAddress
			{
				Address1 = "404 Missing Rd",
				City = "City",
				Country = new Country { Code = Core.Constants.CountryCodes.Bermuda },
				Postcode = "PostCode",
				Port = new UNLOCO { Code = "BM404" },
				OrganizationCode = "NOORG",
				State = "State",
			};

			// Act
			importer.ImportNettingTransaction(message, universalTransaction);

			// Assert
			AssertContains("Pre-condition", "Information - Netting Participant EDICUS matched for branch address", logger.Logs);
			var expectedError = @"Error - No active organization can be found for matching organization address (NOORG).
404 Missing Rd, City, State, PostCode, BM (BM404).";
			AssertContains(expectedError, logger.Logs);
		}

		#region Implementation

		TransactionInfo GetUniversalTransactionForAR() => GetUniversalTransaction(true);

		TransactionInfo GetUniversalTransactionForAP() => GetUniversalTransaction(false);

		TransactionInfo GetUniversalTransaction(bool isAR)
		{
			var jobNumber = "00001000";
			var amount = isAR ? 150m : -120m;
			var currency = new Currency { Code = Core.Constants.CurrencyCodes.Australia };

			var universalTransaction = new TransactionInfo(DefaultDataObjectWriterStrategy.TestInstance)
			{
				Branch = new Branch { Code = "EDICUS" },
				BranchAddress = new OrganizationAddress { OrganizationCode = "EDICUS" },
				Department = new Department { Code = Env.CurrentDepartment.Code },
				Description = "Test Netting Invoice",
				DueDate = ZDateTime.Now,
				ExchangeRate = 1,
				Job = new EntityReference { Key = jobNumber, Type = "Job" },
				JobInvoiceNumber = jobNumber,
				Ledger = isAR ? LedgerTypes.AccountsReceivable : LedgerTypes.AccountsPayable,
				LocalCurrency = currency,
				Number = jobNumber,
				OSCurrency = currency,
				OSTotal = amount,
				OrganizationAddress = new OrganizationAddress { OrganizationCode = "ZTSTRCV1" },
				TransactionType = TransactionType.INV
			};
			var postingJournal = new PostingJournal { OSTotalAmount = amount };
			universalTransaction.SetPostingJournalCollection(() => new List<PostingJournal> { postingJournal });
			universalTransaction.SetShipmentCollection(() => new List<Shipment>());

			return universalTransaction;
		}

		IEDIMessage GetMessage(string sender, string receiver)
		{
			var newFactory = new BusinessObjectFactory();
			var message = newFactory.New<IEDIMessage>();

			var interchange = newFactory.New<IEDIInterchange>();
			interchange.EI_From = sender;
			interchange.EI_To = receiver;

			message.EM_EI = interchange.PK;

			return message;
		}

		NettingSystemPeriod SetupNettingPeriod(NettingSystem ns)
		{
			var nsp = Factory.New<NettingSystemPeriod>();
			nsp.NSP_Period = "234234";
			nsp.NSP_NS_NettingSystem = ns.PK;
			nsp.NSP_EarliestInvoiceDateUtc = ZDateTime.Now.AddDays(-15);
			nsp.NSP_LatestInvoiceDateUtc = ZDateTime.Now.AddDays(15);
			nsp.NSP_NettingExecutionDateUtc = ZDateTime.Now.AddDays(8);
			nsp.NSP_LatestApprovalDateUtc = ZDateTime.Now.AddDays(13);

			nsp.NSP_LatestFXOfferDateUtc = ZDateTime.Now.AddDays(8);
			nsp.NSP_LatestUploadDateUtc = ZDateTime.Now.AddDays(8);

			nsp.NSP_ValueDate = ZDate.Today.AddDays(8);
			nsp.NSP_OfferPrepaymentDate = ZDate.Today.AddDays(8);

			return nsp;
		}

		NettingSystemPeriod SetupNextNettingPeriod(NettingSystem ns, NettingSystemPeriod currentPeriod, string nSP_Period = "23423423")
		{
			var nsp = Factory.New<NettingSystemPeriod>();
			nsp.NSP_Period = nSP_Period;
			nsp.NSP_NS_NettingSystem = ns.PK;
			nsp.NSP_EarliestInvoiceDateUtc = currentPeriod.NSP_EarliestInvoiceDateUtc.AddDays(30);
			nsp.NSP_LatestInvoiceDateUtc = currentPeriod.NSP_LatestInvoiceDateUtc.AddDays(30);
			nsp.NSP_NettingExecutionDateUtc = currentPeriod.NSP_NettingExecutionDateUtc.AddDays(30);

			nsp.NSP_LatestApprovalDateUtc = currentPeriod.NSP_LatestApprovalDateUtc.AddDays(30);
			nsp.NSP_LatestFXOfferDateUtc = currentPeriod.NSP_LatestFXOfferDateUtc.AddDays(30);
			nsp.NSP_LatestUploadDateUtc = currentPeriod.NSP_LatestUploadDateUtc.AddDays(30);

			nsp.NSP_ValueDate = currentPeriod.NSP_ValueDate.AddDays(8);
			nsp.NSP_OfferPrepaymentDate = currentPeriod.NSP_OfferPrepaymentDate.AddDays(8);

			return nsp;
		}

		NettingOrganisation SetupSender(NettingSystem ns, string senderEHubID)
		{
			var sender = Creator.CreateOrgHeader("TSTSND1", true, true);
			sender.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, senderEHubID);

			var sendingCompany = Creator.CreateNewCompany("SEN", orgProxy: sender);
			return Creator.CreateNettingOrganisation(ns, sender, "CUR");
		}

		NettingOrganisation SetupReceiver(NettingSystem ns, string eHubID)
		{
			var receiver = Creator.CreateOrgHeader("TSTRCV1", true, true);
			creator.AddEdiCommunication(receiver, EDICommunicationsMode.Modules.Netting, EDICommunicationsModeFileFormatList.Codes.XmlUniversalTransaction, EDICommunicationsModeCommunicationsTransportList.Codes.EHubService, ns.NS_Code);
			receiver.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubID);

			return Creator.CreateNettingOrganisation(ns, receiver, "CUR");
		}

		void AssertImportTransactionDeleted(NettingTransactionImporter importer, bool isAccountReceivable = true)
		{
			var transactionToTest = isAccountReceivable ? importer.ARNettingTransaction_ForTestOnly : importer.APNettingTransaction_ForTestOnly;
			if (transactionToTest != null)
			{
				AssertEquals("Enterprise.Messaging.Business.BaseMessageProcessor will force factory save in certain senarios. Delete the two transactions to avoid save error.", true, transactionToTest.IsDeleted);
			}
		}

		NettingObjectCreator Creator
		{
			get
			{
				return creator ?? (creator = new NettingObjectCreator(Factory));
			}
		}
		NettingObjectCreator creator;

		NettingSystem SetupNettingSystem(string eHubID)
		{
			var ns = Factory.New<NettingSystem>();
			ns.NS_Code = eHubID;
			ns.NS_GC = GlbCompany.CurrentCompany.PK;
			ns.NS_Description = "bla bla";

			var nettingSystem = Factory.Load<OrgHeader>(GlbBranch.CurrentBranch.GB_OH_OrgProxy);
			nettingSystem.SetLocalCustomsCode(OrgCusCode.CodeTypes.EHubOrganisationID, eHubID);

			var noNettingSystem = Factory.New<NettingOrganisation>();
			noNettingSystem.NSO_NS_NettingSystem = ns.PK;
			noNettingSystem.NSO_NettingType = "CUR";
			noNettingSystem.NSO_OH_Organisation = nettingSystem.PK;

			return ns;
		}

		NettingSystem ns;
		NettingSystemPeriod period;
		ZString nettingSystemEHubID;
		NettingOrganisation sender;
		ZString senderEHubID;
		NettingOrganisation receiver;
		ZString receiverEHubID;

		#endregion

		protected override void SetUp()
		{
			base.SetUp();

			SetupControlAccounts();

			nettingSystemEHubID = "EDIWNS001";
			ns = SetupNettingSystem(nettingSystemEHubID);

			receiverEHubID = "EDIRCV001";
			receiver = SetupReceiver(ns, receiverEHubID);

			senderEHubID = "EDISND001";
			sender = SetupSender(ns, senderEHubID);

			period = SetupNettingPeriod(ns);

			Factory.Save();
		}

		void SetupControlAccounts()
		{
			AccGLHeader aRSuspenseControlAccount = Creator.CreateARSuspenseControlAccount();
			AccGLHeader aPSuspenseControlAccount = Creator.CreateAPSuspenseControlAccount();
			AccGLHeader jobRevenueJournalControlAccount = Creator.CreateJobRevenueJournalControlAccount();
			AccGLHeader cFXAccount = Creator.CreateCFXAccount();
			Factory.Save();

			AccountingConfigurationRegistry.Instance.ARSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aRSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.APSuspenseControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, aPSuspenseControlAccount.PK.ToGuid());
			AccountingConfigurationRegistry.Instance.JobRevenueJournalControlAccount.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, jobRevenueJournalControlAccount.PK.ToGuid());
			GlbDepartment department = Factory.LoadTop1<GlbDepartment>(new ZQuery(GlbDepartmentSchema.GE_Code, "CES"));
			AccountingConfigurationRegistry.Instance.CFXAccount.SetValue(Guid.Empty, Guid.Empty, department.PK.ToGuid(), cFXAccount.PK.ToGuid());
		}
	}
}
