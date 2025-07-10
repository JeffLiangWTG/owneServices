using System.Data;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Messaging.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.GB.CDS.Testing
{
	[TestedType(typeof(CDSEDIMessage))]
	class CDSEDIMessageTests : EnterpriseBusinessObjectTestCase
	{
		public void TestSetDefaultValues()
		{
			var message = Factory.New<CDSEDIMessage>();
			AssertEquals(EDIMessage.ApplicationCodes.GbCustomsDeclarationServices, message.EM_ApplicationCode);
		}

		public void TestInterchange()
		{
			var interchange = Factory.New<CDSInterchange>();
			var message = (CDSEDIMessage)interchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			AssertType<CDSInterchange>(message.Interchange);
		}

		public void TestOriginalOutgoingMessage()
		{
			var outgoingInterchange = Factory.New<CDSInterchange>();
			outgoingInterchange.EI_From = "CCSUK";
			outgoingInterchange.EI_To = "WISETECHGLOBAL";
			var outgoingMessage = (CDSEDIMessage)outgoingInterchange.ContainedMessages.AddNew(typeof(CDSEDIMessage));
			Factory.Save();

			var inboundInterchange = Factory.New<CDSInterchange>();
			inboundInterchange.EI_From = "CCSUK";
			inboundInterchange.EI_To = "WISETECHGLOBAL";
			inboundInterchange.EI_FooterText = $@"<?ccsuk senderid=""CCSUK"" recipientid=""WISETECHGLOBAL"" ext-correlation-id=""{outgoingInterchange.PK}"" x-conversation-id=""77685C3D14D53E25E0540003BA9676AB"" ?>";
			var inboundMessage = (CDSSynchronousResponseEDIMessage)inboundInterchange.ContainedMessages.AddNew(typeof(CDSSynchronousResponseEDIMessage));
			AssertEquals(outgoingMessage, inboundMessage.OriginalOutgoingMessage);
		}

		public void TestEhubTrackingId()
		{
			var msg = Factory.New<CDSEDIMessageForTesting>();
			AssertEquals(ZGuid.Empty, msg.EHubTrackingIdExposed);
		}

		public void TestIndentedXMLMessage()
		{
			var message = Factory.New<CDSEDIMessage>();
			var input = "<Node><Node2><Node3>aaa</Node3></Node2></Node>";
			message.EM_MessageText = input;

			var expected = @"<Node>
  <Node2>
    <Node3>aaa</Node3>
  </Node2>
</Node>";

			AssertEquals(expected, message.EM_MessageTextIndentedXml);
		}

		public void TestIndentedXMLMessage_Example()
		{
			var message = Factory.New<CDSEDIMessage>();
			var input = @"<p:DeclarationFullResponse xsi:schemaLocation=""http://gov.uk/customs/FullDeclarationDataRetrievalService"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:p4=""urn:un:unece:uncefact:data:standard:UnqualifiedDataType:6"" xmlns:p3=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:p2=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:p1=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:p=""http://gov.uk/customs/FullDeclarationDataRetrievalService""><p:FullDeclarationDataDetails><p:HighLevelSummaryDetails><p:MRN>23GBBHJJNTZ1M3VAR1</p:MRN><p:LRN>HYEDUKCM20000000003424</p:LRN><p:VersionID>1</p:VersionID><p:GoodsLocationCode>GBAUABDABDABM</p:GoodsLocationCode><p:CreatedDateTime><p:DateTimeString formatCode=""304"">20231017095600Z</p:DateTimeString></p:CreatedDateTime><p:AcceptanceDateTime><p:DateTimeString formatCode=""304"">20231017095600Z</p:DateTimeString></p:AcceptanceDateTime><p:FinalisedDateTime><p:DateTimeString formatCode=""304"">20231017100608Z</p:DateTimeString></p:FinalisedDateTime></p:HighLevelSummaryDetails><p:AccountDetails><p:MOP>1</p:MOP><p:DutyTaxFee><p:TaxType>A00</p:TaxType><p:Amount>1200.0</p:Amount><p:CoverageAmountType>7</p:CoverageAmountType></p:DutyTaxFee></p:AccountDetails><p:GeneratedConsignmentDetails><p:ROE>6</p:ROE><p:StatusOfEntry-ICS>22</p:StatusOfEntry-ICS><p:GoodsArrivalDateTime><p:DateTimeString formatCode=""304"">20231017095603Z</p:DateTimeString></p:GoodsArrivalDateTime><p:SubmitterID>GB048834222514</p:SubmitterID><p:StatisticalValue currencyID=""GBP"">0.0</p:StatisticalValue></p:GeneratedConsignmentDetails><p:GeneratedItemLevelConsignmentDetails><p:ItemNumber>1</p:ItemNumber><p:VATValue currencyID=""GBP"">0.0</p:VATValue><p:VATRate unitCode=""P1"">20.0</p:VATRate><p:CustomsValue currencyID=""GBP"">10000.0</p:CustomsValue><p:DutyTaxFee><p:TaxBase unitCode=""GBP"">10000.0</p:TaxBase><p:TaxType>A00</p:TaxType><p:Amount currencyID=""GBP"">1200.0</p:Amount><p:TaxRate unitCode=""P1"">12.0</p:TaxRate></p:DutyTaxFee><p:DutyTaxFee><p:TaxBase unitCode=""GBP"">11200.0</p:TaxBase><p:TaxType>B00</p:TaxType><p:Amount currencyID=""GBP"">0.0</p:Amount><p:TaxRate unitCode=""P1"">20.0</p:TaxRate></p:DutyTaxFee></p:GeneratedItemLevelConsignmentDetails><p:FullDeclarationObject><p:Declaration><p:AcceptanceDateTime><p:DateTimeString formatCode=""304"">20231017095600Z</p:DateTimeString></p:AcceptanceDateTime><p:FunctionCode>9</p:FunctionCode><p:FunctionalReferenceID>HYEDUKCM20000000003424</p:FunctionalReferenceID><p:TypeCode>IMA</p:TypeCode><p:ProcedureCategory>H1</p:ProcedureCategory><p:GoodsItemQuantity>1</p:GoodsItemQuantity><p:TotalPackageQuantity>10</p:TotalPackageQuantity><p:BorderTransportMeans><p:RegistrationNationalityCode>GB</p:RegistrationNationalityCode><p:ModeCode>4</p:ModeCode></p:BorderTransportMeans><p:Declarant><p:ID>GB896458895015</p:ID><p:Address /></p:Declarant><p:Exporter><p:Name>CHUN TAT ENTERPRISES CO</p:Name><p:Address><p:CityName>KWAI CHUNG</p:CityName><p:CountryCode>HK</p:CountryCode><p:Line>10/F, CHUNG LAM IND., BLDG., 40-42 </p:Line><p:PostcodeID>1111</p:PostcodeID></p:Address></p:Exporter><p:GoodsShipment><p:TransactionNatureCode>11</p:TransactionNatureCode><p:Consignment><p:ContainerCode>0</p:ContainerCode><p:ArrivalTransportMeans><p:ID>A0M VOL</p:ID><p:IdentificationTypeCode>40</p:IdentificationTypeCode></p:ArrivalTransportMeans><p:GoodsLocation><p:Name>ABDABDABM</p:Name><p:TypeCode>A</p:TypeCode><p:Address><p:TypeCode>U</p:TypeCode><p:CountryCode>GB</p:CountryCode></p:Address></p:GoodsLocation></p:Consignment><p:Destination><p:CountryCode>GB</p:CountryCode></p:Destination><p:DomesticDutyTaxParty><p:SequenceNumeric>1</p:SequenceNumeric><p:ID>GB575456994</p:ID><p:RoleCode>FR1</p:RoleCode></p:DomesticDutyTaxParty><p:GovernmentAgencyGoodsItem><p:SequenceNumeric>1</p:SequenceNumeric><p:StatisticalValueAmount currencyID=""GBP"">0.0</p:StatisticalValueAmount><p:TransactionNatureCode>11</p:TransactionNatureCode><p:AdditionalDocument><p:SequenceNumeric>1</p:SequenceNumeric><p:CategoryCode>N</p:CategoryCode><p:ID>aaaaa</p:ID><p:TypeCode>935</p:TypeCode><p:LPCOExemptionCode>AC</p:LPCOExemptionCode></p:AdditionalDocument><p:AdditionalDocument><p:SequenceNumeric>2</p:SequenceNumeric><p:CategoryCode>9</p:CategoryCode><p:ID>AAAAA</p:ID><p:Name>SEE ATTACHED WORKSHEET B60005991</p:Name><p:TypeCode>WKS</p:TypeCode><p:LPCOExemptionCode>JP</p:LPCOExemptionCode></p:AdditionalDocument><p:AdditionalInformation><p:SequenceNumeric>1</p:SequenceNumeric><p:StatementCode>00500</p:StatementCode><p:StatementDescription>IMPORTER</p:StatementDescription></p:AdditionalInformation><p:Commodity><p:Description>GGGGGGG</p:Description><p:Classification><p:ID>61071100</p:ID><p:IdentificationTypeCode>TSP</p:IdentificationTypeCode></p:Classification><p:Classification><p:ID>00</p:ID><p:IdentificationTypeCode>TRC</p:IdentificationTypeCode></p:Classification><p:DutyTaxFee><p:SequenceNumeric>1</p:SequenceNumeric><p:DutyRegimeCode>100</p:DutyRegimeCode><p:Payment /></p:DutyTaxFee><p:GoodsMeasure><p:GrossMassMeasure>10000.0</p:GrossMassMeasure><p:NetNetWeightMeasure>10000.0</p:NetNetWeightMeasure><p:TariffQuantity>10.0</p:TariffQuantity></p:GoodsMeasure><p:InvoiceLine><p:ItemChargeAmount currencyID=""GBP"">10000.0</p:ItemChargeAmount></p:InvoiceLine></p:Commodity><p:ExportCountry><p:ID>HK</p:ID></p:ExportCountry><p:GovernmentProcedure><p:CurrentCode>40</p:CurrentCode><p:PreviousCode>00</p:PreviousCode></p:GovernmentProcedure><p:GovernmentProcedure><p:CurrentCode>000</p:CurrentCode></p:GovernmentProcedure><p:Origin><p:CountryCode>US</p:CountryCode><p:TypeCode>1</p:TypeCode></p:Origin><p:Packaging><p:SequenceNumeric>1</p:SequenceNumeric><p:MarksNumbersID>sdsdsds</p:MarksNumbersID><p:QuantityQuantity>8</p:QuantityQuantity><p:TypeCode>TB</p:TypeCode></p:Packaging><p:ValuationAdjustment><p:AdditionCode>0000</p:AdditionCode></p:ValuationAdjustment></p:GovernmentAgencyGoodsItem><p:Importer><p:ID>GB896458895015</p:ID><p:Address /></p:Importer><p:TradeTerms><p:ConditionCode>CIF</p:ConditionCode></p:TradeTerms><p:UCR><p:TraderAssignedReferenceID>3GB896458895015-B60007163/3</p:TraderAssignedReferenceID></p:UCR></p:GoodsShipment></p:Declaration></p:FullDeclarationObject></p:FullDeclarationDataDetails></p:DeclarationFullResponse>";
			message.EM_MessageText = input;

			var expected = @"<p:DeclarationFullResponse xsi:schemaLocation=""http://gov.uk/customs/FullDeclarationDataRetrievalService"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:p4=""urn:un:unece:uncefact:data:standard:UnqualifiedDataType:6"" xmlns:p3=""urn:wco:datamodel:WCO:Declaration_DS:DMS:2"" xmlns:p2=""urn:wco:datamodel:WCO:DEC-DMS:2"" xmlns:p1=""urn:wco:datamodel:WCO:Response_DS:DMS:2"" xmlns:p=""http://gov.uk/customs/FullDeclarationDataRetrievalService"">
  <p:FullDeclarationDataDetails>
    <p:HighLevelSummaryDetails>
      <p:MRN>23GBBHJJNTZ1M3VAR1</p:MRN>
      <p:LRN>HYEDUKCM20000000003424</p:LRN>
      <p:VersionID>1</p:VersionID>
      <p:GoodsLocationCode>GBAUABDABDABM</p:GoodsLocationCode>
      <p:CreatedDateTime>
        <p:DateTimeString formatCode=""304"">20231017095600Z</p:DateTimeString>
      </p:CreatedDateTime>
      <p:AcceptanceDateTime>
        <p:DateTimeString formatCode=""304"">20231017095600Z</p:DateTimeString>
      </p:AcceptanceDateTime>
      <p:FinalisedDateTime>
        <p:DateTimeString formatCode=""304"">20231017100608Z</p:DateTimeString>
      </p:FinalisedDateTime>
    </p:HighLevelSummaryDetails>
    <p:AccountDetails>
      <p:MOP>1</p:MOP>
      <p:DutyTaxFee>
        <p:TaxType>A00</p:TaxType>
        <p:Amount>1200.0</p:Amount>
        <p:CoverageAmountType>7</p:CoverageAmountType>
      </p:DutyTaxFee>
    </p:AccountDetails>
    <p:GeneratedConsignmentDetails>
      <p:ROE>6</p:ROE>
      <p:StatusOfEntry-ICS>22</p:StatusOfEntry-ICS>
      <p:GoodsArrivalDateTime>
        <p:DateTimeString formatCode=""304"">20231017095603Z</p:DateTimeString>
      </p:GoodsArrivalDateTime>
      <p:SubmitterID>GB048834222514</p:SubmitterID>
      <p:StatisticalValue currencyID=""GBP"">0.0</p:StatisticalValue>
    </p:GeneratedConsignmentDetails>
    <p:GeneratedItemLevelConsignmentDetails>
      <p:ItemNumber>1</p:ItemNumber>
      <p:VATValue currencyID=""GBP"">0.0</p:VATValue>
      <p:VATRate unitCode=""P1"">20.0</p:VATRate>
      <p:CustomsValue currencyID=""GBP"">10000.0</p:CustomsValue>
      <p:DutyTaxFee>
        <p:TaxBase unitCode=""GBP"">10000.0</p:TaxBase>
        <p:TaxType>A00</p:TaxType>
        <p:Amount currencyID=""GBP"">1200.0</p:Amount>
        <p:TaxRate unitCode=""P1"">12.0</p:TaxRate>
      </p:DutyTaxFee>
      <p:DutyTaxFee>
        <p:TaxBase unitCode=""GBP"">11200.0</p:TaxBase>
        <p:TaxType>B00</p:TaxType>
        <p:Amount currencyID=""GBP"">0.0</p:Amount>
        <p:TaxRate unitCode=""P1"">20.0</p:TaxRate>
      </p:DutyTaxFee>
    </p:GeneratedItemLevelConsignmentDetails>
    <p:FullDeclarationObject>
      <p:Declaration>
        <p:AcceptanceDateTime>
          <p:DateTimeString formatCode=""304"">20231017095600Z</p:DateTimeString>
        </p:AcceptanceDateTime>
        <p:FunctionCode>9</p:FunctionCode>
        <p:FunctionalReferenceID>HYEDUKCM20000000003424</p:FunctionalReferenceID>
        <p:TypeCode>IMA</p:TypeCode>
        <p:ProcedureCategory>H1</p:ProcedureCategory>
        <p:GoodsItemQuantity>1</p:GoodsItemQuantity>
        <p:TotalPackageQuantity>10</p:TotalPackageQuantity>
        <p:BorderTransportMeans>
          <p:RegistrationNationalityCode>GB</p:RegistrationNationalityCode>
          <p:ModeCode>4</p:ModeCode>
        </p:BorderTransportMeans>
        <p:Declarant>
          <p:ID>GB896458895015</p:ID>
          <p:Address />
        </p:Declarant>
        <p:Exporter>
          <p:Name>CHUN TAT ENTERPRISES CO</p:Name>
          <p:Address>
            <p:CityName>KWAI CHUNG</p:CityName>
            <p:CountryCode>HK</p:CountryCode>
            <p:Line>10/F, CHUNG LAM IND., BLDG., 40-42 </p:Line>
            <p:PostcodeID>1111</p:PostcodeID>
          </p:Address>
        </p:Exporter>
        <p:GoodsShipment>
          <p:TransactionNatureCode>11</p:TransactionNatureCode>
          <p:Consignment>
            <p:ContainerCode>0</p:ContainerCode>
            <p:ArrivalTransportMeans>
              <p:ID>A0M VOL</p:ID>
              <p:IdentificationTypeCode>40</p:IdentificationTypeCode>
            </p:ArrivalTransportMeans>
            <p:GoodsLocation>
              <p:Name>ABDABDABM</p:Name>
              <p:TypeCode>A</p:TypeCode>
              <p:Address>
                <p:TypeCode>U</p:TypeCode>
                <p:CountryCode>GB</p:CountryCode>
              </p:Address>
            </p:GoodsLocation>
          </p:Consignment>
          <p:Destination>
            <p:CountryCode>GB</p:CountryCode>
          </p:Destination>
          <p:DomesticDutyTaxParty>
            <p:SequenceNumeric>1</p:SequenceNumeric>
            <p:ID>GB575456994</p:ID>
            <p:RoleCode>FR1</p:RoleCode>
          </p:DomesticDutyTaxParty>
          <p:GovernmentAgencyGoodsItem>
            <p:SequenceNumeric>1</p:SequenceNumeric>
            <p:StatisticalValueAmount currencyID=""GBP"">0.0</p:StatisticalValueAmount>
            <p:TransactionNatureCode>11</p:TransactionNatureCode>
            <p:AdditionalDocument>
              <p:SequenceNumeric>1</p:SequenceNumeric>
              <p:CategoryCode>N</p:CategoryCode>
              <p:ID>aaaaa</p:ID>
              <p:TypeCode>935</p:TypeCode>
              <p:LPCOExemptionCode>AC</p:LPCOExemptionCode>
            </p:AdditionalDocument>
            <p:AdditionalDocument>
              <p:SequenceNumeric>2</p:SequenceNumeric>
              <p:CategoryCode>9</p:CategoryCode>
              <p:ID>AAAAA</p:ID>
              <p:Name>SEE ATTACHED WORKSHEET B60005991</p:Name>
              <p:TypeCode>WKS</p:TypeCode>
              <p:LPCOExemptionCode>JP</p:LPCOExemptionCode>
            </p:AdditionalDocument>
            <p:AdditionalInformation>
              <p:SequenceNumeric>1</p:SequenceNumeric>
              <p:StatementCode>00500</p:StatementCode>
              <p:StatementDescription>IMPORTER</p:StatementDescription>
            </p:AdditionalInformation>
            <p:Commodity>
              <p:Description>GGGGGGG</p:Description>
              <p:Classification>
                <p:ID>61071100</p:ID>
                <p:IdentificationTypeCode>TSP</p:IdentificationTypeCode>
              </p:Classification>
              <p:Classification>
                <p:ID>00</p:ID>
                <p:IdentificationTypeCode>TRC</p:IdentificationTypeCode>
              </p:Classification>
              <p:DutyTaxFee>
                <p:SequenceNumeric>1</p:SequenceNumeric>
                <p:DutyRegimeCode>100</p:DutyRegimeCode>
                <p:Payment />
              </p:DutyTaxFee>
              <p:GoodsMeasure>
                <p:GrossMassMeasure>10000.0</p:GrossMassMeasure>
                <p:NetNetWeightMeasure>10000.0</p:NetNetWeightMeasure>
                <p:TariffQuantity>10.0</p:TariffQuantity>
              </p:GoodsMeasure>
              <p:InvoiceLine>
                <p:ItemChargeAmount currencyID=""GBP"">10000.0</p:ItemChargeAmount>
              </p:InvoiceLine>
            </p:Commodity>
            <p:ExportCountry>
              <p:ID>HK</p:ID>
            </p:ExportCountry>
            <p:GovernmentProcedure>
              <p:CurrentCode>40</p:CurrentCode>
              <p:PreviousCode>00</p:PreviousCode>
            </p:GovernmentProcedure>
            <p:GovernmentProcedure>
              <p:CurrentCode>000</p:CurrentCode>
            </p:GovernmentProcedure>
            <p:Origin>
              <p:CountryCode>US</p:CountryCode>
              <p:TypeCode>1</p:TypeCode>
            </p:Origin>
            <p:Packaging>
              <p:SequenceNumeric>1</p:SequenceNumeric>
              <p:MarksNumbersID>sdsdsds</p:MarksNumbersID>
              <p:QuantityQuantity>8</p:QuantityQuantity>
              <p:TypeCode>TB</p:TypeCode>
            </p:Packaging>
            <p:ValuationAdjustment>
              <p:AdditionCode>0000</p:AdditionCode>
            </p:ValuationAdjustment>
          </p:GovernmentAgencyGoodsItem>
          <p:Importer>
            <p:ID>GB896458895015</p:ID>
            <p:Address />
          </p:Importer>
          <p:TradeTerms>
            <p:ConditionCode>CIF</p:ConditionCode>
          </p:TradeTerms>
          <p:UCR>
            <p:TraderAssignedReferenceID>3GB896458895015-B60007163/3</p:TraderAssignedReferenceID>
          </p:UCR>
        </p:GoodsShipment>
      </p:Declaration>
    </p:FullDeclarationObject>
  </p:FullDeclarationDataDetails>
</p:DeclarationFullResponse>";

			AssertEquals(expected, message.EM_MessageTextIndentedXml);
		}

		public void TestEmptyMessageDeserialization()
		{
			var message = Factory.New<CDSCancelDeclarationEDIMessage>();
			message.EM_MessageText = "";
			var messageDataObject = message.MessageDataObject;
			AssertNull(messageDataObject);

			var message2 = Factory.New<CDSCancelDeclarationEDIMessage>();
			message2.EM_MessageText = null;
			var messageDataObject2 = message2.MessageDataObject;
			AssertNull(messageDataObject2);

			var message3 = Factory.New<CDSCancelDeclarationEDIMessage>();
			message3.EM_MessageText = "   ";
			var messageDataObject3 = message3.MessageDataObject;
			AssertNull(messageDataObject3);
		}
	}

	[TestedType(typeof(CDSEDIMessage<Response>))]
	class CDSEDIMessageGenericTests : EnterpriseBusinessObjectTestCase
	{
	}

	class CDSEDIMessageForTesting : CDSEDIMessage
	{
		public CDSEDIMessageForTesting(BusinessObjectFactory factory, DataRow row) : base(factory, row)
		{
		}

		public ZGuid EHubTrackingIdExposed => base.EHubTrackingId;
	}
}
