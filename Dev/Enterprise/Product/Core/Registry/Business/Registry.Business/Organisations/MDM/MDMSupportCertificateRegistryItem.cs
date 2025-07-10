using Enterprise.Integration;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	public class MDMSupportCertificateRegistryItem : StronglyTypedRegistryItem<SystemToSystemTrustInfo>
	{
		public MDMSupportCertificateRegistryItem(string name, MultilingualString category, MultilingualString caption, MultilingualString hint, MDMProductCodes productCode)
			: base(new RegistryItemImpl(name, category, caption, hint, new MDMSupportCertificateRegistryDataType(productCode), RegistryStorageFlags.System, RegistryOptions.IsOnlyForSupport, new SystemToSystemTrustInfo()))
		{
		}
	}
}
