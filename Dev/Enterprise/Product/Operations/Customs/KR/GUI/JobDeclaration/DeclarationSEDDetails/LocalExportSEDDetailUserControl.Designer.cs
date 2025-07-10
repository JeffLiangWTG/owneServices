using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.KR.GUI
{
	partial class LocalExportSEDDetailUserControl
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
			this.MainPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			this.SEDDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SEDDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.KR.Business.JobDeclaration);
			// 
			// MainPanel
			// 
			this.MainPanel.AllowDrop = true;
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 155, true);
			this.MainPanel.TabIndex = 2;
			// 
			// SEDDetailsGroupBox
			// 
			this.SEDDetailsGroupBox.CaptionResourceString = Enterprise.Customs.KR.GUI.Res.GetData("0ae180fc-da40-4051-beb6-3568aa1eee68", "SED Details");
			this.SEDDetailsGroupBox.Controls.Add(this.MainPanel);
			this.SEDDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SEDDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.SEDDetailsGroupBox.Name = "SEDDetailsGroupBox";
			this.SEDDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 172, true);
			this.SEDDetailsGroupBox.TabIndex = 0;
			this.SEDDetailsGroupBox.TabStop = false;
			// 
			// LocalExportSEDDetailUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.SEDDetailsGroupBox);
			this.Name = "LocalExportSEDDetailUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 172, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SEDDetailsGroupBox.ResumeLayout(false);
			this.SEDDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.DynamicLayoutPanel MainPanel;
		private ZGroupBox SEDDetailsGroupBox;
	}
}
