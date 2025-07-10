using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7PackTabControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.PackDetailsUserControl = new EUH7PackDetailsUserControl();
			this.PackDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.PackDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PackDetailsTabControl.SuspendLayout();
			this.PackDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaPack);
			// 
			// PackDetailsTabControl
			// 
			this.PackDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.PackDetailsTabControl.Controls.Add(this.PackDetailsTabPage);
			this.PackDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.PackDetailsTabControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackDetailsTabControl.Name = "PackDetailsTabControl";
			this.PackDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 413, true);
			this.PackDetailsTabControl.TabIndex = 0;
			// 
			// PackDetailsTabPage
			// 
			this.PackDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("374bb945-86a0-44a3-8aad-0084af0b16a6", "Pack Details");
			this.PackDetailsTabPage.Controls.Add(this.PackDetailsUserControl);
			this.PackDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.PackDetailsTabPage.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackDetailsTabPage.Name = "PackDetailsTabPage";
			this.PackDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.PackDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 391, true);
			this.PackDetailsTabPage.TabIndex = 0;
			this.PackDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// PackDetailsUserControl
			//
			this.BindingSource.SetBindingMember(this.PackDetailsUserControl, ".");
			this.PackDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PackDetailsUserControl.Name = "PackDetailsUserControl";
			// 
			// EUH7PackTabControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.PackDetailsTabControl);
			this.Name = "EUH7PackTabControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 413, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PackDetailsTabControl.ResumeLayout(false);
			this.PackDetailsTabControl.PerformLayout();
			this.PackDetailsTabPage.ResumeLayout(false);
			this.PackDetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private EUH7PackDetailsUserControl PackDetailsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl PackDetailsTabControl;
		private ZArchitecture.GUI.ZTabPage PackDetailsTabPage;
		private System.ComponentModel.IContainer components;
	}
}
