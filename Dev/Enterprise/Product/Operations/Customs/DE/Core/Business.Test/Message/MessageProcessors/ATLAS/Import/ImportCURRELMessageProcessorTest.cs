using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Common;
using Enterprise.Customs.DE.Business.MasterFiles;
using Enterprise.Customs.DE.Business.Testing;
using Enterprise.Customs.DE.Messaging;
using Enterprise.Customs.Universal;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.UniversalDataBuss.DataObjects.Universal;
using Enterprise.UniversalDataBuss.Management;
using Enterprise.Warehouse.Transactions.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;
using EDIMessageTypeList = Enterprise.Customs.DE.Messaging.EDIMessageTypeList;
using WhsDataTestHelper = Enterprise.Customs.DE.Business.Testing.WhsDataTestHelper;

namespace Enterprise.Customs.DE.Business.Declaration.Testing
{
	[TestedType(typeof(ImportCURRELMessageProcessor))]
	sealed class ImportCURRELMessageProcessorTest : ImportMessageProcessorAffectingDeclarationAbstractTest<ImportCURRELMessageProcessor, AtlasInboundEDIMessage<ICURREL>>
	{
		public void TestGetLinkedObject()
		{
			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertType("Type is CusEntryHeader", typeof(CusEntryHeader), Message.EM_LinkedObject);
				AssertEquals("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
			});
		}

