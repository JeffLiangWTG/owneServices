using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common;
using Enterprise.Customs.Common.EU;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business;
using Enterprise.Customs.EU.NCTS.Business.CodeDescriptionPairLists;
using Enterprise.Customs.EU.NCTS.Business.Testing;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.MasterFiles.Business;
using NUnit.Framework;
using NctsGuarantee = Enterprise.Customs.EU.NCTS.Business.NctsGuarantee;

namespace Enterprise.Customs.GB.Business.NCTS.Testing
{
	class NctsMessageSenderTest : TestCaseWithFactory
	{
		public void TestSendIE013()
		{
			TestSendDepartureMessage(GB_NCTS5DeparturePhaseList.Codes.Amendment, NCTS5DepartureCustomsStatusList.Codes.AmendmentRequested);
			AssertHtmlTableContainsRows(
				departureHeader.LinkedMessages[0].EM_MessageInterpretation,
				new Dictionary<string, string>
				{
					{ "Message Type", "CC013C" },
					{ "Customs Office (Departure)", "GB000011" },
					{ "Reduced Dataset", "0" },
					{ "Security", "0" },
					{ "Binding Itinerary", "0" },
					{ "Principle EORI", "GB0123456789000" },
					{ "Message Recipient", "NTA.GB" }
				});
		}

		[TestDate(2024, 11, 19)]
		public void TestSendIE013_ShouldSetValuationDate()
		{
			var sendingObject = new NctsHeaderMessageSendingObject(departureHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject) { MessageType = GB_NCTS5DeparturePhaseList.Codes.Amendment };
			var sender = new NctsMessageSender(sendingAction);

			AssertEquals("Precondition", ZDateTime.Empty, departureHeader.MovementHeader.BM_ValuationDate);
			sender.Send();

			AssertEquals(ZDateTime.Now, departureHeader.MovementHeader.BM_ValuationDate);
		}

		public void TestSendIE014()
		{
			var sendingObject = new NctsHeaderMessageSendingObject(departureHeader);
			sendingObject.Justification = "Because it's worth it";
			TestSendDepartureMessage(GB_NCTS5DeparturePhaseList.Codes.CancellationAmendment, providedSendingObject: sendingObject);
			var message = departureHeader.LinkedMessages[0];
			AssertContains("Justification in XML", "<justification>Because it's worth it</justification>", message.EM_MessageText);
			AssertContains(@"<tr><td>Message Type</td><td>CC014C</td></tr><tr><td>Customs Office (Departure)</td><td>GB000011</td></tr><tr><td>Principle EORI</td><td>GB0123456789000</td></tr>", message.EM_MessageInterpretation);
		}

		void SetupIE015Header()
		{
			var movementHeader = departureHeader.MovementHeader;
			var principalOrg = NCTSTestHelper.CreateOrgAddressForTest(Factory, "GB123456");
			principalOrg.Address2 = "XX";
			departureHeader.Principal.E2_Contact = "Bob";
			departureHeader.Principal.E2_Phone = "1234567890";
			departureHeader.Principal.E2_OA_Address = principalOrg.PK;
			_ = NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "GB000011", ZDateTime.Empty);
			_ = NCTSTestHelper.CreateCustomsOfficeForTest(movementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestination, "DE000011", ZDateTime.Empty);

			movementHeader.BM_PaperlessInbondNum = "2394539099200000000001";
			movementHeader.BM_InBondEntryType = NctsPhase5DeclarationTypeList.Codes.T1;
			movementHeader.BM_AdditionalDeclarationType = NctsTypeOfAdditionalDeclarationList.Codes.D;
			movementHeader.BM_RL_NKDestinationPort = Core.Constants.CountryCodes.Germany;
			movementHeader.BM_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.UnitedKingdom;
			movementHeader.BM_GrossWeight = 123.45678m;
			movementHeader.BM_PortOfPresentationCode = "GBDVR";
			movementHeader.BM_ForeignDestPortKCode = "DEFRA";
			movementHeader.BM_UniqueConsignmentReference = "HEADER REF";

