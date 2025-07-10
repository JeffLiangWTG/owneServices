namespace Enterprise.Customs.FR.GUI
{
	partial class EntryInstructionDetailsUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.FR.Business.Declaration.JobDeclaration);
			// 
			// DetailsUserControl
			// 
			this.DetailsUserControl.UserControlType = typeof(Enterprise.Customs.FR.GUI.EntryInstructionDetailBasicUserControl);
			// 
			// EntryInstructionGridUserControl
			// 
			this.EntryInstructionGridUserControl.UserControlType = typeof(Enterprise.Customs.FR.GUI.EntryInstructionGridUserControl);
			//
			// GuaranteesUserControl
			// 
			this.GuaranteesUserControl.UserControlType = typeof(Enterprise.Customs.FR.GUI.EntryInstructionGuaranteesUserControl);
			// 
			// EntryInstructionDetailsUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "EntryInstructionDetailsUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1280, 456, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
