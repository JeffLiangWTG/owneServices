using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRRE7;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRRE7DataProvider
	{
		public IGOVCBRRE7MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRRE7MessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.NoticeDateTime = dt1;
			}
			result.ApplicationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.AcceptDateTime = dt2;
			}
			result.DeclarationOffice = response.Declaration.DeclarationOfficeId?.Value ?? ZString.Empty;
			result.ContentDescription = response.Declaration.AdditionalInformation?.Content?.Value ?? ZString.Empty;
			return result;
		}
	}
}
