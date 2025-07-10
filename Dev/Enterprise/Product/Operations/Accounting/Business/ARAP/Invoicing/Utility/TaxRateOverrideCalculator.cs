using System;
using CargoWise.EntityFramework;
using Enterprise.Integration.Accounting;
using Enterprise.MasterFiles.Business;
using static Enterprise.Core.Constants;

namespace Enterprise.Accounting.Business.ARAP.Invoicing
{
	public class TaxRateOverrideCalculator
	{
		public TaxRateOverrideCalculator(BusinessObjectFactory factory, Func<AccChargeTaxOverride> taxRateOverrideProvider)
		{
			this.factory = factory;
			this.taxRateOverrideProvider = taxRateOverrideProvider;
			ReCalculate();
		}

		public void ReCalculate()
		{
			CachedTaxRateOverride = null;

			bool isSkipTaxOverrideForInterCompanyInvoiceImport =
				factory.HasContext(BusinessContext.InterCompanyInvoiceImport) &&
				!AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

			if (!isSkipTaxOverrideForInterCompanyInvoiceImport)
			{
				CachedTaxRateOverride = taxRateOverrideProvider.Invoke();

				if (CachedTaxRateOverride != null)
				{
					if (!factory.HasContext(BusinessContext.InterCompanyInvoiceImport))
					{
						if (CachedTaxRateOverride.AO_TransactionContext == TaxOverrideTransactionContext.Codes.All)
						{
							taxOverrideAction = TaxOverrideAction.NonINTAll;
							return;
						}
					}
					else
					{
						if (CachedTaxRateOverride.AO_TransactionContext == TaxOverrideTransactionContext.Codes.All)
						{
							taxOverrideAction = TaxOverrideAction.INTAll;
							return;
						}
						else if (CachedTaxRateOverride.AO_TransactionContext == TaxOverrideTransactionContext.Codes.IntercompanyInvoiceImport)
						{
							if (CachedTaxRateOverride.AO_DefaultingRule == TaxOverrideDefaultingRule.Codes.SumARAmount)
							{
								taxOverrideAction = TaxOverrideAction.INTSumARAmount;
								return;
							}
							else if (CachedTaxRateOverride.AO_DefaultingRule == TaxOverrideDefaultingRule.Codes.CopyARAmount)
							{
								taxOverrideAction = TaxOverrideAction.INTCopyARAmount;
								return;
							}
						}
					}
				}
			}

			taxOverrideAction = TaxOverrideAction.Default;
		}

		public bool IsUseTaxRateOverride => taxOverrideAction == TaxOverrideAction.INTSumARAmount || taxOverrideAction == TaxOverrideAction.INTAll;
		public bool IsUseSumARAmount => taxOverrideAction == TaxOverrideAction.INTSumARAmount;
		public bool IsUseCopyARAmount => taxOverrideAction == TaxOverrideAction.INTCopyARAmount;
		public bool IsUseTransactionContextAll => taxOverrideAction == TaxOverrideAction.INTAll || taxOverrideAction == TaxOverrideAction.NonINTAll;
		public bool IsUseDefaultTaxLogic => taxOverrideAction == TaxOverrideAction.Default;
		public AccChargeTaxOverride CachedTaxRateOverride { get; private set; }

		public static bool GetIsUseTaxOverrideForInterCompanyInvoiceImport(BusinessObjectFactory factory) =>
				factory.HasContext(BusinessContext.InterCompanyInvoiceImport) &&
				AccountingMasterFilesRegistry.Instance.UseImportCompanyChargeCodesTaxOverridesforIntercompanyInvoiceImport.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty);

		TaxOverrideAction taxOverrideAction;
		readonly BusinessObjectFactory factory;
		readonly Func<AccChargeTaxOverride> taxRateOverrideProvider;

		enum TaxOverrideAction
		{
			Default,
			NonINTAll,
			INTAll,
			INTSumARAmount,
			INTCopyARAmount,
		}
	}
}
