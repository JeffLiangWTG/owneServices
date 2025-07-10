using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business.Freight.AirlineMessagingCargoIMPVersion
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]

	public class AirlineImpVersionCollection : RegistryBusinessObjectCollectionTemplate
	{
		public AirlineImpVersionCollection()
		{
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return SetCurrentFallbackLevel(new AirlineImpVersion());
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			var clone = new AirlineImpVersionCollection();
			clone.CurrentFallbackLevel = fallbackLevel;
			return clone;
		}

		public new AirlineImpVersion this[int i]
		{
			get { return SetCurrentFallbackLevel((AirlineImpVersion)Elements[i]); }
		}

		public new AirlineImpVersion AddNew()
		{
			return SetCurrentFallbackLevel((AirlineImpVersion)base.AddNew());
		}

		AirlineImpVersion SetCurrentFallbackLevel(AirlineImpVersion collectionItem)
		{
			collectionItem.CurrentFallbackLevel = CurrentFallbackLevel;
			return collectionItem;
		}
	}
}
