using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Adua.Internet.Es.Aeat.Dit.Adu.Aden.Enswsv5;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ENSAmendmentMessageBuilder))]
	public class ENSAmendmentMessageBuilderTest : ENSCommonMessageBuilderTest<ENSAmendmentMessageBuilder, IENSAmendmentMessageDataProvider, Cc313A>
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

		public void TestPopulateHeader()
		{
			mockProvider.Setup(m => m.Header).Returns((IENSAmendmentHeader)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateConsignor()
		{
			mockProvider.Setup(m => m.Consignor).Returns((IENSAddressInformation)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateConsignee()
		{
			mockProvider.Setup(m => m.Consignee).Returns((IENSAddressInformation)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateNotifyParty()
		{
			mockProvider.Setup(m => m.NotifyParty).Returns((IENSAddressInformation)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateCommonGoodsItems()
		{
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IENSCommonLine>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateCommonLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IENSCommonLine[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateCertificates()
		{
			mockLine1.Setup(m => m.Certificates).Returns((IReadOnlyCollection<IENSDocument>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateCertificate()
		{
			mockLine1.Setup(m => m.Certificates).Returns(new IENSDocument[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateSpecialMentions()
		{
			mockLine1.Setup(m => m.SpecialMentions).Returns((IReadOnlyCollection<ZString>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("SPEMENMT2", messageText);
				AssertNotContains("AddInfCodMT23", messageText);
			});
		}

		public override void TestPopulateSpecialMention()
		{
			mockLine1.Setup(m => m.SpecialMentions).Returns(new ZString[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("AddInfCodMT23", messageText);
		}

		public override void TestPopulateLineConsignor()
		{
			mockLine1.Setup(m => m.Consignor).Returns((IENSAddressInformation)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateCommodityCode()
		{
			mockLine1.Setup(m => m.CommodityCode).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("COMCODGODITM", messageText);
				AssertNotContains("ComNomCMD1", messageText);
			});
		}

		public override void TestPopulateLineConsignee()
		{
			mockLine1.Setup(m => m.Consignee).Returns((IENSAddressInformation)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateContainers()
		{
			mockLine1.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("CONNR2", messageText);
				AssertNotContains("ConNumNR21", messageText);
			});
		}

		public override void TestPopulateContainer()
		{
			mockLine1.Setup(m => m.Containers).Returns(new ZString[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("ConNumNR21", messageText);
		}

		public override void TestPopulateBorderTransportMeans()
		{
			mockLine1.Setup(m => m.BorderTransportMeans).Returns((IReadOnlyCollection<IENSBorderTransport>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateBorderTransport()
		{
			mockLine1.Setup(m => m.BorderTransportMeans).Returns(new IENSBorderTransport[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulatePackages()
		{
			mockLine1.Setup(m => m.Packages).Returns((IReadOnlyCollection<IENSPackage>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulatePackage()
		{
			mockLine1.Setup(m => m.Packages).Returns(new IENSPackage[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateLineNotifyParty()
		{
			mockLine1.Setup(m => m.NotifyParty).Returns((IENSAddressInformation)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateItineraryCountries()
		{
			mockProvider.Setup(m => m.ItineraryCountries).Returns((IReadOnlyCollection<ZString>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("ITI", messageText);
				AssertNotContains("CouOfRouCodITI1", messageText);
			});
		}

		public override void TestPopulateRepresentativeTrader()
		{
			mockProvider.Setup(m => m.RepresentativeTrader).Returns((IENSAddressInformation)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateLodgingPerson()
		{
			mockProvider.Setup(m => m.LodgingPerson).Returns((IENSAddressInformation)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateSeals()
		{
			mockProvider.Setup(m => m.Seals).Returns((IReadOnlyCollection<IENSSeal>)(IEnumerable<ZString>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateSeal()
		{
			mockProvider.Setup(m => m.Seals).Returns(new IENSSeal[] { null });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("SEAID529", messageText);
				AssertNotContains("SeaIdSEAID530", messageText);
				AssertNotContains("SeaIdSEAID530LNG", messageText);
			});
		}

		public override void TestPopulateCommonFirstEntryCustomsOffice()
		{
			mockProvider.Setup(m => m.FirstEntryCustomsOffice).Returns(ZString.Empty);
			mockProvider.Setup(m => m.FirstEntryExpectedArrivalDate).Returns(ZDateTime.Empty);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions("Both Empty", () =>
			{
				AssertNotContains("CUSOFFFENT730", messageText);
				AssertNotContains("RefNumCUSOFFFENT731", messageText);
				AssertNotContains("ExpDatOfArrFIRENT733", messageText);
			});

			mockProvider.Setup(m => m.FirstEntryExpectedArrivalDate).Returns(ZDateTime.Now);
			messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions("Only office empty", () =>
			{
				AssertContains("CUSOFFFENT730", messageText);
				AssertNotContains("RefNumCUSOFFFENT731", messageText);
				AssertContains("<ExpDatOfArrFIRENT733>", messageText);
			});

			mockProvider.Setup(m => m.FirstEntryCustomsOffice).Returns("ES009999");
			mockProvider.Setup(m => m.FirstEntryExpectedArrivalDate).Returns(ZDateTime.Empty);
			messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions("Only date empty", () =>
			{
				AssertContains("CUSOFFFENT730", messageText);
				AssertContains("<RefNumCUSOFFFENT731>", messageText);
				AssertNotContains("ExpDatOfArrFIRENT733", messageText);
			});
		}

		public override void TestPopulateSubsequentEntriesCustomsOffices()
		{
			mockProvider.Setup(m => m.SubsequentEntriesCustomsOffices).Returns((IReadOnlyCollection<ZString>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateSubsequentEntriesCustomsOffice()
		{
			mockProvider.Setup(m => m.SubsequentEntriesCustomsOffices).Returns(new ZString[] { null });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("CUSOFFSENT740", messageText);
				AssertNotContains("RefNumSUBENR909", messageText);
			});
		}

		public override void TestPopulateEntryCarrierTrader()
		{
			mockProvider.Setup(m => m.EntryCarrierTrader).Returns((IENSAddressInformation)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.EnsAmendment;

		protected override ZString GetSignedMessageTestFileContent() => GetTestFileContents(
#if NETFRAMEWORK
			XMLTestFileConstants.ENSTestFilePath,
#else
			XMLTestFileConstants.ENSTestFilePath + ".net8",
#endif
			"TestENSAmendmentMessageSigned.txt");
		protected override ZString GetUnsignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.ENSTestFilePath, "TestENSAmendmentMessage.txt");

		protected override ENSAmendmentMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ENSAmendmentMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override ENSAmendmentMessageBuilder CreateMessageBuilderWithNullProvider() => new ENSAmendmentMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected Mock<IENSCommonLine> mockLine1;

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider.Setup(m => m.SenderId).Returns("89890001K");

			mockProvider.Setup(m => m.Priority).Returns("A");
			mockProvider.Setup(m => m.SenderName).Returns("Certi");

			mockProvider.Setup(m => m.ItineraryCountries).Returns(new ZString[] { "ES", "FR", "GB" });
			mockProvider.Setup(m => m.FirstEntryCustomsOffice).Returns("ES009999");
			mockProvider.Setup(m => m.FirstEntryExpectedArrivalDate).Returns(new ZDateTime(2020, 03, 12, 14, 55, 00));
			mockProvider.Setup(m => m.SubsequentEntriesCustomsOffices).Returns(new ZString[] { "FR000010", "IT014100" });

			var mockConsignor = SetUpENSAddressInformation("89890001K", "Lobo Lobate", "Lobera de Valdelacierva", "Por poco en Vinuelas", "19069", "ES", "ES");
			mockProvider.Setup(m => m.Consignor).Returns(mockConsignor);

			var mockConsignee = SetUpENSAddressInformation("89890002K", "Ave", "Acostadero eras de abajo", "Fuentelahigera de Albatages", "19024", "ES", "ES");
			mockProvider.Setup(m => m.Consignee).Returns(mockConsignee);

			var mockNotifyParty = SetUpENSAddressInformation("89890003K", "Otro", "Calle Numero", "Colmenar Viejo", "28000", "ES", "ES");
			mockProvider.Setup(m => m.NotifyParty).Returns(mockNotifyParty);

			var mockCertificate1 = SetUpENSCertificate("N703", "ESAEOC1", "ES");
			var mockCertificate2 = SetUpENSCertificate("N706", "ESAEOC2", "FR");

			var mockBorderTransportMean = SetUpBorderTransport();

			var mockPackage1 = SetUpENSPackage(4, 0);
			var mockPackage2 = SetUpENSPackage(0, 6);

			mockLine1 = SetUpENSLine(1, new IENSDocument[] { mockCertificate1, mockCertificate2 }, new ZString[] { "Mention1", "Mention2" }, Enumerable.Empty<ZString>(), new IENSBorderTransport[] { mockBorderTransportMean, mockBorderTransportMean }, Enumerable.Empty<IENSPackage>());
			var mockLine2 = SetUpENSLine(2, Enumerable.Empty<IENSDocument>(), Enumerable.Empty<ZString>(), new ZString[] { "Container1", "Container2" }, Enumerable.Empty<IENSBorderTransport>(), new IENSPackage[] { mockPackage1, mockPackage2 });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });

			var mockRepresentativeTrader = SetUpENSAddressInformation("ESA08005688", "PEDRO", "CASTELLANA", "MADRID", "28003", "ES", "ES");
			mockProvider.Setup(m => m.RepresentativeTrader).Returns(mockRepresentativeTrader);

			var mockLodgingPerson = SetUpENSAddressInformation("ESA12345678", "importer name", "importer address complete", "MADRID", "12345", "ES", "ES");
			mockProvider.Setup(m => m.LodgingPerson).Returns(mockLodgingPerson);

			var mockSeal = SetUpSeal();
			mockProvider.Setup(m => m.Seals).Returns(new[] { mockSeal, mockSeal });

			var mockEntryCarrierTrader = SetUpENSAddressInformation("ESA08005688", "ROSA", "MONCLOA", "MADRID", "28007", "ES", "ES");
			mockProvider.Setup(m => m.EntryCarrierTrader).Returns(mockEntryCarrierTrader);

			var mockHeader = SetUpHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);
		}

		Mock<IENSAmendmentHeader> SetUpHeader()
		{
			var mockHeader = new Mock<IENSAmendmentHeader>();
			mockHeader.Setup(m => m.MRN).Returns("99989036AZM0000101");
			mockHeader.Setup(m => m.BorderTransportMode).Returns("2");
			mockHeader.Setup(m => m.TotalLinesNum).Returns(2);
			mockHeader.Setup(m => m.TotalPackagesQty).Returns(21);
			mockHeader.Setup(m => m.TotalGrossWeight).Returns(150.0M);
			mockHeader.Setup(m => m.AmendmentPlace).Returns("Madrid");
			mockHeader.Setup(m => m.AmendmentPlaceLanguage).Returns("ES");
			mockHeader.Setup(m => m.SpecificCircumstanceInd).Returns("A");
			mockHeader.Setup(m => m.MethodOfPayment).Returns("C");
			mockHeader.Setup(m => m.CommercialReferenceNumber).Returns("REFERENCIACOMER1");
			mockHeader.Setup(m => m.ConveyanceReferenceNumber).Returns("REFERENCIACONVE1");
			mockHeader.Setup(m => m.PlaceOfLoading).Returns("ESMadrid");
			mockHeader.Setup(m => m.PlaceOfLoadingLanguage).Returns("ES");
			mockHeader.Setup(m => m.PlaceOfUnloading).Returns("ESSegovia");
			mockHeader.Setup(m => m.PlaceOfUnloadingLanguage).Returns("ES");
			mockHeader.Setup(m => m.AmendmentDate).Returns(new ZDateTime(2020, 03, 12, 14, 55, 00));

			var mockBorderTransport = SetUpBorderTransport();
			mockHeader.Setup(m => m.BorderTransportInfo).Returns(mockBorderTransport);
			return mockHeader;
		}

		IENSDocument SetUpENSCertificate(ZString certType, ZString certRef, ZString certLang)
		{
			var mockCert = new Mock<IENSDocument>();
			mockCert.Setup(m => m.Name).Returns(certType);
			mockCert.Setup(m => m.Number).Returns(certRef);
			mockCert.Setup(m => m.Language).Returns(certLang);
			return mockCert.Object;
		}

		IENSPackage SetUpENSPackage(ZInt packagesQty, ZInt piecesQty)
		{
			var mockPackage = new Mock<IENSPackage>();
			mockPackage.Setup(m => m.PackageType).Returns("NE");
			mockPackage.Setup(m => m.Marks).Returns("PAQUETES1");
			mockPackage.Setup(m => m.MarksLanguage).Returns("ES");
			mockPackage.Setup(m => m.PackagesQty).Returns(packagesQty);
			mockPackage.Setup(m => m.PiecesQty).Returns(piecesQty);
			return mockPackage.Object;
		}

		Mock<IENSCommonLine> SetUpENSLine(ZInt lineNumber, IEnumerable<IENSDocument> certificates, IEnumerable<ZString> specialMentions, IEnumerable<ZString> containers, IEnumerable<IENSBorderTransport> borderTransportMeans, IEnumerable<IENSPackage> packages)
		{
			var mockLine = new Mock<IENSCommonLine>();
			mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine.Setup(m => m.GoodsDescription).Returns("DESCRIPCION PARTIDA1");
			mockLine.Setup(m => m.GoodsDescriptionLanguage).Returns("ES");
			mockLine.Setup(m => m.GrossWeight).Returns(4.0M);
			mockLine.Setup(m => m.MethodOfPayment).Returns("A");
			mockLine.Setup(m => m.CommercialReferenceNumber).Returns("Reference");
			mockLine.Setup(m => m.UNDangerousCode).Returns("1");
			mockLine.Setup(m => m.PlaceOfLoading).Returns("ESMadrid");
			mockLine.Setup(m => m.PlaceOfLoadingLanguage).Returns("ES");
			mockLine.Setup(m => m.PlaceOfUnloading).Returns("ESSegovia");
			mockLine.Setup(m => m.PlaceOfUnloadingLanguage).Returns("ES");
			mockLine.Setup(m => m.Certificates).Returns((IReadOnlyCollection<IENSDocument>)certificates);
			mockLine.Setup(m => m.SpecialMentions).Returns((IReadOnlyCollection<ZString>)specialMentions);
			mockLine.Setup(m => m.CommodityCode).Returns("123456");
			mockLine.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)containers);
			mockLine.Setup(m => m.BorderTransportMeans).Returns((IReadOnlyCollection<IENSBorderTransport>)borderTransportMeans);
			mockLine.Setup(m => m.Packages).Returns((IReadOnlyCollection<IENSPackage>)packages);

			var mockConsignor = SetUpENSAddressInformation("89890001K", "Lobo Lobate", "Lobera de Valdelacierva", "Por poco en Vinuelas", "19069", "ES", "ES");
			mockLine.Setup(m => m.Consignor).Returns(mockConsignor);

			var mockConsignee = SetUpENSAddressInformation("89890002K", "Ave", "Acostadero eras de abajo", "Fuentelahigera de Albatages", "19024", "ES", "ES");
			mockLine.Setup(m => m.Consignee).Returns(mockConsignee);

			var mockNotifyParty = SetUpENSAddressInformation("89890003K", "Otro", "Calle Numero", "Colmenar Viejo", "28000", "ES", "ES");
			mockLine.Setup(m => m.NotifyParty).Returns(mockNotifyParty);
			return mockLine;
		}

		IENSSeal SetUpSeal()
		{
			var mockSeal = new Mock<IENSSeal>();
			mockSeal.Setup(m => m.Id).Returns("SealId");
			mockSeal.Setup(m => m.Language).Returns("ES");
			return mockSeal.Object;
		}
	}
}
