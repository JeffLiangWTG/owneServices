using System.Text.RegularExpressions;
using CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.GB.Business.Declaration;
using Enterprise.Customs.GB.Business.Testing;
using Enterprise.Customs.GB.CDS.Messaging.Testing;
using Enterprise.Customs.Universal.Testing;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;

namespace Enterprise.Customs.GB.CDS.Testing
{
	public class CDSResponseMessagePrettierTests : TestCaseWithFactory
	{
		public void TestMakeHumanReadable()
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_MessageText =
				@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>123</FunctionalReferenceID>
    <ID>18GB123456789</ID>
  </Declaration>
</Response>";

			AssertEquals(
				"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Declaration has been legally accepted</H3>" +
				"<p><strong>Function Code: </strong>01-ACC<br><strong>Old CHIEF Report Code: </strong>E2<br><strong>MRN: </strong>18GB123456789<br><strong>LRN: </strong>123<br><strong>Issued Date: </strong>2018-07-27 12:12</p>",
				message.EM_MessageInterpretation);
		}

		public void TestMakeHumanReadable_REJ_Cancelled()
		{
			var entry = Factory.New<CusEntryHeader>();
			entry.CH_EntryStatus = EDIMessageStatusList.Codes.Cancelled;

			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_LinkedObject = entry;
			message.EM_MessageText =
				@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>03</FunctionCode>
  <FunctionalReferenceID>REJ-Cancelled</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Declaration>
    <FunctionalReferenceID>123</FunctionalReferenceID>
    <ID>18GB123456789</ID>
  </Declaration>
</Response>";

			AssertContains(
				"<H3>Response from CDS: Pre-lodged declaration canceled OK</H3><p><strong>Function Code: </strong>03-REJ<br>",
				message.EM_MessageInterpretation);
		}

		public void TestMakeHumanReadable_REJ_Rejected()
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_MessageText =
				@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>03</FunctionCode>
  <FunctionalReferenceID>REJ-Rejected</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20180727121212Z</DateTimeString>
  </IssueDateTime>
  <Error>
    <Description>How do you expect us to process your dodgy messages?</Description>
    <ValidationCode>DODGY101</ValidationCode>
  </Error>
  <Declaration>
    <FunctionalReferenceID>123</FunctionalReferenceID>
    <ID>18GB123456789</ID>
  </Declaration>
</Response>";

			AssertContains(
				"<H3>Response from CDS: Message has been rejected</H3><p><strong>Function Code: </strong>03-REJ<br>",
				message.EM_MessageInterpretation);
		}

		public void TestInterpretationOfErrorWithSequencesHigherThanOne()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "Import_Obligation_REJ";
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var outgoingMessage = (CDSEDIMessage)entry.Messages.AddNew(typeof(CDSEDIMessage));
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			#region sent message
			outgoingMessage.EM_MessageText = @"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>9</FunctionCode>
    <FunctionalReferenceID>HYEDUKMIK0000000000034</FunctionalReferenceID>
    <TypeCode>IMB</TypeCode>
    <GoodsItemQuantity>1</GoodsItemQuantity>
    <TotalGrossMassMeasure>150.000</TotalGrossMassMeasure>
    <TotalPackageQuantity>5</TotalPackageQuantity>
    <Agent>
      <FunctionCode>2</FunctionCode>
    </Agent>
    <AuthorisationHolder>
      <ID>GB387345516000</ID>
      <CategoryCode>SDE</CategoryCode>
    </AuthorisationHolder>
    <Declarant>
      <Name>TTM 5.1 DECLARANT</Name>
      <ID>GB666673196000</ID>
      <Address>
        <CityName>SDFSDF</CityName>
        <CountryCode>GB</CountryCode>
        <Line>JKHSDFSDF</Line>
        <PostcodeID>DFSFDSFSD</PostcodeID>
      </Address>
    </Declarant>
    <Exporter>
      <Name>SKYLINE DISPLAYS INC</Name>
      <Address>
        <CityName>BURNSVILLE</CityName>
        <CountryCode>US</CountryCode>
        <Line>1301 CLIFF ROAD EAST</Line>
        <PostcodeID>30000</PostcodeID>
      </Address>
    </Exporter>
    <GoodsShipment>
      <Consignment>
        <ContainerCode>0</ContainerCode>
        <GoodsLocation>
          <ID>WLALONBTW</ID>
          <TypeCode>A</TypeCode>
          <Address>
            <TypeCode>U</TypeCode>
            <CountryCode>GB</CountryCode>
          </Address>
        </GoodsLocation>
      </Consignment>
      <Destination>
        <CountryCode>GB</CountryCode>
      </Destination>
      <ExportCountry>
        <ID>US</ID>
      </ExportCountry>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <Commodity>
          <Description>NEWSPAPERS</Description>
          <GoodsMeasure>
            <GrossMassMeasure>150</GrossMassMeasure>
            <NetNetWeightMeasure>100</NetNetWeightMeasure>
            <TariffQuantity>1.00000</TariffQuantity>
          </GoodsMeasure>
          <InvoiceLine>
            <ItemChargeAmount currencyID=""GBP"">1.00000</ItemChargeAmount>
          </InvoiceLine>
        </Commodity>
        <CustomsValuation>
          <MethodCode>1</MethodCode>
        </CustomsValuation>
        <ExportCountry>
          <ID>US</ID>
        </ExportCountry>
        <GovernmentProcedure>
          <CurrentCode>40</CurrentCode>
          <PreviousCode>00</PreviousCode>
        </GovernmentProcedure>
        <GovernmentProcedure>
          <CurrentCode>000</CurrentCode>
        </GovernmentProcedure>
        <Origin>
          <CountryCode>US</CountryCode>
          <TypeCode>1</TypeCode>
        </Origin>
        <Packaging>
          <SequenceNumeric>1</SequenceNumeric>
          <MarksNumbersID>red</MarksNumbersID>
          <QuantityQuantity>10</QuantityQuantity>
          <TypeCode>PK</TypeCode>
        </Packaging>
        <PreviousDocument>
          <CategoryCode>Z</CategoryCode>
          <ID>ASDASDDSA</ID>
          <TypeCode>380</TypeCode>
        </PreviousDocument>
        <PreviousDocument>
          <CategoryCode>Z</CategoryCode>
          <ID>INV123</ID>
          <TypeCode>380</TypeCode>
        </PreviousDocument>
        <PreviousDocument>
          <CategoryCode>C</CategoryCode>
          <ID>GBSDEGB387345516000</ID>
          <TypeCode>512</TypeCode>
        </PreviousDocument>
      </GovernmentAgencyGoodsItem>
      <Importer>
        <ID>GB387345516000</ID>
      </Importer>
      <PreviousDocument>
        <CategoryCode>Z</CategoryCode>
        <ID>18GB10IENS45678901</ID>
        <TypeCode>355</TypeCode>
      </PreviousDocument>
      <PreviousDocument>
        <CategoryCode>Z</CategoryCode>
        <ID>BL123456-1</ID>
        <TypeCode>705</TypeCode>
      </PreviousDocument>
      <PreviousDocument>
        <CategoryCode>Z</CategoryCode>
        <ID>9GB666673196000-B00030905</ID>
        <TypeCode>DCR</TypeCode>
      </PreviousDocument>
      <UCR>
        <TraderAssignedReferenceID>9GB666673196000-B00030905</TraderAssignedReferenceID>
      </UCR>
    </GoodsShipment>
  </Declaration>
