using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR73;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR73DataProvider
	{
		public IGOVCBRR73MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBRR73MessageData();
			result.ResultType = response.Status.NameCode.Value;
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			result.DocumentSubmitDescription = response.Status.Description?.Value ?? string.Empty;
			result.CustomsOfficerID = response.Authenticator?.Id?.Value ?? string.Empty;
			result.CustomsOfficerName = response.Authenticator?.Name?.Value ?? string.Empty;
			return result;
		}
	}
}
