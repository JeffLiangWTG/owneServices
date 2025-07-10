namespace Enterprise.DocumentScanning.GUI
{
	public partial class ZAllocateDocumentsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		protected internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl DocumentAllocationTabControl;
		private Enterprise.ZArchitecture.GUI.ZTabPage AllocatedDocumentsTabPage;
		private Enterprise.ZArchitecture.GUI.ZButton UnallocateBoundButton;
		private CargoWise.Windows.UI.KPanel GraphicalDisplayControlPanel;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		internal Enterprise.DocumentScanning.GUI.BindableGridControl AllocatedGridControl;
		internal Enterprise.DocumentScanning.GUI.BindableGridControl UnallocatedGridControl;
#if !WINZOR
		private Enterprise.ZArchitecture.GUI.ZButton ImportConfigurationBoundButton;
		private Enterprise.ZArchitecture.GUI.ZButton ImportBoundButton;
#endif
		private Enterprise.ZArchitecture.GUI.ZButton AllocateBoundButton;
		private Enterprise.ZArchitecture.GUI.ZTabPage UnallocatedDocumentsTabPage;
		private Enterprise.ZArchitecture.GUI.ZButton CreateNewJobButton;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private Enterprise.ZArchitecture.GUI.ZPanel bottomPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox filterGroupBox;
		private Enterprise.ZArchitecture.GUI.ZButton clearButton;
		private Enterprise.ZArchitecture.GUI.ZButton filterButton;
		private Enterprise.ZArchitecture.ZTextBox statusContainsTextBox;
		private Enterprise.ZArchitecture.ZTextBox notesContainsTextBox;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox addingUserCodeFindBox;
		private Enterprise.ZArchitecture.GUI.ZDateEdit endDateEdit;
		private Enterprise.ZArchitecture.GUI.ZDateEdit startDateEdit;
		internal Enterprise.ZArchitecture.GUI.ZButton RefreshButton;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo10 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo11 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo16 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo17 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo12 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo13 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo14 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo15 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			this.DocumentAllocationTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
#if !WINZOR
			this.previewTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.ImportBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.fileBrowserControl = new Enterprise.DocumentScanning.GUI.AllocateEDocs.FileBrowserUserControl();
			this.ImportConfigurationBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
#endif
			this.UnallocatedDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.UnallocatedDocumentsSpecificDocumentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.UnallocatedDocumentsAllDeletedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UnallocatedDocumentsDepartmentSpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UnallocatedDocumentsBranchSpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.UnallocatedDocumentsCompanySpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
#if !WINZOR
			this.importBoundButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
			this.importConfigurationBoundButton2 = new Enterprise.ZArchitecture.GUI.ZButton();
#endif
			this.filterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.clearButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.filterButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.statusContainsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.notesContainsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.addingUserCodeFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.endDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.startDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.CreateNewJobButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.UnallocatedGridControl = new Enterprise.DocumentScanning.GUI.BindableGridControl();
			this.AllocateBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AllocatedDocumentsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AllocatedDocumentsSpecificDocumentLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AllocatedDocumentsDepartmentSpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AllocatedDocumentsBranchSpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AllocatedDocumentsCompanySpecificCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AllocatedGridControl = new Enterprise.DocumentScanning.GUI.BindableGridControl();
			this.UnallocateBoundButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.GraphicalDisplayControlPanel = new CargoWise.Windows.UI.KPanel();
			this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.bottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DocumentAllocationTabControl.SuspendLayout();
#if !WINZOR
			this.previewTabPage.SuspendLayout();
			this.fileBrowserControl.SuspendLayout();
#endif
			this.UnallocatedDocumentsTabPage.SuspendLayout();
			this.filterGroupBox.SuspendLayout();
			this.addingUserCodeFindBox.SuspendLayout();
			this.endDateEdit.SuspendLayout();
			this.startDateEdit.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnallocatedGridControl.Grid)).BeginInit();
			this.UnallocatedGridControl.SuspendLayout();
			this.AllocatedDocumentsTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllocatedGridControl.Grid)).BeginInit();
			this.AllocatedGridControl.SuspendLayout();
			this.PostingButtonsUserControl.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.bottomPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 464, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 23, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(466);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(467);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.DocumentScanning.Business.AllocateDocumentsManager);
			// 
			// DocumentAllocationTabControl
			// 
			this.DocumentAllocationTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
#if !WINZOR
			this.DocumentAllocationTabControl.Controls.Add(this.previewTabPage);
#endif
			this.DocumentAllocationTabControl.Controls.Add(this.UnallocatedDocumentsTabPage);
			this.DocumentAllocationTabControl.Controls.Add(this.AllocatedDocumentsTabPage);
			this.DocumentAllocationTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.DocumentAllocationTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocumentAllocationTabControl.Name = "DocumentAllocationTabControl";
#if !WINZOR
			this.DocumentAllocationTabControl.SelectedIndex = 1;
#else
			this.DocumentAllocationTabControl.SelectedIndex = 0;
#endif
			this.DocumentAllocationTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(525, 487, true);
			this.DocumentAllocationTabControl.TabIndex = 0;
			this.DocumentAllocationTabControl.SelectedIndexChanged += new System.EventHandler(this.DocumentAllocationTabControl_SelectedIndexChanged);
