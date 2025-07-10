using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LexpedicionModificaV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ExpeditionAmendmentMessageBuilder))]
	public class ExpeditionAmendmentMessageBuilderTest : T2LExpeditionCommonMessageBuilderTest<ExpeditionAmendmentMessageBuilder, IExpeditionAmendmentMessageDataProvider, T2LexpedicionModificaV1Ent, IExpeditionAmendmentHeader, IExpeditionLine>
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

		public override void TestPopulateDeclarante()
		{
			mockHeader.Setup(m => m.Declarant).Returns((IPartyNameProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateExpedidor()
		{
			mockHeader.Setup(m => m.Sender).Returns((IPartyProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateDestinatario()
		{
			mockHeader.Setup(m => m.Consignee).Returns((IPartyProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateCommunications()
		{
			mockHeader.Setup(m => m.Communications).Returns((IT2LCommunicationsCommon)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulatePartidas()
		{
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IExpeditionLine>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IExpeditionLine[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateBultos()
		{
			mockLine1.Setup(m => m.Packages).Returns((IReadOnlyCollection<IPackageCommonNumbers>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulatePackage()
		{
			mockLine1.Setup(m => m.Packages).Returns(new IPackageCommonNumbers[] { null });
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

		public void TestPopulateDocumentos()
		{
			mockLine1.Setup(m => m.DocumentsSubmitted).Returns((IReadOnlyCollection<IExpeditionDocumentSubmitted>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDocument()
		{
			mockLine1.Setup(m => m.DocumentsSubmitted).Returns(new IExpeditionDocumentSubmitted[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.T2lExpeditionAmendment;

		protected override ZString GetSignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.T2LTestFilePath, "TestExpeditionAmendmentMessageSigned.txt");
		protected override ZString GetUnsignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.T2LTestFilePath, "TestExpeditionAmendmentMessage.txt");

		protected override ExpeditionAmendmentMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ExpeditionAmendmentMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override ExpeditionAmendmentMessageBuilder CreateMessageBuilderWithNullProvider() => new ExpeditionAmendmentMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected Mock<IExpeditionAmendmentHeader> mockHeader;
		protected Mock<IExpeditionLine> mockLine1;

		protected override void SetUp()
		{
			base.SetUp();

			mockHeader = SetUpCommonHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);

			var mockPackage1 = BuilderHelperTest.SetUpPackageCommon(4, 0);
			var mockPackage2 = BuilderHelperTest.SetUpPackageCommon(0, 6);
			mockLine1 = SetUpCommonLine(1, new ZString[] { "Container1", "Container2" }, new[] { mockPackage1, mockPackage2 }, Enumerable.Empty<IVehicleCommon>(), Enumerable.Empty<IExpeditionDocumentSubmitted>());

			var mockVehicle = BuilderHelperTest.SetUpVehicle("XX", "Ford", "Focus");
			var mockDocument = SetUpDocument();
			var mockLine2 = SetUpCommonLine(2, Array.Empty<ZString>(), Enumerable.Empty<IPackageCommonNumbers>(), new[] { mockVehicle, mockVehicle }, new[] { mockDocument.Object, mockDocument.Object });

			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}

		protected Mock<IExpeditionAmendmentHeader> SetUpCommonHeader()
		{
			var mockHeader = new Mock<IExpeditionAmendmentHeader>();

			mockHeader.Setup(m => m.ExpeditionCountry).Returns("ES");
			mockHeader.Setup(m => m.TotalLinesNum).Returns(2);
			mockHeader.Setup(m => m.TotalPackagesQty).Returns(1);
			mockHeader.Setup(m => m.ContainersIndicator).Returns(true);

			var mockDeclarant = BuilderHelperTest.SetUpPartyName("89890001K", "Pelinganos");
			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);

			mockHeader.Setup(m => m.ExpeditionCustomsOffice).Returns("001900");
			mockHeader.Setup(m => m.DestinationCountry).Returns("ES");
			mockHeader.Setup(m => m.ConveyanceId).Returns("Tractor Guerini");

			var mockExpediter = BuilderHelperTest.SetUpParty("89890001K", "Lobo Lobate", "Lobera de Valdelacierva", "Por poco en Vinuelas", "19069", "ES");
			mockHeader.Setup(m => m.Sender).Returns(mockExpediter);

			var mockAddressee = BuilderHelperTest.SetUpParty("89890001K", "Ave", "Acostadero eras de abajo", "Fuentelahigera de Albatages", "19024", "ES");
			mockHeader.Setup(m => m.Consignee).Returns(mockAddressee);

			var mockCommunications = SetupCommunications();
			mockHeader.Setup(m => m.Communications).Returns(mockCommunications);

			SetUpSpecificHeaderFieldsCore(mockHeader);

			return mockHeader;
		}

		protected Mock<IExpeditionLine> SetUpCommonLine(ZInt lineNumber, IEnumerable<ZString> containers, IEnumerable<IPackageCommonNumbers> packages, IEnumerable<IVehicleCommon> vehicles, IEnumerable<IExpeditionDocumentSubmitted> documents)
		{
			var mockLine = new Mock<IExpeditionLine>();
			mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine.Setup(m => m.GoodsCode).Returns("04071911");
			mockLine.Setup(m => m.GoodsDescription).Returns("goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaafbbbbb");
			mockLine.Setup(m => m.GrossWeightInKG).Returns(4);
			mockLine.Setup(m => m.NetWeightInKG).Returns(3);
			mockLine.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)containers);
			mockLine.Setup(m => m.Packages).Returns((IReadOnlyCollection<IPackageCommonNumbers>)packages);
			mockLine.Setup(m => m.Vehicles).Returns((IReadOnlyCollection<IVehicleCommon>)vehicles);
			mockLine.Setup(m => m.DocumentsSubmitted).Returns((IReadOnlyCollection<IExpeditionDocumentSubmitted>)documents);

			return mockLine;
		}

		protected void SetUpSpecificHeaderFieldsCore(Mock<IExpeditionAmendmentHeader> mockHeader)
		{
			mockHeader.Setup(m => m.ExpeditionT2LReference).Returns("12ES001900L0000012");
		}
	}
}
