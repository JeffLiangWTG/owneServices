using Enterprise.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.JP.Common
{
	public class DefaultBrokerAndCredentialRegistryItem : StronglyTypedRegistryItem<DefaultBrokerAndCredential>
	{
		public DefaultBrokerAndCredentialRegistryItem(
					string name,
					MultilingualString category,
					MultilingualString caption,
					MultilingualString hint,
					RegistryStorageFlags storage)
						: base(new RegistryItemImpl(
						name, category, caption, hint, new DefaultBrokerAndCredentialRegistryDataType(), storage))
		{
		}
	}

	[RegistryEditor("Enterprise.Customs.JP.GUI.DefaultBrokerAndCredentialRegistryItemEditor, Enterprise.Customs.JP.GUI")]
	public class DefaultBrokerAndCredentialRegistryDataType : NonPersistentBusinessObjectRegistryDataType<DefaultBrokerAndCredential>
	{
		public DefaultBrokerAndCredentialRegistryDataType()
		{
		}
	}
}
