namespace Enterprise.Customs.BR.Manifest.GUI
{
	partial class BRManifestCountrySpecificUserControl
	{
		void InitializeComponent()
		{
			this.CustomsOwnNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.BR.Manifest.Business.AsycudaManifestHeader);
			// 
			// MasterUCRTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsOwnNumberTextBox, "CustomsOwnNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.BR.Manifest.Business.AsycudaManifestHeader)(null)).CustomsOwnNumber)));
			this.CustomsOwnNumberTextBox.CaptionResourceString = null;
			this.CustomsOwnNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(109, 0, true);
			this.CustomsOwnNumberTextBox.Name = "CustomsOwnNumberTextBox";
			this.CustomsOwnNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(262, 20, true);
			this.CustomsOwnNumberTextBox.TabIndex = 0;
			// 
			// BRManifestCountrySpecificUserControl
			// 
			this.Controls.Add(this.CustomsOwnNumberTextBox);
			this.Name = "BRManifestCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(957, 270, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		internal Enterprise.ZArchitecture.ZTextBox CustomsOwnNumberTextBox;		
	}
}
