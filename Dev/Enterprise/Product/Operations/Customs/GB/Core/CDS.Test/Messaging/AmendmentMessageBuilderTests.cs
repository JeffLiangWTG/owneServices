using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.GB.MessageDefinitions.CDS;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.Business.WorldCustomsOrganisation.MessageSending;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Declaration.MultiLineAddInfos;
using Enterprise.Customs.GB.CDS.Messaging.MessageBuilders;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using static Enterprise.Customs.Business.UniversalReferenceConstants;
using static Enterprise.Customs.GB.CDS.Constants;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class AmendmentMessageBuilderTests : TestCaseWithFactory
	{
		public void TestBuild()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			var invoice = entryHeader.Declaration.Invoices[0];
			var invoiceLine = invoice.InvoiceLines[0];
			invoiceLine.JI_Tariff = "1234567890";
			invoiceLine.AdditionalProcedureCodes.RemoveAndDeleteAll();
			invoiceLine.JI_LinePrice = 200;

			var container3 = entryHeader.Declaration.CusContainers.AddNew();
			container3.CO_ContainerNumber = "CNT789";

			var container4 = entryHeader.Declaration.CusContainers.AddNew();
			container4.CO_ContainerNumber = "CNT999";

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedAmendment.xml"), amendmentMessage);
		}

		public void TestBuildWithAttributeChange()
		{
			using (GBCustomsDataRegistry.Instance.CDSStatisticalValueManualOverride.SetTemporaryValue(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty, true))
			{
				var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
				Factory.Save();
				MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

				entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

				var invoice = entryHeader.Declaration.Invoices[0];
				var invoiceLine = invoice.InvoiceLines[0];
				invoiceLine.JI_LinePrice = 200;

				entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
				entryHeader.CH_CustomsMessageRemarks = "Amending";
				entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
				var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

				var messageBuilderManager = new MessageBuilderManager();
				var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
				var amendmentMessage = messageBuilder.Build();

				AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedAmendmentWithAttributeChange.xml"), amendmentMessage);
			}
		}

		public void TestBuildProducesCorrectOrderForAdditionalInformations()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "test", dataGrouping: "CDS");
			var code1 = helper.CreateCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);

			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);

			Factory.Save();

			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			var addInfo = Factory.New<AdditionalInfo>();
			addInfo.CSI_Code = "ADD1";
			addInfo.CSI_Description = "DESC";
			addInfo.CSI_ReferenceNumber = "123";
			entryHeader.Declaration.AdditionalInfos.Add(addInfo);

			entryHeader.Declaration.JE_GoodsLocation = "12345";

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";

			Factory.Save();
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(@"<AdditionalInformation>
      <StatementCode>ADD1</StatementCode>
      <StatementDescription>DESC</StatementDescription>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>Amending</StatementDescription>
      <StatementTypeCode>AES</StatementTypeCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>06A</DocumentSectionCode>
      </Pointer>
    </AdditionalInformation>", amendmentMessage);
		}

		public void TestBuildWhenAddingAdditionalInformationAtDeclarationLevel()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "test", dataGrouping: "CDS");
			var code1 = helper.CreateCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);

			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			Factory.Save();
			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			var addInfo = Factory.New<AdditionalInfo>();
			addInfo.CSI_Code = "ADD1";
			addInfo.CSI_Description = "DESC";
			addInfo.CSI_ReferenceNumber = "123";
			entryHeader.Declaration.AdditionalInfos.Add(addInfo);

			entryHeader.Declaration.JE_GoodsLocation = "12345";

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";

			Factory.Save();
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedAmendmentWithAddedAdditionalInfo.xml"), amendmentMessage);
		}

		public void TestBuildWithDeletionOfBorderTransportMeansIdentificationTypeCode()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entryHeader.Declaration.JE_MessageType = "EXP";
			entryHeader.Declaration.ZG_Box18TransportID = "A1234";
			entryHeader.Declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.Road;
			entryHeader.Declaration.CustomsEntryInstructions[0].CEI_Style = ExportDeclarationTypeList.Codes.DeclarationForExport;  // B1
			entryHeader.Declaration.JE_DeclarationType = ExportDeclarationTypeList.Codes.DeclarationForExport;  // B1
			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			entryHeader.Declaration.JE_TransportMode = Enterprise.Customs.Business.TransportTypeList.Codes.FixedTransportInstallations;

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedAmendmentWithDeletedBorderTransportMeansIdentificationTypeCode.xml"), amendmentMessage);
		}

		[StressTest]
		public void TestBuildWithChangeAndAdditonToDutyTaxFee()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var entryLine = entryHeader.AllEntryLines[0];
			entryLine.RandomLine.JI_ConcessionOrder = "";
			entryLine.RandomLine.JI_PrimaryPreference = "100";
			entryLine.Fees.RemoveAndDeleteAll();
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeType = "A50";
			fee1.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			fee1.CF_BaseValue = 17500m;

			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			entryLine.RandomLine.JI_ConcessionOrder = "123456";
			entryLine.RandomLine.JI_PrimaryPreference = "120";

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedAmendmentWithChangeAndAdditionInDutyTaxFee.xml"), amendmentMessage);
		}

		[StressTest]
		public void TestBuildWithChangeOfPaymentMethodAndAddedDutyTaxFee()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var entryLine = entryHeader.AllEntryLines[0];
			entryLine.RandomLine.JI_ConcessionOrder = "";
			entryLine.RandomLine.JI_PrimaryPreference = "100";
			entryLine.Fees.RemoveAndDeleteAll();
			entryLine.RandomLine.ZG_MethodOfPayment = "";
			var fee1 = entryLine.Fees.AddNew();
			fee1.CF_ChargeType = "A50";
			fee1.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			fee1.CF_BaseValue = 17500m;

			var fee2 = entryLine.Fees.AddNew();
			fee2.CF_ChargeType = "A50";
			fee2.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			fee2.CF_BaseValue = 666m;

			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			entryLine.RandomLine.ZG_MethodOfPayment = "P";
			var fee3 = entryLine.Fees.AddNew();
			fee3.CF_ChargeType = "A50";
			fee3.CF_MethodOfCalculation = MethodOfCalculation.Percentage;
			fee3.CF_BaseValue = 100m;

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedAmendmentWithChangeOfPaymentMethodAndAddedDutyTaxFee.xml"), amendmentMessage);
		}

		public void TestBuildWithChangeAlsoCopiesTheCorrectAttributes()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			entryHeader.LRN = entryHeader.DeclarationUCR;
			MessageSendingTestHelper.CreateMessageFromSampleXML(entryHeader, EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NewMessageForAttributeTest.xml"), typeof(CDSNewDeclarationEDIMessage), CDSEDIMessageTypeList.Codes.NewDeclaration);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageSendingObject = decWrapper.SendingObjectsCollection.OfType<JobDeclarationMessageSendingObject>().FirstOrDefault();
			if (messageSendingObject != null)
			{
				messageSendingObject.MessageType = CDSEDIMessageTypeList.Codes.ArrivalNotification;
				messageSendingObject.VOCReason = "Arrival Notification";
				messageSendingObject.ChangeAcknowledgementIndicator = AmendmentCancellationReasonCode.Codes.A_Nil;
			}

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader, new AmendmentMessageHelperForTest(new PointerParser())), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(@"<ItemChargeAmount currencyID=""GBP"">65.40</ItemChargeAmount>", amendmentMessage);
		}

		public void TestBuildWhenAddingTwoClassifications()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "test", dataGrouping: "CDS");
			var code1 = helper.CreateCusCodeList("CDS", Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.AdditionalInformation, "ADD1", ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);
			helper.CreateCusCodeListAttribute(code1.PK, Universal.RefCusCodeListAttributeTypes.Codes.Level, EU.Business.UniversalReferenceConstants.RefCusCodeListAttributes.Values.Header);

			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var entryLine = entryHeader.AllEntryLines[0];
			entryLine.Fees.RemoveAndDeleteAll();
			var invoiceLine = entryHeader.Declaration.InvoiceLines[0];
			invoiceLine.JI_ZZF_NKTaxType = "";
			invoiceLine.JI_SupplementaryCode1 = "";
			invoiceLine.JI_SupplementaryCode2 = "";
			Factory.Save();

			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			invoiceLine.JI_ZZF_NKTaxType = "673"; // Zero Rated
			invoiceLine.JI_SupplementaryCode1 = "VATZ";

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";

			Factory.Save();
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedAmendmentWithTwoAddedClassifications.xml"), amendmentMessage);
		}

		public void TestBuildWhenChangingTwoClassifications()
		{
			var initialXML = EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NamMessageWhenModyingTwoClassificationChildNodes.xml");
			var comparisonXML = EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NewMessageWhenModyingTwoClassificationChildNodes.xml");

			var tester = new AmendmentMessageTester(initialXML, comparisonXML);
			var amendmentXML = tester.RunTest();

			AssertXMLEquals("XML output is incorrect", @"<Declaration>
  <GoodsShipment>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <Commodity>
        <Classification>
          <ID>61071100</ID>
        </Classification>
        <Classification>
          <ID>00</ID>
        </Classification>
      </Commodity>
    </GovernmentAgencyGoodsItem>
  </GoodsShipment>
</Declaration>", amendmentXML);
		}

		public void TestBuildWhenChangingSecondOfTwoPreviousDocuments()
		{
			var initialXML = EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NamMessageWhenModyingSecondOfTwoPreviousDocumentChildNodes.xml");
			var comparisonXML = EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NewMessageWhenModyingSecondOfTwoPreviousDocumentChildNodes.xml");

			var tester = new AmendmentMessageTester(initialXML, comparisonXML);
			var amendmentXML = tester.RunTest();

			AssertXMLEquals("XML output is incorrect", @"<Declaration>
  <GoodsShipment>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <PreviousDocument />
      <PreviousDocument>
        <ID>BBGBBB1234</ID>
      </PreviousDocument>
    </GovernmentAgencyGoodsItem>
  </GoodsShipment>
</Declaration>", amendmentXML);
		}

		public void TestBuildWhenAddingAdditionalDocument()
		{
			var initialXML = EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NamMessageWhenAddingAdditionalDocument.xml");
			var comparisonXML = EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NewMessageWhenAddingAdditionalDocument.xml");

			var tester = new AmendmentMessageTester(initialXML, comparisonXML);
			var amendmentXML = tester.RunTest();

			AssertXMLEquals("XML output is incorrect", @"<Declaration>
  <GoodsShipment>
    <GovernmentAgencyGoodsItem>
      <SequenceNumeric>1</SequenceNumeric>
      <AdditionalDocument />
      <AdditionalDocument />
      <AdditionalDocument>
        <CategoryCode>N</CategoryCode>
        <ID>1</ID>
        <TypeCode>A</TypeCode>
        <LPCOExemptionCode>AC</LPCOExemptionCode>
      </AdditionalDocument>
    </GovernmentAgencyGoodsItem>
  </GoodsShipment>
</Declaration>", amendmentXML);
		}

		public void TestBuildWhenAlteringDate()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);

			var gbDataGrouping = helper.CreateNewOrGetExistingDataGrouping("GB", "United Kingdom");
			var cdsDataGrouping = helper.CreateNewOrGetExistingDataGrouping("CDS", "GB Customs Declaration Services (CDS)", parent: gbDataGrouping);

			var importCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfImportDirection;
			var exportCodeType = Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.SupportingDocumentOfExportDirection;
			var attributeNameValuePairs = new Dictionary<string, string[]>();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM", "HEADER" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(gbDataGrouping.ZZZ_DataGrouping,
				new string[] { importCodeType, exportCodeType }, "9999", "9999 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue, ZDateTime.MaxSmallDateTimeValue);

			attributeNameValuePairs.Clear();
			attributeNameValuePairs.Add("Level", new string[] { "ITEM" });
			attributeNameValuePairs.Add("ACTAV", new string[] { "AC" });
			helper.CreateCusCodeListsForMultipleTypesWithAttributes(cdsDataGrouping.ZZZ_DataGrouping,
				new string[] { importCodeType, exportCodeType }, "9999", "9999 DES", attributeNameValuePairs, ZDateTime.MinSmallDateTimeValue.AddDays(1), ZDateTime.MaxSmallDateTimeValue);

			Factory.Save();

			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);

			var decSuppDoc = Factory.New<SupportingDocument>();
			decSuppDoc.CSI_Code = "9999";
			decSuppDoc.CSI_ReferenceNumber = "InvLineDocWithoutDate";
			entryHeader.Declaration.Invoices[0].InvoiceLines[0].SupportingDocuments.Add(decSuppDoc);
			Factory.Save();
			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			var decSuppDoctoUpdate = entryHeader.Declaration.Invoices[0].InvoiceLines[0].SupportingDocuments[1];
			decSuppDoctoUpdate.CSI_DateOfIssue = new ZDateTime(2024, 6, 15);

			Factory.Save();
			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);
			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;
			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedAmendmentWhenAddingDateToAdditionalDocument.xml"), amendmentMessage);
		}

		public void TestBuildWhenChangingAuthorisations()
		{
			var entryHeader = RequestMessageTests.CreateSampleEntryHeaderForH1(Factory);
			var entryLine = entryHeader.AllEntryLines[0];
			entryLine.Fees.RemoveAndDeleteAll();
			var invoiceLine = entryHeader.Declaration.InvoiceLines[0];
			Factory.Save();

			MessageSendingTestHelper.CreateOriginalNewMessage(entryHeader);

			entryHeader.MovementReferenceNumberSetter("MRN123", ZDateTime.BrettsBirthday);

			var auth = entryHeader.EntryInstruction.CusAuthorizationUsages.AddNew();
			auth.AGC_Code = "DPU";
			auth.AGC_Number = "GB506586239000";

			entryHeader.ZG_AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.A_CommodityCode;
			entryHeader.CH_CustomsMessageRemarks = "Amending";

			var guarantee = entryHeader.Declaration.Guarantees.AddNew();
			guarantee.EntryInstructionID = entryHeader.EntryInstruction.PK;
			guarantee.PW_BondNumber = "12345";
			guarantee.PW_BondType = GuaranteeTypeList.Codes.Guarantee;

			Factory.Save();
			entryHeader.CH_EntryStatus = GBCommonConstants.EntryStatusCodes.DeclarationAccepted;

			var decWrapper = new JobDeclarationMessageSendingObjectParent(entryHeader.Declaration);

			var messageBuilderManager = new MessageBuilderManager();
			var messageBuilder = messageBuilderManager.NewMessageBuilder(MessageSendingTestHelper.GetSendingObjectFromEntry(decWrapper.SendingObjectsCollection, entryHeader), new ErrorCollector(), new Customs.Business.CusdecMessageFunction.Amended());
			var amendmentMessage = messageBuilder.Build();

			AssertXMLContains(EmbeddedResource.GetExpectedMessageXml(@"Messaging.ExpectedAmendmentWhenAddingMultipleElementsToDeclarationLevel.xml"), amendmentMessage);
		}

		public void TestBuildWhenAddingAuthorisation()
		{
			var initialXML = EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NamMessageWhenAddingAuthorisation.xml");
			var comparisonXML = EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NewMessageWhenAddingAuthorisation.xml");

			var tester = new AmendmentMessageTester(initialXML, comparisonXML);
			var amendmentXML = tester.RunTest();

			AssertXMLEquals("XML output is incorrect", @"<Declaration>
  <AuthorisationHolder />
  <AuthorisationHolder />
  <AuthorisationHolder>
    <ID>GB896458895015</ID>
    <CategoryCode>ATR</CategoryCode>
  </AuthorisationHolder>
</Declaration>", amendmentXML);
		}
	}

	public class AmendmentMessageHelperForTest : EU.Business.WorldCustomsOrganisation.MessageBuilders.Testing.AmendmentMessageHelperForTest
	{
		public AmendmentMessageHelperForTest(PointerParser pointerParser) : base(pointerParser)
		{
		}

		protected override EDIMessage CreateNewMessageForComparison(WCOJobDeclarationMessageSendingObject objectToSend)
		{
			EDIMessage result = null;
			var entry = ((JobDeclarationMessageSendingObject)objectToSend).Header;

			if (!entry.IsNull)
			{
				var newMessageXML = EmbeddedResource.GetExpectedMessageXml(@"Messaging.TestFiles.NamMessageForAttributeTest.xml");
				newMessageXML = newMessageXML.Replace(GB.Business.Declaration.CusEntryHeader.BGMReferencePlaceHolderXmlFriendly, entry.CH_BGMReference);
				newMessageXML = newMessageXML.Replace(GB.Business.Declaration.CusEntryHeader.UCRReferencePlaceHolderXmlFriendly, entry.DeclarationUCR);
				newMessageXML = newMessageXML.Replace(GB.Business.Declaration.CusEntryHeader.LRNReferencePlaceHolderXmlFriendly, entry.LRN);

				var newMessage = entry.Messages.AddNew(typeof(CDSAmendmentComparisonEDIMessage));
				newMessage.EM_ApplicationCode = entry.GetApplicationCodeForMessage();
				newMessage.EM_MessageText = newMessageXML;
				newMessage.EM_ApplicationReference = EDIMessageApplicationReferencesForAmendment.Current;
				result = newMessage;
			}

			return result;
		}
	}
}
