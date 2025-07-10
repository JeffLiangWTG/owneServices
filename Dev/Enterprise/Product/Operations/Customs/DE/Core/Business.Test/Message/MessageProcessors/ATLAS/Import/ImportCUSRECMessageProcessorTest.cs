using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.DE;
using Enterprise.Customs.DE.Business.MonthlyClosing;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.DataObjects.Universal.Customs;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportCUSRECMessageProcessor))]
	sealed class ImportCUSRECMessageProcessorTest : ImportMessageProcessorAffectingDeclarationAbstractTest<ImportCUSRECMessageProcessor, AtlasInboundEDIMessage<ICUSREC>>
		, ITestEntryLinesLockedAfterProcessing
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType<CusEntryHeader>("Type is CusEntryHeader", Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestUpdateWarehouse_ShouldNotBeTriggered_WhenCEI_OA_Warehouse2IsEmpty_IntoWarehouseWarehousing()
		{
			var testCaseIndex = 0;
			var testCases = new[] { (warehouse2IsEmpty: true, expectUpdateWarehouseTriggered: false), (warehouse2IsEmpty: false, expectUpdateWarehouseTriggered: true) };
			foreach ((bool warehouse2IsEmpty, bool expectUpdateWarehouseTriggered) in testCases)
			{
				testCaseIndex++;
				RunTestCase(warehouse2IsEmpty, expectUpdateWarehouseTriggered);
			}

			void RunTestCase(bool warehouse2IsEmpty, bool expectUpdateWarehouseTriggered)
			{
				var referenceNumber = $"ATE15000062052019587{testCaseIndex}";
				var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
				orgHeader.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEAUTH123", CusAuthorisationRuleTypeList.Codes.Release,
					CusAuthorisationReleaseRuleList.Codes._1);

				var testHelper = new WhsDataTestHelper(Factory);
				var declarationToTest = testHelper.GetNewDeclarationWithInstruction(Factory, "IMP", $"B0001{testCaseIndex}", referenceNumber, 20, true);
				var activeEntryHeader = (CusEntryHeader)declarationToTest.ActiveEntryHeaders.Single();
				var invoiceLine = declarationToTest.InvoiceLines[0];
				invoiceLine.CusEntryLine.ZG_CustomsStatus = "TX6";
				activeEntryHeader.EntryNumberInfo.ClearValue();
				AssertEquals("Precondition: Into Bonded Warehouse", true, activeEntryHeader.IsIntoWarehouseWarehousing);

				Factory.Save();

				dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);

				var authorizationUsage = declarationToTest.CustomsEntryInstructions[0].CusAuthorizationUsages.AddNew();
				authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
				authorizationUsage.AGC_OH_Owner = orgHeader.PK;

				if (warehouse2IsEmpty)
				{
					activeEntryHeader.EntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				}

				outgoingMessage.EM_LinkedObject = activeEntryHeader;
				Factory.Save();

				var cusReconEntry = Factory.New<CusReconEntry>();
				cusReconEntry.CRE_CH_OriginalEntry = activeEntryHeader.PK;
				cusReconEntry.CRE_EntryType = CusReconConstants.Lodged;
				cusReconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

				ProcessMessage(messageMock.Object, true);

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
				var dataTransferEvents = activeEntryHeader.Logs.Find(query);
				AssertEquals(expectUpdateWarehouseTriggered, dataTransferEvents.Any(x => x.RelatedEDIMessage.Message.EM_MessageSubType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));
			}
		}

		public void TestUpdateWarehouse_IntoWarehouseWarehousing()
		{
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "TX6", true, iCancel: false);
			const string referenceNumber = "ATE150000620520195874";
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEAUTH123", CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);

			var testHelper = new WhsDataTestHelper(Factory);
			declaration = testHelper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0001", referenceNumber, 20, true);
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.Single();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.CusEntryLine.ZG_CustomsStatus = "TX6";
			entryHeader.EntryNumberInfo.ClearValue();
			AssertEquals("Precondition: Into Bonded Warehouse", true, declaration.ActiveEntryHeaders[0].IsIntoWarehouseWarehousing);

			Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishShipmentForWHSInward(entryHeader, false);
			Factory.Save();
			var entryInstruction = declaration.CustomsEntryInstructions[0];

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			authorizationUsage.AGC_OH_Owner = orgHeader.PK;

			outgoingMessage.EM_LinkedObject = entryHeader;
			Factory.Save();

			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			cusReconEntry.CRE_EntryType = CusReconConstants.Lodged;
			cusReconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			var entryLine = cusReconEntry.CusReconEntryLines.AddNew();
			entryLine.CRL_OriginalEntryLineNumber = 1;

			CombineAssertions(() =>
			{
				var msg = messageMock.Object;
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 20);

				ProcessMessage(msg, true);
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
				var dataTransferEvents = entryHeader.Logs.Find(query);

				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, msg.EM_Status);
				AssertEquals("EntryStatus", UniversalReferenceConstants.EntryStatus.TX7, entryLine.CRL_CustomsStatus);

				AssertEquals(2, dataTransferEvents.Count(x => x.RelatedEDIMessage.Message.EM_MessageSubType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));

				AssertEquals(Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, entryHeader.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryHeader.EntryNumber, invoiceLine.CusEntryLine.CL_LineNumber, 20);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase("<PendingCustomsResponse>-1", 0);
			});
		}

		[GuiTest]
		public void TestUpdateWarehouse_OutOfWarehouseWarehousing()
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CreateAuthorisationWithRule(CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, "DEAUTH123", CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);

			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "TX6", true, iCancel: false);
			const string referenceNumber = "ATE150000620520195874";
			const string prevReferenceNumber = "ATE150000620520195873";

			var testHelper = new WhsDataTestHelper(Factory);
			var inwardDecl = testHelper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0001", prevReferenceNumber, 20, true);
			var inwardEntryHeader = (CusEntryHeader)inwardDecl.ActiveEntryHeaders.Single();
			inwardDecl.InvoiceLines[0].CusEntryLine.ZG_CustomsStatus = "TX6";

			Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishShipmentForWHSInward(inwardEntryHeader, false);
			Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishAcceptEventForWHSInwardAndSaveIfNeeded(inwardEntryHeader);

			Factory.Save();

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryHeader.EntryNumber, 1, 20);

			declaration = testHelper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0002", referenceNumber, 1, false, prevReferenceNumber);
			AssertEquals("Precondition: OutOf Bonded Warehouse", true, declaration.ActiveEntryHeaders[0].IsOutOfWarehouseWarehousing);
			var entryInstruction = declaration.CustomsEntryInstructions[0];

			declaration.InvoiceLines[0].CusEntryLine.ZG_CustomsStatus = "TX6";
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.Single();
			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			authorizationUsage.AGC_OH_Owner = orgHeader.PK;

			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			cusReconEntry.CRE_EntryType = CusReconConstants.Lodged;
			cusReconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			var entryLine = cusReconEntry.CusReconEntryLines.AddNew();
			entryLine.CRL_OriginalEntryLineNumber = 1;

			outgoingMessage.EM_LinkedObject = entryHeader;
			Factory.Save();

			CombineAssertions(() =>
			{
				var msg = messageMock.Object;

				ProcessMessage(msg, true);
				var dataTransferEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
				var dataTransferEvents = declaration.Logs.Find(dataTransferEventQuery);

				var finalizedEventQuery = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.WarehouseJobCanNowBeFinalisedCode);
				var finalizedEvents = declaration.Logs.Find(finalizedEventQuery);

				AssertEquals("EM_Status", EDIMessageStatusList.Codes.ProcessedOK, msg.EM_Status);
				AssertEquals("EntryStatus", UniversalReferenceConstants.EntryStatus.TX7, entryHeader.CH_EntryStatus);

				AssertEquals(1, dataTransferEvents.Count(x => x.RelatedEDIMessage.Message.EM_MessageSubType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));
				AssertEquals(1, finalizedEvents.Length);

				AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.JE_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryHeader.EntryNumber, 1, 19);
			});
		}

		public void TestShouldPublishWhsOutwardAcceptEvent()
		{
			const string referenceNumber = "ATE150000620520195874";
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);

			var orgHeader = Factory.New<OrgHeader>();
			orgHeader.OH_Code = "ORG123";

			var testHelper = new WhsDataTestHelper(Factory);
			declaration = testHelper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0002", referenceNumber, 1, false);
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.Single();

			outgoingMessage.EM_LinkedObject = entryHeader;
			var message = messageMock.Object;

			var processor = Processor;
			processor.PreProcessMessage(message);

			var count = 0;
			foreach (var (expectedShouldPublish, entryStatus, authorizationType, ruleCode, ruleValue) in GetExpectedShouldPublishAcceptEvent())
			{
				var authNumber = $"DEAUTH12{count++}";
				orgHeader.CreateAuthorisationWithRule(authorizationType, authNumber, ruleCode, ruleValue);

				entryHeader.CH_EntryStatus = entryStatus;
				var entryInstruction = declaration.CustomsEntryInstructions[0];
				var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();

				authorizationUsage.AGC_Code = authorizationType;
				authorizationUsage.AGC_OH_Owner = orgHeader.PK;
				authorizationUsage.AGC_Number = authNumber;

				Factory.Save();

				var shouldPublishAcceptEvent = processor.ShouldPublishWhsOutwardAcceptEvent(entryHeader);
				AssertEquals($"Should publish accept event for {entryStatus}", expectedShouldPublish, shouldPublishAcceptEvent);

				entryInstruction.CusAuthorizationUsages.RemoveAndDeleteAll();
				Factory.Save();
			}

			IEnumerable<(bool shouldPublishAccept, string entryStatus, string authorizationType, string ruleCode, string ruleValue)> GetExpectedShouldPublishAcceptEvent()
			{
				yield return (false, UniversalReferenceConstants.EntryStatus.TX1, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX2, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX3, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX4, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX5, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX6, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);
				yield return (true, UniversalReferenceConstants.EntryStatus.TX7, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX7, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._2);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX7, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Usage, CusAuthorisationReleaseRuleList.Codes._1);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX7, CusAuthorizationHeaderTypeList.Codes.CustomsValue, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX8, CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords, CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1);
			}
		}

		public void TestGetLinkedObjectNoReferencedMessageIdentifier()
		{
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ZString.Empty);
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType<CusEntryHeader>("Type is CusEntryHeader", Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestGetLinkedObjectMRN()
		{
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
			entryHeader.CusEntryNumber.CE_EntryNum = "24DE12345678901234";
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType<CusEntryHeader>("Type is CusEntryHeader", Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferencedMessageIdentifier).Returns("ABC000000");
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertNull("No LinkedObject", Message.EM_LinkedObject);
				AssertEquals("Message status == error", EDIMessage.Status.Error, Message.EM_Status);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSREC)null);
			AssertNoExceptionThrown(() => ProcessMessage(Message));
		}

		public void TestWithEmptyReferenceNumber()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(m => m.MRN).Returns((string)null);
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("The prepared data provider should not provide any error notification", false, dataProviderMock.Object.NotificationSeverity.Any(x => x == NotificationTypeList.Codes.Error));
				AssertEquals(nameof(entryHeader.CH_EntryStatus), UniversalReferenceConstants.EntryStatus.REJ, entryHeader.CH_EntryStatus);
				AssertEquals(nameof(entryHeader.CH_Status), EDIMessage.Status.Rejected, entryHeader.CH_Status);
			});
		}

		public void TestProcessMessage()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.RC2, entryHeader.CH_EntryStatus);
				AssertEquals("CH_Status", EDIMessageStatusList.Codes.Received, entryHeader.CH_Status);
				AssertCusEntryNum(true, "ATB150000620520195875");

				dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATA000000000000000000");
				ProcessMessage(Message);
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.RC1, entryHeader.CH_EntryStatus);
				AssertEquals("CH_Status", EDIMessageStatusList.Codes.Received, entryHeader.CH_Status);
				AssertCusEntryNum(true, "ATA000000000000000000");

				dataProviderMock.Setup(m => m.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Error });
				ProcessMessage(Message);
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.REJ, entryHeader.CH_EntryStatus);
				AssertEquals("CH_Status", EDIMessageStatusList.Codes.Rejected, entryHeader.CH_Status);
				AssertCusEntryNum(false);
			});
		}

		public void TestProcessMessageMRN()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.RC2, entryHeader.CH_EntryStatus);
				AssertEquals("CH_Status", EDIMessageStatusList.Codes.Received, entryHeader.CH_Status);
				AssertCusEntryNum(true, "ATB150000620520195875");

				dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
				dataProviderMock.Setup(m => m.MRN).Returns("24DE56789A12345678");
				ProcessMessage(Message);
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.RC1, entryHeader.CH_EntryStatus);
				AssertEquals("CH_Status", EDIMessageStatusList.Codes.Received, entryHeader.CH_Status);
				AssertCusEntryNum(true, "24DE56789A12345678");

				dataProviderMock.Setup(m => m.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Error });
				ProcessMessage(Message);
				AssertEquals("EM_Status", EDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.REJ, entryHeader.CH_EntryStatus);
				AssertEquals("CH_Status", EDIMessageStatusList.Codes.Rejected, entryHeader.CH_Status);
				AssertCusEntryNum(false);
			});
		}

		public void TestEventsCreated()
		{
			ProcessMessage(Message);
			var entryStatus = entryHeader.CH_EntryStatus;
			AssertEquals(entryStatus, entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestEventsCreatedOnUpdate()
		{
			var referenceNumber = "ATE150000620520195875";
			var authorizationType = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			var ruleCode = CusAuthorisationRuleTypeList.Codes.Release;
			var valueFrom = CusAuthorisationReleaseRuleList.Codes._1;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CreateAuthorisationWithRule(authorizationType, "DEAUTH123", ruleCode, valueFrom);
			Factory.Save();

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 02, 15, 10, 39, 41);
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			cusReconEntry.CRE_OriginalEntryNumber = "ABC";
			cusReconEntry.CRE_EntryDate = new ZDate(2020, 01, 01);

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = authorizationType;
			authorizationUsage.AGC_Number = "DEAUTH123";
			authorizationUsage.AGC_OH_Owner = orgHeader.PK;

			ProcessMessage(Message);

			AssertEquals(UniversalReferenceConstants.EntryStatus.TX7, cusReconEntry.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CUSREC – Customs Receipt Message Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CUSREC – Customs Receipt Message Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Receipt Message. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>MRN</td><td>24DE12345678901234</td></tr>"
								   + "<tr><td>Registration Number</td><td>ATB150000620520195875</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
								   + "<tr><td>Registration Date</td><td>17.09.2020</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_EmptyFieldsNotShown()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(m => m.MRN).Returns((string)null);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.RegistrationDate).Returns(ZDate.Empty);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			CombineAssertions(() =>
			{
				AssertNotContains("No Registration Number", "<td>Registration Number</td>", email.Body);
				AssertNotContains("No MRN", "<td>MRN</td>", email.Body);
				AssertNotContains("No Local Reference Number", "<td>Local Reference Number</td>", email.Body);
				AssertNotContains("No Registration Date", "<td>Registration Date</td>", email.Body);
				AssertNotContains("Table is not shown when it has no rows", @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">", email.Body);
			});
		}

		public void TestPopulateLogbookRegNumAndMrn()
		{
			ProcessMessage(Message);
			AssertEquals("ATB150000620520195875, 24DE12345678901234", Message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNum()
		{
			dataProviderMock.Setup(m => m.MRN).Returns((string)null);
			ProcessMessage(Message);
			AssertEquals("ATB150000620520195875", Message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookMRN()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
			ProcessMessage(Message);
			AssertEquals(MRN, Message.GetLogbookRegistrationNumber());
		}

		public void TestUpdateCusReconEntryAndLines()
		{
			AssertCusReconEntryAndLinesUpdated("ATE150000620520195875",
				CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords,
				CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1, true);
		}

		public void TestUpdateCusReconEntryAndLines_InvalidReferenceNumber()
		{
			AssertCusReconEntryAndLinesUpdated("ATD150000620520195875",
				CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords,
				CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1, false);
		}

		public void TestUpdateCusReconEntryAndLines_InvalidAuthorizationType()
		{
			AssertCusReconEntryAndLinesUpdated("ATE150000620520195875",
				CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTir,
				CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._1, false);
		}

		public void TestUpdateCusReconEntryAndLines_InvalidAuthorizationRuleCode()
		{
			AssertCusReconEntryAndLinesUpdated("ATE150000620520195875",
				CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords,
				CusAuthorisationRuleTypeList.Codes.BusinessReference, CusAuthorisationReleaseRuleList.Codes._1, false);
		}

		public void TestUpdateCusReconEntryAndLines_InvalidAuthorizationRuleValue()
		{
			AssertCusReconEntryAndLinesUpdated("ATE150000620520195875",
				CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords,
				CusAuthorisationRuleTypeList.Codes.Release, CusAuthorisationReleaseRuleList.Codes._2, false);
		}

		public void TestCusReconEntryNotDeletedWhenHeaderErrorAndEntryHeaderHasMrn()
		{
			dataProviderMock.Setup(m => m.NotificationSeverity).Returns([NotificationTypeList.Codes.Error]);
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var cusReconEntrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();
			var cusReconEntryLine1 = cusReconEntry.CusReconEntryLines.AddNew();
			var cusReconEntryLine2 = cusReconEntry.CusReconEntryLines.AddNew();
			var cusReconEntryLineSnapshot = cusReconEntryLine2.CusReconSnapshots.AddNew();

			AssertEquals("Precondition", "ATB150000620520195875", entryHeader.MovementReferenceNumber);

			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("cusReconEntry not deleted", false, cusReconEntry.IsDeleted);
				AssertEquals("cusReconEntrySnapshot not deleted", false, cusReconEntrySnapshot.IsDeleted);
				AssertEquals("cusReconEntryLine1 not deleted", false, cusReconEntryLine1.IsDeleted);
				AssertEquals("cusReconEntryLine2 not deleted", false, cusReconEntryLine2.IsDeleted);
				AssertEquals("cusReconEntryLineSnapshot not deleted", false, cusReconEntryLineSnapshot.IsDeleted);
			});
		}

		public void TestCusReconEntryIsDeletedWhenHeaderErrorAndEntryHeaderHasNoMrn()
		{
			entryHeader.CusEntryNumber.Delete();

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(m => m.NotificationSeverity).Returns([NotificationTypeList.Codes.Error]);
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var cusReconEntrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();
			var cusReconEntryLine1 = cusReconEntry.CusReconEntryLines.AddNew();
			var cusReconEntryLine2 = cusReconEntry.CusReconEntryLines.AddNew();
			var cusReconEntryLineSnapshot = cusReconEntryLine2.CusReconSnapshots.AddNew();

			AssertEquals("Precondition", ZString.Empty, entryHeader.MovementReferenceNumber);

			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("cusReconEntry deleted", true, cusReconEntry.IsDeleted);
				AssertEquals("cusReconEntrySnapshot deleted", true, cusReconEntrySnapshot.IsDeleted);
				AssertEquals("cusReconEntryLine1 deleted", true, cusReconEntryLine1.IsDeleted);
				AssertEquals("cusReconEntryLine2 deleted", true, cusReconEntryLine2.IsDeleted);
				AssertEquals("cusReconEntryLineSnapshot deleted", true, cusReconEntryLineSnapshot.IsDeleted);
			});
		}

		public void TestCusReconEntryDeletedWhenReferenceNumberAndMrnEmptyInMessage()
		{
			entryHeader.CusEntryNumber.Delete();

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(m => m.MRN).Returns((string)null);
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var cusReconEntrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();
			var cusReconEntryLine1 = cusReconEntry.CusReconEntryLines.AddNew();
			var cusReconEntryLine2 = cusReconEntry.CusReconEntryLines.AddNew();
			var cusReconEntryLineSnapshot = cusReconEntryLine2.CusReconSnapshots.AddNew();

			AssertEquals("Precondition", ZString.Empty, entryHeader.MovementReferenceNumber);

			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("cusReconEntry deleted", true, cusReconEntry.IsDeleted);
				AssertEquals("cusReconEntrySnapshot deleted", true, cusReconEntrySnapshot.IsDeleted);
				AssertEquals("cusReconEntryLine1 deleted", true, cusReconEntryLine1.IsDeleted);
				AssertEquals("cusReconEntryLine2 deleted", true, cusReconEntryLine2.IsDeleted);
				AssertEquals("cusReconEntryLineSnapshot deleted", true, cusReconEntryLineSnapshot.IsDeleted);
			});
		}

		public void TestProcessMessage_UpdateRegistrationNumbersInRelatedOrder()
		{
			var testHelper = new WhsDataTestHelper(Factory);

			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATE150000620520195875");

			declaration = testHelper.GetNewDeclaration("IMP", "DECL1234", "ATE150000620520195875", 5.0m);
			declaration.IsOutwardOrderImported = true;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			entryInstruction.CEI_Procedure = intoWarehouseWarehousingProcedureCode.Left(2);
			entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;

			entryHeader = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX7;
			entryInstruction.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;
			outgoingMessage.EM_LinkedObject = entryHeader;

			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			cusReconEntry.CRE_EntryType = CusReconConstants.Lodged;
			cusReconEntry.CRE_OA_DeclarantAddress = Factory.NewWithValidTestData<OrgAddress>().PK;

			var (entryLine1, entryLine2) = CreateEntryLinesAndLinkedInvoicesForWarehouseTest(entryHeader, declaration, entryInstruction, testHelper.Part, outOfWarehouseWarehousingProcedureCode);

			var order = testHelper.WhsHelper.CreateWhsOrder(testHelper.Importer.PK, testHelper.WhsWarehouse.PK, testHelper.Importer.PK, "OrderRef") as WhsOrder;

			var orderJobPivot = Factory.NewWithValidTestData<WhsDocketJobPivot>();
			orderJobPivot.WV_ParentId = declaration.PK;
			orderJobPivot.WV_ParentTableCode = declaration.TablePrefix;
			orderJobPivot.WV_WD_Docket = order.PK;

			var orderLine1 = testHelper.WhsHelper.CreateWhsOrderLine(order.PK, testHelper.Part.PK, 1, "1234", "old value 1", ZString.Empty, ZString.Empty, ZString.Empty, 100m, 20m, "KG", 0, ZString.Empty, ZString.Empty, 1, ZGuid.Empty, ZString.Empty) as WhsOrderLine;
			orderLine1.WE_WB_CustomsData = orderLine1.CustomsData.PK;
			var orderLine2 = testHelper.WhsHelper.CreateWhsOrderLine(order.PK, testHelper.Part.PK, 1, "1234", "old value 2", ZString.Empty, ZString.Empty, ZString.Empty, 100m, 20m, "KG", 0, ZString.Empty, ZString.Empty, 1, ZGuid.Empty, ZString.Empty) as WhsOrderLine;
			orderLine2.WE_WB_CustomsData = orderLine2.CustomsData.PK;

			var authorizationType = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			var ruleCode = CusAuthorisationRuleTypeList.Codes.Release;
			var valueFrom = CusAuthorisationReleaseRuleList.Codes._1;
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CreateAuthorisationWithRule(authorizationType, "DEAUTH123", ruleCode, valueFrom);

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = CusAuthorizationHeaderTypeList.Codes.EntryOfDataInTheDeclarantsRecords;
			authorizationUsage.AGC_OH_Owner = orgHeader.PK;

			Factory.Save(); // populate DocketId

			var invoiceLine1 = declaration.InvoiceLines[0];
			invoiceLine1.JI_BondedWHSOrderNumber = order.WD_DocketID;
			invoiceLine1.JI_BondedWHSOrderLineNumber = 000001;

			var invoiceLine2 = declaration.InvoiceLines[1];
			invoiceLine2.JI_BondedWHSOrderNumber = order.WD_DocketID;
			invoiceLine2.JI_BondedWHSOrderLineNumber = 000002;

			CombineAssertions(() =>
			{
				ProcessMessage(Message, doSave: true);

				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, Message.EM_Status);
				AssertEquals("atribute 1 updated: EntryKey", "ATE150000620520195875", orderLine1.CustomsData.WB_EntryKey);
				AssertEquals("atribute 2 updated: LineNo", (short)1, orderLine1.CustomsData.WB_EntryLineNo);

				AssertEquals("atribute 2 updated: EntryKey", "ATE150000620520195875", orderLine2.CustomsData.WB_EntryKey);
				AssertEquals("atribute 2 updated: LineNo", (short)2, orderLine2.CustomsData.WB_EntryLineNo);
			});
		}

		public void TestUpdateWarehouse_WarehouseAdjustment()
		{
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL4", false, iCancel: true);
			CreateWarehouseAdjustmentRefCusProcedure();

			var testHelper = new WhsDataTestHelper(Factory);
			var jobDeclaration = testHelper.GetNewDeclaration("WAD", "DECL1234", "23DE1234567890", 5.0m);

			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var cusEntryHeader = (CusEntryHeader)jobDeclaration.ActiveEntryHeaders[0];
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "23DE1234567890";
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_Style = ImportEntryTypeList.Codes.CollectiveClearanceCustomsWarehouse;
			entryInstruction.CEI_OA_Warehouse = testHelper.WhsWarehouse.WW_OA_WarehouseAddress;
			cusEntryHeader.CH_WarehouseTransactionStatus = "OCP";

			outgoingMessage.EM_LinkedObject = cusEntryHeader;

			CreateEntryLinesAndLinkedInvoicesForWarehouseTest(cusEntryHeader, jobDeclaration, entryInstruction, testHelper.Part, warehouseAdjustmentProcedureCode);

			Factory.Save();
			var line1Mock = new Mock<ICUSRECGoodsItem>();
			line1Mock.Setup(l => l.SequenceNumber).Returns("2");
			line1Mock.Setup(l => l.NotificationSeverity).Returns("WRG");

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { line1Mock.Object });
			ProcessMessage(Message, true);

			CombineAssertions(() =>
			{
				var universalShipmentMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "UDM"));
				AssertNotNull("shipment has been exported", universalShipmentMessage);
				var parsedShipment = universalShipmentMessage.GetEM_MessageTextReader().Parse<Shipment>();
				var actualNumberOfInvoiceLines = parsedShipment.CommercialInfo.CommercialInvoiceCollection.SelectMany(i => i.CommercialInvoiceLineCollection ?? Enumerable.Empty<CommercialInvoiceLine>()).Count();
				AssertEquals("only one line exported", 1, actualNumberOfInvoiceLines);
			});
		}

		public void TestUpdateWarehouse_WarehouseAdjustment_REJ()
		{
			CreateWarehouseAdjustmentRefCusProcedure();

			var testHelper = new WhsDataTestHelper(Factory);
			var jobDeclaration = testHelper.GetNewDeclaration("WAD", "DECL1234", "23DE1234567890", 5.0m);

			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			var cusEntryHeader = (CusEntryHeader)jobDeclaration.ActiveEntryHeaders[0];
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "23DE1234567890";
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_Style = ImportEntryTypeList.Codes.CollectiveClearanceCustomsWarehouse;
			entryInstruction.CEI_OA_Warehouse = testHelper.WhsWarehouse.WW_OA_WarehouseAddress;
			cusEntryHeader.CH_WarehouseTransactionStatus = "OCP";

			outgoingMessage.EM_LinkedObject = cusEntryHeader;

			CreateEntryLinesAndLinkedInvoicesForWarehouseTest(cusEntryHeader, jobDeclaration, entryInstruction, testHelper.Part, warehouseAdjustmentProcedureCode);

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(m => m.MRN).Returns((string)null);

			Factory.Save();
			ProcessMessage(Message, true);

			var universalShipmentMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "UDM"));
			AssertNull("no shipment has been exported", universalShipmentMessage);
		}

		public void TestLineStatusIfLinesHaveErrorForWarehouseAdjustment()
		{
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			var entryLine4 = entryHeader.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 4;

			declaration.JE_MessageType = DEJobMessageTypeList.Codes.WarehouseAdjustment;

			var line1Mock = new Mock<ICUSRECGoodsItem>();
			line1Mock.Setup(l => l.SequenceNumber).Returns("1");
			line1Mock.Setup(l => l.NotificationSeverity).Returns("INF");
			var line2Mock = new Mock<ICUSRECGoodsItem>();
			line2Mock.Setup(l => l.SequenceNumber).Returns("2");
			line2Mock.Setup(l => l.NotificationSeverity).Returns("WRG");
			var line3Mock = new Mock<ICUSRECGoodsItem>();
			line3Mock.Setup(l => l.SequenceNumber).Returns("4");
			line3Mock.Setup(l => l.NotificationSeverity).Returns("ERR");

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { line1Mock.Object, line2Mock.Object, line3Mock.Object });

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("Line 1", "RC2", entryLine1.ZG_CustomsStatus);
				AssertEquals("Line 2", "RL4", entryLine2.ZG_CustomsStatus);
				AssertEquals("Line 3", "RC2", entryLine3.ZG_CustomsStatus);
				AssertEquals("Line 4", "RL4", entryLine4.ZG_CustomsStatus);
			});
		}

		(CusEntryLine, CusEntryLine) CreateEntryLinesAndLinkedInvoicesForWarehouseTest(CusEntryHeader cusEntryHeader, JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction, OrgSupplierPart part, ZString procedureCode)
		{
			var entryLine1 = cusEntryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var entryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var invHeader1 = jobDeclaration.Invoices.AddNew();
			invHeader1.JZ_InvoiceNumber = "1";

			cusEntryHeader.MergedLines.AddNew();
			var invoiceLine = cusEntryHeader.InvoiceLines.Single();
			invoiceLine.JI_CEI = entryInstruction.PK;
			invoiceLine.JI_CL = entryLine1.PK;
			invoiceLine.JI_Procedure = procedureCode;

			var invHeader2 = jobDeclaration.Invoices.AddNew();
			invHeader2.JZ_InvoiceAmount = 6 * 100m;
			invHeader2.JZ_InvoiceNumber = "2";

			var invoiceLine2 = invHeader2.JobComInvoiceLines.AddNew();
			invoiceLine2.SetIsGoingIntoBondedWarehouseCoreForTesting(true);
			invoiceLine2.JI_PartNo = part.OP_PartNum;
			invoiceLine2.JI_InvoiceQuantity = 6.0m;
			invoiceLine2.JI_InvoiceUQ = "NO";
			invoiceLine2.JI_CustomsUnitQty = "KG";
			invoiceLine2.JI_CustomsQuantity = 60;
			invoiceLine2.JI_LinePrice = 600;
			invoiceLine2.JI_CL = entryLine2.PK;
			invoiceLine2.JI_Procedure = procedureCode;
			invoiceLine2.JI_CEI = entryInstruction.PK;

			return (entryLine1, entryLine2);
		}

		protected override ZString MessageFriendlyName => "Import CUSREC Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSREC>> Processor => new ImportCUSRECMessageProcessor(logger);

		protected override AtlasInboundEDIMessage<ICUSREC> Message => messageMock.Object;

		public string DeclarationMessageType => Messaging.EDIMessageTypeList.Codes.Import;

		EDIMessage ITestEntryLinesLockedAfterProcessing.PrepareMessagesAndGetMessageToProcessForEntryLinesLockedTest(CusEntryHeader entryHeader)
		{
			var outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(entryHeader, "1122334455");
			outgoingMessage.EM_ApplicationCode = "DEA";
			outgoingMessage.EM_MessageType = Messaging.EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			entryHeader.Messages.Add(outgoingMessage);
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns((string)null);
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("1122334455");
			Factory.Save();

			return Message;
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";

			outgoingMessage = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(entryHeader, "outgoing");
			outgoingMessage.EM_ApplicationCode = "DEA";
			outgoingMessage.EM_MessageType = Messaging.EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_MessageNum = "ABC123456";

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSREC>>();
			dataProviderMock = new Mock<ICUSREC>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195875");
			dataProviderMock.Setup(x => x.MRN).Returns(MRN);
			dataProviderMock.Setup(x => x.ReferencedMessageIdentifier).Returns("ABC123456");
			dataProviderMock.Setup(x => x.NotificationSeverity).Returns(new ZString[] { NotificationTypeList.Codes.Information });
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("MAS/22/11/22027");
			dataProviderMock.Setup(m => m.RegistrationDate).Returns(new ZDate(2020, 9, 17));
			messageMock.Setup(x => x.DataProvider).Returns(dataProviderMock.Object);

			Factory.Save();
		}
		CusEntryHeader entryHeader;
		Mock<ICUSREC> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICUSREC>> messageMock;
		EDIMessage outgoingMessage;

		readonly ZString outOfWarehouseWarehousingProcedureCode = "4071";
		readonly ZString intoWarehouseWarehousingProcedureCode = "7100";
		readonly ZString warehouseAdjustmentProcedureCode = "10";
		const string MRN = "24DE12345678901234";

		void AssertCusEntryNum(bool expectToExist, string expectedEntryNum = "")
		{
			var cusEntryNum = CusEntryNumber.Load(entryHeader, "MRN", Core.Constants.CountryCodes.Germany);
			if (expectToExist)
			{
				AssertNotNull("CusEntryNum exists", cusEntryNum);
				AssertEquals("CE_EntryNum", expectedEntryNum, cusEntryNum.CE_EntryNum);
				AssertEquals("CE_Category", "CUS", cusEntryNum.CE_Category);
				AssertEquals("CE_IssueDate", new ZDate(2020, 9, 17), cusEntryNum.CE_IssueDate);
				AssertEquals("CE_RN_NKCountryCode", Core.Constants.CountryCodes.Germany, cusEntryNum.CE_RN_NKCountryCode);
				AssertEquals("CE_EntryIsSystemGenerated", true, cusEntryNum.CE_EntryIsSystemGenerated);
				cusEntryNum.Delete();
			}
			else
			{
				AssertNull("CusEntryNum doesn't exist", cusEntryNum);
			}
		}

		void AssertCusReconEntryAndLinesUpdated(ZString referenceNumber, ZString authorizationType, ZString ruleCode, ZString valueFrom, bool cusReconEntryAndLinesUpdated)
		{
			var orgHeader = Factory.NewWithValidTestData<OrgHeader>();
			orgHeader.CreateAuthorisationWithRule(authorizationType, "DEAUTH123", ruleCode, valueFrom);
			Factory.Save();

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_LocalClearanceDate = new ZDateTime(2021, 02, 15, 10, 39, 41);
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			var entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX5;
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.RC2;
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			cusReconEntry.CRE_OriginalEntryNumber = "ABC";
			cusReconEntry.CRE_EntryDate = new ZDate(2020, 01, 01);
			var cusReconEntrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();
			var cusReconEntryLine1 = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine1.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX5;
			var cusReconEntryLine2 = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine2.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX6;
			var cusReconEntryLineSnapshot = cusReconEntryLine2.CusReconSnapshots.AddNew();

			var authorizationUsage = entryInstruction.CusAuthorizationUsages.AddNew();
			authorizationUsage.AGC_Code = authorizationType;
			authorizationUsage.AGC_Number = "DEAUTH123";
			authorizationUsage.AGC_OH_Owner = orgHeader.PK;
			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				if (cusReconEntryAndLinesUpdated)
				{
					AssertEquals("cusReconEntry.CRE_OriginalEntryNumber updated", "ATE150000620520195875", cusReconEntry.CRE_OriginalEntryNumber);
					AssertEquals("cusReconEntry.CRE_EntryDate updated", new ZDate(2021, 02, 15), cusReconEntry.CRE_EntryDate);
					AssertEquals("cusReconEntryLine1.CRL_CustomsStatus updated", UniversalReferenceConstants.EntryStatus.TX7, cusReconEntryLine1.CRL_CustomsStatus);
					AssertEquals("cusReconEntryLine2.CRL_CustomsStatus updated", UniversalReferenceConstants.EntryStatus.TX7, cusReconEntryLine2.CRL_CustomsStatus);
					AssertEquals("entry line status updated", UniversalReferenceConstants.EntryStatus.TX7, entryLine.ZG_CustomsStatus);
					AssertEquals("entry status updated", UniversalReferenceConstants.EntryStatus.TX7, entryHeader.CH_EntryStatus);
				}
				else
				{
					AssertEquals("cusReconEntry.CRE_OriginalEntryNumber not updated", "ABC", cusReconEntry.CRE_OriginalEntryNumber);
					AssertEquals("cusReconEntry.CRE_EntryDate not updated", new ZDate(2020, 01, 01), cusReconEntry.CRE_EntryDate);
					AssertEquals("cusReconEntryLine1.CRL_CustomsStatus not updated", UniversalReferenceConstants.EntryStatus.TX5, cusReconEntryLine1.CRL_CustomsStatus);
					AssertEquals("cusReconEntryLine2.CRL_CustomsStatus not updated", UniversalReferenceConstants.EntryStatus.TX6, cusReconEntryLine2.CRL_CustomsStatus);
					AssertEquals("entry line status not updated", UniversalReferenceConstants.EntryStatus.TX5, entryLine.ZG_CustomsStatus);
					AssertEquals("entry status not updated", UniversalReferenceConstants.EntryStatus.RC2, entryHeader.CH_EntryStatus);
				}

				AssertEquals("cusReconEntry not deleted", false, cusReconEntry.IsDeleted);
				AssertEquals("cusReconEntrySnapshot not deleted", false, cusReconEntrySnapshot.IsDeleted);
				AssertEquals("cusReconEntryLine1 not deleted", false, cusReconEntryLine1.IsDeleted);
				AssertEquals("cusReconEntryLine2 not deleted", false, cusReconEntryLine2.IsDeleted);
				AssertEquals("cusReconEntryLineSnapshot not deleted", false, cusReconEntryLineSnapshot.IsDeleted);
			});
		}

		void CreateWarehouseAdjustmentRefCusProcedure()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingDataGrouping(Core.Constants.CountryCodes.Germany);
			var procedure = Factory.New<RefCusProcedure>();
			procedure.ZZ6_ProcedureCode = warehouseAdjustmentProcedureCode;
			procedure.ZZ6_ZZZ_NKDataGrouping = Core.Constants.CountryCodes.Germany;
			procedure.ZZ6_ShipmentType = DEJobMessageTypeList.Codes.WarehouseAdjustment;
			procedure.ZZ6_OutOfWarehouse = WarehouseMoveStatus.Codes.Yes;
			procedure.ZZ6_Description = "Adjustment";
		}
	}
}
