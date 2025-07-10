using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCDOCC_v514.CCDOCCV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(AnnexAESMessageBuilder))]
	class AnnexAESMessageBuilderTest : AESCommonMessageBuilderTest<AnnexAESMessageBuilder, IAnnexAESMessageDataProvider, Ccdoccv1Ent>
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

		public override void TestPopulatePhaseID()
		{
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PhaseID", messageText);
		}

		public void TestPopulateExportOperation()
		{
			mockProvider.Setup(m => m.ExportOperation).Returns((IAESCommonExportOperationMRN)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDispatchRequest()
		{
			mockProvider.Setup(m => m.DispatchRequestCode).Returns(ZString.Empty);

			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());

				var messageText = CreateMessageBuilder().GetSignedMessageText();
				var expectedResult = @$"</{XMLTestFileConstants.XmlElementNamespace}ExportOperation>
    <{XMLTestFileConstants.XmlElementNamespace}DocumentoDigitalizado>";

				AssertContains(expectedResult, messageText);
			});
		}

		public void TestPopulateDocuments()
		{
			mockProvider.Setup(m => m.Documents).Returns((IReadOnlyCollection<IAnnexDocCommon>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ExportAnnexes;

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.AESTestFilePath, "TestAESAnnex.txt");

		protected override AnnexAESMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new AnnexAESMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override AnnexAESMessageBuilder CreateMessageBuilderWithNullProvider() => new AnnexAESMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.Message).Returns(SetUpMessage().Object);
			mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

			var mockExportOperation = new Mock<IAESCommonExportOperationMRN>();
			mockExportOperation.Setup(m => m.MRN).Returns("PRLSVNE000006");
			mockProvider.Setup(m => m.ExportOperation).Returns(mockExportOperation.Object);

			mockProvider.Setup(m => m.DispatchRequestCode).Returns("S");

			var mockAnnex1 = SetUpAnnex("1Desc", "BTI", new byte[] { 0, 1 }, "A12345678");
			var mockAnnex2 = SetUpAnnex("2Desc", "BTI", new byte[] { 0, 1 }, "A12345678");
			var mockAnnex = new IAnnexDocCommon[] { mockAnnex1.Object, mockAnnex2.Object };
			mockProvider.Setup(m => m.Documents).Returns(mockAnnex);
		}

		Mock<IAnnexDocCommon> SetUpAnnex(ZString description, ZString referenceNumber, ZBlob image, ZString extension)
		{
			var mockDoc = new Mock<IAnnexDocCommon>();
			mockDoc.Setup(m => m.Description).Returns(description);
			mockDoc.Setup(m => m.ReferenceNumber).Returns(referenceNumber);
			mockDoc.Setup(m => m.Image).Returns(image);
			mockDoc.Setup(m => m.Extension).Returns(extension);

			return mockDoc;
		}
	}
}
