using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class TransportAndChargeCodeMappingRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public TransportAndChargeCodeMappingRegistryBusinessObjectCollection()
			: base()
		{
		}

		public TransportAndChargeCodeMappingRegistryBusinessObjectCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new TransportAndChargeCodeMappingRegistryBusinessObject this[int i]
		{
			get { return (TransportAndChargeCodeMappingRegistryBusinessObject)Elements[i]; }
		}

		public new TransportAndChargeCodeMappingRegistryBusinessObject AddNew()
		{
			return (TransportAndChargeCodeMappingRegistryBusinessObject)base.AddNew();
		}

		public ZString FindNominalCostCode(ZString transportMode, ZString chargeCode)
		{
			TransportAndChargeCodeMappingRegistryBusinessObject element = FindElement(transportMode, chargeCode);
			return element != null ? element.NominalCostCode : ZString.Empty;
		}

		public ZString FindNominalRevenueCode(ZString transportMode, ZString chargeCode)
		{
			TransportAndChargeCodeMappingRegistryBusinessObject element = FindElement(transportMode, chargeCode);
			return element != null ? element.NominalRevenueCode : ZString.Empty;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new TransportAndChargeCodeMappingRegistryBusinessObjectCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new TransportAndChargeCodeMappingRegistryBusinessObject(CurrentFallbackLevel);
		}

		TransportAndChargeCodeMappingRegistryBusinessObject FindElement(ZString transportMode, ZString chargeCode)
		{
			TransportAndChargeCodeMappingRegistryBusinessObject result = null;
			if (transportMode.IsEmpty || chargeCode.IsEmpty)
			{
				return result;
			}

			foreach (TransportAndChargeCodeMappingRegistryBusinessObject element in Elements)
			{
				if (element.TransportModeCode == transportMode && CurrentChargeCode(element.CurrentCharge) == chargeCode)
				{
					result = element;
					break;
				}
			}
			return result;
		}

		static ZString CurrentChargeCode(AccChargeCode currentCharge)
		{
			return currentCharge != null ? currentCharge.AC_Code : ZString.Empty;
		}
	}
}
