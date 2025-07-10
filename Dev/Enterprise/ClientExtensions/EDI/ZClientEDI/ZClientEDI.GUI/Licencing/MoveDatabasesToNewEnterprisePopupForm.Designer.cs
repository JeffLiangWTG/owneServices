
namespace Enterprise.Client.EDI.LicenceKeyBuilder.GUI
{
	partial class MoveDatabasesToNewEnterprisePopupForm
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

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MoveDatabasesToNewEnterprisePopupForm));
			this.ButtonOk = new Enterprise.ZArchitecture.GUI.ZButton();
			this.ButtonCancel = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.LicenceEnterpriseIDFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
			this.AllowAutoLoginCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.StatusDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.LicenceEnterpriseIDFindBox.SuspendLayout();
			this.StatusDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 301, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 0, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.MoveDatabasesToNewEnterpriseBizObj);
			// 
			// ButtonOk
			// 
			this.ButtonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonOk.IsCaptionOverridden = true;
			this.ButtonOk.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(372, 267, true);
			this.ButtonOk.Name = "ButtonOk";
			this.ButtonOk.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonOk.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonOk.TabIndex = 4;
			this.ButtonOk.Text = "OK";
			this.ButtonOk.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonOk.ToolTipCaption = null;
			this.ButtonOk.UseVisualStyleBackColor = true;
			this.ButtonOk.Click += new System.EventHandler(this.ButtonOk_Click);
			// 
			// ButtonCancel
			// 
			this.ButtonCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.ButtonCancel.IsCaptionOverridden = true;
			this.ButtonCancel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(453, 267, true);
			this.ButtonCancel.Name = "ButtonCancel";
			this.ButtonCancel.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ButtonCancel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.ButtonCancel.TabIndex = 5;
			this.ButtonCancel.Text = "Cancel";
			this.ButtonCancel.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.ButtonCancel.ToolTipCaption = null;
			this.ButtonCancel.UseVisualStyleBackColor = true;
			// 
			// zLabel1
			// 
			this.zLabel1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.zLabel1.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(527, 136, true);
			this.zLabel1.TabIndex = 0;
			this.zLabel1.Text = resources.GetString("zLabel1.Text");
			// 
			// LicenceEnterpriseIDFindBox
			// 
			this.LicenceEnterpriseIDFindBox.AllowDrop = true;
			this.LicenceEnterpriseIDFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.LicenceEnterpriseIDFindBox, "LicenceEnterpriseID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.MoveDatabasesToNewEnterpriseBizObj)(null)).LicenceEnterpriseID)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.MoveDatabasesToNewEnterpriseBizObj)(null)).LicenceEnterpriseList)));
			this.LicenceEnterpriseIDFindBox.BindToList = "LicenceEnterpriseList";
			this.LicenceEnterpriseIDFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 163, true);
			this.LicenceEnterpriseIDFindBox.Name = "LicenceEnterpriseIDFindBox";
			this.LicenceEnterpriseIDFindBox.ShouldResize = true;
			this.LicenceEnterpriseIDFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 15, true);
			this.LicenceEnterpriseIDFindBox.TabIndex = 1;
			// 
			// AllowAutoLoginCheckBox
			// 
			this.AllowAutoLoginCheckBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.AllowAutoLoginCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.AllowAutoLoginCheckBox, "AllowWebAutoLogin");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.MoveDatabasesToNewEnterpriseBizObj)(null)).AllowWebAutoLogin)));
			this.AllowAutoLoginCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.AllowAutoLoginCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AllowAutoLoginCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 235, true);
			this.AllowAutoLoginCheckBox.Name = "AllowAutoLoginCheckBox";
			this.AllowAutoLoginCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 13, true);
			this.AllowAutoLoginCheckBox.TabIndex = 3;
			this.AllowAutoLoginCheckBox.UseVisualStyleBackColor = true;
			// 
			// StatusDropEdit
			// 
			this.StatusDropEdit.AllowDrop = true;
			this.StatusDropEdit.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.StatusDropEdit, "RegistrationStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.EDI.LicenceKeyBuilder.Business.MoveDatabasesToNewEnterpriseBizObj)(null)).RegistrationStatus)));
			this.StatusDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(127, 199, true);
			this.StatusDropEdit.Name = "StatusDropEdit";
			this.StatusDropEdit.PreBoundMaxLength = 3;
			this.StatusDropEdit.ShouldResizeByMaxLength = true;
			this.StatusDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 15, true);
			this.StatusDropEdit.TabIndex = 2;
			// 
			// MoveDatabasesToNewEnterprisePopupForm
			// 
			this.AcceptButton = this.ButtonOk;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.ButtonCancel;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(548, 301, true);
			this.Controls.Add(this.StatusDropEdit);
			this.Controls.Add(this.AllowAutoLoginCheckBox);
			this.Controls.Add(this.LicenceEnterpriseIDFindBox);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.ButtonCancel);
			this.Controls.Add(this.ButtonOk);
			this.DataSourceType = typeof(Enterprise.Client.EDI.LicenceKeyBuilder.Business.MoveDatabasesToNewEnterpriseBizObj);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "MoveDatabasesToNewEnterprisePopupForm";
			this.RememberFormPosition = false;
			this.RememberFormSize = false;
			this.Text = "Set Enterprise ID/Code";
			this.Controls.SetChildIndex(this.ButtonOk, 0);
			this.Controls.SetChildIndex(this.ButtonCancel, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.LicenceEnterpriseIDFindBox, 0);
			this.Controls.SetChildIndex(this.AllowAutoLoginCheckBox, 0);
			this.Controls.SetChildIndex(this.StatusDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.LicenceEnterpriseIDFindBox.ResumeLayout(true);
			this.LicenceEnterpriseIDFindBox.PerformLayout();
			this.StatusDropEdit.ResumeLayout(true);
			this.StatusDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZButton ButtonOk;
		private Enterprise.ZArchitecture.GUI.ZButton ButtonCancel;
		private Enterprise.ZArchitecture.ZLabel zLabel1;
		private Enterprise.ZArchitecture.GUI.ZCodeFindBox LicenceEnterpriseIDFindBox;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AllowAutoLoginCheckBox;
		private Enterprise.ZArchitecture.GUI.ZDropEdit StatusDropEdit;
	}
}
