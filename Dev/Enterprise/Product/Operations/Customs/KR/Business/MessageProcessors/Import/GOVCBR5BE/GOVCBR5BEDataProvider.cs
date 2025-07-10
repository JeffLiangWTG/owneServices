using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5BE;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5BEDataProvider
	{
		public GOVCBR5BEMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5BEMessageData(factory);

			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.DateTimeNoSecond, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.NoticeDateTime = dt;
			}
			if (DateTime.TryParseExact(response.Declaration.DutyTaxFee?.Payment?.DueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.PaymentDate = new ZDate(dt1);
			}
			result.ResultType = response.Status.NameCode.Value;
			result.ResultCode = response.Declaration.ReasonCode?.Value ?? ZString.Empty;
			result.ResultReason = response.Declaration.Reason?.Value ?? ZString.Empty;
			result.ApprovalCode = response.Declaration.AdditionalInformation?.StatementCode?.Value ?? ZString.Empty;
			result.PaymentType = response.Declaration.DutyTaxFee?.Payment?.MethodCode?.Value ?? ZString.Empty;
			result.DeclarationProcedureType = response.Declaration.CustomsProcedure?.TypeCode?.Value ?? ZString.Empty;
			result.CustomsManagerName = response.Authenticator.Name.Value;
			result.DeclarationOffice = response.Declaration.DeclarationOfficeId.Value;

			return result;
		}
	}
}
