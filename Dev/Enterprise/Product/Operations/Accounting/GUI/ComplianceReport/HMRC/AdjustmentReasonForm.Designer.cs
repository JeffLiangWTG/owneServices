namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC
{
	partial class AdjustmentReasonForm
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
			this.errorsText = new Enterprise.ZArchitecture.ZLabel();
			this.adjustmentsText = new Enterprise.ZArchitecture.ZLabel();
			this.cw1Box = new Enterprise.ZArchitecture.ZCalcEdit();
			this.errorsBox = new Enterprise.ZArchitecture.ZCalcEdit();
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
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 117, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 20, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow);
			// 
			// rowText
			// 
			this.BindingSource.SetBindingMember(this.rowText, "RowText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow)(null)).RowText)));
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
			this.computerByCW1Text.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a5392c4e-29a4-4647-98c2-205c82e55a60", "Current Period");
			this.computerByCW1Text.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.computerByCW1Text.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 0, true);
			this.computerByCW1Text.Name = "computerByCW1Text";
			this.computerByCW1Text.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.computerByCW1Text.TabIndex = 2;
			this.computerByCW1Text.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// errorsText
			// 
			this.errorsText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("3428f373-a73d-43a9-ba53-80dfed003d8a", "Previous Period");
			this.errorsText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.errorsText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 0, true);
			this.errorsText.Name = "errorsText";
			this.errorsText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.errorsText.TabIndex = 3;
			this.errorsText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// adjustmentsText
			// 
			this.adjustmentsText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9a6a33d3-9d04-432b-9ea0-0be2f134b0e3", "Adjustments");
			this.adjustmentsText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.adjustmentsText.ForeColor = System.Drawing.SystemColors.ControlText;
			this.adjustmentsText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 0, true);
			this.adjustmentsText.Name = "adjustmentsText";
			this.adjustmentsText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.adjustmentsText.TabIndex = 4;
			this.adjustmentsText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// cw1Box
			// 
			this.BindingSource.SetBindingMember(this.cw1Box, "CW1Amount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow)(null)).CW1Amount)));
			this.cw1Box.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("9f408dc9-4222-4394-9f25-33bc0339ef47", "GBP");
			this.cw1Box.DecimalPlaces = 2;
			this.cw1Box.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(350, 22, true);
			this.cw1Box.Name = "cw1Box";
			this.cw1Box.ReadOnly = true;
			this.cw1Box.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.cw1Box.TabIndex = 5;
			this.cw1Box.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// errorsBox
			// 
			this.BindingSource.SetBindingMember(this.errorsBox, "ErrorsAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow)(null)).ErrorsAmount)));
			this.errorsBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("c02e49ae-727c-4cc0-94ba-6053ca9a83cc", " ");
			this.errorsBox.DecimalPlaces = 2;
			this.errorsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(465, 22, true);
			this.errorsBox.Name = "errorsBox";
			this.errorsBox.ReadOnly = true;
			this.errorsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.errorsBox.TabIndex = 6;
			this.errorsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// adjustmentsBox
			// 
			this.BindingSource.SetBindingMember(this.adjustmentsBox, "AdjustmentsAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow)(null)).AdjustmentsAmount)));
			this.adjustmentsBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("99bd5e84-22c0-49f7-ae42-b60fdd73bcc7", " ");
			this.adjustmentsBox.DecimalPlaces = 2;
			this.adjustmentsBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(580, 22, true);
			this.adjustmentsBox.Name = "adjustmentsBox";
			this.adjustmentsBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.adjustmentsBox.TabIndex = 7;
			this.adjustmentsBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// reasonDropEdit
			// 
			this.reasonDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.reasonDropEdit, "ReasonHolder.Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow)(null)).ReasonHolder.Code)));
			this.reasonDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("73a8a0be-8385-4b5e-865c-f6a83d96128b", " ");
			this.reasonDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 22, true);
			this.reasonDropEdit.Name = "reasonDropEdit";
			this.reasonDropEdit.PreBoundMaxLength = 2;
			this.reasonDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 17, true);
			this.reasonDropEdit.TabIndex = 8;
			// 
			// totalText
			// 
			this.totalText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("d026677e-3124-4690-9b50-0f6aef4d9e72", "Total");
			this.totalText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.totalText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(900, 0, true);
			this.totalText.Name = "totalText";
			this.totalText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.totalText.TabIndex = 9;
			this.totalText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// totalBox
			// 
			this.BindingSource.SetBindingMember(this.totalBox, "TotalAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow)(null)).TotalAmount)));
			this.totalBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("462ebd79-ff7f-49a0-a775-f2a10eb56a70", " ");
			this.totalBox.DecimalPlaces = 2;
			this.totalBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(900, 22, true);
			this.totalBox.Name = "totalBox";
			this.totalBox.ReadOnly = true;
			this.totalBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 17, true);
			this.totalBox.TabIndex = 10;
			this.totalBox.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// notesText
			// 
			this.notesText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("a1e81882-2bad-49f0-af1c-c2882859a91a", "Notes for adjustment:");
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
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow)(null)).ReasonHolder.Reason)));
			this.notesBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("8c814954-8497-4b58-8b76-e3528cf3a617", " ");
			this.notesBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.notesBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(26, 85, true);
			this.notesBox.Name = "notesBox";
			this.notesBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(717, 17, true);
			this.notesBox.TabIndex = 12;
			// 
			// okButton
			// 
			this.okButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("eb8e559b-15bb-47d6-a73b-2c2dc1cd55e7", "OK");
			this.okButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(842, 85, true);
			this.okButton.Name = "okButton";
			this.okButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.okButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.okButton.TabIndex = 13;
			this.okButton.ToolTipCaption = null;
			this.okButton.UseVisualStyleBackColor = true;
			this.okButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("92f90ca6-98de-410c-943c-9ac0ef5f9218", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(925, 85, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 20, true);
			this.cancelButton.TabIndex = 14;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// reasonText
			// 
			this.reasonText.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("b96a0c7e-d5a2-4ac1-9ad3-0f179063ad6e", "Reason");
			this.reasonText.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.reasonText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(700, 0, true);
			this.reasonText.Name = "reasonText";
			this.reasonText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(175, 20, true);
			this.reasonText.TabIndex = 15;
			this.reasonText.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// AdjustmentReasonForm
			// 
			this.AcceptButton = this.okButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("39143de5-68f2-4b25-93aa-6ad47e4009c1", "Adjustment Reason Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1020, 137, true);
			this.Controls.Add(this.reasonText);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.okButton);
			this.Controls.Add(this.notesBox);
			this.Controls.Add(this.notesText);
			this.Controls.Add(this.totalBox);
			this.Controls.Add(this.totalText);
			this.Controls.Add(this.reasonDropEdit);
			this.Controls.Add(this.adjustmentsBox);
			this.Controls.Add(this.errorsBox);
			this.Controls.Add(this.cw1Box);
			this.Controls.Add(this.adjustmentsText);
			this.Controls.Add(this.errorsText);
			this.Controls.Add(this.computerByCW1Text);
			this.Controls.Add(this.rowText);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataRow";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AdjustmentReasonForm";
			this.Controls.SetChildIndex(this.rowText, 0);
			this.Controls.SetChildIndex(this.computerByCW1Text, 0);
			this.Controls.SetChildIndex(this.errorsText, 0);
			this.Controls.SetChildIndex(this.adjustmentsText, 0);
			this.Controls.SetChildIndex(this.cw1Box, 0);
			this.Controls.SetChildIndex(this.errorsBox, 0);
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
		private ZArchitecture.ZLabel errorsText;
		private ZArchitecture.ZLabel adjustmentsText;
		private ZArchitecture.ZCalcEdit cw1Box;
		private ZArchitecture.ZCalcEdit errorsBox;
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
