using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Accounting.Business.XmlSerializers")]
	public class JobInvoicingDefaultGatewayDepartmentsCollection : RegistryBusinessObjectCollectionTemplate
	{
		public new JobInvoicingDefaultGatewayDepartments this[int i]
		{
			get { return (JobInvoicingDefaultGatewayDepartments)Elements[i]; }
		}

		public new JobInvoicingDefaultGatewayDepartments AddNew()
		{
			return (JobInvoicingDefaultGatewayDepartments)base.AddNew();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new JobInvoicingDefaultGatewayDepartmentsCollection();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new JobInvoicingDefaultGatewayDepartments();
		}
	}
}
