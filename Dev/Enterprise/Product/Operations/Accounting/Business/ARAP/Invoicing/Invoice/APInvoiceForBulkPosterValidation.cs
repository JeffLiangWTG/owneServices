using System;
using System.Linq;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class APInvoiceForBulkPosterValidation : APInvoiceValidation
	{
		public APInvoiceForBulkPosterValidation(APInvoiceForBulkPoster parent)
			: base(parent)
		{
		}

		APInvoiceForBulkPoster ParentPoster => Parent as APInvoiceForBulkPoster;

		protected override void CheckAH_OSTaxAmount()
		{
			APInvoiceForBulkPoster aPInvoice = Parent as APInvoiceForBulkPoster;
			if (Math.Sign(aPInvoice.AH_OSExTaxAmount) == -Math.Sign(aPInvoice.AH_OSTaxAmount) && Math.Sign(aPInvoice.AH_OSExTaxAmount) != 0)
			{
				aPInvoice.AH_OSTaxAmountInfo.AddError(Res.GetString("218ba456-82cf-4430-84a5-09eacafc3d3f", "Tax Amount must have the same sign as the Charge Amount."));
			}
		}

		protected override void CheckAH_InvoiceDate()
		{
			base.CheckAH_InvoiceDate();

			if (!Parent.AH_InvoiceDateInfo.HasErrors() && !Parent.IsLocalCurrencyTransaction && ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, Parent.IsLocalCurrencyTransaction, Parent.AH_GC, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnInvoiceDate.Code))
			{
				foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)Parent).ParentCollections)
				{
					if (collection is APInvoiceForBulkPosterCollection )
					{
						if (collection.Any(bizObj =>
						{
							var invoice = bizObj as APInvoiceForBulkPoster;
							return invoice.PK != Parent.PK
								&& invoice.AH_RX_NKTransactionCurrency == Parent.AH_RX_NKTransactionCurrency
								&& invoice.AH_InvoiceDate != Parent.AH_InvoiceDate;
						}))
						{
							Parent.AH_InvoiceDateInfo.AddError(Res.GetString("712ca868-502e-45e8-9321-6be253d0bfac", @"The “AP Invoice Posting Exchange Rate Option” has been set to ""INV"".
All Invoice Dates in this Bulk AP Invoice Posting must be the same to facilitate the application of posting using Exchange Rate set for Invoice Date."));
						}
					}
				}
			}
		}

		protected override void CheckAH_PostDate()
		{
			base.CheckAH_PostDate();

			if (!Parent.AH_InvoiceDateInfo.HasErrors() && !Parent.IsLocalCurrencyTransaction && ExchangeRateCalculator.IsExRateOptionApplicable(ExchangeRateValidLedgerEnum.AP, Parent.IsLocalCurrencyTransaction, Parent.AH_GC, AccountingConstants.InvoicePostingExchangeRateOption.ExchangeRateBasedOnPostDate.Code))
			{
				foreach (BusinessObjectCollection collection in ((IBusinessObjectInternals)Parent).ParentCollections)
				{
					if (collection is APInvoiceForBulkPosterCollection )
					{
						if (collection.Any(bizObj =>
						{
							var invoice = bizObj as APInvoiceForBulkPoster;
							return invoice.PK != Parent.PK
								&& invoice.AH_RX_NKTransactionCurrency == Parent.AH_RX_NKTransactionCurrency
								&& invoice.AH_PostDate != Parent.AH_PostDate;
						}))
						{
							Parent.AH_PostDateInfo.AddError(Res.GetString("1319de9f-abb5-4744-ae3d-1ca5e78b7241", @"The “AP Invoice Posting Exchange Rate Option” has been set to ""PST"".
All Post Dates in this Bulk AP Invoice Posting must be the same to facilitate the application of posting using Exchange Rate set for Post Date."));
						}
					}
				}
			}
		}

		public void ValidateTaxDate() => ValidateCalculatedProperty(ParentPoster.TaxDateInfo);

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0051:Remove unused private members", Justification = "CheckTaxDate is used internally")]
		void CheckTaxDate()
		{
			if (ParentPoster.AccTaxRate == null)
			{
				return;
			}

			MandatoryValidation.CheckEntered(ParentPoster.TaxDateInfo);
			if (!ParentPoster.TaxDateInfo.HasErrors())
			{
				var rateExists = ParentPoster.AccTaxRate.DoesRateExists(ParentPoster.TaxDate);
				if (!rateExists)
				{
					ParentPoster.TaxDateInfo.AddError(Res.GetString("ca1fe78f-d916-4ae6-bca8-ab6d9eb78fe3", "No rate found for selected date."));
				}
			}
		}
	}
}
