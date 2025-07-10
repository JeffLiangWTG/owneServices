using System;
using System.Windows.Forms;
using Enterprise.Core.Forms;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class GlPeriodForm
	{


		#region Windows Form Designer generated code
		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.TabControl = new ZTemplateTabControl();
			this.DetailsTabPage = new ZTabPage();
			this.ReopenForAdjustmentsCheckBox = new ZCheckBox();
			this.ReopenGeneralLedgerPeriodCheckBox = new ZCheckBox();
			this.ReopenSubLedgerPeriodCheckBox = new ZCheckBox();
			this.PeriodEdit = new ZPeriodEdit();
			this.EndDateEdit = new ZDateEdit();
			this.StartDateEdit = new ZDateEdit();
			this.YearEdit = new ZYearEdit();
			this.LogsTabPage = new ZLogsTabPage();
			this.CloseButton = new ZPostOrCancelButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.TabControl.SuspendLayout();
			this.DetailsTabPage.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 190, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 22, true);
			this.MainStatusBar.TabIndex = 3;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(256);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(257);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(AccPeriodManagement);
			// 
			// TabControl
			// 
			this.TabControl.Controls.Add(this.DetailsTabPage);
			this.TabControl.Controls.Add(this.LogsTabPage);
			this.TabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.TabControl.Name = "TabControl";
			this.TabControl.SelectedIndex = 0;
			this.TabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 157, true);
			this.TabControl.TabIndex = 0;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.DetailsTabPage.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GlPeriodForm|639612f7-4f16-45ff-8654-ea8ff0e6ee75", "Period Details");
			this.DetailsTabPage.Controls.Add(this.ReopenForAdjustmentsCheckBox);
			this.DetailsTabPage.Controls.Add(this.ReopenGeneralLedgerPeriodCheckBox);
			this.DetailsTabPage.Controls.Add(this.ReopenSubLedgerPeriodCheckBox);
			this.DetailsTabPage.Controls.Add(this.PeriodEdit);
			this.DetailsTabPage.Controls.Add(this.EndDateEdit);
			this.DetailsTabPage.Controls.Add(this.StartDateEdit);
			this.DetailsTabPage.Controls.Add(this.YearEdit);
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 130, true);
			this.DetailsTabPage.TabIndex = 0;
			// 
			// ReopenForAdjustmentsCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ReopenForAdjustmentsCheckBox, "AM_IsSubledgerClosedForAdjustments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AccPeriodManagement)(null)).AM_IsSubledgerClosedForAdjustments)));
			this.ReopenForAdjustmentsCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenForAdjustmentsCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 15, true);
			this.ReopenForAdjustmentsCheckBox.Name = "ReopenForAdjustmentsCheckBox";
			this.ReopenForAdjustmentsCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 22, true);
			this.ReopenForAdjustmentsCheckBox.TabIndex = 5;
			// 
			// ReopenGeneralLedgerPeriodCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ReopenGeneralLedgerPeriodCheckBox, "AM_IsGeneralLedgerClosed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AccPeriodManagement)(null)).AM_IsGeneralLedgerClosed)));
			this.ReopenGeneralLedgerPeriodCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenGeneralLedgerPeriodCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 69, true);
			this.ReopenGeneralLedgerPeriodCheckBox.Name = "ReopenGeneralLedgerPeriodCheckBox";
			this.ReopenGeneralLedgerPeriodCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 22, true);
			this.ReopenGeneralLedgerPeriodCheckBox.TabIndex = 7;
			// 
			// ReopenSubLedgerPeriodCheckBox
			// 
			this.BindingSource.SetBindingMember(this.ReopenSubLedgerPeriodCheckBox, "AM_IsSubLedgerClosed");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((AccPeriodManagement)(null)).AM_IsSubLedgerClosed)));
			this.ReopenSubLedgerPeriodCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ReopenSubLedgerPeriodCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(235, 41, true);
			this.ReopenSubLedgerPeriodCheckBox.Name = "ReopenSubLedgerPeriodCheckBox";
			this.ReopenSubLedgerPeriodCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(160, 22, true);
			this.ReopenSubLedgerPeriodCheckBox.TabIndex = 6;
			// 
			// PeriodEdit
			// 
			this.PeriodEdit.BackColor = System.Drawing.SystemColors.Window;
			this.BindingSource.SetBindingMember(this.PeriodEdit, "AM_Period");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZInt)(((AccPeriodManagement)(null)).AM_Period)));
			this.PeriodEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 41, true);
			this.PeriodEdit.Name = "PeriodEdit";
			this.PeriodEdit.TabIndex = 2;
			// 
			// EndDateEdit
			// 
			this.EndDateEdit.AutoCompleteMonthThreshold = 1;
			this.EndDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.EndDateEdit, "AM_EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccPeriodManagement)(null)).AM_EndDate)));
			this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 95, true);
			this.EndDateEdit.Name = "EndDateEdit";
			this.EndDateEdit.TabIndex = 4;
			// 
			// StartDateEdit
			// 
			this.StartDateEdit.AutoCompleteMonthThreshold = 1;
			this.StartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.StartDateEdit, "AM_StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((AccPeriodManagement)(null)).AM_StartDate)));
			this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 69, true);
			this.StartDateEdit.Name = "StartDateEdit";
			this.StartDateEdit.TabIndex = 3;
			// 
			// YearEdit
			// 
			this.BindingSource.SetBindingMember(this.YearEdit, "AM_Year");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((AccPeriodManagement)(null)).AM_Year)));
			this.YearEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(90, 15, true);
			this.YearEdit.Name = "YearEdit";
			this.YearEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.YearEdit.TabIndex = 1;
			this.YearEdit.TextAlign = HorizontalAlignment.Left;
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.BackColor = System.Drawing.SystemColors.Control;
			this.LogsTabPage.ExcludeFromBindingOnSave = true;
			this.LogsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.LogsTabPage.Name = "LogsTabPage";
			this.LogsTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(401, 130, true);
			this.LogsTabPage.TabIndex = 1;
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GlPeriodForm|898a3fae-8cec-4b3c-8450-c8583f88f061", "Close");
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(330, 163, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			this.CloseButton.TabIndex = 8;
			this.CloseButton.Click += new EventHandler(this.CloseButton_Click);
			// 
			// GlPeriodForm
			// 

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(409, 212, true);
			this.Controls.Add(this.TabControl);
			this.Controls.Add(this.CloseButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(AccPeriodManagement);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.GlPeriod.BaseGlPeriod";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.IsPostOnly = true;
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(415, 238, true);
			this.Name = "GlPeriodForm";
			this.RememberFormSize = false;
			this.ShouldSerializeTabPageMethods = false;
			this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.TabControl, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.TabControl.ResumeLayout(false);
			this.DetailsTabPage.ResumeLayout(false);
			this.DetailsTabPage.PerformLayout();
			this.ResumeLayout(false);
		}

		#endregion

	}
}