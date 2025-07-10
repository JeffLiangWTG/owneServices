using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRRR3;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRRR3DataProvider
	{
		public IGOVCBRRR3MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBRRR3MessageData();

			result.DeclarationType = ((ZString)response.Declaration.TypeCode.Value).SubstringSafe(6, 3);
			result.ResultType = response.Status.NameCode.Value;
			result.ContentDescription = response.Status.Description?.Value ?? ZString.Empty;
			result.CustomsOfficeAndDivision = response.Declaration.DeclarationOfficeId.Value;
			result.CustomsPersonName = response.Authenticator?.Name?.Value ?? ZString.Empty;

			if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.CustomsDateTime = dt;
			}
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.NoticeDateTime = dt1;
			}
			result.NoticeNumber = response.Declaration.Id.Value;
			result.ConfirmNumber = response.Declaration.AdditionalDocument?.Id?.Value ?? ZString.Empty;
			result.AmendSequence = response.Declaration.VersionId?.Value == null ? ZInt.Zero : new ZInt(response.Declaration.VersionId.Value);

			return result;
		}
	}
}
