using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class CreditControlledDocumentsCheckConfigurationCollection : AmountBasedAuthorisationRequirementCollection
	{
		public new CreditControlledDocumentsCheckConfiguration this[int x]
		{
			get { return (CreditControlledDocumentsCheckConfiguration)base[x]; }
		}

		public new CreditControlledDocumentsCheckConfiguration AddNew()
		{
			return (CreditControlledDocumentsCheckConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new CreditControlledDocumentsCheckConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new CreditControlledDocumentsCheckConfiguration();
		}
	}
}