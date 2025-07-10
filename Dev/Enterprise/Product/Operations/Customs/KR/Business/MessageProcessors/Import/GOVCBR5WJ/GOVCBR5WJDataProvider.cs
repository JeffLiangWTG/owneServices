using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5WJ;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5WJDataProvider
	{
		public IGOVCBR5WJMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5WJMessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDate = new ZDate(dt);
			}
			result.HSDescription = response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.CargoDescription?.Value ?? ZString.Empty;
			result.TradeName = response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Description?.Value ?? ZString.Empty;
			result.BrandName = response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.Name?.Value ?? ZString.Empty;
			result.ModelName = response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.DetailedCommodity?.Description?.Value ?? ZString.Empty;
			result.ClassificationReason = response.Declaration.Note?.Value ?? ZString.Empty;

			var addInfo = response.Declaration.AdditionalInformation;
			if (addInfo != null)
			{
				result.ContentDescription1 = addInfo.Count > 0 ? addInfo[0].StatementDescription?.Value ?? string.Empty : string.Empty;
				result.ContentDescription2 = addInfo.Count > 1 ? addInfo[1].StatementDescription?.Value ?? string.Empty : string.Empty;
			}

			result.DeclarantCompanyName = response.Declaration.Submitter?.Name?.Value ?? ZString.Empty;
			result.ImporterCompanyName = response.Declaration.Importer?.Name?.Value ?? ZString.Empty;
			result.AnalysisNumber = response.FunctionalReferenceId.Value;
			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			result.EntryLineNo = (ZInt)response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.SequenceNumeric;
			result.InvoiceLineNo = (ZInt)response.Declaration.GoodsShipment.GovernmentAgencyGoodsItem.Commodity.SequenceNumeric;

			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.DeclarationDate = new ZDate(dt1);
			}
			result.DecisionHSCode = response.Declaration.Amendment?.AdjustmentDescription?.Value ?? ZString.Empty;
			result.HSCode = response.Declaration.Amendment?.StatementDescription?.Value ?? ZString.Empty;

			return result;
		}
	}
}
