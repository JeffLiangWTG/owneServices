namespace Enterprise.Customs.IT.GUI
{
	partial class EntryInstructionDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.EntryInstructionTabControl.SuspendLayout();
			this.AuthorisationsTabPage.SuspendLayout();
			this.EntryInstructionGridUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// EntryInstructionTabControl
			// 
			this.EntryInstructionTabControl.Controls.SetChildIndex(this.AuthorisationsTabPage, 0);
			// 
			// AuthorisationsTabPage
			// 
			this.AuthorisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AuthorisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			// 
			// DetailsUserControl
			//
			this.DetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1329, 405, true);
			// 
			// EntryInstructionGridUserControl
			// 
			this.EntryInstructionGridUserControl.UserControlType = typeof(Enterprise.Customs.IT.GUI.EntryInstructionGridUserControl);
			// 
			// EntryInstructionDetailsUserControl
			// 
			this.Name = "EntryInstructionDetailsUserControl";
			this.EntryInstructionTabControl.ResumeLayout(false);
			this.EntryInstructionTabControl.PerformLayout();
			this.AuthorisationsTabPage.ResumeLayout(false);
			this.AuthorisationsTabPage.PerformLayout();
			this.EntryInstructionGridUserControl.ResumeLayout(true);
			this.EntryInstructionGridUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