</MetaData>";
			#endregion

			var incoming = (CDSResponseEDIMessage)entry.Messages.AddNew(typeof(CDSResponseEDIMessage));
			incoming.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incoming.EM_Status = EDIMessage.Status.Queued;
			#region incoming
			incoming.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>03</FunctionCode>
  <FunctionalReferenceID>4d0da426c5c245dfa7a868f28a6c96e7</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190114162641Z</DateTimeString>
  </IssueDateTime>
  <Error>
    <ValidationCode>DMS10020</ValidationCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>67A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>68A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>3</SequenceNumeric>
      <DocumentSectionCode>99A</DocumentSectionCode>
      <TagID>D013</TagID>
    </Pointer>
  </Error> 
  <Error>
    <ValidationCode>DMS10020</ValidationCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>67A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>68A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>3</SequenceNumeric>
      <DocumentSectionCode>99A</DocumentSectionCode>
      <TagID>D019</TagID>
    </Pointer>
  </Error>
<!-- more were in the original but I have snipped them -->
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000000034</FunctionalReferenceID>
    <ID>19GB0JJYW1J0XFGVR8</ID>
    <RejectionDateTime>
      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190114162641Z</DateTimeString>
    </RejectionDateTime>
    <VersionID>1</VersionID>
  </Declaration>
</Response>";
			#endregion

			var interpretation = incoming.EM_MessageInterpretation;
			AssertContains("Should refer to the original value '512' even though it's not the first PreviousDocument but the third",
								 @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Message has been rejected</H3><p><strong>Function Code: </strong>03-REJ<br><strong>Old CHIEF Report Code: </strong>27 (or N3/S3)<br><strong>Declaration Version: </strong>1<br><strong>MRN: </strong>19GB0JJYW1J0XFGVR8<br><strong>LRN: </strong>HYEDUKMIK0000000000034<br><strong>Issued Date: </strong>2019-01-14 16:26</p><p>Error Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Header/Item</td><td>Error Code</td><td>Error Description</td><td>Field Name</td><td>Original Value</td><td>Data Element</td><td>Path (for technical support only)</td></tr><tr><td>Item 1</td><td>DMS10020</td><td>&nbsp;</td><td>Document name, coded</td><td>512</td><td>1/1 & 1/2</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem/PreviousDocument[3]/TypeCode<br>42A/67A/68A[1]/99A[3]/D013[1]</td></tr><tr><td>Item 1</td><td>DMS10020</td><td>&nbsp;</td><td>Previous document type, coded</td><td>512</td><td>2/1</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem/PreviousDocument[3]/TypeCode<br>42A/67A/68A[1]/99A[3]/D019[1]</td></tr></table></p>"
						, interpretation);
		}

		public void TestSelectCorrectOutgoingMessageWhenCalculatingInterpretation()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "Import_Obligation_REJ";
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var outgoingMessage1 = (CDSEDIMessage)entry.Messages.AddNew(typeof(CDSEDIMessage));
			outgoingMessage1.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage1.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			outgoingMessage1.EM_MessageText = @"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <GoodsShipment>
      <TradeTerms>ONE</TradeTerms>
    </GoodsShipment>
  </Declaration>
</MetaData>";

			var incoming1 = (CDSResponseEDIMessage)entry.Messages.AddNew(typeof(CDSResponseEDIMessage));
			incoming1.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incoming1.EM_Status = EDIMessage.Status.Queued;
			incoming1.EM_MessageText = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>03</FunctionCode>
  <Error>
    <ValidationCode>DMS12056</ValidationCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>67A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>22B</DocumentSectionCode>
    </Pointer>
  </Error>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000000011</FunctionalReferenceID>
    <ID>19GB0JICQQN0SFGVR8</ID>
    <RejectionDateTime>
      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190114154129Z</DateTimeString>
    </RejectionDateTime>
    <VersionID>1</VersionID>
  </Declaration>