#if !WINZOR
			// 
			// previewTabPage
			// 
			this.previewTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.previewTabPage.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("30937799-471a-4e4e-bf0b-976c7e55e3c9", "Preview");
			this.previewTabPage.Controls.Add(this.ImportBoundButton);
			this.previewTabPage.Controls.Add(this.fileBrowserControl);
			this.previewTabPage.Controls.Add(this.ImportConfigurationBoundButton);
			this.previewTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.previewTabPage.Name = "previewTabPage";
			this.previewTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.previewTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 465, true);
			this.previewTabPage.TabIndex = 2;
			// 
			// ImportBoundButton
			// 
			this.ImportBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportBoundButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|c8bcffcf-4852-47c9-a487-29877c48cc41", "&Import");
			this.ImportBoundButton.IsCaptionOverridden = false;
			this.ImportBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(293, 416, true);
			this.ImportBoundButton.Name = "ImportBoundButton";
			this.ImportBoundButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.ImportBoundButton.TabIndex = 3;
			this.ImportBoundButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ImportBoundButton.ToolTipCaption = null;
			this.ImportBoundButton.Click += new System.EventHandler(this.ImportBoundButton_Click);
			// 
			// fileBrowserControl
			// 
			this.fileBrowserControl.AllowDrop = true;
			this.fileBrowserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.fileBrowserControl, "FileImporterForImport.DefaultImportDirectory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).FileImporterForImport.DefaultImportDirectory)));
			this.fileBrowserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 3, true);
			this.fileBrowserControl.Name = "fileBrowserControl";
			this.fileBrowserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(516, 407, true);
			this.fileBrowserControl.TabIndex = 2;
			// 
			// ImportConfigurationBoundButton
			// 
			this.ImportConfigurationBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ImportConfigurationBoundButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|7b3f0294-545d-4680-8e96-864381ce9cec", "I&mport Configuration");
			this.ImportConfigurationBoundButton.ForeColor = System.Drawing.SystemColors.ControlText;
			this.ImportConfigurationBoundButton.IsCaptionOverridden = false;
			this.ImportConfigurationBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(374, 416, true);
			this.ImportConfigurationBoundButton.Name = "ImportConfigurationBoundButton";
			this.ImportConfigurationBoundButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ImportConfigurationBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 22, true);
			this.ImportConfigurationBoundButton.TabIndex = 4;
			this.ImportConfigurationBoundButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ImportConfigurationBoundButton.ToolTipCaption = null;
			this.ImportConfigurationBoundButton.Click += new System.EventHandler(this.ImportConfigurationBoundButton_Click);
#endif
			// 
			// UnallocatedDocumentsTabPage
			// 
			this.UnallocatedDocumentsTabPage.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|a6124b22-8f24-4349-8b81-7d595fb4cf00", "Unallocated eDocs");
			this.UnallocatedDocumentsTabPage.Controls.Add(this.UnallocatedDocumentsSpecificDocumentLabel);
			this.UnallocatedDocumentsTabPage.Controls.Add(this.UnallocatedDocumentsAllDeletedCheckBox);
			this.UnallocatedDocumentsTabPage.Controls.Add(this.UnallocatedDocumentsDepartmentSpecificCheckBox);
			this.UnallocatedDocumentsTabPage.Controls.Add(this.UnallocatedDocumentsBranchSpecificCheckBox);
			this.UnallocatedDocumentsTabPage.Controls.Add(this.UnallocatedDocumentsCompanySpecificCheckBox);
#if !WINZOR
			this.UnallocatedDocumentsTabPage.Controls.Add(this.importBoundButton2);
			this.UnallocatedDocumentsTabPage.Controls.Add(this.importConfigurationBoundButton2);
