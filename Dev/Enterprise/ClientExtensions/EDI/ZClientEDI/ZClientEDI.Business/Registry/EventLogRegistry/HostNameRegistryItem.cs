using System;
using System.Text.RegularExpressions;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Res = ZClientEDI.Business.Res;

namespace Enterprise.Client.EDI.Registry.Business
{
	public class HostNameStringListRegistryItem : StringRegistryItem
	{
		public HostNameStringListRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
			: base(new HostNameStringListImpl(name, category, caption, hint, storage, options, defaultValue))
		{
		}

		class HostNameStringListImpl : RegistryItemImpl
		{
			public HostNameStringListImpl(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options, string defaultValue)
				: base(name, category, caption, hint, new HostNameStringListRegistryDataType(), storage, options, defaultValue)
			{
			}
		}
	}

	public class HostNameStringListRegistryDataType : StringRegistryDataType
	{
		protected override void ValidateCore(IRegistryItem registryItem, string stringList, Guid companyPK, Guid branchPK,
			Guid departmentPK)
		{
			base.ValidateCore(registryItem, stringList, companyPK, branchPK, departmentPK);

			var validHostNameStringList = new Regex(@"^(([a-zA-Z0-9]|[a-zA-Z0-9][a-zA-Z0-9\-_]*[a-zA-Z0-9]),)*([A-Za-z0-9]|[A-Za-z0-9][A-Za-z0-9\-_]*[A-Za-z0-9])$");

			if (!validHostNameStringList.IsMatch(stringList) && !string.IsNullOrEmpty(stringList))
			{
				throw new RegistryValidationException(Res.GetString("093760F5-5C1E-4560-B214-934B6093A071", "Invalid input: Please input valid machine names and separate each machine name by ','."));
			}
		}
	}
}

