using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class WorkItemWorkflowDescriptor : ProcessManagement.Business.WorkItemWorkflowDescriptor
	{
		public override Type WorkflowProviderType
		{
			get { return typeof(NewWorkItem); }
		}

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new EDIWorkItemFormCustomisationSettingsProvider();
		}

		protected override bool SupportsReapplyTemplatesMenuItemCore => true;
	}
}