#endif
			this.UnallocatedDocumentsTabPage.Controls.Add(this.filterGroupBox);
			this.UnallocatedDocumentsTabPage.Controls.Add(this.CreateNewJobButton);
			this.UnallocatedDocumentsTabPage.Controls.Add(this.RefreshButton);
			this.UnallocatedDocumentsTabPage.Controls.Add(this.UnallocatedGridControl);
			this.UnallocatedDocumentsTabPage.Controls.Add(this.AllocateBoundButton);
			this.UnallocatedDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.UnallocatedDocumentsTabPage.Name = "UnallocatedDocumentsTabPage";
			this.UnallocatedDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 465, true);
			this.UnallocatedDocumentsTabPage.TabIndex = 0;
			// 
			// UnallocatedDocumentsSpecificDocumentLabel
			// 
			this.UnallocatedDocumentsSpecificDocumentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.UnallocatedDocumentsSpecificDocumentLabel.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("a91ee9aa-c152-4b14-b0e7-ccac02534376", "Show Documents for");
			this.UnallocatedDocumentsSpecificDocumentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.UnallocatedDocumentsSpecificDocumentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 385, true);
			this.UnallocatedDocumentsSpecificDocumentLabel.Name = "UnallocatedDocumentsSpecificDocumentLabel";
			this.UnallocatedDocumentsSpecificDocumentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 15, true);
			this.UnallocatedDocumentsSpecificDocumentLabel.TabIndex = 2;
			// 
			// UnallocatedDocumentsAllDeletedCheckBox
			// 
			this.UnallocatedDocumentsAllDeletedCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.UnallocatedDocumentsAllDeletedCheckBox, "ShowUnallocatedDocumentsForAllDeleted");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).ShowUnallocatedDocumentsForAllDeleted)));
			this.UnallocatedDocumentsAllDeletedCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("b9932b00-c3f6-4020-b799-0cb686c181f6", "All Deleted");
			this.UnallocatedDocumentsAllDeletedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(423, 383, true);
			this.UnallocatedDocumentsAllDeletedCheckBox.Name = "UnallocatedDocumentsAllDeletedCheckBox";
			this.UnallocatedDocumentsAllDeletedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 20, true);
			this.UnallocatedDocumentsAllDeletedCheckBox.TabIndex = 6;
			this.UnallocatedDocumentsAllDeletedCheckBox.UseVisualStyleBackColor = true;
			// 
			// UnallocatedDocumentsDepartmentSpecificCheckBox
			// 
			this.UnallocatedDocumentsDepartmentSpecificCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.UnallocatedDocumentsDepartmentSpecificCheckBox, "ShowUnallocatedDocumentsForAllDepartments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).ShowUnallocatedDocumentsForAllDepartments)));
			this.UnallocatedDocumentsDepartmentSpecificCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("f73b5ed7-1cc7-4645-bcb9-5b030f5db658", "All Departments");
			this.UnallocatedDocumentsDepartmentSpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnallocatedDocumentsDepartmentSpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(312, 383, true);
			this.UnallocatedDocumentsDepartmentSpecificCheckBox.Name = "UnallocatedDocumentsDepartmentSpecificCheckBox";
			this.UnallocatedDocumentsDepartmentSpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(107, 20, true);
			this.UnallocatedDocumentsDepartmentSpecificCheckBox.TabIndex = 5;
			this.UnallocatedDocumentsDepartmentSpecificCheckBox.UseVisualStyleBackColor = true;
			// 
			// UnallocatedDocumentsBranchSpecificCheckBox
			// 
			this.UnallocatedDocumentsBranchSpecificCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.UnallocatedDocumentsBranchSpecificCheckBox, "ShowUnallocatedDocumentsForAllBranches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).ShowUnallocatedDocumentsForAllBranches)));
			this.UnallocatedDocumentsBranchSpecificCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("7ba30349-1665-4853-a510-5c4f11894312", "All Branches");
			this.UnallocatedDocumentsBranchSpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnallocatedDocumentsBranchSpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(214, 383, true);
			this.UnallocatedDocumentsBranchSpecificCheckBox.Name = "UnallocatedDocumentsBranchSpecificCheckBox";
			this.UnallocatedDocumentsBranchSpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.UnallocatedDocumentsBranchSpecificCheckBox.TabIndex = 4;
			this.UnallocatedDocumentsBranchSpecificCheckBox.UseVisualStyleBackColor = false;
			// 
			// UnallocatedDocumentsCompanySpecificCheckBox
			// 
			this.UnallocatedDocumentsCompanySpecificCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.UnallocatedDocumentsCompanySpecificCheckBox, "ShowUnallocatedDocumentsForAllCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).ShowUnallocatedDocumentsForAllCompanies)));
			this.UnallocatedDocumentsCompanySpecificCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("3dc6bc98-0056-488d-b561-2c6b0370d3ec", "All Companies");
			this.UnallocatedDocumentsCompanySpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.UnallocatedDocumentsCompanySpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(115, 383, true);
			this.UnallocatedDocumentsCompanySpecificCheckBox.Name = "UnallocatedDocumentsCompanySpecificCheckBox";
			this.UnallocatedDocumentsCompanySpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.UnallocatedDocumentsCompanySpecificCheckBox.TabIndex = 3;
			this.UnallocatedDocumentsCompanySpecificCheckBox.UseVisualStyleBackColor = true;
#if !WINZOR
			// 
			// importBoundButton2
			// 
			this.importBoundButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.importBoundButton2.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("901352e5-ca20-4e99-8046-813976aa2e3c", "Import");
			this.importBoundButton2.IsCaptionOverridden = false;
			this.importBoundButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 413, true);
			this.importBoundButton2.Name = "importBoundButton2";
			this.importBoundButton2.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.importBoundButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.importBoundButton2.TabIndex = 6;
			this.importBoundButton2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.importBoundButton2.ToolTipCaption = null;
			// 
			// importConfigurationBoundButton2
			// 
			this.importConfigurationBoundButton2.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.importConfigurationBoundButton2.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("1f4f1bc0-ce3e-4881-a2a1-d7a9ba6c984f", "Import Configuration");
			this.importConfigurationBoundButton2.ForeColor = System.Drawing.SystemColors.ControlText;
			this.importConfigurationBoundButton2.IsCaptionOverridden = false;
			this.importConfigurationBoundButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 413, true);
			this.importConfigurationBoundButton2.Name = "importConfigurationBoundButton2";
			this.importConfigurationBoundButton2.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.importConfigurationBoundButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(136, 22, true);
			this.importConfigurationBoundButton2.TabIndex = 7;
			this.importConfigurationBoundButton2.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.importConfigurationBoundButton2.ToolTipCaption = null;
