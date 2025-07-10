using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC044C_v515.CC044CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(NotifUnloadingNCTSMessageBuilder))]
	class NotifUnloadingNCTSMessageBuilderTest : NCTSCommonMessageBuilderTest<NotifUnloadingNCTSMessageBuilder, INotifUnloadingNCTSMessageDataProvider, Cc044Cv1Ent>
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
			mockProvider.Setup(m => m.TransitOperation).Returns((INotifUnloadingNCTSTransitOperation)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransitOperation>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateCustomsOfficeOfDestinationActual()
		{
			mockProvider.Setup(m => m.CustomsOfficeOfDestinationActual).Returns("");
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfDestinationActual>", CreateMessageBuilder().GetSignedMessageText());
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

		public void TestPopulateUnloadingRemarks()
		{
			mockProvider.Setup(m => m.UnloadingRemarks).Returns((IUnloadingRemarksNCTS)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}UnloadingRemark>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateUnloadingRemarksStateOfSealsNotSpecified()
		{
			mockProvider.Setup(m => m.UnloadingRemarks.StateOfSealsSpecified).Returns(false);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}UnloadingRemark>", CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}stateOfSeals>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateUnloadingRemarksStateOfSealsFalse()
		{
			mockProvider.Setup(m => m.UnloadingRemarks.StateOfSeals).Returns(false);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}UnloadingRemark>", CreateMessageBuilder().GetSignedMessageText());
				AssertContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}stateOfSeals>0", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateConsignment()
		{
			mockProvider.Setup(m => m.Consignment).Returns((INotifUnloadingConsignment)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Consignment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateGrossMassZero()
		{
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns((IReadOnlyCollection<INotifUnloadingNCTSHouseConsignment>)null);
			mockProvider.Setup(m => m.Consignment.GrossMass).Returns(ZDecimal.Zero);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}grossMass>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateGrossMassFlagFalse()
		{
			mockLine1.Setup(m => m.Commodity.GoodsMeasure.GrossMassSpecified).Returns(false);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object });
			mockHouseConsignment1.Setup(m => m.GrossMassSpecified).Returns(false);
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			mockProvider.Setup(m => m.Consignment.GrossMassSpecified).Returns(false);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}grossMass>", CreateMessageBuilder().GetSignedMessageText());
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
			mockHouseConsignment1.Setup(m => m.DepartureTransportMeans).Returns((IReadOnlyCollection<ICommonDepartureTransportMeans>)null);
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}DepartureTransportMeans>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateSupportingDocument()
		{
			mockProvider.Setup(m => m.Consignment.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithInfo>)null);
			mockHouseConsignment1.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithInfo>)null);
			mockLine1.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithInfo>)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}SupportingDocument>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateTransportDocument()
		{
			mockProvider.Setup(m => m.Consignment.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockHouseConsignment1.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockLine1.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportDocument>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateAdditionalReference()
		{
			mockProvider.Setup(m => m.Consignment.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockHouseConsignment1.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockLine1.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}AdditionalReference>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateHouseConsignment()
		{
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns((IReadOnlyCollection<INotifUnloadingNCTSHouseConsignment>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}HouseConsignment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateConsignmentItem()
		{
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns((IReadOnlyCollection<INotifUnloadingNCTSConsignmentItem>)null);
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}ConsignmentItem>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateCommodity()
		{
			mockLine1.Setup(m => m.Commodity).Returns((INotifUnloadingCommodity)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Commodity>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateCommodityCode()
		{
			mockLine1.Setup(m => m.Commodity.CommodityCode).Returns((INCTSCommonCommodityCode)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CommodityCode>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateGoodsMeasure()
		{
			mockLine1.Setup(m => m.Commodity.GoodsMeasure).Returns((INCTSCommonGoodsMeasure)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsMeasure>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateNetMass()
		{
			mockLine1.Setup(m => m.Commodity.GoodsMeasure.NetMassSpecified).Returns(false);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsMeasure>", CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}netMass>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulatePackaging()
		{
			mockLine1.Setup(m => m.Packaging).Returns((IReadOnlyCollection<INCTSCommonPackaging>)null);
			mockHouseConsignment1.Setup(m => m.ConsignmentItem).Returns(new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object });
			mockProvider.Setup(m => m.Consignment.HouseConsignment).Returns(new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Packaging>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		#endregion
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Ncts5ArrivalDownloadGoods;

		protected override NotifUnloadingNCTSMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new NotifUnloadingNCTSMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override NotifUnloadingNCTSMessageBuilder CreateMessageBuilderWithNullProvider() => new NotifUnloadingNCTSMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(NCTSXMLTestFileConstants.NCTSTestFilePath, "TestNotifUnloading.txt");

		#region Structures SetUp

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MessageSender).Returns("A78587268");
			mockProvider.Setup(m => m.MessageIdentification).Returns("20221213141220798000");
			mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);
			mockProvider.Setup(m => m.TransitOperation).Returns(SetUpNotifUnloadingTransitOperation().Object);
			mockProvider.Setup(m => m.CustomsOfficeOfDestinationActual).Returns("ES000101");
			mockProvider.Setup(m => m.TraderAtDestination).Returns(BuilderHelperTest.SetUpPartyId("ES89890001K"));
			mockProvider.Setup(m => m.RepresentativeAtDestination).Returns(BuilderHelperTest.SetUpPartyId("ES89890001K"));

			mockProvider.Setup(m => m.UnloadingRemarks).Returns(SetUpUnloadingRemarksNCTS().Object);

			mockConsignment = SetUpNotifUnloadingConsignment();
			mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		}

		protected Mock<INotifUnloadingNCTSTransitOperation> SetUpNotifUnloadingTransitOperation()
		{
			var mockTransitOperation = new Mock<INotifUnloadingNCTSTransitOperation>();
			mockTransitOperation.Setup(m => m.MRN).Returns("22ES000101100909B7");
			mockTransitOperation.Setup(m => m.OtherThingsToReport).Returns("Other things");
			return mockTransitOperation;
		}

		protected Mock<IUnloadingRemarksNCTS> SetUpUnloadingRemarksNCTS()
		{
			var mockUnloadingRemark = new Mock<IUnloadingRemarksNCTS>();
			mockUnloadingRemark.Setup(m => m.Conform).Returns(false);
			mockUnloadingRemark.Setup(m => m.UnloadingDate).Returns(new ZDateTime(2022, 12, 15));
			mockUnloadingRemark.Setup(m => m.StateOfSeals).Returns(true);
			mockUnloadingRemark.Setup(m => m.StateOfSealsSpecified).Returns(true);
			mockUnloadingRemark.Setup(m => m.UnloadingRemark).Returns("Disconform");
			return mockUnloadingRemark;
		}

		Mock<INotifUnloadingConsignment> mockConsignment;
		Mock<INCTSCommonTransportEquipment> mockTransportEquipment1;
		Mock<INotifUnloadingNCTSHouseConsignment> mockHouseConsignment1;
		Mock<INotifUnloadingNCTSConsignmentItem> mockLine1;

		protected Mock<INotifUnloadingConsignment> SetUpNotifUnloadingConsignment()
		{
			var mockConsignment = new Mock<INotifUnloadingConsignment>();
			mockConsignment.Setup(m => m.GrossMass).Returns(60.204m);
			mockConsignment.Setup(m => m.GrossMassSpecified).Returns(true);

			mockTransportEquipment1 = SetUpTransportEquipment("1", "CSQU3054383", "2", SetUpSeals(), SetUpGoodsReference());
			var mockTransportEquipment2 = SetUpTransportEquipment("2", "CSQJ3054383", "0", Enumerable.Empty<ISealCommon>(), Enumerable.Empty<INCTSCommonGoodsReference>());
			var mockTransportEquipments = new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };
			mockConsignment.Setup(m => m.TransportEquipment).Returns(mockTransportEquipments);

			var mockDepartureTransportMeans1 = SetUpDepartureTransportMeans("1", "20", "Transport", "ES");
			var mockDepartureTransportMeans2 = SetUpDepartureTransportMeans("2", "21", "Ship", "PT");
			var mockDepatureTransportMeans = new ICommonDepartureTransportMeans[] { mockDepartureTransportMeans1.Object, mockDepartureTransportMeans2.Object };
			mockConsignment.Setup(m => m.DepartureTransportMeans).Returns(mockDepatureTransportMeans);

			var mockSupportingDoc1 = SetUpCommonDocumentWithInfo("1", "N821", "DocRef1", "Complement1");
			var mockSupportingDoc2 = SetUpCommonDocumentWithInfo("2", "N380", "DocRef2", "Complement2");
			var mockSupportingDocs = new INCTSCommonDocumentWithInfo[] { mockSupportingDoc1.Object, mockSupportingDoc2.Object };
			mockConsignment.Setup(m => m.SupportingDocument).Returns(mockSupportingDocs);

			var mockTransportDoc1 = SetUpCommonDocument("1", "N785", "DocRef3");
			var mockTransportDoc2 = SetUpCommonDocument("2", "N705", "DocRef4");
			var mockTransportDocs = new ICommonDocumentSequenceNumber[] { mockTransportDoc1.Object, mockTransportDoc2.Object };
			mockConsignment.Setup(m => m.TransportDocument).Returns(mockTransportDocs);

			var mockAdditionalRef1 = SetUpCommonDocument("1", "Y025", "DocRef5");
			var mockAdditionalRef2 = SetUpCommonDocument("2", "Y026", "DocRef6");
			var mockAdditionalRefs = new ICommonDocumentSequenceNumber[] { mockAdditionalRef1.Object, mockAdditionalRef2.Object };
			mockConsignment.Setup(m => m.AdditionalReference).Returns(mockAdditionalRefs);

			var mockPackage1 = SetUpCommonPackages("1", "NE", "16", "Mark1");
			var mockPackage2 = SetUpCommonPackages("2", "BX", "1", "Mark2");
			var mockPackages = new INCTSCommonPackaging[] { mockPackage1.Object, mockPackage2.Object };

			mockLine1 = SetUpNotifUnloadingCommonLine("1", "1", mockPackages, mockSupportingDocs, mockTransportDocs, mockAdditionalRefs, SetUpNotifUnloadingCommodity().Object);
			var mockLine2 = SetUpNotifUnloadingCommonLine("2", "2", Enumerable.Empty<INCTSCommonPackaging>(), Enumerable.Empty<INCTSCommonDocumentWithInfo>(),
				Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>());
			var mockLines = new INotifUnloadingNCTSConsignmentItem[] { mockLine1.Object, mockLine2.Object };

			mockHouseConsignment1 = SetUpNotifUnloadingHouseConsignment("1", 60.204m, mockDepatureTransportMeans, mockSupportingDocs, mockTransportDocs, mockAdditionalRefs, mockLines);
			var mockHouseConsignment2 = SetUpNotifUnloadingHouseConsignment("2", 10.000m, Enumerable.Empty<ICommonDepartureTransportMeans>(), Enumerable.Empty<INCTSCommonDocumentWithInfo>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<ICommonDocumentSequenceNumber>(), Enumerable.Empty<INotifUnloadingNCTSConsignmentItem>());
			var mockHouseConsignments = new INotifUnloadingNCTSHouseConsignment[] { mockHouseConsignment1.Object, mockHouseConsignment2.Object };
			mockConsignment.Setup(m => m.HouseConsignment).Returns(mockHouseConsignments);

			return mockConsignment;
		}

		protected Mock<INotifUnloadingCommodity> SetUpNotifUnloadingCommodity()
		{
			var mockCommodity = new Mock<INotifUnloadingCommodity>();
			mockCommodity.Setup(m => m.DescriptionOfGoods).Returns("Description1");
			mockCommodity.Setup(m => m.CusCode).Returns("CusCode");
			mockCommodity.Setup(m => m.CommodityCode).Returns(SetUpCommonCommodityCode().Object);

			var mockGoodsMeasure = new Mock<INCTSCommonGoodsMeasure>();
			mockGoodsMeasure.Setup(m => m.GrossMass).Returns(30.102m);
			mockGoodsMeasure.Setup(m => m.GrossMassSpecified).Returns(true);
			mockGoodsMeasure.Setup(m => m.NetMass).Returns(19.000m);
			mockGoodsMeasure.Setup(m => m.NetMassSpecified).Returns(true);
			mockCommodity.Setup(m => m.GoodsMeasure).Returns(mockGoodsMeasure.Object);

			return mockCommodity;
		}

		protected Mock<INotifUnloadingNCTSHouseConsignment> SetUpNotifUnloadingHouseConsignment(ZString sequenceNumber, ZDecimal grossMass, IEnumerable<ICommonDepartureTransportMeans> departureTransportMeans, IEnumerable<INCTSCommonDocumentWithInfo> supDocs, IEnumerable<ICommonDocumentSequenceNumber> transDocs, IEnumerable<ICommonDocumentSequenceNumber> additionalRefs, IEnumerable<INotifUnloadingNCTSConsignmentItem> lines)
		{
			var mockHouseConsignment = new Mock<INotifUnloadingNCTSHouseConsignment>();
			mockHouseConsignment.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
			mockHouseConsignment.Setup(m => m.GrossMass).Returns(grossMass);
			mockHouseConsignment.Setup(m => m.GrossMassSpecified).Returns(true);
			mockHouseConsignment.Setup(m => m.DepartureTransportMeans).Returns((IReadOnlyCollection<ICommonDepartureTransportMeans>)departureTransportMeans);
			mockHouseConsignment.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithInfo>)supDocs);
			mockHouseConsignment.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transDocs);
			mockHouseConsignment.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalRefs);
			mockHouseConsignment.Setup(m => m.ConsignmentItem).Returns((IReadOnlyCollection<INotifUnloadingNCTSConsignmentItem>)lines);
			return mockHouseConsignment;
		}

		protected Mock<INotifUnloadingNCTSConsignmentItem> SetUpNotifUnloadingCommonLine(ZString lineNumber, ZString goodsItemNumber, IEnumerable<INCTSCommonPackaging> packages,
			IEnumerable<INCTSCommonDocumentWithInfo> supportingDocuments, IEnumerable<ICommonDocumentSequenceNumber> transportDocuments, IEnumerable<ICommonDocumentSequenceNumber> additionalRef,
			INotifUnloadingCommodity commodity = null)
		{
			var mockLine = new Mock<INotifUnloadingNCTSConsignmentItem>();
			mockLine.Setup(m => m.GoodsItemNumber).Returns(lineNumber);
			mockLine.Setup(m => m.DeclarationGoodsItemNumber).Returns(goodsItemNumber);
			mockLine.Setup(m => m.Commodity).Returns(commodity);
			mockLine.Setup(m => m.Packaging).Returns((IReadOnlyCollection<INCTSCommonPackaging>)packages);
			mockLine.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<INCTSCommonDocumentWithInfo>)supportingDocuments);
			mockLine.Setup(m => m.TransportDocument).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)transportDocuments);
			mockLine.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<ICommonDocumentSequenceNumber>)additionalRef);
			return mockLine;
		}

		#endregion
	}
}
