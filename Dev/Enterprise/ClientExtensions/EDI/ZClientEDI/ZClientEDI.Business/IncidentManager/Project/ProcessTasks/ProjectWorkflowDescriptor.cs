using System;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Client.EDI.IncidentManager.Business
{
	public class ProjectWorkflowDescriptor : ProcessManagement.Business.ProjectWorkflowDescriptor
	{
		#region ID / Description / Type

		public override Type WorkflowProviderType
		{
			get { return typeof(EDIProject); }
		}

		#endregion

		protected override FormCustomisationSettingsProvider GetFormCustomisationSettingsProvider()
		{
			return new EDIProjectFormCustomisationSettingsProvider();
		}
	}
}

