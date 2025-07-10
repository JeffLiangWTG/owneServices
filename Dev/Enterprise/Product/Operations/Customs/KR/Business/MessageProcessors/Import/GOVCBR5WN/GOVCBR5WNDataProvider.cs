using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5WN;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5WNDataProvider
	{
		public GOVCBR5WNMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5WNMessageData(factory);

			if (DateTime.TryParseExact(response.Declaration.JurisdictionDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.DeclarationDate = new ZDate(dt);
			}
			result.ImportDeclarationNumber = response.FunctionalReferenceId.Value;

			if (!string.IsNullOrEmpty(response.Declaration.IssueDateTime) && DateTime.TryParseExact(response.Declaration.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.AmendmentDeclarationDate = new ZDate(dt1);
			}
			else
			{
				result.AmendmentDeclarationDate = ZDate.Empty;
			}
			result.AmendmentVersionNo = response.Declaration.SequenceNumeric == null ? ZInt.Zero : (ZInt)response.Declaration.SequenceNumeric;
			result.AmendmentSequenceNo = response.Declaration.VersionId.Value;

			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.NoticeDate = new ZDate(dt2);
			}
			if (DateTime.TryParseExact(response.AdditionalInformation.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt3))
			{
				result.ProcessedDate = new ZDate(dt3);
			}
			result.PayerCompanyName = response.Declaration.Payer.Name.Value;
			result.PayerRepresentativeName = response.Declaration.Payer.Contact.Name.Value;
			result.PayerAddressLine = response.Declaration.Payer.Address.Line.Value;
			result.CustomsDepartment = response.Declaration.Authenticator.Contact.DepartmentName.Value;
			result.CustomsPersonName = response.Declaration.Authenticator.Contact.Name.Value;
			result.CustomsPrimaryOfficial = response.Declaration.Authenticator.Contact.PrimaryOfficial?.Value ?? ZString.Empty;
			result.CustomsPersonPhoneNumber = response.Declaration.Authenticator.Communication?.Id?.Value ?? ZString.Empty;
			result.TaxAdjustmentReason = response.Amendment.Content.Value;
			result.CustomsOffice = response.Declaration.DeclarationOffice.Value;
			result.NoticeNumber = response.Declaration.Id.Value;
			result.DutyTaxDifference = response.Declaration.AdditionalDocument.AmountAmount.Value;

			var dict = new Dictionary<ZString, ZDecimal>();
			var tariffFormatter = new TariffFormatter();
			foreach (var dutyTaxFee in response.Declaration.DutyTaxFee)
			{
				var deductAmount = dutyTaxFee.DeductAmount.Value;
				dict.Add(dutyTaxFee.TypeCode.Value, deductAmount);
				result.TotalDifferenceAmount += deductAmount;
				switch (dutyTaxFee.TypeCode.Value)
				{
					case EntryTaxTypeList.Codes.CUD:
						result.DutyAmountDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes.IND:
						result.SpecialConsumptionTaxDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes.ENV:
						result.TransportationTaxDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes.ACT:
						result.LiquorTaxDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5AB:
						result.EducationTaxDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes.CAP:
						result.AgriculturalTaxDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes.VAT:
						result.VATDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5AU:
						result.PenaltyOnDutyForUnderDeclarationDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5AV:
						result.PenaltyOnDomesticTaxForUnderDeclarationDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5AW:
						result.PenaltyOnDutyForLatePaymentDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5AX:
						result.PenaltyOnDomesticTaxForLatePaymentDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5AS:
						result.PenaltyOnMissedDeclarationDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5AC:
						result.PenaltyOnLateDeclarationDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5AZ:
						result.PenaltyOnNonCompliantDeclarationDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5BA:
						result.PenaltyOnBreachOfReExportationDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5AY:
						result.PenaltyOnMissedDeclarationForPersonalItemsDifference = deductAmount;
						break;
					case EntryTaxTypeList.Codes._5BB:
						result.PenaltyOnOverdrawbackDifference = deductAmount;
						break;
				}
			}
			result.DutyTax = dict;

			GOVCBR5WNLineMessageData lineMessageData = null;
			foreach (var consignment in response.Declaration.Consignment)
			{
				lineMessageData = result.Lines.AddNew();
				lineMessageData.EntryLineNo = (ZShort)consignment.SequenceNumeric;

				foreach (var consignmentItem in consignment.ConsignmentItem)
				{
					if (consignmentItem.SequenceNumeric == 1)
					{
						lineMessageData.HSCodeBefore = tariffFormatter.DisplayFormat(consignmentItem.Classification.Id.Value);
						lineMessageData.HSDescriptionBefore = consignmentItem.CargoDescription.Value;
						lineMessageData.DescriptionBefore = consignmentItem.Description?.Value ?? ZString.Empty;
						lineMessageData.QuantityBefore = consignmentItem.CountQuantity?.Value ?? ZDecimal.Zero;
					}
					else
					{
						lineMessageData.HSCodeAfter = tariffFormatter.DisplayFormat(consignmentItem.Classification.Id.Value);
						lineMessageData.HSDescriptionAfter = consignmentItem.CargoDescription.Value;
						lineMessageData.DescriptionAfter = consignmentItem.Description?.Value ?? ZString.Empty;
						lineMessageData.QuantityAfter = consignmentItem.CountQuantity?.Value ?? ZDecimal.Zero;
					}

					foreach (var dutyTaxfees in consignmentItem.DutyTaxFee)
					{
						var detailMessageData = lineMessageData.DutyTaxDetails.AddNew();
						detailMessageData.DutyTaxType = dutyTaxfees.TypeCode.Value;
						detailMessageData.BeforeOrAfterAmendmentIndicator = consignmentItem.SequenceNumeric == 1 ? Constants.BeforeOrAfterAmendment.Before : Constants.BeforeOrAfterAmendment.After;
						detailMessageData.BaseValue = new ZDecimal(dutyTaxfees.DutyAssesmentStandard.Value);
						detailMessageData.ReducedOrExemptAmount = dutyTaxfees.DeductAmount?.Value ?? ZDecimal.Zero;
						if (detailMessageData.BeforeOrAfterAmendmentIndicator == Constants.BeforeOrAfterAmendment.After)
						{
							foreach (var additionalInformation in consignment.AdditionalInformation)
							{
								if (detailMessageData.DutyTaxType == additionalInformation.StatementCode.Value)
								{
									detailMessageData.IncreaseDutyTaxAmount = new ZDecimal(additionalInformation.StatementDescription.Value);
									break;
								}
							}
						}
						if (dutyTaxfees.TaxRateNumeric != null)
						{
							detailMessageData.DutyTaxRate = dutyTaxfees.TaxRateNumeric.Value;
						}
						detailMessageData.DutyTaxAmount = dutyTaxfees.Payment.TaxAssessedAmount.Value;

						if (detailMessageData.DutyTaxType == EntryTaxTypeList.Codes.CUD)
						{
							if (detailMessageData.BeforeOrAfterAmendmentIndicator == Constants.BeforeOrAfterAmendment.Before)
							{
								lineMessageData.AdditionalTariffAmountBefore = dutyTaxfees.EmergencyTariffAmount?.Value ?? ZDecimal.Zero;
							}
							else
							{
								lineMessageData.AdditionalTariffAmountAfter = dutyTaxfees.EmergencyTariffAmount?.Value ?? ZDecimal.Zero;
							}
						}
					}
				}
			}
			return result;
		}
	}
}
