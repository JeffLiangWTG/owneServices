using System;

namespace Enterprise.BarcodeParsing.GUI
{
	partial class BarcodeParsingDiagnosticsForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ResultsTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.RuleSet = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.MatchedRulesLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.IsGS1CheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.ClearButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.RelatedEntityGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.BarcodeTextBoxWithGS1Support = new Enterprise.BarcodeParsing.GUI.ZTextBoxWithGS1Support();
            this.ModuleCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.DiagnosticsTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.TargetFieldDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.RunRulesButton = new Enterprise.ZArchitecture.GUI.ZButton();
            this.SupplierGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.BuyerGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.BottomPanel.SuspendLayout();
            this.TopPanel.SuspendLayout();
            this.RuleSet.SuspendLayout();
            this.RelatedEntityGuidFindBox.SuspendLayout();
            this.ModuleCodeDropEdit.SuspendLayout();
            this.DiagnosticsTypeDropEdit.SuspendLayout();
            this.TargetFieldDropEdit.SuspendLayout();
            this.SupplierGuidFindBox.SuspendLayout();
            this.BuyerGuidFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 580, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(605, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics);
            // 
            // BottomPanel
            // 
            this.BottomPanel.Controls.Add(this.ResultsTextBox);
            this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 173, true);
            this.BottomPanel.Name = "BottomPanel";
            this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 409, true);
            this.BottomPanel.TabIndex = 2;
            // 
            // ResultsTextBox
            // 
            this.BindingSource.SetBindingMember(this.ResultsTextBox, "Results");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics)(null)).Results)));
            this.ResultsTextBox.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("112879b2-f6e5-4a2a-bbe7-73aac527798e", "Results");
            this.ResultsTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.ResultsTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ResultsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ResultsTextBox.Multiline = true;
            this.ResultsTextBox.Name = "ResultsTextBox";
            this.ResultsTextBox.ReadOnly = true;
            this.ResultsTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
            this.ResultsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 409, true);
            this.ResultsTextBox.TabIndex = 9;
            // 
            // TopPanel
            // 
            this.TopPanel.BackColor = System.Drawing.SystemColors.Control;
            this.TopPanel.Controls.Add(this.RuleSet);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 173, true);
            this.TopPanel.TabIndex = 1;
            // 
            // RuleSet
            // 
            this.RuleSet.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("c1b56fd8-b7f4-4b55-a9a6-d77718740f61", "Rule Set Parameters");
            this.RuleSet.Controls.Add(this.MatchedRulesLinkLabel);
            this.RuleSet.Controls.Add(this.IsGS1CheckBox);
            this.RuleSet.Controls.Add(this.ClearButton);
            this.RuleSet.Controls.Add(this.RelatedEntityGuidFindBox);
            this.RuleSet.Controls.Add(this.BarcodeTextBoxWithGS1Support);
            this.RuleSet.Controls.Add(this.ModuleCodeDropEdit);
            this.RuleSet.Controls.Add(this.DiagnosticsTypeDropEdit);
            this.RuleSet.Controls.Add(this.TargetFieldDropEdit);
            this.RuleSet.Controls.Add(this.RunRulesButton);
            this.RuleSet.Controls.Add(this.SupplierGuidFindBox);
            this.RuleSet.Controls.Add(this.BuyerGuidFindBox);
            this.RuleSet.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.RuleSet.Name = "RuleSet";
            this.RuleSet.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 165, true);
            this.RuleSet.TabIndex = 33;
            this.RuleSet.TabStop = false;
            // 
            // MatchedRulesLinkLabel
            // 
            this.MatchedRulesLinkLabel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.MatchedRulesLinkLabel.AutoSize = true;
            this.MatchedRulesLinkLabel.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("2eca0678-e81e-4160-9642-49af0c991da1", "Matched rules");
            this.MatchedRulesLinkLabel.IsFontBold = false;
            this.MatchedRulesLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(257, 135, true);
            this.MatchedRulesLinkLabel.Name = "MatchedRulesLinkLabel";
            this.MatchedRulesLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
            this.MatchedRulesLinkLabel.TabIndex = 34;
            this.MatchedRulesLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.MatchedRulesLinkLabel_LinkClicked);
            // 
            // IsGS1CheckBox
            // 
            this.IsGS1CheckBox.AutoSize = true;
            this.BindingSource.SetBindingMember(this.IsGS1CheckBox, "IsGS1Barcode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((bool)(((Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics)(null)).IsGS1Barcode)));
            this.IsGS1CheckBox.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("3d5b1629-750a-4c19-b21a-b09ce12d7a71", "Is GS1");
            this.IsGS1CheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.IsGS1CheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(325, 103, true);
            this.IsGS1CheckBox.Name = "IsGS1CheckBox";
            this.IsGS1CheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(54, 16, true);
            this.IsGS1CheckBox.TabIndex = 33;
            this.IsGS1CheckBox.TabStop = false;
            this.IsGS1CheckBox.UseVisualStyleBackColor = true;
            // 
            // ClearButton
            // 
            this.ClearButton.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("f0ff27e9-7e5d-4e14-b0f2-da2a6522366d", "Clear");
            this.ClearButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(520, 73, true);
            this.ClearButton.Name = "ClearButton";
            this.ClearButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 20, true);
            this.ClearButton.TabIndex = 32;
            this.ClearButton.UseVisualStyleBackColor = true;
            this.ClearButton.Click += new System.EventHandler(this.ClearButton_Click);
            // 
            // RelatedEntityGuidFindBox
            // 
            this.RelatedEntityGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RelatedEntityGuidFindBox, "RelatedEntityPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics)(null)).RelatedEntityPK)));
            this.RelatedEntityGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 46, true);
            this.RelatedEntityGuidFindBox.Name = "RelatedEntityGuidFindBox";
            this.RelatedEntityGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 18, true);
            this.RelatedEntityGuidFindBox.TabIndex = 4;
            // 
            // BarcodeTextBoxWithGS1Support
            // 
            this.BarcodeTextBoxWithGS1Support.AcceptsReturn = true;
            this.BindingSource.SetBindingMember(this.BarcodeTextBoxWithGS1Support, "Barcode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics)(null)).Barcode)));
            this.BarcodeTextBoxWithGS1Support.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 74, true);
            this.BarcodeTextBoxWithGS1Support.Name = "BarcodeTextBoxWithGS1Support";
            this.BarcodeTextBoxWithGS1Support.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 18, true);
            this.BarcodeTextBoxWithGS1Support.TabIndex = 5;
            this.BarcodeTextBoxWithGS1Support.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.BarcodeTextBox_KeyPress);
            this.BarcodeTextBoxWithGS1Support.TextChanged += BarcodeTextBox_TextChanged;
            // 
            // ModuleCodeDropEdit
            // 
            this.ModuleCodeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ModuleCodeDropEdit, "ModuleCode");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics)(null)).ModuleCode)));
            this.ModuleCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 21, true);
            this.ModuleCodeDropEdit.Name = "ModuleCodeDropEdit";
            this.ModuleCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 18, true);
            this.ModuleCodeDropEdit.TabIndex = 1;
            // 
            // DiagnosticsTypeDropEdit
            // 
            this.DiagnosticsTypeDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DiagnosticsTypeDropEdit, "DiagnosticsType");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics)(null)).DiagnosticsType)));
            this.DiagnosticsTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 102, true);
            this.DiagnosticsTypeDropEdit.Name = "DiagnosticsTypeDropEdit";
            this.DiagnosticsTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 18, true);
            this.DiagnosticsTypeDropEdit.TabIndex = 6;
            // 
            // TargetFieldDropEdit
            // 
            this.TargetFieldDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TargetFieldDropEdit, "TargetField");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics)(null)).TargetField)));
            this.TargetFieldDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 102, true);
            this.TargetFieldDropEdit.Name = "TargetFieldDropEdit";
            this.TargetFieldDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 18, true);
            this.TargetFieldDropEdit.TabIndex = 7;
            // 
            // RunRulesButton
            // 
            this.RunRulesButton.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.RunRulesButton.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("84775e4c-63ba-44e8-b69c-fe561244145c", "Run Barcode Parsing Rules");
            this.RunRulesButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 130, true);
            this.RunRulesButton.Name = "RunRulesButton";
            this.RunRulesButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 25, true);
            this.RunRulesButton.TabIndex = 8;
            this.RunRulesButton.UseVisualStyleBackColor = true;
            this.RunRulesButton.Click += new System.EventHandler(this.RunRulesButton_Click);
            // 
            // SupplierGuidFindBox
            // 
            this.SupplierGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SupplierGuidFindBox, "SupplierPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics)(null)).SupplierPK)));
            this.SupplierGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 21, true);
            this.SupplierGuidFindBox.Name = "SupplierGuidFindBox";
            this.SupplierGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 18, true);
            this.SupplierGuidFindBox.TabIndex = 2;
            // 
            // BuyerGuidFindBox
            // 
            this.BuyerGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.BuyerGuidFindBox, "BuyerPK");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics)(null)).BuyerPK)));
            this.BuyerGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 46, true);
            this.BuyerGuidFindBox.Name = "BuyerGuidFindBox";
            this.BuyerGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(198, 18, true);
            this.BuyerGuidFindBox.TabIndex = 3;
            // 
            // BarcodeParsingDiagnosticsForm
            // 
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.BarcodeParsing.GUI.Res.GetData("5f974995-3395-4717-b346-709b7949c080", "Barcode Parsing Diagnostics Form");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(606, 606, true);
            this.Controls.Add(this.BottomPanel);
            this.Controls.Add(this.TopPanel);
            this.DataSourceAssemblyName = "Enterprise.Diagnostics";
            this.DataSourceType = typeof(Enterprise.BarcodeParsing.Business.BarcodeParsingDiagnostics);
            this.DataSourceTypeName = "Enterprise.Diagnostics.BarcodeParsingDiagnostics";
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(621, 643, true);
            this.Name = "BarcodeParsingDiagnosticsForm";
            this.Text = "Barcode Parsing Diagnostics Form";
            this.Controls.SetChildIndex(this.TopPanel, 0);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.BottomPanel, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.BottomPanel.ResumeLayout(false);
            this.BottomPanel.PerformLayout();
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            this.RuleSet.ResumeLayout(false);
            this.RuleSet.PerformLayout();
            this.RelatedEntityGuidFindBox.ResumeLayout(true);
            this.RelatedEntityGuidFindBox.PerformLayout();
            this.ModuleCodeDropEdit.ResumeLayout(true);
            this.ModuleCodeDropEdit.PerformLayout();
            this.DiagnosticsTypeDropEdit.ResumeLayout(true);
            this.DiagnosticsTypeDropEdit.PerformLayout();
            this.TargetFieldDropEdit.ResumeLayout(true);
            this.TargetFieldDropEdit.PerformLayout();
            this.SupplierGuidFindBox.ResumeLayout(true);
            this.SupplierGuidFindBox.PerformLayout();
            this.BuyerGuidFindBox.ResumeLayout(true);
            this.BuyerGuidFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZPanel TopPanel;
		private ZTextBoxWithGS1Support BarcodeTextBoxWithGS1Support;
		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.ZTextBox ResultsTextBox;
		protected ZArchitecture.GUI.ZButton RunRulesButton;
		protected ZArchitecture.GUI.ZGuidFindBox SupplierGuidFindBox;
		protected ZArchitecture.GUI.ZGuidFindBox BuyerGuidFindBox;
		protected ZArchitecture.GUI.ZGuidFindBox RelatedEntityGuidFindBox;
		private ZArchitecture.GUI.ZGroupBox RuleSet;
		protected ZArchitecture.GUI.ZButton ClearButton;
		protected ZArchitecture.GUI.ZDropEdit ModuleCodeDropEdit;
		protected ZArchitecture.GUI.ZDropEdit DiagnosticsTypeDropEdit;
		protected ZArchitecture.GUI.ZDropEdit TargetFieldDropEdit;
		protected ZArchitecture.GUI.ZCheckBox IsGS1CheckBox;
		protected Enterprise.ZArchitecture.GUI.ZLinkLabel MatchedRulesLinkLabel;
	}
}
