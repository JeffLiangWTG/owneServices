using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.DeclaSimpliImporV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(DUASimplifiedImportMessageBuilder))]
	class DUASimplifiedImportMessageBuilderTest : ImportDUACommonMessageBuilderTest<DUASimplifiedImportMessageBuilder, IDUASimplifiedImportMessageDataProvider, DeclaSimpliImporV1Ent, IDUAImportCommonHeader, IDUAImportCommonLine>
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

		public override void TestPopulateImportador()
		{
			mockHeader.Setup(m => m.Importer).Returns((IImportImporterProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateDeclarante()
		{
			mockHeader.Setup(m => m.Declarant).Returns((IImportDeclarantPartyIdProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateGarantiaGRN()
		{
			mockHeader.Setup(m => m.GRNGuarantees).Returns(new ZString[] { null });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("CBgarantiaGRN", messageText);
		}

		public override void TestPopulateGarantiaGRNATC()
		{
			mockHeader.Setup(m => m.GRNGuaranteesCan).Returns(new ZString[] { null });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("CBgarantiaGRNATC", messageText);
		}

		public override void TestPopulateSegmentosDeServicioIsTestFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("Test=", messageText);
		}

		public override void TestCAaduanaNullWhenMRNDeclared()
		{
			mockProvider.Setup(m => m.MRN).Returns("99989036AZM0000101");
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertContains("<NumeroReferenciaDUA>99989036AZM0000101</NumeroReferenciaDUA>", messageText);
				AssertNotContains("CAaduana", messageText);
			});
		}

		public override void TestServDatadoEnCeutaMelillaFalse()
		{
			mockProvider.Setup(m => m.IsCeutaOrMelilla).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("ServDatadoEnCeutaMelilla", messageText);
		}

		public void TestPopulatePartidas()
		{
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IDUAImportCommonLine>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IDUAImportCommonLine[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateEmpaquetamientoInterno()
		{
			mockLine1.Setup(m => m.InternalPackages).Returns((IReadOnlyCollection<IPackageCommonNumbers>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateInternalPackage()
		{
			mockLine1.Setup(m => m.InternalPackages).Returns(new IPackageCommonNumbers[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateVehiculos()
		{
			mockLine1.Setup(m => m.Vehicles).Returns((IReadOnlyCollection<IVehicleCommon>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateVehicle()
		{
			mockLine1.Setup(m => m.Vehicles).Returns(new IVehicleCommon[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateOtrasUnidadesDeMedida()
		{
			mockLine1.Setup(m => m.OtherMeasurementUnitsCode).Returns(ZString.Empty);
			mockLine1.Setup(m => m.OtherMeasurementUnitsNumber).Returns(ZDecimal.Zero);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("Both Empty", () =>
			{
				AssertNotContains("C31OtrasUnidadesDeMedida", messageText);
				AssertNotContains("C31OtrasUnidadesDeMedidaCodigo", messageText);
				AssertNotContains("C31OtrasUnidadesDeMedidaNumero", messageText);
			});

			mockLine1.Setup(m => m.OtherMeasurementUnitsCode).Returns("Code");
			messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("Only number empty shows number as 0", () =>
			{
				AssertContains("C31OtrasUnidadesDeMedida", messageText);
				AssertContains("<C31OtrasUnidadesDeMedidaCodigo>", messageText);
				AssertContains("<C31OtrasUnidadesDeMedidaNumero>0</C31OtrasUnidadesDeMedidaNumero>", messageText);
			});

			mockLine1.Setup(m => m.OtherMeasurementUnitsCode).Returns(ZString.Empty);
			mockLine1.Setup(m => m.OtherMeasurementUnitsNumber).Returns(100);
			messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("Only code empty", () =>
			{
				AssertContains("C31OtrasUnidadesDeMedida", messageText);
				AssertNotContains("C31OtrasUnidadesDeMedidaCodigo", messageText);
				AssertContains("<C31OtrasUnidadesDeMedidaNumero>", messageText);
			});
		}

		public void TestPopulateContenedores()
		{
			mockLine1.Setup(m => m.Containers).Returns(new ZString[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			AssertNotContains("C31Contenedor", messageText);
		}

		public void TestPopulateCodigoAdicionalTaric()
		{
			mockLine1.Setup(m => m.TariffSupplementaryCodes).Returns(new ZString[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			AssertNotContains("C333CodigoAdicionalTaric", messageText);
		}

		public void TestPopulateCodigoAdicional()
		{
			mockLine1.Setup(m => m.ConcessionsCPC).Returns(new ZString[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			AssertNotContains("C372CodigoAdicional", messageText);
		}

		public void TestPopulateDocumentoCargoPrecedente()
		{
			mockLine1.Setup(m => m.PrecedentDocumentType).Returns(ZString.Empty);
			mockLine1.Setup(m => m.PrecedentDocumentClass).Returns(ZString.Empty);
			mockLine1.Setup(m => m.PrecedentDocumentReference).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("All Empty", () =>
			{
				AssertNotContains("C40DocumentoCargoPrecedente", messageText);
				AssertNotContains("C40TipoDocumento", messageText);
				AssertNotContains("C40ClaseDocumento", messageText);
				AssertNotContains("C40ReferenciaDocumento", messageText);
			});

			mockLine1.Setup(m => m.PrecedentDocumentClass).Returns("class");
			mockLine1.Setup(m => m.PrecedentDocumentReference).Returns("ref");
			messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("Only type empty", () =>
			{
				AssertContains("C40DocumentoCargoPrecedente", messageText);
				AssertContains("<C40ClaseDocumento>", messageText);
				AssertContains("<C40ReferenciaDocumento>", messageText);
			});

			mockLine1.Setup(m => m.PrecedentDocumentType).Returns("type");
			mockLine1.Setup(m => m.PrecedentDocumentClass).Returns(ZString.Empty);
			messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("Only class empty", () =>
			{
				AssertContains("C40DocumentoCargoPrecedente", messageText);
				AssertContains("<C40TipoDocumento>", messageText);
				AssertContains("<C40ReferenciaDocumento>", messageText);
			});

			mockLine1.Setup(m => m.PrecedentDocumentClass).Returns("class");
			mockLine1.Setup(m => m.PrecedentDocumentReference).Returns(ZString.Empty);
			messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("Only reference empty", () =>
			{
				AssertContains("C40DocumentoCargoPrecedente", messageText);
				AssertContains("<C40TipoDocumento>", messageText);
				AssertContains("<C40ClaseDocumento>", messageText);
				AssertNotContains("C40ReferenciaDocumento", messageText);
			});
		}

		public void TestPopulateUnidadesSuplementarias()
		{
			mockLine1.Setup(m => m.SupplementaryUnitsCode).Returns(ZString.Empty);
			mockLine1.Setup(m => m.SupplementaryUnitsNumber).Returns(ZDecimal.Zero);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("Both Empty", () =>
			{
				AssertNotContains("C41UnidadesSuplementarias", messageText);
				AssertNotContains("C41UnidadesCodigo", messageText);
				AssertNotContains("C41UnidadesNumero", messageText);
			});

			mockLine1.Setup(m => m.SupplementaryUnitsCode).Returns("Code");
			messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("Only number empty shows number as 0", () =>
			{
				AssertContains("C41UnidadesSuplementarias", messageText);
				AssertContains("<C41UnidadesCodigo>", messageText);
				AssertContains("<C41UnidadesNumero>0</C41UnidadesNumero>", messageText);
			});

			mockLine1.Setup(m => m.SupplementaryUnitsCode).Returns(ZString.Empty);
			mockLine1.Setup(m => m.SupplementaryUnitsNumber).Returns(100);
			messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			CombineAssertions("Only code empty", () =>
			{
				AssertContains("C41UnidadesSuplementarias", messageText);
				AssertNotContains("C41UnidadesCodigo", messageText);
				AssertContains("<C41UnidadesNumero>", messageText);
			});
		}

		public void TestPopulateDocumentosYCertificados()
		{
			mockLine1.Setup(m => m.DocumentsAndCertificates).Returns((IReadOnlyCollection<IImportCommonC44CertificateDocument>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDocument()
		{
			mockLine1.Setup(m => m.DocumentsAndCertificates).Returns((IReadOnlyCollection<IImportCommonC44CertificateDocument>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateSpecialInstructions()
		{
			mockLine1.Setup(m => m.SpecialInstructions).Returns(new ZString[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			AssertNotContains("C44IndicacionesEspeciales", messageText);
		}

		public void TestPopulateTributoDeclarado()
		{
			mockLine1.Setup(m => m.DeclaredTaxes).Returns((IReadOnlyCollection<IDUAImportDeclaredTax>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDeclaredTax()
		{
			mockLine1.Setup(m => m.DeclaredTaxes).Returns(new IDUAImportDeclaredTax[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportSimplifiedPreDeclaration;
		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.ImportTestFilePath, "TestDUASimplifiedImportMessage.txt");

		protected override DUASimplifiedImportMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new DUASimplifiedImportMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override DUASimplifiedImportMessageBuilder CreateMessageBuilderWithNullProvider() => new DUASimplifiedImportMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected Mock<IDUAImportCommonHeader> mockHeader;
		protected Mock<IDUAImportCommonLine> mockLine1;

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider.Setup(m => m.MRN).Returns(ZString.Empty);

			mockProvider.Setup(m => m.IsCeutaOrMelilla).Returns(true);

			mockHeader = SetUpHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);

			var mockInternalPackage1 = BuilderHelperTest.SetUpPackageCommon(50, 0);
			var mockInternalPackage2 = BuilderHelperTest.SetUpPackageCommon(0, 200);
			var mockVehicle = BuilderHelperTest.SetUpVehicle("VS8ZAZB7861ZB6913", "RENAULT", "LAGUNA 2007");
			var mockDocument1 = SetUpImportC44CertificateDocument("X001", "ES3600000001", "CC", 20.401, new ZDateTime(2020, 12, 25));
			var mockDocument2 = SetUpImportC44CertificateDocument("X002", "ES3600000002", ZString.Empty, ZDecimal.Zero, ZDateTime.Empty);
			var mockDeclaredTax1 = SetUpDeclaredTax("AAA", 111.111, 22.123456, "MA", "ZZ", ZDecimal.Zero);
			var mockDeclaredTax2 = SetUpDeclaredTax("BBB", 22.222, 65.654321, "MI", ZString.Empty, 20.55);

			mockLine1 = SetUpLine(1, new ZString[] { "Container1", "Container2" }, ZString.Empty, ZString.Empty, new[] { mockInternalPackage1, mockInternalPackage2 }, Enumerable.Empty<IVehicleCommon>(), new[] { mockDocument1, mockDocument2 }, Enumerable.Empty<IDUAImportDeclaredTax>());
			var mockLine2 = SetUpLine(2, Array.Empty<ZString>(), "40", "00", Enumerable.Empty<IPackageCommonNumbers>(), new[] { mockVehicle, mockVehicle }, Enumerable.Empty<IImportCommonC44CertificateDocument>(), new[] { mockDeclaredTax1, mockDeclaredTax2 });

			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}

		protected Mock<IDUAImportCommonHeader> SetUpHeader()
		{
			var mockHeader = new Mock<IDUAImportCommonHeader>();

			mockHeader.Setup(m => m.ShipmentType).Returns("IM");
			mockHeader.Setup(m => m.TotalLinesNum).Returns(2);
			mockHeader.Setup(m => m.DeclarationEmail).Returns("mail@mail.com");
			mockHeader.Setup(m => m.OtherEmail).Returns("other.mail@mail.com");
			mockHeader.Setup(m => m.OriginCountry).Returns("DE");
			mockHeader.Setup(m => m.GoodsLocation).Returns("ES000081UBICA01");

			var mockImporter = SetUpImporter();
			mockHeader.Setup(m => m.Importer).Returns(mockImporter);

			var mockDeclarant = SetUpImportDeclarant();
			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);

			mockHeader.Setup(m => m.CustomsOfficeOfDestination).Returns("009999");
			mockHeader.Setup(m => m.Procedure).Returns("A");
			mockHeader.Setup(m => m.TotalPackagesNum).Returns(3);
			mockHeader.Setup(m => m.CommercialReference).Returns("RUECode");
			mockHeader.Setup(m => m.IsContainerised).Returns(true);
			mockHeader.Setup(m => m.CurrencyCode).Returns("EUR");
			mockHeader.Setup(m => m.TotalTributesAmount).Returns(200.555);
			mockHeader.Setup(m => m.PaymentMode).Returns("A");
			mockHeader.Setup(m => m.ClearanceGuarantee).Returns("guarantee1");
			mockHeader.Setup(m => m.PendenciesGuarantee).Returns("guarantee2");
			mockHeader.Setup(m => m.GRNGuarantees).Returns(new ZString[] { "guarantee3", "guarantee4" });
			mockHeader.Setup(m => m.PaymentModeCan).Returns("B");
			mockHeader.Setup(m => m.ClearanceGuaranteeCan).Returns(ZString.Empty);
			mockHeader.Setup(m => m.PendenciesGuaranteeCan).Returns("guaranteeCan2");
			mockHeader.Setup(m => m.GRNGuaranteesCan).Returns((IReadOnlyCollection<ZString>)Enumerable.Empty<ZString>());

			return mockHeader;
		}

		protected Mock<IDUAImportCommonLine> SetUpLine(ZInt lineNumber, ZString[] containers, ZString requestedCPC, ZString previousCPC, IEnumerable<IPackageCommonNumbers> packages, IEnumerable<IVehicleCommon> vehicles, IEnumerable<IImportCommonC44CertificateDocument> documents, IEnumerable<IDUAImportDeclaredTax> declaredTaxes)
		{
			var mockLine = new Mock<IDUAImportCommonLine>();
			mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine.Setup(m => m.Containers).Returns(containers);
			mockLine.Setup(m => m.GoodsDescription).Returns("goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaafbbbbb");
			mockLine.Setup(m => m.TariffCode).Returns("7318110000");
			mockLine.Setup(m => m.OriginCountry).Returns("IT");
			mockLine.Setup(m => m.RequestedCPC).Returns(requestedCPC);
			mockLine.Setup(m => m.PreviousCPC).Returns(previousCPC);

			mockLine.Setup(m => m.ExternalPackagingType).Returns("BX");
			mockLine.Setup(m => m.OtherMeasurementUnitsCode).Returns("KGM");
			mockLine.Setup(m => m.OtherMeasurementUnitsNumber).Returns(20.2222);
			mockLine.Setup(m => m.TariffSupplementaryCodes).Returns(new ZString[] { "1234", "5678" });
			mockLine.Setup(m => m.ProductTitleForSpecialTaxes).Returns("AAA");
			mockLine.Setup(m => m.SpecialTaxesIndicator).Returns("A");
			mockLine.Setup(m => m.GrossWeightInKG).Returns(500.66);
			mockLine.Setup(m => m.PreferenceCode).Returns("1");
			mockLine.Setup(m => m.ReductionCode).Returns("23");
			mockLine.Setup(m => m.ConcessionsCPC).Returns(new ZString[] { "100", "200" });
			mockLine.Setup(m => m.NetWeightInKG).Returns(442.6);
			mockLine.Setup(m => m.Contingency).Returns("123456");
			mockLine.Setup(m => m.PrecedentDocumentType).Returns("X");
			mockLine.Setup(m => m.PrecedentDocumentClass).Returns("SUM");
			mockLine.Setup(m => m.PrecedentDocumentReference).Returns("SUM12345689");
			mockLine.Setup(m => m.SupplementaryUnitsCode).Returns("BB");
			mockLine.Setup(m => m.SupplementaryUnitsNumber).Returns(66.8888);
			mockLine.Setup(m => m.InvoiceValue).Returns(450.3333);
			mockLine.Setup(m => m.SpecialInstructions).Returns(new ZString[] { "inst1", "inst2", "inst3" });
			mockLine.Setup(m => m.TotalValue).Returns(2567.366);

			mockLine.Setup(m => m.InternalPackages).Returns((IReadOnlyCollection<IPackageCommonNumbers>)packages);
			mockLine.Setup(m => m.Vehicles).Returns((IReadOnlyCollection<IVehicleCommon>)vehicles);
			mockLine.Setup(m => m.DocumentsAndCertificates).Returns((IReadOnlyCollection<IImportCommonC44CertificateDocument>)documents);
			mockLine.Setup(m => m.DeclaredTaxes).Returns((IReadOnlyCollection<IDUAImportDeclaredTax>)declaredTaxes);

			return mockLine;
		}
	}
}
