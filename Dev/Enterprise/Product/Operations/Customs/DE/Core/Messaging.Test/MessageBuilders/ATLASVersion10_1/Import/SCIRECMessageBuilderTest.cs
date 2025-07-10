using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.DE.MessageContracts;
using CargoWise.Customs.DE.MessageContracts.Import;
using Enterprise.Customs.DE.Messaging.Testing;
using Moq;
using NUnit.Framework;
using static Enterprise.Customs.DE.Messaging.Testing.MessageBuilderTestHelper;

namespace Enterprise.Customs.DE.Messaging.ATLASVersion10_1.Testing
{
	sealed class SCIRECMessageBuilderTest : MessageBuilderTest<SCIRECMessageBuilder, CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.VSCIRJ>
	{
		[TestDate(2021, 04, 20, 10, 25, 04)]
		[ExpectNoExceptions]
		public void TestCompleteMessage()
		{
			headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns("ESUMA");
			UpdateMockedHeaderToPopulateInwardProcessing();
			UpdateMockedHeaderToPopulateCustomsWarehouse();

			scirecBuilder = new SCIRECMessageBuilder(messageHeaderMock.Object);
			var actual = scirecBuilder.GetXMLMessage().AsString();
			NUnit.Framework.Assert.That(actual, Is.EqualTo(new CargoWise.IO.EmbeddedResourceRetriever().GetString($"{TestExtensionsATLASVersion10_1.ImportTestFilesResourcePath}.TestSCIRECMessage.txt")));
		}

		[ExpectNoExceptions]
		public void TestPopulateHeader_LRNTruncated()
		{
			headerMock.Setup(m => m.LocalReferenceNumber).Returns("LRN0000000000000000000111");
			NUnit.Framework.Assert.That(scirecBuilder.GenerateMessage().Header.LRN, Is.EqualTo("LRN0000000000000000000"), "LRN should be truncated to 22 characters.");
		}

