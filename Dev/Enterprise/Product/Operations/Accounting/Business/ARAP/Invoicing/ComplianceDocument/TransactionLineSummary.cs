using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;

namespace Enterprise.Accounting.Business
{
	public class TransactionLineSummary : NonPersistentBusinessObject
	{
		public TransactionLineSummary(InvoicingLineBase invoicingLine) : base(new BusinessObjectFactory())
		{
			if (invoicingLine != null)
			{
				AL_Amount = invoicingLine.AL_LineAmount;
				AL_AC = invoicingLine.ChargeCode?.AC_Code ?? invoicingLine.GLHeader?.AccountNum ?? ZString.Empty;
				AL_AG = invoicingLine.GLHeader?.AccountNum ?? ZString.Empty;
				AL_Desc = invoicingLine.AL_Desc;
				AL_GB = invoicingLine.Branch.GB_Code;
				AL_GE = invoicingLine.Department.GE_Code;
				AL_RX_NKTransactionCurrency = invoicingLine.AL_RX_NKTransactionCurrency;
				AL_ExchangeRate = invoicingLine.AL_ExchangeRate;
				AL_OSExTaxAmount = invoicingLine.AL_OSExTaxAmount;
				AL_AT = invoicingLine.TaxRate.AT_Code;
				AL_TaxDate = invoicingLine.AL_TaxDate;
				PostingGroup = invoicingLine.PostingGroupID;
				AL_LineAmount = invoicingLine.AL_LineAmount;
				AL_OSTaxAmount = invoicingLine.AL_OSTaxAmount;
				AL_LocalTotalAmount = invoicingLine.AL_LocalTotalAmount;
				AL_LocalExTaxAmount = invoicingLine.AL_LocalExTaxAmount;
				Ledger = invoicingLine.TransactionHeader.AH_Ledger;
				TransactionType = invoicingLine.TransactionHeader.AH_TransactionType;
				TransactionNum = invoicingLine.TransactionHeader.AH_TransactionNum;
				AL_Calc_InputGSTVATRecoverablePercentage = invoicingLine.AL_Calc_InputGSTVATRecoverablePercentage;
				AL_OSTaxAmount_Recoverable = invoicingLine.AL_OSTaxAmount_Recoverable;
				AL_OSTaxAmount_NotRecoverable = invoicingLine.AL_OSTaxAmount_NotRecoverable;
				AL_LocalTaxAmount_NotRecoverable = invoicingLine.AL_LocalTaxAmount_NotRecoverable;
				AL_LocalTaxAmount_Recoverable = invoicingLine.AL_LocalTaxAmount_Recoverable;
				AL_OSExtraTaxAmount = invoicingLine.AL_OSExtraTaxAmount;
				AL_LocalTaxAmount = invoicingLine.AL_LocalTaxAmount;
				AL_OverseasTotal = invoicingLine.AL_OverseasTotal;
				AL_GovtChargeCode = invoicingLine.AL_GovtChargeCode;
				AL_A9_VATClass = invoicingLine.VATClass?.A9_Code ?? ZString.Empty;
				TaxReportingBasisHumanReadableName = invoicingLine.TaxReportingBasisHumanReadableName;
				AL_ExchangeRate_Decimals = invoicingLine.ExchangeRateDecimals;
				CurrencyDecimals = invoicingLine.CurrencyDecimals;
				Decimals = invoicingLine.Decimals;
				PercentageDecimals = invoicingLine.PercentageDecimals;
			}
		}

		public int Decimals { get; set; }
		public int CurrencyDecimals { get; set; }
		public int PercentageDecimals { get; set; }
		public ZDecimal AL_ExchangeRate_Decimals { get; set; }
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal AL_Amount { get; set; }
		public ZString AL_AC { get; set; }
		public ZString AL_AG { get; set; }
		public ZString AL_Desc { get; set; }
		public ZString AL_GB { get; set; }
		public ZString AL_GE { get; set; }
		public ZString AL_RX_NKTransactionCurrency { get; set; }
		public ZDecimal AL_ExchangeRate { get; set; }
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSExTaxAmount { get; set; }
		public ZString AL_AT { get; set; }
		public ZDateTime AL_TaxDate { get; set; }
		public ZInt PostingGroup { get; set; }
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal AL_LineAmount { get; set; }
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSTaxAmount { get; set; }
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OverseasTotal { get; set; }
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal AL_LocalTotalAmount { get; set; }
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal AL_LocalExTaxAmount { get; set; }
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal AL_LocalTaxAmount { get; set; }
		public ZString Ledger { get; set; }
		public ZString TransactionType { get; set; }
		public ZString TransactionNum { get; set; }
		[DecimalPlaces(nameof(PercentageDecimals))]
		public ZDecimal AL_Calc_InputGSTVATRecoverablePercentage { get; set; }
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSTaxAmount_Recoverable { get; set; }
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSTaxAmount_NotRecoverable { get; set; }
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal AL_LocalTaxAmount_NotRecoverable { get; set; }
		[DecimalPlaces(nameof(Decimals))]
		public ZDecimal AL_LocalTaxAmount_Recoverable { get; set; }
		[DecimalPlaces(nameof(CurrencyDecimals))]
		public ZDecimal AL_OSExtraTaxAmount { get; set; }
		public ZString AL_GovtChargeCode { get; set; }
		public ZString AL_A9_VATClass { get; set; }
		public ZString TaxReportingBasisHumanReadableName { get; set; }
	}
}
