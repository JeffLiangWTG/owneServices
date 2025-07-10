using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.AnulaImportacionV1Ent;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(CANPreDUAImportMessageBuilder))]
	class CANPreDUAImportMessageBuilderTest : ImportAbstractMessageBuilderTest<CANPreDUAImportMessageBuilder, ICANPreDUAImportMessageDataProvider, AnulaImportacionV1Ent>
	{
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportPreDeclarationCancellation;
		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.ImportTestFilePath, "TestCANPreDUAImportMessage.txt");

		protected override CANPreDUAImportMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new CANPreDUAImportMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override CANPreDUAImportMessageBuilder CreateMessageBuilderWithNullProvider() => new CANPreDUAImportMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider.Setup(m => m.MRN).Returns("99989036AZM0000101");
		}

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
	}
}
