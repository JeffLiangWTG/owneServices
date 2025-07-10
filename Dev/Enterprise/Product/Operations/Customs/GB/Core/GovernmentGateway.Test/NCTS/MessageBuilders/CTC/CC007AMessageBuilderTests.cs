using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	class CC007AMessageBuilderTests : TestCaseWithFactory
	{
		public void TestDoNotPopulateArrAgrLocCodHEA62()
		{
			dataProviderMock.SetupSequence(m => m.IsSimplifiedArrivalProcedure).Returns(true).Returns(false);
			AssertNotContains("ArrAgrLocCodHEA62", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("ArrAgrLocCodHEA62", messageBuilder.GetXMLMessageWithoutNamespaces());
			destinationTraderMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestPopulateArrAgrLocOfGooHEA63()
		{
			dataProviderMock.SetupSequence(m => m.IsSimplifiedArrivalProcedure).Returns(true).Returns(false);
			AssertNotContains("ArrAgrLocOfGooHEA63", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains("<ArrAgrLocOfGooHEA63>AgreedLocCode</ArrAgrLocOfGooHEA63>", messageBuilder.GetXMLMessageWithoutNamespaces());
			destinationTraderMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestPopulateArrAutLocOfGooHEA65()
		{
			dataProviderMock.SetupSequence(m => m.IsSimplifiedArrivalProcedure).Returns(true).Returns(false);
			AssertContains("<ArrAutLocOfGooHEA65>AgreedLocCode</ArrAutLocOfGooHEA65>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("ArrAutLocOfGooHEA65", messageBuilder.GetXMLMessageWithoutNamespaces());
			destinationTraderMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC007AMessageBuilder()
		{
			var expectedXml = CTCMessageBuilderUtilities.GetEmbeddedResourceFile("CC007AMessageXml.xml");
			AssertEquals(expectedXml, messageBuilder.GetXMLMessageWithoutNamespaces());
			destinationTraderMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		protected override void SetUp()
		{
			base.SetUp();

			destinationTraderMock = new Mock<ITrader>();
			destinationTraderMock.Setup(m => m.TIN).Returns("GB954133135000");

			var enRouteIncident1 = SetupEnRouteIncident("IncidentInfo1", "20111210", "EndorsementAuthority1", "EndorsementPlace1", Core.Constants.CountryCodes.UnitedKingdom);
			var enRouteEventSeal1 = SetupEnRouteEventSeals("2", "Seal");
			var enRouteTranshipment1 = SetupEnRouteTranshipment("NewTransportation1", Core.Constants.CountryCodes.UnitedKingdom, "20111211", "EndorsementAuthority1", "EndorsementPlace1", Core.Constants.CountryCodes.UnitedKingdom);
			var enRouteEvent1 = SetupEnRouteEvent("EventPlace1", Core.Constants.CountryCodes.UnitedKingdom, enRouteIncident1, enRouteEventSeal1, enRouteTranshipment1);

			var enRouteIncident2 = SetupEnRouteIncident("IncidentInfo2", "20111212", "EndorsementAuthority2", "EndorsementPlace2", Core.Constants.CountryCodes.UnitedKingdom);
			var enRouteEventSeal2 = SetupEnRouteEventSeals("2", "Seal");

			var enRouteTranshipment2 = SetupEnRouteTranshipment("NewTransportation2", Core.Constants.CountryCodes.UnitedKingdom, "20111213", "EndorsementAuthority2", "EndorsementPlace2", Core.Constants.CountryCodes.UnitedKingdom);
			var enRouteEvent2 = SetupEnRouteEvent("EventPlace2", Core.Constants.CountryCodes.UnitedKingdom, enRouteIncident2, enRouteEventSeal2, enRouteTranshipment2);

			var enRouteIncident3 = SetupEnRouteIncident("IncidentInfo3", "20111214", "EndorsementAuthority3", "EndorsementPlace3", Core.Constants.CountryCodes.UnitedKingdom);
			var enRouteEvent3 = SetupEnRouteEvent("EventPlace3", Core.Constants.CountryCodes.UnitedKingdom, enRouteIncident3, null, null);

			dataProviderMock = new Mock<ICC007ADeclaration>();
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("21GB00008110000E08");
			dataProviderMock.Setup(m => m.CustomsSubPlace).Returns("CustomsSubPlace");
			dataProviderMock.Setup(m => m.ArrivalNotificationPlace).Returns("NotificationPlace");
			dataProviderMock.Setup(m => m.ArrivalNotificationPlaceLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.ArrivalAgreedLocationOfGoodsCode).Returns("AgreedLocCode");
			dataProviderMock.Setup(m => m.IsSimplifiedArrivalProcedure).Returns(true);
			dataProviderMock.Setup(m => m.ArrivalNotificationDate).Returns("20111213");
			dataProviderMock.Setup(m => m.DialogLanguageIndicatorAtDestination).Returns("EN-US");
			dataProviderMock.Setup(m => m.Destination).Returns(destinationTraderMock.Object);
			dataProviderMock.Setup(m => m.CustomsPresentationOfficeRefNumber).Returns("GB000081");
			dataProviderMock.Setup(m => m.EnRouteEvents).Returns(new IEnRouteEvent[] { enRouteEvent1, enRouteEvent2, enRouteEvent3 });

			errorCollector = new ErrorCollector();
			messageBuilder = new CC007AXmlMessageBuilder(dataProviderMock.Object, errorCollector);
		}

		IEnRouteEvent SetupEnRouteEvent(ZString eventPlace, ZString eventCountryCode, IEnRouteIncident enRouteIncident, IEnRouteEventSeal enRouteEventSeal, IEnRouteTranshipment enRouteTranshipment)
		{
			var enRouteEventMock = new Mock<IEnRouteEvent>();
			enRouteEventMock.Setup(m => m.EventPlace).Returns(eventPlace);
			enRouteEventMock.Setup(m => m.EventPlaceLanguage).Returns("EN-US");
			enRouteEventMock.Setup(m => m.EventCountryCode).Returns(eventCountryCode);
			enRouteEventMock.Setup(m => m.ControlAlreadyInNcts).Returns(true);
			enRouteEventMock.Setup(m => m.EnRouteIncident).Returns(enRouteIncident);
			enRouteEventMock.Setup(m => m.EnRouteEventSeal).Returns(enRouteEventSeal);
			enRouteEventMock.Setup(m => m.EnRouteTranshipment).Returns(enRouteTranshipment);
			return enRouteEventMock.Object;
		}

		IEnRouteIncident SetupEnRouteIncident(ZString incidentInformation, ZString endorsementDate, ZString endorsementAuthority, ZString endorsementPlace, ZString endorsementCountry)
		{
			var enRouteIncidentMock = new Mock<IEnRouteIncident>();
			enRouteIncidentMock.Setup(m => m.IncidentInformation).Returns(incidentInformation);
			enRouteIncidentMock.Setup(m => m.IncidentInformationLanguage).Returns("EN-US");
			enRouteIncidentMock.Setup(m => m.EndorsementDate).Returns(endorsementDate);
			enRouteIncidentMock.Setup(m => m.EndorsementAuthority).Returns(endorsementAuthority);
			enRouteIncidentMock.Setup(m => m.EndorsementAuthorityLanguage).Returns("EN-US");
			enRouteIncidentMock.Setup(m => m.EndorsementPlace).Returns(endorsementPlace);
			enRouteIncidentMock.Setup(m => m.EndorsementPlaceLanguage).Returns(ZString.Empty);
			enRouteIncidentMock.Setup(m => m.EndorsementCountry).Returns(endorsementCountry);
			return enRouteIncidentMock.Object;
		}

		IEnRouteEventSeal SetupEnRouteEventSeals(ZString sealCount, ZString sealIdentity)
		{
			var sealID1Mock = new Mock<ISealID>();
			sealID1Mock.Setup(m => m.SealIdentity).Returns(sealIdentity + "1");
			sealID1Mock.Setup(m => m.SealIdentityLanguage).Returns("EN-US");

			var sealID2Mock = new Mock<ISealID>();
			sealID2Mock.Setup(m => m.SealIdentity).Returns(sealIdentity + "2");
			sealID2Mock.Setup(m => m.SealIdentityLanguage).Returns("EN-US");

			var enRouteEventSealMock = new Mock<IEnRouteEventSeal>();
			enRouteEventSealMock.Setup(m => m.SealCount).Returns(sealCount);
			enRouteEventSealMock.Setup(m => m.ContainerSeals).Returns(new ISealID[] { sealID1Mock.Object, sealID2Mock.Object });
			return enRouteEventSealMock.Object;
		}

		IEnRouteTranshipment SetupEnRouteTranshipment(ZString newTransportID, ZString newTransportCountry, ZString endorsementDate, ZString endorsementAuthority, ZString endorsementPlace, ZString endorsementCountry)
		{
			var enRouteTranshipmentMock = new Mock<IEnRouteTranshipment>();
			enRouteTranshipmentMock.Setup(m => m.NewTransportID).Returns(newTransportID);
			enRouteTranshipmentMock.Setup(m => m.NewTransportIDLanguage).Returns("EN-US");
			enRouteTranshipmentMock.Setup(m => m.NewTransportCountry).Returns(newTransportCountry);
			enRouteTranshipmentMock.Setup(m => m.EndorsementDate).Returns(endorsementDate);
			enRouteTranshipmentMock.Setup(m => m.EndorsementAuthority).Returns(endorsementAuthority);
			enRouteTranshipmentMock.Setup(m => m.EndorsementAuthorityLanguage).Returns("EN-US");
			enRouteTranshipmentMock.Setup(m => m.EndorsementPlace).Returns(endorsementPlace);
			enRouteTranshipmentMock.Setup(m => m.EndorsementPlaceLanguage).Returns("EN-US");
			enRouteTranshipmentMock.Setup(m => m.EndorsementCountry).Returns(endorsementCountry);
			enRouteTranshipmentMock.Setup(m => m.ContainerNumbers).Returns(new ZString[] { "Container1", "Container2" });
			return enRouteTranshipmentMock.Object;
		}

		Mock<ITrader> destinationTraderMock;
		Mock<ICC007ADeclaration> dataProviderMock;
		ErrorCollector errorCollector;
		CC007AXmlMessageBuilder messageBuilder;
	}
}
