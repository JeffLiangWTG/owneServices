using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class IMDMultiMessageManagerTest : TestCaseWithFactory
	{
		public void TestNotificationCollection()
		{
			var declaration = JobDeclaration.New(Factory);
			var entry = declaration.CustomsEntryHeaders.AddNew();

			var testManager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.LodgeWithoutPay);
			var manager1 = new IMDMessageManager(entry, testManager);

			testManager.AllMessageManagersExposed = new Customs.Business.SingleMessageManager[] { manager1 };

			declaration.JobComInvoiceGroupHeaders[0].JobComInvoiceHeaders.AddNew();
			var line = declaration.FilteredInvoiceLines.AddNew();
			using (declaration.SuspendValidationTesting())
			{
				declaration.JE_OH_ImporterInfo.AddMessageError("You must enter an importer");
			}
			line.AddInfo.ZA_WRN = "abc";
			var cpDec = entry.Questions.AddNew();
			using (cpDec.SuspendValidationTesting())
			{
				cpDec.ON_AnswerInfo.AddMessageError("CPDec is not answered");
			}
			AssertEquals(@"Warehouse Reference Number (WRN): Warehouse reference number is only required for a nature 30.
Goods Origin: A country/region of origin is required.
Importer: You must enter an importer",
				testManager.Validation.MessageErrors.ToUniqueMessageListString().Replace("\n", "\r\n"));
		}

		public void TestNotificationCollection_ConsolidatedDeclaration()
		{
			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var declaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			var entryHeader = declaration.CustomsEntryHeaders[0];

			var line = (JobComInvoiceLine)declaration.Invoices.AddNew().InvoiceLines.AddNew();
			using (declaration.SuspendValidationTesting())
			{
				declaration.JE_OH_ImporterInfo.AddMessageError("You must enter an importer");
			}
			line.AddInfo.ZA_WRN = "abc";
			var cpDec = entryHeader.Questions.AddNew();
			using (cpDec.SuspendValidationTesting())
			{
				cpDec.ON_AnswerInfo.AddMessageError("CPDec is not answered");
			}

			var testManager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.LodgeWithoutPay);
			var testSingleManager = new IMDMessageManager(entryHeader, testManager);
			testManager.AllMessageManagersExposed = new Customs.Business.SingleMessageManager[] { testSingleManager };
			AssertEquals(@"Warehouse Reference Number (WRN): Warehouse reference number is only required for a nature 30.
Goods Origin: A country/region of origin is required.
Importer: You must enter an importer", testManager.Validation.MessageErrors.ToUniqueMessageListString().Replace("\n", "\r\n"));
		}

		public void TestGetManagersFromEntry()
		{
			var testDec = JobDeclaration.New(Factory);
			var entry1 = testDec.CustomsEntryHeaders.AddNew();
			var entry2 = testDec.CustomsEntryHeaders.AddNew();

			var testManager = new IMDMultiMessageManagerForTest(testDec, CMRMessageTypes.LodgeWithoutPay);
			var manager1 = new IMDMessageManager(entry1, testManager);
			var manager2 = new IMDMessageManager(entry2, testManager);

			testManager.AllMessageManagersExposed = new Customs.Business.SingleMessageManager[] { manager1, manager2 };

			var result = testManager.GetManagersFromEntryHeader(new CusEntryHeader[] { entry1 });
			AssertEquals("Result should have a manager for Entry1", 1, result.Length);
			AssertEquals("Result should have a manager for Entry1 and it should be the same instance. Base behaviour depends on the fact that it is the same behaviour for CanSave ManagersToExclude", manager1, result[0]);

			result = testManager.GetManagersFromEntryHeader(new CusEntryHeader[] { entry2 });
			AssertEquals("Result should have a manager for Entry2", 1, result.Length);
			AssertEquals("Result should have a manager for Entry2", manager2, result[0]);
		}

		public void TestSendWithdrawalMessagesWithPassedEntries()
		{
			var testDec = JobDeclaration.New(Factory);
			SetUpEntriesForAmendmentTesting(testDec);
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);

			var entry1 = testDec.CustomsEntryHeaders[0];
			entry1.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			var entry2 = testDec.CustomsEntryHeaders[1];
			entry2.CH_Status = CustomsEntryStatus.NotSent.Code;

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var entry1Loaded = factory2.Load<CusEntryHeader>(entry1.PK);
			entry1Loaded.Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var testManager = new IMDMultiMessageManager(entry1Loaded.Declaration, CMRMessageTypes.Withdrawal);
			testManager.SendWithdrawalMessages(new CusEntryHeader[] { entry1Loaded });

			factory2.Save();//trigger status change
			AssertEquals("Entry1 should have a withdrawal message", CustomsEntryStatus.AwaitingWithdrawal.Code, entry1Loaded.CH_Status);

			var entry2Loaded = factory2.Load<CusEntryHeader>(entry2.PK);
			AssertEquals("Entry2 should not be changed", CustomsEntryStatus.NotSent.Code, entry2Loaded.CH_Status);
		}

		public void TestSendAmendmentMessagesWithPassedEntries()
		{
			var testDec = JobDeclaration.New(Factory);
			SetUpEntriesForAmendmentTesting(testDec);

			var entry1 = testDec.CustomsEntryHeaders[0];
			entry1.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			var entry2 = testDec.CustomsEntryHeaders[1];
			entry2.CH_Status = CustomsEntryStatus.NotSent.Code;
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			Factory.Save();

			var factory2 = new BusinessObjectFactory();
			var entry1Loaded = factory2.Load<CusEntryHeader>(entry1.PK);
			entry1Loaded.Declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			var testManager = new IMDMultiMessageManager(entry1Loaded.Declaration, CMRMessageTypes.Withdrawal);
			testManager.SendMessagesFromAmendmentMenu(new CusEntryHeader[] { entry1Loaded });

			factory2.Save();//trigger status change
			AssertEquals("Entry1 should have been amended", CustomsEntryStatus.AwaitingAmendment.Code, entry1Loaded.CH_Status);

			var entry2Loaded = factory2.Load<CusEntryHeader>(entry2.PK);
			AssertEquals("Entry2 should not be changed", CustomsEntryStatus.NotSent.Code, entry2Loaded.CH_Status);
		}

		public void TestSendMessagesFromAmendmentMenuHandlesAllAmendableEntries()
		{
			var testDec = JobDeclaration.New(Factory);
			var line3 = SetUpEntriesForAmendmentTesting(testDec);
			line3.JI_Description = "Changed";
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			var originalEntry = testDec.CustomsEntryHeaders[0];
			var amendEntryWithoutChange = testDec.CustomsEntryHeaders[1];
			var amendEntryWithChange = testDec.CustomsEntryHeaders[2];

			AssertEquals("PreCondition:Original Entry", CustomsEntryStatus.NotSent.Code, originalEntry.CH_Status);
			AssertEquals("PreCondition:AmendEntryWithoutChange", CustomsEntryStatus.ClearAmendment.Code, amendEntryWithoutChange.CH_Status);
			AssertEquals("PreCondition:AmendEntryWithChange", CustomsEntryStatus.ClearFormalLodge.Code, amendEntryWithChange.CH_Status);

			var testManager = new IMDMultiMessageManager(testDec, CMRMessageTypes.Amendment);
			testManager.SendMessagesFromAmendmentMenu(new CusEntryHeader[] { amendEntryWithoutChange, amendEntryWithChange });
			Factory.Save();//To trigger status update
			AssertEquals("There should be no message generated for OriginalEntry as this was an amendment menu clicked", 0, originalEntry.Messages.Count);
			AssertEquals("1 message for AmendEntryWithoutChange as users choose to send an amendment for all entries", 1, amendEntryWithoutChange.Messages.Count);
			AssertEquals("Status for AmendEntryWithoutChange", CustomsEntryStatus.AwaitingAmendment.Code, amendEntryWithoutChange.CH_Status);
			AssertEquals("1 message for AmendEntryWithChange as users choose to send an amendment for all entries", 1, amendEntryWithChange.Messages.Count);
			AssertEquals("Status for AmendEntryWithoutChange", CustomsEntryStatus.AwaitingAmendment.Code, amendEntryWithChange.CH_Status);
		}

		[TestDate(2005, 10, 10)]
		public void TestEntrySubmittedDatePopulatedForLodgeWithPayOrLodgeWithoutPayOrPreLodge()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var sender = new SendsMessagesToCustomsShutterUpperer();
			declaration.MessageInitiator = sender;
			declaration.CustomsEntryHeaders.AddNew();

			declaration.JE_EntrySubmittedDate = new ZDateTime(2005, 1, 1);
			var testManager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.PreLodge);
			testManager.OnOneOrMoreOriginalsSent();
			AssertEquals("Entry submitted date is changed", new ZDateTime(2005, 10, 10), declaration.JE_EntrySubmittedDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2005, 1, 1);
			testManager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.Payment);
			testManager.OnOneOrMoreOriginalsSent();
			AssertEquals("Entry submitted date should stay as this is payment", new ZDateTime(2005, 1, 1), declaration.JE_EntrySubmittedDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2005, 1, 1);
			testManager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.LodgeWithPay);
			testManager.OnOneOrMoreOriginalsSent();
			AssertEquals("Entry submitted date is changed", new ZDateTime(2005, 10, 10), declaration.JE_EntrySubmittedDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2005, 1, 1);
			testManager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.Amendment);
			testManager.OnOneOrMoreOriginalsSent();
			AssertEquals("Entry submitted date should stay as this is an amendment", new ZDateTime(2005, 1, 1), declaration.JE_EntrySubmittedDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2005, 1, 1);
			testManager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.LodgeWithoutPay);
			testManager.OnOneOrMoreOriginalsSent();
			AssertEquals("Entry submitted date is changed", new ZDateTime(2005, 10, 10), declaration.JE_EntrySubmittedDate);

			declaration.JE_EntrySubmittedDate = new ZDateTime(2005, 1, 1);
			testManager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.Withdrawal);
			testManager.OnOneOrMoreOriginalsSent();
			AssertEquals("Entry submitted date should stay as this is a withdrawal", new ZDateTime(2005, 1, 1), declaration.JE_EntrySubmittedDate);
		}

		public void TestSetSentWithMessageErrorsForOriginal()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var header = declaration.Invoices.AddNew();
			header.JobComInvoiceLines.AddNew();

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "1";
			declaration.JE_TotalNoOfPacks = 1;//create a packing group
			var pack = declaration.PackingGroups[0];
			pack.CR_HouseContainerNumber = 1;

			declaration.DoMerge();
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AddOutgoingMessageToEntryHeader(entryHeader);

			var manager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.LodgeWithPay);
			manager.OnOneOrMoreOriginalsSent();
			AssertEquals("Flag is set", expected: true, entryHeader.Messages.LastOutgoingMessage.EM_SendWithMessageErrors);
			AssertEquals("No merge", expected: false, declaration.MergeManager.RequiresMerge);
		}

		public void TestSentWithMessageErrorsForOriginalIsNotSetWhenNoErrors()
		{
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AddOutgoingMessageToEntryHeader(entryHeader);

			var manager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.LodgeWithPay);
			manager.OnOneOrMoreOriginalsSent();
			AssertEquals("Flag is not set", expected: false, entryHeader.Messages.LastOutgoingMessage.EM_SendWithMessageErrors);
		}

		public void TestSetSentWithMessageErrorsForAmendmet()
		{
			var declaration = JobDeclaration.New(Factory);
			declaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			declaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var header = declaration.Invoices.AddNew();
			header.JobComInvoiceLines.AddNew();

			var bill = declaration.Bills.AddNew();
			bill.CU_BillNum = "1";
			declaration.JE_TotalNoOfPacks = 1;//create a packing group
			var pack = declaration.PackingGroups[0];
			pack.CR_HouseContainerNumber = 1;

			declaration.DoMerge();
			Factory.Save();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			AddOutgoingMessageToEntryHeader(entryHeader);

			var manager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.Amendment);
			manager.OnOneOrMoreAmendmentsSent();
			AssertEquals("Flag is set", expected: true, entryHeader.Messages.LastOutgoingMessage.EM_SendWithMessageErrors);
			AssertEquals("No merge", expected: false, declaration.MergeManager.RequiresMerge);
		}

		public void TestSentWithMessageErrorsForAmendmentIsNotSetWhenNoErrors()
		{
			var declaration = JobDeclaration.New(Factory);
			var entryHeader = declaration.CustomsEntryHeaders.AddNew();
			AddOutgoingMessageToEntryHeader(entryHeader);

			var manager = new IMDMultiMessageManagerForTest(declaration, CMRMessageTypes.Amendment);
			manager.OnOneOrMoreAmendmentsSent();
			AssertEquals("Flag is not set", expected: false, entryHeader.Messages.LastOutgoingMessage.EM_SendWithMessageErrors);
		}

		public void TestCanSendAmendment()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = ZString.Empty;
			entryHeader.CH_EntryStatus = ZString.Empty;
			SetUpInvoicesForEntry(entryHeader);
			var messageManager = new IMDMultiMessageManager(testDec, CMRMessageTypes.Amendment);
			AssertEquals("Can Send Amendment", expected: false, messageManager.CanSendAmendment);

			entryHeader.CH_Status = CustomsEntryStatus.FailAmendment.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals("Can Send Amendment", expected: true, messageManager.CanSendAmendment);
		}

		public void TestWarningPaymentDate()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew();
			message.EM_MessageNum = "1";
			message.EM_MessageType = CMRMessage.CMRMessageTypes.PAYREC;
			message.EM_SystemCreateTimeUtc = ZDateTime.Today;
			var messageManager = new IMDMultiMessageManager(testDec, CMRMessageTypes.Amendment);
			var coll = messageManager.CheckBusinessObjectLevelValidationIfRequired();
			Assert(!coll.NotificationsAsString().Contains(IMDMultiMessageManager.WarningPaymentDate));

			message.EM_SystemCreateTimeUtc = ZDateTime.Today.AddYears(-5);
			coll = messageManager.CheckBusinessObjectLevelValidationIfRequired();
			Assert(coll.NotificationsAsString().Contains(IMDMultiMessageManager.WarningPaymentDate));
		}

		public void TestSettingSubmittedDateDoesNotCauseDirtyInAutoMerge()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.CustomsEntryHeaders.AddNew();

			Factory.Save();

			AssertEquals("requires merge", expected: false, testDec.MergeManager.RequiresMerge);
			var manager = new IMDMultiMessageManagerForTest(testDec, CMRMessageTypes.LodgeWithPay);
			manager.OnOneOrMoreOriginalsSent();
			AssertEquals("requires merge", expected: false, testDec.MergeManager.RequiresMerge);
		}

		public void TestSendMessagesForAmendment()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			SetUpInvoicesForEntry(entryHeader);
			entryHeader.CH_Status = CustomsEntryStatus.ClearSAC.Code;
			AssertMessageTypeGenerated(CMRMessage.MessageSubTypes.Change, CMRMessageTypes.Amendment, testDec);
		}

		public void TestSendMessagesForLodgement()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			SetUpInvoicesForEntry(entryHeader);
			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertMessageTypeGenerated(CMRMessage.MessageSubTypes.Original, CMRMessageTypes.LodgeWithoutPay, testDec);
			var messageGenerated = (CMRIMDMessage)entryHeader.Messages[0];
			AssertEquals("IsPrelodge", expected: false, messageGenerated.IsPreLodgeMessage);
		}

		public void TestSendMessagesForPreLodgement()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			SetUpInvoicesForEntry(entryHeader);
			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertMessageTypeGenerated(CMRMessage.MessageSubTypes.Original, CMRMessageTypes.PreLodge, testDec);
			var messageGenerated = (CMRIMDMessage)entryHeader.Messages[0];
			AssertEquals("IsPrelodge", expected: true, messageGenerated.IsPreLodgeMessage);
		}

		public void TestSendQueuedEntryLodgement()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.JE_EDITransmitDate = ZDateTime.Today.AddHours(8);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			SetUpInvoicesForEntry(entryHeader);
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			SetUpCertificatesAndBrokersLicence();
			var manager = new IMDMultiMessageManager(testDec, CMRMessageTypes.LodgeWithoutPay);
			manager.SendQueuedMessage(testDec, new NotificationBuffer(), "ABC");
			AssertEquals("One message generated", 1, testDec.CustomsEntryHeaders[0].Messages.Count);
			AssertEquals("Message generated User", "ABC", testDec.CustomsEntryHeaders[0].Messages[0].EM_SystemCreateUser);
		}

		public void TestSendQueuedMessageWithEFTPaymentInformations_NoExceptionThrown()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAABBB111";
			SetUpCertificatesAndBrokersLicence();
			SetUpInvoicesForEntry(entryHeader);
			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			entryHeader.CustomsChargeAmountPayableNow = 0m;
			entryHeader.AQISServicePaymentAmountPayableNow = 0m;

			var manager = new IMDMultiMessageManager(testDec, CMRMessageTypes.Payment);
			manager.EFTPaymentInformations = new EFTPaymentInformationCollection(testDec);
			var notifications = new NotificationBuffer();
			AssertNoExceptionThrown("Get EFTPaymentInformation Collection.", () => manager.SendQueuedMessage(testDec, notifications, "ABC"));

			AssertContains("Payment Message has no Amounts to Pay.", string.Join(",", notifications.GetEventsByType(CargoWise.ComponentModel.NotificationType.Error).Select(x => x.Message)));
			AssertEquals("Message should not have been generated", 0, entryHeader.Messages.Count);
			AssertEquals("Entry Status is Failed Payment", CustomsEntryStatus.FailPayment.Code, entryHeader.CH_Status);
			AssertEquals("Declaration Status is Failed Payment", CustomsEntryStatus.FailPayment.Code, testDec.JE_MessageStatus);

			testDec.JE_MessageStatus = entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			entryHeader.CustomsChargeAmountPayableNow = 100m;
			entryHeader.AQISServicePaymentAmountPayableNow = 200m;

			manager = new IMDMultiMessageManager(testDec, CMRMessageTypes.Payment);
			manager.EFTPaymentInformations = new EFTPaymentInformationCollection(testDec, initialiseFromLastClearance: false);
			notifications = new NotificationBuffer();
			AssertNoExceptionThrown("Get EFTPaymentInformation Collection.", () => manager.SendQueuedMessage(testDec, notifications, "ABC"));

			AssertEquals(false, notifications.HasErrors);
			AssertEquals("Message should have been generated", 1, entryHeader.Messages.Count);
			AssertEquals("Entry Status is unchanged", CustomsEntryStatus.NotSent.Code, entryHeader.CH_Status);
			AssertEquals("Declaration Status is unchanged", CustomsEntryStatus.NotSent.Code, entryHeader.Declaration.JE_MessageStatus);
		}

		public void TestSendMessagesForWithdrawals()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			SetUpInvoicesForEntry(entryHeader);
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertMessageTypeGenerated(CMRMessage.MessageSubTypes.Withdraw, CMRMessageTypes.Withdrawal, testDec);
		}

		public void TestSendMessagesForPaymentMessage()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.EntryNumber = "AAABBB111";
			SetUpCertificatesAndBrokersLicence();
			SetUpInvoicesForEntry(entryHeader);
			var entryHeaderWithoutEntryNumber = testDec.CustomsEntryHeaders.AddNew();
			SetUpInvoicesForEntry(entryHeaderWithoutEntryNumber);
			var entryHeaderWithEntryNumberButWithoutAmount = testDec.CustomsEntryHeaders.AddNew();
			entryHeaderWithEntryNumberButWithoutAmount.EntryNumber = "CCCAAA111";
			SetUpInvoicesForEntry(entryHeaderWithEntryNumberButWithoutAmount);
			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			var manager = new IMDMultiMessageManager(testDec, CMRMessageTypes.Payment);
			manager.EFTPaymentInformations = new EFTPaymentInformationCollection(testDec);
			AssertEquals("there should be two items in the collection", 2, manager.EFTPaymentInformations.Count);
			AssertEquals("Item 1 EntryHeader", entryHeader, manager.EFTPaymentInformations[0].EntryHeader);
			AssertEquals("Item 2 EntryHeader", entryHeaderWithEntryNumberButWithoutAmount, manager.EFTPaymentInformations[1].EntryHeader);

			manager.SendMessages(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals("Has amount to pay", expected: false, manager.EFTPaymentInformations[0].HasAmountsToPay);
			AssertEquals("No message generated as there is no amount to pay", 0, entryHeader.Messages.Count);

			manager.EFTPaymentInformations[0].CustomsChargeAmountPayableNow = 10m;
			AssertEquals("Has amount to pay", expected: true, manager.EFTPaymentInformations[0].HasAmountsToPay);
			AssertEquals("Has amount to pay", expected: false, manager.EFTPaymentInformations[1].HasAmountsToPay);

			AssertEquals("ShouldSendPaymentMessagefor this entry", expected: true, manager.ShouldGeneratePaymentMessageForThisEntryHeader(entryHeader));
			AssertEquals("ShouldSendPaymentMessagefor this entry", expected: false, manager.ShouldGeneratePaymentMessageForThisEntryHeader(entryHeaderWithoutEntryNumber));
			AssertEquals("ShouldSendPaymentMessagefor this entry", expected: false, manager.ShouldGeneratePaymentMessageForThisEntryHeader(entryHeaderWithEntryNumberButWithoutAmount));

			manager.SendMessages(new SendsMessagesToCustomsShutterUpperer(false));
			AssertEquals("One message generated", 1, entryHeader.Messages.Count);
			AssertEquals("Message type generated", CMRMessage.CMRMessageTypes.PAYSTD, entryHeader.Messages[0].EM_MessageType);
			AssertEquals("Entry header without number not generated", 0, entryHeaderWithoutEntryNumber.Messages.Count);
			AssertEquals("Entry header with number, but without amount to pay not generated", 0, entryHeaderWithEntryNumberButWithoutAmount.Messages.Count);
		}

		public void TestSendMessagesForConsolidatedDeclarationLodgement()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC";
			var deliveryAddress = importer.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			deliveryAddress.OA_Address1 = "Addr 1";
			deliveryAddress.OA_Address2 = "Addr 2";
			deliveryAddress.OA_City = "CTY";
			deliveryAddress.OA_State = "NSW";
			deliveryAddress.OA_PostCode = "2001";

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			leadDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			leadDeclaration.JE_AgentsReference = "AGENT123";
			leadDeclaration.JE_CustomsDischargePort = "AUSYD";
			leadDeclaration.JE_EDITransmitDate = ZDateTime.Today.AddHours(8);
			leadDeclaration.JE_OH_Importer = importer.PK;
			leadDeclaration.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			var entryHeader = leadDeclaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "AAA";

			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = entryHeader.Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			var aggregatedDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			aggregatedDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			SetUpCertificatesAndBrokersLicence();
			var manager = new IMDMultiMessageManager(aggregatedDeclaration, CMRMessageTypes.LodgeWithoutPay);
			Assert("SendMessages", manager.SendMessages(aggregatedDeclaration.MessageInitiator));

			aggregatedDeclaration.EntryHeader.Calculator.DeriveStatusIfRequired();
			consolidatedDeclaration.ImportAggregateDeclaration(aggregatedDeclaration);

			AssertEquals("Message should have been generated", 1, consolidatedDeclaration.Messages.Count);
			var outMessageText = consolidatedDeclaration.Messages[0].EM_MessageText;
			CombineAssertions(() =>
			{
				AssertContains("Senders Reference", $"BGM+929:::IMD+{consolidatedDeclaration.CRD_JobReferenceNumber}/DAT1:1+9", outMessageText);
				AssertContains("Agent Reference", $"RFF+ADU:{consolidatedDeclaration.CRD_JobReferenceNumber} AGENT123", outMessageText);
				AssertContains("Importer Delivery Address", "NAD+DP++CTY++ADDR 1::ADDR 2++:::NSW+2001+AU", outMessageText);
				AssertContains("Broker licence number", "NAD+CB+54321::95", outMessageText);
			});
		}

		public void TestSendMessagesForConsolidatedDeclarationAmendment()
		{
			var importer = Factory.New<OrgHeader>();
			importer.OH_Code = "ABC";
			var deliveryAddress = importer.Addresses.AddNew();
			deliveryAddress.AddressCapability.SetCapabilityEnabled(nameof(AddressType.PIC));
			deliveryAddress.OA_Address1 = "Addr 1";
			deliveryAddress.OA_Address2 = "Addr 2";
			deliveryAddress.OA_City = "CTY";
			deliveryAddress.OA_State = "NSW";
			deliveryAddress.OA_PostCode = "2001";

			var consolidatedDeclaration = ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory, 1);
			var leadDeclaration = (JobDeclaration)consolidatedDeclaration.LeadDeclaration;
			leadDeclaration.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			leadDeclaration.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			leadDeclaration.JE_AgentsReference = "AGENT123";
			leadDeclaration.JE_CustomsDischargePort = "AUSYD";
			leadDeclaration.JE_EDITransmitDate = ZDateTime.Today.AddHours(8);
			leadDeclaration.JE_OH_Importer = importer.PK;
			leadDeclaration.ImporterDeliveryAddress.E2_OA_Address = deliveryAddress.PK;
			var entryHeader = leadDeclaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "AAA";
			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;

			var entryLine = entryHeader.MergedLines.AddNew();
			var invoiceLine = entryHeader.Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entryHeader.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();

			var aggregatedDeclaration = (JobDeclaration)consolidatedDeclaration.BuildAggregateJobDeclaration();
			aggregatedDeclaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			SetUpCertificatesAndBrokersLicence();
			var manager = new IMDMultiMessageManager(aggregatedDeclaration, CMRMessageTypes.Amendment);
			Assert("SendMessages", manager.SendMessages(aggregatedDeclaration.MessageInitiator));

			aggregatedDeclaration.EntryHeader.DeriveConsolidatedStatus();
			consolidatedDeclaration.ImportAggregateDeclaration(aggregatedDeclaration);

			AssertEquals("Message should have been generated", 1, consolidatedDeclaration.Messages.Count);
			var outMessageText = consolidatedDeclaration.Messages[0].EM_MessageText;
			CombineAssertions(() =>
			{
				AssertContains("Senders Reference", $"BGM+929:::IMD+{consolidatedDeclaration.CRD_JobReferenceNumber}/DAT0:1+4", outMessageText);
				AssertContains("Agent Reference", $"RFF+ADU:{consolidatedDeclaration.CRD_JobReferenceNumber} AGENT123", outMessageText);
				AssertContains("Importer Delivery Address", "NAD+DP++CTY++ADDR 1::ADDR 2++:::NSW+2001+AU", outMessageText);
				AssertContains("Broker licence number", "NAD+CB+54321::95", outMessageText);
			});
		}

		#region ScheduleLodgementMessage Tests

		public void TestSchedulePreLodgementMessage()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = ScheduleImportMessage(testDec, CMRMessageTypes.PreLodge, ZDateTime.Today.AddDays(1));

				Assert("Generates a PreLodgement message", (entryHeader.Messages.FirstOrDefault() as CMRIMDMessage).IsPreLodgeMessage);
				AssertEquals("Sets entry header status to WPL", CustomsEntryStatus.AwaitingPreLodge.Code, entryHeader.CH_Status);
				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
			}
		}

		[TestDate(2023, 09, 04)]
		public void TestScheduleLodgementMessageWithoutPay()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = ScheduleImportMessage(testDec, CMRMessageTypes.LodgeWithoutPay, ZDateTime.Today.AddDays(1));

				AssertEquals("Does not generate a message", 0, entryHeader.Messages.Count);
				AssertEquals("Sets entry header status to SLU", CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code, entryHeader.CH_Status);
				AssertEquals("Sets declaration Message Status to SLU", CustomsEntryStatus.ScheduledLodgeWithoutPayment.Code, testDec.JE_MessageStatus);
				Assert("Declaration becomes ReadOnly when queued", testDec.ReadOnly);

				var dsmLog = testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault();
				AssertEquals("Creates DSM log with event time at 8AM", "2023-09-05T08:00:00", dsmLog.SL_EventTime.ToISO8601String());
				AssertEquals("Creates DSM log with message mode in reference", "LodgeWithoutPay", dsmLog.SL_Reference);

				var customsCommencedLog = testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomsCommencedCode).FirstOrDefault();
				AssertNull("Customs Commenced not logged", customsCommencedLog);
			}
		}

		[TestDate(2023, 09, 04)]
		public void TestScheduleLodgementMessageWithPay()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = ScheduleImportMessage(testDec, CMRMessageTypes.LodgeWithPay, ZDateTime.Today.AddDays(1));

				AssertEquals("Does not generate a message", 0, entryHeader.Messages.Count);
				AssertEquals("Sets entry header status to SLP", CustomsEntryStatus.ScheduledLodgeWithPayment.Code, entryHeader.CH_Status);
				AssertEquals("Sets declaration Message Status to SLP", CustomsEntryStatus.ScheduledLodgeWithPayment.Code, testDec.JE_MessageStatus);
				Assert("Declaration becomes ReadOnly when queued", testDec.ReadOnly);

				var dsmLog = testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault();
				AssertEquals("Creates DSM log with event time at 8AM", "2023-09-05T08:00:00", dsmLog.SL_EventTime.ToISO8601String());
				AssertEquals("Creates DSM log with message mode in reference", "LodgeWithPay", dsmLog.SL_Reference);

				var customsCommencedLog = testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.CustomsCommencedCode).FirstOrDefault();
				AssertNull("Customs Commenced not logged", customsCommencedLog);
			}
		}

		public void TestScheduleLodgementMessageWithoutPay_TransmitDateIsToday()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = ScheduleImportMessage(testDec, CMRMessageTypes.LodgeWithoutPay, ZDateTime.Today);

				AssertEquals("Generates a Lodgement message", expected: false, (entryHeader.Messages.FirstOrDefault() as CMRIMDMessage).IsPreLodgeMessage);
				AssertEquals("Sets entry header status to WFL", CustomsEntryStatus.AwaitingFormalLodge.Code, entryHeader.CH_Status);
				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
			}
		}

		public void TestScheduleLodgementMessageWithPay_TransmitDateIsToday()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = ScheduleImportMessage(testDec, CMRMessageTypes.LodgeWithPay, ZDateTime.Today);

				AssertEquals("Generates a Lodgement message", expected: false, (entryHeader.Messages.FirstOrDefault() as CMRIMDMessage).IsPreLodgeMessage);
				AssertEquals("Sets entry header status to WFL", CustomsEntryStatus.AwaitingFormalLodge.Code, entryHeader.CH_Status);
				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
			}
		}

		public void TestScheduleLodgementMessageWithoutPay_TransmitDateNotSet()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = ScheduleImportMessage(testDec, CMRMessageTypes.LodgeWithoutPay, ZDateTime.Empty);

				AssertEquals("Generates a Lodgement message", expected: false, (entryHeader.Messages.FirstOrDefault() as CMRIMDMessage).IsPreLodgeMessage);
				AssertEquals("Sets entry header status to WFL", CustomsEntryStatus.AwaitingFormalLodge.Code, entryHeader.CH_Status);
				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
			}
		}

		public void TestScheduleLodgementMessageWithPay_TransmitDateNotSet()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = ScheduleImportMessage(testDec, CMRMessageTypes.LodgeWithPay, ZDateTime.Empty);

				AssertEquals("Generates a Lodgement message", expected: false, (entryHeader.Messages.FirstOrDefault() as CMRIMDMessage).IsPreLodgeMessage);
				AssertEquals("Sets entry header status to WFL", CustomsEntryStatus.AwaitingFormalLodge.Code, entryHeader.CH_Status);
				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
			}
		}

		public void TestScheduleLodgementMessageWithoutPay_SchedulingDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: false))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = ScheduleImportMessage(testDec, CMRMessageTypes.LodgeWithoutPay, ZDateTime.Today.AddDays(1));

				AssertEquals("Generates a Lodgement message", expected: false, (entryHeader.Messages.FirstOrDefault() as CMRIMDMessage).IsPreLodgeMessage);
				AssertEquals("Sets entry header status to WFL", CustomsEntryStatus.AwaitingFormalLodge.Code, entryHeader.CH_Status);
				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
			}
		}

		public void TestScheduleLodgementMessageWithPay_SchedulingDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: false))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = ScheduleImportMessage(testDec, CMRMessageTypes.LodgeWithPay, ZDateTime.Today.AddDays(1));

				AssertEquals("Generates a Lodgement message", expected: false, (entryHeader.Messages.FirstOrDefault() as CMRIMDMessage).IsPreLodgeMessage);
				AssertEquals("Sets entry header status to WFL", CustomsEntryStatus.AwaitingFormalLodge.Code, entryHeader.CH_Status);
				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
			}
		}

		CusEntryHeader ScheduleImportMessage(JobDeclaration testDec, CMRMessageTypes messageType, ZDateTime transmitDate)
		{
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			SetUpInvoicesForEntry(entryHeader);
			testDec.JE_EDITransmitDate = transmitDate;

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			SetUpCertificatesAndBrokersLicence();
			var manager = new IMDMultiMessageManager(testDec, messageType);
			manager.SendMessages(testDec.MessageInitiator);
			testDec.Factory.Save();

			return entryHeader;
		}

		#endregion

		#region SchedulePaymentMessage Tests

		[TestDate(2023, 12, 12, 12, 12, 0)]
		public void TestSchedulePaymentMessage()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = SchedulePaymentMessage(testDec, ZDateTime.Now.AddDays(1));

				AssertEquals("Does not generate a message", 0, entryHeader.Messages.Count);
				AssertEquals("Sets declaration Message Status to SPY", CustomsEntryStatus.ScheduledPayment.Code, testDec.JE_MessageStatus);
				Assert("Declaration becomes ReadOnly when queued", testDec.ReadOnly);

				var dsmLog = testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault();
				AssertEquals("Creates DSM log with event time at scheduled time", "13 Dec 2023 12:12", dsmLog.SL_EventTime.ToBestReadableDateTimeString());
				AssertEquals("Creates DSM log with message mode in reference", "Payment", dsmLog.SL_Reference);
				AssertEquals(0, entryHeader.Messages.Count);
			}
		}

		public void TestSchedulePaymentMessage_SchedulingDisabled()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: false))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = SchedulePaymentMessage(testDec, ZDateTime.Today.AddDays(1));
				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
				AssertEquals(1, entryHeader.Messages.Cast<CMRPAYSTDMessage>().Count());
			}
		}

		public void TestSchedulePaymentMessage_TransmitDateIsNow()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = SchedulePaymentMessage(testDec, ZDateTime.Now.AddSeconds(-5));
				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
				AssertEquals(1, entryHeader.Messages.Cast<CMRPAYSTDMessage>().Count());
			}
		}

		public void TestSchedulePaymentMessage_TransmitDateNotSet()
		{
			using (ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.QENT, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true))
			{
				var testDec = JobDeclaration.New(Factory);
				var entryHeader = SchedulePaymentMessage(testDec, ZDateTime.Empty);

				AssertNull("Does not Create a DSM log", testDec.Logs.Find(l => l.SL_SE_NKEvent == AutoEvents.DeferredScheduledMessageCode).FirstOrDefault());
				AssertEquals(1, entryHeader.Messages.Cast<CMRPAYSTDMessage>().Count());
			}
		}

		CusEntryHeader SchedulePaymentMessage(JobDeclaration testDec, ZDateTime transmitDate)
		{
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			SetUpInvoicesForEntry(entryHeader);
			entryHeader.ScheduledPaymentDate = transmitDate;

			var eFTPaymentInformation = new EFTPaymentInformation(entryHeader);
			eFTPaymentInformation.CustomsChargeAmountPayableNow = 100m;
			var payInfoCollection = new EFTPaymentInformationCollection(testDec);
			payInfoCollection.Add(eFTPaymentInformation);

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			SetUpCertificatesAndBrokersLicence();
			var manager = new IMDMultiMessageManager(testDec, CMRMessageTypes.Payment)
			{
				EFTPaymentInformations = payInfoCollection
			};
			manager.SendMessages(testDec.MessageInitiator);
			testDec.Factory.Save();

			return entryHeader;
		}

		#endregion

		public void TestShouldSendMessagesInTestMode()
		{
			Env.Registry.CMRTestMode = false;
			var manager = new IMDMultiMessageManager(JobDeclaration.New(Factory), CMRMessageTypes.Payment);
			Assert(!manager.ShouldSendMessagesInTestMode);
			Env.Registry.CMRTestMode = true;
			Assert(manager.ShouldSendMessagesInTestMode);
		}

		public void TestOriginalMessageTypeUsedInConfirmation()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;

			var manager = new IMDMultiMessageManagerForTest(testDec, CMRMessageTypes.Payment);
			AssertEquals("OriginalMessageTypeUsedInConfirmation", "payment", manager.OriginalMessageTypeUsedInConfirmation);

			manager = new IMDMultiMessageManagerForTest(testDec, CMRMessageTypes.LodgeWithPay);
			AssertEquals("OriginalMessageTypeUsedInConfirmation", "original", manager.OriginalMessageTypeUsedInConfirmation);
		}

		public void TestCanSendOriginalCanSendWithdrawal()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeaderOriginalSendable = testDec.CustomsEntryHeaders.AddNew();
			var entryHeaderWithdrawable = testDec.CustomsEntryHeaders.AddNew();

			entryHeaderOriginalSendable.CH_Status = CustomsEntryStatus.NotSent.Code;
			entryHeaderWithdrawable.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			SetUpInvoicesForEntry(entryHeaderOriginalSendable);
			SetUpInvoicesForEntry(entryHeaderWithdrawable);
			var testManager = new IMDMultiMessageManager(testDec, CMRMessageTypes.PreLodge);

			var testManager1 = new IMDMessageManager(entryHeaderOriginalSendable, testManager);
			AssertEquals("PreCondition: Original sendable", expected: true, testManager1.CanSendOriginal);

			var testManager2 = new IMDMessageManager(entryHeaderWithdrawable, testManager);
			AssertEquals("PreCondition: Withdrawal sendable", expected: true, testManager2.CanSendWithdrawal);

			AssertEquals("Original sendable", expected: true, testManager.CanSendOriginalMessage);
			AssertEquals("Withdrawal sendable", expected: true, testManager.CanSendWithdrawal);

			entryHeaderWithdrawable.CH_Status = CustomsEntryStatus.ClearWithdrawal.Code;
			AssertEquals("Original sendable", expected: true, testManager.CanSendOriginalMessage);
			AssertEquals("Withdrawal sendable", expected: false, testManager.CanSendWithdrawal);

			entryHeaderOriginalSendable.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Original sendable", expected: false, testManager.CanSendOriginalMessage);
			AssertEquals("Withdrawal sendable", expected: true, testManager.CanSendWithdrawal);
		}

		public void TestOnHasChangesWithNoAmendment()
		{
			var testDec = JobDeclaration.New(Factory);
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			entryHeader.CH_TotalPaid = 100m;
			entryHeader.Charges.AddNew("AAA", 10m);
			entryHeader.Charges.AddNew("BBB", 11m);

			var entryLine = entryHeader.MergedLines.AddNew();
			entryLine.CL_CustomsValue = 1000m;
			entryLine.Fees.AddOrUpdate("CCC", 20m);
			entryLine.Fees.AddOrUpdate("DDD", 21m);

			Factory.Save();
			AssertEquals("PreCondition: Saved", expected: true, testDec.IsInDatabase);

			entryHeader.CH_TotalPaid = 150m;
			var charge = entryHeader.Charges["AAA"];
			charge.C1_ChargeAmount = 12m;

			entryHeader.Charges.AddNew("AAB", 30m);

			entryLine.CL_CustomsValue = 1500m;
			var lineFee = entryLine.Fees.GetOrAddFeeByFeeType("CCC");
			lineFee.CF_ChargeAmount = 22m;

			entryLine.Fees.AddOrUpdate("EEE", 32m);

			AssertEquals("Entry is not lodged yet", expected: false, entryHeader.IsStatusPostLodge);

			var manager = new IMDMultiMessageManagerForTest(testDec, CMRMessageTypes.LodgeWithPay) as Customs.Business.IMessageManager;
			var information = manager.GetRequiredMessagesInformation();
			var savingoptions = manager.GetDeferredAmendmentSavingOptions();
			savingoptions.SetSaveWithoutEntryChangesValueForTestingTo(true);
			manager.ProcessWhenChangesAreSavedWithoutSending(savingoptions, information);

			AssertEquals("Total paid should stay as it is as entry is not lodged yet", 150m, entryHeader.CH_TotalPaid);
			AssertEquals("Charge with AAA should stay as it is as entry is not lodged yet", 12m, entryHeader.Charges.GetAmount("AAA"));
			AssertEquals("Charge with BBB should stay as it is as entry is not lodged yet", 11m, entryHeader.Charges.GetAmount("BBB"));
			AssertEquals("Charge with AAB should stay as it is as entry is not lodged yet", 30m, entryHeader.Charges.GetAmount("AAB"));

			AssertEquals("Customs Value should stay as it is as entry is not lodged yet", 1500m, entryLine.CL_CustomsValue);
			AssertEquals("Charge with CCC should stay as it is as entry is not lodged yet", 22m, entryLine.Fees.GetAmount("CCC"));
			AssertEquals("Charge with DDD should stay as it is as entry is not lodged yet", 21m, entryLine.Fees.GetAmount("DDD"));
			AssertEquals("Charge with EEE should stay as it is as entry is not lodged yet", 32m, entryLine.Fees.GetAmount("EEE"));

			entryHeader.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("Entry is lodged now", expected: true, entryHeader.IsStatusPostLodge);

			manager.ProcessWhenChangesAreSavedWithoutSending(savingoptions, information);

			AssertEquals("Total paid should be recovered from DB entry's amount", 100m, entryHeader.CH_TotalPaid);
			AssertEquals("Charge with AAA should be recovered from DB entry's amount", 10m, entryHeader.Charges.GetAmount("AAA"));
			AssertEquals("Charge with BBB should be recovered from DB entry's amount", 11m, entryHeader.Charges.GetAmount("BBB"));
			AssertEquals("Charge with AAB should be recovered as DB entry does not have it", 0m, entryHeader.Charges.GetAmount("AAB"));

			AssertEquals("Customs Value should be recovered from DB entry's amount", 1000m, entryLine.CL_CustomsValue);
			AssertEquals("Charge with CCC should be recovered from DB entry's amount", 20m, entryLine.Fees.GetAmount("CCC"));
			AssertEquals("Charge with DDD should be recovered from DB entry's amount", 21m, entryLine.Fees.GetAmount("DDD"));
			AssertEquals("Charge with EEE should be removed as DB entry does not have it", 0m, entryLine.Fees.GetAmount("EEE"));
		}

		public void TestSendWheneverPossibleOnceMessagingActive()
		{
			var testDec = JobDeclaration.New(Factory);
			AssertEquals("SendWheneverPossibleOnceMessagingActive", expected: true, new IMDMultiMessageManagerForTest(testDec, CMRMessageTypes.LodgeWithPay).SendWheneverPossibleOnceMessagingActive);
		}

		public void TestAllMessageManagers()
		{
			var testDec = JobDeclaration.New(Factory);
			var entry = testDec.CustomsEntryHeaders.AddNew();
			SetUpInvoicesForEntry(entry);
			var manager = new IMDMultiMessageManagerForTest(testDec, CMRMessageTypes.LodgeWithPay);

			var allMessageManagers = manager.GetAllMessageManagersExposed();
			AssertEquals("AllMessageManagers.Length", 1, allMessageManagers.Length);
		}

		public void TestCustomsClearanceCommencedLogAddedOnMessageSent()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.CustomsEntryHeaders.AddNew();
			var manager = new IMDMultiMessageManagerForTest(testDec, CMRMessageTypes.LodgeWithPay);
			var mostRecentCommencedLog = testDec.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(AutoEvents.ExportCustomsCommenced);
			AssertNull("CustomsCommenced log not added", mostRecentCommencedLog);
			var messageInitiator = new Customs.Business.SendsMessagesToCustomsShutterUpperer();
			manager.OnMessagesSent(messageInitiator);
			mostRecentCommencedLog = testDec.LogsOfDeclarationOrShipment.MostRecentLogByEventTime(AutoEvents.ExportCustomsCommenced);
			AssertNotNull("CustomsCommenced log added now", mostRecentCommencedLog);
		}

		public void TestJE_EntrySubmittedDateIsSet()
		{
			var testDec = JobDeclaration.New(Factory);
			testDec.CustomsEntryHeaders.AddNew();
			var manager = new IMDMultiMessageManagerForTest(testDec, CMRMessageTypes.LodgeWithPay);
			AssertEquals("JE_EntrySubmittedDate is empty", expected: true, testDec.JE_EntrySubmittedDate.IsEmpty);
			manager.OnOneOrMoreOriginalsSent();
			AssertEquals("JE_EntrySubmittedDate is set", expected: false, testDec.JE_EntrySubmittedDate.IsEmpty);
		}

		JobComInvoiceLine SetUpEntriesForAmendmentTesting(JobDeclaration testDec)
		{
			testDec.JE_MessageType = Common.Shared.SharedJobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();

			var originalEntry = testDec.CustomsEntryHeaders.AddNew();
			originalEntry.CH_Status = CustomsEntryStatus.NotSent.Code;
			AssertEquals("IsStatusPostLodge", expected: false, originalEntry.IsStatusPostLodge);

			var amendEntryWithoutChange = testDec.CustomsEntryHeaders.AddNew();
			amendEntryWithoutChange.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			AssertEquals("IsStatusPostLodge", expected: true, amendEntryWithoutChange.IsStatusPostLodge);

			var amendEntryWithChange = testDec.CustomsEntryHeaders.AddNew();
			amendEntryWithChange.CH_Status = CustomsEntryStatus.ClearFormalLodge.Code;
			AssertEquals("IsStatusPostLodge", expected: true, amendEntryWithChange.IsStatusPostLodge);

			var entryLine1 = originalEntry.MergedLines.AddNew();
			entryLine1.CL_LineNumber = (ZShort)1;
			var entryLine2 = amendEntryWithoutChange.MergedLines.AddNew();
			entryLine2.CL_LineNumber = (ZShort)1;
			var entryLine3 = amendEntryWithChange.MergedLines.AddNew();
			entryLine3.CL_LineNumber = (ZShort)1;

			var invoice1 = testDec.Invoices.AddNew();
			invoice1.AddInfo.ZA_EFD = "010101";
			var line1 = invoice1.JobComInvoiceLines.AddNew();
			line1.JI_CL = entryLine1.PK;

			var invoice2 = testDec.Invoices.AddNew();
			invoice2.AddInfo.ZA_EFD = "020202";
			var line2 = invoice2.JobComInvoiceLines.AddNew();
			line2.JI_CL = entryLine2.PK;

			var invoice3 = testDec.Invoices.AddNew();
			invoice3.AddInfo.ZA_EFD = "030303";
			var line3 = invoice3.JobComInvoiceLines.AddNew();
			line3.JI_CL = entryLine3.PK;

			testDec.Bills.AddNew();
			SetUpCertificatesAndBrokersLicence();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			Factory.Save();

			return line3;
		}

		void AddOutgoingMessageToEntryHeader(CusEntryHeader entryHeader)
		{
			var iMDMessage = entryHeader.Messages.AddNew(typeof(CMRIMDMessage));
			iMDMessage.EM_ReceiveTransmit = EDIInterchange.Direction.Transmit;
			iMDMessage.EM_Status = Messaging.Integration.EDIMessageStatusList.Codes.Sent;
			iMDMessage.EM_MessageText = CMRImportDeclarationTestData.IMD;
		}

		void SetUpInvoicesForEntry(CusEntryHeader entry)
		{
			var entryLine = entry.MergedLines.AddNew();
			var invoiceLine = entry.Declaration.Invoices.AddNew().JobComInvoiceLines.AddNew();
			invoiceLine.JI_CL = entryLine.PK;
			entry.Declaration.MergeManager.DisablePreSaveMergeRequirementForTesting();
			Factory.Save();
		}

		void AssertMessageTypeGenerated(string subMessageType, CMRMessageTypes messageType, JobDeclaration testDec)
		{
			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			SetUpCertificatesAndBrokersLicence();
			var manager = new IMDMultiMessageManager(testDec, messageType);
			manager.SendMessages(testDec.MessageInitiator);
			AssertEquals("One message generated", 1, testDec.CustomsEntryHeaders[0].Messages.Count);
			AssertEquals("Message type generated", subMessageType, testDec.CustomsEntryHeaders[0].Messages[0].EM_MessageSubType);
		}

		void SetUpCertificatesAndBrokersLicence()
		{
			var brokerLicence = GlbStaff.CurrentUser.Certificates.AddNew();
			brokerLicence.XZ_RefNumber = "54321";
			brokerLicence.XZ_Type = CertificateTypePairList.Codes.BR1;

			Env.Registry.AUCustoms.LocalCustomsBranchIdentifier = "AAA";
			Common.AU.CMR.Testing.CMRUtilitiesTest.SetupCertificates();

			Env.Security.CustomsDeclarationSendWithMessageErrors.IsAllowed = true;
		}

		sealed class IMDMultiMessageManagerForTest : IMDMultiMessageManager
		{
			public IMDMultiMessageManagerForTest(JobDeclaration jobDeclaration, CMRMessageTypes messageType) : base(jobDeclaration, messageType)
			{
			}

			internal Customs.Business.SingleMessageManager[] GetAllMessageManagersExposed() => GetAllMessageManagers();

			internal new void OnOneOrMoreOriginalsSent() => base.OnOneOrMoreOriginalsSent();

			internal new void OnOneOrMoreAmendmentsSent() => base.OnOneOrMoreAmendmentsSent();

			internal new void OnMessagesSent(Customs.Business.ISendsMessagesToCustoms sender) => base.OnMessagesSent(sender);

			internal new string OriginalMessageTypeUsedInConfirmation => base.OriginalMessageTypeUsedInConfirmation;

			internal new bool SendWheneverPossibleOnceMessagingActive => base.SendWheneverPossibleOnceMessagingActive;

			internal Customs.Business.SingleMessageManager[] AllMessageManagersExposed;

			protected override Customs.Business.SingleMessageManager[] GetAllMessageManagers() => AllMessageManagersExposed ?? base.GetAllMessageManagers();
		}
	}
}