			movementHeader.BM_CustomsOfficeAtBorder = "GB000011";
			movementHeader.BM_ConveyanceNumber = "CN000";
			movementHeader.BM_ActiveBorderIdentificationType = "40";
			movementHeader.BM_TOLCarrierID = "IATA0000";
			movementHeader.BM_RN_NKTOLCarrierNationality = Core.Constants.CountryCodes.UnitedKingdom;

			movementHeader.BM_InlandTransportMode = ModeOfTransportList.Codes._3_RoadTransport;
			movementHeader.BM_TransportAtDeparture = "12345678";
			movementHeader.BM_RN_NKTransportAtDepartureCountry = "AU";
			movementHeader.BM_TransportAtDepartureTrailer1RegNo = "AB12";
			movementHeader.BM_RN_NKTransportAtDepartureTrailer1Nationality = "NZ";
			movementHeader.BM_TransportAtDepartureTrailer2RegNo = "CD34";
			movementHeader.BM_RN_NKTransportAtDepartureTrailer2Nationality = "ZA";

			var guarantee = departureHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondType = "B";
			guarantee.PW_BondNumber = "00AA1234567890123";
			guarantee.PW_BondAmount = 123.45m;
			guarantee.PW_Password = "ABCD";

			var bill = departureHeader.Bills.AddNew();
			bill.B0_ReferenceID = "BILL REF";
			bill.B0_Weight = 123.45678m;
			var consignor = NCTSTestHelper.CreateOrgAddressForTest(Factory, "CO1", suffix: "", traderTin: string.Empty);
			consignor.OA_Address2 = "XX";
			bill.Consignor.E2_OA_Address = consignor.PK;
			var consignee = NCTSTestHelper.CreateOrgAddressForTest(Factory, "PE1", suffix: "", traderTin: string.Empty);
			consignee.OA_Address2 = "XX";
			bill.Consignee.E2_OA_Address = consignor.PK;

			var goodsItem = bill.GoodsItems.AddNew();
			goodsItem.BY_RN_NKCountryOfDestination = Core.Constants.CountryCodes.Germany;
			goodsItem.BY_RN_NKCountryOfDispatch = Core.Constants.CountryCodes.UnitedKingdom;
			goodsItem.BY_CommercialReferenceNumber = "ITEM REF";
			goodsItem.BY_Type = NctsPhase5DeclarationTypeList.Codes.T1;
			goodsItem.BY_Description = "Item Description";
			goodsItem.BY_CusC4Number = "123456789";
			goodsItem.BY_HarmonisedTariff = "1234567890";
			goodsItem.BY_GrossWeight = 123.45m;
			goodsItem.BY_NetWeight = 100.00m;

			var package = goodsItem.Packages.AddNew();
			package.B5_UnitType = "BX";
			package.B5_UnitCount = 123;
		}

