using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FK;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	public class GOVCBR5FKDataProvider
	{
		public GOVCBR5FKMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5FKMessageData(factory);

			if (DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.SubmissionDate = new ZDate(dt);
			}

			if (response.Status?.EffectiveDateTime != null)
			{
				if (DateTime.TryParseExact(response.Status.EffectiveDateTime, DateFormatType.DateTimeNoSecond, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
				{
					result.NoticeDate = dt1;
				}
			}
			result.ImportDeclarationNumber = response.Declaration.Id.Value;
			result.NoticeCode = response.Status?.NameCode?.Value ?? ZString.Empty;
			result.InDepositAmount = response.Declaration.GoodsShipment?.DutyTaxFee?.Payment?.PaymentAmount?.Value ?? ZDecimal.Zero;
			result.DelayPaymentAmount = response.Declaration.GoodsShipment?.DutyTaxFee?.Payment?.DelayPaymentAmount?.Value ?? ZDecimal.Zero;
			result.TotalInterestAndPenalty = response.Declaration.GoodsShipment?.DutyTaxFee?.TotalTaxAmount?.Value ?? ZDecimal.Zero;
			result.NoticeNumber = response.Declaration.GoodsShipment?.DutyTaxFee?.Payment?.ReferenceId?.Value ?? ZString.Empty;
			result.CustomerOfficer = response.Authenticator?.Name?.Value ?? ZString.Empty;
			result.AmendSequence = new ZInt(response.Declaration.VersionId.Value);
			result.TransactionNatureCode = new ZString(response.Declaration.TransactionNatureCode?.Value ?? ZString.Empty).SubstringSafe(0, 1);

			var charges = new List<GOVCBR5FKChargeMessageData>();
			if (response.Declaration.DutyTaxFee != null)
			{
				foreach (var dutyTax in response.Declaration.DutyTaxFee)
				{
					var taxAmount = dutyTax.Payment.TaxAssessedAmount.Value;
					var differenceAmount = dutyTax.DifferenceAmount.Value;
					var chargeMessageData = new GOVCBR5FKChargeMessageData();
					chargeMessageData.TypeCode = dutyTax.TypeCode.Value;
					if (result.TransactionNatureCode == StatementHeaderStatusList.Codes.O)
					{
						chargeMessageData.Amount = taxAmount;
					}
					else
					{
						chargeMessageData.Amount = differenceAmount;
					}
					charges.Add(chargeMessageData);

					switch (dutyTax.TypeCode.Value)
					{
						case EntryTaxTypeList.Codes.CUD:
							result.DutyAmountPayable = taxAmount;
							result.DutyAmountDifference = differenceAmount;
							break;
						case EntryTaxTypeList.Codes.ACT:
							result.LiquorTaxPayable = taxAmount;
							result.LiquorTaxDifference = differenceAmount;
							break;
						case EntryTaxTypeList.Codes.CAP:
							result.AgricultureTaxPayable = taxAmount;
							result.AgricultureTaxDifference = differenceAmount;
							break;
						case EntryTaxTypeList.Codes.ENV:
							result.TransportationTaxPayable = taxAmount;
							result.TransportationTaxDifference = differenceAmount;
							break;
						case EntryTaxTypeList.Codes._5AB:
							result.EducationTaxPayable = taxAmount;
							result.EducationTaxDifference = differenceAmount;
							break;
						case EntryTaxTypeList.Codes.IND:
							result.SpecialConsumptionTaxPayable = taxAmount;
							result.SpecialConsumptionTaxDifference = differenceAmount;
							break;
						case EntryTaxTypeList.Codes.VAT:
							result.VATPayable = taxAmount;
							result.VATDifference = differenceAmount;
							break;
						case EntryTaxTypeList.Codes._5AC:
						case EntryTaxTypeList.Codes._5AY:
						case EntryTaxTypeList.Codes._5AK:
							result.DeclarationPenaltyPayable += taxAmount;
							result.DeclarationPenaltyDifference += differenceAmount;
							break;
					}
				}
				result.Charges = charges.ToArray();
			}

			return result;
		}
	}
}
