namespace Enterprise.Customs.DE.GUI
{
	partial class TemporaryStorageUserControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.JobDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CustomersReferenceTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CustomerOrganisation = new Enterprise.MasterFiles.GUI.ZOrganisationControl();
			this.PresenterOrgAddress = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.RepresentativeOrgAddress = new Enterprise.MasterFiles.GUI.ZOrgAddressControl();
			this.cusTempStorageDecsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JobDetailsPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SUMCustomsDetailsUserControl = new Enterprise.Customs.DE.GUI.SumACustomsDetailsUserControl();
			this.REXCustomsDetailsUserControl = new Enterprise.Customs.DE.GUI.ReExportCustomsDetailsUserControl();
			this.JobDetailsPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.JobDetailsPanel3 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.SUMTransportDetailsUserControl = new Enterprise.Customs.DE.GUI.SumATransportDetailsUserControl();
			this.REXTransportDetailsUserControl = new Enterprise.Customs.DE.GUI.ReExportTransportDetailsUserControl();
			this.OrganisationPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OrganisationPanel2 = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.JobDetailsGroupBox.SuspendLayout();
			this.CustomerOrganisation.SuspendLayout();
			this.PresenterOrgAddress.SuspendLayout();
			this.RepresentativeOrgAddress.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.cusTempStorageDecsGrid)).BeginInit();
			this.cusTempStorageDecsGrid.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.JobDetailsPanel1.SuspendLayout();
			this.SUMCustomsDetailsUserControl.SuspendLayout();
			this.REXCustomsDetailsUserControl.SuspendLayout();
			this.JobDetailsPanel2.SuspendLayout();
			this.JobDetailsPanel3.SuspendLayout();
			this.SUMTransportDetailsUserControl.SuspendLayout();
			this.REXTransportDetailsUserControl.SuspendLayout();
			this.OrganisationPanel1.SuspendLayout();
			this.OrganisationPanel2.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader);
			// 
			// NotificationProvider
			// 
			this.NotificationProvider.NotificationRenderer = null;
			// 
			// JobDetailsGroupBox
			// 
			this.JobDetailsGroupBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("81b3cd68-d2aa-4533-a5f7-1adf7a3f349e", "Job Details");
			this.JobDetailsGroupBox.Controls.Add(this.CustomersReferenceTextBox);
			this.JobDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.JobDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobDetailsGroupBox.Name = "JobDetailsGroupBox";
			this.JobDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 45, true);
			this.JobDetailsGroupBox.TabIndex = 4;
			this.JobDetailsGroupBox.TabStop = false;
			// 
			// CustomersReferenceTextBox
			// 
			this.BindingSource.SetBindingMember(this.CustomersReferenceTextBox, "SJH_ReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_ReferenceNumber)));
			this.CustomersReferenceTextBox.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("08be83e4-f18a-40e3-980f-726828b34261", "Customer Reference");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_OH_Customer)));
			this.CustomerOrganisation.BindToOrganisations = "Lookups.Customers";
			this.CustomerOrganisation.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("d6f5a8b6-f6f3-4ae4-b6b3-75a1259287c7", "Customer");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_OA_Presenter_ZAddress)));
			this.PresenterOrgAddress.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("3f388317-c42a-461a-98ef-fb4fd6c81564", "Presenter");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.ZArchitecture.Business.ZAddress)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).SJH_OA_Representative_ZAddress)));
			this.RepresentativeOrgAddress.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("3d4200bd-9139-40e8-8911-23b5c11c2b3f", "Representative");
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
			// cusTempStorageDecsGrid
			// 
			this.cusTempStorageDecsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.cusTempStorageDecsGrid, "CusTempStorageDecsWithValidData");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecsWithValidData)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecsWithValidData)).SyncRoot)).STH_DeclarationType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecsWithValidData)).SyncRoot)).ReferenceNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageDec)(((System.Collections.IList)(((Enterprise.Customs.DE.Business.CusTempStorage.CusTempStorageJobHeader)(null)).CusTempStorageDecsWithValidData)).SyncRoot)).STH_MessageStatus)));
			this.cusTempStorageDecsGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("ac6cedbb-69d4-4715-90cc-0e79811158b9", "Type");
			zTextBoxColumnStyleInfo1.ColumnName = "STH_DeclarationType";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("1969d591-ee88-49be-9fbe-9d44605c4b8f", "Reg No.");
			zTextBoxColumnStyleInfo2.ColumnName = "ReferenceNumber";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(175);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.DE.GUI.Res.GetData("8fc61def-f5f7-41a5-893b-6aa3e709b236", "Status");
			zTextBoxColumnStyleInfo3.ColumnName = "STH_MessageStatus";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.cusTempStorageDecsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.cusTempStorageDecsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.cusTempStorageDecsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.cusTempStorageDecsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cusTempStorageDecsGrid.GridId = "8d32b612-3745-42e7-a1da-dc79a23a5041";
			this.cusTempStorageDecsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.cusTempStorageDecsGrid.LayoutKey = "cusTempStorageDecsGrid";
			this.cusTempStorageDecsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 0, true);
			this.cusTempStorageDecsGrid.Name = "cusTempStorageDecsGrid";
			this.cusTempStorageDecsGrid.ReadOnly = true;
			this.cusTempStorageDecsGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.cusTempStorageDecsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(383, 265, true);
			this.cusTempStorageDecsGrid.TabIndex = 7;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.JobDetailsPanel1);
			this.MainPanel.Controls.Add(this.OrganisationPanel1);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 503, true);
			this.MainPanel.TabIndex = 102;
			// 
			// JobDetailsPanel1
			// 
			this.JobDetailsPanel1.Controls.Add(this.SUMCustomsDetailsUserControl);
			this.JobDetailsPanel1.Controls.Add(this.REXCustomsDetailsUserControl);
			this.JobDetailsPanel1.Controls.Add(this.JobDetailsPanel2);
			this.JobDetailsPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.JobDetailsPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(258, 0, true);
			this.JobDetailsPanel1.Name = "JobDetailsPanel1";
			this.JobDetailsPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 503, true);
			this.JobDetailsPanel1.TabIndex = 2;
			// 
			// SUMCustomsDetailsUserControl
			// 
			this.SUMCustomsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SUMCustomsDetailsUserControl, ".");
			this.SUMCustomsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SUMCustomsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 265, true);
			this.SUMCustomsDetailsUserControl.Name = "SUMCustomsDetailsUserControl";
			this.SUMCustomsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 238, true);
			this.SUMCustomsDetailsUserControl.TabIndex = 6;
			// 
			// REXCustomsDetailsUserControl
			// 
			this.REXCustomsDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REXCustomsDetailsUserControl, ".");
			this.REXCustomsDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.REXCustomsDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 265, true);
			this.REXCustomsDetailsUserControl.Name = "REXCustomsDetailsUserControl";
			this.REXCustomsDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 238, true);
			this.REXCustomsDetailsUserControl.TabIndex = 6;
			// 
			// JobDetailsPanel2
			// 
			this.JobDetailsPanel2.Controls.Add(this.cusTempStorageDecsGrid);
			this.JobDetailsPanel2.Controls.Add(this.JobDetailsPanel3);
			this.JobDetailsPanel2.Dock = System.Windows.Forms.DockStyle.Top;
			this.JobDetailsPanel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobDetailsPanel2.Name = "JobDetailsPanel2";
			this.JobDetailsPanel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 265, true);
			this.JobDetailsPanel2.TabIndex = 4;
			// 
			// JobDetailsPanel3
			// 
			this.JobDetailsPanel3.Controls.Add(this.SUMTransportDetailsUserControl);
			this.JobDetailsPanel3.Controls.Add(this.REXTransportDetailsUserControl);
			this.JobDetailsPanel3.Controls.Add(this.JobDetailsGroupBox);
			this.JobDetailsPanel3.Dock = System.Windows.Forms.DockStyle.Left;
			this.JobDetailsPanel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.JobDetailsPanel3.Name = "JobDetailsPanel3";
			this.JobDetailsPanel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 265, true);
			this.JobDetailsPanel3.TabIndex = 3;
			// 
			// SUMTransportDetailsUserControl
			// 
			this.SUMTransportDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.SUMTransportDetailsUserControl, ".");
			this.SUMTransportDetailsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.SUMTransportDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 45, true);
			this.SUMTransportDetailsUserControl.Name = "SUMTransportDetailsUserControl";
			this.SUMTransportDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 220, true);
			this.SUMTransportDetailsUserControl.TabIndex = 5;
			// 
			// REXTransportDetailsUserControl
			// 
			this.REXTransportDetailsUserControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.REXTransportDetailsUserControl, ".");
			this.REXTransportDetailsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 49, true);
			this.REXTransportDetailsUserControl.Name = "REXTransportDetailsUserControl";
			this.REXTransportDetailsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(397, 244, true);
			this.REXTransportDetailsUserControl.TabIndex = 5;
			// 
			// OrganisationPanel1
			// 
			this.OrganisationPanel1.Controls.Add(this.RepresentativeOrgAddress);
			this.OrganisationPanel1.Controls.Add(this.OrganisationPanel2);
			this.OrganisationPanel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.OrganisationPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OrganisationPanel1.Name = "OrganisationPanel1";
			this.OrganisationPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(258, 503, true);
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
			// TemporaryStorageUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainPanel);
			this.Name = "TemporaryStorageUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1038, 503, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.JobDetailsGroupBox.ResumeLayout(false);
			this.JobDetailsGroupBox.PerformLayout();
			this.CustomerOrganisation.ResumeLayout(true);
			this.CustomerOrganisation.PerformLayout();
			this.PresenterOrgAddress.ResumeLayout(true);
			this.PresenterOrgAddress.PerformLayout();
			this.RepresentativeOrgAddress.ResumeLayout(true);
			this.RepresentativeOrgAddress.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.cusTempStorageDecsGrid)).EndInit();
			this.cusTempStorageDecsGrid.ResumeLayout(false);
			this.cusTempStorageDecsGrid.PerformLayout();
			this.MainPanel.ResumeLayout(false);
			this.MainPanel.PerformLayout();
			this.JobDetailsPanel1.ResumeLayout(false);
			this.JobDetailsPanel1.PerformLayout();
			this.SUMCustomsDetailsUserControl.ResumeLayout(true);
			this.SUMCustomsDetailsUserControl.PerformLayout();
			this.REXCustomsDetailsUserControl.ResumeLayout(true);
			this.REXCustomsDetailsUserControl.PerformLayout();
			this.JobDetailsPanel2.ResumeLayout(false);
			this.JobDetailsPanel2.PerformLayout();
			this.JobDetailsPanel3.ResumeLayout(false);
			this.JobDetailsPanel3.PerformLayout();
			this.SUMTransportDetailsUserControl.ResumeLayout(true);
			this.SUMTransportDetailsUserControl.PerformLayout();
			this.REXTransportDetailsUserControl.ResumeLayout(true);
			this.REXTransportDetailsUserControl.PerformLayout();
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
		ZArchitecture.ZGrid cusTempStorageDecsGrid;
		MasterFiles.GUI.ZOrgAddressControl PresenterOrgAddress;
		private ZArchitecture.GUI.ZPanel MainPanel;
		private ZArchitecture.GUI.ZPanel OrganisationPanel2;
		private ZArchitecture.GUI.ZPanel OrganisationPanel1;
		private ZArchitecture.GUI.ZPanel JobDetailsPanel1;
		private ZArchitecture.GUI.ZPanel JobDetailsPanel3;
		private ZArchitecture.GUI.ZPanel JobDetailsPanel2;
		private SumATransportDetailsUserControl SUMTransportDetailsUserControl;
		private ReExportTransportDetailsUserControl REXTransportDetailsUserControl;
		private SumACustomsDetailsUserControl SUMCustomsDetailsUserControl;
		private ReExportCustomsDetailsUserControl REXCustomsDetailsUserControl;
	}
}
