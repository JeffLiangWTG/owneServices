using System.Xml.Serialization;
using CargoWise.EntityFramework;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.BufferManagement.Business
{
	[XmlSerializerAssembly("Enterprise.BufferManagement.Business.XmlSerializers")]
	[CodeProperty("Code"), DescriptionProperty("Description")]
	public class WorkflowCategory : RegistryBusinessObject
	{
		protected override int MaxDescriptionLength => 256;

		protected override RegistryBusinessObjectTemplate GetClone(FallbackLevel fallbackLevel, BusinessObjectFactory factory)
			=> new WorkflowCategory();

		protected override void ValidateCodeCore()
		{
			base.ValidateCodeCore();

			if (Code == BMConstants.JobLevelWorkflowCategoryCode)
			{
				CodeInfo.AddError(Res.GetString("d28dce4e-ddfc-45ee-be07-4aa7402a760d", "The code '{0}' is reserved and may not be used as a Workflow Category code. Please enter a different code", BMConstants.JobLevelWorkflowCategoryCode));
			}
		}
	}
}
