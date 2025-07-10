using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5UO;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5UODataProvider
	{
		public GOVCBR5UOMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBR5UOMessageData(factory);

			result.RefundDeclarationNumber = response.Declaration.Id.Value;
			result.ImportCompanyName = response.Declaration.Submitter.Name.Value;
			result.ImportRepresentativeName = response.Declaration.Submitter.Contact.RepresentativeName.Value;
			result.FirstImportAddressLine = response.Declaration.Submitter.Address.Description.Value;
			result.SecondImportAddressLine = response.Declaration.Submitter.Address.Line?.Value ?? ZString.Empty;
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
			result.ApprovalNo = response.Declaration.AdditionalDocument.Id.Value;
			result.BankCodeName = response.Declaration.BankAccount.Name.Value;
			result.BankCodeName2 = response.Declaration.BankAccount.BranchName.Value;
			result.BankAccountNumber = response.Declaration.BankAccount.Id.Value;

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt3))
			{
				result.NoticeDate = new ZDate(dt3);
			}

			decimal totalPenaltyOnDutyTax = 0;
			decimal totalInterestAmount = 0;
			foreach (var tax in response.Declaration.DutyTaxFee)
			{
				if (tax.DutyRegimeCode.Value == RefundDeterminationCodeList.Codes._125)
				{
					totalPenaltyOnDutyTax += tax.Payment.TaxAssessedAmount.Value;
				}
				else if (tax.DutyRegimeCode.Value == RefundDeterminationCodeList.Codes._119)
				{
					totalInterestAmount += tax.Payment.TaxAssessedAmount.Value;
				}
				else if (tax.DutyRegimeCode.Value == RefundDeterminationCodeList.Codes._124)
				{
					switch (tax.TypeCode.Value)
					{
						case EntryTaxTypeList.Codes.CUD:
							{
								result.DutyAmount = tax.Payment.TaxAssessedAmount.Value;
								break;
							}
						case EntryTaxTypeList.Codes.IND:
							{
								result.SpecialConsumptionTax = tax.Payment.TaxAssessedAmount.Value;
								break;
							}
						case EntryTaxTypeList.Codes.ENV:
							{
								result.TransportationTax = tax.Payment.TaxAssessedAmount.Value;
								break;
							}
						case EntryTaxTypeList.Codes.ACT:
							{
								result.LiquorTax = tax.Payment.TaxAssessedAmount.Value;
								break;
							}
						case EntryTaxTypeList.Codes._5AB:
							{
								result.EducationTax = tax.Payment.TaxAssessedAmount.Value;
								break;
							}
						case EntryTaxTypeList.Codes.CAP:
							{
								result.AgricultureTax = tax.Payment.TaxAssessedAmount.Value;
								break;
							}
						case EntryTaxTypeList.Codes.VAT:
							{
								result.VAT = tax.Payment.TaxAssessedAmount.Value;
								break;
							}
						case EntryTaxTypeList.Codes._5AC:
						case EntryTaxTypeList.Codes._5AY:
						case EntryTaxTypeList.Codes._5CT:
							{
								result.Penalty += tax.Payment.TaxAssessedAmount.Value;
								break;
							}
						case EntryTaxTypeList.Codes._5CS:
							{
								result.NonDutyTaxRevenue = tax.Payment.TaxAssessedAmount.Value;
								break;
							}
						case EntryTaxTypeList.Codes._5CZ:
							{
								result.TotalAmount = tax.Payment.TaxAssessedAmount.Value;
								break;
							}
					}
				}
			}
			result.RefundAmount = result.TotalAmount - result.Penalty;
			result.Penalty = result.Penalty + totalPenaltyOnDutyTax + totalInterestAmount;

			return result;
		}
	}
}
