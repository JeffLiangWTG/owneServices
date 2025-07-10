using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC007C_v515.CC007CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ArrivalNCTSMessageBuilder))]
	class ArrivalNCTSMessageBuilderTest : NCTSCommonMessageBuilderTest<ArrivalNCTSMessageBuilder, IArrivalNCTSMessageDataProvider, Cc007Cv1Ent>
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
			mockProvider.Setup(m => m.TransitOperation).Returns((IArrivalNCTSTransitOperation)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransitOperation>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateAuthorisation()
		{
			mockProvider.Setup(m => m.Authorisations).Returns((IReadOnlyCollection<INCTSCommonAuthorisation>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Authorisation>", CreateMessageBuilder().GetSignedMessageText());
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

		public void TestPopulateIndicators()
		{
			mockProvider.Setup(m => m.Indicators).Returns((IArrivalNCTSIndicators)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Indicadores007>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulatePreviousG4()
		{
			mockIndicators.Setup(m => m.PreviousG4).Returns((IReadOnlyCollection<IArrivalNCTS5PreviousG4>)null);
			mockProvider.Setup(m => m.Indicators).Returns(mockIndicators.Object);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}G4Previos>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateConsignment()
		{
			mockProvider.Setup(m => m.Consignment).Returns((IArrivalNCTSConsigment)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Consignment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateLocationOfGoods()
		{
			mockProvider.Setup(m => m.Consignment.LocationOfGoods).Returns((INCTSCommonLocationOfGoods)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}LocationOfGoods>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateIncident()
		{
			mockProvider.Setup(m => m.Consignment.Incident).Returns((IReadOnlyCollection<IArrivalNCTSIncident>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Incident>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateEndorsement()
		{
			mockIncident1.Setup(m => m.Endorsement).Returns((IArrivalNCTSEndorsement)null);
			mockProvider.Setup(m => m.Consignment.Incident).Returns(new IArrivalNCTSIncident[] { mockIncident1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Endorsement>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateLocation()
		{
			mockIncident1.Setup(m => m.Location).Returns((IArrivalNCTSLocation)null);
			mockProvider.Setup(m => m.Consignment.Incident).Returns(new IArrivalNCTSIncident[] { mockIncident1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Location>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateGNSS()
		{
			mockIncident1.Setup(m => m.Location.GNSS).Returns((IArrivalNCTSGNSS)null);
			mockProvider.Setup(m => m.Consignment.Incident).Returns(new IArrivalNCTSIncident[] { mockIncident1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GNSS>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateAddress()
		{
			mockIncident1.Setup(m => m.Location.Address).Returns((INCTSCommonAddress)null);
			mockProvider.Setup(m => m.Consignment.Incident).Returns(new IArrivalNCTSIncident[] { mockIncident1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Address>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateTransportEquipment()
		{
			mockIncident1.Setup(m => m.TransportEquipment).Returns((IReadOnlyCollection<INCTSCommonTransportEquipment>)null);
			mockProvider.Setup(m => m.Consignment.Incident).Returns(new IArrivalNCTSIncident[] { mockIncident1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportEquipment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateSeals()
		{
			mockTransportEquipment1.Setup(m => m.Seals).Returns((IReadOnlyCollection<ISealCommon>)null);
			mockIncident1.Setup(m => m.TransportEquipment).Returns(new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object });
			mockProvider.Setup(m => m.Consignment.Incident).Returns(new IArrivalNCTSIncident[] { mockIncident1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Seal>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateGoodsReference()
		{
			mockTransportEquipment1.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<INCTSCommonGoodsReference>)null);
			mockIncident1.Setup(m => m.TransportEquipment).Returns(new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object });
			mockProvider.Setup(m => m.Consignment.Incident).Returns(new IArrivalNCTSIncident[] { mockIncident1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}GoodsReference>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateTranshipment()
		{
			mockIncident1.Setup(m => m.Transhipment).Returns((IArrivalNCTSTranshipment)null);
			mockProvider.Setup(m => m.Consignment.Incident).Returns(new IArrivalNCTSIncident[] { mockIncident1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Transhipment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateTransportMeans()
		{
			mockIncident1.Setup(m => m.Transhipment.TransportMeans).Returns((ITransportMediumInfoCommon)null);
			mockProvider.Setup(m => m.Consignment.Incident).Returns(new IArrivalNCTSIncident[] { mockIncident1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransportMeans>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		#endregion
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Ncts5ArrivalNotification;

		protected override ArrivalNCTSMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ArrivalNCTSMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override ArrivalNCTSMessageBuilder CreateMessageBuilderWithNullProvider() => new ArrivalNCTSMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(NCTSXMLTestFileConstants.NCTSTestFilePath, "TestArrivalNCTS.txt");

		#region Structures SetUp

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MessageSender).Returns("A78587268");
			mockProvider.Setup(m => m.MessageIdentification).Returns("20221213141220798000");
			mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);
			mockProvider.Setup(m => m.TransitOperation).Returns(SetUpArrivalTransitOperation().Object);

			var mockAuthorisation1 = SetUpCommonAuthorisation("1", "ACR", "000001");
			var mockAuthorisation2 = SetUpCommonAuthorisation("2", "SSE", "000002");
			var mockAuthorisations = new INCTSCommonAuthorisation[] { mockAuthorisation1.Object, mockAuthorisation2.Object };
			mockProvider.Setup(m => m.Authorisations).Returns(mockAuthorisations);

			mockProvider.Setup(m => m.CustomsOfficeOfDestinationActual).Returns("ES000101");
			mockProvider.Setup(m => m.TraderAtDestination).Returns(BuilderHelperTest.SetUpPartyId("ES89890001K"));
			mockProvider.Setup(m => m.RepresentativeAtDestination).Returns(BuilderHelperTest.SetUpPartyId("ES89890001K"));
			mockProvider.Setup(m => m.Indicators).Returns(SetUpIndicators("0", "T").Object);

			mockConsignment = SetUpArrivalConsignment();
			mockProvider.Setup(m => m.Consignment).Returns(mockConsignment.Object);
		}

		protected Mock<IArrivalNCTSTransitOperation> SetUpArrivalTransitOperation()
		{
			var mockTransitOperation = new Mock<IArrivalNCTSTransitOperation>();
			mockTransitOperation.Setup(m => m.MRN).Returns("22ES000101100909B7");
			mockTransitOperation.Setup(m => m.SimplifiedProcedure).Returns(ZBool.False);
			mockTransitOperation.Setup(m => m.IncidentFlag).Returns(ZBool.True);
			return mockTransitOperation;
		}

		protected Mock<IArrivalNCTSIndicators> SetUpIndicators(ZString goodsDirectlyShipped, ZString tirParcialTotalUnloading)
		{
			mockIndicators = new Mock<IArrivalNCTSIndicators>();
			mockIndicators.Setup(m => m.GoodsDirectlyShipped).Returns(goodsDirectlyShipped);
			mockIndicators.Setup(m => m.AutomaticCompletion).Returns("U");
			mockIndicators.Setup(m => m.TIRPageCompletion).Returns("12345");
			mockIndicators.Setup(m => m.TIRParcialTotalUnloading).Returns(tirParcialTotalUnloading);
			mockIndicators.Setup(m => m.ReceptionSummary).Returns("ES9999000002");
			mockIndicators.Setup(m => m.SummaryTypeIndicator).Returns("SP");

			var mockPreviousG41 = SetUpPreviousG4("1", "mrn1");
			var mockPreviousG42 = SetUpPreviousG4("2", "mrn2");
			var mockPreviousG4s = new IArrivalNCTS5PreviousG4[] { mockPreviousG41.Object, mockPreviousG42.Object };
			mockIndicators.Setup(m => m.PreviousG4).Returns(mockPreviousG4s);

			return mockIndicators;
		}

		Mock<IArrivalNCTSConsigment> mockConsignment;
		Mock<IArrivalNCTSIndicators> mockIndicators;
		Mock<IArrivalNCTSIncident> mockIncident1;
		Mock<INCTSCommonTransportEquipment> mockTransportEquipment1;

		Mock<IArrivalNCTSConsigment> SetUpArrivalConsignment()
		{
			var mockConsignment = new Mock<IArrivalNCTSConsigment>();

			mockConsignment.Setup(m => m.LocationOfGoods).Returns(SetUpLocationOfGoods().Object);

			mockTransportEquipment1 = SetUpTransportEquipment("1", "CSQU3054383", "2", SetUpSeals(), SetUpGoodsReference());
			var mockTransportEquipment2 = SetUpTransportEquipment("2", "CSQJ3054383", "0", Enumerable.Empty<ISealCommon>(), Enumerable.Empty<INCTSCommonGoodsReference>());
			var mockTransportEquipments = new INCTSCommonTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };

			mockIncident1 = SetUpIncident("1", "1", "Desvio", mockTransportEquipments, SetUpEndorsements().Object, SetUpLocation().Object, SetUpTranshipment().Object);
			var mockIncident2 = SetUpIncident("2", "2", "Desvio2", Enumerable.Empty<INCTSCommonTransportEquipment>());
			var mockIncidents = new IArrivalNCTSIncident[] { mockIncident1.Object, mockIncident2.Object };
			mockConsignment.Setup(m => m.Incident).Returns(mockIncidents);

			return mockConsignment;
		}

		Mock<IArrivalNCTSIncident> SetUpIncident(ZString sequenceNumber, ZString code, ZString text, IEnumerable<INCTSCommonTransportEquipment> mockTransportEquipments, IArrivalNCTSEndorsement mockEndorsements = null, IArrivalNCTSLocation mockLocation = null, IArrivalNCTSTranshipment mockTranshipment = null)
		{
			var mockIncident = new Mock<IArrivalNCTSIncident>();

			mockIncident.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
			mockIncident.Setup(m => m.Code).Returns(code);
			mockIncident.Setup(m => m.Text).Returns(text);

			mockIncident.Setup(m => m.Endorsement).Returns(mockEndorsements);
			mockIncident.Setup(m => m.Location).Returns(mockLocation);
			mockIncident.Setup(m => m.TransportEquipment).Returns((IReadOnlyCollection<INCTSCommonTransportEquipment>)mockTransportEquipments);
			mockIncident.Setup(m => m.Transhipment).Returns(mockTranshipment);

			return mockIncident;
		}

		Mock<IArrivalNCTSEndorsement> SetUpEndorsements()
		{
			var mockEndorsements = new Mock<IArrivalNCTSEndorsement>();
			mockEndorsements.Setup(m => m.Date).Returns(new ZDateTime(2024, 08, 03));
			mockEndorsements.Setup(m => m.Authority).Returns("Authority");
			mockEndorsements.Setup(m => m.Place).Returns("Madrid");
			mockEndorsements.Setup(m => m.Country).Returns("ES");
			return mockEndorsements;
		}

		Mock<IArrivalNCTSLocation> SetUpLocation()
		{
			var mockLocation = new Mock<IArrivalNCTSLocation>();
			mockLocation.Setup(m => m.Qualifier).Returns("U");
			mockLocation.Setup(m => m.UNLocode).Returns("ADALV");
			mockLocation.Setup(m => m.Country).Returns("AN");

			var mockGNSS = new Mock<IArrivalNCTSGNSS>();
			mockGNSS.Setup(m => m.Latitude).Returns("1");
			mockGNSS.Setup(m => m.Longitude).Returns("2");
			mockLocation.Setup(m => m.GNSS).Returns(mockGNSS.Object);

			mockLocation.Setup(m => m.Address).Returns(SetUpAddress().Object);

			return mockLocation;
		}

		Mock<IArrivalNCTSTranshipment> SetUpTranshipment()
		{
			var mockTranshipment = new Mock<IArrivalNCTSTranshipment>();
			mockTranshipment.Setup(m => m.ContainerIndicator).Returns(ZBool.False);
			mockTranshipment.Setup(m => m.TransportMeans).Returns(BuilderHelperTest.SetUpTransportMediumInfo("21", "Ship", "ES").Object);
			return mockTranshipment;
		}

		Mock<IArrivalNCTS5PreviousG4> SetUpPreviousG4(ZString sequenceNumber, ZString mrn)
		{
			var mockPreviousG4 = new Mock<IArrivalNCTS5PreviousG4>();
			mockPreviousG4.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
			mockPreviousG4.Setup(m => m.PreviousG4MRN).Returns(mrn);
			return mockPreviousG4;
		}

		#endregion
	}
}
