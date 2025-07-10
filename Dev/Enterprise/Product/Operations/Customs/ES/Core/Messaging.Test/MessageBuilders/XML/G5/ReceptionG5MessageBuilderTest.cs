using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G5.G5RecNotifV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(ReceptionG5MessageBuilder))]
class ReceptionG5MessageBuilderTest : G5CommonMessageBuilderTest<ReceptionG5MessageBuilder, IReceptionG5MessageDataProvider, G5RecNotifV1Ent>
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

	public void TestPopulateTestIndicatorIsTestFalse()
	{
		mockProvider.Setup(m => m.IsTest).Returns(false);
		CombineAssertions(() =>
		{
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("test indicator is not set", "<TestIndicator", messageText);
			AssertContains("Recipient is correct", ">ES.AEAT</Recipient>", messageText);
		});
	}

	public void TestPopulateHeader()
	{
		mockProvider.Setup(m => m.Header).Returns((IG5CommonHeader)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateGoodsLocationWithNationalLocation()
	{
		var mockLocation = new Mock<IG5LocationGoods>();
		mockLocation.Setup(m => m.NationalLocation).Returns("ES002801ACME1");
		mockLocation.Setup(m => m.GenericLocation).Returns((IGenericLocation)null);
		mockHeader.Setup(m => m.GoodsLocationOrigin).Returns(mockLocation.Object);
		mockHeader.Setup(m => m.GoodsLocationDestination).Returns(mockLocation.Object);

		var expectedOriginResult = @"<LocationOfGoodsAtOrigin xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adds/jdit/g5/ws/G5DataGroupsV1.xsd"">
      <NationalLocation xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adds/jdit/g5/ws/G5DataElementsV1.xsd"">ES002801ACME1</NationalLocation>
    </LocationOfGoodsAtOrigin>";

		var expectedDestinationResult = @"<LocationOfGoodsAtDestination xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adds/jdit/g5/ws/G5DataGroupsV1.xsd"">
      <NationalLocation xmlns=""https://www3.agenciatributaria.gob.es/static_files/common/internet/dep/aduanas/es/aeat/adds/jdit/g5/ws/G5DataElementsV1.xsd"">ES002801ACME1</NationalLocation>
    </LocationOfGoodsAtDestination>";

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertContains("Origin", expectedOriginResult, messageText);
			AssertContains("Destination", expectedDestinationResult, messageText);
		});
	}

	public void TestPopulateGoodsLocationWithoutValue()
	{
		var mockLocation = new Mock<IG5LocationGoods>();
		mockLocation.Setup(m => m.NationalLocation).Returns(ZString.Empty);
		mockLocation.Setup(m => m.GenericLocation).Returns((IGenericLocation)null);
		mockHeader.Setup(m => m.GoodsLocationOrigin).Returns(mockLocation.Object);
		mockHeader.Setup(m => m.GoodsLocationDestination).Returns(mockLocation.Object);

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("Origin", "<LocationOfGoodsAtOrigin", messageText);
			AssertNotContains("Destination", "<LocationOfGoodsAtDestination", messageText);
		});
	}

	public void TestPopulateGoodsLocationNull()
	{
		mockHeader.Setup(m => m.GoodsLocationOrigin).Returns((IG5LocationGoods)null);
		mockHeader.Setup(m => m.GoodsLocationDestination).Returns((IG5LocationGoods)null);

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			var messageText = CreateMessageBuilder().GetSignedMessageText();
			AssertNotContains("Origin", "<LocationOfGoodsAtOrigin", messageText);
			AssertNotContains("Destination", "<LocationOfGoodsAtDestination", messageText);
		});
	}

	public void TestPopulateArrivalTransportMeans()
	{
		mockHeader.Setup(m => m.ArrivalTransportMeans).Returns((ICommonArrivalTransportMeans)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateTransportDocument()
	{
		mockHeader.Setup(m => m.TransportDocument).Returns((IDocumentsCommon)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateConsignor()
	{
		mockHeader.Setup(m => m.Consignor).Returns((IG5PartyInfo)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateConsignee()
	{
		mockHeader.Setup(m => m.Consignee).Returns((IG5PartyInfo)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateDeclarant()
	{
		mockHeader.Setup(m => m.Declarant).Returns((IG5PartyInfo)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateRepresentative()
	{
		mockHeader.Setup(m => m.Representative).Returns((IG5RepresentativeInfo)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateSupportingDocuments()
	{
		mockHeader.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateSupportingDocument()
	{
		mockHeader.Setup(m => m.SupportingDocuments).Returns(new IDocumentsCommon[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateAdditionalInfos()
	{
		mockHeader.Setup(m => m.AdditionalInfos).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateAdditionalInfo()
	{
		mockHeader.Setup(m => m.AdditionalInfos).Returns(new IDocumentsCommon[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLines()
	{
		mockProvider.Setup(m => m.Lines).Returns((IReadOnlyCollection<IG5CommonLine>)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateLine()
	{
		mockProvider.Setup(m => m.Lines).Returns(new IG5CommonLine[] { null });
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulatePreviousTSDWithTransportMeans()
	{
		mockLine1.Setup(m => m.PreviousDocument.PreviousTSD.TransportMeans).Returns(BuilderHelperTest.SetUpCommonArrivalTransportMeans("20191201ACM40302", "40"));
		mockLine1.Setup(m => m.PreviousDocument.PreviousTSD.TransportDocument).Returns((IDocumentsCommon)null);
		mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

		var expectedResult = @"</MRN>
        <TransportMeans>";

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertContains(expectedResult, CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulatePreviousTSDWithoutTransportMeansAndDocument()
	{
		mockLine1.Setup(m => m.PreviousDocument.PreviousTSD.TransportMeans).Returns((ICommonArrivalTransportMeans)null);
		mockLine1.Setup(m => m.PreviousDocument.PreviousTSD.TransportDocument).Returns((IDocumentsCommon)null);
		mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

		var expectedResult = @"</MRN>
        <GoodsItemIdentifier>";

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertContains(expectedResult, CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulatePreviousTSDNull()
	{
		mockLine1.Setup(m => m.PreviousDocument.PreviousTSD).Returns((IG5PreviousTSD)null);
		mockProvider.Setup(m => m.Lines).Returns(new[] { mockLine1.Object });

		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains("<PreviousTSD", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.G5v1Reception;

	protected override ReceptionG5MessageBuilder CreateMessageBuilder() => MockRandomGenerator(new ReceptionG5MessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override ReceptionG5MessageBuilder CreateMessageBuilderWithNullProvider() => new ReceptionG5MessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.G5TestFilePath, "TestG5ReceptionMessage.txt");

	#region Structures SetUp

	Mock<IG5CommonHeader> mockHeader;
	Mock<IG5CommonLine> mockLine1;

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.SenderId).Returns("A78587268");
		mockProvider.Setup(m => m.MRN).Returns("PRLSVNE000006");

		mockHeader = SetUpHeader();
		mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);

		var mockPackages = BuilderHelperTest.SetUpInternalPackages();
		var mockTransportDoc = BuilderHelperTest.SetUpDocument("A003", "99466889874");
		var mockContainer1 = SetUpTransportEquipment("ACME1234567", "B", new ZString[] { "SEAL1", "SEAL2" });
		var mockContainer2 = SetUpTransportEquipment("Container", ZString.Empty, Enumerable.Empty<ZString>());
		var mockSupDoc1 = BuilderHelperTest.SetUpDocument("N703", "55466889874");
		var mockSupDoc2 = BuilderHelperTest.SetUpDocument("N704", "45466889874");
		var mockAddInfo1 = BuilderHelperTest.SetUpDocument(ZString.Empty, "Cambio de ultima hora de vuelo2");
		var mockAddInfo2 = BuilderHelperTest.SetUpDocument("12345", ZString.Empty);

		mockLine1 = SetUpCommonLine("1", new[] { mockPackages, mockPackages }, null, new[] { mockContainer1, mockContainer2 }, Enumerable.Empty<IDocumentsCommon>(), new[] { mockAddInfo1, mockAddInfo2 });
		var mockLine2 = SetUpCommonLine("2", Enumerable.Empty<IInternalPackageIdentificationCommon>(), mockTransportDoc, Enumerable.Empty<IG5TransportEquipment>(), new[] { mockSupDoc1, mockSupDoc2 }, Enumerable.Empty<IDocumentsCommon>());

		var mockLines = new IG5CommonLine[] { mockLine1.Object, mockLine2.Object };

		mockProvider.Setup(m => m.Lines).Returns(mockLines);
	}

	#endregion
}
