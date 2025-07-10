using System;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Security.ActiveDirectory
{
	public class DomainCredentialsCollectionRegistryItem : StronglyTypedRegistryItem<DomainCredentialsCollection>
	{
		public DomainCredentialsCollectionRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, DomainCredentialsCollection defaultValue)
			: base(new RegistryItemImpl(name, category, caption, hint, new DomainCredentialsCollectionRegistryDataType(defaultValue), storage, options))
		{
		}

		protected override void SetValueCore(Guid companyOrOwnerPK, Guid branchPK, Guid departmentPK, object newValue)
		{
			base.SetValueCore(companyOrOwnerPK, branchPK, departmentPK, newValue);

			DirectorySearcherFactory.ClearCache();
		}
	}
}
