using System;
using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Accounting;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public interface IPostingCharge
	{
		ZString InvoiceType { get; set; }
		ZString ChargeType { get; }
		ZString ChargeGroup { get; }
		bool IsDisbursementCharge { get; }
		IPostingJob Job { get; }
		ZGuid ChargeCode { get; }
		bool BillInLocalCurrency { get; }
		ZGuid Branch { get; }
		ZGuid Department { get; }
		ZShort DisplaySequence { get; }
		IDisposable SuspendAutoCalculations();
	}

	public interface IReceivablesTaxAmountCalculation
	{
		ZDecimal OsExTaxAmount { get; }
		ZDecimal LocalExTaxAmount { get; }
		ZDecimal OsGSTAmount { get; }
		ZDecimal OsExtraTaxAmount { get; }
		ZDecimal LocalGSTAmount { get; }
		ZDecimal LocalExtraTaxAmount { get; }
		ZDecimal OsTaxAmount { get; }
		ZDecimal LocalTaxAmount { get; }
		AccTaxRate GSTRate { get; }
		ZDecimal Rate { get; }
		ZDecimal EffectiveExtraRate { get; }
		bool ShouldExclude { get; }
		void RecalculateOsTaxAmount();
		void AdjustOsTaxAmount(ZDecimal amountToAdjust, bool adjustOSTaxOnly = false);
		void AdjustLocalTaxAmount(ZDecimal amountToAdjust);
		void AdjustOsExtraTaxAmount(ZDecimal amountToAdjust);
		void AdjustLocalExtraTaxAmount(ZDecimal amountToAdjust);
	}

	public interface IReceivablesPostingCharge : IPostingCharge, ISellComplianceDescription
	{
		ZDecimal OsExTaxAmount { get; }
		ZDecimal OsTaxAmount { get; }
		RefCurrency SellCurrency { get; }
		OrgHeader Debtor { get; }
		ZGuid DebtorAddressPK { get; set; }
		ZGuid DebtorContactPK { get; set; }
		ZGuid TaxRate { get; }
		ZDecimal OSSellAmount { get; }
		ZDecimal LocalSellAmount { get; set; }
		ZGuid SellGSTRate { get; }
		ZDate SellTaxDate { get; set; }
		ZGuid SellTaxMessage { get; }
		ZGuid SellWHTRate { get; }
		bool PreventInvoicePrintGrouping { get; }
		ZDecimal SellExchangeRate { get; set; }
		ZDecimal InvoiceSellExchangeRate { get; }
		ZDecimal OSSellTaxAmount { get; }
		ZDecimal LocalSellTaxAmount { get; }
		ZDecimal OSSellWHTAmount { get; }
		ZDecimal CFXAmount { get; }
		ZBool IsLocalClientCharge { get; }
		ZBool IsAgentCharge { get; }
		ZBool IsDeferredCharge { get; }
		ZBool IsRevenuePosted { get; }
		ZBool IsParentJobWorkOnHold { get; }
		ZBool IsParentJobInvoicingOnHold { get; }
		ZDateTime ARInvoiceDate { get; }
		ZDateTime ARInvoiceTaxDate { get; }
		ZString SellReference { get; }
		ZShort TaxRatePostingGroupId { get; }
		bool IsCommentChargeCode { get; }
		bool HasErrors { get; }
		void SetRevenueTransactionLine(InvoicingLineBase line);
		void CreateCFXTransactionLine(JCJournalHeader cFXHeader, ZDateTime postingTime);
		void ValidateAll();
		void InitializeSellAddressContact();
		bool IsSisterCompanyCharge(bool shouldApplyLocalCompanyFilter);
		bool IsAllowedToPostSellCharge { get; }
		ZString GovtChargeCode { get; }
		ZString SellPlaceOfSupply { get; }
		ZString SellSupplyType { get; }
		ZGuid SellTaxBranch { get; }
	}

	public interface IPayablesPostingCharge : IPostingCharge
	{
		RefCurrency CostCurrency { get; }
		OrgHeader Creditor { get; }
	}
}
