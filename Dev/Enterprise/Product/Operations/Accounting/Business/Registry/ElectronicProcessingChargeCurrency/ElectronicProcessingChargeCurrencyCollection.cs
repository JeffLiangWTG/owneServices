using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class ElectronicProcessingChargeCurrencyCollection : RegistryBusinessObjectCollectionTemplate
	{
		public ElectronicProcessingChargeCurrencyCollection()
		{
		}

		public ElectronicProcessingChargeCurrencyCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new ElectronicProcessingChargeCurrency this[int x]
		{
			get { return (ElectronicProcessingChargeCurrency)base[x]; }
		}

		public new ElectronicProcessingChargeCurrency AddNew()
		{
			return (ElectronicProcessingChargeCurrency)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new ElectronicProcessingChargeCurrency();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new ElectronicProcessingChargeCurrencyCollection();
		}
	}
}
