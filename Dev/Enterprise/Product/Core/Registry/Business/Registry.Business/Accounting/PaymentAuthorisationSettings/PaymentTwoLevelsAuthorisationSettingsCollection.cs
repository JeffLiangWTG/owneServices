using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class PaymentTwoLevelsAuthorisationSettingsCollection : AmountBasedAuthorisationRequirementCollection
	{
		public new PaymentTwoLevelsAuthorisationSettings this[int x]
		{
			get { return (PaymentTwoLevelsAuthorisationSettings)base[x]; }
		}

		public new PaymentTwoLevelsAuthorisationSettings AddNew()
		{
			return (PaymentTwoLevelsAuthorisationSettings)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new PaymentTwoLevelsAuthorisationSettingsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new PaymentTwoLevelsAuthorisationSettings();
		}

		public override string ToString()
		{
			string result = "";
			foreach (PaymentTwoLevelsAuthorisationSettings settings in this)
			{
				result += settings.Range + " " + settings.Amount + " " + settings.AuthorisationRequirement + "; ";
			}
			return result;
		}
	}
}
