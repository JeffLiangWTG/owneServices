using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRRCA;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRRCADataProvider
	{
		public IGOVCBRRCAMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBRRCAMessageData();
			if (DateTime.TryParseExact(response.Declaration.AcceptanceDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			result.RefundDeclarationNumber = response.Declaration.Id.Value;
			result.NoticeType = response.Declaration.TransactionNatureCode.Value;
			result.CustomsPersonName = response.Authenticator?.Name?.Value ?? ZString.Empty;
			result.DeclarationOffice = response.Declaration.DeclarationOfficeId.Value;
			result.ResultReason = response.Status?.Description?.Value ?? ZString.Empty;
			result.ContentDescription = response.Declaration.AdditionalInformation?.Content?.Value ?? ZString.Empty;
			result.ContentDescription2 = response.Declaration.AdditionalInformation?.StatementDescription?.Value ?? ZString.Empty;
			return result;
		}
	}
}
