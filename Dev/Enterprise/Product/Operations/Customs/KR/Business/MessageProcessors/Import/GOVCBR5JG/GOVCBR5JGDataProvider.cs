using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5JG;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5JGDataProvider
	{
		public IGOVCBR5JGMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBR5JGMessageData();

			result.PaymentNumber = response.Declaration.Id.Value;
			result.ImportCompanyName = response.Declaration.Payer.Name.Value;
			result.ImportRepresentativeName = response.Declaration.Payer.Contact.Name.Value;
			result.ImportAddressLine1 = response.Declaration.Payer.Address?.Line?.Value;
			result.CustomsOffice = response.Declaration.PaymentOffice.Id.Value;

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.PaymentDate = (ZDate)dt;
			}
			result.DutyTaxFeeType = response.Declaration.DutyTaxFee.TypeCode.Value;
			result.BankAccountNumber = response.Declaration.BankAccount.Id.Value;
			result.CustomsOfficeBankAccountNumber = response.Declaration.BankAccount.ReferenceId.Value;
			result.NoticeAmount = response.Declaration.InvoiceAmount.Value;
			result.TemporaryOpeningFee = response.Declaration.DutyTaxFee.Payment.TaxAssessedAmount.Value;
			result.InspectionFee = response.Declaration.DutyTaxFee.Payment.PaymentAmount.Value;
			if (result.DutyTaxFeeType == NonTaxIncomeFeeTypeCodeList.Codes._11)
			{
				result.PermissionApplicationFee = response.Declaration.InvoiceAmount.Value;
			}
			result.ElectronNoticeNumber = response.Declaration.SequenceNumeric.ToString();
			result.CarrierID = response.Declaration.Carrier?.Id?.Value;
			result.AgentID = response.Declaration.Agent?.Id?.Value;
			result.DeclarantID = response.Declaration.Submitter?.Id?.Value;

			return result;
		}
	}
}
