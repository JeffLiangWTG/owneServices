using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class AdditionalHouseBillOfLadingTypeCollection : RegistryBusinessObjectCollection
	{
		public AdditionalHouseBillOfLadingTypeCollection()
		{
		}

		public AdditionalHouseBillOfLadingTypeCollection(FallbackLevel fallbackLevel) : base(fallbackLevel)
		{
		}

		public new AdditionalHouseBillOfLadingType this[int i]
		{
			get { return (AdditionalHouseBillOfLadingType)Elements[i]; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new AdditionalHouseBillOfLadingTypeCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new AdditionalHouseBillOfLadingType(CurrentFallbackLevel);
		}

		public new AdditionalHouseBillOfLadingType AddNew()
		{
			return (AdditionalHouseBillOfLadingType)base.AddNew();
		}

		protected override bool AllowNewCore
		{
			get
			{
				return false;
			}
		}

		protected override bool AllowRemoveCore
		{
			get
			{
				return false;
			}
		}
	}
}
