using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CC014C_v515.CC014CV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(CancelNCTSMessageBuilder))]
	class CancelNCTSMessageBuilderTest : NCTSCommonMessageBuilderTest<CancelNCTSMessageBuilder, ICancelNCTSMessageDataProvider, Cc014Cv1Ent>
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

		public void TestPopulateInvalidation()
		{
			mockProvider.Setup(m => m.Invalidation).Returns((ICancelNCTSInvalidation)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}Invalidation>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateCustomsOfficeOfDeparture()
		{
			mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("");
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}CustomsOfficeOfDeparture>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateHolderOfTheTransitProcedure()
		{
			mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns((INCTSCommonHolderOfTheTransitProcedure)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{NCTSXMLTestFileConstants.XmlElementNamespace}HolderOfTheTransitProcedure>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.Ncts5DepartureCancellation;

		protected override CancelNCTSMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new CancelNCTSMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override CancelNCTSMessageBuilder CreateMessageBuilderWithNullProvider() => new CancelNCTSMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(NCTSXMLTestFileConstants.NCTSTestFilePath, "TestCancelNCTS.txt");

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MessageSender).Returns("A78587268");
			mockProvider.Setup(m => m.MessageIdentification).Returns("20221213141220798000");
			mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

			var mockTransitOperation = SetUpTransitOperationMRN();
			mockProvider.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);

			var mockInvalidation = new Mock<ICancelNCTSInvalidation>();
			mockInvalidation.Setup(m => m.IsInitiatedByCustoms).Returns(false);
			mockInvalidation.Setup(m => m.Justification).Returns("Justificacion de anulacion");
			mockProvider.Setup(m => m.Invalidation).Returns(mockInvalidation.Object);

			mockProvider.Setup(m => m.CustomsOfficeOfDeparture).Returns("ES000101");

			mockProvider.Setup(m => m.HolderOfTheTransitProcedure).Returns(SetUpCommonHolderOfTheTransitProcedure().Object);
		}
	}
}
