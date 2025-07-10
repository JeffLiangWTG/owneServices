using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.GUI
{
	partial class SpecialProceduresUserControl
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
			this.DynamicSpecialProceduresPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobDeclaration);
			// 
			// DynamicSpecialProceduresPanel
			// 
			this.DynamicSpecialProceduresPanel.AllowDrop = true;
			this.DynamicSpecialProceduresPanel.AutoScroll = true;
			this.DynamicSpecialProceduresPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicSpecialProceduresPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DynamicSpecialProceduresPanel.Name = "DynamicSpecialProceduresPanel";
			this.DynamicSpecialProceduresPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1107, 401, true);
			this.DynamicSpecialProceduresPanel.TabIndex = 0;
			// 
			// SpecialProceduresUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.DynamicSpecialProceduresPanel);
			this.Name = "SpecialProceduresUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1107, 401, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicSpecialProceduresPanel;
	}
}
