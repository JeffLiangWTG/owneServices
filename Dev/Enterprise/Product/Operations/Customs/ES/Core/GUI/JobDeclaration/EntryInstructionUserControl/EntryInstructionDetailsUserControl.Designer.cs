namespace Enterprise.Customs.ES.GUI
{
	partial class EntryInstructionDetailsUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.EntryInstructionTabControl.SuspendLayout();
			this.AuthorisationsTabPage.SuspendLayout();
			this.DetailsUserControl.SuspendLayout();
			this.EntryInstructionGridUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// EntryInstructionTabControl
			// 
			this.EntryInstructionTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 432, true);
			this.EntryInstructionTabControl.Controls.SetChildIndex(this.AuthorisationsTabPage, 0);
			// 
			// DetailsUserControl
			// 
			this.DetailsUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.EntryInstructionDetailBasicUserControl);
			// 
			// EntryInstructionGridUserControl
			// 
			this.EntryInstructionGridUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 150, true);
			this.EntryInstructionGridUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.EntryInstructionGridUserControl);
			// 
			// GuaranteesUserControl
			// 
			this.GuaranteesUserControl.UserControlType = typeof(Enterprise.Customs.EU.GUI.EntryInstructionGuaranteesUserControl);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.Declaration.JobDeclaration);
			// 
			// EntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "EntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 586, true);
			this.EntryInstructionTabControl.ResumeLayout(false);
			this.EntryInstructionTabControl.PerformLayout();
			this.AuthorisationsTabPage.ResumeLayout(false);
			this.AuthorisationsTabPage.PerformLayout();
			this.DetailsUserControl.ResumeLayout(true);
			this.DetailsUserControl.PerformLayout();
			this.EntryInstructionGridUserControl.ResumeLayout(true);
			this.EntryInstructionGridUserControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
