using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Business.JobInvoicing;
using Enterprise.Accounting.Business.JobInvoicing.Posting;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;

namespace Enterprise.Client.EDI.Billing.Business
{
	public class EDIARInvoiceCharge : IReceivablesPostingCharge
	{
		public EDIARInvoiceCharge(ARInvoiceLine invoiceLine)
		{
			InvoiceLine = invoiceLine;
			Invoice = invoiceLine.Invoice as ARInvoice;
		}

		public ARInvoiceLine InvoiceLine { get; private set; }

		public ARInvoice Invoice { get; private set; }

		public ZDecimal OsExTaxAmount => 0;

		public ZDecimal OsTaxAmount => 0;

		public RefCurrency SellCurrency => Invoice.TransactionCurrency;

		public OrgHeader Debtor => Invoice.Header;

		public ZGuid DebtorAddressPK { get; set; }

		public ZGuid DebtorContactPK { get; set; }

		public ZGuid TaxRate => InvoiceLine.AL_AT;

		public ZDecimal OSSellAmount => InvoiceLine.AL_OSExTaxAmount;

		public ZDecimal LocalSellAmount { get => InvoiceLine.AL_LocalExTaxAmount; set => throw new NotImplementedException(); }

		public ZGuid SellGSTRate => InvoiceLine.AL_AT;

		public ZDate SellTaxDate { get => InvoiceLine.AL_TaxDate; set => throw new NotImplementedException(); }

		public ZGuid SellTaxMessage => ZGuid.Empty;

		public ZGuid SellWHTRate => InvoiceLine.AL_AW;

		public bool PreventInvoicePrintGrouping => InvoiceLine.AL_PreventInvoicePrintGrouping;

		public ZDecimal SellExchangeRate { get => InvoiceLine.AL_ExchangeRate; set => throw new NotImplementedException(); }

		public ZDecimal InvoiceSellExchangeRate => Invoice.AH_ExchangeRate;

		public ZDecimal OSSellTaxAmount => InvoiceLine.AL_OSTaxAmount;

		public ZDecimal LocalSellTaxAmount => InvoiceLine.AL_LocalTaxAmount;

		public ZDecimal OSSellWHTAmount => InvoiceLine.AL_OSWHTAmount;

		public ZDecimal CFXAmount => 0;

		public ZBool IsLocalClientCharge => false;

		public ZBool IsAgentCharge => false;

		public ZBool IsDeferredCharge => InvoiceTypeCalculationProvider.IsDeferredInvoiceType(InvoiceType);

		public ZBool IsRevenuePosted => false;

		public ZBool IsParentJobWorkOnHold => false;

		public ZBool IsParentJobInvoicingOnHold => false;

		public ZDateTime ARInvoiceDate => Invoice.InvoiceDate;

		public ZDateTime ARInvoiceTaxDate => Invoice.InvoiceTaxDate;

		public ZString SellReference => "";

		public ZShort TaxRatePostingGroupId => InvoiceLine.TaxRate?.AT_PostingGroupId ?? ZShort.Zero;

		public bool IsCommentChargeCode => InvoiceLine.IsCommentCharge;

		public bool HasErrors => InvoiceLine.HasErrors;

		public bool IsAllowedToPostSellCharge => true;

		public ZString GovtChargeCode => InvoiceLine.AL_GovtChargeCode;

		public ZString SellPlaceOfSupply => InvoiceLine.AL_PlaceOfSupply;

		public ZString SellSupplyType => InvoiceLine.AL_SupplyType;

		public ZGuid SellTaxBranch => InvoiceLine.AL_GB_TaxBranch;

		public ZString InvoiceType { get => Invoice.AH_TransactionCategory; set => throw new NotImplementedException(); }

		public ZString ChargeType => InvoiceLine.ChargeCode?.AC_ChargeType ?? ZString.Empty;

		public ZString ChargeGroup => InvoiceLine.ChargeCode?.AC_ChargeGroup ?? ZString.Empty;

		public bool IsDisbursementCharge => ChargeType == Core.Constants.ChargeType.Disbursement;

		public IPostingJob Job => null;

		public ZGuid ChargeCode => InvoiceLine.AL_AC;

		public bool BillInLocalCurrency => InvoiceLine.AL_RX_NKTransactionCurrency == Invoice.Company.GC_RX_NKLocalCurrency;

		public ZGuid Branch => InvoiceLine.AL_GB;

		public ZGuid Department => InvoiceLine.AL_GE;

		public ZShort DisplaySequence => InvoiceLine.AL_Sequence;

		public void CreateCFXTransactionLine(JCJournalHeader cFXHeader, ZDateTime postingTime) { }
		public void InitializeSellAddressContact() { }
		public bool IsSisterCompanyCharge(bool shouldApplyLocalCompanyFilter) => false;
		public void SetRevenueTransactionLine(InvoicingLineBase line) { }
		public IDisposable SuspendAutoCalculations() => null;
		public void ValidateAll() { }

		#region ISellComplianceDescription

		ZString ISellComplianceDescription.Description { get => InvoiceLine.AL_Desc; set => throw new NotImplementedException(); }

		ZString ISellComplianceDescription.SellComplianceDescription => ZString.Empty;

		#endregion
	}
}