		[TestDate(2023, 09, 13, 01, 02, 03, 456)]
		[TestUtcOffset(1, 0, 0)]
		public void TestSendIE015()
		{
			SetupIE015Header();
			TestSendDepartureMessage(GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration);
			const string expectedXml = @"<?xml version=""1.0"" encoding=""utf-8""?>
<q1:CC015C xmlns:q1=""http://ncts.dgtaxud.ec"">
  <messageSender>GB111</messageSender>
  <messageRecipient>NTA.GB</messageRecipient>
  <preparationDateAndTime>2023-09-13T01:02:03</preparationDateAndTime>
  <messageIdentification>&lt;&lt;SENDERS REFERENCE PLACE HOLDER&gt;&gt;</messageIdentification>
  <messageType>CC015C</messageType>
  <correlationIdentifier>&lt;&lt;SENDERS REFERENCE PLACE HOLDER&gt;&gt;</correlationIdentifier>
  <TransitOperation>
    <LRN>2394539099200000000001</LRN>
    <declarationType>T1</declarationType>
    <additionalDeclarationType>D</additionalDeclarationType>
    <security>0</security>
    <reducedDatasetIndicator>0</reducedDatasetIndicator>
    <communicationLanguageAtDeparture>en</communicationLanguageAtDeparture>
    <bindingItinerary>0</bindingItinerary>
  </TransitOperation>
  <CustomsOfficeOfDeparture>
    <referenceNumber>GB000011</referenceNumber>
  </CustomsOfficeOfDeparture>
  <CustomsOfficeOfDestinationDeclared>
    <referenceNumber>DE000011</referenceNumber>
  </CustomsOfficeOfDestinationDeclared>
  <HolderOfTheTransitProcedure>
    <identificationNumber>GB0123456789000</identificationNumber>
  </HolderOfTheTransitProcedure>
  <Guarantee>
    <sequenceNumber>1</sequenceNumber>
    <guaranteeType>B</guaranteeType>
    <GuaranteeReference>
      <sequenceNumber>1</sequenceNumber>
      <GRN>00AA1234567890123</GRN>
      <accessCode>ABCD</accessCode>
      <amountToBeCovered>123.45</amountToBeCovered>
      <currency>GBP</currency>
    </GuaranteeReference>
  </Guarantee>
  <Consignment>
    <countryOfDispatch>GB</countryOfDispatch>
    <countryOfDestination>DE</countryOfDestination>
    <containerIndicator>0</containerIndicator>
    <inlandModeOfTransport>3</inlandModeOfTransport>
    <grossMass>123.45678</grossMass>
    <referenceNumberUCR>HEADER REF</referenceNumberUCR>
    <DepartureTransportMeans>
      <sequenceNumber>1</sequenceNumber>
      <typeOfIdentification>30</typeOfIdentification>
      <identificationNumber>12345678</identificationNumber>
      <nationality>AU</nationality>
    </DepartureTransportMeans>
    <DepartureTransportMeans>
      <sequenceNumber>2</sequenceNumber>
      <typeOfIdentification>31</typeOfIdentification>
      <identificationNumber>AB12</identificationNumber>
      <nationality>NZ</nationality>
    </DepartureTransportMeans>
    <DepartureTransportMeans>
      <sequenceNumber>3</sequenceNumber>
      <typeOfIdentification>31</typeOfIdentification>
      <identificationNumber>CD34</identificationNumber>
      <nationality>ZA</nationality>
    </DepartureTransportMeans>
    <ActiveBorderTransportMeans>
      <sequenceNumber>1</sequenceNumber>
      <customsOfficeAtBorderReferenceNumber>GB000011</customsOfficeAtBorderReferenceNumber>
      <typeOfIdentification>40</typeOfIdentification>
      <identificationNumber>IATA0000</identificationNumber>
      <nationality>GB</nationality>
      <conveyanceReferenceNumber>CN000</conveyanceReferenceNumber>
    </ActiveBorderTransportMeans>
    <PlaceOfLoading>
      <UNLocode>GBDVR</UNLocode>
    </PlaceOfLoading>
    <PlaceOfUnloading>
      <UNLocode>DEFRA</UNLocode>
    </PlaceOfUnloading>
    <HouseConsignment>
      <sequenceNumber>1</sequenceNumber>
      <grossMass>123.45678</grossMass>
      <referenceNumberUCR>BILL REF</referenceNumberUCR>
      <Consignor>
        <name>Oscorp Industries</name>
        <Address>
          <streetAndNumber>Street and No XX</streetAndNumber>
          <postcode>MK16 XX</postcode>
          <city>Milton Keynes</city>
          <country>GB</country>
        </Address>
      </Consignor>
      <Consignee>
        <name>Oscorp Industries</name>
        <Address>
          <streetAndNumber>Street and No XX</streetAndNumber>
          <postcode>MK16 XX</postcode>
          <city>Milton Keynes</city>
          <country>GB</country>
        </Address>
      </Consignee>
      <ConsignmentItem>
        <goodsItemNumber>1</goodsItemNumber>
        <declarationGoodsItemNumber>1</declarationGoodsItemNumber>
        <declarationType>T1</declarationType>
        <countryOfDispatch>GB</countryOfDispatch>
        <countryOfDestination>DE</countryOfDestination>
        <referenceNumberUCR>ITEM REF</referenceNumberUCR>
        <Commodity>
          <descriptionOfGoods>Item Description</descriptionOfGoods>
          <cusCode>123456789</cusCode>
          <CommodityCode>
            <harmonizedSystemSubHeadingCode>123456</harmonizedSystemSubHeadingCode>
          </CommodityCode>
          <GoodsMeasure>
            <grossMass>123.45</grossMass>
            <netMass>100</netMass>
          </GoodsMeasure>
        </Commodity>
        <Packaging>
          <sequenceNumber>1</sequenceNumber>
          <typeOfPackages>BX</typeOfPackages>
          <numberOfPackages>123</numberOfPackages>
        </Packaging>
      </ConsignmentItem>
    </HouseConsignment>
  </Consignment>
</q1:CC015C>";
			var message = departureHeader.LinkedMessages[0];
			AssertXmlEquals(expectedXml, message.EM_MessageText);
			AssertHtmlTableContainsRows(message.EM_MessageInterpretation, new Dictionary<string, string>
			{
				{ "Message Type", "CC015C" },
				{ "Declaration Type", "T1" },
				{ "Additional Declaration Type", "D" },
				{ "LRN (Local Reference Number)", "2394539099200000000001" },
				{ "Customs Office (Departure)", "GB000011" },
				{ "Customs Office (Destination)", "DE000011" },
				{ "Reduced Dataset", "0" },
				{ "Security", "0" },
				{ "Binding Itinerary", "0" },
				{ "Principle EORI", "GB0123456789000" },
				{ "Message Recipient", "NTA.GB" }
			});
			AssertContains("<H3>Guarantees</H3>", message.EM_MessageInterpretation);
			AssertContains("00AA1234567890123", message.EM_MessageInterpretation);
			AssertContains("123.45 GBP", message.EM_MessageInterpretation);
			AssertContains("<H3>Consignments</H3>", message.EM_MessageInterpretation);
			AssertContains("ITEM REF", message.EM_MessageInterpretation);
			AssertContains("Item Description", message.EM_MessageInterpretation);
			AssertContains("123456", message.EM_MessageInterpretation);
		}

