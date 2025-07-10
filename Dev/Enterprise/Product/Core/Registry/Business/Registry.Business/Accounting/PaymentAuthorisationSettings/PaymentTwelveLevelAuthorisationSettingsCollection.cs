using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PaymentTwelveLevelAuthorisationSettingsCollection : AmountBasedAuthorisationRequirementCollection
	{
		public new PaymentTwelveLevelAuthorisationSettings this[int x]
		{
			get { return (PaymentTwelveLevelAuthorisationSettings)base[x]; }
		}

		public new PaymentTwelveLevelAuthorisationSettings AddNew()
		{
			return (PaymentTwelveLevelAuthorisationSettings)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PaymentTwelveLevelAuthorisationSettingsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PaymentTwelveLevelAuthorisationSettings();
		}
	}
}
