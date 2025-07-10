using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class IntercompanyPostingConfigurationRegistryItem : StronglyTypedRegistryItem<IntercompanyPostingConfigurationCollection>
	{
		public IntercompanyPostingConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new IntercompanyPostingConfigurationRegistryItemImpl(name, category, caption, hint, storage))
		{
		}

		class IntercompanyPostingConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public IntercompanyPostingConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new IntercompanyPostingConfigurationRegistryDataType(), storage)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				IntercompanyPostingConfigurationCollection result = new IntercompanyPostingConfigurationCollection();
				result.AddDefaultValues(companyPK);
				return result;
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.IntercompanyPostingConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
#if DEBUG
	internal
#endif
	class IntercompanyPostingConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<IntercompanyPostingConfigurationCollection>
	{
		public IntercompanyPostingConfigurationRegistryDataType()
		{
		}
	}
}