#endif
			// 
			// filterGroupBox
			// 
			this.filterGroupBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("4469f1ab-0890-4b70-964d-bcedda855094", "Unallocated eDocs Filter");
			this.filterGroupBox.Controls.Add(this.clearButton);
			this.filterGroupBox.Controls.Add(this.filterButton);
			this.filterGroupBox.Controls.Add(this.statusContainsTextBox);
			this.filterGroupBox.Controls.Add(this.notesContainsTextBox);
			this.filterGroupBox.Controls.Add(this.addingUserCodeFindBox);
			this.filterGroupBox.Controls.Add(this.endDateEdit);
			this.filterGroupBox.Controls.Add(this.startDateEdit);
			this.filterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.filterGroupBox.Name = "filterGroupBox";
			this.filterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 104, true);
			this.filterGroupBox.TabIndex = 0;
			this.filterGroupBox.TabStop = false;
			// 
			// clearButton
			// 
			this.clearButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("e8ee00b9-1784-4291-a35f-029479b00fb7", "Clear");
			this.clearButton.IsCaptionOverridden = false;
			this.clearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 45, true);
			this.clearButton.Name = "clearButton";
			this.clearButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.clearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.clearButton.TabIndex = 6;
			this.clearButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.clearButton.ToolTipCaption = null;
			this.clearButton.UseVisualStyleBackColor = true;
			this.clearButton.Click += new System.EventHandler(this.clearButton_Click);
			// 
			// filterButton
			// 
			this.filterButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("af5c99fa-4ea6-4603-9787-dd62780ba170", "Find");
			this.filterButton.IsCaptionOverridden = false;
			this.filterButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(393, 16, true);
			this.filterButton.Name = "filterButton";
			this.filterButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.filterButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.filterButton.TabIndex = 5;
			this.filterButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.filterButton.ToolTipCaption = null;
			this.filterButton.UseVisualStyleBackColor = true;
			this.filterButton.Click += new System.EventHandler(this.filterButton_Click);
			// 
			// statusContainsTextBox
			// 
			this.BindingSource.SetBindingMember(this.statusContainsTextBox, "AllocationStatusForFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).AllocationStatusForFilter)));
			this.statusContainsTextBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("9be78461-b9b0-4808-a783-3d5a3b5f4d9e", "Status Contains");
			this.statusContainsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(287, 71, true);
			this.statusContainsTextBox.Name = "statusContainsTextBox";
			this.statusContainsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.statusContainsTextBox.TabIndex = 4;
			// 
			// notesContainsTextBox
			// 
			this.BindingSource.SetBindingMember(this.notesContainsTextBox, "AllocationNotesForFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).AllocationNotesForFilter)));
			this.notesContainsTextBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("b1194764-142f-4c38-9824-eda58dafaccf", "Notes Contains");
			this.notesContainsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 71, true);
			this.notesContainsTextBox.Name = "notesContainsTextBox";
			this.notesContainsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.notesContainsTextBox.TabIndex = 3;
			// 
			// addingUserCodeFindBox
			// 
			this.addingUserCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.addingUserCodeFindBox, "AddingUserForFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).AddingUserForFilter)));
			this.addingUserCodeFindBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("bfa7511e-29fb-4fc6-8885-b9c4d819c9e6", "Adding User");
			this.addingUserCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(95, 45, true);
			this.addingUserCodeFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.addingUserCodeFindBox.Name = "addingUserCodeFindBox";
			this.addingUserCodeFindBox.ShouldResize = true;
			this.addingUserCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 17, true);
			this.addingUserCodeFindBox.TabIndex = 2;
			// 
			// endDateEdit
			// 
			this.endDateEdit.AllowDrop = true;
			this.endDateEdit.AutoCompleteMonthThreshold = 1;
			this.endDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.endDateEdit, "EndDateForFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).EndDateForFilter)));
			this.endDateEdit.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("5b39de15-f35e-4473-b46e-35c8b6d96937", "To");
			this.endDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 19, true);
			this.endDateEdit.Name = "endDateEdit";
			this.endDateEdit.TabIndex = 1;
			// 
			// startDateEdit
			// 
			this.startDateEdit.AllowDrop = true;
			this.startDateEdit.AutoCompleteMonthThreshold = 1;
			this.startDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.startDateEdit, "StartDateForFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).StartDateForFilter)));
			this.startDateEdit.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("f865afa5-f9d5-45e5-9d10-c55226d17c70", "Scan Date (UTC) From");
			this.startDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(135, 19, true);
			this.startDateEdit.Name = "startDateEdit";
			this.startDateEdit.TabIndex = 0;
			// 
			// CreateNewJobButton
			// 
			this.CreateNewJobButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CreateNewJobButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|5FF8B36E-9B13-4432-B6FE-D3638EADD6E5", "&Create New Job");
			this.CreateNewJobButton.IsCaptionOverridden = false;
			this.CreateNewJobButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(250, 413, true);
			this.CreateNewJobButton.Name = "CreateNewJobButton";
			this.CreateNewJobButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CreateNewJobButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 22, true);
			this.CreateNewJobButton.TabIndex = 8;
			this.CreateNewJobButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CreateNewJobButton.ToolTipCaption = null;
			this.CreateNewJobButton.Click += new System.EventHandler(this.CreateNewJobButton_Click);
			// 
			// RefreshButton
			// 
			this.RefreshButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.RefreshButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|69a2b143-17cb-43b4-8f20-77d90a8ffbe1", "&Refresh");
			this.RefreshButton.IsCaptionOverridden = false;
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(433, 413, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.RefreshButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.RefreshButton.TabIndex = 10;
			this.RefreshButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.RefreshButton.ToolTipCaption = null;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// UnallocatedGridControl
			// 
			this.UnallocatedGridControl.AllowDrop = true;
			this.UnallocatedGridControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.UnallocatedGridControl.BindTo = "UnallocatedDocumentsView";
			// 
			// 
			// 
			this.UnallocatedGridControl.Grid.AllowDrop = true;
			this.UnallocatedGridControl.Grid.AllowNavigation = false;
			this.UnallocatedGridControl.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|9926a49c-2723-49cb-a834-e647446b7d30", "File Name");
			zTextBoxColumnStyleInfo12.ColumnName = "SC_FileName";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo3.ColumnName = "SC_Date";
			zDateEditColumnStyleInfo3.IsReadOnly = true;
			zDateEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zDropEditColumnStyleInfo10.BindToList = "SC_FormCategory_List";
			zDropEditColumnStyleInfo10.ColumnName = "SM_Type";
			zDropEditColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo11.BindToList = "SC_DocType_List";
			zDropEditColumnStyleInfo11.ColumnName = "SC_DocType";
			zDropEditColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|A1CE6232-2AEC-4BE6-AEB8-ABCAC8B354A7", "Scanned Barcode");
			zTextBoxColumnStyleInfo13.ColumnName = "ScannedBarcodeValue";
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo14.ColumnName = "SC_DescMultilingual";
			zTextBoxColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo3.BindToList = "SC_ParentID_List";
			zGuidFindBoxColumnStyleInfo3.ColumnName = "SC_ParentID";
			zGuidFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo15.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("BindableGridControl|0ed1b8e0-bfad-4aad-9bec-09c02050dd1c", "Adding User");
			zTextBoxColumnStyleInfo15.ColumnName = "SC_AddingUser";
			zTextBoxColumnStyleInfo15.IsReadOnly = true;
			zTextBoxColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo16.ColumnName = "AllocationStatus";
			zTextBoxColumnStyleInfo16.IsReadOnly = true;
			zTextBoxColumnStyleInfo16.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo17.ColumnName = "AllocationNotes";
			zTextBoxColumnStyleInfo17.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo5.ColumnName = "SC_IsPublished";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo6.ColumnName = "SC_IsSystemGenerated";
			zCheckBoxColumnStyleInfo6.IsReadOnly = true;
			zCheckBoxColumnStyleInfo6.IsVisible = false;
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo12.BindToList = "SC_DocSource_List";
			zDropEditColumnStyleInfo12.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|e5267e09-9599-47c8-9231-1d9be61679bd", "Document Source");
			zDropEditColumnStyleInfo12.ColumnName = "SC_RDS_NKDocSource";
			zDropEditColumnStyleInfo12.IsVisible = false;
			zDropEditColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zDropEditColumnStyleInfo13.BindToList = "GlbCompanyList";
			zDropEditColumnStyleInfo13.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("1c745a93-4af1-4593-b1ac-d8f5ac1c7096", "Company");
			zDropEditColumnStyleInfo13.ColumnName = "CompanyCode";
			zDropEditColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo14.BindToList = "GlbBranchList";
			zDropEditColumnStyleInfo14.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("72531b51-e300-4698-989e-3a419ff0ea7c", "Branch");
			zDropEditColumnStyleInfo14.ColumnName = "BranchCode";
			zDropEditColumnStyleInfo14.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo15.BindToList = "GlbDepartmentList";
			zDropEditColumnStyleInfo15.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("b2662cb5-b997-4c6f-ba00-78276aade338", "Department");
			zDropEditColumnStyleInfo15.ColumnName = "DepartmentCode";
			zDropEditColumnStyleInfo15.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo10);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo11);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo3);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo16);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo17);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo12);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo13);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo14);
			this.UnallocatedGridControl.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo15);
			this.UnallocatedGridControl.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.UnallocatedGridControl.Grid.DocumentManipulationTarget = null;
			this.UnallocatedGridControl.Grid.DragDropTarget = null;
			this.UnallocatedGridControl.Grid.GridId = "f04d1e6b-53dd-4654-b146-a7055262ae8d";
			this.UnallocatedGridControl.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.UnallocatedGridControl.Grid.LayoutKey = "documentsZGrid1";
			this.UnallocatedGridControl.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UnallocatedGridControl.Grid.Name = "InnerGrid";
			this.UnallocatedGridControl.Grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.UnallocatedGridControl.Grid.ShowAllocateMenuItem = true;
			this.UnallocatedGridControl.Grid.ShowCopyLinkMenuItem = true;
			this.UnallocatedGridControl.Grid.ShowCopyMenuItem = true;
			this.UnallocatedGridControl.Grid.ShowCutMenuItem = true;
			this.UnallocatedGridControl.Grid.ShowDeleteMenuItem = true;
			this.UnallocatedGridControl.Grid.ShowDeletePermanentlyMenuItem = true;
			this.UnallocatedGridControl.Grid.ShowDeliverDocumentMenuItem = true;
			this.UnallocatedGridControl.Grid.ShowPasteMenuItem = true;
			this.UnallocatedGridControl.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 259, true);
			this.UnallocatedGridControl.Grid.TabIndex = 0;
			this.UnallocatedGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 113, true);
			this.UnallocatedGridControl.Name = "UnallocatedGridControl";
			this.UnallocatedGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 259, true);
			this.UnallocatedGridControl.TabIndex = 1;
			// 
			// AllocateBoundButton
			// 
			this.AllocateBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.AllocateBoundButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|3724724a-7129-4f36-9fc0-16b79e7e0a0c", "&Allocate");
			this.AllocateBoundButton.IsCaptionOverridden = false;
			this.AllocateBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 413, true);
			this.AllocateBoundButton.Name = "AllocateBoundButton";
			this.AllocateBoundButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AllocateBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.AllocateBoundButton.TabIndex = 9;
			this.AllocateBoundButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AllocateBoundButton.ToolTipCaption = null;
			this.AllocateBoundButton.Click += new System.EventHandler(this.AllocateBoundButton_Click);
			// 
			// AllocatedDocumentsTabPage
			// 
			this.AllocatedDocumentsTabPage.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|0af086b3-495a-4bd7-a9ee-a97eaacb2404", "Allocated eDocs");
			this.AllocatedDocumentsTabPage.Controls.Add(this.AllocatedDocumentsSpecificDocumentLabel);
			this.AllocatedDocumentsTabPage.Controls.Add(this.AllocatedDocumentsDepartmentSpecificCheckBox);
			this.AllocatedDocumentsTabPage.Controls.Add(this.AllocatedDocumentsBranchSpecificCheckBox);
			this.AllocatedDocumentsTabPage.Controls.Add(this.AllocatedDocumentsCompanySpecificCheckBox);
			this.AllocatedDocumentsTabPage.Controls.Add(this.AllocatedGridControl);
			this.AllocatedDocumentsTabPage.Controls.Add(this.UnallocateBoundButton);
			this.AllocatedDocumentsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 19, true);
			this.AllocatedDocumentsTabPage.Name = "AllocatedDocumentsTabPage";
			this.AllocatedDocumentsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 465, true);
			this.AllocatedDocumentsTabPage.TabIndex = 1;
			// 
			// AllocatedDocumentsSpecificDocumentLabel
			// 
			this.AllocatedDocumentsSpecificDocumentLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.AllocatedDocumentsSpecificDocumentLabel.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("5a60d00e-29f9-4cc9-9701-6ecd461e1c3f", "Show Documents for");
			this.AllocatedDocumentsSpecificDocumentLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.AllocatedDocumentsSpecificDocumentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, 415, true);
			this.AllocatedDocumentsSpecificDocumentLabel.Name = "AllocatedDocumentsSpecificDocumentLabel";
			this.AllocatedDocumentsSpecificDocumentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(109, 15, true);
			this.AllocatedDocumentsSpecificDocumentLabel.TabIndex = 1;
			// 
			// AllocatedDocumentsDepartmentSpecificCheckBox
			// 
			this.AllocatedDocumentsDepartmentSpecificCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.AllocatedDocumentsDepartmentSpecificCheckBox, "ShowAllocatedDocumentsForAllDepartments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).ShowAllocatedDocumentsForAllDepartments)));
			this.AllocatedDocumentsDepartmentSpecificCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("fbb3161c-bb90-417b-af94-be9ad2c034ce", "All Departments");
			this.AllocatedDocumentsDepartmentSpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllocatedDocumentsDepartmentSpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 413, true);
			this.AllocatedDocumentsDepartmentSpecificCheckBox.Name = "AllocatedDocumentsDepartmentSpecificCheckBox";
			this.AllocatedDocumentsDepartmentSpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 20, true);
			this.AllocatedDocumentsDepartmentSpecificCheckBox.TabIndex = 4;
			this.AllocatedDocumentsDepartmentSpecificCheckBox.UseVisualStyleBackColor = true;
			// 
			// AllocatedDocumentsBranchSpecificCheckBox
			// 
			this.AllocatedDocumentsBranchSpecificCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.AllocatedDocumentsBranchSpecificCheckBox, "ShowAllocatedDocumentsForAllBranches");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).ShowAllocatedDocumentsForAllBranches)));
			this.AllocatedDocumentsBranchSpecificCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("84afadbc-81e6-4da3-b65a-5beefefef282", "All Branches");
			this.AllocatedDocumentsBranchSpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllocatedDocumentsBranchSpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(215, 413, true);
			this.AllocatedDocumentsBranchSpecificCheckBox.Name = "AllocatedDocumentsBranchSpecificCheckBox";
			this.AllocatedDocumentsBranchSpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(94, 20, true);
			this.AllocatedDocumentsBranchSpecificCheckBox.TabIndex = 3;
			this.AllocatedDocumentsBranchSpecificCheckBox.UseVisualStyleBackColor = false;
			// 
			// AllocatedDocumentsCompanySpecificCheckBox
			// 
			this.AllocatedDocumentsCompanySpecificCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.BindingSource.SetBindingMember(this.AllocatedDocumentsCompanySpecificCheckBox, "ShowAllocatedDocumentsForAllCompanies");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.DocumentScanning.Business.AllocateDocumentsManager)(null)).ShowAllocatedDocumentsForAllCompanies)));
			this.AllocatedDocumentsCompanySpecificCheckBox.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("fc3598b3-97d2-46db-b00c-986891580511", "All Companies");
			this.AllocatedDocumentsCompanySpecificCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllocatedDocumentsCompanySpecificCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 413, true);
			this.AllocatedDocumentsCompanySpecificCheckBox.Name = "AllocatedDocumentsCompanySpecificCheckBox";
			this.AllocatedDocumentsCompanySpecificCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(95, 20, true);
			this.AllocatedDocumentsCompanySpecificCheckBox.TabIndex = 2;
			this.AllocatedDocumentsCompanySpecificCheckBox.UseVisualStyleBackColor = true;
			// 
			// AllocatedGridControl
			// 
			this.AllocatedGridControl.AllowDrop = true;
			this.AllocatedGridControl.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AllocatedGridControl.BindTo = "AllocatedDocumentsView";
			// 
			// 
			// 
			this.AllocatedGridControl.Grid.AllowDrop = true;
			this.AllocatedGridControl.Grid.AllowNavigation = false;
			this.AllocatedGridControl.Grid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|9926a49c-2723-49cb-a834-e647446b7d30", "File Name");
			zTextBoxColumnStyleInfo1.ColumnName = "SC_FileName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDateEditColumnStyleInfo1.ColumnName = "SC_Date";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.ColumnName = "SM_Type";
			zTextBoxColumnStyleInfo2.IsReadOnly = true;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "SC_DocType";
			zTextBoxColumnStyleInfo3.IsReadOnly = true;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zTextBoxColumnStyleInfo4.ColumnName = "SC_DescMultilingual";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zGuidFindBoxColumnStyleInfo1.BindToList = "ParentMain.SM_ParentFK_List";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ParentMain+SM_ParentFK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zGuidFindBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|41a3fb61-c661-4527-971f-6f1e8eecefc9", "Adding User");
			zTextBoxColumnStyleInfo5.ColumnName = "SC_AddingUser";
			zTextBoxColumnStyleInfo5.IsReadOnly = true;
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "SC_IsPublished";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "SC_IsSystemGenerated";
			zCheckBoxColumnStyleInfo2.IsReadOnly = true;
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo1.BindToList = "GlbCompanyList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("1c745a93-4af1-4593-b1ac-d8f5ac1c7096", "Company");
			zDropEditColumnStyleInfo1.ColumnName = "CompanyCode";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo2.BindToList = "GlbBranchList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("5b2780f9-3693-45fa-a005-9662abf9e3ca", "Branch");
			zDropEditColumnStyleInfo2.ColumnName = "BranchCode";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zDropEditColumnStyleInfo3.BindToList = "GlbDepartmentList";
			zDropEditColumnStyleInfo3.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("3af8f990-7099-4b8c-b363-3ff2b22ea926", "Department");
			zDropEditColumnStyleInfo3.ColumnName = "DepartmentCode";
			zDropEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.AllocatedGridControl.Grid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.AllocatedGridControl.Grid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AllocatedGridControl.Grid.DocumentManipulationTarget = null;
			this.AllocatedGridControl.Grid.DragDropTarget = null;
			this.AllocatedGridControl.Grid.GridId = "f04d1e6b-53dd-4654-b146-a7055262ae8d";
			this.AllocatedGridControl.Grid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.AllocatedGridControl.Grid.LayoutKey = "documentsZGrid1";
			this.AllocatedGridControl.Grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.AllocatedGridControl.Grid.Name = "InnerGrid";
			this.AllocatedGridControl.Grid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.NoRemovePossible;
			this.AllocatedGridControl.Grid.ShowCopyLinkMenuItem = true;
			this.AllocatedGridControl.Grid.ShowCopyMenuItem = true;
			this.AllocatedGridControl.Grid.ShowCutMenuItem = true;
			this.AllocatedGridControl.Grid.ShowDeleteMenuItem = true;
			this.AllocatedGridControl.Grid.ShowDeliverDocumentMenuItem = true;
			this.AllocatedGridControl.Grid.ShowEditPropertiesMenuItem = true;
			this.AllocatedGridControl.Grid.ShowUnallocateMenuItem = true;
			this.AllocatedGridControl.Grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 408, true);
			this.AllocatedGridControl.Grid.TabIndex = 0;
			this.AllocatedGridControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 0, true);
			this.AllocatedGridControl.Name = "AllocatedGridControl";
			this.AllocatedGridControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(509, 408, true);
			this.AllocatedGridControl.TabIndex = 0;
			// 
			// UnallocateBoundButton
			// 
			this.UnallocateBoundButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.UnallocateBoundButton.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|c0902693-bf1b-47ff-8e2a-dbb44d020533", "&Unallocate");
			this.UnallocateBoundButton.IsCaptionOverridden = false;
			this.UnallocateBoundButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(438, 412, true);
			this.UnallocateBoundButton.Name = "UnallocateBoundButton";
			this.UnallocateBoundButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.UnallocateBoundButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 22, true);
			this.UnallocateBoundButton.TabIndex = 5;
			this.UnallocateBoundButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.UnallocateBoundButton.ToolTipCaption = null;
			this.UnallocateBoundButton.Click += new System.EventHandler(this.UnallocateBoundButton_Click);
			// 
			// GraphicalDisplayControlPanel
			// 
			this.GraphicalDisplayControlPanel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.GraphicalDisplayControlPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GraphicalDisplayControlPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GraphicalDisplayControlPanel.Name = "GraphicalDisplayControlPanel";
			this.GraphicalDisplayControlPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 487, true);
			this.GraphicalDisplayControlPanel.TabIndex = 0;
			// 
			// PostingButtonsUserControl
			// 
			this.PostingButtonsUserControl.AllowDrop = true;
			this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(686, 2, true);
			this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
			this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
			this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 26, true);
			this.PostingButtonsUserControl.TabIndex = 2;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.GraphicalDisplayControlPanel);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.DocumentAllocationTabControl);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 487, true);
			this.splitContainer1.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(500);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(420);
			this.splitContainer1.TabIndex = 3;
			// 
			// bottomPanel
			// 
			this.bottomPanel.Controls.Add(this.PostingButtonsUserControl);
			this.bottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.bottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 487, true);
			this.bottomPanel.Name = "bottomPanel";
			this.bottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 28, true);
			this.bottomPanel.TabIndex = 4;
			// 
			// ZAllocateDocumentsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.DocumentScanning.GUI.Res.GetData("ZAllocateDocumentsForm|eb26404b-1545-4000-858b-8552402e9fed", "Allocate eDocs");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(948, 515, true);
			this.Controls.Add(this.splitContainer1);
			this.Controls.Add(this.bottomPanel);
			this.DataSourceAssemblyName = "DocumentScanning";
			this.DataSourceType = typeof(Enterprise.DocumentScanning.Business.AllocateDocumentsManager);
			this.DataSourceTypeName = "Enterprise.DocumentScanning.Business.AllocateDocumentsManager";
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(700, 427, true);
			this.Name = "ZAllocateDocumentsForm";
			this.Controls.SetChildIndex(this.bottomPanel, 0);
			this.Controls.SetChildIndex(this.splitContainer1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.DocumentAllocationTabControl.ResumeLayout(false);
			this.DocumentAllocationTabControl.PerformLayout();
#if !WINZOR
			this.previewTabPage.ResumeLayout(false);
			this.previewTabPage.PerformLayout();
			this.fileBrowserControl.ResumeLayout(true);
			this.fileBrowserControl.PerformLayout();
#endif
			this.UnallocatedDocumentsTabPage.ResumeLayout(false);
			this.UnallocatedDocumentsTabPage.PerformLayout();
			this.filterGroupBox.ResumeLayout(false);
			this.filterGroupBox.PerformLayout();
			this.addingUserCodeFindBox.ResumeLayout(true);
			this.addingUserCodeFindBox.PerformLayout();
			this.endDateEdit.ResumeLayout(true);
			this.endDateEdit.PerformLayout();
			this.startDateEdit.ResumeLayout(true);
			this.startDateEdit.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.UnallocatedGridControl.Grid)).EndInit();
			this.UnallocatedGridControl.ResumeLayout(true);
			this.UnallocatedGridControl.PerformLayout();
			this.AllocatedDocumentsTabPage.ResumeLayout(false);
			this.AllocatedDocumentsTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.AllocatedGridControl.Grid)).EndInit();
			this.AllocatedGridControl.ResumeLayout(true);
			this.AllocatedGridControl.PerformLayout();
			this.PostingButtonsUserControl.ResumeLayout(true);
			this.PostingButtonsUserControl.PerformLayout();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.splitContainer1.PerformLayout();
			this.bottomPanel.ResumeLayout(false);
			this.bottomPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

