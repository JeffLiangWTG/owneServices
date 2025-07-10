using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.DocumentScanning.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.DocumentScanning.GUI
{
	public partial class ArchiveEDocsForm : ZChildForm
	{
		public ArchiveEDocsForm(ArchiveEDocsManager businessEntity)
			: base(businessEntity)
		{
			InitializeComponent();

			Manager.TotalSizeInfo.ValueChanged += new EventHandler(UpdateTotalSizeLabel);
			Manager.NoDocumentsAddedToList += new EventHandler(Manager_NoDocumentsAddedToList);
			UpdateContextMenu();

			OrganisationInstructionsLabel.Text = Res.GetString("ArchiveEDocsForm|OrganisationInstructionsLabel", "Select the Organization you want to filter by. This will search all Shipments, Declarations, Consols and Port Transport jobs for the matching Organization.");
			DatesInstructionsLabel.Text = Res.GetString("ArchiveEDocsForm|DatesInstructionsLabel", "Select the dates to filter by. Only Shipments, Declarations, Consols and Port Transport jobs that match the ETA, ETD, Jobs Closed dates will be returned.");
			InstructionsLabel.Text = Res.GetString("ArchiveEDocsForm|InstructionsLabel", "To remove items from the CD, right click on the grid row with your mouse and select 'Remove from CD'");

			//SelectOrganisationGroupbox.Text = Res.GetString("ArchiveEDocsForm|SelectOrganization", "1. Select Organization (Required)");
			//SelectDatesGroupBox.Text = Res.GetString("ArchiveEDocsForm|SelectDates", "2. Select Dates (Optional)");
			//SelectTypesGroupBox.Text = Res.GetString("ArchiveEDocsForm|SelectSearchType", "3. Select Search Type (Required)");
		}

		public ArchiveEDocsManager Manager
		{
			get { return (ArchiveEDocsManager)BusinessEntity; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		#region Actions

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		protected override void ZForm_Closing(object sender, CancelEventArgs e)
		{
			//Don't want to trigger handleSaveButton, because we shouldn't save data from this form.
		}

		void BurnCDButton_Click(object sender, EventArgs e)
		{
			if (ListToArchiveGrid.ListManager.List.Count > 0)
			{
				ChooseCDSoftwareManager cDSoftwareManager = new ChooseCDSoftwareManager(Manager.MasterFactory, Manager);
				ChooseCDSoftwareForm chooseSoftwareForm = new ChooseCDSoftwareForm(cDSoftwareManager);
				ZFormModaliser.Show(chooseSoftwareForm, this);
			}
			else
			{
				Globals.Message.Show(Res.GetString("389fa977-b8b1-4976-b3a2-3658be8f83fe", "There are no eDocs to write to CD. Please select an Organization and/or Dates from the filter at the top and press the 'Add to CD' button."), Res.GetString("1a4ee481-e65f-43b8-9466-cddf14d468d6", "No eDocs Selected"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		void UpdateTotalSizeLabel(object sender, EventArgs e)
		{
			TotalSizeLabel.ForeColor = (Convert.ToInt32(Manager.TotalSize.ToString()) > 700) ? Color.Red : SystemColors.ControlText;
		}

		void UpdateContextMenu()
		{
			RenameMenuItem(ArchivedDocumentsGrid.ContextMenu, Res.GetString("e843d46e-e2db-49d2-be5b-6cf28f3f109e", "Remove"), Res.GetString("d28e58c7-0a36-450d-9cae-ef67845bcfa3", "Remove from CD"));
			RenameMenuItem(ListToArchiveGrid.ContextMenu, Res.GetString("e843d46e-e2db-49d2-be5b-6cf28f3f109e", "Remove"), Res.GetString("d28e58c7-0a36-450d-9cae-ef67845bcfa3", "Remove from CD"));
		}

		void RenameMenuItem(ContextMenu menu, ZString oldName, ZString replacementName)
		{
			MenuItem itemToReplace = null;

			foreach (MenuItem item in menu.MenuItems)
			{
				if (item.Text.IndexOf(oldName) != -1)
				{
					itemToReplace = item; break;
				}
			}

			if (itemToReplace != null)
			{
				itemToReplace.Text = replacementName;
			}
		}

		[SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule", Justification = "Baseline")]
		void AddToCDButton_Click(object sender, EventArgs e)
		{
			Manager.Validation.ValidateAll();
			Manager.RefreshBinding();
			if (!Manager.HasErrors)
			{
				using (ProgressForm progressForm = new ProgressForm())
				{
					progressForm.ShowCancelButton = false;
					progressForm.ShowProgressBar = false;
					progressForm.Status = Res.GetString("cf0134d8-aa70-408e-a8bc-787a1bc7aca3", "Please wait while the system searches for matching eDocs.");
					progressForm.ShowInTaskbar = false;
					ZFormModaliser.Show(progressForm, this);
					Application.DoEvents();

					try
					{
						Manager.CreateArchiveList();
					}
					finally
					{
						progressForm.Close();
					}
				}
			}
			else
			{
				Globals.Message.Show(Res.GetString("ee99a2f5-9b6c-4f5d-94e3-7d94b7831a0b", "Please correct any errors and try again."), Res.GetString("48b13da2-fcd8-4adc-8ca6-add31dad7ca7", "Could not find matching eDocs"), MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
			}
		}

		void Manager_NoDocumentsAddedToList(object sender, EventArgs e)
		{
			Globals.Message.Show(Res.GetString("34f2a4c5-8a1d-4a7b-8293-2c4dcca7d9b6", "No eDocs found on jobs that match the search criteria."), Res.GetString("712397d5-94ee-4348-8552-b83986f80fff", "No eDocs Found"), MessageBoxButtons.OK, MessageBoxIcon.None);
		}

		#endregion
	}
}
