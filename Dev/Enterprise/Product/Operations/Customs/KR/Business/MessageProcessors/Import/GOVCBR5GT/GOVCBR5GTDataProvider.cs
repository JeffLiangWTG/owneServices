using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5GT;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5GTDataProvider
	{
		public IGOVCBR5GTMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5GTMessageData();

			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.AmendDate = new ZDate(dt);
			}
			result.DeclarationOffice = response.Declaration.DeclarationOfficeId.Value;
			result.CustomsPersonName = response.Authenticator.Name.Value;
			result.ComplementNumber = response.Declaration.AdditionalDocument.Id.Value;
			result.CustomsPersonPhoneNumber = response.Authenticator.Communication.Id.Value;
			result.AmendAcceptResult = response.Declaration.AdditionalInformation.Content.Value;

			return result;
		}
	}
}
