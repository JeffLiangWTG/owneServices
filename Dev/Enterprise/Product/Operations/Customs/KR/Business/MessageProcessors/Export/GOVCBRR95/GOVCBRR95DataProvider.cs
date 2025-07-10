using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR95;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR95DataProvider
	{
		public IGOVCBRR95MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRR95MessageData();

			if (DateTime.TryParseExact(response.Control.InspectionEndDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.InspectionDate = new ZDate(dt);
			}
			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			result.InspectionSequenceNo = new ZInt(response.Declaration.VersionId.Value);
			result.DeclarantCompanyName = response.Declaration.Submitter?.Name?.Value ?? ZString.Empty;
			result.CustomsOfficeAndDivision = response.Declaration.DeclarationOfficeId.Value;
			result.InspectionPersonName = response.Authenticator?.Name?.Value ?? ZString.Empty;
			result.CustomsPersonPhoneNumber = response.Authenticator?.Communication?.Id?.Value ?? ZString.Empty;

			return result;
		}
	}
}
