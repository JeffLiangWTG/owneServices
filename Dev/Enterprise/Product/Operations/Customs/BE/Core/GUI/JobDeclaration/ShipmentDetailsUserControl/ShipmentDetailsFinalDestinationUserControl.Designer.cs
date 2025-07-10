namespace Enterprise.Customs.BE.GUI
{
	partial class ShipmentDetailsFinalDestinationUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

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
		private void InitializeComponent()
		{
			this.FinalDestinationFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.FinalDestinationFindBox.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BE.Business.Declaration.JobDeclaration);
			//
			// FinalDestinationFindBox
			//
			this.FinalDestinationFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.FinalDestinationFindBox, "JE_RL_NKFinalDestination");
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BE.Business.Declaration.JobDeclaration)(null)).JE_RL_NKFinalDestination)));
			this.FinalDestinationFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.FinalDestinationFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.RefUNLOCO;
			this.FinalDestinationFindBox.Name = "FinalDestinationFindBox";
			this.FinalDestinationFindBox.CaptionResourceString = Enterprise.Customs.BE.GUI.Res.GetData("48B3287D-A4D3-4E4F-A67B-8E582851514D", "[UCC 5/8] Dest.", "[UCC 5/8] Destination", "[UCC 5/8] Country of Destination");
			this.FinalDestinationFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
			this.FinalDestinationFindBox.ParentType = null;
			this.FinalDestinationFindBox.PreBoundMaxLength = 5;
			this.FinalDestinationFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			this.FinalDestinationFindBox.TabIndex = 0;
			//
			// ShipmentDetailsFinalDestinationUserControl
			//
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.FinalDestinationFindBox);
			this.Name = "ShipmentDetailsFinalDestinationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(154, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FinalDestinationFindBox.ResumeLayout(true);
			this.FinalDestinationFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
		internal Enterprise.ZArchitecture.GUI.ZCodeFindBox FinalDestinationFindBox;
	}
}
