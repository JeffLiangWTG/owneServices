namespace Enterprise.Customs.EU.GUI
{
	public partial class AdditionalSupplementaryCodesAndGDMUserControl
	{
		private void InitializeComponent()
		{
			this.AdditionalSupplementaryCodesUserControl = new Enterprise.Customs.EU.GUI.AdditionalSupplementaryCodesUserControl();
			this.GDMLink = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.AdditionalSupplementaryCodesUserControl.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceLine);
			// 
			// AdditionalSupplementaryCodesUserControl
			// 
			this.AdditionalSupplementaryCodesUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.AdditionalSupplementaryCodesUserControl, ".");
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.AdditionalSupplementaryCodesUserControl, false);
			this.AdditionalSupplementaryCodesUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AdditionalSupplementaryCodesUserControl.Name = "AdditionalSupplementaryCodesUserControl";
			this.AdditionalSupplementaryCodesUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(233, 20, true);
			this.AdditionalSupplementaryCodesUserControl.TabIndex = 0;
			// 
			// GDMLink
			// 
			this.GDMLink.AutoSize = true;
			this.GDMLink.IsFontBold = false;
			this.GDMLink.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(233, 3, true);
			this.GDMLink.Name = "GDMLink";
			this.GDMLink.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 13, true);
			this.GDMLink.TabIndex = 1;
			this.GDMLink.Text = Enterprise.Customs.EU.GUI.Res.GetString("396d2736-fcbd-4d32-bcf4-54d0f630daf2", "GDM");
			this.GDMLink.Click += new System.EventHandler(this.GDMLink_Clicked);
			// 
			// AdditionalSupplementaryCodesAndGDMUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AdditionalSupplementaryCodesUserControl);
			this.Controls.Add(this.GDMLink);
			this.Name = "AdditionalSupplementaryCodesAndGDMUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 20, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.AdditionalSupplementaryCodesUserControl.ResumeLayout(true);
			this.AdditionalSupplementaryCodesUserControl.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		AdditionalSupplementaryCodesUserControl AdditionalSupplementaryCodesUserControl;
		Enterprise.ZArchitecture.GUI.ZLinkLabel GDMLink;
	}
}
