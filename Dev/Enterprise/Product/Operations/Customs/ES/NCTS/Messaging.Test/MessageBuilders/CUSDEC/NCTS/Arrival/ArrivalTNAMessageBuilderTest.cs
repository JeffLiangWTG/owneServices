using System;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ArrivalMessageBuilder))]
	public class ArrivalTNAMessageBuilderTest : ArrivalBaseMessageBuilderTest
	{
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureTnnPlusAvi;

		protected override ZBool IsOnlyArrivalNotification => false;

		protected override ZBool IsOnlyUnloadingRemarks => false;

		protected override ZBool IsArrivalWithAVI => true;

		protected override ZBool IsArrivalWithOBS => false;

		protected override ZBool IsArrivalWithTNN => true;

		protected override ZString GetTestFile() => GetTestFileContents(NCTSTestFileConstants.TestFilePath, "TestArrivalTNAMessage.txt");

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

		public override void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory1()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory1).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1++T1:116:141'", messageText);
		}

		public override void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory2()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory2).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+88033090:122:148'", messageText);
		}

		public override void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory3()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory3).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+88033090:122:148+T1:116:141'", messageText);
		}

		public override void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory4()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory4).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+88033090:122:148+T1:116:141'", messageText);
		}

		public override void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory5()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory5).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+88033090:122:148+T1:116:141'", messageText);
		}

		[TestDate(2020, 3, 9, 16, 13, 23, 456)]
		public override void TestUNBNotTest()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns("1210244B");
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("UNB+UNOA:1+1210244B:ZZ+AEATADUE:ZZ+200309:1613+<<MSGNO PLACEHOLDER>>++&EE'", messageText);
		}
	}
}
