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
	[TestedType(typeof(TIRMessageBuilder))]
	class TIRMessageBuilderTest : NCTSEDIFACTMessageBuilderTest<TIRMessageBuilder, ITIRMessageDataProvider, CUSDECMessage>
	{
		protected override ZString ExpectedMessageType => Common.CusEntryNumberTypes.EU.TIRCarnetNumber;
		protected override ZString DeclarantIdForUNBSegment => "1210244B";

		protected override ZString GetTestFile() => GetTestFileContents(NCTSTestFileConstants.TestFilePath, "TestTIRMessage.txt");

		protected override TIRMessageBuilder CreateMessageBuilder() => new TIRMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType);
		protected override TIRMessageBuilder CreateMessageBuilderWithNullProvider() => new TIRMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

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

		public void TestAddNewSG30Group_NoDocumentReferenceNumber()
		{
			mockLine1.Setup(m => m.DocumentReferenceNumber).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("RFF+AFB", messageText);
		}

		public void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory1()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory1).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("CST+1+120799:122:148'", messageText);
		}

		public void TestAddNewSG31Group_NoPackagesInInternalPackages()
		{
			var mockPackages = new Mock<ITIRInternalPackagesInfo>();
			mockPackages.Setup(m => m.Packages).Returns((IReadOnlyCollection<IInternalPackageIdentificationCommon>)Enumerable.Empty<IInternalPackageIdentificationCommon>());
			mockPackages.Setup(m => m.IsVehiclePackage).Returns(false);
			mockLine1.Setup(m => m.InternalPackages).Returns(mockPackages.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PAC+50+1+BX'PCI++MARCABULTINT'", messageText);
		}

		public void TestAddNewSG31Group_OneInternalPackage()
		{
			var mockPackages = new Mock<ITIRInternalPackagesInfo>();
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
			var mockPackages = new Mock<ITIRInternalPackagesInfo>();
			var internalPackage = BuilderHelperTest.SetUpInternalPackages();
			mockPackages.Setup(m => m.Packages).Returns(new[] { internalPackage, internalPackage });
			mockPackages.Setup(m => m.IsVehiclePackage).Returns(false);
			mockLine1.Setup(m => m.InternalPackages).Returns(mockPackages.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("PAC++1'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T:50+BX'PCI++MARCABULTIMARCABULTIMARCABULTIAASDN:T:50+BX'", messageText);
		}

		public void TestAddNewSG31Group_SetInternalPackagesWhenVehicles_OneInternalVehiclePackage()
		{
			var mockPackages = new Mock<ITIRInternalPackagesInfo>();
			var internalPackage = SetUpVehicleInternalPackage("vincode1", 1);
			mockPackages.Setup(m => m.Packages).Returns(new[] { internalPackage });
			mockPackages.Setup(m => m.IsVehiclePackage).Returns(true);
			mockLine1.Setup(m => m.InternalPackages).Returns(mockPackages.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("PAC+1+1+FR'PCI++vincode1'", messageText);
		}

		public void TestAddNewSG31Group_SetInternalPackagesWhenVehicles_SixInternalVehiclePackage()
		{
			var mockPackages = new Mock<ITIRInternalPackagesInfo>();
			var internalPackage1 = SetUpVehicleInternalPackage("vincode1", 6);
			var internalPackage2 = SetUpVehicleInternalPackage("vincode2", 6);
			var internalPackage3 = SetUpVehicleInternalPackage("vincode3", 6);
			var internalPackage4 = SetUpVehicleInternalPackage("vincode4", 6);
			var internalPackage5 = SetUpVehicleInternalPackage("vincode5", 6);
			var internalPackage6 = SetUpVehicleInternalPackage("vincode6", 6);
			mockPackages.Setup(m => m.Packages).Returns(new[] { internalPackage1, internalPackage2, internalPackage3, internalPackage4, internalPackage5, internalPackage6 });
			mockPackages.Setup(m => m.IsVehiclePackage).Returns(true);
			mockLine1.Setup(m => m.InternalPackages).Returns(mockPackages.Object);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("PAC+6+1+FR'PCI++vincode1:vincode2'PCI++vincode3:vincode4'PCI++vincode5:vincode6'", messageText);
		}

		[TestDate(2020, 3, 9, 16, 13, 23, 456)]
		public override void TestUNBNotTest()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns("1210244B");
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("UNB+UNOA:1+1210244B:ZZ+AEATADUE:ZZ+200309:1613+<<MSGNO PLACEHOLDER>>++&EE'", messageText);
		}

		protected Mock<ITIRMessageDataProvider> mockProvider;

		protected void SetUpMockProvider()
		{
			mockProvider = new Mock<ITIRMessageDataProvider>();

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
			mockProvider.Setup(m => m.LocalReferenceNumber).Returns("1234123444");
			mockProvider.Setup(m => m.CustomsProcedureCategory5).Returns("2801");
			mockProvider.Setup(m => m.CountryOfOrigin).Returns("ES");
			mockProvider.Setup(m => m.CountryOfDestination).Returns("RU");
			mockProvider.Setup(m => m.LocationOfGoodsExamCustomsOffice).Returns("4611");
			mockProvider.Setup(m => m.LocationOfGoodsExam).Returns("VLC001");
			mockProvider.Setup(m => m.CodeOfLoadingLocation).Returns("ESMAD");
			mockProvider.Setup(m => m.CodeOfUnloadingLocation).Returns("PLWAR");
			mockProvider.Setup(m => m.GoodsInContainerIndicator).Returns(true);
			mockProvider.Setup(m => m.SecurityDeclaration).Returns(true);
			mockProvider.Setup(m => m.CountryCodes).Returns(new ZString[] { "ES", "FR", "IS" });
			mockProvider.Setup(m => m.SealCodes).Returns(new ZString[] { "SEAL1", "SEAL2" });
			mockProvider.Setup(m => m.TransportMethodOfPayment).Returns("A");
			mockProvider.Setup(m => m.ConveyanceReferenceNumber).Returns("REFECONVE886612");
			mockProvider.Setup(m => m.TIRCarnetNumber).Returns("XC38000000");
			mockProvider.Setup(m => m.TIRCarnetExpiryDate).Returns(ZDateTime.BrettsBirthday);
			mockProvider.Setup(m => m.ReferenceNumber).Returns("REFNUM");
			mockProvider.Setup(m => m.SpecificCircumstancesIndicator).Returns("CODE");
			mockProvider.Setup(m => m.TotalNumberOfGoods).Returns(15);
			mockProvider.Setup(m => m.TotalNumberOfPackageElements).Returns(1586L);

			var mockCustomsTransitOffice = SetUpCustomsOfficeOfTransit("PL", "002110");
			mockProvider.Setup(m => m.CustomsOfficeOfTransit).Returns(mockCustomsTransitOffice);

			var mockLoadingTransport = BuilderHelperTest.SetUpTransportMediumInfo(ZString.Empty, "M-5678-GD", "ES");
			mockProvider.Setup(m => m.LoadingTransport).Returns(mockLoadingTransport.Object);

			var mockBorderTransportMode = BuilderHelperTest.SetUpTransportMediumInfo("3", "M-1234-GD", "ES");
			mockProvider.Setup(m => m.BorderTransportMode).Returns(mockBorderTransportMode.Object);

			var mockConsignor = BuilderHelperTest.SetUpParty("A01010101", "EXPORTADORA ESPANOLA S.A.", "BASOA 6", "VITORIA", "01012", "ES");
			mockProvider.Setup(m => m.Consignor).Returns(mockConsignor);

			var mockConsignee = BuilderHelperTest.SetUpParty("A01010102", "ITALIAN SPA", "VIA OLIVUZZA", "ASPRA", "01013", "IT");
			mockProvider.Setup(m => m.Consignee).Returns(mockConsignee);

			var mockDeclarant = BuilderHelperTest.SetUpPartyEmail("1210244B", "GUTIERREZ S.A.", "MIDIRECCION.CORREO.EN.CASTILLAYLEON@MIXMAIL.COM");
			mockProvider.Setup(m => m.Declarant).Returns(mockDeclarant);

			var mockHolder = BuilderHelperTest.SetUpParty("A01010108", "OBLIGADOS PRINCIPALES SA", "MORATIN 28", "MADRID", "01014", "ES");
			mockProvider.Setup(m => m.Holder).Returns(mockHolder);

			var mockSecurityCarrier = BuilderHelperTest.SetUpParty("A01010109", "TRANSPORTISTAS EJEMPLO", "BASOA 6", "VITORIA", "01015", "ES");
			mockProvider.Setup(m => m.SecurityCarrier).Returns(mockSecurityCarrier);

			var mockSecurityConsignor = BuilderHelperTest.SetUpParty("A01010119", "EXPEDIDOR SEGURO SA", "BASOA 6", "VITORIA", "01012", "ES");
			mockProvider.Setup(m => m.SecurityConsignor).Returns(mockSecurityConsignor);

			var mockSecurityConsignee = BuilderHelperTest.SetUpParty("A01010121", "DESTINATARIO SEGURO SA", "BASOA 6", "VITORIA", "01012", "ES");
			mockProvider.Setup(m => m.SecurityConsignee).Returns(mockSecurityConsignee);

			var mockPackageIdentification = BuilderHelperTest.SetUpInternalPackages();

			mockLine1 = SetUpTirLine(1, new[] { mockPackageIdentification }, false);
			var mockDocument1 = BuilderHelperTest.SetUpDocument("N380", "ES-187/10");
			var mockDocument2 = BuilderHelperTest.SetUpDocument("X001", "ES3600000002");
			mockLine1.Setup(m => m.Documents).Returns(new[] { mockDocument1, mockDocument2 });

			var mockLine2 = SetUpTirLine(2, new[] { mockPackageIdentification, mockPackageIdentification }, false);
			var mockExternalPackages = BuilderHelperTest.SetUpExternalPackages(2, new ZString[] { "TCKU2126154", "TCKU2126133", "AA", "BB", "CC", "DD", "EE", "FF", "GG", "HH" });
			mockLine2.Setup(m => m.ExternalPackages).Returns(mockExternalPackages);
			mockLine2.Setup(m => m.Documents).Returns((IReadOnlyCollection<IDocumentsCommon>)Enumerable.Empty<IDocumentsCommon>());

			var mockVehiclePackageIdentification1 = SetUpVehicleInternalPackage("vincode1", 3);
			var mockVehiclePackageIdentification2 = SetUpVehicleInternalPackage("vincode2", 3);
			var mockVehiclePackageIdentification3 = SetUpVehicleInternalPackage("vincode3", 3);

			var mockLine3 = SetUpTirLine(3, new[] { mockVehiclePackageIdentification1, mockVehiclePackageIdentification2, mockVehiclePackageIdentification3 }, true);
			mockLine3.Setup(m => m.ExternalPackages).Returns(mockExternalPackages);
			mockLine3.Setup(m => m.Documents).Returns((IReadOnlyCollection<IDocumentsCommon>)Enumerable.Empty<IDocumentsCommon>());

			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object, mockLine3.Object });
		}

		Mock<ITIRLine> SetUpTirLine(ZInt goodsItemNumber, IInternalPackageIdentificationCommon[] packages, ZBool isVehiclePackage)
		{
			var tirLine = new Mock<ITIRLine>();
			tirLine.Setup(m => m.GoodsItemNumber).Returns(goodsItemNumber);
			tirLine.Setup(m => m.DocumentLineNo).Returns("1");
			tirLine.Setup(m => m.GoodsCustomsProcedureCategory1).Returns("120799");
			tirLine.Setup(m => m.GoodsDescription).Returns("CALZADO DE PIEL DE COCODRILO PARA SEÑORAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAABBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBBCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCCDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDDEEEEEEEEEE");
			tirLine.Setup(m => m.GrossWeightInKG).Returns(0.100);
			tirLine.Setup(m => m.OtherUnitsNumber).Returns(2500);
			tirLine.Setup(m => m.OtherUnitsQualifier).Returns("UN");
			tirLine.Setup(m => m.DangerousGoodsCode).Returns("4134");
			tirLine.Setup(m => m.DangerousGoodsCode).Returns("4134");
			tirLine.Setup(m => m.DocumentTypeCode).Returns("AAE");
			tirLine.Setup(m => m.DocumentReferenceNumber).Returns("DUA08ES00280112345679001");
			tirLine.Setup(m => m.DocumentClass).Returns("Z");

			var mockInternalPackages = new Mock<ITIRInternalPackagesInfo>();
			mockInternalPackages.Setup(m => m.Packages).Returns(packages);
			mockInternalPackages.Setup(m => m.IsVehiclePackage).Returns(isVehiclePackage);
			tirLine.Setup(m => m.InternalPackages).Returns(mockInternalPackages.Object);

			return tirLine;
		}

		Mock<ITIRLine> mockLine1;

		IInternalPackageIdentificationCommon SetUpVehicleInternalPackage(ZString tag, ZLong numberOfElements)
		{
			var mockPackages = new Mock<IInternalPackageIdentificationCommon>();
			mockPackages.Setup(m => m.Tag).Returns(tag);
			mockPackages.Setup(m => m.ElementsType).Returns("FR");
			mockPackages.Setup(m => m.NumberOfElements).Returns(numberOfElements);
			return mockPackages.Object;
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
