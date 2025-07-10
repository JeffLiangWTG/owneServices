using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.Adua.Internet.Es.Aeat.Dit.Adu.Aden.Enswsv5;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(ENSDeviationMessageBuilder))]
	public class ENSDeviationMessageBuilderTest : SummaryDeclarationsCommonMessageBuilderTest<ENSDeviationMessageBuilder, IENSDeviationMessageDataProvider, Cc323A>
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

		public void TestPopulateHeader()
		{
			mockProvider.Setup(m => m.Header).Returns((IENSDeviationHeader)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateActualEntryCustomsOffice()
		{
			mockProvider.Setup(m => m.ActualEntryCustomsOffice).Returns(ZString.Empty);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("CUSOFFENTACTOFF700", messageText);
				AssertNotContains("RefNumCUSOFFENTACTOFF701", messageText);
			});
		}

		public void TestPopulateFirstEntryCustomsOffice()
		{
			mockProvider.Setup(m => m.FirstEntryCustomsOffice).Returns(ZString.Empty);
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("CUSOFFFENT730", messageText);
				AssertNotContains("RefNumCUSOFFFENT731", messageText);
			});
		}

		public void TestPopulateRequestingTrader()
		{
			mockProvider.Setup(m => m.RequestingTrader).Returns((IENSAddressInformation)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateImportOperations()
		{
			mockProvider.Setup(m => m.ImportOperations).Returns((IReadOnlyCollection<IENSDeviationImportOperation>)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateImportOperation()
		{
			mockProvider.Setup(m => m.ImportOperations).Returns(new IENSDeviationImportOperation[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateGoodsItems()
		{
			mockImportOperation1.Setup(m => m.GoodsItems).Returns((IReadOnlyCollection<ZString>)null);
			mockProvider.Setup(m => m.ImportOperations).Returns(new[] { mockImportOperation1.Object });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			CombineAssertions(() =>
			{
				AssertNotContains("GOOITEIMP248", messageText);
				AssertNotContains("IteNumGIIMP297", messageText);
			});
		}

		public void TestPopulateLine()
		{
			mockImportOperation1.Setup(m => m.GoodsItems).Returns(new ZString[] { null });
			mockProvider.Setup(m => m.ImportOperations).Returns(new[] { mockImportOperation1.Object });
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("IteNumGIIMP297", messageText);
		}

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.EnsDeviationRequest;

		protected override ZString GetSignedMessageTestFileContent() => GetTestFileContents(
#if NETFRAMEWORK
			XMLTestFileConstants.ENSTestFilePath,
#else
			XMLTestFileConstants.ENSTestFilePath + ".net8",
#endif
			"TestENSDeviationMessageSigned.txt");
		protected override ZString GetUnsignedMessageTestFileContent() => GetTestFileContents(XMLTestFileConstants.ENSTestFilePath, "TestENSDeviationMessage.txt");

		protected override ENSDeviationMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ENSDeviationMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));
		protected override ENSDeviationMessageBuilder CreateMessageBuilderWithNullProvider() => new ENSDeviationMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override void SetUp()
		{
			base.SetUp();
			mockProvider.Setup(m => m.SenderId).Returns("89890001K");

			mockProvider.Setup(m => m.Priority).Returns("A");
			mockProvider.Setup(m => m.SenderName).Returns("Certi");
			mockProvider.Setup(m => m.ActualEntryCustomsOffice).Returns("GR001077");
			mockProvider.Setup(m => m.FirstEntryCustomsOffice).Returns("ES009999");

			var mockHeader = SetUpHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);

			var mockRequestingTrader = SetUpENSAddressInformation("89890002K", "Ave", "Acostadero eras de abajo", "Fuentelahigera de Albatages", "19024", "ES", "ES");
			mockProvider.Setup(m => m.RequestingTrader).Returns(mockRequestingTrader);

			mockImportOperation1 = SetUpImportOperation(new ZString[] { "1", "2" });
			var mockImportOperation2 = SetUpImportOperation(Enumerable.Empty<ZString>());
			mockProvider.Setup(m => m.ImportOperations).Returns(new[] { mockImportOperation1.Object, mockImportOperation2.Object });
		}

		protected Mock<IENSDeviationImportOperation> mockImportOperation1;

		Mock<IENSDeviationHeader> SetUpHeader()
		{
			var mockHeader = new Mock<IENSDeviationHeader>();
			mockHeader.Setup(m => m.BorderTransportMode).Returns("4");
			mockHeader.Setup(m => m.FirstEntryOfficeCountry).Returns("ES");
			mockHeader.Setup(m => m.InformationType).Returns("1");
			mockHeader.Setup(m => m.DeviationReferenceNumber).Returns("ES000000018");
			mockHeader.Setup(m => m.TransportId).Returns("TransportId");
			mockHeader.Setup(m => m.ExpectedArrivalDate).Returns(new ZDateTime(2020, 03, 12, 14, 55, 00));
			return mockHeader;
		}

		Mock<IENSDeviationImportOperation> SetUpImportOperation(IEnumerable<ZString> goodsItems)
		{
			var mockImportOperation = new Mock<IENSDeviationImportOperation>();
			mockImportOperation.Setup(m => m.ReferenceNumber).Returns("08ES00461170000171");
			mockImportOperation.Setup(m => m.GoodsItems).Returns((IReadOnlyCollection<ZString>)goodsItems);
			return mockImportOperation;
		}
	}
}
