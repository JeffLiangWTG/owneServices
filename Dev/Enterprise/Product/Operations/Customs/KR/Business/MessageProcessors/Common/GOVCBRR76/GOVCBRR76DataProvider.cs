using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR76;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR76DataProvider
	{
		public IGOVCBRR76MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBRR76MessageData();

			result.DeclarationType = response.Declaration.TypeCode.Value.Substring(6);
			result.ApplicationNumber = response.Declaration.Id.Value;
			result.CustomsOfficeAndDivision = response.Declaration.DeclarationOfficeId.Value;
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.NoticeDateTime = dt1;
			}
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.AcceptDateTime = dt2;
			}

			return result;
		}
	}
}
