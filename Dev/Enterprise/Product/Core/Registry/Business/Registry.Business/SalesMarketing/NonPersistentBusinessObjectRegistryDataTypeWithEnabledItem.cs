using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public abstract class NonPersistentBusinessObjectRegistryDataTypeWithEnabledItem<T> : NonPersistentBusinessObjectRegistryDataType<T> where T : IRegistryBusiness
	{
		protected NonPersistentBusinessObjectRegistryDataTypeWithEnabledItem()
			: base()
		{
		}

		protected NonPersistentBusinessObjectRegistryDataTypeWithEnabledItem(T defaultValue)
			: base(defaultValue)
		{
		}

		protected abstract bool HasEnabledItem(T proposedValue);

		protected virtual string ValidationMessage => ResString.GetMultilingualString("AD3B8B02-E80C-42DA-A929-0F4793370829", "This registry should have an enabled item before it is saved.");

		protected override void ValidateBeforeRegistryFormSaveCore(IRegistryItem registryItem, T proposedValue, Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			base.ValidateBeforeRegistryFormSaveCore(registryItem, proposedValue, companyPK, branchPK, departmentPK);

			if (!HasEnabledItem(proposedValue))
			{
				throw new RegistryValidationException(ValidationMessage);
			}
		}
	}
}
