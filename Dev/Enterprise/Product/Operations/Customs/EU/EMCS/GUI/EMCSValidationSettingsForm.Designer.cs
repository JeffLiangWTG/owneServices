using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.EU.EMCS.GUI
{
	public partial class EMCSValidationSettingsForm
	{
		private new void InitializeComponent()
		{
			this.ExplanationOnReasonForShortageCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.ConfirmButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 83, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			// 
			// ExplanationOnReasonForShortageCheckBox
			// 
			this.ExplanationOnReasonForShortageCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.ExplanationOnReasonForShortageCheckBox, "ZG_ExplanationOnReasonForShortageValidation");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration)(null)).ZG_ExplanationOnReasonForShortageValidation)));
			this.ExplanationOnReasonForShortageCheckBox.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("A8343481-A68C-476E-A16B-CBEDC3D5A37A", "Explanation on Reason for Shortage");
			this.ExplanationOnReasonForShortageCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ExplanationOnReasonForShortageCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 12, true);
			this.ExplanationOnReasonForShortageCheckBox.Name = "ExplanationOnReasonForShortageCheckBox";
			this.ExplanationOnReasonForShortageCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(197, 17, true);
			this.ExplanationOnReasonForShortageCheckBox.TabIndex = 0;
			this.ExplanationOnReasonForShortageCheckBox.UseVisualStyleBackColor = true;
			// 
			// ConfirmButton
			// 
			this.ConfirmButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ConfirmButton.CaptionResourceString = Enterprise.Customs.EU.EMCS.GUI.Res.GetData("36B8CDE6-C303-49D8-A4AF-ABA8398602B1", "OK");
			this.ConfirmButton.IsCaptionOverridden = false;
			this.ConfirmButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 52, true);
			this.ConfirmButton.Name = "ConfirmButton";
			this.ConfirmButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ConfirmButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 25, true);
			this.ConfirmButton.TabIndex = 0;
			this.ConfirmButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ConfirmButton.ToolTipCaption = null;
			this.ConfirmButton.Click += new System.EventHandler(this.ConfirmButton_Click);
			// 
			// EMCSValidationSettingsForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(455, 107, true);
			this.Controls.Add(this.ExplanationOnReasonForShortageCheckBox);
			this.Controls.Add(this.ConfirmButton);
			this.DataSourceType = typeof(Enterprise.Customs.EU.EMCS.Business.EMCSJobDeclaration);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "EMCSValidationSettingsForm";
			this.Controls.SetChildIndex(this.ConfirmButton, 0);
			this.Controls.SetChildIndex(this.ExplanationOnReasonForShortageCheckBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		ZCheckBox ExplanationOnReasonForShortageCheckBox;
		Enterprise.ZArchitecture.GUI.ZButton ConfirmButton;
	}
}
