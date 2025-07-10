using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PaymentAuthorisationSettings : AmountBasedMultiLevelAuthorisationRequirement
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PaymentAuthorisationSettings();
		}

		protected PaymentAuthorisationSettingsCollection ParentCollection
		{
			get { return (PaymentAuthorisationSettingsCollection)GetParentCollection(this, typeof(PaymentAuthorisationSettingsCollection)); }
		}
	}
}
