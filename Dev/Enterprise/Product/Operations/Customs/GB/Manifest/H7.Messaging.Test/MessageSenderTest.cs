using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.Common.Shared;
using Enterprise.Customs.GB.Business;
using Enterprise.Customs.GB.Business.CodeDescriptionPairLists;
using Enterprise.Customs.GB.CDS;
using Enterprise.Customs.GB.H7.Business;
using Enterprise.Customs.GB.Registry;
using Enterprise.Customs.Universal.Testing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Integration;
using Enterprise.Messaging.Business;
using NUnit.Framework;
using static Enterprise.Core.Constants;

namespace Enterprise.Customs.GB.H7.Messaging.Testing
{
	sealed class MessageSenderTest : TestCaseWithFactory
	{
		public void TestSendBirdsMessage_New()
		{
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "1H7", ZString.Empty, ZString.Empty);
			Factory.Save();

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_RX_NKExCurrency = "CNY";
			exchangeRate.RE_SellRate = 7m;

			var bill = CreateTestBill();
			bill.Header.AMA_VesselName = "TEST1234";
			bill.ABL_ShipmentType = EntrySubStyleCodeList.Codes.D;

			var sendingObject = new MessageSendingObject(bill);
			sendingObject.Action = GBMessageTypeList.Codes.New;
			sendingObject.SubStyle = "D";

			var messageSender = new MessageSender(new MessageSendingObject[] { sendingObject });
			messageSender.Send();

			var message = bill.Messages.FirstOrDefault() as CDSNewDeclarationEDIMessage;
			CombineAssertions(() =>
			{
				AssertNotNull(message);
				AssertEquals(expectedBirdsNewMessageXml, message.EM_MessageText);
				AssertEquals(MessageStatusList.Codes.AwaitingOriginal, bill.ABL_MessageStatus);
			});
		}

		public void TestSendH7Message_New()
		{
			var dischargePort = "GBBEL";
			SetupNorthernIrelandRegion(dischargePort);

			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateRefCusProcedure("CDS", "APC", "40", "00", "1H7", ZString.Empty, ZString.Empty);
			Factory.Save();

			var exchangeRate = Factory.New<RefExchangeRate>();
			exchangeRate.RE_StartDate = ZDateTime.Today.AddDays(-1);
			exchangeRate.RE_ExpiryDate = ZDateTime.Today.AddDays(2);
			exchangeRate.RE_ExRateType = ExchangeRateTypes.Code.CustomsRate;
			exchangeRate.RE_RX_NKExCurrency = "CNY";
			exchangeRate.RE_SellRate = 7m;

			var bill = CreateTestBill(dischargePort);
			bill.ABL_ShipmentType = EntrySubStyleCodeList.Codes.D;

			var sendingObject = new MessageSendingObject(bill);
			sendingObject.Action = GBMessageTypeList.Codes.New;
			sendingObject.SubStyle = "D";

			var messageSender = new MessageSender(new MessageSendingObject[] { sendingObject });
			messageSender.Send();

			var message = bill.Messages.FirstOrDefault() as CDSNewDeclarationEDIMessage;
			CombineAssertions(() =>
			{
				AssertNotNull(message);
				AssertEquals(expectedH7NewMessageXml, message.EM_MessageText);
				AssertEquals(MessageStatusList.Codes.AwaitingOriginal, bill.ABL_MessageStatus);
			});
		}

		public void TestSendH7Message_Arrival()
		{
			var bill = CreateTestBill();
			bill.LocalReferenceNumber = "LRN";
			bill.MovementReferenceNumber = "MRN";

			var sendingObject = new MessageSendingObject(bill);
			sendingObject.Action = "ARR";
			sendingObject.AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.C_GoodsPresentationNotice;
			sendingObject.AmendmentInvalidationReason = "GoodsPresentationNotice";

			var messageSender = new MessageSender(new MessageSendingObject[] { sendingObject });
			messageSender.Send();

			var message = bill.Messages.FirstOrDefault() as CDSArrivalAmendmentDeclarationEDIMessage;
			CombineAssertions(() =>
			{
				AssertNotNull(message);
				AssertEquals(expectedH7ArrivalMessageXML, message.EM_MessageText);
				AssertEquals(MessageStatusList.Codes.AwaitingChange, bill.ABL_MessageStatus);
			});
		}

