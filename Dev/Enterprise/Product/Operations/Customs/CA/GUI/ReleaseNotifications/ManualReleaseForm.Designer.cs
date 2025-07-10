using System;
using CargoWise.Types;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.CA.GUI
{
	partial class ManualReleaseForm
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
		protected new void InitializeComponent()
		{
			this.DeleteButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.RNSManualReleaseGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.ReleaseDateZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ReleaseReasonTextBox = new Enterprise.ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.RNSManualReleaseGroupBox.SuspendLayout();
			this.ReleaseDateZDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 120, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CA.Business.ManualReleaseCancelBO);
			// 
			// DeleteButton
			// 
			this.DeleteButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.DeleteButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(436, 96, true);
			this.DeleteButton.Name = "DeleteButton";
			this.DeleteButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.DeleteButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.DeleteButton.TabIndex = 3;
			this.DeleteButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ManualReleaseForm|02CC8B0E-ECD6-4488-B0F7-315825AED661", "Delete");
			this.DeleteButton.UseVisualStyleBackColor = true;
			this.DeleteButton.Click += new System.EventHandler(this.DeleteButton_Click);
			// 
			// SaveButton
			// 
			this.SaveButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(355, 96, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ManualReleaseForm|82B5F50F-7B5F-4539-B1B4-CB8E45BC3340", "Save");
			this.SaveButton.UseVisualStyleBackColor = true;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// RNSManualReleaseGroupBox
			// 
			this.RNSManualReleaseGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.RNSManualReleaseGroupBox.Controls.Add(this.ReleaseDateZDateEdit);
			this.RNSManualReleaseGroupBox.Controls.Add(this.ReleaseReasonTextBox);
			this.RNSManualReleaseGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 2, true);
			this.RNSManualReleaseGroupBox.Name = "RNSManualReleaseGroupBox";
			this.RNSManualReleaseGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(511, 88, true);
			this.RNSManualReleaseGroupBox.TabIndex = 1;
			this.RNSManualReleaseGroupBox.TabStop = false;
			this.RNSManualReleaseGroupBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ManualReleaseForm|8F21A792-EC66-4059-9E7F-C765E965084B", "Manual Release");
			// 
			// ReleaseDateZDateEdit
			// 
			this.ReleaseDateZDateEdit.AllowDrop = true;
			this.ReleaseDateZDateEdit.AutoCompleteMonthThreshold = 1;
			this.ReleaseDateZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.ReleaseDateZDateEdit, "ManualReleaseDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZDateTime)(((Enterprise.Customs.CA.Business.ManualReleaseCancelBO)(null)).ManualReleaseDate)));
			this.ReleaseDateZDateEdit.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ManualReleaseForm|6BEDF4D5-AAC3-47DF-8CA3-6227AA181925", "Manual Release Date");
			this.ReleaseDateZDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.ReleaseDateZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 58, true);
			this.ReleaseDateZDateEdit.Name = "ReleaseDateZDateEdit";
			this.ReleaseDateZDateEdit.TabIndex = 1;
			// 
			// ReleaseReasonTextBox
			// 
			this.ReleaseReasonTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.ReleaseReasonTextBox, "ManualReleaseReason");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((ZString)(((Enterprise.Customs.CA.Business.ManualReleaseCancelBO)(null)).ManualReleaseReason)));
			this.ReleaseReasonTextBox.CaptionResourceString = Enterprise.Customs.CA.GUI.Res.GetData("ManualReleaseForm|B1C47915-9771-4B8E-AEFF-6CC850AA149F", "Manual Release Reason");
			this.ReleaseReasonTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(142, 20, true);
			this.ReleaseReasonTextBox.Name = "ReleaseReasonTextBox";
			this.ReleaseReasonTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(363, 20, true);
			this.ReleaseReasonTextBox.TabIndex = 0;
			// 
			// ManualReleaseForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(517, 144, true);
			this.Controls.Add(this.RNSManualReleaseGroupBox);
			this.Controls.Add(this.DeleteButton);
			this.Controls.Add(this.SaveButton);
			this.DataSourceType = typeof(Enterprise.Customs.CA.Business.RNSRequestBO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 183, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(533, 183, true);
			this.Name = "ManualReleaseForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.DeleteButton, 0);
			this.Controls.SetChildIndex(this.RNSManualReleaseGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.RNSManualReleaseGroupBox.ResumeLayout(false);
			this.RNSManualReleaseGroupBox.PerformLayout();
			this.ReleaseDateZDateEdit.ResumeLayout(true);
			this.ReleaseDateZDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public ZButton DeleteButton;
		public ZButton SaveButton;
		private ZGroupBox RNSManualReleaseGroupBox;
		public ZDateEdit ReleaseDateZDateEdit;
		public ZTextBox ReleaseReasonTextBox;
	}
}