		[TestDate(2020, 09, 24, 15, 31, 00)]
		public void TestNewRlbEventAlwaysCreatedOnHeader()
		{
			CombineAssertions(() =>
			{
				ProcessMessage(Message);
				var rlb = entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == UniversalReferenceConstants.EntryStatus.RLB);
				AssertEquals("EventRaised", new ZDateTime(2020, 09, 24, 15, 31, 00), rlb.SL_EventTime);

				var goodsItem = GetGoodsItem("1", "0", "N", "N", "J");
				dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object });
				ProcessMessage(Message);
				AssertEquals("NewEventRaised", 2, entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsEntryStatus).Count(e => e.SL_Reference == UniversalReferenceConstants.EntryStatus.RLB));
			});
		}

		public void TestNewRlxEventCreatedOnHeaderWhenEncountered()
		{
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			var entryLine4 = entryHeader.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 4;
			var entryLine5 = entryHeader.AllEntryLines.AddNew();
			entryLine5.CL_LineNumber = 5;
			var goodsItem1 = GetGoodsItem("1", "1", "J", "N", "J");
			var goodsItem2 = GetGoodsItem("2", "6", "J", "J", "J");
			var goodsItem3 = GetGoodsItem("3", "6", "A", "J", "J");
			var goodsItem4 = GetGoodsItem("4", "6", "A", "N", "J");
			var goodsItem5 = GetGoodsItem("5", "6", "A", "N", "B");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object, goodsItem3.Object, goodsItem4.Object, goodsItem5.Object });

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertNotNull("RL1", entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == UniversalReferenceConstants.EntryStatus.RL1));
				AssertNotNull("RL2", entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == UniversalReferenceConstants.EntryStatus.RL2));
				AssertNotNull("RL3", entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == UniversalReferenceConstants.EntryStatus.RL3));
				AssertNotNull("RL4", entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == UniversalReferenceConstants.EntryStatus.RL4));
				AssertNotNull("RL5", entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference == UniversalReferenceConstants.EntryStatus.RL5));
			});
		}

		[TestDate(2020, 09, 24, 15, 31, 00)]
		public void TestOnlyOneRlxEventCreatedOnHeaderPerStatus()
		{
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var goodsItem1 = GetGoodsItem("1", "0", "N", "N", "J");
			var goodsItem2 = GetGoodsItem("2", "0", "N", "N", "J");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object });

			ProcessMessage(Message);
			var rl4 = entryHeader.Logs.GetAllLogsByEventOrderByPostedTimeUtcDESC(Events.CustomsEntryStatus).Single(e => e.SL_Reference == UniversalReferenceConstants.EntryStatus.RL4);
			AssertEquals("EventRaised", new ZDateTime(2020, 09, 24, 15, 31, 00), rl4.SL_EventTime);
		}

		[TestDate(2020, 09, 24, 15, 31, 00)]
		public void TestGetLinkedObject_FromTemporaryReferenceNumber()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATC996151771020016389");

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertSame("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
				AssertEquals("CE_EntryNum", "ATB0000000000000", entryHeader.CusEntryNumber.CE_EntryNum);
				AssertEquals("CE_IssueDate", new ZDateTime(2020, 09, 24, 15, 31, 00), entryHeader.CusEntryNumber.CE_IssueDate);
			});
		}

		[TestDate(2024, 06, 12, 15, 31, 00)]
		public void TestGetLinkedObject_FromMRN_CusEntryNumUpdated()
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATC996151771020016389");

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.MRN).Returns("23DE12345678901234");

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertSame("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
				AssertEquals("CE_EntryNum", "23DE12345678901234", entryHeader.CusEntryNumber.CE_EntryNum);
				AssertEquals("CE_IssueDate", new ZDateTime(2024, 06, 12, 15, 31, 00), entryHeader.CusEntryNumber.CE_IssueDate);
			});
		}

		public void TestGetLinkedObject_FromMRN()
		{
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "23DE12345678901234";
			entryLine = entryHeader.AllEntryLines.AddNew();

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.MRN).Returns("23DE12345678901234");

			ProcessMessage(Message);

			AssertSame("Correct LinkedObject obtained", entryHeader, Message.EM_LinkedObject);
		}

		[TestDate(2022, 12, 21, 08, 28, 00)]
		public void TestNoLinkedObject()
		{
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");
			var startTime = ZDateTime.UtcNow;

			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");
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
			dataProviderMock.Setup(m => m.ReferenceNumber).Returns("ATB0000000000000");
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
			messageMock.Setup(m => m.DataProvider).Returns((ICURREL)null);
			AssertNoExceptionThrown(() => ProcessMessage(Message));
		}

		public void TestProcessMessage()
		{
			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, Message.EM_Status);
				AssertEquals("ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL5, entryLine.ZG_CustomsStatus);
			});
		}

		public void TestWarehouseProcessing_ShouldNotBeTriggered_WhenCEI_OA_Warehouse2IsEmpty_IntoWarehouseWarehousing()
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
				var declarationReference = $"DECL123{testCaseIndex}";
				var entryNumber = $"ATC99615177102001638{testCaseIndex}";

				var testHelper = new WhsDataTestHelper<JobDeclaration, OrgSupplierPart, BaseCusClassification, CusClassPartPivot>(Factory);
				var jobDeclaration = testHelper.GetNewDeclaration("IMP", declarationReference, entryNumber, 5.0m);

				testHelper.WhsWarehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
				var whsReceive = testHelper.GetNewWhsReceive(testHelper.WhsWarehouse.PK, jobDeclaration.Importer.PK, $"3-{declarationReference}");
				whsReceive.WD_CustomsParentReference = $"3-{declarationReference}-EDIDATEDI";
				testHelper.GetNewWhsReceiveLine(whsReceive.PK, testHelper.Part.PK, "12345", 11, 11, 11);

				var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
				entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
				entryInstruction.CEI_Procedure = intoWarehouseWarehousingProcedureCode.Left(2);
				var cusEntryHeader = (CusEntryHeader)jobDeclaration.ActiveEntryHeaders.First();
				var mrnEntryNumber = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
				mrnEntryNumber.CE_EntryNum = entryNumber;
				cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
				entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;

				var warehouse = Factory.NewWithValidTestData<OrgHeader>();
				entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

				CreateEntryLinesAndLinkedInvoicesForWarehouseTest(cusEntryHeader, jobDeclaration, entryInstruction, testHelper.Part, intoWarehouseWarehousingProcedureCode);

				SetupOutgoingMessage(jobDeclaration.ActiveEntryHeaders.First());

				var goodsItem1 = GetGoodsItem("1", ZString.Empty, "J", ZString.Empty, ZString.Empty);
				var goodsItem2 = GetGoodsItem("2", "6", "A", "N", "J");

				dataProviderMock.Setup(x => x.ReferenceNumber).Returns(entryNumber);
				dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object });

				messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICURREL>>();
				messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
				if (warehouse2IsEmpty)
				{
					cusEntryHeader.EntryInstruction.CEI_OA_Warehouse2 = ZGuid.Empty;
				}
				Factory.Save();

				ProcessMessage(messageMock.Object, true);

				var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
				var dataTransferEvents = cusEntryHeader.Logs.Find(query);
				AssertEquals(expectUpdateWarehouseTriggered, dataTransferEvents.Any(x => x.RelatedEDIMessage.Message.EM_MessageSubType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML));
			}
		}

		public void TestWarehouseProcessing_Into()
		{
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL4", false, iCancel: true);
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL5", true, iCancel: false);
			var testHelper = new WhsDataTestHelper<JobDeclaration, OrgSupplierPart, BaseCusClassification, CusClassPartPivot>(Factory);
			var jobDeclaration = testHelper.GetNewDeclaration("IMP", "DECL1234", "ATC996151771020016388", 5.0m);

			testHelper.WhsWarehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var whsReceive = testHelper.GetNewWhsReceive(testHelper.WhsWarehouse.PK, jobDeclaration.Importer.PK, "3-DECL1234");
			whsReceive.WD_CustomsParentReference = "3-DECL1234-EDIDATEDI";
			testHelper.GetNewWhsReceiveLine(whsReceive.PK, testHelper.Part.PK, "12345", 11, 11, 11);

			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			entryInstruction.CEI_Procedure = intoWarehouseWarehousingProcedureCode.Left(2);
			var cusEntryHeader = (CusEntryHeader)jobDeclaration.ActiveEntryHeaders.First();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATC996151771020016388";
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;

			var warehouse = Factory.NewWithValidTestData<OrgHeader>();
			entryInstruction.CEI_OA_Warehouse2 = warehouse.MainAddress.PK;

			var (entryLine1, entryLine2) = CreateEntryLinesAndLinkedInvoicesForWarehouseTest(cusEntryHeader, jobDeclaration, entryInstruction, testHelper.Part, intoWarehouseWarehousingProcedureCode);

			SetupOutgoingMessage(jobDeclaration.ActiveEntryHeaders.First());

			var goodsItem1 = GetGoodsItem("1", ZString.Empty, "J", ZString.Empty, ZString.Empty);
			var goodsItem2 = GetGoodsItem("2", "6", "A", "N", "J");

			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATC996151771020016388");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICURREL>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			var msg = messageMock.Object;

			Factory.Save();
			ProcessMessage(msg, true);

			var query = new ZQuery(StmALogSchema.SL_SE_NKEvent, AutoEvents.DataExportCode);
			var dataTransferEvents = cusEntryHeader.Logs.Find(query);

			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, msg.EM_Status);
				AssertEquals("ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL5, entryLine1.ZG_CustomsStatus);
				AssertEquals("ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL4, entryLine2.ZG_CustomsStatus);

				var universalShipmentMessage = dataTransferEvents.Single(x => x.RelatedEDIMessage.Message.EM_MessageSubType == WorkflowTriggerActionTypeConstants.Codes.SendUniversalShipmentXML).RelatedEDIMessage.Message;
				var parsedShipment = universalShipmentMessage.GetEM_MessageTextReader().Parse<Shipment>();
				
				var commercialInfo = parsedShipment.CommercialInfo;
				AssertEquals("Two Commercial Invoices should be created, one per invoice header", 2, commercialInfo.CommercialInvoiceCollection.Count);

				var invoices = commercialInfo.CommercialInvoiceCollection;
				var invoice1 = invoices.Single(x => x.InvoiceNumber.HasValue && x.InvoiceNumber.Value == "1");
				var invoice2 = invoices.Single(x => x.InvoiceNumber.HasValue && x.InvoiceNumber.Value == "2");

				AssertEquals("First Invoice Line linked CusEntryLine has Status == 'RL5', it should be sent in the Shipment", 1, invoice1.CommercialInvoiceLineCollection.Count);
				AssertNull("First Invoice Line linked CusEntryLine has Status == 'RL4', it should not be sent in the Shipment", invoice2.CommercialInvoiceLineCollection);
			});
		}

		[GuiTest]
		public void TestWarehouseProcessing_OutOf()
		{
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL1", false, iCancel: true);
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL2", false, iCancel: false);
			Business.Testing.WarehouseIntegration.BondedWarehousingHelperTest.CreateCustomsStatus(Factory, "RL5", true, iCancel: false);

			var testHelper = new WhsDataTestHelper<JobDeclaration, OrgSupplierPart, BaseCusClassification, CusClassPartPivot>(Factory);
			var jobDeclaration = testHelper.GetNewDeclaration("IMP", "DECL1234", "ATC996151771020016388", 5.0m);
			jobDeclaration.JE_WarehouseTransactionStatus = WarehouseTransactionStatusList.Codes.OutwardCreatedPending;

			testHelper.WhsWarehouse.WarehouseAddress.OA_RN_NKCountryCode = Core.Constants.CountryCodes.Germany;
			var whsReceive = testHelper.GetNewWhsReceive(testHelper.WhsWarehouse.PK, jobDeclaration.Importer.PK, "3-DECL1234");
			whsReceive.WD_CustomsParentReference = "3-DECL1234-EDIDATEDI";
			testHelper.GetNewWhsReceiveLine(whsReceive.PK, testHelper.Part.PK, "12345", 11, 11, 11);

			var entryInstruction = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
			entryInstruction.CEI_Procedure = outOfWarehouseWarehousingProcedureCode.Left(2);
			var cusEntryHeader = (CusEntryHeader)jobDeclaration.ActiveEntryHeaders[0];
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(cusEntryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATC996151771020016388";
			cusEntryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;

			var (entryLine1, entryLine2) = CreateEntryLinesAndLinkedInvoicesForWarehouseTest(cusEntryHeader, jobDeclaration, entryInstruction, testHelper.Part, outOfWarehouseWarehousingProcedureCode);

			var entryLine3 = cusEntryHeader.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;

			var invoiceLine3 = jobDeclaration.Invoices[0].JobComInvoiceLines.AddNew();
			invoiceLine3.JI_CEI = entryInstruction.PK;
			invoiceLine3.JI_CL = entryLine3.PK;
			invoiceLine3.JI_Procedure = outOfWarehouseWarehousingProcedureCode;

			var entryInstruction2 = jobDeclaration.CustomsEntryInstructions.AddNew();
			entryInstruction2.CEI_Style = ImportDeclarationTypeList.Codes.EZA;
			entryInstruction2.CEI_Procedure = outOfWarehouseWarehousingProcedureCode.Left(2);
			var cusEntryHeader2 = jobDeclaration.CustomsEntryHeaders.AddNew();
			cusEntryHeader2.CH_CEI_Instruction = entryInstruction2.PK;
			entryInstruction2.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;

			var entryLine4 = cusEntryHeader2.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 1;

			var invHeader2 = jobDeclaration.Invoices[1];

			var invoiceLine4 = invHeader2.JobComInvoiceLines.AddNew();
			invoiceLine4.JI_PartNo = testHelper.Part.OP_PartNum;
			invoiceLine4.JI_InvoiceQuantity = 6.0m;
			invoiceLine4.JI_InvoiceUQ = "NO";
			invoiceLine4.JI_CustomsUnitQty = "KG";
			invoiceLine4.JI_CustomsQuantity = 60;
			invoiceLine4.JI_LinePrice = 600;
			invoiceLine4.JI_CL = entryLine4.PK;
			invoiceLine4.JI_Procedure = outOfWarehouseWarehousingProcedureCode;
			invoiceLine4.JI_CEI = entryInstruction.PK;

			SetupOutgoingMessage(jobDeclaration.ActiveEntryHeaders[0]);

			var goodsItem1 = GetGoodsItem("1", ZString.Empty, "J", ZString.Empty, ZString.Empty);
			var goodsItem2 = GetGoodsItem("2", "6", "A", "J", "N");
			var goodsItem3 = GetGoodsItem("3", "6", "A", "N", "N");

			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATC996151771020016388");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object, goodsItem3.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICURREL>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);

			var msg = messageMock.Object;

			Factory.Save();
			ProcessMessage(msg, true);

			CombineAssertions(() =>
			{
				var universalShipmentMessage = Factory.LoadTop1<EDIMessage>(new ZQuery(EDIMessageSchema.EM_ApplicationCode, "UDM"));
				AssertNotNull(universalShipmentMessage);
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, msg.EM_Status);
				AssertEquals("ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL5, entryLine1.ZG_CustomsStatus);
				AssertEquals("ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL2, entryLine2.ZG_CustomsStatus);

				var parsedShipment = universalShipmentMessage.GetEM_MessageTextReader().Parse<Shipment>();

				var commercialInfo = parsedShipment.CommercialInfo;
				AssertEquals("Two Commercial Invoices should be created, one per invoice header", 2, commercialInfo.CommercialInvoiceCollection.Count);

				var invoices = commercialInfo.CommercialInvoiceCollection;
				var invoice1 = invoices.Single(x => x.InvoiceNumber.HasValue && x.InvoiceNumber.Value == "1");
				var invoice2 = invoices.Single(x => x.InvoiceNumber.HasValue && x.InvoiceNumber.Value == "2");

				AssertEquals("First invoice should have 1 line, one RL5 and one unsent RL1", 1, invoice1.CommercialInvoiceLineCollection.Count);
				AssertEquals("Second invoice should have 2 lines, one unsent (status blank) and one RL2", 2, invoice2.CommercialInvoiceLineCollection.Count);

				var invoiceLineShoudlHaveEntryNumber = invoice1.CommercialInvoiceLineCollection.Single(il => il.LineNo == 1);
				AssertEquals("invoice line should have EntryNumber", "ATC996151771020016388", invoiceLineShoudlHaveEntryNumber.EntryNumber);
				AssertEquals("invoice line should have EntryLineNumber", (ZShort)1, invoiceLineShoudlHaveEntryNumber.EntryLineNumber);

				var invoiceLineShoudlNotHaveEntryNumber = invoice2.CommercialInvoiceLineCollection.Single(il => il.LineNo == 1);
				AssertNull("invoice line should not have EntryNumber", invoiceLineShoudlNotHaveEntryNumber.EntryNumber);
				AssertNull("invoice line should not have EntryLineNumber", invoiceLineShoudlNotHaveEntryNumber.EntryLineNumber);
			});
		}

		public void TestProcessMessage_CusEntryHeader_SetRL1()
		{
			var goodsItem1 = GetGoodsItem("1", "6", "A", "N", "B");
			var goodsItem2 = GetGoodsItem("2", "6", "A", "N", "B");

			AssertCusEntryHeaderStatusUpdated(new[] { goodsItem1.Object, goodsItem2.Object },
				UniversalReferenceConstants.EntryStatus.RL1);
		}

		public void TestProcessMessage_CusEntryHeader_SetRL2()
		{
			var goodsItem1 = GetGoodsItem("1", "6", "A", "J", "J");
			var goodsItem2 = GetGoodsItem("2", "0", "N", "J", "N");

			AssertCusEntryHeaderStatusUpdated(new[] { goodsItem1.Object, goodsItem2.Object },
				UniversalReferenceConstants.EntryStatus.RL2);
		}

		public void TestProcessMessage_CusEntryHeader_SetRL3()
		{
			var goodsItem1 = GetGoodsItem("1", "1", "J", "J", "J");
			var goodsItem2 = GetGoodsItem("2", "9", "N", "N", "N");

			AssertCusEntryHeaderStatusUpdated(new[] { goodsItem1.Object, goodsItem2.Object },
				UniversalReferenceConstants.EntryStatus.RL3);
		}

		public void TestProcessMessage_CusEntryHeader_SetRL4()
		{
			var goodsItem1 = GetGoodsItem("1", "0", "N", "N", "J");
			var goodsItem2 = GetGoodsItem("2", "0", "N", "N", "J");

			AssertCusEntryHeaderStatusUpdated(new[] { goodsItem1.Object, goodsItem2.Object },
				UniversalReferenceConstants.EntryStatus.RL4);
		}

		[TestDate(2020, 09, 24, 15, 31, 00)]
		void AssertCusEntryHeaderStatusUpdated(ICURRELGoodsItem[] goodsItems, ZString expectedEntryStatus)
		{
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(goodsItems);
			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("CH_EntryStatus changed", expectedEntryStatus, entryHeader.CH_EntryStatus);
				AssertEquals("CH_EntryReleaseDate unchanged", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
			});
		}

		[TestDate(2020, 09, 24, 15, 31, 00)]
		public void TestProcessMessage_CusEntryHeader_SetRL5AndReleaseDate()
		{
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var goodsItem = GetGoodsItem("1", "0", "N", "N", "J");
			var goodsItem2 = GetGoodsItem("2", "0", "J", "N", "N");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object, goodsItem2.Object });
			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("CH_EntryStatus changed", UniversalReferenceConstants.EntryStatus.RL5, entryHeader.CH_EntryStatus);
				AssertEquals("CH_EntryReleaseDate changed", new ZDateTime(2020, 09, 24, 15, 31, 00), entryHeader.CH_EntryReleaseDate);
			});
		}

		[TestDate(2020, 09, 24, 15, 31, 00)]
		public void TestProcessMessage_CusEntryHeader_SetRL5ButNotReleaseDate()
		{
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var goodsItem = GetGoodsItem("1", "0", "N", "N", "J");
			var goodsItem2 = GetGoodsItem("2", "0", "J", "N", "N");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object, goodsItem2.Object });
			entryHeader.CH_EntryReleaseDate = new ZDateTime(2019, 09, 24, 15, 31, 00);
			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("CH_EntryStatus changed", UniversalReferenceConstants.EntryStatus.RL5, entryHeader.CH_EntryStatus);
				AssertEquals("CH_EntryReleaseDate unchanged", new ZDateTime(2019, 09, 24, 15, 31, 00), entryHeader.CH_EntryReleaseDate);
			});
		}

		[TestDate(2020, 09, 24, 15, 31, 00)]
		public void TestProcessMessage_CusEntryHeader_SetRLB()
		{
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var goodsItem = GetGoodsItem("1", "0", "N", "N", "J");
			var goodsItem2 = GetGoodsItem("2", "1", "N", "N", "N");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object, goodsItem2.Object });
			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("CH_EntryStatus changed", UniversalReferenceConstants.EntryStatus.RLB, entryHeader.CH_EntryStatus);
				AssertEquals("CH_EntryReleaseDate unchanged", ZDateTime.Empty, entryHeader.CH_EntryReleaseDate);
			});
		}

		public void TestProcessMessage_PartialLines()
		{
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, Message.EM_Status);
				AssertEquals("Line1 ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL5, entryLine.ZG_CustomsStatus);
				AssertEquals("Line2 ZG_CustomsStatus", ZString.Empty, entryLine2.ZG_CustomsStatus);
			});
		}

		public void TestProcessMessage_MultipleLinesStatus()
		{
			var entryLine2 = entryHeader.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 2;
			var entryLine3 = entryHeader.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 3;
			var entryLine4 = entryHeader.AllEntryLines.AddNew();
			entryLine4.CL_LineNumber = 4;
			var entryLine5 = entryHeader.AllEntryLines.AddNew();
			entryLine5.CL_LineNumber = 5;

			var goodsItem = GetGoodsItem("1", "1", "J", "N", "J");
			var goodsItem2 = GetGoodsItem("2", "6", "J", "J", "J");
			var goodsItem3 = GetGoodsItem("3", "6", "A", "J", "J");
			var goodsItem4 = GetGoodsItem("4", "6", "A", "N", "J");
			var goodsItem5 = GetGoodsItem("5", "6", "A", "N", "B");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object, goodsItem2.Object, goodsItem3.Object, goodsItem4.Object, goodsItem5.Object });

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("EM_Status", Enterprise.Messaging.Integration.EDIMessageStatusList.Codes.ProcessedOK, Message.EM_Status);
				AssertEquals("Line1 ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL3, entryLine.ZG_CustomsStatus);
				AssertEquals("Line2 ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL5, entryLine2.ZG_CustomsStatus);
				AssertEquals("Line3 ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL2, entryLine3.ZG_CustomsStatus);
				AssertEquals("Line4 ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL4, entryLine4.ZG_CustomsStatus);
				AssertEquals("Line5 ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL1, entryLine5.ZG_CustomsStatus);
			});
		}

		public void TestProcessMessage_UpdateRegistrationNumbersInRelatedOrder()
		{
			var testHelper = new WhsDataTestHelper(Factory);

			var goodsItem1 = GetGoodsItem("1", ZString.Empty, "J", ZString.Empty, ZString.Empty);
			var goodsItem2 = GetGoodsItem("2", ZString.Empty, "J", ZString.Empty, ZString.Empty);
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATE996151771020016388");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object });

			declaration = testHelper.GetNewDeclaration("IMP", "DECL1234", "ATE996151771020016388", 5.0m);
			declaration.IsOutwardOrderImported = true;

			var entryInstruction = declaration.CustomsEntryInstructions.AddNew();
			entryInstruction.CEI_Style = ImportDeclarationTypeList.Codes.EZL;
			entryInstruction.CEI_Procedure = intoWarehouseWarehousingProcedureCode.Left(2);
			entryInstruction.CEI_OA_Warehouse = declaration.WarehouseDocAddress.E2_OA_Address;

			entryHeader = declaration.ActiveEntryHeaders[0] as CusEntryHeader;
			entryHeader.CH_CEI_Instruction = entryInstruction.PK;
			entryInstruction.CEI_OA_Warehouse2 = declaration.WarehouseDocAddress.E2_OA_Address;

			var (entryLine1, entryLine2) = CreateEntryLinesAndLinkedInvoicesForWarehouseTest(entryHeader, declaration, entryInstruction, testHelper.Part, outOfWarehouseWarehousingProcedureCode);
			entryLine1.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL5;
			entryLine2.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL5;

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
				AssertEquals("atribute 1 updated: EntryKey", "ATE996151771020016388", orderLine1.CustomsData.WB_EntryKey);
				AssertEquals("atribute 2 updated: LineNo", (short)1, orderLine1.CustomsData.WB_EntryLineNo);

				AssertEquals("atribute 2 updated: EntryKey", "ATE996151771020016388", orderLine2.CustomsData.WB_EntryKey);
				AssertEquals("atribute 2 updated: LineNo", (short)2, orderLine2.CustomsData.WB_EntryLineNo);
			});
		}

		public void TestGenerateEmail()
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATA001234560920203302");
			dataProviderMock.Setup(m => m.MRN).Returns("23DE12345678901234");

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CURREL – Customs Release Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CURREL – Customs Release Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Response for Release. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Temporary Registration Number</td><td>ATA001234560920203302</td></tr>"
								   + "<tr><td>Registration Number</td><td>ATC996151771020016389</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
								   + "<tr><td>MRN</td><td>23DE12345678901234</td></tr>"
								   + "<tr><td>Customs Notification</td><td>Notification for test</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable("Single", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_EmptyFieldsNotShown()
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.PresentationModalitiesNotification).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.MRN).Returns(ZString.Empty);

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;
			ProcessMessage(Message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			CombineAssertions(() =>
			{
				AssertNotContains("No Temporary Registration Number", "<td>Temporary Registration Number</td>", email.Body);
				AssertNotContains("No Local Reference Number", "<td>Local Reference Number</td>", email.Body);
				AssertNotContains("No MRN", "<td>MRN</td>", email.Body);
				AssertNotContains("No Customs Notification", "<td>Customs Notification</td>", email.Body);
			});
		}

		public void TestGenerateEmail_AcceptanceFlag_N()
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATA001234560920203302");

			var goodsItem1 = GetGoodsItem("1", ZString.Empty, ZString.Empty, "N", ZString.Empty);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object });

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CURREL – Customs Release – Declaration not accepted Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CURREL – Customs Release – Declaration not accepted Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Response for Release. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Temporary Registration Number</td><td>ATA001234560920203302</td></tr>"
								   + "<tr><td>Registration Number</td><td>ATC996151771020016389</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
								   + "<tr><td>Customs Notification</td><td>Notification for test</td></tr>"
								   + "<tr><td>Notification</td><td>Declaration not accepted</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable("Acceptance flag: ", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_DirectiveFlag_1()
		{
			AssertGenerateEmail_DirectiveFlag("1");
		}

		public void TestGenerateEmail_DirectiveFlag_2()
		{
			AssertGenerateEmail_DirectiveFlag("2");
		}

		public void TestGenerateEmail_DirectiveFlag_3()
		{
			AssertGenerateEmail_DirectiveFlag("3");
		}

		public void TestGenerateEmail_DirectiveFlag_4()
		{
			AssertGenerateEmail_DirectiveFlag("4");
		}

		public void TestGenerateEmail_DirectiveFlag_5()
		{
			AssertGenerateEmail_DirectiveFlag("5");
		}

		public void TestGenerateEmail_DirectiveFlag_9()
		{
			AssertGenerateEmail_DirectiveFlag("9");
		}

		public void AssertGenerateEmail_DirectiveFlag(string directiveFlag)
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATA001234560920203302");

			var goodsItem1 = GetGoodsItem("1", directiveFlag, ZString.Empty, ZString.Empty, ZString.Empty);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object });

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CURREL – Customs Release – Customs Control/Inspection Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CURREL – Customs Release – Customs Control/Inspection Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Response for Release. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
							   + "<tr><td>Temporary Registration Number</td><td>ATA001234560920203302</td></tr>"
							   + "<tr><td>Registration Number</td><td>ATC996151771020016389</td></tr>"
							   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
							   + "<tr><td>Customs Notification</td><td>Notification for test</td></tr>"
							   + "<tr><td>Notification</td><td>Customs Control/Inspection</td></tr>"
							   + "</table>";
			AssertEmailForSingleRecipientWithTable("Directive flag: ", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_RejectionFlag_J()
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATA001234560920203302");

			var goodsItem1 = GetGoodsItem("1", ZString.Empty, ZString.Empty, ZString.Empty, "J");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object });

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CURREL – Customs Release – Declaration rejected Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CURREL – Customs Release – Declaration rejected Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Response for Release. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Temporary Registration Number</td><td>ATA001234560920203302</td></tr>"
								   + "<tr><td>Registration Number</td><td>ATC996151771020016389</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
								   + "<tr><td>Customs Notification</td><td>Notification for test</td></tr>"
								   + "<tr><td>Notification</td><td>Declaration rejected</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable("Rejection flag: ", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_MultipleFlags_Acceptance_Directive()
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATA001234560920203302");

			var goodsItem1 = GetGoodsItem("1", ZString.Empty, ZString.Empty, "N", ZString.Empty);
			var goodsItem2 = GetGoodsItem("2", "9", ZString.Empty, ZString.Empty, ZString.Empty);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object });

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CURREL – Customs Release – Declaration not accepted / Customs Control/Inspection Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CURREL – Customs Release – Declaration not accepted / Customs Control/Inspection Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Response for Release. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Temporary Registration Number</td><td>ATA001234560920203302</td></tr>"
								   + "<tr><td>Registration Number</td><td>ATC996151771020016389</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
								   + "<tr><td>Customs Notification</td><td>Notification for test</td></tr>"
								   + "<tr><td>Notification</td><td>Declaration not accepted<br>Customs Control/Inspection</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable("Acceptance and directive flags: ", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_MultipleFlags_Rejection_Directive()
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATA001234560920203302");

			var goodsItem1 = GetGoodsItem("1", "9", ZString.Empty, ZString.Empty, ZString.Empty);
			var goodsItem2 = GetGoodsItem("2", ZString.Empty, ZString.Empty, ZString.Empty, "J");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object });

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CURREL – Customs Release – Declaration rejected / Customs Control/Inspection Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CURREL – Customs Release – Declaration rejected / Customs Control/Inspection Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Response for Release. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Temporary Registration Number</td><td>ATA001234560920203302</td></tr>"
								   + "<tr><td>Registration Number</td><td>ATC996151771020016389</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
								   + "<tr><td>Customs Notification</td><td>Notification for test</td></tr>"
								   + "<tr><td>Notification</td><td>Declaration rejected<br>Customs Control/Inspection</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable("Rejection and directive flags: ", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_MultipleFlags_Acceptance_Rejection()
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATA001234560920203302");

			var goodsItem1 = GetGoodsItem("1", ZString.Empty, ZString.Empty, "N", ZString.Empty);
			var goodsItem2 = GetGoodsItem("2", ZString.Empty, ZString.Empty, ZString.Empty, "J");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1.Object, goodsItem2.Object });

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var declaration = entryHeader.Declaration;
			var reference = declaration.JE_DeclarationReference;
			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();

			var subject = $"Import CURREL – Customs Release – Declaration rejected / Declaration not accepted Response for {reference}";
			var bodyMessageTitle = $"<title>{subject}</title>";
			var bodyMessageHeader = $@"<strong>Import CURREL – Customs Release – Declaration rejected / Declaration not accepted Response for <a href=""edient:Command=ShowEditForm&LicenceCode={GlbCompany.CurrentCompany.LicenceKeyIdentifier}&ControllerID=JobDeclaration&BusinessEntityPK=" + declaration.PK;
			var bodyMessageSummary = $@"Your Import Declaration for Job {reference} received a Customs Response for Release. For details please follow the link to the job.";
			var bodyMessageTable = @"<table border=""1"" cellpadding=""1"" cellspacing=""0"" class=""table"">"
								   + "<tr><td>Temporary Registration Number</td><td>ATA001234560920203302</td></tr>"
								   + "<tr><td>Registration Number</td><td>ATC996151771020016389</td></tr>"
								   + "<tr><td>Local Reference Number</td><td>MAS/22/11/22027</td></tr>"
								   + "<tr><td>Customs Notification</td><td>Notification for test</td></tr>"
								   + "<tr><td>Notification</td><td>Declaration rejected<br>Declaration not accepted</td></tr>"
								   + "</table>";
			AssertEmailForSingleRecipientWithTable("Acceptance and rejection flags: ", email, "test@mail.com", subject, bodyMessageTitle, bodyMessageHeader, bodyMessageSummary, bodyMessageTable);
		}

		public void TestGenerateEmail_WhenReportAttachedInMessage_ShouldContainReportAsAttachment()
		{
			messageMock.Setup(m => m.AttachedDocuments).Returns(new List<AttachedDocument>
			{
				new AttachedDocument
				{
					FileName = "ZBE-1-DE9007458-0000-DE005876_58760000003474619.pdf",
					Type = new DocumentType { Code = "CAU", Description = "Report GCRELF" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("XXX=")),
				},
				new AttachedDocument
				{
					FileName = "file2.pdf",
					Type = new DocumentType { Code = "BBB", Description = "BBB Desc" },
					ImageData = (SubStreamableStream)new MemoryStream(Convert.FromBase64String("YYY=")),
				}
			});

			var user = Factory.NewWithValidTestData<GlbStaff>();
			user.GS_EmailAddress = "test@mail.com";
			outgoingMessage.EM_SystemCreateUser = user.GS_Code;

			ProcessMessage(Message);

			var email = Env.OutgoingCustomsMailManager.EmailsCreated.Single();
			var emailAttachments = email.Attachments.Cast<AttachmentDef>().ToList();

			var reportFile = emailAttachments.Single(x =>
				x.DisplayName == "ZBE-1-DE9007458-0000-DE005876_58760000003474619.pdf");
			AssertEquals("Report file content", Convert.FromBase64String("XXX="), reportFile.Data);

			var nonReportFile = emailAttachments.FirstOrDefault(x => x.DisplayName == "BBB Desc");
			AssertNull("Non-report file is not attached", nonReportFile);
		}

		public void TestPopulateLogbookRegNum()
		{
			dataProviderMock.Setup(m => m.TemporaryReferenceNumber).Returns("ATA001234560920203302");
			dataProviderMock.Setup(m => m.MRN).Returns("23DE12345678901234");
			ProcessMessage(Message);
			AssertEquals("ATC996151771020016389, ATA001234560920203302, 23DE12345678901234", Message.GetLogbookRegistrationNumber());
		}

		public void TestProcessMessage_WhenRejectionFlagN_LineStatusSetToRL2()
		{
			var goodsItem = GetGoodsItem("1", ZString.Empty, ZString.Empty, ZString.Empty, "N");

			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object });

			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				AssertEquals("ZG_CustomsStatus", UniversalReferenceConstants.EntryStatus.RL2, entryLine.ZG_CustomsStatus);
			});
		}

		public void TestSetCusReconEntryLineStatus_RL3()
		{
			TestProcessMessage_CRL_CustomsStatus(
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("1", "1", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("2", "2", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("3", "3", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("4", "4", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("5", "5", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("6", "9", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("7", "1", "J", "J", "J")));
		}

		public void TestSetCusReconEntryLineStatus_RL5()
		{
			TestProcessMessage_CRL_CustomsStatus(
				new TestCase(UniversalReferenceConstants.EntryStatus.RL5, GetGoodsItem("1", "0", "J", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL5, GetGoodsItem("2", "0", "J", "J", "J")));
		}

		public void TestSetCusReconEntryLineStatus_RL2()
		{
			TestProcessMessage_CRL_CustomsStatus(
				new TestCase(UniversalReferenceConstants.EntryStatus.RL2, GetGoodsItem("1", "0", "N", "J", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL2, GetGoodsItem("2", "0", "N", "J", "J")));
		}

		public void TestSetCusReconEntryLineStatus_RL4()
		{
			TestProcessMessage_CRL_CustomsStatus(
				new TestCase(UniversalReferenceConstants.EntryStatus.RL4, GetGoodsItem("1", "0", "N", "N", "J")));
		}

		public void TestSetCusReconEntryLineStatus_RL1()
		{
			TestProcessMessage_CRL_CustomsStatus(
				new TestCase(UniversalReferenceConstants.EntryStatus.RL1, GetGoodsItem("1", "0", "N", "N", "N")));
		}

		public void TestSetCusReconEntryLineStatus_CusReconEntryNull()
		{
			CombineAssertions(() =>
			{
				AssertNull("CusReconEntry is null", entryHeader.GetCusReconEntry());
				AssertNoExceptionThrown(() => ProcessMessage(Message));
			});
		}

		public void TestSetCusReconEntryLineStatus_CusReconEntryLineNull()
		{
			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			CombineAssertions(() =>
			{
				AssertNull("CusReconEntryLine is null", cusReconEntry.GetCusReconEntryLineByOriginalEntryLineNumber("1"));
				AssertNoExceptionThrown(() => ProcessMessage(Message));
			});
		}

		public void TestSetCusReconEntryLineStatus_All_EntryLines_Deleted()
		{
			TestProcessMessage_CRL_CustomsStatus(
				new TestCase(UniversalReferenceConstants.EntryStatus.RL4, GetGoodsItem("1", "0", "N", "N", "J")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL1, GetGoodsItem("2", "0", "N", "N", "N")));
		}

		public void TestSetCusReconEntryLineStatus_Some_EntryLines_Deleted()
		{
			TestProcessMessage_CRL_CustomsStatus(
				new TestCase(UniversalReferenceConstants.EntryStatus.RL4, GetGoodsItem("1", "0", "N", "N", "J")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL2, GetGoodsItem("2", "0", "N", "J", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL1, GetGoodsItem("3", "0", "N", "N", "N")));
		}

		public void TestSetCusReconEntryLineStatus_No_EntryLines_Deleted()
		{
			TestProcessMessage_CRL_CustomsStatus(
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("1", "1", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("2", "2", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("3", "3", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("4", "4", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("5", "5", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("6", "9", "N", "N", "N")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL3, GetGoodsItem("7", "1", "J", "J", "J")),
				new TestCase(UniversalReferenceConstants.EntryStatus.RL2, GetGoodsItem("8", "0", "N", "J", "N")));
		}

		public void TestProcessMessage_CUSTAXMessageAlreadyProcessed()
		{
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX1;

			entryLine.ZG_CustomsStatus = UniversalReferenceConstants.EntryStatus.RL2;

			var goodsItem = GetGoodsItem("1", "1", "J", "N", "J"); // Normally would cause EntryLine status to be set to 'RL3'
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object });

			ProcessMessage(Message);
			CombineAssertions(() =>
			{
				AssertEquals("Line1 ZG_CustomsStatus not updated as Entry Header CH_EntryStatus has 'TX' prefix", UniversalReferenceConstants.EntryStatus.RL2, entryLine.ZG_CustomsStatus);

				var logEvent = entryHeader.Logs.MostRecentLogByEventTime(Events.CustomsEntryStatus, e => e.SL_Reference.In(new List<ZString> { UniversalReferenceConstants.EntryStatus.RL2, UniversalReferenceConstants.EntryStatus.RL3 }));
				AssertNull("No CustomsEntryStatus event created for the line status as CH_EntryStatus has 'TX' prefix", logEvent);
			});
		}

		public void TestProcessMessage_NoCusReconLineDeletedWhenCUSTAXMessageAlreadyProcessed()
		{
			entryHeader.CH_EntryStatus = UniversalReferenceConstants.EntryStatus.TX1;

			TestProcessMessage_CRL_CustomsStatus(new TestCase(expectedStatus: ZString.Empty, GetGoodsItem("5", "6", "A", "N", "B")));
		}

		void TestProcessMessage_CRL_CustomsStatus(params TestCase[] testCases)
		{
			for (var i = 1; i < testCases.Length; i++)
			{
				var line = entryHeader.AllEntryLines.AddNew();
				line.CL_LineNumber = new ZShort(testCases[i].GoodsItemMock.Object.SequenceNumber);
			}

			var cusReconEntry = Factory.New<CusReconEntry>();
			cusReconEntry.CRE_CH_OriginalEntry = entryHeader.PK;
			var entrySnapshot = cusReconEntry.CusReconSnapshots.AddNew();

			var lineSnapshotsToBeDeleted = new List<CusReconSnapshot>();
			var expectedStatusesOfCusReconEntryLines = new Dictionary<ZGuid, string>();

			foreach (var testCase in testCases)
			{
				var cusReconEntryLine = cusReconEntry.CusReconEntryLines.AddNew();
				cusReconEntryLine.CRL_OriginalEntryLineNumber = new ZShort(testCase.GoodsItemMock.Object.SequenceNumber);

				var lineSnapshot = cusReconEntryLine.CusReconSnapshots.AddNew();

				if (testCase.ShouldBeDeleted && !entryHeader.CH_EntryStatus.StartsWith("TX"))
				{
					lineSnapshotsToBeDeleted.Add(lineSnapshot);
				}
				else
				{
					expectedStatusesOfCusReconEntryLines.Add(cusReconEntryLine.PK, testCase.ExpectedStatus);
				}
			}

			dataProviderMock.Setup(m => m.GoodsItems).Returns(testCases.Select(m => m.GoodsItemMock.Object).ToArray());
			ProcessMessage(Message);

			CombineAssertions(() =>
			{
				var nRemainingEntries = testCases.Count(td => !(td.ShouldBeDeleted && !entryHeader.CH_EntryStatus.StartsWith("TX")));

				AssertEquals("# Remaining CusReconEntryLines", nRemainingEntries, cusReconEntry.CusReconEntryLines.Count);

				for (int i = 0; i < cusReconEntry.CusReconEntryLines.Count; i++)
				{
					var line = cusReconEntry.CusReconEntryLines[i];
					AssertEquals($"CRL_CustomsStatus[{i}]", expectedStatusesOfCusReconEntryLines[line.PK], line.CRL_CustomsStatus);
				}

				var cusReconEntryShouldBeDeleted = testCases.All(td => td.ShouldBeDeleted && !entryHeader.CH_EntryStatus.StartsWith("TX"));

				AssertEquals($"CusReconEntry should be deleted if all elements of {nameof(CusReconEntry.CusReconEntryLines)} are deleted and the Entry Header CH_Status does not start with 'TX'", cusReconEntryShouldBeDeleted, cusReconEntry.IsDeleted);
				AssertEquals("CusReconEntrySnapshot should be deleted if the CusReconEntry is deleted the Entry Header CH_Status does not start with 'TX'", cusReconEntryShouldBeDeleted, entrySnapshot.IsDeleted);

				foreach (var shouldBeDeleted in lineSnapshotsToBeDeleted)
				{
					AssertEquals($"Lines where {nameof(CusReconEntryLine.CRL_CustomsStatus)} expected to be in (RL1,RL4) should be deleted", true, shouldBeDeleted.IsDeleted);
				}
			});
		}

		class TestCase
		{
			public string ExpectedStatus { get; set; }
			public Mock<ICURRELGoodsItem> GoodsItemMock { get; set; }

			public bool ShouldBeDeleted => ExpectedStatus == UniversalReferenceConstants.EntryStatus.RL1 ||
										   ExpectedStatus == UniversalReferenceConstants.EntryStatus.RL4;

			public TestCase(string expectedStatus, Mock<ICURRELGoodsItem> goodsItemMock)
			{
				ExpectedStatus = expectedStatus;
				GoodsItemMock = goodsItemMock;
			}
		}

		protected override bool ExpectedDelayStatusError => true;

		protected override ZString MessageFriendlyName => "Import CURREL Message Processor";

		protected override DEBranchCustomsApplicationTypeMessageProcessor<AtlasInboundEDIMessage<ICURREL>> Processor => new ImportCURRELMessageProcessor(logger);

		protected override AtlasInboundEDIMessage<ICURREL> Message => messageMock.Object;

		protected override void SetUp()
		{
			base.SetUp();

			Factory.NewWithValidTestData<RefDataGrouping>().ZZZ_DataGrouping = "DE";
			CreateRefProcedure(outOfWarehouseWarehousingProcedureCode, "Y", "N");
			CreateRefProcedure(intoWarehouseWarehousingProcedureCode, "N", "Y");

			entryHeader = (CusEntryHeader)declaration.ActiveEntryHeaders.AddNew();
			var mrnEntryNumber = CusEntryNumber.LoadOrCreate(entryHeader, CusEntryNumberTypes.Standard.MovementReferenceNumber, Core.Constants.CountryCodes.Germany);
			mrnEntryNumber.CE_EntryNum = "ATC996151771020016389";
			entryLine = entryHeader.AllEntryLines.AddNew();
			entryLine.CL_LineNumber = 1;

			outgoingMessage = SetupOutgoingMessage(entryHeader);

			var goodsItem = GetGoodsItem("1", ZString.Empty, "J", ZString.Empty, ZString.Empty);

			dataProviderMock = new Mock<ICURREL> { CallBase = true };
			dataProviderMock.Setup(x => x.MessageIdentifier).Returns("FINTAX20833294803129238170736212505");
			dataProviderMock.Setup(x => x.ReferenceNumber).Returns("ATC996151771020016389");
			dataProviderMock.Setup(x => x.TemporaryReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(x => x.LocalReferenceNumber).Returns("MAS/22/11/22027");
			dataProviderMock.Setup(x => x.PresentationModalitiesNotification).Returns("Notification for test");
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem.Object });

			messageMock = Factory.NewMoq<AtlasInboundEDIMessage<ICURREL>>();
			messageMock.Setup(m => m.DataProvider).Returns(dataProviderMock.Object);
			Factory.Save();
		}

		void CreateRefProcedure(ZString procedureCode, string isOutOfWarehouse, string isIntoWarehouse)
		{
			var procedureOutOfWarehouse = Factory.NewWithValidTestData<RefCusProcedure>();
			procedureOutOfWarehouse.ZZ6_ZZZ_NKDataGrouping = "DE";
			procedureOutOfWarehouse.ZZ6_ShipmentType = "IMP";
			procedureOutOfWarehouse.ZZ6_ProcedureCode = procedureCode.Left(2);
			procedureOutOfWarehouse.ZZ6_Concession = procedureCode.PadRight(7).Right(3);
			procedureOutOfWarehouse.ZZ6_OutOfWarehouse = isOutOfWarehouse;
			procedureOutOfWarehouse.ZZ6_IntoWarehouse = isIntoWarehouse;
			procedureOutOfWarehouse.ZZ6_PreviousProcedureCode = procedureCode.Substring(2, 2);
		}

		CusEntryHeader entryHeader;
		CusEntryLine entryLine;
		Mock<ICURREL> dataProviderMock;
		Mock<AtlasInboundEDIMessage<ICURREL>> messageMock;
		EDIMessage outgoingMessage;
		readonly ZString outOfWarehouseWarehousingProcedureCode = "4071";
		readonly ZString intoWarehouseWarehousingProcedureCode = "7100";

		EDIMessage SetupOutgoingMessage(BusinessObject parent)
		{
			var message = CreateOriginalMessageLinkedToParent<AtlasEDIMessage>(parent, "outgoing");
			message.EM_ApplicationCode = EDIMessage.ApplicationCodes.DECustomsAtlasSystem;
			message.EM_MessageType = EDIMessageTypeList.Codes.Import;
			message.EM_MessageSubType = ImportMessageSubTypeList.Codes.FreeCirculationSingleDeclaration;
			message.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;

			return message;
		}

		Mock<ICURRELGoodsItem> GetGoodsItem(ZString sequenceNumber, ZString directiveFlag, ZString issuingFlag, ZString acceptanceFlag, ZString rejectionFlag)
		{
			var goodsItem = new Mock<ICURRELGoodsItem> { CallBase = true };
			goodsItem.Setup(x => x.SequenceNumber).Returns(sequenceNumber);
			goodsItem.Setup(x => x.DirectiveFlag).Returns(directiveFlag);
			goodsItem.Setup(x => x.IssuingFlag).Returns(issuingFlag);
			goodsItem.Setup(x => x.AcceptanceFlag).Returns(acceptanceFlag);
			goodsItem.Setup(m => m.RejectionFlag).Returns(rejectionFlag);
			return goodsItem;
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
	}
}
