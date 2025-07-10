using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBRREN;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBRRENDataProvider
	{
		public IGOVCBRRENMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBRRENMessageData();

			result.ExportDeclarationNumber = response.Id.Value;
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			result.SequenceNo = new ZInt(response.Declaration.VersionId.Value);
			result.FunctionCode = response.Declaration.FunctionCode.Value;
			result.SuspendedType = response.Declaration.TypeCode.Value;
			result.SuspendedCode = response.Declaration.TransactionNatureCode.Value;
			result.SuspendedReason = response.Declaration.AdditionalInformation?.Content?.Value ?? ZString.Empty;
			if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.BeginningDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.StartDateTime = dt1;
			}
			if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.EndingDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.EndDateTime = dt2;
			}
			result.CustomsPersonName = response.Declaration.Authenticator?.Name?.Value ?? ZString.Empty;
			result.CustomsPersonPhoneNumber = response.Declaration.Authenticator?.Communication?.Id?.Value ?? ZString.Empty;
			return result;
		}
	}
}
