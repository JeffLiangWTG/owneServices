using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class HouseBillOfLadingTypeCollection : RegistryBusinessObjectCollection
	{
		public HouseBillOfLadingTypeCollection()
		{
		}

		public HouseBillOfLadingTypeCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new HouseBillOfLadingType this[int i]
		{
			get { return (HouseBillOfLadingType)Elements[i]; }
		}

		public new HouseBillOfLadingType AddNew()
		{
			return (HouseBillOfLadingType)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new HouseBillOfLadingTypeCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new HouseBillOfLadingType(CurrentFallbackLevel);
		}
	}
}
