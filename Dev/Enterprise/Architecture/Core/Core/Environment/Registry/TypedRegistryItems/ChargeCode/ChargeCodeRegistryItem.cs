using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.ZArchitecture.Environment
{
	public class ChargeCodeRegistryItem : ChargeCodeRegistryItemWrapper<Guid>
	{
		public ChargeCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultChargeCode)
			: base(new SingleChargeCodeRegistryItemImpl(name, category, caption, hint, defaultChargeCode)) { }

		public ChargeCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, string defaultChargeCode)
			: base(new SingleChargeCodeRegistryItemImpl(name, category, caption, hint, storage, defaultChargeCode)) { }

		public ChargeCodeRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultChargeCode)
			: base(new SingleChargeCodeRegistryItemImpl(name, category, caption, hint, storage, options, defaultChargeCode)) { }

		#region class SingleChargeCodeRegistryItemImpl

		class SingleChargeCodeRegistryItemImpl : ChargeCodeRegistryItemImpl
		{
			public SingleChargeCodeRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, string defaultChargeCode)
				: base(name, category, caption, hint, new GuidRegistryDataType(), defaultChargeCode) { }

			public SingleChargeCodeRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, string defaultChargeCode)
				: base(name, category, caption, hint, new GuidRegistryDataType(), storage, defaultChargeCode) { }

			public SingleChargeCodeRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultChargeCode)
				: base(name, category, caption, hint, new GuidRegistryDataType(), storage, options, defaultChargeCode) { }

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return GetChargeCodePK(DefaultChargeCode, companyPK);
			}
		}

		#endregion
	}
}
