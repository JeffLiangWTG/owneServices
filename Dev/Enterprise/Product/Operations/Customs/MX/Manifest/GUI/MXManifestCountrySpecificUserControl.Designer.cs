namespace Enterprise.Customs.MX.Manifest.GUI
{
	partial class MXManifestCountrySpecificUserControl
	{
		private void InitializeComponent()
		{
            this.LastForeignPortCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.LastForeignPortCodeFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.Customs.MX.Manifest.Business.AsycudaManifestHeader);
            // 
            // LastForeignPortCodeFindBox
            // 
            this.LastForeignPortCodeFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.LastForeignPortCodeFindBox, "LastForeignPort");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.MX.Manifest.Business.AsycudaManifestHeader)(null)).LastForeignPort)));
            this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.LastForeignPortCodeFindBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
            this.LastForeignPortCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.LastForeignPortCodeFindBox.Name = "LastForeignPortCodeFindBox";
            this.LastForeignPortCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(324, 20, true);
            this.LastForeignPortCodeFindBox.TabIndex = 1;
			// 
			// MXManifestCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.Controls.Add(this.LastForeignPortCodeFindBox);
            this.Name = "MXManifestCountrySpecificUserControl";
            this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(330, 27, true);
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.LastForeignPortCodeFindBox.ResumeLayout(true);
            this.LastForeignPortCodeFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		internal ZArchitecture.GUI.ZCodeFindBox LastForeignPortCodeFindBox;
	}
}
