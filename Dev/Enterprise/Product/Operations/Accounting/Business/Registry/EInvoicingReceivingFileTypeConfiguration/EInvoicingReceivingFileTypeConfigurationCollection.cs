using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class EInvoicingReceivingFileTypeConfigurationCollection : RegistryBusinessObjectCollectionTemplate
	{
		public EInvoicingReceivingFileTypeConfigurationCollection()
		{
		}

		public EInvoicingReceivingFileTypeConfigurationCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public new EInvoicingReceivingFileTypeConfiguration this[int x]
		{
			get { return (EInvoicingReceivingFileTypeConfiguration)base[x]; }
		}

		public new EInvoicingReceivingFileTypeConfiguration AddNew()
		{
			return (EInvoicingReceivingFileTypeConfiguration)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new EInvoicingReceivingFileTypeConfiguration();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new EInvoicingReceivingFileTypeConfigurationCollection();
		}
	}
}
