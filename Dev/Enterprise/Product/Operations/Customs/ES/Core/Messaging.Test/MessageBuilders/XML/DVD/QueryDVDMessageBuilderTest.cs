using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.DVD.ConsultaDVDH2V1Ent;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(QueryDVDMessageBuilder))]
	class QueryDVDMessageBuilderTest : DVDCommonMessageBuilderTest<QueryDVDMessageBuilder, IQueryDVDMessageDataProvider, ConsultaDvdh2V1Ent>
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

		public override void TestPopulateSegmentosDeServicioIsTestFalse()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("IndicadorTest", messageText);
		}

		public void TestPopulateIndicadorDatosATCFalse()
		{
			mockProvider.Setup(m => m.RequestATCData).Returns(false);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("IndicadorDatosATC", messageText);
		}

		#endregion
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.DvdH2Query;

		protected override QueryDVDMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new QueryDVDMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override QueryDVDMessageBuilder CreateMessageBuilderWithNullProvider() => new QueryDVDMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.DVDTestFilePath, "TestQueryDVD.txt");

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.MRN).Returns("20ES00999830001277");
			mockProvider.Setup(m => m.RequestATCData).Returns(true);
		}
	}
}
