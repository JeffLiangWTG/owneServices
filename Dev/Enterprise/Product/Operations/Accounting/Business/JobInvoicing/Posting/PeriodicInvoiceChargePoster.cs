using CargoWise.Types;
using Enterprise.Accounting.Business.ARAP.Invoicing;
using Enterprise.Accounting.Registry.Business;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.JobInvoicing.Posting
{
	public class PeriodicInvoiceChargePoster : ChargePoster
	{
		public PeriodicInvoiceChargePoster(PeriodicInvoice periodicInvoice)
			: base(periodicInvoice.Factory)
		{
			PeriodicInvoice = periodicInvoice;
		}

		readonly PeriodicInvoice PeriodicInvoice;

		bool IsMiscInvoicesTotalAlreradyincluded;

		protected override ZDecimal GetPostingExchangeRate(IReceivablesPostingChargeCollection charges)
		{
			ZDecimal result = 0m;
			if (charges.IsBillInLocalCurrency)
			{
				result = 1m;
			}
			else
			{
				ZDecimal foreignTotal = 0m;
				ZDecimal localTotal = 0m;
				foreach (IReceivablesPostingCharge charge in charges)
				{
					foreignTotal += charge.OsExTaxAmount + charge.OsTaxAmount;
					localTotal += charge.LocalSellAmount + charge.LocalSellTaxAmount;
				}
				result = Env.CurrentCompany.ExchangeRate.GetRate(localTotal, foreignTotal);
			}
			return result;
		}

		protected internal override bool OverrideChargeExchangeRatesOnPosting
		{
			get { return false; }
		}

		protected override void SetInvoiceHeaderDetails(IReceivablesPostingChargeCollection charges, InvoicingBase invoice, ZString postingCurrency, ZDecimal postingCurrencyExchangeRate)
		{
			base.SetInvoiceHeaderDetails(charges, invoice, postingCurrency, postingCurrencyExchangeRate);

			invoice.AH_GB = GlbBranch.CurrentBranch.PK;
			invoice.AH_GE = GlbDepartment.CurrentDepartment.PK;

			invoice.AH_Desc = invoice is ARInvoice ?
				AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.ARPeriodicInvoice,
						Res.GetString("a7003ac8-63f5-4028-ae23-10602a59e0be", "AR PERIODIC INVOICE")) :
				AccountingConfigurationRegistry.Instance.GetTransactionDescriptionFromCode(AccountingConstants.VoucherItemRegistryCode.ARPeriodicCreditNote,
						Res.GetString("4dd05c74-2e61-433c-9497-6a8a40089dfb", "AR PERIODIC CREDIT NOTE"));

			ZGuid jobPK = ZGuid.Empty;
			foreach (IReceivablesPostingCharge charge in charges)
			{
				if (charge.Job == null || !charge.Job.PK.IsValid ||
					(jobPK.IsValid && jobPK != charge.Job.PK))
				{
					jobPK = ZGuid.Empty;
					break;
				}
				if (!jobPK.IsValid && charge.Job.PK.IsValid)
				{
					jobPK = charge.Job.PK;
				}
			}
			invoice.AH_JH = jobPK;
		}

		protected override void SetInvoiceLineDetails(InvoicingLineBase line, IReceivablesPostingCharge charge, ZDateTime postTime, ZDate invoiceDate, ZString ledger, ZString postingCurrency, ZDecimal postingCurrencyExchangeRate, IReceivablesPostingChargeCollection charges)
		{
			if (!charge.BillInLocalCurrency)
			{
				line.AL_ExchangeRate = charge.SellExchangeRate;
			}
			base.SetInvoiceLineDetails(line, charge, postTime, invoiceDate, ledger, postingCurrency, postingCurrencyExchangeRate, charges);
			ZInt multiplier = line.InvoiceBase is ARCreditNote ? -1 : 1;
			((AccountingSuspenders.IRunMethodSuspending)line).RunMethodSuspended = true;
			try
			{
				line.AL_LocalExTaxAmount = multiplier * charge.LocalSellAmount;
				if (charge.SellCurrency.RX_Code == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
				{
					line.AL_LocalTaxAmount = multiplier * charge.OSSellTaxAmount;
				}
				else
				{
					line.AL_LocalTaxAmount = multiplier * charge.LocalSellTaxAmount;
				}
			}
			finally
			{
				((AccountingSuspenders.IRunMethodSuspending)line).RunMethodSuspended = false;
			}
		}

		protected override ZDate GetInvoiceDateFromInvoice(InvoicingBase invoice) => PeriodicInvoice?.InvoiceDate.Date ?? ZDate.Today;

		protected override ZDecimal ResultingInvoiceTotalInLocalCurrency(IReceivablesPostingChargeCollection charges)
		{
			ZDecimal totalMiscInvoicesAmount = 0M;
			if (!IsMiscInvoicesTotalAlreradyincluded)
			{
				foreach (InvoicingBase invoice in PeriodicInvoice.SelectedMiscInvoices)
				{
					totalMiscInvoicesAmount += invoice.AH_LocalTotal;
				}
				IsMiscInvoicesTotalAlreradyincluded = true;
			}
			return base.ResultingInvoiceTotalInLocalCurrency(charges) + totalMiscInvoicesAmount;
		}

		protected internal override bool ShouldSetTransactionHeaderBranch => false;

		protected internal override bool ShouldSetConsolidatedInvoiceRef
		{
			get { return false; }
		}
	}
}
