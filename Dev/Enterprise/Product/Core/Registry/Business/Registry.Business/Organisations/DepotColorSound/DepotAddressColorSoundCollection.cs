using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class DepotAddressColorSoundCollection : RegistryBusinessObjectCollectionTemplate
	{
		public DepotAddressColorSoundCollection()
			: base()
		{
		}

		public DepotAddressColorSoundCollection(BusinessObjectFactory factory)
			: base(factory)
		{
		}

		public DepotAddressColorSoundCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new DepotAddressColorSound this[int x]
		{
			get { return (DepotAddressColorSound)Elements[x]; }
		}

		public new DepotAddressColorSound AddNew()
		{
			return (DepotAddressColorSound)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new DepotAddressColorSoundCollection(fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new DepotAddressColorSound(CurrentFallbackLevel, CurrentFactory);
		}
	}
}
