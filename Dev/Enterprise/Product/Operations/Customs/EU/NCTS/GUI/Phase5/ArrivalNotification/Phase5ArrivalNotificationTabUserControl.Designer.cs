namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5ArrivalNotificationTabUserControl
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
			this.ArrivalDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicArrivalDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.DeclarationDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicDeclarationDetailsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ArrivalDetailsGroupBox.SuspendLayout();
			this.DeclarationDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// ArrivalDetailsGroupBox
			// 
			this.ArrivalDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("E83DCEE4-8035-497E-AF25-D920F35A7690", "Arrival Details");
			this.ArrivalDetailsGroupBox.Controls.Add(this.DynamicArrivalDetailsPanel);
			this.ArrivalDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ArrivalDetailsGroupBox.Name = "ArrivalDetailsGroupBox";
			this.ArrivalDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(810, 360, true);
			this.ArrivalDetailsGroupBox.TabIndex = 0;
			this.ArrivalDetailsGroupBox.TabStop = false;
			// 
			// DynamicArrivalDetailsPanel
			// 
			this.DynamicArrivalDetailsPanel.AllowDrop = true;
			this.DynamicArrivalDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicArrivalDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicArrivalDetailsPanel.Name = "DynamicArrivalDetailsPanel";
			this.DynamicArrivalDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(804, 360, true);
			this.DynamicArrivalDetailsPanel.TabIndex = 1;
			// 
			// DeclarationDetailsGroupBox
			// 
			this.DeclarationDetailsGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.DeclarationDetailsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("F4D45651-EA41-4270-BC36-FED7FBCD926B", "Declaration Details");
			this.DeclarationDetailsGroupBox.Controls.Add(this.DynamicDeclarationDetailsPanel);
			this.DeclarationDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(817, 0, true);
			this.DeclarationDetailsGroupBox.Name = "DeclarationDetailsGroupBox";
			this.DeclarationDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(499, 300, true);
			this.DeclarationDetailsGroupBox.TabIndex = 2;
			this.DeclarationDetailsGroupBox.TabStop = false;
			// 
			// DynamicDeclarationDetailsPanel
			// 
			this.DynamicDeclarationDetailsPanel.AllowDrop = true;
			this.DynamicDeclarationDetailsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicDeclarationDetailsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicDeclarationDetailsPanel.Name = "DynamicDeclarationDetailsPanel";
			this.DynamicDeclarationDetailsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(493, 261, true);
			this.DynamicDeclarationDetailsPanel.TabIndex = 3;
			// 
			// Phase5ArrivalNotificationTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ArrivalDetailsGroupBox);
			this.Controls.Add(this.DeclarationDetailsGroupBox);
			this.Name = "Phase5ArrivalNotificationTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1316, 380, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ArrivalDetailsGroupBox.ResumeLayout(false);
			this.ArrivalDetailsGroupBox.PerformLayout();
			this.DeclarationDetailsGroupBox.ResumeLayout(false);
			this.DeclarationDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion


		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicDeclarationDetailsPanel;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicArrivalDetailsPanel;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox ArrivalDetailsGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox DeclarationDetailsGroupBox;
	}
}
