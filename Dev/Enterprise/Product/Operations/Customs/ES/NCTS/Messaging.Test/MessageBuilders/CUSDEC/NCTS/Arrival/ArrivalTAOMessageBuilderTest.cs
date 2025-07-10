using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging;
using Enterprise.Customs.ES.Messaging.MessageBuilders;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;
using NUnit.Framework;

namespace Enterprise.Customs.ES.NCTS.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ArrivalMessageBuilder))]
	public class ArrivalTAOMessageBuilderTest : ArrivalBaseMessageBuilderTest
	{
		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.NctsArrivalNotificationWithDepartureAndUnloadingRemarksTnnPlusAviPlusOb;

		protected override ZBool IsOnlyArrivalNotification => false;

		protected override ZBool IsOnlyUnloadingRemarks => false;

		protected override ZBool IsArrivalWithAVI => true;

		protected override ZBool IsArrivalWithOBS => true;

		protected override ZBool IsArrivalWithTNN => true;

		protected override ZString GetTestFile() => GetTestFileContents(NCTSTestFileConstants.TestFilePath, "TestArrivalTAOMessage.txt");

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
			AssertContains("CST+1++T1:116:141+M3:ZZZ:148+HB12345678901+12345678'", messageText);
		}

		public override void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory2()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory2).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+88033090:122:148++M3:ZZZ:148+HB12345678901+12345678'", messageText);
		}

		public override void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory3()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory3).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+88033090:122:148+T1:116:141++HB12345678901+12345678'", messageText);
		}

		public override void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory4()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory4).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+88033090:122:148+T1:116:141+M3:ZZZ:148++12345678'", messageText);
		}

		public override void TestAddCSTSegmentOnSG30_NoGoodsCustomsProcedureCategory5()
		{
			mockLine1.Setup(m => m.GoodsCustomsProcedureCategory5).Returns(ZString.Empty);
			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("CST+1+88033090:122:148+T1:116:141+M3:ZZZ:148+HB12345678901'", messageText);
		}

		[TestDate(2020, 3, 9, 16, 13, 23, 456)]
		public override void TestUNBNotTest()
		{
			mockProvider.Setup(m => m.IsTest).Returns(false);
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns("1210244B");
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("UNB+UNOA:1+1210244B:ZZ+AEATADUE:ZZ+200309:1613+<<MSGNO PLACEHOLDER>>++&EE'", messageText);
		}

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider.Setup(m => m.DeclarantIdForUNBSegment).Returns(DeclarantIdForUNBSegment);

			var mockPackages = BuilderHelperTest.SetUpInternalPackages();

			var mockLine1 = SetUpLine(1, ZString.Empty, "M3", "HB12345678901", "12345678", new ZString[] { "3", "6" }, new[] { mockPackages });
			var mockDocument1 = BuilderHelperTest.SetUpDocument("380", "187");
			var mockDocument2 = BuilderHelperTest.SetUpDocument("X001", "ES3600000002");
			mockLine1.Setup(m => m.Documents).Returns(new[] { mockDocument1, mockDocument2 });

			var mockLine2 = SetUpLine(2, "T1", ZString.Empty, ZString.Empty, ZString.Empty, Array.Empty<ZString>(), new[] { mockPackages, mockPackages });
			var mockExternalPackages = BuilderHelperTest.SetUpExternalPackages(2, new ZString[] { "CONTEN1111", "CONTEN2222", "CONTEN3333" });
			mockLine2.Setup(m => m.ExternalPackages).Returns(mockExternalPackages);
			mockLine2.Setup(m => m.Documents).Returns((IReadOnlyCollection<IDocumentsCommon>)Enumerable.Empty<IDocumentsCommon>());

			mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object, mockLine2.Object });
		}
	}
}
