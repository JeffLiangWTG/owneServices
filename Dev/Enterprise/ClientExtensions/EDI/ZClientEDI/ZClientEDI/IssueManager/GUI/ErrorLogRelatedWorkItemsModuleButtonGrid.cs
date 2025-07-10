using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using CargoWise.EntityFramework;
using Enterprise.Client.EDI.IncidentManager.Business;
using Enterprise.Client.EDI.IncidentManager.GUI;
using Enterprise.Client.EDI.IssueManager.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.EDI.IssueManager.GUI
{
	partial class ErrorLogRelatedWorkItemsModuleButtonGrid : RelatedWorkItemModuleButtonGrid
	{
		public ErrorLogRelatedWorkItemsModuleButtonGrid()
		{
			InitializeComponent();
			createIncidentsButton.AllowOverlap(mainLayoutPanel);
		}

		[Browsable(false), DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
		public EdiHelpErrorLog Issue
		{
			get { return issue; }
			set { issue = value; }
		}
		EdiHelpErrorLog issue;

		protected override void Edit(BusinessObject selected, object sender)
		{
			base.Edit(selected, sender);
			NewWorkItemForm workItemForm = LastShownZForm as NewWorkItemForm;
			if (workItemForm != null && workItemForm.DataSource != null)
			{
				workItemForm.DataSource.Factory.Saved += new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
		}

		protected override void OnEditFormClosed(object sender, EventArgs e)
		{
			base.OnEditFormClosed(sender, e);
			NewWorkItemForm workItemForm = sender as NewWorkItemForm;
			if (workItemForm != null)
			{
				workItemForm.DataSource.Factory.Saved -= new BusinessObjectFactory.SavedEventHandler(Factory_Saved);
			}
		}

		void CreateIncidentsButton_Click(object sender, EventArgs e)
		{
			if (!Issue.HasWorkItems)
			{
				Globals.Message.ShowError("There are no work items attached to this issue. Please create a work item first in order to create incidents.");
			}
			else if (Issue.Occurrences.Count == 0)
			{
				Globals.Message.ShowError("There are no occurrences for this issue. Incidents cannot be created.");
			}
			else if (Globals.Message.Show("This operation may create lots of incidents, do you want to continue?", "Confirmation", MessageBoxButtons.OKCancel, DialogResult.Cancel) == DialogResult.OK)
			{
				CreateIncidents();
			}
		}

		void CreateIncidents()
		{
			List<SupportIncident> incidents = new List<SupportIncident>(Issue.CreateIncidents(true));
			string message;

			if (incidents.Count > 0)
			{
				message = EdiHelpErrorLog.GetIncidentCreatedMessage(incidents);
			}
			else
			{
				message = "No new incidents were created. All the client databases that the occurrences belong to already have a related work item attached to this issue.";
			}

			Globals.Message.ShowInformation(message);
		}

		void Factory_Saved(BusinessObjectFactory factory, bool savedSuccessfully)
		{
			Issue.ReloadRelatedIncidents();
		}
	}
}
