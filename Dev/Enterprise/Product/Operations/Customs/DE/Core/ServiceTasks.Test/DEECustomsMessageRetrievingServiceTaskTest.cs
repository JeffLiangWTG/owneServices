using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.ServiceTasks.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Customs.DE.ServiceTasks.Testing
{
	[TestedType(typeof(DEECustomsMessageRetrievingServiceTask))]
	public class DEECustomsMessageRetrieverServiceTaskTest : GMDCustomsMessagingServiceTest<DEECustomsMessageRetrievingServiceTask>
	{
		public void TestServiceAttribute()
		{
			AssertSingleHostedServiceAttribute("DEE", "DE AES Customs Message Retrieving", "DEC");
		}

		public void TestHostedServiceBindingAttribute()
		{
			TestHelper.AssertSingleHostedServiceAttribute<DEECustomsMessageRetrievingServiceTask>(ServiceTaskApplicationCodeList.Codes.DEEMessageRetrieving,
				ServiceTaskApplicationCodeList.Descriptions.DEEMessageRetrieving,
				"DEC",
				typeof(DEECustomsMessageRetrievingServiceTask),
				"60Seconds",
				Core.Constants.CountryCodes.Germany,
				true);
		}

		protected override IReadOnlyList<TaskNudgeInformationForTest> ExpectedHostedServiceBusinessObjectBindingAttributes
		{
			get
			{
				return new TaskNudgeInformationForTest[]
				{
					new TaskNudgeInformationForTest(
						EDIInterchangeSchema.Constants.TableName,
						ServiceTaskApplicationCodeList.Descriptions.DEEMessageRetrieving,
						EDIInterchangeSchema.Constants.EI_Status + "=" + EDIInterchange.Status.Queued,
						EDIInterchangeSchema.Constants.EI_ReceiveTransmit + "=" + EDIInterchange.Direction.Receive,
						EDIInterchangeSchema.Constants.EI_IsActive + "=Y",
						EDIInterchangeSchema.Constants.EI_InterchangeType + "=" + GenericMessageDeliveryInterchangeTypeList.Codes.DECustomsAesSystem,
						EDIInterchangeSchema.Constants.EI_ApplicationCode + "=" + EDIMessage.ApplicationCodes.GenericMessageDelivery),
				};
			}
		}

		protected override void AssertResult(BusinessObjectFactory factory, GMDCustomsMessagingServiceTestHelperData testData, DEECustomsMessageRetrievingServiceTask serviceTask)
		{
			var interchange = factory.Load<EDIInterchange>(testData.InterchangePK);
			CombineAssertions(() =>
			{
				AssertEquals("EI_Status", EDIInterchange.Status.Received, interchange.EI_Status);
				AssertEquals("ContainedMessages.Count", 1, interchange.ContainedMessages.Count);
				AssertMessage(interchange.ContainedMessages[0], nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPRF), Messaging.EDIMessageTypeList.Codes.AES, interchange.EI_BodyText, interchange.EI_InterchangeNum);
			});
		}

		protected override DEECustomsMessageRetrievingServiceTask CreateServiceTask() => new DEECustomsMessageRetrievingServiceTask();

		protected override GMDCustomsMessagingServiceTestHelperData SetupDataForTesting()
		{
			var interchange = CreateInterchange(nameof(CargoWise.Customs.DE.MessageDefinitions.AESVersion3_0.DEXPRF));
			return new GMDCustomsMessagingServiceTestHelperData()
			{
				InterchangePK = interchange.PK
			};
		}

		void AssertMessage(EDIMessage message, ZString aplicationReference, ZString messageType, ZString bodyText, ZString messageNum)
		{
			AssertEquals("message.EM_ApplicationCode", EDIMessage.ApplicationCodes.DECustomsAesSystem, message.EM_ApplicationCode);
			AssertEquals("message.EM_ApplicationReference", aplicationReference, message.EM_ApplicationReference);
			AssertEquals("message.EM_ReceiveTransmit", EDIMessage.Direction.Receive, message.EM_ReceiveTransmit);
			AssertEquals("message.EM_MessageNum", messageNum, message.EM_MessageNum);
			AssertEquals("message.EM_MessageType", messageType, message.EM_MessageType);
			AssertEquals("message.EM_MessageSubType", Messaging.ExportMessageSubTypeList.Codes.EXP, message.EM_MessageSubType);
			AssertEquals("message.EM_MessageText", bodyText, message.EM_MessageText);
			AssertEquals("message.EM_Status", EDIMessage.Status.Queued, message.EM_Status);
		}

		EDIInterchange CreateInterchange(ZString messageType)
		{
			var interchange = Factory.New<EDIInterchange>();
			interchange.EI_ApplicationCode = EDIInterchange.ApplicationCodes.GenericMessageDelivery;
			interchange.EI_InterchangeType = EDIInterchange.ApplicationCodes.DECustomsAesSystem;
			interchange.EI_ReceiveTransmit = EDIInterchange.Direction.Receive;
			interchange.EI_From = "DEEAES";
			interchange.EI_To = "KDSER";
			interchange.EI_Status = EDIInterchange.Status.Queued;
			interchange.EI_BodyText = $@"<?xml version=""1.0"" encoding=""utf-8""?><DECustomsData>
<LogbookTime>2019-11-20T15:29:00.2347048+02:00</LogbookTime>
<CustomsData>
<{messageType}>
  <preparationDateAndTime>2000-01-01T00:00:00</preparationDateAndTime>
  <messageIdentification>0000000000</messageIdentification>
  <messageGroup>EXP</messageGroup>
  <messageType>{messageType}</messageType>
  <messageVersion>F.1.0</messageVersion>
  <correlationIdentifier>CUSCAN58750000000518175240519105043</correlationIdentifier>
  <MessageSender>
    <referenceNumber>DE000000</referenceNumber>
  </MessageSender>
  <MessageRecipient>
    <identificationNumber>AAA</identificationNumber>
    <subsidiaryNumber>0000</subsidiaryNumber>
  </MessageRecipient>
  <ExportOperation>
    <MRN>00DE000000000000E0</MRN>
    <declarationType>AA</declarationType>
    <additionalDeclarationType>A</additionalDeclarationType>
    <exportDeclarationType>00000100</exportDeclarationType>
    <partyConstellation>0000</partyConstellation>
    <declarationRecordationDateAndTime>2000-01-01T00:00:00</declarationRecordationDateAndTime>
    <declarationAcceptanceDateAndTime>2000-01-01T00:00:00</declarationAcceptanceDateAndTime>
    <releaseDateAndTime>2000-01-01T00:00:00</releaseDateAndTime>
    <security>1</security>
  </ExportOperation>
  <CustomsOfficeOfExport>
    <referenceNumber>DE000001</referenceNumber>
  </CustomsOfficeOfExport>
  <ContractualPartner>
    <name>Name2</name>
    <Address>
      <streetAndNumber>Token1</streetAndNumber>
      <postcode>Token1</postcode>
      <city>Token1</city>
      <country>DE</country>
    </Address>
  </ContractualPartner>
  <Exporter>
    <name>Name1</name>
    <Address>
      <streetAndNumber>Token1</streetAndNumber>
      <postcode>Token1</postcode>
      <city>Token1</city>
      <country>DE</country>
    </Address>
  </Exporter>
  <Declarant>
    <name>Name3</name>
    <Address>
      <streetAndNumber>Token1</streetAndNumber>
      <postcode>Token1</postcode>
      <city>Token1</city>
      <country>DE</country>
    </Address>
  </Declarant>
  <GoodsShipment>
    <countryOfDestination>DE</countryOfDestination>
    <Consignment>
      <grossMass>0.001</grossMass>
    </Consignment>
    <GoodsItem>
      <sequenceNumber>1</sequenceNumber>
      <declarationGoodsItemNumber>1</declarationGoodsItemNumber>
      <Procedure>
        <requestedProcedure>11</requestedProcedure>
        <previousProcedure>22</previousProcedure>
      </Procedure>
      <Commodity>
        <descriptionOfGoods>Token1</descriptionOfGoods>
        <CommodityCode>
          <harmonizedSystemSubHeadingCode>123456</harmonizedSystemSubHeadingCode>
          <combinedNomenclatureCode>33</combinedNomenclatureCode>
        </CommodityCode>
        <GoodsMeasure>
          <grossMass>0.000</grossMass>
          <netMass>0.000001</netMass>
        </GoodsMeasure>
      </Commodity>
      <Packaging>
        <sequenceNumber>1</sequenceNumber>
        <typeOfPackages>T1</typeOfPackages>
      </Packaging>
    </GoodsItem>
  </GoodsShipment>
</{messageType}>
</CustomsData>
</DECustomsData>
";
			return interchange;
		}
	}
}
