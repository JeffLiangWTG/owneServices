using System;
using System.Collections.Generic;
using CargoWise.Customs.ES.MessageDefinitions.Version1.T2LPOUS.CCIEP01V1Ent;
using CargoWise.Types;
using Moq;
using NUnit.Framework;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

[TestedType(typeof(RequestT2LMessageBuilder))]
class RequestT2LMessageBuilderTest : T2LPOUSCommonMessageBuilderTest<RequestT2LMessageBuilder, IRequestT2LMessageDataProvider, Iep01Type>
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

	public void TestPopulateProofOperationInformationForT2LT2LF()
	{
		mockProvider.Setup(m => m.ProofOperationInformationForT2LT2LF).Returns((IT2LPOUSRequestProofOperationInformationForT2LT2LF)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}ProofOperationInformationForT2LT2LF>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateRequestedValidityOfTheProof()
	{
		mockProvider.Setup(m => m.ProofOperationInformationForT2LT2LF.RequestedValidityOfTheProof).Returns((IT2LPOUSRequestedValidityOfTheProof)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}RequestedValididtyOfTheProof>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateAuthorisation()
	{
		mockProvider.Setup(m => m.Authorisation).Returns((IT2LPOUSAuthorisation)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}Authorisation>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulatePersonRequestingProof()
	{
		mockProvider.Setup(m => m.PersonReqPres).Returns((IT2LPOUSCommonPersonReqPresWithAddress)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}PersonRequestingProof>", CreateMessageBuilder().GetSignedMessageText());
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

	public void TestPopulateRepresentative()
	{
		mockProvider.Setup(m => m.Representative).Returns((IT2LPOUSCommonPersonReqPres)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}Representative>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateContactPerson()
	{
		mockProvider.Setup(m => m.PersonReqPres.ContactPerson).Returns((IPartyContactProvider)null);
		mockProvider.Setup(m => m.Representative.ContactPerson).Returns((IPartyContactProvider)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}ContactPersonInformation>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateCompetentCustomsOffice()
	{
		mockProvider.Setup(m => m.CustomsOffice).Returns(ZString.Empty);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}CompetentCustomsOffice>", CreateMessageBuilder().GetSignedMessageText());
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

	public void TestPopulateSendEmailU()
	{
		mockProvider.Setup(m => m.SendEmailU).Returns(ZString.Empty);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}SendEmailU>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateSendEmailExp()
	{
		mockProvider.Setup(m => m.SendEmailExp).Returns(ZString.Empty);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}SendEmailExp>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateGoodsShipment()
	{
		mockProvider.Setup(m => m.GoodsShipment).Returns((IT2LPOUSGoodsShipment)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}GoodsShipmentForT2LT2LF>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateContainerIndication()
	{
		mockProvider.Setup(m => m.GoodsShipment.ContainerIndication).Returns((IT2LPOUSCommonContainerIndicator)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertContains($"<{XMLTestFileConstants.XmlElementNamespace}ContainerIndication>0</{XMLTestFileConstants.XmlElementNamespace}ContainerIndication>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateTransportEquipment()
	{
		mockProvider.Setup(m => m.GoodsShipment.TransportEquipment).Returns((IReadOnlyCollection<IT2LPOUSTransportEquipment>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}TransportEquipment>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateAdditionalInformation()
	{
		mockProvider.Setup(m => m.GoodsShipment.AdditionalInformation).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		mockItem1.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		mockProvider.Setup(m => m.GoodsShipment.GoodItems).Returns(new IT2LPOUSRequestAndReceptionGoodItem[] { mockItem1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}AdditionalInformation>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulatePreviousDocument()
	{
		mockProvider.Setup(m => m.GoodsShipment.PreviousDocument).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		mockItem1.Setup(m => m.PreviousDocument).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		mockProvider.Setup(m => m.GoodsShipment.GoodItems).Returns(new IT2LPOUSRequestAndReceptionGoodItem[] { mockItem1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}PreviousDocument>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateSupportingDocument()
	{
		mockProvider.Setup(m => m.GoodsShipment.SupportingDocument).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		mockItem1.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		mockProvider.Setup(m => m.GoodsShipment.GoodItems).Returns(new IT2LPOUSRequestAndReceptionGoodItem[] { mockItem1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}SupportingDocument>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateTransportDocument()
	{
		mockProvider.Setup(m => m.GoodsShipment.TransportDocument).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}TransportDocument>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateAdditionalReference()
	{
		mockProvider.Setup(m => m.GoodsShipment.AdditionalReference).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		mockItem1.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<IDocumentsCommon>)null);
		mockProvider.Setup(m => m.GoodsShipment.GoodItems).Returns(new IT2LPOUSRequestAndReceptionGoodItem[] { mockItem1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}AdditionalReference>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateGoodItem()
	{
		mockProvider.Setup(m => m.GoodsShipment.GoodItems).Returns((IReadOnlyCollection<IT2LPOUSRequestAndReceptionGoodItem>)null);
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}GoodsShipmentItem>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateCommodityCode()
	{
		mockItem1.Setup(m => m.CommodityCode).Returns((ICommodityCodeCommon)null);
		mockProvider.Setup(m => m.GoodsShipment.GoodItems).Returns(new IT2LPOUSRequestAndReceptionGoodItem[] { mockItem1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}CommodityCode>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulateGoodsMeasure()
	{
		mockItem1.Setup(m => m.GoodsMeasure).Returns((IGoodsMeasureCommon)null);
		mockProvider.Setup(m => m.GoodsShipment.GoodItems).Returns(new IT2LPOUSRequestAndReceptionGoodItem[] { mockItem1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}GoodsMeasure>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	public void TestPopulatePackaging()
	{
		mockItem1.Setup(m => m.Package).Returns((IReadOnlyCollection<IT2LPOUSRequestAndReceptionPackaging>)null);
		mockProvider.Setup(m => m.GoodsShipment.GoodItems).Returns(new IT2LPOUSRequestAndReceptionGoodItem[] { mockItem1.Object });
		CombineAssertions(() =>
		{
			AssertNoExceptionThrown(() => CreateMessageBuilder().GetSignedMessageText());
			AssertNotContains($"<{XMLTestFileConstants.XmlElementNamespace}Packaging>", CreateMessageBuilder().GetSignedMessageText());
		});
	}

	#endregion

	protected override ZString ExpectedMessageType => DeclarationMessageTypeList.Codes.T2lRequestPous;

	protected override RequestT2LMessageBuilder CreateMessageBuilder() => MockRandomGenerator(new RequestT2LMessageBuilder(mockProvider.Object, ExpectedMessageType, ExpectedMessageSubType));

	protected override RequestT2LMessageBuilder CreateMessageBuilderWithNullProvider() => new RequestT2LMessageBuilder(default, ExpectedMessageType, ExpectedMessageSubType);

	protected override ZString GetTestFile() => GetTestFileContents(XMLTestFileConstants.T2LPOUSTestFilePath, "TestT2LPOUSRequest.txt");

	#region Structures SetUp

	Mock<IT2LPOUSRequestAndReceptionGoodItem> mockItem1;

	protected override void SetUp()
	{
		base.SetUp();

		var mockProofOperationInformation = new Mock<IT2LPOUSRequestProofOperationInformationForT2LT2LF>();

		mockProofOperationInformation.Setup(m => m.LRN).Returns("LRNReference");
		mockProofOperationInformation.Setup(m => m.DeclarationType).Returns("T2L");

		var mockRequestedValidityOfTheProof = new Mock<IT2LPOUSRequestedValidityOfTheProof>();
		mockRequestedValidityOfTheProof = SetUpValidityOfTheProofCommon(95, "Justification");
		mockProofOperationInformation.Setup(m => m.RequestedValidityOfTheProof).Returns(mockRequestedValidityOfTheProof.Object);

		mockProofOperationInformation.Setup(m => m.RequestType).Returns("01");
		mockProofOperationInformation.Setup(m => m.NationalOnlyRequest).Returns(ZBool.True);
		mockProvider.Setup(m => m.ProofOperationInformationForT2LT2LF).Returns(mockProofOperationInformation.Object);

		var mockAuthorisation = SetUpAuthorisationCommon("C511", "ReferenceNumber", "Holder");
		mockProvider.Setup(m => m.Authorisation).Returns(mockAuthorisation.Object);

		mockProvider.Setup(m => m.PersonReqPres).Returns(SetUpPersonReqPresCommonWithAddress().Object);
		mockProvider.Setup(m => m.Representative).Returns(SetUpPersonReqPresCommon("ES89890001K", "Test", "test@gmail.com", "123456789").Object);
		mockProvider.Setup(m => m.CustomsOffice).Returns("ES002801");
		mockProvider.Setup(m => m.SendEmailL).Returns("S");
		mockProvider.Setup(m => m.SendEmailU).Returns("S");
		mockProvider.Setup(m => m.SendEmailExp).Returns("S");

		var commodityCode = BuilderHelperTest.SetUpCommodityCode("870821", "00");

		mockItem1 = SetUpGoodItem(1, commodityCode, "Description", "0010001-6", 1000.505m, 900.120m, SetUpPackagingList(), SetUpAdditionalInfoList(),
			SetUpPreviousDocList(), SetUpSupportingDocList(), SetUpAdditionalDocList());
		var mockGoodsShipment = SetUpGoodsShipmentCommon(mockItem1);
		mockProvider.Setup(m => m.GoodsShipment).Returns(mockGoodsShipment.Object);
	}

	#endregion
}
