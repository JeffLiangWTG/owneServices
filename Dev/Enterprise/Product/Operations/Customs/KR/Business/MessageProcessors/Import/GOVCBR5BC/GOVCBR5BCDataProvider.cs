using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BC;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5BCDataProvider
	{
		public IGOVCBR5BCMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBR5BCMessageData();
			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Declaration.AuthenticationDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.ApprovalDate = new ZDate(dt);
			}
			result.AmendType = response.Amendment.ChangeReasonCode.Value;
			result.ResultType = response.Status.NameCode.Value;
			result.CustomsOffice = response.Declaration.DeclarationOfficeId.Value;
			result.ResultReason = response.Declaration.Reason?.Value;
			result.CustomsManagerName = response.Authenticator.Name.Value;

			return result;
		}
	}
}
