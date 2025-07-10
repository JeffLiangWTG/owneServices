using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportCUSTAXMessageProcessor))]
	sealed class ImportCUSTAXMessageProcessorTest : ImportMessageProcessorAffectingDeclarationAbstractTest<ImportCUSTAXMessageProcessor, AtlasInboundEDIMessage<ICUSTAX>>
	{
		public void TestUpdateEntryLineStatus() => CombineAssertions(() =>
		{
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			foreach ((string completionFlag, string customsStatus) in GetExpectedCustomsStatuses())
			{
				dataProviderMockLine.Setup(m => m.LineNumber).Returns("1");
				dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(completionFlag);
				entryLine1.ZG_CustomsStatus = "OLD";
				entryLine2.ZG_CustomsStatus = "OLD";

				ProcessMessage(Message);
				AssertEquals($"For LineCompletionFlag='{completionFlag}'", customsStatus, entryLine1.ZG_CustomsStatus);
				AssertEquals("Unchanged For second Line", "OLD", entryLine2.ZG_CustomsStatus);
			}

			IEnumerable<(string completionFlag, string customsStatus)> GetExpectedCustomsStatuses()
			{
				yield return ("INV", "OLD");
				yield return ("", "OLD");
				yield return (ImportCompletionFlagList.Codes._1, UniversalReferenceConstants.EntryStatus.TX1);
				yield return (ImportCompletionFlagList.Codes._2, UniversalReferenceConstants.EntryStatus.TX2);
				yield return (ImportCompletionFlagList.Codes._3, UniversalReferenceConstants.EntryStatus.TX3);
				yield return (ImportCompletionFlagList.Codes._4, UniversalReferenceConstants.EntryStatus.TX4);
				yield return (ImportCompletionFlagList.Codes._5, UniversalReferenceConstants.EntryStatus.TX5);
				yield return (ImportCompletionFlagList.Codes._6, UniversalReferenceConstants.EntryStatus.TX6);
				yield return (ImportCompletionFlagList.Codes._7, UniversalReferenceConstants.EntryStatus.TX7);
				yield return (ImportCompletionFlagList.Codes._8, UniversalReferenceConstants.EntryStatus.TX8);
			}
		});

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
				var testHelper = new WhsDataTestHelper(Factory);
				var declarationToTest = testHelper.GetNewDeclarationWithInstruction(Factory, "IMP", $"B000{testCaseIndex}", referenceNumber, 20, true);

				var activeEntryHeader = (CusEntryHeader)declarationToTest.ActiveEntryHeaders.Single();
				if (warehouse2IsEmpty)
				{
					activeEntryHeader.EntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				}

				Factory.Save();

				dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
				dataProviderMock.Setup(m => m.CompletionFlag).Returns(ImportCompletionFlagList.Codes._7);
				dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._6);

				outgoingMessage.EM_LinkedObject = activeEntryHeader;
				Factory.Save();

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

			var testHelper = new WhsDataTestHelper(Factory);
			declaration = testHelper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0001", referenceNumber, 20, true);
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.Single();
			var invoiceLine = declaration.InvoiceLines[0];
			invoiceLine.CusEntryLine.ZG_CustomsStatus = "TX6";

			Customs.Business.WarehouseExtensions.JobDeclarationWarehouseExtensions.PublishShipmentForWHSInward(entryHeader, false); //Enterprise.Customs.Business.WarehouseExtensions
			AssertEquals("Precondition: Into Bonded Warehouse", true, declaration.ActiveEntryHeaders[0].IsIntoWarehouseWarehousing);

			Factory.Save();

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
			dataProviderMock.Setup(m => m.CompletionFlag).Returns(ImportCompletionFlagList.Codes._7);
			dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._6);

			outgoingMessage.EM_LinkedObject = entryHeader;
			Factory.Save();

			CombineAssertions(() =>
			{
				var msg = messageMock.Object;

				ProcessMessage(msg, true);
				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
				var dataTransferEvents = entryHeader.Logs.Find(query);

				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, msg.EM_Status);
				AssertEquals("EntryStatus", UniversalReferenceConstants.EntryStatus.TX7, entryHeader.CH_EntryStatus);
				AssertEquals("EntryStatus", UniversalReferenceConstants.EntryStatus.TX6, invoiceLine.CusEntryLine.ZG_CustomsStatus);

				AssertEquals(2, dataTransferEvents.Count(x => x.RelatedEDIMessage.Message.EM_MessageSubType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));

				AssertEquals(Customs.Business.WarehouseTransactionStatusList.Codes.InwardCreated, entryHeader.CH_WarehouseTransactionStatus);
				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(entryHeader.EntryNumber, invoiceLine.CusEntryLine.CL_LineNumber, 20);
			});
		}

		[TestDate(2024, 9, 10)]
		public void TestUpdateWarehouse_IntoInwardProcessing()
		{
			var helper = WhsDataTestHelper.New(Factory);

			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "TX6", true, iCancel: false);
			helper.WhsHelper.EnableWarehouseForBond(helper.IprWhsWarehouse, true);

			const string referenceNumber = "ATE150000620520195874";

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			using (helper.WhsHelper.UseAllocationEngineMock())
			{
				var inwardDecl = helper.GetNewDeclarationWithInstructionForInwardProcessing(Factory, "IMP", "B0000", referenceNumber, 10000, true);
				entryHeader = (CusEntryHeader)inwardDecl.ActiveEntryHeaders.Single();

				var invoiceLine = inwardDecl.InvoiceLines[0];
				invoiceLine.JI_BondedWhsQuantity = 100m;
				invoiceLine.CusEntryLine.ZG_CustomsStatus = "TX6";

				entryHeader.PublishShipmentForWHSInward(false);
				Factory.Save();

				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(referenceNumber, 1, 100m);

				dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
				dataProviderMock.Setup(m => m.CompletionFlag).Returns(ImportCompletionFlagList.Codes._7);
				dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._6);
				dataProviderMockLine.Setup(m => m.ExportLimitDate).Returns(new DateTime(2024, 9, 20));

				var msg = messageMock.Object;

				ProcessMessage(msg, true);

				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, msg.EM_Status);
				AssertEquals("EntryStatus", UniversalReferenceConstants.EntryStatus.TX7, entryHeader.CH_EntryStatus);
				AssertEquals("CustomsStatus", UniversalReferenceConstants.EntryStatus.TX6, invoiceLine.CusEntryLine.ZG_CustomsStatus);

				var inventory = WhsDataTestHelper.GetWhsInventoryFromDatabase(entryHeader.EntryNumber + "-" + invoiceLine.CusEntryLine.CL_LineNumber);
				var inventoryLineWithStock = inventory.Single(x => x.WI_TotalUnits == 100.0m);

				var docketLinePk = inventoryLineWithStock.WI_WE_InDocketLine;
				var bwhAttributeQuery = new ZDBOnlyQuery(typeof(IWhsBondedWarehouseAttribute));
				bwhAttributeQuery.AddToFilter(WhsBondedWarehouseAttributeSchema.WB_ParentID, docketLinePk);
				var bwhAttribute = Factory.LoadTop1<IWhsBondedWarehouseAttribute>(bwhAttributeQuery);

				AssertEquals(new ZDate(2024, 9, 20), bwhAttribute.WB_CustomsDeadline);
			}
		}

		[GuiTest]
		public void TestUpdateWarehouse_OutOfWarehouseWarehousing_DeclarationCleared()
		{
			AssertUpdateWarehouse_OutOfWarehouseWarehousing(ImportCompletionFlagList.Codes._7, UniversalReferenceConstants.EntryStatus.TX7, true);
		}

		[GuiTest]
		public void TestUpdateWarehouse_OutOfWarehouseWarehousing_DeclarationNotCleared()
		{
			AssertUpdateWarehouse_OutOfWarehouseWarehousing(ImportCompletionFlagList.Codes._8, UniversalReferenceConstants.EntryStatus.TX8, false);
		}

		void AssertUpdateWarehouse_OutOfWarehouseWarehousing(string completionFlag, string expectedStatus, bool expectAcceptEvent)
		{
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

			WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryHeader.EntryNumber, 1, 20);

			declaration = testHelper.GetNewDeclarationWithInstruction(Factory, "IMP", "B0002", referenceNumber, 1, false, prevReferenceNumber);
			AssertEquals("Precondition: OutOf Bonded Warehouse", true, declaration.ActiveEntryHeaders[0].IsOutOfWarehouseWarehousing);

			declaration.InvoiceLines[0].CusEntryLine.ZG_CustomsStatus = "TX7";
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.Single();

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
			dataProviderMock.Setup(m => m.CompletionFlag).Returns(completionFlag);
			dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(completionFlag);

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

				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, msg.EM_Status);
				AssertEquals("EntryStatus", expectedStatus, entryHeader.CH_EntryStatus);

				AssertEquals(1, dataTransferEvents.Count(x => x.RelatedEDIMessage.Message.EM_MessageSubType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));

				if (expectAcceptEvent)
				{
					AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreated, declaration.JE_WarehouseTransactionStatus);
					AssertEquals("Expect warehouse job to be finalized when declaration is cleared",1, finalizedEvents.Length);
				}
				else
				{
					AssertEquals(WarehouseTransactionStatusList.Codes.OutwardCreatedPending, declaration.JE_WarehouseTransactionStatus);
					AssertEquals("Expect warehouse job to not be finalized when declaration is not cleared", 0, finalizedEvents.Length);
				}

				WhsDataTestHelper.AssertInventoryAvailabilityInActualDatabase(inwardEntryHeader.EntryNumber, 1, 19);
			});
		}

		public void TestShouldPublishWhsOutwardAcceptEvent()
		{
			foreach (var (expectedShouldPublish, entryStatus) in GetExpectedShouldPublishAcceptEvent())
			{
				entryHeader.CH_EntryStatus = entryStatus;
				var shouldPublishAcceptEvent = Processor.ShouldPublishWhsOutwardAcceptEvent(entryHeader);
				AssertEquals($"Should publish accept event for {entryStatus}", expectedShouldPublish, shouldPublishAcceptEvent);
			}

			IEnumerable<(bool shouldPublishAccept, string entryStatus)> GetExpectedShouldPublishAcceptEvent()
			{
				yield return (false, UniversalReferenceConstants.EntryStatus.TX1);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX2);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX3);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX4);
				yield return (true, UniversalReferenceConstants.EntryStatus.TX5);
				yield return (true, UniversalReferenceConstants.EntryStatus.TX6);
				yield return (true, UniversalReferenceConstants.EntryStatus.TX7);
				yield return (false, UniversalReferenceConstants.EntryStatus.TX8);
			}
		}

		public void TestGetLinkedObject()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType<CusEntryHeader>("Type is CusEntryHeader", Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		public void TestGetLinkedObject_MRN()
		{
			mrnEntryNumber.CE_EntryNum = "19DE485154386041M4";
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("19DE485154386041M4");
			Factory.Save();

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType<CusEntryHeader>("Type is CusEntryHeader", Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		[TestDate(2022, 12, 21, 08, 28, 00)]
		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");
			ProcessMessage(Message);
			var startTime = ZDateTime.UtcNow;

			CombineAssertions(() =>
			{
				messageMock.Setup(x => x.EM_SystemCreateTimeUtc).Returns(startTime);
				ProcessMessage(Message);
				AssertNull("No LinkedObject", Message.EM_LinkedObject);
				AssertEquals("EM_Status not changed", EDIMessage.Status.Queued, Message.EM_Status);
				AssertEquals("EM_HeldUntilDate", startTime.AddMinutes(1), Message.EM_HeldUntilDate);

				messageMock.Setup(x => x.EM_SystemCreateTimeUtc).Returns(startTime.AddMinutes(-6));
				ProcessMessage(Message);
				AssertNull("No LinkedObject", Message.EM_LinkedObject);
				AssertEquals("EM_Status set to 'ERR' after 5 minutes", EDIMessage.Status.Error, Message.EM_Status);
			});
		}

		public void TestSetEM_HeldUntilDate()
		{
			var startTime = ZDateTime.UtcNow;
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB0000000000000");
			messageMock.Setup(x => x.EM_SystemCreateTimeUtc).Returns(startTime);
			CombineAssertions(() =>
			{
				Assert("EM_HeldUntilDate is empty", Message.EM_HeldUntilDate.IsEmpty);
				ProcessMessage(Message);
				Assert("EM_HeldUntilDate is not empty", !Message.EM_HeldUntilDate.IsEmpty);
			});
		}

		public void TestDataProviderNullDueToInvalidMessage()
		{
			messageMock.Setup(m => m.DataProvider).Returns((ICUSTAX)null);
			AssertNoExceptionThrown(() => ProcessMessage(Message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", AesEDIMessage.Status.ProcessedOK, Message.EM_Status);
				AssertEquals("CH_EntryStatus", UniversalReferenceConstants.EntryStatus.TX8, entryHeader.CH_EntryStatus);
			});
		}

		public void TestEventsCreated()
		{
			ProcessMessage(Message);
			var entryStatus = entryHeader.CH_EntryStatus;
			AssertEquals(entryStatus, entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus)?.SL_Reference);
		}

		public void TestTotalPaidUpdated()
		{
			ProcessMessage(Message);
			var totalPaid = entryHeader.CH_TotalPaid;

			AssertEquals(6.6m, totalPaid);
		}

		public void TestUpdateEntryHeaderStatus()
		{
			CombineAssertions(() =>
			{
				for (int i = 1; i <= 8; i++)
				{
					dataProviderMock.Setup(m => m.CompletionFlag).Returns(i.ToString());
					ProcessMessage(Message);
					AssertEquals($"CH_EntryStatus when CompletionFlag = '{i}'", $"TX{i}", entryHeader.CH_EntryStatus);
				}

				entryHeader.CH_EntryStatus = "XYZ";
				dataProviderMock.Setup(m => m.CompletionFlag).Returns("0");
				ProcessMessage(Message);
				AssertEquals("Invalid CompletionFlag, CH_EntryStatus not updated", "XYZ", entryHeader.CH_EntryStatus);

				dataProviderMock.Setup(m => m.CompletionFlag).Returns(ZString.Empty);
				ProcessMessage(Message);
				AssertEquals("CompletionFlag not specified, CH_EntryStatus not updated", "XYZ", entryHeader.CH_EntryStatus);
			});
		}

		public void TestUpdateCusReconEntry()
		{
			mrnEntryNumber.CE_EntryNum = "ATE150000620520195875";
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATE150000620520195875");
			dataProviderMock.Setup(m => m.CompletionFlag).Returns(ImportCompletionFlagList.Codes._7);
			var cusReconEntry = CreateCusReconEntry(entryHeader);
			cusReconEntry.CRE_OriginalEntryNumber = "ABC";
			var cusReconEntrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("cusReconEntry.CRE_OriginalEntryNumber updated", "ATE150000620520195875", cusReconEntry.CRE_OriginalEntryNumber);
				AssertEquals("cusReconEntry.CRE_EntryDate updated", new ZDate(2021, 02, 15), cusReconEntry.CRE_EntryDate);
				AssertEquals("cusReconEntry not deleted", false, cusReconEntry.IsDeleted);
				AssertEquals("cusReconEntrySnapshot not deleted", false, cusReconEntrySnapshot.IsDeleted);
			});
		}

		public void TestUpdateOrDeleteCusReconEntryAndLines_MRN()
		{
			mrnEntryNumber.CE_EntryNum = "19DE48515D386041M4";
			Factory.Save();

			dataProviderMock.Setup(m => m.MRN).Returns("19DE48515D386041M4");
			dataProviderMock.Setup(m => m.CompletionFlag).Returns(ImportCompletionFlagList.Codes._7);
			var cusReconEntry = CreateCusReconEntry(entryHeader);
			cusReconEntry.CRE_OriginalEntryNumber = "ABC";
			var cusReconEntrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();

			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("cusReconEntry.CRE_OriginalEntryNumber updated for MRN 10th charakter is 'D' or 'E'", "19DE48515D386041M4", cusReconEntry.CRE_OriginalEntryNumber);
				AssertEquals("cusReconEntry.CRE_EntryDate updated", new ZDate(2021, 02, 15), cusReconEntry.CRE_EntryDate);
				AssertEquals("cusReconEntry not deleted", false, cusReconEntry.IsDeleted);
				AssertEquals("cusReconEntrySnapshot not deleted", false, cusReconEntrySnapshot.IsDeleted);
			});
		}

		public void TestUpdateCusReconEntry_RegistrationDateNotSpecified()
		{
			mrnEntryNumber.CE_EntryNum = "ATE150000620520195875";
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATE150000620520195875");
			dataProviderMock.Setup(m => m.CompletionFlag).Returns(ImportCompletionFlagList.Codes._7);
			dataProviderMock.Setup(m => m.RegistrationDate).Returns((DateTime?)null);
			var cusReconEntry = CreateCusReconEntry(entryHeader);
			ProcessMessage(Message);
			AssertEquals("CRE_EntryDate set to default", ZDate.Invalid, cusReconEntry.CRE_EntryDate);
		}

		public void TestDeleteCusReconEntry()
		{
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var cusReconEntrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
			var cusReconEntryLineSnapshot = cusReconEntryLine.CusReconSnapshots.AddNew();
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("cusReconEntry deleted", true, cusReconEntry.IsDeleted);
				AssertEquals("cusReconEntrySnapshot deleted", true, cusReconEntrySnapshot.IsDeleted);
				AssertEquals("cusReconEntryLine deleted", true, cusReconEntryLine.IsDeleted);
				AssertEquals("cusReconEntryLineSnapshot deleted", true, cusReconEntryLineSnapshot.IsDeleted);
			});
		}

		public void TestDeleteCusReconEntry_MRN()
		{
			mrnEntryNumber.CE_EntryNum = "19DE48515A386041M4";
			Factory.Save();

			dataProviderMock.Setup(m => m.MRN).Returns("19DE48515A386041M4");
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var cusReconEntrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();
			var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
			var cusReconEntryLineSnapshot = cusReconEntryLine.CusReconSnapshots.AddNew();
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("\"cusReconEntry deleted for MRN 10th charakter NOT 'D' or 'E'\"", true, cusReconEntry.IsDeleted);
				AssertEquals("cusReconEntrySnapshot deleted", true, cusReconEntrySnapshot.IsDeleted);
				AssertEquals("cusReconEntryLine deleted", true, cusReconEntryLine.IsDeleted);
				AssertEquals("cusReconEntryLineSnapshot deleted", true, cusReconEntryLineSnapshot.IsDeleted);
			});
		}

		public void TestUpdateCusReconEntryLine()
		{
			mrnEntryNumber.CE_EntryNum = "ATD150000620520195875";
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATD150000620520195875");
			dataProviderMock.Setup(m => m.CompletionFlag).Returns(ImportCompletionFlagList.Codes._7);
			dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._7);
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var cusReconEntryLine1 = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine1.CRL_OriginalEntryLineNumber = 1;
			cusReconEntryLine1.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX6;
			var cusReconEntryLineSnapshot2 = cusReconEntryLine1.CusReconSnapshots.AddNew();
			var cusReconEntryLine2 = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine2.CRL_OriginalEntryLineNumber = 2;
			cusReconEntryLine2.CRL_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX6;
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("cusReconEntryLine1.CRL_CustomsStatus updated", UniversalReferenceConstants.EntryStatus.TX7, cusReconEntryLine1.CRL_CustomsStatus);
				AssertEquals("cusReconEntryLine1 not deleted", false, cusReconEntryLine1.IsDeleted);
				AssertEquals("cusReconEntryLineSnapshot1 not deleted", false, cusReconEntryLineSnapshot2.IsDeleted);
				AssertEquals("cusReconEntryLine2.CRL_CustomsStatus not updated", UniversalReferenceConstants.EntryStatus.TX6, cusReconEntryLine2.CRL_CustomsStatus);
				AssertEquals("cusReconEntryLine2 not deleted", false, cusReconEntryLine2.IsDeleted);
			});
		}

		public void TestDeleteCusReconEntryLine()
		{
			mrnEntryNumber.CE_EntryNum = "ATD150000620520195875";
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATD150000620520195875");
			dataProviderMock.Setup(m => m.CompletionFlag).Returns(ImportCompletionFlagList.Codes._7);
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var cusReconEntrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();
			var cusReconEntryLine1 = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine1.CRL_OriginalEntryLineNumber = 1;
			var cusReconEntryLineSnapshot1 = cusReconEntryLine1.CusReconSnapshots.AddNew();
			var cusReconEntryLine2 = cusReconEntry.CusReconEntryLines.AddNew();
			cusReconEntryLine2.CRL_OriginalEntryLineNumber = 2;
			var cusReconEntryLineSnapshot2 = cusReconEntryLine2.CusReconSnapshots.AddNew();
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("cusReconEntryLine1 deleted", true, cusReconEntryLine1.IsDeleted);
				AssertEquals("cusReconEntryLineSnapshot1 deleted", true, cusReconEntryLineSnapshot1.IsDeleted);
				AssertEquals("cusReconEntryLine2 not deleted", false, cusReconEntryLine2.IsDeleted);
				AssertEquals("cusReconEntryLineSnapshot2 not deleted", false, cusReconEntryLineSnapshot2.IsDeleted);
				AssertEquals("cusReconEntry not deleted", false, cusReconEntry.IsDeleted);
				AssertEquals("cusReconEntrySnapshot not deleted", false, cusReconEntrySnapshot.IsDeleted);
			});
		}

		public void TestUpdateEntryReleaseDate()
		{
			ProcessMessage(Message);
			AssertEquals(ZDate.Today, entryHeader.CH_EntryReleaseDate);
		}

		public void TestUpdateEntryReleaseDate_EntryReleaseDateNotEmpty()
		{
			entryHeader.CH_EntryReleaseDate = new ZDate(2020, 01, 01);
			ProcessMessage(Message);
			AssertEquals(new ZDate(2020, 01, 01), entryHeader.CH_EntryReleaseDate);
		}

		public void TestGenerateEmail()
		{
			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			dataProviderMock.Setup(m => m.MRN).Returns("19DE48515A386041M4");
			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CUSTAX – Customs Tax Assessment Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CUSTAX – Customs Tax Assessment Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Tax Assessment. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>MRN</td><td>19DE48515A386041M4</td></tr>"
								   + "<tr><td>Registration Number</td><td>ATB150000620520195875</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
								   + "<tr><td>Completion Flag</td><td>8 - Erledigung au&#223;erhalb ATLAS vor Bescheiderstellung</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable(ZString.Empty, email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestPopulateLogbookRegNum()
		{
			ProcessMessage(Message);
			AssertEquals("ATB150000620520195875", Message.GetLogbookRegistrationNumber());
		}

		public void TestPopulateLogbookRegNum_MRN()
		{
			mrnEntryNumber.CE_EntryNum = "19DE485154386041M4";
			dataProviderMock.Setup(x => x.MRN).Returns("19DE485154386041M4");
			Factory.Save();

			ProcessMessage(Message);
			AssertEquals("ATB150000620520195875, 19DE485154386041M4", Message.GetLogbookRegistrationNumber());
		}

		public void TestCusEntryLineFeesDeleted()
		{
			var cusEntryLine = entryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 2;
			var feeToBeDeleted = cusEntryLine.ConfirmedFees.AddNew();
			var feeToStay = cusEntryLine.Fees.AddNew();
			feeToStay.CF_Source = "CW1";

			dataProviderMock.Setup(m => m.Lines).Returns(new[] { dataProviderMockLineForFees.Object });

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("cusEntryLineFee with CF_Source = 'CUS' deleted", true, feeToBeDeleted.IsDeleted);
				AssertEquals("cusEntryLineFee with CF_Source = 'CW1' not deleted", false, feeToStay.IsDeleted);
			});
		}

		public void TestCusEntryLineFeesCreated()
		{
			var cusEntryLine = entryHeader.AllEntryLines.AddNew();
			cusEntryLine.CL_LineNumber = 2;
			cusEntryLine.ConfirmedFees.RemoveAll(); //Ensure Empty
			dataProviderMock.Setup(m => m.Lines).Returns(new[] { dataProviderMockLineForFees.Object });

			ProcessMessage(Message);

			AssertEquals("New cusEntryLineFee Created", dataProviderMockLineForFees.Object.Duties.Count, cusEntryLine.ConfirmedFees.Count);
		}

		[ExpectNoExceptions]
		public void TestCusEntryLineUpdateDutyPercent()
		{
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_DutyPercent = 5;

			var line1 = new Mock<ICUSTAXLine>();
			line1.Setup(x => x.LineNumber).Returns("1");
			line1.Setup(x => x.Duties).Returns(GetDuties());

			var line2 = new Mock<ICUSTAXLine>();
			line2.Setup(x => x.LineNumber).Returns("2");
			line2.Setup(x => x.Duties).Returns(Array.Empty<ICUSTAXLineDuty>());

			dataProviderMock.Setup(x => x.Lines).Returns([line1.Object, line2.Object]);

			ProcessMessage(Message);
			NUnit.Framework.Assert.Multiple(() =>
			{
				NUnit.Framework.Assert.That(entryLine1.CL_DutyPercent, Is.EqualTo((ZDecimal)1));
				NUnit.Framework.Assert.That(entryLine2.CL_DutyPercent, Is.EqualTo((ZDecimal)5));
			});

			ICUSTAXLineDuty[] GetDuties()
			{
				var duty1 = new Mock<ICUSTAXLineDuty>();
				duty1.Setup(x => x.DutyRates).Returns(GetRates(1));
				duty1.Setup(x => x.ChargeType).Returns("A0000");

				var duty2 = new Mock<ICUSTAXLineDuty>();
				duty2.Setup(x => x.DutyRates).Returns(GetRates(2));
				duty2.Setup(x => x.ChargeType).Returns("A0001");
				return [duty1.Object, duty2.Object];
			}

			ICUSTAXLineDutyRate[] GetRates(decimal rate)
			{
				var rate1 = new Mock<ICUSTAXLineDutyRate>();
				rate1.Setup(x => x.Rate).Returns(rate);
				rate1.Setup(x => x.AssessmentScale).Returns("00");

				var rate2 = new Mock<ICUSTAXLineDutyRate>();
				rate2.Setup(x => x.Rate).Returns(rate + 7m);
				rate2.Setup(x => x.AssessmentScale).Returns("01");

				return [rate1.Object, rate2.Object];
			}
		}

		public void TestCusEntryLineUpdateCustomsValue()
		{
			var entryLine1 = entryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			entryLine2.CL_CustomsValue = 1234.5m;

			var line1 = new Mock<ICUSTAXLine>();
			line1.Setup(x => x.LineNumber).Returns("1");
			line1.Setup(x => x.CustomsValue).Returns(5.01m);

			var line2 = new Mock<ICUSTAXLine>();
			line2.Setup(x => x.LineNumber).Returns("2");
			line2.Setup(x => x.CustomsValue).Returns(default(decimal?));

			dataProviderMock.Setup(x => x.Lines).Returns(new[] { line1.Object, line2.Object });

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("cusEntryLine1 CustomsValue", 5.01m, entryLine1.CL_CustomsValue);
				AssertEquals("cusEntryLine2 CustomsValue not changed", 1234.5m, entryLine2.CL_CustomsValue);
			});
		}

		public void TestUpdateJZ_ValuationDateOverride()
		{
			var acceptanceDate = new DateTime(2022, 11, 25);
			CombineAssertions(() =>
			{
				var entryLine = entryHeader.MergedLines.AddNew();

				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				invoiceLine1.JI_CL = entryLine.PK;

				AssertEquals("JZ_ValuationDateOverride empty", ZDateTime.Empty, invoice1.JZ_ValuationDateOverride);

				dataProviderMock.Setup(x => x.AcceptanceDate).Returns(acceptanceDate);

				ProcessMessage(Message);
				AssertEquals("JZ_ValuationDateOverride updated", acceptanceDate, invoice1.JZ_ValuationDateOverride);
			});
		}

		public void TestUpdateJZ_ValuationDateOverride_ManyInvoices()
		{
			var acceptanceDate = new DateTime(2022, 11, 25);
			CombineAssertions(() =>
			{
				var entryLine = entryHeader.MergedLines.AddNew();

				var invoice1 = declaration.Invoices.AddNew();
				var invoiceLine1 = invoice1.InvoiceLines.AddNew();
				invoiceLine1.JI_CL = entryLine.PK;

				var invoice2 = declaration.Invoices.AddNew();
				var invoiceLine2 = invoice2.InvoiceLines.AddNew();
				invoiceLine2.JI_CL = entryLine.PK;

				dataProviderMock.Setup(x => x.AcceptanceDate).Returns(acceptanceDate);

				ProcessMessage(Message);
				AssertEquals("JZ_ValuationDateOverride invoice1 updated", acceptanceDate, invoice1.JZ_ValuationDateOverride);
				AssertEquals("JZ_ValuationDateOverride invoice2 updated", acceptanceDate, invoice2.JZ_ValuationDateOverride);
			});
		}

		public void TestSaveAndRecalculateDeclaration()
		{
			var acceptanceDate = new DateTime(2022, 11, 25);
			var invoice = declaration.Invoices.AddNew();
			invoice.JZ_InvoiceAmount = 3000m;
			invoice.JZ_RX_NKInvoice_Currency = declaration.LocalCurrencyCode;
			var charge = invoice.Charges.AddNew(Common.CustomsChargeTypeList.Codes.OverseasFreight, 6000m, declaration.LocalCurrencyCode);

			var entryLine1 = entryHeader.MergedLines.AddNew();
			var invoiceLine1 = declaration.InvoiceLines.AddNew();
			invoiceLine1.JI_LinePrice = 1500m;
			invoiceLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entryHeader.MergedLines.AddNew();
			var invoiceLine2 = declaration.InvoiceLines.AddNew();
			invoiceLine2.JI_LinePrice = 1500m;
			invoiceLine2.JI_CL = entryLine1.PK;
			Factory.Save();

			dataProviderMock.Setup(x => x.AcceptanceDate).Returns(acceptanceDate);

			CombineAssertions(() =>
			{
				invoiceLine1.ApportionedCharges.RemoveAndDeleteAll();
				invoiceLine2.ApportionedCharges.RemoveAndDeleteAll();
				ProcessMessage(Message);
				AssertEquals("Invoice 1 Apportioned Charge OFT", 3000m, invoiceLine1.ApportionedCharges[Common.CustomsChargeTypeList.Codes.OverseasFreight].J7_Amount);
				AssertEquals("Invoice 2 Apportioned Charge OFT", 3000m, invoiceLine2.ApportionedCharges[Common.CustomsChargeTypeList.Codes.OverseasFreight].J7_Amount);
			});
		}

		public void TestProcessMessage_UpdateRegistrationNumbersInRelatedOrder()
		{
			var testHelper = new WhsDataTestHelper(Factory);

			var line1 = GetLine("1", 5.01m);
			var line2 = GetLine("2", 10.02m);
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATC996151771020016388");
			dataProviderMock.Setup(m => m.Lines).Returns(new[] { line1.Object, line2.Object });

			declaration = testHelper.GetNewDeclaration("IMP", "DECL1234", "ATC996151771020016388", 5.0m);
			declaration.IsOutwardOrderImported = true;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			entryInstruction.CEI_Procedure = intoWarehouseWarehousingProcedureCode.Left(2);
			entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;

			entryHeader = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;

			var (entryLine1, entryLine2) = CreateEntryLinesAndLinkedInvoicesForWarehouseTest(entryHeader, declaration, entryInstruction, testHelper.Part, outOfWarehouseWarehousingProcedureCode);
			entryLine1.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX5;
			entryLine2.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.TX6;

			var order = testHelper.WhsHelper.CreateWhsOrder(testHelper.Importer.PK, testHelper.WhsWarehouse.PK, testHelper.Importer.PK, "OrderRef") as WhsOrder;

			var orderJobPivot = Factory.NewWithValidTestData<WhsDocketJobPivot>();
			orderJobPivot.WV_ParentId = declaration.PK;
			orderJobPivot.WV_ParentTableCode = declaration.TablePrefix;
			orderJobPivot.WV_WD_Docket = order.PK;

			var orderLine1 = testHelper.WhsHelper.CreateWhsOrderLine(order.PK, testHelper.Part.PK, 1, "1234", "old value 1", ZString.Empty, ZString.Empty, ZString.Empty, 100m, 20m, "KG", 0, ZString.Empty, ZString.Empty, 1, ZGuid.Empty, ZString.Empty) as WhsOrderLine;
			orderLine1.WE_WB_CustomsData = orderLine1.CustomsData.PK;
			var orderLine2 = testHelper.WhsHelper.CreateWhsOrderLine(order.PK, testHelper.Part.PK, 1, "1234", "old value 2", ZString.Empty, ZString.Empty, ZString.Empty, 100m, 20m, "KG", 0, ZString.Empty, ZString.Empty, 1, ZGuid.Empty, ZString.Empty) as WhsOrderLine;
			orderLine2.WE_WB_CustomsData = orderLine2.CustomsData.PK;

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
				AssertEquals("atribute 1 updated: EntryKey", "ATC996151771020016388", orderLine1.CustomsData.WB_EntryKey);
				AssertEquals("atribute 1 updated: LineNo", (short)1, orderLine1.CustomsData.WB_EntryLineNo);

				AssertEquals("atribute 2 updated: EntryKey", "ATC996151771020016388", orderLine2.CustomsData.WB_EntryKey);
				AssertEquals("atribute 2 updated: LineNo", (short)2, orderLine2.CustomsData.WB_EntryLineNo);
			});
		}

		public void TestAddDeclarationClearedMessage_WhenSingleEntryIsClearedWithTX1()
		{
			AssertDeclarationClearedEventAddedIfSingleEntryIsCleared(ImportCompletionFlagList.Codes._1);
		}

		public void TestAddDeclarationClearedMessage_WhenSingleEntryIsClearedWithTX2()
		{
			AssertDeclarationClearedEventAddedIfSingleEntryIsCleared(ImportCompletionFlagList.Codes._2);
		}

		public void TestAddDeclarationClearedMessage_WhenSingleEntryIsClearedWithTX3()
		{
			AssertDeclarationClearedEventAddedIfSingleEntryIsCleared(ImportCompletionFlagList.Codes._3);
		}

		public void TestAddDeclarationClearedMessage_WhenSingleEntryIsClearedWithTX5()
		{
			AssertDeclarationClearedEventAddedIfSingleEntryIsCleared(ImportCompletionFlagList.Codes._5);
		}

		public void TestAddDeclarationClearedMessage_WhenSingleEntryIsClearedWithTX6()
		{
			AssertDeclarationClearedEventAddedIfSingleEntryIsCleared(ImportCompletionFlagList.Codes._6);
		}

		public void TestAddDeclarationClearedMessage_WhenSingleEntryIsClearedWithTX8()
		{
			AssertDeclarationClearedEventAddedIfSingleEntryIsCleared(ImportCompletionFlagList.Codes._8);
		}

		public void TestAddDeclarationClearedMessage_WhenAllEntriesAreCleared()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var extraEntryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			extraEntryHeader.CH_CEI_Instruction = instruction.PK;
			extraEntryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX8;

			dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._8);

			ProcessMessage(Message);

			AssertDeclarationCustomsClearedEventsCountEquals(1);
		}

		public void TestDoNotAddDeclarationClearedMessage_WhenCurrentEntryIsClearedAndAnotherEntryIsNotCleared()
		{
			var instruction = declaration.CustomsEntryInstructions.AddNew();
			var extraEntryHeader = declaration.ActiveEntryHeaders.AddNew() as CusEntryHeader;
			extraEntryHeader.CH_CEI_Instruction = instruction.PK;
			extraEntryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX7;

			dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._8);

			ProcessMessage(Message);

			AssertDeclarationCustomsClearedEventsCountEquals(0);
		}

		protected override void SetUp()
		{
			base.SetUp();

			declaration.JE_ApplicationCode = DeclarationApplicationCodeList.Codes.Builtin;
			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATB150000620520195875";
			mrnEntryNumber.CE_EntryType = CusEntryNumberTypes.Standard.MovementReferenceNumber;

			outgoingMessage = CreateOriginalMessageLinkedToParent<AesEDIMessage>(entryHeader, "outgoing");
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			outgoingMessage.EM_MessageType = EDIMessageTypeList.Codes.Import;
			outgoingMessage.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			dataProviderMockLine = new Mock<ICUSTAXLine>();
			dataProviderMockLine.Setup(x => x.LineNumber).Returns("1");
			dataProviderMockLine.Setup(m => m.Duties).Returns(Array.Empty<ICUSTAXLineDuty>());
			dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._8);

			var dataProviderMockLineForFeesDutyLine = new Mock<ICUSTAXLineDuty>();
			dataProviderMockLineForFeesDutyLine.Setup(m => m.ChargeAmount).Returns(1000.0m);
			dataProviderMockLineForFeesDutyLine.Setup(m => m.ChargeType).Returns("C1234");
			dataProviderMockLineForFeesDutyLine.Setup(m => m.BaseValue).Returns(500m);
			dataProviderMockLineForFeesDutyLine.Setup(m => m.MethodOfCalculation).Returns("ANY");
			dataProviderMockLineForFeesDutyLine.Setup(m => m.MethodOfPayment).Returns("MOP");
			dataProviderMockLineForFeesDutyLine.Setup(m => m.DutyRates).Returns(Array.Empty<ICUSTAXLineDutyRate>());

			dataProviderMockLineForFees = new Mock<ICUSTAXLine>();
			dataProviderMockLineForFees.Setup(x => x.LineNumber).Returns("2");
			dataProviderMockLineForFees.Setup(x => x.CustomsValue).Returns((decimal?)null);
			dataProviderMockLineForFees.Setup(x => x.LineCompletionFlag).Returns(ImportCompletionFlagList.Codes._4);
			dataProviderMockLineForFees.Setup(m => m.Duties).Returns(new[] { dataProviderMockLineForFeesDutyLine.Object });

			dataProviderMock = new Mock<ICUSTAX>();
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("0000000009");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATB150000620520195875");
			dataProviderMock.Setup(x => x.CompletionFlag).Returns(ImportCompletionFlagList.Codes._8);
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("MAS/22/11/22027");
			dataProviderMock.Setup(x => x.RegistrationDate).Returns(new DateTime(2021, 02, 15, 10, 26, 59));
			dataProviderMock.Setup(x => x.Lines).Returns(new[] { dataProviderMockLine.Object });
			dataProviderMock.Setup(m => m.TotalCustomsDutyAmount).Returns(6.6m);

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICUSTAX>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			Factory.Save();
		}

		protected override ZString MessageFriendlyName => "Import CUSTAX Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICUSTAX>> Processor => new ImportCUSTAXMessageProcessor(logger);

		protected override AtlasInboundEDIMessage<ICUSTAX> Message => messageMock.Object;
		protected override bool ExpectedDelayStatusError => true;

		Mock<ICUSTAXLine> GetLine(ZString lineNumber, ZDecimal customsValue)
		{
			var line = new Mock<ICUSTAXLine> { CallBase = true };
			line.Setup(x => x.LineNumber).Returns(lineNumber);
			line.Setup(x => x.CustomsValue).Returns(customsValue);
			return line;
		}

		(CusEntryLine, CusEntryLine) CreateEntryLinesAndLinkedInvoicesForWarehouseTest(CusEntryHeader cusEntryHeader, JobDeclaration jobDeclaration, CusEntryInstruction entryInstruction, OrgSupplierPart part, ZString procedureCode)
		{
			var entryLine1 = cusEntryHeader.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var entryLine2 = cusEntryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			var invHeader1 = jobDeclaration.Invoices.Single();
			invHeader1.JZ_InvoiceNumber = "1";

			var invoiceLine = cusEntryHeader.InvoiceLines.Cast<JobComInvoiceLine>().Single();
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

		CusReconEntry CreateCusReconEntry(CusEntryHeader entryHeader)
		{
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			cusReconEntry.CRE_EntryDate = new ZDate(2020, 01, 01);
			return cusReconEntry;
		}

		void AssertDeclarationClearedEventAddedIfSingleEntryIsCleared(string completionFlag)
		{
			dataProviderMockLine.Setup(m => m.LineCompletionFlag).Returns(completionFlag);

			ProcessMessage(Message);

			AssertDeclarationCustomsClearedEventsCountEquals(1);
		}

		void AssertDeclarationCustomsClearedEventsCountEquals(int expectedCount)
		{
			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, "CLR")
				.AddToFilter(new ZQuery(StmALogSchema.SL_Reference, "Customs Cleared"));

			var declarationCustomsClearedEvents = entryHeader.Declaration.Logs.Find(query);

			AssertEquals(expectedCount, declarationCustomsClearedEvents.Length);
		}

		CusEntryHeader entryHeader;
		CusEntryNumber mrnEntryNumber;
		Mock<ICUSTAX> dataProviderMock;
		Mock<ICUSTAXLine> dataProviderMockLine;
		Mock<ICUSTAXLine> dataProviderMockLineForFees;
		Mock<AtlasInboundEDIMessage<ICUSTAX>> messageMock;
		EDIMessage outgoingMessage;
		readonly ZString outOfWarehouseWarehousingProcedureCode = "4071";
		readonly ZString intoWarehouseWarehousingProcedureCode = "7100";
	}
}
