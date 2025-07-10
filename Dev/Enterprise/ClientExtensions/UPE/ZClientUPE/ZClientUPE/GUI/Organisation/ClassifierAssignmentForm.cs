using System;
using CargoWise.ComponentModel;
using Enterprise.Client.UPE.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Client.UPE.GUI
{
	public partial class ClassifierAssignmentForm : ZChildForm
	{
		public ClassifierAssignmentForm()
		{
		}

		public ClassifierAssignmentForm(UPEStaffAssignmentUpdater updater)
			: base(updater)
		{
		}

		public new UPEStaffAssignmentUpdater BusinessEntity
		{
			get { return (UPEStaffAssignmentUpdater)base.BusinessEntity; }
		}

		public override string FormCaption
		{
			get { return "Update Clasifier Roles"; }
		}

		protected void OKButton_Click(object sender, EventArgs e)
		{
			if (!BusinessEntity.StaffPKToReplaceInfo.HasErrors() && !BusinessEntity.NewStaffPKInfo.HasErrors())
			{
				try
				{
					BusinessEntity.ProcessingProgressed += new EventHandler(StaffAssignmentUpdater_ProcessingProgressed);
					using (Progress = new ProgressForm())
					{
						Progress.ShowCancelButton = false;
						Progress.Show();
						BusinessEntity.UpdateClassiferRole();
						Globals.Message.ShowInformation(BusinessEntity.NumberOfRecordsUpdated + " staff assignment(s) have been updated.");
					}
				}
				finally
				{
					BusinessEntity.ProcessingProgressed -= new EventHandler(StaffAssignmentUpdater_ProcessingProgressed);
					Close();
				}
			}
		}

		protected ProgressForm Progress;

		void StaffAssignmentUpdater_ProcessingProgressed(object sender, EventArgs e)
		{
			if (Progress != null)
			{
				string status = BusinessEntity.PercentageComplete != 100 ? "Processing" : "Complete";
				Progress.SetStatusAndPercentComplete(status, BusinessEntity.PercentageComplete);
			}
		}
	}
}
