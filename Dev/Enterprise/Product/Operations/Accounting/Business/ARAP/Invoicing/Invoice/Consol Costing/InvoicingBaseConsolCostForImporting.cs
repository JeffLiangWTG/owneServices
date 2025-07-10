using System.Data;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseConsolCostForImporting : JobConsolCost
	{
		public InvoicingBaseConsolCostForImporting(BusinessObjectFactory factory, DataRow row)
			: base(factory, row)
		{
		}

		internal InvoicingBaseBulkConsolCostImporter Importer
		{
			get;
			set;
		}

		[DecimalPlaces(nameof(ExchangeRateDecimalPlaces))]
		public ZDecimal ExchangeRateInInvoiceCurrency
		{
			get
			{
				ZDecimal result = 0m;
				if (Importer != null && Importer.Parent != null)
				{
					InvoicingBase parentAPInvoice = Importer.Parent.ParentAPInvoice;
					if (parentAPInvoice != null && parentAPInvoice.TransactionCurrency != null)
					{
						if (E6_RX_NKCurrency == parentAPInvoice.AH_RX_NKTransactionCurrency)
						{
							result = E6_ExchangeRate;
						}
						else
						{
							result = GetExchangeRateBasedOnJobBillingExchangeRateConfiguration(parentAPInvoice.TransactionCurrency);
						}
					}
				}
				return result;
			}
		}

		public ZPropertyInfo ExchangeRateInInvoiceCurrencyInfo
		{
			get { return GetZPropertyInfo(nameof(ExchangeRateInInvoiceCurrency)); }
		}

		#region IsSelectedForImport

		ZBool fIsSelectedForImport;
		public ZBool IsSelectedForImport
		{
			get { return fIsSelectedForImport; }
			set
			{
				SetNonPersistentPropertyValue(IsSelectedForImportInfo, ref fIsSelectedForImport, value);
				Importer.UpdateSelectedTotal();
			}
		}

		public ZPropertyInfo IsSelectedForImportInfo
		{
			get { return GetZPropertyInfo(nameof(IsSelectedForImport)); }
		}

		#endregion
	}
}
