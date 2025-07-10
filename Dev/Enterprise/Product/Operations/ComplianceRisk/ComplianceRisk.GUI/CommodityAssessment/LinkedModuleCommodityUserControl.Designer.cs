namespace Enterprise.ComplianceRisk.GUI
{
	partial class LinkedModuleCommodityUserControl
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
			this.CommodityRiskStatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.CommodityRiskStatusLabel = new Enterprise.ZArchitecture.ZLabel();
			this.AssessmentNotesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.AssessmentNotesGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CommodityBorderWiseLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
			this.AssessmentInitializeButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommodityRiskStatusDropEdit.SuspendLayout();
			this.AssessmentNotesGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.Integration.ILinkedModuleComplianceCommodityDetail);
			// 
			// CommodityRiskStatusDropEdit
			// 
			this.CommodityRiskStatusDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommodityRiskStatusDropEdit, "RiskStatusDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ComplianceRisk.Integration.ILinkedModuleComplianceCommodityDetail)(null)).RiskStatusDescription)));
			this.CommodityRiskStatusDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.CommodityRiskStatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(91, 56, true);
			this.CommodityRiskStatusDropEdit.Name = "CommodityRiskStatusDropEdit";
			this.CommodityRiskStatusDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowDescription;
			this.CommodityRiskStatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 18, true);
			this.CommodityRiskStatusDropEdit.TabIndex = 1;
			// 
			// CommodityRiskStatusLabel
			// 
			this.CommodityRiskStatusLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.CommodityRiskStatusLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 56, true);
			this.CommodityRiskStatusLabel.Name = "CommodityRiskStatusLabel";
			this.CommodityRiskStatusLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 18, true);
			this.CommodityRiskStatusLabel.TabIndex = 2;
			this.CommodityRiskStatusLabel.Text = "Risk Status";
			this.CommodityRiskStatusLabel.UseMnemonic = false;
			// 
			// AssessmentNotesTextBox
			// 
			this.BindingSource.SetBindingMember(this.AssessmentNotesTextBox, "AssessmentNotes");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Integration.ILinkedModuleComplianceCommodityDetail)(null)).AssessmentNotes)));
			this.AssessmentNotesTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AssessmentNotesTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.AssessmentNotesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 14, true);
			this.AssessmentNotesTextBox.Multiline = true;
			this.AssessmentNotesTextBox.Name = "AssessmentNotesTextBox";
			this.AssessmentNotesTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.AssessmentNotesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 157, true);
			this.AssessmentNotesTextBox.TabIndex = 4;
			// 
			// AssessmentNotesGroupBox
			// 
			this.AssessmentNotesGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AssessmentNotesGroupBox.AutoSize = true;
			this.AssessmentNotesGroupBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("8a937678-1850-4dfe-8227-5881346a2414", "Notes");
			this.AssessmentNotesGroupBox.Controls.Add(this.AssessmentNotesTextBox);
			this.AssessmentNotesGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 78, true);
			this.AssessmentNotesGroupBox.Name = "AssessmentNotesGroupBox";
			this.AssessmentNotesGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(420, 174, true);
			this.AssessmentNotesGroupBox.TabIndex = 5;
			this.AssessmentNotesGroupBox.TabStop = false;
			// 
			// CommodityBorderWiseLinkLabel
			// 
			this.CommodityBorderWiseLinkLabel.AutoSize = true;
			this.BindingSource.SetBindingMember(this.CommodityBorderWiseLinkLabel, "HarmonizedBorderWiseTextual");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.Integration.ILinkedModuleComplianceCommodityDetail)(null)).HarmonizedBorderWiseTextual)));
			this.CommodityBorderWiseLinkLabel.IsFontBold = false;
			this.CommodityBorderWiseLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 29, true);
			this.CommodityBorderWiseLinkLabel.Name = "CommodityBorderWiseLinkLabel";
			this.CommodityBorderWiseLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.CommodityBorderWiseLinkLabel.TabIndex = 6;
			this.CommodityBorderWiseLinkLabel.TabStop = false;
			this.CommodityBorderWiseLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.CommodityBorderWiseLinkLabel_LinkClicked);
			// 
			// AssessmentInitializeButton
			// 
			this.AssessmentInitializeButton.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("111DCB47-D659-40C6-8AAC-3F1110118573", "Initiate Assessment");
			this.AssessmentInitializeButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.AssessmentInitializeButton.Name = "AssessmentInitializeButton";
			this.AssessmentInitializeButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(120, 22, true);
			this.AssessmentInitializeButton.TabIndex = 7;
			this.AssessmentInitializeButton.ToolTipCaption = null;
			this.AssessmentInitializeButton.UseVisualStyleBackColor = true;
			this.AssessmentInitializeButton.Click += new System.EventHandler(this.AssessmentInitializeButton_Click);
			// 
			// LinkedModuleCommodityUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.AutoSize = true;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.AssessmentInitializeButton);
			this.Controls.Add(this.CommodityBorderWiseLinkLabel);
			this.Controls.Add(this.AssessmentNotesGroupBox);
			this.Controls.Add(this.CommodityRiskStatusLabel);
			this.Controls.Add(this.CommodityRiskStatusDropEdit);
			this.Name = "LinkedModuleCommodityUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 260, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommodityRiskStatusDropEdit.ResumeLayout(true);
			this.CommodityRiskStatusDropEdit.PerformLayout();
			this.AssessmentNotesGroupBox.ResumeLayout(false);
			this.AssessmentNotesGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion
		private ZArchitecture.GUI.ZDropEdit CommodityRiskStatusDropEdit;
		private ZArchitecture.ZLabel CommodityRiskStatusLabel;
		private ZArchitecture.ZTextBox AssessmentNotesTextBox;
		private ZArchitecture.GUI.ZGroupBox AssessmentNotesGroupBox;
		private ZArchitecture.GUI.ZLinkLabel CommodityBorderWiseLinkLabel;
		internal ZArchitecture.GUI.ZButton AssessmentInitializeButton;
	}
}
