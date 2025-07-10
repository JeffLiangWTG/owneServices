namespace Enterprise.Customs.GB.GUI.Ccsuk
{
	partial class SplitConsignmentForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.splitConsignmentUserControl1 = new Enterprise.Customs.GB.GUI.Ccsuk.SplitConsignmentUserControl();
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 182, true);
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 209, true);
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.splitConsignmentUserControl1);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 182, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 24, true);
			// 
			// splitConsignmentUserControl1
			// 
			this.splitConsignmentUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.splitConsignmentUserControl1, "."); 
			this.splitConsignmentUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitConsignmentUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitConsignmentUserControl1.Name = "splitConsignmentUserControl1";
			this.splitConsignmentUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(391, 182, true);
			this.splitConsignmentUserControl1.TabIndex = 0;
			// 
			// SplitConsignmentForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true; 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(472, 326, true);			
			this.CaptionResourceString = Enterprise.Customs.GB.GUI.Res.GetData("SplitConsignmentForm|0aa24e35-24cf-4d16-9078-96cd3357e6b5", "Split Consignment");
			this.Name = "SplitConsignmentForm";
			this.ShouldSerializeTabPageMethods = false;
			this.MainTabControl.ResumeLayout(false);
			this.MainTabPage.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private SplitConsignmentUserControl splitConsignmentUserControl1;
		 
	}
}
