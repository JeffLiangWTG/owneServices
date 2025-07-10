using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5GX;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5GXDataProvider
	{
		public IGOVCBR5GXMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5GXMessageData();

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDate = (ZDate)dt;
			}
			result.ExportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.AdditionalInformation.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.ComplementDueDate = (ZDate)dt1;
			}
			result.ComplementReasonCode = response.Declaration.ReasonCode.Value;
			result.ComplementReasonName = response.Declaration.Reason.Value;
			result.ComplementDescription = response.Declaration.Amendment.StatementDescription.Value;
			result.CustomsOffice = response.Declaration.DeclarationOfficeId.Value;
			result.CustomsOfficeName = response.Declaration.DeclarationOffice.Value;
			result.DeclarantID = response.Declaration.Submitter.Id.Value;
			result.DeclarantName = response.Declaration.Submitter.Name.Value;
			result.ComplementNumber = response.Id.Value;

			return result;
		}
	}
}
