using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture;

namespace Enterprise.Customs.AU.Declaration.GUI
{
	public partial class BulkLookupChangeForm
	{
		protected override void InitializeComponent()
		{
			this.cancelBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.continueBtn = new Enterprise.ZArchitecture.GUI.ZButton();
			this.fromDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.removeTreatmentCodeCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.treatmentTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.instructionLabel = new Enterprise.ZArchitecture.ZLabel();
			this.estimatedLookupCountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.estimatedLookupCountLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.fromDetailsGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 239, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 10, true);
			this.MainStatusBar.TabIndex = 7;
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.BulkLookupChanger);
			// 
			// CancelBtn
			// 
			this.cancelBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.cancelBtn.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("BulkLookupChangeForm|41ca5d33-c137-4c13-a8ea-9fd6622c04d3", "Cancel");
			this.cancelBtn.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(524, 228, true);
			this.cancelBtn.Name = "CancelBtn";
			this.cancelBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.cancelBtn.TabIndex = 6;
			this.cancelBtn.UseVisualStyleBackColor = true;
			// 
			// ContinueBtn
			// 
			this.continueBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.continueBtn.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("BulkLookupChangeForm|beb87cde-ee37-4eb9-9d95-6f859a9bed11", "Continue");
			this.continueBtn.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(443, 228, true);
			this.continueBtn.Name = "ContinueBtn";
			this.continueBtn.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.continueBtn.TabIndex = 5;
			this.continueBtn.UseVisualStyleBackColor = true;
			this.continueBtn.Click += new System.EventHandler(this.ContinueBtn_Click);
			// 
			// FromDetailsGroupBox
			// 
			this.fromDetailsGroupBox.Controls.Add(this.removeTreatmentCodeCheckBox);
			this.fromDetailsGroupBox.Controls.Add(this.treatmentTextBox);
			this.fromDetailsGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.fromDetailsGroupBox, false);
			this.fromDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 90, true);
			this.fromDetailsGroupBox.Name = "FromDetailsGroupBox";
			this.fromDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 119, true);
			this.fromDetailsGroupBox.TabIndex = 1;
			this.fromDetailsGroupBox.TabStop = false;
			// 
			// RemoveTreatmentCodeCheckBox
			// 
			this.removeTreatmentCodeCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.removeTreatmentCodeCheckBox, "RemoveTreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.AU.Declaration.Business.BulkLookupChanger)(null)).RemoveTreatmentCode)));
			this.removeTreatmentCodeCheckBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("BulkLookupChangeForm|e1a377be-1db9-41cd-82f3-fec9176ac5f6", "Remove Treatment Code", "The Treatment Code on all selected lookups will be cleared (made blank).");
			this.removeTreatmentCodeCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.removeTreatmentCodeCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(210, 26, true);
			this.removeTreatmentCodeCheckBox.Name = "RemoveTreatmentCodeCheckBox";
			this.removeTreatmentCodeCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.removeTreatmentCodeCheckBox.TabIndex = 70;
			// 
			// TreatmentTextBox
			// 
			this.BindingSource.SetBindingMember(this.treatmentTextBox, "TreatmentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.AU.Declaration.Business.BulkLookupChanger)(null)).TreatmentCode)));
			this.treatmentTextBox.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("BulkLookupChangeForm|9f89de4d-f2dc-47da-8213-2b72b2e5395b", "New Treatment Code", "The Treatment Code on selected Lookups will be changed to this value.");
			this.treatmentTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(123, 23, true);
			this.treatmentTextBox.Name = "TreatmentTextBox";
			this.treatmentTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.treatmentTextBox.TabIndex = 69;
			// 
			// InstructionLabel
			// 
			this.instructionLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("BulkLookupChangeForm|0b3d3f49-46c2-4edd-9f55-cddaf5485177", "This form allows you to do a \'Bulk\' change of all the Classification Lookups currently selected by the Grid Filter selections that you have made. The details in all selected Lookups will be changed to the details entered below.  To remove (make blank) check the \'Remove\' check box.");
			this.instructionLabel.Dock = System.Windows.Forms.DockStyle.Top;
			this.instructionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(13, 13, true);
			this.instructionLabel.Margin = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(20, true);
			this.instructionLabel.Name = "InstructionLabel";
			this.instructionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(586, 77, true);
			this.instructionLabel.TabIndex = 0;
			this.instructionLabel.Text = "`";
			this.instructionLabel.TextAlign = System.Drawing.ContentAlignment.TopLeft;
			// 
			// EstimatedLookupCountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.estimatedLookupCountCalcEdit, "EstimatedLookupCount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.AU.Declaration.Business.BulkLookupChanger)(null)).EstimatedLookupCount)));
			this.estimatedLookupCountCalcEdit.BorderStyle = System.Windows.Forms.BorderStyle.None;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.estimatedLookupCountCalcEdit, false);
			this.estimatedLookupCountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(122, 215, true);
			this.estimatedLookupCountCalcEdit.Name = "EstimatedLookupCountCalcEdit";
			this.estimatedLookupCountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(69, 13, true);
			this.estimatedLookupCountCalcEdit.TabIndex = 3;
			this.estimatedLookupCountCalcEdit.Text = "0";
			this.estimatedLookupCountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// EstimatedLookupCountLabel
			// 
			this.estimatedLookupCountLabel.AutoSize = true;
			this.estimatedLookupCountLabel.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("BulkLookupChangeForm|10edae8e-7940-4c3a-993a-13eb7afc54bb", "Lookups selected.");
			this.estimatedLookupCountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(200, 215, true);
			this.estimatedLookupCountLabel.Name = "EstimatedLookupCountLabel";
			this.estimatedLookupCountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.estimatedLookupCountLabel.TabIndex = 4;
			// 
			// BulkLookupChangeForm
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(612, 262, true);
			this.CaptionResourceString = Enterprise.Customs.AU.Declaration.GUI.Res.GetData("BulkLookupChangeForm|e52bb343-2a59-49f7-87a0-036445a3df2a", "Bulk Lookup Change");
			this.Controls.Add(this.estimatedLookupCountLabel);
			this.Controls.Add(this.estimatedLookupCountCalcEdit);
			this.Controls.Add(this.fromDetailsGroupBox);
			this.Controls.Add(this.instructionLabel);
			this.Controls.Add(this.cancelBtn);
			this.Controls.Add(this.continueBtn);
			this.DataSourceAssemblyName = "Enterprise.Customs.AU.Declaration.Business";
			this.DataSourceType = typeof(Enterprise.Customs.AU.Declaration.Business.BulkLookupChanger);
			this.DataSourceTypeName = "Enterprise.Customs.AU.Declaration.Business.BulkLookupChanger";
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 300, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(628, 285, true);
			this.Name = "BulkLookupChangeForm";
			this.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(13, true);
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.continueBtn, 0);
			this.Controls.SetChildIndex(this.cancelBtn, 0);
			this.Controls.SetChildIndex(this.instructionLabel, 0);
			this.Controls.SetChildIndex(this.fromDetailsGroupBox, 0);
			this.Controls.SetChildIndex(this.estimatedLookupCountCalcEdit, 0);
			this.Controls.SetChildIndex(this.estimatedLookupCountLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.fromDetailsGroupBox.ResumeLayout(false);
			this.fromDetailsGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		private ZButton cancelBtn;
		private ZButton continueBtn;
		private ZGroupBox fromDetailsGroupBox;
		private ZLabel instructionLabel;
		private ZCalcEdit estimatedLookupCountCalcEdit;
		private ZLabel estimatedLookupCountLabel;
		private ZTextBox treatmentTextBox;
		private ZCheckBox removeTreatmentCodeCheckBox;
	}
}
