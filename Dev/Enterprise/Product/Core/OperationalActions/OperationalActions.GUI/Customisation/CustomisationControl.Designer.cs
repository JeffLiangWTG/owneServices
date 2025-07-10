namespace Enterprise.Services.OperationalActions.GUI
{
	partial class CustomisationControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.GUI.ZGroupBox actionsGroupBox;
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo zTranslatableTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTranslatableTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZTabPage methodsTab;
			Enterprise.Services.OperationalActions.GUI.MethodsTabPage methodsControl;
			Enterprise.Services.OperationalActions.GUI.FieldsTabPage fieldsControl;
			Enterprise.ZArchitecture.GUI.ZTabControl settingsTabControl;
			Enterprise.Services.OperationalActions.GUI.DocumentsTabPage documentsControl;

			CargoWise.Windows.UI.KSplitContainer mainSplitContainer;
			this.actionsGrid = new Enterprise.ZArchitecture.ZGrid();
			this.documentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.fieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			actionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			methodsTab = new Enterprise.ZArchitecture.GUI.ZTabPage();
			methodsControl = new Enterprise.Services.OperationalActions.GUI.MethodsTabPage();
			fieldsControl = new Enterprise.Services.OperationalActions.GUI.FieldsTabPage();
			settingsTabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			documentsControl = new Enterprise.Services.OperationalActions.GUI.DocumentsTabPage();
			mainSplitContainer = new CargoWise.Windows.UI.KSplitContainer();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			actionsGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.actionsGrid)).BeginInit();
			this.actionsGrid.SuspendLayout();
			methodsTab.SuspendLayout();
			methodsControl.SuspendLayout();
			fieldsControl.SuspendLayout();
			settingsTabControl.SuspendLayout();
			documentsTabPage.SuspendLayout();
			documentsControl.SuspendLayout();
			this.fieldsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(mainSplitContainer)).BeginInit();
			mainSplitContainer.Panel1.SuspendLayout();
			mainSplitContainer.Panel2.SuspendLayout();
			mainSplitContainer.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Services.OperationalActions.Business.OperationalActionManager);
			// 
			// actionsGroupBox
			// 
			actionsGroupBox.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("CustomisationControl|9c94304b-51a3-4d96-9b99-f279da5911ca", "Actions");
			actionsGroupBox.Controls.Add(this.actionsGrid);
			actionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			actionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			actionsGroupBox.Name = "actionsGroupBox";
			actionsGroupBox.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(7, 3, 7, 7, true);
			actionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 192, true);
			actionsGroupBox.TabIndex = 0;
			actionsGroupBox.TabStop = false;
			// 
			// actionsGrid
			// 
			this.actionsGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.actionsGrid, "Actions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)).SU_MenuName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)).SU_MenuPath)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)).SU_FilterList)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)).SU_SE_NKDocumentEvent)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)).Lookups.Events)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)).SU_Calc_IsPublished)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)).SU_IsSystemDefined)));
			this.actionsGrid.CaptionVisible = false;
			zTranslatableTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("CustomisationControl|5832d3cb-093d-42e3-989e-a5082efda0ab", "Name", "Action Name", "The name to show in the menu.");
			zTranslatableTextBoxColumnStyleInfo1.ColumnName = "SU_MenuName";
			zTranslatableTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTranslatableTextBoxColumnStyleInfo2.ColumnName = "SU_MenuPath";
			zTranslatableTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("CustomisationControl|7b3b1f01-0b0c-4ab4-86cb-909941f9c6a0", "Filter", "The filter determines if and when an action appears in the menu.");
			zTextBoxColumnStyleInfo1.ColumnName = "SU_FilterList";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDropEditColumnStyleInfo1.BindToList = "Lookups+Events";
			zDropEditColumnStyleInfo1.ColumnName = "SU_SE_NKDocumentEvent";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.ColumnName = "SU_Calc_IsPublished";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("CustomisationControl|d32a2f09-1c10-4a6f-824d-be3639450826", "Sys", "System", "System defined actions are provided by CargoWise.");
			zCheckBoxColumnStyleInfo2.ColumnName = "SU_IsSystemDefined";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.actionsGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo1);
			this.actionsGrid.ColumnStyles.Add(zTranslatableTextBoxColumnStyleInfo2);
			this.actionsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.actionsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.actionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.actionsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.actionsGrid.CopySelectedRowsAllowed = true;
			this.actionsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.actionsGrid.GridId = "a8d54ab7-f429-4524-b456-1e352a4b5e67";
			this.actionsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.actionsGrid.LayoutKey = "zGrid1";
			this.actionsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 16, true);
			this.actionsGrid.Name = "actionsGrid";
			this.actionsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 169, true);
			this.actionsGrid.TabIndex = 0;
			// 
			// methodsTab
			// 
			methodsTab.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("CustomisationControl|013cdd35-abce-4d50-8e08-a0c8de559390", "Defined Processes to Run");
			methodsTab.Controls.Add(methodsControl);
			methodsTab.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			methodsTab.Name = "methodsTab";
			methodsTab.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			methodsTab.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 181, true);
			methodsTab.TabIndex = 2;
			// 
			// methodsControl
			// 
			methodsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(methodsControl, "Actions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)))));
			methodsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			methodsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			methodsControl.Name = "methodsControl";
			methodsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 170, true);
			methodsControl.TabIndex = 1;
			// 
			// fieldsControl
			// 
			fieldsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(fieldsControl, "Actions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)))));
			fieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			fieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			fieldsControl.Name = "fieldsControl";
			fieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(684, 177, true);
			fieldsControl.TabIndex = 0;
			// 
			// settingsTabControl
			// 
			settingsTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			settingsTabControl.Controls.Add(this.documentsTabPage);
			settingsTabControl.Controls.Add(methodsTab);
			settingsTabControl.Controls.Add(this.fieldsTabPage);
			settingsTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			settingsTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			settingsTabControl.Name = "settingsTabControl";
			settingsTabControl.SelectedIndex = 0;
			settingsTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 204, true);
			settingsTabControl.TabIndex = 2;
			// 
			// documentsTabPage
			// 
			this.documentsTabPage.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("CustomisationControl|fa7bf053-432f-4c62-bb7e-d6a21f63c3b7", "Documents to Run");
			this.documentsTabPage.Controls.Add(documentsControl);
			this.documentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.documentsTabPage.Name = "documentsTabPage";
			this.documentsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.documentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 181, true);
			this.documentsTabPage.TabIndex = 0;
			this.documentsTabPage.UseVisualStyleBackColor = true;
			// 
			// documentsControl
			// 
			documentsControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(documentsControl, "Actions");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((Enterprise.Services.OperationalActions.Business.OperationalAction)(((System.Collections.IList)(((Enterprise.Services.OperationalActions.Business.OperationalActionManager)(null)).Actions)).SyncRoot)))));
			documentsControl.Dock = System.Windows.Forms.DockStyle.Fill;
			documentsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			documentsControl.Name = "documentsControl";
			documentsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(681, 170, true);
			documentsControl.TabIndex = 0;
			// 
			// fieldsTabPage
			// 
			this.fieldsTabPage.CaptionResourceString = Enterprise.Services.OperationalActions.GUI.Res.GetData("CustomisationControl|4435f8e6-6901-45ea-95a2-8993f89d95c7", "Fields to Show");
			this.fieldsTabPage.Controls.Add(fieldsControl);
			this.fieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.fieldsTabPage.Name = "fieldsTabPage";
			this.fieldsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.fieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(689, 182, true);
			this.fieldsTabPage.TabIndex = 1;
			this.fieldsTabPage.UseVisualStyleBackColor = true;
			// 
			// mainSplitContainer
			// 
			mainSplitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
			mainSplitContainer.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			mainSplitContainer.Name = "mainSplitContainer";
			mainSplitContainer.Orientation = System.Windows.Forms.Orientation.Horizontal;
			// 
			// mainSplitContainer.Panel1
			// 
			mainSplitContainer.Panel1.Controls.Add(actionsGroupBox);
			// 
			// mainSplitContainer.Panel2
			// 
			mainSplitContainer.Panel2.Controls.Add(settingsTabControl);
			mainSplitContainer.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 399, true);
			mainSplitContainer.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
			mainSplitContainer.TabIndex = 1;
			// 
			// CustomisationControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(mainSplitContainer);
			this.Name = "CustomisationControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 399, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			actionsGroupBox.ResumeLayout(false);
			actionsGroupBox.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.actionsGrid)).EndInit();
			this.actionsGrid.ResumeLayout(false);
			this.actionsGrid.PerformLayout();
			methodsTab.ResumeLayout(false);
			methodsTab.PerformLayout();
			methodsControl.ResumeLayout(true);
			methodsControl.PerformLayout();
			fieldsControl.ResumeLayout(true);
			fieldsControl.PerformLayout();
			settingsTabControl.ResumeLayout(false);
			settingsTabControl.PerformLayout();
			this.documentsTabPage.ResumeLayout(false);
			this.documentsTabPage.PerformLayout();
			documentsControl.ResumeLayout(true);
			documentsControl.PerformLayout();
			this.fieldsTabPage.ResumeLayout(false);
			this.fieldsTabPage.PerformLayout();
			mainSplitContainer.Panel1.ResumeLayout(false);
			mainSplitContainer.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(mainSplitContainer)).EndInit();
			mainSplitContainer.ResumeLayout(false);
			mainSplitContainer.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}
		private Enterprise.ZArchitecture.GUI.ZTabPage documentsTabPage;
		private Enterprise.ZArchitecture.ZGrid actionsGrid;
		private Enterprise.ZArchitecture.GUI.ZTabPage fieldsTabPage;
	}
}
