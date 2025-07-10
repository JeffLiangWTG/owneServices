using System.Linq;
using System.Xml.Serialization;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	public class WorkflowValidationProcessTypeCollection : RegistryBusinessObjectCollectionTemplate
	{
		public WorkflowValidationProcessTypeCollection()
			: base()
		{
		}

		public WorkflowValidationProcessTypeCollection(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			: base(fallbackLevel, factory)
		{
		}

		public bool Contains(ZString processType)
		{
			return !processType.IsEmpty && this.Cast<WorkflowValidationProcessType>().Any(x => x.ProcessType == processType);
		}

		public new WorkflowValidationProcessType this[int i]
		{
			get { return (WorkflowValidationProcessType)Elements[i]; }
		}

		public new WorkflowValidationProcessType AddNew()
		{
			return (WorkflowValidationProcessType)base.AddNew();
		}

		protected override BusinessObject CreateNonPersistentBusinessObject()
		{
			return new WorkflowValidationProcessType();
		}

		protected override RegistryBusinessObjectCollectionTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WorkflowValidationProcessTypeCollection();
		}

		public static WorkflowValidationProcessTypeCollection DefaultValue => new WorkflowValidationProcessTypeCollection() { };
	}
}
