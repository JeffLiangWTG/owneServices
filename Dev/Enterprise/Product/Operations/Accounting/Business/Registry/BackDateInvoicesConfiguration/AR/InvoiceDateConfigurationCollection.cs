using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class InvoiceDateConfigurationCollection : RegistryBusinessObjectCollectionTemplate, IInvoiceDateConfigurationCollection
	{
		public new InvoiceDateConfiguration this[int i]
		{
			get { return (InvoiceDateConfiguration)Elements[i]; }
		}

		public new InvoiceDateConfiguration AddNew()
		{
			return (InvoiceDateConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new InvoiceDateConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new InvoiceDateConfiguration();
		}
	}
}