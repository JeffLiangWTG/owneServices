using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.Base.Transaction
{
	public interface ILineMatching : IBusiness
	{
		void SetDefaultValues();
		void UpdateOriginalAmounts();
		void ResetAmounts();

		AccChargeCode ChargeCode { get; }
		Charge Charge { get; }

		ZGuid GenericCharge { get; }
		ZGuid AL_JH { get; set; }
		ZGuid AL_GB { get; set; }
		ZGuid AL_GE { get; set; }
		ZDecimal AL_OSExTaxAmount { get; }
		ZDecimal AL_OSTaxAmount { get; }
		ZDecimal AL_OSAmount { get; }
		ZDecimal AL_ExchangeRate { get; }
		ZGuid AL_AT { get; set; }
		ZString AL_RX_NKTransactionCurrency { get; }

		ZString ChargeType { get; }
		ZPropertyInfo ChargeTypeInfo { get; }
		[List("Lookups.TransactionCurrencies")]
		ZString ChargeCurrency { get; }
		ZPropertyInfo ChargeCurrencyInfo { get; }
		ZDecimal ChargeExRate { get; }
		ZPropertyInfo ChargeExRateInfo { get; }
		ZDecimal ChargeAmount { get; }
		ZPropertyInfo ChargeAmountInfo { get; }
		ZDecimal PaidAmountInChargeCurrency { get; set; }
		ZPropertyInfo PaidAmountInChargeCurrencyInfo { get; }

		ZString TransactionNumber { get; }
		ZPropertyInfo TransactionNumberInfo { get; }
		ZString ConsolidatedInvoiceRef { get; }
		ZPropertyInfo ConsolidatedInvoiceRefInfo { get; }
		ZDecimal OutstandingAmount { get; }
		ZPropertyInfo OutstandingAmountInfo { get; }
		ZDecimal LocalOutstandingAmount { get; }
		ZPropertyInfo LocalOutstandingAmountInfo { get; }
		ZDecimal PaidAmount { get; set; }
		ZPropertyInfo PaidAmountInfo { get; }
		ZDecimal LocalPaidAmount { get; }
		ZPropertyInfo LocalPaidAmountInfo { get; }

		ZDecimal OriginalPaidAmount { get; }

		ZBool IsFullyPay { get; set; }
		ZPropertyInfo IsFullyPayInfo { get; }
		RefCurrencyCollection Currencies { get; }
	}
}
