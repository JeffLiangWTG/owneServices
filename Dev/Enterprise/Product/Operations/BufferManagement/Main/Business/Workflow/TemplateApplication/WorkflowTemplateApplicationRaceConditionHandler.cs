using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.BufferManagement.Business
{
	public class WorkflowTemplateApplicationRaceConditionHandler : TemplateApplicationRaceConditionHandlerBase<ProcessHeader>
	{
		public WorkflowTemplateApplicationRaceConditionHandler(BusinessObjectFactory factory, bool tryHandleConflictsAutomatically)
			: base(factory, tryHandleConflictsAutomatically, parentColumn: ProcessHeaderSchema.FH_ParentId)
		{
		}

		protected override bool ShouldProcessCore() => BMSRegistryProvider.IsBufferManagementEnabled && WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationConcurrencyProtection.Value;

		protected override bool ShouldLock => WorkflowDataRegistry.Instance.EnableWorkflowTemplateApplicationRaceConditionHandlerProcessHeaderLock.Value;

		protected override bool ShouldCheck(ProcessHeader processHeader)
		{
			return processHeader.FH_ParentId.IsValid
					&& ((processHeader.FH_P0_Template.IsEmpty && processHeader.FH_ParentTemplateId.IsValid) || !processHeader.IsWorkflow);
		}

		protected override bool ParentExists(ProcessHeader processHeader)
		{
			var parent = processHeader.Parent as BusinessObject;
			return parent != null && !parent.IsDeleted && parent.IsInDatabase;
		}

		protected override bool IsDuplicate(ProcessHeader original, ProcessHeader suspected)
		{
			return original.FH_ParentId == suspected.FH_ParentId
				&& original.FH_ParentTemplateId == suspected.FH_ParentTemplateId
				&& original.FH_CompletionStatement.ToString().Equals(suspected.FH_CompletionStatement.ToString(), StringComparison.OrdinalIgnoreCase);
		}

		protected override bool Merge(ProcessHeader original, ProcessHeader duplicate)
		{
			if (duplicate.Tasks.Any(t => t.IsInDatabase))
			{
				var templateName = duplicate.GetTemplate()?.P0_Name ?? (NoResString)"Unknown Template"; // It is an exception message

				var message = $@"Unreconcilable concurrency error due to Workflow Template application in {duplicate.FH_CompletionStatement} from {templateName}.
This error has occurred because the same Workflow Template has been applied twice concurrently and Workflows have been duplicated.
This operation has been cancelled. The duplicate workflow has the Create dates 1:{original.InstantiationTime} 2:{duplicate.InstantiationTime}."; // It is an exception message

				ThrowConcurrencyException(duplicate, message);
			}

			duplicate.Delete();
			return true;
		}
	}
}
