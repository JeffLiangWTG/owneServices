using Enterprise.ZArchitecture;

namespace Enterprise.Customs.EU.H7.GUI
{
	partial class EUH7ItemTabControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.ItemDetailsUserControl = new EUH7ItemDetailsUserControl();
			this.ItemDetailsTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.ItemDetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ItemDetailsTabControl.SuspendLayout();
			this.ItemDetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ASYCUDA.Business.AsycudaPackedItem);
			// 
			// ItemDetailsTabControl
			// 
			this.ItemDetailsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.ItemDetailsTabControl.Controls.Add(this.ItemDetailsTabPage);
			this.ItemDetailsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemDetailsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ItemDetailsTabControl.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ItemDetailsTabControl.Name = "ItemDetailsTabControl";
			this.ItemDetailsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 413, true);
			this.ItemDetailsTabControl.TabIndex = 0;
			// 
			// ItemDetailsTabPage
			// 
			this.ItemDetailsTabPage.CaptionResourceString = Enterprise.Customs.EU.H7.GUI.Res.GetData("9edb9432-a6fa-4f4e-8258-9dc0e6b38568", "Packed Item Details");
			this.ItemDetailsTabPage.Controls.Add(this.ItemDetailsUserControl);
			this.ItemDetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 19, true);
			this.ItemDetailsTabPage.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ItemDetailsTabPage.Name = "ItemDetailsTabPage";
			this.ItemDetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.ItemDetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1009, 391, true);
			this.ItemDetailsTabPage.TabIndex = 0;
			this.ItemDetailsTabPage.UseVisualStyleBackColor = true;
			// 
			// ItemDetailsUserControl
			//
			this.BindingSource.SetBindingMember(this.ItemDetailsUserControl, ".");
			this.ItemDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ItemDetailsUserControl.Name = "ItemDetailsUserControl";
			// 
			// EUH7ItemTabControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoScroll = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.ItemDetailsTabControl);
			this.Name = "EUH7ItemTabControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1013, 413, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ItemDetailsTabControl.ResumeLayout(false);
			this.ItemDetailsTabControl.PerformLayout();
			this.ItemDetailsTabPage.ResumeLayout(false);
			this.ItemDetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private EUH7ItemDetailsUserControl ItemDetailsUserControl;
		private Enterprise.ZArchitecture.GUI.ZTemplateTabControl ItemDetailsTabControl;
		private ZArchitecture.GUI.ZTabPage ItemDetailsTabPage;
		private System.ComponentModel.IContainer components;
	}
}
