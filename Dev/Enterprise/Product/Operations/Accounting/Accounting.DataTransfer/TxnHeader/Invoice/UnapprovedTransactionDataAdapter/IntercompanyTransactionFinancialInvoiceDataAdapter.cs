using System;
using Enterprise.Accounting.Registry.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Accounting.DataTransfer.Invoices
{
	public class IntercompanyTransactionFinancialInvoiceDataAdapter : UnapprovedTransactionFinancialInvoiceDataAdapter
	{
		protected override TransactionBuilderConfig GetTransactionBuilderConfig(bool isCrossLedgerImport)
		{
			var config = base.GetTransactionBuilderConfig(isCrossLedgerImport);
			config.SetOrganisation = false;
			if (AccountingConfigurationRegistry.Instance.CarryOverInvoiceLineDescriptionDuringIntercompanyInvoicePosting.GetFallBackValueAtAllLevels(GlbCompany.CurrentCompany.PK.ToGuid(), Guid.Empty, Guid.Empty))
			{
				config.UseChargeDescAsLineDesc = true;
			}
			return config;
		}
	}
}
