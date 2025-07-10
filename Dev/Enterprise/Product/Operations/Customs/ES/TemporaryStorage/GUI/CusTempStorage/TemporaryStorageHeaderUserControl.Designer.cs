namespace Enterprise.Customs.ES.TemporaryStorage.GUI
{
	partial class TemporaryStorageHeaderUserControl
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
			this.JobDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomersReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomerOrganisation = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.PresenterOrgAddress = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.RepresentativeOrgAddress = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JobDetailsPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.CustomsDetailsUserControl = new Enterprise.Customs.ES.TemporaryStorage.GUI.CustomsDetailsUserControl();
			this.JobDetailsPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JobDetailsPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.TransportDetailsUserControl = new Enterprise.Customs.ES.TemporaryStorage.GUI.TransportDetailsUserControl();
			this.OrganisationPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrganisationPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JobDetailsGroupBox.SuspendLayout();
			this.CustomerOrganisation.SuspendLayout();
			this.PresenterOrgAddress.SuspendLayout();
			this.RepresentativeOrgAddress.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.JobDetailsPanel1.SuspendLayout();
			this.CustomsDetailsUserControl.SuspendLayout();
			this.JobDetailsPanel2.SuspendLayout();
			this.JobDetailsPanel3.SuspendLayout();
			this.TransportDetailsUserControl.SuspendLayout();
			this.OrganisationPanel1.SuspendLayout();
			this.OrganisationPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// NotificationProvider
			// 
			this.NotificationProvider.NotificationRenderer = null;
			// 
			// JobDetailsGroupBox
			// 
			this.JobDetailsGroupBox.Controls.Add(this.CustomersReferenceTextBox);
			this.JobDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.JobDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobDetailsGroupBox.Name = "JobDetailsGroupBox";
			this.JobDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 45, true);
			this.JobDetailsGroupBox.TabIndex = 4;
			this.JobDetailsGroupBox.TabStop = false;
			this.JobDetailsGroupBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("6FD3BD23-4054-47A5-B7CD-94629EBF763A", "Job Details");
			// 
			// CustomersReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomersReferenceTextBox, "SJH_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_ReferenceNumber)));
			this.CustomersReferenceTextBox.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("835F6C10-7184-4A8A-A0EB-C125694B5367", "Customer Reference");
			this.CustomersReferenceTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 17, true);
			this.CustomersReferenceTextBox.Name = "CustomersReferenceTextBox";
			this.CustomersReferenceTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(261, 20, true);
			this.CustomersReferenceTextBox.TabIndex = 4;
			// 
			// CustomerOrganisation
			// 
			this.CustomerOrganisation.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomerOrganisation, "SJH_OH_Customer");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_OH_Customer)));
			this.CustomerOrganisation.BindToOrganisations = "Lookups.Customers";
			this.CustomerOrganisation.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("40A6F47D-86EE-4B51-80CE-CF84C8767A08", "Customer");
			this.CustomerOrganisation.Captions = new string[] {
		"Customer"};
			this.CustomerOrganisation.Dock = System.Windows.Forms.DockStyle.Top;
			this.CustomerOrganisation.IsCaptionOverridden = false;
			this.CustomerOrganisation.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.CustomerOrganisation.Name = "CustomerOrganisation";
			this.CustomerOrganisation.PopupCaption = "";
			this.CustomerOrganisation.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 152, true);
			this.CustomerOrganisation.TabIndex = 1;
			// 
			// PresenterOrgAddress
			// 
			this.PresenterOrgAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PresenterOrgAddress, "SJH_OA_Presenter_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_OA_Presenter_ZAddress)));
			this.PresenterOrgAddress.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("92727C90-8274-4ED2-BEE7-5129BBEF49B3", "Presenter");
			this.PresenterOrgAddress.Dock = System.Windows.Forms.DockStyle.Fill;
			this.PresenterOrgAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.PresenterOrgAddress.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.PresenterOrgAddress.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.PresenterOrgAddress.Name = "PresenterOrgAddress";
			this.PresenterOrgAddress.OnlyStopOnDebtor = false;
			this.PresenterOrgAddress.PopupCaption = "";
			this.PresenterOrgAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.PresenterOrgAddress.TabIndex = 2;
			// 
			// RepresentativeOrgAddress
			// 
			this.RepresentativeOrgAddress.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RepresentativeOrgAddress, "SJH_OA_Representative_ZAddress");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.ES.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_OA_Representative_ZAddress)));
			this.RepresentativeOrgAddress.CaptionResourceString = Enterprise.Customs.ES.TemporaryStorage.GUI.Res.GetData("3A79D6BB-A562-42DB-8F42-923A4D2DA289", "Representative");
			this.RepresentativeOrgAddress.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RepresentativeOrgAddress.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 306, true);
			this.RepresentativeOrgAddress.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.RepresentativeOrgAddress.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.RepresentativeOrgAddress.Name = "RepresentativeOrgAddress";
			this.RepresentativeOrgAddress.OnlyStopOnDebtor = false;
			this.RepresentativeOrgAddress.PopupCaption = "";
			this.RepresentativeOrgAddress.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(250, 152, true);
			this.RepresentativeOrgAddress.TabIndex = 3;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.JobDetailsPanel1);
			this.MainPanel.Controls.Add(this.OrganisationPanel1);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 529, true);
			this.MainPanel.TabIndex = 102;
			// 
			// JobDetailsPanel1
			// 
			this.JobDetailsPanel1.Controls.Add(this.CustomsDetailsUserControl);
			this.JobDetailsPanel1.Controls.Add(this.JobDetailsPanel2);
			this.JobDetailsPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobDetailsPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 0, true);
			this.JobDetailsPanel1.Name = "JobDetailsPanel1";
			this.JobDetailsPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 529, true);
			this.JobDetailsPanel1.TabIndex = 2;
			// 
			// CustomsDetailsUserControl
			// 
			this.CustomsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CustomsDetailsUserControl, ".");
			this.CustomsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.CustomsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 293, true);
			this.CustomsDetailsUserControl.Name = "CustomsDetailsUserControl";
			this.CustomsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 236, true);
			this.CustomsDetailsUserControl.TabIndex = 6;
			// 
			// JobDetailsPanel2
			// 
			this.JobDetailsPanel2.Controls.Add(this.JobDetailsPanel3);
			this.JobDetailsPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.JobDetailsPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobDetailsPanel2.Name = "JobDetailsPanel2";
			this.JobDetailsPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(410, 293, true);
			this.JobDetailsPanel2.TabIndex = 4;
			// 
			// JobDetailsPanel3
			// 
			this.JobDetailsPanel3.Controls.Add(this.TransportDetailsUserControl);
			this.JobDetailsPanel3.Controls.Add(this.JobDetailsGroupBox);
			this.JobDetailsPanel3.Dock = System.Windows.Forms.DockStyle.Left;
			this.JobDetailsPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobDetailsPanel3.Name = "JobDetailsPanel3";
			this.JobDetailsPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 293, true);
			this.JobDetailsPanel3.TabIndex = 3;
			// 
			// TransportDetailsUserControl
			// 
			this.TransportDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TransportDetailsUserControl, ".");
			this.TransportDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Left;
			this.TransportDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 45, true);
			this.TransportDetailsUserControl.Name = "TransportDetailsUserControl";
			this.TransportDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 248, true);
			this.TransportDetailsUserControl.TabIndex = 5;
			// 
			// OrganisationPanel1
			// 
			this.OrganisationPanel1.Controls.Add(this.RepresentativeOrgAddress);
			this.OrganisationPanel1.Controls.Add(this.OrganisationPanel2);
			this.OrganisationPanel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.OrganisationPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationPanel1.Name = "OrganisationPanel1";
			this.OrganisationPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 529, true);
			this.OrganisationPanel1.TabIndex = 1;
			// 
			// OrganisationPanel2
			// 
			this.OrganisationPanel2.Controls.Add(this.PresenterOrgAddress);
			this.OrganisationPanel2.Controls.Add(this.CustomerOrganisation);
			this.OrganisationPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.OrganisationPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationPanel2.Name = "OrganisationPanel2";
			this.OrganisationPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 306, true);
			this.OrganisationPanel2.TabIndex = 0;
			// 
			// TemporaryStorageHeaderUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "TemporaryStorageHeaderUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(668, 529, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JobDetailsGroupBox.ResumeLayout(false);
			this.JobDetailsGroupBox.PerformLayout();
			this.CustomerOrganisation.ResumeLayout(true);
			this.CustomerOrganisation.PerformLayout();
			this.PresenterOrgAddress.ResumeLayout(true);
			this.PresenterOrgAddress.PerformLayout();
			this.RepresentativeOrgAddress.ResumeLayout(true);
			this.RepresentativeOrgAddress.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.JobDetailsPanel1.ResumeLayout(false);
			this.JobDetailsPanel1.PerformLayout();
			this.CustomsDetailsUserControl.ResumeLayout(true);
			this.CustomsDetailsUserControl.PerformLayout();
			this.JobDetailsPanel2.ResumeLayout(false);
			this.JobDetailsPanel2.PerformLayout();
			this.JobDetailsPanel3.ResumeLayout(false);
			this.JobDetailsPanel3.PerformLayout();
			this.TransportDetailsUserControl.ResumeLayout(true);
			this.TransportDetailsUserControl.PerformLayout();
			this.OrganisationPanel1.ResumeLayout(false);
			this.OrganisationPanel1.PerformLayout();
			this.OrganisationPanel2.ResumeLayout(false);
			this.OrganisationPanel2.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZGroupBox JobDetailsGroupBox;
		private ZArchitecture.ZTextBox CustomersReferenceTextBox;
		private MasterFiles.GUI.ZOrganisationControl CustomerOrganisation;
		private MasterFiles.GUI.ZOrgAddressControl RepresentativeOrgAddress;
		private MasterFiles.GUI.ZOrgAddressControl PresenterOrgAddress;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZPanel OrganisationPanel2;
		private ZArchitecture.GUI.ZPanel OrganisationPanel1;
		protected ZArchitecture.GUI.ZPanel JobDetailsPanel1;
		protected ZArchitecture.GUI.ZPanel JobDetailsPanel3;
		private ZArchitecture.GUI.ZPanel JobDetailsPanel2;
		protected TransportDetailsUserControl TransportDetailsUserControl;
		protected CustomsDetailsUserControl CustomsDetailsUserControl;
	}
}
