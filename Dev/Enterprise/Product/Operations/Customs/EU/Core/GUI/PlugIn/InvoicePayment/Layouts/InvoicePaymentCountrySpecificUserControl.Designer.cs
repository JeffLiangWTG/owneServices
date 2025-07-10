namespace Enterprise.Customs.EU.GUI
{
	partial class InvoicePaymentCountrySpecificUserControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		void InitializeComponent()
		{
			this.CommercialPaymentCodeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PaymentAmountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PaymentNoTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.PaymentDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.CommercialPaymentCodeDropEdit.SuspendLayout();
			this.PaymentDateEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader);
			// 
			// CommercialPaymentCodeDropEdit
			// 
			this.CommercialPaymentCodeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CommercialPaymentCodeDropEdit, "ZG_CommercialPaymentCode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(null)).ZG_CommercialPaymentCode)));
			this.CommercialPaymentCodeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 96, true);
			this.CommercialPaymentCodeDropEdit.Name = "CommercialPaymentCodeDropEdit";
			this.CommercialPaymentCodeDropEdit.ShouldResizeByMaxLength = true;
			this.CommercialPaymentCodeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(170, 20, true);
			this.CommercialPaymentCodeDropEdit.TabIndex = 1;
			// 
			// PaymentAmountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PaymentAmountCalcEdit, "JZ_PaymentAmount");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(null)).JZ_PaymentAmount)));
			this.PaymentAmountCalcEdit.CaptionResourceString = null;
			this.PaymentAmountCalcEdit.DecimalPlaces = 2;
			this.PaymentAmountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 70, true);
			this.PaymentAmountCalcEdit.Name = "PaymentAmountCalcEdit";
			this.PaymentAmountCalcEdit.ShouldEscapeAllSpecialCharacters = false;
			this.PaymentAmountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.PaymentAmountCalcEdit.TabIndex = 2;
			this.PaymentAmountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// PaymentNoTextBox
			// 
			this.PaymentNoTextBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.PaymentNoTextBox, "JZ_PaymentNo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(null)).JZ_PaymentNo)));
			this.PaymentNoTextBox.CaptionResourceString = null;
			this.PaymentNoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 18, true);
			this.PaymentNoTextBox.Name = "PaymentNoTextBox";
			this.PaymentNoTextBox.ShouldEscapeAllSpecialCharacters = false;
			this.PaymentNoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(119, 20, true);
			this.PaymentNoTextBox.TabIndex = 3;
			// 
			// PaymentDateEdit
			// 
			this.PaymentDateEdit.AllowDrop = true;
			this.PaymentDateEdit.AutoCompleteMonthThreshold = 1;
			this.PaymentDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.PaymentDateEdit, "JZ_PaymentDate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.EU.Business.Declaration.JobComInvoiceHeader)(null)).JZ_PaymentDate)));
			this.PaymentDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(129, 44, true);
			this.PaymentDateEdit.Name = "PaymentDateEdit";
			this.PaymentDateEdit.TabIndex = 4;
			// 
			// InvoicePaymentCountrySpecificUserControl
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.CommercialPaymentCodeDropEdit);
			this.Controls.Add(this.PaymentAmountCalcEdit);
			this.Controls.Add(this.PaymentNoTextBox);
			this.Controls.Add(this.PaymentDateEdit);
			this.Name = "InvoicePaymentCountrySpecificUserControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(315, 145, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.CommercialPaymentCodeDropEdit.ResumeLayout(true);
			this.CommercialPaymentCodeDropEdit.PerformLayout();
			this.PaymentDateEdit.ResumeLayout(true);
			this.PaymentDateEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		internal Enterprise.ZArchitecture.GUI.ZDropEdit CommercialPaymentCodeDropEdit;
		internal Enterprise.ZArchitecture.ZCalcEdit PaymentAmountCalcEdit;
		internal Enterprise.ZArchitecture.ZTextBox PaymentNoTextBox;
		internal Enterprise.ZArchitecture.GUI.ZDateEdit PaymentDateEdit;
	}
}
