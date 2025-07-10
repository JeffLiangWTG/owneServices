using CargoWise.ComponentModel;
using Enterprise.Accounting.GeneralLedgerData.Business;
using Enterprise.ZArchitecture.Environment;

namespace Enterprise.Accounting.GUI.PeriodManagement
{
	partial class GldDateRangeForm<T>
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

		#region Component Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zDateEdit2 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.zDateEdit1 = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 152, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 24, true);
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
			this.BindingSource.DataSourceType = typeof(Enterprise.Accounting.Business.PeriodManagement.GldDateRangeSetting);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.CloseButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GldDateRangeForm|30696507-0F5F-47A8-889B-145BEAF5E520", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(296, 124, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.CloseButton.TabIndex = 5;
			// 
			// OKButton
			// 
			this.OKButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.OKButton.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GldDateRangeForm|2F2CBD9C-24C3-4C4B-A7F3-89B8A13C61A6", "Update");
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(216, 124, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 24, true);
			this.OKButton.TabIndex = 4;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// zDateEdit2
			// 
			this.zDateEdit2.AutoCompleteMonthThreshold = 1;
			this.zDateEdit2.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit2, "EndDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PeriodManagement.GldDateRangeSetting)(null)).EndDate)));
			this.zDateEdit2.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GldDateRangeForm|9E970F1F-E856-4D68-B81D-C36C18E44F72", "End Date");
			this.zDateEdit2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 64, true);
			this.zDateEdit2.Name = "zDateEdit2";
			this.zDateEdit2.TabIndex = 2;
			// 
			// zDateEdit1
			// 
			this.zDateEdit1.AutoCompleteMonthThreshold = 1;
			this.zDateEdit1.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.zDateEdit1, "StartDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Accounting.Business.PeriodManagement.GldDateRangeSetting)(null)).StartDate)));
			this.zDateEdit1.CaptionResourceString = Enterprise.Accounting.GUI.Res.GetData("GldDateRangeForm|B9F1B8CF-02E6-4FAD-970C-65C3CAF54A11", "Start Date");
			this.zDateEdit1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 40, true);
			this.zDateEdit1.Name = "zDateEdit1";
			this.zDateEdit1.TabIndex = 1;

			this.AcceptButton = this.OKButton;

			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(386, 176, true);
			this.Controls.Add(this.zDateEdit1);
			this.Controls.Add(this.zDateEdit2);
			this.Controls.Add(this.CloseButton);
			this.Controls.Add(this.OKButton);
			this.DataSourceAssemblyName = "Enterprise.Accounting.Business";
			this.DataSourceType = typeof(Enterprise.Accounting.Business.PeriodManagement.GldDateRangeSetting);
			this.DataSourceTypeName = "Enterprise.Accounting.Business.PeriodManagement.UpdateGldPeriodsDateRangeSetting";
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "GldDateRangeForm";
			this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CloseButton, 0);
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
