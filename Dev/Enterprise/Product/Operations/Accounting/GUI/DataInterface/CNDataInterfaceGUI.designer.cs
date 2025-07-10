namespace Enterprise.Accounting.GUI.DataInterface.ChinaStandard_GBT24589_1;

public partial class CNDataInterfaceGUI
{
	#region Designer generated code

	/// <summary>
	/// Required method for Designer support - do not modify
	/// the contents of this method with the code editor.
	/// </summary>
	new void InitializeComponent()
	{
		this.InterfaceFileGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.DirectoryPopUpButton = new Enterprise.ZArchitecture.GUI.ZButton();
		this.DirectoryTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.FromPeriodEdit = new Enterprise.ZArchitecture.GUI.ZPeriodEdit();
		this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
		this.GenerateButton = new Enterprise.ZArchitecture.GUI.ZButton();
		this.LogTextBox = new Enterprise.ZArchitecture.ZTextBox();
		this.DateGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
		this.zGroupBox1 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.ExportFixedAssetsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.ExportPayrollsCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.ExportReferenceFilesCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.ExportARAPCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.ExportGeneralLedgerCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
		this.zGroupBox2 = new Enterprise.ZArchitecture.GUI.ZGroupBox();
		this.OldChartTypeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
		this.NewChartTypeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
		this.InterfaceFileGroupBox.SuspendLayout();
		this.DateGroupBox.SuspendLayout();
		this.BranchGuidFindBox.SuspendLayout();
		this.zGroupBox1.SuspendLayout();
		this.zGroupBox2.SuspendLayout();
		this.SuspendLayout();
		// 
		// MainStatusBar
		// 
		this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 481, true);
		this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 24, true);
		// 
		// BindingSource
		// 
		this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper);
		// 
		// InterfaceFileGroupBox
		// 
		this.InterfaceFileGroupBox.Controls.Add(this.DirectoryPopUpButton);
		this.InterfaceFileGroupBox.Controls.Add(this.DirectoryTextBox);
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.InterfaceFileGroupBox, false);
		this.InterfaceFileGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 171, true);
		this.InterfaceFileGroupBox.Name = "InterfaceFileGroupBox";
		this.InterfaceFileGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 53, true);
		this.InterfaceFileGroupBox.TabIndex = 12;
		this.InterfaceFileGroupBox.TabStop = false;
		// 
		// DirectoryPopUpButton
		// 
		this.DirectoryPopUpButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(495, 19, true);
		this.DirectoryPopUpButton.Name = "DirectoryPopUpButton";
		this.DirectoryPopUpButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(28, 21, true);
		this.DirectoryPopUpButton.TabIndex = 14;
		this.DirectoryPopUpButton.Text = "...";
		this.DirectoryPopUpButton.Click += new System.EventHandler(this.DirectoryPopUpButton_Click);
		// 
		// DirectoryTextBox
		// 
		this.BindingSource.SetBindingMember(this.DirectoryTextBox, "ExportDirectory");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).ExportDirectory)));
		this.DirectoryTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7a34d44c-bdd1-4ae9-8691-aee4a12679da", "Output Path");
		this.DirectoryTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 19, true);
		this.DirectoryTextBox.Name = "DirectoryTextBox";
		this.DirectoryTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 20, true);
		this.DirectoryTextBox.TabIndex = 6;
		// 
		// FromPeriodEdit
		// 
		this.BindingSource.SetBindingMember(this.FromPeriodEdit, "Period");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).Period)));
		this.FromPeriodEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("4b12b74b-3f8e-46fc-a375-5f956af3b592", "Period");
		this.FromPeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(93, 19, true);
		this.FromPeriodEdit.Name = "FromPeriodEdit";
		this.FromPeriodEdit.TabIndex = 4;
		// 
		// CloseButton
		// 
		this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("XmlExportForm|b8703c06-15ec-44cb-a18c-cd012585e2c2", "&Close", "Close.");
		this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(464, 438, true);
		this.CloseButton.Name = "CloseButton";
		this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
		this.CloseButton.TabIndex = 8;
		this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
		// 
		// GenerateButton
		// 
		this.GenerateButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
		this.GenerateButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a00b6050-de9b-4664-9dab-c798160848af", "&Generate");
		this.GenerateButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(376, 438, true);
		this.GenerateButton.Name = "GenerateButton";
		this.GenerateButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 23, true);
		this.GenerateButton.TabIndex = 7;
		this.GenerateButton.Click += new System.EventHandler(this.GenerateButton_Click);
		// 
		// LogTextBox
		// 
		this.LogTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
		| System.Windows.Forms.AnchorStyles.Right)));
		this.BindingSource.SetBindingMember(this.LogTextBox, "Log");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).Log)));
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.LogTextBox, false);
		this.LogTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 235, true);
		this.LogTextBox.Multiline = true;
		this.LogTextBox.Name = "LogTextBox";
		this.LogTextBox.ReadOnly = true;
		this.LogTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
		this.LogTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(542, 197, true);
		this.LogTextBox.TabIndex = 17;
		this.LogTextBox.TabStop = false;
		// 
		// DateGroupBox
		// 
		this.DateGroupBox.Controls.Add(this.BranchGuidFindBox);
		this.DateGroupBox.Controls.Add(this.FromPeriodEdit);
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.DateGroupBox, false);
		this.DateGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 59, true);
		this.DateGroupBox.Name = "DateGroupBox";
		this.DateGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 51, true);
		this.DateGroupBox.TabIndex = 6;
		this.DateGroupBox.TabStop = false;
		// 
		// BranchGuidFindBox
		// 
		this.BranchGuidFindBox.AllowDrop = true;
		this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "Branch");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).Branch)));
		this.BranchGuidFindBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("7697faa0-2175-440f-b319-37df206f6b7a", "Branch");
		this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 19, true);
		this.BranchGuidFindBox.Name = "BranchGuidFindBox";
		this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 20, true);
		this.BranchGuidFindBox.TabIndex = 5;
		// 
		// zGroupBox1
		// 
		this.zGroupBox1.Controls.Add(this.ExportFixedAssetsCheckBox);
		this.zGroupBox1.Controls.Add(this.ExportPayrollsCheckBox);
		this.zGroupBox1.Controls.Add(this.ExportReferenceFilesCheckBox);
		this.zGroupBox1.Controls.Add(this.ExportARAPCheckBox);
		this.zGroupBox1.Controls.Add(this.ExportGeneralLedgerCheckBox);
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
		this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 2, true);
		this.zGroupBox1.Name = "zGroupBox1";
		this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 52, true);
		this.zGroupBox1.TabIndex = 0;
		this.zGroupBox1.TabStop = false;
		// 
		// ExportFixedAssetsCheckBox
		// 
		this.BindingSource.SetBindingMember(this.ExportFixedAssetsCheckBox, "ExportFixedAssets");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).ExportFixedAssets)));
		this.ExportFixedAssetsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d83fabaa-3db3-4bd8-861d-db2fb1015a66", "Export Fixed Assets");
		this.ExportFixedAssetsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.ExportFixedAssetsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 46, true);
		this.ExportFixedAssetsCheckBox.Name = "ExportFixedAssetsCheckBox";
		this.ExportFixedAssetsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(152, 20, true);
		this.ExportFixedAssetsCheckBox.TabIndex = 4;
		this.ExportFixedAssetsCheckBox.Visible = false;
		// 
		// ExportPayrollsCheckBox
		// 
		this.BindingSource.SetBindingMember(this.ExportPayrollsCheckBox, "ExportPayrolls");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).ExportPayrolls)));
		this.ExportPayrollsCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("df69e074-ca34-433b-823c-0bdde7a14dbb", "Export Payrolls");
		this.ExportPayrollsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.ExportPayrollsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 46, true);
		this.ExportPayrollsCheckBox.Name = "ExportPayrollsCheckBox";
		this.ExportPayrollsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
		this.ExportPayrollsCheckBox.TabIndex = 3;
		this.ExportPayrollsCheckBox.Visible = false;
		// 
		// ExportReferenceFilesCheckBox
		// 
		this.BindingSource.SetBindingMember(this.ExportReferenceFilesCheckBox, "ExportReferenceFiles");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).ExportReferenceFiles)));
		this.ExportReferenceFilesCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("976ffa0e-815f-4785-abaa-9dd27180c4a6", "Export Reference Files");
		this.ExportReferenceFilesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.ExportReferenceFilesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(389, 19, true);
		this.ExportReferenceFilesCheckBox.Name = "ExportReferenceFilesCheckBox";
		this.ExportReferenceFilesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(143, 20, true);
		this.ExportReferenceFilesCheckBox.TabIndex = 3;
		// 
		// ExportARAPCheckBox
		// 
		this.BindingSource.SetBindingMember(this.ExportARAPCheckBox, "ExportARAP");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).ExportARAP)));
		this.ExportARAPCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("f8238176-9250-4ff1-852c-7a2fd86f88b7", "Export AR AP");
		this.ExportARAPCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.ExportARAPCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 19, true);
		this.ExportARAPCheckBox.Name = "ExportARAPCheckBox";
		this.ExportARAPCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(151, 20, true);
		this.ExportARAPCheckBox.TabIndex = 2;
		// 
		// ExportGeneralLedgerCheckBox
		// 
		this.BindingSource.SetBindingMember(this.ExportGeneralLedgerCheckBox, "ExportGeneralLedger");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).ExportGeneralLedger)));
		this.ExportGeneralLedgerCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ab91f5bd-35e5-4340-ab4a-144643bb1ac3", "Export General Ledger");
		this.ExportGeneralLedgerCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.ExportGeneralLedgerCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(21, 19, true);
		this.ExportGeneralLedgerCheckBox.Name = "ExportGeneralLedgerCheckBox";
		this.ExportGeneralLedgerCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(185, 20, true);
		this.ExportGeneralLedgerCheckBox.TabIndex = 1;
		// 
		// zGroupBox2
		// 
		this.zGroupBox2.Controls.Add(this.OldChartTypeRadioButton);
		this.zGroupBox2.Controls.Add(this.NewChartTypeRadioButton);
		this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox2, false);
		this.zGroupBox2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(7, 116, true);
		this.zGroupBox2.Name = "zGroupBox2";
		this.zGroupBox2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(543, 53, true);
		this.zGroupBox2.TabIndex = 19;
		this.zGroupBox2.TabStop = false;
		// 
		// OldChartTypeRadioButton
		// 
		this.OldChartTypeRadioButton.AutoCheck = false;
		this.BindingSource.SetBindingMember(this.OldChartTypeRadioButton, "OLdChartType");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).OLdChartType)));
		this.OldChartTypeRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("837fd45b-1fe7-4a50-b264-e3d8a84b8a7c", "Old Chart(1-5)");
		this.OldChartTypeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.OldChartTypeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 19, true);
		this.OldChartTypeRadioButton.Name = "OldChartTypeRadioButton";
		this.OldChartTypeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 16, true);
		this.OldChartTypeRadioButton.TabIndex = 3;
		// 
		// NewChartTypeRadioButton
		// 
		this.NewChartTypeRadioButton.AutoCheck = false;
		this.BindingSource.SetBindingMember(this.NewChartTypeRadioButton, "NewChartType");
		// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
		CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper)(null)).NewChartType)));
		this.NewChartTypeRadioButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("522fabfa-37de-44e1-b6d0-8b4e1492fa65", "New Chart(1-6)");
		this.NewChartTypeRadioButton.Checked = true;
		this.NewChartTypeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
		this.NewChartTypeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(31, 19, true);
		this.NewChartTypeRadioButton.Name = "NewChartTypeRadioButton";
		this.NewChartTypeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
		this.NewChartTypeRadioButton.TabIndex = 2;
		this.NewChartTypeRadioButton.TabStop = true;
		// 
		// CNDataInterfaceGUI
		// 
		this.AcceptButton = this.GenerateButton;
		this.CancelButton = this.CloseButton;
		this.CaptionRenderingEnabled = true;
		this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("010eff57-a0ce-4dbb-b539-8491fe1bc753", "China Standard GB-T 24589.1");
		this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(561, 505, true);
		this.Controls.Add(this.zGroupBox2);
		this.Controls.Add(this.zGroupBox1);
		this.Controls.Add(this.DateGroupBox);
		this.Controls.Add(this.InterfaceFileGroupBox);
		this.Controls.Add(this.CloseButton);
		this.Controls.Add(this.LogTextBox);
		this.Controls.Add(this.GenerateButton);
		this.DataSourceAssemblyName = "Enterprise.Accounting.DataTransfer";
		this.DataSourceType = typeof(Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaStandard2010DataInterfaceWrapper);
		this.DataSourceTypeName = "Enterprise.Accounting.DataTransfer.DataInterface.ChinaStandard_GBT24589_1.ChinaSt" +
