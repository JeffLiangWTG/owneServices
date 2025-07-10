using System;
using System.Text;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using Enterprise.Client.TNT.AirCargo;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

#pragma warning disable WTG1001 // Do not use the 'private' keyword.

namespace Enterprise.Client.TNT.GUI
{
	public partial class DeclarationFromAirCargoForm : ZChildForm, INotifications
	{
		public DeclarationFromAirCargoForm(DeclarationsFromCusMAWBCreator creator)
			: base(creator)
		{
			ProgressTextBox.ReadOnly = true;
			MasterBillNumLabel.Text = BusinessEntity.MasterBill.CM_MAWB;
		}

		public new DeclarationsFromCusMAWBCreator BusinessEntity
		{
			get { return (DeclarationsFromCusMAWBCreator)base.BusinessEntity; }
		}

		void CreateButton_Click(object sender, EventArgs e)
		{
			DialogResult confirmationResult = Globals.Message.ShowConfirmation(
					"Are you sure you want to create formal declaration for all Non-SAC House Bills?" + System.Environment.NewLine + "Confirm to Create",
					"Formal Declarations Creation",
					"Y", MessageBoxIcon.Question);
			if (confirmationResult == DialogResult.OK)
			{
				CreateButton.Enabled = false;
				CloseButton.Text = "Stop";
				BusinessEntity.Progress += new TNTProgressEventHandler(DeclarationCreator_Progress);
				Cancelled += new EventHandler(BusinessEntity.StopCreation);
				try
				{
					BusinessEntity.Factory.SuspendValidation();
					BusinessEntity.CreateDeclarations(this);
				}
				finally
				{
					BusinessEntity.Factory.ResumeValidation();
					BusinessEntity.Progress -= new TNTProgressEventHandler(DeclarationCreator_Progress);
					Cancelled -= new EventHandler(BusinessEntity.StopCreation);
					CloseButton.Text = "Cl&ose";
				}
			}
		}

		void CloseButton_Click(object sender, EventArgs e)
		{
			Close();
		}

		event EventHandler Cancelled;

		void DeclarationFromAirCargoForm_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (Cancelled != null)
			{
				e.Cancel = true;
				DialogResult response = Globals.Message.Show("The system is in the process of auto creating declarations." + System.Environment.NewLine + "Are you sure you want to stop it?", "Stop Auto Creations", MessageBoxButtons.YesNo, MessageBoxIcon.Warning, DialogResult.No);
				if (response == DialogResult.Yes)
				{
					Cancelled(sender, e);
				}
			}
		}

		void ClearLogIfRequired()
		{
			if (ProgressTextBox.Lines.Length > 1000)
			{
				StringBuilder existingLog = new StringBuilder();
				for (int i = 500; i < ProgressTextBox.Lines.Length; i++)
				{
					existingLog.Append(ProgressTextBox.Lines[i] + System.Environment.NewLine);
				}
				ProgressTextBox.Text = existingLog.ToString();
			}
		}

		void DeclarationCreator_Progress(object sender, TNTProgressEventArgs e)
		{
			MessageStatusBarPanel.Text = e.Message;
			ProgressBar.Value = e.PercentComplete;
		}

		#region INotifications Members

		void INotifications.Add(INotification @event)
		{
			Notify(@event);
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("CargoWiseOne", "CW1049:DontUseApplicationDoEventsRule")]
		void Notify(INotification @event)
		{
			ClearLogIfRequired();
			ProgressTextBox.Text += @event.Message + System.Environment.NewLine;
			Application.DoEvents();
		}

		#endregion
	}
}
