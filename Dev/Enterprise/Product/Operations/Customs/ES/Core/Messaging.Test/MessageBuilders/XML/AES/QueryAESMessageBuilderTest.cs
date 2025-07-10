using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CCAESC_v514.CCAESCV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(QueryAESMessageBuilder))]
	public class QueryAESMessageBuilderTest : AESCommonMessageBuilderTest<QueryAESMessageBuilder, IQueryAESMessageDataProvider, Ccaescv1Ent>
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

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.ExportQuery;

		protected override QueryAESMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new QueryAESMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override QueryAESMessageBuilder CreateMessageBuilderWithNullProvider() => new QueryAESMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.AESTestFilePath, "TestAESQuery.txt");

		#region Structures SetUp

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.Message).Returns(SetUpMessage().Object);
			mockProvider.Setup(m => m.IsFinalPeriod).Returns(true);
			mockProvider.Setup(m => m.PhaseIDSpecified).Returns(true);

			var mockExportOperation = new Mock<IAESCommonExportOperationMRN>();
			mockExportOperation.Setup(m => m.MRN).Returns("22ES000101100355B0");
			mockProvider.Setup(m => m.ExportOperation).Returns(mockExportOperation.Object);
		}
		#endregion
	}
}
