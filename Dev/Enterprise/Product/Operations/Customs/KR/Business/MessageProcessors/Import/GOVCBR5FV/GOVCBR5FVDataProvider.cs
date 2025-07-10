using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FV;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5FVDataProvider
	{
		public GOVCBR5FVMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5FVMessageData(factory);

			result.TaxInvoiceNumber = response.Id.Value;
			result.ImportDeclarationNumber = response.Declaration.PreviousDocument.Id.Value;
			result.NoticeNumber = response.Declaration.DutyTaxFee.Payment.ReferenceId.Value;
			result.RefundApprovalNo = response.Declaration.Id?.Value ?? ZString.Empty;
			result.ImporterID = response.Declaration.Importer.Id.Value;
			result.ImporterIDType = response.Declaration.Importer.Id.SchemeAgencyId.Value == AgencyIdentificationCodeContentType.Ktx
										? IdentificationType.BusinessRegNo : IdentificationType.KoreanRegNoForResident;
			result.ImporterCompanyName = response.Declaration.Importer.Name?.Value ?? ZString.Empty;
			result.ImporterRepresentativeName = response.Declaration.Importer.Contact?.Name?.Value ?? ZString.Empty;
			result.ImporterAddressLine = response.Declaration.Importer.Address?.Line?.Value ?? ZString.Empty;
			if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.PaymentDateTime, DateFormatType.DateYY, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.PaymentDate = new ZDate(dt);
			}
			result.BlankCount = response.Declaration.AdditionalInformation.Content.Value;
			result.CustomsValue = response.Declaration.DutyTaxFee.AdValoremTaxBaseAmount.Value;
			result.Tax = response.Declaration.DutyTaxFee.Payment.TaxAssessedAmount.Value;
			if (DateTime.TryParseExact(response.Declaration.Amendment?.AmendmentDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.AmendDate = new ZDate(dt1);
			}
			result.IssueReasonCode = response.Declaration.ReasonCode.Value;
			result.TaxInvoiceType = response.Declaration.AdditionalDocument.TypeCode.Value;
			result.RefundType = response.Declaration.TransactionNatureCode.Value;
			result.ReIssueYN = response.Declaration.AdditionalInformation.StatementCode.Value;

			return result;
		}
	}
}
