using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5SN;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5SNDataProvider
	{
		public IGOVCBR5SNMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5SNMessageData();

			result.ValueDeclarationTemplateNumber = response.Id.Value;
			if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.ApprovalDate = new ZDate(dt);
			}
			if (DateTime.TryParseExact(response.Declaration.ExpirationDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.EffectiveToDate = new ZDate(dt1);
			}
			result.ResultType = response.Status.NameCode.Value;
			result.DeclarationOffice = response.Declaration.DeclarationOfficeId.Value;
			result.CustomsPersonName = response.Authenticator.Name.Value;
			result.ResultReason = response.Status.Description?.Value ?? ZString.Empty;
			result.IdentificationNumber = response.Declaration.Id?.Value ?? ZString.Empty;
			return result;
		}
	}
}
