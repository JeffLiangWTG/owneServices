using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class LocationsChargesCollection : RegistryBusinessObjectCollectionTemplate
	{
		public LocationsChargesCollection()
		{
		}

		public LocationsChargesCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new LocationsChargesGroup this[int i]
		{
			get { return (LocationsChargesGroup)Elements[i]; }
		}

		public LocationsChargesGroup this[string s]
		{
			get
			{
				LocationsChargesGroup result = null;
				foreach (LocationsChargesGroup element in this)
				{
					if (s.StartsWith(element.Location))
					{
						result = element;
						if (element.Location == s)
						{
							break;
						}
					}
				}
				return result;
			}
		}

		public new LocationsChargesGroup AddNew()
		{
			return (LocationsChargesGroup)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new LocationsChargesGroup(CurrentFallbackLevel, CurrentFactory);
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new LocationsChargesCollection(fallbackLevel, factory);
		}
	}
}