		[TestUtcOffset(1, 0, 0)]
		public void TestSendIE015_ShouldNotIncludeTimezoneOffsetInArrivalDateAndTimeEstimated()
		{
			SetupIE015Header();

			var movementHeader = departureHeader.MovementHeader;
			var co = movementHeader.CustomsOffices.AddNew();
			co.CY_Code = "TRA";
			co.CY_Date = new ZDateTime(2023, 9, 13, 10, 15, 25);

			TestSendDepartureMessage(GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration);
			AssertContains(@"<arrivalDateAndTimeEstimated>2023-09-13T10:15:25</arrivalDateAndTimeEstimated>", departureHeader.LinkedMessages[0].EM_MessageText);
		}

		[TestDate(2024, 11, 19)]
		public void TestSendIE015_ShouldSetValuationDate()
		{
			var sendingObject = new NctsHeaderMessageSendingObject(departureHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject) { MessageType = GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration };
			var sender = new NctsMessageSender(sendingAction);

			AssertEquals("Precondition", ZDateTime.Empty, departureHeader.MovementHeader.BM_ValuationDate);
			sender.Send();

			AssertEquals(ZDateTime.Now, departureHeader.MovementHeader.BM_ValuationDate);
		}

		public void TestSendIE170()
		{
			TestSendDepartureMessage(GB_NCTS5DeparturePhaseList.Codes.PresentationOfAPreLodgedDeclaration, NCTS5DepartureCustomsStatusList.Codes.PreLodged);
			AssertContains(@"<tr><td>Message Type</td><td>CC170C</td></tr><tr><td>Customs Office (Departure)</td><td>GB000011</td></tr><tr><td>Principle EORI</td><td>GB0123456789000</td></tr><tr><td>Message Recipient</td><td>NTA.GB</td></tr>", departureHeader.LinkedMessages[0].EM_MessageInterpretation);
		}

