using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.EU.NCTS.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.GB.GovernmentGateway.GatewayApplications.CTC.Messaging.Testing
{
	class CC015BMessageBuilderTest : TestCaseWithFactory
	{
		public void TestPopulateMessageHeaderSets_AppRefMES14()
		{
			CreateMockSetups_V1();

			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			AssertContains("Should set AppRefMES14=NCTS", @"<AppRefMES14>NCTS</AppRefMES14>", messageBuilder.GetXMLMessageWithoutNamespaces());
			dataProviderMock.VerifyAll();
		}

		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC015BMessageBuilder_SafetyAndSecurity()
		{
			var principalMock = new Mock<ITrader>();
			principalMock.Setup(m => m.TIN).Returns("GB0123456789001");
			principalMock.Setup(m => m.Name).Returns("Principal Name");
			principalMock.Setup(m => m.StreetAndNumber).Returns("Principal Street and Number");
			principalMock.Setup(m => m.PostalCode).Returns("Principal Postcode");
			principalMock.Setup(m => m.CountryCode).Returns("Principal Country");
			principalMock.Setup(m => m.City).Returns("Principal City");

			var consignorMock = new Mock<ITrader>();
			consignorMock.Setup(m => m.TIN).Returns("GB0123456789002");
			consignorMock.Setup(m => m.Name).Returns("Consignor Name");
			consignorMock.Setup(m => m.StreetAndNumber).Returns("Consignor Street and Number");
			consignorMock.Setup(m => m.PostalCode).Returns("Consignor Postcode");
			consignorMock.Setup(m => m.CountryCode).Returns("Consignor Country");
			consignorMock.Setup(m => m.City).Returns("Consignor City");
			consignorMock.Setup(m => m.NameAndAddressLanguage).Returns("");

			var consigneeMock = new Mock<ITrader>();
			consigneeMock.Setup(m => m.TIN).Returns("GB0123456789003");
			consigneeMock.Setup(m => m.Name).Returns("Consignee Name");
			consigneeMock.Setup(m => m.StreetAndNumber).Returns("Consignee Street and Number");
			consigneeMock.Setup(m => m.PostalCode).Returns("Consignee Postcode");
			consigneeMock.Setup(m => m.CountryCode).Returns("Consignee Country");
			consigneeMock.Setup(m => m.City).Returns("Consignee City");
			consigneeMock.Setup(m => m.NameAndAddressLanguage).Returns("");

			declarantMock = CTCMessageBuilderUtilities.SetupTrader("Declarant1", "", "", "", "", "", "GB0123456789004", "", "Representative", "EN-US");
			declarantMock.Reset();

			carrierMock = CTCMessageBuilderUtilities.SetupTrader("Carrier1", "Carrier Addr", "Carrier Post", "Carrier City", "IE", "EN-US", "GB0123456789005", "", "", "");
			carrierMock.Reset();

			var securityConsignorMock = new Mock<ITrader>();
			securityConsignorMock.Setup(m => m.TIN).Returns("GB0123456789006");

			var securityConsigneeMock = new Mock<ITrader>();
			securityConsigneeMock.Setup(m => m.TIN).Returns("GB0123456789007");

			var consignorGoodsItemMock = new Mock<ITrader>();
			var consigneeGoodsItemMock = new Mock<ITrader>();
			var consignorSecurityGoodsItemMock = new Mock<ITrader>();
			var consigneeSecurityGoodsItemMock = new Mock<ITrader>();

			var package1 = CTCMessageBuilderUtilities.SetupPackage("MARKA", "BX", 10, 0, false, false);
			var package2 = CTCMessageBuilderUtilities.SetupPackage("MARKB", "CT", 20, 0, false, false);
			var packages = new IPackage[] { package1, package2 };
			var goodsItem1 = SetupGoodsItem("1", "123456780000", "T1", "DESC1", "A1", "K1", "C1", "U1", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, Enumerable.Empty<ZString>(), packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;
			var goodsItem2 = SetupGoodsItem("2", "123456780000", "T2", "DESC2", "A2", "K2", "C2", "U2", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, new ZString[] { "CONTAINER1", "CONTAINER2" }, packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;

			var dataProviderMock = new Mock<ICC015BDeclaration>();
			dataProviderMock.Setup(m => m.IsSimplifiedNctsProcedure).Returns(false);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("NCTS");
			dataProviderMock.Setup(m => m.TypeOfDeclaration).Returns("T1");
			dataProviderMock.Setup(m => m.CountryOfDestinationCode).Returns(Core.Constants.CountryCodes.Italy);
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsCode).Returns("PRE-LODGED");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoods).Returns("LOC");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.PlaceOfLoadingCode).Returns("DOVER007");
			dataProviderMock.Setup(m => m.CountryOfDispatchExportCode).Returns(Core.Constants.CountryCodes.UnitedKingdom);
			dataProviderMock.Setup(m => m.CustomsSubPlace).Returns("DOVER ERTS");
			dataProviderMock.Setup(m => m.InlandTransportMode).Returns("1");
			dataProviderMock.Setup(m => m.TransportModeAtBorder).Returns("3");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDeparture).Returns("NC15 REG1");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDepartureLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportAtDeparture).Returns(Core.Constants.CountryCodes.Ireland);
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorder).Returns("NC15 REG2");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorderLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportCrossingBorder).Returns(Core.Constants.CountryCodes.France);
			dataProviderMock.Setup(m => m.TypeOfMeansOfTransportCrossingBorder).Returns("2");
			dataProviderMock.Setup(m => m.IsContainerised).Returns(true);
			dataProviderMock.Setup(m => m.DialogLanguageIndicatorAtDeparture).Returns("EN-US");
			dataProviderMock.Setup(m => m.TotalNumberOfPackages).Returns(new ZLong(60));
			dataProviderMock.Setup(m => m.TotalGrossMass).Returns(20.2222m);
			dataProviderMock.Setup(m => m.DeclarationPlace).Returns("Dover");
			dataProviderMock.Setup(m => m.DeclarationPlaceLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.DeclarationDate).Returns("19980809");
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.AuthorisedConsigneeTIN).Returns("GB0123456789012");
			dataProviderMock.Setup(m => m.DepartureCustomsOfficeReferenceNumber).Returns("AA123456");
			dataProviderMock.Setup(m => m.TransitCustomsOffices).Returns(new List<EU.NCTS.Messaging.ICustomsOffice>() { CTCMessageBuilderUtilities.SetupCustomsOffice("NN123456", "201210120606"), CTCMessageBuilderUtilities.SetupCustomsOffice("TT123456", "201210130707") });
			dataProviderMock.Setup(m => m.DestinationCustomsOfficeReferenceNumber).Returns("ZZ123456");
			dataProviderMock.Setup(m => m.ControlResultCode).Returns("A3");
			dataProviderMock.Setup(m => m.ControlResultDateLimit).Returns("19980816");
			dataProviderMock.Setup(m => m.Seals).Returns(new List<ISealID>() { CTCMessageBuilderUtilities.SetupSeal("SEAL1"), CTCMessageBuilderUtilities.SetupSeal("SEAL2"), CTCMessageBuilderUtilities.SetupSeal("SEAL3") });
			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { CTCMessageBuilderUtilities.SetupGuarantee("9", "12346789", "AAAAAAAAAA", "ABCD", 5, new ZString[] { "DE" }), CTCMessageBuilderUtilities.SetupGuarantee("1", "987654321", "BBBBB", "WXYZ", 10, Enumerable.Empty<ZString>()) });
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem1, goodsItem2 });
			dataProviderMock.Setup(m => m.Itinerary).Returns(new List<ZString>() { Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Switzerland });

			dataProviderMock.Setup(m => m.CommercialReferenceNumber).Returns("HQDOV007");
			dataProviderMock.Setup(m => m.ConveyanceReferenceNumber).Returns("MONOPOLI007");
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCode).Returns("UnloadingPlace");
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCodeLanguage).Returns("EN-US");

			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(false);
			dataProviderMock.Setup(m => m.SpecificCircumstanceIndicator).Returns("B");
			dataProviderMock.Setup(m => m.TransportChargesMethodOfPayment).Returns("Y");
			dataProviderMock.Setup(m => m.Principal).Returns(principalMock.Object);
			dataProviderMock.Setup(m => m.Consignor).Returns(consignorMock.Object);
			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			dataProviderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			dataProviderMock.Setup(m => m.Carrier).Returns(carrierMock.Object);
			dataProviderMock.Setup(m => m.SecurityConsignor).Returns(securityConsignorMock.Object);
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns(securityConsigneeMock.Object);

			dataProviderMock.Setup(m => m.IsConsigneeDefinedAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.IsConsignorDefinedAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(false);

			errorCollector = new ErrorCollector();
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);

			AssertSecurityNodes(false, dataProviderMock);
			AssertSecurityNodes(true, dataProviderMock);

			principalMock.VerifyAll();
			consignorMock.VerifyAll();
			consigneeMock.VerifyAll();
			declarantMock.VerifyAll();
			carrierMock.VerifyAll();
			securityConsignorMock.VerifyAll();
			securityConsigneeMock.VerifyAll();
			consignorGoodsItemMock.VerifyAll();
			consigneeGoodsItemMock.VerifyAll();
			consignorSecurityGoodsItemMock.VerifyAll();
			consigneeSecurityGoodsItemMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		void AssertSecurityNodes(bool safetyAndSecurityDataValue, Mock<ICC015BDeclaration> dataProviderMock)
		{
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(safetyAndSecurityDataValue);
			AssertEquals("pre-req", safetyAndSecurityDataValue, dataProviderMock.Object.SafetyAndSecurityData);

			var xmlMessage = messageBuilder.GetXMLMessageWithoutNamespaces();

			foreach (var headerSecurityNode in new string[]
			{
				"SpeCirIndHEA1",
				"TraChaMetOfPayHEA1",
				"ComRefNumHEA",
				"ConRefNumHEA",
				"CodPlUnHEA357",
				"TRACORSEC037",
				"TRACONSEC029",
				"CARTRA100",
				"CouOfRouCodITI1",
				"MetOfPayGDI12",
				"ComRefNumGIM1",
				"UNDanGooCodGDI1"
			})
			{
				if (safetyAndSecurityDataValue)
				{
					AssertContains(headerSecurityNode, xmlMessage);
				}
				else
				{
					AssertNotContains(headerSecurityNode, xmlMessage);
				}
			}
		}

		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC015BMessageBuilder()
		{
			var principalMock = new Mock<ITrader>();
			principalMock.Setup(m => m.TIN).Returns("GB0123456789001");
			principalMock.Setup(m => m.HolderIDTIR).Returns("TIR001");
			principalMock.Setup(m => m.Name).Returns("Principal Name");
			principalMock.Setup(m => m.StreetAndNumber).Returns("Principal Street and Number");
			principalMock.Setup(m => m.PostalCode).Returns("Principal Postcode");
			principalMock.Setup(m => m.CountryCode).Returns("Principal Country");
			principalMock.Setup(m => m.City).Returns("Principal City");

			var consignorMock = new Mock<ITrader>();
			consignorMock.Setup(m => m.TIN).Returns("GB0123456789002");
			consignorMock.Setup(m => m.Name).Returns("Consignor Name");
			consignorMock.Setup(m => m.StreetAndNumber).Returns("Consignor Street and Number");
			consignorMock.Setup(m => m.PostalCode).Returns("Consignor Postcode");
			consignorMock.Setup(m => m.CountryCode).Returns("Consignor Country");
			consignorMock.Setup(m => m.City).Returns("Consignor City");
			consignorMock.Setup(m => m.NameAndAddressLanguage).Returns("");

			var consigneeMock = new Mock<ITrader>();
			consigneeMock.Setup(m => m.TIN).Returns("GB0123456789003");
			consigneeMock.Setup(m => m.Name).Returns("Consignee Name");
			consigneeMock.Setup(m => m.StreetAndNumber).Returns("Consignee Street and Number");
			consigneeMock.Setup(m => m.PostalCode).Returns("Consignee Postcode");
			consigneeMock.Setup(m => m.CountryCode).Returns("Consignee Country");
			consigneeMock.Setup(m => m.City).Returns("Consignee City");
			consigneeMock.Setup(m => m.NameAndAddressLanguage).Returns("");

			declarantMock = CTCMessageBuilderUtilities.SetupTrader("Declarant1", "", "", "", "", "", "GB0123456789004", "", "Representative", "EN-US");
			carrierMock = CTCMessageBuilderUtilities.SetupTrader("Carrier1", "Carrier Addr", "Carrier Post", "Carrier City", "IE", "EN-US", "GB0123456789005", "", "", "");

			var securityConsignorMock = new Mock<ITrader>();
			securityConsignorMock.Setup(m => m.TIN).Returns("GB0123456789006");

			var securityConsigneeMock = new Mock<ITrader>();
			securityConsigneeMock.Setup(m => m.TIN).Returns("GB0123456789007");

			var consignorGoodsItemMock = new Mock<ITrader>();
			var consigneeGoodsItemMock = new Mock<ITrader>();
			var consignorSecurityGoodsItemMock = new Mock<ITrader>();
			var consigneeSecurityGoodsItemMock = new Mock<ITrader>();

			var package1 = CTCMessageBuilderUtilities.SetupPackage("MARKA", "BX", 10, 0, false, false);
			var package2 = CTCMessageBuilderUtilities.SetupPackage("MARKB", "CT", 20, 0, false, false);
			var packages = new IPackage[] { package1, package2 };
			var goodsItem1 = SetupGoodsItem("1", "123456780000", "T1", "DESC1", "A1", "K1", "C1", "U1", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, Enumerable.Empty<ZString>(), packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;
			var goodsItem2 = SetupGoodsItem("2", "123456780000", "T2", "DESC2", "A2", "K2", "C2", "U2", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, new ZString[] { "CONTAINER1", "CONTAINER2" }, packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;

			dataProviderMock = new Mock<ICC015BDeclaration>();
			dataProviderMock.Setup(m => m.IsSimplifiedNctsProcedure).Returns(false);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("NCTS");
			dataProviderMock.Setup(m => m.TypeOfDeclaration).Returns("T1");
			dataProviderMock.Setup(m => m.CountryOfDestinationCode).Returns(Core.Constants.CountryCodes.Italy);
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsCode).Returns("PRE-LODGED");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoods).Returns("LOC");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.PlaceOfLoadingCode).Returns("DOVER007");
			dataProviderMock.Setup(m => m.CountryOfDispatchExportCode).Returns(Core.Constants.CountryCodes.UnitedKingdom);
			dataProviderMock.Setup(m => m.CustomsSubPlace).Returns("DOVER ERTS");
			dataProviderMock.Setup(m => m.InlandTransportMode).Returns("1");
			dataProviderMock.Setup(m => m.TransportModeAtBorder).Returns("3");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDeparture).Returns("NC15 REG1");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDepartureLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportAtDeparture).Returns(Core.Constants.CountryCodes.Ireland);
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorder).Returns("NC15 REG2");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorderLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportCrossingBorder).Returns(Core.Constants.CountryCodes.France);
			dataProviderMock.Setup(m => m.TypeOfMeansOfTransportCrossingBorder).Returns("2");
			dataProviderMock.Setup(m => m.IsContainerised).Returns(true);
			dataProviderMock.Setup(m => m.DialogLanguageIndicatorAtDeparture).Returns("EN-US");
			dataProviderMock.Setup(m => m.AccompanyingDocumentLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.TotalNumberOfPackages).Returns(new ZLong(60));
			dataProviderMock.Setup(m => m.TotalGrossMass).Returns(20.2222m);
			dataProviderMock.Setup(m => m.DeclarationPlace).Returns("Dover");
			dataProviderMock.Setup(m => m.DeclarationPlaceLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.DeclarationDate).Returns("19980809");
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.AuthorisedConsigneeTIN).Returns("GB0123456789012");
			dataProviderMock.Setup(m => m.DepartureCustomsOfficeReferenceNumber).Returns("AA123456");
			dataProviderMock.Setup(m => m.TransitCustomsOffices).Returns(new List<EU.NCTS.Messaging.ICustomsOffice>() { CTCMessageBuilderUtilities.SetupCustomsOffice("NN123456", "201210120606"), CTCMessageBuilderUtilities.SetupCustomsOffice("TT123456", "201210130707") });
			dataProviderMock.Setup(m => m.DestinationCustomsOfficeReferenceNumber).Returns("ZZ123456");
			dataProviderMock.Setup(m => m.ControlResultCode).Returns("A3");
			dataProviderMock.Setup(m => m.ControlResultDateLimit).Returns("19980816");
			dataProviderMock.Setup(m => m.Seals).Returns(new List<ISealID>() { CTCMessageBuilderUtilities.SetupSeal("SEAL1"), CTCMessageBuilderUtilities.SetupSeal("SEAL2"), CTCMessageBuilderUtilities.SetupSeal("SEAL3") });
			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { CTCMessageBuilderUtilities.SetupGuarantee("9", "12346789", "AAAAAAAAAA", "ABCD", 5, new ZString[] { "DE" }), CTCMessageBuilderUtilities.SetupGuarantee("1", "987654321", "BBBBB", "WXYZ", 10, Enumerable.Empty<ZString>()) });
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem1, goodsItem2 });
			dataProviderMock.Setup(m => m.Itinerary).Returns(new List<ZString>() { Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Switzerland });

			dataProviderMock.Setup(m => m.CommercialReferenceNumber).Returns("HQDOV007");
			dataProviderMock.Setup(m => m.ConveyanceReferenceNumber).Returns("MONOPOLI007");
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCode).Returns("UnloadingPlace");
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCodeLanguage).Returns("EN-US");

			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(false);
			dataProviderMock.Setup(m => m.AuthorisedLocationOfGoodsCode).Returns("954131533-GB60DEP");
			dataProviderMock.Setup(m => m.SpecificCircumstanceIndicator).Returns("B");
			dataProviderMock.Setup(m => m.TransportChargesMethodOfPayment).Returns("Y");
			dataProviderMock.Setup(m => m.Principal).Returns(principalMock.Object);
			dataProviderMock.Setup(m => m.Consignor).Returns(consignorMock.Object);
			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			dataProviderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			dataProviderMock.Setup(m => m.NumberOfSeals).Returns(3);
			dataProviderMock.Setup(m => m.Carrier).Returns(carrierMock.Object);
			dataProviderMock.Setup(m => m.SecurityConsignor).Returns(securityConsignorMock.Object);
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns(securityConsigneeMock.Object);

			dataProviderMock.Setup(m => m.IsConsigneeDefinedAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.IsConsignorDefinedAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(false);

			errorCollector = new ErrorCollector();
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);

			var expectedXml = CTCMessageBuilderUtilities.GetEmbeddedResourceFile("CC015BMessageXml.xml");
			AssertEquals(expectedXml, messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.Seals).Returns(new List<ISealID>() { });
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			AssertNotContains("<SEAINFSLI", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.AuthorisedConsigneeTIN).Returns("");
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			AssertNotContains("<TRAAUTCONTRA", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.ControlResultCode).Returns("");
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			dataProviderMock.Setup(m => m.ControlResultDateLimit).Returns("");
			AssertNotContains("<CONRESERS", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { CTCMessageBuilderUtilities.SetupGuarantee("9", "12346789", "AAAAAAAAAA", "ABCD", 5, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("1", "987654321", "BBBBB", "WXYZ", 10, Enumerable.Empty<ZString>()) });
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			AssertNotContains("<VALLIMNONECLIM", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.IsSimplifiedNctsProcedure).Returns(true);
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			var xml = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<AutLocOfGooCodHEA41>954131533-GB60DEP</AutLocOfGooCodHEA41>", xml);
			AssertNotContains("<AgrLocOfGooCodHEA38>", xml);
			AssertNotContains("<AgrLocOfGooHEA39>", xml);
			AssertNotContains("<AgrLocOfGooHEA39LNG>", xml);

			dataProviderMock.Setup(m => m.TypeOfDeclaration).Returns("TIR");
			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(true);

			consignorMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consigneeMock.Setup(m => m.TIN).Returns(ZString.Empty);

			consignorMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");
			consigneeMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			xml = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("Principal has TIR reference", "<HITPC126>TIR001</HITPC126>  </TRAPRIPC1>", xml.Replace("\r\n", "").Replace("\t", ""));
			AssertContains("Principal has name too", " <NamPC17>Principal Name</NamPC17>", xml.Replace("\r\n", "").Replace("\t", ""));
			AssertContains("Language Code Trimmed", "<TRACONCO1LNG>EN</TRACONCO1LNG>", xml);
			AssertContains("Language Code Trimmed", "<NADLNGCE>EN</NADLNGCE>", xml);

			principalMock.Setup(m => m.TIN).Returns(ZString.Empty);
			carrierMock.Setup(m => m.TIN).Returns(ZString.Empty);

			xml = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("Language Code Trimmed", "<NADCARTRA121>EN</NADCARTRA121>", xml);

			principalMock.VerifyAll();
			consignorMock.VerifyAll();
			consigneeMock.VerifyAll();

			securityConsignorMock.VerifyAll();
			securityConsigneeMock.VerifyAll();
			consignorGoodsItemMock.VerifyAll();
			consigneeGoodsItemMock.VerifyAll();
			consignorSecurityGoodsItemMock.VerifyAll();
			consigneeSecurityGoodsItemMock.VerifyAll();
		}

		public void TestCC015BMessageBuilder_NumOfPacGS24_NumOfPieGS25()
		{
			CreateMockSetups_V1();

			var consignorGoodsItemMock = new Mock<ITrader>();
			var consigneeGoodsItemMock = new Mock<ITrader>();
			var consignorSecurityGoodsItemMock = new Mock<ITrader>();
			var consigneeSecurityGoodsItemMock = new Mock<ITrader>();

			var testData = new (ZLong packageNumber, ZLong pieceNumber, bool isPBulk, bool isUnpacked, bool shouldIncludeGS24, bool shouldIncludeGS25)[]
			{
				//IsBulk=False IsUnpacked=False
				(1, 1, false, false, true, false),
				(1, 0, false, false, true, false),
				(0, 0, false, false, true, false),
				(0, 1, false, false, true, false),
				//IsBulk=True IsUnpacked=False
				(1, 1, true, false, false, false),
				(1, 0, true, false, false, false),
				(0, 0, true, false, false, false),
				(0, 1, true, false, false, false),
				//IsBulk=False IsUnpacked=True
				(1, 1, false, true, false, true),
				(1, 0, false, true, false, true),
				(0, 0, false, true, false, true),
				(0, 1, false, true, false, true)
			};

			CombineAssertions(() =>
			{
				foreach (var (packageNumber, pieceNumber, isBulk, isUnpacked, shouldIncludeGS24, shouldIncludeGS25) in testData)
				{
					var package1 = CTCMessageBuilderUtilities.SetupPackage("MARKA", "BX", packageNumber, pieceNumber, isBulk, isUnpacked);
					var packages = new IPackage[] { package1 };
					var goodsItem1 = SetupGoodsItem("1", "123456780000", "T1", "DESC1", "A1", "K1", "C1", "U1", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, Enumerable.Empty<ZString>(), packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;
					dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem1 });
					messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
					var xml = messageBuilder.GetXMLMessageWithoutNamespaces().Replace("\r\n", "").Replace("\t", "");
					if (shouldIncludeGS24)
					{
						AssertContains($"should include NumOfPacGS24:{packageNumber}", $"<NumOfPacGS24>{packageNumber}</NumOfPacGS24>", xml);
					}
					else
					{
						AssertNotContains($"should not include NumOfPacGS24", "<NumOfPacGS24>", xml);
					}

					if (shouldIncludeGS25)
					{
						AssertContains($"should include NumOfPacGS25:{pieceNumber}", $"<NumOfPieGS25>{pieceNumber}</NumOfPieGS25>", xml);
					}
					else
					{
						AssertNotContains("should not include NumOfPacGS25", "<NumOfPieGS25>", xml);
					}
					dataProviderMock.VerifyAll();
				}
				dataProviderMock.VerifyAll();
			});

			dataProviderMock.VerifyAll();
		}

		[TestDate(2011, 12, 13, 14, 15, 16)]
		public void TestCC015BMessageBuilder_GuaranteeSpecialMentions()
		{
			CreateMockSetups_V1();

			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { CTCMessageBuilderUtilities.SetupGuarantee("1", "11111111", "NONZERO", "1111", 10, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("9", "22222222", "NONZERO", "1111", 5, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("9", "33333333", "EMPTY", "1111", 0, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("9", "44444444", "EMPTY", "1111", 0, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("9", "55555555", "EMPTY", "1111", 0, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("9", "66666666", "EMPTY", "1111", 0, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("9", "77777777", "EMPTY", "1111", 0, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("9", "88888888", "EMPTY", "1111", 0, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("9", "99999999", "EMPTY", "1111", 0, new ZString[] { "" }), CTCMessageBuilderUtilities.SetupGuarantee("9", "XXXXXXXX", "10th Guarantee", "1111", 100, new ZString[] { "" }), });
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			AssertContains("Should only output 9 guarantees", expectedGuaranteeNodes, messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertContains("Should only output non-zero Special Mention liabilities", expectedSpecialMentionNodes, messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.VerifyAll();
		}

		public void TestConsignee_ItemLevel()
		{
			CreateMockSetups_V2();
			declarantMock.Reset();
			carrierMock.Reset();

			principalMock = new Mock<ITrader>();
			consignorMock = new Mock<ITrader>();
			consigneeMock = new Mock<ITrader>();

			securityConsignorMock.Setup(m => m.TIN).Returns("GB0123456789006");
			securityConsigneeMock.Setup(m => m.TIN).Returns("GB0123456789007");

			consigneeGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789009");
			consigneeGoodsItemMock.Setup(m => m.Name).Returns("Consignee GI Name");
			consigneeGoodsItemMock.Setup(m => m.StreetAndNumber).Returns("Consignee GI Street and Number");
			consigneeGoodsItemMock.Setup(m => m.PostalCode).Returns("Consignee GI Postcode");
			consigneeGoodsItemMock.Setup(m => m.CountryCode).Returns("Consignee GI Country");
			consigneeGoodsItemMock.Setup(m => m.City).Returns("Consignee GI City");
			consigneeGoodsItemMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			dataProviderMock.Setup(m => m.Consignor).Returns(consignorMock.Object);
			dataProviderMock.Setup(m => m.IsConsigneeDefinedAtGoodsItemLevel).Returns(true);
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);

			AssertContains("Should be Item Level Consignee", @"<TRACONCE2>
      <NamCE27>Consignee GI Name</NamCE27>
      <StrAndNumCE222>Consignee GI Street and Number</StrAndNumCE222>
      <PosCodCE223>Consignee GI Postcode</PosCodCE223>
      <CitCE224>Consignee GI City</CitCE224>
      <CouCE225>Consignee GI Country</CouCE225>
      <NADLNGGICE>EN</NADLNGGICE>
      <TINCE259>GB0123456789009</TINCE259>
    </TRACONCE2>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("Should not be Headeer Level Consignee", " <TRACONCE1>", messageBuilder.GetXMLMessageWithoutNamespaces());

			principalMock.VerifyAll();
			consignorMock.VerifyAll();
			consigneeMock.VerifyAll();
			declarantMock.VerifyAll();
			carrierMock.VerifyAll();
			securityConsignorMock.VerifyAll();
			securityConsigneeMock.VerifyAll();
			consignorGoodsItemMock.VerifyAll();
			consigneeGoodsItemMock.VerifyAll();
			consignorSecurityGoodsItemMock.VerifyAll();
			consigneeSecurityGoodsItemMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestCC015BMessageBuilder_PopulateNADLNGGICE()
		{
			CreateMockSetups_V1();

			dataProviderMock.Setup(m => m.IsConsigneeDefinedAtGoodsItemLevel).Returns(true);
			consigneeGoodsItemMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consigneeGoodsItemMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			var xmlMessage = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("Language Code Trimmed", "<NADLNGGICE>EN</NADLNGGICE>", xmlMessage);

			dataProviderMock.VerifyAll();
			consigneeGoodsItemMock.VerifyAll();
		}

		public void TestConsignor_ItemLevel()
		{
			CreateMockSetups_V2();
			declarantMock.Reset();
			carrierMock.Reset();

			principalMock = new Mock<ITrader>();
			consignorMock = new Mock<ITrader>();
			consigneeMock = new Mock<ITrader>();

			securityConsignorMock.Setup(m => m.TIN).Returns("GB0123456789006");
			securityConsigneeMock.Setup(m => m.TIN).Returns("GB0123456789007");

			consignorGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789008");
			consignorGoodsItemMock.Setup(m => m.Name).Returns("Consignor GI Name");
			consignorGoodsItemMock.Setup(m => m.StreetAndNumber).Returns("Consignor GI Street and Number");
			consignorGoodsItemMock.Setup(m => m.PostalCode).Returns("Consignor GI Postcode");
			consignorGoodsItemMock.Setup(m => m.CountryCode).Returns("Consignor GI Country");
			consignorGoodsItemMock.Setup(m => m.City).Returns("Consignor GI City");
			consignorGoodsItemMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			dataProviderMock.Setup(m => m.IsConsignorDefinedAtGoodsItemLevel).Returns(true);
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);

			AssertContains("Should be Item Level Consignor", @"<TRACONCO2>
      <NamCO27>Consignor GI Name</NamCO27>
      <StrAndNumCO222>Consignor GI Street and Number</StrAndNumCO222>
      <PosCodCO223>Consignor GI Postcode</PosCodCO223>
      <CitCO224>Consignor GI City</CitCO224>
      <CouCO225>Consignor GI Country</CouCO225>
      <NADLNGGTCO>EN</NADLNGGTCO>
      <TINCO259>GB0123456789008</TINCO259>
    </TRACONCO2>
", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("Should not be Header Level Consignor", "<TRACONCO1>", messageBuilder.GetXMLMessageWithoutNamespaces());

			principalMock.VerifyAll();
			consignorMock.VerifyAll();
			consigneeMock.VerifyAll();
			declarantMock.VerifyAll();
			carrierMock.VerifyAll();
			securityConsignorMock.VerifyAll();
			securityConsigneeMock.VerifyAll();
			consignorGoodsItemMock.VerifyAll();
			consigneeGoodsItemMock.VerifyAll();
			consignorSecurityGoodsItemMock.VerifyAll();
			consigneeSecurityGoodsItemMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestCC015BMessageBuilder_PopulateNADLNGGTCO()
		{
			CreateMockSetups_V1();

			dataProviderMock.Setup(m => m.IsConsignorDefinedAtGoodsItemLevel).Returns(true);

			consignorGoodsItemMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consignorGoodsItemMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			var xmlMessage = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("Language Code Trimmed", "<NADLNGGTCO>EN</NADLNGGTCO>", xmlMessage);

			dataProviderMock.VerifyAll();
			consignorGoodsItemMock.VerifyAll();
		}

		public void TestSecurityConsignee_Level()
		{
			CreateMockSetups_V2();
			declarantMock.Reset();
			carrierMock.Reset();

			principalMock = new Mock<ITrader>();
			consignorMock = new Mock<ITrader>();
			consigneeMock = new Mock<ITrader>();

			securityConsignorMock.Setup(m => m.TIN).Returns("GB0123456789006");
			securityConsigneeMock.Setup(m => m.TIN).Returns("GB0123456789007");

			consignorSecurityGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789010");
			consigneeSecurityGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789011");

			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			dataProviderMock.Setup(m => m.Consignor).Returns(consignorMock.Object);
			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);

			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			AssertContains("Should be Header Level Consignee", @"<TRACONSEC029>
    <TINTRACONSEC036>GB0123456789007</TINTRACONSEC036>
  </TRACONSEC029>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("Should not be Item Level Consignee", " <TRACONSECGOO013>", messageBuilder.GetXMLMessageWithoutNamespaces());
			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(true);
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			AssertContains("Should be Item Level Consignee", @"<TRACONSECGOO013>
      <TINTRACONSECGOO020>GB0123456789011</TINTRACONSECGOO020>
    </TRACONSECGOO013>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("Should not be Header Level Consignee", " <TRACONSEC029>", messageBuilder.GetXMLMessageWithoutNamespaces());

			principalMock.VerifyAll();
			consignorMock.VerifyAll();
			consigneeMock.VerifyAll();
			declarantMock.VerifyAll();
			carrierMock.VerifyAll();
			securityConsignorMock.VerifyAll();
			securityConsigneeMock.VerifyAll();
			consignorGoodsItemMock.VerifyAll();
			consigneeGoodsItemMock.VerifyAll();
			consignorSecurityGoodsItemMock.VerifyAll();
			consigneeSecurityGoodsItemMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestCC015BMessageBuilder_PopulateTRACONSEC029LNG()
		{
			CreateMockSetups_V2();
			declarantMock.Reset();
			carrierMock.Reset();

			principalMock = new Mock<ITrader>();
			consignorMock = new Mock<ITrader>();
			consigneeMock = new Mock<ITrader>();
			securityConsignorMock = new Mock<ITrader>();

			securityConsigneeMock.Setup(m => m.TIN).Returns("GB0123456789007");
			securityConsigneeMock.Setup(m => m.Name).Returns("Consignee Security Name(CES)");
			securityConsigneeMock.Setup(m => m.StreetAndNumber).Returns("(CES)1 WiseTech Way");
			securityConsigneeMock.Setup(m => m.PostalCode).Returns("(CES)EH51GG");
			securityConsigneeMock.Setup(m => m.CountryCode).Returns("IE");
			securityConsigneeMock.Setup(m => m.City).Returns("(CES)Galway");
			securityConsigneeMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			dataProviderMock.Setup(m => m.Consignor).Returns(consignorMock.Object);
			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);

			securityConsigneeMock.Setup(m => m.TIN).Returns(ZString.Empty);
			securityConsigneeMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			var xmlMessage = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("Language Code Trimmed", "<TRACONSEC029LNG>EN</TRACONSEC029LNG>", xmlMessage);

			principalMock.VerifyAll();
			consignorMock.VerifyAll();
			consigneeMock.VerifyAll();
			declarantMock.VerifyAll();
			carrierMock.VerifyAll();
			securityConsignorMock.VerifyAll();
			securityConsigneeMock.VerifyAll();
			consignorGoodsItemMock.VerifyAll();
			consigneeGoodsItemMock.VerifyAll();
			consignorSecurityGoodsItemMock.VerifyAll();
			consigneeSecurityGoodsItemMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestCC015BMessageBuilder_PopulateTRACONSECGOO013LNG()
		{
			CreateMockSetups_V1();

			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(true);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);

			consigneeSecurityGoodsItemMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consigneeSecurityGoodsItemMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			var xmlMessage = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("Language Code Trimmed", "<TRACONSECGOO013LNG>EN</TRACONSECGOO013LNG>", xmlMessage);

			consigneeSecurityGoodsItemMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestSecurityConsignor_Level()
		{
			CreateMockSetups_V2();
			declarantMock.Reset();
			carrierMock.Reset();

			principalMock = new Mock<ITrader>();
			consignorMock = new Mock<ITrader>();
			consigneeMock = new Mock<ITrader>();

			securityConsignorMock.Setup(m => m.TIN).Returns("GB0123456789006");
			securityConsigneeMock.Setup(m => m.TIN).Returns("GB0123456789007");

			consignorSecurityGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789010");
			consigneeSecurityGoodsItemMock.Setup(m => m.TIN).Returns("GB0123456789011");

			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			dataProviderMock.Setup(m => m.Consignor).Returns(consignorMock.Object);
			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);

			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			AssertContains("Should be Header Level Consignor", @"<TRACORSEC037>
    <TINTRACORSEC044>GB0123456789006</TINTRACORSEC044>
  </TRACORSEC037>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("Should not be Item Level Consignor", " <TRACORSECGOO021>", messageBuilder.GetXMLMessageWithoutNamespaces());

			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(true);
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);

			AssertContains("Should be Item Level Consignor", @"<TRACORSECGOO021>
      <TINTRACORSECGOO028>GB0123456789010</TINTRACORSECGOO028>
    </TRACORSECGOO021>", messageBuilder.GetXMLMessageWithoutNamespaces());
			AssertNotContains("Should not be Header Level Consignor", " <TRACORSEC037>", messageBuilder.GetXMLMessageWithoutNamespaces());

			principalMock.VerifyAll();
			consignorMock.VerifyAll();
			consigneeMock.VerifyAll();
			declarantMock.VerifyAll();
			carrierMock.VerifyAll();
			securityConsignorMock.VerifyAll();
			securityConsigneeMock.VerifyAll();
			consignorGoodsItemMock.VerifyAll();
			consigneeGoodsItemMock.VerifyAll();
			consignorSecurityGoodsItemMock.VerifyAll();
			consigneeSecurityGoodsItemMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestCC015BMessageBuilder_PopulateTRACORSEC037LNG()
		{
			CreateMockSetups_V2();
			declarantMock.Reset();
			carrierMock.Reset();

			principalMock = new Mock<ITrader>();
			consignorMock = new Mock<ITrader>();
			consigneeMock = new Mock<ITrader>();

			securityConsignorMock.Setup(m => m.TIN).Returns("GB0123456789006");
			securityConsignorMock.Setup(m => m.Name).Returns("Consignor Security Name(CRS)");
			securityConsignorMock.Setup(m => m.StreetAndNumber).Returns("(CRS) 1 WiseTech Way");
			securityConsignorMock.Setup(m => m.PostalCode).Returns("(CRS) EH51GG");
			securityConsignorMock.Setup(m => m.CountryCode).Returns("IE");
			securityConsignorMock.Setup(m => m.City).Returns("(CRS) Galway");
			securityConsignorMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			securityConsigneeMock.Setup(m => m.TIN).Returns("GB0123456789007");

			dataProviderMock.Setup(m => m.Consignee).Returns(consigneeMock.Object);
			dataProviderMock.Setup(m => m.Consignor).Returns(consignorMock.Object);
			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);

			securityConsignorMock.Setup(m => m.TIN).Returns(ZString.Empty);
			securityConsignorMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			var xmlMessage = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("Language Code Trimmed", "<TRACORSEC037LNG>EN</TRACORSEC037LNG>", xmlMessage);

			principalMock.VerifyAll();
			consignorMock.VerifyAll();
			consigneeMock.VerifyAll();
			declarantMock.VerifyAll();
			carrierMock.VerifyAll();
			securityConsignorMock.VerifyAll();
			securityConsigneeMock.VerifyAll();
			consignorGoodsItemMock.VerifyAll();
			consigneeGoodsItemMock.VerifyAll();
			consignorSecurityGoodsItemMock.VerifyAll();
			consigneeSecurityGoodsItemMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestCC015BMessageBuilder_PopulateTRACORSECGOO021LNG()
		{
			CreateMockSetups_V1();

			consignorSecurityGoodsItemMock.Reset();

			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(true);
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			consignorSecurityGoodsItemMock.Setup(m => m.TIN).Returns(ZString.Empty);
			consignorSecurityGoodsItemMock.Setup(m => m.NameAndAddressLanguage).Returns("EN-US");

			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			var xmlMessage = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<TRACORSECGOO021LNG>EN</TRACORSECGOO021LNG>", xmlMessage);

			consignorSecurityGoodsItemMock.VerifyAll();
			dataProviderMock.VerifyAll();
		}

		public void TestCC015BMessageBuilder_NCTSAccDocHEA601LNG()
		{
			CreateMockSetups_V2();
			dataProviderMock.Setup(m => m.AccompanyingDocumentLanguage).Returns("FR-FR");
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
			var xmlMessage = messageBuilder.GetXMLMessageWithoutNamespaces();
			AssertContains("<NCTSAccDocHEA601LNG>EN</NCTSAccDocHEA601LNG>", xmlMessage);
		}

		void CreateMockSetups_V1()
		{
			consignorGoodsItemMock = new Mock<ITrader>();
			consigneeGoodsItemMock = new Mock<ITrader>();
			consignorSecurityGoodsItemMock = new Mock<ITrader>();
			consigneeSecurityGoodsItemMock = new Mock<ITrader>();

			var package1 = CTCMessageBuilderUtilities.SetupPackage("MARKA", "BX", 10, 0, false, false);
			var package2 = CTCMessageBuilderUtilities.SetupPackage("MARKB", "CT", 20, 0, false, false);
			var packages = new IPackage[] { package1, package2 };
			var goodsItem1 = SetupGoodsItem("1", "123456780000", "T1", "DESC1", "A1", "K1", "C1", "U1", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, Enumerable.Empty<ZString>(), packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;
			var goodsItem2 = SetupGoodsItem("2", "123456780000", "T2", "DESC2", "A2", "K2", "C2", "U2", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, new ZString[] { "CONTAINER1", "CONTAINER2" }, packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;

			dataProviderMock = new Mock<ICC015BDeclaration>();
			dataProviderMock.Setup(m => m.IsSimplifiedNctsProcedure).Returns(false);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("NCTS");
			dataProviderMock.Setup(m => m.TypeOfDeclaration).Returns("T1");
			dataProviderMock.Setup(m => m.CountryOfDestinationCode).Returns(Core.Constants.CountryCodes.Italy);
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsCode).Returns("PRE-LODGED");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoods).Returns("LOC");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.PlaceOfLoadingCode).Returns("DOVER007");
			dataProviderMock.Setup(m => m.CountryOfDispatchExportCode).Returns(Core.Constants.CountryCodes.UnitedKingdom);
			dataProviderMock.Setup(m => m.CustomsSubPlace).Returns("DOVER ERTS");
			dataProviderMock.Setup(m => m.InlandTransportMode).Returns("1");
			dataProviderMock.Setup(m => m.TransportModeAtBorder).Returns("3");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDeparture).Returns("NC15 REG1");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDepartureLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportAtDeparture).Returns(Core.Constants.CountryCodes.Ireland);
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorder).Returns("NC15 REG2");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorderLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportCrossingBorder).Returns(Core.Constants.CountryCodes.France);
			dataProviderMock.Setup(m => m.TypeOfMeansOfTransportCrossingBorder).Returns("2");
			dataProviderMock.Setup(m => m.IsContainerised).Returns(true);
			dataProviderMock.Setup(m => m.DialogLanguageIndicatorAtDeparture).Returns("EN-US");
			dataProviderMock.Setup(m => m.TotalNumberOfPackages).Returns(new ZLong(60));
			dataProviderMock.Setup(m => m.TotalGrossMass).Returns(20.2222m);
			dataProviderMock.Setup(m => m.DeclarationPlace).Returns("Dover");
			dataProviderMock.Setup(m => m.DeclarationPlaceLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.DeclarationDate).Returns("19980809");
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.AuthorisedConsigneeTIN).Returns("GB0123456789012");
			dataProviderMock.Setup(m => m.DepartureCustomsOfficeReferenceNumber).Returns("AA123456");
			dataProviderMock.Setup(m => m.TransitCustomsOffices).Returns(new List<EU.NCTS.Messaging.ICustomsOffice>() { CTCMessageBuilderUtilities.SetupCustomsOffice("NN123456", "201210120606"), CTCMessageBuilderUtilities.SetupCustomsOffice("TT123456", "201210130707") });
			dataProviderMock.Setup(m => m.DestinationCustomsOfficeReferenceNumber).Returns("ZZ123456");
			dataProviderMock.Setup(m => m.ControlResultCode).Returns("A3");
			dataProviderMock.Setup(m => m.ControlResultDateLimit).Returns("19980816");
			dataProviderMock.Setup(m => m.Seals).Returns(new List<ISealID>() { CTCMessageBuilderUtilities.SetupSeal("SEAL1"), CTCMessageBuilderUtilities.SetupSeal("SEAL2"), CTCMessageBuilderUtilities.SetupSeal("SEAL3") });
			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { CTCMessageBuilderUtilities.SetupGuarantee("9", "12346789", "AAAAAAAAAA", "ABCD", 5, new ZString[] { "DE" }), CTCMessageBuilderUtilities.SetupGuarantee("1", "987654321", "BBBBB", "WXYZ", 10, Enumerable.Empty<ZString>()) });
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem1, goodsItem2 });
			dataProviderMock.Setup(m => m.Itinerary).Returns(new List<ZString>() { Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Switzerland });

			dataProviderMock.Setup(m => m.CommercialReferenceNumber).Returns("HQDOV007");
			dataProviderMock.Setup(m => m.ConveyanceReferenceNumber).Returns("MONOPOLI007");
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCode).Returns("UnloadingPlace");
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCodeLanguage).Returns("EN-US");

			dataProviderMock.Setup(m => m.IsConsigneeDefinedAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.IsConsignorDefinedAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(false);

			errorCollector = new ErrorCollector();
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
		}

		void CreateMockSetups_V2()
		{
			principalMock = new Mock<ITrader>();
			consignorMock = new Mock<ITrader>();
			consigneeMock = new Mock<ITrader>();

			declarantMock = CTCMessageBuilderUtilities.SetupTrader("Declarant1", "", "", "", "", "", "GB0123456789004", "", "Representative", "EN-US");
			carrierMock = CTCMessageBuilderUtilities.SetupTrader("Carrier1", "Carrier Addr", "Carrier Post", "Carrier City", "IE", "EN-US", "GB0123456789005", "", "", "");

			securityConsignorMock = new Mock<ITrader>();
			securityConsigneeMock = new Mock<ITrader>();
			consignorGoodsItemMock = new Mock<ITrader>();
			consigneeGoodsItemMock = new Mock<ITrader>();
			consignorSecurityGoodsItemMock = new Mock<ITrader>();
			consigneeSecurityGoodsItemMock = new Mock<ITrader>();

			var package1 = CTCMessageBuilderUtilities.SetupPackage("MARKA", "BX", 10, 0, false, false);
			var package2 = CTCMessageBuilderUtilities.SetupPackage("MARKB", "CT", 20, 0, false, false);
			var packages = new IPackage[] { package1, package2 };
			var goodsItem1 = SetupGoodsItem("1", "123456780000", "T1", "DESC1", "A1", "K1", "C1", "U1", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, Enumerable.Empty<ZString>(), packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;
			var goodsItem2 = SetupGoodsItem("2", "123456780000", "T2", "DESC2", "A2", "K2", "C2", "U2", consignorGoodsItemMock.Object, consigneeGoodsItemMock.Object, new ZString[] { "CONTAINER1", "CONTAINER2" }, packages, consignorSecurityGoodsItemMock.Object, consigneeSecurityGoodsItemMock.Object).Object;

			dataProviderMock = new Mock<ICC015BDeclaration>();
			dataProviderMock.Setup(m => m.IsSimplifiedNctsProcedure).Returns(false);
			dataProviderMock.Setup(m => m.LocalReferenceNumber).Returns("NCTS");
			dataProviderMock.Setup(m => m.TypeOfDeclaration).Returns("T1");
			dataProviderMock.Setup(m => m.CountryOfDestinationCode).Returns(Core.Constants.CountryCodes.Italy);
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsCode).Returns("PRE-LODGED");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoods).Returns("LOC");
			dataProviderMock.Setup(m => m.AgreedLocationOfGoodsLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.PlaceOfLoadingCode).Returns("DOVER007");
			dataProviderMock.Setup(m => m.CountryOfDispatchExportCode).Returns(Core.Constants.CountryCodes.UnitedKingdom);
			dataProviderMock.Setup(m => m.CustomsSubPlace).Returns("DOVER ERTS");
			dataProviderMock.Setup(m => m.InlandTransportMode).Returns("1");
			dataProviderMock.Setup(m => m.TransportModeAtBorder).Returns("3");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDeparture).Returns("NC15 REG1");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportAtDepartureLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportAtDeparture).Returns(Core.Constants.CountryCodes.Ireland);
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorder).Returns("NC15 REG2");
			dataProviderMock.Setup(m => m.IdentityOfMeansOfTransportCrossingBorderLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.NationalityOfMeansOfTransportCrossingBorder).Returns(Core.Constants.CountryCodes.France);
			dataProviderMock.Setup(m => m.TypeOfMeansOfTransportCrossingBorder).Returns("2");
			dataProviderMock.Setup(m => m.IsContainerised).Returns(true);
			dataProviderMock.Setup(m => m.DialogLanguageIndicatorAtDeparture).Returns("EN-US");
			dataProviderMock.Setup(m => m.TotalNumberOfPackages).Returns(new ZLong(60));
			dataProviderMock.Setup(m => m.TotalGrossMass).Returns(20.2222m);
			dataProviderMock.Setup(m => m.DeclarationPlace).Returns("Dover");
			dataProviderMock.Setup(m => m.DeclarationPlaceLanguage).Returns("EN-US");
			dataProviderMock.Setup(m => m.DeclarationDate).Returns("19980809");
			dataProviderMock.Setup(m => m.SafetyAndSecurityData).Returns(true);
			dataProviderMock.Setup(m => m.AuthorisedConsigneeTIN).Returns("GB0123456789012");
			dataProviderMock.Setup(m => m.DepartureCustomsOfficeReferenceNumber).Returns("AA123456");
			dataProviderMock.Setup(m => m.TransitCustomsOffices).Returns(new List<EU.NCTS.Messaging.ICustomsOffice>() { CTCMessageBuilderUtilities.SetupCustomsOffice("NN123456", "201210120606"), CTCMessageBuilderUtilities.SetupCustomsOffice("TT123456", "201210130707") });
			dataProviderMock.Setup(m => m.DestinationCustomsOfficeReferenceNumber).Returns("ZZ123456");
			dataProviderMock.Setup(m => m.ControlResultCode).Returns("A3");
			dataProviderMock.Setup(m => m.ControlResultDateLimit).Returns("19980816");
			dataProviderMock.Setup(m => m.Seals).Returns(new List<ISealID>() { CTCMessageBuilderUtilities.SetupSeal("SEAL1"), CTCMessageBuilderUtilities.SetupSeal("SEAL2"), CTCMessageBuilderUtilities.SetupSeal("SEAL3") });
			dataProviderMock.Setup(m => m.Guarantees).Returns(new List<IGuarantee>() { CTCMessageBuilderUtilities.SetupGuarantee("9", "12346789", "AAAAAAAAAA", "ABCD", 5, new ZString[] { "DE" }), CTCMessageBuilderUtilities.SetupGuarantee("1", "987654321", "BBBBB", "WXYZ", 10, Enumerable.Empty<ZString>()) });
			dataProviderMock.Setup(m => m.GoodsItems).Returns(new IDepartureGoodsItem[] { goodsItem1, goodsItem2 });
			dataProviderMock.Setup(m => m.Itinerary).Returns(new List<ZString>() { Core.Constants.CountryCodes.Germany, Core.Constants.CountryCodes.Switzerland });

			dataProviderMock.Setup(m => m.CommercialReferenceNumber).Returns("HQDOV007");
			dataProviderMock.Setup(m => m.ConveyanceReferenceNumber).Returns("MONOPOLI007");
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCode).Returns("UnloadingPlace");
			dataProviderMock.Setup(m => m.PlaceOfUnloadingCodeLanguage).Returns("EN-US");

			dataProviderMock.Setup(m => m.IsTIRDeclaration).Returns(false);
			dataProviderMock.Setup(m => m.SpecificCircumstanceIndicator).Returns("B");
			dataProviderMock.Setup(m => m.TransportChargesMethodOfPayment).Returns("Y");
			dataProviderMock.Setup(m => m.Principal).Returns(principalMock.Object);
			dataProviderMock.Setup(m => m.Declarant).Returns(declarantMock.Object);
			dataProviderMock.Setup(m => m.Carrier).Returns(carrierMock.Object);
			dataProviderMock.Setup(m => m.SecurityConsignor).Returns(securityConsignorMock.Object);
			dataProviderMock.Setup(m => m.SecurityConsignee).Returns(securityConsigneeMock.Object);

			dataProviderMock.Setup(m => m.IsConsigneeDefinedAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.IsConsignorDefinedAtGoodsItemLevel).Returns(false);
			dataProviderMock.Setup(m => m.HasSecurityAtGoodsItemLevel).Returns(false);

			errorCollector = new ErrorCollector();
			messageBuilder = new CC015BXmlMessageBuilder(dataProviderMock.Object, errorCollector);
		}

		Mock<IDepartureGoodsItem> SetupGoodsItem(
									ZString itemNumber,
									ZString commodityCode,
									ZString typeOfDeclaration,
									ZString goodsDescription,
									ZString countryOfDispatchExportCode,
									ZString countryOfDestinationCode,
									ZString commercialReferenceNumber,
									ZString dangerousGoodsCode,
									ITrader consignor,
									ITrader consignee,
									IEnumerable<ZString> containers,
									IEnumerable<IPackage> packages,
									ITrader consignorSecurity,
									ITrader consigneeSecurity)
		{
			var result = new Mock<IDepartureGoodsItem>();
			result.Setup(m => m.ItemNumber).Returns(new ZInt(itemNumber));
			result.Setup(m => m.CommodityCode).Returns(commodityCode);
			result.Setup(m => m.TypeOfDeclaration).Returns(typeOfDeclaration);
			result.Setup(m => m.GoodsDescription).Returns(goodsDescription);
			result.Setup(m => m.GoodsDescriptionLanguage).Returns("EN-US");
			result.Setup(m => m.GrossMass).Returns(10.2222m);
			result.Setup(m => m.NetMass).Returns(1.1111m);
			result.Setup(m => m.CountryOfDispatchExportCode).Returns(countryOfDispatchExportCode);
			result.Setup(m => m.CountryOfDestinationCode).Returns(countryOfDestinationCode);
			result.Setup(m => m.TransportChargesMethodOfPayment).Returns("M");
			result.Setup(m => m.CommercialReferenceNumber).Returns(commercialReferenceNumber);
			result.Setup(m => m.UNDangerousGoodsCode).Returns(dangerousGoodsCode);
			result.Setup(m => m.PreviousAdministrativeReferences).Returns(new IPreviousAdministrativeReference[] { CTCMessageBuilderUtilities.SetupPreviousAdministrativeReference("T1", "PD1"), CTCMessageBuilderUtilities.SetupPreviousAdministrativeReference("T2", "PD2") });
			result.Setup(m => m.ProducedDocumentsCertificates).Returns(new IProducedDocumentCertificate[] { CTCMessageBuilderUtilities.SetupDocumentCertificate("380", "SD1", "REF1"), CTCMessageBuilderUtilities.SetupDocumentCertificate("18", "SD2", "REF2") });
			result.Setup(m => m.SpecialMentions).Returns(new IStatement[] { CTCMessageBuilderUtilities.SetupSpecialMention("AI1", "00100", "1", Core.Constants.CountryCodes.Andorra), CTCMessageBuilderUtilities.SetupSpecialMention("AI2", "00200", "0", Core.Constants.CountryCodes.Iceland) });
			result.Setup(m => m.Consignor).Returns(consignor);
			result.Setup(m => m.Consignee).Returns(consignee);
			result.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)containers);
			result.Setup(m => m.Packages).Returns((IReadOnlyCollection<IPackage>)packages);
			result.Setup(m => m.ConsignorSecurity).Returns(consignorSecurity);
			result.Setup(m => m.ConsigneeSecurity).Returns(consigneeSecurity);
			return result;
		}

		Mock<ITrader> principalMock;
		Mock<ITrader> consignorMock;
		Mock<ITrader> consigneeMock;
		Mock<ITrader> declarantMock;
		Mock<ITrader> carrierMock;
		Mock<ITrader> securityConsignorMock;
		Mock<ITrader> securityConsigneeMock;
		Mock<ITrader> consigneeSecurityGoodsItemMock;
		Mock<ITrader> consignorSecurityGoodsItemMock;
		Mock<ITrader> consigneeGoodsItemMock;
		Mock<ITrader> consignorGoodsItemMock;
		Mock<ICC015BDeclaration> dataProviderMock;
		ErrorCollector errorCollector;
		CC015BXmlMessageBuilder messageBuilder;

		readonly string expectedGuaranteeNodes = @"
  <GUAGUA>
    <GuaTypGUA1>1</GuaTypGUA1>
    <GUAREFREF>
      <GuaRefNumGRNREF1>11111111</GuaRefNumGRNREF1>
      <OthGuaRefREF4>NONZERO</OthGuaRefREF4>
      <AccCodREF6>1111</AccCodREF6>
      <VALLIMECVLE>
        <NotValForECVLE1>0</NotValForECVLE1>
      </VALLIMECVLE>
    </GUAREFREF>
  </GUAGUA>
  <GUAGUA>
    <GuaTypGUA1>9</GuaTypGUA1>
    <GUAREFREF>
      <GuaRefNumGRNREF1>22222222</GuaRefNumGRNREF1>
      <OthGuaRefREF4>NONZERO</OthGuaRefREF4>
      <AccCodREF6>1111</AccCodREF6>
      <VALLIMECVLE>
        <NotValForECVLE1>0</NotValForECVLE1>
      </VALLIMECVLE>
    </GUAREFREF>
  </GUAGUA>
  <GUAGUA>
    <GuaTypGUA1>9</GuaTypGUA1>
    <GUAREFREF>
      <GuaRefNumGRNREF1>33333333</GuaRefNumGRNREF1>
      <OthGuaRefREF4>EMPTY</OthGuaRefREF4>
      <AccCodREF6>1111</AccCodREF6>
      <VALLIMECVLE>
        <NotValForECVLE1>0</NotValForECVLE1>
      </VALLIMECVLE>
    </GUAREFREF>
  </GUAGUA>
  <GUAGUA>
    <GuaTypGUA1>9</GuaTypGUA1>
    <GUAREFREF>
      <GuaRefNumGRNREF1>44444444</GuaRefNumGRNREF1>
      <OthGuaRefREF4>EMPTY</OthGuaRefREF4>
      <AccCodREF6>1111</AccCodREF6>
      <VALLIMECVLE>
        <NotValForECVLE1>0</NotValForECVLE1>
      </VALLIMECVLE>
    </GUAREFREF>
  </GUAGUA>
  <GUAGUA>
    <GuaTypGUA1>9</GuaTypGUA1>
    <GUAREFREF>
      <GuaRefNumGRNREF1>55555555</GuaRefNumGRNREF1>
      <OthGuaRefREF4>EMPTY</OthGuaRefREF4>
      <AccCodREF6>1111</AccCodREF6>
      <VALLIMECVLE>
        <NotValForECVLE1>0</NotValForECVLE1>
      </VALLIMECVLE>
    </GUAREFREF>
  </GUAGUA>
  <GUAGUA>
    <GuaTypGUA1>9</GuaTypGUA1>
    <GUAREFREF>
      <GuaRefNumGRNREF1>66666666</GuaRefNumGRNREF1>
      <OthGuaRefREF4>EMPTY</OthGuaRefREF4>
      <AccCodREF6>1111</AccCodREF6>
      <VALLIMECVLE>
        <NotValForECVLE1>0</NotValForECVLE1>
      </VALLIMECVLE>
    </GUAREFREF>
  </GUAGUA>
  <GUAGUA>
    <GuaTypGUA1>9</GuaTypGUA1>
    <GUAREFREF>
      <GuaRefNumGRNREF1>77777777</GuaRefNumGRNREF1>
      <OthGuaRefREF4>EMPTY</OthGuaRefREF4>
      <AccCodREF6>1111</AccCodREF6>
      <VALLIMECVLE>
        <NotValForECVLE1>0</NotValForECVLE1>
      </VALLIMECVLE>
    </GUAREFREF>
  </GUAGUA>
  <GUAGUA>
    <GuaTypGUA1>9</GuaTypGUA1>
    <GUAREFREF>
      <GuaRefNumGRNREF1>88888888</GuaRefNumGRNREF1>
      <OthGuaRefREF4>EMPTY</OthGuaRefREF4>
      <AccCodREF6>1111</AccCodREF6>
      <VALLIMECVLE>
        <NotValForECVLE1>0</NotValForECVLE1>
      </VALLIMECVLE>
    </GUAREFREF>
  </GUAGUA>
  <GUAGUA>
    <GuaTypGUA1>9</GuaTypGUA1>
    <GUAREFREF>
      <GuaRefNumGRNREF1>99999999</GuaRefNumGRNREF1>
      <OthGuaRefREF4>EMPTY</OthGuaRefREF4>
      <AccCodREF6>1111</AccCodREF6>
      <VALLIMECVLE>
        <NotValForECVLE1>0</NotValForECVLE1>
      </VALLIMECVLE>
    </GUAREFREF>
  </GUAGUA>";

		readonly string expectedSpecialMentionNodes = @"
    <SPEMENMT2>
      <AddInfMT21>10GBP11111111</AddInfMT21>
      <AddInfCodMT23>CAL</AddInfCodMT23>
    </SPEMENMT2>
    <SPEMENMT2>
      <AddInfMT21>5GBP22222222</AddInfMT21>
      <AddInfCodMT23>CAL</AddInfCodMT23>
    </SPEMENMT2>
    <SPEMENMT2>
      <AddInfMT21>AI1</AddInfMT21>
      <AddInfCodMT23>00100</AddInfCodMT23>
      <ExpFroECMT24>1</ExpFroECMT24>
      <ExpFroCouMT25>AD</ExpFroCouMT25>
    </SPEMENMT2>
    <SPEMENMT2>
      <AddInfMT21>AI2</AddInfMT21>
      <AddInfCodMT23>00200</AddInfCodMT23>
      <ExpFroECMT24>0</ExpFroECMT24>
      <ExpFroCouMT25>IS</ExpFroCouMT25>
    </SPEMENMT2>";
	}
}
