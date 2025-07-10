using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.ES.Business;
using Enterprise.Customs.ES.Business.CusTempStorage;
using Enterprise.Customs.ES.Business.MessageWrappers;
using Enterprise.Customs.ES.Messaging.MessageBuilders;

namespace Enterprise.Customs.ES.TemporaryStorage.Business.MessageWrappers;

public class G5CommonHeaderWrapper : G5SimplifiedHeaderWrapper, IG5CommonHeader
{
	public G5CommonHeaderWrapper(TemporaryStorageHeader tempHeader) : base(tempHeader)
	{
	}

	public ZString OriginCustomsOffice => tempHeader.AMA_CustomsOffice;

	public IG5LocationGoods GoodsLocationOrigin => goodsLocationOrigin ??= new G5LocationGoodsWrapper(tempHeader.AMA_CustomsOffice, (CusGoodsLocation)tempHeader.GoodsLocation);
	G5LocationGoodsWrapper goodsLocationOrigin;

	public ZString DestinationCustomsOffice => tempHeader.DestinationCustomsOffice;

	public IG5LocationGoods GoodsLocationDestination => goodsLocationDestination ??= new G5LocationGoodsWrapper(tempHeader.AMA_CustomsOffice, tempHeader.DestinationGoodsLocation);
	G5LocationGoodsWrapper goodsLocationDestination;

	public ZString TSWarehouse => tempHeader.AuthorizationNumber;

	public ICommonArrivalTransportMeans ArrivalTransportMeans
	{
		get
		{
			if (arrivalTransportMeans == null)
			{
				var type = tempHeader.TransportType;
				var means = tempHeader.ArrivalTransportMeansCode;

				arrivalTransportMeans = type.IsEmpty && means.IsEmpty ? null : new CommonArrivalTransportMeansWrapper(type, means);
			}
			return arrivalTransportMeans;
		}
	}
	CommonArrivalTransportMeansWrapper arrivalTransportMeans;

	public IDocumentsCommon TransportDocument
	{
		get
		{
			if (transportDocument == null)
			{
				var type = bill?.TypeOfBillDocument ?? ZString.Empty;
				var number = bill?.ABL_BillNumber ?? ZString.Empty;

				transportDocument = type.IsEmpty && number.IsEmpty ? null : new DocumentCommonWrapper(type, number);
			}
			return transportDocument;
		}
	}
	DocumentCommonWrapper transportDocument;

	public IG5PartyInfo Consignor => consignor ??= bill != null
														? new G5PartyInfoWrapper(bill.Shipper, bill.ABL_ShipperRegNo, bill.ABL_ShipperName, bill.ABL_ShipperRegNoType, bill.ABL_ShipperStreet1, bill.ABL_ShipperStreet2, bill.Lookups.ShipperState_List.GetDescriptionFromCode(bill.ABL_ShipperState), bill.ABL_RN_NKShipperCountry, bill.ABL_ShipperPostcode, bill.ABL_ShipperCity, bill.ABL_ShipperPhone)
														: null;
	G5PartyInfoWrapper consignor;

	public IG5PartyInfo Consignee => consignee ??= bill != null
														? new G5PartyInfoWrapper(bill.Consignee, bill.ABL_ConsigneeRegNo, bill.ABL_ConsigneeName, bill.ABL_ConsigneeRegNoType, bill.ABL_ConsigneeStreet1, bill.ABL_ConsigneeStreet2, bill.Lookups.ConsigneeState_List.GetDescriptionFromCode(bill.ABL_ConsigneeState), bill.ABL_RN_NKConsigneeCountry, bill.ABL_ConsigneePostcode, bill.ABL_ConsigneeCity, bill.ABL_ConsigneePhone)
														: null;
	G5PartyInfoWrapper consignee;

	public IReadOnlyCollection<IDocumentsCommon> SupportingDocuments
	{
		get
		{
			if (supportingDocuments == null)
			{
				var docs = new List<DocumentCommonWrapper>();

				if (bill != null)
				{
					docs.AddRange(bill.SupportingDocuments.Select(doc => new DocumentCommonWrapper(doc.CSI_Code, doc.CSI_ReferenceNumber)));
				}

				supportingDocuments = docs.AsReadOnly();
			}
			return supportingDocuments;
		}
	}
	IReadOnlyCollection<DocumentCommonWrapper> supportingDocuments;

	public ZString TotalLinesNum => bill?.PackedItems.Where(x => !x.IsMissing).Count().ToString() ?? ZString.Empty;

	public ZInt TotalPackagesNum => bill?.PackedItems.Cast<TemporaryStoragePackedItem>().Where(x => !x.IsMissing).Sum(x => x.TotalPackageQuantity) ?? ZInt.Zero;

	public ZDecimal TotalGrossWeightInKG => bill?.PackedItems.Cast<TemporaryStoragePackedItem>().Where(x => !x.IsMissing).Sum(x => x.GrossWeightInKG > 1 ? Math.Ceiling(x.GrossWeightInKG) : x.GrossWeightInKG) ?? ZInt.Zero;
}
