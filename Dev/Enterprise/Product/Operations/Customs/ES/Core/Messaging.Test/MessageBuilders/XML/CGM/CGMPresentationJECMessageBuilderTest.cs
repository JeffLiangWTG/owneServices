using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.CGM.CGMEJECV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(CGMPresentationJECMessageBuilder))]
sealed class CGMPresentationJECMessageBuilderTest : CGMCommonMessageBuilderTest<CGMPresentationJECMessageBuilder, ICGMPresentationJECMessageDataProvider, Cgmejecv1EntType>
{
	#region Tests
	public void TestPopulateSendEmailL()
	{
		mockProvider.Setup(m => m.SendEmailL).Returns(ZString.Empty);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}SendEmailL>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulatePersonPresentingGoodsToCustomsForCGM()
	{
		mockProvider.Setup(m => m.PersonPresentingGoodsToCustomsForCGM).Returns((ICGMPartyProviderWithAddressAndContactPerson)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateRepresentativeAtArrivalForCGM()
	{
		mockProvider.Setup(m => m.RepresentativeAtArrivalForCGM).Returns((ICGMPartyProviderWithAddressAndContactPerson)null);
		AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
	}

	public void TestPopulateTransportEquipments()
	{
		mockProvider.Setup(m => m.TransportEquipments).Returns((IReadOnlyCollection<ICGMPresentationTransportEquipment>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}TransportEquipment>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateGoodsItems()
	{
		mockProvider.Setup(m => m.GoodsItems).Returns((IReadOnlyCollection<ICGMPresentationGoodsItem>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}PresentationGoodsItems>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.CgmPresentationJec;

	protected override CGMPresentationJECMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new CGMPresentationJECMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override CGMPresentationJECMessageBuilder CreateMessageBuilderWithNullProvider() => new CGMPresentationJECMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.CGMTestFilePath, "TestCGMPresentationJEC.txt");

	#region Structures SetUp

	protected override void SetUp()
	{
		base.SetUp();

		mockProvider.Setup(m => m.SendEmailL).Returns("S");
		mockProvider.Setup(m => m.PresentationCustomsOffice).Returns("ES000101");
		mockProvider.Setup(m => m.MRN).Returns("PRLSVNE000006");

		mockProvider.Setup(m => m.PersonPresentingGoodsToCustomsForCGM).Returns(SetUpCGMPartyProviderWithAddressAndContactPerson("12345678A", "NORON EHF", "SKUTUVOGUR 7", "104 REYKJAVIK", "40025", "IS", "Prometeo", "test_email@taric.es", "616123443"));
		mockProvider.Setup(m => m.RepresentativeAtArrivalForCGM).Returns(SetUpCGMPartyProviderWithAddressAndContactPerson("89890001K", "Lobo Lobate", "Lobera de Valdelacierva", "Por poco en Vinuelas", "19069", "ES", "Prometeo2", "test_email2@taric.es", "616123886"));

		mockProvider.Setup(m => m.IsContainerised).Returns(true);

		var mockTransportEquipment1 = SetUpTransportEquipment("HXDU1234567", [1, 2, 3]);
		var mockTransportEquipment2 = SetUpTransportEquipment("HXDU1234589", Enumerable.Empty<ZInt>());
		var mockTransportEquipments = new ICGMPresentationTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object };
		mockProvider.Setup(m => m.TransportEquipments).Returns(mockTransportEquipments);

		var mockPrevDoc1 = SetUpPreviousDocument("N337", "22ES00280180102186", "1A", 1, "KGMG", 1.5m, 1);
		var mockPrevDoc2 = SetUpPreviousDocument("N338", "22ES00280180102187", "BX", 2, "UDF", 3m, 2);
		var mockPrevDocs = new ICGMPresentationPreviousDocument[] { mockPrevDoc1.Object, mockPrevDoc2.Object };

		var mockGoodsItem1 = SetUpGoodsItem(1, mockPrevDocs, 1);
		var mockGoodsItem2 = SetUpGoodsItem(2, Enumerable.Empty<ICGMPresentationPreviousDocument>(), 2);
		var mockGoodsItems = new ICGMPresentationGoodsItem[] { mockGoodsItem1.Object, mockGoodsItem2.Object };
		mockProvider.Setup(m => m.GoodsItems).Returns(mockGoodsItems);
	}

	Mock<ICGMPresentationTransportEquipment> SetUpTransportEquipment(ZString containerNumber, IEnumerable<ZInt> goodsReferences)
	{
		var mockTransportEquipment = new Mock<ICGMPresentationTransportEquipment>();
		mockTransportEquipment.Setup(m => m.ContainerNumber).Returns(containerNumber);
		mockTransportEquipment.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<ZInt>)goodsReferences);

		return mockTransportEquipment;
	}

	Mock<ICGMPresentationGoodsItem> SetUpGoodsItem(ZInt itemNumber, IEnumerable<ICGMPresentationPreviousDocument> prevDocs, ZInt itemNumberT2L)
	{
		var mockItem = new Mock<ICGMPresentationGoodsItem>();
		mockItem.Setup(m => m.GoodsItemNumber).Returns(itemNumber);
		mockItem.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<ICGMPresentationPreviousDocument>)prevDocs);
		mockItem.Setup(m => m.T2LT2LFgoodsItemNumber).Returns(itemNumberT2L);
		return mockItem;
	}

	Mock<ICGMPresentationPreviousDocument> SetUpPreviousDocument(ZString type, ZString reference, ZString packageType, ZInt numberOfPackages, ZString measurement, ZDecimal quantity, ZInt goodItemId)
	{
		var mockPreviousDoc = new Mock<ICGMPresentationPreviousDocument>();
		mockPreviousDoc.Setup(m => m.Name).Returns(type);
		mockPreviousDoc.Setup(m => m.Number).Returns(reference);
		mockPreviousDoc.Setup(m => m.TypeOfPackages).Returns(packageType);
		mockPreviousDoc.Setup(m => m.NumberOfPackages).Returns(numberOfPackages);
		mockPreviousDoc.Setup(m => m.MeasurementUnitAndQualifier).Returns(measurement);
		mockPreviousDoc.Setup(m => m.Quantity).Returns(quantity);
		mockPreviousDoc.Setup(m => m.GoodsItemId).Returns(goodItemId);
		return mockPreviousDoc;
	}

	#endregion
}
