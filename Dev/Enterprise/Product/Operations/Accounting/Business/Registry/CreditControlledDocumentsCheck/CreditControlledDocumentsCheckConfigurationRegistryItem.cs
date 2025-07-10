using System;
using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	public class CreditControlledDocumentsCheckConfigurationRegistryItem : StronglyTypedRegistryItem<CreditControlledDocumentsCheckConfigurationCollection>
	{
		public CreditControlledDocumentsCheckConfigurationRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
			: base(new CreditControlledDocumentsCheckConfigurationRegistryItemImpl(name, category, caption, hint, storage))
		{
		}

		class CreditControlledDocumentsCheckConfigurationRegistryItemImpl : RegistryItemImpl
		{
			public CreditControlledDocumentsCheckConfigurationRegistryItemImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage)
				: base(name, category, caption, hint, new CreditControlledDocumentsCheckConfigurationRegistryDataType(), storage)
			{
			}

			protected override object GetDefaultValueCore(Guid companyPK, Guid branchPK, Guid departmentPK)
			{
				return new CreditControlledDocumentsCheckConfigurationCollection();
			}
		}
	}

	[RegistryEditor("Enterprise.Accounting.Registry.GUI.CreditControlledDocumentsCheckConfigurationRegistryItemEditor, Enterprise.Accounting.GUI")]
	public class CreditControlledDocumentsCheckConfigurationRegistryDataType : NonPersistentBusinessObjectRegistryDataType<CreditControlledDocumentsCheckConfigurationCollection>
	{
		public CreditControlledDocumentsCheckConfigurationRegistryDataType()
		{
		}
	}
}