"andardDataInterfaceWrapper";
		this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
		this.Name = "CNDataInterfaceGUI";
		this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
		this.Controls.SetChildIndex(this.GenerateButton, 0);
		this.Controls.SetChildIndex(this.LogTextBox, 0);
		this.Controls.SetChildIndex(this.CloseButton, 0);
		this.Controls.SetChildIndex(this.InterfaceFileGroupBox, 0);
		this.Controls.SetChildIndex(this.MainStatusBar, 0);
		this.Controls.SetChildIndex(this.DateGroupBox, 0);
		this.Controls.SetChildIndex(this.zGroupBox1, 0);
		this.Controls.SetChildIndex(this.zGroupBox2, 0);
		((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
		((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
		this.InterfaceFileGroupBox.ResumeLayout(false);
		this.InterfaceFileGroupBox.PerformLayout();
		this.DateGroupBox.ResumeLayout(false);
		this.DateGroupBox.PerformLayout();
		this.BranchGuidFindBox.ResumeLayout(true);
		this.BranchGuidFindBox.PerformLayout();
		this.zGroupBox1.ResumeLayout(false);
		this.zGroupBox1.PerformLayout();
		this.zGroupBox2.ResumeLayout(false);
		this.zGroupBox2.PerformLayout();
		this.ResumeLayout(false);
		this.PerformLayout();
	}
#endregion

}

