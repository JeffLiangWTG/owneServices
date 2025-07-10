using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Business.MessageGeneration;
using Enterprise.Customs.EU.NCTS.Messaging;
using Enterprise.Customs.FR.Business;
using Enterprise.MasterFiles.Business;
using Moq;
using NUnit.Framework;
using IPackage = Enterprise.Customs.EU.NCTS.Messaging.IPackage;

namespace Enterprise.Customs.FR.NCTS.Messaging.Testing
{
	class CC015BMessageBuilderTest : TestCaseWithFactory
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

		public void TestGuaranteesOutput()
		{
			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { MessageBuilderUtilities.SetupGuarantee("0", "12346789", "AAAAAAAAAA", "ABCD", 12.68m) });
			var message1 = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<GUAREFREF>", message1);
			AssertContains("<MonDetSucDeNaiREF1012>", message1);
			AssertContains("<AccCodREF6>", message1);
			AssertContains("<VALLIMECVLE>", message1);
			AssertContains("<GuaRefNumGRNREF1>", message1);
			AssertNotContains("<OthGuaRefREF4>", message1);

			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { MessageBuilderUtilities.SetupGuarantee("1", "12346789", "AAAAAAAAAA", "ABCD", 12.68m) });
			var message2 = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<GUAREFREF>", message2);
			AssertContains("<MonDetSucDeNaiREF1012>", message2);
			AssertContains("<AccCodREF6>", message2);
			AssertContains("<VALLIMECVLE>", message2);
			AssertContains("<GuaRefNumGRNREF1>", message2);
			AssertNotContains("<OthGuaRefREF4>", message2);

			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { MessageBuilderUtilities.SetupGuarantee("2", "12346789", "AAAAAAAAAA", "ABCD", 12.68m) });
			var message3 = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<GUAREFREF>", message3);
			AssertContains("<MonDetSucDeNaiREF1012>", message3);
			AssertContains("<AccCodREF6>", message3);
			AssertContains("<VALLIMECVLE>", message3);
			AssertContains("<GuaRefNumGRNREF1>", message3);
			AssertNotContains("<OthGuaRefREF4>", message3);

			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { MessageBuilderUtilities.SetupGuarantee("3", "12346789", "AAAAAAAAAA", "ABCD", 12.68m) });
			var message4 = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<GUAREFREF>", message4);
			AssertNotContains("<MonDetSucDeNaiREF1012>", message4);
			AssertNotContains("<AccCodREF6>", message4);
			AssertNotContains("<VALLIMECVLE>", message4);
			AssertNotContains("<GuaRefNumGRNREF1>", message4);
			AssertContains("<OthGuaRefREF4>", message4);

			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { MessageBuilderUtilities.SetupGuarantee("4", "12346789", "AAAAAAAAAA", "ABCD", 12.68m) });
			var message5 = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<GUAREFREF>", message5);
			AssertContains("<MonDetSucDeNaiREF1012>", message5);
			AssertContains("<AccCodREF6>", message5);
			AssertContains("<VALLIMECVLE>", message5);
			AssertContains("<GuaRefNumGRNREF1>", message5);
			AssertNotContains("<OthGuaRefREF4>", message5);

			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { MessageBuilderUtilities.SetupGuarantee("9", "12346789", "AAAAAAAAAA", "ABCD", 12.68m) });
			var message6 = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<GUAREFREF>", message6);
			AssertContains("<MonDetSucDeNaiREF1012>", message6);
			AssertContains("<AccCodREF6>", message6);
			AssertContains("<VALLIMECVLE>", message6);
			AssertContains("<GuaRefNumGRNREF1>", message6);
			AssertNotContains("<OthGuaRefREF4>", message6);

			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { MessageBuilderUtilities.SetupGuarantee("B", "12346789", "AAAAAAAAAA", "ABCD", 12.68m) });
			var message7 = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<GUAREFREF>", message7);
			AssertContains("<MonDetSucDeNaiREF1012>", message7);
			AssertNotContains("<AccCodREF6>", message7);
			AssertNotContains("<VALLIMECVLE>", message7);
			AssertNotContains("<GuaRefNumGRNREF1>", message7);
			AssertContains("<OthGuaRefREF4>", message7);

			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { MessageBuilderUtilities.SetupGuarantee("5", "12346789", "AAAAAAAAAA", "ABCD", 12.68m), MessageBuilderUtilities.SetupGuarantee("6", "987654321", "BBBBB", "WXYZ", 32.33m), MessageBuilderUtilities.SetupGuarantee("7", "987654321", "BBBBB", "WXYZ", 32.33m), MessageBuilderUtilities.SetupGuarantee("8", "987654321", "BBBBB", "WXYZ", 32.33m), MessageBuilderUtilities.SetupGuarantee("A", "987654321", "BBBBB", "WXYZ", 32.33m) });
			AssertNotContains("<GUAREFREF>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestDoNotPopulateSEAINFSLI_IfNatureOfSealsIs2()
		{
			dataProviderMock.Setup(m => m.NatureOfSeals).Returns(NatureOfSealsList.Codes.NS1);
			AssertContains("<SEAINFSLI>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.NatureOfSeals).Returns(NatureOfSealsList.Codes.NS2);
			AssertNotContains("<SEAINFSLI>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestDoNotPopulateTRAAUTCONTRA_IfAuthorisedConsigneeTINIsEmpty()
		{
			dataProviderMock.Setup(m => m.AuthorisedConsigneeTIN).Returns("TIN223");
			AssertContains("<TRAAUTCONTRA>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.AuthorisedConsigneeTIN).Returns(ZString.Empty);
			AssertNotContains("<TRAAUTCONTRA>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestDoNotPopulateValFacTotHEA1000_IfTotalInvoiceValueIsZero()
		{
			dataProviderMock.Setup(m => m.TotalInvoiceValue).Returns(1m);
			AssertContains("<ValFacTotHEA1000>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.TotalInvoiceValue).Returns(0m);
			AssertNotContains("<ValFacTotHEA1000>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestDoNotPopulateValFacGDS1013_IfBillValueIsZero()
		{
			var goodsItemMock = SetupGoodsItem(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<EU.NCTS.Business.PackageWrapper>(), null, null);
			goodsItemMock.Setup(m => m.BillValue).Returns(1m);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItemMock.Object });
			AssertContains("<ValFacGDS1013>", messageBuilder.GetXMLMessageWithoutNamespaces());

			goodsItemMock.Setup(m => m.BillValue).Returns(0m);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItemMock.Object });
			AssertNotContains("<ValFacGDS1013>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC015BMessageBuilder()
		{
			var expectedXml = MessageBuilderUtilities.GetEmbeddedResourceFile("CC015BMessageXml.xml");
			AssertEquals(expectedXml, messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestRoundingOfTotalGuaranteeTaxAndLiabilityAmount()
		{
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			CombineAssertions(() =>
			{
				AssertContains("Guarantee amount shouldn't have any decimal in the message", "<MonDetSucDeNaiREF1012>32</MonDetSucDeNaiREF1012>", xmlMsgWithNoNamespaces);
			});
		}

		public void TestRoundingOfTotalInvoiceValue()
		{
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			CombineAssertions(() =>
			{
				AssertContains("Total Invoice Value shouldn't have any decimal in the message", "<ValFacTotHEA1000>123</ValFacTotHEA1000>", xmlMsgWithNoNamespaces);
			});
		}

		public void TestRoundingOfBillValue()
		{
			var package = MessageBuilderUtilities.SetupPackage(ZString.Empty, "BX", 0, 15, false, false);
			var goodsItem = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), new IPackage[] { package }, null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("Bill Value shouldn't have any decimal in the message", "<ValFacGDS1013>123</ValFacGDS1013>", xmlMsgWithNoNamespaces);
		}

		public void TestDatPreDepEnTraHEA1019()
		{
			CombineAssertions(() =>
			{
				AssertNotContains("Pre lodge indicator 0", "<DatPreDepEnTraHEA1019>", messageBuilder.GetXMLMessageWithoutNamespaces());
				dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
				dataProviderMock.Setup(m => m.ProvisionalTransitDepartureDate).Returns("07122020");
				AssertContains("Pre lodge indicator not 0", "<DatPreDepEnTraHEA1019>", messageBuilder.GetXMLMessageWithoutNamespaces());
			});
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

		public void TestPopulateTRAPRIPC1_NameAddAddress()
		{
			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(true);
			principalMock.Setup(m => m.TIN).Returns(ZString.Empty);
			principalMock.Setup(m => m.Name).Returns("STEPHEN BRENNAN PERSONAL SHIPMENTS SPECIALISTS  ");
			principalMock.Setup(m => m.CompanyName).Returns("STEPHEN BRENNAN PERSONAL SHIPMENTS NAME  ");
			principalMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			principalMock.Setup(m => m.StreetAndNumber).Returns("10 MOYANGUL DRIVE  ");
			principalMock.Setup(m => m.PostalCode).Returns("3033");
			principalMock.Setup(m => m.City).Returns("KEILOR EAST");
			principalMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.Australia);
			principalMock.Setup(m => m.HolderIDTIR).Returns("AU0123456789348");
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains(@"<TRAPRIPC1>
    <NamPC17>STEPHEN BRENNAN PERSONAL SHIPMENTS NAME</NamPC17>
    <StrAndNumPC122>10 MOYANGUL DRIVE</StrAndNumPC122>
    <PosCodPC123>3033</PosCodPC123>
    <CitPC124>KEILOR EAST</CitPC124>
    <CouPC125>AU</CouPC125>
    <NADLNGPC>EN</NADLNGPC>
    <HITPC126>AU0123456789348</HITPC126>
  </TRAPRIPC1>", xmlMsgWithNoNamespaces);
		}

		public void TestPopulateTRAPRIPC1_Null()
		{
			dataProviderMock.Setup(m => m.Principal).Returns((ITrader)null);
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			CombineAssertions(() =>
			{
				AssertNotContains("No Group", "<TRAPRIPC1>", xmlMsgWithNoNamespaces);
				AssertContains("Error Collector", "Principal is empty", errorCollector.GetErrorsAsString());
			});
		}

		public void TestPopulateTRACONCO1_NameAddAddress()
		{
			consignorMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consignorMock.Setup(m => m.Name).Returns("STEPHEN BRENNAN PERSONAL SHIPMENTS SPECIALISTS  ");
			consignorMock.Setup(m => m.CompanyName).Returns("STEPHEN BRENNAN PERSONAL SHIPMENTS NAME  ");
			consignorMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			consignorMock.Setup(m => m.StreetAndNumber).Returns("HEERDTER LOHWEG 63-71  ");
			consignorMock.Setup(m => m.PostalCode).Returns("40549");
			consignorMock.Setup(m => m.City).Returns("DUESSELDORF");
			consignorMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.Germany);
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains(@"<TRACONCO1>
    <NamCO17>STEPHEN BRENNAN PERSONAL SHIPMENTS NAME</NamCO17>
    <StrAndNumCO122>HEERDTER LOHWEG 63-71</StrAndNumCO122>
    <PosCodCO123>40549</PosCodCO123>
    <CitCO124>DUESSELDORF</CitCO124>
    <CouCO125>DE</CouCO125>
    <NADLNGCO>EN</NADLNGCO>
  </TRACONCO1>", xmlMsgWithNoNamespaces);
		}

		public void TestPopulateTRACONCE1_NameAddAddress()
		{
			consigneeMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consigneeMock.Setup(m => m.Name).Returns("CREVIN SA  ");
			consigneeMock.Setup(m => m.CompanyName).Returns("CREVIN NAME  ");
			consigneeMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			consigneeMock.Setup(m => m.StreetAndNumber).Returns("CARRER DEL LLOBREGAT 21  ");
			consigneeMock.Setup(m => m.PostalCode).Returns("08223");
			consigneeMock.Setup(m => m.City).Returns("TERRASSA");
			consigneeMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.Spain);
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains(@"<TRACONCE1>
    <NamCE17>CREVIN NAME</NamCE17>
    <StrAndNumCE122>CARRER DEL LLOBREGAT 21</StrAndNumCE122>
    <PosCodCE123>08223</PosCodCE123>
    <CitCE124>TERRASSA</CitCE124>
    <CouCE125>ES</CouCE125>
    <NADLNGCE>EN</NADLNGCE>
  </TRACONCE1>", xmlMsgWithNoNamespaces);
		}

		public void TestPopulateREPREP_NameAddAddress()
		{
			declarantMock.Setup(m => m.TIN).Returns(ZString.Empty);
			declarantMock.Setup(m => m.Name).Returns("CATIMINI ");
			declarantMock.Setup(m => m.NameAndAddressLanguage).Returns("FR");
			declarantMock.Setup(m => m.StreetAndNumber).Returns("94 RUE CHOLETAISE B.P.67");
			declarantMock.Setup(m => m.PostalCode).Returns("63250");
			declarantMock.Setup(m => m.City).Returns("CELLES-SUR-DUROLLE");
			declarantMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.France);
			declarantMock.Setup(m => m.RepresentativeCapacity).Returns("THE BOSS  ");
			declarantMock.Setup(m => m.RepresentativeCapacityLanguage).Returns(ZString.Empty);
			AssertContains(@"<REPREP>
    <NamREP5>CATIMINI</NamREP5>
    <RueEtNREP1006>94 RUE CHOLETAISE B.P.67</RueEtNREP1006>
    <CodPosREP1007>63250</CodPosREP1007>
    <VilREP1008>CELLES-SUR-DUROLLE</VilREP1008>
    <PayREP1009>FR</PayREP1009>
    <RepCapREP18>THE BOSS</RepCapREP18>
  </REPREP>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateREPREP_Null()
		{
			dataProviderMock.Setup(m => m.Declarant).Returns((ITrader)null);
			AssertNotContains("<REPREP>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTRACONCO2_NameAddAddress()
		{
			var consignorMock = new Mock<ITrader>();
			consignorMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consignorMock.Setup(m => m.Name).Returns("DEMAC SRL  ");
			consignorMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			consignorMock.Setup(m => m.StreetAndNumber).Returns("VIA MILANO 42-44  ");
			consignorMock.Setup(m => m.PostalCode).Returns("20011");
			consignorMock.Setup(m => m.City).Returns("LOMBARDIA");
			consignorMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.Italy);

			var goodsItem = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, consignorMock.Object, null, Enumerable.Empty<ZString>(), Enumerable.Empty<IPackage>(), null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			dataProviderMock.Setup(m => m.Consignor).Returns((ITrader)null);
			AssertContains(@"<TRACONCO2>
      <NamCO27>DEMAC SRL</NamCO27>
      <StrAndNumCO222>VIA MILANO 42-44</StrAndNumCO222>
      <PosCodCO223>20011</PosCodCO223>
      <CitCO224>LOMBARDIA</CitCO224>
      <CouCO225>IT</CouCO225>
      <NADLNGGTCO>EN</NADLNGGTCO>
    </TRACONCO2>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTRACONCO2_Null()
		{
			var goodsItem = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<IPackage>(), null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			CombineAssertions(() =>
			{
				AssertNotContains("No Group", "<TRACONCO2>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertEquals("There should be no error when lack of goods item consignor.", 0, errorCollector.ErrorCount);
			});
		}

		public void TestPopulateTRACONCE2_NameAddAddress()
		{
			var consigneeMock = new Mock<ITrader>();
			consigneeMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consigneeMock.Setup(m => m.Name).Returns("SYMPOSIUM RECORDS  ");
			consigneeMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			consigneeMock.Setup(m => m.StreetAndNumber).Returns("110 DERWENT AVENUE  ");
			consigneeMock.Setup(m => m.PostalCode).Returns("EN4 8LZ");
			consigneeMock.Setup(m => m.City).Returns("EAST BARNET");
			consigneeMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.UnitedKingdom);

			var goodsItem = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, null, consigneeMock.Object, Enumerable.Empty<ZString>(), Enumerable.Empty<IPackage>(), null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			dataProviderMock.Setup(m => m.Consignee).Returns((ITrader)null);
			AssertContains(@"<TRACONCE2>
      <NamCE27>SYMPOSIUM RECORDS</NamCE27>
      <StrAndNumCE222>110 DERWENT AVENUE</StrAndNumCE222>
      <PosCodCE223>EN4 8LZ</PosCodCE223>
      <CitCE224>EAST BARNET</CitCE224>
      <CouCE225>GB</CouCE225>
      <NADLNGGICE>EN</NADLNGGICE>
    </TRACONCE2>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTRACONCE2_Null()
		{
			var goodsItem = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<IPackage>(), null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			CombineAssertions(() =>
			{
				AssertNotContains("No Group", "<TRACONCE2>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertEquals("There should be no error when lack of goods item consignee.", 0, errorCollector.ErrorCount);
			});
		}

		public void TestPopulatePACGS2()
		{
			var package = MessageBuilderUtilities.SetupPackage(ZString.Empty, "BX", 10, 15, true, true);
			var goodsItem = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), new IPackage[] { package }, null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			var xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			CombineAssertions(() =>
			{
				AssertNotContains("bulk -> NumOfPacGS24 is not present.", "NumOfPacGS24", xmlMsgWithNoNamespaces);
				AssertNotContains("bulk -> NumOfPieGS25 is not present.", "NumOfPieGS25", xmlMsgWithNoNamespaces);
			});

			package = MessageBuilderUtilities.SetupPackage(ZString.Empty, "BX", 10, 15, false, true);
			goodsItem = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), new IPackage[] { package }, null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			CombineAssertions(() =>
			{
				AssertContains("Not bulk, unpacked -> NumOfPacGS24 is equal to 0.", "<NumOfPacGS24>0</NumOfPacGS24>", xmlMsgWithNoNamespaces);
				AssertContains("Not bulk, unpacked -> NumOfPieGS25 is equal to NumberOfPieces.", "<NumOfPieGS25>15</NumOfPieGS25>", xmlMsgWithNoNamespaces);
			});

			package = MessageBuilderUtilities.SetupPackage(ZString.Empty, "BX", 10, 15, false, false);
			goodsItem = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), new IPackage[] { package }, null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			xmlMsgWithNoNamespaces = messageBuilder.GetXMLMessageWithoutNamespaces();
			CombineAssertions(() =>
			{
				AssertContains("Not bulk, not unpacked -> NumOfPacGS24 is equal to NumberOfPacks.", "<NumOfPacGS24>10</NumOfPacGS24>", xmlMsgWithNoNamespaces);
				AssertContains("Not bulk, not unpacked -> NumOfPieGS25 is equal to 0.", "<NumOfPieGS25>0</NumOfPieGS25>", xmlMsgWithNoNamespaces);
			});
		}

		public void TestPopulateTRACORSECGOO021_NameAddAddress()
		{
			var consignorSecurityMock = new Mock<ITrader>();
			consignorSecurityMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consignorSecurityMock.Setup(m => m.Name).Returns("ZAO KARABACHSKY METALLURGICHESKY ZAVOD  ");
			consignorSecurityMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			consignorSecurityMock.Setup(m => m.StreetAndNumber).Returns("CHELIABINSKAJA OBLAST, KARABASH  ");
			consignorSecurityMock.Setup(m => m.PostalCode).Returns("456140");
			consignorSecurityMock.Setup(m => m.City).Returns("YL. OSVOBOZDENJA URALA 27-A");
			consignorSecurityMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.Poland);

			var goodsItem = SetupGoodsItem(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<IPackage>(), consignorSecurityMock.Object, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			AssertContains(@"<TRACORSECGOO021>
      <NamTRACORSECGOO025>ZAO KARABACHSKY METALLURGICHESKY ZAVOD</NamTRACORSECGOO025>
      <StrNumTRACORSECGOO027>CHELIABINSKAJA OBLAST, KARABASH</StrNumTRACORSECGOO027>
      <PosCodTRACORSECGOO026>456140</PosCodTRACORSECGOO026>
      <CitTRACORSECGOO022>YL. OSVOBOZDENJA URALA 27-A</CitTRACORSECGOO022>
      <CouCodTRACORSECGOO023>PL</CouCodTRACORSECGOO023>
      <TRACORSECGOO021LNG>EN</TRACORSECGOO021LNG>
    </TRACORSECGOO021>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTRACORSECGOO021_Null()
		{
			var goodsItem = SetupGoodsItem(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<IPackage>(), null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			CombineAssertions(() =>
			{
				AssertNotContains("<TRACORSECGOO021>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertEquals("There should be no error when lack of goods item consignor security.", 0, errorCollector.ErrorCount);
			});
		}

		public void TestPopulateTRACONSECGOO013_NameAddAddress()
		{
			var consigneeSecurityMock = new Mock<ITrader>();
			consigneeSecurityMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consigneeSecurityMock.Setup(m => m.Name).Returns("NAZAR TEKSTIL SAN VE TIC A.S.  ");
			consigneeSecurityMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			consigneeSecurityMock.Setup(m => m.StreetAndNumber).Returns("ADANA YOLU UEZERI 17 KM  ");
			consigneeSecurityMock.Setup(m => m.PostalCode).Returns("35410");
			consigneeSecurityMock.Setup(m => m.City).Returns("KAHRAMANMARAS");
			consigneeSecurityMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.Turkey);

			var goodsItem = SetupGoodsItem(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<IPackage>(), null, consigneeSecurityMock.Object).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			AssertContains(@"<TRACONSECGOO013>
      <NamTRACONSECGOO017>NAZAR TEKSTIL SAN VE TIC A.S.</NamTRACONSECGOO017>
      <StrNumTRACONSECGOO019>ADANA YOLU UEZERI 17 KM</StrNumTRACONSECGOO019>
      <PosCodTRACONSECGOO018>35410</PosCodTRACONSECGOO018>
      <CityTRACONSECGOO014>KAHRAMANMARAS</CityTRACONSECGOO014>
      <CouCodTRACONSECGOO015>TR</CouCodTRACONSECGOO015>
      <TRACONSECGOO013LNG>EN</TRACONSECGOO013LNG>
    </TRACONSECGOO013>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTRACONSECGOO013_Null()
		{
			var goodsItem = SetupGoodsItem(ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<IPackage>(), null, null).Object;
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem });
			CombineAssertions(() =>
			{
				AssertNotContains("No Group", "<TRACONSECGOO013>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertEquals("There should be no error when lack of goods item consignee security.", 0, errorCollector.ErrorCount);
			});
		}

		public void TestPopulateTRACORSEC037_NameAddAddress()
		{
			securityConsignorMock.Setup(m => m.TIN).Returns(ZString.Empty);
			securityConsignorMock.Setup(m => m.Name).Returns("PROFEC TECHNOLOGIES  ");
			securityConsignorMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			securityConsignorMock.Setup(m => m.StreetAndNumber).Returns("10 BETTS AVENUE  ");
			securityConsignorMock.Setup(m => m.PostalCode).Returns("IP53R");
			securityConsignorMock.Setup(m => m.City).Returns("MARTLESHAM HEATH");
			securityConsignorMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.UnitedKingdom);

			AssertContains(@"<TRACORSEC037>
    <NamTRACORSEC041>PROFEC TECHNOLOGIES</NamTRACORSEC041>
    <StrNumTRACORSEC043>10 BETTS AVENUE</StrNumTRACORSEC043>
    <PosCodTRACORSEC042>IP53R</PosCodTRACORSEC042>
    <CitTRACORSEC038>MARTLESHAM HEATH</CitTRACORSEC038>
    <CouCodTRACORSEC039>GB</CouCodTRACORSEC039>
    <TRACORSEC037LNG>EN</TRACORSEC037LNG>
  </TRACORSEC037>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTRACORSEC037_Null()
		{
			dataProviderMock.Setup(m => m.SecurityConsignor).Returns((ITrader)null);
			CombineAssertions(() =>
			{
				AssertNotContains("No Group", "<TRACORSEC037>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertEquals("There should be no error when lack of consignee security.", 0, errorCollector.ErrorCount);
			});
		}

		public void TestPopulateTRACONSEC029_NameAddAddress()
		{
			securityConsigneeMock.Setup(m => m.TIN).Returns(ZString.Empty);
			securityConsigneeMock.Setup(m => m.Name).Returns("NCD LTD  ");
			securityConsigneeMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-EN");
			securityConsigneeMock.Setup(m => m.StreetAndNumber).Returns("UNIT 55, BATTERSEA BUSINESS CENTRE  ");
			securityConsigneeMock.Setup(m => m.PostalCode).Returns("SW5 6QL");
			securityConsigneeMock.Setup(m => m.City).Returns("LAVENDER HILL");
			securityConsigneeMock.Setup(m => m.CountryCode).Returns(Core.Constants.CountryCodes.UnitedKingdom);

			AssertContains(@"<TRACONSEC029>
    <NameTRACONSEC033>NCD LTD</NameTRACONSEC033>
    <StrNumTRACONSEC035>UNIT 55, BATTERSEA BUSINESS CENTRE</StrNumTRACONSEC035>
    <PosCodTRACONSEC034>SW5 6QL</PosCodTRACONSEC034>
    <CitTRACONSEC030>LAVENDER HILL</CitTRACONSEC030>
    <CouCodTRACONSEC031>GB</CouCodTRACONSEC031>
    <TRACONSEC029LNG>EN</TRACONSEC029LNG>
  </TRACONSEC029>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTRACONSEC029_Null()
		{
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns((ITrader)null);
			CombineAssertions(() =>
			{
				AssertNotContains("No Group", "<TRACONSEC029>", messageBuilder.GetXMLMessageWithoutNamespaces());
				AssertEquals("There should be no error when lack of consignee security.", 0, errorCollector.ErrorCount);
			});
		}
		public void TestSecHEA358()
		{
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(false);
			AssertNotContains(@"<SecHEA358>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			AssertContains(@"<SecHEA358>1", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestCheckConsignorOnGoodsItem()
		{
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(ZBool.True);
			var goodsItem1Mock = SetupGoodsItem("1", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<EU.NCTS.Business.PackageWrapper>(), null, null);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1Mock.Object });

			errorCollector.WipeErrors();
			dataProviderMock.Setup(m => m.SecurityConsignor).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.ConsignorSecurity).Returns((ITrader)null);
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("Error when no consignor on security level and goods item level.", "The 'safety-security' attribute being checked, the security shipper must be entered at Security Consignor level or at Articles / Goods Item level.", errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			dataProviderMock.Setup(m => m.SecurityConsignor).Returns(securityConsignorMock.Object);
			goodsItem1Mock.Setup(m => m.ConsignorSecurity).Returns((ITrader)null);
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("No error when has consignor on security level.", 0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			dataProviderMock.Setup(m => m.SecurityConsignor).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.ConsignorSecurity).Returns(securityConsignorMock.Object);
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("No error when has consignor on goods item level.", 0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			var goodsItem2Mock = SetupGoodsItem("2", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<EU.NCTS.Business.PackageWrapper>(), null, null);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1Mock.Object, goodsItem2Mock.Object });
			dataProviderMock.Setup(m => m.SecurityConsignor).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.ConsignorSecurity).Returns((ITrader)null);
			goodsItem2Mock.Setup(m => m.ConsignorSecurity).Returns((ITrader)null);
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("Should have one error, even there are two goods items.", @"The 'safety-security' attribute being checked, the security shipper must be entered at Security Consignor level or at Articles / Goods Item level.", errorCollector.GetErrorsAsString());
		}

		public void TestCheckConsigneeOnGoodsItem()
		{
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(ZBool.True);
			var goodsItem1Mock = SetupGoodsItem("1", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<EU.NCTS.Business.PackageWrapper>(), null, null);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1Mock.Object });

			errorCollector.WipeErrors();
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.ConsigneeSecurity).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.SpecialMentions).Returns((IReadOnlyCollection<IStatement>)Enumerable.Empty<IStatement>());
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("Error when no consignee on security level and goods item level, and no 10600 special mention.", "The 'safety-security' attribute being checked, the safety recipient must be entered at Security Consignee level or at Articles / Goods Item level.", errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns(securityConsigneeMock.Object);
			goodsItem1Mock.Setup(m => m.ConsigneeSecurity).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.SpecialMentions).Returns((IReadOnlyCollection<IStatement>)Enumerable.Empty<IStatement>());
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("No error when has consignee on security level.", 0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.ConsigneeSecurity).Returns(securityConsigneeMock.Object);
			goodsItem1Mock.Setup(m => m.SpecialMentions).Returns((IReadOnlyCollection<IStatement>)Enumerable.Empty<IStatement>());
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("No error when has consignee on goods item level.", 0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.ConsigneeSecurity).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.SpecialMentions).Returns(new[] { MessageBuilderUtilities.SetupSpecialMention("AI1", "00100", "1", Core.Constants.CountryCodes.Andorra) });
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("Error because the special mention is not 10600.", "The 'safety-security' attribute being checked, the safety recipient must be entered at Security Consignee level or at Articles / Goods Item level.", errorCollector.GetErrorsAsString());

			errorCollector.WipeErrors();
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.ConsigneeSecurity).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.SpecialMentions).Returns(new[] { MessageBuilderUtilities.SetupSpecialMention("AI1", "10600", "1", Core.Constants.CountryCodes.Andorra) });
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("No error when has 10600 special mention.", 0, errorCollector.ErrorCount);

			errorCollector.WipeErrors();
			var goodsItem2Mock = SetupGoodsItem(
				"2", ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<EU.NCTS.Business.PackageWrapper>(), null, null);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItem1Mock.Object, goodsItem2Mock.Object });
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.ConsigneeSecurity).Returns((ITrader)null);
			goodsItem1Mock.Setup(m => m.SpecialMentions).Returns((IReadOnlyCollection<IStatement>)Enumerable.Empty<IStatement>());
			goodsItem2Mock.Setup(m => m.ConsigneeSecurity).Returns((ITrader)null);
			goodsItem2Mock.Setup(m => m.SpecialMentions).Returns((IReadOnlyCollection<IStatement>)Enumerable.Empty<IStatement>());
			messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertEquals("Should have only one error, even the two goods item both have errors.", @"The 'safety-security' attribute being checked, the safety recipient must be entered at Security Consignee level or at Articles / Goods Item level.", errorCollector.GetErrorsAsString());
		}

		public void TestPopulateDonSurSecHEA1001()
		{
			dataProviderMock.Setup(m => m.ProvisionalTransitDepartureDate).Returns("07122020");
			dataProviderMock.Setup(m => m.IsPrelodge).Returns(false);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(false);
			AssertContains(@"<DonSurSecHEA1001>0</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsPrelodge).Returns(false);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.NotEU);
			AssertContains(@"<DonSurSecHEA1001>2</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsPrelodge).Returns(false);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.None);
			AssertContains(@"<DonSurSecHEA1001>2</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsPrelodge).Returns(false);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			AssertContains(@"<DonSurSecHEA1001>0</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(false);
			AssertContains(@"<DonSurSecHEA1001>0</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.NotEU);
			AssertContains(@"<DonSurSecHEA1001>2</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.None);
			AssertContains(@"<DonSurSecHEA1001>2</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.NotEU);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			AssertContains(@"<DonSurSecHEA1001>1</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.None);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			AssertContains(@"<DonSurSecHEA1001>1</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			AssertContains(@"<DonSurSecHEA1001>0</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTINCARTRA254()
		{
			dataProviderMock.Setup(m => m.SecurityCarrierEORI).Returns("FR1234567");
			dataProviderMock.Setup(m => m.ProvisionalTransitDepartureDate).Returns("07122020");
			dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.NotEU);
			AssertContains(@"<DonSurSecHEA1001>1</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains("<CARTRA100>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains("<TINCARTRA254>FR1234567", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.NotEU);
			AssertContains(@"<DonSurSecHEA1001>2</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("CARTRA100", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("TINCARTRA254", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.NorthernIreland);
			AssertContains(@"<DonSurSecHEA1001>0</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains("<CARTRA100>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains("<TINCARTRA254>FR1234567", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateCouCodTRACORSEC039()
		{
			dataProviderMock.Setup(m => m.ProvisionalTransitDepartureDate).Returns("07122020");
			dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.NotEU);
			AssertContains(@"<DonSurSecHEA1001>1</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains(@"<TINTRACORSEC044>GB0123456789004</TINTRACORSEC044>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains("<CouCodTRACORSEC039>FR", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.NotEU);
			AssertContains(@"<DonSurSecHEA1001>2</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains(@"<TINTRACORSEC044>GB0123456789004</TINTRACORSEC044>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("<CouCodTRACORSEC039>FR<", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.NorthernIreland);
			AssertContains(@"<DonSurSecHEA1001>0</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains(@"<TINTRACORSEC044>GB0123456789004</TINTRACORSEC044>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("<CouCodTRACORSEC039>FR<", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestPopulateTINTRACONSEC036()
		{
			dataProviderMock.Setup(m => m.ProvisionalTransitDepartureDate).Returns("07122020");
			dataProviderMock.Setup(m => m.IsPrelodge).Returns(true);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.NotEU);
			AssertContains(@"<DonSurSecHEA1001>1</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains(@"<TINTRACONSEC036>GB0123456789005</TINTRACONSEC036>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("<CouCodTRACONSEC031>FR<", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.NotEU);
			AssertContains(@"<DonSurSecHEA1001>2</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains(@"<TINTRACONSEC036>GB0123456789005</TINTRACONSEC036>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains("<CouCodTRACONSEC031>FR<", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.NorthernIreland);
			AssertContains(@"<DonSurSecHEA1001>0</DonSurSecHEA1001>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains(@"<TINTRACONSEC036>GB0123456789005</TINTRACONSEC036>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("<CouCodTRACONSEC031>FR<", messageBuilder.GetXMLMessageWithoutNamespaces());
		}

		public void TestOnlyPopulateWhenSafetyAndSecurityData()
		{
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCodeLanguage).Returns("UK");
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(ZBool.False);
			var message1 = messageBuilder.GetXMLMessageWithoutNamespaces();

			var fields = new List<string> {
				"<ITI>", "<CARTRA100>", "<TRACORSEC037>", "<TRACONSEC029>",
				"<ComRefNumHEA>", "<TraChaMetOfPayHEA1>", "<SpeCirIndHEA1>", "<SecHEA358>", "<ConRefNumHEA>", "<CodPlUnHEA357>", "<CodPlUnHEA357LNG>",
				"<MetOfPayGDI12>", "<ComRefNumGIM1>", "<UNDanGooCodGDI1>", "<TRACORSECGOO021>", "<TRACONSECGOO013>"
			};

			CombineAssertions(() =>
			{
				foreach (var field in fields)
				{
					AssertNotContains(field, message1);
				}
			});

			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(ZBool.True);
			var message2 = messageBuilder.GetXMLMessageWithoutNamespaces();

			CombineAssertions(() =>
			{
				foreach (var field in fields)
				{
					AssertContains(field, message2);
				}
			});
		}

		public void TestPopulateTRACONCO2_OnlyIfWrapperConsignorIsNull()
		{
			var orgAddress = Factory.New<OrgHeader>().MainAddress;
			var traderWrapper = EU.NCTS.Business.TraderWrapper.New(orgAddress, true, false);

			dataProviderMock.Setup(m => m.Consignor).Returns(traderWrapper);

			var goodsItemMock = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<EU.NCTS.Business.PackageWrapper>(), null, null);
			goodsItemMock.Setup(m => m.Consignor).Returns(traderWrapper);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItemMock.Object });

			var message = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertNotContains("<TRACONCO2>", message);
			dataProviderMock.Setup(m => m.Consignor).Returns((ITrader)null);

			message = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<TRACONCO2>", message);
		}

		public void TestPopulateTRACONCE2_OnlyIfWrapperConsigneeIsNull()
		{
			var orgAddress = Factory.New<OrgHeader>().MainAddress;
			var traderWrapper = EU.NCTS.Business.TraderWrapper.New(orgAddress, true, false);

			dataProviderMock.Setup(m => m.Consignee).Returns(traderWrapper);

			var goodsItemMock = SetupGoodsItem(
				ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty, ZString.Empty,
				ZString.Empty, ZString.Empty, null, null, Enumerable.Empty<ZString>(), Enumerable.Empty<EU.NCTS.Business.PackageWrapper>(), null, null);
			goodsItemMock.Setup(m => m.Consignee).Returns(traderWrapper);
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new[] { goodsItemMock.Object });

			var message = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertNotContains("<TRACONCE2>", message);

			dataProviderMock.Setup(m => m.Consignee).Returns((ITrader)null);

			message = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<TRACONCE2>", message);
		}

		protected override void SetUp()
		{
			base.SetUp();

			principalMock = new Mock<ITrader>();
			principalMock.Setup(m => m.TIN).Returns("GB0123456789001");

			consignorMock = new Mock<ITrader>();
			consignorMock.Setup(m => m.TIN).Returns("GB0123456789002");

			consigneeMock = new Mock<ITrader>();
			consigneeMock.Setup(m => m.TIN).Returns("GB0123456789003");

			declarantMock = new Mock<ITrader>();
			declarantMock.Setup(m => m.TIN).Returns("FR0123456789002");

			securityConsignorMock = new Mock<ITrader>();
			securityConsignorMock.Setup(m => m.TIN).Returns("GB0123456789004");
			securityConsignorMock.Setup(m => m.CountryCode).Returns("FR");

			securityConsigneeMock = new Mock<ITrader>();
			securityConsigneeMock.Setup(m => m.TIN).Returns("GB0123456789005");
			securityConsigneeMock.Setup(m => m.CountryCode).Returns("FR");

			var consignorGoodsItemMock = new Mock<ITrader>();
			consignorGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789006");

			var consigneeGoodsItemMock = new Mock<ITrader>();
			consigneeGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789007");

			var consignorSecurityGoodsItemMock = new Mock<ITrader>();
			consignorSecurityGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789008");
			consignorSecurityGoodsItemMock.Setup(m => m.CountryCode).Returns("FR");

			var consigneeSecurityGoodsItemMock = new Mock<ITrader>();
			consigneeSecurityGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789009");
			consigneeSecurityGoodsItemMock.Setup(m => m.CountryCode).Returns("FR");

			var package1 = MessageBuilderUtilities.SetupPackage("MARKA", "BX", 10, 0, false, false);
			var package2 = MessageBuilderUtilities.SetupPackage("MARKB", "CT", 20, 0, false, false);
			var packages = new IPackage[] { package1, package2 };

			var goodsItem1 = SetupGoodsItem("1", "1234567891", "T1", "DESC1", "A1", "K1", "C1", "U1", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, Enumerable.Empty<ZString>(), packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;
			var goodsItem2 = SetupGoodsItem("2", "1234567892", "T2", "DESC2", "A2", "K2", "C2", "U2", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, new ZString[] { "CONTAINER1", "CONTAINER2" }, packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;
			var goodsItem3 = SetupGoodsItem("10", "1234567891", "T1", "DESC1", "A1", "K1", "C1", "U1", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, Enumerable.Empty<ZString>(), packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;

			dataProviderMock = new Mock<ICC015BDeclaration>();
			dataProviderMock.Setup(m => m.IsProduction).Returns(true);
			dataProviderMock.Setup(m => m.AgreementNumber).Returns("12345678");
			dataProviderMock.Setup(m => m.PrincipalTIN).Returns("FR0123456789002");
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("NCT00000001");
			dataProviderMock.Setup(m => m.TypeOfDeclaration).Returns("T-");
			dataProviderMock.Setup(m => m.CountryOfDestinationCode).Returns(Core.Constants.CountryCodes.Russia);
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsCode).Returns("PRE-LODGED");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoods).Returns("LOC");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.AuthorisedLocationOfGoodsCode).Returns("PRE-LODGED");
			dataProviderMock.Setup(m => m.PlaceOfLoadingCode).Returns("CNKWE");
			dataProviderMock.Setup(m => m.PlaceOfLoading).Returns(new ZString("Guiyang Longdongbao International A").SubstringSafe(0, 17));
			dataProviderMock.Setup(m => m.CountryOfDispatchExportCode).Returns(Core.Constants.CountryCodes.Singapore);
			dataProviderMock.Setup(m => m.CustomsSubPlace).Returns("DOVER ERTS");
			dataProviderMock.Setup(m => m.InlandTransportMode).Returns("1");
			dataProviderMock.Setup(m => m.TransportModeAtBorder).Returns("2");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDeparture).Returns("REG DEP1");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDepartureLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportAtDeparture).Returns(Core.Constants.CountryCodes.NewZealand);
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorder).Returns("REG DEP2");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorderLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportCrossingBorder).Returns(Core.Constants.CountryCodes.Japan);
			dataProviderMock.Setup(m => m.TypeOfMeansOfTransportCrossingBorder).Returns("2");
			dataProviderMock.Setup(m => m.IsContainerised).Returns(true);
			dataProviderMock.Setup(m => m.DialogLanguageIndicatorAtDeparture).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.AccompanyingDocumentLanguage).Returns("EN");
			dataProviderMock.Setup(m => m.TotalNumberOfItems).Returns(2);
			dataProviderMock.Setup(m => m.TotalNumberOfPackages).Returns(new ZLong(60));
			dataProviderMock.Setup(m => m.TotalGrossMass).Returns(20.2468m);
			dataProviderMock.Setup(m => m.TotalInvoiceValue).Returns(123.456m);
			dataProviderMock.Setup(m => m.DeclarationPlace).Returns("Brisbane");
			dataProviderMock.Setup(m => m.DeclarationPlaceLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.DeclarationDate).Returns("19710918");
			dataProviderMock.Setup(m => m.SpecificCircumstanceIndicator).Returns("E");
			dataProviderMock.Setup(m => m.TransportChargesMethodOfPayment).Returns("Y");
			dataProviderMock.Setup(m => m.CommercialReferenceNumber).Returns("COMM-REF1");
			dataProviderMock.Setup(m => m.SecurityIndicator).Returns(false);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.IsPrelodge).Returns(false);
			dataProviderMock.Setup(m => m.ModeOfRepresentation).Returns("1");
			dataProviderMock.Setup(m => m.ConveyanceReferenceNumber).Returns("CONV-REF1");
			dataProviderMock.Setup(m => m.TransportReferenceNumber).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCode).Returns("UAODS");
			dataProviderMock.Setup(m => m.PlaceOfUnloading).Returns(new ZString("Odesa").SubstringSafe(0, 17));
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCodeLanguage).Returns(ZString.Empty);
			dataProviderMock.Setup(m => m.Principal).Returns(principalMock.Object);
			dataProviderMock.Setup(m => m.Consignor).Returns(consignorMock.Object);
			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			dataProviderMock.Setup(m => m.AuthorisedConsigneeTIN).Returns("GB0123456789003");
			dataProviderMock.Setup(m => m.DepartureCustomsOfficeReferenceNumber).Returns("AA123456");
			dataProviderMock.Setup(m => m.TransitCustomsOffices).Returns(new List<EU.NCTS.Messaging.ICustomsOffice>() { MessageBuilderUtilities.SetupCustomsOffice("NN123456", "201210120606"), MessageBuilderUtilities.SetupCustomsOffice("TT123456", "201210130707") });
			dataProviderMock.Setup(m => m.DestinationCustomsOfficeReferenceNumber).Returns("ZZ123456");
			dataProviderMock.Setup(m => m.ControlResultCode).Returns("A3");
			dataProviderMock.Setup(m => m.ControlResultDateLimit).Returns("20111220");
			dataProviderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			dataProviderMock.Setup(m => m.NumberOfSeals).Returns(3);
			dataProviderMock.Setup(m => m.NatureOfSeals).Returns("1");
			dataProviderMock.Setup(m => m.Seals).Returns(new List<ISealID>() { MessageBuilderUtilities.SetupSeal("SEAL1"), MessageBuilderUtilities.SetupSeal("SEAL2"), MessageBuilderUtilities.SetupSeal("SEAL3") });
			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { MessageBuilderUtilities.SetupGuarantee("9", "12346789", "AAAAAAAAAA", "ABCD", 12.68m), MessageBuilderUtilities.SetupGuarantee("1", "987654321", "BBBBB", "WXYZ", 32.33m) });
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem1, goodsItem2, goodsItem3 });
			dataProviderMock.Setup(m => m.Itinerary).Returns(new List<ZString>() { Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Switzerland });
			dataProviderMock.Setup(m => m.SecurityConsignor).Returns(securityConsignorMock.Object);
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns(securityConsigneeMock.Object);
			dataProviderMock.Setup(m => m.SecurityConsignorCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityConsigneeCountryGroup).Returns(SecurityTraderCountryGroup.EuForSafetyAndSecurity);
			dataProviderMock.Setup(m => m.SecurityCarrierEORI).Returns("FR0123456789002");
			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(false);

			errorCollector = new ErrorCollector();
			messageBuilder = new CC015BMessageBuilder(dataProviderMock.Object, new NctsMessageFunctionSet.DeclarationDataMessage(), errorCollector);
		}
		Mock<ITrader> principalMock;
		Mock<ITrader> consignorMock;
		Mock<ITrader> consigneeMock;
		Mock<ITrader> declarantMock;
		Mock<ITrader> securityConsignorMock;
		Mock<ITrader> securityConsigneeMock;
		Mock<ICC015BDeclaration> dataProviderMock;
		ErrorCollector errorCollector;
		CC015BMessageBuilder messageBuilder;

		Mock<IDepartureGoodsItem> SetupGoodsItem(ZString itemNumber, ZString commodityCode, ZString typeOfDeclaration, ZString goodsDescription, ZString countryOfDispatchExportCode, ZString countryOfDestinationCode, ZString commercialReferenceNumber, ZString dangerousGoodsCode, ITrader consignor, ITrader consignee,
			IEnumerable<ZString> containers, IEnumerable<IPackage> packages, ITrader consignorSecurity, ITrader consigneeSecurity)
		{
			var result = new Mock<IDepartureGoodsItem>();
			result.Setup(m => m.ItemNumber).Returns(new ZInt(itemNumber));
			result.Setup(m => m.CommodityCode).Returns(commodityCode);
			result.Setup(m => m.TypeOfDeclaration).Returns(typeOfDeclaration);
			result.Setup(m => m.GoodsDescription).Returns(goodsDescription);
			result.Setup(m => m.GoodsDescriptionLanguage).Returns(ZString.Empty);
			result.Setup(m => m.GrossMass).Returns(10.1234m);
			result.Setup(m => m.NetMass).Returns(1.1234m);
			result.Setup(m => m.CountryOfDispatchExportCode).Returns(countryOfDispatchExportCode);
			result.Setup(m => m.CountryOfDestinationCode).Returns(countryOfDestinationCode);
			result.Setup(m => m.TransportChargesMethodOfPayment).Returns("M");
			result.Setup(m => m.CommercialReferenceNumber).Returns(commercialReferenceNumber);
			result.Setup(m => m.UNDangerousGoodsCode).Returns(dangerousGoodsCode);
			result.Setup(m => m.BillValue).Returns(123.456m);
			result.Setup(m => m.PreviousAdministrativeReferences).Returns(new IPreviousAdministrativeReference[] { MessageBuilderUtilities.SetupPreviousAdministrativeReference("T1", "PD1"), MessageBuilderUtilities.SetupPreviousAdministrativeReference("T2", "PD2") });
			result.Setup(m => m.ProducedDocumentsCertificates).Returns(new IProducedDocumentCertificate[] { MessageBuilderUtilities.SetupDocumentCertificate("380", "SD1", "REF1"), MessageBuilderUtilities.SetupDocumentCertificate("18", "SD2", "REF2") });
			result.Setup(m => m.SpecialMentions).Returns(new IStatement[] { MessageBuilderUtilities.SetupSpecialMention("AI1", "00100", "1", Core.Constants.CountryCodes.Andorra), MessageBuilderUtilities.SetupSpecialMention("AI2", "00200", "0", Core.Constants.CountryCodes.Iceland) });
			result.Setup(m => m.Consignor).Returns(consignor);
			result.Setup(m => m.Consignee).Returns(consignee);
			result.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)containers);
			result.Setup(m => m.Packages).Returns((IReadOnlyCollection<IPackage>)packages);
			result.Setup(m => m.ConsignorSecurity).Returns(consignorSecurity);
			result.Setup(m => m.ConsigneeSecurity).Returns(consigneeSecurity);
			return result;
		}
	}
}
