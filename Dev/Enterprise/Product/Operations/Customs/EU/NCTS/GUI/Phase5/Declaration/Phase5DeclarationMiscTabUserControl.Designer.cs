namespace Enterprise.Customs.EU.NCTS.GUI
{
	partial class Phase5DeclarationMiscTabUserControl
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
			this.DynamicMiscOptionsPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.MiscOptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MiscOptionsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.NCTS.Business.NctsHeader);
			// 
			// DynamicMiscOptionsPanel
			// 
			this.DynamicMiscOptionsPanel.AllowDrop = true;
			this.DynamicMiscOptionsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicMiscOptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicMiscOptionsPanel.Name = "DynamicMiscOptionsPanel";
			this.DynamicMiscOptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(423, 121, true);
			this.DynamicMiscOptionsPanel.TabIndex = 1;
			// 
			// MiscOptionsGroupBox
			// 
			this.MiscOptionsGroupBox.CaptionResourceString = Enterprise.Customs.EU.NCTS.GUI.Res.GetData("10f46fd4-5175-4dd2-9475-0fb87102ee1c", "Miscellaneous Options");
			this.MiscOptionsGroupBox.Controls.Add(this.DynamicMiscOptionsPanel);
			this.MiscOptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MiscOptionsGroupBox.Name = "MiscOptionsGroupBox";
			this.MiscOptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(429, 140, true);
			this.MiscOptionsGroupBox.TabIndex = 0;
			this.MiscOptionsGroupBox.TabStop = false;
			// 
			// Phase5DeclarationMiscTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MiscOptionsGroupBox);
			this.Name = "Phase5DeclarationMiscTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1001, 720, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MiscOptionsGroupBox.ResumeLayout(false);
			this.MiscOptionsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZGroupBox MiscOptionsGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicMiscOptionsPanel;

		#endregion
	}
}
