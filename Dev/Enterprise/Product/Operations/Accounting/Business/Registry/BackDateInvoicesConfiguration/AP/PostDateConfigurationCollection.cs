using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class PostDateConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new PostDateConfiguration this[int i]
		{
			get { return (PostDateConfiguration)Elements[i]; }
		}

		public new PostDateConfiguration AddNew()
		{
			return (PostDateConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PostDateConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PostDateConfiguration();
		}
	}
}
