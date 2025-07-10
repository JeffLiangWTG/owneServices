using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.PreDeclaIncompletaV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(PreDUAIncompleteImportMessageBuilder))]
	class PreDUAIncompleteImportMessageBuilderTest : ImportCommonMessageBuilderTest<PreDUAIncompleteImportMessageBuilder, IPreDUAIncompleteImportMessageDataProvider, PreDeclaIncompletaV1Ent, IPDIHeader, IImportCommonLine>
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

		public override void TestPopulateSegmentosDeServicioIsTestFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("Test=", messageText);
		}

		public void TestPopulatePartidas()
		{
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IImportCommonLine>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IImportCommonLine[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateContenedores()
		{
			mockLine1.Setup(m => m.Containers).Returns(new ZString[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("C31Contenedores", messageText);
		}

		public void TestPopulateRegimen()
		{
			mockLine1.Setup(m => m.RequestedCPC).Returns(ZString.Empty);
			mockLine1.Setup(m => m.PreviousCPC).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions("Both Empty", () =>
			{
				AssertNotContains("C371RegimenAduanero", messageText);
				AssertNotContains("C371RegimenSolicitado", messageText);
				AssertNotContains("C371RegimenPrecedente", messageText);
			});

			mockLine1.Setup(m => m.RequestedCPC).Returns("20");
			messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions("Only previousCPC empty", () =>
			{
				AssertContains("C371RegimenAduanero", messageText);
				AssertContains("<C371RegimenSolicitado>", messageText);
			});

			mockLine1.Setup(m => m.RequestedCPC).Returns(ZString.Empty);
			mockLine1.Setup(m => m.PreviousCPC).Returns("30");
			messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions("Only requestedCPC empty", () =>
			{
				AssertContains("C371RegimenAduanero", messageText);
				AssertContains("<C371RegimenPrecedente>", messageText);
			});
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportIncompletePreDeclaration;
		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.ImportTestFilePath, "TestPreDUAIncompleteImportMessage.txt");

		protected override PreDUAIncompleteImportMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new PreDUAIncompleteImportMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override PreDUAIncompleteImportMessageBuilder CreateMessageBuilderWithNullProvider() => new PreDUAIncompleteImportMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected Mock<IPDIHeader> mockHeader;
		protected Mock<IImportCommonLine> mockLine1;

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MRN).Returns("99989036AZM0000101");

			mockHeader = SetUpHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);

			mockLine1 = SetUpLine(1, new ZString[] { "Container1", "Container2" }, ZString.Empty, ZString.Empty, Enumerable.Empty<IPackageCommonNumbers>(), Enumerable.Empty<IVehicleCommon>(), Enumerable.Empty<IImportCommonC44CertificateDocument>(), Enumerable.Empty<IDUAImportDeclaredTax>());
			var mockLine2 = SetUpLine(2, Array.Empty<ZString>(), "40", "00", Enumerable.Empty<IPackageCommonNumbers>(), Enumerable.Empty<IVehicleCommon>(), Enumerable.Empty<IImportCommonC44CertificateDocument>(), Enumerable.Empty<IDUAImportDeclaredTax>());
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}

		protected Mock<IPDIHeader> SetUpHeader()
		{
			var mockHeader = new Mock<IPDIHeader>();

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

			SetUpSpecificHeaderFieldsCore(mockHeader);

			return mockHeader;
		}

		protected Mock<IImportCommonLine> SetUpLine(ZInt lineNumber, ZString[] containers, ZString requestedCPC, ZString previousCPC, IEnumerable<IPackageCommonNumbers> packages, IEnumerable<IVehicleCommon> vehicles, IEnumerable<IImportCommonC44CertificateDocument> documents, IEnumerable<IDUAImportDeclaredTax> declaredTaxes)
		{
			var mockLine = new Mock<IImportCommonLine>();

			mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine.Setup(m => m.Containers).Returns(containers);
			mockLine.Setup(m => m.GoodsDescription).Returns("goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaafbbbbb");
			mockLine.Setup(m => m.TariffCode).Returns("7318110000");
			mockLine.Setup(m => m.OriginCountry).Returns("IT");
			mockLine.Setup(m => m.RequestedCPC).Returns(requestedCPC);
			mockLine.Setup(m => m.PreviousCPC).Returns(previousCPC);

			return mockLine;
		}

		protected void SetUpSpecificHeaderFieldsCore(Mock<IPDIHeader> mockHeader)
		{
			mockHeader.Setup(m => m.CustomsOffice).Returns("009999");
		}
	}
}
