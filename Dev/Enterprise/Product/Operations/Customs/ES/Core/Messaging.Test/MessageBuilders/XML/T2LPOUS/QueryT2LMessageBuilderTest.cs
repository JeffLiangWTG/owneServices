using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01CONSV1Ent;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(QueryT2LMessageBuilder))]
	class QueryT2LMessageBuilderTest : XMLMessageBuilderTest<QueryT2LMessageBuilder, IQueryT2LMessageDataProvider, Iep01ConsEntType>
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

		public void TestPopulateSegmentosDeServicioIsTestFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}testIndicator>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulatePreviousMRN()
		{
			mockProvider.Setup(m => m.MRN).Returns(ZString.Empty);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}MRNT2L>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.T2lQueryPous;

		protected override QueryT2LMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new QueryT2LMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override QueryT2LMessageBuilder CreateMessageBuilderWithNullProvider() => new QueryT2LMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
		protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
		protected ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.T2LPOUSTestFilePath, "TestT2LPOUSQuery.txt");

		#region Structures SetUp

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MRN).Returns("23ES002801L00009M2");
		}

		#endregion
	}
}
