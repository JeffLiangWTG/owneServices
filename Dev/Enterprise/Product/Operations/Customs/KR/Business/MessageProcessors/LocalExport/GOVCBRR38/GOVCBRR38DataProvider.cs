using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR38;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR38DataProvider
	{
		public IGOVCBRR38MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBRR38MessageData();
			result.DeclarationType = response.Declaration.TypeCode.Value;
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			if (DateTime.TryParseExact(response.Declaration.AcceptanceDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.AcceptDateTime = dt1;
			}
			result.CustomsOfficeAndDivision = response.Declaration.DeclarationOfficeId.Value;
			result.DeclarationNumber = response.Declaration.Id.Value;
			result.ConfirmNumber = response.Declaration.FunctionalReferenceId?.Value;
			result.ContentDescription = response.Status?.Description?.Value;
			return result;
		}
	}
}
