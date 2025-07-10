using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR43;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR43DataProvider
	{
		public GOVCBRR43MessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRR43MessageData();

			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			result.AmendSequence = new ZInt(response.Declaration.VersionId?.Value ?? ZString.Empty);
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.DeclarationDate = new ZDate(dt1);
			}
			if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.NoticeDateTime = dt2;
			}
			if (DateTime.TryParseExact(response.Declaration.AuthenticationDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt3))
			{
				result.AfterReExportScheduledDate = new ZDate(dt3);
			}
			result.ResultType = response.Status.NameCode.Value;
			result.ResultReason = response.Status.Description?.Value;
			result.CustomsManagerName = response.Authenticator?.Name?.Value;
			result.CustomsOfficeAndDivision = response.Declaration.DeclarationOfficeId?.Value;
			result.CustomsOfficeContent = response.Declaration.AdditionalInformation?.Content?.Value;

			var dutyFulfillmentList = new List<ZString>();
			foreach (var additionalInformation in response.AdditionalInformation)
			{
				var item = additionalInformation.Content?.Value;
				if (!string.IsNullOrEmpty(item))
				{
					dutyFulfillmentList.Add(item);
				}
			}
			result.DutyFulfillment = dutyFulfillmentList.ToArray();

			var goodsShipmentList = new List<GOVCBRR43LineMessageData>();
			foreach (var goodsShipment in response.Declaration.GoodsShipment)
			{
				var invoiceLine = new GOVCBRR43LineMessageData();
				invoiceLine.EntryLineNo = (ZInt)goodsShipment.SequenceNumeric;
				invoiceLine.InvoiceLineNo = ZShort.ParseSafe(goodsShipment.GovernmentAgencyGoodsItem.Commodity.IdentityQualifierCode.Value, 0);

				goodsShipmentList.Add(invoiceLine);
			}
			result.InvoiceLines = goodsShipmentList.ToArray();

			return result;
		}
	}
}
