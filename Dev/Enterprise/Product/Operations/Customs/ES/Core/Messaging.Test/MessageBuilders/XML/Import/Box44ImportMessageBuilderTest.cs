using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Import.ImportDocCas44PendV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(Box44ImportMessageBuilder))]
	class Box44ImportMessageBuilderTest : ImportAbstractMessageBuilderTest<Box44ImportMessageBuilder, IBox44ImportMessageDataProvider, ImportDocCas44PendV1Ent>
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
			mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IBox44Line>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLine()
		{
			mockProvider.Setup(m => m.Lines).Returns(new IBox44Line[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateCertificates()
		{
			mockBox44Line1.Setup(m => m.DocumentsAndCertificates).Returns((IReadOnlyCollection<IImportCommonC44CertificateDocument>)null);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockBox44Line1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateCertificate()
		{
			mockBox44Line1.Setup(m => m.DocumentsAndCertificates).Returns(new IImportCommonC44CertificateDocument[] { null });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockBox44Line1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Box44Documents;

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.ImportTestFilePath, "TestBox44ImportMessage.txt");

		protected override Box44ImportMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new Box44ImportMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override Box44ImportMessageBuilder CreateMessageBuilderWithNullProvider() => new Box44ImportMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			//Decimals should have a comma instead of a point (CertQuantityAmount)
			base.SetUp();

			mockProvider.Setup(m => m.MRN).Returns("99989036AZM0000101");

			var mockCertificate1 = SetUpImportC44CertificateDocument("1407", "ref", "KN", 27.4444, new ZDateTime(2020, 03, 12));
			var mockCertificate2 = SetUpImportC44CertificateDocument("1234", "ref2", ZString.Empty, ZDecimal.Zero, ZDateTime.Empty);

			mockBox44Line1 = SetUpLine(1, new IImportCommonC44CertificateDocument[] { mockCertificate1 });
			var mockLine2 = SetUpLine(2, new IImportCommonC44CertificateDocument[] { mockCertificate1, mockCertificate2 });
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockBox44Line1.Object, mockLine2.Object });
		}

		Mock<IBox44Line> mockBox44Line1;

		Mock<IBox44Line> SetUpLine(ZInt lineNumber, IEnumerable<IImportCommonC44CertificateDocument> certificates)
		{
			var mockLine1 = new Mock<IBox44Line>();
			mockLine1.Setup(m => m.LineNumber).Returns(lineNumber);
			mockLine1.Setup(m => m.DocumentsAndCertificates).Returns((IReadOnlyCollection<IImportCommonC44CertificateDocument>)certificates);
			return mockLine1;
		}
	}
}
