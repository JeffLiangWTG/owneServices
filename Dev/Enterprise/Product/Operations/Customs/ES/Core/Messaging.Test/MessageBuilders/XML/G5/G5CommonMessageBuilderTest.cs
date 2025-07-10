using System.Collections.Generic;
using CargoWise.Types;
using Moq;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders.Testing;

public abstract class G5CommonMessageBuilderTest<TMessageBuilder, TProvider, T> : XMLMessageBuilderTest<TMessageBuilder, TProvider, T>
	where TProvider : class, IG5GenericMessageDataProvider
	where TMessageBuilder : G5CommonMessageBuilder<TProvider, T>
{
	protected override sealed ZString GetSignedMessageTestFileContent() => GetTestFile();
	protected override sealed ZString GetUnsignedMessageTestFileContent() => GetTestFile();
	protected abstract ZString GetTestFile();

	#region Structures Common SetUp

	protected Mock<IG5CommonHeader> SetUpHeader()
	{
		var mockHeader = new Mock<IG5CommonHeader>();

		mockHeader.Setup(m => m.LRN).Returns("MY0000215654884455");

		var mockDeclarant = SetUpIG5PartyInfo("12345678A", "NORON EHF", "1", "SKUTUVOGUR", "Extra Address", "7", "po box", "subdivision", "104 REYKJAVIK", "40025", "IS", "EM", "mail.mail@mail.com");
		mockHeader.Setup(m => m.Declarant).Returns(mockDeclarant);

		var mockRepresentative = SetUpRepresentativeInfo();
		mockHeader.Setup(m => m.Representative).Returns(mockRepresentative);

		var mockAddInfo1 = BuilderHelperTest.SetUpDocument(ZString.Empty, "Cambio de ultima hora de vuelo");
		var mockAddInfo2 = BuilderHelperTest.SetUpDocument("12345", ZString.Empty);
		mockHeader.Setup(m => m.AdditionalInfos).Returns(new[] { mockAddInfo1, mockAddInfo2 });

		mockHeader.Setup(m => m.OriginCustomsOffice).Returns("ES001900");
		mockHeader.Setup(m => m.DestinationCustomsOffice).Returns("DE002800");
		mockHeader.Setup(m => m.TSWarehouse).Returns("ESTST02019000001");
		mockHeader.Setup(m => m.TotalLinesNum).Returns("2");
		mockHeader.Setup(m => m.TotalPackagesNum).Returns(1);
		mockHeader.Setup(m => m.TotalGrossWeightInKG).Returns(400.2222225M);

		var mockGoodsLocationOrigin = SetUpG5LocationGoods(ZString.Empty, "A", "V");
		mockHeader.Setup(m => m.GoodsLocationOrigin).Returns(mockGoodsLocationOrigin);

		var mockGoodsLocationDestination = SetUpG5LocationGoods(ZString.Empty, "B", "Y");
		mockHeader.Setup(m => m.GoodsLocationDestination).Returns(mockGoodsLocationDestination);

		var mockArrivalTransportMeans = BuilderHelperTest.SetUpCommonArrivalTransportMeans("20191201ACM40302", "40");
		mockHeader.Setup(m => m.ArrivalTransportMeans).Returns(mockArrivalTransportMeans);

		var mockTransportDoc = BuilderHelperTest.SetUpDocument("N703", "55466889874");
		mockHeader.Setup(m => m.TransportDocument).Returns(mockTransportDoc);

		var mockConsignor = SetUpIG5PartyInfo("89890001K", "Lobo Lobate", "2", "Lobera de Valdelacierva", "Extra Address2", "120", "box", "subdiv", "Por poco en Vinuelas", "19069", "ES", "EM", "mail.mail@mail.com");
		mockHeader.Setup(m => m.Consignor).Returns(mockConsignor);

		var mockAddressee = SetUpIG5PartyInfo("89890001K", "Ave", "3", "Acostadero eras de abajo", "Extra Address3", "44", "po", "div", "Fuentelahigera de Albatages", "19024", "ES", "TE", "912365478");
		mockHeader.Setup(m => m.Consignee).Returns(mockAddressee);

		var mockSupDoc1 = BuilderHelperTest.SetUpDocument("380", "187");
		var mockSupDoc2 = BuilderHelperTest.SetUpDocument("X001", "ES3600000002");
		mockHeader.Setup(m => m.SupportingDocuments).Returns(new[] { mockSupDoc1, mockSupDoc2 });

		return mockHeader;
	}

	protected IG5LocationGoods SetUpG5LocationGoods(ZString nationalLocation, ZString genericLocationType, ZString genericLocationQualifier)
	{
		var mockLocation = new Mock<IG5LocationGoods>();
		mockLocation.Setup(m => m.NationalLocation).Returns(nationalLocation);
		mockLocation.Setup(m => m.GenericLocation).Returns(BuilderHelperTest.SetUpGenericLocation(genericLocationType, genericLocationQualifier));
		return mockLocation.Object;
	}

	protected IG5PartyInfo SetUpIG5PartyInfo(ZString id, ZString name, ZString type, ZString street, ZString streetAddLine, ZString number, ZString poBox, ZString subDivision, ZString city, ZString postCode, ZString country, ZString comType, ZString comId)
	{
		var addressInformation = new Mock<IG5PartyInfo>();
		addressInformation.Setup(m => m.Id).Returns(id);
		addressInformation.Setup(m => m.Name).Returns(name);
		addressInformation.Setup(m => m.Type).Returns(type);
		addressInformation.Setup(m => m.Street).Returns(street);
		addressInformation.Setup(m => m.StreetAddLine).Returns(streetAddLine);
		addressInformation.Setup(m => m.Number).Returns(number);
		addressInformation.Setup(m => m.POBox).Returns(poBox);
		addressInformation.Setup(m => m.State).Returns(subDivision);
		addressInformation.Setup(m => m.City).Returns(city);
		addressInformation.Setup(m => m.PostCode).Returns(postCode);
		addressInformation.Setup(m => m.Country).Returns(country);
		addressInformation.Setup(m => m.CommunicationType).Returns(comType);
		addressInformation.Setup(m => m.CommunicationId).Returns(comId);
		return addressInformation.Object;
	}

	protected IG5RepresentativeInfo SetUpRepresentativeInfo()
	{
		var addressInformation = new Mock<IG5RepresentativeInfo>();
		addressInformation.Setup(m => m.Id).Returns("321654987A");
		addressInformation.Setup(m => m.Name).Returns("Name aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbb");
		addressInformation.Setup(m => m.Type).Returns("2");
		addressInformation.Setup(m => m.Street).Returns("Street aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbbccc");
		addressInformation.Setup(m => m.StreetAddLine).Returns("Extra Address4");
		addressInformation.Setup(m => m.Number).Returns("street number aaaaaaaaaaaaaaaaaaaaabbb");
		addressInformation.Setup(m => m.POBox).Returns("PO Box aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaabbb");
		addressInformation.Setup(m => m.State).Returns("Sub division aaaaaaaaaaaaaaaaaaaaaabbb");
		addressInformation.Setup(m => m.City).Returns("MADRID");
		addressInformation.Setup(m => m.PostCode).Returns("28010");
		addressInformation.Setup(m => m.Country).Returns("ES");
		addressInformation.Setup(m => m.CommunicationType).Returns("TE");
		addressInformation.Setup(m => m.CommunicationId).Returns("987654321");
		addressInformation.Setup(m => m.Status).Returns("2");
		return addressInformation.Object;
	}

	protected Mock<IG5CommonLine> SetUpCommonLine(ZString lineNumber, IEnumerable<IInternalPackageIdentificationCommon> packages, IDocumentsCommon transportDocument, IEnumerable<IG5TransportEquipment> containers, IEnumerable<IDocumentsCommon> supportingDocs, IEnumerable<IDocumentsCommon> additionalInfo)
	{
		var mockLine = new Mock<IG5CommonLine>();
		mockLine.Setup(m => m.LineNumber).Returns(lineNumber);
		mockLine.Setup(m => m.PackagesNum).Returns(100);
		mockLine.Setup(m => m.Packages).Returns((IReadOnlyCollection<IInternalPackageIdentificationCommon>)packages);
		mockLine.Setup(m => m.GrossWeightInKG).Returns(200.2222225M);
		mockLine.Setup(m => m.TransportDocument).Returns(transportDocument);
		mockLine.Setup(m => m.UCRCode).Returns("04071911");
		mockLine.Setup(m => m.CommodityCode).Returns("COMMCODE");
		mockLine.Setup(m => m.GoodsDescription).Returns("Huevos revueltos");
		mockLine.Setup(m => m.CusCode).Returns("cuscode78");
		mockLine.Setup(m => m.TransportEquipments).Returns((IReadOnlyCollection<IG5TransportEquipment>)containers);
		mockLine.Setup(m => m.PresentationDateAtOrigin).Returns(new ZDateTime(2020, 10, 13, 14, 30, 20));
		mockLine.Setup(m => m.SupportingDocuments).Returns((IReadOnlyCollection<IDocumentsCommon>)supportingDocs);
		mockLine.Setup(m => m.AdditionalInfo).Returns((IReadOnlyCollection<IDocumentsCommon>)additionalInfo);

		var mockPrevDoc = SetUpPreviousDocument();
		mockLine.Setup(m => m.PreviousDocument).Returns(mockPrevDoc);

		return mockLine;
	}

	IG5PreviousDocument SetUpPreviousDocument()
	{
		var mockDoc = new Mock<IG5PreviousDocument>();
		mockDoc.Setup(m => m.PreviousTSD).Returns(SetUpPreviousTSD());
		mockDoc.Setup(m => m.PreviousGeneric).Returns(SetUpGenericPreviousDocument());
		return mockDoc.Object;
	}

	IG5PreviousTSD SetUpPreviousTSD()
	{
		var mockDoc = new Mock<IG5PreviousTSD>();
		mockDoc.Setup(m => m.MRN).Returns("99989036AZM0000202");
		mockDoc.Setup(m => m.TransportMeans).Returns((ICommonArrivalTransportMeans)null);
		mockDoc.Setup(m => m.GoodsItemId).Returns("2");

		var mockTransportDoc = BuilderHelperTest.SetUpDocument("N705", "55466889222");
		mockDoc.Setup(m => m.TransportDocument).Returns(mockTransportDoc);
		return mockDoc.Object;
	}

	ICommonDocumentGoodsItemId SetUpGenericPreviousDocument()
	{
		var mockDoc = new Mock<ICommonDocumentGoodsItemId>();
		mockDoc.Setup(m => m.Name).Returns("235");
		mockDoc.Setup(m => m.Number).Returns("reference");
		mockDoc.Setup(m => m.GoodsItemId).Returns("4");
		return mockDoc.Object;
	}

	protected IG5TransportEquipment SetUpTransportEquipment(ZString id, ZString status, IEnumerable<ZString> seals)
	{
		var mockContainer = new Mock<IG5TransportEquipment>();
		mockContainer.Setup(m => m.Id).Returns(id);
		mockContainer.Setup(m => m.PackedStatus).Returns(status);
		mockContainer.Setup(m => m.SealIds).Returns((IReadOnlyCollection<ZString>)seals);
		return mockContainer.Object;
	}

	#endregion
}
