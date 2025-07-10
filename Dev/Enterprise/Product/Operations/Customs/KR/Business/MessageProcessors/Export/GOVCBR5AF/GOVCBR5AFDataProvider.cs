using System;
using System.Globalization;
using System.IO;
using System.Linq;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5AF;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5AFDataProvider
	{
		public IGOVCBR5AFMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5AFMessageData();
			result.DeclarationType = ((ZString)response.Declaration.TypeCode.Value).SubstringSafe(6);
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Declaration.AcceptanceDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.AcceptDateTime = dt1;
			}

			result.CustomsPersonID = response.Authenticator?.Id?.Value ?? ZString.Empty;
			result.CustomsPersonName = response.Authenticator?.Name?.Value ?? ZString.Empty;

			result.CustomsManagerID = response.Declaration.Authenticator?.Id?.Value ?? ZString.Empty;
			result.CustomsManagerName = response.Declaration.Authenticator?.Name?.Value ?? ZString.Empty;

			result.AmendAcceptResult = response.Status.FirstOrDefault()?.Description?.Value ?? ZString.Empty;

			return result;
		}
	}
}
