using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework.Testing;
using CargoWise.IO;
using CargoWise.Types;
using Enterprise.Customs.Business.Testing;
using Enterprise.Customs.Universal;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture;
using NUnit.Framework;

namespace Enterprise.Customs.AU.Declaration.Business.Testing
{
	sealed class UniversalShipmentMessageBuilderTest : TestCaseWithFactory
	{
		public void TestEventReference()
		{
			void AssertEventReference(string messageType, string messageTypeEventReference, bool isSendToEU)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = CMRUnderbondRequestCodes.Codes.Quarantine;

				var invoice = declaration.Invoices.AddNew();

				var header = invoice.QuarantineExDocHeader;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;

				if (isSendToEU)
				{
					declaration.JE_RL_NKPortOfArrival = "ESBCN";
				}

				var messageBuilder = new UniversalShipmentMessageBuilder(header, messageType, new NotificationBuffer());
				messageBuilder.GenerateMessage();

				var message = header.Messages[0];
				if (isSendToEU)
				{
					AssertContains($"Should contains the expected event reference - {messageTypeEventReference} when message type is {messageType}.", $"<EventReference>|LOC=EU|MST={messageTypeEventReference}</EventReference>", message.EM_MessageText);
				}
				else
				{
					AssertContains($"Should contains the expected event reference - {messageTypeEventReference} when message type is {messageType}.", $"<EventReference>|MST={messageTypeEventReference}</EventReference>", message.EM_MessageText);
				}
			}

			ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.EXDOCS_Errata51, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true);
			AssertEventReference(NEXDOCMessageType.Codes.Order, "ORDER", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.Lodge, "LODGE", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.Withdrawal, "WITHDRAW", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.Amend, "AMEND", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.ManualAmend, "AMEND", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.TransferEDN, "TRFEDN", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.CancelEDN, "CANEDN", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.Cancellation, "CANREX", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.ReissueCertificate, "REISSUE", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.ReplacementCertificate, "REPLACE", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.PreviewCertificate, "PREVIEW", isSendToEU: false);
			AssertEventReference(NEXDOCMessageType.Codes.ReadREX, "READREX", isSendToEU: false);

