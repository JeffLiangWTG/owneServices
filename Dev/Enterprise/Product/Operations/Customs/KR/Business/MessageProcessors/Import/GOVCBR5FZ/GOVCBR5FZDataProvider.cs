using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CargoWise.Customs.KR.MessageDefinitions.GOVCBR5FZ;
using CargoWise.Types;
using Enterprise.Customs.KR.Messaging;
using static Enterprise.Customs.KR.Messaging.Constants;

namespace Enterprise.Customs.KR.Business
{
	class GOVCBR5FZDataProvider
	{
		public IGOVCBR5FZMessageData GetMessageData(TextReader messageTextReader)
		{
			var response = KRXmlObjectSerializer.Deserialize<Response>(messageTextReader);

			var result = new GOVCBR5FZMessageData();

			result.TaxInvoiceCode = response.Declaration.AdditionalInformation.StatementCode.Value;
			result.TaxInvoiceType = response.Declaration.AdditionalInformation.StatementTypeCode.Value;
			result.TaxInvoiceNumber = response.Declaration.Id.Value;
			result.SumPaymentNumber = response.Declaration.PreviousDocument?.Id?.Value ?? ZString.Empty;
			result.CustomsOfficeID = response.Authenticator.Id.Value;
			result.CustomsOfficeName = response.Authenticator.Name.Value;
			result.CustomsOfficeAddressLine1 = response.Authenticator.Address.Line.Value;
			result.ImporterID = response.Declaration.Payer.Id.Value;
			result.ImporterType = response.Declaration.Payer.Id.SchemeAgencyId.ToString();
			result.ImporterCompanyName = response.Declaration.Payer.Name?.Value ?? ZString.Empty;
			result.ImporterRepresentativeName = response.Declaration.Payer.Contact?.RepresentativeName?.Value ?? ZString.Empty;
			result.ImporterAddressLine = response.Declaration.Payer.Address?.Line?.Value ?? ZString.Empty;
			if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.PaymentDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime dt))
			{
				result.PaymentDate = new ZDate(dt);
			}
			result.BlankCount = response.Declaration.AdditionalInformation.Content.Value;
			result.TotalVATBaseAmount = response.Declaration.DutyTaxFee.AdValoremTaxBaseAmount.Value;
			result.TotalVAT = response.Declaration.DutyTaxFee.Payment.TaxAssessedAmount.Value;
			if (response.Declaration.AdditionalInformation.PeriodDateTime != null)
			{
				if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.PeriodDateTime.Substring(0, 8), DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime startDate))
				{
					result.PeriodStartDate = new ZDate(startDate);
				}
				if (DateTime.TryParseExact(response.Declaration.AdditionalInformation.PeriodDateTime.Substring(8), DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime endDate))
				{
					result.PeriodEndDate = new ZDate(endDate);
				}
			}
			result.Remark = response.AdditionalInformation?.Content?.Value ?? ZString.Empty;
			result.TotalNumber = response.Declaration.LoadingListQuantity.Value;

			var goodsShipments = new List<GOVCBR5FZDeclaration>();
			foreach (var goodsShipment in response.Declaration.GoodsShipment)
			{
				var declaration = new GOVCBR5FZDeclaration();

				foreach (var additionalDocument in goodsShipment.AdditionalDocument)
				{
					if (additionalDocument.TypeCode.Value == ElectronicDocumentTypeList.Codes._929)
					{
						declaration.ImportDeclarationNumber = additionalDocument.Id.Value;
					}
					else
					{
						declaration.PaymentNumber = additionalDocument.Id.Value;
					}
				}
				declaration.SequenceNumber = (ZShort)goodsShipment.SequenceNumeric;

				if (DateTime.TryParseExact(goodsShipment.AdditionalInformation.PaymentDateTime, DateFormatType.Date, null, DateTimeStyles.AssumeLocal, out DateTime date))
				{
					declaration.PaymentDate = new ZDate(date);
				}

				declaration.VATBaseAmount = goodsShipment.DutyTaxFee.AdValoremTaxBaseAmount.Value;
				declaration.Vat = goodsShipment.DutyTaxFee.Payment.TaxAssessedAmount.Value;

				goodsShipments.Add(declaration);
			}
			result.Declarations = goodsShipments.ToArray();

			return result;
		}
	}
}
