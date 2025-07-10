namespace Enterprise.Customs.KR.GUI
{
	partial class ImportAmendmentEntryDetailsUserControl
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
      this.DynamicImportAmendmentEntryDetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      this.PenaltyAndRefundDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.PenaltyAndRefundDetailsGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // DynamicImportAmendmentEntryDetailsLayoutPanel
      // 
      this.DynamicImportAmendmentEntryDetailsLayoutPanel.AllowDrop = true;
      this.DynamicImportAmendmentEntryDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Top;
      this.DynamicImportAmendmentEntryDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.DynamicImportAmendmentEntryDetailsLayoutPanel.Name = "DynamicImportAmendmentEntryDetailsLayoutPanel";
      this.DynamicImportAmendmentEntryDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 173, true);
      this.DynamicImportAmendmentEntryDetailsLayoutPanel.TabIndex = 0;
      // 
      // PenaltyAndRefundDetailsGroupBox
      // 
      this.PenaltyAndRefundDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("2d3f1018-134f-4b53-ac13-8fdec1522b9f", "Penalty && Refund Details");
      this.PenaltyAndRefundDetailsGroupBox.Controls.Add(this.DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel);
      this.PenaltyAndRefundDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.PenaltyAndRefundDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 172, true);
      this.PenaltyAndRefundDetailsGroupBox.Name = "PenaltyAndRefundDetailsGroupBox";
      this.PenaltyAndRefundDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 305, true);
      this.PenaltyAndRefundDetailsGroupBox.TabIndex = 1;
      this.PenaltyAndRefundDetailsGroupBox.TabStop = false;
      // 
      // DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel
      // 
      this.DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel.AllowDrop = true;
      this.DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
      this.DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel.Name = "DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel";
      this.DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 289, true);
      this.DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel.TabIndex = 1;
      // 
      // ImportAmendmentEntryDetailsUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.PenaltyAndRefundDetailsGroupBox);
      this.Controls.Add(this.DynamicImportAmendmentEntryDetailsLayoutPanel);
      this.Name = "ImportAmendmentEntryDetailsUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(557, 477, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.PenaltyAndRefundDetailsGroupBox.ResumeLayout(false);
      this.PenaltyAndRefundDetailsGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.DynamicLayoutPanel DynamicImportAmendmentEntryDetailsLayoutPanel;
		private ZArchitecture.GUI.ZGroupBox PenaltyAndRefundDetailsGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel DynamicImportAmendmentPenaltyAndRefundDetailsLayoutPanel;
	}
}
