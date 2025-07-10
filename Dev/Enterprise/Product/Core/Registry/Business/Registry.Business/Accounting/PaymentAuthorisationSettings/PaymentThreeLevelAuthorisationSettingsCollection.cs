using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PaymentThreeLevelAuthorisationSettingsCollection : AmountBasedAuthorisationRequirementCollection
	{
		public new PaymentThreeLevelAuthorisationSettings this[int x]
		{
			get { return (PaymentThreeLevelAuthorisationSettings)base[x]; }
		}

		public new PaymentThreeLevelAuthorisationSettings AddNew()
		{
			return (PaymentThreeLevelAuthorisationSettings)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PaymentThreeLevelAuthorisationSettingsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PaymentThreeLevelAuthorisationSettings();
		}
	}
}
