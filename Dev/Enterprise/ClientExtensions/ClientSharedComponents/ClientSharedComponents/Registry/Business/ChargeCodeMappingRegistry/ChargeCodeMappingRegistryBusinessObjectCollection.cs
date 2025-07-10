using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ClientSharedComponents.Registry
{
	[XmlSerializerAssembly("Enterprise.ClientSharedComponents.XmlSerializers")]
	public class ChargeCodeMappingRegistryBusinessObjectCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ChargeCodeMappingRegistryBusinessObjectCollection()
			: base()
		{
		}

		public ChargeCodeMappingRegistryBusinessObjectCollection(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		public new ChargeCodeMappingRegistryBusinessObject this[int i]
		{
			get { return (ChargeCodeMappingRegistryBusinessObject)Elements[i]; }
		}

		public new ChargeCodeMappingRegistryBusinessObject AddNew()
		{
			return (ChargeCodeMappingRegistryBusinessObject)base.AddNew();
		}

		public ZString FindCode(ZString code)
		{
			ZString result = ZString.Empty;
			AccChargeCode currentChargeCode;
			foreach (ChargeCodeMappingRegistryBusinessObject element in Elements)
			{
				currentChargeCode = element.CurrentChargeCode;
				if (currentChargeCode != null && currentChargeCode.AC_Code == code)
				{
					result = element.ExternalCode;
					break;
				}
			}
			return result;
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ChargeCodeMappingRegistryBusinessObjectCollection(fallbackLevel);
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ChargeCodeMappingRegistryBusinessObject(CurrentFallbackLevel);
		}
	}
}
