using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot(ElementName = "ChargeCodes")]
	public class ChargeCodeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ChargeCodeCollection()
		{
		}

		public ChargeCodeCollection(LocationsChargesGroup parentLocationsChargesGroup, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
			fParentLocationsChargesGroup = parentLocationsChargesGroup;
		}

		public new ChargeCodeGroup this[int i]
		{
			get { return (ChargeCodeGroup)Elements[i]; }
		}

		public new ChargeCodeGroup AddNew()
		{
			return (ChargeCodeGroup)base.AddNew();
		}

		public ChargeCodeCollection Clone(LocationsChargesGroup parentLocationsChargesGroup, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ChargeCodeCollection result = (ChargeCodeCollection)Clone(fallbackLevel, factory);
			result.fParentLocationsChargesGroup = parentLocationsChargesGroup;

			return result;
		}

		public LocationsChargesGroup ParentLocationsChargesGroup
		{
			get { return fParentLocationsChargesGroup; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeCollection(null, fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ChargeCodeGroup(CurrentFallbackLevel, CurrentFactory);
		}

		LocationsChargesGroup fParentLocationsChargesGroup;
	}
}
