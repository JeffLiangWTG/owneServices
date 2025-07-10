
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.Customs.CA.Business;

namespace Enterprise.Customs.CA.Registry
{
	public partial class EntryChargeTypeList
	{
		public static Dictionary<ZString, ZString> GetPaymentParty(ZString direct)
		{
			var result = new Dictionary<ZString, ZString>();
			switch (direct)
			{
				case ARLDailyNoticeDocumentWrapper.DailyAccounting.TransactionDetails.IsImporterDirectPaymentIdentifier:
					result.Add(Codes.CustomsValueForTax, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.CustomsValueInInvoiceCurrency, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.Duty1, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.Duty2, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.Duty3, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.K84LateFilingPenalty, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.Others, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.TotalDutyAmount, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.TotalExciseTaxAmount, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.TotalGSTAmount, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.TotalGSTDirectAmount, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.TotalNonBillableSIMAAmount, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.TotalSIMAAmount, PaymentPartyCodeDescriptionList.Codes.Importer);
					break;
				case ARLDailyNoticeDocumentWrapper.DailyAccounting.TransactionDetails.IsGSTDirectPaymentIdentifier:
					result.Add(Codes.TotalGSTAmount, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.TotalGSTDirectAmount, PaymentPartyCodeDescriptionList.Codes.Importer);
					result.Add(Codes.CustomsValueForTax, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.CustomsValueInInvoiceCurrency, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.Duty1, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.Duty2, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.Duty3, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.K84LateFilingPenalty, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.Others, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalDutyAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalExciseTaxAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalNonBillableSIMAAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalSIMAAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					break;
				default:
					result.Add(Codes.CustomsValueForTax, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.CustomsValueInInvoiceCurrency, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.Duty1, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.Duty2, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.Duty3, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.K84LateFilingPenalty, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.Others, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalDutyAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalExciseTaxAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalGSTAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalGSTDirectAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalNonBillableSIMAAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					result.Add(Codes.TotalSIMAAmount, PaymentPartyCodeDescriptionList.Codes.Broker);
					break;
			}
			return result;
		}
	}
}
