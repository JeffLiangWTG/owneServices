using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5UB;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5UBDataProvider
	{
		public GOVCBR5UBMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5UBMessageData(factory);

			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			result.CustomsPersonName = response.Authenticator.Name.Value;
			result.DeclarationOffice = response.Declaration.DeclarationOfficeId.Value;

			if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.ApprovalDate = new ZDate(dt);
			}

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.DateTime, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.NoticeDateTime = dt1;
			}

			result.PenaltyExemptionCode = response.Declaration.SubTypeCode.Value;
			result.ResultType = response.Status.NameCode.Value;
			result.ResultReason = response.Status.Description.Value;
			result.NoticeNumber = response.Declaration.DutyTaxFee?.Payment?.ReferenceId?.Value ?? ZString.Empty;
			result.PenaltyExemptionReqSequence = new ZInt(response.Declaration.VersionId.Value);
			result.PenaltyExemptionAmount = response.Declaration.InvoiceAmount.Value;
			result.ResultTypeDescription = factory.GetCachedValue<AdditionalTexReductionResultTypeList>().GetDescriptionFromCode(result.ResultType) ?? ZString.Empty;

			return result;
		}
	}
}
