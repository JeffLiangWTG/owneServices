using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobInvoiceDescriptionCollection : RegistryBusinessObjectCollectionTemplate, IRegistrySettingCollection
	{
		public new JobInvoiceDescription this[int i]
		{
			get { return (JobInvoiceDescription)Elements[i]; }
		}

		public new JobInvoiceDescription AddNew()
		{
			return (JobInvoiceDescription)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobInvoiceDescriptionCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JobInvoiceDescription();
		}
	}
}