</Response>";

			outgoingMessage1.EM_ApplicationReference = "XXX123";
			incoming1.EM_ApplicationReference = outgoingMessage1.EM_ApplicationReference;
			AssertContains("ONE", incoming1.EM_MessageInterpretation);

			var outgoingMessage2 = (CDSEDIMessage)entry.Messages.AddNew(typeof(CDSEDIMessage));
			outgoingMessage2.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage2.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			outgoingMessage2.EM_MessageText = outgoingMessage1.EM_MessageText.Replace("ONE", "TWO");

			var incoming2 = (CDSResponseEDIMessage)entry.Messages.AddNew(typeof(CDSResponseEDIMessage));
			incoming2.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incoming2.EM_Status = EDIMessage.Status.Queued;

			outgoingMessage2.EM_ApplicationReference = "YYY456";
			incoming2.EM_ApplicationReference = outgoingMessage2.EM_ApplicationReference;
			incoming2.EM_MessageText = incoming1.EM_MessageText;  // Identical rejections should have different interpretations

			AssertContains("ONE", incoming1.EM_MessageInterpretation);
			AssertContains("TWO", incoming2.EM_MessageInterpretation);
			AssertContains("ONE", incoming1.EM_MessageInterpretation);
		}

		public void TestMakeHumanReadable_InterpretationOnlyIfNodeIsPresent()
		{
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "Import_Obligation_REJ";
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;

			var entryLine2 = entry.AllEntryLines.AddNew();
			entryLine2.CL_LineNumber = 3;
			var inv2 = dec.Invoices.AddNew();
			var invLine2 = inv2.InvoiceLines.AddNew();
			invLine2.JI_CL = entryLine2.PK;

			var entryLine3 = entry.AllEntryLines.AddNew();
			entryLine3.CL_LineNumber = 4;
			var inv3 = dec.Invoices.AddNew();
			var invLine3 = inv3.InvoiceLines.AddNew();
			invLine3.JI_CL = entryLine3.PK;

			var declarationEdiMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			declarationEdiMessage.EM_MessageText = friendlyCodeWithPointerTest.GetRequestMessageXmlContent();
			entry.Messages.Add(declarationEdiMessage);
			Factory.Save();

			AssertMakeHumanReadable_InterpretationOnlyIfNodeIsPresent(entry, false);
			AssertMakeHumanReadable_InterpretationOnlyIfNodeIsPresent(entry, true);
		}

		void AssertMakeHumanReadable_InterpretationOnlyIfNodeIsPresent(CusEntryHeader entry, bool needToShowAssessedAmount)
		{
			var responseEdiMessage = (CDSResponseEDIMessage)entry.Messages.AddNew(typeof(CDSResponseEDIMessage));
			responseEdiMessage.EM_MessageText = new Response
			{
				FunctionCode = new ResponseFunctionCodeType
				{
					Value = "03"
				},
				FunctionalReferenceID = new ResponseFunctionalReferenceIDType
				{
					Value = "c41cb7554783489c94e24939cb1ccf51"
				},
				Bank = new ResponseBank
				{
					ID = new BankAccountIdentificationIDType
					{
						Value = "GB25CITI08320011963155"
					},
					ReferenceID = new BankReferenceIDType
					{
						Value = "CITIGB2LLON"
					}
				},
				Error = new ResponseError[]
				{
					new ResponseError
					{
						ValidationCode = new ErrorValidationCodeType
						{
							Value = "DMS1001"
						},
						Pointer = new ResponseErrorPointer[]
						{
							new ResponseErrorPointer
							{
								DocumentSectionCode = new PointerDocumentSectionCodeType1
								{
									Value = "42A"
								}
							},
							new ResponseErrorPointer
							{
								SequenceNumeric = 1,
								SequenceNumericSpecified = true,
								DocumentSectionCode = new PointerDocumentSectionCodeType1
								{
									Value = "67A"
								}
							},
							new ResponseErrorPointer
							{
								SequenceNumeric = 4,
								SequenceNumericSpecified = true,
								DocumentSectionCode = new PointerDocumentSectionCodeType1
								{
									Value = "68A"
								}
							},
							new ResponseErrorPointer
							{
								DocumentSectionCode = new PointerDocumentSectionCodeType1
								{
									Value = "23A"
								}
							},
							new ResponseErrorPointer
							{
								TagID = new PointerTagIDType1
								{
									Value = "128"
								},
								DocumentSectionCode = new PointerDocumentSectionCodeType1
								{
									Value = "65A"
								}
							}
						}
					}
				},
				Status = new ResponseStatus[]
				{
					new ResponseStatus
					{
						NameCode = new StatusNameCodeType
						{
							Value = "39"
						},
						EffectiveDateTime = new StatusEffectiveDateTimeType
						{
							Item = new StatusEffectiveDateTimeTypeDateTimeString
							{
								formatCode = FormatCodeType.Item304,
								Value = "20181222115304Z"
							}
						}
					}
				},
				Amendment = new ResponseAmendment[]
				{
					new ResponseAmendment
					{
						ChangeReasonCode = new AmendmentChangeReasonCodeType1
						{
							Value = "AMD"
						},
						Pointer = new ResponseAmendmentPointer[]
						{
							new ResponseAmendmentPointer
							{
								DocumentSectionCode = new PointerDocumentSectionCodeType1
								{
									Value = "42A"
								}
							},
							new ResponseAmendmentPointer
							{
								SequenceNumeric = 1,
								DocumentSectionCode = new PointerDocumentSectionCodeType1
								{
									Value = "15A"
								},
								TagID = new PointerTagIDType1
								{
									Value = "T014"
								}
							}
						}
					},
				},
				AdditionalInformation = new ResponseAdditionalInformation[]
				{
					new ResponseAdditionalInformation
					{
						StatementCode = new AdditionalInformationStatementCodeType1
						{
							Value = "A2"
						},
						StatementTypeCode = new AdditionalInformationStatementTypeCodeType1
						{
							Value = "AFB"
						},
						StatementDescription = new AdditionalInformationStatementDescriptionTextType1
						{
							Value = "BlaBla"
						},
						LimitDateTime = new AdditionalInformationLimitDateTimeType
						{
							Item = new AdditionalInformationLimitDateTimeTypeDateTimeString
							{
								formatCode = FormatCodeType.Item304,
								Value = "20181211115304Z"
							}
						}
					}
				},
				Declaration = new ResponseDeclaration
				{
					FunctionalReferenceID = new DeclarationFunctionalReferenceIDType1
					{
						Value = "Import_Obligation_REJ"
					},
					ID = new DeclarationIdentificationIDType1
					{
						Value = "18GBJCUDI9ADRHWD54"
					},
					VersionID = new DeclarationVersionIDType
					{
						Value = "2"
					},
					GoodsShipment = new ResponseDeclarationGovernmentAgencyGoodsItem[]
					{
						new ResponseDeclarationGovernmentAgencyGoodsItem
						{
							SequenceNumeric = 1,
							Commodity = new ResponseDeclarationGovernmentAgencyGoodsItemDutyTaxFee[]
							{
								new ResponseDeclarationGovernmentAgencyGoodsItemDutyTaxFee
								{
									AdValoremTaxBaseAmount = new DutyTaxFeeAdValoremTaxBaseAmountType1
									{
										Value = 5000m
									},
									DeductAmount = new DutyTaxFeeDeductAmountType1
									{
										Value = 0.00m,
									},
									DutyRegimeCode = new DutyTaxFeeDutyRegimeCodeType1
									{
										Value = "100"
									},
									TaxRateNumeric = 9.70m,
									TypeCode = new DutyTaxFeeTypeCodeType1
									{
										Value = "A00"
									},
									Payment = new ResponseDeclarationGovernmentAgencyGoodsItemDutyTaxFeePayment
									{
										TaxAssessedAmount = new PaymentTaxAssessedAmountType1
										{
											Value = 97.00m
										},
										PaymentAmount = new PaymentPaymentAmountType1
										{
											Value = 97.00m
										}
									}
								},
								new ResponseDeclarationGovernmentAgencyGoodsItemDutyTaxFee
								{
									DeductAmount = new DutyTaxFeeDeductAmountType1
									{
										Value = 0.00m,
									},
									DutyRegimeCode = new DutyTaxFeeDutyRegimeCodeType1
									{
										Value = "100"
									},
									TaxRateNumeric = 20.00m,
									TypeCode = new DutyTaxFeeTypeCodeType1
									{
										Value = "B00"
									},
									Payment = new ResponseDeclarationGovernmentAgencyGoodsItemDutyTaxFeePayment
									{
										TaxAssessedAmount = new PaymentTaxAssessedAmountType1
										{
											Value = needToShowAssessedAmount ? 219.20m : 219.40m
										},
										PaymentAmount = new PaymentPaymentAmountType1
										{
											Value = 219.40m
										}
									}
								},
							}
						}
					}
				},
				Declaration1 = new CargoWise.Customs.GB.MessageDefinitions.CDS.DataFileSchema.Version1_0.MetaData.Declaration
				{
					BorderTransportMeans = new DeclarationBorderTransportMeans
					{
						RegistrationNationalityCode = new BorderTransportMeansRegistrationNationalityCodeType
						{
							Value = "NL"
						}
					}
				}
			}.Serialize();

			if (needToShowAssessedAmount)
			{
				var expectedInterpretation = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Message has been rejected</H3><p><strong>Function Code: </strong>03-REJ<br><strong>Old CHIEF Report Code: </strong>27 (or N3/S3)<br><strong>Bank: </strong>GB25CITI08320011963155<br><strong>Declaration Version: </strong>2<br><strong>MRN: </strong>18GBJCUDI9ADRHWD54<br><strong>LRN: </strong>Import_Obligation_REJ</p><p>Additional Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Statement Type</td><td>Statement Code/Description</td><td>Limit Date Time</td></tr><tr><td>AFB - Customs position motivation</td><td>A2 - BlaBla</td><td>11-Dec-18 11:53:04</td></tr></table></p><p>Amendment Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Header/Item</td><td>Change Reason Code</td><td>Field Name</td><td>New Value</td><td>Path</td></tr><tr><td>Header</td><td>AMD</td><td>Nationality of means of transport crossing the border,coded</td><td>NL</td><td>Declaration/BorderTransportMeans/RegistrationNationalityCode<br>42A/15A/T014[1]</td></tr></table></p><p>Status Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Status</td><td>Status Description</td><td>Effective Date Time</td></tr><tr><td>39</td><td>Granted</td><td>22-Dec-18 11:53:04</td></tr></table></p><p>Payment Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Item</td><td>Tax Type or Reference</td><td>Payment Amount</td><td>Assessed Amount</td><td>Tax Rate</td><td>Tax Base</td></tr><tr><td>Item 1</td><td>A00</td><td>97.00</td><td>97.00</td><td>0%</td><td>5000</td></tr><tr><td>Item 1</td><td>B00</td><td>219.40</td><td>219.20</td><td>0%</td><td>&nbsp;</td></tr></table></p><p>Error Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Header/Item</td><td>Error Code</td><td>Error Description</td><td>Field Name</td><td>Original Value</td><td>Data Element</td><td>Path (for technical support only)</td></tr><tr><td>Item 4</td><td>DMS1001</td><td>&nbsp;</td><td>Net net weight</td><td>100</td><td>6/1</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem[4]/Commodity/GoodsMeasure/NetNetWeightMeasure<br>42A/67A[1]/68A[4]/23A/65A/128[1]</td></tr></table></p>";
				AssertXMLEquals(expectedInterpretation, responseEdiMessage.EM_MessageInterpretation);
			}
			else
			{
				var expectedInterpretation = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Message has been rejected</H3><p><strong>Function Code: </strong>03-REJ<br><strong>Old CHIEF Report Code: </strong>27 (or N3/S3)<br><strong>Bank: </strong>GB25CITI08320011963155<br><strong>Declaration Version: </strong>2<br><strong>MRN: </strong>18GBJCUDI9ADRHWD54<br><strong>LRN: </strong>Import_Obligation_REJ</p><p>Additional Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Statement Type</td><td>Statement Code/Description</td><td>Limit Date Time</td></tr><tr><td>AFB - Customs position motivation</td><td>A2 - BlaBla</td><td>11-Dec-18 11:53:04</td></tr></table></p><p>Amendment Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Header/Item</td><td>Change Reason Code</td><td>Field Name</td><td>New Value</td><td>Path</td></tr><tr><td>Header</td><td>AMD</td><td>Nationality of means of transport crossing the border,coded</td><td>NL</td><td>Declaration/BorderTransportMeans/RegistrationNationalityCode<br>42A/15A/T014[1]</td></tr></table></p><p>Status Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Status</td><td>Status Description</td><td>Effective Date Time</td></tr><tr><td>39</td><td>Granted</td><td>22-Dec-18 11:53:04</td></tr></table></p><p>Payment Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Item</td><td>Tax Type or Reference</td><td>Payment Amount</td><td>Tax Rate</td><td>Tax Base</td></tr><tr><td>Item 1</td><td>A00</td><td>97.00</td><td>0%</td><td>5000</td></tr><tr><td>Item 1</td><td>B00</td><td>219.40</td><td>0%</td><td>&nbsp;</td></tr></table></p><p>Error Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Header/Item</td><td>Error Code</td><td>Error Description</td><td>Field Name</td><td>Original Value</td><td>Data Element</td><td>Path (for technical support only)</td></tr><tr><td>Item 4</td><td>DMS1001</td><td>&nbsp;</td><td>Net net weight</td><td>100</td><td>6/1</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem[4]/Commodity/GoodsMeasure/NetNetWeightMeasure<br>42A/67A[1]/68A[4]/23A/65A/128[1]</td></tr></table></p>";
				AssertXMLEquals(expectedInterpretation, responseEdiMessage.EM_MessageInterpretation);
			}
		}

		public void TestStatementCodeDescriptionLookup()
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_MessageText =
				@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>09</FunctionCode>
  <FunctionalReferenceID>092b64b87eea4f3482af0b13627ffbad</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190121151243Z</DateTimeString>
  </IssueDateTime>
  <AdditionalInformation>
    <StatementCode>A1</StatementCode>
    <StatementTypeCode>AFB</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>A2</StatementCode>
    <StatementTypeCode>AFB</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>B1</StatementCode>
    <StatementTypeCode>AFB</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>A1</StatementCode>
    <StatementTypeCode>AFB</StatementTypeCode>
    <StatementDescription>Use this description instead of the lookup</StatementDescription>
  </AdditionalInformation>

  <AdditionalInformation>
    <StatementCode>H01</StatementCode>
    <StatementTypeCode>AES</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>H02</StatementCode>
    <StatementTypeCode>BLF</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>N01</StatementCode>
    <StatementTypeCode>ACA</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>N02</StatementCode>
    <StatementTypeCode>CEX</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>N03</StatementCode>
    <StatementTypeCode>1</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>N04</StatementCode>
    <StatementTypeCode>2</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>N05</StatementCode>
    <StatementTypeCode>3</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>N06</StatementCode>
    <StatementTypeCode>4</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>N07</StatementCode>
    <StatementTypeCode>5</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>E01</StatementCode>
    <StatementTypeCode>6</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>E02</StatementCode>
    <StatementTypeCode>7</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>E03</StatementCode>
    <StatementTypeCode>8</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>X00</StatementCode>
    <StatementTypeCode>9</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>1</StatementCode>
    <StatementTypeCode>ALV</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>2</StatementCode>
    <StatementTypeCode>ALV</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>3</StatementCode>
    <StatementTypeCode>ALV</StatementTypeCode>
  </AdditionalInformation>
  <AdditionalInformation>
    <StatementCode>Q01</StatementCode>
    <StatementTypeCode>QRY</StatementTypeCode>
  </AdditionalInformation>

  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000000053</FunctionalReferenceID>
    <ID>19GB0TH1LGFNWFGVR5</ID>
    <VersionID>1</VersionID>
  </Declaration>
