using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR21;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR21DataProvider
	{
		public IGOVCBRR21MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRR21MessageData();

			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.DeclarationDate = new ZDate(dt1);
			}
			result.HouseBillNumber = response.Declaration.TransportContractDocument?.Id?.Value ?? ZString.Empty;
			result.RejectionCode = response.Declaration.ReasonCode?.Value ?? ZString.Empty;
			if (DateTime.TryParseExact(response.Declaration.RejectionDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.RejectionDate = new ZDate(dt2);
			}
			result.CustomsOffice = response.Declaration.DeclarationOfficeId.Value;
			result.CustomsPersonName = response.Authenticator.Name.Value;

			return result;
		}
	}
}