		public void TestSendH7Message_Cancel()
		{
			var bill = CreateTestBill();
			bill.LocalReferenceNumber = "LRN";
			bill.MovementReferenceNumber = "MRN";

			var sendingObject = new MessageSendingObject(bill);
			sendingObject.Action = "CAN";
			sendingObject.AmendmentReasonCode = AmendmentCancellationReasonCode.Codes.C_NotRequired;
			sendingObject.AmendmentInvalidationReason = "NotRequired";

			var messageSender = new MessageSender(new MessageSendingObject[] { sendingObject });
			messageSender.Send();

			var message = bill.Messages.FirstOrDefault() as CDSArrivalAmendmentDeclarationEDIMessage;
			CombineAssertions(() =>
			{
				AssertNotNull(message);
				AssertEquals(expectedH7CancelMessageXml, message.EM_MessageText);
				AssertEquals(MessageStatusList.Codes.AwaitingDelete, bill.ABL_MessageStatus);
			});
		}

		[TestDate(2024, 12, 19, 0, 0, 0)]
		public void TestSendH7QueryMessage()
		{
			using (Factory.AddDisposableService())
			{
				var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
				header.AMA_CustomsProfile = "PR1";
				var bill = header.Bills.AddNew();
				header.Branch.OrgProxy.CustomsCodes.AddNew(OrgCusCode.EuropeanUnionSharedCodeTypes.Eori, "987654321ABC");
				bill.MovementReferenceNumber = "MRN0001";
				CreateBadgeCode("PR1", "CDS");
				CreatePassword("GB987654321ABC", "GB987654321ABC.PR1");

				var sendingObject = new MessageSendingObject(bill);
				sendingObject.Action = "QUE";
				sendingObject.QueryType = H7QueryTypeList.Codes.MRNSummary;

				var sender = new MessageSender(new MessageSendingObject[] { sendingObject });
				AssertEquals(1, sender.Send());
				var message = bill.Messages.FirstOrDefault() as EDIMessage;
				AssertNotNull(message);
				AssertContains(expectedH7QueryMessageXML, message.EM_MessageText.Replace("\r\n", "").Replace(" ", ""));
				AssertNullOrEmpty(ZArchitecture.Environment.UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestSend_ShowProgress()
		{
			var header = Factory.New<AsycudaManifestHeader>();
			var bill1 = header.Bills.AddNew();
			var bill2 = header.Bills.AddNew();

			var sendingObject = new MessageSendingObject(bill1);
			sendingObject.Action = GBMessageTypeList.Codes.New;

			var sendingObject2 = new MessageSendingObject(bill2);
			sendingObject2.Action = GBMessageTypeList.Codes.New;

			var progressUpdateLogs = new List<(int ProcessedMessages, int MessagesToProcess)>();

			Action<int, int> progressUpdateCallback = (message, progress) =>
			{
				progressUpdateLogs.Add((message, progress));
			};

			var messageSender = new MessageSender(new MessageSendingObject[] { sendingObject, sendingObject2 }, progressUpdateCallback);
			messageSender.Send();

			CombineAssertions(() =>
			{
				AssertEquals("Message should be sent for bill 1", 1, bill1.Messages.Count);
				AssertEquals("Message should be sent for bill 2", 1, bill2.Messages.Count);

				AssertEquals("Expected logs count", 2, progressUpdateLogs.Count);

				AssertEquals("First log's ProcessedMessages", 1, progressUpdateLogs[0].ProcessedMessages);
				AssertEquals("First log's MessagesToProcess", 2, progressUpdateLogs[0].MessagesToProcess);

				AssertEquals("Second log's ProcessedMessages", 2, progressUpdateLogs[1].ProcessedMessages);
				AssertEquals("Second log's MessagesToProcess", 2, progressUpdateLogs[1].MessagesToProcess);
			});
		}

		void CreateBadgeCode(ZString badge, ZString cspCode)
		{
			var badgeCodeSettings = GBCustomsDataRegistry.Instance.BadgeCodes.GetFallBackValueAtAllLevels(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty);
			var badgeCodeSetting = badgeCodeSettings.AddNew();
			badgeCodeSetting.BadgeCode = badge;
			badgeCodeSetting.CSPCode = cspCode;

			GBCustomsDataRegistry.Instance.BadgeCodes.SetValue(Guid.Empty, GlbBranch.CurrentBranch.PK.ToGuid(), Guid.Empty, badgeCodeSettings);
		}

		GlbExternalPassword_GB CreatePassword(string eori, string userId)
		{
			var extPwd = Factory.New<GlbExternalPassword_GB>();
			extPwd.GP_GC = GlbCompany.CurrentCompany.PK.ToGuid();
			extPwd.EORI = eori;
			extPwd.GP_UserID = userId;
			extPwd.GP_PasswordType = "CDS";
			extPwd.GP_CurrentPassword = "NOWPWD";
			extPwd.GP_NextPassword = "NEXTPWD";
			extPwd.GP_IssueDate = DateTime.Today.AddDays(-5);
			extPwd.GP_ExpiryDate = DateTime.Today.AddDays(5);
			extPwd.GP_PasswordStatus = PasswordStatusList.Codes.PasswordOK;
			return extPwd;
		}

		AsycudaBill CreateTestBill(string dischargePort = "GBLHR")
		{
			var header = Factory.NewWithValidTestData<AsycudaManifestHeader>();
			header.PresentationOffice = "ABC";
			header.AMA_TransportMode = "SEA";
			header.AMA_RL_NKPortOfDischarge = dischargePort;

			var declarantOrg = Factory.New<OrgHeader>();
			var relatedPartyOrg = Factory.New<OrgHeader>();
			relatedPartyOrg.CustomsCodes.AddNew(OrgCusCode.CodeTypes.CustomsClientCode, "GB001", CountryCodes.UnitedKingdom);

			var relatedParty = declarantOrg.AllRelatedParties.AddNew();
			relatedParty.PR_OH_RelatedParty = relatedPartyOrg.PK;
			relatedParty.PR_PartyType = RelatedPartyTypeList.Codes.CustomsOffice;
			relatedParty.PR_FreightDirection = RelatedPartyDirectionList.Codes.Forwarder;

			var declarant = declarantOrg.Addresses.AddNew();
			declarant.OA_CompanyNameOverride = "declarant";
			declarant.OA_City = "Sydney";
			declarant.OA_RN_NKCountryCode = "AU";
			declarant.OA_Address1 = "address line1";
			declarant.OA_Address2 = "address line2";
			header.AMA_OA_Declarant = declarant.PK;

			var representative = Factory.New<OrgAddress>();
			representative.OA_CompanyNameOverride = "representative";
			representative.OA_City = "Los Angeles";
			representative.OA_RN_NKCountryCode = "US";
			representative.OA_Address1 = "address line1";
			representative.OA_Address2 = "address line2";
			header.AMA_OA_Representative = declarant.PK;

			header.AMA_AgentType = EU.H7.Business.EUH7AgentTypes.Codes.DIR;

			var bill = header.Bills.AddNew();

			var shipper = Factory.New<OrgAddress>();
			shipper.OA_CompanyNameOverride = "shipper";
			shipper.OA_City = "Sydney";
			shipper.OA_RN_NKCountryCode = "AU";
			shipper.OA_Address1 = "address line1";
			shipper.OA_Address2 = "address line2";
			bill.ABL_OA_Shipper = shipper.PK;

			bill.CusGoodsLocation.CGL_Type = "A";
			bill.CusGoodsLocation.CGL_Qualifier = "B";
			bill.CusGoodsLocation.CGL_AdditionalIdentifier = "CusGoodsLocationName";

			var consignee = Factory.New<OrgAddress>();
			consignee.OA_CompanyNameOverride = "consignee";
			consignee.OA_City = "Los Angeles";
			consignee.OA_RN_NKCountryCode = "US";
			consignee.OA_Address1 = "address line1";
			consignee.OA_Address2 = "address line2";
			bill.ABL_OA_Consignee = consignee.PK;

			bill.ABL_GrossWeight = 1;
			bill.ABL_GrossWeightUQ = "KG";
			bill.ABL_TransportValue = 1;
			bill.ABL_RX_NKTransportValueCurrency = "AUD";
			bill.LocalReferenceNumber = "TestLRN";
			bill.ABL_CustomsValue = 5;
			bill.ABL_RX_NKCustomsValueCurrency = "GBP";

			var billPreviousDocument = bill.PreviousDocuments.AddNew();
			billPreviousDocument.CSI_Code = "333";
			billPreviousDocument.CSI_ReferenceNumber = "654321";
			billPreviousDocument.CSI_LineNo = 321;
			billPreviousDocument.CSI_SystemCreateTimeUtc = new ZDateTime(2029, 1, 1);

			var packedItem = bill.PackedItems.AddNew();
			packedItem.API_Tariff = "12345678";
			packedItem.API_CustomsQty2 = 12.000000;
			packedItem.API_GrossWeight = 200123.456m;
			packedItem.API_GrossWeightUQ = "G";
			packedItem.API_GoodsValue = 42;
			packedItem.API_RX_NKGoodsValueCurrency = "CNY";
			packedItem.API_NetWeight = 600m;
			packedItem.API_NetWeightUQ = "G";

			var packedItemAdditionalInfo = packedItem.AdditionalInfos.AddNew();
			packedItemAdditionalInfo.CSI_Code = "380";
			packedItemAdditionalInfo.CSI_Description = "Heaven and Earth.";

			var packedItemSupportingDocument = packedItem.SupportingDocuments.AddNew();
			packedItemSupportingDocument.CSI_Code = "108C";
			packedItemSupportingDocument.CSI_ReferenceNumber = "An intense dissatisfaction with the world";
			packedItemSupportingDocument.CSI_ReferenceNumber2 = "about it";
			packedItemSupportingDocument.CSI_Availability = "A";
			packedItemSupportingDocument.CSI_Actions = "C";
			packedItemSupportingDocument.CSI_Description = "And a compulsion to do something";
			packedItemSupportingDocument.CSI_DateOfIssue = new ZDateTime(2013, 01, 01);

			var packedItemPreviousDocument = packedItem.PreviousDocuments.AddNew();
			packedItemPreviousDocument.CSI_Code = "382";
			packedItemPreviousDocument.CSI_ReferenceNumber = "123456";
			packedItemPreviousDocument.CSI_LineNo = 123;
			packedItemPreviousDocument.CSI_SystemCreateTimeUtc = new ZDateTime(2022, 1, 1);

			var pack = bill.Packs.AddNew();
			pack.APA_GoodsDescription = "GoodsDescription";

			packedItem.AsycudaPackPackedItemPivots.AddPivotFor(pack);
			packedItem.API_GoodsDescription = "GoodsDescription";

			return bill;
		}

		void SetupNorthernIrelandRegion(string unloco)
		{
			var belfast = new RefUNLOCO.Loader(Factory).Load(unloco);
			if (belfast.CountryStates == null || string.Compare(belfast.CountryStates.RW_RegionName, "NORTHERN IRELAND", true) != 0)
			{
				var ni = Factory.New<RefCountryStates>();
				ni.RW_RN_NKCountryCode = "GB";
				ni.RW_RegionName = "NORTHERN IRELAND";
				belfast.RL_RW = ni.PK;
			}
		}

		const string expectedBirdsNewMessageXml = @"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>9</FunctionCode>
    <FunctionalReferenceID>TestLRN</FunctionalReferenceID>
    <TypeCode>IMD</TypeCode>
    <GoodsItemQuantity>1</GoodsItemQuantity>
    <TotalPackageQuantity>0</TotalPackageQuantity>
    <Agent>
      <Name>declarant</Name>
      <FunctionCode>2</FunctionCode>
      <Address>
        <CityName>Sydney</CityName>
        <CountryCode>AU</CountryCode>
        <Line>address line1 address line2</Line>
        <PostcodeID>NA</PostcodeID>
      </Address>
    </Agent>
    <BorderTransportMeans>
      <ModeCode>1</ModeCode>
    </BorderTransportMeans>
    <Exporter>
      <Name>shipper</Name>
      <Address>
        <CityName>Sydney</CityName>
        <CountryCode>AU</CountryCode>
        <Line>address line1address line2</Line>
        <PostcodeID>NA</PostcodeID>
      </Address>
    </Exporter>
    <GoodsShipment>
      <Consignment>
        <ContainerCode>0</ContainerCode>
        <ArrivalTransportMeans>
          <ID>TEST1234</ID>
          <IdentificationTypeCode>11</IdentificationTypeCode>
        </ArrivalTransportMeans>
        <GoodsLocation>
          <Name>CusGoodsLocationName</Name>
          <TypeCode>A</TypeCode>
          <Address>
            <TypeCode>B</TypeCode>
            <CountryCode>GB</CountryCode>
          </Address>
        </GoodsLocation>
      </Consignment>
      <CustomsValuation>
        <FreightChargeAmount currencyID=""AUD"">1.00</FreightChargeAmount>
      </CustomsValuation>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>1</CategoryCode>
          <EffectiveDateTime>
            <DateTimeString formatCode=""102"" xmlns=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"">20130101</DateTimeString>
          </EffectiveDateTime>
          <ID>An intense dissatisfaction with the world</ID>
          <Name>And a compulsion to do something</Name>
          <TypeCode>08C</TypeCode>
          <LPCOExemptionCode>AC</LPCOExemptionCode>
          <Submitter>
            <Name>about it</Name>
          </Submitter>
        </AdditionalDocument>
        <AdditionalInformation>
          <StatementCode>380</StatementCode>
          <StatementDescription>Heaven and Earth.</StatementDescription>
        </AdditionalInformation>
        <Commodity>
          <Description>GoodsDescription</Description>
          <GoodsMeasure>
            <GrossMassMeasure>200.123456</GrossMassMeasure>
            <NetNetWeightMeasure>0.6</NetNetWeightMeasure>
            <TariffQuantity>12</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""CNY"">42.00</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>
        <GovernmentProcedure>
          <CurrentCode>00</CurrentCode>
          <PreviousCode>20</PreviousCode>
        </GovernmentProcedure>
        <GovernmentProcedure>
          <CurrentCode>21V</CurrentCode>
        </GovernmentProcedure>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <QuantityQuantity>0</QuantityQuantity>
        </Packaging>
        <PreviousDocument>
          <CategoryCode>Z</CategoryCode>
          <ID>123456</ID>
          <TypeCode>382</TypeCode>
          <LineNumeric>123</LineNumeric>
        </PreviousDocument>
        <PreviousDocument>
          <CategoryCode>Z</CategoryCode>
          <ID>654321</ID>
          <TypeCode>333</TypeCode>
          <LineNumeric>321</LineNumeric>
        </PreviousDocument>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
    <SupervisingOffice>
      <ID>GB001</ID>
    </SupervisingOffice>
  </Declaration>
</MetaData>";

		const string expectedH7NewMessageXml = @"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>9</FunctionCode>
    <FunctionalReferenceID>TestLRN</FunctionalReferenceID>
    <TypeCode>IMD</TypeCode>
    <GoodsItemQuantity>1</GoodsItemQuantity>
    <Agent>
      <Name>declarant</Name>
      <FunctionCode>2</FunctionCode>
      <Address>
        <CityName>Sydney</CityName>
        <CountryCode>AU</CountryCode>
        <Line>address line1 address line2</Line>
        <PostcodeID>NA</PostcodeID>
      </Address>
    </Agent>
    <BorderTransportMeans>
      <ModeCode>1</ModeCode>
    </BorderTransportMeans>
    <Declarant>
      <Name>declarant</Name>
      <Address>
        <CityName>Sydney</CityName>
        <CountryCode>AU</CountryCode>
        <Line>address line1 address line2</Line>
        <PostcodeID>NA</PostcodeID>
      </Address>
    </Declarant>
    <Exporter>
      <Name>shipper</Name>
      <Address>
        <CityName>Sydney</CityName>
        <CountryCode>AU</CountryCode>
        <Line>address line1address line2</Line>
        <PostcodeID>NA</PostcodeID>
      </Address>
    </Exporter>
    <GoodsShipment>
      <Consignment>
        <GoodsLocation>
          <Name>CusGoodsLocationName</Name>
          <TypeCode>A</TypeCode>
          <Address>
            <TypeCode>B</TypeCode>
            <CountryCode>GB</CountryCode>
          </Address>
        </GoodsLocation>
      </Consignment>
      <CustomsValuation>
        <FreightChargeAmount currencyID=""AUD"">1.00</FreightChargeAmount>
      </CustomsValuation>
      <GovernmentAgencyGoodsItem>
        <CustomsValueAmount currencyID=""GBP"">6.00</CustomsValueAmount>
        <SequenceNumeric>1</SequenceNumeric>
        <AdditionalDocument>
          <CategoryCode>1</CategoryCode>
          <EffectiveDateTime>
            <DateTimeString formatCode=""102"" xmlns=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"">20130101</DateTimeString>
          </EffectiveDateTime>
          <ID>An intense dissatisfaction with the world</ID>
          <Name>And a compulsion to do something</Name>
          <TypeCode>08C</TypeCode>
          <LPCOExemptionCode>AC</LPCOExemptionCode>
          <Submitter>
            <Name>about it</Name>
          </Submitter>
        </AdditionalDocument>
        <AdditionalInformation>
          <StatementCode>380</StatementCode>
          <StatementDescription>Heaven and Earth.</StatementDescription>
        </AdditionalInformation>
        <Commodity>
          <Description>GoodsDescription</Description>
          <Classification>
            <ID>123456</ID>
            <IdentificationTypeCode>TSP</IdentificationTypeCode>
          </Classification>
          <GoodsMeasure>
            <GrossMassMeasure>200.123456</GrossMassMeasure>
            <TariffQuantity>12</TariffQuantity>
          </GoodsMeasure>
        </Commodity>
        <GovernmentProcedure>
          <CurrentCode>40</CurrentCode>
          <PreviousCode>00</PreviousCode>
        </GovernmentProcedure>
        <GovernmentProcedure>
          <CurrentCode>1H7</CurrentCode>
        </GovernmentProcedure>
        <PreviousDocument>
          <CategoryCode>Z</CategoryCode>
          <ID>123456</ID>
          <TypeCode>382</TypeCode>
          <LineNumeric>123</LineNumeric>
        </PreviousDocument>
        <PreviousDocument>
          <CategoryCode>Z</CategoryCode>
          <ID>654321</ID>
          <TypeCode>333</TypeCode>
          <LineNumeric>321</LineNumeric>
        </PreviousDocument>
      </GovernmentAgencyGoodsItem>
      <Importer>
        <Name>consignee</Name>
        <Address>
          <CityName>Los Angeles</CityName>
          <CountryCode>US</CountryCode>
          <Line>address line1 address line2</Line>
          <PostcodeID>NA</PostcodeID>
        </Address>
      </Importer>
    </GoodsShipment>
  </Declaration>
</MetaData>";

		const string expectedH7ArrivalMessageXML = @"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>13</FunctionCode>
    <FunctionalReferenceID>LRN</FunctionalReferenceID>
    <ID>MRN</ID>
    <TypeCode>GPR</TypeCode>
  </Declaration>
</MetaData>";

		const string expectedH7CancelMessageXml = @"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <WCODataModelVersionCode>3.6</WCODataModelVersionCode>
  <WCOTypeName>DEC</WCOTypeName>
  <ResponsibleCountryCode>GB</ResponsibleCountryCode>
  <ResponsibleAgencyName>HMRC</ResponsibleAgencyName>
  <AgencyAssignedCustomizationVersionCode>v2.1</AgencyAssignedCustomizationVersionCode>
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>13</FunctionCode>
    <FunctionalReferenceID>LRN</FunctionalReferenceID>
    <ID>MRN</ID>
    <TypeCode>INV</TypeCode>
    <AdditionalInformation>
      <StatementDescription>NotRequired</StatementDescription>
      <StatementTypeCode>AES</StatementTypeCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>06A</DocumentSectionCode>
      </Pointer>
    </AdditionalInformation>
    <Amendment>
      <ChangeReasonCode>1</ChangeReasonCode>
    </Amendment>
  </Declaration>
</MetaData>";

		const string expectedH7QueryMessageXML = @"<DataContext><DataSourceCollection><DataSource><Type>AsycudaBill</Type><Key>EU123456</Key></DataSource></DataSourceCollection><Company><Code>EDI</Code><Country><Code>GB</Code><Name>UnitedKingdom</Name></Country><Name>EagleDatamationInternational</Name></Company><DataProvider>EDIDATEDI</DataProvider><EnterpriseID>EDI</EnterpriseID><ServerID>DAT</ServerID></DataContext><EventTime>2024-12-19T00:00:00.000+00:00</EventTime><EventType>SVR</EventType><EventReference>|MST=QUERY|SER=GBCustomsCDS</EventReference><ContextCollection><Context><Type>EntryNumberType</Type><Value>MRN</Value></Context><Context><Type>EntryNumber</Type><Value>MRN0001</Value></Context><Context><Type>NotificationType</Type><Value>status</Value></Context><Context><Type>QueryString</Type><Value>partyRole=submitter</Value></Context><Context><Type>Key</Type><Value>EDIDAT.GB987654321ABC.PR1</Value></Context></ContextCollection>";
	}
}
