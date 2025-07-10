using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.AnulaPDCVinculacionV1Ent;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(CancelDVDMessageBuilder))]
	class CancelDVDMessageBuilderTest : DVDCommonMessageBuilderTest<CancelDVDMessageBuilder, ICancelDVDMessageDataProvider, AnulaPdcVinculacionV1Ent>
	{
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.DvdH2Cancellation;

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
			AssertNotContains("IndicadorTest", messageText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider.Setup(m => m.MRN).Returns("20ES00999830001277");
		}

		protected override CancelDVDMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new CancelDVDMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override CancelDVDMessageBuilder CreateMessageBuilderWithNullProvider() => new CancelDVDMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.DVDTestFilePath, "TestCancelDVD.txt");
	}
}
