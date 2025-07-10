using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRRR5;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRRR5DataProvider
	{
		public IGOVCBRRR5MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBRRR5MessageData();
			if (response.Declaration.AdditionalDocument?.IssueDateTime != null)
			{
				if (DateTime.TryParseExact(response.Declaration.AdditionalDocument.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
				{
					result.NoticeDateTime = dt;
				}
			}
			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			result.ChangeReasonType = response.Status?.NameCode?.Value ?? ZString.Empty;
			result.ChangeReasonDescription = response.Declaration.Reason?.Value ?? ZString.Empty;
			result.ChangeReasonDescription2 = response.Declaration.AdditionalInformation?.Content?.Value ?? ZString.Empty;
			result.InspectionChangeType = response.Declaration.Control?.TypeCode?.Value ?? ZString.Empty;
			result.CustomsManagerID = response.Declaration.Authenticator?.Id?.Value ?? ZString.Empty;
			result.CustomsManagerName = response.Declaration.Authenticator?.Name?.Value ?? ZString.Empty;
			result.SubCustomsManagerID = response.Authenticator?.Id?.Value ?? ZString.Empty;
			result.SubCustomsManagerName = response.Authenticator?.Name?.Value ?? ZString.Empty;

			return result;
		}
	}
}
