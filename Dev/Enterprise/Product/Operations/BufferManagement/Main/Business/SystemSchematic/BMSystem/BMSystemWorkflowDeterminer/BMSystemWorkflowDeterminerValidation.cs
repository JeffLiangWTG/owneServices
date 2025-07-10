using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;

namespace Enterprise.BufferManagement.Business
{
	public class BMSystemWorkflowDeterminerValidation : AutoBMSystemWorkflowDeterminerValidation
	{
		public BMSystemWorkflowDeterminerValidation(AutoBMSystemWorkflowDeterminer parent)
			: base(parent)
		{
		}

		protected new BMSystemWorkflowDeterminer Parent => (BMSystemWorkflowDeterminer)base.Parent;

		protected override void CheckFSW_WorkflowType()
		{
			base.CheckFSW_WorkflowType();

			MandatoryValidation.CheckEntered(Parent.FSW_WorkflowTypeInfo);
			ListValidation.ErrorIfInvalidCode(Parent.FSW_WorkflowTypeInfo);

			var system = Parent.System;

			if (system != null)
			{
				PropertyIsUniqueInCollectionValidation.CheckPropertyIsUniqueInCollection(Parent.FSW_WorkflowTypeInfo, system.RelatedWorkflowTypes);

				if (!Parent.FSW_WorkflowType.IsEmpty)
				{
					var otherSystem = BMSystem.GetSystemsForWorkflowType(Parent.FSW_WorkflowType, Parent.Factory).Where(s => s != system).FirstOrDefault();

					if (otherSystem != null)
					{
						Parent.FSW_WorkflowTypeInfo.AddError(Res.GetString("70e1f3ce-9fe9-40ac-a019-8b81e7574729", "The Workflow Type is in use by {0} and must only be used by one system.", otherSystem.FS_Name));
					}
				}
			}

			var workflowDescriptor = WorkflowDescriptors.Instance.TryGetValueSafe(Parent.FSW_WorkflowType);

			if (workflowDescriptor != null && !workflowDescriptor.SupportsBufferManagement)
			{
				Parent.FSW_WorkflowTypeInfo.AddError(Res.GetString("14e30c53-c18d-467d-b9d0-65380b9c4dd0", "This Workflow Type does not support Buffer Management."));
			}
		}
	}
}