</Response>";

			AssertEquals(
				"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Declaration is now cleared</H3><p><strong>Function Code: </strong>09-CLE<br><strong>Declaration Version: </strong>1<br><strong>MRN: </strong>19GB0TH1LGFNWFGVR5<br><strong>LRN: </strong>HYEDUKMIK0000000000053<br><strong>Issued Date: </strong>2019-01-21 15:12</p><p>Additional Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Statement Type</td><td>Statement Code/Description</td><td>Limit Date Time</td></tr><tr><td>AFB - Customs position motivation</td><td>A1 - Satisfactory (All Declarations which have not resulted in A2 or B1)</td><td>&nbsp;</td></tr><tr><td>AFB - Customs position motivation</td><td>A2 - Considered Satisfactory (There are no Control Tasks for the Declaration)</td><td>&nbsp;</td></tr><tr><td>AFB - Customs position motivation</td><td>B1 - Not Satisfactory (There is a “Major Discrepancy” Control Result for the Declaration)</td><td>&nbsp;</td></tr><tr><td>AFB - Customs position motivation</td><td>A1 - Use this description instead of the lookup</td><td>&nbsp;</td></tr><tr><td>AES - Textual Explanation</td><td>H01 - DEFRA: Awaiting Decision</td><td>&nbsp;</td></tr><tr><td>BLF - Control explanation</td><td>H02 - DEFRA: To Be Inspected</td><td>&nbsp;</td></tr><tr><td>ACA - Document type to be presented for document control</td><td>N01 - DEFRA: Entry refused following inspection</td><td>&nbsp;</td></tr><tr><td>CEX - Clearance instructions for export</td><td>N02 - DEFRA:  Destroy</td><td>&nbsp;</td></tr><tr><td>1 - General Validation Result</td><td>N03 - DEFRA: Transform</td><td>&nbsp;</td></tr><tr><td>2 - Validation Result Additional UnitType</td><td>N04 - DEFRA: Re-export or re-dispatch</td><td>&nbsp;</td></tr><tr><td>3 - Validation Result Supplementary UnitType</td><td>N05 - DEFRA:  Use for other purposes</td><td>&nbsp;</td></tr><tr><td>4 - Validation Result Commodity Code</td><td>N06 - DEFRA: Recalled</td><td>&nbsp;</td></tr><tr><td>5 - Validation Result Document Type</td><td>N07 - DEFRA: Not acceptable</td><td>&nbsp;</td></tr><tr><td>6 - Tariff Action Code</td><td>E01 - DEFRA: Simplified Frontier Declaration used but Inspection Location on the Advance Notification does not identify LCP premises</td><td>&nbsp;</td></tr><tr><td>7 - Tariff Condition Code</td><td>E02 - DEFRA: Full Declaration used but Inspection Location on the Advance Notification identifies LCP premises</td><td>&nbsp;</td></tr><tr><td>8 - Tariff Measure Id</td><td>E03 - DEFRA: Unexpected data - Transit, Transhipment or Specific Warehouse</td><td>&nbsp;</td></tr><tr><td>9 - Tariff Document Occurence Type</td><td>X00 - DEFRA: No match</td><td>&nbsp;</td></tr><tr><td>ALV - DEFRA Control</td><td>1 - Document control</td><td>&nbsp;</td></tr><tr><td>ALV - DEFRA Control</td><td>2 - Physical control</td><td>&nbsp;</td></tr><tr><td>ALV - DEFRA Control</td><td>3 - Undetermined control</td><td>&nbsp;</td></tr><tr><td>QRY - Query from CDS</td><td>Q01 - Query</td><td>&nbsp;</td></tr></table></p>",
				message.EM_MessageInterpretation);
		}

		public void TestUsingValidationAndDescription()
		{
			var testHelper = new CDSResponseStatusTestDataHelper(Factory);
			testHelper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode, "ErrorCode");
			testHelper.CreateNewOrGetExistingCusCodeList(Core.Constants.Customs.Universal.RefDataGrouping.Codes.CustomsDeclarationService, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode, "CDS13000", "Credibility check: incredible value found", ZDateTime.Today.AddDays(-2), ZDateTime.Today.AddDays(2));
			Factory.Save();

			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_MessageText =
				@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>01</FunctionCode>
  <FunctionalReferenceID>b9818abc9a484579bd60af55316cddca</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190619120406+01</DateTimeString>
  </IssueDateTime>
  <AdditionalInformation>
    <StatementCode>smartErrorMsg</StatementCode>
    <StatementDescription>Credibility check: incredible value found</StatementDescription>
    <StatementTypeCode>1</StatementTypeCode>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>07B</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>53A</DocumentSectionCode>
    </Pointer>
  </AdditionalInformation>
  <Error>
    <Description>Invalid value</Description>
    <ValidationCode>CDS13000</ValidationCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>67A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>41A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>58B</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>371</DocumentSectionCode>
    </Pointer>
  </Error>
  <Declaration>
    <AcceptanceDateTime>
      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20190619120402+01</DateTimeString>
    </AcceptanceDateTime>
    <FunctionalReferenceID>HYEDUKMIK0000000001014</FunctionalReferenceID>
    <ID>19GB6Q54XXWF0FGVR0</ID>
    <VersionID>1</VersionID>
  </Declaration>
