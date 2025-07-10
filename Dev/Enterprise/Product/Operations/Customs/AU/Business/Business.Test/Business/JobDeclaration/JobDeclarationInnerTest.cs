using System.Data;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common.AU;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class JobDeclarationInnerTest : TestCaseWithFactory
	{
		public void TestPackingGroups()
		{
			var declaration = Factory.New<JobDeclarationForTest>();

			AssertNotNull(declaration.PackingGroups);
			AssertEquals(declaration.PackingGroups, declaration.PackingGroups);
		}

		public void TestIsPackingInformationRelevant()
		{
			var declaration = Factory.New<JobDeclarationForTest>();
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, declaration.IsPackingInformationRelevantCore);
			AssertEquals(true, declaration.IsPackingGroupCollectionRegisteredEditable);

			declaration.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;
			AssertEquals(false, declaration.IsPackingInformationRelevantCore);
			AssertEquals(false, declaration.IsPackingGroupCollectionRegisteredEditable);

			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals(false, declaration.IsPackingInformationRelevantCore);
			AssertEquals(false, declaration.IsPackingGroupCollectionRegisteredEditable);

			declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertEquals(true, declaration.IsPackingInformationRelevantCore);
			AssertEquals(true, declaration.IsPackingGroupCollectionRegisteredEditable);

			declaration.JE_TransportMode = Core.Constants.TransportModes.Other;
			AssertEquals(false, declaration.IsPackingInformationRelevantCore);
			AssertEquals(false, declaration.IsPackingGroupCollectionRegisteredEditable);
		}

		public void TestShouldKeepDeletedLinesOnAmendment()
		{
			var testDec = Factory.New<JobDeclarationForTest>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals("ShouldKeepDeletedLinesOnAmendment", true, testDec.ShouldKeepDeletedLinesOnAmendmentCore);

			testDec.JE_MergeBy = OrgConstants.MergeInvoiceLines.NotMerge;
			var invoice = testDec.Invoices.AddNew();
			var invoiceLine1 = invoice.JobComInvoiceLines.AddNew();
			var invoiceLine2 = invoice.JobComInvoiceLines.AddNew();

			var merger = new LineMerger(testDec);
			merger.DoMerge();

			var entryHeader = testDec.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = "AAA";
			entryHeader.CH_Status = CustomsEntryStatus.ClearAmendment.Code;
			var entryLine1 = invoiceLine1.CusEntryLine;
			var entryLine2 = invoiceLine2.CusEntryLine;
			entryHeader.CH_HighestLineNumber = 2;//lodged

			invoiceLine1.Delete();
			merger.DoMerge();
			AssertEquals("EntryLine1 should not be deleted", false, entryLine1.IsDeleted);
			AssertEquals("EntryLine1 should be in PendingDeletionLines", true, entryHeader.PendingDeletionEntryLines.Contains(entryLine1));
			AssertEquals("EntryLine1 should not be in MergedLines", false, entryHeader.MergedLines.Contains(entryLine1));
			AssertEquals("EntryLine2 should be in MergedLines", true, entryHeader.MergedLines.Contains(entryLine2));
			AssertEquals("EntryLine2 should not be in PendingDeletionEntryLines", false, entryHeader.PendingDeletionEntryLines.Contains(entryLine2));
		}

		[ExpectNoExceptions]
		public void TestMergeMultiTimesWithDeletedInvoiceHeader()
		{
			ErrorReporter.Clear();
			var testDec = Factory.New<JobDeclarationForTest>();
			testDec.JE_MessageType = JobMessageTypeList.Codes.ExWarehouse;

			var invoiceHeader = testDec.Invoices.AddNew();
			invoiceHeader.JZ_InvoiceAmount = 14173m;
			var invoiceLine = invoiceHeader.JobComInvoiceLines.AddNew();
			invoiceLine.JI_LinePrice = 14173m;
			invoiceLine.AddInfo.ZA_TILV = "100USD";

			var merger = new LineMerger(testDec);
			merger.DoMerge();

			//delete the invoice header
			invoiceHeader.Delete();

			//then add it back
			var invoiceHeader2 = testDec.Invoices.AddNew();
			invoiceHeader2.JZ_InvoiceAmount = 14173m;
			var invoiceLine2 = invoiceHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 14173m;
			invoiceLine2.AddInfo.ZA_TILV = "100USD";

			//merge again
			merger.DoMerge();

			AssertEquals("Total Error Count should be 0", 0, ErrorReporter.TotalErrorCount);
		}

		public void TestJE_MessageStatusDescriptionIncludingOutstandingAmendments()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;

			var log = testDec.OutstandingAmendmentLogManger.AddANewOutstandingAmendmentLog("TESTTEST", new ZDateTime(2005, 1, 1, 1, 1, 1));
			AssertEquals("PreCondition: There is an outstanding amendment", true, testDec.HasOutstandingAmendment);

			var result = testDec.JE_MessageStatusDescriptionIncludingOustandingAmendments;
			AssertEquals("Result should indicate there is an outstanding amendment", true, result.Contains(testDec.OutstandingAmendmentDescription));

			log.Cancel();
			AssertEquals("PreCondition: There is no outstanding amendment", false, testDec.HasOutstandingAmendment);

			result = testDec.JE_MessageStatusDescriptionIncludingOustandingAmendments;
			AssertEquals("Result should not indicate there is an outstanding amendment", false, result.Contains(testDec.OutstandingAmendmentDescription));
		}

		public void TestJE_MessageStatusDescriptionIncludingOutstandingAmendmentsNotQueued()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;

			var log = testDec.OutstandingAmendmentLogManger.AddANewLogForSaveWithoutEntryChanges(new ZDateTime(2005, 1, 1, 1, 1, 1));
			AssertEquals("PreCondition: There is an outstanding Not Queued amendment", true, testDec.HasOutstandingAmendmentNotQueued);

			var result = testDec.JE_MessageStatusDescriptionIncludingOustandingAmendments;
			AssertEquals("Result should indicate there is an outstanding Not Queued amendment", true, result.Contains(testDec.OutstandingAmendmentNotQueuedDescription));

			log.Cancel();
			AssertEquals("PreCondition: There is no outstanding amendment", false, testDec.HasOutstandingAmendmentNotQueued);

			result = testDec.JE_MessageStatusDescriptionIncludingOustandingAmendments;
			AssertEquals("Result should not indicate there is an outstanding Not Queued amendment", false, result.Contains(testDec.OutstandingAmendmentNotQueuedDescription));
		}

		public void TestJE_MessageStatusDescriptionIncludingManualAmendment()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
			var log = testDec.Logs.AddNew(Events.ManualMatchDone, "Reply to CI Amendment Received", new ZDateTimeOffset(2005, 1, 1, 1, 1, 1));
			AssertEquals("PreCondition: There is a Manual Amendment Log", true, testDec.HasOutstandingManualAmendments);
			var result = testDec.JE_MessageStatusDescriptionIncludingOustandingAmendments;
			AssertEquals("Result should indicate there is a Manual Amendment", true, result.Contains(testDec.OutstandingManualAmendmentDescription));
		}

		public void TestJE_MessageStatusDescriptionIncludingFailedAmendment()
		{
			var declaration = JobDeclarationForTest.New(Factory);
			declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
			var log = declaration.Logs.AddNew(Events.DeclarationAmendmentRejected, "Amendment rejected, no changes lodged with Customs", new ZDateTimeOffset(2005, 1, 1, 1, 1, 1));
			AssertEquals("PreCondition: There is a Failed Amendment Log", true, declaration.HasOutstandingFailedAmendments);
			Assert("Result should indicate there is a Failed Amendment", declaration.JE_MessageStatusDescriptionIncludingOustandingAmendments.Contains(declaration.AmendmentFailedDescription));
		}

		public void TestJE_MessageStatusDescriptionIncludingConsolidatedEntryChanged()
		{
			var declaration = JobDeclarationForTest.New(Factory);
			declaration.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;
			var log = declaration.Logs.AddNew(Events.ConsolidatedEntryChanged, "", new ZDateTimeOffset(2005, 1, 1, 1, 1, 1));
			AssertEquals("PreCondition: There is a Failed Amendment Log", true, declaration.HasConsolidatedEntryChanges);
			Assert("Result should indicate there is a Failed Amendment", declaration.JE_MessageStatusDescriptionIncludingOustandingAmendments.Contains(declaration.ConsolidatedEntryChangedDescription));
		}

		public void TestMessageStatusExtraDetails()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageStatus = CustomsEntryStatus.ClearFormalLodge.Code;

			var log = testDec.OutstandingAmendmentLogManger.AddANewOutstandingAmendmentLog("TESTTEST", new ZDateTime(2005, 1, 1, 1, 1, 1));
			AssertEquals("PreCondition: There is an outstanding amendment", true, testDec.HasOutstandingAmendment);

			var result = testDec.MessageStatusExtraDetails;
			AssertEquals("Result should have details about the outstanding amendment", true, result.Contains(log.SL_EventTime.ToString()));
			AssertEquals("Result should have details about the outstanding amendment", true, result.Contains("TESTTEST"));

			log.Cancel();
			AssertEquals("PreCondition: There is no outstanding amendment", false, testDec.HasOutstandingAmendment);

			result = testDec.MessageStatusExtraDetails;
			AssertEquals("Result should not have details about the outstanding amendment", false, result.Contains(log.SL_EventTime.ToString()));
			AssertEquals("Result should not have details about the outstanding amendment", false, result.Contains("TESTTEST"));
		}

		public void TestIsEntryClear()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("IsEntryClear", true, testDec.IsEntryClear);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("IsEntryClear", false, testDec.IsEntryClear);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals("IsEntryClear", true, testDec.IsEntryClear);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Rejected.Code;
			AssertEquals("IsEntryClear", false, testDec.IsEntryClear);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.ATDReceived.Code;
			AssertEquals("IsEntryClear", true, testDec.IsEntryClear);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Processing.Code;
			AssertEquals("IsEntryClear", false, testDec.IsEntryClear);
		}

		#region Test Update Container Count for Import CMR

		public void TestImportCMRDecsHaveUpdatedContainerCount()
		{
			var jobDec = CreateJobDecWithContainers();
			jobDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			Assert(!jobDec.IsDeleted);
			Factory.Save();
			AssertEquals(2, jobDec.JE_ContainerCount.ToZInt());
		}

		BaseJobDeclaration CreateJobDecWithContainers()
		{
			var jobDec = Factory.New<BaseJobDeclaration>();
			jobDec.JE_TransportMode = jobDec.TransportModeSeaCodeForTesting;
			jobDec.FillWithValidTestData();
			AssertEquals(0, jobDec.JE_ContainerCount.ToZInt());

			var container = jobDec.CusContainers.AddNew();
			container.CO_ContainerNumber = "CONT123";

			var container2 = jobDec.CusContainers.AddNew();
			container.CO_ContainerNumber = "Cont456";

			return jobDec;
		}

		public void TestUpdateJE_ContainerCount()
		{
			var jobDec = CreateJobDecWithContainers();
			AssertEquals("PreCondition: JobType Export", JobMessageTypeList.Codes.Export, jobDec.JE_MessageType);
			jobDec.JE_ContainerCount = 2;
			AssertEquals("JE_ContainerCount should not be updated when export", 2, jobDec.JE_ContainerCount.ToZInt());
			jobDec.JE_MessageType = JobMessageTypeList.Codes.Import;
			jobDec.JE_ContainerCount = 4;
			jobDec.CusContainers.AddNew();
			Factory.Save();
			AssertEquals("JE_ContainerCount automatically updated for non export jobs", 3, jobDec.JE_ContainerCount.ToZInt());
		}

		#endregion

		public void TestLastHeldMessageExtraDetails()
		{
			var startDate = new ZDateTime(2005, 12, 6);
			var testDec = Factory.New<JobDeclarationForTest>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			AssertEquals("No additional information is available.", testDec.LastHeldMessageExtraDetails);
			var header = testDec.CustomsEntryHeaders.AddNew();

			var message1 = header.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_SystemCreateTimeUtc = startDate;
			message1.EM_MessageInterpretation = "Some sort of random crap\r\nWarnings:\r\nCuckoo Squeakers are awesome";
			Factory.Save();
			AssertEquals("Warnings:\r\nCuckoo Squeakers are awesome", testDec.LastHeldMessageExtraDetails);

			var message2 = header.Messages.AddNew();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_SystemCreateTimeUtc = startDate.AddDays(1);
			message2.EM_MessageInterpretation = "Some sort of random crap\r\nCuckoo Squeakers are awesome";
			Factory.Save();
			AssertEquals("Some sort of random crap\r\nCuckoo Squeakers are awesome", testDec.LastHeldMessageExtraDetails);

			var message3 = header.Messages.AddNew();
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_SystemCreateTimeUtc = startDate.AddDays(2);
			message3.EM_MessageInterpretation = "Some sort of random crap\r\nErrors:\r\nYou don't like Cuckoo Squeakers enough";
			Factory.Save();
			AssertEquals("Errors:\r\nYou don't like Cuckoo Squeakers enough", testDec.LastHeldMessageExtraDetails);

			var message4 = header.Messages.AddNew();
			message4.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message4.EM_SystemCreateTimeUtc = startDate.AddDays(3);
			message4.EM_MessageInterpretation = "Some sort of random crap\r\nErrors:\r\nYou don't like Cuckoo Squeakers enough\r\n\r\nWarnings: Cuckoo Squeakers are Awesome!";
			Factory.Save();
			AssertEquals("Errors:\r\nYou don't like Cuckoo Squeakers enough\r\n\r\nWarnings: Cuckoo Squeakers are Awesome!", testDec.LastHeldMessageExtraDetails);

			var message5 = header.Messages.AddNew();
			message5.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message5.EM_SystemCreateTimeUtc = startDate.AddDays(4);
			message5.EM_MessageInterpretation = "Some sort of random crap\r\nWarnings:\r\nYou don't like Cuckoo Squeakers enough\r\n\r\nErrors: Cuckoo Squeakers are Awesome!";
			Factory.Save();
			AssertEquals("Warnings:\r\nYou don't like Cuckoo Squeakers enough\r\n\r\nErrors: Cuckoo Squeakers are Awesome!", testDec.LastHeldMessageExtraDetails);

			var message6 = header.Messages.AddNew();
			message6.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message6.EM_MessageType = CMRMessage.CMRMessageTypes.CARST;
			message6.EM_SystemCreateTimeUtc = startDate.AddDays(5);
			message6.EM_MessageInterpretation = "CARST message Interpretation, should be ignored!";
			Factory.Save();
			AssertEquals("Warnings:\r\nYou don't like Cuckoo Squeakers enough\r\n\r\nErrors: Cuckoo Squeakers are Awesome!", testDec.LastHeldMessageExtraDetails);

			var additionalHeader = testDec.CustomsEntryHeaders.AddNew();
			Factory.Save();
			AssertEquals("Multiple Entries Found - Please see the Entries Tab for more information.", testDec.LastHeldMessageExtraDetails);
		}

		public void TestLastHeldMessageExtraDetailsExports()
		{
			var startDate = new ZDateTime(2005, 12, 6);
			var testDec = Factory.New<JobDeclarationForTest>();
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Export;
			AssertEquals("No additional information is available.", testDec.LastHeldMessageExtraDetails);

			var message1 = testDec.Messages.AddNew();
			message1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message1.EM_SystemCreateTimeUtc = startDate;
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.WARREL;
			message1.EM_MessageInterpretation = "Some sort of random text\r\nWarnings:\r\nThe warning text.";
			AssertEquals("Warnings:\r\nThe warning text.", testDec.LastHeldMessageExtraDetails);
			message1.EM_Status = EDIMessage.Status.Discarded;
			AssertEquals("Discarded messages are ignored", "No additional information is available.", testDec.LastHeldMessageExtraDetails);
			message1.EM_Status = "CLR";
			AssertEquals("Warnings:\r\nThe warning text.", testDec.LastHeldMessageExtraDetails);
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.EXDR;
			AssertEquals("EXD replies are ignored", "No additional information is available.", testDec.LastHeldMessageExtraDetails);
			message1.EM_MessageType = CMRMessage.CMRMessageTypes.WARREL;
			AssertEquals("Warnings:\r\nThe warning text.", testDec.LastHeldMessageExtraDetails);
			var message2 = testDec.Messages.AddNew();
			message2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message2.EM_SystemCreateTimeUtc = startDate.AddSeconds(1);
			message2.EM_MessageType = CMRMessage.CMRMessageTypes.WARREL;
			message2.EM_MessageInterpretation = "Some sort of random text\r\nWarnings:\r\nThe warning text 2.";
			AssertEquals("Warnings:\r\nThe warning text 2.", testDec.LastHeldMessageExtraDetails);
			var message3 = testDec.Messages.AddNew();
			message3.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			message3.EM_SystemCreateTimeUtc = startDate.AddSeconds(-1);
			message3.EM_MessageType = CMRMessage.CMRMessageTypes.WARREL;
			message3.EM_MessageInterpretation = "Some sort of random text\r\nWarnings:\r\nThe warning text 3.";
			AssertEquals("Warnings:\r\nThe warning text 2.", testDec.LastHeldMessageExtraDetails);
		}

		public void TestIncotermAndChargeFactoryRefreshedWhenApplicationCodeSet()
		{
			var testDec = Factory.New<TestJobDec>();
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceLegacyMessages;
			testDec.RefreshIncotermAndChargeFactorCalled = false;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			AssertEquals("Refreshed", true, testDec.RefreshIncotermAndChargeFactorCalled);
		}

		public void TestBackDoorForSavingForExport()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Export;
			testDec.JE_EntryStatus = "CLO";

			var invoice = testDec.Invoices.AddNew();
			var invoiceLine = invoice.JobComInvoiceLines.AddNew();

			var testInitiator = new SendsMessagesToCustomsShutterUpperer(false);
			testInitiator.AnswerToContinueWithAction = true;
			testDec.MessageInitiator = testInitiator;

			Factory.Save();

			testDec.JE_RL_NKFinalDestination = "NZAKL";
			var result = testDec.CanContinueWithSaveSendingExportAmendmentIfNeeded();
			AssertEquals("Changed", true, testDec.DoChangesResultInADifferentMessage(ExportDeclarationType.Undefined));
			AssertEquals("Changed, but users can save", true, result);
		}

		public void TestRefreshJE_EntryStatusAndAmendmentLogsWhenMergeIsThrownAwayForCMR()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_EntryStatus = "AAA";
			var oustandingAmendmentLog = testDec.Logs.AddNew(Events.DeclarationAmendmentQueued, "TEST");
			var oustandingAmendmentLogNotQueued = testDec.Logs.AddNew(Events.DeclarationAmendedPermitApproved, "TEST");
			testDec.CustomsEntryHeaders.AddNew();
			Assert(testDec.HasOutstandingAmendment);
			Assert(testDec.HasOutstandingAmendmentNotQueued);
			testDec.ThrowAwayMerge();
			AssertEquals("Entry status", CustomsEntryStatus.NotSent.Code, testDec.JE_EntryStatus);
			Assert(!testDec.HasOutstandingAmendment);
			Assert(!testDec.HasOutstandingAmendmentNotQueued);
		}

		public void TestDeriveImportDeclarationStatusDoesNotCauseDirtyInAutoMerge()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageType = Customs.Business.JobMessageTypeList.Codes.Import;
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			MergeManagerTestHelper.InvokeNotifyThatDeclarationIsInAMergedState(testDec.MergeManager);
			Factory.Save();

			AssertEquals("Merge is not dirty", false, testDec.MergeManager.RequiresMerge);
			testDec.DeriveImportDeclarationStatus();
			AssertEquals("Merge is not dirty", false, testDec.MergeManager.RequiresMerge);
		}

		public void TestIsCurrentStatusAnAmendableOne()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew();
			message.EM_MessageText = "A";

			testDec.JE_EntryStatus = ZString.Empty;
			AssertEquals("IsCurrentStatusAnAmendableOne", false, testDec.IsCurrentStatusAnAmendableOne);

			testDec.JE_MessageStatus = CustomsEntryStatus.FailAmendment.Code;
			AssertEquals("IsCurrentStatusAnAmendableOne", true, testDec.IsCurrentStatusAnAmendableOne);

			testDec.JE_EntryStatus = CustomsEntryStatus.FailWithdrawal.Code;
			AssertEquals("IsCurrentStatusAnAmendableOne", true, testDec.IsCurrentStatusAnAmendableOne);
			testDec.JE_MessageStatus = ZString.Empty;

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Processing.Code;
			AssertEquals("IsCurrentStatusAnAmendableOne", false, testDec.IsCurrentStatusAnAmendableOne);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			AssertEquals("IsCurrentStatusAnAmendableOne", true, testDec.IsCurrentStatusAnAmendableOne);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			AssertEquals("IsCurrentStatusAnAmendableOne", true, testDec.IsCurrentStatusAnAmendableOne);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Held.Code;
			AssertEquals("IsCurrentStatusAnAmendableOne", true, testDec.IsCurrentStatusAnAmendableOne);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.MultiStatus.Code;
			AssertEquals("IsCurrentStatusAnAmendableOne", true, testDec.IsCurrentStatusAnAmendableOne);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Rejected.Code;
			AssertEquals("IsCurrentStatusAnAmendableOne", true, testDec.IsCurrentStatusAnAmendableOne);

			testDec.JE_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			AssertEquals("IsCurrentStatusAnAmendableOne", false, testDec.IsCurrentStatusAnAmendableOne);
		}

		public void TestDeriveImportDeclarationStatus()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_ApplicationCode = Core.Constants.AUCustoms.ImportMessagingMode.ForceCMRMessages;
			testDec.JE_MessageType = JobMessageTypeList.Codes.Import;

			testDec.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			var entryHeader = testDec.CustomsEntryHeaders.AddNew();
			var message = entryHeader.Messages.AddNew();
			message.EM_MessageText = "A";

			entryHeader.CH_Status = CustomsEntryStatus.NotSent.Code;
			entryHeader.CH_EntryStatus = ZString.Empty;
			AssertEquals("TestDec's MessageStatus", CustomsEntryStatus.NotSent.Code, testDec.JE_MessageStatus);
			AssertEquals("TestDec's EntryStatus", ZString.Empty, testDec.JE_EntryStatus);

			entryHeader.CH_Status = CustomsEntryStatus.ClearPreLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Processing.Code;
			testDec.DeriveImportDeclarationStatus();
			AssertEquals("TestDec's MessageStatus", CustomsEntryStatus.ClearPreLodge.Code, testDec.JE_MessageStatus);
			AssertEquals("TestDec's EntryStatus", CMRImportEntryAdvice.Processing.Code, testDec.JE_EntryStatus);

			entryHeader.CH_Status = CustomsEntryStatus.FailFormalLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Clear.Code;
			testDec.DeriveImportDeclarationStatus();
			AssertEquals("TestDec's MessageStatus", CustomsEntryStatus.FailFormalLodge.Code, testDec.JE_MessageStatus);
			AssertEquals("TestDec's EntryStatus", CMRImportEntryAdvice.Clear.Code, testDec.JE_EntryStatus);

			entryHeader.CH_Status = CustomsEntryStatus.FailSAC.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Held.Code;
			testDec.DeriveImportDeclarationStatus();
			AssertEquals("TestDec's MessageStatus", CustomsEntryStatus.FailSAC.Code, testDec.JE_MessageStatus);
			AssertEquals("TestDec's EntryStatus", CMRImportEntryAdvice.Held.Code, testDec.JE_EntryStatus);

			entryHeader.CH_Status = CustomsEntryStatus.FailPreLodge.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Finalised.Code;
			testDec.DeriveImportDeclarationStatus();
			AssertEquals("TestDec's MessageStatus", CustomsEntryStatus.FailPreLodge.Code, testDec.JE_MessageStatus);
			AssertEquals("TestDec's EntryStatus", CMRImportEntryAdvice.Finalised.Code, testDec.JE_EntryStatus);

			entryHeader.CH_Status = CustomsEntryStatus.ClearSAC.Code;
			entryHeader.CH_EntryStatus = CMRImportEntryAdvice.Withdrawn.Code;
			testDec.DeriveImportDeclarationStatus();
			AssertEquals("TestDec's MessageStatus", CustomsEntryStatus.ClearSAC.Code, testDec.JE_MessageStatus);
			AssertEquals("TestDec's EntryStatus", CMRImportEntryAdvice.Withdrawn.Code, testDec.JE_EntryStatus);

			testDec.PlaceDeclarationWorkComplete("");
			AssertEquals("TestDec's MessageStatus", CustomsEntryStatus.DeclarationWorkComplete.Code, testDec.JE_MessageStatus);
			AssertEquals("TestDec's EntryStatus", CustomsEntryStatus.DeclarationWorkComplete.Code, testDec.JE_EntryStatus);
		}

		public void TestWEADeclarationGetsImportValidation()
		{
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_MessageType = JobMessageTypeList.Codes.WarehousedByExternalAgent;
			Assert("Import validation", testDec.Validation is ImportJobDeclarationValidation);
		}

		public void TestSupportsBondedWarehousing()
		{
			var declaration = JobDeclarationForTest.New(Factory);
			declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			var importer = OrgHeader.New(Factory);
			importer.CompanyData.OB_IMUsedBondedWhs = true;
			declaration.JE_OH_Importer = importer.PK;
			AssertEquals("SupportsBondedWarehousing", true, declaration.SupportsBondedWarehousing);
		}

		public void TestSynchroniserType()
		{
			var shipment = Factory.New<ForwardingShipment>();
			var testDec = JobDeclarationForTest.New(Factory);
			testDec.JE_JS = shipment.PK;
			AssertEquals("ShipmentSynchroniser.GetType", typeof(JobDeclarationSynchroniser), testDec.ShipmentSynchroniser.GetType());
		}

		public void TestIsHouseBillContainerIsRegisteredWhenNotSAC()
		{
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Import;
			AssertEquals(true, Declaration.IsPackingGroupCollectionRegisteredEditable);

			Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.SelfAssessedClearance;
			AssertEquals(false, Declaration.IsPackingGroupCollectionRegisteredEditable);
			Declaration.JE_MessageSubType = JobDeclaration.MessageSubType.FormalEntry;
			AssertEquals(true, Declaration.IsPackingGroupCollectionRegisteredEditable);
		}

		public void TestInvoiceHeaderCollectionCount()
		{
			var invoice1 = Declaration.Invoices.AddNew();
			var invoice2 = Declaration.Invoices.AddNew();
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Export;
			Assert("Invoice1 has no errors", !invoice1.HasRowErrors);
			Assert("Invoice2 has no errors", !invoice2.HasRowErrors);
			Declaration.JE_MessageType = JobMessageTypeList.Codes.Quarantine;
			AssertHasError(Declaration.JE_MessageTypeInfo, QuarantineJobDeclarationValidation.MultiInvoicesNotAllowedForExDoc);

			invoice2.Delete();
			Declaration.Validation.ValidateJE_MessageType();
			AssertNoError(Declaration.JE_MessageTypeInfo, QuarantineJobDeclarationValidation.MultiInvoicesNotAllowedForExDoc);
		}

		#region Implementation

		JobDeclarationForTest Declaration
		{
			get
			{
				if (fDeclaration == null)
				{
					fDeclaration = JobDeclarationForTest.New(Factory);
					shutterUpperer = new SendsMessagesToCustomsShutterUpperer(false);
					fDeclaration.MessageInitiator = shutterUpperer;
				}

				return fDeclaration;
			}
		}

		JobDeclarationForTest fDeclaration;
		SendsMessagesToCustomsShutterUpperer shutterUpperer;

		public class TestJobDec : JobDeclaration
		{
			public TestJobDec(BusinessObjectFactory factory, DataRow row)
				: base(factory, row)
			{
			}

			public bool RefreshIncotermAndChargeFactorCalled;
			public override void RefreshIncotermAndChargeFactory()
			{
				RefreshIncotermAndChargeFactorCalled = true;
				base.RefreshIncotermAndChargeFactory();
			}
		}

		#endregion

	}

	partial class JobDeclarationForTest : JobDeclaration
	{
		public JobDeclarationForTest(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public static new JobDeclarationForTest New(BusinessObjectFactory factory)
		{
			return factory.New<JobDeclarationForTest>();
		}

		new internal void DeriveImportDeclarationStatus() => base.DeriveImportDeclarationStatus();
		new internal bool IsPackingInformationRelevantCore => base.IsPackingInformationRelevantCore;
		new internal bool IsPackingGroupCollectionRegisteredEditable => base.IsPackingGroupCollectionRegisteredEditable;
		new internal bool ShouldKeepDeletedLinesOnAmendmentCore => base.IsPackingGroupCollectionRegisteredEditable;
	}
}
