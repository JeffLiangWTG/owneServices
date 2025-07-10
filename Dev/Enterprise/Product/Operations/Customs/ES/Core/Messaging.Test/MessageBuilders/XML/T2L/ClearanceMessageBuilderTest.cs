using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LdatadoV2Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ClearanceMessageBuilder))]
	class ClearanceMessageBuilderTest : T2LCommonMessageBuilderTest<ClearanceMessageBuilder, IClearanceMessageDataProvider, T2LdatadoV2Ent, IT2LHeaderCommon, IT2LLineCommon>
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

		public void TestPopulateClearanceDeclarante()
		{
			mockClearanceHeader.Setup(m => m.Declarant).Returns((IPartyNameProvider)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateClearanceCommunications()
		{
			mockClearanceHeader.Setup(m => m.Communications).Returns((IT2LCommunicationsCommon)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulatePartidas()
		{
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IClearanceLine>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IClearanceLine[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.T2lClearance;

		protected override ZString GetSignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.T2LTestFilePath, "TestClearanceMessageSigned.txt");
		protected override ZString GetUnsignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.T2LTestFilePath, "TestClearanceMessage.txt");

		protected override ClearanceMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ClearanceMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override ClearanceMessageBuilder CreateMessageBuilderWithNullProvider() => new ClearanceMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();

			mockClearanceHeader = SetUpHeader();
			mockProvider.Setup(m => m.Header).Returns(mockClearanceHeader.Object);

			var mockLine1 = SetUpLine(1, new ZString[] { "Container1", "Container2" });
			var mockLine2 = SetUpLine(2, Array.Empty<ZString>());
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}

		protected Mock<IClearanceHeader> mockClearanceHeader;

		Mock<IClearanceHeader> SetUpHeader()
		{
			var mockHeader = new Mock<IClearanceHeader>();
			mockHeader.Setup(m => m.ReceptionCustomsOffice).Returns("009999");
			mockHeader.Setup(m => m.ReceptionT2LReference).Returns("12ES001900L0000012");
			mockHeader.Setup(m => m.GoodsLocation).Returns("9998123456");
			mockHeader.Setup(m => m.TotalLinesNum).Returns(2);
			mockHeader.Setup(m => m.ContainersIndicator).Returns(true);

			var mockDeclarant = BuilderHelperTest.SetUpPartyName("ESA78587268", "Declarant name");
			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);

			var mockCommunications = SetupCommunications();
			mockHeader.Setup(m => m.Communications).Returns(mockCommunications);
			return mockHeader;
		}

		Mock<IClearanceLine> SetUpLine(ZInt lineNumber, IEnumerable<ZString> containers)
		{
			var mockLine = new Mock<IClearanceLine>();
			mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine.Setup(m => m.SummaryDeclaration).Returns("4611000134512345");
			mockLine.Setup(m => m.LineNumberReferenced).Returns(2);
			mockLine.Setup(m => m.GrossWeightInKG).Returns(200);
			mockLine.Setup(m => m.PackageQty).Returns(20);
			mockLine.Setup(m => m.Containers).Returns((IReadOnlyCollection<ZString>)containers);
			return mockLine;
		}
	}
}
