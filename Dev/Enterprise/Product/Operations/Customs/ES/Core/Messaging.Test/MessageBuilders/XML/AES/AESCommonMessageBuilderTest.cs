using System;
using System.Collections.Generic;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

public abstract class AESCommonMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
	where TProvider : class, IAESCommonDataProvider
	where TMessageBuilder : AESCommonMessageBuilder<TProvider, T>
{
	protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
	protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
	protected abstract ZString GetTestFile();
	public abstract void TestPopulatePhaseID();

	#region Structures SetUp

	protected Mock<IAESCommonMessage> SetUpMessage()
	{
		var mockMessage = new Mock<IAESCommonMessage>();
		mockMessage.Setup(m => m.Sender).Returns("A78587268");
		mockMessage.Setup(m => m.MessageIdentification).Returns("TEST_PWS");
		return mockMessage;
	}

	protected Mock<IAESCommonTransportEquipment> SetUpTransportEquipment(ZString sequenceNuber, ZString totalOfSeals, IEnumerable<ISealCommon> seals, IEnumerable<ICommonGoodsReference> goodsReferences)
	{
		var mockTransportEquipment = new Mock<IAESCommonTransportEquipment>();
		mockTransportEquipment.Setup(m => m.SequenceNumber).Returns(sequenceNuber);
		mockTransportEquipment.Setup(m => m.ContainerNumber).Returns("HXDU1234567");
		mockTransportEquipment.Setup(m => m.NumberOfSeals).Returns(totalOfSeals);
		mockTransportEquipment.Setup(m => m.Seals).Returns((IReadOnlyCollection<ISealCommon>)seals);
		mockTransportEquipment.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<ICommonGoodsReference>)goodsReferences);

		return mockTransportEquipment;
	}

	protected Mock<ICommonGoodsReference> SetUpGoodReference(ZString sequenceNuber, ZString itemNumber)
	{
		var mockGoodReference = new Mock<ICommonGoodsReference>();
		mockGoodReference.Setup(m => m.SequenceNumber).Returns(sequenceNuber);
		mockGoodReference.Setup(m => m.GoodsItemNumber).Returns(itemNumber);

		return mockGoodReference;
	}

	protected Mock<IAESCommonLocationOfGoods> SetUpLocationOfGoods(IPartyContactProvider contactPerson)
	{
		var mockLocation = new Mock<IAESCommonLocationOfGoods>();
		mockLocation.Setup(m => m.LocationType).Returns("B");
		mockLocation.Setup(m => m.LocationQualifier).Returns("Y");
		mockLocation.Setup(m => m.LocationId).Returns("010101GENE");
		mockLocation.Setup(m => m.LocationAdditionalId).Returns("1234");
		mockLocation.Setup(m => m.LocationUNloCode).Returns("Code");
		mockLocation.Setup(m => m.LocationCustomOffice).Returns("ES009999");

		var mockLocationGNSS = new Mock<ICommonGNSS>();
		mockLocationGNSS.Setup(m => m.Latitude).Returns("40°30′N");
		mockLocationGNSS.Setup(m => m.Longitude).Returns("3°40′O");
		mockLocation.Setup(m => m.LocationGNSS).Returns(mockLocationGNSS.Object);

		mockLocation.Setup(m => m.LocationEconomicOperatorId).Returns("12345678A");

		mockLocation.Setup(m => m.LocationAddress).Returns(BuilderHelperTest.SetUpAddress("Street", "Madrid", "28003", "ES"));

		var mockLocationPostCodeAddress = new Mock<ICommonPostcodeAddress>();
		mockLocationPostCodeAddress.Setup(m => m.HouseNumber).Returns("HouseNumber");
		mockLocationPostCodeAddress.Setup(m => m.PostCode).Returns("28003");
		mockLocationPostCodeAddress.Setup(m => m.Country).Returns("ES");
		mockLocation.Setup(m => m.LocationPostcodeAddress).Returns(mockLocationPostCodeAddress.Object);

		mockLocation.Setup(m => m.LocationContactPerson).Returns(contactPerson);

		return mockLocation;
	}

	protected Mock<ICommonDepartureTransportMeans> SetUpDepartureTransportMeans(ZString sequenceNumber)
	{
		var mockDepartureTransportMeans = new Mock<ICommonDepartureTransportMeans>();
		mockDepartureTransportMeans.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockDepartureTransportMeans.Setup(m => m.TransportMode).Returns("30");
		mockDepartureTransportMeans.Setup(m => m.TransportId).Returns("Plane");
		mockDepartureTransportMeans.Setup(m => m.TransportNationality).Returns("ES");

		return mockDepartureTransportMeans;
	}

	protected Mock<IAESCommonDocument> SetUpAESCommonDocument(ZString sequenceNumber, ZString name, ZString number, ZString line, ZString measurement, ZDecimal quantity, ZBool quantitySpecified)
	{
		var mockPreviousDocument = new Mock<IAESCommonDocument>();
		mockPreviousDocument.Setup(m => m.SequenceNumber).Returns(sequenceNumber);
		mockPreviousDocument.Setup(m => m.Name).Returns(name);
		mockPreviousDocument.Setup(m => m.Number).Returns(number);
		mockPreviousDocument.Setup(m => m.LineNumber).Returns(line);
		mockPreviousDocument.Setup(m => m.Measurement).Returns(measurement);
		mockPreviousDocument.Setup(m => m.Quantity).Returns(quantity);
		mockPreviousDocument.Setup(m => m.QuantitySpecified).Returns(quantitySpecified);

		return mockPreviousDocument;
	}

	protected Mock<IAESCommonSupportingDocumentExtraFields> SetUpCommonSupportingDocumentExtraFields(ZString issueAuthorityName, DateTime documentDate)
	{
		var mockCommonDocument = new Mock<IAESCommonSupportingDocumentExtraFields>();
		mockCommonDocument.Setup(m => m.IssuingAuthorityName).Returns(issueAuthorityName);
		mockCommonDocument.Setup(m => m.DocumentDate).Returns(documentDate);
		mockCommonDocument.Setup(m => m.DocumentDateSpecified).Returns(true);

		return mockCommonDocument;
	}

	#endregion
}
