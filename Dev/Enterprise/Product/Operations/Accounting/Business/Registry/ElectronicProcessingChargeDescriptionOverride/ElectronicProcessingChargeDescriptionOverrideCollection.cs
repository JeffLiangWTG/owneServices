using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ElectronicProcessingChargeDescriptionOverrideCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ElectronicProcessingChargeDescriptionOverrideCollection()
		{
		}

		public ElectronicProcessingChargeDescriptionOverrideCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ElectronicProcessingChargeDescriptionOverride this[int x]
		{
			get { return (ElectronicProcessingChargeDescriptionOverride)base[x]; }
		}

		public new ElectronicProcessingChargeDescriptionOverride AddNew()
		{
			return (ElectronicProcessingChargeDescriptionOverride)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ElectronicProcessingChargeDescriptionOverride();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ElectronicProcessingChargeDescriptionOverrideCollection();
		}
	}
}
