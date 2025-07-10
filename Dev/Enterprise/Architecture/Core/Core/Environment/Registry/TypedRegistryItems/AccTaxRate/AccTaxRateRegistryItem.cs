using System;

using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class AccTaxRateRegistryItem : AccTaxRateRegistryItemWrapper<Guid>
	{
		public AccTaxRateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultTaxRate)
			: base(new SingleAccTaxRateRegistryItemImpl(name, category, caption, hint, defaultTaxRate)) { }

		public AccTaxRateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, string defaultTaxRate)
			: base(new SingleAccTaxRateRegistryItemImpl(name, category, caption, hint, storage, defaultTaxRate)) { }

		public AccTaxRateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultTaxRate)
			: base(new SingleAccTaxRateRegistryItemImpl(name, category, caption, hint, storage, options, defaultTaxRate)) { }

		#region class SingleAccTaxRateCodeRegistryItemImpl

		class SingleAccTaxRateRegistryItemImpl : AccTaxRateRegistryItemImpl
		{
			public SingleAccTaxRateRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultTaxRate)
				: base(name, category, caption, hint, new GuidRegistryDataType(), defaultTaxRate) { }

			public SingleAccTaxRateRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, string defaultTaxRate)
				: base(name, category, caption, hint, new GuidRegistryDataType(), storage, defaultTaxRate) { }

			public SingleAccTaxRateRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultTaxRate)
				: base(name, category, caption, hint, new GuidRegistryDataType(), storage, options, defaultTaxRate) { }

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetTaxRatePK(DefaultTaxRate, companyPK);
			}
		}

		#endregion
	}
}
