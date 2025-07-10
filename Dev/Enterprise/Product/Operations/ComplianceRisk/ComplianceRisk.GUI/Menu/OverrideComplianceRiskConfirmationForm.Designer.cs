using System.Runtime.InteropServices;
using CargoWiseOne.ResourceStrings;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.ComplianceRisk.GUI
{
	partial class OverrideComplianceRiskConfirmationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(OverrideComplianceRiskConfirmationForm));
			this.ConfirmationTableLayoutPanel = new CargoWise.Windows.UI.KTableLayoutPanel();
			this.MessagePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WarningLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.WarningLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.WarningLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.WarningJobReferenceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.WarningPictureBox = new Enterprise.ZArchitecture.GUI.ZPictureBox();
			this.ConfirmationPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ConfirmationJobReferenceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.ConfirmationLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.OverrideClearReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OverrideClearReasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ConfirmationLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.ConfirmationLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.UserConfirmationTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OverrideButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.ConfirmationTableLayoutPanel.SuspendLayout();
			this.MessagePanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarningPictureBox)).BeginInit();
			this.ConfirmationPanel.SuspendLayout();
			this.OverrideClearReasonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 337, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 28, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.ComplianceRisk.GUI.OverrideComplianceRiskConfirmationModel);
			// 
			// ConfirmationTableLayoutPanel
			// 
			this.ConfirmationTableLayoutPanel.ColumnCount = 1;
			this.ConfirmationTableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ConfirmationTableLayoutPanel.Controls.Add(this.MessagePanel, 0, 0);
			this.ConfirmationTableLayoutPanel.Controls.Add(this.ConfirmationPanel, 0, 1);
			this.ConfirmationTableLayoutPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ConfirmationTableLayoutPanel.Name = "ConfirmationTableLayoutPanel";
			this.ConfirmationTableLayoutPanel.RowCount = 2;
			this.ConfirmationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 100F));
			this.ConfirmationTableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(206)));
			this.ConfirmationTableLayoutPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 334, true);
			this.ConfirmationTableLayoutPanel.TabIndex = 1;
			// 
			// MessagePanel
			// 
			this.MessagePanel.Controls.Add(this.WarningLabel2);
			this.MessagePanel.Controls.Add(this.WarningLabel3);
			this.MessagePanel.Controls.Add(this.WarningLabel1);
			this.MessagePanel.Controls.Add(this.WarningJobReferenceLabel);
			this.MessagePanel.Controls.Add(this.WarningPictureBox);
			this.MessagePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.MessagePanel.Name = "MessagePanel";
			this.MessagePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 123, true);
			this.MessagePanel.TabIndex = 1;
			// 
			// WarningLabel2
			// 
			this.WarningLabel2.AutoSize = true;
			this.WarningLabel2.BackColor = System.Drawing.Color.Transparent;
			this.WarningLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WarningLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 83, true);
			this.WarningLabel2.Name = "WarningLabel2";
			this.WarningLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 14, true);
			this.WarningLabel2.TabIndex = 0;
			this.WarningLabel2.TabStop = true;
			// 
			// WarningLabel3
			// 
			this.WarningLabel3.AutoSize = true;
			this.WarningLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WarningLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 99, true);
			this.WarningLabel3.Name = "WarningLabel3";
			this.WarningLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(426, 14, true);
			this.WarningLabel3.TabIndex = 1;
			this.WarningLabel3.TabStop = true;
			// 
			// WarningLabel1
			// 
			this.WarningLabel1.AutoSize = true;
			this.WarningLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.WarningLabel1.IsFontBold = true;
			this.WarningLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 50, true);
			this.WarningLabel1.Name = "WarningLabel1";
			this.WarningLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(390, 14, true);
			this.WarningLabel1.TabIndex = 2;
			this.WarningLabel1.TabStop = true;
			// 
			// WarningJobReferenceLabel
			// 
			this.WarningJobReferenceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.WarningJobReferenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 18, true);
			this.WarningJobReferenceLabel.Name = "WarningJobReferenceLabel";
			this.WarningJobReferenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(310, 18, true);
			this.WarningJobReferenceLabel.TabIndex = 3;
			this.WarningJobReferenceLabel.TabStop = true;
			// 
			// WarningPictureBox
			// 
			this.WarningPictureBox.Image = ((System.Drawing.Image)(resources.GetObject("WarningPictureBox.Image")));
			this.WarningPictureBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(29, 12, true);
			this.WarningPictureBox.Name = "WarningPictureBox";
			this.WarningPictureBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(32, 32, true);
			this.WarningPictureBox.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
			this.WarningPictureBox.TabIndex = 4;
			this.WarningPictureBox.TabStop = false;
			// 
			// ConfirmationPanel
			// 
			this.ConfirmationPanel.Controls.Add(this.ConfirmationJobReferenceLabel);
			this.ConfirmationPanel.Controls.Add(this.ConfirmationLabel1);
			this.ConfirmationPanel.Controls.Add(this.OverrideClearReasonTextBox);
			this.ConfirmationPanel.Controls.Add(this.OverrideClearReasonDropEdit);
			this.ConfirmationPanel.Controls.Add(this.ConfirmationLabel2);
			this.ConfirmationPanel.Controls.Add(this.ConfirmationLabel3);
			this.ConfirmationPanel.Controls.Add(this.UserConfirmationTextBox);
			this.ConfirmationPanel.Controls.Add(this.OverrideButton);
			this.ConfirmationPanel.Controls.Add(this.CancelButton);
			this.ConfirmationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 130, true);
			this.ConfirmationPanel.Name = "ConfirmationPanel";
			this.ConfirmationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(871, 201, true);
			this.ConfirmationPanel.TabIndex = 3;
			// 
			// ConfirmationJobReferenceLabel
			// 
			this.ConfirmationJobReferenceLabel.AutoSize = true;
			this.ConfirmationJobReferenceLabel.BackColor = System.Drawing.Color.Transparent;
			this.ConfirmationJobReferenceLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ConfirmationJobReferenceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(297, 134, true);
			this.ConfirmationJobReferenceLabel.Name = "ConfirmationJobReferenceLabel";
			this.ConfirmationJobReferenceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 14, true);
			this.ConfirmationJobReferenceLabel.TabIndex = 0;
			// 
			// ConfirmationLabel1
			// 
			this.ConfirmationLabel1.AutoSize = true;
			this.ConfirmationLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ConfirmationLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 10, true);
			this.ConfirmationLabel1.Name = "ConfirmationLabel1";
			this.ConfirmationLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(147, 14, true);
			this.ConfirmationLabel1.TabIndex = 1;
			this.ConfirmationLabel1.TabStop = true;
			this.ConfirmationLabel1.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// OverrideClearReasonTextBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideClearReasonTextBox, "Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.ComplianceRisk.GUI.OverrideComplianceRiskConfirmationModel)(null)).Reason)));
			this.OverrideClearReasonTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.OverrideClearReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 51, true);
			this.OverrideClearReasonTextBox.Multiline = true;
			this.OverrideClearReasonTextBox.Name = "OverrideClearReasonTextBox";
			this.OverrideClearReasonTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.OverrideClearReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(678, 52, true);
			this.OverrideClearReasonTextBox.TabIndex = 2;
			// 
			// OverrideClearReasonDropEdit
			// 
			this.OverrideClearReasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.OverrideClearReasonDropEdit, "Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.ComplianceRisk.GUI.OverrideComplianceRiskConfirmationModel)(null)).Code)));
			this.OverrideClearReasonDropEdit.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("b3e4c91d-0393-4b9c-bba1-038c47345a55", "Party Job Clearing Reason Drop Edit");
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.OverrideClearReasonDropEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Top);
			this.OverrideClearReasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(86, 30, true);
			this.OverrideClearReasonDropEdit.Name = "OverrideClearReasonDropEdit";
			this.OverrideClearReasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(380, 18, true);
			this.OverrideClearReasonDropEdit.TabIndex = 1;
			// 
			// ConfirmationLabel2
			// 
			this.ConfirmationLabel2.AutoSize = true;
			this.ConfirmationLabel2.BackColor = System.Drawing.SystemColors.Window;
			this.ConfirmationLabel2.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ConfirmationLabel2, false);
			this.ConfirmationLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(84, 108, true);
			this.ConfirmationLabel2.Name = "ConfirmationLabel2";
			this.ConfirmationLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(162, 14, true);
			this.ConfirmationLabel2.TabIndex = 3;
			this.ConfirmationLabel2.TabStop = true;
			// 
			// ConfirmationLabel3
			// 
			this.ConfirmationLabel3.AutoSize = true;
			this.ConfirmationLabel3.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.ConfirmationLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(85, 134, true);
			this.ConfirmationLabel3.Name = "ConfirmationLabel3";
			this.ConfirmationLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 14, true);
			this.ConfirmationLabel3.TabIndex = 4;
			this.ConfirmationLabel3.TabStop = true;
			// 
			// UserConfirmationTextBox
			// 
			this.UserConfirmationTextBox.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("3dcc21a7-8ea1-4570-b115-97631acb605d", "Override Clear Confirmation");
			this.UserConfirmationTextBox.CausesValidation = false;
			this.UserConfirmationTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.UserConfirmationTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(97, 134, true);
			this.UserConfirmationTextBox.Name = "UserConfirmationTextBox";
			this.UserConfirmationTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 18, true);
			this.UserConfirmationTextBox.TabIndex = 3;
			this.UserConfirmationTextBox.TextChanged += new System.EventHandler(this.UserConfirmationTextBox_TextChanged);
			this.UserConfirmationTextBox.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.UserConfirmationTextBox_KeyPress);
			// 
			// OverrideButton
			// 
			this.OverrideButton.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("78e87c11-2e6f-465b-b577-a50c2aaf4ef4", "Override");
			this.OverrideButton.Enabled = false;
			this.OverrideButton.IsCaptionOverridden = true;
			this.OverrideButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(329, 166, true);
			this.OverrideButton.Name = "OverrideButton";
			this.OverrideButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.OverrideButton.TabIndex = 4;
			this.OverrideButton.Text = "Override";
			this.OverrideButton.ToolTipCaption = null;
			this.OverrideButton.UseVisualStyleBackColor = true;
			this.OverrideButton.Click += new System.EventHandler(this.OverrideButton_Click);
			// 
			// CancelButton
			// 
			this.CancelButton.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("fef8d6ee-1a2e-4f6f-928b-8f44ec266f9d", "Cancel");
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(474, 166, true);
			this.CancelButton.Name = "CancelButton";
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(110, 23, true);
			this.CancelButton.TabIndex = 5;
			this.CancelButton.ToolTipCaption = null;
			this.CancelButton.UseVisualStyleBackColor = true;
			this.CancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// OverrideComplianceRiskConfirmationForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.ComplianceRisk.GUI.Res.GetData("7ad611be-50de-4d55-a3cf-d67f2181129d", "Compliance Risk Override Confirmation");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 365, true);
			this.Controls.Add(this.ConfirmationTableLayoutPanel);
			this.DataSourceType = typeof(Enterprise.ComplianceRisk.GUI.OverrideComplianceRiskConfirmationModel);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "OverrideComplianceRiskConfirmationForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.ConfirmationTableLayoutPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ConfirmationTableLayoutPanel.ResumeLayout(false);
			this.ConfirmationTableLayoutPanel.PerformLayout();
			this.MessagePanel.ResumeLayout(false);
			this.MessagePanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.WarningPictureBox)).EndInit();
			this.ConfirmationPanel.ResumeLayout(false);
			this.ConfirmationPanel.PerformLayout();
			this.OverrideClearReasonDropEdit.ResumeLayout(true);
			this.OverrideClearReasonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private CargoWise.Windows.UI.KTableLayoutPanel ConfirmationTableLayoutPanel;
		private ZArchitecture.GUI.ZPanel MessagePanel;
		private Enterprise.ZArchitecture.GUI.ZPictureBox WarningPictureBox;
		private ZArchitecture.GUI.ZPanel ConfirmationPanel;
		private ZArchitecture.ZLabel ConfirmationLabel2;
		private ZArchitecture.ZLabel ConfirmationLabel3;
		private new ZArchitecture.GUI.ZButton CancelButton;
		private ZArchitecture.GUI.ZButton OverrideButton;
		private ZArchitecture.ZTextBox UserConfirmationTextBox;
		internal ZArchitecture.ZLabel ConfirmationLabel1;
		internal ZArchitecture.ZTextBox OverrideClearReasonTextBox;
		private ZArchitecture.GUI.ZDropEdit OverrideClearReasonDropEdit;
		private ZArchitecture.ZLabel WarningJobReferenceLabel;
		private ZArchitecture.ZLabel WarningLabel1;
		private ZArchitecture.ZLabel WarningLabel3;
		private ZArchitecture.ZLabel ConfirmationJobReferenceLabel;
		private ZArchitecture.ZLabel WarningLabel2;
	}
}
