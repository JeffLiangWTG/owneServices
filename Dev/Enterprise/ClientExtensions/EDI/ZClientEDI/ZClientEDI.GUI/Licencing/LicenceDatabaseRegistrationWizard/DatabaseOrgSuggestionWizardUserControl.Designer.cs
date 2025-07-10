namespace Enterprise.Client.EDI.Licencing.GUI
{
	partial class DatabaseOrgSuggestionWizardUserControl
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.IContainer components = null;

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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.MainGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.SetSelectSuggestionButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OrgSuggestionGrid = new Enterprise.ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.MainGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgSuggestionGrid)).BeginInit();
			this.OrgSuggestionGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion);
			// 
			// MainGroupBox
			// 
			this.MainGroupBox.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("93f8bd8e-6a33-46e4-8df7-b9084942c507", "Matching Suggestions");
			this.MainGroupBox.Controls.Add(this.SetSelectSuggestionButton);
			this.MainGroupBox.Controls.Add(this.OrgSuggestionGrid);
			this.MainGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainGroupBox.Name = "MainGroupBox";
			this.MainGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 373, true);
			this.MainGroupBox.TabIndex = 0;
			this.MainGroupBox.TabStop = false;
			// 
			// SetSelectSuggestionButton
			// 
			this.SetSelectSuggestionButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.SetSelectSuggestionButton.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b7925a1e-f518-4f5e-a92f-9eca2f3fa082", "Set Selected Suggestion");
			this.SetSelectSuggestionButton.IsCaptionOverridden = false;
			this.SetSelectSuggestionButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 341, true);
			this.SetSelectSuggestionButton.Name = "SetSelectSuggestionButton";
			this.SetSelectSuggestionButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SetSelectSuggestionButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(155, 23, true);
			this.SetSelectSuggestionButton.TabIndex = 0;
			this.SetSelectSuggestionButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.SetSelectSuggestionButton.ToolTipCaption = null;
			this.SetSelectSuggestionButton.UseVisualStyleBackColor = true;
			this.SetSelectSuggestionButton.Click += new System.EventHandler(this.SetSelectSuggestionButton_Click);
			// 
			// OrgSuggestionGrid
			// 
			this.OrgSuggestionGrid.AllowBeginDrag = false;
			this.OrgSuggestionGrid.AllowDragDropWithChanges = false;
			this.OrgSuggestionGrid.AllowNavigation = false;
			this.OrgSuggestionGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.OrgSuggestionGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)))));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)).Header.LicenceEnterpriseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)).Header.LicenceEnterpriseCode)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)).Header.OH_Code)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)).OrgName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)).Header.CityName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)).Header.OH_Category)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)).Header.OH_IsActive)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)).Header.LicenceDatabaseCount)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.EdiLicenceDatabaseOrgSuggestion)(null)).LDS_TotalScore)));
			this.OrgSuggestionGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("0408f398-986b-458a-b4b5-15290c06f6b4", "Enterprise ID");
			zTextBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo1.ColumnName = "Header+LicenceEnterpriseID";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("a6d62804-597f-4883-8d4b-5e9264eb9c0a", "Enterprise Code");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "Header+LicenceEnterpriseCode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo3.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("26996752-48d5-4b71-ba4f-99fa4c9e9929", "Org Code");
			zTextBoxColumnStyleInfo3.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo3.ColumnName = "Header+OH_Code";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("c96cf147-34b3-477a-8cf8-3040151ae1ed", "Org Name");
			zTextBoxColumnStyleInfo4.ColumnName = "OrgName";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo5.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("b172eee2-a507-4a4d-90a0-f797ab5c28f6", "City");
			zTextBoxColumnStyleInfo5.ColumnName = "Header+CityName";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo6.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("2cc1757c-808c-47ef-94af-1676e2e8852e", "Category");
			zTextBoxColumnStyleInfo6.ColumnName = "Header+OH_Category";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("68dfcd6c-f99e-4c7d-9d90-0d8519c8f0d9", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "Header+OH_IsActive";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo7.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("9996a659-2e57-41e0-81b3-13c4e8a26e8b", "Database Count");
			zTextBoxColumnStyleInfo7.ColumnName = "Header+LicenceDatabaseCount";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo8.CaptionResourceString = CargoWiseOne.ResourceStrings.Res.GetData("be75c7bf-ce60-4ba1-8728-b88707151274", "Matching Score");
			zTextBoxColumnStyleInfo8.ColumnName = "LDS_TotalScore";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.OrgSuggestionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.OrgSuggestionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.OrgSuggestionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.OrgSuggestionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.OrgSuggestionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.OrgSuggestionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.OrgSuggestionGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.OrgSuggestionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.OrgSuggestionGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.OrgSuggestionGrid.GridId = "88453a2b-8e06-4c00-94f3-ba48dd335cbc";
			this.OrgSuggestionGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.OrgSuggestionGrid.LayoutKey = "OrgSuggestionGrid";
			this.OrgSuggestionGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.OrgSuggestionGrid.Name = "OrgSuggestionGrid";
			this.OrgSuggestionGrid.ReadOnly = true;
			this.OrgSuggestionGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.OrgSuggestionGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 319, true);
			this.OrgSuggestionGrid.TabIndex = 4;
			// 
			// DatabaseOrgSuggestionWizardUserControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.MainGroupBox);
			this.Name = "DatabaseOrgSuggestionWizardUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(920, 373, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.MainGroupBox.ResumeLayout(false);
			this.MainGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.OrgSuggestionGrid)).EndInit();
			this.OrgSuggestionGrid.ResumeLayout(false);
			this.OrgSuggestionGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZGroupBox MainGroupBox;
		protected ZArchitecture.ZGrid OrgSuggestionGrid;
		private ZArchitecture.GUI.ZButton SetSelectSuggestionButton;
	}
}