		[TestDate(2023, 09, 13, 01, 02, 03, 456)]
		[TestUtcOffset(1, 0, 0)]
		public void TestSendIE007()
		{
			AssertEquals("Has ArrivalCustomsOffice", "GB000011", arrivalHeader.ArrivalMovementHeader.DestinationCustomsOfficeCodeForArrival);
			TestSendArrivalMessage(GB_NCTS5ArrivalPhaseList.Codes.ArrivalNotification);
			const string expectedXml_Less_Elements = @"<?xml version=""1.0"" encoding=""utf-8""?>
<q1:CC007C xmlns:q1=""http://ncts.dgtaxud.ec"">
  <messageSender>GB111</messageSender>
  <messageRecipient>NTA.GB</messageRecipient>
  <preparationDateAndTime>2023-09-13T01:02:03</preparationDateAndTime>
  <messageIdentification>&lt;&lt;SENDERS REFERENCE PLACE HOLDER&gt;&gt;</messageIdentification>
  <messageType>CC007C</messageType>
  <correlationIdentifier>&lt;&lt;SENDERS REFERENCE PLACE HOLDER&gt;&gt;</correlationIdentifier>
  <TransitOperation>
    <arrivalNotificationDateAndTime>2023-09-13T01:02:03</arrivalNotificationDateAndTime>
    <simplifiedProcedure>0</simplifiedProcedure>
    <incidentFlag>0</incidentFlag>
  </TransitOperation>
  <CustomsOfficeOfDestinationActual>
    <referenceNumber>GB000011</referenceNumber>
  </CustomsOfficeOfDestinationActual>
</q1:CC007C>";
			AssertXmlEquals(expectedXml_Less_Elements, arrivalHeader.Messages[0].EM_MessageText);
			AssertContains(@"<tr><td>Message Type</td><td>CC007C</td></tr><tr><td>Simplified Procedure</td><td>0</td></tr><tr><td>Message Recipient</td><td>NTA.GB</td></tr>", arrivalHeader.Messages[0].EM_MessageInterpretation);

			arrivalHeader.Messages.RemoveAndDeleteAll();
			var authorizationUsage1 = arrivalHeader.CusAuthorizationUsages.AddNew();
			authorizationUsage1.AGC_OH_Owner = principal.Header.PK;
			authorizationUsage1.AGC_Number = "usage1";
			authorizationUsage1.AGC_Code = Business.CodeDescriptionPairLists.CusAuthorizationHeaderTypeList.Codes.AuthorizedConsigneeTransit;
			TestSendArrivalMessage(GB_NCTS5ArrivalPhaseList.Codes.ArrivalNotification);
			const string expectedXml_More_Elements = @"<?xml version=""1.0"" encoding=""utf-8""?>
<q1:CC007C xmlns:q1=""http://ncts.dgtaxud.ec"">
  <messageSender>GB111</messageSender>
  <messageRecipient>NTA.GB</messageRecipient>
  <preparationDateAndTime>2023-09-13T01:02:03</preparationDateAndTime>
  <messageIdentification>&lt;&lt;SENDERS REFERENCE PLACE HOLDER&gt;&gt;</messageIdentification>
  <messageType>CC007C</messageType>
  <correlationIdentifier>&lt;&lt;SENDERS REFERENCE PLACE HOLDER&gt;&gt;</correlationIdentifier>
  <TransitOperation>
    <arrivalNotificationDateAndTime>2023-09-13T01:02:03</arrivalNotificationDateAndTime>
    <simplifiedProcedure>1</simplifiedProcedure>
    <incidentFlag>0</incidentFlag>
  </TransitOperation>
  <Authorisation>
    <sequenceNumber>1</sequenceNumber>
    <type>C522</type>
    <referenceNumber>usage1</referenceNumber>
  </Authorisation>
  <CustomsOfficeOfDestinationActual>
    <referenceNumber>GB000011</referenceNumber>
  </CustomsOfficeOfDestinationActual>
</q1:CC007C>";
			AssertXmlEquals(expectedXml_More_Elements, arrivalHeader.Messages[0].EM_MessageText);
			AssertContains(@"<tr><td>Message Type</td><td>CC007C</td></tr><tr><td>Simplified Procedure</td><td>1</td></tr><tr><td>Message Recipient</td><td>NTA.GB</td></tr>", arrivalHeader.Messages[0].EM_MessageInterpretation);
		}

