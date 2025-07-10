using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PaymentSixLevelAuthorisationSettingsCollection : AmountBasedAuthorisationRequirementCollection
	{
		public new PaymentSixLevelAuthorisationSettings this[int x]
		{
			get { return (PaymentSixLevelAuthorisationSettings)base[x]; }
		}

		public new PaymentSixLevelAuthorisationSettings AddNew()
		{
			return (PaymentSixLevelAuthorisationSettings)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PaymentSixLevelAuthorisationSettingsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PaymentSixLevelAuthorisationSettings();
		}
	}
}
