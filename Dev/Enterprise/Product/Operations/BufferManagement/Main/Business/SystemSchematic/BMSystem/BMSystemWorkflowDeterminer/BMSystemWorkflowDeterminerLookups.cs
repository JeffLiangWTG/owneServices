using CargoWise.Application;
using CargoWise.Integration;
using Enterprise.MasterFiles.Integration;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.BufferManagement.Business
{
	public class BMSystemWorkflowDeterminerLookups : AutoBMSystemWorkflowDeterminerLookups
	{
		public BMSystemWorkflowDeterminerLookups(AutoBMSystemWorkflowDeterminer parent)
			: base(parent)
		{
		}

		public ICodeDescriptionPairList WorkflowTypes
		{
			get => Factory.GetCachedValue("IWorkflowDescriptorList", () => (CodeDescriptionPairList)ObjectFactory.Get<IWorkflowDescriptorList>());
		}
	}
}