		public void TestSendIE044()
		{
			TestSendArrivalMessage(GB_NCTS5ArrivalPhaseList.Codes.UnloadingRemarks, NCTS5ArrivalCustomsStatusList.Codes.UnloadRemarks);
			AssertContains(@"<tr><td>Message Type</td><td>CC044C</td></tr><tr><td>Message Recipient</td><td>NTA.GB</td></tr>", arrivalHeader.Messages[0].EM_MessageInterpretation);
		}

		void TestSendDepartureMessage(string messageType, string customsStatus = "", NctsHeaderMessageSendingObject providedSendingObject = null)
		{
			var eori = principal.Header.GetEoriDetails();
			var sendingObject = providedSendingObject ?? new NctsHeaderMessageSendingObject(departureHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = messageType;
			var sender = new NctsMessageSender(sendingAction);
			var message = sender.Send();

			AssertEquals("Message count", 1, departureHeader.LinkedMessages.Count);
			AssertNotNullOrEmpty("EORI", eori);
			CombineAssertions(() =>
			{
				var linkedMessage = departureHeader.LinkedMessages[0];
				AssertEquals("Application code", "GBN", linkedMessage.EM_ApplicationCode);
				AssertEquals("Message Type", messageType, linkedMessage.EM_MessageType);
				AssertEquals("Message Owner", eori, linkedMessage.EM_MessageOwner);
				AssertEquals("Direction", "TRX", linkedMessage.EM_ReceiveTransmit);
				AssertEquals("Status", "QUE", linkedMessage.EM_Status);
				AssertXmlRootElement(
					linkedMessage.EM_MessageText,
					$"CC{messageType}C",
					"http://ncts.dgtaxud.ec"
				);
				AssertEquals("Header Status", LogicalStatusList.Codes.Sent, departureHeader.EffectiveMessageStatus);
				AssertEquals("Movement Header Customs Status", customsStatus, departureHeader.MovementHeader.BM_CustomsStatus);
				AssertEquals("Movement Header Phase", messageType, departureHeader.MovementHeader.BM_Phase);
				AssertEquals("Message Linked table", message.EM_LinkTable, CusInBondMoveHeader.Schema.TableName);
				AssertEquals("Message Linked Object", message.EM_LinkUniqueID, departureHeader.MovementHeader.PK);
			});
		}

		void TestSendArrivalMessage(string messageType, string customsStatus = "")
		{
			var sendingObject = new NctsHeaderMessageSendingObject(arrivalHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = messageType;
			var sender = new NctsMessageSender(sendingAction);
			_ = sender.Send();

			AssertEquals("Message count", 1, arrivalHeader.Messages.Count);
			CombineAssertions(() =>
			{
				AssertEquals("Application code", "GBN", arrivalHeader.Messages[0].EM_ApplicationCode);
				AssertEquals("Message Type", messageType, arrivalHeader.Messages[0].EM_MessageType);
				AssertEquals("Direction", "TRX", arrivalHeader.Messages[0].EM_ReceiveTransmit);
				AssertEquals("Status", "QUE", arrivalHeader.Messages[0].EM_Status);
				AssertXmlRootElement(
					arrivalHeader.Messages[0].EM_MessageText,
					$"CC{messageType}C",
					@"http://ncts.dgtaxud.ec");
				AssertEquals("Header Status", LogicalStatusList.Codes.Sent, arrivalHeader.EffectiveMessageStatus);
				AssertEquals("Movement Header Customs Status", customsStatus, arrivalHeader.ArrivalMovementHeader.BM_CustomsStatus);
				AssertEquals("Movement Header Phase", messageType, arrivalHeader.ArrivalMovementHeader.BM_Phase);
			});
		}

		public void TestAddPermitRecordOnMessageSend()
		{
			NCTSTestHelper.SetupC0009ForEuAndCtCountries(Factory);

			var org = Factory.NewWithValidTestData<OrgHeader>();
			_ = org.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "123456789000", GlbCompany.CurrentCompany.GC_RN_NKCountryCode);
			departureHeader.Principal.E2_OA_Address = org.MainAddress.PK;

			var guaranteeHeader = Factory.NewWithValidTestData<CusGuaranteeHeader>();
			guaranteeHeader.CPH_Number = "GUA1";
			guaranteeHeader.CPH_OH_PermitHolder = org.PK;
			guaranteeHeader.CPH_Type = EU.Business.CodeDescriptionPairLists.EUGuaranteeTypeList.Codes.TRA;
			guaranteeHeader.CPH_ApplicationCode = Customs.Common.Shared.CusPermitHeaderApplicationCodeList.Codes.Guarantee;
			var transaction1 = guaranteeHeader.CusGuaranteeLineTransactions.AddNew();
			transaction1.CPL_Reference = "Entry Number";
			transaction1.CPL_TranValue = 100m;
			transaction1.CPL_TransactionType = Enterprise.Customs.Business.PermitTransactionTypeList.Codes.CUS;
			transaction1.CPL_TransactionStatus = Enterprise.Customs.Business.PermitTransactionStatusList.Codes.Confirmed;
			transaction1.CPL_TransactionCategory = Enterprise.Customs.Business.PermitTransactionCategoryList.Codes.CUM;
			transaction1.CPL_AppId = "Entry Reference";
			transaction1.CPL_Comment = "Instruction Desc.";
			transaction1.CPL_Procedure = "AAA";

			var guarantee = departureHeader.MovementHeader.Guarantees.AddNew();
			guarantee.PW_BondAmount = 10m;
			guarantee.PW_BondNumber = "GUA1";

			var sendingObject = new NctsHeaderMessageSendingObject(departureHeader);
			var sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = GB_NCTS5DeparturePhaseList.Codes.DepartureDeclaration;
			var sender = new NctsMessageSender(sendingAction);
			_ = sender.Send();
			Factory.Save();
			AssertGuaranteeLineTransactions(departureHeader, guarantee);

			sendingObject = new NctsHeaderMessageSendingObject(departureHeader);
			sendingAction = new NctsMessageSendingAction(sendingObject);
			sendingAction.MessageType = GB_NCTS5DeparturePhaseList.Codes.Amendment;
			_ = new NctsMessageSender(sendingAction).Send();
			Factory.Save();
			AssertGuaranteeLineTransactions(departureHeader, guarantee);
		}

		void AssertGuaranteeLineTransactions(NctsHeader nctsHeader, NctsGuarantee guarantee)
		{
			CombineAssertions(() =>
			{
				var permit = departureHeader.GetPermitRecords();
				AssertEquals(1, permit.Count);

				var transactionsGuarantee = guarantee.CusGuarantee.CusGuaranteeLineTransactions;
				AssertEquals("Transactions count", 2, transactionsGuarantee.Count);

				var transaction = transactionsGuarantee[1];
				AssertEquals("Transaction ID", "1", transaction.CPL_AppId);
				AssertEquals("Transaction Comment", $"NCTS departure {nctsHeader.LocalReferenceNumber}", transaction.CPL_Comment);
				AssertEquals("Transaction Reference", nctsHeader.BH_JobReference, transaction.CPL_Reference);
				AssertEquals("Transaction Reference Line No.", 0, transaction.CPL_ReferenceNumberLine);
				AssertEquals("Transaction Value", -guarantee.PW_BondAmount, transaction.CPL_TranValue);
				AssertEquals("Transaction Category", Enterprise.Customs.Business.PermitTransactionCategoryList.Codes.CUM, transaction.CPL_TransactionCategory);
				AssertEquals("Transaction Status", Enterprise.Customs.Business.PermitTransactionStatusList.Codes.Pending, transaction.CPL_TransactionStatus);
				AssertEquals("Transaction Type", Enterprise.Customs.Business.PermitTransactionTypeList.Codes.TRA, transaction.CPL_TransactionType);
			});
		}

		static void AssertXmlRootElement(string xml, string expectedLocalName, string expectedNamespace)
		{
			var doc = XDocument.Parse(xml);

			if (doc.Root == null)
			{
				throw new Exception("XML has no root element.");
			}

			var actualName = doc.Root.Name.LocalName;
			var actualNs = doc.Root.Name.NamespaceName;

			if (actualName != expectedLocalName)
			{
				throw new Exception($"Expected root name '{expectedLocalName}', but got '{actualName}'.");
			}

			if (actualNs != expectedNamespace)
			{
				throw new Exception($"Expected root namespace '{expectedNamespace}', but got '{actualNs}'.");
			}
		}

		static void AssertXmlEquals(string expectedXml, string actualXml)
		{
			var expected = NormalizeNamespaces(XElement.Parse(expectedXml));
			var actual = NormalizeNamespaces(XElement.Parse(actualXml));

			if (!XNode.DeepEquals(expected, actual))
			{
				throw new Exception("XML content does not match.\n\nExpected:\n" + expected + "\n\nActual:\n" + actual);
			}
		}

		static XElement NormalizeNamespaces(XElement element, XNamespace defaultNamespace = null)
		{
			var currentNs = element.Name.Namespace;
			var effectiveNs = currentNs != XNamespace.None ? currentNs : defaultNamespace ?? XNamespace.None;

			var normalized = new XElement(
				effectiveNs + element.Name.LocalName,
				element.Attributes().Where(a => !a.IsNamespaceDeclaration), // Strip explicit xmlns=""
				element.Nodes().Select(n =>
				{
					if (n is XElement child)
					{
						return NormalizeNamespaces(child, effectiveNs);
					}
					else if (n is XText text)
					{
						return new XText(text.Value.Trim());
					}
					else
					{
						return n;
					}
				})
			);

			return normalized;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE0270:Use coalesce expression", Justification = "Will make the function less readable.")]
		static void AssertHtmlTableContainsRows(string html, Dictionary<string, string> expectedRows)
		{
			var doc = new HtmlAgilityPack.HtmlDocument();
			doc.LoadHtml(html);

			foreach (var rowPair in expectedRows)
			{
				var key = rowPair.Key;
				var expectedValue = rowPair.Value;

				var row = doc.DocumentNode
							 .SelectNodes("//tr")
							 ?.FirstOrDefault(tr =>
								 tr.SelectNodes("td")?.Count == 2 &&
								 tr.SelectNodes("td")[0].InnerText.Trim() == key);

				if (row == null)
				{
					throw new Exception($"Missing row with key: '{key}'");
				}

				var actualValue = row.SelectNodes("td")[1].InnerText.Trim();
				if (actualValue != expectedValue)
				{
					throw new Exception($"Value mismatch for '{key}': expected '{expectedValue}', got '{actualValue}'");
				}
			}
		}

		protected override void SetUp()
		{
			var companyOrg = GlbCompany.CurrentCompany.OrgProxy;
			_ = companyOrg.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "111", Core.Constants.CountryCodes.UnitedKingdom);

			departureHeader = Factory.New<NctsHeader>();
			departureHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			departureHeader.SetMovementType(NctsMovementType.Codes.Departure);
			departureHeader.MovementHeader.BM_TypeOfSecurity = NctsTypeOfSecurityList.Codes.NON;
			departureHeader.BH_GB = GlbBranch.CurrentBranch.PK;
			_ = NCTSTestHelper.CreateCustomsOfficeForTest(departureHeader.MovementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDeparture, "GB000011", ZDateTime.Empty, clearOffices: true);

			arrivalHeader = Factory.New<NctsHeader>();
			arrivalHeader.BH_ApplicationCode = CusInBondApplicationCodeList.Codes.NCTS5;
			arrivalHeader.SetMovementType(NctsMovementType.Codes.Arrival);
			arrivalHeader.BH_GB = GlbBranch.CurrentBranch.PK;
			_ = NCTSTestHelper.CreateCustomsOfficeForTest(arrivalHeader.ArrivalMovementHeader, OfficeCodes_NCTS.Codes.NCTSOfficeOfDestinationForArrival, "GB000011", ZDateTime.Empty, clearOffices: true);

			principal = NCTSTestHelper.CreateOrgAddressForTest(Factory, traderId: "AR1");
			departureHeader.Principal.OrganisationPK = principal.Header.PK;
		}

		NctsHeader departureHeader, arrivalHeader;
		OrgAddress principal;
	}
}
