using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEJECV1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(PresentationT2LMessageBuilder))]
	class PresentationT2LMessageBuilderTest : T2LPOUSCommonMessageBuilderTest<PresentationT2LMessageBuilder, IPresentationT2LMessageDataProvider, IejecType>
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

		public void TestPopulatePresentationCustomsOffice()
		{
			mockProvider.Setup(m => m.CustomsOffice).Returns(ZString.Empty);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}PresentationCustomsOffice>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateSendEmailL()
		{
			mockProvider.Setup(m => m.SendEmailL).Returns(ZString.Empty);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}SendEmailL>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulatePreviousMRN()
		{
			mockProvider.Setup(m => m.PreviousMRN).Returns(ZString.Empty);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}MRNT2LT2LF>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulatePersonPresentingProof()
		{
			mockProvider.Setup(m => m.PersonReqPres).Returns((IT2LPOUSCommonPersonReqPresWithAddress)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}PersonPresentingProof>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateAddress()
		{
			mockProvider.Setup(m => m.PersonReqPres.Address).Returns((IPartyAddressProvider)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}Address>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateContactPerson()
		{
			mockProvider.Setup(m => m.PersonReqPres.ContactPerson).Returns((IPartyContactProvider)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}ContactPersonInformation>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateLocationOfGoods()
		{
			mockProvider.Setup(m => m.LocationOfGoods).Returns(ZString.Empty);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}LocationOfGoods>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateContainerIndication()
		{
			mockProvider.Setup(m => m.ContainerIndication).Returns((IT2LPOUSCommonContainerIndicator)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertContains($"<{XMLTestFileConstants.XmlElementNamespace}ContainerIndication>0</{XMLTestFileConstants.XmlElementNamespace}ContainerIndication>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateTransportEquipment()
		{
			mockProvider.Setup(m => m.TransportEquipment).Returns((IReadOnlyCollection<IT2LPOUSTransportEquipment>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}TransportEquipment>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulateGoodItem()
		{
			mockProvider.Setup(m => m.GoodItems).Returns((IReadOnlyCollection<IT2LPOUSPresentationGoodItem>)null);
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}GoodsShipmentItem>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulatePreviousDocument()
		{
			mockItem1.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IT2LPOUSPresentationPreviousDocument>)null);
			mockProvider.Setup(m => m.GoodItems).Returns(new IT2LPOUSPresentationGoodItem[] { mockItem1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}PreviousDocument>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		public void TestPopulatePackaging()
		{
			mockPrevDoc1.Setup(m => m.Packaging).Returns((IT2LPOUSCommonPackaging)null);
			mockItem1.Setup(m => m.PreviousDocuments).Returns(new IT2LPOUSPresentationPreviousDocument[] { mockPrevDoc1.Object });
			mockProvider.Setup(m => m.GoodItems).Returns(new IT2LPOUSPresentationGoodItem[] { mockItem1.Object });
			CombineAssertions(() =>
			{
				AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}TypeOfPackages>", CreateMessageBuilder().GetSignedMessageText());
				AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}NumberOfPackages>", CreateMessageBuilder().GetSignedMessageText());
			});
		}

		#endregion

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.T2lPresentationPous;

		protected override PresentationT2LMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new PresentationT2LMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override PresentationT2LMessageBuilder CreateMessageBuilderWithNullProvider() => new PresentationT2LMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.T2LPOUSTestFilePath, "TestT2LPOUSPresentation.txt");

		#region Structures SetUp

		Mock<IT2LPOUSPresentationPreviousDocument> mockPrevDoc1;
		Mock<IT2LPOUSPresentationGoodItem> mockItem1;

		protected override void SetUp()
		{
			base.SetUp();

			mockProvider.Setup(m => m.CustomsOffice).Returns("ES002801");
			mockProvider.Setup(m => m.SendEmailL).Returns("S");

			mockProvider.Setup(m => m.PreviousMRN).Returns("23ES002801L00009M2");
			mockProvider.Setup(m => m.PersonReqPres).Returns(SetUpPersonReqPresCommonWithAddress().Object);
			mockProvider.Setup(m => m.LocationOfGoods).Returns("28013M0000");
			mockProvider.Setup(m => m.ContainerIndication.IsContainerised).Returns(ZBool.True);

			var mockTransportEquipment1 = SetUpTransportEquipment("Container1", new ZInt[] { 1, 2 });
			var mockTransportEquipment2 = SetUpTransportEquipment("Container2", Enumerable.Empty<ZInt>());
			var mockTransportEquipment3 = SetUpTransportEquipment("Container3", new ZInt[] { 0 });
			var mockTransportEquipment = new IT2LPOUSTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object, mockTransportEquipment3.Object };
			mockProvider.Setup(m => m.TransportEquipment).Returns(mockTransportEquipment);

			mockPrevDoc1 = SetUpPreviousDocument("N337", "22ES00280180102186", "1A", 1, true, "KGMG", 1.5m, true, 1);
			var mockPrevDoc2 = SetUpPreviousDocument("N338", "22ES00280180102187", "BX", 2, false, "UDF", 2m, false, 2);
			var mockPrevDocs = new IT2LPOUSPresentationPreviousDocument[] { mockPrevDoc1.Object, mockPrevDoc2.Object };

			mockItem1 = SetUpGoodItem(1, mockPrevDocs, 1);
			var mockItem2 = SetUpGoodItem(2, Enumerable.Empty<IT2LPOUSPresentationPreviousDocument>(), 2);
			var mockItems = new IT2LPOUSPresentationGoodItem[] { mockItem1.Object, mockItem2.Object };

			mockProvider.Setup(m => m.GoodItems).Returns(mockItems);
		}

		Mock<IT2LPOUSPresentationGoodItem> SetUpGoodItem(ZInt itemNumber, IEnumerable<IT2LPOUSPresentationPreviousDocument> prevDocs, ZInt itemNumberT2L)
		{
			var mockItem = new Mock<IT2LPOUSPresentationGoodItem>();
			mockItem.Setup(m => m.GoodsItemNumber).Returns(itemNumber);
			mockItem.Setup(m => m.PreviousDocuments).Returns((IReadOnlyCollection<IT2LPOUSPresentationPreviousDocument>)prevDocs);
			mockItem.Setup(m => m.T2LT2LFgoodsItemNumber).Returns(itemNumberT2L);
			return mockItem;
		}

		Mock<IT2LPOUSPresentationPreviousDocument> SetUpPreviousDocument(ZString type, ZString reference, ZString packageType, ZInt numberOfPackages, ZBool numberOfPackagesValue, ZString measurement, ZDecimal quantity, ZBool quantityValueSpecified, ZInt goodItemId)
		{
			var mockPreviousDoc = new Mock<IT2LPOUSPresentationPreviousDocument>();
			mockPreviousDoc.Setup(m => m.Name).Returns(type);
			mockPreviousDoc.Setup(m => m.Number).Returns(reference);
			mockPreviousDoc.Setup(m => m.Packaging.TypeOfPackages).Returns(packageType);
			mockPreviousDoc.Setup(m => m.Packaging.NumberOfPackages).Returns(numberOfPackages);
			mockPreviousDoc.Setup(m => m.Packaging.NumberOfPackagesValueSpecified).Returns(numberOfPackagesValue);
			mockPreviousDoc.Setup(m => m.MeasurementUnitAndQualifier).Returns(measurement);
			mockPreviousDoc.Setup(m => m.Quantity).Returns(quantity);
			mockPreviousDoc.Setup(m => m.QuantityValueSpecified).Returns(quantityValueSpecified);
			mockPreviousDoc.Setup(m => m.GoodsItemIdentifier).Returns(goodItemId);
			return mockPreviousDoc;
		}

		#endregion
	}
}
