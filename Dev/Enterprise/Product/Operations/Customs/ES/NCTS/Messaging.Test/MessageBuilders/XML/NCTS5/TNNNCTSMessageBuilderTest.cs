using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTNNC_v515.CCTNNCV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(TNNNCTSMessageBuilder))]
	class TNNNCTSMessageBuilderTest : NCTSCommonMessageBuilderTest<TNNNCTSMessageBuilder, ITNNNCTSMessageDataProvider, Cctnncv1Ent>
	{
		#region Tests
		public override void TestConstructor()
		{
			AssertExceptionThrown<ArgumentNullException>("When provider is null", () => CreateMessageBuilderWithNullProvider());
		}

		[TestDate(2020, 1, 9, 15, 13, 23, 456)]
		public override void TestCreateEDIMessage()
		{
			var messageBuilder = CreateMessageBuilder();

			CombineAssertions(() =>
			{
				AssertEquals("messageBuilder.MessageType", ExpectedMessageType, messageBuilder.MessageType);
				AssertEquals("messageBuilder.MessageSubType", ExpectedMessageSubType, messageBuilder.MessageSubType);
				AssertEquals("messageBuilder.Provider", mockProvider.Object, messageBuilder.Provider);
				AssertUnsignedMessageText(messageBuilder.UnsignedMessageText);
				AssertSignedMessageText(messageBuilder.GetSignedMessageText());
			});
		}

		public void TestPopulatePhaseID()
		{
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PhaseID", messageText);
		}

		public void TestPopulateTransitOperation()
		{
			mockProvider.Setup(m => m.TransitOperation).Returns((ITNNNCTSTransitOperation)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransitOperation>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateHolderOfTheTransitProcedure()
		{
			mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns((INCTSCommonHolderOfTheTransitProcedureWithAddress)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}HolderOfTheTransitProcedure>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateAddress()
		{
			mockProvider.Setup(m => m.HolderOfTheTransitProcedure.Address).Returns((INCTSCommonAddressInfo)null);
			mockProvider.Setup(m => m.Consignment.Consignor.Address).Returns((INCTSCommonAddressInfo)null);
			mockProvider.Setup(m => m.Consignment.CommonConsignmentData.Consignee.Address).Returns((INCTSCommonAddressInfo)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Address>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateTraderAtDestination()
		{
			mockProvider.Setup(m => m.TraderAtDestination).Returns((IPartyIdProvider)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TraderAtDestination>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateRepresentativeAtDestination()
		{
			mockProvider.Setup(m => m.RepresentativeAtDestination).Returns((IPartyIdProvider)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}RepresentanteEnDestino>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateDigitizedDocument()
		{
			mockProvider.Setup(m => m.DigitizedDocument).Returns((ITNNNCTSDigitizedDocument)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}DocumentoDigitalizado>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateConsignment()
		{
			mockProvider.Setup(m => m.Consignment).Returns((ITNNNCTSConsignment)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Consignment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateConsignor()
		{
			mockProvider.Setup(m => m.Consignment.Consignor).Returns((INCTSPartyNameProviderWithAddress)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Consignor>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateConsignee()
		{
			mockProvider.Setup(m => m.Consignment.CommonConsignmentData.Consignee).Returns((INCTSPartyNameProviderWithAddress)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Consignee>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateTransportEquipment()
		{
			mockProvider.Setup(m => m.Consignment.TransportEquipment).Returns((IReadOnlyCollection<INCTSCommonTransportEquipment>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportEquipment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateSeals()
		{
			mockTransportEquipment1.Setup(m => m.Seals).Returns((IReadOnlyCollection<ISealCommon>)null);
			mockProvider.Setup(m => m.Consignment.TransportEquipment).Returns(new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Seal>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateGoodsReference()
		{
			mockTransportEquipment1.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<INCTSCommonGoodsReference>)null);
			mockProvider.Setup(m => m.Consignment.TransportEquipment).Returns(new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsReference>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateDepartureTransportMeans()
		{
			mockProvider.Setup(m => m.Consignment.DepartureTransportMeans).Returns((IReadOnlyCollection<ICommonDepartureTransportMeans>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}DepartureTransportMeans>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateCountryOfRouting()
		{
			mockProvider.Setup(m => m.Consignment.CommonConsignmentData.CountryOfRoutingOfConsignment).Returns((IReadOnlyCollection<ICommonCountryOfRoutingOfConsignment>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CountryOfRoutingOfConsignment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateActiveBorderTransportMeans()
		{
			mockProvider.Setup(m => m.Consignment.ActiveBorderTransportMeans).Returns((IReadOnlyCollection<INCTSCommonActiveBorderTransportMeans>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}ActiveBorderTransportMeans>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateTransportCharges()
		{
			mockProvider.Setup(m => m.Consignment.CommonConsignmentData.MethodOfPayment).Returns("");
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportCharges>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateHouseConsignment()
		{
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns((IReadOnlyCollection<ITNNNCTSHouseConsignment>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}HouseConsignment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateConsignmentItem()
		{
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns((IReadOnlyCollection<ITNNNCTSConsignmentItem>)null);
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}ConsignmentItem>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateCommodity()
		{
			mockLine1.Setup(m => m.Commodity).Returns((ITNNNCTSCommodity)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new ITNNNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Commodity>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateCommodityCode()
		{
			mockLine1.Setup(m => m.Commodity.CommodityCode).Returns((INCTSCommonCommodityCode)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new ITNNNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CommodityCode>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateDangerousGoods()
		{
			mockLine1.Setup(m => m.Commodity.DangerousGoods).Returns((IReadOnlyCollection<ICommonDangerousGoods>)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new ITNNNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}DangerousGoods>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateGoodsMeasure()
		{
			mockLine1.Setup(m => m.Commodity.GoodsMeasure).Returns((INCTSCommonGoodsMeasure)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new ITNNNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsMeasure>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulatePackaging()
		{
			mockLine1.Setup(m => m.Packaging).Returns((IReadOnlyCollection<INCTSCommonPackaging>)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new ITNNNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Packaging>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateSupportingDocument()
		{
			mockProvider.Setup(m => m.Consignment.CommonConsignmentData.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithItem>)null);
			mockHouseConsignment1.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithItem>)null);
			mockLine1.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithInfo>)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new ITNNNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}SupportingDocument>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateTransportDocument()
		{
			mockProvider.Setup(m => m.Consignment.CommonConsignmentData.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockHouseConsignment1.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockLine1.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new ITNNNCTSConsignmentItem[] { mockLine1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportDocument>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateAdditionalReference()
		{
			mockProvider.Setup(m => m.Consignment.CommonConsignmentData.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockHouseConsignment1.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockLine1.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new ITNNNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}AdditionalReference>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateAdditionalInfo()
		{
			mockProvider.Setup(m => m.Consignment.CommonConsignmentData.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockHouseConsignment1.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockLine1.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new ITNNNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}AdditionalInformation>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Ncts5IndirectDepartureRegistration;

		protected override TNNNCTSMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new TNNNCTSMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override TNNNCTSMessageBuilder CreateMessageBuilderWithNullProvider() => new TNNNCTSMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(NCTSXMLTestFileConstants.NCTSTestFilePath, "TestTNNNCTS.txt");

		#region Structures SetUp

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MessageSender).Returns("A78587268");
			mockProvider.Setup(m => m.MessageIdentification).Returns("20221213141220798000");
			mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);
			mockProvider.Setup(m => m.TransitOperation).Returns(SetUpTNNTransitOperation().Object);

			mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("ES000101");
			mockProvider.Setup(m => m.CustomsOfficeOfDestinationDeclared).Returns("CH006251");
			mockProvider.Setup(m => m.CustomsOfficeOfDestinationActual).Returns("FR002020");

			mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(SetUpHolderOfTheTransitProcedureWithAddress().Object);
			mockProvider.Setup(m => m.TraderAtDestination).Returns(BuilderHelperTest.SetUpPartyId("A87654321"));
			mockProvider.Setup(m => m.RepresentativeAtDestination).Returns(BuilderHelperTest.SetUpPartyId("A12345678"));

			var mockAnnex1 = SetUpDigitizedDocument("A", "location1", "1Desc", "BTI", new byte[] { 0, 1 }, "PDF");
			mockProvider.Setup(m => m.DigitizedDocument).Returns(mockAnnex1.Object);

			mockConsignment = SetUpTNNConsignment();
			mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		}

		Mock<ITNNNCTSConsignment> mockConsignment;
		Mock<INCTSCommonTransportEquipment> mockTransportEquipment1;
		Mock<ITNNNCTSHouseConsignment> mockHouseConsignment1;
		Mock<ITNNNCTSConsignmentItem> mockLine1;
		Mock<ITNNNCTSCommodity> mockCommodity;

		Mock<ITNNNCTSTransitOperation> SetUpTNNTransitOperation()
		{
			var mockTransitOperation = new Mock<ITNNNCTSTransitOperation>();
			mockTransitOperation.Setup(m => m.MRN).Returns("22ES000101100909B7");
			mockTransitOperation.Setup(m => m.CommonTransitOperation).Returns(SetUpTransitOperationCommon().Object);
			mockTransitOperation.Setup(m => m.DeclarationAcceptanceDate).Returns(new ZDateTime(2022, 12, 15));
			mockTransitOperation.Setup(m => m.ReleaseDate).Returns(new ZDateTime(2022, 10, 20));
			return mockTransitOperation;
		}

		Mock<ITNNNCTSDigitizedDocument> SetUpDigitizedDocument(ZString type, ZString location, ZString description, ZString referenceNumber, ZBlob image, ZString extension)
		{
			var mockDoc = new Mock<ITNNNCTSDigitizedDocument>();
			mockDoc.Setup(m => m.DocumentType).Returns(type);
			mockDoc.Setup(m => m.DocumentLocation).Returns(location);
			mockDoc.Setup(m => m.Description).Returns(description);
			mockDoc.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
			mockDoc.Setup(m => m.Image).Returns(image);
			mockDoc.Setup(m => m.Extension).Returns(extension);

			return mockDoc;
		}

		Mock<ITNNNCTSConsignment> SetUpTNNConsignment()
		{
			var mockConsignment = new Mock<ITNNNCTSConsignment>();
			mockConsignment.Setup(m => m.ContainerIndicator).Returns(ZBool.True);
			mockConsignment.Setup(m => m.InlandModeOfTransport).Returns("2");
			mockConsignment.Setup(m => m.ModeOfTransportAtTheBorder).Returns("3");

			mockTransportEquipment1 = SetUpTransportEquipment("1", "CSQU3054383", "2", SetUpSeals(), SetUpGoodsReference());
			var mockTransportEquipment2 = SetUpTransportEquipment("2", "CSQJ3054383", "0", Enumerable.Empty<ISealCommon>(), Enumerable.Empty<INCTSCommonGoodsReference>());
			var mockTransportEquipments = new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };
			mockConsignment.Setup(m => m.TransportEquipment).Returns(mockTransportEquipments);

			var mockDepartureTransportMeans1 = SetUpDepartureTransportMeans("1", "20", "Transport", "ES");
			var mockDepartureTransportMeans2 = SetUpDepartureTransportMeans("2", "21", "Ship", "PT");
			var mockDepatureTransportMeans = new ICommonDepartureTransportMeans[] { mockDepartureTransportMeans1.Object, mockDepartureTransportMeans2.Object };
			mockConsignment.Setup(m => m.DepartureTransportMeans).Returns(mockDepatureTransportMeans);

			var mockSupportingDocConsigmentAndHouse1 = SetUpCommonDocumentWithItem("1", "N821", "DocRef1", "Complement1", "1");
			var mockSupportingDocConsigmentAndHouse2 = SetUpCommonDocumentWithItem("2", "N380", "DocRef2", "Complement2", "2");
			var mockSupportingDocsConsigmentAndHouse = new INCTSCommonDocumentWithItem[] { mockSupportingDocConsigmentAndHouse1.Object, mockSupportingDocConsigmentAndHouse2.Object };

			var mockTransportDoc1 = SetUpCommonDocument("1", "N785", "DocRef3");
			var mockTransportDoc2 = SetUpCommonDocument("2", "N705", "DocRef4");
			var mockTransportDocs = new ICommonDocumentSequenceNumber[] { mockTransportDoc1.Object, mockTransportDoc2.Object };

			var mockAdditionalRef1 = SetUpCommonDocument("1", "Y025", "DocRef5");
			var mockAdditionalRef2 = SetUpCommonDocument("2", "Y026", "DocRef6");
			var mockAdditionalRefs = new ICommonDocumentSequenceNumber[] { mockAdditionalRef1.Object, mockAdditionalRef2.Object };

			var mockAdditionalInfo1 = SetUpCommonDocument("1", "20100", "Text1");
			var mockAdditionalInfo2 = SetUpCommonDocument("2", "20200", "Text2");
			var mockAdditionalInfos = new ICommonDocumentSequenceNumber[] { mockAdditionalInfo1.Object, mockAdditionalInfo2.Object };

			var mockCommonConsignmentData = new Mock<INCTSCommonConsignmentDepartureAndAmendmentAndTNN>();
			mockCommonConsignmentData.Setup(m => m.CountryOfDispatch).Returns("PT");
			mockCommonConsignmentData.Setup(m => m.CountryOfDestination).Returns("ES");
			mockCommonConsignmentData.Setup(m => m.GrossMass).Returns(60.204m);
			mockCommonConsignmentData.Setup(m => m.ReferenceNumberUCR).Returns("UCR");
			mockCommonConsignmentData.Setup(m => m.Consignee).Returns(SetUpNCTSPartyNameProviderWithAddress().Object);
			mockCommonConsignmentData.Setup(m => m.SupportingDocument).Returns(mockSupportingDocsConsigmentAndHouse);
			mockCommonConsignmentData.Setup(m => m.TransportDocument).Returns(mockTransportDocs);
			mockCommonConsignmentData.Setup(m => m.AdditionalReference).Returns(mockAdditionalRefs);
			mockCommonConsignmentData.Setup(m => m.AdditionalInformation).Returns(mockAdditionalInfos);

			var mockCountryOfRouting1 = SetUpCommonCountryOfRouting("1", "ES");
			var mockCountryOfRouting2 = SetUpCommonCountryOfRouting("2", "PT");
			var mockCountriesOfRouting = new ICommonCountryOfRoutingOfConsignment[] { mockCountryOfRouting1.Object, mockCountryOfRouting2.Object };
			mockCommonConsignmentData.Setup(m => m.CountryOfRoutingOfConsignment).Returns(mockCountriesOfRouting);
			mockCommonConsignmentData.Setup(m => m.MethodOfPayment).Returns("A");
			mockConsignment.Setup(m => m.CommonConsignmentData).Returns(mockCommonConsignmentData.Object);

			mockConsignment.Setup(m => m.Consignor).Returns(SetUpNCTSPartyNameProviderWithAddress("ES12345678A", "Consignor").Object);

			var mockActiveBorderTransportMeans1 = SetUpActiveBorderTransportMeans("1", "21", "Transport", "ES", "Conveyance1");
			var mockActiveBorderTransportMeans2 = SetUpActiveBorderTransportMeans("2", "22", "Ship", "PT", "Conveyance2");
			var mockActiveBorderTransportMeans = new INCTSCommonActiveBorderTransportMeans[] { mockActiveBorderTransportMeans1.Object, mockActiveBorderTransportMeans2.Object };
			mockConsignment.Setup(m => m.ActiveBorderTransportMeans).Returns(mockActiveBorderTransportMeans);

			mockCommodity = SetUpTNNCommodity();

			var mockPackage1 = SetUpCommonPackages("1", "NE", "16", "Mark1");
			var mockPackage2 = SetUpCommonPackages("2", "BX", "1", "Mark2");
			var mockPackages = new INCTSCommonPackaging[] { mockPackage1.Object, mockPackage2.Object };

			var mockSupportingDoc1 = SetUpCommonDocumentWithInfo("1", "N821", "DocRef1", "INFO");
			var mockSupportingDoc2 = SetUpCommonDocumentWithInfo("2", "N380", "DocRef2", "INFO2");
			var mockSupportingDocs = new INCTSCommonDocumentWithInfo[] { mockSupportingDoc1.Object, mockSupportingDoc2.Object };

			mockLine1 = SetUpTNNNCTSConsignmentItem("1", "1", "T1", "ES", "UCRI1", mockPackages, mockSupportingDocs, mockTransportDocs, mockAdditionalRefs, mockAdditionalInfos, mockCommodity.Object);
			var mockLine2 = SetUpTNNNCTSConsignmentItem("2", "2", "T2", "IT", "UCRI2", Enumerable.Empty<INCTSCommonPackaging>(), Enumerable.Empty<INCTSCommonDocumentWithInfo>(),
				Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>());
			var mockLines = new ITNNNCTSConsignmentItem[] { mockLine1.Object, mockLine2.Object };

			mockHouseConsignment1 = SetUpTNNNCTSHouseConsignment("1", 60.204m, "UCRHC1", mockLines, mockSupportingDocsConsigmentAndHouse, mockTransportDocs, mockAdditionalRefs, mockAdditionalInfos);
			var mockHouseConsignment2 = SetUpTNNNCTSHouseConsignment("2", 10.000m, "UCRHC2", Enumerable.Empty<ITNNNCTSConsignmentItem>(), Enumerable.Empty<INCTSCommonDocumentWithItem>(),
				Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>());
			var mockHouseConsignments = new ITNNNCTSHouseConsignment[] { mockHouseConsignment1.Object, mockHouseConsignment2.Object };
			mockConsignment.Setup(m => m.HouseConsignment).Returns(mockHouseConsignments);

			return mockConsignment;
		}

		Mock<ITNNNCTSHouseConsignment> SetUpTNNNCTSHouseConsignment(ZString sequenceNumber, ZDecimal grossMass, ZString ucrReference, IEnumerable<ITNNNCTSConsignmentItem> lines,
			IEnumerable<INCTSCommonDocumentWithItem> supportingDocuments, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments,
			IEnumerable<ICommonDocumentSequenceNumber> additionalRef, IEnumerable<ICommonDocumentSequenceNumber> additionalInfo)
		{
			var mockHouseConsignment = new Mock<ITNNNCTSHouseConsignment>();
			mockHouseConsignment.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
			mockHouseConsignment.Setup(m => m.GrossMass).Returns(grossMass);
			mockHouseConsignment.Setup(m => m.ReferenceNumberUCR).Returns(ucrReference);
			mockHouseConsignment.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithItem>)supportingDocuments);
			mockHouseConsignment.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transportDocuments);
			mockHouseConsignment.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalRef);
			mockHouseConsignment.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalInfo);
			mockHouseConsignment.Setup(m => m.ConsignmentItem).Returns((IReadOnlyCollection<ITNNNCTSConsignmentItem>)lines);
			return mockHouseConsignment;
		}

		Mock<ITNNNCTSConsignmentItem> SetUpTNNNCTSConsignmentItem(ZString lineNumber, ZString goodsItemNumber, ZString declarationType, ZString countryOfDestination, ZString reference,
			IEnumerable<INCTSCommonPackaging> packages, IEnumerable<INCTSCommonDocumentWithInfo> supportingDocuments, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments,
			IEnumerable<ICommonDocumentSequenceNumber> additionalRef, IEnumerable<ICommonDocumentSequenceNumber> additionalInfo, ITNNNCTSCommodity commodity = null)
		{
			var mockLine = new Mock<ITNNNCTSConsignmentItem>();
			mockLine.Setup(m => m.GoodsItemNumber).Returns(lineNumber);
			mockLine.Setup(m => m.DeclarationGoodsItemNumber).Returns(goodsItemNumber);
			mockLine.Setup(m => m.DeclarationType).Returns(declarationType);
			mockLine.Setup(m => m.CountryOfDestination).Returns(countryOfDestination);
			mockLine.Setup(m => m.ReferenceNumberUCR).Returns(reference);
			mockLine.Setup(m => m.Commodity).Returns(commodity);
			mockLine.Setup(m => m.Packaging).Returns((IReadOnlyCollection<INCTSCommonPackaging>)packages);
			mockLine.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithInfo>)supportingDocuments);
			mockLine.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transportDocuments);
			mockLine.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalRef);
			mockLine.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalInfo);
			return mockLine;
		}

		Mock<ITNNNCTSCommodity> SetUpTNNCommodity()
		{
			var mockCommodity = new Mock<ITNNNCTSCommodity>();
			mockCommodity.Setup(m => m.DescriptionOfGoods).Returns("Description1");
			mockCommodity.Setup(m => m.CommodityCode).Returns(SetUpCommonCommodityCode().Object);

			var mockDangerousGoods1 = SetUpCommonDangerousGoods("1", "0004");
			var mockDangerousGoods2 = SetUpCommonDangerousGoods("2", "0002");
			var mockDangerousGoods = new ICommonDangerousGoods[] { mockDangerousGoods1.Object, mockDangerousGoods2.Object };
			mockCommodity.Setup(m => m.DangerousGoods).Returns(mockDangerousGoods);

			var mockGoodsMeasure = new Mock<INCTSCommonGoodsMeasure>();
			mockGoodsMeasure.Setup(m => m.GrossMass).Returns(30.102m);
			mockGoodsMeasure.Setup(m => m.NetMass).Returns(19.000m);
			mockCommodity.Setup(m => m.GoodsMeasure).Returns(mockGoodsMeasure.Object);

			return mockCommodity;
		}

		#endregion
	}
}