</Response>";

			var formattedHtml = @"
<style>
		body, 
		p, 
		td {
			font-family: Verdana, Arial, Helvetica, sans-serif; 
			font-size: 13px;
		}
	</style>
	<H3>Response from CDS: Declaration has been legally accepted</H3>
	<p><strong>Function Code: </strong>01-ACC<br><strong>Old CHIEF Report Code: </strong>E2<br><strong>Declaration Version: </strong>1<br><strong>MRN: </strong>19GB6Q54XXWF0FGVR0<br><strong>LRN: </strong>HYEDUKMIK0000000001014<br><strong>Issued Date: </strong>2019-06-19 12:04</p>
	<p>Additional Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
			<tr>
				<td>Statement Type</td>
				<td>Statement Code/Description</td>
				<td>Limit Date Time</td>
			</tr>
			<tr>
				<td>1 - General Validation Result</td>
				<td>smartErrorMsg - Credibility check: incredible value found</td>
				<td>&nbsp;</td>
			</tr>
		</table>
	</p>
	<p>Error Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table"">
			<tr>
				<td>Header/Item</td>
				<td>Error Code</td>
				<td>Error Description</td>
				<td>Field Name</td>
				<td>Original Value</td>
				<td>Data Element</td>
				<td>Path (for technical support only)</td>
			</tr>
			<tr>
				<td>Header</td>
				<td>CDS13000</td>
				<td>Invalid value (Credibility check: incredible value found)</td>
				<td>Charges type, coded</td>
				<td>absent</td>
				<td>4/9</td>
				<td>Declaration/GoodsShipment/CustomsValuation/ChargeDeduction/ChargesTypeCode<br>42A/67A/41A[1]/58B/371</td>
			</tr>
		</table>
	</p>";

			AssertMultilineASCIIEquals(
				"Should have created the correct HTML Code.",
				Regex.Replace(formattedHtml, @"[\r\n\t]", ""),
				message.EM_MessageInterpretation);
		}

		public void TestGetErrorDescriptionShouldUseERRCD()
		{
			var errorCode = "DMS1001";
			var helper = new UniversalReferenceTestDataHelper(Factory);
			helper.CreateNewOrGetExistingCusCodeType("CDSER", "CDSErrorCode");
			helper.CreateNewOrGetExistingCusCodeType(Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode, "ErrorCode");
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, "CDSER", errorCode, "I am a CDSERR", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			helper.CreateNewOrGetExistingCusCodeList(Core.Constants.CountryCodes.UnitedKingdom, Core.Constants.Customs.Universal.RefCusCodeListTypes.Codes.ErrorCode, errorCode, "I am a ERRCD", ZDateTime.Today, ZDateTime.Today.AddDays(1));
			Factory.Save();

			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_MessageText = new Response
			{
				FunctionCode = new ResponseFunctionCodeType(),
				Error = new ResponseError[]
				{
					new ResponseError
					{
						ValidationCode = new ErrorValidationCodeType
						{
							Value = "DMS1001"
						}
					}
				},
				Declaration = new ResponseDeclaration()
			}.Serialize();

			var text = message.EM_MessageInterpretation;
			Assert("The type CDSERR is obsolete, we use ERRCD instead.", text.Contains("I am a ERRCD"));
			Assert("The type CDSERR is obsolete, we use ERRCD instead.", !text.Contains("I am a CDSERR"));
		}

		public void TestDisplayEmptyPaymentInfoTable()
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_MessageText =
				@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>13</FunctionCode>
	<FunctionalReferenceID>b8924e9367c34901bb3afc8419572044</FunctionalReferenceID>
	<IssueDateTime>
		<DateTimeString formatCode=""304"">20191107153438Z</DateTimeString>
	</IssueDateTime>
	<Bank>
		<ReferenceID>CITIGB2LLON</ReferenceID>
		<ID>GB25CITI08320011963155</ID>
	</Bank>
	<Status>
		<NameCode>115</NameCode>
	</Status>
	<Declaration>
		<FunctionalReferenceID>R251_TC05_1710_007</FunctionalReferenceID >
		<ID>20GB5KOKNWWL7FGVR0</ID>
		   <VersionID>3</VersionID>
		   <GoodsShipment>
			   <GovernmentAgencyGoodsItem>
				   <SequenceNumeric>1</SequenceNumeric>
				   <Commodity/>
			   </GovernmentAgencyGoodsItem>
		   </GoodsShipment>
	   </Declaration >
   </Response>";

			AssertEquals(
				"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Duties and taxes have been calculated and are due</H3>" +
				"<p><strong>Function Code: </strong>13-TAX<br><strong>Old CHIEF Report Code: </strong>E2<br><strong>Bank: </strong>GB25CITI08320011963155<br><strong>Declaration Version: </strong>3<br>" +
				"<strong>MRN: </strong>20GB5KOKNWWL7FGVR0<br><strong>LRN: </strong>R251_TC05_1710_007</p>" +
				"<p>Status Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Status</td><td>Status Description</td><td>Effective Date Time</td></tr><tr><td>115</td><td>Provisional customs debt</td><td>&nbsp;</td></tr></table></p>",
				message.EM_MessageInterpretation);
		}

		public void TestStatusInformationDescription()
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_MessageText =
				@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>16</FunctionCode> <FunctionalReferenceID>fb2794252d6b45e6b5def84d53bb969f</FunctionalReferenceID> <IssueDateTime> <DateTimeString formatCode=""304"">20191113172408Z</DateTimeString> </IssueDateTime> <AdditionalInformation> <StatementCode /> <!-- This references DE 5/12: Customs office of exit --> <StatementDescription>GB000041</StatementDescription> <StatementTypeCode>CEX</StatementTypeCode> </AdditionalInformation> <Status> <EffectiveDateTime> <DateTimeString>Wed Nov 13 00:00:00 UTC 2019</DateTimeString> </EffectiveDateTime> <NameCode>A1</NameCode> </Status> <Declaration> <FunctionalReferenceID>3.1_P1_TT_1311RM41</FunctionalReferenceID> <ID>20GB5KOKNWWL7FGVR0</ID> <VersionID>2</VersionID> </Declaration>
   </Response>";

			AssertEquals(
				"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Goods have exited the Customs Union</H3><p><strong>Function Code: </strong>16-EOG<br><strong>Declaration Version: </strong>2<br><strong>MRN: </strong>20GB5KOKNWWL7FGVR0<br><strong>LRN: </strong>3.1_P1_TT_1311RM41</p><p>Additional Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Statement Type</td><td>Statement Code/Description</td><td>Limit Date Time</td></tr><tr><td>CEX - Clearance instructions for export</td><td>GB000041</td><td>&nbsp;</td></tr></table></p><p>Status Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Status</td><td>Status Description</td><td>Effective Date Time</td></tr><tr><td>A1</td><td>Satisfactory (All Declarations which have not resulted in A2 or B1)</td><td>&nbsp;</td></tr></table></p>",
				message.EM_MessageInterpretation);
		}

		public void TestStatus_IndicativeCustomsDebt()
		{
			var message = Factory.New<CDSResponseEDIMessage>();
			message.EM_MessageText =
				@"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>16</FunctionCode> <FunctionalReferenceID>fb2794252d6b45e6b5def84d53bb969f</FunctionalReferenceID> <IssueDateTime> <DateTimeString formatCode=""304"">20191113172408Z</DateTimeString> </IssueDateTime> <AdditionalInformation> <StatementCode /> <!-- This references DE 5/12: Customs office of exit --> <StatementDescription>GB000041</StatementDescription> <StatementTypeCode>CEX</StatementTypeCode> </AdditionalInformation> <Status> <EffectiveDateTime> <DateTimeString>Wed Nov 13 00:00:00 UTC 2019</DateTimeString> </EffectiveDateTime> <NameCode>67</NameCode> </Status> <Declaration> <FunctionalReferenceID>3.1_P1_TT_1311RM41</FunctionalReferenceID> <ID>20GB5KOKNWWL7FGVR0</ID> <VersionID>2</VersionID> </Declaration>
   </Response>";

			AssertEquals(
				"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Goods have exited the Customs Union</H3><p><strong>Function Code: </strong>16-EOG<br><strong>Declaration Version: </strong>2<br><strong>MRN: </strong>20GB5KOKNWWL7FGVR0<br><strong>LRN: </strong>3.1_P1_TT_1311RM41</p><p>Additional Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Statement Type</td><td>Statement Code/Description</td><td>Limit Date Time</td></tr><tr><td>CEX - Clearance instructions for export</td><td>GB000041</td><td>&nbsp;</td></tr></table></p><p>Status Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\"><tr><td>Status</td><td>Status Description</td><td>Effective Date Time</td></tr><tr><td>67</td><td>Indicative Customs Debt</td><td>&nbsp;</td></tr></table></p>",
				message.EM_MessageInterpretation);
		}

		public void TestInterpretationOfBlank_ChangeReasonCode()
		{
			var realCDSRespoinse = @"<Response xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>07</FunctionCode>
  <FunctionalReferenceID>d10ec172520b4cf497dcfb098f07f55e</FunctionalReferenceID>
  <IssueDateTime>`
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20201006094705Z</DateTimeString>
  </IssueDateTime>
  <Amendment>
 <ChangeReasonCode />
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>15A</DocumentSectionCode>
      <TagID>T014</TagID>
    </Pointer>
  </Amendment>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000002733</FunctionalReferenceID>
    <ID>20GBB38EW0CHK2YCR9</ID>
    <VersionID>2</VersionID>
  </Declaration>
  <Declaration xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>9</FunctionCode>
    <BorderTransportMeans>
      <RegistrationNationalityCode>GB</RegistrationNationalityCode>
    </BorderTransportMeans>
  </Declaration>
</Response>";

			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;

			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "Import_Obligation_REJ";
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;
			var inv1 = dec.Invoices.AddNew();
			var invLine1 = inv1.InvoiceLines.AddNew();
			invLine1.JI_CL = entryLine1.PK;

			var declarationEdiMessage = Factory.New<CDSNewDeclarationEDIMessage>();
			declarationEdiMessage.EM_MessageText = friendlyCodeWithPointerTest.GetRequestMessageXmlContent();
			entry.Messages.Add(declarationEdiMessage);
			Factory.Save();

			var responseEdiMessage = (CDSResponseEDIMessage)entry.Messages.AddNew(typeof(CDSResponseEDIMessage));
			responseEdiMessage.EM_MessageText = realCDSRespoinse;

			var expectedInterpretation = "<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Declaration has been updated by Customs</H3>" +
				"<p><strong>Function Code: </strong>07-RES<br><strong>Declaration Version: </strong>2<br><strong>MRN: </strong>20GBB38EW0CHK2YCR9<br><strong>LRN: </strong>HYEDUKMIK0000000002733<br>" +
				"<strong>Issued Date: </strong>2020-10-06 09:47</p><p>Amendment Information:<table border=\"1\" cellpadding=\"1\" cellspacing=\"0\" width=\"100%\" class=\"table\">" +
				"<tr><td>Header/Item</td><td>Change Reason Code</td><td>Field Name</td><td>New Value</td><td>Path</td></tr><tr><td>Header</td><td>Not Defined</td>" +
				"<td>Nationality of means of transport crossing the border,coded</td><td>GB</td><td>Declaration/BorderTransportMeans/RegistrationNationalityCode<br>42A/15A/T014[1]</td></tr></table></p>";
			AssertXMLContains(expectedInterpretation, responseEdiMessage.EM_MessageInterpretation);
		}

		public void TestInterpretationOfAmendmentRejection()
		{
			// This test has been added to test the interpretation of a rejection response.  CDS does not currently output the Sequence Number for the DutyTaxFee Node (50A), so I have artificially added it in the example response below.
			var dec = Factory.New<JobDeclaration>();
			dec.JE_DeclarationReference = "B123";
			dec.JE_ApplicationCode = GB.Registry.Business.DeclarationApplicationCodeList.Codes.Customs_Declaration_Services;
			var entry = dec.CustomsEntryHeaders.AddNew();
			entry.CH_BGMReference = "Import_Obligation_REJ";
			var entryLine1 = entry.AllEntryLines.AddNew();
			entryLine1.CL_LineNumber = 1;

			var outgoingMessage = (CDSEDIMessage)entry.Messages.AddNew(typeof(CDSEDIMessage));
			outgoingMessage.EM_ReceiveTransmit = EDIMessage.Direction.Transmit;
			outgoingMessage.EM_ApplicationCode = EDIMessage.ApplicationCodes.GbCustomsDeclarationServices;
			#region sent message
			outgoingMessage.EM_MessageText = @"<MetaData xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DocumentMetaData-DMS:2"">
  <Declaration xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns=""urn:wco:datamodel:WCO:DEC-DMS:2"">
    <FunctionCode>13</FunctionCode>
    <FunctionalReferenceID>HYEDUKMIK0000000003418</FunctionalReferenceID>
    <ID>21GB2YNMGQKXZRXKR0</ID>
    <TypeCode>COR</TypeCode>
    <AdditionalInformation>
      <StatementDescription>Amend Quota</StatementDescription>
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
    <AdditionalInformation>
      <StatementDescription>Amend Quota</StatementDescription>
      <StatementTypeCode>AES</StatementTypeCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>2</SequenceNumeric>
        <DocumentSectionCode>06A</DocumentSectionCode>
      </Pointer>
    </AdditionalInformation>
    <AdditionalInformation>
      <StatementDescription>Amend Quota</StatementDescription>
      <StatementTypeCode>AES</StatementTypeCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>3</SequenceNumeric>
        <DocumentSectionCode>06A</DocumentSectionCode>
      </Pointer>
    </AdditionalInformation>
    <Amendment>
      <ChangeReasonCode>24</ChangeReasonCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>67A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>68A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>50A</DocumentSectionCode>
        <TagID>164</TagID>
      </Pointer>
    </Amendment>
    <Amendment>
      <ChangeReasonCode>24</ChangeReasonCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>67A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>68A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>50A</DocumentSectionCode>
        <TagID>401</TagID>
      </Pointer>
    </Amendment>
    <Amendment>
      <ChangeReasonCode>24</ChangeReasonCode>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>42A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>67A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>68A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>1</SequenceNumeric>
        <DocumentSectionCode>23A</DocumentSectionCode>
      </Pointer>
      <Pointer>
        <SequenceNumeric>2</SequenceNumeric>
        <DocumentSectionCode>50A</DocumentSectionCode>
      </Pointer>
    </Amendment>
    <GoodsShipment>
      <GovernmentAgencyGoodsItem>
        <SequenceNumeric>1</SequenceNumeric>
        <Commodity>
          <DutyTaxFee>
            <DutyRegimeCode>120</DutyRegimeCode>
            <QuotaOrderID>058024</QuotaOrderID>
          </DutyTaxFee>
        </Commodity>
      </GovernmentAgencyGoodsItem>
    </GoodsShipment>
  </Declaration>
</MetaData>";
			#endregion
			var incoming = (CDSResponseEDIMessage)entry.Messages.AddNew(typeof(CDSResponseEDIMessage));
			incoming.EM_ReceiveTransmit = EDIMessage.Direction.Receive;
			incoming.EM_Status = EDIMessage.Status.Queued;
			#region incoming
			incoming.EM_MessageText = @"<Response xmlns:xsi=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:xsd=""http://www.w3.org/2001/XMLSchema"" xmlns=""urn:wco:datamodel:WCO:RES-DMS:2"">
  <FunctionCode>03</FunctionCode>
  <FunctionalReferenceID>6e4b1ab93c294d8d8406f6000fd08574</FunctionalReferenceID>
  <IssueDateTime>
    <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20210316152901Z</DateTimeString>
  </IssueDateTime>
  <AdditionalInformation>
    <StatementDescription>Invalid quota number</StatementDescription>
    <StatementTypeCode>1</StatementTypeCode>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>07B</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>53A</DocumentSectionCode>
    </Pointer>
  </AdditionalInformation>
  <Error>
    <Description>Invalid quota number</Description>
    <ValidationCode>CDS40013</ValidationCode>
    <Pointer>
      <DocumentSectionCode>42A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>67A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>68A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <DocumentSectionCode>23A</DocumentSectionCode>
    </Pointer>
    <Pointer>
      <SequenceNumeric>1</SequenceNumeric>
      <DocumentSectionCode>50A</DocumentSectionCode>
      <TagID>401</TagID>
    </Pointer>
  </Error>
  <Declaration>
    <FunctionalReferenceID>HYEDUKMIK0000000003418</FunctionalReferenceID>
    <ID>21GB2YNMGQKXZRXKR0</ID>
    <RejectionDateTime>
      <DateTimeString formatCode=""304"" xmlns=""urn:wco:datamodel:WCO:Response_DS:DMS:2"">20210316152901Z</DateTimeString>
    </RejectionDateTime>
    <VersionID>1</VersionID>
  </Declaration>
</Response>";
			#endregion
			var interpretation = incoming.EM_MessageInterpretation;
			var expectedInterpretation = @"<style>body, p, td {font-family: Verdana, Arial, Helvetica, sans-serif; font-size: 13px;}</style><H3>Response from CDS: Message has been rejected</H3>" +
				@"<p><strong>Function Code: </strong>03-REJ<br><strong>Old CHIEF Report Code: </strong>27 (or N3/S3)<br><strong>Declaration Version: </strong>1<br>" +
				@"<strong>MRN: </strong>21GB2YNMGQKXZRXKR0<br><strong>LRN: </strong>HYEDUKMIK0000000003418<br><strong>Issued Date: </strong>2021-03-16 15:29</p>" +
				@"<p>Additional Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Statement Type</td><td>Statement Code/Description</td><td>Limit Date Time</td></tr><tr><td>1 - General Validation Result</td><td>Invalid quota number</td><td>&nbsp;</td></tr></table></p><p>Error Information:<table border=""1"" cellpadding=""1"" cellspacing=""0"" width=""100%"" class=""table""><tr><td>Header/Item</td><td>Error Code</td><td>Error Description</td><td>Field Name</td><td>Original Value</td><td>Data Element</td><td>Path (for technical support only)</td></tr><tr><td>Item 1</td><td>CDS40013</td><td>Invalid quota number ()</td><td>Quota identifier</td><td>058024</td><td>8/1</td><td>Declaration/GoodsShipment/GovernmentAgencyGoodsItem/Commodity/DutyTaxFee/QuotaOrderID<br>42A/67A/68A[1]/23A/50A[1]/401[1]</td></tr></table></p>";

			AssertEquals("Should give the correct previous quota value of 058024",
				expectedInterpretation,
				interpretation);
		}

		protected override void SetUp()
		{
			base.SetUp();
			friendlyCodeWithPointerTest = new FriendlyCodeWithPointerTest();
		}

		FriendlyCodeWithPointerTest friendlyCodeWithPointerTest;
	}
}
