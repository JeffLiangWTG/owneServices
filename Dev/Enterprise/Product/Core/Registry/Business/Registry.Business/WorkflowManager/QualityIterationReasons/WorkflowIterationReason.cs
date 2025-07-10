using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Registry.Business
{
	[XmlSerializerAssembly("Enterprise.Registry.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class WorkflowIterationReason : RegistryBusinessObject
	{
		public WorkflowIterationReason()
		{
		}

		public WorkflowIterationReason(FallbackLevel fallbackLevel)
			: base(fallbackLevel)
		{
		}

		protected override int MaxDescriptionLength
		{
			get { return 256; }
		}

		#region Clone

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
		{
			return new WorkflowIterationReason();
		}

		#endregion
	}
}
