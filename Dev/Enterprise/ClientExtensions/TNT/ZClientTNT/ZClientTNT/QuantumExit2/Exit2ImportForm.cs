using System.Linq;
using System.Windows.Forms;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.TNT
{
	/// <summary>
	/// Form to control quantum Interface Exit 2 import to Enterprise.
	/// </summary>
	public partial class Exit2ImportForm : ZChildForm
	{
		public Exit2ImportForm(Exit2ImportManager businessEntity)
			: base(businessEntity)
		{
		}

		public Exit2ImportForm()
			: base()
		{
		}

		public override string FormCaption
		{
			get { return "Quantum Interface Exit 2 Import"; }
		}

		public override string FormVerb
		{
			get { return ""; }
		}

		protected virtual Exit2ImportManager Manager
		{
			get { return (Exit2ImportManager)BusinessEntity; }
		}

		/// <summary>
		/// Clean up any resources being used.
		/// </summary>

		protected virtual void ProcessAllMatches()
		{
			Manager.ProcessAllMatches(false);
		}

		protected ProgressForm QuantumImportProgressForm;

		protected void ProcessAndSave()
		{
			using (QuantumImportProgressForm = new ProgressForm())
			{
				QuantumImportProgressForm.ShowCancelButton = false;
				Manager.OnProgress += new TNTProgressEventHandler(Manager_OnProgress);
				QuantumImportProgressForm.ShowModalTo(this);
				ProcessAllMatches();
				QuantumImportProgressForm.Status = "Saving everything";
				QuantumImportProgressForm.PercentComplete = 100;
				Save(Manager.SaveFactories.ToArray());
			}
			IsSaved = true;
			Globals.Message.Show(Manager.GetExit2ProcessReport());
			if (!Manager.ErrorText.IsEmpty)
			{
				Globals.Message.Show("The following errors were encoutered during process:" + System.Environment.NewLine + System.Environment.NewLine + Manager.ErrorText, "Processing Error", MessageBoxButtons.OK, DialogResult.OK);
			}
		}

		protected bool IsSaved;

		#region Events

		private void ConsolSearchButton_Click(object sender, System.EventArgs e)
		{
			var selectedMawb = QuantumMawbsGrid.ListManager.GetCurrent() as QuantumMawb;

			if (selectedMawb != null)
			{
				selectedMawb.SearchEnterpriseConsols();
			}
		}

		private void MatchConsolButton_Click(object sender, System.EventArgs e)
		{
			MatchConsolCore();
		}

		protected virtual void MatchConsolCore()
		{
			if (QuantumMawbsGrid.SelectedElements.Length == 1 && EnterpriseConsolsGrid.SelectedElements.Length == 1)
			{
				QuantumMawb selectedMawb = (QuantumMawb)QuantumMawbsGrid.SelectedElements[0];
				ForwardingConsol selectedConsol = (ForwardingConsol)EnterpriseConsolsGrid.SelectedElements[0];

				selectedMawb.LinkedConsol = selectedConsol;
				ProcessButton.Enabled = Manager.IsAnyMawbLinkedToAnEnterpriseConsol;
			}
		}

		private void UnmatchConsolButton_Click(object sender, System.EventArgs e)
		{
			if (QuantumMawbsGrid.SelectedElements.Length == 1)
			{
				QuantumMawb selectedMawb = (QuantumMawb)QuantumMawbsGrid.SelectedElements[0];

				selectedMawb.LinkedConsol = null;
				ProcessButton.Enabled = Manager.IsAnyMawbLinkedToAnEnterpriseConsol;
			}
		}

		private void ProcessButton_Click(object sender, System.EventArgs e)
		{
			ProcessAndSave();
			Close();
		}

		private void CloseButton_Click(object sender, System.EventArgs e)
		{
			Close();
		}

		private void Exit2ImportForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			DialogResult confirmationResult = DialogResult.OK;

			if (!IsSaved && Manager.IsAnyMawbLinkedToAnEnterpriseConsol)
			{
				confirmationResult = Globals.Message.ShowConfirmation(
					"Confirm to Close",
					"You have already linked some consols. Are you sure you want to close without processing?",
					"Y", MessageBoxIcon.Question);
			}

			e.Cancel = (confirmationResult != DialogResult.OK);
		}

		#endregion

		private void Manager_OnProgress(object sender, TNTProgressEventArgs e)
		{
			QuantumImportProgressForm.Status = e.Message;
			QuantumImportProgressForm.PercentComplete = e.PercentComplete;
		}
	}
}
