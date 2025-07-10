using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class IntercompanyClearingConfigurationRegistryItem : StronglyTypedRegistryItem<IntercompanyClearingConfigurationCollection>
	{
		public IntercompanyClearingConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new IntercompanyClearingConfigurationRegistryItemImpl(name, category, caption, hint, storage))
		{
		}

		class IntercompanyClearingConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public IntercompanyClearingConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new IntercompanyClearingConfigurationRegistryDataType(), storage)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				IntercompanyClearingConfigurationCollection result = new IntercompanyClearingConfigurationCollection();
				result.AddDefaultValues(companyPK);
				return result;
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.IntercompanyClearingConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class IntercompanyClearingConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IntercompanyClearingConfigurationCollection>
	{
		public IntercompanyClearingConfigurationRegistryDataType()
		{
		}
	}
}
