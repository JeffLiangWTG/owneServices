using System;
using System.IO;
using System.Windows.Forms;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using Enterprise.Accounting.DataTransfer.GLJournals;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.GeneralLedger.GLJournals
{
	public partial class CsvExportGLTransactionForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		System.ComponentModel.Container components = null;

		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.PrimaryFiltersGroupBox = new ZGroupBox();
			this.ToPeriodPeriodEdit = new ZPeriodEdit();
			this.FromPeriodPeriodEdit = new ZPeriodEdit();
			this.DescriptionDisplayDropEdit = new ZDropEdit();
			this.DepartmentPKGuidFindBox = new ZGuidFindBox();
			this.BranchPKGuidFindBox = new ZGuidFindBox();
			this.EndGLAccountGuidFindBox = new ZGuidFindBox();
			this.StartGLAccountGuidFindBox = new ZGuidFindBox();
			this.ToDateDateEdit = new ZDateEdit();
			this.FromDateDateEdit = new ZDateEdit();
			this.SortByGroupBox = new ZGroupBox();
			this.SourceRadioButton = new ZRadioButton();
			this.PeriodRadioButton = new ZRadioButton();
			this.CancelButton = new ZButton();
			this.ExportButton = new ZButton();
			this.LogTextBox = new ZTextBox();
			this.DirectoryGroupBox = new ZGroupBox();
			this.BrowseButton = new ZButton();
			this.ExportDirectoryTextBox = new ZTextBox();
			this.BatchGroupBox = new ZGroupBox();
			this.BatchNumberCalcEdit = new ZCalcEdit();
			this.ExportExistingBatchCheckBox = new ZCheckBox();
			this.CreateAndExportBatchCheckBox = new ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.PrimaryFiltersGroupBox.SuspendLayout();
			this.SortByGroupBox.SuspendLayout();
			this.DirectoryGroupBox.SuspendLayout();
			this.BatchGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 531, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 24, true);
			this.MainStatusBar.SizingGrip = false;
			this.MainStatusBar.TabIndex = 7;
			this.MainStatusBar.Visible = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(292);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(GLTransactionBusinessObject);
			// 
			// PrimaryFiltersGroupBox
			// 
			this.PrimaryFiltersGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.PrimaryFiltersGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|62d0f26e-de7d-46a2-873f-a043dcf9cca3", "Primary Filters");
			this.PrimaryFiltersGroupBox.Controls.Add(this.ToPeriodPeriodEdit);
			this.PrimaryFiltersGroupBox.Controls.Add(this.FromPeriodPeriodEdit);
			this.PrimaryFiltersGroupBox.Controls.Add(this.DescriptionDisplayDropEdit);
			this.PrimaryFiltersGroupBox.Controls.Add(this.DepartmentPKGuidFindBox);
			this.PrimaryFiltersGroupBox.Controls.Add(this.BranchPKGuidFindBox);
			this.PrimaryFiltersGroupBox.Controls.Add(this.EndGLAccountGuidFindBox);
			this.PrimaryFiltersGroupBox.Controls.Add(this.StartGLAccountGuidFindBox);
			this.PrimaryFiltersGroupBox.Controls.Add(this.ToDateDateEdit);
			this.PrimaryFiltersGroupBox.Controls.Add(this.FromDateDateEdit);
			this.PrimaryFiltersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 12, true);
			this.PrimaryFiltersGroupBox.Name = "PrimaryFiltersGroupBox";
			this.PrimaryFiltersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 193, true);
			this.PrimaryFiltersGroupBox.TabIndex = 0;
			this.PrimaryFiltersGroupBox.TabStop = false;
			// 
			// ToPeriodPeriodEdit
			// 
			this.BindingSource.SetBindingMember(this.ToPeriodPeriodEdit, "ToPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((GLTransactionBusinessObject)(null)).ToPeriod)));
			this.ToPeriodPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|3c26d1e8-e0c4-4cb1-b9af-f116e71973c3", "To");
			this.ToPeriodPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 20, true);
			this.ToPeriodPeriodEdit.Name = "ToPeriodPeriodEdit";
			this.ToPeriodPeriodEdit.TabIndex = 3;
			// 
			// FromPeriodPeriodEdit
			// 
			this.BindingSource.SetBindingMember(this.FromPeriodPeriodEdit, "FromPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((GLTransactionBusinessObject)(null)).FromPeriod)));
			this.FromPeriodPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|7f2537fd-76b0-4a10-87f2-f577b73caed3", "Period Range From");
			this.FromPeriodPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.FromPeriodPeriodEdit.Name = "FromPeriodPeriodEdit";
			this.FromPeriodPeriodEdit.TabIndex = 1;
			// 
			// DescriptionDisplayDropEdit
			// 
			this.BindingSource.SetBindingMember(this.DescriptionDisplayDropEdit, "DescriptionDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GLTransactionBusinessObject)(null)).DescriptionDisplay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLTransactionBusinessObject)(null)).DescriptionDisplayList)));
			this.DescriptionDisplayDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|11b72dfd-1675-4869-943e-84b1c556d5e2", "Description Display");
			this.DescriptionDisplayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 166, true);
			this.DescriptionDisplayDropEdit.Name = "DescriptionDisplayDropEdit";
			this.DescriptionDisplayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.DescriptionDisplayDropEdit.TabIndex = 17;
			// 
			// DepartmentPKGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.DepartmentPKGuidFindBox, "DepartmentPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GLTransactionBusinessObject)(null)).DepartmentPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLTransactionBusinessObject)(null)).DepartmentList)));
			this.DepartmentPKGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|9e9ebc0d-2cba-429e-ab19-737208d2db89", "Department");
			this.DepartmentPKGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 139, true);
			this.DepartmentPKGuidFindBox.Name = "DepartmentPKGuidFindBox";
			this.DepartmentPKGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.DepartmentPKGuidFindBox.TabIndex = 15;
			// 
			// BranchPKGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.BranchPKGuidFindBox, "BranchPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GLTransactionBusinessObject)(null)).BranchPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLTransactionBusinessObject)(null)).BranchList)));
			this.BranchPKGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|80611533-a304-4828-9a9c-b74ba1428fd4", "Branch");
			this.BranchPKGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 115, true);
			this.BranchPKGuidFindBox.Name = "BranchPKGuidFindBox";
			this.BranchPKGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.BranchPKGuidFindBox.TabIndex = 13;
			// 
			// EndGLAccountGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.EndGLAccountGuidFindBox, "EndGLAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GLTransactionBusinessObject)(null)).EndGLAccountPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLTransactionBusinessObject)(null)).GLAccountList)));
			this.EndGLAccountGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|4d8205ef-97dd-4546-aa5d-453565051356", "End GL Account");
			this.EndGLAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 91, true);
			this.EndGLAccountGuidFindBox.Name = "EndGLAccountGuidFindBox";
			this.EndGLAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.EndGLAccountGuidFindBox.TabIndex = 11;
			// 
			// StartGLAccountGuidFindBox
			// 
			this.BindingSource.SetBindingMember(this.StartGLAccountGuidFindBox, "StartGLAccountPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((GLTransactionBusinessObject)(null)).StartGLAccountPK)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((GLTransactionBusinessObject)(null)).GLAccountList)));
			this.StartGLAccountGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|2df79833-5409-475c-b114-16fa824b3dac", "Start GL Account");
			this.StartGLAccountGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 67, true);
			this.StartGLAccountGuidFindBox.Name = "StartGLAccountGuidFindBox";
			this.StartGLAccountGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.StartGLAccountGuidFindBox.TabIndex = 9;
			// 
			// ToDateDateEdit
			// 
			this.ToDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.ToDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ToDateDateEdit, "ToDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GLTransactionBusinessObject)(null)).ToDate)));
			this.ToDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|ce69fe1d-bec3-4b02-b69f-75d9503939e8", "To");
			this.ToDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(286, 43, true);
			this.ToDateDateEdit.Name = "ToDateDateEdit";
			this.ToDateDateEdit.TabIndex = 7;
			// 
			// FromDateDateEdit
			// 
			this.FromDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.FromDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.FromDateDateEdit, "FromDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((GLTransactionBusinessObject)(null)).FromDate)));
			this.FromDateDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|a4cf0fb6-ba86-4ba3-ab44-7e9b9ccad5cf", "Date Range From");
			this.FromDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 43, true);
			this.FromDateDateEdit.Name = "FromDateDateEdit";
			this.FromDateDateEdit.TabIndex = 5;
			// 
			// SortByGroupBox
			// 
			this.SortByGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.SortByGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|270192c9-75d5-4a45-9979-70f0cafbcc48", "Sort By");
			this.SortByGroupBox.Controls.Add(this.SourceRadioButton);
			this.SortByGroupBox.Controls.Add(this.PeriodRadioButton);
			this.SortByGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 211, true);
			this.SortByGroupBox.Name = "SortByGroupBox";
			this.SortByGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 43, true);
			this.SortByGroupBox.TabIndex = 1;
			this.SortByGroupBox.TabStop = false;
			// 
			// SourceRadioButton
			// 
			this.SourceRadioButton.AutoCheck = false;
			this.SourceRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.SourceRadioButton, "SortBySource");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GLTransactionBusinessObject)(null)).SortBySource)));
			this.SourceRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|e5757a03-eac3-421e-bc16-a7044fbd5e1e", "Source");
			this.SourceRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.SourceRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 20, true);
			this.SourceRadioButton.Name = "SourceRadioButton";
			this.SourceRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(58, 17, true);
			this.SourceRadioButton.TabIndex = 1;
			this.SourceRadioButton.TabStop = true;
			this.SourceRadioButton.UseVisualStyleBackColor = true;
			// 
			// PeriodRadioButton
			// 
			this.PeriodRadioButton.AutoCheck = false;
			this.PeriodRadioButton.AutoSize = true;
			this.BindingSource.SetBindingMember(this.PeriodRadioButton, "SortByPeriod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GLTransactionBusinessObject)(null)).SortByPeriod)));
			this.PeriodRadioButton.Checked = true;
			this.PeriodRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|c6722e9a-e542-421e-bd6c-a7a807cd33cf", "Period");
			this.PeriodRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PeriodRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 20, true);
			this.PeriodRadioButton.Name = "PeriodRadioButton";
			this.PeriodRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 17, true);
			this.PeriodRadioButton.TabIndex = 0;
			this.PeriodRadioButton.TabStop = true;
			this.PeriodRadioButton.UseVisualStyleBackColor = true;
			// 
			// CancelButton
			// 
			this.CancelButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CancelButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|7cde44a3-1448-48af-93bc-3250ee6c48d7", "Cancel");
			this.CancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(346, 523, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelButton.TabIndex = 6;
			// 
			// ExportButton
			// 
			this.ExportButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ExportButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|202c32dd-9eff-4ecc-a739-c1b6c54211f1", "Export");
			this.ExportButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(263, 523, true);
			this.ExportButton.Name = "ExportButton";
			this.ExportButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ExportButton.TabIndex = 5;
			this.ExportButton.Click += new EventHandler(this.ExportButton_Click);
			// 
			// LogTextBox
			// 
			this.LogTextBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LogTextBox, "Log");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GLTransactionBusinessObject)(null)).Log)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LogTextBox, false);
			this.LogTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 400, true);
			this.LogTextBox.Multiline = true;
			this.LogTextBox.Name = "LogTextBox";
			this.LogTextBox.ReadOnly = true;
			this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.LogTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 115, true);
			this.LogTextBox.TabIndex = 4;
			// 
			// DirectoryGroupBox
			// 
			this.DirectoryGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.DirectoryGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|3f256501-b6dc-4f8c-90fe-da4b7e4198d8", "Export Directory");
			this.DirectoryGroupBox.Controls.Add(this.BrowseButton);
			this.DirectoryGroupBox.Controls.Add(this.ExportDirectoryTextBox);
			this.DirectoryGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 339, true);
			this.DirectoryGroupBox.Name = "DirectoryGroupBox";
			this.DirectoryGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 50, true);
			this.DirectoryGroupBox.TabIndex = 3;
			this.DirectoryGroupBox.TabStop = false;
			// 
			// BrowseButton
			// 
			this.BrowseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(358, 17, true);
			this.BrowseButton.Name = "BrowseButton";
			this.BrowseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 23, true);
			this.BrowseButton.TabIndex = 1;
			this.BrowseButton.Text = "...";
			this.BrowseButton.UseVisualStyleBackColor = true;
			this.BrowseButton.Click += new EventHandler(this.BrowseButton_Click);
			// 
			// ExportDirectoryTextBox
			// 
			this.BindingSource.SetBindingMember(this.ExportDirectoryTextBox, "ExportDirectory");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((GLTransactionBusinessObject)(null)).ExportDirectory)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ExportDirectoryTextBox, false);
			this.ExportDirectoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 19, true);
			this.ExportDirectoryTextBox.Name = "ExportDirectoryTextBox";
			this.ExportDirectoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 20, true);
			this.ExportDirectoryTextBox.TabIndex = 0;
			// 
			// BatchGroupBox
			// 
			this.BatchGroupBox.Anchor = ((AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BatchGroupBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|99210f85-6e84-4cb2-8dba-baebf9fb3ede", "Batch");
			this.BatchGroupBox.Controls.Add(this.BatchNumberCalcEdit);
			this.BatchGroupBox.Controls.Add(this.ExportExistingBatchCheckBox);
			this.BatchGroupBox.Controls.Add(this.CreateAndExportBatchCheckBox);
			this.BatchGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 260, true);
			this.BatchGroupBox.Name = "BatchGroupBox";
			this.BatchGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 73, true);
			this.BatchGroupBox.TabIndex = 2;
			this.BatchGroupBox.TabStop = false;
			// 
			// BatchNumberCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.BatchNumberCalcEdit, "BatchNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((GLTransactionBusinessObject)(null)).BatchNumber)));
			this.BatchNumberCalcEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|f107f731-68df-46c5-9b09-3aab560cf592", "Batch Number");
			this.BatchNumberCalcEdit.Decimals = 0;
			this.BatchNumberCalcEdit.IsCalculatorEnabled = false;
			this.BatchNumberCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(315, 41, true);
			this.BatchNumberCalcEdit.Name = "BatchNumberCalcEdit";
			this.BatchNumberCalcEdit.ShowGroupSeparators = false;
			this.BatchNumberCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(71, 20, true);
			this.BatchNumberCalcEdit.TabIndex = 2;
			this.BatchNumberCalcEdit.Text = "0";
			this.BatchNumberCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ExportExistingBatchCheckBox
			// 
			this.ExportExistingBatchCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExportExistingBatchCheckBox, "ExportExistingBatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GLTransactionBusinessObject)(null)).ExportExistingBatch)));
			this.ExportExistingBatchCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|30cfabb3-b16c-4ba5-b8d7-9fd55b6f4bbe", "Export Existing Batch");
			this.ExportExistingBatchCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExportExistingBatchCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 43, true);
			this.ExportExistingBatchCheckBox.Name = "ExportExistingBatchCheckBox";
			this.ExportExistingBatchCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.ExportExistingBatchCheckBox.TabIndex = 1;
			this.ExportExistingBatchCheckBox.UseVisualStyleBackColor = true;
			// 
			// CreateAndExportBatchCheckBox
			// 
			this.CreateAndExportBatchCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CreateAndExportBatchCheckBox, "CreateAndExportBatch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((GLTransactionBusinessObject)(null)).CreateAndExportBatch)));
			this.CreateAndExportBatchCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|4a105cbf-f955-41d3-9df2-f536334e0e43", "Create And Export Batch");
			this.CreateAndExportBatchCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CreateAndExportBatchCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(19, 20, true);
			this.CreateAndExportBatchCheckBox.Name = "CreateAndExportBatchCheckBox";
			this.CreateAndExportBatchCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.CreateAndExportBatchCheckBox.TabIndex = 0;
			this.CreateAndExportBatchCheckBox.UseVisualStyleBackColor = true;
			// 
			// CsvExportGLTransactionForm
			// 
			this.AcceptButton = this.ExportButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(433, 555, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("CsvExportGLTransactionForm|d774dd6c-23be-4b6b-b362-efefab4fea2f", "Export GL Transactions");
			this.Controls.Add(this.DirectoryGroupBox);
			this.Controls.Add(this.LogTextBox);
			this.Controls.Add(this.ExportButton);
			this.Controls.Add(this.CancelButton);
			this.Controls.Add(this.BatchGroupBox);
			this.Controls.Add(this.SortByGroupBox);
			this.Controls.Add(this.PrimaryFiltersGroupBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.DataTransfer";
			this.DataSourceType = typeof(GLTransactionBusinessObject);
			this.DataSourceTypeName = "Enterprise.Accounting.DataTransfer.GLJournals.GLTransactionBusinessObject";
			this.ForeColor = System.Drawing.SystemColors.ControlText;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "CsvExportGLTransactionForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.PrimaryFiltersGroupBox, 0);
			this.Controls.SetChildIndex(this.SortByGroupBox, 0);
			this.Controls.SetChildIndex(this.BatchGroupBox, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.ExportButton, 0);
			this.Controls.SetChildIndex(this.LogTextBox, 0);
			this.Controls.SetChildIndex(this.DirectoryGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.PrimaryFiltersGroupBox.ResumeLayout(false);
			this.PrimaryFiltersGroupBox.PerformLayout();
			this.SortByGroupBox.ResumeLayout(false);
			this.SortByGroupBox.PerformLayout();
			this.DirectoryGroupBox.ResumeLayout(false);
			this.DirectoryGroupBox.PerformLayout();
			this.BatchGroupBox.ResumeLayout(false);
			this.BatchGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}
