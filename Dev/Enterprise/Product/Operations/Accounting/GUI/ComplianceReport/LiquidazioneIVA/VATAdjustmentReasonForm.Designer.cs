namespace Enterprise.Accounting.GUI.ComplianceReport.LiquidazioneIVA
{
	partial class VATAdjustmentReasonForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.rowText = new Enterprise.ZArchitecture.ZLabel();
			this.computerByCW1Text = new Enterprise.ZArchitecture.ZLabel();
			this.adjustmentsText = new Enterprise.ZArchitecture.ZLabel();
			this.cw1Box = new Enterprise.ZArchitecture.ZCalcEdit();
			this.adjustmentsBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.reasonDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.totalText = new Enterprise.ZArchitecture.ZLabel();
			this.totalBox = new Enterprise.ZArchitecture.ZCalcEdit();
			this.notesText = new Enterprise.ZArchitecture.ZLabel();
			this.notesBox = new Enterprise.ZArchitecture.ZTextBox();
			this.okButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.reasonText = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.reasonDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 122, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 20, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.LIQSubmissionDataRow);
			// 
			// rowText
			// 
			this.BindingSource.SetBindingMember(this.rowText, "RowText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.LIQSubmissionDataRow)(null)).RowText)));
			this.rowText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.rowText, false);
			this.rowText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 24, true);
			this.rowText.Name = "rowText";
			this.rowText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(290, 30, true);
			this.rowText.TabIndex = 1;
			this.rowText.Text = "<RowText>";
			this.rowText.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// computerByCW1Text
			// 
			this.computerByCW1Text.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("91eb57ad-e9de-4b8e-b1ea-ecbd3273ca93", "Current Period");
			this.computerByCW1Text.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.computerByCW1Text.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 0, true);
			this.computerByCW1Text.Name = "computerByCW1Text";
			this.computerByCW1Text.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.computerByCW1Text.TabIndex = 2;
			this.computerByCW1Text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// adjustmentsText
			// 
			this.adjustmentsText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("1d890444-eecd-4a1a-bc50-a62a45a660cc", "Adjustments");
			this.adjustmentsText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.adjustmentsText.ForeColor = System.Drawing.SystemColors.ControlText;
			this.adjustmentsText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 0, true);
			this.adjustmentsText.Name = "adjustmentsText";
			this.adjustmentsText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.adjustmentsText.TabIndex = 4;
			this.adjustmentsText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cw1Box
			// 
			this.BindingSource.SetBindingMember(this.cw1Box, "CW1Amount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.LIQSubmissionDataRow)(null)).CW1Amount)));
			this.cw1Box.CaptionResourceString = null;
			this.cw1Box.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cw1Box, false);
			this.cw1Box.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 22, true);
			this.cw1Box.Name = "cw1Box";
			this.cw1Box.ReadOnly = true;
			this.cw1Box.ShouldEscapeAllSpecialCharacters = false;
			this.cw1Box.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.cw1Box.TabIndex = 5;
			this.cw1Box.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// adjustmentsBox
			// 
			this.BindingSource.SetBindingMember(this.adjustmentsBox, "AdjustmentsAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.LIQSubmissionDataRow)(null)).AdjustmentsAmount)));
			this.adjustmentsBox.CaptionResourceString = null;
			this.adjustmentsBox.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.adjustmentsBox, false);
			this.adjustmentsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(470, 22, true);
			this.adjustmentsBox.Name = "adjustmentsBox";
			this.adjustmentsBox.ShouldEscapeAllSpecialCharacters = false;
			this.adjustmentsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.adjustmentsBox.TabIndex = 7;
			this.adjustmentsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// reasonDropEdit
			// 
			this.reasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reasonDropEdit, "ReasonHolder.Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.LIQSubmissionDataRow)(null)).ReasonHolder.Code)));
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.reasonDropEdit, false);
			this.reasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 22, true);
			this.reasonDropEdit.Name = "reasonDropEdit";
			this.reasonDropEdit.PreBoundMaxLength = 2;
			this.reasonDropEdit.ShouldResizeByMaxLength = true;
			this.reasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.reasonDropEdit.TabIndex = 8;
			// 
			// totalText
			// 
			this.totalText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b49776bb-3955-467c-86bb-1c788cc0a411", "Total");
			this.totalText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.totalText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(789, 0, true);
			this.totalText.Name = "totalText";
			this.totalText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.totalText.TabIndex = 9;
			this.totalText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// totalBox
			// 
			this.BindingSource.SetBindingMember(this.totalBox, "TotalAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.LIQSubmissionDataRow)(null)).TotalAmount)));
			this.totalBox.CaptionResourceString = null;
			this.totalBox.DecimalPlaces = 2;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.totalBox, false);
			this.totalBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(789, 22, true);
			this.totalBox.Name = "totalBox";
			this.totalBox.ReadOnly = true;
			this.totalBox.ShouldEscapeAllSpecialCharacters = false;
			this.totalBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.totalBox.TabIndex = 10;
			this.totalBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// notesText
			// 
			this.notesText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ec779fed-0d38-4f20-b96c-4a91b04b7ab8", "Notes for adjustment:");
			this.notesText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.notesText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 65, true);
			this.notesText.Name = "notesText";
			this.notesText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(135, 20, true);
			this.notesText.TabIndex = 11;
			// 
			// notesBox
			// 
			this.BindingSource.SetBindingMember(this.notesBox, "ReasonHolder.Reason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.LIQSubmissionDataRow)(null)).ReasonHolder.Reason)));
			this.notesBox.CaptionResourceString = null;
			this.notesBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.notesBox, false);
			this.notesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 85, true);
			this.notesBox.Name = "notesBox";
			this.notesBox.ShouldEscapeAllSpecialCharacters = false;
			this.notesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(695, 20, true);
			this.notesBox.TabIndex = 12;
			// 
			// okButton
			// 
			this.okButton.IsCaptionOverridden = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.okButton, false);
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(731, 84, true);
			this.okButton.Name = "okButton";
			this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.okButton.TabIndex = 13;
			this.okButton.Text = "OK";
			this.okButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.IsCaptionOverridden = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.cancelButton, false);
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(814, 84, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.cancelButton.TabIndex = 14;
			this.cancelButton.Text = "Cancel";
			this.cancelButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// reasonText
			// 
			this.reasonText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("167071af-8cd3-42be-9452-29a7697e4fc3", "Reason");
			this.reasonText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.reasonText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 0, true);
			this.reasonText.Name = "reasonText";
			this.reasonText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.reasonText.TabIndex = 15;
			this.reasonText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// VATAdjustmentReasonForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(929, 142, true);
			this.Controls.Add(this.reasonText);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.notesBox);
			this.Controls.Add(this.notesText);
			this.Controls.Add(this.totalBox);
			this.Controls.Add(this.totalText);
			this.Controls.Add(this.reasonDropEdit);
			this.Controls.Add(this.adjustmentsBox);
			this.Controls.Add(this.cw1Box);
			this.Controls.Add(this.adjustmentsText);
			this.Controls.Add(this.computerByCW1Text);
			this.Controls.Add(this.rowText);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.LIQSubmissionDataRow);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ComplianceReport.LiquidazioneIVA.LIQSubmissionData" +
    "Row";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "VATAdjustmentReasonForm";
			this.Controls.SetChildIndex(this.rowText, 0);
			this.Controls.SetChildIndex(this.computerByCW1Text, 0);
			this.Controls.SetChildIndex(this.adjustmentsText, 0);
			this.Controls.SetChildIndex(this.cw1Box, 0);
			this.Controls.SetChildIndex(this.adjustmentsBox, 0);
			this.Controls.SetChildIndex(this.reasonDropEdit, 0);
			this.Controls.SetChildIndex(this.totalText, 0);
			this.Controls.SetChildIndex(this.totalBox, 0);
			this.Controls.SetChildIndex(this.notesText, 0);
			this.Controls.SetChildIndex(this.notesBox, 0);
			this.Controls.SetChildIndex(this.okButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.reasonText, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.reasonDropEdit.ResumeLayout(true);
			this.reasonDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel rowText;
		private ZArchitecture.ZLabel computerByCW1Text;
		private ZArchitecture.ZLabel adjustmentsText;
		private ZArchitecture.ZCalcEdit cw1Box;
		private ZArchitecture.ZCalcEdit adjustmentsBox;
		private ZArchitecture.GUI.ZDropEdit reasonDropEdit;
		private ZArchitecture.ZLabel totalText;
		private ZArchitecture.ZCalcEdit totalBox;
		private ZArchitecture.ZLabel notesText;
		private ZArchitecture.ZTextBox notesBox;
		private ZArchitecture.GUI.ZButton okButton;
		private ZArchitecture.GUI.ZButton cancelButton;
		private ZArchitecture.ZLabel reasonText;
	}
}
