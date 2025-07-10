using CargoWise.Application;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.Registry.Business
{
	public class WorkflowValidationProcessTypeLookups : ZLookups
	{
		public WorkflowValidationProcessTypeLookups(WorkflowValidationProcessType parent)
			: base(parent)
		{
		}

		public CodeDescriptionPairList ProcessTypeList => processTypeList ??= GetTemplateWorkflowDescriptorList();
		CodeDescriptionPairList processTypeList;

		CodeDescriptionPairList GetTemplateWorkflowDescriptorList() => (CodeDescriptionPairList)ObjectFactory.Get<ITemplateWorkflowDescriptorList>();
	}
}
