using System.Collections.Generic;
using System.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Accounting.Business.ConsolCosting;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class InvoicingBaseBulkConsolCostImporter : InvoiceBulkOperation
	{
		public InvoicingBaseBulkConsolCostImporter(APInvoiceConsolCosting parent)
			: base(parent.Factory)
		{
			this.Parent = parent;
		}

		#region Public Members

		public APInvoiceConsolCosting Parent { get; private set; }

		public void LoadConsolsCollection()
		{
			Consols.Load(Filters.GetQuery());
		}

		public void Import()
		{
			using (Parent.ParentAPInvoice.GetConsolCostImportPopupSuspender())
			using (Parent.ConsolCosts.SuspendListChanged())
			using (Parent.ConsolSummary.UpdateSuspender.GetSuspender())
			using (CreditChecker.GetCreditLimitValidationSuspender(Factory))
			{
				Factory.SuspendValidation();
				try
				{
					var consolCostsSelectedForImport = GetAllConsolCostsSelectedForImport();
					var accrualCalculator = Factory.GetCachedValue(ConsolAndCostAccrualCalculator.ConsolAndCostAccrualCalculatorKey,
																() => { return new ConsolAndCostAccrualCalculator(Factory, Parent.ParentAPInvoice.AH_OH); } );
					accrualCalculator.PendingConsolCosts.Clear();
					accrualCalculator.PendingConsolCosts.AddRange(consolCostsSelectedForImport);	// do not run populate SQL yet, postpone until required (e.g. GUI requires it)

					foreach (JobConsolCost consolCost in consolCostsSelectedForImport)
					{
						JobConsolCost newConsolCost = Parent.ConsolCosts.AddNew();
						using (new JobConsolCost.ConsolChangedSuspender(newConsolCost))
						{
							using (newConsolCost.ReportSettingParentSuspender.GetSuspender())
							{
								newConsolCost.SetE6_ParentIDAndE6_ParentTableCodeTogether(consolCost.E6_ParentID, consolCost.E6_ParentTableCode);
							}
							newConsolCost.PopulateMissingApportionments(consolCost);
						}
						new InvoicingBaseConsolCostImporter(Factory, newConsolCost, Parent.ParentAPInvoice).ImportCostsIntoCosting(new BusinessObject[] { consolCost });
					}
				}
				finally
				{
					Factory.ResumeValidation();
				}
			}
		}

		#endregion

		#region GUI Bindable Members

		#region SelectedTotal

		[ReadOnly(true)]
		public ZDecimal SelectedTotal { get; private set; }

		public ZPropertyInfo SelectedTotalInfo
		{
			get { return GetZPropertyInfo(nameof(SelectedTotal)); }
		}

		public void UpdateSelectedTotal()
		{
			ZDecimal result = 0m;
			foreach (JobConsolCost consolCost in GetAllConsolCostsSelectedForImport())
			{
				if (consolCost.E6_RX_NKCurrency == Parent.ParentAPInvoice.AH_RX_NKTransactionCurrency)
				{
					result += consolCost.E6_OSCostAmount;
				}
				else
				{
					decimal amountInInvoiceCurrency;
					if (Parent.ParentAPInvoice.AH_RX_NKTransactionCurrency == GlbCompany.CurrentCompany.GC_RX_NKLocalCurrency)
					{
						amountInInvoiceCurrency = Env.CurrentCompany.ExchangeRate.ForeignToLocal(consolCost.E6_OSCostAmount, consolCost.E6_ExchangeRate);
					}
					else
					{
						ZDecimal exRate = Parent.ParentAPInvoice.AH_ExchangeRate;
						if (Parent.ParentAPInvoice.AH_PostedToEFT)
						{
							exRate = consolCost.GetExchangeRateBasedOnJobBillingExchangeRateConfiguration(Parent.ParentAPInvoice.TransactionCurrency);
							if (exRate == 0m)
							{
								exRate = Parent.ParentAPInvoice.AH_ExchangeRate;
							}
						}
						decimal localAmount = Env.CurrentCompany.ExchangeRate.ForeignToLocalWithoutRounding(consolCost.E6_OSCostAmount, consolCost.E6_ExchangeRate);
						amountInInvoiceCurrency = Env.CurrentCompany.ExchangeRate.LocalToForeign(localAmount, exRate, (string)Parent.ParentAPInvoice.AH_RX_NKTransactionCurrency);
					}
					result += amountInInvoiceCurrency;
				}
			}

			SelectedTotal = result;
			SelectedTotalInfo.RefreshBinding();
		}

		#endregion

		#region Filters

		public new InvoicingBaseBulkConsolCostImporterFilters Filters
		{
			get { return (InvoicingBaseBulkConsolCostImporterFilters)FiltersCore; }
		}

		protected override InvoiceBulkOperationFilters GetNewFilters()
		{
			InvoicingBaseBulkConsolCostImporterFilters result = new InvoicingBaseBulkConsolCostImporterFilters(Parent.ParentAPInvoice);
			result.ConsolCostPKsToExclude.AddRange(Parent.ConsolCosts.GetPKs());
			return result;
		}

		#endregion

		#region Consols

		InvoicingBaseConsolCollectionForImporting fConsols;
		public InvoicingBaseConsolCollectionForImporting Consols
		{
			get { return fConsols ?? (fConsols = new InvoicingBaseConsolCollectionForImporting(this, new BusinessObjectFactory())); }
		}

		#endregion

		#region ConsolsFilteredByViewingPermission

		FilteredInvoicingBaseConsolCollectionForImportingView fConsolsFilteredByViewingPermission;
		public FilteredInvoicingBaseConsolCollectionForImportingView ConsolsFilteredByViewingPermission
		{
			get
			{
				if (fConsolsFilteredByViewingPermission == null && Consols != null)
				{
					fConsolsFilteredByViewingPermission = new FilteredInvoicingBaseConsolCollectionForImportingView(Consols);
				}

				return fConsolsFilteredByViewingPermission;
			}
		}

		#endregion

		#endregion

		#region Implementation

		List<JobConsolCost> GetAllConsolCostsSelectedForImport()
		{
			List<JobConsolCost> result = new List<JobConsolCost>();
			foreach (InvoicingBaseConsolForImporting consol in ConsolsFilteredByViewingPermission)
			{
				result.AddRange(consol.GetConsolCostsSelectedForImport());
			}
			return result;
		}

		#endregion
	}
}
