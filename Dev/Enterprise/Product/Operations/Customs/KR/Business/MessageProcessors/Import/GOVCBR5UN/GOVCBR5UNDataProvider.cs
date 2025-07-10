using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5UN;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5UNDataProvider
	{
		public GOVCBR5UNMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBR5UNMessageData();

			result.RefundDeclarationNumber = response.Declaration.Id.Value;
			result.ImportCompanyName = response.Declaration.Submitter.Name.Value;
			result.ImportRepresentativeName = response.Declaration.Submitter.Contact.Name.Value;
			result.ImportAddressLine1 = response.Declaration.Submitter.Address.Description.Value;
			result.ImportAddressLine2 = response.Declaration.Submitter.Address.Line?.Value ?? ZString.Empty;
			result.CustomsOfficeName = response.Authenticator.Name.Value;
			result.CustomsDivisionName = response.Authenticator.Contact.DepartmentName.Value;

			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.SubmissionDate = new ZDate(dt1);
			}
			if (DateTime.TryParseExact(response.Declaration.AdditionalDocument.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.ApprovalDate = new ZDate(dt2);
			}
			if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt3))
			{
				result.PaymentDate = new ZDate(dt3);
			}

			result.ApprovalNo = response.Declaration.AdditionalDocument.Id.Value;
			result.BankAccountNumber = response.Declaration.BankAccount.Id.Value;
			result.NoticeNumber = response.Declaration.PreviousDocument.Id.Value;

			var dict = new Dictionary<ZString, IDutyTaxFee>();
			foreach (var tax in response.Declaration.DutyTaxFee)
			{
				IDutyTaxFee dutyTaxfee;
				if (!dict.TryGetValue(tax.TypeCode.Value, out dutyTaxfee))
				{
					dutyTaxfee = new DutyTaxFee();
					dict.Add(tax.TypeCode.Value, dutyTaxfee);
				}

				if (tax.AdditionalTaxTypeCode.Value == RefundDeterminationCodeList.Codes._11)
				{
					dutyTaxfee.OriginalAmount = tax.Payment.TaxAssessedAmount.Value;
				}
				else if (tax.AdditionalTaxTypeCode.Value == RefundDeterminationCodeList.Codes._12)
				{
					dutyTaxfee.SupplementaryAmount = tax.Payment.TaxAssessedAmount.Value;
				}
				else if (tax.AdditionalTaxTypeCode.Value == RefundDeterminationCodeList.Codes._13)
				{
					dutyTaxfee.DifferenceAmount = tax.Payment.TaxAssessedAmount.Value;
				}
				dutyTaxfee.DutyTaxType = tax.TypeCode.Value;
			}

			result.DutyTax = dict;

			return result;
		}
	}
}
