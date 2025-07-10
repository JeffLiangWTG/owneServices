using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ModificacionPdcCas40V1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(Box40AmendmentImportMessageBuilder))]
	class Box40AmendmentImportMessageBuilderTest : ImportAbstractMessageBuilderTest<Box40AmendmentImportMessageBuilder, IBox40AmendmentImportMessageDataProvider, ModificacionPdcCas40V1Ent>
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

		public void TestPopulatePartidas()
		{
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IBox40AmendmentLine>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IBox40AmendmentLine[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ImportAmendmentBox40;

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.ImportTestFilePath, "TestBox40AmendmentImportMessage.txt");

		protected override Box40AmendmentImportMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new Box40AmendmentImportMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override Box40AmendmentImportMessageBuilder CreateMessageBuilderWithNullProvider() => new Box40AmendmentImportMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MRN).Returns("99989036AZM0000101");

			var mockLine1 = SetUpLine(1, "X", "SUM", "SUM12345689");
			var mockLine2 = SetUpLine(2, "A", "DUA", "DUA12345689");
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}

		Mock<IBox40AmendmentLine> SetUpLine(ZInt lineNumber, ZString docType, ZString docClass, ZString docReference)
		{
			var mockLine1 = new Mock<IBox40AmendmentLine>();
			mockLine1.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine1.Setup(m => m.PrecedentDocumentType).Returns(docType);
			mockLine1.Setup(m => m.PrecedentDocumentClass).Returns(docClass);
			mockLine1.Setup(m => m.PrecedentDocumentReference).Returns(docReference);
			return mockLine1;
		}
	}
}
