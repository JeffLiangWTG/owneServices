using System;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.Business.WarehouseExtensions;
using Enterprise.Customs.Common.AU.CMR;
using Enterprise.Customs.DataRegistry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Warehouse.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Schema;
using WarehouseTransactionStatusList = Enterprise.Customs.Business.WarehouseTransactionStatusList;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class DSAMessageProcessorTest : BaseImportDeclarationMessageProcessorTest
	{
		public void TestDSAMessageIsDiscardedWhenDisableDSAMessages()
		{
			// set up a DSA Message
			// process the message
			// verify message status is 'Discarded'
			// verify no changes were applied to declaration

			using (AUCustomsDataRegistry.Instance.DisableDSAMessages.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var testJobDeclaration = Factory.New<JobDeclaration>();
				testJobDeclaration.JE_DeclarationReference = "S00001178";
				var entryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
				entryHeader.CH_BGMReference = "S00001178/1";
				entryHeader.EntryNumber = "AAACG4CNN";

				var message1 = Factory.New<CMRDSAMessage>();
				message1.EM_MessageText = CMRImportDeclarationTestData.DSA;
				message1.EM_MessageNum = "1";

				testJobDeclaration.JE_HouseBill = "123456";
				var packGroup1 = (PackingGroup)testJobDeclaration.Bills[0].PackingGroups.AddNew();
				packGroup1.CR_HouseContainerNumber = 1;
				var packGroup2 = (PackingGroup)testJobDeclaration.Bills[0].PackingGroups.AddNew();
				packGroup2.CR_HouseContainerNumber = 2;
				var pack1 = packGroup1.Packages.AddNew();
				var pack2 = packGroup2.Packages.AddNew();

				AssertNull("Pre-condition", message1.EM_LinkedObject);
				AssertEquals("Pre-condition", 0, entryHeader.Messages.Count);
				AssertEquals("Pre-condition", 0, packGroup1.Messages.Count);
				AssertEquals("Pre-condition", 0, packGroup2.Messages.Count);
				AssertNotEquals("Pre-condition", EDIMessage.Status.Received, message1.EM_Status);
				AssertNotEquals("Pre-condition", CMRImportEntryAdvice.Finalised.Code, testJobDeclaration.JE_EntryStatus);
				AssertNotEquals("Pre-condition", CMRImportEntryAdvice.Finalised.Code, message1.EM_MessageSubType);

				var originalDeclarationStatus = testJobDeclaration.JE_EntryStatus;
				var originalEntryHeaderStatus = entryHeader.CH_EntryStatus;

				CMRMessageResponseProcessor processor = GetMessageProcessor();
				processor.PreProcessMessage(message1);
				processor.ProcessMessage(message1);
				AssertEquals("Message Status", EDIMessage.Status.Discarded, message1.EM_Status);

				CombineAssertions(() =>
				{
					AssertEquals("Linked object", null, message1.EM_LinkedObject);
					AssertEquals("No messages added to Entry Header", 0, entryHeader.Messages.Count);
					AssertEquals("No messages should be cloned/added to the packing group", 0, packGroup1.Messages.Count);
					AssertEquals("No messages should be cloned/added to the packing group", 0, packGroup2.Messages.Count);
					AssertEquals("Declaration Status unchanged", originalDeclarationStatus, testJobDeclaration.JE_EntryStatus);
					AssertEquals("Entry Header Status unchanged", originalEntryHeaderStatus, entryHeader.CH_EntryStatus);

					AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "", packGroup1.GetAbbreviatedStatusDescriptionForTransportLine());
					AssertEquals("Should be cached cargoStatusForTransportLine", "", packGroup1.GetCargoStatusFromLatestMessage());
					AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "", packGroup2.GetAbbreviatedStatusDescriptionForTransportLine());
					AssertEquals("Should be cached cargoStatusForTransportLine", "", packGroup2.GetCargoStatusFromLatestMessage());

					AssertEquals("", testJobDeclaration.JE_ConsolidatedCargoStatus);
					AssertEquals("", packGroup1.CR_CargoStatus);
					AssertEquals("", packGroup2.CR_CargoStatus);
				});
			}
		}

		public void TestProcessDSAMessage1()
		{
			var testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_DeclarationReference = "S00001178";
			var entryHeader = testJobDeclaration.CustomsEntryHeaders.AddNew();
			entryHeader.CH_BGMReference = "S00001178/1";
			entryHeader.EntryNumber = "AAACG4CNN";

			testJobDeclaration.JE_HouseBill = "123456";
			var container1 = declaration.CusContainers.AddNew();
			var container2 = declaration.CusContainers.AddNew();
			var packGroup1 = (PackingGroup)testJobDeclaration.Bills[0].PackingGroups.AddNew();
			packGroup1.CR_HouseContainerNumber = 1;
			packGroup1.CR_CO_Container = container1.PK;
			var packGroup2 = (PackingGroup)testJobDeclaration.Bills[0].PackingGroups.AddNew();
			packGroup2.CR_HouseContainerNumber = 2;
			packGroup1.CR_CO_Container = container2.PK;
			var pack1 = packGroup1.Packages.AddNew();
			var pack2 = packGroup2.Packages.AddNew();

			var message1 = Factory.New<CMRDSAMessage>();
			message1.EM_MessageText = CMRImportDeclarationTestData.DSA;
			message1.EM_MessageNum = "1";

			AssertNull("Pre-condition", message1.EM_LinkedObject);
			AssertEquals("Pre-condition", 0, entryHeader.Messages.Count);
			AssertEquals("Pre-condition", 0, packGroup1.Messages.Count);
			AssertEquals("Pre-condition", 0, packGroup2.Messages.Count);
			AssertNotEquals("Pre-condition", EDIMessage.Status.Received, message1.EM_Status);
			AssertNotEquals("Pre-condition", CMRImportEntryAdvice.Finalised.Code, testJobDeclaration.JE_EntryStatus);
			AssertNotEquals("Pre-condition", CMRImportEntryAdvice.Finalised.Code, message1.EM_MessageSubType);

			var processor = GetMessageProcessor();
			processor.PreProcessMessage(message1);
			processor.ProcessMessage(message1);
			Factory.Save();
			AssertEquals("Linked object", entryHeader, message1.EM_LinkedObject);
			AssertEquals("1 message added to Entry Header", 1, entryHeader.Messages.Count);
			AssertEquals("No messages should be cloned/added to the packing group anymore", 0, packGroup1.Messages.Count);
			AssertEquals("No messages should be cloned/added to the packing group anymore", 0, packGroup2.Messages.Count);
			AssertEquals("Message Status", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("Declaration Status", CMRImportEntryAdvice.Finalised.Code, testJobDeclaration.JE_EntryStatus);
			AssertEquals("Entry Header Status", CMRImportEntryAdvice.Finalised.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Message sub-type", CMRImportEntryAdvice.Finalised.Code, message1.EM_MessageSubType);

			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "Y/N/Y/Y", packGroup1.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "HELD", packGroup1.GetCargoStatusFromLatestMessage());
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "Y/Y/Y/Y", packGroup2.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "CLEAR", packGroup2.GetCargoStatusFromLatestMessage());

			AssertEquals("MIX", testJobDeclaration.JE_ConsolidatedCargoStatus);
			AssertEquals("HLD", packGroup1.CR_CargoStatus);
			AssertEquals("CLR", packGroup2.CR_CargoStatus);

			message1 = Factory.New<CMRDSAMessage>();
			message1.EM_MessageText = CMRImportDeclarationTestData.DSA.Replace(":HELD'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'", ":XXXX'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'")
																		.Replace(":CLEAR'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'", ":HELD'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'").Replace(":XXXX'", ":CLEAR'");
			message1.EM_MessageNum = "2";

			processor = GetMessageProcessor();
			processor.PreProcessMessage(message1);
			processor.ProcessMessage(message1);
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "Y/Y/Y/Y", packGroup1.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "CLEAR", packGroup1.GetCargoStatusFromLatestMessage());
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "Y/N/Y/Y", packGroup2.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "HELD", packGroup2.GetCargoStatusFromLatestMessage());
		}

		public void TestProcessDSAMessage2()
		{
			JobDeclaration testJobDeclaration = Factory.New<JobDeclaration>();
			testJobDeclaration.JE_DeclarationReference = "S00001178";

			CMRDSAMessage message1 = Factory.New<CMRDSAMessage>();
			message1.EM_MessageText = CMRImportDeclarationTestData.DSA.Replace("S00001178/1/DAT1", "S00001178");

			AssertNull("Pre-condition", message1.EM_LinkedObject);
			AssertEquals("Pre-condition", 0, testJobDeclaration.Messages.Count);
			AssertNotEquals("Pre-condition", EDIMessage.Status.Received, message1.EM_Status);
			AssertNotEquals("Pre-condition", CMRImportEntryAdvice.Finalised.Code, testJobDeclaration.JE_EntryStatus);
			AssertNotEquals("Pre-condition", CMRImportEntryAdvice.Finalised.Code, message1.EM_MessageSubType);

			CMRMessageResponseProcessor processor = GetMessageProcessor();
			processor.PreProcessMessage(message1);
			processor.ProcessMessage(message1);
			AssertEquals("Linked object", testJobDeclaration, message1.EM_LinkedObject);
			AssertEquals("1 message added to declaratopn", 1, testJobDeclaration.Messages.Count);
			AssertEquals("Message Status", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("Message sub-type", CMRImportEntryAdvice.Finalised.Code, message1.EM_MessageSubType);
			AssertEquals("Declaration number", "AAACG4CNN", testJobDeclaration.DeclarationNumber);
			AssertEquals("ErrorEmailCount", 0, processor.ErrorEmailSendCount);
			AssertEquals("AcknowledgementEmailCount", 1, processor.AcknowledgementEmailSendCount);
		}

		public void TestPublishUniversalShipmentToBondedWarehouseInwardOnFinalised()
		{
			var helper = new WhsDataTestHelper(Factory);

			using (helper.WhsHelper.UsePutawayEngineManagerMock())
			{
				CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
				string dSAData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+FJJB HB72 81G:1+11'DTM+9:20161124133400000000:ZZZ'TDT+20++A'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+WISETECH GLOBAL LIMITED'NAD+IM++MS AMY TEST'RFF+ABO:B00160215/1/CMT2::3'RFF+ACW:FID'RFF+AAE:N20'RFF+ABT:ENT0123456::2'RFF+ABQ:WI00120243T1'RFF+ADU:B00160215/1'RFF+AMI:FINALISED'DOC+S+1'UNT+17+000001'";
				JobComInvoiceLine invoiceLine;
				var payrecMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, dSAData, "B00160215", "ENT0123456", 110m, out invoiceLine);
				var declaration = invoiceLine.Declaration;
				Factory.Save();
				declaration.PublishShipmentForWHSInward(true);
				Factory.Save();
				var dataExportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
				var processor = GetMessageProcessor();
				processor.PreProcessMessage(payrecMessage);
				processor.ProcessMessage(payrecMessage);
				Factory.Save();
				declaration.Reload();
				AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreated, declaration.WarehouseTransactionStatus);
				AssertEquals(true, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Levels have been updated. (WHS Receipt: <a href=")));
				AssertNotEquals(dataExportLog, declaration.Logs.MostRecentLogByEventTime(Events.DataExport));
			}
		}

		public void TestNoPublishUniversalShipmentToBondedWarehouseInwardOnHeld()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			string dSAData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+335B 2JIC 131G:1+11'DTM+9:20161124125500000000:ZZZ'TDT+20++A'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+WISETECH GLOBAL LIMITED'NAD+IM++MS AMY TEST'RFF+ABO:B00160215/1/CMT2::3'RFF+ACW:FID'RFF+AAE:N20'RFF+ABT:ENT0123456::2'RFF+ABQ:WI00120243T1'RFF+ADU:B00160215/1'RFF+AMI:HELD'DOC+I'RFF+AEA:F'RFF+ADP:2'RFF+ADQ:CPMATCH'RFF+AFD:192'ERP+:2'DOC+S+'UNT+23+000001'";
			JobComInvoiceLine invoiceLine;
			var payrecMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, dSAData, "B00160215", "ENT0123456", 110m, out invoiceLine);
			var declaration = invoiceLine.Declaration;
			Factory.Save();
			declaration.PublishShipmentForWHSInward(true);
			Factory.Save();
			AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
			var dataExportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
			var processor = GetMessageProcessor();
			processor.PreProcessMessage(payrecMessage);
			processor.ProcessMessage(payrecMessage);
			Factory.Save();
			declaration.Reload();
			AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
			AssertEquals(false, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Levels have been updated. (WHS Receipt: <a href=")));
			AssertEquals(dataExportLog, declaration.Logs.MostRecentLogByEventTime(Events.DataExport));
		}

		public void TestNoPublishUniversalShipmentToBondedWarehouseInwardOnClear()
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			string dSAData = "UNH+000001+CUSRES:D:99B:UN'BGM+961:::DSA+164A GGF8 H81G:1+11'DTM+9:20161124114300000000:ZZZ'TDT+20++A'NAD+MR+AAA374M::95'NAD+VT+AA33HF::95'NAD+CB+54321::95+WISETECH GLOBAL LIMITED'NAD+IM++MS AMY TEST'RFF+ABO:B00160215/1/CMT2::2'RFF+ACW:FID'RFF+AAE:N20'RFF+ABT:ENT0123456::1'RFF+ABQ:WI00120243T1'RFF+ADU:B00160215/1'RFF+AMI:CLEAR'DOC+S+1'UNT+17+000001";
			JobComInvoiceLine invoiceLine;
			var payrecMessage = CreateDataForUniversalTesting(JobMessageTypeList.Codes.Import, dSAData, "B00160215", "ENT0123456", 110m, out invoiceLine);
			var declaration = invoiceLine.Declaration;
			Factory.Save();
			declaration.PublishShipmentForWHSInward(true);
			Factory.Save();
			AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
			var dataExportLog = declaration.Logs.MostRecentLogByEventTime(Events.DataExport);
			var processor = GetMessageProcessor();
			processor.PreProcessMessage(payrecMessage);
			processor.ProcessMessage(payrecMessage);
			Factory.Save();
			declaration.Reload();
			AssertEquals(WarehouseTransactionStatusList.Codes.InwardCreationHeld, declaration.WarehouseTransactionStatus);
			AssertEquals(false, Env.OutgoingCustomsMailManager.EmailsCreated.Exists(x => x.Body.Contains("<p>Stock Levels have been updated. (WHS Receipt: <a href=")));
			AssertEquals(dataExportLog, declaration.Logs.MostRecentLogByEventTime(Events.DataExport));
		}

		public void TestUpdateConsolidatedDeclaration()
		{
			GlbStaff.CurrentUser.GS_Code = ZArchitecture.Environment.User.ServiceUserCode;

			var consolidatedDeclaration = Customs.Business.Testing.ConsolidatedDeclarationTestHelper.CreateConsolidatedDeclaration<ConsolidatedDeclaration>(Factory);
			var leadDec = consolidatedDeclaration.LeadDeclaration as JobDeclaration;
			leadDec.JE_DeclarationReference = "S00001178";
			var entryHeader = leadDec.CustomsEntryHeaders[0];
			entryHeader.CH_BGMReference = "CE00122382";
			entryHeader.EntryNumber = "AAACG4CNN";

			leadDec.JE_HouseBill = "123456";
			var packGroup1 = (PackingGroup)leadDec.Bills[0].PackingGroups.AddNew();
			packGroup1.CR_HouseContainerNumber = 1;
			var pack1 = packGroup1.Packages.AddNew();

			var otherDec = consolidatedDeclaration.JobDeclarations[1] as JobDeclaration;
			otherDec.JE_HouseBill = "1234567";
			var entryHeader2 = otherDec.CustomsEntryHeaders[0];
			entryHeader2.CH_BGMReference = "CE00122382";
			entryHeader2.EntryNumber = "AAACG4CNN";
			var packGroup2 = (PackingGroup)otherDec.Bills[0].PackingGroups.AddNew();
			packGroup2.CR_HouseContainerNumber = 2;
			var pack2 = packGroup2.Packages.AddNew();
			Factory.Save();

			consolidatedDeclaration.CRD_JobReferenceNumber = "CE00122382";
			var message1 = Factory.New<CMRDSAMessage>();
			message1.EM_MessageText = CMRImportDeclarationTestData.DSAForConsolidatedDeclaration;
			message1.EM_MessageNum = "1";

			AssertNull("Pre-condition", message1.EM_LinkedObject);
			AssertEquals("Pre-condition", 0, entryHeader.Messages.Count);
			AssertEquals("Pre-condition", 0, packGroup1.Messages.Count);
			AssertEquals("Pre-condition", 0, packGroup2.Messages.Count);
			AssertNotEquals("Pre-condition", EDIMessage.Status.Received, message1.EM_Status);
			AssertNotEquals("Pre-condition", CMRImportEntryAdvice.Finalised.Code, message1.EM_MessageSubType);
			AssertNotEquals("Pre-condition", CMRImportEntryAdvice.Finalised.Code, leadDec.JE_EntryStatus);

			var processor = GetMessageProcessor();
			processor.PreProcessMessage(message1);
			processor.ProcessMessage(message1);
			Factory.Save();
			AssertEquals("Linked object", consolidatedDeclaration, message1.EM_LinkedObject);
			AssertEquals("1 message added to ConsolidatedDeclaration", 1, consolidatedDeclaration.Messages.Count);
			AssertEquals("No messages should be cloned/added to the packing group anymore", 0, packGroup1.Messages.Count);
			AssertEquals("No messages should be cloned/added to the packing group anymore", 0, packGroup2.Messages.Count);
			AssertEquals("Message Status", EDIMessage.Status.Received, message1.EM_Status);
			AssertEquals("Declaration Status", CMRImportEntryAdvice.Finalised.Code, leadDec.JE_EntryStatus);
			AssertEquals("Declaration Status", CMRImportEntryAdvice.Finalised.Code, otherDec.JE_EntryStatus);
			AssertEquals("Entry Header Status", CMRImportEntryAdvice.Finalised.Code, entryHeader.CH_EntryStatus);
			AssertEquals("Message sub-type", CMRImportEntryAdvice.Finalised.Code, message1.EM_MessageSubType);

			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "Y/N/Y/Y", packGroup1.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "HELD", packGroup1.GetCargoStatusFromLatestMessage());
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "Y/Y/Y/Y", packGroup2.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "CLEAR", packGroup2.GetCargoStatusFromLatestMessage());

			AssertEquals("HLD", leadDec.JE_ConsolidatedCargoStatus);
			AssertEquals("CLR", otherDec.JE_ConsolidatedCargoStatus);
			AssertEquals("HLD", packGroup1.CR_CargoStatus);
			AssertEquals("CLR", packGroup2.CR_CargoStatus);

			message1 = Factory.New<CMRDSAMessage>();
			message1.EM_MessageText = CMRImportDeclarationTestData.DSAForConsolidatedDeclaration.Replace(":HELD'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'", ":XXXX'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'")
																		.Replace(":CLEAR'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:YES'", ":HELD'CST+1'FTX+AHN+++CARGO REPORT ACS EVALUATED:NO'").Replace(":XXXX'", ":CLEAR'");
			message1.EM_MessageNum = "2";

			processor = GetMessageProcessor();
			processor.PreProcessMessage(message1);
			processor.ProcessMessage(message1);
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "Y/Y/Y/Y", packGroup1.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "CLEAR", packGroup1.GetCargoStatusFromLatestMessage());
			AssertEquals("Should be cached abbreviatedCargoStatusDescriptionForTransportLine", "Y/N/Y/Y", packGroup2.GetAbbreviatedStatusDescriptionForTransportLine());
			AssertEquals("Should be cached cargoStatusForTransportLine", "HELD", packGroup2.GetCargoStatusFromLatestMessage());
		}

		protected override ZString GetExpectedMessageCode() => CMRMessage.CMRMessageTypes.DSA;

		protected override ZString GetExpectedMessageName() => "Declaration Status Advice Message (DSA)";

		protected override CMRMessageResponseProcessor GetMessageProcessor() => new DSAMessageProcessor(logger);

		protected override Type IncomingMessageType => typeof(CMRDSAMessage);

		CMRDSAMessage CreateDataForUniversalTesting(ZString messageType, ZString messageText, ZString declarationReference, ZString entryNumber, ZDecimal quantity, out JobComInvoiceLine invoiceLine)
		{
			CustomsDataRegistry.Instance.WHSUniversalXMLChangeDate.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, ZDateTime.Now.AddYears(-1).ToDateTime());
			var entryHeader = GetNewEntryHeader(messageType, declarationReference, entryNumber, quantity);
			var dSAMessage = (CMRDSAMessage)entryHeader.Messages.AddNew(typeof(CMRDSAMessage));
			dSAMessage.EM_MessageText = messageText;
			dSAMessage.EM_LinkedObject = entryHeader;
			var entryLine = entryHeader.MergedLines[0];
			invoiceLine = entryLine.RandomLine;
			return dSAMessage;
		}

		CusEntryHeader GetNewEntryHeader(ZString messageType, ZString declarationReference, ZString entryNumber, ZDecimal quantity)
		{
			AssertNotNull(PostMasterGroup);
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = messageType;
			declaration.JE_OH_Importer = Importer.PK;
			declaration.JE_DeclarationReference = declarationReference;
			declaration.WarehouseDocAddress.E2_OA_Address = Warehouse.MainAddress.PK;

			var warehouse = Factory.LoadTop1<IWhsWarehouse>(new ZQuery(WhsWarehouseSchema.WW_WarehouseCode, "WHS"));
			if (warehouse == null)
			{
				var helper = ObjectFactory.New<IWhsTransactionTestHelper>(Factory);
				warehouse = (IWhsWarehouse)helper.CreateWarehouse("WHS", "BOND");
				warehouse.WW_WarehouseName = Warehouse.MainAddress.OA_Address1;
				warehouse.WW_OA_WarehouseAddress = Warehouse.MainAddress.PK;
				warehouse.WW_IsBondedWarehouse = true;
				warehouse.WW_IsVirtualWarehouse = true;
				((IWhsArea)warehouse.Areas[0]).WA_AreaType = "BON";
				warehouse.WW_GB_RelatedCompanyBranch = base.entryHeader.RegistryBranchPK;
				warehouse.WW_AutoPrintPackingSlip = false;
			}

			declaration.Invoices.DeleteAll();
			var invoice = declaration.Invoices.AddNew();
			invoice.AddInfo.ZA_ORG = Core.Constants.CountryCodes.France;
			invoice.JZ_InvoiceAmount = quantity * 100m;
			invoice.JZ_RX_NKInvoice_Currency = Core.Constants.CurrencyCodes.Australia;
			invoice.Charges.AddNew("OFT", quantity * 10m, Core.Constants.CurrencyCodes.Australia);

			var invoiceLine = invoice.JobComInvoiceLines.AddNew();
			invoiceLine.JI_PartNo = Part.OP_PartNum;
			invoiceLine.JI_IsPackToBondForLine = true;
			invoiceLine.JI_InvoiceQuantity = quantity;
			invoiceLine.JI_InvoiceUQ = "NO";
			invoiceLine.JI_CustomsUnitQty = "KG";
			invoiceLine.JI_CustomsQuantity = quantity * 10m;
			invoiceLine.JI_LinePrice = quantity * 100m;
			invoiceLine.AddInfo.ZA_WRQ = quantity;
			invoiceLine.AddInfo.ZA_WRU = "NO";
			declaration.MessageInitiator = new SendsMessagesToCustomsShutterUpperer();
			declaration.DoMerge();
			var entryHeader = declaration.CustomsEntryHeaders[0];
			entryHeader.EntryNumber = entryNumber;
			return entryHeader;
		}

		GlbGroup PostMasterGroup
		{
			get
			{
				if (postMasterGroup == null)
				{
					postMasterGroup = Factory.Load<GlbGroup>(Core.Constants.Groups.PostMastersGroupPK);
					postMasterGroup.Staff[0].GS_EmailAddress = "test@cargowise.com";
					var groupNotification = new AutoBillingGroupNotification();
					groupNotification.SendGroupPK = postMasterGroup.PK;
					CustomsDataRegistry.Instance.AutoBillingEmailNotificationGroup.SetValue(Env.CurrentCompany.PK, Guid.Empty, Guid.Empty, groupNotification);
				}
				return postMasterGroup;
			}
		}
		GlbGroup postMasterGroup;

		OrgHeader Importer
		{
			get
			{
				if (importer == null)
				{
					importer = Factory.New<OrgHeader>();
					importer.OH_Code = "IMP";
					importer.MiscServ.OM_IMPartAttrib1Name = "VIN1";
					importer.MiscServ.OM_IMPartAttrib1Type = "NON";
					importer.CompanyData.OB_IMUsedBondedWhs = true;
					importer.OH_IsWarehouseClient = true;
				}
				return importer;
			}
		}
		OrgHeader importer;

		OrgHeader Warehouse
		{
			get
			{
				if (warehouse == null)
				{
					warehouse = Factory.New<OrgHeader>();
					warehouse.OH_Code = "W1";
					warehouse.MainAddress.LocalControlledPremisesID = "23423";
				}
				return warehouse;
			}
		}
		OrgHeader warehouse;

		AUOrgSupplierPart Part
		{
			get
			{
				if (part == null)
				{
					part = Factory.New<AUOrgSupplierPart>();
					part.OP_PartNum = "~~1";
					part.OP_StockKeepingUnit = "NO";
					part.RelatedOrganisations.AddOrganisationIfNotExist(Importer.PK, OrgPartRelation.RelationshipTypes.Owner);
					part.AddNewImportPivotWithClassification(Classification.PK);
				}
				return part;
			}
		}
		AUOrgSupplierPart part;

		Classification Classification
		{
			get
			{
				if (classification == null)
				{
					classification = Factory.New<Classification>();
					classification.CC_ClassificationType = Classification.ClassificationType.IMP;
					classification.CC_RN_NKCountryCode = GlbCompany.CurrentCompany.GC_RN_NKCountryCode;
					classification.CC_LookupCode = "~~1L";
					classification.CC_TariffNum = StatTariff.SC_TariffClassificationNumber + StatTariff.SC_StatisticalClassificationCode;
				}
				return classification;
			}
		}
		Classification classification;

		CMRStatisticalClassificationPeriodSnapshot StatTariff
		{
			get
			{
				if (statTariff == null)
				{
					statTariff = Factory.New<CMRStatisticalClassificationPeriodSnapshot>();
					statTariff.SC_TariffClassificationNumber = "00000000";
					statTariff.SC_StatisticalClassificationCode = "00";
					statTariff.SC_QuantityUnit = "KG";
					statTariff.SC_StartDate = new ZDateTime(2005, 1, 1);
				}
				return statTariff;
			}
		}
		CMRStatisticalClassificationPeriodSnapshot statTariff;
	}
}
