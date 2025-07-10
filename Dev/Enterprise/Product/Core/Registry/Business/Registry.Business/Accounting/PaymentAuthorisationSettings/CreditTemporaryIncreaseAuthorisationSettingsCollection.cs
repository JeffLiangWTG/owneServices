using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class CreditTemporaryIncreaseAuthorisationSettingsCollection : AmountOrPercentageBasedThreeLevelAuthorisationRequirementCollection
	{
		public new CreditTemporaryIncreaseAuthorisationSettings this[int x]
		{
			get { return (CreditTemporaryIncreaseAuthorisationSettings)base[x]; }
		}

		public new CreditTemporaryIncreaseAuthorisationSettings AddNew()
		{
			return (CreditTemporaryIncreaseAuthorisationSettings)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CreditTemporaryIncreaseAuthorisationSettingsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CreditTemporaryIncreaseAuthorisationSettings();
		}
	}
}
