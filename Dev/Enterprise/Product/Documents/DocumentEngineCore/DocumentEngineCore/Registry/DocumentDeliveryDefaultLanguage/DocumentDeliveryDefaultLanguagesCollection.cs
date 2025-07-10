using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.DocumentEngineCore.Registry
{
	[XmlSerializerAssembly("Enterprise.DocumentEngineCore.XmlSerializers")]
	public class DocumentDeliveryDefaultLanguagesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new DocumentDeliveryDefaultLanguages this[int i] => (DocumentDeliveryDefaultLanguages)Elements[i];

		public new DocumentDeliveryDefaultLanguages AddNew()
		{
			return (DocumentDeliveryDefaultLanguages)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DocumentDeliveryDefaultLanguagesCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DocumentDeliveryDefaultLanguages();
		}

		protected override void OnRemoved(BusinessObject bizO)
		{
			base.OnRemoved(bizO);
			RunPreSaveValidation();
		}
	}
}
