using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ElectronicProcessingChargeConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ElectronicProcessingChargeConfigurationCollection()
		{
		}

		public ElectronicProcessingChargeConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ElectronicProcessingChargeConfiguration this[int x]
		{
			get { return (ElectronicProcessingChargeConfiguration)base[x]; }
		}

		public new ElectronicProcessingChargeConfiguration AddNew()
		{
			return (ElectronicProcessingChargeConfiguration)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ElectronicProcessingChargeConfiguration();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ElectronicProcessingChargeConfigurationCollection();
		}
	}
}
