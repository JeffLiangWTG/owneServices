namespace Enterprise.Customs.KR.GUI
{
	partial class Import5FNMessageDetailsUserControl
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
      this.DetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.DetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      this.PostClearanceDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.PostClearanceDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      this.DutyReductionDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.DutyReductionDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      this.ReExportReductionDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.ReExportReductionDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.DetailsGroupBox.SuspendLayout();
      this.PostClearanceDetailsGroupBox.SuspendLayout();
      this.DutyReductionDetailsGroupBox.SuspendLayout();
      this.ReExportReductionDetailsGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // BindingSource
      // 
      this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclarationMiscMessageSendingObject);
      // 
      // DetailsGroupBox
      // 
      this.DetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("9F3DB2CA-D83B-4C1A-A015-A29B297C68F6", "Details");
      this.DetailsGroupBox.Controls.Add(this.DetailsPanel);
      this.DetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.DetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.DetailsGroupBox.Name = "DetailsGroupBox";
      this.DetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 126, true);
      this.DetailsGroupBox.TabIndex = 0;
      this.DetailsGroupBox.TabStop = false;
      // 
      // DetailsPanel
      // 
      this.DetailsPanel.AllowDrop = true;
      this.DetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.DetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
      this.DetailsPanel.Name = "DetailsPanel";
      this.DetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 107, true);
      this.DetailsPanel.TabIndex = 0;
      // 
      // PostClearanceDetailsGroupBox
      // 
      this.PostClearanceDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("FA6D0ACD-39B4-485F-B4E2-3CC399A1BEB5", "Post Clearance Details");
      this.PostClearanceDetailsGroupBox.Controls.Add(this.PostClearanceDetailsPanel);
      this.PostClearanceDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.PostClearanceDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 126, true);
      this.PostClearanceDetailsGroupBox.Name = "PostClearanceDetailsGroupBox";
      this.PostClearanceDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 94, true);
      this.PostClearanceDetailsGroupBox.TabIndex = 1;
      this.PostClearanceDetailsGroupBox.TabStop = false;
      // 
      // PostClearanceDetailsPanel
      // 
      this.PostClearanceDetailsPanel.AllowDrop = true;
      this.PostClearanceDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.PostClearanceDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
      this.PostClearanceDetailsPanel.Name = "PostClearanceDetailsPanel";
      this.PostClearanceDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(805, 75, true);
      this.PostClearanceDetailsPanel.TabIndex = 0;
      // 
      // DutyReductionDetailsGroupBox
      // 
      this.DutyReductionDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("BB8382A2-7F7F-42A2-8C04-62FD135ABBE2", "Duty Reduction Details");
      this.DutyReductionDetailsGroupBox.Controls.Add(this.DutyReductionDetailsPanel);
      this.DutyReductionDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Left;
      this.DutyReductionDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 220, true);
      this.DutyReductionDetailsGroupBox.Name = "DutyReductionDetailsGroupBox";
      this.DutyReductionDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(413, 71, true);
      this.DutyReductionDetailsGroupBox.TabIndex = 2;
      this.DutyReductionDetailsGroupBox.TabStop = false;
      // 
      // DutyReductionDetailsPanel
      // 
      this.DutyReductionDetailsPanel.AllowDrop = true;
      this.DutyReductionDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.DutyReductionDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
      this.DutyReductionDetailsPanel.Name = "DutyReductionDetailsPanel";
      this.DutyReductionDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(407, 52, true);
      this.DutyReductionDetailsPanel.TabIndex = 0;
      // 
      // ReExportReductionDetailsGroupBox
      // 
      this.ReExportReductionDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("422B2FBB-C105-4D11-86AF-A8643938F3E1", "Re-export Reduction Details");
      this.ReExportReductionDetailsGroupBox.Controls.Add(this.ReExportReductionDetailsPanel);
      this.ReExportReductionDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ReExportReductionDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(413, 220, true);
      this.ReExportReductionDetailsGroupBox.Name = "ReExportReductionDetailsGroupBox";
      this.ReExportReductionDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(398, 71, true);
      this.ReExportReductionDetailsGroupBox.TabIndex = 3;
      this.ReExportReductionDetailsGroupBox.TabStop = false;
      // 
      // ReExportReductionDetailsPanel
      // 
      this.ReExportReductionDetailsPanel.AllowDrop = true;
      this.ReExportReductionDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.ReExportReductionDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
      this.ReExportReductionDetailsPanel.Name = "ReExportReductionDetailsPanel";
      this.ReExportReductionDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(392, 52, true);
      this.ReExportReductionDetailsPanel.TabIndex = 0;
      // 
      // Import5FNMessageDetailsUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.ReExportReductionDetailsGroupBox);
      this.Controls.Add(this.DutyReductionDetailsGroupBox);
      this.Controls.Add(this.PostClearanceDetailsGroupBox);
      this.Controls.Add(this.DetailsGroupBox);
      this.Name = "Import5FNMessageDetailsUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(811, 291, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.DetailsGroupBox.ResumeLayout(false);
      this.DetailsGroupBox.PerformLayout();
      this.PostClearanceDetailsGroupBox.ResumeLayout(false);
      this.PostClearanceDetailsGroupBox.PerformLayout();
      this.DutyReductionDetailsGroupBox.ResumeLayout(false);
      this.DutyReductionDetailsGroupBox.PerformLayout();
      this.ReExportReductionDetailsGroupBox.ResumeLayout(false);
      this.ReExportReductionDetailsGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.DynamicLayoutPanel DetailsPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel PostClearanceDetailsPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel DutyReductionDetailsPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel ReExportReductionDetailsPanel;
		public ZArchitecture.GUI.ZGroupBox DetailsGroupBox;
		public ZArchitecture.GUI.ZGroupBox PostClearanceDetailsGroupBox;
		public ZArchitecture.GUI.ZGroupBox DutyReductionDetailsGroupBox;
		public ZArchitecture.GUI.ZGroupBox ReExportReductionDetailsGroupBox;
	}
}
