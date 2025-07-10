using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CargoWise.Customs.ES.MessageDefinitions.Version1;
using CargoWise.Customs.ES.MessageDefinitions.Version1.AES.ES_CC507C_v514.CC507CV1Ent;
using CargoWise.Types;

namespace Enterprise.Customs.ES.Messaging.MessageBuilders;

[WTG.StaticAnalysis.Annotation.CodeAlive("will be used in the future.")]
public class EALAESMessageBuilder : AESCommonMessageBuilder<IEALAESMessageDataProvider, Cc507Cv1Ent>
{
	public EALAESMessageBuilder(IEALAESMessageDataProvider provider, ZString messageType, ZString messageSubType)
		: base(provider, messageType, messageSubType)
	{
	}

	protected override ZString GetMessageType() => "CC507C";

	protected override Cc507Cv1Ent GenerateXMLMessage()
	{
		var declaration = GetPopulatedTransactionId<Cc507Cv1Ent>();
		if (declaration != null)
		{
			declaration.Cc507C = GetPopulatedCC507C();
		}
		return declaration;
	}

	Cc507CType GetPopulatedCC507C()
	{
		var cC507Cv514 = GetPopulatedMessage<Cc507CType>();
		if (cC507Cv514 != null)
		{
			cC507Cv514.ExportOperation = GetPopulatedExportOperation();
			cC507Cv514.CustomsOfficeOfExitActual = GetPopulatedCustomsOffice<CustomsOfficeOfExitActualType02>(provider.CustomsOfficeOfExitActual);
			cC507Cv514.GoodsShipment = GetPopulatedGoodsShipment();
		}
		return cC507Cv514;
	}

	ExportOperationType507 GetPopulatedExportOperation()
	{
		var exportOperation = provider.ExportOperation;
		return exportOperation == null ? null : new ExportOperationType507
		{
			Mrn = exportOperation.MRN,
			StoringFlag = exportOperation.StoringFlag ? Flag.Item1 : Flag.Item0,
			DiscrepanciesExist = exportOperation.DiscrepanciesExist ? Flag.Item1 : Flag.Item0
		};
	}

	GoodsShipmentType507 GetPopulatedGoodsShipment()
	{
		var goodsShipment = provider.GoodsShipment;
		return goodsShipment == null ? null : new GoodsShipmentType507
		{
			Consignment = GetPopulatedEALConsignment(goodsShipment.Consignment),
			GoodsItem = goodsShipment.GoodsItem.ConvertToCollection(GetPopulatedGoodsItem)
		};
	}

	ConsignmentType02 GetPopulatedEALConsignment(IEALAESConsignment consignment)
	{
		return consignment == null ? null : new ConsignmentType02
		{
			ModeOfTransportAtTheBorder = consignment.ModeOfTransportAtTheBorder,
			ReferenceNumberUcr = consignment.ReferenceNumberUCR,
			ExitCarrier = GetPopulatedPartyIdProviderWithContactPerson<ExitCarrierType02, ContactPersonType02>(consignment.ExitCarrier),
			TransportEquipment = consignment.TransportEquipment.ConvertToCollection(GetPopulatedTransportEquipment<TransportEquipmentType05, SealType02, GoodsReferenceType04>),
			LocationOfGoods = GetPopulatedEALLocationOfGoods(consignment.LocationOfGoods),
			ActiveBorderTransportMeans = GetPopulatedTransportMediumInfoCommon<ActiveBorderTransportMeansType02>(consignment.ActiveBorderTransportMeans),
			TransportDocument = GetPopulatedTransportDocument(consignment.TransportDocument)
		};
	}

	Collection<LocationOfGoodsType02> GetPopulatedEALLocationOfGoods(IEALAESLocationOfGoods locationOfGoodsProvider)
	{
		return locationOfGoodsProvider == null ? null : new Collection<LocationOfGoodsType02>()
		{
			new LocationOfGoodsType02
			{
				SequenceNumber = locationOfGoodsProvider.SequenceNumber,
				TypeOfLocation = locationOfGoodsProvider.LocationType,
				QualifierOfIdentification = locationOfGoodsProvider.LocationQualifier,
				AuthorisationNumber = locationOfGoodsProvider.LocationId
			}
		};
	}

	GoodsItemType507 GetPopulatedGoodsItem(IEALAESGoodsItem goodsItem)
	{
		return goodsItem == null ? null : new GoodsItemType507()
		{
			DeclarationGoodsItemNumber = goodsItem.SequenceNumber,
			ReferenceNumberUcr = goodsItem.ReferenceNumberUCR,
			Commodity = GetPopulatedEALCommodity(goodsItem.Commodity),
			Packaging = GetPopulatedPackaging(goodsItem),
			TransportDocument = GetPopulatedTransportDocument(goodsItem.TransportDocuments),
		};
	}

	CommodityType06 GetPopulatedEALCommodity(IEALAESCommodity commodity)
	{
		return commodity == null ? null : new CommodityType06()
		{
			GoodsMeasure = GetPopulatedCommonGoodsMeasure<GoodsMeasureType08>(commodity.GoodsMeasure)
		};
	}

	Collection<PackagingType04> GetPopulatedPackaging(IEALAESGoodsItem goodsItem)
	{
		var packagesCollection = new Collection<PackagingType04>();
		foreach (var package in goodsItem.Packaging.ConvertToCollection(GetPopulatedCommonPackage<PackagingType04>) ?? Enumerable.Empty<IPackageWithSequenceAndPackNumCommon>())
		{
			packagesCollection.Add((PackagingType04)package);
		}

		return packagesCollection;
	}

	Collection<TransportDocumentType03> GetPopulatedTransportDocument(IReadOnlyCollection<ICommonDocumentSequenceNumber> transportDocuments)
	{
		var transportDocumentsCollection = new Collection<TransportDocumentType03>();
		foreach (var transportDocument in transportDocuments.ConvertToCollection(GetPopulatedCommonDocumentSequenceNumber<TransportDocumentType03>) ?? Enumerable.Empty<IDocumentSequenceNumberCommon>())
		{
			transportDocumentsCollection.Add((TransportDocumentType03)transportDocument);
		}

		return transportDocumentsCollection;
	}
}
