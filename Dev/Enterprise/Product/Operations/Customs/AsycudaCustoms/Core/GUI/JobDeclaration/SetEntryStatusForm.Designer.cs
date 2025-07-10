
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Customs.AsycudaCustoms.GUI
{
	partial class SetEntryStatusForm
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
		private new void InitializeComponent()
		{
			this.CancelAddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.AddButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.EventTimeZDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.EntryZGuidDropEdit = new Enterprise.ZArchitecture.GUI.ZGuidDropEdit();
			this.EntryStatusZDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.EventTimeZDateEdit.SuspendLayout();
			this.EntryZGuidDropEdit.SuspendLayout();
			this.EntryStatusZDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 147, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.SetEntryStatusDetail);
			// 
			// CancelAddButton
			// 
			this.CancelAddButton.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("5E7D1FFC-DAF9-45BB-9C3A-897A4C826045", "Cancel");
			this.CancelAddButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelAddButton.IsCaptionOverridden = false;
			this.CancelAddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(252, 109, true);
			this.CancelAddButton.Name = "CancelAddButton";
			this.CancelAddButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelAddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelAddButton.TabIndex = 7;
			this.CancelAddButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelAddButton.ToolTipCaption = null;
			this.CancelAddButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// AddButton
			// 
			this.AddButton.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("379A0BA5-27BC-4937-82BB-69D0E49A9490", "OK");
			this.CancelAddButton.DialogResult = System.Windows.Forms.DialogResult.OK;
			this.AddButton.IsCaptionOverridden = false;
			this.AddButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(149, 109, true);
			this.AddButton.Name = "AddButton";
			this.AddButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.AddButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.AddButton.TabIndex = 6;
			this.AddButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.AddButton.ToolTipCaption = null;
			this.AddButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// EventTimeZDateEdit
			// 
			this.EventTimeZDateEdit.AllowDrop = true;
			this.EventTimeZDateEdit.AutoCompleteMonthThreshold = 1;
			this.EventTimeZDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EventTimeZDateEdit, "EventTime");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.SetEntryStatusDetail)(null)).EventTime)));
			this.EventTimeZDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.EventTimeZDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 64, true);
			this.EventTimeZDateEdit.Name = "EventTimeZDateEdit";
			this.EventTimeZDateEdit.TabIndex = 5;
			// 
			// EntryZGuidDropEdit
			// 
			this.EntryZGuidDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryZGuidDropEdit, "CusEntryHeaderPK");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.SetEntryStatusDetail)(null)).CusEntryHeaderPK)));
			this.EntryZGuidDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 12, true);
			this.EntryZGuidDropEdit.Name = "EntryZGuidDropEdit";
			this.EntryZGuidDropEdit.PreBoundMaxLength = 30;
			this.EntryZGuidDropEdit.ShouldResizeByMaxLength = false;
			this.EntryZGuidDropEdit.ShowDescriptionBox = false;
			this.EntryZGuidDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.EntryZGuidDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.EntryZGuidDropEdit.TabIndex = 3;
			// 
			// EntryStatusZDropEdit
			// 
			this.EntryStatusZDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.EntryStatusZDropEdit, "EntryStatus");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.AsycudaCustoms.Business.SetEntryStatusDetail)(null)).EntryStatus)));
			this.EntryStatusZDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 38, true);
			this.EntryStatusZDropEdit.Name = "EntryStatusZDropEdit";
			this.EntryStatusZDropEdit.ShouldResizeByMaxLength = true;
			this.EntryStatusZDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.EntryStatusZDropEdit.TabIndex = 4;
			// 
			// SetEntryStatusForm
			// 
			this.AcceptButton = this.AddButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelAddButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.AsycudaCustoms.GUI.Res.GetData("a1f7ddee-bc21-4b73-8a5b-02a4f4b63f2a", "Set Customs Entry Status");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 171, true);
			this.Controls.Add(this.EntryStatusZDropEdit);
			this.Controls.Add(this.CancelAddButton);
			this.Controls.Add(this.EventTimeZDateEdit);
			this.Controls.Add(this.AddButton);
			this.Controls.Add(this.EntryZGuidDropEdit);
			this.DataSourceType = typeof(Enterprise.Customs.AsycudaCustoms.Business.SetEntryStatusDetail);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 200, true);
			this.Name = "SetEntryStatusForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.EntryZGuidDropEdit, 0);
			this.Controls.SetChildIndex(this.AddButton, 0);
			this.Controls.SetChildIndex(this.EventTimeZDateEdit, 0);
			this.Controls.SetChildIndex(this.CancelAddButton, 0);
			this.Controls.SetChildIndex(this.EntryStatusZDropEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.EventTimeZDateEdit.ResumeLayout(true);
			this.EventTimeZDateEdit.PerformLayout();
			this.EntryZGuidDropEdit.ResumeLayout(true);
			this.EntryZGuidDropEdit.PerformLayout();
			this.EntryStatusZDropEdit.ResumeLayout(true);
			this.EntryStatusZDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

	
		private ZArchitecture.GUI.ZButton CancelAddButton;
		private ZArchitecture.GUI.ZButton AddButton;
		protected ZArchitecture.GUI.ZDateEdit EventTimeZDateEdit;
		private ZGuidDropEdit EntryZGuidDropEdit;
		private ZDropEdit EntryStatusZDropEdit;
	}
}
