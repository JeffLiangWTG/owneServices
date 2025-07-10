using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Modules.Testing;

namespace Enterprise.BufferManagement.GUI.Test
{
	public class DummyModuleForVisualBoards : DummyFilterGridModule
	{
		protected override IBusinessObjectCollection GetNewGridCollection()
		{
			return new OrgHeaderCollection(Factory);
		}

		public override bool SupportsWorkflow
		{
			get { return SupportsWorkflowOverride; }
		}

		public override string WorkflowType
		{
			get
			{
				if (!string.IsNullOrEmpty(WorkflowTypeOverride))
				{
					return WorkflowTypeOverride;
				}
				else
				{
					var elementType = this.GetElementType();
					var workflowDescriptor = elementType != null ? WorkflowDescriptors.Instance.Values.FirstOrDefault(d => d.WorkflowProviderType.IsAssignableFrom(elementType) || elementType.IsAssignableFrom(d.WorkflowProviderType)) : null;

					return workflowDescriptor != null ? workflowDescriptor.Code : null;
				}
			}
		}

		internal bool SupportsWorkflowOverride { get; set; }

		public override BusinessObject[] GetSelectedBusinessObjects()
		{
			return SelectedBusinessObjects ?? base.GetSelectedBusinessObjects();
		}
	}
}