			AssertEventReference(NEXDOCMessageType.Codes.Order, "ORDER", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.Lodge, "LODGE", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.Withdrawal, "WITHDRAW", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.Amend, "AMEND", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.ManualAmend, "AMEND", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.TransferEDN, "TRFEDN", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.CancelEDN, "CANEDN", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.Cancellation, "CANREX", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.ReissueCertificate, "REISSUE", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.ReplacementCertificate, "REPLACE", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.PreviewCertificate, "PREVIEW", isSendToEU: true);
			AssertEventReference(NEXDOCMessageType.Codes.ReadREX, "READREX", isSendToEU: true);
		}

		public void TestMessageSubType()
		{
			EDIMessage CreateNEXDOCMessage(string messageType)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = CMRUnderbondRequestCodes.Codes.Quarantine;

				var invoice = declaration.Invoices.AddNew();

				var header = invoice.QuarantineExDocHeader;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;

				var messageBuilder = new UniversalShipmentMessageBuilder(header, messageType, new NotificationBuffer());
				messageBuilder.GenerateMessage();
				return header.Messages[0];
			}

			var orderMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.Order);
			AssertEquals("EM_MessageSubType is ROR", NEXDOCMessageType.Codes.Order, orderMessage.EM_MessageSubType);

			var lodgeMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.Lodge);
			AssertEquals("EM_MessageSubType is RLG", NEXDOCMessageType.Codes.Lodge, lodgeMessage.EM_MessageSubType);

			var amendMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.Amend);
			AssertEquals("EM_MessageSubType is RAM", NEXDOCMessageType.Codes.Amend, amendMessage.EM_MessageSubType);

			var manualAmendMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.ManualAmend);
			AssertEquals("EM_MessageSubType is RMA", NEXDOCMessageType.Codes.ManualAmend, manualAmendMessage.EM_MessageSubType);

			var withdrawalMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.Withdrawal);
			AssertEquals("EM_MessageSubType is RWD", NEXDOCMessageType.Codes.Withdrawal, withdrawalMessage.EM_MessageSubType);

			var cancellationMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.Cancellation);
			AssertEquals("EM_MessageSubType is RCN", NEXDOCMessageType.Codes.Cancellation, cancellationMessage.EM_MessageSubType);

			var rexForwardMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.REXForward);
			AssertEquals("EM_MessageSubType is RRF", NEXDOCMessageType.Codes.REXForward, rexForwardMessage.EM_MessageSubType);

			var rexTransferMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.REXTransfer);
			AssertEquals("EM_MessageSubType is RRT", NEXDOCMessageType.Codes.REXTransfer, rexTransferMessage.EM_MessageSubType);

			var replacementMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.ReplacementCertificate);
			AssertEquals("EM_MessageSubType is RRP", NEXDOCMessageType.Codes.ReplacementCertificate, replacementMessage.EM_MessageSubType);

			var previewCertificateMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.PreviewCertificate);
			AssertEquals("EM_MessageSubType is RPR", NEXDOCMessageType.Codes.PreviewCertificate, previewCertificateMessage.EM_MessageSubType);

			var transferEDNMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.TransferEDN);
			AssertEquals("EM_MessageSubType is RET", NEXDOCMessageType.Codes.TransferEDN, transferEDNMessage.EM_MessageSubType);

			var cancelEDNMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.CancelEDN);
			AssertEquals("EM_MessageSubType is REC", NEXDOCMessageType.Codes.CancelEDN, cancelEDNMessage.EM_MessageSubType);

			var readREXMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.ReadREX);
			AssertEquals("EM_MessageSubType is RRX", NEXDOCMessageType.Codes.ReadREX, readREXMessage.EM_MessageSubType);

			var reissueCertMessage = CreateNEXDOCMessage(NEXDOCMessageType.Codes.ReissueCertificate);
			AssertEquals("EM_MessageSubType is REI", NEXDOCMessageType.Codes.ReissueCertificate, reissueCertMessage.EM_MessageSubType);
		}

		[TestDate(2020, 06, 15)]
		public void TestNoteWhenMessageTypeIsTransferEdn()
		{
			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CMRUnderbondRequestCodes.Codes.Quarantine;

			var invoice = declaration.Invoices.AddNew();

			var header = invoice.QuarantineExDocHeader;
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			header.QH_RequestForPermitNumber = "TST001";
			header.QH_LastAmendDateTime = new ZDateTimeOffset(ZDateTime.Now.AddDays(1));

			quarantineHeader.Declaration.RelatedDeclarationForTransferEDN = declaration;

			var messageBuilder = new UniversalShipmentMessageBuilder(quarantineHeader, NEXDOCMessageType.Codes.TransferEDN, new NotificationBuffer());
			messageBuilder.GenerateMessage();

			var message = quarantineHeader.Messages[0];
			var expectedNoteData =
@"<NoteCollection>
      <Note>
        <Description>EXDOC trf REX</Description>
        <NoteText>TST001</NoteText>
      </Note>
      <Note>
        <Description>EXDOC trf LastAmendTime</Description>
        <NoteText>2020-06-16T00:00:00.000+00:00</NoteText>
      </Note>
    </NoteCollection>";

			AssertContains("Should build the REX Number and LastAmendDateTime in Note - EXDOC Transfer REX And LastAmendTime.", expectedNoteData, message.EM_MessageText);
		}

		public void TestMessageSavedSucessfully()
		{
			var sucess = messageBuilder.GenerateMessage();
			Assert("Message is generated successfully.", sucess);

			AssertEquals(1, quarantineHeader.Messages.Count);
			Assert("Message is saved sucessfully.", quarantineHeader.Messages[0].IsInDatabase);
			Assert("Interchange is saved sucessfully.", quarantineHeader.Messages[0].Interchange.IsInDatabase);
		}

		public void TestGenerateMessage_UsingAnotherFactoryForIndependentWork()
		{
			var msgBuilder = new UniversalShipmentMessageBuilderForTestingObjectDisposedException(quarantineHeader, NEXDOCMessageType.Codes.Lodge, new NotificationBuffer());
			try
			{
				msgBuilder.GenerateMessage();
			}
			catch (Exception)
			{
				AssertNoExceptionThrown(() => quarantineHeader.Factory.Save());
			}
		}

		[TestDate(2018, 2, 8)]
		public void TestGenerateMessage()
		{
			var sucess = messageBuilder.GenerateMessage();
			Assert("Message is generated successfully.", sucess);
			AssertEquals(1, quarantineHeader.Messages.Count);

			var message = quarantineHeader.Messages[0];
			var interchange = message.Interchange;
			AssertMultilineASCIIEquals(string.Format(new EmbeddedResourceRetriever().GetString(GetEmbeddedResourcePath("TestRFPUniversalShipment.txt")), interchange.EI_SessionGUID, interchange.EI_InterchangeNum, message.EM_MessageNum), quarantineHeader.Messages[0].EM_MessageText);
		}

		[TestDate(2018, 2, 8)]
		public void TestGenerateMessageFromShipment()
		{
			var consol = Factory.New<ForwardingConsol>();
			consol.JK_UniqueConsignRef = "C00003434";
			consol.JK_TransportMode = Core.Constants.TransportModes.Sea;
			consol.JK_RL_NKLoadPort = "AUSYD";
			consol.JK_RL_NKDischargePort = "USLAX";
			var container = consol.Containers.AddNew();
			container.JC_ContainerNum = "C1";
			var shipment = consol.Shipments.AddNew();
			shipment.JS_UniqueConsignRef = "S00006565";
			shipment.JS_HouseBill = "Shipment1";
			shipment.JS_PackingMode = Core.Constants.ContainerModes.FCL;
			shipment.JS_F3_NKPackType = "123";
			var packLine1 = shipment.OuterPackLines.AddNew();
			packLine1.JL_PackageCount = 12;
			packLine1.JL_F3_NKPackType = "AA";
			packLine1.JL_JC = container.PK;
			var declaration = helper.Declaration;
			declaration.JE_JS = shipment.PK;

			TestHelper.MakeConsolRelevantToDeclaration(consol, declaration);
			declaration.ShipmentSynchroniser.SetEnabled(enabled: false, enableDetection: false);
			declaration.ShipmentSynchroniser.Synchronise(true);
			AssertEquals(1, declaration.CusContainers.Count);

			var pivot = declaration.CusContainers[0].InvoiceLinePivotCollection.AddNew();
			pivot.C2_JI = helper.Line1.PK;
			pivot.C2_SplitValue = 3.45m;
			pivot.C2_GrossWeight = 2.34m;
			pivot.C2_NetWeight = 1.23m;
			pivot.C2_PackQty = 12;

			var success = messageBuilder.GenerateMessage();
			Assert("Message is generated successfully.", success);
			AssertEquals(1, quarantineHeader.Messages.Count);

			AssertContains(@"<PackingLineCollection Content=""Complete"">
      <PackingLine>
        <ContainerNumber>C1</ContainerNumber>
        <PackedItemCollection>
          <PackedItem>
            <CommercialInvoiceLineLink>1</CommercialInvoiceLineLink>
            <GoodsValue>3.45</GoodsValue>
            <GrossWeight>2.34</GrossWeight>
            <GrossWeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </GrossWeightUnit>
            <NetWeight>1.23</NetWeight>
            <NetWeightUnit>
              <Code>KG</Code>
              <Description>Kilogram</Description>
            </NetWeightUnit>
            <PackedQuantity>12</PackedQuantity>
          </PackedItem>
        </PackedItemCollection>
      </PackingLine>
    </PackingLineCollection>", quarantineHeader.Messages[0].EM_MessageText);

			AssertMultilineASCIIEquals(string.Format(new EmbeddedResourceRetriever().GetString(GetEmbeddedResourcePath("TestRFPUniversalShipmentFromShipment.txt")), quarantineHeader.Messages[0].Interchange.EI_SessionGUID, quarantineHeader.Messages[0].Interchange.EI_InterchangeNum, quarantineHeader.Messages[0].EM_MessageNum), quarantineHeader.Messages[0].EM_MessageText);
		}

		[TestDate(2019, 5, 28)]
		public void TestRecipientID()
		{
			var sucess = messageBuilder.GenerateMessage();
			Assert("Message is generated successfully.", sucess);
			AssertEquals(1, quarantineHeader.Messages.Count);
			var message = quarantineHeader.Messages[0];
			AssertEquals("NEXDOCS", message.Interchange.EI_To);
			using (AUCustomsDataRegistry.Instance.NEXDOCSTestingSystem.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				sucess = messageBuilder.GenerateMessage();
				Assert("Message is generated successfully.", sucess);
				AssertEquals(2, quarantineHeader.Messages.Count);
				AssertEquals("NEXDOCSTest", quarantineHeader.Messages.Cast<EDIMessage>().FirstOrDefault(m => m.PK != message.PK).Interchange.EI_To);
			}
		}

		[TestDate(2021, 9, 17)]
		public void TestNexdocReissueCertificate()
		{
			var quarantineExDocHeader = helper.Header1.QuarantineExDocHeader;
			var messageBuilder = new UniversalShipmentMessageBuilder(quarantineExDocHeader, NEXDOCMessageType.Codes.ReissueCertificate, new NotificationBuffer());

			messageBuilder.GenerateMessage();
			AssertEquals("Should create message with EM_MessageSubType REI.", NEXDOCMessageType.Codes.ReissueCertificate, quarantineExDocHeader.Messages[0].EM_MessageSubType);
			quarantineExDocHeader.Messages[0].EM_Status = "FAL";
			quarantineExDocHeader.Messages.RemoveAll();

			messageBuilder.GenerateMessage(true);
			AssertEquals("Should create message with EM_MessageSubType REI.", NEXDOCMessageType.Codes.ReissueCertificate, quarantineExDocHeader.Messages[0].EM_MessageSubType);
			AssertEquals("suspend true, should create message with EM_Status PND.", "PND", quarantineExDocHeader.Messages[0].EM_Status);
			AssertEquals("suspend true, should create interchange with EI_Status PND.", "PND", quarantineExDocHeader.Messages[0].Interchange.EI_Status);
		}

		public void TestOrgAddress()
		{
			var exporter = Factory.New<OrgHeader>();
			exporter.OH_FullName = "THE MAGIC SAND FOOD COMPANY";
			exporter.OH_RL_NKClosestPort = "CNSNZ";
			exporter.OH_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;
			var exporterAddress = exporter.MainAddress;
			exporterAddress.City = "Shenzhen";
			exporterAddress.StateCode = "44"; // Guangdong

			var declaration = Factory.New<JobDeclaration>();
			declaration.JE_MessageType = CMRUnderbondRequestCodes.Codes.Quarantine;
			declaration.JE_OH_Exporter = exporter.PK;

			var importerDocumentaryAddress = declaration.ImporterDocumentaryAddress;
			importerDocumentaryAddress.E2_AddressOverride = true;
			importerDocumentaryAddress.E2_CompanyName = "China Corp";
			importerDocumentaryAddress.E2_Address1 = "Dropoff Address";
			importerDocumentaryAddress.E2_City = "Yangzhen";
			importerDocumentaryAddress.E2_RN_NKCountryCode = "CN"; // China
			importerDocumentaryAddress.E2_State = "11"; // Beijing
			importerDocumentaryAddress.E2_ScreeningStatus = ScreeningStatusesList.Codes.PermanentClear;

			var invoice = declaration.Invoices.AddNew();
			var header = invoice.QuarantineExDocHeader;
			header.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;

			var messageBuilder = new UniversalShipmentMessageBuilder(header, EXDOCMessageTypeCodes.Codes.LDG, new NotificationBuffer());
			messageBuilder.GenerateMessage();
			var message = header.Messages[0];

			var expectedOrgAddressData =
@"
      <OrganizationAddress>
        <AddressType>Exporter</AddressType>
        <Address1></Address1>
        <Address2></Address2>
        <AddressOverride>false</AddressOverride>
        <AddressShortCode></AddressShortCode>
        <City>Shenzhen</City>
        <CompanyName>THE MAGIC SAND FOOD COMPANY</CompanyName>
        <Country>
          <Code>CN</Code>
          <Name>China</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <OrganizationCategory>BUS</OrganizationCategory>
        <OrganizationCode>MAGSANSNZ</OrganizationCode>
        <Phone></Phone>
        <Port>
          <Code>CNSNZ</Code>
          <Name>Shenzhen</Name>
        </Port>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLP</Code>
          <Description>Permanent Clear</Description>
        </ScreeningStatus>
        <State Description=""Guangdong"">44</State>
      </OrganizationAddress>";

			AssertContains("Org Address <State> has Name in Description attribute.", expectedOrgAddressData, message.EM_MessageText);

			var expectedJobDocAddressData =
@"
      <OrganizationAddress>
        <AddressType>ImporterDocumentaryAddress</AddressType>
        <AdditionalAddressInformation></AdditionalAddressInformation>
        <Address1>Dropoff Address</Address1>
        <Address2></Address2>
        <AddressOverride>true</AddressOverride>
        <City>Yangzhen</City>
        <CompanyName>China Corp</CompanyName>
        <Contact></Contact>
        <Country>
          <Code>CN</Code>
          <Name>China</Name>
        </Country>
        <Email></Email>
        <Fax></Fax>
        <GovRegNum></GovRegNum>
        <GovRegNumType>
          <Code>DEF</Code>
          <Description>Default</Description>
        </GovRegNumType>
        <Mobile></Mobile>
        <Phone></Phone>
        <Postcode></Postcode>
        <ScreeningStatus>
          <Code>CLP</Code>
          <Description>Permanent Clear</Description>
        </ScreeningStatus>
        <State Description=""Beijing"">11</State>
      </OrganizationAddress>";

			AssertContains("Org Address <State> has Name in Description attribute.", expectedJobDocAddressData, message.EM_MessageText);
		}

		public void TestMessageStatus()
		{
			void AssertMessageStatus(string messageType, string expectedMessageStatus)
			{
				var declaration = Factory.New<JobDeclaration>();
				declaration.JE_MessageType = CMRUnderbondRequestCodes.Codes.Quarantine;

				var invoice = declaration.Invoices.AddNew();

				var header = invoice.QuarantineExDocHeader;
				header.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;

				var messageBuilder = new UniversalShipmentMessageBuilder(header, messageType, new NotificationBuffer());
				messageBuilder.GenerateMessage();

				string assertMessage = expectedMessageStatus.IsNullOrEmpty() ? $"Declaratin message status should be empty when message type is {messageType}." : $"Declaratin message status should be {expectedMessageStatus} when message type is {messageType}.";
				AssertEquals(assertMessage, expectedMessageStatus, declaration.JE_MessageStatus);
			}

			CombineAssertions(() =>
			{
				AssertMessageStatus(NEXDOCMessageType.Codes.Order, RFPMessage.Status.AwaitingResponse);
				AssertMessageStatus(NEXDOCMessageType.Codes.Lodge, RFPMessage.Status.AwaitingResponse);
				AssertMessageStatus(NEXDOCMessageType.Codes.Withdrawal, RFPMessage.Status.AwaitingResponse);
				AssertMessageStatus(NEXDOCMessageType.Codes.Amend, RFPMessage.Status.AwaitingResponse);
				AssertMessageStatus(NEXDOCMessageType.Codes.TransferEDN, RFPMessage.Status.AwaitingResponse);
				AssertMessageStatus(NEXDOCMessageType.Codes.CancelEDN, RFPMessage.Status.AwaitingResponse);
				AssertMessageStatus(NEXDOCMessageType.Codes.Cancellation, RFPMessage.Status.AwaitingResponse);
				AssertMessageStatus(NEXDOCMessageType.Codes.ReissueCertificate, RFPMessage.Status.AwaitingResponse);
				AssertMessageStatus(NEXDOCMessageType.Codes.ReplacementCertificate, RFPMessage.Status.AwaitingResponse);
				AssertMessageStatus(NEXDOCMessageType.Codes.PreviewCertificate, "");
				AssertMessageStatus(NEXDOCMessageType.Codes.ReadREX, "");
			});
		}

		protected override void SetUp()
		{
			var date1 = new ZDateTime(2019, 3, 1);
			var date2 = new ZDateTime(2019, 3, 31);

			var universalhelper = new UniversalReferenceTestDataHelper(Factory);
			const string natyp = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.NEXDOCSAttachmentType;
			universalhelper.CreateNewOrGetExistingCusCodeType(natyp, "NATYP Desc.");
			universalhelper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY1", "Doc Type DESC 1", date1, date2);
			universalhelper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY2", "Doc Type DESC 2", date1, date2);
			universalhelper.CreateNewOrGetExistingCusCodeList("AU", natyp, "TY3", "Doc Type DESC 3", date1, date2);
			Factory.Save();

			base.SetUp();

			nexdocOtherGoodsFunc = ZZCustomsFunctionalityEffectiveDate.TemporarilySetFunctionality(Universal.Constants.FunctionalityTypes.NEXDOC_OTH, Core.Constants.CountryCodes.Australia, ZDateTime.Today, value: true);
			helper = new ZTestHelper(Factory);
			helper.PopulateSimpleQuarantineDeclaration();
			var declaration = helper.Declaration;
			declaration.JE_PaymentMethod = JobDeclaration.PaymentMethods.Default;
			var eDoc1 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice1.pdf", "F1", description: "F1 Desc");
			var eDoc2 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice2.pdf", "F2", description: "F2 Desc");
			var eDoc3 = declaration.DocManagerInfo.AddFileOrDocument(new byte[1], "Invoice3.pdf", "F3", description: "F3 Desc");
			var invoice = helper.Header1;
			var pivot1Invoice = invoice.EDocPivotCollection.AddNew();
			pivot1Invoice.CSD_DocType = "TY1";
			pivot1Invoice.CSD_StorageDocReference = eDoc1.UniqueKey;
			pivot1Invoice.CSD_Description = "File 1";
			var pivot2Invoice = invoice.EDocPivotCollection.AddNew();
			pivot2Invoice.CSD_DocType = "TY2";
			pivot2Invoice.CSD_StorageDocReference = eDoc1.UniqueKey;
			pivot2Invoice.CSD_Description = "File 2";
			var invoiceLine = helper.Line1;
			var pivot1InvoiceLine = invoiceLine.EDocPivotCollection.AddNew();
			pivot1InvoiceLine.CSD_DocType = "TY3";
			pivot1InvoiceLine.CSD_StorageDocReference = eDoc2.UniqueKey;
			pivot1InvoiceLine.CSD_Description = "File 3";
			var pivot2InvoiceLine = invoiceLine.EDocPivotCollection.AddNew();
			pivot2InvoiceLine.CSD_DocType = "TY3";
			pivot2InvoiceLine.CSD_StorageDocReference = eDoc3.UniqueKey;
			pivot2InvoiceLine.CSD_Description = "File 4";

			quarantineHeader = helper.Header1.QuarantineExDocHeader;
			quarantineHeader.QH_ProduceType = EXDOCCommodityCodes.Codes.OtherGoods;
			quarantineHeader.QH_QuotaType = "XXX";
			quarantineHeader.QH_PrintLocation = EXDOCCodeOrganisation.Codes.Organisation;
			messageBuilder = new UniversalShipmentMessageBuilder(quarantineHeader, NEXDOCMessageType.Codes.Lodge, new NotificationBuffer());
		}

		protected override void TearDown()
		{
			base.TearDown();
			nexdocOtherGoodsFunc?.Dispose();
		}

		ZTestHelper helper;
		QuarantineExDocHeader quarantineHeader;
		UniversalShipmentMessageBuilder messageBuilder;
		IDisposable nexdocOtherGoodsFunc;

		string GetEmbeddedResourcePath(string fileName) => "Enterprise.Customs.AU.Declaration.Business.Testing.MessageBuilders.Export.EXDOC.TestFiles." + fileName;

		sealed class UniversalShipmentMessageBuilderForTestingObjectDisposedException : UniversalShipmentMessageBuilder
		{
			public UniversalShipmentMessageBuilderForTestingObjectDisposedException(QuarantineExDocHeader header, string messageType, CargoWise.ComponentModel.INotifications notifications)
				: base(header, messageType, notifications)
			{
			}

			protected override Messaging.Integration.IXmlEDIInterchange SetDeliveryRecipient(UniversalDataBuss.DataObjects.Universal.Shipment shipment, CargoWise.EntityFramework.BusinessObjectFactory factory)
			{
				base.SetDeliveryRecipient(shipment, factory);
				throw new Exception();
			}
		}
	}
}
