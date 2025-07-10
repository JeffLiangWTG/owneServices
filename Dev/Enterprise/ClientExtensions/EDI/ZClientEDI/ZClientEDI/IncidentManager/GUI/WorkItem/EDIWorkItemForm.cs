using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.Mail.Business;
using Enterprise.Client.EDI.Mail.GUI;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.GUI;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public class EDIWorkItemForm : WorkItemForm
	{
		public EDIWorkItemForm()
		{
		}

		public EDIWorkItemForm(EDIWorkItem workItem, ZWorkflowTabPage workflowTab = null)
			: base(workItem, workflowTab)
		{
		}

		#region Email

		protected override ContinueWithSave ValidateAndSave()
		{
			ContinueWithSave result = base.ValidateAndSave();

			if (result == ContinueWithSave.Yes)
			{
				var workItem = (EDIWorkItem)DataSource;
				foreach (CustomerServiceEmail email in workItem.DequeueClientEmails())
				{
					ZFormModaliser.ShowDialogAndDispose(new CustomerServiceEmailForm(email));
				}
			}

			return result;
		}

		#endregion
	}
}
