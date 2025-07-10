
namespace Enterprise.Client.UPE.GUI
{
	partial class ClassifierAssignmentForm
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
			this.StaffToReplaceLabel = new Enterprise.ZArchitecture.ZLabel();
			this.NewStaffLabel = new Enterprise.ZArchitecture.ZLabel();
			this.StaffToReplaceFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.NewStaffFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 146, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 26, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(137);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(138);
			// 
			// StaffToReplaceLabel
			// 
			this.StaffToReplaceLabel.AutoSize = true;
			this.StaffToReplaceLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 9, true);
			this.StaffToReplaceLabel.Name = "StaffToReplaceLabel";
			this.StaffToReplaceLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(113, 13, true);
			this.StaffToReplaceLabel.TabIndex = 1;
			this.StaffToReplaceLabel.Text = "Select staff to replace";
			// 
			// NewStaffLabel
			// 
			this.NewStaffLabel.AutoSize = true;
			this.NewStaffLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 69, true);
			this.NewStaffLabel.Name = "NewStaffLabel";
			this.NewStaffLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(167, 13, true);
			this.NewStaffLabel.TabIndex = 3;
			this.NewStaffLabel.Text = "Select new staff for classifier role";
			// 
			// StaffToReplaceFindBox
			// 
			this.StaffToReplaceFindBox.AllowDrop = true;
			this.StaffToReplaceFindBox.BindTo = "StaffPKToReplace";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.Business.UPEStaffAssignmentUpdater)(null)).StaffPKToReplace)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPEStaffAssignmentUpdater)(null)).StaffPKToReplaceInfo)));
			this.StaffToReplaceFindBox.BindToList = "Staff";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEStaffAssignmentUpdater)(null)).Staff)));
			this.StaffToReplaceFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 28, true);
			this.StaffToReplaceFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.StaffToReplaceFindBox.Name = "StaffToReplaceFindBox";
			this.StaffToReplaceFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.StaffToReplaceFindBox.TabIndex = 2;
			// 
			// NewStaffFindBox
			// 
			this.NewStaffFindBox.AllowDrop = true;
			this.NewStaffFindBox.BindTo = "NewStaffPK";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.Types.ZGuid)(((Enterprise.Client.UPE.Business.UPEStaffAssignmentUpdater)(null)).NewStaffPK)));
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((CargoWise.EntityFramework.ZPropertyInfo)(((Enterprise.Client.UPE.Business.UPEStaffAssignmentUpdater)(null)).NewStaffPKInfo)));
			this.NewStaffFindBox.BindToList = "Staff";
			// Compile time check for the above BindTo. If this line fails, DO NOT modify this code. ALWAYS use the designer.
			CargoWise.EntityFramework.ZBindToChecker.CheckBindTo(((System.Collections.IList)(((Enterprise.Client.UPE.Business.UPEStaffAssignmentUpdater)(null)).Staff)));
			this.NewStaffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(15, 88, true);
			this.NewStaffFindBox.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
			this.NewStaffFindBox.Name = "NewStaffFindBox";
			this.NewStaffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.NewStaffFindBox.TabIndex = 4;
			// 
			// CloseButton
			// 
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(180, 116, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 7;
			this.CloseButton.Text = "Cancel";
			this.CloseButton.UseVisualStyleBackColor = true;
			// 
			// OKButton
			// 
			this.OKButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(99, 116, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OKButton.TabIndex = 6;
			this.OKButton.Text = "OK";
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// ClassifierAssignmentForm
			// 
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 172, true);
			this.Controls.Add(this.StaffToReplaceLabel);
			this.Controls.Add(this.StaffToReplaceFindBox);
			this.Controls.Add(this.NewStaffFindBox);
			this.Controls.Add(this.NewStaffLabel);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "ZClientUPE";
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.UPEStaffAssignmentUpdater";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ClassifierAssignmentForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.NewStaffLabel, 0);
			this.Controls.SetChildIndex(this.NewStaffFindBox, 0);
			this.Controls.SetChildIndex(this.StaffToReplaceFindBox, 0);
			this.Controls.SetChildIndex(this.StaffToReplaceLabel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private Enterprise.ZArchitecture.ZLabel StaffToReplaceLabel;
		private Enterprise.ZArchitecture.ZLabel NewStaffLabel;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox StaffToReplaceFindBox;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox NewStaffFindBox;
		private Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private Enterprise.ZArchitecture.GUI.ZButton OKButton;
	}
}