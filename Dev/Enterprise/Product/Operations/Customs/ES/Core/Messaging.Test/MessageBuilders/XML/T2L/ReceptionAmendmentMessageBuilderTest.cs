using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LrecepcionModificaV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ReceptionAmendmentMessageBuilder))]
	public class ReceptionAmendmentMessageBuilderTest : T2LReceptionCommonMessageBuilderTest<ReceptionAmendmentMessageBuilder, IReceptionAmendmentMessageDataProvider, T2LrecepcionModificaV1Ent>
	{
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.T2lReceptionAmendment;

		protected override ZString GetSignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.T2LTestFilePath, "TestReceptionAmendmentMessageSigned.txt");
		protected override ZString GetUnsignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.T2LTestFilePath, "TestReceptionAmendmentMessage.txt");

		protected override ReceptionAmendmentMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ReceptionAmendmentMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override ReceptionAmendmentMessageBuilder CreateMessageBuilderWithNullProvider() => new ReceptionAmendmentMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

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

		public override void TestPopulatePartidas()
		{
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IT2LLineCommon>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IT2LLineCommon[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateBultos()
		{
			mockLine1.Setup(m => m.Packages).Returns((IReadOnlyCollection<IPackageCommonNumbers>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulatePackage()
		{
			mockLine1.Setup(m => m.Packages).Returns(new IPackageCommonNumbers[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateVehiculos()
		{
			mockLine1.Setup(m => m.Vehicles).Returns((IReadOnlyCollection<IVehicleCommon>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public override void TestPopulateVehicle()
		{
			mockLine1.Setup(m => m.Vehicles).Returns(new IVehicleCommon[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected Mock<IReceptionHeader> mockHeader;
		protected Mock<IT2LLineCommon> mockLine1;

		protected override void SetUp()
		{
			base.SetUp();

			mockHeader = SetUpCommonHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);

			var mockPackage1 = BuilderHelperTest.SetUpPackageCommon(4, 0);
			var mockPackage2 = BuilderHelperTest.SetUpPackageCommon(0, 6);
			mockLine1 = SetUpCommonLine(1, new ZString[] { "Container1", "Container2" }, new[] { mockPackage1, mockPackage2 }, Enumerable.Empty<IVehicleCommon>(), Enumerable.Empty<IExpeditionDocumentSubmitted>());

			var mockVehicle = BuilderHelperTest.SetUpVehicle("XX", "Ford", "Focus");
			var mockLine2 = SetUpCommonLine(2, Array.Empty<ZString>(), Enumerable.Empty<IPackageCommonNumbers>(), new[] { mockVehicle, mockVehicle }, Enumerable.Empty<IExpeditionDocumentSubmitted>());

			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}

		protected Mock<IT2LLineCommon> SetUpCommonLine(ZInt lineNumber, IEnumerable<ZString> containers, IEnumerable<IPackageCommonNumbers> packages, IEnumerable<IVehicleCommon> vehicles, IEnumerable<IExpeditionDocumentSubmitted> documents)
		{
			var mockLine = new Mock<IT2LLineCommon>();
			mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine.Setup(m => m.GoodsCode).Returns("04071911");
			mockLine.Setup(m => m.GoodsDescription).Returns("goods description aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaafbbbbb");
			mockLine.Setup(m => m.GrossWeightInKG).Returns(4);
			mockLine.Setup(m => m.NetWeightInKG).Returns(3);
			mockLine.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)containers);
			mockLine.Setup(m => m.Packages).Returns((IReadOnlyCollection<IPackageCommonNumbers>)packages);
			mockLine.Setup(m => m.Vehicles).Returns((IReadOnlyCollection<IVehicleCommon>)vehicles);

			return mockLine;
		}
	}
}
