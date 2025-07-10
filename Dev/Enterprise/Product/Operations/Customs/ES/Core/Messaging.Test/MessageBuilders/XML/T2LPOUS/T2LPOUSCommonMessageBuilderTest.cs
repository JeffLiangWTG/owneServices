using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

public abstract class T2LPOUSCommonMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
	where TProvider : class, IT2LPOUSCommonDataProvider
	where TMessageBuilder : T2LPOUSCommonMessageBuilder<TProvider, T>
{
	protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
	protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
	protected abstract ZString GetTestFile();

	#region Structures SetUp

	protected Mock<IT2LPOUSCommonPersonReqPresWithAddress> SetUpPersonReqPresCommonWithAddress()
	{
		var mockPerson = new Mock<IT2LPOUSCommonPersonReqPresWithAddress>();
		mockPerson.Setup(m => m.Id).Returns("ES89890001K");

		mockPerson.Setup(m => m.Address).Returns(BuilderHelperTest.SetUpAddress("Street", "City", "PostCode", "Country"));

		var mockContactPerson = new Mock<IPartyContactProvider>();
		mockContactPerson.Setup(m => m.Name).Returns("Test");
		mockContactPerson.Setup(m => m.Email).Returns("test@gmail.com");
		mockContactPerson.Setup(m => m.PhoneNumber).Returns("123456789");

		mockPerson.Setup(m => m.ContactPerson).Returns(mockContactPerson.Object);

		return mockPerson;
	}

	protected Mock<IT2LPOUSTransportEquipment> SetUpTransportEquipment(ZString containerIndicator, IEnumerable<ZInt> goodReference)
	{
		var mockTransportEquipment = new Mock<IT2LPOUSTransportEquipment>();

		mockTransportEquipment.Setup(m => m.ContainerIdentificationNumber).Returns(containerIndicator);
		mockTransportEquipment.Setup(m => m.GoodsReference).Returns((IReadOnlyCollection<ZInt>)goodReference);

		return mockTransportEquipment;
	}

	protected Mock<IT2LPOUSRequestedValidityOfTheProof> SetUpValidityOfTheProofCommon(ZInt numberOfDays, ZString justification)
	{
		var mockRequestedValidityOfTheProof = new Mock<IT2LPOUSRequestedValidityOfTheProof>();

		mockRequestedValidityOfTheProof.Setup(m => m.NumberOfDays).Returns(numberOfDays);
		mockRequestedValidityOfTheProof.Setup(m => m.Justification).Returns(justification);

		return mockRequestedValidityOfTheProof;
	}

	protected Mock<IT2LPOUSCommonPersonReqPres> SetUpPersonReqPresCommon(ZString id, ZString name, ZString email, ZString phoneNumber)
	{
		var mockPerson = new Mock<IT2LPOUSCommonPersonReqPres>();
		mockPerson.Setup(m => m.Id).Returns(id);

		var mockContactPerson = new Mock<IPartyContactProvider>();
		mockContactPerson.Setup(m => m.Name).Returns(name);
		mockContactPerson.Setup(m => m.Email).Returns(email);
		mockContactPerson.Setup(m => m.PhoneNumber).Returns(phoneNumber);

		mockPerson.Setup(m => m.ContactPerson).Returns(mockContactPerson.Object);

		return mockPerson;
	}

	protected Mock<IT2LPOUSAuthorisation> SetUpAuthorisationCommon(ZString typeOfAuthorisation, ZString decisionReferenceNumber, ZString holderOfTheAuthorisation)
	{
		var mockAuthorisation = new Mock<IT2LPOUSAuthorisation>();

		mockAuthorisation.Setup(m => m.TypeOfAuthorisation).Returns(typeOfAuthorisation);
		mockAuthorisation.Setup(m => m.DecisionReferenceNumber).Returns(decisionReferenceNumber);
		mockAuthorisation.Setup(m => m.HolderOfTheAuthorisation).Returns(holderOfTheAuthorisation);

		return mockAuthorisation;
	}

	protected Mock<IT2LPOUSGoodsShipment> SetUpGoodsShipmentCommon(Mock<IT2LPOUSRequestAndReceptionGoodItem> mockItem1)
	{
		var mockGoodsShipment = new Mock<IT2LPOUSGoodsShipment>();
		mockGoodsShipment.Setup(m => m.ContainerIndication.IsContainerised).Returns(ZBool.True);

		var mockTransportEquipment1 = SetUpTransportEquipment("Container1", new ZInt[] { 1, 2 });
		var mockTransportEquipment2 = SetUpTransportEquipment("Container2", Enumerable.Empty<ZInt>());
		var mockTransportEquipment3 = SetUpTransportEquipment("Container3", new ZInt[] { 0 });
		var mockTransportEquipment = new IT2LPOUSTransportEquipment[] { mockTransportEquipment1.Object, mockTransportEquipment2.Object, mockTransportEquipment3.Object };
		mockGoodsShipment.Setup(m => m.TransportEquipment).Returns(mockTransportEquipment);

		mockGoodsShipment.Setup(m => m.AdditionalInformation).Returns(SetUpAdditionalInfoList);

		mockGoodsShipment.Setup(m => m.PreviousDocument).Returns(SetUpPreviousDocList);

		mockGoodsShipment.Setup(m => m.SupportingDocument).Returns(SetUpSupportingDocList);

		var mockTransDoc1 = BuilderHelperTest.SetUpDocument("N601", "TransDoc1");
		var mockTransDoc2 = BuilderHelperTest.SetUpDocument("N602", "TransDoc2");
		var mockTransDoc = new IDocumentsCommon[] { mockTransDoc1, mockTransDoc2 };
		mockGoodsShipment.Setup(m => m.TransportDocument).Returns(mockTransDoc);

		mockGoodsShipment.Setup(m => m.AdditionalReference).Returns(SetUpAdditionalDocList);

		var mockItem2 = SetUpGoodItem(2, null, "Description2", "0010001-6", 3000m, 600.2m, Enumerable.Empty<IT2LPOUSRequestAndReceptionPackaging>(), Enumerable.Empty<IDocumentsCommon>(),
			Enumerable.Empty<IDocumentsCommon>(), Enumerable.Empty<IDocumentsCommon>(), Enumerable.Empty<IDocumentsCommon>());
		var mockItems = new IT2LPOUSRequestAndReceptionGoodItem[] { mockItem1.Object, mockItem2.Object };
		mockGoodsShipment.Setup(m => m.GoodItems).Returns(mockItems);

		return mockGoodsShipment;
	}

	protected IT2LPOUSRequestAndReceptionPackaging[] SetUpPackagingList()
	{
		var packaging1 = SetUpPackaging("Mark1", 1, ZBool.True, "1A");
		var packaging2 = SetUpPackaging("Mark2", 1, ZBool.False, "BX");
		return new IT2LPOUSRequestAndReceptionPackaging[] { packaging1.Object, packaging2.Object };
	}

	protected IDocumentsCommon[] SetUpAdditionalInfoList()
	{
		var mockAdditionalInfo1 = BuilderHelperTest.SetUpDocument("00001", "AdditionalInfo1");
		var mockAdditionalInfo2 = BuilderHelperTest.SetUpDocument("00002", "AdditionalInfo2");
		return new IDocumentsCommon[] { mockAdditionalInfo1, mockAdditionalInfo2 };
	}

	protected IDocumentsCommon[] SetUpPreviousDocList()
	{
		var mockPrevDoc1 = BuilderHelperTest.SetUpDocument("C601", "PrevDoc1");
		var mockPrevDoc2 = BuilderHelperTest.SetUpDocument("C602", "PrevDoc2");
		return new IDocumentsCommon[] { mockPrevDoc1, mockPrevDoc2 };
	}

	protected IDocumentsCommon[] SetUpAdditionalDocList()
	{
		var mockAdditDoc1 = BuilderHelperTest.SetUpDocument("V001", "AdditDoc1");
		var mockAdditDoc2 = BuilderHelperTest.SetUpDocument("V002", "AdditDoc2");
		return new IDocumentsCommon[] { mockAdditDoc1, mockAdditDoc2 };
	}

	protected IDocumentsCommon[] SetUpSupportingDocList()
	{
		var mockSupDoc1 = BuilderHelperTest.SetUpDocument("Y601", "SupDoc1");
		var mockSupDoc2 = BuilderHelperTest.SetUpDocument("Y602", "SupDoc2");
		return new IDocumentsCommon[] { mockSupDoc1, mockSupDoc2 };
	}

	protected Mock<IT2LPOUSRequestAndReceptionGoodItem> SetUpGoodItem(ZInt itemNumber, ICommodityCodeCommon commodityCode, ZString description, ZString cusCode, ZDecimal grossMass, ZDecimal netMass, IEnumerable<IT2LPOUSRequestAndReceptionPackaging> packaging,
		IEnumerable<IDocumentsCommon> additionalInfo, IEnumerable<IDocumentsCommon> previousDoc, IEnumerable<IDocumentsCommon> supportingDoc, IEnumerable<IDocumentsCommon> additionalRef)
	{
		var mockItem = new Mock<IT2LPOUSRequestAndReceptionGoodItem>();

		mockItem.Setup(m => m.GoodsItemNumber).Returns(itemNumber);
		mockItem.Setup(m => m.CommodityCode).Returns(commodityCode);
		mockItem.Setup(m => m.Description).Returns(description);
		mockItem.Setup(m => m.CusCode).Returns(cusCode);
		mockItem.Setup(m => m.GoodsMeasure).Returns(BuilderHelperTest.SetUpGoodsMeasure(grossMass, netMass));
		mockItem.Setup(m => m.Package).Returns((IReadOnlyCollection<IT2LPOUSRequestAndReceptionPackaging>)packaging);
		mockItem.Setup(m => m.AdditionalInformation).Returns((IReadOnlyCollection<IDocumentsCommon>)additionalInfo);
		mockItem.Setup(m => m.PreviousDocument).Returns((IReadOnlyCollection<IDocumentsCommon>)previousDoc);
		mockItem.Setup(m => m.SupportingDocument).Returns((IReadOnlyCollection<IDocumentsCommon>)supportingDoc);
		mockItem.Setup(m => m.AdditionalReference).Returns((IReadOnlyCollection<IDocumentsCommon>)additionalRef);

		return mockItem;
	}

	protected Mock<IT2LPOUSRequestAndReceptionPackaging> SetUpPackaging(ZString marks, ZInt numberOfPackages, ZBool numberOfPackagesSpecified, ZString typeOfPackages)
	{
		var mockPackaging = new Mock<IT2LPOUSRequestAndReceptionPackaging>();

		mockPackaging.Setup(m => m.Marks).Returns(marks);
		mockPackaging.Setup(m => m.NumberOfPackages).Returns(numberOfPackages);
		mockPackaging.Setup(m => m.NumberOfPackagesValueSpecified).Returns(numberOfPackagesSpecified);
		mockPackaging.Setup(m => m.TypeOfPackages).Returns(typeOfPackages);

		return mockPackaging;
	}

	#endregion
}
