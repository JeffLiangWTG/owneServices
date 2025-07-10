using CargoWise.Customs.FR.MessageDefinitions.DeltaT.TCL_NATIONAL;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class CC014AMessageBuilderTest : TestCaseWithFactory
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

		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC014AMessageBuilder()
		{
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			var xmlFile = MessageBuilderUtilities.GetEmbeddedResourceFile("CC014AMessageXml.xml");
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
				AssertNotContains("No Group", "<TRAPRIPC1>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertContains("Error Collector", "Principal is empty", errorCollector.GetErrorsAsString());
			});
		}

		protected override void SetUp()
		{
			base.SetUp();
			principalMock = new Mock<ITrader>();
			principalMock.Setup(m => m.TIN).Returns("GB0123456789001");

			dataProviderMock = new Mock<ICC014ADeclaration>();
			dataProviderMock.Setup(m => m.IsProduction).Returns(new ZBool("1"));
			dataProviderMock.Setup(m => m.AgreementNumber).Returns("12345678");
			dataProviderMock.Setup(m => m.PrincipalTIN).Returns("FR0123456789002");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("NCT00000001");
			dataProviderMock.Setup(m => m.Principal).Returns(principalMock.Object);
			dataProviderMock.Setup(m => m.DepartureCustomsOfficeReferenceNumber).Returns("AA123456");
			dataProviderMock.Setup(m => m.CancellationRegularJustification).Returns(JustificationReglementaireInvalidation.Item2);
			dataProviderMock.Setup(m => m.CancellationDate).Returns("20200622");
			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(false);

			errorCollector = new ErrorCollector();
			messageBuilder = new CC014AMessageBuilder(dataProviderMock.Object, new NctsMessageFunctionSet.DeclarationCancellationRequestMessage("CancellationReason", "CancellationComment"), errorCollector);
		}
		Mock<ITrader> principalMock;
		Mock<ICC014ADeclaration> dataProviderMock;
		ErrorCollector errorCollector;
		CC014AMessageBuilder messageBuilder;
	}
}