#endregion

#if !WINZOR
		private ZArchitecture.GUI.ZTabPage previewTabPage;
		private AllocateEDocs.FileBrowserUserControl fileBrowserControl;
		private ZArchitecture.GUI.ZButton importBoundButton2;
		private ZArchitecture.GUI.ZButton importConfigurationBoundButton2;
#endif
		internal ZArchitecture.GUI.ZCheckBox UnallocatedDocumentsCompanySpecificCheckBox;
		internal ZArchitecture.GUI.ZCheckBox UnallocatedDocumentsBranchSpecificCheckBox;
		internal ZArchitecture.ZLabel UnallocatedDocumentsSpecificDocumentLabel;
		internal ZArchitecture.GUI.ZCheckBox UnallocatedDocumentsDepartmentSpecificCheckBox;
		internal ZArchitecture.ZLabel AllocatedDocumentsSpecificDocumentLabel;
		internal ZArchitecture.GUI.ZCheckBox AllocatedDocumentsDepartmentSpecificCheckBox;
		internal ZArchitecture.GUI.ZCheckBox AllocatedDocumentsBranchSpecificCheckBox;
		internal ZArchitecture.GUI.ZCheckBox AllocatedDocumentsCompanySpecificCheckBox;
		internal ZArchitecture.GUI.ZCheckBox UnallocatedDocumentsAllDeletedCheckBox;
	}
}
