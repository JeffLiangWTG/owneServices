using Enterprise.Client.UPE.Business;

namespace Enterprise.Client.UPE.GUI
{
	public partial class CalloutCreditCardPaymentForm : Enterprise.Client.UPE.GUI.CalloutPaymentDetailsForm
	{
		Enterprise.ZArchitecture.ZTextBox CardNumberTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel1;
		Enterprise.ZArchitecture.ZLabel zLabel2;
		Enterprise.ZArchitecture.ZLabel zLabel3;
		Enterprise.ZArchitecture.ZLabel zLabel4;
		Enterprise.ZArchitecture.ZTextBox SecurityCodeTextBox;
		Enterprise.ZArchitecture.ZTextBox NameTextBox;
		Enterprise.ZArchitecture.ZLabel zLabel5;
		Enterprise.ZArchitecture.ZTextBox zTextBox1;
		Enterprise.ZArchitecture.GUI.ZDropEdit ExpiryDate_MonthDropEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit ExpiryDate_YearDropEdit;
		Enterprise.ZArchitecture.ZLabel zLabel6;

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		protected override void InitializeComponent()
		{
			this.CardNumberTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel1 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel2 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel3 = new Enterprise.ZArchitecture.ZLabel();
			this.zLabel4 = new Enterprise.ZArchitecture.ZLabel();
			this.SecurityCodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.zLabel5 = new Enterprise.ZArchitecture.ZLabel();
			this.zTextBox1 = new Enterprise.ZArchitecture.ZTextBox();
			this.ExpiryDate_MonthDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.ExpiryDate_YearDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.zLabel6 = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// TotalAmountCalcEdit
			// 
			this.TotalAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(206, 163, true);
			// 
			// TotalAmountLabel
			// 
			this.TotalAmountLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(128, 162, true);
			// 
			// OKButton
			// 
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(170, 189, true);
			// 
			// CancelButton
			// 
			this.CancelButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 189, true);
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 22, true);
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(156);
			// 
			// ErrorStatusBarPanel
			// 
			this.ErrorStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(157);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Client.UPE.Business.CalloutCreditCardPayment);
			// 
			// CardNumberTextBox
			// 
			this.BindingSource.SetBindingMember(this.CardNumberTextBox, "CardNumber");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.CalloutCreditCardPayment)(null)).CardNumber)));
			this.CardNumberTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 9, true);
			this.CardNumberTextBox.Name = "CardNumberTextBox";
			this.CardNumberTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.CardNumberTextBox.TabIndex = 0;
			// 
			// zLabel1
			// 
			this.zLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 7, true);
			this.zLabel1.Name = "zLabel1";
			this.zLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
			this.zLabel1.TabIndex = 1;
			this.zLabel1.Text = "Card Number";
			// 
			// zLabel2
			// 
			this.zLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 33, true);
			this.zLabel2.Name = "zLabel2";
			this.zLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(108, 21, true);
			this.zLabel2.TabIndex = 3;
			this.zLabel2.Text = "Name on Credit Card";
			// 
			// zLabel3
			// 
			this.zLabel3.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 83, true);
			this.zLabel3.Name = "zLabel3";
			this.zLabel3.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.zLabel3.TabIndex = 8;
			this.zLabel3.Text = "Security Code";
			// 
			// zLabel4
			// 
			this.zLabel4.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 58, true);
			this.zLabel4.Name = "zLabel4";
			this.zLabel4.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.zLabel4.TabIndex = 5;
			this.zLabel4.Text = "Expiry Date";
			// 
			// SecurityCodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.SecurityCodeTextBox, "SecurityCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.CalloutCreditCardPayment)(null)).SecurityCode)));
			this.SecurityCodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 84, true);
			this.SecurityCodeTextBox.Name = "SecurityCodeTextBox";
			this.SecurityCodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 20, true);
			this.SecurityCodeTextBox.TabIndex = 4;
			// 
			// NameTextBox
			// 
			this.BindingSource.SetBindingMember(this.NameTextBox, "NameOnCard");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.CalloutCreditCardPayment)(null)).NameOnCard)));
			this.NameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 34, true);
			this.NameTextBox.Name = "NameTextBox";
			this.NameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(200, 20, true);
			this.NameTextBox.TabIndex = 1;
			// 
			// zLabel5
			// 
			this.zLabel5.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 101, true);
			this.zLabel5.Name = "zLabel5";
			this.zLabel5.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 21, true);
			this.zLabel5.TabIndex = 10;
			this.zLabel5.Text = "VPOS";
			// 
			// zTextBox1
			// 
			this.BindingSource.SetBindingMember(this.zTextBox1, "VPOSCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Client.UPE.Business.CalloutCreditCardPayment)(null)).VPOSCode)));
			this.zTextBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 110, true);
			this.zTextBox1.Name = "zTextBox1";
			this.zTextBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(91, 20, true);
			this.zTextBox1.TabIndex = 5;
			// 
			// ExpiryDate_MonthDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ExpiryDate_MonthDropEdit, "ExpiryDate_Month");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.CalloutCreditCardPayment)(null)).ExpiryDate_Month)));
			this.ExpiryDate_MonthDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 59, true);
			this.ExpiryDate_MonthDropEdit.Name = "ExpiryDate_MonthDropEdit";
			this.ExpiryDate_MonthDropEdit.PreBoundMaxLength = 2;
			this.ExpiryDate_MonthDropEdit.ShowDescriptionBox = false;
			this.ExpiryDate_MonthDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.ExpiryDate_MonthDropEdit.TabIndex = 2;
			// 
			// ExpiryDate_YearDropEdit
			// 
			this.BindingSource.SetBindingMember(this.ExpiryDate_YearDropEdit, "ExpiryDate_Year");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Client.UPE.Business.CalloutCreditCardPayment)(null)).ExpiryDate_Year)));
			this.ExpiryDate_YearDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(190, 59, true);
			this.ExpiryDate_YearDropEdit.Name = "ExpiryDate_YearDropEdit";
			this.ExpiryDate_YearDropEdit.PreBoundMaxLength = 2;
			this.ExpiryDate_YearDropEdit.ShowDescriptionBox = false;
			this.ExpiryDate_YearDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(43, 20, true);
			this.ExpiryDate_YearDropEdit.TabIndex = 3;
			// 
			// zLabel6
			// 
			this.zLabel6.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(175, 59, true);
			this.zLabel6.Name = "zLabel6";
			this.zLabel6.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(12, 18, true);
			this.zLabel6.TabIndex = 107;
			this.zLabel6.Text = "/";
			// 
			// CalloutCreditCardPaymentForm
			// 

			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(338, 239, true);
			this.Controls.Add(this.zLabel6);
			this.Controls.Add(this.ExpiryDate_YearDropEdit);
			this.Controls.Add(this.ExpiryDate_MonthDropEdit);
			this.Controls.Add(this.zTextBox1);
			this.Controls.Add(this.zLabel5);
			this.Controls.Add(this.zLabel1);
			this.Controls.Add(this.CardNumberTextBox);
			this.Controls.Add(this.zLabel2);
			this.Controls.Add(this.zLabel3);
			this.Controls.Add(this.zLabel4);
			this.Controls.Add(this.SecurityCodeTextBox);
			this.Controls.Add(this.NameTextBox);
			this.DataSourceType = typeof(Enterprise.Client.UPE.Business.CalloutCreditCardPayment);
			this.DataSourceTypeName = "Enterprise.Client.UPE.Business.CalloutCreditCardPayment";
			this.Name = "CalloutCreditCardPaymentForm";
			this.Controls.SetChildIndex(this.TotalAmountLabel, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.TotalAmountCalcEdit, 0);
			this.Controls.SetChildIndex(this.CancelButton, 0);
			this.Controls.SetChildIndex(this.NameTextBox, 0);
			this.Controls.SetChildIndex(this.SecurityCodeTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel4, 0);
			this.Controls.SetChildIndex(this.zLabel3, 0);
			this.Controls.SetChildIndex(this.zLabel2, 0);
			this.Controls.SetChildIndex(this.CardNumberTextBox, 0);
			this.Controls.SetChildIndex(this.zLabel1, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.zLabel5, 0);
			this.Controls.SetChildIndex(this.zTextBox1, 0);
			this.Controls.SetChildIndex(this.ExpiryDate_MonthDropEdit, 0);
			this.Controls.SetChildIndex(this.ExpiryDate_YearDropEdit, 0);
			this.Controls.SetChildIndex(this.zLabel6, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
