using System;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5TV;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;
using Constants = Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5TVDataProvider
	{
		public GOVCBR5TVMessageData GetMessageData(BusinessObjectFactory factory, TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);
			var result = new GOVCBR5TVMessageData(factory);

			result.NoticeNumber = response.Declaration.Id.Value;
			result.FormattedNoticeNumber = MessageFunctions.GetFormattedNumber(response.Declaration.Id.Value, new int[] { 0, 3, 5 });
			result.CustomsOfficeAndCustomsDivision = response.Declaration.DeclarationOfficeId.Value;
			result.CustomsManagerName = response.Authenticator.Name?.Value ?? ZString.Empty;
			result.CustomsPersonName = response.Authenticator.Contact?.Name?.Value ?? ZString.Empty;
			result.CustomsPersonPhoneNumber = response.Authenticator.Communication.Id.Value;
			result.ImportDeclarationNumber = response.Declaration.PreviousDocument.Id.Value;
			if (DateTime.TryParseExact(response.Declaration.PreviousDocument.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.ImportDeclarationDate = (ZDate)dt;
			}
			result.EntryLineCount = (ZInt)response.Declaration.LoadingListQuantity.Value;
			result.TotalCustomsDisbursementDifferenceAmount = response.Declaration.DutyTaxFee.DifferenceAmount.Value;
			result.CorrectionReason = response.Declaration.Reason.Value;
			if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.LimitDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt1))
			{
				result.CorrectionDate = (ZDate)dt1;
			}
			if (DateTime.TryParseExact(response.IssueDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt2))
			{
				result.NoticeDate = (ZDate)dt2;
			}
			result.CustomsOfficeName = MessageFunctions.GetCustomsOffice(factory, response.Declaration.DeclarationOfficeId.Value.Substring(0, 3));
			result.CustomsDepartmentName = MessageFunctions.GetCustomsDepartment(factory, response.Declaration.DeclarationOfficeId.Value.Substring(3, 2));
			result.NoticeCustomsOffice = response.Declaration.ResponsibleGovernmentAgency.Id.Value;
			result.PayerCompanyName = response.Declaration.Payer.Name.Value;
			result.PayerRepresentativeName = response.Declaration.Payer.Contact.Name.Value;
			result.AttachedDocumentName = response.Declaration.AttachedDocument?.Name?.Value ?? ZString.Empty;
			result.DeclarantID = response.Declaration.Submitter?.Id?.Value ?? ZString.Empty;

			var tariffFormatter = new TariffFormatter();
			foreach (var goodsShipment in response.Declaration.GoodsShipment)
			{
				foreach (var dutytax in goodsShipment.DutyTaxFee)
				{
					switch (dutytax.TypeCode.Value)
					{
						case EntryTaxTypeList.Codes.CUD:
							result.TotalDutyDifferenceAmount += dutytax.DifferenceAmount.Value;
							break;
						case EntryTaxTypeList.Codes.VAT:
							result.TotalDifferenceVAT += dutytax.DifferenceAmount.Value;
							break;
						case EntryTaxTypeList.Codes.IND:
							result.TotalIndividualConsumptionDifferenceTax += dutytax.DifferenceAmount.Value;
							break;
						case EntryTaxTypeList.Codes.ACT:
							result.TotalLiquorDifferenceTax += dutytax.DifferenceAmount.Value;
							break;
						case EntryTaxTypeList.Codes.CAP:
							result.TotalSpecialAgriculturalDifferenceTax += dutytax.DifferenceAmount.Value;
							break;
						case EntryTaxTypeList.Codes.ENV:
							result.TotalTransportationDifferenceTax += dutytax.DifferenceAmount.Value;
							break;
						case EntryTaxTypeList.Codes._5AB:
							result.TotalEducationDifferenceTax += dutytax.DifferenceAmount.Value;
							break;
					}
				}
				foreach (var commodity in goodsShipment.GovernmentAgencyGoodsItem.Commodity)
				{
					var lineMessageData = result.Lines.AddNew();
					lineMessageData.EntryLineNo = (ZShort)goodsShipment.SequenceNumeric;
					lineMessageData.BeforeOrAfterAmendmentIndicator = commodity.SequenceNumeric == 1 ? (int)Constants.BeforeOrAfterAmendment.Before : (int)Constants.BeforeOrAfterAmendment.After;
					lineMessageData.HSCode = tariffFormatter.DisplayFormat(commodity.Classification.Id.Value);
					lineMessageData.DutyRate = commodity.DutyTaxFee.TaxRateNumeric;
					lineMessageData.CustomsValueKRW = commodity.DutyTaxFee.AdValoremTaxBaseAmount.Value;
					lineMessageData.LinesTotalTax = commodity.DutyTaxFee.Payment.TaxAssessedAmount.Value;

					foreach (var dutyTax in goodsShipment.DutyTaxFee)
					{
						switch (dutyTax.TypeCode.Value)
						{
							case EntryTaxTypeList.Codes.CUD:
								lineMessageData.DutyAmount += GetTaxAmount(dutyTax, lineMessageData.BeforeOrAfterAmendmentIndicator);
								break;
							case EntryTaxTypeList.Codes.VAT:
								lineMessageData.VAT += GetTaxAmount(dutyTax, lineMessageData.BeforeOrAfterAmendmentIndicator);
								break;
							case EntryTaxTypeList.Codes.IND:
								lineMessageData.IndividualConsumptionTax += GetTaxAmount(dutyTax, lineMessageData.BeforeOrAfterAmendmentIndicator);
								break;
							case EntryTaxTypeList.Codes.ACT:
								lineMessageData.LiquorTax += GetTaxAmount(dutyTax, lineMessageData.BeforeOrAfterAmendmentIndicator);
								break;
							case EntryTaxTypeList.Codes.CAP:
								lineMessageData.SpecialAgriculturalTax += GetTaxAmount(dutyTax, lineMessageData.BeforeOrAfterAmendmentIndicator);
								break;
							case EntryTaxTypeList.Codes.ENV:
								lineMessageData.TransportationTax += GetTaxAmount(dutyTax, lineMessageData.BeforeOrAfterAmendmentIndicator);
								break;
							case EntryTaxTypeList.Codes._5AB:
								lineMessageData.EducationTax += GetTaxAmount(dutyTax, lineMessageData.BeforeOrAfterAmendmentIndicator);
								break;
						}
					}
				}
			}

			return result;
		}

		static ZDecimal GetTaxAmount(ResponseDeclarationGoodsShipmentDutyTaxFee duty, ZInt amendment)
		{
			return amendment == 1 ? duty.Payment.PaymentAmount.Value : duty.Payment.TaxAssessedAmount.Value;
		}
	}
}
