using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class CC007AMessageBuilderTest : TestCaseWithFactory
	{
		public void TestDoNotPopulateArrAgrLocCodHEA62()
		{
			dataProviderMock.Setup(m => m.IsSimplifiedArrivalProcedure).Returns(true);
			AssertNotContains("ArrAgrLocCodHEA62", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsSimplifiedArrivalProcedure).Returns(false);
			AssertNotContains("ArrAgrLocCodHEA62", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateArrAgrLocOfGooHEA63()
		{
			dataProviderMock.Setup(m => m.IsSimplifiedArrivalProcedure).Returns(true);
			AssertNotContains("ArrAgrLocOfGooHEA63", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsSimplifiedArrivalProcedure).Returns(false);
			AssertContains("<ArrAgrLocOfGooHEA63>AP</ArrAgrLocOfGooHEA63>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateArrAutLocOfGooHEA65()
		{
			dataProviderMock.Setup(m => m.IsSimplifiedArrivalProcedure).Returns(true);
			AssertContains("<ArrAutLocOfGooHEA65>AP</ArrAutLocOfGooHEA65>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsSimplifiedArrivalProcedure).Returns(false);
			AssertNotContains("ArrAutLocOfGooHEA65", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		[TestDate(2018, 04, 23, 10, 00, 16)]
		public void TestCC007AMessageBuilder()
		{
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			var xmlFile = MessageBuilderUtilities.GetEmbeddedResourceFile("CC007AMessageXml.xml");
			AssertEquals(xmlFile, xmlMsgWithNoNamespaces);
		}

		public void TestPopulateTRADESTRD_NameAndAddress()
		{
			destinationMock.Setup(m => m.TIN).Returns(ZString.Empty);
			destinationMock.Setup(m => m.Name).Returns("KAPSULE FRANCAISE");
			destinationMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			destinationMock.Setup(m => m.StreetAndNumber).Returns("ZI DE BAJOLET");
			destinationMock.Setup(m => m.PostalCode).Returns("91 470");
			destinationMock.Setup(m => m.City).Returns("TROYES CEDEX");
			destinationMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.France);
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains(@"<TRADESTRD>
    <NamTRD7>KAPSULE FRANCAISE</NamTRD7>
    <StrAndNumTRD22>ZI DE BAJOLET</StrAndNumTRD22>
    <PosCodTRD23>91 470</PosCodTRD23>
    <CitTRD24>TROYES CEDEX</CitTRD24>
    <CouTRD25>FR</CouTRD25>
    <NADLNGRD>EN</NADLNGRD>
  </TRADESTRD>", xmlMsgWithNoNamespaces);
		}

		public void TestPopulateTRADESTRD_Null()
		{
			dataProviderMock.Setup(m => m.Destination).Returns((ITrader)null);
			CombineAssertions(() =>
			{
				AssertNotContains("No Group", "<TRADESTRD>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertContains("Error Collector", "Destination is empty", errorCollector.GetErrorsAsString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			destinationMock = new Mock<ITrader>();
			destinationMock.Setup(m => m.TIN).Returns("FR42856003100257");

			var enRouteIncident1 = SetupEnRouteIncident("Scelles arraches", "20180423", "Gendarmerie", "Paris", Core.Constants.CountryCodes.France);
			var enRouteEventSeal1 = SetupEnRouteEventSeal("1", "Douanes francaises");
			var enRouteTranshipment1 = SetupEnRouteTranshipment("28 TY 89", Core.Constants.CountryCodes.France, "20180423", "Police", "Lyon", Core.Constants.CountryCodes.France);
			var enRouteEvent1 = SetupEnRouteEvent("PARIS", Core.Constants.CountryCodes.France, enRouteIncident1, enRouteEventSeal1, enRouteTranshipment1);

			var enRouteIncident2 = SetupEnRouteIncident("Scelles non conformes", "20180423", "Gendarmerie", "Lyon", Core.Constants.CountryCodes.France);
			var enRouteEventSeal2 = SetupEnRouteEventSeal("1", "Douanes");
			var enRouteTranshipment2 = SetupEnRouteTranshipment("10 WS 45", Core.Constants.CountryCodes.France, "20180423", "Gendarmerie", "Bordeaux", Core.Constants.CountryCodes.France);
			var enRouteEvent2 = SetupEnRouteEvent("LYON", Core.Constants.CountryCodes.France, enRouteIncident2, enRouteEventSeal2, enRouteTranshipment2);

			var enRouteIncident3 = SetupEnRouteIncident("Marchandise absente", "20180423", "Police", "Marseille", Core.Constants.CountryCodes.France);
			var enRouteEvent3 = SetupEnRouteEvent("MARSEILLE", Core.Constants.CountryCodes.France, enRouteIncident3, null, null);

			dataProviderMock = new Mock<ICC007ADeclaration>();
			dataProviderMock.Setup(m => m.IsProduction).Returns(new ZBool("1"));
			dataProviderMock.Setup(m => m.AgreementNumber).Returns("12345678");
			dataProviderMock.Setup(m => m.DeclarantTIN).Returns("FR0123456789002");
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("18FR00400000000637");
			dataProviderMock.Setup(m => m.CustomsSubPlace).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.ArrivalNotificationPlace).Returns("Le Havre");
			dataProviderMock.Setup(m => m.ArrivalNotificationPlaceLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.ArrivalAgreedLocationOfGoodsCode).Returns("AP");
			dataProviderMock.Setup(m => m.ArrivalAgreedLocationOfGoodsLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.IsSimplifiedArrivalProcedure).Returns(true);
			dataProviderMock.Setup(m => m.ArrivalNotificationDate).Returns("20180423");
			dataProviderMock.Setup(m => m.DialogLanguageIndicatorAtDestination).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.Destination).Returns(destinationMock.Object);
			dataProviderMock.Setup(m => m.CustomsPresentationOfficeRefNumber).Returns("FR002300");
			dataProviderMock.Setup(m => m.EnRouteEvents).Returns(new IEnRouteEvent[] { enRouteEvent1, enRouteEvent2, enRouteEvent3 });

			errorCollector = new ErrorCollector();
			messageBuilder = new CC007AMessageBuilder(dataProviderMock.Object, new NctsMessageFunctionSet.ArrivalNotificationMessage(), errorCollector);
		}
		Mock<ITrader> destinationMock;
		Mock<ICC007ADeclaration> dataProviderMock;
		ErrorCollector errorCollector;
		CC007AMessageBuilder messageBuilder;

		IEnRouteEvent SetupEnRouteEvent(ZString eventPlace, ZString eventCountryCode, IEnRouteIncident enRouteIncident, IEnRouteEventSeal enRouteEventSeal, IEnRouteTranshipment enRouteTranshipment)
		{
			var enRouteEventMock = new Mock<IEnRouteEvent>();
			enRouteEventMock.Setup(m => m.EventPlace).Returns(eventPlace);
			enRouteEventMock.Setup(m => m.EventPlaceLanguage).Returns(ZString.Empty);
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
			enRouteIncidentMock.Setup(m => m.IncidentInformationLanguage).Returns(ZString.Empty);
			enRouteIncidentMock.Setup(m => m.EndorsementDate).Returns(endorsementDate);
			enRouteIncidentMock.Setup(m => m.EndorsementAuthority).Returns(endorsementAuthority);
			enRouteIncidentMock.Setup(m => m.EndorsementAuthorityLanguage).Returns(ZString.Empty);
			enRouteIncidentMock.Setup(m => m.EndorsementPlace).Returns(endorsementPlace);
			enRouteIncidentMock.Setup(m => m.EndorsementPlaceLanguage).Returns(ZString.Empty);
			enRouteIncidentMock.Setup(m => m.EndorsementCountry).Returns(endorsementCountry);
			return enRouteIncidentMock.Object;
		}

		IEnRouteEventSeal SetupEnRouteEventSeal(ZString sealCount, ZString sealIdentity)
		{
			var sealIDMock = new Mock<ISealID>();
			sealIDMock.Setup(m => m.SealIdentity).Returns(sealIdentity);
			sealIDMock.Setup(m => m.SealIdentityLanguage).Returns(ZString.Empty);

			var enRouteEventSealMock = new Mock<IEnRouteEventSeal>();
			enRouteEventSealMock.Setup(m => m.SealCount).Returns(sealCount);
			enRouteEventSealMock.Setup(m => m.ContainerSeals).Returns(new ISealID[] { sealIDMock.Object });
			return enRouteEventSealMock.Object;
		}

		IEnRouteTranshipment SetupEnRouteTranshipment(ZString newTransportID, ZString newTransportCountry, ZString endorsementDate, ZString endorsementAuthority, ZString endorsementPlace, ZString endorsementCountry)
		{
			var enRouteTranshipmentMock = new Mock<IEnRouteTranshipment>();
			enRouteTranshipmentMock.Setup(m => m.NewTransportID).Returns(newTransportID);
			enRouteTranshipmentMock.Setup(m => m.NewTransportIDLanguage).Returns(ZString.Empty);
			enRouteTranshipmentMock.Setup(m => m.NewTransportCountry).Returns(newTransportCountry);
			enRouteTranshipmentMock.Setup(m => m.EndorsementDate).Returns(endorsementDate);
			enRouteTranshipmentMock.Setup(m => m.EndorsementAuthority).Returns(endorsementAuthority);
			enRouteTranshipmentMock.Setup(m => m.EndorsementAuthorityLanguage).Returns(ZString.Empty);
			enRouteTranshipmentMock.Setup(m => m.EndorsementPlace).Returns(endorsementPlace);
			enRouteTranshipmentMock.Setup(m => m.EndorsementPlaceLanguage).Returns(ZString.Empty);
			enRouteTranshipmentMock.Setup(m => m.EndorsementCountry).Returns(endorsementCountry);
			enRouteTranshipmentMock.Setup(m => m.ContainerNumbers).Returns((IReadOnlyCollection<ZString>)Enumerable.Empty<ZString>());
			return enRouteTranshipmentMock.Object;
		}
	}
}
