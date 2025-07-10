using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;
using Res = Enterprise.Accounting.Business.Res;

namespace Enterprise.Accounting.Registry.Business
{
	public class AllowBackPostingSubLedgerTransactionRegistryDataType : BooleanRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, bool proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (!proposedValue)
			{
				BackDateInvoicesConfiguration backDateInvoicesConfiguration = AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration.GetValueWithoutFallback(companyPK, Guid.Empty, Guid.Empty);
				if (backDateInvoicesConfiguration.OverridePostDate || backDateInvoicesConfiguration.DefaultPostDateFromInvoiceDate)
				{
					throw new RegistryValidationException(Res.GetString("D29F7986-385C-4100-A5D0-AD7E1E6431C8", @"You can't disallow Back Posting, because '{0}' registry item has ticked '{1}' and/or '{2}' options. 
Change those options at first.",
							((IRegistryItemInternals)AccountingConfigurationRegistry.Instance.BackDateInvoicesConfiguration).Location,
							backDateInvoicesConfiguration.OverridePostDateInfo.HumanReadableName,
							backDateInvoicesConfiguration.DefaultPostDateFromInvoiceDateInfo.HumanReadableName));
				}
			}
		}
	}
}