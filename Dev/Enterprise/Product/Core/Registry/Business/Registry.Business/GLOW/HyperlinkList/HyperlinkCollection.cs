using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HyperlinkCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new Hyperlink this[int i] => (Hyperlink)Elements[i];

		public new Hyperlink AddNew()
		{
			return (Hyperlink)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new Hyperlink();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HyperlinkCollection();
		}
	}
}
