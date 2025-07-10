using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[XmlRoot("Charges")]
	public class ChargeGroupAndChargeCodeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ChargeGroupAndChargeCodeCollection()
		{
		}

		public ChargeGroupAndChargeCodeCollection(LandedCostingGroup parentLandedCostingGroup, FallbackLevel fallbackLevel, BusinessObjectFactory factory) : base(fallbackLevel, factory)
		{
			fParentLandedCostingGroup = parentLandedCostingGroup;
		}

		public new ChargeGroupAndChargeCode this[int i]
		{
			get { return (ChargeGroupAndChargeCode)Elements[i]; }
		}

		public new ChargeGroupAndChargeCode AddNew()
		{
			return (ChargeGroupAndChargeCode)base.AddNew();
		}

		public ChargeGroupAndChargeCodeCollection Clone(LandedCostingGroup parentLandedCostingGroup, FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			ChargeGroupAndChargeCodeCollection result = (ChargeGroupAndChargeCodeCollection)Clone(fallbackLevel, factory);
			result.fParentLandedCostingGroup = parentLandedCostingGroup;

			return result;
		}

		public LandedCostingGroup ParentLandedCostingGroup
		{
			get { return fParentLandedCostingGroup; }
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeGroupAndChargeCodeCollection(null, fallbackLevel, factory);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ChargeGroupAndChargeCode(CurrentFallbackLevel, CurrentFactory);
		}

		LandedCostingGroup fParentLandedCostingGroup;
	}
}
