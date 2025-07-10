using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class CC141AMessageBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateHITPC126()
		{
			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(true);
			principalMock.Setup(m => m.TIN).Returns("GB0123456789001");
			principalMock.Setup(m => m.Name).Returns("STEPHEN BRENNAN PERSONAL SHIPMENTS SPECIALISTS");
			principalMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			principalMock.Setup(m => m.StreetAndNumber).Returns("10 MOYANGUL DRIVE");
			principalMock.Setup(m => m.PostalCode).Returns("3033");
			principalMock.Setup(m => m.City).Returns("KEILOR EAST");
			principalMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.Australia);
			principalMock.Setup(m => m.HolderIDTIR).Returns("AU0123456789348");

			principalMock.Setup(m => m.TIN).Returns(ZString.Empty);
			AssertContains("HITPC126 is populated from the principal's holder ID.", "<HITPC126>AU0123456789348</HITPC126>", messageBuilder.GetXMLMessageWithoutNamespaces());

			principalMock.Setup(m => m.TIN).Returns("1234567890");
			AssertContains("To populate HITPC126 or not has nothing to do with TIN.", "<HITPC126>AU0123456789348</HITPC126>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(false);
			AssertNotContains("We only populate HITPC126 if declaration is TIR.", "<HITPC126>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		[TestDate(2020, 5, 20, 10, 0, 0)]
		public void TestCC141AMessageBuilder()
		{
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			var xmlFile = MessageBuilderUtilities.GetEmbeddedResourceFile("CC141AMessageXml.xml");
			AssertEquals(xmlFile, xmlMsgWithNoNamespaces);
		}

		public void TestNamPC17PresenceIfTIR()
		{
			principalMock.Setup(m => m.TIN).Returns("GB13131313131313");
			principalMock.Setup(m => m.Name).Returns("STEPHEN BRENNAN PERSONAL SHIPMENTS SPECIALISTS  ");
			principalMock.Setup(m => m.CompanyName).Returns("STEPHEN BRENNAN PERSONAL SHIPMENTS NAME  ");
			principalMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			principalMock.Setup(m => m.StreetAndNumber).Returns("10 MOYANGUL DRIVE");
			principalMock.Setup(m => m.PostalCode).Returns("3033");
			principalMock.Setup(m => m.City).Returns("KEILOR EAST");
			principalMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.Australia);
			principalMock.Setup(m => m.HolderIDTIR).Returns("AU0123456789348");
			AssertContains(@"<TRAPRIPC1>
    <TINPC159>GB13131313131313</TINPC159>
  </TRAPRIPC1>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(true);
			AssertContains(@"<TRAPRIPC1>
    <NamPC17>STEPHEN BRENNAN PERSONAL SHIPMENTS NAME</NamPC17>
    <TINPC159>GB13131313131313</TINPC159>
    <HITPC126>AU0123456789348</HITPC126>
  </TRAPRIPC1>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTRAPRIPC1_NameAndAddress()
		{
			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(true);
			principalMock.Setup(m => m.TIN).Returns(ZString.Empty);
			principalMock.Setup(m => m.Name).Returns("METAROM FRANCE  ");
			principalMock.Setup(m => m.CompanyName).Returns("METAROM NAME  ");
			principalMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			principalMock.Setup(m => m.StreetAndNumber).Returns("9-11, AVENUE DE LA LIBERATION  ");
			principalMock.Setup(m => m.PostalCode).Returns("24200");
			principalMock.Setup(m => m.City).Returns("SARLAT EN PERIGOD");
			principalMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.France);
			principalMock.Setup(m => m.HolderIDTIR).Returns("FR0123456789451");
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains(@"<TRAPRIPC1>
    <NamPC17>METAROM NAME</NamPC17>
    <StrAndNumPC122>9-11, AVENUE DE LA LIBERATION</StrAndNumPC122>
    <PosCodPC123>24200</PosCodPC123>
    <CitPC124>SARLAT EN PERIGOD</CitPC124>
    <CouPC125>FR</CouPC125>
    <NADLNGPC>EN</NADLNGPC>
    <HITPC126>FR0123456789451</HITPC126>
  </TRAPRIPC1>", xmlMsgWithNoNamespaces);
		}

		public void TestPopulateTRAPRIPC1_Null()
		{
			dataProviderMock.Setup(m => m.Principal).Returns((ITrader)null);
			CombineAssertions(() =>
			{
				AssertNotContains("<TRAPRIPC1>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertContains("Error Collector", "Principal is empty", errorCollector.GetErrorsAsString());
			});
		}

		public void TestPopulateCNECNE_Null()
		{
			dataProviderMock.Setup(m => m.Consignee).Returns((ITrader)null);
			CombineAssertions(() =>
			{
				AssertNotContains("<CNECNE>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertContains("Error Collector", "Consignee is empty", errorCollector.GetErrorsAsString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			principalMock = new Mock<ITrader>();
			principalMock.Setup(m => m.TIN).Returns("GB0123456789001");

			consigneeMock = new Mock<ITrader>();
			consigneeMock.Setup(m => m.TIN).Returns("GB0123456789003");
			consigneeMock.Setup(m => m.Name).Returns("Oscorp Industries3");
			consigneeMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			consigneeMock.Setup(m => m.StreetAndNumber).Returns("Street and No3");
			consigneeMock.Setup(m => m.PostalCode).Returns("MK16 XX3");
			consigneeMock.Setup(m => m.City).Returns("Milton Keynes3");
			consigneeMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.UnitedKingdom);

			dataProviderMock = new Mock<ICC141ADeclaration>();
			dataProviderMock.Setup(m => m.IsProduction).Returns(new ZBool("1"));
			dataProviderMock.Setup(m => m.MovementReferenceNumber).Returns("MRN123");
			dataProviderMock.Setup(m => m.Principal).Returns(principalMock.Object);
			dataProviderMock.Setup(m => m.DepartureCustomsOfficeReferenceNumber).Returns("AA123456");
			dataProviderMock.Setup(m => m.DestinationCustomsOfficeReferenceNumber).Returns("BB123456");
			dataProviderMock.Setup(m => m.IsTC11DeliveredByCustoms).Returns(true);
			dataProviderMock.Setup(m => m.TC11Date).Returns("20200523");
			dataProviderMock.Setup(m => m.QueryInformation).Returns("queryInformation");
			dataProviderMock.Setup(m => m.IsQueryAvailableOnPaper).Returns(false);
			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(false);

			errorCollector = new ErrorCollector();
			messageBuilder = new CC141AMessageBuilder(dataProviderMock.Object, new NctsMessageFunctionSet.InformationAboutNonArrivedMovementMessage(), errorCollector);
		}
		Mock<ITrader> principalMock;
		Mock<ITrader> consigneeMock;
		Mock<ICC141ADeclaration> dataProviderMock;
		ErrorCollector errorCollector;
		CC141AMessageBuilder messageBuilder;
	}
}
