using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	[XmlRoot("DocumentsAllowedForSigningItems")]
	public class DocumentsAllowedForSigningItemCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DocumentsAllowedForSigningItemCollection()
		{
		}

		public new DocumentsAllowedForSigningItem this[int i]
		{
			get { return (DocumentsAllowedForSigningItem)Elements[i]; }
		}

		public new DocumentsAllowedForSigningItem AddNew()
		{
			return (DocumentsAllowedForSigningItem)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocumentsAllowedForSigningItem();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentsAllowedForSigningItemCollection();
		}
	}
}
