using System.Windows.Forms;
using Enterprise.Accounting.Business.PeriodManagement;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	public partial class ExtendLastFinancialYearForm
	{


		#region Windows Form Designer generated code

		new void InitializeComponent()
		{
			this.CloseButton = new ZButton();
			this.OKButton = new ZButton();
			this.zPeriodFormatDropEdit = new ZDropEdit();
			this.zEndDateEdit = new ZDateEdit();
			this.zStartDateEdit = new ZDateEdit();
			this.zFinancialYearTextBox = new ZArchitecture.ZTextBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.zPeriodFormatDropEdit.SuspendLayout();
			this.zEndDateEdit.SuspendLayout();
			this.zStartDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 149, true);
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
			this.BindingSource.DataSourceType = typeof(ExtendLastFinancialYearSettings);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExtendLastFinancialYearForm|F2825443-FDF1-4D2B-9788-03547EC964FC", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(306, 121, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CloseButton.TabIndex = 5;
			this.CloseButton.ToolTipCaption = null;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExtendLastFinancialYearForm|64AF583A-ED84-40B1-816D-F6173A9A13A9", "Continue");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(226, 121, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// zPeriodFormatDropEdit
			// 
			this.zPeriodFormatDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.zPeriodFormatDropEdit, "PeriodFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ExtendLastFinancialYearSettings)(null)).PeriodFormat)));
			this.zPeriodFormatDropEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExtendLastFinancialYearForm|BB02B1ED-F669-4514-93AE-052B76026BD2", "Period format");
			this.zPeriodFormatDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 38, true);
			this.zPeriodFormatDropEdit.Name = "zPeriodFormatDropEdit";
			this.zPeriodFormatDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(232, 17, true);
			this.zPeriodFormatDropEdit.TabIndex = 2;
			// 
			// zEndDateEdit
			// 
			this.zEndDateEdit.AllowDrop = true;
			this.zEndDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zEndDateEdit, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ExtendLastFinancialYearSettings)(null)).EndDate)));
			this.zEndDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExtendLastFinancialYearForm|96800BB4-7B44-4FF0-BEB0-2328F0A225E5", "Financial Year End Date");
			this.zEndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 89, true);
			this.zEndDateEdit.Name = "zEndDateEdit";
			this.zEndDateEdit.TabIndex = 4;
			// 
			// zStartDateEdit
			// 
			this.zStartDateEdit.AllowDrop = true;
			this.zStartDateEdit.AutoCompleteMonthThreshold = 1;
			this.BindingSource.SetBindingMember(this.zStartDateEdit, "StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((ExtendLastFinancialYearSettings)(null)).StartDate)));
			this.zStartDateEdit.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExtendLastFinancialYearForm|D8489522-0CCF-480D-AD21-5BC516AB9BA6", "Financial Year Start Date");
			this.zStartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 63, true);
			this.zStartDateEdit.Name = "zStartDateEdit";
			this.zStartDateEdit.ReadOnly = true;
			this.zStartDateEdit.TabIndex = 3;
			// 
			// zFinancialYearTextBox
			// 
			this.BindingSource.SetBindingMember(this.zFinancialYearTextBox, "FinancialYear");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZShort)(((ExtendLastFinancialYearSettings)(null)).FinancialYear)));
			this.zFinancialYearTextBox.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExtendLastFinancialYearForm|4FFA33D7-1882-4AC1-9534-864B27746FAC", "Financial year");
			this.zFinancialYearTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 12, true);
			this.zFinancialYearTextBox.Name = "zFinancialYearTextBox";
			this.zFinancialYearTextBox.ReadOnly = true;
			this.zFinancialYearTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(57, 17, true);
			this.zFinancialYearTextBox.TabIndex = 1;
			// 
			// ExtendLastFinancialYearForm
			// 
			this.AcceptButton = this.OKButton;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("ExtendLastFinancialYearForm|D91DCDB3-55A9-428D-8EE0-9DE7C427581C", "Modify Financial Year Range");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(396, 173, true);
			this.Controls.Add(this.zStartDateEdit);
			this.Controls.Add(this.zEndDateEdit);
			this.Controls.Add(this.zPeriodFormatDropEdit);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.zFinancialYearTextBox);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(ExtendLastFinancialYearSettings);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.PeriodManagement.ExtendLastFinancialYearSettings";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ExtendLastFinancialYearForm";
			this.Controls.SetChildIndex(this.zFinancialYearTextBox, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.zPeriodFormatDropEdit, 0);
			this.Controls.SetChildIndex(this.zEndDateEdit, 0);
			this.Controls.SetChildIndex(this.zStartDateEdit, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zPeriodFormatDropEdit.ResumeLayout(true);
			this.zPeriodFormatDropEdit.PerformLayout();
			this.zEndDateEdit.ResumeLayout(true);
			this.zEndDateEdit.PerformLayout();
			this.zStartDateEdit.ResumeLayout(true);
			this.zStartDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion

	}
}