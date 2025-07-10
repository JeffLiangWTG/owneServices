namespace Enterprise.Client.EDI.UserManagement.GUI
{
	partial class EdiAgreementAssignmentUserControl
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
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.corporateAgreementsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.corporateAgreementsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.kSplitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.addAcceptanceButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.agreementAcceptanceGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.agreementAcceptanceGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.corporateAgreementsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.corporateAgreementsGrid)).BeginInit();
			this.corporateAgreementsGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).BeginInit();
			this.kSplitContainer1.Panel1.SuspendLayout();
			this.kSplitContainer1.Panel2.SuspendLayout();
			this.kSplitContainer1.SuspendLayout();
			this.agreementAcceptanceGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.agreementAcceptanceGrid)).BeginInit();
			this.agreementAcceptanceGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignmentCollection);
			// 
			// corporateAgreementsGroupBox
			// 
			this.corporateAgreementsGroupBox.Controls.Add(this.corporateAgreementsGrid);
			this.corporateAgreementsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.corporateAgreementsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.corporateAgreementsGroupBox.Name = "corporateAgreementsGroupBox";
			this.corporateAgreementsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 157, true);
			this.corporateAgreementsGroupBox.TabIndex = 0;
			this.corporateAgreementsGroupBox.TabStop = false;
			this.corporateAgreementsGroupBox.Text = "Corporate Agreements";
			// 
			// corporateAgreementsGrid
			// 
			this.corporateAgreementsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.corporateAgreementsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).EAE_AgreementType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).EAE_AgreementTypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).EAE_VariantCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).EAE_VariantCodeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).EAE_AllowOnlineAcceptance)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).CurrentVersion)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).LastAcceptedDateUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).ClientAgreementOrgPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).EAE_ParentTableCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).EAE_ParentID)));
			this.corporateAgreementsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "EAE_AgreementType";
			zDropEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "EAE_AgreementTypeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.ColumnName = "EAE_VariantCode";
			zDropEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "EAE_VariantCodeDescription";
			zTextBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "EAE_AllowOnlineAcceptance";
			zCheckBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "CurrentVersion";
			zTextBoxColumnStyleInfo3.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "LastAcceptedDateUtc";
			zDateEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ClientAgreementOrgPK";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zGuidFindBoxColumnStyleInfo1.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation;
			zDropEditColumnStyleInfo3.ColumnName = "ParentTableCodeDescription";
			zDropEditColumnStyleInfo3.DefaultCollectionIndex = 0;
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDropEditColumnStyleInfo3.ShowInDropDown = ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "EAE_ParentID";
			zGuidFindBoxColumnStyleInfo2.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zGuidFindBoxColumnStyleInfo2.ModuleID = Enterprise.Client.EDI.Modules.ClientModuleRegistration.LicenceDatabase;
			this.corporateAgreementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.corporateAgreementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.corporateAgreementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.corporateAgreementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.corporateAgreementsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.corporateAgreementsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.corporateAgreementsGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.corporateAgreementsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.corporateAgreementsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.corporateAgreementsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.corporateAgreementsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.corporateAgreementsGrid.GridId = "f0ef0252-c4ee-49bf-99bb-8697ccd3a380";
			this.corporateAgreementsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.corporateAgreementsGrid.LayoutKey = "corporateAgreementsGrid";
			this.corporateAgreementsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.corporateAgreementsGrid.Name = "corporateAgreementsGrid";
			this.corporateAgreementsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 140, true);
			this.corporateAgreementsGrid.TabIndex = 0;
			// 
			// kSplitContainer1
			// 
			this.kSplitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.kSplitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.kSplitContainer1.Name = "kSplitContainer1";
			this.kSplitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// kSplitContainer1.Panel1
			// 
			this.kSplitContainer1.Panel1.Controls.Add(this.corporateAgreementsGroupBox);
			// 
			// kSplitContainer1.Panel2
			// 
			this.kSplitContainer1.Panel2.Controls.Add(this.addAcceptanceButton);
			this.kSplitContainer1.Panel2.Controls.Add(this.agreementAcceptanceGroupBox);
			this.kSplitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 518, true);
			this.kSplitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(157);
			this.kSplitContainer1.SplitterWidth = 8;
			this.kSplitContainer1.TabIndex = 1;
			// 
			// addAcceptanceButton
			// 
			this.addAcceptanceButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.addAcceptanceButton.CaptionResourceString = ZClientEDI.Res.GetData("c0b03169-4619-4a0d-9448-ee395a0dee49", "Add acceptance");
			this.addAcceptanceButton.IsCaptionOverridden = true;
			this.addAcceptanceButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(750, 236, true);
			this.addAcceptanceButton.Name = "addAcceptanceButton";
			this.addAcceptanceButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 37, true);
			this.addAcceptanceButton.TabIndex = 1;
			this.addAcceptanceButton.Text = "Add acceptance";
			this.addAcceptanceButton.ToolTipCaption = null;
			this.addAcceptanceButton.UseVisualStyleBackColor = true;
			this.addAcceptanceButton.Click += new System.EventHandler(this.AddAcceptanceButton_Click);
			// 
			// agreementAcceptanceGroupBox
			// 
			this.agreementAcceptanceGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.agreementAcceptanceGroupBox.Controls.Add(this.agreementAcceptanceGrid);
			this.agreementAcceptanceGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.agreementAcceptanceGroupBox.Name = "agreementAcceptanceGroupBox";
			this.agreementAcceptanceGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(859, 230, true);
			this.agreementAcceptanceGroupBox.TabIndex = 0;
			this.agreementAcceptanceGroupBox.TabStop = false;
			this.agreementAcceptanceGroupBox.Text = "Agreement Acceptance";
			// 
			// agreementAcceptanceGrid
			// 
			this.agreementAcceptanceGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.agreementAcceptanceGrid, "AcceptanceLogs");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).AcceptanceLogs)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).AcceptanceLogs)).SyncRoot)).EUL_AcceptedByName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).AcceptanceLogs)).SyncRoot)).EUL_AcceptedByJobTitle)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).AcceptanceLogs)).SyncRoot)).EUL_AcceptedByEmail)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).AcceptanceLogs)).SyncRoot)).AgreementVariant)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).AcceptanceLogs)).SyncRoot)).AgreementVersion)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).AcceptanceLogs)).SyncRoot)).EUL_AcceptanceTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).AcceptanceLogs)).SyncRoot)).DatabaseNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAcceptanceLog)(((System.Collections.IList)(((Enterprise.Client.EDI.UserManagement.Business.EdiUserAgreementAssignment)(null)).AcceptanceLogs)).SyncRoot)).ServerCode)));
			this.agreementAcceptanceGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo4.Caption = "Accepting Name";
			zTextBoxColumnStyleInfo4.CaptionResourceString = ZClientEDI.Res.GetData("bfc4cf3d-e8e3-4be2-b1bb-ffd3b44f7504", "Accepting Name");
			zTextBoxColumnStyleInfo4.ColumnName = "EUL_AcceptedByName";
			
			zTextBoxColumnStyleInfo4.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo4.IsReadOnly = true;
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.Caption = "Job Title";
			zTextBoxColumnStyleInfo5.CaptionResourceString = ZClientEDI.Res.GetData("84327b7e-1bdc-4d39-b6c5-9f62f2ebd3b7", "Job Title");
			zTextBoxColumnStyleInfo5.ColumnName = "EUL_AcceptedByJobTitle";
			zTextBoxColumnStyleInfo5.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.Caption = "Accepting Email";
			zTextBoxColumnStyleInfo6.CaptionResourceString = ZClientEDI.Res.GetData("26fbce61-1185-429d-b89b-0580562b5b94", "Accepting Email");
			zTextBoxColumnStyleInfo6.ColumnName = "EUL_AcceptedByEmail";
			zTextBoxColumnStyleInfo6.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo6.IsReadOnly = true;
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.Caption = "Variant";
			zTextBoxColumnStyleInfo7.CaptionResourceString = ZClientEDI.Res.GetData("bc9caa47-18ef-4ed2-b45f-09e9c25a44f0", "Variant");
			zTextBoxColumnStyleInfo7.ColumnName = "AgreementVariant";
			zTextBoxColumnStyleInfo7.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo7.IsReadOnly = true;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo8.Caption = "Version";
			zTextBoxColumnStyleInfo8.CaptionResourceString = ZClientEDI.Res.GetData("483941d1-d65f-4467-9db8-2b4be1c42296", "Version");
			zTextBoxColumnStyleInfo8.ColumnName = "AgreementVersion";
			zTextBoxColumnStyleInfo8.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo8.IsReadOnly = true;
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo2.Caption = "Acceptance Date (UTC)";
			zDateEditColumnStyleInfo2.CaptionResourceString = ZClientEDI.Res.GetData("9750e30c-727f-4daf-9a39-3e71c9f0dbc7", "Acceptance Date (UTC)");
			zDateEditColumnStyleInfo2.ColumnName = "EUL_AcceptanceTimeUtc";
			zDateEditColumnStyleInfo2.DefaultCollectionIndex = 0;
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = ZClientEDI.Res.GetData("f0d43e43-94b3-4529-a84f-5ee1a070f678", "Database Number");
			zCalcEditColumnStyleInfo1.ColumnName = "DatabaseNumber";
			zCalcEditColumnStyleInfo1.DefaultCollectionIndex = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo9.CaptionResourceString = ZClientEDI.Res.GetData("dd495c9d-9fcd-474f-a012-db12a369ff0f", "Server Code");
			zTextBoxColumnStyleInfo9.ColumnName = "ServerCode";
			zTextBoxColumnStyleInfo9.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.agreementAcceptanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.agreementAcceptanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.agreementAcceptanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.agreementAcceptanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.agreementAcceptanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.agreementAcceptanceGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.agreementAcceptanceGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.agreementAcceptanceGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.agreementAcceptanceGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.agreementAcceptanceGrid.GridId = "cd56c4f8-ba9f-43e4-b0de-d03ed6303e28";
			this.agreementAcceptanceGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.agreementAcceptanceGrid.LayoutKey = "agreementAcceptanceGrid";
			this.agreementAcceptanceGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 15, true);
			this.agreementAcceptanceGrid.Name = "agreementAcceptanceGrid";
			this.agreementAcceptanceGrid.ReadOnly = true;
			this.agreementAcceptanceGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(855, 213, true);
			this.agreementAcceptanceGrid.TabIndex = 0;
			// 
			// EdiAgreementAssignmentUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.Controls.Add(this.kSplitContainer1);
			this.Name = "EdiAgreementAssignmentUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(863, 518, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.corporateAgreementsGroupBox.ResumeLayout(false);
			this.corporateAgreementsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.corporateAgreementsGrid)).EndInit();
			this.corporateAgreementsGrid.ResumeLayout(false);
			this.corporateAgreementsGrid.PerformLayout();
			this.kSplitContainer1.Panel1.ResumeLayout(false);
			this.kSplitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.kSplitContainer1)).EndInit();
			this.kSplitContainer1.ResumeLayout(false);
			this.kSplitContainer1.PerformLayout();
			this.agreementAcceptanceGroupBox.ResumeLayout(false);
			this.agreementAcceptanceGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.agreementAcceptanceGrid)).EndInit();
			this.agreementAcceptanceGrid.ResumeLayout(false);
			this.agreementAcceptanceGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer kSplitContainer1;
		private ZArchitecture.ZGrid corporateAgreementsGrid;
		private ZArchitecture.ZGrid agreementAcceptanceGrid;
		private ZArchitecture.GUI.ZGroupBox corporateAgreementsGroupBox;
		private ZArchitecture.GUI.ZGroupBox agreementAcceptanceGroupBox;
		private ZArchitecture.GUI.ZButton addAcceptanceButton;
		
		
	}
}
