using System.ComponentModel;
using Enterprise.MasterFiles.GUI;

namespace Enterprise.Client.EDI.IncidentManager.GUI
{
	public partial class EDIWorkflowTabPage : ZWorkflowTabPage
	{
		public EDIWorkflowTabPage()
		{
		}

		public EDIWorkflowTabPage(IContainer container)
		{
			container.Add(this);
		}

		protected override ZWorkflowUserControl TrackingUserControl
		{
			get
			{
				if (trackingUserControl == null)
				{
					trackingUserControl = new EDIWorkflowUserControl();
				}
				return trackingUserControl;
			}
		}

		ZWorkflowUserControl trackingUserControl;
	}
}
