namespace Enterprise.Accounting.GUI.ComplianceReport.HMRC
{
	partial class MTDDeclarationForm
	{
		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected new void InitializeComponent()
		{
			this.declarationCheckBoxText1 = new Enterprise.ZArchitecture.ZLabel();
			this.declarationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.confirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.cancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.errorLabel = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 153, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataColumns);
			// 
			// declarationCheckBoxText1
			// 
			this.BindingSource.SetBindingMember(this.declarationCheckBoxText1, "DeclarationHeaderText");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataColumns)(null)).DeclarationHeaderText)));
			this.declarationCheckBoxText1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.declarationCheckBoxText1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
			this.declarationCheckBoxText1.IsFontBold = true;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.declarationCheckBoxText1, false);
			this.declarationCheckBoxText1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 6, true);
			this.declarationCheckBoxText1.Name = "declarationCheckBoxText1";
			this.declarationCheckBoxText1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 35, true);
			this.declarationCheckBoxText1.TabIndex = 1;
			// 
			// declarationCheckBox
			// 
			this.BindingSource.SetBindingMember(this.declarationCheckBox, "Declaration");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataColumns)(null)).Declaration)));
			this.declarationCheckBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("78a296bd-dea1-4b03-98ea-359158369a3a", "When you submit this VAT information you are making a legal declaration that the information is true and complete. A false declaration can result in prosecution.");
			this.declarationCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.declarationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 48, true);
			this.declarationCheckBox.Name = "declarationCheckBox";
			this.declarationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(427, 35, true);
			this.declarationCheckBox.TabIndex = 2;
			this.declarationCheckBox.UseVisualStyleBackColor = true;
			// 
			// confirmButton
			// 
			this.confirmButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("30ff9167-e63d-42c7-8fdb-a72799bfe574", "Confirm");
			this.confirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(147, 117, true);
			this.confirmButton.Name = "confirmButton";
			this.confirmButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.confirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.confirmButton.TabIndex = 4;
			this.confirmButton.ToolTipCaption = null;
			this.confirmButton.UseVisualStyleBackColor = true;
			this.confirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// cancelButton
			// 
			this.cancelButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("de720846-a1ce-44b8-a06c-ac19af2fb148", "Cancel");
			this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.cancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(232, 117, true);
			this.cancelButton.Name = "cancelButton";
			this.cancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.cancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 25, true);
			this.cancelButton.TabIndex = 5;
			this.cancelButton.ToolTipCaption = null;
			this.cancelButton.UseVisualStyleBackColor = true;
			this.cancelButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// errorLabel
			// 
			this.errorLabel.AutoSize = true;
			this.errorLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.errorLabel.ForeColor = System.Drawing.Color.Red;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.errorLabel, false);
			this.errorLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(16, 91, true);
			this.errorLabel.Name = "errorLabel";
			this.errorLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(348, 13, true);
			this.errorLabel.TabIndex = 3;
			this.errorLabel.Text = "You need to accept the declaration before you can submit your VAT return.";
			this.errorLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			this.errorLabel.Visible = false;
			// 
			// MTDDeclarationForm
			// 
			this.AcceptButton = this.confirmButton;
			this.CancelButton = this.cancelButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("78bcbbbc-55c3-4729-ad9a-686862b777e5", "Confirm VAT Return Submission");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(461, 177, true);
			this.Controls.Add(this.cancelButton);
			this.Controls.Add(this.confirmButton);
			this.Controls.Add(this.errorLabel);
			this.Controls.Add(this.declarationCheckBox);
			this.Controls.Add(this.declarationCheckBoxText1);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataColumns);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.ComplianceReport.HMRC.MTDSubmissionDataColumns";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "MTDDeclarationForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.declarationCheckBoxText1, 0);
			this.Controls.SetChildIndex(this.declarationCheckBox, 0);
			this.Controls.SetChildIndex(this.errorLabel, 0);
			this.Controls.SetChildIndex(this.confirmButton, 0);
			this.Controls.SetChildIndex(this.cancelButton, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZLabel declarationCheckBoxText1;
		private ZArchitecture.GUI.ZCheckBox declarationCheckBox;
		private ZArchitecture.ZLabel errorLabel;
		private ZArchitecture.GUI.ZButton confirmButton;
		private ZArchitecture.GUI.ZButton cancelButton;
	}
}
