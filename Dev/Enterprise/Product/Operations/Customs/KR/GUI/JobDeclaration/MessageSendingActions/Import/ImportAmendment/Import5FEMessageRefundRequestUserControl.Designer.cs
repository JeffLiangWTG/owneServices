namespace Enterprise.Customs.KR.GUI
{
	partial class Import5FEMessageRefundRequestUserControl
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
      this.LeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
      this.RefundHeaderGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.RefundHeaderPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      this.DeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.DeclarationPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      this.PaidAndRefundOfTaxAndPenaltyGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
      this.PaidAndRefundOfTaxAndPenaltyPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
      this.LeftPanel.SuspendLayout();
      this.RefundHeaderGroupBox.SuspendLayout();
      this.DeclarationGroupBox.SuspendLayout();
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.SuspendLayout();
      this.SuspendLayout();
      // 
      // LeftPanel
      // 
      this.LeftPanel.Controls.Add(this.RefundHeaderGroupBox);
      this.LeftPanel.Controls.Add(this.DeclarationGroupBox);
      this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
      this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.LeftPanel.Name = "LeftPanel";
      this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 433, true);
      this.LeftPanel.TabIndex = 0;
      // 
      // RefundHeaderGroupBox
      // 
      this.RefundHeaderGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("b1bcc9e6-db57-4076-b5f8-ff30ce7d573f", "Refund Header");
      this.RefundHeaderGroupBox.Controls.Add(this.RefundHeaderPanel);
      this.RefundHeaderGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.RefundHeaderGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 140, true);
      this.RefundHeaderGroupBox.Name = "RefundHeaderGroupBox";
      this.RefundHeaderGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 293, true);
      this.RefundHeaderGroupBox.TabIndex = 1;
      this.RefundHeaderGroupBox.TabStop = false;
      // 
      // RefundHeaderPanel
      // 
      this.RefundHeaderPanel.AllowDrop = true;
      this.RefundHeaderPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.RefundHeaderPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
      this.RefundHeaderPanel.Name = "RefundHeaderPanel";
      this.RefundHeaderPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 276, true);
      this.RefundHeaderPanel.TabIndex = 0;
      // 
      // DeclarationGroupBox
      // 
      this.DeclarationGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("5a8ee1f6-ad0b-4f7a-b92c-5b3c204eb663", "Declaration");
      this.DeclarationGroupBox.Controls.Add(this.DeclarationPanel);
      this.DeclarationGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
      this.DeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
      this.DeclarationGroupBox.Name = "DeclarationGroupBox";
      this.DeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 140, true);
      this.DeclarationGroupBox.TabIndex = 0;
      this.DeclarationGroupBox.TabStop = false;
      // 
      // DeclarationPanel
      // 
      this.DeclarationPanel.AllowDrop = true;
      this.DeclarationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.DeclarationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
      this.DeclarationPanel.Name = "DeclarationPanel";
      this.DeclarationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(359, 123, true);
      this.DeclarationPanel.TabIndex = 0;
      // 
      // PaidAndRefundOfTaxAndPenaltyGroupBox
      // 
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("21f99bdc-b486-42b0-bbd7-32c4b1e7ae0c", "Paid, Refund (Tax && Penalty)");
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.Controls.Add(this.PaidAndRefundOfTaxAndPenaltyPanel);
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(363, 0, true);
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.Name = "PaidAndRefundOfTaxAndPenaltyGroupBox";
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 433, true);
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.TabIndex = 1;
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.TabStop = false;
      // 
      // PaidAndRefundOfTaxAndPenaltyPanel
      // 
      this.PaidAndRefundOfTaxAndPenaltyPanel.AllowDrop = true;
      this.PaidAndRefundOfTaxAndPenaltyPanel.AutoScroll = true;
      this.PaidAndRefundOfTaxAndPenaltyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
      this.PaidAndRefundOfTaxAndPenaltyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
      this.PaidAndRefundOfTaxAndPenaltyPanel.Name = "PaidAndRefundOfTaxAndPenaltyPanel";
      this.PaidAndRefundOfTaxAndPenaltyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(326, 416, true);
      this.PaidAndRefundOfTaxAndPenaltyPanel.TabIndex = 0;
      // 
      // Import5FEMessageRefundRequestUserControl
      // 
      this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
      this.CaptionRenderingEnabled = true;
      this.Controls.Add(this.PaidAndRefundOfTaxAndPenaltyGroupBox);
      this.Controls.Add(this.LeftPanel);
      this.Name = "Import5FEMessageRefundRequestUserControl";
      this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(693, 433, true);
      ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
      this.LeftPanel.ResumeLayout(false);
      this.LeftPanel.PerformLayout();
      this.RefundHeaderGroupBox.ResumeLayout(false);
      this.RefundHeaderGroupBox.PerformLayout();
      this.DeclarationGroupBox.ResumeLayout(false);
      this.DeclarationGroupBox.PerformLayout();
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.ResumeLayout(false);
      this.PaidAndRefundOfTaxAndPenaltyGroupBox.PerformLayout();
      this.ResumeLayout(false);
      this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZPanel LeftPanel;
		internal ZArchitecture.GUI.ZGroupBox DeclarationGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DeclarationPanel;
		internal ZArchitecture.GUI.ZGroupBox RefundHeaderGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel RefundHeaderPanel;
		internal ZArchitecture.GUI.ZGroupBox PaidAndRefundOfTaxAndPenaltyGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel PaidAndRefundOfTaxAndPenaltyPanel;
	}
}
