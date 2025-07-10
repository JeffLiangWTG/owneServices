using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR98;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR98DataProvider
	{
		public IGOVCBRR98MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRR98MessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			result.DeclarantName = response.Declaration.Submitter.Name.Value;
			var goodsShipmentList = new List<GoodsShipment>();

			foreach (var entry in response.Declaration.GoodsShipment)
			{
				var goodsShipment = new GoodsShipment();

				goodsShipment.ExportDeclarationNumber = entry.GovernmentAgencyGoodsItem.PreviousDocument.Id.Value;
				if (DateTime.TryParseExact(entry.GovernmentAgencyGoodsItem.PreviousDocument.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
				{
					goodsShipment.DeclarationDate = new ZDate(dt1);
				}
				if (DateTime.TryParseExact(entry.AdditionalInformation.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
				{
					goodsShipment.EntryReleaseDate = new ZDate(dt2);
				}
				if (DateTime.TryParseExact(entry.Consignment.BorderTransportMeans.PortActivityDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt3))
				{
					goodsShipment.LoadingDate = new ZDate(dt3);
				}
				goodsShipment.ExporterCompanyName = entry.Exporter.Name.Value;
				goodsShipment.TradeName = entry.GovernmentAgencyGoodsItem.Commodity.CargoDescription.Value;
				goodsShipment.TotalPackQty = (ZInt)(entry.GovernmentAgencyGoodsItem.Packaging?.QuantityQuantity?.Value ?? ZInt.Zero);
				goodsShipment.TotalGrossWeightInKG = entry.GovernmentAgencyGoodsItem.Commodity.SizeMeasure.Value;

				goodsShipmentList.Add(goodsShipment);
			}
			result.GoodsShipment = goodsShipmentList.ToArray();

			return result;
		}
	}
}
