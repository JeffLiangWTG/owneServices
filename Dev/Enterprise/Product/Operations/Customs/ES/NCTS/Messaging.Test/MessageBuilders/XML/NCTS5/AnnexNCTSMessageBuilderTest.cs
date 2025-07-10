using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCDOTC_v515.CCDOTCV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(AnnexNCTSMessageBuilder))]
	class AnnexNCTSMessageBuilderTest : NCTSCommonMessageBuilderTest<AnnexNCTSMessageBuilder, IAnnexNCTSMessageDataProvider, Ccdotcv1Ent>
	{
		#region Tests
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

		public void TestPopulatePhaseID()
		{
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("PhaseID", messageText);
		}

		public void TestPopulateTransitOperation()
		{
			mockProvider.Setup(m => m.TransitOperation).Returns((INCTSCommonTransitOperationMRN)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}TransitOperation>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateDispatchRequestCode()
		{
			mockProvider.Setup(m => m.DispatchRequestCode).Returns("");
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}SolicitudDespacho>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateAuthorisation()
		{
			mockProvider.Setup(m => m.Documents).Returns((IReadOnlyCollection<IAnnexDocCommon>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}DocumentoDigitalizado>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Ncts5DepartureAnnexes;

		protected override AnnexNCTSMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new AnnexNCTSMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override AnnexNCTSMessageBuilder CreateMessageBuilderWithNullProvider() => new AnnexNCTSMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(NCTSXMLTestFileConstants.NCTSTestFilePath, "TestAnnexNCTS.txt");

		#region Structures SetUp

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MessageSender).Returns("A78587268");
			mockProvider.Setup(m => m.MessageIdentification).Returns("20221213141220798000");
			mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);
			mockProvider.Setup(m => m.TransitOperation).Returns(SetUpTransitOperationMRN().Object);

			mockProvider.Setup(m => m.DispatchRequestCode).Returns("S");
			var mockAnnex1 = SetUpAnnex("1Desc", "BTI", new byte[] { 0, 1 }, "PDF");
			var mockAnnex2 = SetUpAnnex("2Desc", "BTI", new byte[] { 0, 1 }, "PDF");
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
		#endregion
	}
}
