using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ConsultaImportacionV2Ent;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(DUAImportQueryMessageBuilder))]
	class DUAImportQueryMessageBuilderTest : ImportAbstractMessageBuilderTest<DUAImportQueryMessageBuilder, IDUAImportQueryMessageDataProvider, ConsultaImportacionV2Ent>
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

		public override void TestPopulateSegmentosDeServicioIsTestFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("Test=", messageText);
		}

		public void TestRequestATCEmpty()
		{
			mockProvider.Setup(m => m.RequestATCData).Returns(ZString.Empty);
			var messageText = CreateMessageBuilder().GetSignedMessageText().ToString();
			AssertNotContains("DatosEnATC", messageText);
		}
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportQuery;
		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.ImportTestFilePath, "TestDUAImportQueryMessage.txt");

		protected override DUAImportQueryMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new DUAImportQueryMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override DUAImportQueryMessageBuilder CreateMessageBuilderWithNullProvider() => new DUAImportQueryMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MRN).Returns("21ES00999912345678");
			mockProvider.Setup(m => m.RequestATCData).Returns("S");
		}
	}
}
