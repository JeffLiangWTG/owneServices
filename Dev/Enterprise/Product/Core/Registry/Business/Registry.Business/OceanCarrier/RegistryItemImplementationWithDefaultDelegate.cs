using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.OceanCarrier
{
	sealed class RegistryItemImplementationWithDefaultDelegate<T> : RegistryItemImpl
	{
		public RegistryItemImplementationWithDefaultDelegate(
			string name, MultilingualString category, MultilingualString caption, MultilingualString hint,
			IRegistryDataType dataType, RegistryStorageFlags storage, RegistryOptions options,
			Func<Guid, Guid, Guid, T> accessor)
			: base(name, category, caption, hint, dataType, storage, options)
		{
			if (accessor == null)
			{
				throw new ArgumentNullException(nameof(accessor));
			}

			this.accessor = accessor;
		}

		protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
		{
			return accessor(companyPK, branchPK, departmentPK);
		}

		readonly Func<Guid, Guid, Guid, T> accessor;
	}
}
