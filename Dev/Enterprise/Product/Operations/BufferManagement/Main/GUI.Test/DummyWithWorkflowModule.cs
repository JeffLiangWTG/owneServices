using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class DummyWithWorkflowModule : DummyFilterGridModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new DummyWithWorkflowCollection(Factory, Filter);
		}

		public override string WorkflowType => WorkflowDescriptors.DummyWorkflowDescriptorCode;

		public override bool SupportsWorkflow => true;
	}
}
