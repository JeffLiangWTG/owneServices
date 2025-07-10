using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.NCTS.ES_CCTRAC_v515.CCTRACV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(QueryNCTSMessageBuilder))]
	public class QueryNCTSMessageBuilderTest : NCTSCommonMessageBuilderTest<QueryNCTSMessageBuilder, IQueryNCTSMessageDataProvider, Cctracv1Ent>
	{
		#region Test

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

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.TransitNcts5Query;

		protected override QueryNCTSMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new QueryNCTSMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override QueryNCTSMessageBuilder CreateMessageBuilderWithNullProvider() => new QueryNCTSMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(NCTSXMLTestFileConstants.NCTSTestFilePath, "TestQueryNCTS.txt");

		#region Structures SetUp

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MessageSender).Returns("A78587268");
			mockProvider.Setup(m => m.MessageIdentification).Returns("20221213141220798000");
			mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

			var mockTransitOperation = SetUpTransitOperationMRN();
			mockProvider.Setup(m => m.TransitOperation).Returns(mockTransitOperation.Object);
		}

		#endregion
	}
}
