namespace Enterprise.Customs.EU.Manifest.GUI
{
	partial class EUCountryCustomsOfficesUserControl
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
			this.OfficesGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).BeginInit();
			this.CustomsOfficesGrid.SuspendLayout();
			this.CustomsOfficeFindBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// OfficesGroupBox
			// 
			this.OfficesGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OfficesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OfficesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 153, true);
			// 
			// CustomsOfficesGrid
			// 
			this.CustomsOfficesGrid.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
			| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.CustomsOfficesGrid, "EUCustomsOffices");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.Business.AsycudaManifestHeader)(null)).EUCustomsOffices)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.EuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.Business.AsycudaManifestHeader)(null)).EUCustomsOffices)).SyncRoot)).CY_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.EuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.Business.AsycudaManifestHeader)(null)).EUCustomsOffices)).SyncRoot)).CY_Data)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.EuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.Business.AsycudaManifestHeader)(null)).EUCustomsOffices)).SyncRoot)).CY_OfficeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Customs.EU.Business.EuOfficeCode)(((System.Collections.IList)(((Enterprise.Customs.EU.Manifest.Business.AsycudaManifestHeader)(null)).EUCustomsOffices)).SyncRoot)).CY_Date)));
			this.CustomsOfficesGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.CustomsOfficesGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(553, 128, true);
			// 
			// CustomsOfficeFindBox
			// 
			this.BindingSource.SetBindingMember(this.CustomsOfficeFindBox, "AMA_CustomsOffice");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Manifest.Business.AsycudaManifestHeader)(null)).AMA_CustomsOffice)));
			this.CustomsOfficeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(117, 21, true);
			this.CustomsOfficeFindBox.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Manifest.Business.AsycudaManifestHeader);
			// 
			// CountryCustomsOfficesUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Name = "CountryCustomsOfficesUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 153, true);
			this.OfficesGroupBox.ResumeLayout(false);
			this.OfficesGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.CustomsOfficesGrid)).EndInit();
			this.CustomsOfficesGrid.ResumeLayout(false);
			this.CustomsOfficesGrid.PerformLayout();
			this.CustomsOfficeFindBox.ResumeLayout(true);
			this.CustomsOfficeFindBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
