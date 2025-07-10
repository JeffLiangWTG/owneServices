using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class KoreaSouthEInvoicingDataElementConfigurationCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new KoreaSouthEInvoicingDataElementConfiguration this[int i]
		{
			get { return (KoreaSouthEInvoicingDataElementConfiguration)Elements[i]; }
		}

		public new KoreaSouthEInvoicingDataElementConfiguration AddNew()
		{
			return (KoreaSouthEInvoicingDataElementConfiguration)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new KoreaSouthEInvoicingDataElementConfigurationCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new KoreaSouthEInvoicingDataElementConfiguration();
		}

		protected override bool AllowNewCore => false;

		protected override bool AllowRemoveCore => false;
	}
}
