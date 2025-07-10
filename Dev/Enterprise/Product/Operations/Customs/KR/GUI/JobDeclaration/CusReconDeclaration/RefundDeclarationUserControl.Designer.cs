namespace Enterprise.Customs.KR.GUI
{
	partial class RefundDeclarationUserControl
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
			this.PayerGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.PayerPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.CustomsDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomsDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.RefundDeclarationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefundDeclarationDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.LeftPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PayerGroupBox.SuspendLayout();
			this.CustomsDetailsGroupBox.SuspendLayout();
			this.RefundDeclarationDetailsGroupBox.SuspendLayout();
			this.LeftPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// PayerGroupBox
			// 
			this.PayerGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("B1230DC3-48D9-46F5-992C-25A004D070C0", "Payer");
			this.PayerGroupBox.Controls.Add(this.PayerPanel);
			this.PayerGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.PayerGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PayerGroupBox.Name = "PayerGroupBox";
			this.PayerGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 164, true);
			this.PayerGroupBox.TabIndex = 1;
			this.PayerGroupBox.TabStop = false;
			// 
			// PayerPanel
			// 
			this.PayerPanel.AllowDrop = true;
			this.PayerPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PayerPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.PayerPanel.Name = "PayerPanel";
			this.PayerPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 143, true);
			this.PayerPanel.TabIndex = 1;
			// 
			// CustomsDetailsGroupBox
			// 
			this.CustomsDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("88129AA8-9668-48BA-A797-1DEE60983626", "Customs Detail");
			this.CustomsDetailsGroupBox.Controls.Add(this.CustomsDetailsPanel);
			this.CustomsDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 164, true);
			this.CustomsDetailsGroupBox.Name = "CustomsDetailsGroupBox";
			this.CustomsDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 285, true);
			this.CustomsDetailsGroupBox.TabIndex = 2;
			this.CustomsDetailsGroupBox.TabStop = false;
			// 
			// CustomsDetailsPanel
			// 
			this.CustomsDetailsPanel.AllowDrop = true;
			this.CustomsDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.CustomsDetailsPanel.Name = "CustomsDetailsPanel";
			this.CustomsDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 264, true);
			this.CustomsDetailsPanel.TabIndex = 2;
			// 
			// RefundDeclarationDetailsGroupBox
			// 
			this.RefundDeclarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("4BBE18B1-4CA2-42A3-88EE-7AA49299AE24", "Refund Declaration Details");
			this.RefundDeclarationDetailsGroupBox.Controls.Add(this.RefundDeclarationDetailsPanel);
			this.RefundDeclarationDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefundDeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 0, true);
			this.RefundDeclarationDetailsGroupBox.Name = "RefundDeclarationDetailsGroupBox";
			this.RefundDeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 449, true);
			this.RefundDeclarationDetailsGroupBox.TabIndex = 3;
			this.RefundDeclarationDetailsGroupBox.TabStop = false;
			// 
			// RefundDeclarationDetailsPanel
			// 
			this.RefundDeclarationDetailsPanel.AllowDrop = true;
			this.RefundDeclarationDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefundDeclarationDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.RefundDeclarationDetailsPanel.Name = "RefundDeclarationDetailsPanel";
			this.RefundDeclarationDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 428, true);
			this.RefundDeclarationDetailsPanel.TabIndex = 3;
			// 
			// LeftPanel
			// 
			this.LeftPanel.Controls.Add(this.CustomsDetailsGroupBox);
			this.LeftPanel.Controls.Add(this.PayerGroupBox);
			this.LeftPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.LeftPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.LeftPanel.Name = "LeftPanel";
			this.LeftPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 449, true);
			this.LeftPanel.TabIndex = 0;
			// 
			// RefundDeclarationUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.RefundDeclarationDetailsGroupBox);
			this.Controls.Add(this.LeftPanel);
			this.Name = "RefundDeclarationUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 449, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PayerGroupBox.ResumeLayout(false);
			this.PayerGroupBox.PerformLayout();
			this.CustomsDetailsGroupBox.ResumeLayout(false);
			this.CustomsDetailsGroupBox.PerformLayout();
			this.RefundDeclarationDetailsGroupBox.ResumeLayout(false);
			this.RefundDeclarationDetailsGroupBox.PerformLayout();
			this.LeftPanel.ResumeLayout(false);
			this.LeftPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox PayerGroupBox;
		private ZArchitecture.GUI.ZGroupBox CustomsDetailsGroupBox;
		private ZArchitecture.GUI.ZGroupBox RefundDeclarationDetailsGroupBox;
		private ZArchitecture.GUI.DynamicLayoutPanel PayerPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel CustomsDetailsPanel;
		private ZArchitecture.GUI.DynamicLayoutPanel RefundDeclarationDetailsPanel;
		private ZArchitecture.GUI.ZPanel LeftPanel;
	}
}
