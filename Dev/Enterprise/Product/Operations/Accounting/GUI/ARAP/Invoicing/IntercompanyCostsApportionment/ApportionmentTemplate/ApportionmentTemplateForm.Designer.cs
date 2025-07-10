namespace Enterprise.Accounting.GUI.ARAP.Invoicing.IntercompanyCostsApportionment
{
	partial class ApportionmentTemplateForm
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
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			this.mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.TabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.TemplateTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.childSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			this.PercentageTotalCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.NotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DescriptionTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.CompanyGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.templateLineGrid = new Enterprise.ZArchitecture.ZGrid();
			this.LogsTabPage = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.postingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.mainSplitContainer.Panel1.SuspendLayout();
			this.mainSplitContainer.Panel2.SuspendLayout();
			this.mainSplitContainer.SuspendLayout();
			this.TabControl.SuspendLayout();
			this.TemplateTabPage.SuspendLayout();
			this.childSplitContainer.Panel1.SuspendLayout();
			this.childSplitContainer.Panel2.SuspendLayout();
			this.childSplitContainer.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.templateLineGrid)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 381, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate);
			// 
			// mainSplitContainer
			// 
			this.mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.mainSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel2;
			this.mainSplitContainer.IsSplitterFixed = true;
			this.mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.mainSplitContainer.Name = "mainSplitContainer";
			this.mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			this.mainSplitContainer.Panel1.Controls.Add(this.TabControl);
			// 
			// mainSplitContainer.Panel2
			// 
			this.mainSplitContainer.Panel2.Controls.Add(this.postingButtonsUserControl);
			this.mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 405, true);
			this.mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(376);
			this.mainSplitContainer.TabIndex = 1;
			this.mainSplitContainer.TabStop = false;
			// 
			// TabControl
			// 
			this.TabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.TabControl.Controls.Add(this.TemplateTabPage);
			this.TabControl.Controls.Add(this.LogsTabPage);
			this.TabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 376, true);
			this.TabControl.TabIndex = 0;
			// 
			// TemplateTabPage
			// 
			this.TemplateTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentTemplateForm|2ba34ff6-034d-443f-80d1-a96d5b60a7ea", "Template");
			this.TemplateTabPage.Controls.Add(this.childSplitContainer);
			this.TemplateTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.TemplateTabPage.Name = "TemplateTabPage";
			this.TemplateTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.TemplateTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 349, true);
			this.TemplateTabPage.TabIndex = 0;
			this.TemplateTabPage.UseVisualStyleBackColor = true;
			// 
			// childSplitContainer
			// 
			this.childSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			this.childSplitContainer.FixedPanel = System.Windows.Forms.FixedPanel.Panel1;
			this.childSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.childSplitContainer.Name = "childSplitContainer";
			this.childSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// childSplitContainer.Panel1
			// 
			this.childSplitContainer.Panel1.Controls.Add(this.PercentageTotalCalcEdit);
			this.childSplitContainer.Panel1.Controls.Add(this.NotesTextBox);
			this.childSplitContainer.Panel1.Controls.Add(this.DescriptionTextBox);
			this.childSplitContainer.Panel1.Controls.Add(this.CompanyGuidFindBox);
			// 
			// childSplitContainer.Panel2
			// 
			this.childSplitContainer.Panel2.Controls.Add(this.templateLineGrid);
			this.childSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 343, true);
			this.childSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(185);
			this.childSplitContainer.TabIndex = 0;
			this.childSplitContainer.TabStop = false;
			// 
			// PercentageTotalCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PercentageTotalCalcEdit, "PercentageTotal");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).PercentageTotal)));
			this.PercentageTotalCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentTemplateForm|8b9ce247-8f73-4560-9456-29dbaa6f800f", "Percentage Total");
			this.PercentageTotalCalcEdit.Decimals = 3;
			this.PercentageTotalCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(510, 38, true);
			this.PercentageTotalCalcEdit.Name = "PercentageTotalCalcEdit";
			this.PercentageTotalCalcEdit.ReadOnly = true;
			this.PercentageTotalCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PercentageTotalCalcEdit.TabIndex = 6;
			this.PercentageTotalCalcEdit.Text = "0.000";
			this.PercentageTotalCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// NotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.NotesTextBox, "A0_Notes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).A0_Notes)));
			this.NotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 63, true);
			this.NotesTextBox.Multiline = true;
			this.NotesTextBox.Name = "NotesTextBox";
			this.NotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(492, 110, true);
			this.NotesTextBox.TabIndex = 5;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "A0_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).A0_Description)));
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 37, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.DescriptionTextBox.TabIndex = 2;
			// 
			// CompanyGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.CompanyGuidFindBox, "A0_GC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).A0_GC)));
			this.CompanyGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 11, true);
			this.CompanyGuidFindBox.Name = "CompanyGuidFindBox";
			this.CompanyGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.CompanyGuidFindBox.TabIndex = 0;
			// 
			// templateLineGrid
			// 
			this.templateLineGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.templateLineGrid, "Lines");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).Lines)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplateLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).Lines)).SyncRoot)).Company)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplateLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).Lines)).SyncRoot)).Y0_GB)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplateLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).Lines)).SyncRoot)).Y0_GE)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplateLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).Lines)).SyncRoot)).Y0_Description)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplateLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).Lines)).SyncRoot)).Y0_Percentage_DecimalPlaces)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplateLines)(((System.Collections.IList)(((Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate)(null)).Lines)).SyncRoot)).Y0_Percentage)));
			this.templateLineGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentTemplateForm|5bda24fc-c998-4ecc-ab7b-3c174826e076", "Company");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "Company";
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zGuidFindBoxColumnStyleInfo2.ColumnName = "Y0_GB";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "Y0_GE";
			zTextBoxColumnStyleInfo1.ColumnName = "Y0_Description";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "Y0_Percentage_DecimalPlaces";
			zCalcEditColumnStyleInfo1.ColumnName = "Y0_Percentage";
			this.templateLineGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.templateLineGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.templateLineGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.templateLineGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.templateLineGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.templateLineGrid.GridId = "a44d5447-23e9-4a31-8050-62027c8c8e06";
			this.templateLineGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.templateLineGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.templateLineGrid.LayoutKey = "templateLineGrid";
			this.templateLineGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.templateLineGrid.Name = "templateLineGrid";
			this.templateLineGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(619, 154, true);
			this.templateLineGrid.TabIndex = 0;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.ExcludeFromBindingOnSave = true;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(625, 349, true);
			this.LogsTabPage.TabIndex = 1;
			this.LogsTabPage.UseVisualStyleBackColor = true;
			// 
			// postingButtonsUserControl
			// 
			this.postingButtonsUserControl.Dock = System.Windows.Forms.DockStyle.Right;
			this.postingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 0, true);
			this.postingButtonsUserControl.Name = "postingButtonsUserControl";
			this.postingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 25, true);
			this.postingButtonsUserControl.TabIndex = 0;
			// 
			// ApportionmentTemplateForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(633, 405, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ApportionmentTemplateForm|d9fb50bc-dc25-4cdc-9820-fc3cd4dcbfa0", "Apportionment Template");
			this.Controls.Add(this.mainSplitContainer);
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ARAP.Invoicing.AccApportionmentTemplate);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(649, 441, true);
			this.Name = "ApportionmentTemplateForm";
			this.Text = "ApportionmentTemplateForm";
			this.Controls.SetChildIndex(this.mainSplitContainer, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.mainSplitContainer.Panel1.ResumeLayout(false);
			this.mainSplitContainer.Panel2.ResumeLayout(false);
			this.mainSplitContainer.ResumeLayout(false);
			this.TabControl.ResumeLayout(false);
			this.TemplateTabPage.ResumeLayout(false);
			this.childSplitContainer.Panel1.ResumeLayout(false);
			this.childSplitContainer.Panel1.PerformLayout();
			this.childSplitContainer.Panel2.ResumeLayout(false);
			this.childSplitContainer.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.templateLineGrid)).EndInit();
			this.ResumeLayout(false);

		}

		#endregion

		private CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
		private CargoWise.Windows.UI.KSplitContainer childSplitContainer;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl postingButtonsUserControl;
		private Enterprise.ZArchitecture.ZGrid templateLineGrid;
		private Enterprise.ZArchitecture.ZTextBox DescriptionTextBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox CompanyGuidFindBox;
		private Enterprise.ZArchitecture.ZTextBox NotesTextBox;
		private Enterprise.ZArchitecture.ZCalcEdit PercentageTotalCalcEdit;
		private Enterprise.ZArchitecture.GUI.ZTabControl TabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage TemplateTabPage;
		private Enterprise.ZArchitecture.GUI.ZLogsTabPage LogsTabPage;
	}
}
