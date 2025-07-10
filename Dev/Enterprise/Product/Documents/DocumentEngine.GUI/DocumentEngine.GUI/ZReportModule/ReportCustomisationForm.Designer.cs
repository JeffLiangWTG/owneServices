namespace Enterprise.DocumentEngine.GUI
{
	partial class ReportCustomisationForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo ZMacrosFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZMacrosFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.TemplatesUsedGroupBox.SuspendLayout();
			this.PivotAndChildMenuTabControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TemplatesUsedGrid)).BeginInit();
			this.TemplatesUsedGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MenusGrid)).BeginInit();
			this.MenusGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.availableTemplatesGrid)).BeginInit();
			this.availableTemplatesGrid.SuspendLayout();
			this.menuDetailsTab.SuspendLayout();
			this.filterAndDescPanel.SuspendLayout();
			this.documentOptionsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// TemplatesUsedGroupBox
			// 
			this.TemplatesUsedGroupBox.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|25e8ad39-121e-4d0c-8648-0fcd5abd60dd", "Templates Used");
			// 
			// TemplatesUsedGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "SI_DocumentTitle";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.ToolTip = "The title displayed inside rendered document or report.";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.ColumnName = "SI_IsSystemDefined";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.ToolTip = "Is this a system-defined document or report?";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo2.ColumnName = "SI_IsClientSpecific";
			zCheckBoxColumnStyleInfo2.IsMandatory = true;
			zCheckBoxColumnStyleInfo2.ToolTip = "Is this a client-specific document or report?";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "SI_Index";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.ToolTip = "The index representing the order to print.";
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zGuidFindBoxColumnStyleInfo1.BindToList = "DocTypeList";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "SI_RT_DocType";
			zGuidFindBoxColumnStyleInfo1.ToolTip = "If this form has an eDocs tab, this is the Document Type the output will be categ" +
		"orised as on the eDocs tab";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo2.ColumnName = "SO_Name";
			zTextBoxColumnStyleInfo2.IsMandatory = true;
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo12.ColumnName = "SI_IsPasswordProtected";
			zCheckBoxColumnStyleInfo12.IsMandatory = true;
			zCheckBoxColumnStyleInfo12.ToolTip = "Should this document or report be password protected?";
			zCheckBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo13.ColumnName = "SI_IsPasswordProtectedForOpening";
			zCheckBoxColumnStyleInfo13.IsMandatory = true;
			zCheckBoxColumnStyleInfo13.ToolTip = "Password For Opening";
			zCheckBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			this.TemplatesUsedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TemplatesUsedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.TemplatesUsedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.TemplatesUsedGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.TemplatesUsedGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.TemplatesUsedGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TemplatesUsedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo12);
			this.TemplatesUsedGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo13);

			// 
			// MenusGrid
			// 
			zTranslatableTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|c522a2d8-ce00-4c7a-9759-a59f4fb3593a", "Name");
			zTranslatableTextBoxColumnStyleInfo1.ColumnName = "SU_MenuName";
			zTranslatableTextBoxColumnStyleInfo1.IsMandatory = true;
			zTranslatableTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCheckBoxColumnStyleInfo3.ColumnName = "SU_IsSystemDefined";
			zCheckBoxColumnStyleInfo3.IsMandatory = true;
			zCheckBoxColumnStyleInfo3.ToolTip = "Is this a system-defined menu?";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo4.ColumnName = "SU_IsClientSpecific";
			zCheckBoxColumnStyleInfo4.IsMandatory = true;
			zCheckBoxColumnStyleInfo4.ToolTip = "Is this a client-specific menu?";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo5.ColumnName = "SU_IsPublished";
			zCheckBoxColumnStyleInfo5.IsMandatory = true;
			zCheckBoxColumnStyleInfo5.ToolTip = "Is this menu published to all users in the system?";
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.ColumnName = "SU_MenuIndex";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.IsMandatory = true;
			zCalcEditColumnStyleInfo2.ToolTip = "The position of the menu within its menu path.";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|95e0174c-eaeb-4b3f-bca1-0ed5f44cc934", "Supports Web");
			zCheckBoxColumnStyleInfo6.ColumnName = "SU_Calc_IsWebSupportable";
			zCheckBoxColumnStyleInfo6.ToolTip = "Is Published on Web?";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo11.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|2fa0b8cf-5314-4aea-9fff-30352b4d50c0", "Web Visible");
			zCheckBoxColumnStyleInfo11.ColumnName = "SU_IsVisibleOnWeb";
			zCheckBoxColumnStyleInfo11.ToolTip = "Is visible on Web?";
			zCheckBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo7.ColumnName = "SU_SupportsVisualisation";
			zCheckBoxColumnStyleInfo7.ToolTip = "Does this menu support visualisation?";
			zCheckBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo8.ColumnName = "SU_IsModifiable";
			zCheckBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCheckBoxColumnStyleInfo9.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|d56dd80b-a6df-4567-baa5-0c8ee9aa5e95", "Local", "Local Culture", "");
			zCheckBoxColumnStyleInfo9.ColumnName = "SU_IsLocalDocument";
			zCheckBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			ZMacrosFindBoxColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			ZMacrosFindBoxColumnStyleInfo1.ColumnName = "SU_FilterList";
			ZMacrosFindBoxColumnStyleInfo1.ToolTip = "Specify a list of filter criteria, e.g. TRN=\"SEA\".";
			ZMacrosFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			ZMacrosFindBoxColumnStyleInfo1.IsUsedForExpressions = true;
			zTranslatableTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|e647404f-f1c9-42d4-b9ba-bd1e3b9b7629", "Description");
			zTranslatableTextBoxColumnStyleInfo2.ColumnName = "SU_Hint";
			zTranslatableTextBoxColumnStyleInfo2.IsVisible = false;
			zTranslatableTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCheckBoxColumnStyleInfo10.ColumnName = "SU_MustRunOnline";
			zCheckBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "SignByList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|ae6cb6ec-c3b4-4be2-b02e-628303f78a7d", "Sign By", "Digital Signature");
			zDropEditColumnStyleInfo2.ColumnName = "SU_SignBy";
			zDropEditColumnStyleInfo2.IsVisible = false;
			zDropEditColumnStyleInfo2.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			zDropEditColumnStyleInfo2.ToolTip = "The sign method of this document.";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zDropEditColumnStyleInfo1.ColumnName = "SU_DefaultAttachmentType";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.MenusGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo1);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.MenusGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo9);
			this.MenusGrid.ColumnStyles.Add(ZMacrosFindBoxColumnStyleInfo1);
			this.MenusGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo2);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo10);
			this.MenusGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.MenusGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo11);
			this.MenusGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.MenusGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(914, 204, true);
			// 
			// loadTemplateButton
			// 
			this.loadTemplateButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|e47c1718-1196-4834-918e-55923e4cb4a1", "Load");
			// 
			// copyTemplateButton
			// 
			this.copyTemplateButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|dfce4eff-3214-4355-9621-2651c39d8369", "Copy Selected");
			// 
			// newTemplateButton
			// 
			this.newTemplateButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|7b43da11-585c-4890-837c-40e7b2609a52", "New");
			// 
			// editTemplateButton
			// 
			this.editTemplateButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|56184926-1adf-4286-a343-2ac5a18f93f6", "Edit");
			// 
			// removeTemplatePivotButton
			// 
			this.removeTemplatePivotButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|d12eb9da-fb6f-4a64-a2a4-a29f47ba5bb7", "<-");
			// 
			// addTemplatePivotButton
			// 
			this.addTemplatePivotButton.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|4296fc6a-2c5c-446d-afe1-6a9e187263db", "->");
			// 
			// systemCheckBox
			// 
			this.systemCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			// 
			// clientCheckBox
			// 
			this.clientCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(93, 17, true);
			// 
			// visualCheckBox
			// 
			this.visualCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			// 
			// publishedCheckBox
			// 
			this.publishedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(60, 17, true);
			// 
			// modifyCheckBox
			// 
			this.modifyCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(111, 17, true);
			// 
			// localCheckBox
			// 
			this.localCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 17, true);
			// 
			// autoDeliveryCheckBox
			// 
			this.autoDeliveryCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(129, 17, true);
			// 
			// docPackCheckBox
			// 
			this.docPackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(74, 17, true);
			// 
			// zipDocPackCheckBox
			// 
			this.zipDocPackCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 17, true);
			// 
			// filterAndDescPanel
			// 
			this.filterAndDescPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(487, 29, true);
			// 
			// webCheckBox
			// 
			this.webCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 17, true);
			// 
			// webIsVisibleCheckBox
			// 
			this.webIsVisibleCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(49, 17, true);
			// 
			// documentOptionsPanel
			// 
			this.documentOptionsPanel.Visible = false;
			// 
			// MustRunOnlineCheckBox
			// 
			this.MustRunOnlineCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 17, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentEngine.GUI.ReportMenuCustomisation);
			// 
			// ReportCustomisationForm
			// 
			this.CaptionResourceString = Enterprise.DocumentEngine.GUI.Res.GetData("ReportCustomisationForm|785defce-3737-4ae2-8b99-2fffd2cc10c1", "Customize Reports");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 686, true);
			this.DataSourceType = typeof(Enterprise.DocumentEngine.GUI.ReportMenuCustomisation);
			this.DataSourceTypeName = "Enterprise.DocumentEngine.ReportMenuCustomisation";
			this.Name = "ReportCustomisationForm";
			this.TemplatesUsedGroupBox.ResumeLayout(false);
			this.TemplatesUsedGroupBox.PerformLayout();
			this.PivotAndChildMenuTabControl.ResumeLayout(false);
			this.PivotAndChildMenuTabControl.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.TemplatesUsedGrid)).EndInit();
			this.TemplatesUsedGrid.ResumeLayout(false);
			this.TemplatesUsedGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MenusGrid)).EndInit();
			this.MenusGrid.ResumeLayout(false);
			this.MenusGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.availableTemplatesGrid)).EndInit();
			this.availableTemplatesGrid.ResumeLayout(false);
			this.availableTemplatesGrid.PerformLayout();
			this.menuDetailsTab.ResumeLayout(false);
			this.menuDetailsTab.PerformLayout();
			this.filterAndDescPanel.ResumeLayout(false);
			this.filterAndDescPanel.PerformLayout();
			this.documentOptionsPanel.ResumeLayout(false);
			this.documentOptionsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
	}
}
