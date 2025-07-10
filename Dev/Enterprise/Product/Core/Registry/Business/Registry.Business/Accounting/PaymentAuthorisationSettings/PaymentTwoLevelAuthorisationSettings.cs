using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PaymentTwoLevelsAuthorisationSettings : AmountBasedTwoLevelAuthorisationRequirement
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PaymentTwoLevelsAuthorisationSettings();
		}

		protected PaymentTwoLevelsAuthorisationSettingsCollection ParentCollection
		{
			get { return (PaymentTwoLevelsAuthorisationSettingsCollection)GetParentCollection(this, typeof(PaymentTwoLevelsAuthorisationSettingsCollection)); }
		}
	}
}
