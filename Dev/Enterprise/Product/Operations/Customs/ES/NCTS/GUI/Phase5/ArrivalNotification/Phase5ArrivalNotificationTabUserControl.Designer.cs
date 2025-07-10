namespace Enterprise.Customs.ES.NCTS.GUI
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
			this.SummaryDeclarationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.DynamicSummaryDeclarationLayoutPanel = new Enterprise.ZArchitecture.GUI.DynamicLayoutPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SummaryDeclarationGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.NCTS.Business.NctsHeader);
			// 
			// SummaryDeclarationGroupBox
			// 
			this.SummaryDeclarationGroupBox.CaptionResourceString = Enterprise.Customs.ES.NCTS.GUI.Res.GetData("AD6A737D-9282-4459-A075-F3A983F4491E", "Summary Declaration");
			this.SummaryDeclarationGroupBox.Controls.Add(this.DynamicSummaryDeclarationLayoutPanel);
			this.SummaryDeclarationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 366, true);
			this.SummaryDeclarationGroupBox.Name = "SummaryDeclarationGroupBox";
			this.SummaryDeclarationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 163, true);
			this.SummaryDeclarationGroupBox.TabIndex = 3;
			this.SummaryDeclarationGroupBox.TabStop = false;
			// 
			// DynamicSummaryDeclarationLayoutPanel
			// 
			this.DynamicSummaryDeclarationLayoutPanel.AllowDrop = true;
			this.DynamicSummaryDeclarationLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DynamicSummaryDeclarationLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DynamicSummaryDeclarationLayoutPanel.Name = "DynamicSummaryDeclarationLayoutPanel";
			this.DynamicSummaryDeclarationLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(802, 144, true);
			this.DynamicSummaryDeclarationLayoutPanel.TabIndex = 0;
			// 
			// Phase5ArrivalNotificationTabUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.SummaryDeclarationGroupBox);
			this.Name = "Phase5ArrivalNotificationTabUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1317, 532, true);
			this.Controls.SetChildIndex(this.SummaryDeclarationGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.SummaryDeclarationGroupBox.ResumeLayout(false);
			this.SummaryDeclarationGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal ZArchitecture.GUI.ZGroupBox SummaryDeclarationGroupBox;
		internal ZArchitecture.GUI.DynamicLayoutPanel DynamicSummaryDeclarationLayoutPanel;
	}
}
