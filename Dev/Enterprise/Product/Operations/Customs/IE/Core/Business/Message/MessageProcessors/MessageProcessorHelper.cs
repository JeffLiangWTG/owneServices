using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Customs.EU.Business;
using Enterprise.Customs.IE.Business.Declaration;
using Enterprise.Customs.IE.Messaging;

namespace Enterprise.Customs.IE.Business
{
	public static class MessageProcessorHelper
	{
		public static void PopulateConfirmedDutiesAndTaxes(CusEntryLine entryLine, IEnumerable<ITaxTypeProvider> taxProviders)
		{
			foreach (var taxProvider in taxProviders)
			{
				var newFee = entryLine.ConfirmedFees.AddNew() as EU.Business.Declaration.CusEntryLineFee;

				newFee.CF_ChargeType = taxProvider.TaxType;
				newFee.CF_MethodOfCalculation = taxProvider.Unit;
				newFee.CF_BaseValue = taxProvider.Quantity > 0m ? taxProvider.Quantity : taxProvider.Amount;
				newFee.CF_Rate = taxProvider.TaxRate;
				newFee.CF_ChargeAmount = taxProvider.TaxAmount;
				newFee.CF_MethodOfPayment = taxProvider.MethodOfPayment;
			}
		}

		public static void PopulateRequestedDocuments(IRequestedDocumentsProvider requestedDocumentsProvider, IEnumerable<IDocumentAdditionalInformationProvider> additionalInformations, ZDateTime requestDate, ZDateTime dateLimit, ZString documentStatus)
		{
			var requestedDocumentCollection = requestedDocumentsProvider.RequestedDocuments;
			var existingRequests = requestedDocumentCollection.Cast<RequestedDocument>().ToDictionary(x => GetKey(x.CSI_Code, x.RequestInformation, x.CSI_DateOfIssue, x.CSI_DateOfExpiry), (y) => y);
			foreach (var additionalInformation in additionalInformations)
			{
				var documentType = additionalInformation.DocumentType;
				var requestInformation = additionalInformation.RequestInformation;
				if (!existingRequests.Remove(GetKey(documentType, requestInformation, requestDate, dateLimit)))
				{
					var requestedDocument = requestedDocumentCollection.AddNew();
					requestedDocument.CSI_Code = documentType;
					requestedDocument.RequestInformation = requestInformation;
					requestedDocument.CSI_DateOfIssue = requestDate;
					requestedDocument.CSI_DateOfExpiry = dateLimit;
					requestedDocument.CSI_Status = documentStatus;
					if (!additionalInformation.CcQualifier.IsEmpty)
					{
						requestedDocument.CSI_RN_NKCountryCode = additionalInformation.CcQualifier;
					}
				}
			}
		}

		static string GetKey(ZString type, ZString requestInformation, ZDateTime requestDate, ZDateTime dateLimit) => $"{type}_{requestInformation}_{requestDate.ToISO8601ShortDateString()}_{dateLimit.ToISO8601ShortDateString()}";
	}
}
