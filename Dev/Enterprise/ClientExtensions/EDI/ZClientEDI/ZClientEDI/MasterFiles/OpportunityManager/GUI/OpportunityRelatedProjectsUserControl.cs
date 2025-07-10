using System;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.Module;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.MasterFiles.GUI
{
	public partial class OpportunityRelatedProjectsUserControl : ZUserControl
	{
		public OpportunityRelatedProjectsUserControl()
		{
			InitializeComponent();
		}

		void ProjectGrid_DoubleClick(object sender, EventArgs e)
		{
			if (ProjectGrid.ListManager.Position > -1)
			{
				LastController = new EDIProjectController();
				LastController.ShowEditForm((EDIProject)ProjectGrid.ListManager.GetCurrent());
			}
		}

		internal
		EDIProjectController LastController;
	}
}
