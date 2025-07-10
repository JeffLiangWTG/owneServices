using System;
using CargoWise.Common;
using Enterprise.Accounting.Business.Base.Interfaces;

namespace Enterprise.Accounting.Business.Invoicing
{
	public class InvoicingLineTaxDateCacheProvider
	{
		public InvoicingLineTaxDateCacheProvider(ITransactionLineTaxDate transactionLineTaxDate, Func<InvoiceTaxDateCacheProvider> invoiceTaxDateCacheProviderGetter)
		{
			TransactionLineTaxDate = transactionLineTaxDate;
			InvoiceTaxDateCacheProviderGetter = invoiceTaxDateCacheProviderGetter;
		}

		public IDisposable BindChargeEvents()
		{
			var chargeTypeInfo = TransactionLineTaxDate?.ChargeCode?.AC_ChargeTypeInfo;
			if (chargeTypeInfo != null)
			{
				chargeTypeInfo.ValueChanged -= OnChargeTypeChanged;
			}

			Action disposeAction = () =>
			{
				chargeTypeInfo = TransactionLineTaxDate?.ChargeCode?.AC_ChargeTypeInfo;
				if (chargeTypeInfo != null)
				{
					chargeTypeInfo.ValueChanged += OnChargeTypeChanged;
				}
			};

			return new DisposableAction(disposeAction);
		}

		public void StaleCache()
		{
			InvoiceTaxDateCacheProviderGetter?.Invoke()?.StaleCache();
		}

		void OnChargeTypeChanged(object sender, EventArgs e)
		{
			StaleCache();
		}

		readonly ITransactionLineTaxDate TransactionLineTaxDate;
		readonly Func<InvoiceTaxDateCacheProvider> InvoiceTaxDateCacheProviderGetter;
	}
}
