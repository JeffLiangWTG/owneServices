using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BG;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5BGDataProvider
	{
		public IGOVCBR5BGMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5BGMessageData();

			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Declaration.AuthenticationDateTime, DateFormatType.DateTimeNoSecond, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.ApprovalDateTime = dt;
			}

			result.ResultType = response.Status.NameCode.Value;
			result.FaultParty = response.Declaration.ReasonCode?.Value ?? ZString.Empty;
			result.ReasonCode = response.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty;
			result.DeclarationOffice = response.Declaration.DeclarationOfficeId.Value;
			result.CustomsPersonName = response.Authenticator.Name.Value;
			result.OtherReason = response.AdditionalInformation?.Content?.Value ?? ZString.Empty;
			result.DismissalReason = response.Declaration.Reason?.Value ?? ZString.Empty;

			return result;
		}
	}
}
