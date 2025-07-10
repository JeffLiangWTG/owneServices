using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Client.EDI.Registry.Business
{
	[XmlSerializerAssembly("ZClientEDI.Business.XmlSerializers")]
	public class AzureOpenIDConnectConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AzureOpenIDConnectConfiguration();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AzureOpenIDConnectConfigurationCollection();
		}

		public new AzureOpenIDConnectConfiguration this[int index]
		{
			get { return (AzureOpenIDConnectConfiguration)Elements[index]; }
		}

		public new AzureOpenIDConnectConfiguration AddNew()
		{
			return (AzureOpenIDConnectConfiguration)base.AddNew();
		}
	}
}
