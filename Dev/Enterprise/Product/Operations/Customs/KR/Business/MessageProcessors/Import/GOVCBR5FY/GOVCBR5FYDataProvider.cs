using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FY;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5FYDataProvider
	{
		public IGOVCBR5FYMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5FYMessageData();

			result.SumPaymentNumber = response.Declaration.Id.Value;
			result.BankAccountNumber = response.BankAccount.Id.Value;
			result.ImporterID = response.Declaration.Payer.Id.Value;
			result.ImporterType = response.Declaration.Payer.RoleCode.Value;
			result.ImporterCompanyName = response.Declaration.Payer.Name.Value;
			result.ImporterRepresentativeName = response.Declaration.Payer.Contact.Name.Value;
			result.ImporterAddressLine1 = response.Declaration.Payer.Address.Line.Value;
			result.DeclarantID = response.Declaration.Submitter.Id.Value;
			result.CustomsOfficeName = response.Declaration.PaymentOffice.Name.Value;

			if (DateTime.TryParseExact(response.Declaration.ExpirationDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.ExpirationDate = new ZDate(dt);
			}

			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.NoticeDate = new ZDate(dt1);
			}

			if (response.Declaration.DutyTaxFee != null)
			{
				foreach (var totalDuty in response.Declaration.DutyTaxFee)
				{
					switch (totalDuty.TypeCode.Value)
					{
						case EntryTaxTypeList.Codes.CUD:
							result.TotalDutyAmount = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
						case EntryTaxTypeList.Codes.VAT:
							result.TotalVAT = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
						case EntryTaxTypeList.Codes.ACT:
							result.TotalLiquorTax = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
						case EntryTaxTypeList.Codes.CAP:
							result.TotalAgricultureTax = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
						case EntryTaxTypeList.Codes.IND:
							result.TotalSpecialConsumptionTax = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
						case EntryTaxTypeList.Codes._5AA:
							result.TotalTransportationTax = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
						case EntryTaxTypeList.Codes._5AB:
							result.TotalEducationTax = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
						case EntryTaxTypeList.Codes._5AT:
							result.PenaltyForLateDeclaration = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
						case EntryTaxTypeList.Codes._5AK:
							result.PenaltyForMissedDeclarationb = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
						case EntryTaxTypeList.Codes._5CF:
							result.TotalPayableAmount = totalDuty.Payment.TaxAssessedAmount.Value;
							break;
					}
				}
			}

			if (response.Declaration.GoodsShipment != null)
			{
				var goodShipments = new List<GOVCBR5FYDeclaration>();
				foreach (var goodShipment in response.Declaration.GoodsShipment)
				{
					var totalDutyTax = new GOVCBR5FYDeclaration();
					totalDutyTax.ImportDeclarationNumber = goodShipment.GovernmentAgencyGoodsItem?.PreviousDocument?.Id?.Value ?? ZString.Empty;
					totalDutyTax.SequenceNumber = (ZShort)goodShipment.SequenceNumeric;
					totalDutyTax.PaymentReferenceNumber = goodShipment.AdditionalDocument.Id.Value;
					var dutyTaxes = new List<DutyTax>();
					if (goodShipment.DutyTaxFee != null)
					{
						foreach (var dutyTaxFee in goodShipment.DutyTaxFee)
						{
							if (dutyTaxFee.TypeCode.Value == EntryTaxTypeList.Codes._5CZ)
							{
								totalDutyTax.TotalDutyAndTax = dutyTaxFee.Payment?.TaxAssessedAmount?.Value ?? ZDecimal.Zero;
							}
							else
							{
								var dutyTax = new DutyTax();

								dutyTax.DutyTaxFee = dutyTaxFee.Payment?.TaxAssessedAmount?.Value ?? ZDecimal.Zero;
								dutyTax.DutyTaxType = dutyTaxFee.TypeCode?.Value ?? ZString.Empty;

								dutyTaxes.Add(dutyTax);
							}
						}
					}
					totalDutyTax.DutyTaxes = dutyTaxes.Count > 0 ? dutyTaxes.ToArray() : null;
					goodShipments.Add(totalDutyTax);
				}
				result.TotalDutyTax = goodShipments.Count > 0 ? goodShipments.ToArray() : null;
			}

			return result;
		}
	}
}
