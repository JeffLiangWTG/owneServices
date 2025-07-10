namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	partial class InstallationControl
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
			this.DateSiteLiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.InstallCompleteDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateEstimatedLiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AgreedLiveDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DateSiteLiveDateEdit.SuspendLayout();
			this.InstallCompleteDateEdit.SuspendLayout();
			this.DateEstimatedLiveDateEdit.SuspendLayout();
			this.AgreedLiveDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.LicenceHeader);
			// 
			// DateSiteLiveDateEdit
			// 
			this.DateSiteLiveDateEdit.AllowDrop = true;
			this.DateSiteLiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateSiteLiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateSiteLiveDateEdit, "LA_SiteLiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.DateSiteLiveDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|5bb1cdf1-b308-425b-bcbe-5955443f9de5", "Go-Live Complete");
			this.DateSiteLiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 90, true);
			this.DateSiteLiveDateEdit.Name = "DateSiteLiveDateEdit";
			this.DateSiteLiveDateEdit.TabIndex = 3;
			// 
			// InstallCompleteDateEdit
			// 
			this.InstallCompleteDateEdit.AllowDrop = true;
			this.InstallCompleteDateEdit.AutoCompleteMonthThreshold = 1;
			this.InstallCompleteDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.InstallCompleteDateEdit, "LA_InstallationCompleteDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.InstallCompleteDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|772cf03a-deb5-4f7b-af90-2bc390df0d66", "Install Complete");
			this.InstallCompleteDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 13, true);
			this.InstallCompleteDateEdit.Name = "InstallCompleteDateEdit";
			this.InstallCompleteDateEdit.TabIndex = 0;
			// 
			// DateEstimatedLiveDateEdit
			// 
			this.DateEstimatedLiveDateEdit.AllowDrop = true;
			this.DateEstimatedLiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateEstimatedLiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateEstimatedLiveDateEdit, "LA_EstimatedLiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.DateEstimatedLiveDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|865846cf-497b-434c-aa58-1cec1f52cf25", "Planned Go-Live");
			this.DateEstimatedLiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 65, true);
			this.DateEstimatedLiveDateEdit.Name = "DateEstimatedLiveDateEdit";
			this.DateEstimatedLiveDateEdit.TabIndex = 2;
			// 
			// AgreedLiveDateEdit
			// 
			this.AgreedLiveDateEdit.AllowDrop = true;
			this.AgreedLiveDateEdit.AutoCompleteMonthThreshold = 1;
			this.AgreedLiveDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AgreedLiveDateEdit, "LA_AgreedLiveDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			this.AgreedLiveDateEdit.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("LicenceKeyBuilderControl|bf8be94a-b11e-4ddb-b32d-2b8e61265f2b", "Agreed Go-Live");
			this.AgreedLiveDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(111, 39, true);
			this.AgreedLiveDateEdit.Name = "AgreedLiveDateEdit";
			this.AgreedLiveDateEdit.TabIndex = 1;
			// 
			// InstallationControl
			// 
			this.Controls.Add(this.DateSiteLiveDateEdit);
			this.Controls.Add(this.InstallCompleteDateEdit);
			this.Controls.Add(this.DateEstimatedLiveDateEdit);
			this.Controls.Add(this.AgreedLiveDateEdit);
			this.Name = "InstallationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 193, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DateSiteLiveDateEdit.ResumeLayout(true);
			this.DateSiteLiveDateEdit.PerformLayout();
			this.InstallCompleteDateEdit.ResumeLayout(true);
			this.InstallCompleteDateEdit.PerformLayout();
			this.DateEstimatedLiveDateEdit.ResumeLayout(true);
			this.DateEstimatedLiveDateEdit.PerformLayout();
			this.AgreedLiveDateEdit.ResumeLayout(true);
			this.AgreedLiveDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZDateEdit DateSiteLiveDateEdit;
		private ZArchitecture.GUI.ZDateEdit InstallCompleteDateEdit;
		private ZArchitecture.GUI.ZDateEdit DateEstimatedLiveDateEdit;
		private ZArchitecture.GUI.ZDateEdit AgreedLiveDateEdit;
	}
}