		public void TestPlaceHolders()
		{
			var expectedXML = @"<InterchangeControlReference>&lt;&lt;INTERCHANGENUMBERPLACEHOLDER&gt;&gt;</InterchangeControlReference>
    <MessageReferenceNumber>1</MessageReferenceNumber>
    <MessageIdentifier>&lt;&lt;SENDERS REFERENCE PLACE HOLDER&gt;&gt;</MessageIdentifier>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		[TestDate(2020, 1, 16, 11, 38, 04)]
		public void TestPreparationDateTime()
		{
			var expectedXML = @"<Preparation>
      <Date>2020-01-16</Date>
      <Time>12:38:00</Time>
    </Preparation>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestMessageGroup_AAV()
		{
			AssertXMLContainsUnformatted(@"<MessageGroup>AAV</MessageGroup>", scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestMessageGroup_AVV()
		{
			messageHeaderMock.Setup(m => m.MessageGroup).Returns("AVV");
			AssertXMLContainsUnformatted(@"<MessageGroup>AVV</MessageGroup>", scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateInterchangeSender()
		{
			var expectedXML = @"<InterchangeSender>
      <Identification>
        <ReferenceNumber>DE8999783</ReferenceNumber>
        <SubsidiaryNumber>0000</SubsidiaryNumber>
      </Identification>
    </InterchangeSender>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateInterchangeRecipient()
		{
			var expectedXML = @"<InterchangeRecipient>
      <Identification>
        <ReferenceNumber>DE005875</ReferenceNumber>
      </Identification>
    </InterchangeRecipient>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		[TestDate(2021, 04, 20, 10, 25, 25)]
		public void TestPopulateHeader()
		{
			var expectedXML = @"<Header>
    <MessageVersion>J.1.1</MessageVersion>
    <MessageCreationDate>2021-04-20</MessageCreationDate>
    <LRN>ABC12345</LRN>
    <Declaration>
      <Kind>F</Kind>
      <Type>AAV</Type>
    </Declaration>
    <LocalClearanceDate>2021-04-20</LocalClearanceDate>
    <PrematureInputFlag>J</PrematureInputFlag>
	<GoodsItemQuantity>10</GoodsItemQuantity>
    <CustomsGoodsStatus>EU</CustomsGoodsStatus>
    <CustomsAuthorisation>
      <LocalClearanceProcedure>DE001234567</LocalClearanceProcedure>
      <CurrentProcedure>DE001234568</CurrentProcedure>
    </CustomsAuthorisation>
    <GoodsLocation>locationOfGoods</GoodsLocation>
    <DepartureCountry>LV</DepartureCountry>
    <CurrencyCode>EUR</CurrencyCode>
    <AdditionalInformation>AdditionalInformation</AdditionalInformation>
    <RepresentativeRelationshipFlag>0</RepresentativeRelationshipFlag>
    <DeclarationPlace>Hamburg</DeclarationPlace>
    <AuthorisationNumber>1234567890AUTH</AuthorisationNumber>
</Header>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateDeclarant()
		{
			var expectedXML = @"<Declarant>
	<Identification>
		<ReferenceNumber>GR234567890</ReferenceNumber>
		<SubsidiaryNumber>0001</SubsidiaryNumber>
	</Identification>
</Declarant>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateRepresentative()
		{
			var expectedXML = @"<Representative>
	<Identification>
		<ReferenceNumber>GR345678901</ReferenceNumber>
		<SubsidiaryNumber>0002</SubsidiaryNumber>
	</Identification>
</Representative>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		[ExpectNoExceptions]
		public void TestPopulateRepresentative_Null()
		{
			headerMock.Setup(m => m.Representative).Returns((IImportParty)null);
			var message = scirecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.VSCIRJRepresentative)));
		}

		[ExpectNoExceptions]
		public void TestPopulateRepresentative_NoEori()
		{
			var partyWithNoEori = new Mock<IPartyID>();
			partyWithNoEori.Setup(id => id.EoriNumber).Returns(string.Empty);

			var representative = new Mock<IImportParty>();
			representative.Setup(m => m.Identification).Returns(partyWithNoEori.Object);

			headerMock.Setup(m => m.Representative).Returns(representative.Object);

			var message = scirecBuilder.GenerateMessage();
			NUnit.Framework.Assert.That(message.Representative, Is.EqualTo(default(CargoWise.Customs.DE.MessageDefinitions.ATLASVersion10_1.VSCIRJRepresentative)));
		}

		public void TestPopulatePrincipal()
		{
			var expectedXML = @"<Principal>
		<Identification>
			<ReferenceNumber>GR456789012</ReferenceNumber>
			<SubsidiaryNumber>0003</SubsidiaryNumber>
		</Identification>
	</Principal>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulatePrincipal_EmptyEori()
		{
			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(m => m.Name).Returns("Vasya Pupkin");
			address.Setup(m => m.Country).Returns("IR");
			address.Setup(m => m.Address).Returns("1st Ave");
			address.Setup(m => m.City).Returns("Dublin");
			address.Setup(m => m.District).Returns("District");
			address.Setup(m => m.Postcode).Returns("10225");

			var principal = new Mock<IImportParty>();
			principal.Setup(v => v.Address).Returns(address.Object);

			headerMock.Setup(m => m.Principal).Returns(principal.Object);
			var expectedXML = @"<Principal>
			<Name>Vasya Pupkin</Name>
			<Address>
				<Line>1st Ave</Line>
				<Country>IR</Country>
				<Postcode>10225</Postcode>
				<City>Dublin</City>
				<District>District</District>
			</Address>
	</Principal>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateContactPerson()
		{
			var expectedXML = @"<ContactPerson>
		<Name>Bob Baumeister</Name>
		<Position>Sachbearbeiter</Position>
		<PhoneNumber>06131-477447</PhoneNumber>
		<MailAddress>bob.baumeister@samplefreight.com</MailAddress>
	</ContactPerson>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateContactPerson_Null()
		{
			var expectedXML = @"<ContactPerson>
		<Name />
		<Position />
		<PhoneNumber />
		<MailAddress />
	</ContactPerson>";

			headerMock.Setup(m => m.ContactPerson).Returns((IImportPartyContactPerson)null);
			AssertXMLNotContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateBorderTransportMeans()
		{
			var expectedXML = @"<BorderTransportMeans><Mode>1</Mode><Type>07</Type><Information>AA-BB123</Information><Nationality>FR</Nationality></BorderTransportMeans>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateBorderTransportMeans_TruncatedInformation()
		{
			headerMock.Setup(m => m.BorderTransportMeansInformation).Returns(string.Empty.PadLeft(18, 'A'));

			var expectedXML = $@"<BorderTransportMeans><Mode>1</Mode><Type>07</Type><Information>{string.Empty.PadLeft(17, 'A')}</Information><Nationality>FR</Nationality></BorderTransportMeans>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateArrivalTransportMeans()
		{
			AssertXMLContainsUnformatted(@"<ArrivalTransportMeans><Identity>XX-AA123</Identity></ArrivalTransportMeans>", scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulatePreviousAdministrativeReference_PreviousAdministrativeReferencesTypeATNEU()
		{
			var expectedXML = @"<PreviousAdministrativeReferences>
			<Type>ATNEU</Type>
		</PreviousAdministrativeReferences>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulatePreviousAdministrativeReference_PreviousAdministrativeReferencesTypeT1()
		{
			var expectedXML = @"<PreviousAdministrativeReferences>
			<Type>T1</Type>
			<PreviousAdministrativeReference>
				<ReferenceNumber>REFNUM12345</ReferenceNumber>
			</PreviousAdministrativeReference>
		</PreviousAdministrativeReferences>";
			headerMock.Setup(m => m.PreviousAdministrativeReferenceType).Returns("T1");
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPreviousAdministrativeReference_NotPopulated_PreviousAdministrativeReferenceNumberEmpty()
		{
			var expectedXML = @"<PreviousAdministrativeReferences>
			<Type>ATNEU</Type>
		</PreviousAdministrativeReferences>";
			headerMock.Setup(m => m.PreviousAdministrativeReferenceNumber).Returns(string.Empty);
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateSummaryDeclarationIdentificationIndicator_ULD()
		{
			var expectedXML = @"<SummaryDeclaration>
			<IdentificationIndicator>AWB</IdentificationIndicator>
			<GoodsItem>
				<Quantity>1</Quantity>
				<IdentificationByKey>
					<Kind>ULD</Kind>
					<Number>1234567890</Number>
					<Custodian>
						<Identification>
							<ReferenceNumber>DE3333333</ReferenceNumber>
						</Identification>
					</Custodian>
				</IdentificationByKey>
			</GoodsItem>";

			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("ULD");
			AssertXMLContainsUnformatted("ULD", expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateSummaryDeclarationIdentificationIndicator_AWB()
		{
			var expectedXML = @"<SummaryDeclaration>
			<IdentificationIndicator>AWB</IdentificationIndicator>
			<GoodsItem>
				<Quantity>1</Quantity>
				<IdentificationByKey>
					<Kind>AWB</Kind>
					<Number>1234567890</Number>
					<Custodian>
						<Identification>
							<ReferenceNumber>DE3333333</ReferenceNumber>
						</Identification>
					</Custodian>
				</IdentificationByKey>
			</GoodsItem>";

			AssertXMLContainsUnformatted("AWB", expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateSummaryDeclarationIdentificationIndicator_REG()
		{
			var expectedXML = @"<SummaryDeclaration>
			<IdentificationIndicator>REG</IdentificationIndicator>
			<GoodsItem>
				<Quantity>1</Quantity>
				<IdentificationByRegistration>
					<ReferencedRegistrationNumber>ATA123456789123456789</ReferencedRegistrationNumber>
					<ReferencedSequenceNumber>1</ReferencedSequenceNumber>
				</IdentificationByRegistration>
			</GoodsItem>";

			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateSummaryDeclaration_PreviousAdministrativeReferenceTypeATNEU()
		{
			var messageText = scirecBuilder.GetXMLMessage().AsString();
			CombineAssertions(() =>
			{
				AssertXMLContainsUnformatted("Contains SummaryDeclaration", ExpectedSummaryDeclaration, messageText);
				AssertNotXMLContainsUnformatted("Doesn't contain CustomsWarehouse", ExpectedCustomsWarehouse, messageText);
				AssertNotXMLContainsUnformatted("Doesn't contain InwardProcessing", ExpectedInwardProcessing, messageText);
			});
		}

		public void TestPopulateConsignee()
		{
			var expectedXML = @"<Consignee>
		<Identification>
			<ReferenceNumber>GR667890124</ReferenceNumber>
			<SubsidiaryNumber>0005</SubsidiaryNumber>
		</Identification>
	</Consignee>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateConsignee_EmptyEori()
		{
			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(m => m.Name).Returns("Vasya Pupkin");
			address.Setup(m => m.Country).Returns("IR");
			address.Setup(m => m.Address).Returns("1st Ave");
			address.Setup(m => m.City).Returns("Dublin");
			address.Setup(m => m.District).Returns("District");
			address.Setup(m => m.Postcode).Returns("10225");

			var consignee = new Mock<IImportParty>();
			consignee.Setup(v => v.Address).Returns(address.Object);

			headerMock.Setup(m => m.Consignee).Returns(consignee.Object);

			var expectedXML = @"<Consignee>
		<Name>Vasya Pupkin</Name>
		<Address>
			<Line>1st Ave</Line>
			<Country>IR</Country>
			<Postcode>10225</Postcode>
			<City>Dublin</City>
			<District>District</District>
		</Address>
	</Consignee>";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateContainers()
		{
			var expectedXML = @"<Containers>
	  <ContainerFlag>J</ContainerFlag>
	  <Container><IdentificationNumber>CONT1</IdentificationNumber></Container>
	  <Container><IdentificationNumber>CONT2</IdentificationNumber></Container>
	</Containers>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateContainers_Container_ContainerFlag()
		{
			headerMock.Setup(m => m.ContainerFlag).Returns("N");

			var expectedXML = @"<Containers>
	  <ContainerFlag>N</ContainerFlag>
	</Containers>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateContainers_Container_NotPopulated()
		{
			headerMock.Setup(m => m.ContainerFlag).Returns("N");

			var expectedXML = @"<Container>";
			AssertXMLNotContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateForeignTradeStatistics()
		{
			var expectedXML = @"<ForeignTradeStatistics>
	  <InlandTransportMode>3</InlandTransportMode>
	  <TotalGrossMassMeasure>123456789.1</TotalGrossMassMeasure>
	</ForeignTradeStatistics>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateDocument()
		{
			var expectedXML = @"<Document><Division>4</Division><Type>N380</Type><ReferenceNumber>DOC1REFERENCE</ReferenceNumber>  <IssuingDate>2020-10-23</IssuingDate></Document>
				<Document><Division>4</Division><Type>X123</Type><ReferenceNumber>DOC2REFERENCE</ReferenceNumber><IssuingDate>2020-10-24</IssuingDate></Document>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateLine()
		{
			var expectedXML = @"<GoodsItem>
				<SequenceNumber>1</SequenceNumber>
				<Procedure>
					<RequestedPreviousProcedure>4000</RequestedPreviousProcedure>
				</Procedure>
				<GoodsDescription>Anzüge, Kombinationen, Jacken, lange Hosen (einschließlich Kniebundhosen und ähnliche Hosen), Latzhosen und kurze Hosen (ausgenommen Badehosen), für Männer oder Knaben:</GoodsDescription>
				<NetMassMeasure>11866.4</NetMassMeasure>
				<OriginCountry>CN</OriginCountry>
				<SupplementaryInformation>Positionszusatz</SupplementaryInformation>
	";
			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateCommodityCode()
		{
			var expectedXML = @"<CommodityCode><CommodityCode>62034311000</CommodityCode></CommodityCode>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateAdditionalProcedure()
		{
			goodsItemMock.Setup(m => m.AdditionalProcedure).Returns(new[] { GetLongString("C", 9) });
			var expectedXML = $@"<AdditionalProcedure><Code>{GetLongString("C", 9)}</Code></AdditionalProcedure>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateSupplementaryCodes()
		{
			goodsItemMock.Setup(m => m.SupplementaryCodes).Returns(new[] { GetLongString("C", 9) });
			var expectedXML = $@"<SupplementaryCodes><Code>{GetLongString("C", 9)}</Code></SupplementaryCodes>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulatePackage()
		{
			var expectedXML = @"<Package><Kind>CT</Kind><Quantity>970</Quantity><MarksNumbers>1-970</MarksNumbers></Package>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulatePackage_NoPackages()
		{
			goodsItemMock.Setup(m => m.Package).Returns((IImportPackage)null);

			AssertXMLNotContainsUnformatted(@"<Package>", scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateLineForeignTradeStatistics()
		{
			var expectedXML = @"<ForeignTradeStatistics><GrossMassMeasure>11860.5</GrossMassMeasure></ForeignTradeStatistics>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateLineForeignTradeStatistics_GrossMassMeasure_Empty()
		{
			goodsItemMock.Setup(m => m.ForeignTradeStatisticsGrossMassMeasure).Returns(decimal.Zero);

			var expectedXML = @"</Package><ForeignTradeStatistics>";

			AssertNotXMLContainsUnformatted("Does not contain foreight trade statistict", expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateLineInwardMovement()
		{
			var expectedXML = @"<InwardMovement><Amount><Quantity>18220</Quantity><MeasurementUnit>NAR</MeasurementUnit><Qualifier>X</Qualifier></Amount></InwardMovement>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateLineAssessment()
		{
			var expectedXML = @"<Assessment>
					<CustomsValue>100628.6</CustomsValue>
					<Amount>
						<Quantity>18219</Quantity>
						<MeasurementUnit>NAR</MeasurementUnit>
						<Qualifier>X</Qualifier>
					</Amount>
					<SpecificRate>
						<Type>X</Type>
						<Value>10.02</Value>
					</SpecificRate>
					<ContentInformation>
						<Type>X</Type>
						<Degree-Percentage>0.01</Degree-Percentage>
					</ContentInformation>
				</Assessment>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateAssessmentCustomsValue()
		{
			var expectedXML = @"<CustomsValue>100628.6</CustomsValue>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateAssessmentAmount()
		{
			var expectedXML = @"<Amount>
						<Quantity>18219</Quantity>
						<MeasurementUnit>NAR</MeasurementUnit>
						<Qualifier>X</Qualifier>
					</Amount>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateAssessmentAmount_NotMapped()
		{
			goodsItemMock.Setup(m => m.AssessmentAmount).Returns(new IAmount[] { GetAmount(0, "NAR", "X").Object });

			var expectedXML = @"<Assessment>
					<CustomsValue>100628.6</CustomsValue>
					<SpecificRate>
						<Type>X</Type>
						<Value>10.02</Value>
					</SpecificRate>
					<ContentInformation>
						<Type>X</Type>
						<Degree-Percentage>0.01</Degree-Percentage>
					</ContentInformation>
				</Assessment>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateAssessmentSpecificRate()
		{
			var expectedXML = @"<SpecificRate>
						<Type>X</Type>
						<Value>10.02</Value>
					</SpecificRate>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateAssessmentContentInformation()
		{
			var expectedXML = @"<ContentInformation>
						<Type>X</Type>
						<Degree-Percentage>0.01</Degree-Percentage>
					</ContentInformation>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateExciseDuty()
		{
			var expectedXML = @"<ExciseDuty>
					<Code>A123</Code>
					<Degree-Percentage>0.02</Degree-Percentage>
					<Value>1626.28</Value>
					<Amount>
						<Quantity>18219</Quantity>
						<MeasurementUnit>NAR</MeasurementUnit>
						<Qualifier>X</Qualifier>
					</Amount>
				</ExciseDuty>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulatePreferentialTreatment()
		{
			var expectedXML = @"<PreferentialTreatment>
						<RequestedPreferentialTreatment>200</RequestedPreferentialTreatment>
					</PreferentialTreatment>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		public void TestPopulateDocuments()
		{
			var expectedXML = @"<Document>
					<Division>4</Division>
					<Type>7HHF</Type>
					<ReferenceNumber>COSU6271657530</ReferenceNumber>
					<IssuingDate>2020-08-12</IssuingDate>
					<AtHandFlag>J</AtHandFlag>
					<WriteOff>
						<Quantity>1500</Quantity>
						<MeasurementUnit>NAR</MeasurementUnit>
						<Qualifier>Z</Qualifier>
					</WriteOff>
				</Document>";

			AssertXMLContainsUnformatted(expectedXML, scirecBuilder.GetXMLMessage().AsString());
		}

		[ExpectNoExceptions]
		public void TestLocalClearanceDate_Null()
		{
			headerMock.Setup(m => m.LocalClearanceDate).Returns((DateTime?)null);

			NUnit.Framework.Assert.That(scirecBuilder.GenerateMessage().Header.LocalClearanceDateSpecified, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestLocalClearanceDate_DeclarantType()
		{
			headerMock.Setup(m => m.DeclarationType).Returns("ABC");

			NUnit.Framework.Assert.That(scirecBuilder.GenerateMessage().Header.LocalClearanceDateSpecified, Is.EqualTo(false));
		}

		[ExpectNoExceptions]
		public void TestPopulateCustomsWarehouse_GoodsItem_MRN()
		{
			UpdateMockedHeaderToPopulateCustomsWarehouse();
			var goodsItemMock = Mock.Get(headerMock.Object.CustomsWarehouse.GoodsItems.Single());
			goodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("23DE586601055987B7");

			var message = scirecBuilder.GenerateMessage();

			var goodsItem = message.CustomsWarehouse.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateInwardProcessing_GoodsItem_MRN()
		{
			UpdateMockedHeaderToPopulateInwardProcessing();
			var goodsItemMock = Mock.Get(headerMock.Object.InwardProcessing.GoodsItems.Single());
			goodsItemMock.Setup(x => x.ReferencedRegistrationNumber).Returns("23DE586601055987B7");

			var message = scirecBuilder.GenerateMessage();

			var goodsItem = message.InwardProcessing.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		[ExpectNoExceptions]
		public void TestPopulateSummaryDeclaration_GoodsItem_MRN()
		{
			summaryDeclarationMock.Setup(m => m.IdentificationIndicator).Returns("REG");
			var goodsItemMock = Mock.Get(headerMock.Object.SummaryDeclaration.GoodsItems.Single());
			goodsItemMock.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns("23DE586601055987B7");

			var message = scirecBuilder.GenerateMessage();

			var goodsItem = message.SummaryDeclaration.GoodsItem[0];
			CombineAssertions(() =>
			{
				NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.MRN, Is.EqualTo("23DE586601055987B7"));
				NUnit.Framework.Assert.That(goodsItem.IdentificationByRegistration.ReferencedRegistrationNumber, Is.Null.Or.Empty);
			});
		}

		void UpdateMockedHeaderToPopulateCustomsWarehouse()
		{
			var commercialAmount = new Mock<IAmount>();
			commercialAmount.Setup(x => x.MeasurementUnit).Returns("KGM");
			commercialAmount.Setup(x => x.Quantity).Returns(1.234m);
			commercialAmount.Setup(x => x.Qualifier).Returns("A");

			var debitAmount = new Mock<IAmount>();
			debitAmount.Setup(x => x.MeasurementUnit).Returns("KGM");
			debitAmount.Setup(x => x.Quantity).Returns(5.678m);
			debitAmount.Setup(x => x.Qualifier).Returns("M");

			var goodsItems = new Mock<ICustomsWarehouseGoodsItem>();
			goodsItems.Setup(x => x.ReferencedRegistrationNumber).Returns("ATA123456789123456789");
			goodsItems.Setup(x => x.ReferencedSequenceNumber).Returns(1);
			goodsItems.Setup(x => x.AccessViaATLASFlag).Returns(true);
			goodsItems.Setup(x => x.CommodityCode).Returns("12345678");
			goodsItems.Setup(x => x.UsualProcessingFlag).Returns(true);
			goodsItems.Setup(x => x.Complement).Returns("Description text");
			goodsItems.Setup(x => x.CommercialAmount).Returns(commercialAmount.Object);
			goodsItems.Setup(x => x.DebitAmount).Returns(debitAmount.Object);

			var customsWarehouse = new Mock<ICustomsWarehouse>();
			customsWarehouse.Setup(x => x.GoodsItemQuantity).Returns(10);
			customsWarehouse.Setup(x => x.WarehouseOwnerIdentifier).Returns("DE44444444");
			customsWarehouse.Setup(x => x.LocalReferenceNumber).Returns("WTG5678");
			customsWarehouse.Setup(x => x.GoodsItems).Returns(new ICustomsWarehouseGoodsItem[] { goodsItems.Object });

			headerMock.Setup(m => m.CustomsWarehouse).Returns(customsWarehouse.Object);
		}

		void UpdateMockedHeaderToPopulateInwardProcessing()
		{
			var goodsItems = new Mock<IInwardProcessingGoodsItem>();
			goodsItems.Setup(x => x.ReferencedRegistrationNumber).Returns("ATA123456789123456789");
			goodsItems.Setup(x => x.ReferencedSequenceNumber).Returns(1);
			goodsItems.Setup(x => x.AccessViaAtlasFlag).Returns(true);
			goodsItems.Setup(x => x.GoodsRelatedInformation).Returns("Related information");

			var inwardProcessing = new Mock<IInwardProcessing>();
			inwardProcessing.Setup(x => x.GoodsItemQuantity).Returns(20);
			inwardProcessing.Setup(x => x.ProcessingOwnerIdentifier).Returns("DE55555555");
			inwardProcessing.Setup(x => x.SimplifiedGrantAuthorisationFlag).Returns(true);
			inwardProcessing.Setup(x => x.MonitoringCustomsOfficeReferenceNumber).Returns("DE001234");
			inwardProcessing.Setup(x => x.GoodsItems).Returns(new IInwardProcessingGoodsItem[] { goodsItems.Object });

			headerMock.Setup(m => m.InwardProcessing).Returns(inwardProcessing.Object);
		}

		void SetupGoodsItem()
		{
			var package = new Mock<IImportPackage>();
			package.Setup(p => p.Kind).Returns("CT");
			package.Setup(p => p.Quantity).Returns(970);
			package.Setup(p => p.MarksNumbers).Returns("1-970");

			var assessmentSpecificRate = new Mock<IImportSpecificRate>();
			assessmentSpecificRate.Setup(r => r.Type).Returns("X");
			assessmentSpecificRate.Setup(r => r.Value).Returns(10.02m);

			var assessmentContentInformation = new Mock<IContentInformation>();
			assessmentContentInformation.Setup(r => r.ContentType).Returns("X");
			assessmentContentInformation.Setup(r => r.DegreePercentage).Returns(0.01m);

			var exciseDuty = new Mock<IExciseDuty>();
			exciseDuty.Setup(d => d.Code).Returns("A123");
			exciseDuty.Setup(d => d.DegreePercentage).Returns(0.02m);
			exciseDuty.Setup(d => d.Value).Returns(1626.28m);
			exciseDuty.Setup(d => d.Amount).Returns(GetAmount(18219, "NAR", "X").Object);

			var documents = new Mock<IImportLineDocument>();
			documents.Setup(d => d.Division).Returns("4");
			documents.Setup(d => d.DocumentType).Returns("7HHF");
			documents.Setup(d => d.ReferenceNumber).Returns("COSU6271657530");
			documents.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 08, 12));
			documents.Setup(d => d.AtHandFlag).Returns("J");
			documents.Setup(d => d.WriteOff).Returns(GetAmount(1500, "NAR", "Z").Object);

			goodsItemMock = new Mock<ISCIRECLine>();
			goodsItemMock.Setup(l => l.SequenceNumber).Returns(1);
			goodsItemMock.Setup(l => l.RequestedPreviousProcedure).Returns("4000");
			goodsItemMock.Setup(l => l.GoodsDescription).Returns("Anzüge, Kombinationen, Jacken, lange Hosen (einschließlich Kniebundhosen und ähnliche Hosen), Latzhosen und kurze Hosen (ausgenommen Badehosen), für Männer oder Knaben:");
			goodsItemMock.Setup(l => l.NetMassMeasure).Returns(11866.40005m);
			goodsItemMock.Setup(l => l.OriginCountry).Returns("CN");
			goodsItemMock.Setup(l => l.SupplementaryInformation).Returns("Positionszusatz");
			goodsItemMock.Setup(l => l.CommodityCode).Returns("62034311000");
			goodsItemMock.Setup(l => l.AdditionalProcedure).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.SupplementaryCodes).Returns(new[] { "A12" });
			goodsItemMock.Setup(l => l.Package).Returns(package.Object);
			goodsItemMock.Setup(l => l.ForeignTradeStatisticsGrossMassMeasure).Returns(11860.5m);
			goodsItemMock.Setup(l => l.InwardMovementAmount).Returns(GetAmount(18220, "NAR", "X").Object);
			goodsItemMock.Setup(l => l.AssessmentCustomsValue).Returns(100628.6m);
			goodsItemMock.Setup(l => l.AssessmentAmount).Returns(new IAmount[] { GetAmount(18219, "NAR", "X").Object });
			goodsItemMock.Setup(l => l.AssessmentSpecificRate).Returns(new IImportSpecificRate[] { assessmentSpecificRate.Object });
			goodsItemMock.Setup(l => l.AssessmentContentInformation).Returns(new IContentInformation[] { assessmentContentInformation.Object });
			goodsItemMock.Setup(l => l.ExciseDuty).Returns(new IExciseDuty[] { exciseDuty.Object });
			goodsItemMock.Setup(l => l.Documents).Returns(new IImportLineDocument[] { documents.Object });
			goodsItemMock.Setup(m => m.RequestedPreferentialTreatment).Returns("200");
		}

		protected override void SetUp()
		{
			base.SetUp();

			SetupGoodsItem();
			var goodsItems = new Mock<ISummaryDeclarationGoodsItem>();
			goodsItems.Setup(x => x.Quantity).Returns(1);
			goodsItems.Setup(x => x.IdentificationByKeyKind).Returns("AWB");
			goodsItems.Setup(x => x.IdentificationByKeyNumber).Returns("1234567890");
			goodsItems.Setup(x => x.IdentificationByKeyCustodianIdentifier).Returns("DE3333333");
			goodsItems.Setup(x => x.IdentificationByRegistrationReferencedRegistrationNumber).Returns("ATA123456789123456789");
			goodsItems.Setup(x => x.IdentificationByRegistrationReferencedSequenceNumber).Returns(1);

			summaryDeclarationMock = new Mock<ISummaryDeclaration>();
			summaryDeclarationMock.Setup(x => x.IdentificationIndicator).Returns("AWB");
			summaryDeclarationMock.Setup(x => x.GoodsItems)
				.Returns(new ISummaryDeclarationGoodsItem[] { goodsItems.Object });

			var identification1 = new Mock<IPartyID>();
			identification1.Setup(id => id.EoriNumber).Returns("GR234567890");
			identification1.Setup(id => id.EoriBranchSuffix).Returns("0001");
			var declarant = new Mock<IImportParty>();
			declarant.Setup(v => v.Identification).Returns(identification1.Object);

			var identification2 = new Mock<IPartyID>();
			identification2.Setup(id => id.EoriNumber).Returns("GR345678901");
			identification2.Setup(id => id.EoriBranchSuffix).Returns("0002");
			var representative = new Mock<IImportParty>();
			representative.Setup(v => v.Identification).Returns(identification2.Object);

			var identification3 = new Mock<IPartyID>();
			identification3.Setup(id => id.EoriNumber).Returns("GR456789012");
			identification3.Setup(id => id.EoriBranchSuffix).Returns("0003");
			var principal = new Mock<IImportParty>();
			principal.Setup(v => v.Identification).Returns(identification3.Object);

			var identification4 = new Mock<IPartyID>();
			identification4.Setup(id => id.EoriNumber).Returns("GR667890124");
			identification4.Setup(id => id.EoriBranchSuffix).Returns("0005");

			var address = new Mock<IImportPartyIdAddress>();
			address.Setup(m => m.Name).Returns("Vasya Pupkin");
			address.Setup(m => m.Country).Returns("IR");
			address.Setup(m => m.Address).Returns("1st Ave");
			address.Setup(m => m.City).Returns("Dublin");
			address.Setup(m => m.District).Returns("District");
			address.Setup(m => m.Postcode).Returns("10225");

			var consignee = new Mock<IImportParty>();
			consignee.Setup(v => v.Identification).Returns(identification4.Object);
			consignee.Setup(v => v.Address).Returns(address.Object);

			var contactPerson = new Mock<IImportPartyContactPerson>();
			contactPerson.Setup(v => v.MailAddress).Returns("bob.baumeister@samplefreight.com");
			contactPerson.Setup(v => v.PersonName).Returns("Bob Baumeister");
			contactPerson.Setup(v => v.PhoneNumber).Returns("06131-477447");
			contactPerson.Setup(v => v.Position).Returns("Sachbearbeiter");

			var documents1 = new Mock<IImportDocument>();
			documents1.Setup(d => d.Type).Returns("N380");
			documents1.Setup(d => d.ReferenceNumber).Returns("DOC1REFERENCE");
			documents1.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 10, 23));

			var documents2 = new Mock<IImportDocument>();
			documents2.Setup(d => d.Type).Returns("X123");
			documents2.Setup(d => d.ReferenceNumber).Returns("DOC2REFERENCE");
			documents2.Setup(d => d.IssuingDate).Returns(new DateTime(2020, 10, 24));

			headerMock = new Mock<ISCIRECHeader>();
			headerMock.Setup(h => h.DeclarationKind).Returns("F");
			headerMock.Setup(h => h.DeclarationType).Returns("AAV");
			headerMock.Setup(h => h.LocalReferenceNumber).Returns("ABC12345");
			headerMock.Setup(h => h.LocalClearanceDate).Returns(new DateTime(2021, 04, 20));
			headerMock.Setup(h => h.PrematureInputFlag).Returns(true);
			headerMock.Setup(h => h.GoodsItemQuantity).Returns(10);
			headerMock.Setup(h => h.CustomsGoodsStatus).Returns("EU");
			headerMock.Setup(h => h.LocalClearanceProcedure).Returns("DE001234567");
			headerMock.Setup(h => h.ProcedureAuthorisation).Returns("DE001234568");
			headerMock.Setup(h => h.GoodsLocation).Returns("locationOfGoods");
			headerMock.Setup(h => h.DepartureCountry).Returns("LV");
			headerMock.Setup(h => h.CurrencyCode).Returns("USD");
			headerMock.Setup(h => h.AdditionalInformation).Returns("AdditionalInformation");
			headerMock.Setup(h => h.RepresentativeRelationshipFlag).Returns("0");
			headerMock.Setup(h => h.DeclarationPlace).Returns("Hamburg");
			headerMock.Setup(h => h.BorderTransportMeansMode).Returns("1");
			headerMock.Setup(h => h.BorderTransportMeansType).Returns("07");
			headerMock.Setup(h => h.BorderTransportMeansInformation).Returns("AA-BB123");
			headerMock.Setup(h => h.BorderTransportMeansNationality).Returns("FR");
			headerMock.Setup(h => h.ArrivalTransportMeansIdentity).Returns("XX-AA123");
			headerMock.Setup(h => h.PreviousAdministrativeReferenceType).Returns("ATNEU");
			headerMock.Setup(h => h.PreviousAdministrativeReferenceNumber).Returns("REFNUM12345");
			headerMock.Setup(h => h.SummaryDeclaration).Returns(summaryDeclarationMock.Object);
			headerMock.Setup(h => h.ContainerFlag).Returns("J");
			headerMock.Setup(h => h.ContainerIdentificationNumbers).Returns(new List<string> { "CONT1", "CONT2" });
			headerMock.Setup(h => h.DeliveryTermsCode).Returns("CFR");
			headerMock.Setup(h => h.DeliveryTermsDescription).Returns("Description");
			headerMock.Setup(h => h.ForeignTradeStatisticsInlandTransportMode).Returns("3");
			headerMock.Setup(h => h.ForeignTradeStatisticsTotalGrossMassMeasure).Returns(123456789.1m);
			headerMock.Setup(h => h.Declarant).Returns(declarant.Object);
			headerMock.Setup(h => h.Representative).Returns(representative.Object);
			headerMock.Setup(h => h.Principal).Returns(principal.Object);
			headerMock.Setup(h => h.Consignee).Returns(consignee.Object);
			headerMock.Setup(h => h.ContactPerson).Returns(contactPerson.Object);
			headerMock.Setup(h => h.Documents).Returns(new IImportDocument[] { documents1.Object, documents2.Object });
			headerMock.Setup(m => m.Lines).Returns(new ISCIRECLine[] { goodsItemMock.Object });

			var interchangeSender = new Mock<IPartyID>();
			interchangeSender.Setup(i => i.EoriNumber).Returns("DE8999783");
			interchangeSender.Setup(i => i.EoriBranchSuffix).Returns("0000");

			messageHeaderMock = new Mock<IImportMessageHeader>();
			messageHeaderMock.Setup(h => h.InterchangeSender).Returns(interchangeSender.Object);
			messageHeaderMock.Setup(h => h.InterchangeRecipientID).Returns("DE005875");
			messageHeaderMock.Setup(h => h.AuthorisationNumber).Returns("1234567890AUTH");
			messageHeaderMock.Setup(h => h.MessageGroup).Returns("ZBE");
			messageHeaderMock.Setup(h => h.Header).Returns(headerMock.Object);
			messageHeaderMock.Setup(m => m.PreparationDateAndTimeCET).Returns(new CentralEuropeanStandardDateAndTimeProvider(true));

			scirecBuilder = new SCIRECMessageBuilder(messageHeaderMock.Object);
		}

		SCIRECMessageBuilder scirecBuilder;
		Mock<ISCIRECHeader> headerMock;
		Mock<IImportMessageHeader> messageHeaderMock;
		Mock<ISummaryDeclaration> summaryDeclarationMock;
		Mock<ISCIRECLine> goodsItemMock;

		const string ExpectedSummaryDeclaration = @"<SummaryDeclaration>
		<IdentificationIndicator>AWB</IdentificationIndicator>
		<GoodsItem>
			<Quantity>1</Quantity>
			<IdentificationByKey>
				<Kind>AWB</Kind>
				<Number>1234567890</Number>
				<Custodian>
					<Identification>
						<ReferenceNumber>DE3333333</ReferenceNumber>
					</Identification>
				</Custodian>
			</IdentificationByKey>
		</GoodsItem>
	</SummaryDeclaration>";

		const string ExpectedCustomsWarehouse = @"<CustomsWarehouse>
		<SequenceNumber>1</SequenceNumber>
		<GoodsItemQuantity>10</GoodsItemQuantity>
		<CustomsAuthorisation>
			<WarehouseOwner>DE44444444</WarehouseOwner>
		</CustomsAuthorisation>
		<LocalReferenceNumber>WTG5678</LocalReferenceNumber>
		<GoodsItem>
			<SequenceNumber>1</SequenceNumber>
			<ReferencedRegistrationNumber>ATA123456789123456789</ReferencedRegistrationNumber>
			<ReferencedSequenceNumber>1</ReferencedSequenceNumber>
			<AccessViaAtlasFlag>J</AccessViaAtlasFlag>
			<CommodityCode>12345678</CommodityCode>
			<UsualProcessingFlag>J</UsualProcessingFlag>
			<Complement>Description text</Complement>
			<CommercialAmount>
				<Quantity>1.234</Quantity>
				<MeasurementUnit>KGM</MeasurementUnit>
				<Qualifier>A</Qualifier>
			</CommercialAmount>
			<DebitAmount>
				<Quantity>5.678</Quantity>
				<MeasurementUnit>KGM</MeasurementUnit>
				<Qualifier>M</Qualifier>
			</DebitAmount>
		</GoodsItem>
	</CustomsWarehouse>";

		const string ExpectedInwardProcessing = @"<InwardProcessing>
		<SequenceNumber>1</SequenceNumber>
		<GoodsItemQuantity>20</GoodsItemQuantity>
		<CustomsAuthorisation>
			<ProcessingOwner>DE55555555</ProcessingOwner>
		</CustomsAuthorisation>
		<SimplifiedGrantAuthorisationFlag>J</SimplifiedGrantAuthorisationFlag>
		<MonitoringCustomsOffice>
			<Identification>
				<ReferenceNumber>DE001234</ReferenceNumber>
			</Identification>
		</MonitoringCustomsOffice>
		<GoodsItem>
			<SequenceNumber>1</SequenceNumber>
			<ReferencedRegistrationNumber>ATA123456789123456789</ReferencedRegistrationNumber>
			<ReferencedSequenceNumber>1</ReferencedSequenceNumber>
			<AccessViaAtlasFlag>J</AccessViaAtlasFlag>
			<GoodsRelatedInformation>Related information</GoodsRelatedInformation>
		</GoodsItem>
	</InwardProcessing>";
	}
}
