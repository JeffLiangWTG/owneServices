namespace Enterprise.Client.EDI.Registry.GUI
{
	partial class InternalIncidentLicenceSettingsControl
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
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			this.InternalEnterpriseCodeGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LicencedOrganisationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.EdiProdLicenceGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.LicenceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.UatGpcLicenceGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.UatGprLicenceGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.UatStdLicenceGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.UatDprLicenceGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.UatAlpLicenceGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.InternalEnterpriseCodeGrid)).BeginInit();
			this.InternalEnterpriseCodeGrid.SuspendLayout();
			this.LicencedOrganisationGroupBox.SuspendLayout();
			this.EdiProdLicenceGuidFindBox.SuspendLayout();
			this.LicenceGroupBox.SuspendLayout();
			this.UatGpcLicenceGuidFindBox.SuspendLayout();
			this.UatGprLicenceGuidFindBox.SuspendLayout();
			this.UatStdLicenceGuidFindBox.SuspendLayout();
			this.UatDprLicenceGuidFindBox.SuspendLayout();
			this.UatAlpLicenceGuidFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings);
			// 
			// InternalEnterpriseCodeGrid
			// 
			this.InternalEnterpriseCodeGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.InternalEnterpriseCodeGrid, "LicenceEnterpriseKeys");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).LicenceEnterpriseKeys)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.LicenceEnterpriseKey)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).LicenceEnterpriseKeys)).SyncRoot)).LE_PK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.LicenceEnterpriseKey)(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).LicenceEnterpriseKeys)).SyncRoot)).Lookups.LicenceEnterpriseKeyList)));
			this.InternalEnterpriseCodeGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups+LicenceEnterpriseKeyList";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "LE_PK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.InternalEnterpriseCodeGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.InternalEnterpriseCodeGrid.CopySelectedRowsAllowed = true;
			this.InternalEnterpriseCodeGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.InternalEnterpriseCodeGrid.GridId = "b4ad50c5-7329-4772-bcd3-a88503380e28";
			this.InternalEnterpriseCodeGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.InternalEnterpriseCodeGrid.LayoutKey = "InternalEnterpriseCodeGrid";
			this.InternalEnterpriseCodeGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.InternalEnterpriseCodeGrid.Name = "InternalEnterpriseCodeGrid";
			this.InternalEnterpriseCodeGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 115, true);
			this.InternalEnterpriseCodeGrid.TabIndex = 0;
			// 
			// LicencedOrganisationGroupBox
			// 
			this.LicencedOrganisationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LicencedOrganisationGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("InternalIncidentLicenceSettingsControl|c1a238cc-c911-49e6-85b0-c73caecc5cf0", "Enterprise Code");
			this.LicencedOrganisationGroupBox.Controls.Add(this.InternalEnterpriseCodeGrid);
			this.LicencedOrganisationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
			this.LicencedOrganisationGroupBox.Name = "LicencedOrganisationGroupBox";
			this.LicencedOrganisationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(393, 131, true);
			this.LicencedOrganisationGroupBox.TabIndex = 1;
			this.LicencedOrganisationGroupBox.TabStop = false;
			// 
			// EdiProdLicenceGuidFindBox
			// 
			this.EdiProdLicenceGuidFindBox.AllowDrop = true;
			this.EdiProdLicenceGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.EdiProdLicenceGuidFindBox, "EdiProd_LicencePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).EdiProd_LicencePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).Lookups.LicenceList)));
			this.EdiProdLicenceGuidFindBox.BindToList = "Lookups.LicenceList";
			this.EdiProdLicenceGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("InternalIncidentLicenceSettingsControl|96bcfae5-4eb0-4245-a0e5-159c4817c744", "ediProd License");
			this.EdiProdLicenceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 19, true);
			this.EdiProdLicenceGuidFindBox.Name = "EdiProdLicenceGuidFindBox";
			this.EdiProdLicenceGuidFindBox.PreBoundMaxLength = 9;
			this.EdiProdLicenceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 17, true);
			this.EdiProdLicenceGuidFindBox.TabIndex = 2;
			// 
			// LicenceGroupBox
			// 
			this.LicenceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.LicenceGroupBox.CaptionResourceString = ZClientEDI.Res.GetData("InternalIncidentLicenceSettingsControl|a7360191-e619-4004-8f19-5cf9eda125d7", "Default Licenses");
			this.LicenceGroupBox.Controls.Add(this.UatGpcLicenceGuidFindBox);
			this.LicenceGroupBox.Controls.Add(this.UatGprLicenceGuidFindBox);
			this.LicenceGroupBox.Controls.Add(this.UatStdLicenceGuidFindBox);
			this.LicenceGroupBox.Controls.Add(this.UatDprLicenceGuidFindBox);
			this.LicenceGroupBox.Controls.Add(this.UatAlpLicenceGuidFindBox);
			this.LicenceGroupBox.Controls.Add(this.EdiProdLicenceGuidFindBox);
			this.LicenceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 143, true);
			this.LicenceGroupBox.Name = "LicenceGroupBox";
			this.LicenceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(387, 178, true);
			this.LicenceGroupBox.TabIndex = 3;
			this.LicenceGroupBox.TabStop = false;
			// 
			// UatGpcLicenceGuidFindBox
			// 
			this.UatGpcLicenceGuidFindBox.AllowDrop = true;
			this.UatGpcLicenceGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UatGpcLicenceGuidFindBox, "UAT_GPC_LicencePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).UAT_GPC_LicencePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).Lookups.LicenceList)));
			this.UatGpcLicenceGuidFindBox.BindToList = "Lookups.LicenceList";
			this.UatGpcLicenceGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("InternalIncidentLicenceSettingsControl|f1dc897f-2928-4e74-bdcd-52f0fe4aaa84", "UAT GPC License");
			this.UatGpcLicenceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 123, true);
			this.UatGpcLicenceGuidFindBox.Name = "UatGpcLicenceGuidFindBox";
			this.UatGpcLicenceGuidFindBox.PreBoundMaxLength = 9;
			this.UatGpcLicenceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 17, true);
			this.UatGpcLicenceGuidFindBox.TabIndex = 6;
			// 
			// UatGprLicenceGuidFindBox
			// 
			this.UatGprLicenceGuidFindBox.AllowDrop = true;
			this.UatGprLicenceGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UatGprLicenceGuidFindBox, "UAT_GPR_LicencePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).UAT_GPR_LicencePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).Lookups.LicenceList)));
			this.UatGprLicenceGuidFindBox.BindToList = "Lookups.LicenceList";
			this.UatGprLicenceGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("InternalIncidentLicenceSettingsControl|4b420cc1-fb3b-4bdc-be7b-a63b04c1f198", "UAT GPR License");
			this.UatGprLicenceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 149, true);
			this.UatGprLicenceGuidFindBox.Name = "UatGprLicenceGuidFindBox";
			this.UatGprLicenceGuidFindBox.PreBoundMaxLength = 9;
			this.UatGprLicenceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 17, true);
			this.UatGprLicenceGuidFindBox.TabIndex = 7;
			// 
			// UatStdLicenceGuidFindBox
			// 
			this.UatStdLicenceGuidFindBox.AllowDrop = true;
			this.UatStdLicenceGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UatStdLicenceGuidFindBox, "UAT_STD_LicencePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).UAT_STD_LicencePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).Lookups.LicenceList)));
			this.UatStdLicenceGuidFindBox.BindToList = "Lookups.LicenceList";
			this.UatStdLicenceGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("InternalIncidentLicenceSettingsControl|d577f554-6916-4baa-9290-c25d8aec228e", "UAT STD License");
			this.UatStdLicenceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 97, true);
			this.UatStdLicenceGuidFindBox.Name = "UatStdLicenceGuidFindBox";
			this.UatStdLicenceGuidFindBox.PreBoundMaxLength = 9;
			this.UatStdLicenceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 17, true);
			this.UatStdLicenceGuidFindBox.TabIndex = 5;
			// 
			// UatDprLicenceGuidFindBox
			// 
			this.UatDprLicenceGuidFindBox.AllowDrop = true;
			this.UatDprLicenceGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UatDprLicenceGuidFindBox, "UAT_DPR_LicencePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).UAT_DPR_LicencePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).Lookups.LicenceList)));
			this.UatDprLicenceGuidFindBox.BindToList = "Lookups.LicenceList";
			this.UatDprLicenceGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("InternalIncidentLicenceSettingsControl|a1fc8245-9387-40d4-b7dc-7d9cb916108f", "UAT DPR License");
			this.UatDprLicenceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 71, true);
			this.UatDprLicenceGuidFindBox.Name = "UatDprLicenceGuidFindBox";
			this.UatDprLicenceGuidFindBox.PreBoundMaxLength = 9;
			this.UatDprLicenceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 17, true);
			this.UatDprLicenceGuidFindBox.TabIndex = 4;
			// 
			// UatAlpLicenceGuidFindBox
			// 
			this.UatAlpLicenceGuidFindBox.AllowDrop = true;
			this.UatAlpLicenceGuidFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.UatAlpLicenceGuidFindBox, "UAT_ALP_LicencePK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).UAT_ALP_LicencePK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.Registry.Business.InternalIncidentLicenceSettings)(null)).Lookups.LicenceList)));
			this.UatAlpLicenceGuidFindBox.BindToList = "Lookups.LicenceList";
			this.UatAlpLicenceGuidFindBox.CaptionResourceString = ZClientEDI.Res.GetData("InternalIncidentLicenceSettingsControl|66435624-ecfe-48bd-941a-3e8ef5ce5a4e", "UAT ALP License");
			this.UatAlpLicenceGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 45, true);
			this.UatAlpLicenceGuidFindBox.Name = "UatAlpLicenceGuidFindBox";
			this.UatAlpLicenceGuidFindBox.PreBoundMaxLength = 9;
			this.UatAlpLicenceGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(276, 17, true);
			this.UatAlpLicenceGuidFindBox.TabIndex = 3;
			// 
			// InternalIncidentLicenceSettingsControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.LicenceGroupBox);
			this.Controls.Add(this.LicencedOrganisationGroupBox);
			this.Name = "InternalIncidentLicenceSettingsControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(425, 330, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.InternalEnterpriseCodeGrid)).EndInit();
			this.InternalEnterpriseCodeGrid.ResumeLayout(false);
			this.InternalEnterpriseCodeGrid.PerformLayout();
			this.LicencedOrganisationGroupBox.ResumeLayout(false);
			this.LicencedOrganisationGroupBox.PerformLayout();
			this.EdiProdLicenceGuidFindBox.ResumeLayout(true);
			this.EdiProdLicenceGuidFindBox.PerformLayout();
			this.LicenceGroupBox.ResumeLayout(false);
			this.LicenceGroupBox.PerformLayout();
			this.UatGpcLicenceGuidFindBox.ResumeLayout(true);
			this.UatGpcLicenceGuidFindBox.PerformLayout();
			this.UatGprLicenceGuidFindBox.ResumeLayout(true);
			this.UatGprLicenceGuidFindBox.PerformLayout();
			this.UatStdLicenceGuidFindBox.ResumeLayout(true);
			this.UatStdLicenceGuidFindBox.PerformLayout();
			this.UatDprLicenceGuidFindBox.ResumeLayout(true);
			this.UatDprLicenceGuidFindBox.PerformLayout();
			this.UatAlpLicenceGuidFindBox.ResumeLayout(true);
			this.UatAlpLicenceGuidFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.ZGrid InternalEnterpriseCodeGrid;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LicencedOrganisationGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox EdiProdLicenceGuidFindBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox LicenceGroupBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox UatGprLicenceGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox UatStdLicenceGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox UatDprLicenceGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox UatAlpLicenceGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox UatGpcLicenceGuidFindBox;
	}
}
