using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR59;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR59DataProvider
	{
		public IGOVCBRR59MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRR59MessageData();

			result.ImportDeclarationNumber = response.Declaration.Id.Value;

			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.DeclarationDate = new ZDate(dt);
			}

			if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.NoticeDateTime = new ZDateTime(dt1);
			}

			result.ResultType = response.Status.NameCode.Value;
			result.CustomsManagerName = response.Authenticator?.Name?.Value;
			result.CustomsOffice = response.Declaration.DeclarationOfficeId?.Value;
			result.EntryLineNo = (ZInt)response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.SequenceNumeric;
			result.HSCode = response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity?.Classification?.Id?.Value;
			result.Quantity = response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity?.CountQuantity?.Value ?? ZDecimal.Zero;
			result.QuantityUnit = response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity?.CountQuantity?.KcsUnitCode;
			result.NetWeightInKG = response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.GoodsMeasure?.NetNetWeightMeasure?.Value ?? ZDecimal.Zero;

			return result;
		}
	}
}
