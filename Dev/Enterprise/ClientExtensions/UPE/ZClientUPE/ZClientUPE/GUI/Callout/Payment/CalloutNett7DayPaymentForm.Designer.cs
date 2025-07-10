using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutNett7DayPaymentForm : Enterprise.Client.UPE.GUI.CalloutPaymentDetailsForm
	{
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton1;
		Enterprise.ZArchitecture.GUI.ZRadioButton zRadioButton2;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zRadioButton1 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.zRadioButton2 = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// TotalAmountCalcEdit
			// 
			this.TotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 69, true);
			// 
			// TotalAmountLabel
			// 
			this.TotalAmountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(30, 68, true);
			this.TotalAmountLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(72, 95, true);
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			// 
			// CancelButton
			// 
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(153, 95, true);
			this.CancelButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 21, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 122, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 23, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(116);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(117);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.CalloutNett7DayPayment);
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 7, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 22, true);
			this.zLabel1.TabIndex = 9;
			this.zLabel1.Text = "Nett 7 Day Type";
			// 
			// zRadioButton1
			// 
			this.zRadioButton1.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.zRadioButton1, "IsBusiness");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.CalloutNett7DayPayment)(null)).IsBusiness)));
			this.zRadioButton1.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 8, true);
			this.zRadioButton1.Name = "zRadioButton1";
			this.zRadioButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 23, true);
			this.zRadioButton1.TabIndex = 0;
			this.zRadioButton1.Text = "Business";
			// 
			// zRadioButton2
			// 
			this.zRadioButton2.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.zRadioButton2, "IsResidential");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Client.UPE.Business.CalloutNett7DayPayment)(null)).IsResidential)));
			this.zRadioButton2.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.zRadioButton2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(108, 31, true);
			this.zRadioButton2.Name = "zRadioButton2";
			this.zRadioButton2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 22, true);
			this.zRadioButton2.TabIndex = 1;
			this.zRadioButton2.Text = "Residential";
			// 
			// CalloutNett7DayPaymentForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 145, true);
			this.Controls.Add(this.zRadioButton1);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.zRadioButton2);
			this.DataSourceType = typeof(Enterprise.Client.UPE.Business.CalloutNett7DayPayment);
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.CalloutNett7DayPayment";
			this.Name = "CalloutNett7DayPaymentForm";
			this.Controls.SetChildIndex(this.TotalAmountLabel, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.TotalAmountCalcEdit, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.zRadioButton2, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.zRadioButton1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
