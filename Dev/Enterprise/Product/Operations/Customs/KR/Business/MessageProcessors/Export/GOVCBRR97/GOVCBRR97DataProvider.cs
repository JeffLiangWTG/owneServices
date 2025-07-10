using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRR97;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRR97DataProvider
	{
		public IGOVCBRR97MessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBRR97MessageData();
			result.DeclarationType = response.TypeCode.Value;
			if (DateTime.TryParseExact(response.Amendment.AmendmentDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.AmendDate = (ZDate)dt;
			}
			result.ExportDeclarationNumber = response.Id.Value;
			result.AmendType = response.Declaration.TransactionNatureCode.Value;
			result.FaultParty = response.Declaration.Reason?.Value ?? ZString.Empty;
			result.CustomsOfficeAndDivision = response.Declaration.DeclarationOfficeId.Value;
			result.CustomsPersonName = response.Authenticator.Name.Value;
			result.ReasonCode = response.Declaration.AdditionalInformation.StatementCode.Value;
			result.AmendReasonDescription = response.Declaration.AdditionalInformation.StatementTypeCode.Value;
			result.SupplierCompanyName = response.Declaration.Exporter.Name.Value;

			return result;
		}
	}
}
