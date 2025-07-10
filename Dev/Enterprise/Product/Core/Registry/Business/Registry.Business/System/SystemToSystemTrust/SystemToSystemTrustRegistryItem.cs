using CargoWise.SystemToSystemTrust;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class SystemToSystemTrustRegistryItem : StronglyTypedRegistryItem<ISystemToSystemTrustInfo>
	{
		public SystemToSystemTrustRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, RegistryStorageFlags storage, RegistryOptions options)
			: base(new RegistryItemImpl(name, category, caption, hint, new SystemToSystemTrustRegistryDataType(), storage, options)) { }
	}
}
