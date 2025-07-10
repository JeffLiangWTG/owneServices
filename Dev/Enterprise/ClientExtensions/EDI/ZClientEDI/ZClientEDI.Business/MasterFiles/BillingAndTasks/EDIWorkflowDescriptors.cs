using System.Collections.Generic;
using Enterprise.Client.EDI.MasterFiles.Business;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class EDIWorkflowDescriptors : WorkflowDescriptors
	{
		static IEnumerable<WorkflowDescriptor> GetDescriptors()
		{
			yield return new WorkItemWorkflowDescriptor();
			yield return new SupportIncidentWorkflowDescriptor();
			yield return new PSQuotesWorkflowDescriptor();
			yield return new ProjectWorkflowDescriptor();
			yield return new EDIGlbStaffWorkflowDescriptor();
			yield return new IncidentManagementGroupWorkflowDescriptor();
			yield return new IncidentTriageWorkflowDescriptor();
		}

		protected EDIWorkflowDescriptors()
		{
			foreach (var descriptor in GetDescriptors())
			{
				AddDescriptor(descriptor);
			}
		}

		static WorkflowDescriptors New()
		{
			return new EDIWorkflowDescriptors();
		}

		public static void RegisterThisSubTypeOverride()
		{
			OverridableNewDelegate.Value = new NewDelegate(New);
		}
	}
}

