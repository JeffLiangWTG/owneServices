using System;
using Enterprise.Accounting.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class EnableAJLandRJLRegistryDataType : BooleanRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, bool proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);
			if (AccountingConfigurationRegistry.Instance.JournalEntriesNumberCustomisation.GetValueWithoutFallback(companyPK, branchPK, departmentPK).AllocationOption != AccountingConstants.JournalEntriesNumberCustomisationAllocationOption.GEN)
			{
				throw new RegistryValidationException(Res.GetString("1BE7D5E0-9B58-4E22-8B3E-8CF5C742ECD9", @"Please ensure that 'Allocation Option' has been set to 'GEN' in the registry 'Journal Entries Number Customization'."));
			}
		}
	}
}
