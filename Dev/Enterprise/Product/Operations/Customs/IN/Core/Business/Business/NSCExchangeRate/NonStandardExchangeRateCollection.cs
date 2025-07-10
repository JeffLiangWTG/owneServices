using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.Types;
using Enterprise.Customs.Business;
using Enterprise.Customs.Common.IN;

namespace Enterprise.Customs.IN.Business;

public class NonStandardExchangeRateCollection : CusSupportingInfoCollection<NonStandardExchangeRate>
{
	public NonStandardExchangeRateCollection(JobDeclaration parent) : base(parent, CusSupportingInfoTypeList.Codes.NonStandardCurrency)
	{
		parent.Invoices.CollectionCountChange += (s, e) =>
		{
			if (e.BizObject is JobComInvoiceHeader invoice)
			{
				if (e.ItemAdded)
				{
					UnhookEventOnInvoiceHeader(invoice);
					HookEventOnInvoiceHeader(invoice);
				}
				else if (e.ItemRemoved)
				{
					UnhookEventOnInvoiceHeader(invoice);
				}
				SyncWithInvoiceCurrencies();
			}
		};
		parent.Invoices.Cast<JobComInvoiceHeader>().ForEach(HookEventOnInvoiceHeader);
	}

	void UnhookEventOnInvoiceHeader(JobComInvoiceHeader invoice) => invoice.JZ_RX_NKInvoice_CurrencyInfo.ValueChanged -= SyncWithInvoiceCurrencies;
	void HookEventOnInvoiceHeader(JobComInvoiceHeader invoice) => invoice.JZ_RX_NKInvoice_CurrencyInfo.ValueChanged += SyncWithInvoiceCurrencies;

	JobDeclaration Declaration => Master as JobDeclaration;

	public void SyncWithInvoiceCurrencies(object sender = null, EventArgs e = null)
	{
		var nonStandardCurrencies = Declaration.Invoices.Cast<JobComInvoiceHeader>().Where(x => x.IsNonStandardCurrency).Select(x => x.JZ_RX_NKInvoice_Currency).ToHashSet();
		Where(x => !nonStandardCurrencies.Contains(x.CSI_RX_NKCurrency)).ToList().ForEach(RemoveAndDelete);

		var collectionCurrencies = Select(x => x.CSI_RX_NKCurrency).ToHashSet();
		nonStandardCurrencies.Except(collectionCurrencies).ForEach(x => AddNew().CSI_RX_NKCurrency = x);
	}

	protected override bool AllowNewCore => false;

	protected override bool AllowRemoveCore => false;

	public NonStandardExchangeRate GetByCurrencyCode(ZString currency) => Where(x => x.CSI_RX_NKCurrency == currency).FirstOrDefault();
}
