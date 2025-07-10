using System;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class PeriodSetUpForm
	{


		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.CloseButton = new ZButton();
			this.OKButton = new ZButton();
			this.zDropEdit1 = new ZDropEdit();
			this.zDropEdit2 = new ZDropEdit();
			this.accountingYearBasedDropEdit = new ZDropEdit();
			this.zDateEdit2 = new ZDateEdit();
			this.zDateEdit1 = new ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(193);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(193);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(NewYearPeriodSettings);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodSetUpForm|da33fa07-faaa-4f07-a98a-5669f6f01474", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 148, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CloseButton.TabIndex = 5;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodSetUpForm|acc2f8e4-fef3-44e4-9c72-7cb4f406d0e2", "Continue");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 148, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.Click += new EventHandler(this.OKButton_Click);
			// 
			// zDropEdit1
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit1, "PeriodFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NewYearPeriodSettings)(null)).PeriodFormat)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((NewYearPeriodSettings)(null)).PeriodType)));
			this.zDropEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodSetUpForm|ab08ab0c-be12-4f09-8573-3a0864467b1d", "Period format");
			this.zDropEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 16, true);
			this.zDropEdit1.Name = "zDropEdit1";
			this.zDropEdit1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zDropEdit1.TabIndex = 0;
			// 
			// zDropEdit2
			// 
			this.BindingSource.SetBindingMember(this.zDropEdit2, "WeekDay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NewYearPeriodSettings)(null)).WeekDay)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((NewYearPeriodSettings)(null)).WeekDayType)));
			this.zDropEdit2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodSetUpForm|a4e78375-2b86-46f0-88ce-d9c4125a2f5e", "Period End Week Day");
			this.zDropEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 88, true);
			this.zDropEdit2.Name = "zDropEdit2";
			this.zDropEdit2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.zDropEdit2.TabIndex = 3;
			// 
			// zDropEdit3
			// 
			this.BindingSource.SetBindingMember(this.accountingYearBasedDropEdit, "AccountingYearBasedType");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NewYearPeriodSettings)(null)).AccountingYearBasedType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((NewYearPeriodSettings)(null)).AccountingYearBasedTypes)));
			this.accountingYearBasedDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodSetUpForm|798E0E1E-E278-4F2E-BF21-449AA70DE39F", "Accounting Year Based On");
			this.accountingYearBasedDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 112, true);
			this.accountingYearBasedDropEdit.Name = "accountingYearBasedDropEdit";
			this.accountingYearBasedDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 20, true);
			this.accountingYearBasedDropEdit.TabIndex = 4;
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NewYearPeriodSettings)(null)).EndDate)));
			this.zDateEdit2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodSetUpForm|1c6acb77-5227-4a4f-8b12-815ae8b7aa7b", "End Date");
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 64, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 2;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((NewYearPeriodSettings)(null)).StartDate)));
			this.zDateEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodSetUpForm|408825c8-2010-4e9b-a9d5-bfbd12a9cfd6", "Start Date");
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(146, 40, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 1;
			// 
			// PeriodSetUpForm
			// 
			this.AcceptButton = this.OKButton;

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 200, true);
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("PeriodSetUpForm|5c4f9345-649f-48c7-a9ec-7900783cb435", "Set Periods for Accounting Year");
			this.Controls.Add(this.zDateEdit1);
			this.Controls.Add(this.zDateEdit2);
			this.Controls.Add(this.zDropEdit2);
			this.Controls.Add(this.zDropEdit1);
			this.Controls.Add(this.accountingYearBasedDropEdit);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(NewYearPeriodSettings);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.PeriodManagement.NewYearPeriodSettings";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "PeriodSetUpForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zDropEdit1, 0);
			this.Controls.SetChildIndex(this.zDropEdit2, 0);
			this.Controls.SetChildIndex(this.accountingYearBasedDropEdit, 0);
			this.Controls.SetChildIndex(this.zDateEdit2, 0);
			this.Controls.SetChildIndex(this.zDateEdit1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}