using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PaymentAuthorisationSettingsCollection : AmountBasedAuthorisationRequirementCollection
	{
		public new PaymentAuthorisationSettings this[int x]
		{
			get { return (PaymentAuthorisationSettings)base[x]; }
		}

		public new PaymentAuthorisationSettings AddNew()
		{
			return (PaymentAuthorisationSettings)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PaymentAuthorisationSettingsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PaymentAuthorisationSettings();
		}
	}
}
