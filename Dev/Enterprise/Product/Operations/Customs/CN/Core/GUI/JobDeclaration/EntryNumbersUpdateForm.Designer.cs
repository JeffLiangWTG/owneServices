using CargoWise.Types;

namespace Enterprise.Customs.CN.GUI
{
	partial class EntryNumbersUpdateForm
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
			this.UpdateEntryNumbersGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.CIQNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MovementReferenceNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.MovementReferenceNumberDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DeclarationUnifiedNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PreEntryNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.SaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zCancelButton = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UpdateEntryNumbersGroupBox.SuspendLayout();
			this.MovementReferenceNumberDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 149, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 24, true);
			this.MainStatusBar.Visible = false;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.CN.Business.EntryNumbersBO);
			// 
			// UpdateEntryNumbersGroupBox
			// 
			this.UpdateEntryNumbersGroupBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("0a4b9359-8264-4e37-b099-3de30c5984b5", "Update Entry Numbers");
			this.UpdateEntryNumbersGroupBox.Controls.Add(this.CIQNumberTextBox);
			this.UpdateEntryNumbersGroupBox.Controls.Add(this.MovementReferenceNumberTextBox);
			this.UpdateEntryNumbersGroupBox.Controls.Add(this.MovementReferenceNumberDateEdit);
			this.UpdateEntryNumbersGroupBox.Controls.Add(this.DeclarationUnifiedNumberTextBox);
			this.UpdateEntryNumbersGroupBox.Controls.Add(this.PreEntryNumberTextBox);
			this.UpdateEntryNumbersGroupBox.Dock = System.Windows.Forms.DockStyle.Top;
			this.UpdateEntryNumbersGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.UpdateEntryNumbersGroupBox.Name = "UpdateEntryNumbersGroupBox";
			this.UpdateEntryNumbersGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 116, true);
			this.UpdateEntryNumbersGroupBox.TabIndex = 1;
			this.UpdateEntryNumbersGroupBox.TabStop = false;
			// 
			// CIQNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CIQNumberTextBox, "CIQNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EntryNumbersBO)(null)).CIQNumber)));
			this.CIQNumberTextBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("2c1b888b-29b3-4296-91e3-15abe202129b", "CIQ Number");
			this.CIQNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 90, true);
			this.CIQNumberTextBox.Name = "CIQNumberTextBox";
			this.CIQNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 20, true);
			this.CIQNumberTextBox.TabIndex = 4;
			// 
			// MovementReferenceNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.MovementReferenceNumberTextBox, "MovementReferenceNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EntryNumbersBO)(null)).MovementReferenceNumber)));
			this.MovementReferenceNumberTextBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("0b810dc8-d262-4f57-9056-aa28359d68ab", "Customs Entry Number");
			this.MovementReferenceNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 42, true);
			this.MovementReferenceNumberTextBox.Name = "MovementReferenceNumberTextBox";
			this.MovementReferenceNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(187, 20, true);
			this.MovementReferenceNumberTextBox.TabIndex = 1;
			// 
			// MovementReferenceNumberDateEdit
			// 
			this.MovementReferenceNumberDateEdit.AllowDrop = true;
			this.MovementReferenceNumberDateEdit.AutoCompleteMonthThreshold = 1;
			this.MovementReferenceNumberDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.MovementReferenceNumberDateEdit, "MovementReferenceNumberIssueDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.CN.Business.EntryNumbersBO)(null)).MovementReferenceNumberIssueDate)));
			this.MovementReferenceNumberDateEdit.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("318f643e-80fc-4518-90b5-e7acb213370f", "Issue Date");
			this.MovementReferenceNumberDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 42, true);
			this.MovementReferenceNumberDateEdit.Name = "MovementReferenceNumberDateEdit";
			this.MovementReferenceNumberDateEdit.TabIndex = 2;
			// 
			// DeclarationUnifiedNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.DeclarationUnifiedNumberTextBox, "DeclarationUnifiedNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EntryNumbersBO)(null)).DeclarationUnifiedNumber)));
			this.DeclarationUnifiedNumberTextBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("a3ad1e34-f4cf-4e4a-b8bb-8cb422008b2b", "Declaration Unified Num.", "Declaration Unified Number", "");
			this.DeclarationUnifiedNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 18, true);
			this.DeclarationUnifiedNumberTextBox.Name = "DeclarationUnifiedNumberTextBox";
			this.DeclarationUnifiedNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 20, true);
			this.DeclarationUnifiedNumberTextBox.TabIndex = 0;
			// 
			// PreEntryNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.PreEntryNumberTextBox, "PreEntryNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.CN.Business.EntryNumbersBO)(null)).PreEntryNumber)));
			this.PreEntryNumberTextBox.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("f7feb0ec-2eb1-4dcb-b358-788f0c91ee7b", "Pre Entry Number");
			this.PreEntryNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(131, 66, true);
			this.PreEntryNumberTextBox.Name = "PreEntryNumberTextBox";
			this.PreEntryNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(278, 20, true);
			this.PreEntryNumberTextBox.TabIndex = 3;
			// 
			// SaveButton
			//
			this.SaveButton.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("B2D5E25B-C65C-4285-95B8-E3B0D441B2AE", "Save");
			this.SaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(253, 122, true);
			this.SaveButton.Name = "SaveButton";
			this.SaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.SaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.SaveButton.TabIndex = 2;
			this.SaveButton.ToolTipCaption = null;
			this.SaveButton.Click += new System.EventHandler(this.SaveButton_Click);
			// 
			// zCancelButton
			//
			this.zCancelButton.CaptionResourceString = Enterprise.Customs.CN.GUI.Res.GetData("B391C702-A1E7-4776-A8E9-70BD4C150EC3", "Cancel");
			this.zCancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(334, 122, true);
			this.zCancelButton.Name = "zCancelButton";
			this.zCancelButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.zCancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.zCancelButton.TabIndex = 3;
			this.zCancelButton.ToolTipCaption = null;
			this.zCancelButton.Click += new System.EventHandler(this.zCancelButton_Click);
			// 
			// EntryNumbersUpdateForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(421, 173, true);
			this.Controls.Add(this.UpdateEntryNumbersGroupBox);
			this.Controls.Add(this.zCancelButton);
			this.Controls.Add(this.SaveButton);
			this.DataSourceType = typeof(Enterprise.Customs.CN.Business.EntryNumbersBO);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.SizableToolWindow;
			this.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 212, true);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(437, 212, true);
			this.Name = "EntryNumbersUpdateForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
			this.Controls.SetChildIndex(this.SaveButton, 0);
			this.Controls.SetChildIndex(this.zCancelButton, 0);
			this.Controls.SetChildIndex(this.UpdateEntryNumbersGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.UpdateEntryNumbersGroupBox.ResumeLayout(false);
			this.UpdateEntryNumbersGroupBox.PerformLayout();
			this.MovementReferenceNumberDateEdit.ResumeLayout(true);
			this.MovementReferenceNumberDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		public Enterprise.ZArchitecture.GUI.ZButton SaveButton;
		public Enterprise.ZArchitecture.ZTextBox PreEntryNumberTextBox;
		private Enterprise.ZArchitecture.GUI.ZGroupBox UpdateEntryNumbersGroupBox;
		public Enterprise.ZArchitecture.ZTextBox DeclarationUnifiedNumberTextBox;
		public Enterprise.ZArchitecture.ZTextBox MovementReferenceNumberTextBox;
		public ZArchitecture.GUI.ZButton zCancelButton;
		public ZArchitecture.GUI.ZDateEdit MovementReferenceNumberDateEdit;
		public ZArchitecture.ZTextBox CIQNumberTextBox;
	}
}
