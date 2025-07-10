using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Customs.GB.Registry
{
	[XmlSerializerAssembly("Enterprise.Customs.GB.Registry.XmlSerializers")]
	public class McpIslCredentialsSettingCollection : RegistryBusinessObjectCollectionTemplate
	{
		public McpIslCredentialsSettingCollection()
		{
		}

		public McpIslCredentialsSettingCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new McpIslCredentialsSetting AddNew() => (McpIslCredentialsSetting)base.AddNew();

		public new McpIslCredentialsSetting this[int i] => (McpIslCredentialsSetting)Elements[i];

		protected override BusinessObject CreateNonPersistentBusinessObject()
			=> new McpIslCredentialsSetting(CurrentFallbackLevel, CurrentFactory);

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new McpIslCredentialsSettingCollection(fallbackLevel, factory);
	}
}
