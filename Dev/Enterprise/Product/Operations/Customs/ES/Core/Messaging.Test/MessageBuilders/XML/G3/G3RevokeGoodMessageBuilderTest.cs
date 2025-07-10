using System;
using CargoWise.Customs.ES.MessageDefinitions.Version1.G3.G3RevokeV1Ent;
using CargoWise.Types;
using Enterprise.Customs.ES.Messaging.MessageBuilders.Interface.G3;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing
{
	[TestedType(typeof(G3RevokeGoodMessageBuilder))]
	class G3RevokeGoodMessageBuilderTest : G3CommonMessageBuilderTest<G3RevokeGoodMessageBuilder, IG3RevokeGoodMessageDataProvider, G3RevokeV1Ent>
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
			mockProvider.Setup(m => m.Header).Returns((IG3RevokeGoodHeader)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateDeclarant()
		{
			mockHeader.Setup(m => m.Declarant).Returns((IG3Declarant)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateRepresentative()
		{
			mockHeader.Setup(m => m.Representative).Returns((IG3Representative)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateFullAddress()
		{
			var mockDeclarant = new Mock<IG3Declarant>();
			mockDeclarant.Setup(m => m.IdNumber).Returns("ESA78587268");
			mockDeclarant.Setup(m => m.Name).Returns("TARIC, S.A.U.");

			mockDeclarant.Setup(m => m.FullAddress).Returns((IG3FullAddress)null);
			var communication = SetupCommunication();
			mockDeclarant.Setup(m => m.Communication).Returns(communication);

			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant.Object);

			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateCommunication()
		{
			var mockDeclarant = new Mock<IG3Declarant>();
			mockDeclarant.Setup(m => m.IdNumber).Returns("ESA78587268");
			mockDeclarant.Setup(m => m.Name).Returns("TARIC, S.A.U.");

			var fullAddress = SetupFullAddress();
			mockDeclarant.Setup(m => m.FullAddress).Returns(fullAddress);
			mockDeclarant.Setup(m => m.Communication).Returns((IG3Communication)null);

			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant.Object);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateLocationOfGoods()
		{
			mockMasterConsignment.Setup(m => m.LocationOfGoods).Returns((IGenericLocation)null);
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateMasterConsignment()
		{
			mockHeader.Setup(m => m.MasterConsignment).Returns(new IG3RevokeMasterConsignment[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		public void TestPopulateHouseConsignment()
		{
			mockMasterConsignment.Setup(m => m.HouseConsignment).Returns(new IG3RevokeHouseConsignment[] { null });
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
		}

		protected override void SetUp()
		{
			base.SetUp();

			mockHeader = SetupHeader();
			mockProvider.Setup(m => m.Header).Returns(mockHeader.Object);
			var message = SetupMessage();
			mockProvider.Setup(m => m.Message).Returns(message);

			mockMasterConsignment = new Mock<IG3RevokeMasterConsignment>();
			mockHouseConsignment = new Mock<IG3RevokeHouseConsignment>();

			var prevDoc = BuilderHelperTest.SetupDocumentWithGoodItemId("337", "99981000348", "2");
			var prevDoc2 = BuilderHelperTest.SetupDocumentWithGoodItemId("338", "99981000349", "3");
			var mockTransportDoc = BuilderHelperTest.SetUpDocument("5025", "WTG_20241216_2");
			var mockTransportDoc2 = BuilderHelperTest.SetUpDocument("5026", "WTG_20241216_3");
			var mockAddInfo = BuilderHelperTest.SetUpDocument("AddInfoDoc", "3");
			var mockGenericLocation = BuilderHelperTest.SetUpGenericLocation("A", "V");

			mockHouseConsignment.Setup(m => m.PreviousDocument).Returns(new ICommonDocumentGoodsItemId[] { prevDoc });
			mockHouseConsignment.Setup(m => m.TransportDocument).Returns(mockTransportDoc);
			mockHouseConsignment.Setup(m => m.AdditionalInformation).Returns(mockAddInfo);

			mockMasterConsignment.Setup(m => m.PreviousDocument).Returns(new ICommonDocumentGoodsItemId[] { prevDoc2 });
			mockMasterConsignment.Setup(m => m.TransportDocument).Returns(mockTransportDoc2);
			mockMasterConsignment.Setup(m => m.Receptacle).Returns("1234657812");
			mockMasterConsignment.Setup(m => m.LocationOfGoods).Returns(mockGenericLocation);
			mockMasterConsignment.Setup(m => m.TransportEquipmentContainers).Returns(new ZString[] { "CON1" });
			mockMasterConsignment.Setup(m => m.HouseConsignment).Returns(new IG3RevokeHouseConsignment[] { mockHouseConsignment.Object });

			mockHeader.Setup(m => m.MasterConsignment).Returns(new IG3RevokeMasterConsignment[] { mockMasterConsignment.Object });
		}

		protected Mock<IG3RevokeGoodHeader> SetupHeader()
		{
			var mockHeader = new Mock<IG3RevokeGoodHeader>();

			mockHeader.Setup(m => m.LRN).Returns("G300000152");
			mockHeader.Setup(m => m.CustomsOffice).Returns("ES001900");
			mockHeader.Setup(m => m.PersonPresentingGoods).Returns("ESA78587268");

			var mockDeclarant = SetupDeclarant();
			mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);

			var mockRepresentative = SetupRepresentative();
			mockHeader.Setup(m => m.Representative).Returns(mockRepresentative);

			mockHeader.Setup(m => m.DeclarationDate).Returns(new ZDateTime(2020, 1, 9, 15, 13, 23, 456));
			mockHeader.Setup(m => m.PresentationDate).Returns(new ZDateTime(2020, 1, 9, 15, 13, 23, 456));

			return mockHeader;
		}

		Mock<IG3RevokeGoodHeader> mockHeader;
		Mock<IG3RevokeMasterConsignment> mockMasterConsignment;
		Mock<IG3RevokeHouseConsignment> mockHouseConsignment;

		protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.G3RevocationOfGoods;

		protected override G3RevokeGoodMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new G3RevokeGoodMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

		protected override G3RevokeGoodMessageBuilder CreateMessageBuilderWithNullProvider() => new G3RevokeGoodMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

		protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.G3TestFilePath, "TestG3RevokeGoodMessage.txt");
	}
}
