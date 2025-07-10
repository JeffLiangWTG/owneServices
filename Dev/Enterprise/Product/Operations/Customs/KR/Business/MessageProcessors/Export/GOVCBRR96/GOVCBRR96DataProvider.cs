using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR96;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR96DataProvider
	{
		public IGOVCBRR96MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRR96MessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDate = (ZDate)dt;
			}
			result.AnalysisNumber = response.Declaration.Id.Value;
			result.ClassificationReason = response.Declaration.Reason?.Value ?? ZString.Empty;
			result.ContentDescription1 = response.Declaration.AdditionalInformation?.Content?.Value ?? ZString.Empty;
			result.ContentDescription2 = response.Declaration.AdditionalInformation?.StatementDescription?.Value ?? ZString.Empty;
			result.DecisionHSCode = response.Declaration.Amendment?.AdjustmentDescription?.Value ?? ZString.Empty;
			result.HSCode = response.Declaration.Amendment?.AdjustmentDescription?.Value ?? ZString.Empty;
			result.SupplierCompanyName = response.Declaration.Exporter?.Name?.Value ?? ZString.Empty;
			result.TradeName = response.Declaration.GoodsShipment?.GovernmentAgencyGoodsItem?.Commodity?.CargoDescription?.Value ?? ZString.Empty;
			result.ModelName = response.Declaration.GoodsShipment?.GovernmentAgencyGoodsItem?.Commodity?.Description?.Value ?? ZString.Empty;
			result.ExportDeclarationNumber = response.Declaration.PreviousDocument.Id.Value;
			result.EntryLineNo = (ZInt)response.Declaration.PreviousDocument.SequenceNumeric;
			result.InvoiceLineNo = (ZInt)response.Declaration.PreviousDocument.LineNumeric;
			result.DeclarantCompanyName = response.Declaration.Submitter?.Name?.Value ?? ZString.Empty;

			return result;
		}
	}
}
