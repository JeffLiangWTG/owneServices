using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Maintainability", "CA1501:AvoidExcessiveInheritance")]
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PaymentSixLevelAuthorisationSettings : AmountBasedSixLevelAuthorisationRequirement
	{
		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PaymentSixLevelAuthorisationSettings();
		}
	}
}
