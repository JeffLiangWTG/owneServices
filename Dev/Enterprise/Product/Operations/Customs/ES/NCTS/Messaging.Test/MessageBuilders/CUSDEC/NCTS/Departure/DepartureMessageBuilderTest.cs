using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using Enterprise.Edifact.V921ES.Messages.CUSDEC;
using Enterprise.Messaging.Business;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(DepartureMessageBuilder))]
	class DepartureMessageBuilderTest : NCTSEDIFACTMessageBuilderTest<DepartureMessageBuilder, IDepartureMessageDataProvider, CUSDECMessage>
	{
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

		[TestDate(2020, 3, 9, 16, 13, 23, 456)]
		public override void TestUNBNotTest()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns("1210244B");
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("UNB+UNOA:1+1210244B:ZZ+AEATADUE:ZZ+200309:1613+<<MSGNO PLACEHOLDER>>++&EE'", messageText);
		}

		public void TestSecurityDeclarationFalse()
		{
			mockProvider.Setup(m => m.SecurityDeclaration).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();

			CombineAssertions(() =>
			{
				AssertNotContains("LOC+58", messageText);
				AssertNotContains("EQD+CH", messageText);
				AssertNotContains("FTX+PAI", messageText);
				AssertNotContains("FTX+ACR", messageText);
				AssertNotContains("RFF+AJK", messageText);
				AssertNotContains("NAD+GA", messageText);
				AssertNotContains("NAD+GL", messageText);
				AssertNotContains("NAD+UC", messageText);
			});
		}

		public void TestSecurityDeclarationTrue()
		{
			mockProvider.Setup(m => m.SecurityDeclaration).Returns(true);
			var messageText = CreateMessageBuilder().GetSignedMessageText();

			CombineAssertions(() =>
			{
				AssertContains("LOC+58", messageText);
				AssertContains("EQD+CH", messageText);
				AssertContains("FTX+PAI", messageText);
				AssertContains("FTX+ACR", messageText);
				AssertContains("RFF+AJK", messageText);
				AssertContains("NAD+GA", messageText);
				AssertContains("NAD+GL", messageText);
				AssertContains("NAD+UC", messageText);
			});
		}

		public void TestAddNewSG1Group_NoGuaranteeType()
		{
			var mockGuaranteeNumbers = SetUpGuaranteeNumbers(ZString.Empty, "6634");
			mockProvider.Setup(m => m.GuaranteeNumbers).Returns(new[] { mockGuaranteeNumbers.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("RFF+ABK", messageText);
		}

		public void TestAddNewRFFInSG1Group_NoReferenceNumber()
		{
			mockProvider.Setup(m => m.ReferenceNumber).Returns(ZString.Empty);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("RFF+ABJ", messageText);
		}

		public void TestPopulateSG4Groups_NoBorderTransportMode()
		{
			mockProvider.Setup(m => m.BorderTransportMode).Returns((ITransportMediumInfoCommon)null);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("TDT+11++3:CAMION'TPL+:::M-1234-GD:ES'", messageText);
		}

		public void TestPopulateSG4Groups_NoBorderTransportID()
		{
			var mockBorderTransportMode = BuilderHelperTest.SetUpTransportMediumInfo("3", ZString.Empty, "ES");
			mockProvider.Setup(m => m.BorderTransportMode).Returns(mockBorderTransportMode.Object);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("TDT+11++3'", messageText);
		}

		public void TestPopulateSG4Groups_NoTransitTransportMedium()
		{
			mockProvider.Setup(m => m.TransitTransportMedium).Returns((ITransportMediumInfoCommon)null);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("TDT+30++:AVION'TPL+:::M-1234-GD:ES'", messageText);
		}

		public void TestPopulateSG4Groups_NoTransitTransportID()
		{
			var mockTransitTransportMedium = BuilderHelperTest.SetUpTransportMediumInfo(ZString.Empty, ZString.Empty, "ES");
			mockProvider.Setup(m => m.TransitTransportMedium).Returns(mockTransitTransportMedium.Object);

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("TDT+30'", messageText);
		}

		public void TestAddNewSG30Group_NoDocumentReferenceNumber()
		{
			mockLine1.Setup(m => m.DocumentReferenceNumber).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("RFF+AFB", messageText);
		}

		public void TestAddNewSG31Group_NoVehiclePackages()
		{
			mockLine1.Setup(m => m.VehiclePackages).Returns((IVehiclePackagesInfoCommon)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PAC++4'PCI++VS8ZAZB7861ZB6913:RENAULT:LAGUNA 2007'", messageText);
		}

		public void TestAddNewSG31Group_NoPackagesInInternalPackages()
		{
			var mockPackages = new Mock<IDepartureInternalPackagesInfo>();
			mockPackages.Setup(m => m.Packages).Returns((IReadOnlyCollection<IInternalPackageIdentificationCommon>)Enumerable.Empty<IInternalPackageIdentificationCommon>());
			mockPackages.Setup(m => m.IsVehiclePackage).Returns(false);
			mockLine1.Setup(m => m.InternalPackages).Returns(mockPackages.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PAC+50+1+BX'PCI++MARCABULTINT'", messageText);
		}

		public void TestAddNewSG31Group_OneInternalPackage()
		{
			var mockPackages = new Mock<IDepartureInternalPackagesInfo>();
			var internalPackage = BuilderHelperTest.SetUpInternalPackages();
			mockPackages.Setup(m => m.Packages).Returns(new[] { internalPackage });
			mockPackages.Setup(m => m.IsVehiclePackage).Returns(false);
			mockLine1.Setup(m => m.InternalPackages).Returns(mockPackages.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("PAC+50+1+BX'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T'", messageText);
		}

		public void TestAddNewSG31Group_TwoInternalPackages()
		{
			var mockPackages = new Mock<IDepartureInternalPackagesInfo>();
			var internalPackage = BuilderHelperTest.SetUpInternalPackages();
			mockPackages.Setup(m => m.Packages).Returns(new[] { internalPackage, internalPackage });
			mockPackages.Setup(m => m.IsVehiclePackage).Returns(false);
			mockLine1.Setup(m => m.InternalPackages).Returns(mockPackages.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("PAC++1'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T:50+BX'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T:50+BX'", messageText);
		}

		public void TestAddNewSG31Group_OneInternalVehiclePackage()
		{
			var mockPackages = new Mock<IDepartureInternalPackagesInfo>();
			var internalPackage = BuilderHelperTest.SetUpInternalPackages();
			mockPackages.Setup(m => m.Packages).Returns(new[] { internalPackage });
			mockPackages.Setup(m => m.IsVehiclePackage).Returns(true);
			mockLine1.Setup(m => m.InternalPackages).Returns(mockPackages.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("PAC++1'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T:50+BX'", messageText);
		}

		public void TestAddNewSG7Group_NoMethodOfPayment()
		{
			mockLine1.Setup(m => m.GoodsTransportMethodOfPayment).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("TOD+2", messageText);
		}

		public void TestAddNewSG7Group_NoCountry()
		{
			mockLine1.Setup(m => m.GoodsTransportMethodOfPayment).Returns("DG0");
			mockLine1.Setup(m => m.GoodsCountryCode).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("TOD+2+DG0+1'", messageText);
		}

		public void TestAddNewSG7Group_YesCountry()
		{
			mockLine1.Setup(m => m.GoodsTransportMethodOfPayment).Returns("DG0");
			mockLine1.Setup(m => m.GoodsCountryCode).Returns("ES");
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("TOD+2+DG0+:ES'", messageText);
		}

		protected override ZString ExpectedMessageType => "DEP";
		protected override ZString DeclarantIdForUNBSegment => "ES12345678E";

		protected override ZString GetTestFile() => GetTestFileContents(NCTSTestFileConstants.TestFilePath, "TestDepartureMessage.txt");

		protected override DepartureMessageBuilder CreateMessageBuilder() => new DepartureMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);
		protected override DepartureMessageBuilder CreateMessageBuilderWithNullProvider() => new DepartureMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected Mock<IDepartureMessageDataProvider> mockProvider;

		protected void SetUpMockProvider()
		{
			mockProvider = new Mock<IDepartureMessageDataProvider>();

			mockProvider.Setup(m => m.Factory).Returns(Factory);
			mockProvider.Setup(m => m.IsTest).Returns(ZBool.True);
			mockProvider.Setup(m => m.Messages).Returns(new EDIMessageCollection(Factory.New(typeof(DummyBusinessObject)), Factory));
			mockProvider.Setup(m => m.BrokerCode).Returns("AZ");
			mockProvider.Setup(m => m.CertificateName).Returns("CertName");
			mockProvider.Setup(m => m.CertificateThumbPrint).Returns("CertThumbPrint");
			mockProvider.Setup(m => m.CertificateBytes).Returns(BuilderHelperTest.GetCertificateBytes());
			mockProvider.Setup(m => m.DecryptedCertificatePassphrase).Returns(BuilderHelperTest.CertificatePassword);
			mockProvider.Setup(m => m.BusinessObjectReference).Returns("Reference");

			var certificate = Factory.New<MasterFiles.Business.GlbExternalPassword>();
			mockProvider.Setup(m => m.CertificatePK).Returns(certificate.PK);
		}

		protected override void SetUp()
		{
			SetUpMockProvider();
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns(DeclarantIdForUNBSegment);

			mockProvider.Setup(m => m.LocalReferenceNumber).Returns("123412341234");
			mockProvider.Setup(m => m.CustomsProcedureCategory3).Returns("T1");
			mockProvider.Setup(m => m.CustomsProcedureCategory5).Returns("2802");
			mockProvider.Setup(m => m.CountryOfDeparture).Returns("ES");
			mockProvider.Setup(m => m.CountryOfDestination).Returns("FR");
			mockProvider.Setup(m => m.LocationOfGoodsExamCustomsOffice).Returns("4611");
			mockProvider.Setup(m => m.LocationOfGoodsExam).Returns("VLC001");
			mockProvider.Setup(m => m.CodeOfLoadingLocation).Returns("ESMAD");
			mockProvider.Setup(m => m.CodeOfUnloadingLocation).Returns("FRPAR");
			mockProvider.Setup(m => m.GoodsInContainerIndicator).Returns(true);
			mockProvider.Setup(m => m.SecurityDeclaration).Returns(true);
			mockProvider.Setup(m => m.CountryCodes).Returns(new ZString[] { "ES", "FR" });
			mockProvider.Setup(m => m.SealCodes).Returns(new ZString[] { "SEAL1", "SEAL2" });
			mockProvider.Setup(m => m.TransportMethodOfPayment).Returns("A");
			mockProvider.Setup(m => m.ConveyanceReferenceNumber).Returns("REFECONVE886612");
			mockProvider.Setup(m => m.ReferenceNumber).Returns("MIREFER4455");
			mockProvider.Setup(m => m.SpecificCircumstancesIndicator).Returns("B");
			mockProvider.Setup(m => m.TotalNumberOfGoods).Returns(15);
			mockProvider.Setup(m => m.TotalNumberOfPackageElements).Returns(1586L);
			mockProvider.Setup(m => m.NationalSimplificationIndicator).Returns("1");

			var mockCustomsOfficesOfTransit1 = SetUpCustomsOfficeOfTransit("ES", "004611");
			var mockCustomsOfficesOfTransit2 = SetUpCustomsOfficeOfTransit("FR", "005822");
			mockProvider.Setup(m => m.CustomsOfficesOfTransit).Returns(new[] { mockCustomsOfficesOfTransit1, mockCustomsOfficesOfTransit2 });

			var mockCustomsOfficesOfTransit3 = SetUpCustomsOfficeOfTransit("FR", "001141");
			mockProvider.Setup(m => m.CustomsOfficeOfDestination).Returns(mockCustomsOfficesOfTransit3);

			var mockGuaranteeNumbers1 = SetUpGuaranteeNumbers("104ES0002800000205", ZString.Empty);
			var mockGuaranteeNumbers2 = SetUpGuaranteeNumbers("204FR0000000000459", "6634");
			mockProvider.Setup(m => m.GuaranteeNumbers).Returns(new[] { mockGuaranteeNumbers1.Object, mockGuaranteeNumbers2.Object });

			var mockBorderTransportMode = BuilderHelperTest.SetUpTransportMediumInfo("3", "M-1234-GD", "ES");
			mockProvider.Setup(m => m.BorderTransportMode).Returns(mockBorderTransportMode.Object);

			var mockTransitTransportMedium = BuilderHelperTest.SetUpTransportMediumInfo(ZString.Empty, "M-1234-GD", "ES");
			mockProvider.Setup(m => m.TransitTransportMedium).Returns(mockTransitTransportMedium.Object);

			var mockConsignor = BuilderHelperTest.SetUpParty("A02020202", "TRANSITARIA ESPAÑOLA S.A.", "FUENTECILLA 6", "ALBACETE", "02100", "ES");
			mockProvider.Setup(m => m.Consignor).Returns(mockConsignor);

			var mockConsignee = BuilderHelperTest.SetUpParty(ZString.Empty, "NORON EHF", "SKUTUVOGUR 7", "REYKJAVIK", "40025", "FR");
			mockProvider.Setup(m => m.Consignee).Returns(mockConsignee);

			var mockDeclarant = BuilderHelperTest.SetUpPartyEmail("ES12345678E", "JUAN DECLARANTE", "MIDIRECCION.CORREO.EN.CASTILLAYLEON@MIXMAIL.COM");
			mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

			var mockPrincipal = BuilderHelperTest.SetUpParty("ESA98765432", "OBLIGADOS PRINCIPALES SA", "MORATIN 28", "MADRID", "28018", "ES");
			mockProvider.Setup(m => m.Principal).Returns(mockPrincipal);

			var mockRepresentative = BuilderHelperTest.SetUpParty("A56565656", "REPRESENTANTES FISCALES SA", "GOYA 81", "MADRID", "28030", "ES");
			mockProvider.Setup(m => m.Representative).Returns(mockRepresentative);

			var mockSecurityCarrier = BuilderHelperTest.SetUpParty("A34765432", "TRANSPORTISTAS EJEMPLO", "MORATIN 66", "MADRID", "28018", "ES");
			mockProvider.Setup(m => m.SecurityCarrier).Returns(mockSecurityCarrier);

			var mockSecurityConsignor = BuilderHelperTest.SetUpParty("A235565432", "SEGURO SA", "OCA 66", "MADRID", "28051", "ES");
			mockProvider.Setup(m => m.SecurityConsignor).Returns(mockSecurityConsignor);

			var mockSecurityConsignee = BuilderHelperTest.SetUpParty("PL235522432", "DESTINATARIO SEGURO", "RAMISK 33", "VARSOVIA", "99051", "PL");
			mockProvider.Setup(m => m.SecurityConsignee).Returns(mockSecurityConsignee);

			var mockPackages = BuilderHelperTest.SetUpInternalPackages();

			mockLine1 = SetUpLine(1, 8527.45M, ZString.Empty, new[] { mockPackages });
			var mockDocument1 = SetUpDocument("X001", "ES3600000001", ZString.Empty);
			var mockDocument2 = SetUpDocument("X001", "ES3600000002", "KN00000000000100");
			mockLine1.Setup(m => m.Documents).Returns(new[] { mockDocument1.Object, mockDocument2.Object });

			var mockLine2 = SetUpLine(2, 100M, "AU", new[] { mockPackages, mockPackages });
			var mockExternalPackages = BuilderHelperTest.SetUpExternalPackages(50, new ZString[] { "TCKU2126154", "TCKU2126133", "AA", "BB", "CC", "DD", "EE", "FF", "GG", "HH" });
			mockLine2.Setup(m => m.ExternalPackages).Returns(mockExternalPackages);
			var mockVehiclePackages = SetUpVehiclePackages();
			mockLine2.Setup(m => m.VehiclePackages).Returns(mockVehiclePackages.Object);
			mockLine2.Setup(m => m.Documents).Returns((IReadOnlyCollection<IDepartureDocuments>)Enumerable.Empty<IDepartureDocuments>());

			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}

		Mock<IDepartureLine> mockLine1;

		Mock<IGuaranteeNumber> SetUpGuaranteeNumbers(ZString type, ZString code)
		{
			var mockGuaranteeNumbers = new Mock<IGuaranteeNumber>();
			mockGuaranteeNumbers.Setup(m => m.Type).Returns(type);
			mockGuaranteeNumbers.Setup(m => m.AccessCode).Returns(code);
			return mockGuaranteeNumbers;
		}

		Mock<IDepartureLine> SetUpLine(ZInt goodsItemNumber, ZDecimal totalGoodValueInEuros, ZString countryCode, IInternalPackageIdentificationCommon[] packages)
		{
			var mockLine = new Mock<IDepartureLine>();
			mockLine.Setup(m => m.GoodsItemNumber).Returns(goodsItemNumber);
			mockLine.Setup(m => m.CountryOfDeparture).Returns("ES");
			mockLine.Setup(m => m.CountryOfDestination).Returns("FR");
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory1).Returns("12079999");
			mockLine.Setup(m => m.GoodsCustomsProcedureCategory5).Returns("T1");
			mockLine.Setup(m => m.GoodsDescription).Returns("CALZADO DE PIEL DE COCODRILO PARA SEÑORAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDEEEEEEEEEE");
			mockLine.Setup(m => m.GoodsCountryOfOrigin).Returns("ES");
			mockLine.Setup(m => m.GoodsCountryOfDestination).Returns("FR");
			mockLine.Setup(m => m.GrossWeightInKG).Returns(100.01);
			mockLine.Setup(m => m.NetWeightInKG).Returns(123);
			mockLine.Setup(m => m.FiscalUnitsNumber).Returns(12.3);
			mockLine.Setup(m => m.FiscalUnitsQualifier).Returns("HP");
			mockLine.Setup(m => m.OtherUnitsNumber).Returns(2500);
			mockLine.Setup(m => m.OtherUnitsQualifier).Returns("UN");
			mockLine.Setup(m => m.DangerousGoodsCode).Returns("4134");
			mockLine.Setup(m => m.TotalGoodValueInEuros).Returns(totalGoodValueInEuros);
			mockLine.Setup(m => m.DocumentTypeCode).Returns("AFB");
			mockLine.Setup(m => m.DocumentReferenceNumber).Returns("DUA08ES00280112345679001");
			mockLine.Setup(m => m.DocumentLineNo).Returns("1");
			mockLine.Setup(m => m.DocumentClass).Returns("Z");
			mockLine.Setup(m => m.GoodsTransportMethodOfPayment).Returns("DG0");
			mockLine.Setup(m => m.GoodsCountryCode).Returns(countryCode);

			var mockConsignor = BuilderHelperTest.SetUpParty("A02020202", "TRANSITARIA ESPAÑOLA S.A.", "FUENTECILLA 6", "ALBACETE", "02100", "ES");
			mockLine.Setup(m => m.GoodsConsignor).Returns(mockConsignor);

			var mockConsignee = BuilderHelperTest.SetUpParty(ZString.Empty, "NORON EHF", "SKUTUVOGUR 7", "REYKJAVIK", "40025", "FR");
			mockLine.Setup(m => m.GoodsConsignee).Returns(mockConsignee);

			var mockSecurityConsignor = BuilderHelperTest.SetUpParty("A235565432", "SEGURO SA", "OCA 66", "MADRID", "28051", "ES");
			mockLine.Setup(m => m.SecurityGoodsConsignor).Returns(mockSecurityConsignor);

			var mockSecurityConsignee = BuilderHelperTest.SetUpParty("PL235522432", "DESTINATARIO SEGURO", "RAMISK 33", "VARSOVIA", "99051", "PL");
			mockLine.Setup(m => m.SecurityGoodsConsignee).Returns(mockSecurityConsignee);

			var mockInternalPackages = new Mock<IDepartureInternalPackagesInfo>();
			mockInternalPackages.Setup(m => m.Packages).Returns(packages);
			mockInternalPackages.Setup(m => m.IsVehiclePackage).Returns(false);
			mockLine.Setup(m => m.InternalPackages).Returns(mockInternalPackages.Object);

			return mockLine;
		}

		Mock<IVehiclePackagesInfoCommon> SetUpVehiclePackages()
		{
			var mockVehiclePackages = new Mock<IVehiclePackagesInfoCommon>();

			var mockVehicle = BuilderHelperTest.SetUpVehicle("VS8ZAZB7861ZB6913", "RENAULT", "LAGUNA 2007");

			mockVehiclePackages.Setup(m => m.Packages).Returns(new[] { mockVehicle, mockVehicle });
			return mockVehiclePackages;
		}

		Mock<IDepartureDocuments> SetUpDocument(ZString name, ZString number, ZString source)
		{
			var mockDocument = new Mock<IDepartureDocuments>();
			mockDocument.Setup(m => m.Name).Returns(name);
			mockDocument.Setup(m => m.Number).Returns(number);
			mockDocument.Setup(m => m.Source).Returns(source);
			return mockDocument;
		}

		INctsCustomsTransitOfficeProvider SetUpCustomsOfficeOfTransit(ZString state, ZString code)
		{
			var mockCustomsOfficesOfTransit = new Mock<INctsCustomsTransitOfficeProvider>();
			mockCustomsOfficesOfTransit.Setup(m => m.CustomsTransitOfficeState).Returns(state);
			mockCustomsOfficesOfTransit.Setup(m => m.CustomsTransitOfficeCode).Returns(code);
			return mockCustomsOfficesOfTransit.Object;
		}
	}
}
